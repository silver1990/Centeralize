using Exon.TheWeb.Service.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Raybod.SCM.DataAccess.Core;
using Raybod.SCM.DataAccess.Extention;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Audit;
using Raybod.SCM.DataTransferObject.Document;
using Raybod.SCM.DataTransferObject.Document.RevisionConfirmation;
using Raybod.SCM.DataTransferObject.Notification;
using Raybod.SCM.DataTransferObject.User;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Model;
using Raybod.SCM.Domain.Struct;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Services.Utilitys.MailService;
using Raybod.SCM.Services.Utilitys.MailService.Model;
using Raybod.SCM.Utility.Extention;
using Raybod.SCM.Utility.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Hangfire;
using Raybod.SCM.DataTransferObject.Email;
using Raybod.SCM.DataTransferObject._PanelDocument.Communication.Comment;
using Microsoft.AspNetCore.Http;

namespace Raybod.SCM.Services.Implementation
{
    public class RevisionConfirmationService : IRevisionConfirmationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;
        private readonly DbSet<User> _userRepository;
        private readonly DbSet<DocumentRevision> _documentRevisionRepository;
        private readonly DbSet<ConfirmationWorkFlow> _confirmationWorkFlowRepository;
        private readonly DbSet<ConfirmationWorkFlowUser> _confirmationWorkFlowUserRepository;
        private readonly DbSet<UserNotify> _notifyRepository;
        private readonly DbSet<RevisionAttachment> _revisionAttachmentRepository;
        private readonly ITeamWorkAuthenticationService _authenticationServices;
        private readonly ISCMLogAndNotificationService _scmLogAndNotificationService;
        private readonly Utilitys.FileHelper _fileHelper;
        private readonly CompanyConfig _appSettings;
        private readonly IAppEmailService _appEmailService;
        private readonly IViewRenderService _viewRenderService;

        public RevisionConfirmationService(
            IUnitOfWork unitOfWork,
            IWebHostEnvironment hostingEnvironmentRoot,
            IOptions<CompanyAppSettingsDto> appSettings,
            ITeamWorkAuthenticationService authenticationServices,
            ISCMLogAndNotificationService scmLogAndNotificationService,
            IHttpContextAccessor httpContextAccessor,
            IFileService fileService, IAppEmailService appEmailService, IViewRenderService viewRenderService)
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
            _authenticationServices = authenticationServices;
            _scmLogAndNotificationService = scmLogAndNotificationService;
            _userRepository = _unitOfWork.Set<User>();
            _documentRevisionRepository = _unitOfWork.Set<DocumentRevision>();
            _confirmationWorkFlowRepository = _unitOfWork.Set<ConfirmationWorkFlow>();
            _confirmationWorkFlowUserRepository = _unitOfWork.Set<ConfirmationWorkFlowUser>();
            _revisionAttachmentRepository = _unitOfWork.Set<RevisionAttachment>();
            _notifyRepository = _unitOfWork.Set<UserNotify>();
            _fileHelper = new Utilitys.FileHelper(hostingEnvironmentRoot);
            _appEmailService = appEmailService;
            _viewRenderService = viewRenderService;
            httpContextAccessor.HttpContext.Request.Headers.TryGetValue("companyCode", out var CompanyCode);
            _appSettings = appSettings.Value.CompanyConfig.First(a => a.CompanyCode == CompanyCode);
        }

        public async Task<ServiceResult<List<UserMentionDto>>> GetConfirmationUserListAsync(AuthenticateDto authenticate, long revisionId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<UserMentionDto>>(null, MessageId.AccessDenied);

                var dbQuery = _documentRevisionRepository
                    .Where(a => !a.IsDeleted && a.DocumentRevisionId == revisionId && a.Document.ContractCode == authenticate.ContractCode);

                var documentGroupId = await dbQuery
                    .Select(c => c.Document.DocumentGroupId)
                    .FirstOrDefaultAsync();

                if (permission.DocumentGroupIds.Any() && !dbQuery.Any(a => permission.DocumentGroupIds.Contains(a.Document.DocumentGroupId)))
                    return ServiceResultFactory.CreateError<List<UserMentionDto>>(null, MessageId.AccessDenied);

                var roles = new List<string> { SCMRole.RevisionConfirmMng, SCMRole.RevisionConfirmGlbMng };
                var list = await _authenticationServices.GetAllUserHasAccessDocumentAsync(authenticate.ContractCode, roles, documentGroupId);

                foreach (var item in list)
                {
                    item.Image = item.Image != null ?
                        _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + item.Image : "";
                }

                return ServiceResultFactory.CreateSuccess(list);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<UserMentionDto>>(null, exception);
            }
        }


        public async Task<ServiceResult<bool>> SetConfirmationRevisionAsync(AuthenticateDto authenticate,long docId, long revId, AddRevisionConfirmationDto model)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery = _documentRevisionRepository
                     .Where(a => !a.IsDeleted &&
                     a.Document.IsActive &&
                     a.DocumentId == docId &&
                     a.Document.ContractCode == authenticate.ContractCode &&
                     a.DocumentRevisionId == revId);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (permission.DocumentGroupIds.Any() && !dbQuery.Any(a => permission.DocumentGroupIds.Contains(a.Document.DocumentGroupId)))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var revisionModel = await dbQuery
                    .Include(a => a.Document)
                    .ThenInclude(a=>a.DocumentGroup)
                    .FirstOrDefaultAsync();
                if (revisionModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (revisionModel.RevisionStatus>=RevisionStatus.Confirmed)
                    return ServiceResultFactory.CreateError(false, MessageId.RevisionAlreadyConfirm);

                if (model.Attachments == null || !model.Attachments.Any())
                    return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                var isLastRevisionStatusPendingModify = revisionModel.RevisionStatus == RevisionStatus.PendingForModify ? true : false;
                var AddRevisionConfirmationResult = await AddRevisionConfirmationAsync(authenticate, docId, revId, model);
                if (!AddRevisionConfirmationResult.Succeeded)
                    return ServiceResultFactory.CreateError(false, AddRevisionConfirmationResult.Messages[0].Message);

                if (model.UserConfirmations == null || !model.UserConfirmations.Any())
                {
                    revisionModel.RevisionStatus = RevisionStatus.Confirmed;
                    revisionModel.RevisionPageNumber = model.RevisionPageNumber;
                    revisionModel.RevisionPageSize = model.RevisionPageSize;

                    var lastRevisions = await _documentRevisionRepository
                        .Where(a => a.IsLastConfirmRevision && a.DocumentRevisionId != revId && a.DocumentId == docId)
                        .ToListAsync();
                    foreach (var item in lastRevisions)
                    {
                        item.IsLastConfirmRevision = false;
                    }
                    revisionModel.IsLastConfirmRevision = true;
                    BackgroundJob.Enqueue(() => SendEmailToReceiverUserAsync(authenticate, revisionModel));

                }
                else
                {
                    revisionModel.RevisionStatus = RevisionStatus.PendingConfirm;
                }

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    await SendLogAndNotitificationTaskOnSetConfirmRevisionAsync(authenticate, revisionModel, AddRevisionConfirmationResult, isLastRevisionStatusPendingModify);

                    return ServiceResultFactory.CreateSuccess(true);
                }
                return ServiceResultFactory.CreateError(false, MessageId.SaveFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        private async Task SendLogAndNotitificationTaskOnSetConfirmRevisionAsync(AuthenticateDto authenticate,
            DocumentRevision revisionModel, ServiceResult<ConfirmationWorkFlow> AddRevisionConfirmationResult, bool isLastRevisionStatusPendingModify)
        {
            NotifEvent notifEvent = revisionModel.RevisionStatus == RevisionStatus.Confirmed ? NotifEvent.RevisionConfirm : NotifEvent.SendingRevisionConfirmation;
            var logModel = new AddAuditLogDto
            {
                ContractCode = authenticate.ContractCode,
                Description = revisionModel.Document.DocTitle,
                FormCode = revisionModel.DocumentRevisionCode,
                KeyValue = revisionModel.DocumentRevisionId.ToString(),
                NotifEvent = notifEvent,
                RootKeyValue = revisionModel.DocumentRevisionId.ToString(),
                PerformerUserId = authenticate.UserId,
                PerformerUserFullName = authenticate.UserFullName,
                DocumentGroupId = revisionModel.Document.DocumentGroupId
            };

            await _scmLogAndNotificationService
             .SetDonedNotificationAsync(authenticate.UserId, authenticate.ContractCode, revisionModel.DocumentRevisionId.ToString(), isLastRevisionStatusPendingModify ? NotifEvent.RevisionReject : NotifEvent.AddRevision);

            if (notifEvent == NotifEvent.RevisionConfirm&&revisionModel.Document.IsRequiredTransmittal)
            {
                var taskModel = new List<NotifToDto>
                        { new NotifToDto
                        {
                            NotifEvent = NotifEvent.AddTransmittal,
                            Roles = new List<string>
                            {
                                SCMRole.TransmittalMng
                            }
                        }
                        };

                var res1 = await _scmLogAndNotificationService.AddScmAuditLogAsync(logModel, taskModel);
            }
            else
            {
                var ballInCourUserId = AddRevisionConfirmationResult.Result.ConfirmationWorkFlowUsers.Where(a => a.IsBallInCourt)
                    .Select(c => c.UserId)
                    .FirstOrDefault();
                var taskModel = new AddTaskNotificationDto
                {
                    ContractCode = authenticate.ContractCode,
                    Description = revisionModel.Document.DocTitle,
                    FormCode = revisionModel.DocumentRevisionCode,
                    KeyValue = revisionModel.DocumentRevisionId.ToString(),
                    NotifEvent = NotifEvent.BallInCourtRevisionConfirmation,
                    RootKeyValue = revisionModel.DocumentRevisionId.ToString(),
                    PerformerUserId = authenticate.UserId,
                    PerformerUserFullName = authenticate.UserFullName,
                    Users = new List<int>
                            {
                                ballInCourUserId
                            }
                };

                await _scmLogAndNotificationService.AddScmAuditLogAndTaskAsync(logModel, taskModel);
            }
        }

        private async Task<ServiceResult<ConfirmationWorkFlow>> AddRevisionConfirmationAsync(AuthenticateDto authenticate, long docId, long revId, AddRevisionConfirmationDto model)
        {
            //if (usersIds.Count() == 0)
            //    return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);
            var revisionConfirmationModel = new ConfirmationWorkFlow
            {
                DocumentRevisionId = revId,
                ConfirmNote = model.ConfirmNote,
                RevisionPageNumber = model.RevisionPageNumber,
                RevisionPageSize = model.RevisionPageSize,
                Status = ConfirmationWorkFlowStatus.Pending,
                ConfirmationWorkFlowUsers = new List<ConfirmationWorkFlowUser>()
            };

            if (model.UserConfirmations != null && model.UserConfirmations.Any())
            {
                var usersIds = model.UserConfirmations.Select(a => a.UserId).Distinct().ToList();
                if (await _userRepository.CountAsync(a => !a.IsDeleted && a.IsActive && usersIds.Contains(a.Id)) != usersIds.Count())
                    return ServiceResultFactory.CreateError<ConfirmationWorkFlow>(null, MessageId.DataInconsistency);

                foreach (var item in model.UserConfirmations)
                {
                    revisionConfirmationModel.ConfirmationWorkFlowUsers.Add(new ConfirmationWorkFlowUser
                    {
                        UserId = item.UserId,
                        OrderNumber = item.OrderNumber
                    });
                }
                if (revisionConfirmationModel.ConfirmationWorkFlowUsers.Any())
                {
                    var bollincourtUser = revisionConfirmationModel.ConfirmationWorkFlowUsers.OrderBy(a => a.OrderNumber).First();
                    bollincourtUser.IsBallInCourt = true;
                    bollincourtUser.DateStart = DateTime.UtcNow;
                }
            }
            else
            {
                revisionConfirmationModel.Status = ConfirmationWorkFlowStatus.Confirm;
            }

            var res = await AddConfirmationAttachmentAsync(authenticate, docId, revId, revisionConfirmationModel, model.ConfirmationWorkFlowId, model.Attachments);
            if (!res.Succeeded)
                return ServiceResultFactory.CreateError<ConfirmationWorkFlow>(null, res.Messages[0].Message);


            _confirmationWorkFlowRepository.Add(revisionConfirmationModel);

            return ServiceResultFactory.CreateSuccess(revisionConfirmationModel);
        }

        private async Task<ServiceResult<bool>> AddConfirmationAttachmentAsync(AuthenticateDto authenticate, long docId,
            long revId, ConfirmationWorkFlow confirmationWorkFlowModel, long ConfirmationId, List<AddRevConfirmationAttachmentDto> files)
        {
            var acceptAttachType = new List<RevisionAttachmentType> { RevisionAttachmentType.FinalNative, RevisionAttachmentType.Final };

            if (files == null || !files.Any())
                return ServiceResultFactory.CreateError(false, MessageId.FileNotFound);

            if (files.Any(c => !acceptAttachType.Contains(c.AttachType)))
                return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

            if (files.Any(a => a.RevisionAttachmentId > 0) && ConfirmationId <= 0)
                return ServiceResultFactory.CreateError(false, MessageId.FileNotFound);


            var newFile = files.Where(a => a.RevisionAttachmentId <= 0).ToList();
            var oldFileIds = files.Where(a => a.RevisionAttachmentId > 0).Select(a => a.RevisionAttachmentId).ToList();

            var attachModels = new List<RevisionAttachment>();

            // add oldFiles
            if (oldFileIds.Any())
            {
                var confirmationAttachments = await _revisionAttachmentRepository.Where(a => a.ConfirmationWorkFlowId == ConfirmationId && oldFileIds.Contains(a.RevisionAttachmentId))
                       .ToListAsync();

                if (confirmationAttachments.Count() != oldFileIds.Count)
                    return ServiceResultFactory.CreateError(false, MessageId.FileNotFound);

                foreach (var item in confirmationAttachments)
                {
                    attachModels.Add(new RevisionAttachment
                    {
                        ConfirmationWorkFlow = confirmationWorkFlowModel,
                        FileSrc = item.FileSrc,
                        RevisionAttachmentType = item.RevisionAttachmentType,
                        FileName = item.FileName,
                        FileType = item.FileType,
                        FileSize = item.FileSize
                    });

                }
            }

            // add new files
            foreach (var item in newFile)
            {
                var UploadedFile = await _fileHelper
                    .SaveDocumentFromTemp(item.FileSrc, ServiceSetting.UploadFilePathDocument(authenticate.ContractCode, docId, revId));

                if (UploadedFile == null)
                    return ServiceResultFactory.CreateError(false, MessageId.UploudFailed);

                _fileHelper.DeleteDocumentFromTemp(item.FileSrc);

                attachModels.Add(new RevisionAttachment
                {
                    ConfirmationWorkFlow = confirmationWorkFlowModel,
                    FileSrc = item.FileSrc,
                    RevisionAttachmentType = item.AttachType,
                    FileName = item.FileName,
                    FileType = UploadedFile.FileType,
                    FileSize = UploadedFile.FileSize
                });

            }
            // if confirm revision add all confirm file to revisionModel
            if (confirmationWorkFlowModel.Status == ConfirmationWorkFlowStatus.Confirm)
            {
                var revisionFiles = attachModels.Select(item => new RevisionAttachment
                {
                    DocumentRevisionId = revId,
                    FileSrc = item.FileSrc,
                    RevisionAttachmentType = item.RevisionAttachmentType,
                    FileName = item.FileName,
                    FileType = item.FileType,
                    FileSize = item.FileSize
                }).ToList();

                attachModels.AddRange(revisionFiles);
            }

            _revisionAttachmentRepository.AddRange(attachModels);
            return ServiceResultFactory.CreateSuccess(true);
        }

        public async Task<ServiceResult<List<PendingConfirmRevisionListDto>>> GetPendingConfirmRevisionAsync(AuthenticateDto authenticate, DocRevisionQueryDto query)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<PendingConfirmRevisionListDto>>(null, MessageId.AccessDenied);

                var dbQuery = _confirmationWorkFlowRepository
                     .AsNoTracking()
                     .Where(a => !a.IsDeleted &&
                     a.DocumentRevision != null &&
                     a.Status == ConfirmationWorkFlowStatus.Pending &&
                     a.DocumentRevision.RevisionStatus == RevisionStatus.PendingConfirm &&
                     a.DocumentRevision.Document.IsActive &&
                     a.DocumentRevision.Document.ContractCode == authenticate.ContractCode)
                     .OrderByDescending(a => a.ConfirmationWorkFlowId)
                     .AsQueryable();

                if (permission.DocumentGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.DocumentGroupIds.Contains(a.DocumentRevision.Document.DocumentGroupId));

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a =>
                     a.DocumentRevision.Document.DocTitle.Contains(query.SearchText)
                     || a.DocumentRevision.Document.DocNumber.Contains(query.SearchText)
                     || a.DocumentRevision.Description.Contains(query.SearchText));

                if (EnumHelper.ValidateItem(query.DocClass))
                    dbQuery = dbQuery.Where(a => a.DocumentRevision.Document.DocClass == query.DocClass);

                if (query.DocumentGroupIds != null && query.DocumentGroupIds.Any())
                    dbQuery = dbQuery.Where(a => query.DocumentGroupIds.Contains(a.DocumentRevision.Document.DocumentGroupId));

                var totalCount = dbQuery.Count();

                var columnsMap = new Dictionary<string, Expression<Func<ConfirmationWorkFlow, object>>>
                {
                    ["ConfirmationWorkFlowId"] = v => v.ConfirmationWorkFlowId
                };

                dbQuery = dbQuery.ApplayOrdering(query, columnsMap).ApplayPageing(query);

                var result = await dbQuery.Select(a => new PendingConfirmRevisionListDto
                {
                    DocumentId = a.DocumentRevision.DocumentId,
                    ClientDocNumber = (a.DocumentRevision.Document.ClientDocNumber != null) ? a.DocumentRevision.Document.ClientDocNumber : "",
                    DocumentRevisionId = a.DocumentRevisionId.Value,
                    DocumentRevisionCode = a.DocumentRevision.DocumentRevisionCode,
                    Description = a.DocumentRevision.Description,
                    RevisionStatus = a.DocumentRevision.RevisionStatus,
                    DateEnd = a.DocumentRevision.DateEnd.ToUnixTimestamp(),
                    DocTitle = a.DocumentRevision.Document.DocTitle,
                    DocNumber = a.DocumentRevision.Document.DocNumber,
                    DocClass = a.DocumentRevision.Document.DocClass,
                    DocumentGroupCode = a.DocumentRevision.Document.DocumentGroup.DocumentGroupCode,
                    DocumentGroupTitle = a.DocumentRevision.Document.DocumentGroup.Title,
                    UserAudit = a.DocumentRevision.AdderUser != null ? new UserAuditLogDto
                    {
                        CreateDate = a.DocumentRevision.CreatedDate.ToUnixTimestamp(),
                        AdderUserName = a.DocumentRevision.AdderUser.FirstName + " " + a.DocumentRevision.AdderUser.LastName,
                        AdderUserImage = a.DocumentRevision.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + a.DocumentRevision.AdderUser.Image : ""
                    } : null,

                    BallInCourtUser = a.ConfirmationWorkFlowUsers.Any() ?
                    a.ConfirmationWorkFlowUsers.Where(a => a.IsBallInCourt)
                    .Select(c => new UserAuditLogDto
                    {
                        AdderUserId = c.UserId,
                        AdderUserName = c.User.FirstName + " " + c.User.LastName,
                        AdderUserImage = c.User.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + c.User.Image : ""
                    }).FirstOrDefault() : new UserAuditLogDto()
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<PendingConfirmRevisionListDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<ConfirmationWorkflowDto>> GetPendingConfirmRevisionByRevIdAsync(AuthenticateDto authenticate, long revisionId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<ConfirmationWorkflowDto>(null, MessageId.AccessDenied);

                var dbQuery = _confirmationWorkFlowRepository
                     .AsNoTracking()
                     .Where(a => !a.IsDeleted &&
                    a.DocumentRevisionId == revisionId &&
                    a.Status == ConfirmationWorkFlowStatus.Pending &&
                     a.DocumentRevision.Document.IsActive &&
                     a.DocumentRevision.Document.ContractCode == authenticate.ContractCode &&
                     a.DocumentRevision.RevisionStatus == RevisionStatus.PendingConfirm);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<ConfirmationWorkflowDto>(null, MessageId.EntityDoesNotExist);

                if (permission.DocumentGroupIds.Any() && !dbQuery.Any(a => permission.DocumentGroupIds.Contains(a.DocumentRevision.Document.DocumentGroupId)))
                    return ServiceResultFactory.CreateError<ConfirmationWorkflowDto>(null, MessageId.AccessDenied);

                var result = await dbQuery
                        .Select(x => new ConfirmationWorkflowDto
                        {
                            ConfirmNote = x.ConfirmNote,
                            RevisionPageNumber = x.RevisionPageNumber,
                            RevisionPageSize = x.RevisionPageSize,
                            ConfirmUserAudit = x.AdderUserId != null ? new UserAuditLogDto
                            {
                                CreateDate = x.CreatedDate.ToUnixTimestamp(),
                                AdderUserName = x.AdderUser.FullName,
                                AdderUserImage = x.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + x.AdderUser.Image : ""
                            } : null,
                            FinalAttachments = x.ConfirmationAttachments.Where(m => !m.IsDeleted && m.RevisionAttachmentType == RevisionAttachmentType.Final)
                            .Select(c => new RevisionAttachmentDto
                            {
                                AttachType = c.RevisionAttachmentType,
                                FileName = c.FileName,
                                FileSize = c.FileSize,
                                FileSrc = c.FileSrc,
                                FileType = c.FileType,
                                RevisionAttachmentId = c.RevisionAttachmentId
                            }).ToList(),
                            FinalNativeAttachments = x.ConfirmationAttachments.Where(n => !n.IsDeleted && n.RevisionAttachmentType == RevisionAttachmentType.FinalNative)
                            .Select(c => new RevisionAttachmentDto
                            {
                                AttachType = c.RevisionAttachmentType,
                                FileName = c.FileName,
                                FileSize = c.FileSize,
                                FileSrc = c.FileSrc,
                                FileType = c.FileType,
                                RevisionAttachmentId = c.RevisionAttachmentId
                            }).ToList(),
                            ConfirmationUserWorkFlows = x.ConfirmationWorkFlowUsers.Where(a => a.UserId > 0)
                            .Select(e => new ConfirmationUserWorkFlowDto
                            {
                                ConfirmationWorkFlowUserId = e.ConfirmationWorkFlowUserId,
                                DateEnd = e.DateEnd.ToUnixTimestamp(),
                                DateStart = e.DateStart.ToUnixTimestamp(),
                                IsAccept = e.IsAccept,
                                IsBallInCourt = e.IsBallInCourt,
                                Note = e.Note,
                                OrderNumber = e.OrderNumber,
                                UserId = e.UserId,
                                UserFullName = e.User != null ? e.User.FullName : "",
                                UserImage = e.User != null && e.User.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + e.User.Image : ""
                            }).ToList()
                        }).FirstOrDefaultAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<ConfirmationWorkflowDto>(null, exception);
            }
        }

        public async Task<ServiceResult<RejectedUserDto>> GetRejectedUserInfoConfirmationRevisionAsync(AuthenticateDto authenticate, long revisionId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<RejectedUserDto>(null, MessageId.AccessDenied);

                var dbQuery = _confirmationWorkFlowUserRepository
                     .AsNoTracking()
                     .OrderByDescending(c => c.ConfirmationWorkFlowId)
                     .Where(a => !a.ConfirmationWorkFlow.IsDeleted &&
                    a.ConfirmationWorkFlow.DocumentRevisionId == revisionId &&
                     a.ConfirmationWorkFlow.DocumentRevision.Document.ContractCode == authenticate.ContractCode);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<RejectedUserDto>(null, MessageId.EntityDoesNotExist);

                if (permission.DocumentGroupIds.Any() && !dbQuery.Any(a => permission.DocumentGroupIds.Contains(a.ConfirmationWorkFlow.DocumentRevision.Document.DocumentGroupId)))
                    return ServiceResultFactory.CreateError<RejectedUserDto>(null, MessageId.AccessDenied);

                var result = await dbQuery
                    .Where(a => a.IsBallInCourt && !a.IsAccept)
                        .Select(x => new RejectedUserDto
                        {
                            FullName = x.User.FullName,
                            DateEnd = x.DateEnd.ToUnixTimestamp(),
                            Image = x.User.Image,
                            Note = x.Note
                        }).FirstOrDefaultAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<RejectedUserDto>(null, exception);
            }
        }

        public async Task<DownloadFileDto> DownloadRevisionNativeAndFinalFileAsync(AuthenticateDto authenticate, long docId, long revId, RevisionAttachmentType attachType, string fileSrc)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return null;

                if (attachType == RevisionAttachmentType.FinalNative && _appSettings.IsCheckedIP && !_appSettings.RemoteIPs.Contains(authenticate.RemoteIpAddress))
                    return null;

                var dbQuery = _revisionAttachmentRepository
                    .AsNoTracking()
                    .Where(a =>
                    !a.IsDeleted
                    && a.ConfirmationWorkFlowId != null
                    && a.ConfirmationWorkFlow.DocumentRevisionId == revId
                    && a.ConfirmationWorkFlow.DocumentRevision.DocumentId == docId
                    && a.RevisionAttachmentType == attachType
                    && a.FileSrc == fileSrc);

                if (permission.DocumentGroupIds.Any() && !dbQuery.Any(a => permission.DocumentGroupIds.Contains(a.ConfirmationWorkFlow.DocumentRevision.Document.DocumentGroupId)))
                    return null;

                if (!await dbQuery.AnyAsync())
                    return null;

                return await _fileHelper.DownloadDocument(fileSrc, ServiceSetting.UploadFilePathDocument(authenticate.ContractCode, docId, revId));
            }
            catch (Exception exception)
            {
                return null;
            }
        }

        public async Task<ServiceResult<bool>> SetUserConfirmOwnRevisionTaskAsync(AuthenticateDto authenticate, long revisionId, AddConfirmationAnswerDto model)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery = _confirmationWorkFlowRepository
                     .Where(a => !a.IsDeleted &&
                    a.DocumentRevisionId == revisionId &&
                    a.Status == ConfirmationWorkFlowStatus.Pending &&
                    a.DocumentRevision.Document.IsActive &&
                    a.DocumentRevision.Document.ContractCode == authenticate.ContractCode &&
                    a.DocumentRevision.RevisionStatus == RevisionStatus.PendingConfirm)
                     .Include(a => a.ConfirmationAttachments)
                     .Include(a => a.ConfirmationWorkFlowUsers)
                     .ThenInclude(c => c.User)
                     .Include(a => a.DocumentRevision)
                     .ThenInclude(a => a.Document)
                     .ThenInclude(a=>a.DocumentGroup)
                     .AsQueryable();


                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (permission.DocumentGroupIds.Any() && !dbQuery.Any(a => permission.DocumentGroupIds.Contains(a.DocumentRevision.Document.DocumentGroupId)))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var confirmationModel = await dbQuery.FirstOrDefaultAsync();

                if (confirmationModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (confirmationModel.ConfirmationWorkFlowUsers == null && !confirmationModel.ConfirmationWorkFlowUsers.Any())
                    return ServiceResultFactory.CreateError(false, MessageId.ConfirmationWorkflowHaveNotUser);

                if (!confirmationModel.ConfirmationWorkFlowUsers.Any(c => c.UserId == authenticate.UserId && c.IsBallInCourt))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);


                var userBallInCourtModel = confirmationModel.ConfirmationWorkFlowUsers.FirstOrDefault(a => a.IsBallInCourt && a.UserId == authenticate.UserId);
                userBallInCourtModel.DateEnd = DateTime.UtcNow;
                if (model.IsAccept)
                {
                    userBallInCourtModel.IsBallInCourt = false;
                    userBallInCourtModel.IsAccept = true;
                    userBallInCourtModel.Note = model.Note;
                    if (!confirmationModel.ConfirmationWorkFlowUsers.Any(a => a.IsAccept == false))
                    {
                        confirmationModel.Status = ConfirmationWorkFlowStatus.Confirm;
                        confirmationModel.DocumentRevision.RevisionStatus = RevisionStatus.Confirmed;
                        confirmationModel.DocumentRevision.IsLastConfirmRevision = true;
                        confirmationModel.DocumentRevision.RevisionPageNumber = confirmationModel.RevisionPageNumber;
                        confirmationModel.DocumentRevision.RevisionPageSize = confirmationModel.RevisionPageSize;

                        foreach (var item in confirmationModel.ConfirmationAttachments)
                        {
                            if (item.IsDeleted)
                                continue;
                            _revisionAttachmentRepository.Add(new RevisionAttachment
                            {
                                DocumentRevisionId = confirmationModel.DocumentRevisionId,
                                FileName = item.FileName,
                                FileSize = item.FileSize,
                                FileSrc = item.FileSrc,
                                RevisionAttachmentType = item.RevisionAttachmentType,
                                FileType = item.FileType,
                            });
                        }

                        var lastRevisions = await _documentRevisionRepository
                            .Where(a => a.IsLastConfirmRevision &&
                            a.DocumentRevisionId != confirmationModel.DocumentRevision.DocumentRevisionId &&
                            a.DocumentId == confirmationModel.DocumentRevision.DocumentId)
                            .ToListAsync();
                        foreach (var item in lastRevisions)
                        {
                            item.IsLastConfirmRevision = false;
                        }

                        confirmationModel.DocumentRevision.IsLastConfirmRevision = true;
                    }
                    else
                    {
                        var nextBallInCourtModel = confirmationModel.ConfirmationWorkFlowUsers.Where(a => !a.IsAccept)
                             .OrderBy(a => a.OrderNumber)
                             .FirstOrDefault();

                        nextBallInCourtModel.IsBallInCourt = true;
                        userBallInCourtModel.DateStart = DateTime.UtcNow;
                    }
                }
                else
                {
                    userBallInCourtModel.IsAccept = false;
                    userBallInCourtModel.Note = model.Note;
                    confirmationModel.Status = ConfirmationWorkFlowStatus.Reject;

                    confirmationModel.DocumentRevision.RevisionStatus = RevisionStatus.PendingForModify;
                }

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                   

                    await SendingLogAndNotificationTaskOnUserConfirmREvisionAsync(authenticate, confirmationModel, userBallInCourtModel);
                    if (confirmationModel.Status == ConfirmationWorkFlowStatus.Reject)
                    {
                        BackgroundJob.Enqueue(() => SendEmailForRejectDocumentRevisionAsync(authenticate, confirmationModel, model.Note));
                    }
                    if (confirmationModel.DocumentRevision.RevisionStatus == RevisionStatus.Confirmed)
                    {
                        BackgroundJob.Enqueue(() => SendEmailToReceiverUserAsync(authenticate, confirmationModel.DocumentRevision));
                    }
                    return ServiceResultFactory.CreateSuccess(true);
                }
                return ServiceResultFactory.CreateError(false, MessageId.SaveFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<bool> SendEmailForRejectDocumentRevisionAsync(AuthenticateDto authenticate, ConfirmationWorkFlow confirmationModel,string reason)
        {
            var revision = await _documentRevisionRepository.Include(d=>d.Document).Include(d => d.AdderUser).SingleOrDefaultAsync(d => d.DocumentRevisionId == confirmationModel.DocumentRevisionId);
            string url = $"/dashboard/documents/RevDetails/{confirmationModel.DocumentRevisionId}?team={authenticate.ContractCode}&wfid={confirmationModel.ConfirmationWorkFlowId}";
            string faMessage = $"در پروژه  <span style='direction: ltr'>{authenticate.ContractCode}</span> مدرک <span style='direction: ltr'>{revision.Document.DocTitle}</span> به شماره <span style='direction: ltr'>{revision.Document.DocNumber}</span>  به شماره ویرایش <span style='direction: ltr'>{revision.DocumentRevisionCode}</span> جهت اصلاح توسط کاربر {authenticate.UserName} در سیستم مدیریت پروژه رایبد بازگشت داده شد .";
            string enMessage = $"<div style='direction: ltr;text-align:left'>Document titled {revision.Document.DocTitle} - {revision.Document.DocNumber} - Rev. {revision.DocumentRevisionCode} has been rejected by {authenticate.UserFullName} for modification in document preparation.</div>";
            url = _appSettings.ClientHost + url;
            var dat = DateTime.UtcNow.ToPersianDate();
            var emailDTO = new RejectRevisionEmailDTO(reason, url, revision.AdderUser.FullName,_appSettings.CompanyName,faMessage,enMessage);

            var emalRequest = new EmailRequest
            {
                Attachment = null,
                Subject = $"Document Rejection | {revision.Document.DocNumber}",
                Body = await _viewRenderService.RenderToStringAsync("_RejectRevisionEmail", emailDTO),
            };

            emalRequest.To = new List<string> { revision.AdderUser.Email };
            await _appEmailService.SendAsync(emalRequest);
            return true;

        }
        public async Task<ServiceResult<bool>> SendEmailToReceiverUserAsync(AuthenticateDto authenticate, DocumentRevision revision)
        {

            var roles = new List<string> { SCMRole.RevisionMng };

            var users = await _authenticationServices.GetAllUserHasAccessDocumentAsync(authenticate.ContractCode, roles, revision.Document.DocumentGroupId);

            var tqEmailNotify = await _notifyRepository.Where(a => a.TeamWork.ContractCode == authenticate.ContractCode && a.NotifyType == NotifyManagementType.Email && a.NotifyNumber == (int)EmailNotify.RevisionConfirmation && a.IsActive).Select(a => a.UserId).ToListAsync();
            if (tqEmailNotify != null && tqEmailNotify.Any())
                users = users.Where(a => tqEmailNotify.Contains(a.Id)).ToList();
            else
                users = new List<UserMentionDto>();

            var user = await _userRepository.Where(a => a.Id == revision.AdderUserId).FirstOrDefaultAsync();
            if(user!=null)
            {
              
                string faBody = $"در پروژه <span style='direction:ltr'>{authenticate.ContractCode}</span>  ویرایش <span style='direction:ltr'>{revision.DocumentRevisionCode}</span> مدرک <span style='direction:ltr'>{revision.Document.DocTitle}</span> به شماره <span style='direction:ltr'>{revision.Document.DocNumber}</span> توسط کاربر {authenticate.UserFullName}  در سیستم مدیریت پروژه رایبد تائید نهایی شده است.";

                string enBody = $"<div style='direction:ltr'>Document titled {revision.Document.DocTitle} - {revision.Document.DocNumber} - Rev. {revision.DocumentRevisionCode} has  finalized by {authenticate.UserFullName} .</div>";
                var ccEmails =(users!=null&&users.Any())? users
               .Where(a => !string.IsNullOrEmpty(a.Email) && a.Email != user.Email)
               .Select(a => a.Email)
               .ToList():new List<string>();
                CommentMentionNotif model = new CommentMentionNotif(faBody, null, new List<CommentNotifViaEmailDTO>(), _appSettings.CompanyName, enBody);

                var emailRequest = new SendEmailDto
                {
                    To = user.Email,
                    Body = await _viewRenderService.RenderToStringAsync("_TransmittlaNotifEmail", model),
                    CCs = ccEmails,
                    Subject = $"Document Finalization | {revision.Document.DocNumber}-{revision.DocumentRevisionCode}"
                };
                await _appEmailService.SendTransmittalEmailAsync(emailRequest);
            }
            

            return ServiceResultFactory.CreateSuccess(true);
        }
        private async Task SendingLogAndNotificationTaskOnUserConfirmREvisionAsync(AuthenticateDto authenticate,
            ConfirmationWorkFlow confirmationModel, ConfirmationWorkFlowUser userBallInCourtModel)
        {
            await _scmLogAndNotificationService
               .SetDonedNotificationAsync(
               authenticate.UserId,
               authenticate.ContractCode,
               confirmationModel.DocumentRevisionId.ToString(),
               NotifEvent.BallInCourtRevisionConfirmation);

            if (userBallInCourtModel.IsAccept)
            {
                await SendingLogAndTaskOnAccepRevisionAsync(authenticate, confirmationModel);
            }
            else
            {
                await _scmLogAndNotificationService.SetDonedNotificationAsync(authenticate.UserId,
                    authenticate.ContractCode,
                    confirmationModel.DocumentRevisionId.ToString(),
                    NotifEvent.AddRevision);

                await _scmLogAndNotificationService.SetDonedNotificationAsync(authenticate.UserId,
                    authenticate.ContractCode,
                    confirmationModel.DocumentRevisionId.ToString(),
                    NotifEvent.RevisionReject);

                var logModel = new AddAuditLogDto
                {
                    ContractCode = authenticate.ContractCode,
                    Description = confirmationModel.DocumentRevision.Document.DocTitle,
                    FormCode = confirmationModel.DocumentRevision.DocumentRevisionCode,
                    KeyValue = confirmationModel.DocumentRevisionId.ToString(),
                    NotifEvent = NotifEvent.RevisionReject,
                    RootKeyValue = confirmationModel.DocumentRevisionId.ToString(),
                    PerformerUserId = authenticate.UserId,
                    PerformerUserFullName = authenticate.UserFullName,
                    DocumentGroupId = confirmationModel.DocumentRevision.Document.DocumentGroupId,
                };

                var taskModel = new List<NotifToDto>
                            {
                                new NotifToDto
                                {
                                    NotifEvent = NotifEvent.RevisionReject,
                                    Roles = new List<string>
                                    {
                                        SCMRole.RevisionMng
                                    }
                                }
                            };

                var res1 = await _scmLogAndNotificationService.AddDocumentAuditLogAsync(logModel, confirmationModel.DocumentRevision.Document.DocumentGroupId, taskModel);
            }
        }

        private async Task SendingLogAndTaskOnAccepRevisionAsync(AuthenticateDto authenticate, ConfirmationWorkFlow confirmationModel)
        {
            var logNotifEvent = confirmationModel.Status == ConfirmationWorkFlowStatus.Confirm ? NotifEvent.RevisionFinalConfirm : NotifEvent.RevisionAccept;

            var logModel = new AddAuditLogDto
            {
                ContractCode = authenticate.ContractCode,
                Description = confirmationModel.DocumentRevision.Document.DocTitle,
                FormCode = confirmationModel.DocumentRevision.DocumentRevisionCode,
                KeyValue = confirmationModel.DocumentRevisionId.ToString(),
                NotifEvent = logNotifEvent,
                RootKeyValue = confirmationModel.DocumentRevisionId.ToString(),
                PerformerUserId = authenticate.UserId,
                PerformerUserFullName = authenticate.UserFullName,
                DocumentGroupId = confirmationModel.DocumentRevision.Document.DocumentGroupId,
            };

            if (confirmationModel.Status == ConfirmationWorkFlowStatus.Confirm)
            {
                await _scmLogAndNotificationService.SetDonedNotificationAsync(authenticate.UserId,
                   authenticate.ContractCode,
                   confirmationModel.DocumentRevisionId.ToString(),
                   NotifEvent.AddRevision);

                await _scmLogAndNotificationService.SetDonedNotificationAsync(authenticate.UserId,
                    authenticate.ContractCode,
                    confirmationModel.DocumentRevisionId.ToString(),
                    NotifEvent.RevisionReject);
                List<NotifToDto> taskModel = null;
                if (confirmationModel.DocumentRevision.Document.IsRequiredTransmittal)
                {
                    taskModel = new List<NotifToDto>
                            {
                                new NotifToDto
                                {
                                    NotifEvent = NotifEvent.AddTransmittal,
                                    Roles = new List<string>
                                    {
                                        SCMRole.TransmittalMng
                                    }
                                }
                            };
                }
               

                var res1 = await _scmLogAndNotificationService.AddDocumentAuditLogAsync(logModel, confirmationModel.DocumentRevision.Document.DocumentGroupId, taskModel);
            }
            else
            {
                var nextBallInCourtModel = confirmationModel.ConfirmationWorkFlowUsers.Where(a => a.IsBallInCourt)
                .FirstOrDefault();

                var taskModel = new AddTaskNotificationDto
                {
                    ContractCode = authenticate.ContractCode,
                    Description = confirmationModel.DocumentRevision.Document.DocTitle,
                    FormCode = confirmationModel.DocumentRevision.DocumentRevisionCode,
                    KeyValue = confirmationModel.DocumentRevisionId.ToString(),
                    NotifEvent = NotifEvent.BallInCourtRevisionConfirmation,
                    RootKeyValue = confirmationModel.DocumentRevisionId.ToString(),
                    PerformerUserId = authenticate.UserId,
                    PerformerUserFullName = authenticate.UserFullName,
                    Users = new List<int>
                            {
                                nextBallInCourtModel.UserId
                            }
                };

                await _scmLogAndNotificationService.AddScmAuditLogAndTaskAsync(logModel, taskModel);
            }
        }

        public async Task<ServiceResult<ReportConfirmationWorkflowDto>> GetReportConfiemRevisionByRevIdAsync(AuthenticateDto authenticate, long revisionId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<ReportConfirmationWorkflowDto>(null, MessageId.AccessDenied);

                var dbQuery = _confirmationWorkFlowRepository
                     .AsNoTracking()
                     .Where(a => !a.IsDeleted &&
                    a.DocumentRevisionId == revisionId &&
                     a.DocumentRevision.Document.IsActive &&
                     a.DocumentRevision.Document.ContractCode == authenticate.ContractCode)
                     .OrderByDescending(a => a.ConfirmationWorkFlowId)
                     .AsQueryable();

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<ReportConfirmationWorkflowDto>(null, MessageId.EntityDoesNotExist);

                if (permission.DocumentGroupIds.Any() && !dbQuery.Any(a => permission.DocumentGroupIds.Contains(a.DocumentRevision.Document.DocumentGroupId)))
                    return ServiceResultFactory.CreateError<ReportConfirmationWorkflowDto>(null, MessageId.AccessDenied);

                var result = await dbQuery
                        .Select(x => new ReportConfirmationWorkflowDto
                        {
                            ConfirmNote = x.ConfirmNote,
                            RevisionPageNumber = x.RevisionPageNumber,
                            RevisionPageSize = x.RevisionPageSize,
                            ConfirmUserAudit = x.AdderUserId != null ? new UserAuditLogDto
                            {
                                CreateDate = x.CreatedDate.ToUnixTimestamp(),
                                AdderUserName = x.AdderUser.FullName,
                                AdderUserImage = x.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + x.AdderUser.Image : ""
                            } : null,
                            ConfirmationUserWorkFlows = x.ConfirmationWorkFlowUsers.Where(a => a.UserId > 0)
                            .Select(e => new ConfirmationUserWorkFlowDto
                            {
                                ConfirmationWorkFlowUserId = e.ConfirmationWorkFlowUserId,
                                DateEnd = e.DateEnd.ToUnixTimestamp(),
                                DateStart = e.DateStart.ToUnixTimestamp(),
                                IsAccept = e.IsAccept,
                                IsBallInCourt = e.IsBallInCourt,
                                Note = e.Note,
                                OrderNumber = e.OrderNumber,
                                UserId = e.UserId,
                                UserFullName = e.User != null ? e.User.FullName : "",
                                UserImage = e.User != null && e.User.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + e.User.Image : ""
                            }).ToList()
                        }).FirstOrDefaultAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<ReportConfirmationWorkflowDto>(null, exception);
            }
        }
        public async Task<ServiceResult<ReportConfirmationWorkflowDto>> GetReportConfiemRevisionByRevIdForCustomerUserAsync(AuthenticateDto authenticate, long revisionId,bool accessability)
        {
            try
            {
                if (!accessability)
                    return ServiceResultFactory.CreateError<ReportConfirmationWorkflowDto>(null, MessageId.AccessDenied);
               
               

                var dbQuery = _confirmationWorkFlowRepository
                     .AsNoTracking()
                     .Where(a => !a.IsDeleted &&
                    a.DocumentRevisionId == revisionId &&
                     a.DocumentRevision.Document.IsActive &&
                     a.DocumentRevision.Document.ContractCode == authenticate.ContractCode)
                     .OrderByDescending(a => a.ConfirmationWorkFlowId)
                     .AsQueryable();

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<ReportConfirmationWorkflowDto>(null, MessageId.EntityDoesNotExist);

                

                var result = await dbQuery
                        .Select(x => new ReportConfirmationWorkflowDto
                        {
                            ConfirmNote = x.ConfirmNote,
                            RevisionPageNumber = x.RevisionPageNumber,
                            RevisionPageSize = x.RevisionPageSize,
                            ConfirmUserAudit = x.AdderUserId != null ? new UserAuditLogDto
                            {
                                CreateDate = x.CreatedDate.ToUnixTimestamp(),
                                AdderUserName = x.AdderUser.FullName,
                                AdderUserImage = x.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + x.AdderUser.Image : ""
                            } : null,
                            ConfirmationUserWorkFlows = x.ConfirmationWorkFlowUsers.Where(a => a.UserId > 0)
                            .Select(e => new ConfirmationUserWorkFlowDto
                            {
                                ConfirmationWorkFlowUserId = e.ConfirmationWorkFlowUserId,
                                DateEnd = e.DateEnd.ToUnixTimestamp(),
                                DateStart = e.DateStart.ToUnixTimestamp(),
                                IsAccept = e.IsAccept,
                                IsBallInCourt = e.IsBallInCourt,
                                Note = e.Note,
                                OrderNumber = e.OrderNumber,
                                UserId = e.UserId,
                                UserFullName = e.User != null ? e.User.FullName : "",
                                UserImage = e.User != null && e.User.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + e.User.Image : ""
                            }).ToList()
                        }).FirstOrDefaultAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<ReportConfirmationWorkflowDto>(null, exception);
            }
        }

        public async Task<ServiceResult<List<ReportConfirmationWorkflowDto>>> GetReportRevisionConfirmationWorkFlowByRevIdAsync(AuthenticateDto authenticate, long revisionId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<ReportConfirmationWorkflowDto>>(null, MessageId.AccessDenied);

                var dbQuery = _confirmationWorkFlowRepository
                     .AsNoTracking()
                     .Where(a => !a.IsDeleted &&
                     a.DocumentRevisionId == revisionId &&
                     a.DocumentRevision.Document.IsActive &&
                     a.DocumentRevision.Document.ContractCode == authenticate.ContractCode)
                     .OrderByDescending(a => a.ConfirmationWorkFlowId)
                     .AsQueryable();

                if (permission.DocumentGroupIds.Any() && !dbQuery.Any(a => permission.DocumentGroupIds.Contains(a.DocumentRevision.Document.DocumentGroupId)))
                    return ServiceResultFactory.CreateError<List<ReportConfirmationWorkflowDto>>(null, MessageId.AccessDenied);

                var result = await dbQuery
                        .Select(x => new ReportConfirmationWorkflowDto
                        {
                            ConfirmationWorkFlowId = x.ConfirmationWorkFlowId,
                            ConfirmNote = x.ConfirmNote,
                            RevisionPageNumber = x.RevisionPageNumber,
                            RevisionPageSize = x.RevisionPageSize,
                            Status = x.Status,
                            ConfirmUserAudit = x.AdderUserId != null ? new UserAuditLogDto
                            {
                                CreateDate = x.CreatedDate.ToUnixTimestamp(),
                                AdderUserName = x.AdderUser.FullName,
                                AdderUserImage = x.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + x.AdderUser.Image : ""
                            } : null,
                            FinalAttachments = x.ConfirmationAttachments.Where(m => !m.IsDeleted && m.RevisionAttachmentType == RevisionAttachmentType.Final)
                            .Select(c => new RevisionAttachmentDto
                            {
                                AttachType = c.RevisionAttachmentType,
                                FileName = c.FileName,
                                FileSize = c.FileSize,
                                FileSrc = c.FileSrc,
                                FileType = c.FileType,
                                RevisionAttachmentId = c.RevisionAttachmentId
                            }).ToList(),
                            FinalNativeAttachments = x.ConfirmationAttachments.Where(n => !n.IsDeleted && n.RevisionAttachmentType == RevisionAttachmentType.FinalNative)
                            .Select(c => new RevisionAttachmentDto
                            {
                                AttachType = c.RevisionAttachmentType,
                                FileName = c.FileName,
                                FileSize = c.FileSize,
                                FileSrc = c.FileSrc,
                                FileType = c.FileType,
                                RevisionAttachmentId = c.RevisionAttachmentId
                            }).ToList(),
                            ConfirmationUserWorkFlows = x.ConfirmationWorkFlowUsers.Where(a => a.UserId > 0)
                            .Select(e => new ConfirmationUserWorkFlowDto
                            {
                                ConfirmationWorkFlowUserId = e.ConfirmationWorkFlowUserId,
                                DateEnd = e.DateEnd.ToUnixTimestamp(),
                                DateStart = e.DateStart.ToUnixTimestamp(),
                                IsAccept = e.IsAccept,
                                IsBallInCourt = e.IsBallInCourt,
                                Note = e.Note,
                                OrderNumber = e.OrderNumber,
                                UserId = e.UserId,
                                UserFullName = e.User != null ? e.User.FullName : "",
                                UserImage = e.User != null && e.User.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + e.User.Image : ""
                            }).ToList()
                        }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<ReportConfirmationWorkflowDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<List<ListConfirmationUserWorkFlowDto>>> GetReportRevisionConfirmationWorkFlowUserByRevIdAsync(AuthenticateDto authenticate, long revisionId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<ListConfirmationUserWorkFlowDto>>(null, MessageId.AccessDenied);

                var dbQuery = _confirmationWorkFlowRepository
                     .AsNoTracking()
                     .Where(a => !a.IsDeleted &&
                     a.DocumentRevisionId == revisionId &&
                     a.DocumentRevision.Document.IsActive &&
                     a.DocumentRevision.Document.ContractCode == authenticate.ContractCode)
                     .OrderByDescending(a => a.ConfirmationWorkFlowId)
                     .AsQueryable();

                if (permission.DocumentGroupIds.Any() && !dbQuery.Any(a => permission.DocumentGroupIds.Contains(a.DocumentRevision.Document.DocumentGroupId)))
                    return ServiceResultFactory.CreateError<List<ListConfirmationUserWorkFlowDto>>(null, MessageId.AccessDenied);

                var result = await dbQuery
                        .Select(x => new ListConfirmationUserWorkFlowDto
                        {
                            ConfirmNote = x.ConfirmNote,
                            ConfirmUserAudit = x.AdderUserId != null ? new UserAuditLogDto
                            {
                                CreateDate = x.CreatedDate.ToUnixTimestamp(),
                                AdderUserName = x.AdderUser.FullName,
                                AdderUserImage = x.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + x.AdderUser.Image : ""
                            } : null,
                            ConfirmationUserWorkFlows = x.ConfirmationWorkFlowUsers.Where(a => a.UserId > 0)
                            .Select(e => new ConfirmationUserWorkFlowDto
                            {
                                ConfirmationWorkFlowUserId = e.ConfirmationWorkFlowUserId,
                                DateEnd = e.DateEnd.ToUnixTimestamp(),
                                DateStart = e.DateStart.ToUnixTimestamp(),
                                IsAccept = e.IsAccept,
                                IsBallInCourt = e.IsBallInCourt,
                                Note = e.Note,
                                OrderNumber = e.OrderNumber,
                                UserId = e.UserId,
                                UserFullName = e.User != null ? e.User.FullName : "",
                                UserImage = e.User != null && e.User.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + e.User.Image : ""
                            }).ToList()
                        }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<ListConfirmationUserWorkFlowDto>>(null, exception);
            }
        }


    }
}

