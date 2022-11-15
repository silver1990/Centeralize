using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Raybod.SCM.DataAccess.Core;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.Domain.Model;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Utility.Extention;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Raybod.SCM.DataAccess.Extention;
using Raybod.SCM.Services.Utilitys.MailService.Model;
using Raybod.SCM.Services.Utilitys.MailService;
using Raybod.SCM.DataTransferObject.User;
using Raybod.SCM.DataTransferObject.Document.Communication;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.DataTransferObject.Audit;
using Raybod.SCM.DataTransferObject._PanelDocument.Communication.Comment;
using Hangfire;
using Raybod.SCM.DataTransferObject.Email;
using Microsoft.AspNetCore.Http;

namespace Raybod.SCM.Services.Implementation
{
    public class CommunicationTeamCommentService : ICommunicationTeamCommentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITeamWorkAuthenticationService _authenticationService;
        private readonly ISCMLogAndNotificationService _scmLogAndNotificationService;
        private readonly IAppEmailService _appEmailService;
        private readonly DbSet<DocumentCommunication> _documentCommunicationRepository;
        private readonly DbSet<DocumentTQNCR> _documentTQNCRRepository;
        private readonly DbSet<CommunicationTeamComment> _communicationTeamCommentRepository;
        private readonly DbSet<User> _userRepository;
        private readonly Raybod.SCM.Services.Utilitys.FileHelper _fileHelper;
        private readonly CompanyConfig _appSettings;
        private readonly IViewRenderService _viewRenderService;
        public CommunicationTeamCommentService(
            IUnitOfWork unitOfWork,
            IWebHostEnvironment hostingEnvironmentRoot,
            ITeamWorkAuthenticationService authenticationService,
            IOptions<CompanyAppSettingsDto> appSettings,
            IHttpContextAccessor httpContextAccessor,
            ISCMLogAndNotificationService scmLogAndNotificationService,
            IAppEmailService appEmailService, IViewRenderService viewRenderService)
        {
            _unitOfWork = unitOfWork;
            _authenticationService = authenticationService;
            _scmLogAndNotificationService = scmLogAndNotificationService;
            _appEmailService = appEmailService;
            _fileHelper = new Utilitys.FileHelper(hostingEnvironmentRoot);
            _communicationTeamCommentRepository = _unitOfWork.Set<CommunicationTeamComment>();
            _documentCommunicationRepository = _unitOfWork.Set<DocumentCommunication>();
            _documentTQNCRRepository = _unitOfWork.Set<DocumentTQNCR>();
            _userRepository = _unitOfWork.Set<User>();
            _viewRenderService = viewRenderService;
            httpContextAccessor.HttpContext.Request.Headers.TryGetValue("companyCode", out var CompanyCode);
            _appSettings = appSettings.Value.CompanyConfig.First(a => a.CompanyCode == CompanyCode);
        }

        public async Task<ServiceResult<string>> AddCommentAsync(AuthenticateDto authenticate,
            long communicationId, AddCommunicationTeamCommentDto model)
        {
            try
            {
                //var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                //if (!permission.HasPermission)
                //    return ServiceResultFactory.CreateError("", MessageId.AccessDenied);

                var dbQuery = _documentCommunicationRepository
                    .AsNoTracking()
                    .Where(a => a.DocumentCommunicationId == communicationId &&
                    a.DocumentRevision.Document.ContractCode == authenticate.ContractCode);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError("", MessageId.EntityDoesNotExist);

                //if (permission.DocumentGroupIds.Any() && !dbQuery.Any(c => permission.DocumentGroupIds.Contains(c.DocumentRevision.Document.DocumentGroupId)))
                //    return ServiceResultFactory.CreateError("", MessageId.AccessDenied);

                var communicationModel = await dbQuery
                    .AsNoTracking()
                    .Select(c => new
                    {
                        communicationId = c.DocumentCommunicationId,
                        CommunicationCode = c.CommunicationCode,
                        DocumentId = c.DocumentRevision.DocumentId,
                        DocumentGroupId = c.DocumentRevision.Document.DocumentGroupId,
                        DocTitle = c.DocumentRevision.Document.DocTitle,
                        DocNumber = c.DocumentRevision.Document.DocNumber,
                        RevisionId = c.DocumentRevisionId
                    })
                    .FirstOrDefaultAsync();

                if (communicationModel == null)
                    return ServiceResultFactory.CreateError("", MessageId.EntityDoesNotExist);

                var commentModel = new CommunicationTeamComment();

                bool isSetTaskDone = false;
                if (model.ParentCommentId != null && model.ParentCommentId > 0)
                {
                    var parentComment = await _communicationTeamCommentRepository.Where(a => !a.IsDeleted && a.DocumentCommunicationId == communicationId
                     && a.CommunicationTeamCommentId == model.ParentCommentId.Value && a.ParentCommentId == null)
                        .Select(c => new
                        {
                            communicationTeamCommentId = c.CommunicationTeamCommentId,
                            isHasAnswer = c.ReplayComments.Any()
                        }).FirstOrDefaultAsync();

                    isSetTaskDone = !parentComment.isHasAnswer;
                    if (parentComment == null)
                        return ServiceResultFactory.CreateError("", MessageId.EntityDoesNotExist);
                    commentModel.ParentCommentId = model.ParentCommentId.Value;
                }

                List<string> mentionUserEmails = new List<string>();

                if (model.UserIds != null && model.UserIds.Any(a => a > 0))
                {
                    commentModel.CommentUsers = new List<CommunicationTeamCommentUser>();

                    var userIds = model.UserIds.Where(a => a > 0).Select(u => u).Distinct().ToList();
                    foreach (var item in userIds)
                    {
                        commentModel.CommentUsers.Add(new CommunicationTeamCommentUser
                        {
                            UserId = item
                        });
                    }

                    mentionUserEmails = await _userRepository
                        .Where(a => !a.IsDeleted && a.IsActive && userIds.Contains(a.Id) && !string.IsNullOrEmpty(a.Email))
                        .Select(c => c.Email)
                        .ToListAsync();
                }

                commentModel.DocumentCommunicationId = communicationId;
                commentModel.Message = model.Message;

                if (model.Attachments != null && model.Attachments.Count() > 0)
                {
                    if (!_fileHelper.FileExistInTemp(model.Attachments.Select(c => c.FileSrc).ToList()))
                        return ServiceResultFactory.CreateError("", MessageId.FileNotFound);
                    var attachmentResult = await AddCommentAttachmentAsync(authenticate, communicationModel.DocumentId, communicationModel.RevisionId, commentModel, model.Attachments);
                    if (!attachmentResult.Succeeded)
                        return ServiceResultFactory.CreateError("",
                            attachmentResult.Messages.FirstOrDefault().Message);

                    commentModel = attachmentResult.Result;
                }

                _communicationTeamCommentRepository.Add(commentModel);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                  
                    var notifEvent = model.ParentCommentId == null || model.ParentCommentId <= 0 ? NotifEvent.AddComTeamComment : NotifEvent.AddComTeamCommentReply;
                    var res = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = authenticate.ContractCode,
                        RootKeyValue = communicationModel.communicationId.ToString(),
                        Description = "",
                        FormCode = communicationModel.CommunicationCode,
                        KeyValue = commentModel.CommunicationTeamCommentId.ToString(),
                        NotifEvent = notifEvent,
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,
                        DocumentGroupId = communicationModel.DocumentGroupId
                    }, null);

                    if (commentModel.CommentUsers != null && commentModel.CommentUsers.Any())
                        await _scmLogAndNotificationService.AddMentionNotificationTaskAsync(new AddMentionLogDto
                        {
                            ContractCode = authenticate.ContractCode,
                            RootKeyValue = communicationModel.communicationId.ToString(),
                            Message = "کامنت",
                            Description = communicationModel.DocTitle,
                            FormCode = communicationModel.CommunicationCode,
                            KeyValue = commentModel.CommunicationTeamCommentId.ToString(),
                            MentionEvent = MentionEvent.MentionCommentTeamComment,
                            PerformerUserId = authenticate.UserId,
                            PerformerUserFullName = authenticate.UserFullName,
                            DocumentGroupId = communicationModel.DocumentGroupId,
                            ReceiverLogUserIds = commentModel.CommentUsers.Select(a => a.UserId).ToList()
                        });

                    if (mentionUserEmails != null && mentionUserEmails.Any())
                    {
                        List<CommentNotifViaEmailDTO> comments = new List<CommentNotifViaEmailDTO>();
                        if (commentModel.ParentCommentId != null)
                        {

                            comments = await _communicationTeamCommentRepository.OrderBy(c => c.CommunicationTeamCommentId).Where(c => c.ParentCommentId == commentModel.ParentCommentId || c.CommunicationTeamCommentId == commentModel.ParentCommentId).Select(c => new CommentNotifViaEmailDTO
                            {
                                Message = c.Message,
                                SendDate = c.CreatedDate.ToJalaliWithTime(),
                                SenderName = c.AdderUser.FullName
                            }).ToListAsync();
                        }
                        else
                        {

                            comments = await _communicationTeamCommentRepository.OrderBy(c => c.CommunicationTeamCommentId).Where(c => c.ParentCommentId == commentModel.CommunicationTeamCommentId || c.CommunicationTeamCommentId == commentModel.CommunicationTeamCommentId).Select(c => new CommentNotifViaEmailDTO
                            {
                                Message = c.Message,
                                SendDate = c.CreatedDate.ToJalaliWithTime(),
                                SenderName = c.AdderUser.FullName
                            }).ToListAsync();
                        }
                        try
                        {
                            BackgroundJob.Enqueue(() => SendEmailForUserMentionAsync(communicationId, communicationModel.CommunicationCode, authenticate.ContractCode, CommunicationType.Comment, mentionUserEmails, comments, commentModel.CommunicationTeamCommentId));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }


                    return ServiceResultFactory.CreateSuccess(commentModel.CommunicationTeamCommentId.ToString());
                }
                return ServiceResultFactory.CreateError("", MessageId.AddEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException("", exception);
            }
        }

        public async Task<ServiceResult<List<CommunicationTeamCommentListDto>>> GetCommentAsync(AuthenticateDto authenticate,
            long communicationId, CommunicationTeamCommentQueryDto query)
        {
            try
            {
                //var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                //if (!permission.HasPermission)
                //    return ServiceResultFactory.CreateError<List<CommunicationTeamCommentListDto>>(null, MessageId.AccessDenied);

                var dbQuery = _communicationTeamCommentRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted &&
                    a.DocumentCommunicationId == communicationId &&
                    a.ParentCommentId == null &&
                    a.Communication.DocumentRevision.Document.ContractCode == authenticate.ContractCode)
                    .OrderByDescending(a => a.CommunicationTeamCommentId)
                    .AsQueryable();

                //if (permission.DocumentGroupIds.Any() && !dbQuery.Any(c => permission.DocumentGroupIds.Contains(c.Communication.DocumentRevision.Document.DocumentGroupId)))
                //    return ServiceResultFactory.CreateError<List<CommunicationTeamCommentListDto>>(null, MessageId.AccessDenied);

                var totalCount = dbQuery.Count();
                dbQuery = dbQuery.ApplayPageing(query);

                var result = await dbQuery.Select(c => new CommunicationTeamCommentListDto
                {
                    CommentId = c.CommunicationTeamCommentId,
                    Message = c.Message,
                    ParentCommentId = c.ParentCommentId,
                    Attachments = c.Attachments.Where(a => !a.IsDeleted)
                     .Select(a => new CommunicationAttachmentDto
                     {
                         AttachmentId = a.CommunicationAttachmentId,
                         FileName = a.FileName,
                         FileSize = a.FileSize,
                         FileSrc = a.FileSrc,
                         FileType = a.FileType,
                     }).ToList(),

                    UserAudit = c.AdderUser != null ? new UserAuditLogDto
                    {
                        AdderUserId = c.AdderUserId,
                        AdderUserName = c.AdderUser.FullName,
                        CreateDate = c.CreatedDate.ToUnixTimestamp(),
                        AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + c.AdderUser.Image
                    } : null,
                    UserMentions = c.CommentUsers.Select(u => new UserMiniInfoDto
                    {
                        UserFullName = u.User.FullName,
                        UserId = u.UserId,
                        UserName = u.User.UserName
                    }).ToList(),

                    ReplayComments = c.ReplayComments.Where(v => !v.IsDeleted)
                     .Select(v => new CommunicationTeamCommentListDto
                     {
                         CommentId = v.CommunicationTeamCommentId,
                         Message = v.Message,
                         ParentCommentId = v.ParentCommentId,
                         Attachments = v.Attachments.Where(b => !b.IsDeleted)
                         .Select(b => new CommunicationAttachmentDto
                         {
                             AttachmentId = b.CommunicationAttachmentId,
                             FileName = b.FileName,
                             FileSrc = b.FileSrc,
                             FileSize = b.FileSize,
                             FileType = b.FileType,
                         }).ToList(),
                         UserAudit = v.AdderUser != null ? new UserAuditLogDto
                         {
                             AdderUserId = v.AdderUserId,
                             AdderUserName = v.AdderUser.FullName,
                             CreateDate = v.CreatedDate.ToUnixTimestamp(),
                             AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + v.AdderUser.Image
                         } : null,
                         UserMentions = v.CommentUsers.Select(u => new UserMiniInfoDto
                         {
                             UserFullName = u.User.FullName,
                             UserId = u.UserId,
                             UserName = u.User.UserName
                         }).ToList(),
                     }).ToList()
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<CommunicationTeamCommentListDto>>(null, exception);
            }
        }


        private async Task<ServiceResult<CommunicationTeamComment>> AddCommentAttachmentAsync(AuthenticateDto authenticate,
            long documentId, long revId, CommunicationTeamComment commentModel, List<AddRevisionAttachmentDto> attachment)
        {
            commentModel.Attachments = new List<CommunicationAttachment>();

            foreach (var item in attachment)
            {
                var UploadedFile =
                    await _fileHelper.SaveDocumentFromTemp(item.FileSrc, ServiceSetting.UploadFilePathRevisionCommunication(authenticate.ContractCode, documentId, revId));
                if (UploadedFile == null)
                    return ServiceResultFactory.CreateError<CommunicationTeamComment>(null, MessageId.UploudFailed);

                _fileHelper.DeleteDocumentFromTemp(item.FileSrc);
                commentModel.Attachments.Add(new CommunicationAttachment
                {
                    FileName = item.FileName,
                    FileSrc = item.FileSrc,
                    FileType = UploadedFile.FileType,
                    FileSize = UploadedFile.FileSize
                });
            }
            return ServiceResultFactory.CreateSuccess(commentModel);
        }

        #region TQ an NCR
        public async Task<ServiceResult<string>> AddTQAndNCRCommentAsync(AuthenticateDto authenticate,
            long communicationId, CommunicationType communicationType, AddCommunicationTeamCommentDto model)
        {
            try
            {
                //var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                //if (!permission.HasPermission)
                //    return ServiceResultFactory.CreateError("", MessageId.AccessDenied);

                var dbQuery = _documentTQNCRRepository
                    .AsNoTracking()
                    .Where(a => a.CommunicationType == communicationType && a.DocumentTQNCRId == communicationId &&
                    a.DocumentRevision.Document.ContractCode == authenticate.ContractCode);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError("", MessageId.EntityDoesNotExist);

                //if (permission.DocumentGroupIds.Any() && !dbQuery.Any(c => permission.DocumentGroupIds.Contains(c.DocumentRevision.Document.DocumentGroupId)))
                //    return ServiceResultFactory.CreateError("", MessageId.AccessDenied);

                var communicationModel = await dbQuery
                    .AsNoTracking()
                    .Select(c => new
                    {
                        communicationId = c.DocumentTQNCRId,
                        CommunicationCode = c.CommunicationCode,
                        DocumentId = c.DocumentRevision.DocumentId,
                        DocumentGroupId = c.DocumentRevision.Document.DocumentGroupId,
                        DocTitle = c.DocumentRevision.Document.DocTitle,
                        DocNumber = c.DocumentRevision.Document.DocNumber,
                        RevisionId = c.DocumentRevisionId
                    }).FirstOrDefaultAsync();

                if (communicationModel == null)
                    return ServiceResultFactory.CreateError("", MessageId.EntityDoesNotExist);

                var commentModel = new CommunicationTeamComment();

                bool isSetTaskDone = false;
                if (model.ParentCommentId != null && model.ParentCommentId > 0)
                {
                    var parentComment = await _communicationTeamCommentRepository.Where(a => !a.IsDeleted && a.DocumentTQNCRId == communicationId
                     && a.CommunicationTeamCommentId == model.ParentCommentId.Value && a.ParentCommentId == null)
                        .Select(c => new
                        {
                            communicationTeamCommentId = c.CommunicationTeamCommentId,
                            isHasAnswer = c.ReplayComments.Any()
                        }).FirstOrDefaultAsync();

                    isSetTaskDone = !parentComment.isHasAnswer;
                    if (parentComment == null)
                        return ServiceResultFactory.CreateError("", MessageId.EntityDoesNotExist);
                    commentModel.ParentCommentId = model.ParentCommentId.Value;
                }

                List<string> mentionUserEmails = new List<string>();

                if (model.UserIds != null && model.UserIds.Any(a => a > 0))
                {
                    commentModel.CommentUsers = new List<CommunicationTeamCommentUser>();

                    var userIds = model.UserIds.Where(a => a > 0).Select(u => u).Distinct().ToList();
                    foreach (var item in userIds)
                    {
                        commentModel.CommentUsers.Add(new CommunicationTeamCommentUser
                        {
                            UserId = item
                        });
                    }

                    mentionUserEmails = await _userRepository
                        .Where(a => !a.IsDeleted && a.IsActive && userIds.Contains(a.Id) && !string.IsNullOrEmpty(a.Email))
                        .Select(c => c.Email)
                        .ToListAsync();
                }

                commentModel.DocumentTQNCRId = communicationId;
                commentModel.Message = model.Message;

                if (model.Attachments != null && model.Attachments.Count() > 0)
                {
                    if (!_fileHelper.FileExistInTemp(model.Attachments.Select(c => c.FileSrc).ToList()))
                        return ServiceResultFactory.CreateError("", MessageId.FileNotFound);
                    var attachmentResult = await AddCommentAttachmentAsync(authenticate, communicationModel.DocumentId, communicationModel.RevisionId, commentModel, model.Attachments);
                    if (!attachmentResult.Succeeded)
                        return ServiceResultFactory.CreateError("",
                            attachmentResult.Messages.FirstOrDefault().Message);

                    commentModel = attachmentResult.Result;
                }

                _communicationTeamCommentRepository.Add(commentModel);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    NotifEvent notifEvent;
                    if (communicationType == CommunicationType.TQ)
                    {
                        notifEvent = model.ParentCommentId == null || model.ParentCommentId <= 0 ? NotifEvent.AddTQTeamComment : NotifEvent.AddTQTeamCommentReply;
                    }
                    else
                    {
                        notifEvent = model.ParentCommentId == null || model.ParentCommentId <= 0 ? NotifEvent.AddNCRTeamComment : NotifEvent.AddNCRTeamCommentReply;
                    }
                    

                    var res = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = authenticate.ContractCode,
                        RootKeyValue = communicationModel.communicationId.ToString(),
                        Description = "",
                        FormCode = communicationModel.CommunicationCode,
                        KeyValue = commentModel.CommunicationTeamCommentId.ToString(),
                        NotifEvent = notifEvent,
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,
                        DocumentGroupId = communicationModel.DocumentGroupId,
                
                    }, null);

                    if (commentModel.CommentUsers != null && commentModel.CommentUsers.Any())
                        await _scmLogAndNotificationService.AddMentionNotificationTaskAsync(new AddMentionLogDto
                        {
                            ContractCode = authenticate.ContractCode,
                            RootKeyValue = communicationModel.communicationId.ToString(),
                            Message = communicationType == CommunicationType.TQ ? "TQ" : "NCR",
                            Description = communicationModel.DocTitle,
                            FormCode = communicationModel.CommunicationCode,
                            KeyValue = commentModel.CommunicationTeamCommentId.ToString(),
                            MentionEvent = communicationType == CommunicationType.TQ ? MentionEvent.MentionTQTeamComment:MentionEvent.MentionNCRTeamComment,
                            PerformerUserId = authenticate.UserId,
                            PerformerUserFullName = authenticate.UserFullName,
                            DocumentGroupId = communicationModel.DocumentGroupId,
                            ReceiverLogUserIds = commentModel.CommentUsers.Select(a => a.UserId).ToList(),
                 
                        });

                    if (mentionUserEmails != null && mentionUserEmails.Any())
                    {
                        List<CommentNotifViaEmailDTO> comments = new List<CommentNotifViaEmailDTO>();
                        if (commentModel.ParentCommentId != null)
                        {

                            comments = await _communicationTeamCommentRepository.OrderBy(c => c.CommunicationTeamCommentId).Where(c => c.ParentCommentId == commentModel.ParentCommentId || c.CommunicationTeamCommentId == commentModel.ParentCommentId).Select(c => new CommentNotifViaEmailDTO
                            {
                                Message = c.Message,
                                SendDate = c.CreatedDate.ToJalaliWithTime(),
                                SenderName = c.AdderUser.FullName
                            }).ToListAsync();
                        }
                        else
                        {

                            comments = await _communicationTeamCommentRepository.OrderBy(c => c.CommunicationTeamCommentId).Where(c => c.ParentCommentId == commentModel.CommunicationTeamCommentId || c.CommunicationTeamCommentId == commentModel.CommunicationTeamCommentId).Select(c => new CommentNotifViaEmailDTO
                            {
                                Message = c.Message,
                                SendDate = c.CreatedDate.ToJalaliWithTime(),
                                SenderName = c.AdderUser.FullName
                            }).ToListAsync();
                        }
                        try
                        {
                            BackgroundJob.Enqueue(() => SendEmailForUserMentionAsync(communicationId, communicationModel.CommunicationCode, authenticate.ContractCode, communicationType, mentionUserEmails, comments, commentModel.CommunicationTeamCommentId));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }


                    return ServiceResultFactory.CreateSuccess(commentModel.CommunicationTeamCommentId.ToString());
                }
                return ServiceResultFactory.CreateError("", MessageId.AddEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException("", exception);
            }
        }

        public async Task<ServiceResult<List<CommunicationTeamCommentListDto>>> GetTQAndNCRCommentAsync(AuthenticateDto authenticate,
            long communicationId, CommunicationTeamCommentQueryDto query)
        {
            try
            {
                //var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                //if (!permission.HasPermission)
                //    return ServiceResultFactory.CreateError<List<CommunicationTeamCommentListDto>>(null, MessageId.AccessDenied);

                var dbQuery = _communicationTeamCommentRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted &&
                    a.DocumentTQNCRId == communicationId &&
                    a.DocumentTQNCR.CommunicationType == query.Type &&
                    a.ParentCommentId == null &&
                    a.DocumentTQNCR.DocumentRevision.Document.ContractCode == authenticate.ContractCode)
                    .OrderByDescending(a => a.CommunicationTeamCommentId)
                    .AsQueryable();

                //if (permission.DocumentGroupIds.Any() && !dbQuery.Any(c => permission.DocumentGroupIds.Contains(c.DocumentTQNCR.DocumentRevision.Document.DocumentGroupId)))
                //    return ServiceResultFactory.CreateError<List<CommunicationTeamCommentListDto>>(null, MessageId.AccessDenied);



                var totalCount = dbQuery.Count();
                dbQuery = dbQuery.ApplayPageing(query);

                var result = await dbQuery.Select(c => new CommunicationTeamCommentListDto
                {
                    CommentId = c.CommunicationTeamCommentId,
                    Message = c.Message,
                    ParentCommentId = c.ParentCommentId,
                    Attachments = c.Attachments.Where(a => !a.IsDeleted)
                     .Select(a => new CommunicationAttachmentDto
                     {
                         AttachmentId = a.CommunicationAttachmentId,
                         FileName = a.FileName,
                         FileSize = a.FileSize,
                         FileSrc = a.FileSrc,
                         FileType = a.FileType,
                     }).ToList(),

                    UserAudit = c.AdderUser != null ? new UserAuditLogDto
                    {
                        AdderUserId = c.AdderUserId,
                        AdderUserName = c.AdderUser.FullName,
                        CreateDate = c.CreatedDate.ToUnixTimestamp(),
                        AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + c.AdderUser.Image
                    } : null,
                    UserMentions = c.CommentUsers.Select(u => new UserMiniInfoDto
                    {
                        UserFullName = u.User.FullName,
                        UserId = u.UserId,
                        UserName = u.User.UserName
                    }).ToList(),

                    ReplayComments = c.ReplayComments.Where(v => !v.IsDeleted)
                     .Select(v => new CommunicationTeamCommentListDto
                     {
                         CommentId = v.CommunicationTeamCommentId,
                         Message = v.Message,
                         ParentCommentId = v.ParentCommentId,
                         Attachments = v.Attachments.Where(b => !b.IsDeleted)
                         .Select(b => new CommunicationAttachmentDto
                         {
                             AttachmentId = b.CommunicationAttachmentId,
                             FileName = b.FileName,
                             FileSrc = b.FileSrc,
                             FileSize = b.FileSize,
                             FileType = b.FileType,
                         }).ToList(),
                         UserAudit = v.AdderUser != null ? new UserAuditLogDto
                         {
                             AdderUserId = v.AdderUserId,
                             AdderUserName = v.AdderUser.FullName,
                             CreateDate = v.CreatedDate.ToUnixTimestamp(),
                             AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + v.AdderUser.Image
                         } : null,
                         UserMentions = v.CommentUsers.Select(u => new UserMiniInfoDto
                         {
                             UserFullName = u.User.FullName,
                             UserId = u.UserId,
                             UserName = u.User.UserName
                         }).ToList(),
                     }).ToList()
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<CommunicationTeamCommentListDto>>(null, exception);
            }
        }

        #endregion

        public async Task<bool> SendEmailForUserMentionAsync(long communicationId, string communicationCode, string contractCode, CommunicationType type, List<string> emails, List<CommentNotifViaEmailDTO> comments,long targetId)
        {
            string description = "";
            string url = "";
            string subject = "";
            switch (type)
            {
                case CommunicationType.Comment:
                    description = $"کامنت <span style='direction:ltr'>{communicationCode}</span>";
                    url = $"/dashboard/documents/communication/commentReply/{communicationId}?team={contractCode}&targetId={targetId}";
                    subject = $"Mention | {communicationCode}";
                    break;
                case CommunicationType.TQ:
                    description = $"TQ <span style='direction:ltr'>{communicationCode}</span>";
                    url = $"/dashboard/documents/communication/tqReply/{communicationId}?team={contractCode}&targetId={targetId}";
                    subject = $"Mention | {communicationCode}";
                    break;
                case CommunicationType.NCR:
                    description = $"NCR <span style='direction:ltr'>{communicationCode}</span>";
                    url = $"/dashboard/documents/communication/ncrReply/{communicationId}?team={contractCode}&targetId={targetId}";
                    subject = $"Mention | {communicationCode}";
                    break;
                default:
                    break;
            }

            url = _appSettings.ClientHost + url;
            string faMessage = $"به نام شما در {description} اشاره شده است.برای مشاهده و پاسخ پیام رو دکمه زیر کلیک کنید";
            string enMessage = $"<div style='direction:ltr;text-align:left'>You are mentioned in {type.GetDisplayName()} No. {communicationCode}.Click button below to view.</div>";
            CommentMentionNotif model = new CommentMentionNotif(faMessage, url, comments,_appSettings.CompanyName,enMessage);
            var emalRequest = new EmailRequest
            {
                Attachment = null,
                Subject = subject,
                Body = await _viewRenderService.RenderToStringAsync("_SendCommentMentionNotifViaEmail", model),
               
            };

            foreach (var address in emails)
            {
                emalRequest.To = new List<string> { address };
                await _appEmailService.SendAsync(emalRequest);
            }
            return true;
        }

    }
}
