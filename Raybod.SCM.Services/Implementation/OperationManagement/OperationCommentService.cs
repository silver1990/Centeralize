using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Raybod.SCM.DataAccess.Core;
using Raybod.SCM.DataAccess.Extention;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject._PanelDocument.Communication.Comment;
using Raybod.SCM.DataTransferObject._PanelOperation;
using Raybod.SCM.DataTransferObject.Audit;
using Raybod.SCM.DataTransferObject.Document;
using Raybod.SCM.DataTransferObject.Email;
using Raybod.SCM.DataTransferObject.OperationComment;
using Raybod.SCM.DataTransferObject.User;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Model;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Services.Utilitys.MailService;
using Raybod.SCM.Services.Utilitys.MailService.Model;
using Raybod.SCM.Utility.Extention;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Implementation
{
    public class OperationCommentService : IOperationCommentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITeamWorkAuthenticationService _authenticationService;
        private readonly ISCMLogAndNotificationService _scmLogAndNotificationService;
        private readonly IAppEmailService _appEmailService;
        private readonly DbSet<Operation> _operationRepository;
        private readonly DbSet<OperationComment> _operationCommentRepository;
        private readonly DbSet<OperationAttachment> _operationAttachmentRepository;
        private readonly DbSet<User> _userRepository;
        private readonly Raybod.SCM.Services.Utilitys.FileHelper _fileHelper;
        private readonly CompanyConfig _appSettings;
        private readonly IViewRenderService _viewRenderService;

        public OperationCommentService(
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
            _operationCommentRepository = _unitOfWork.Set<OperationComment>();
            _operationRepository = _unitOfWork.Set<Operation>();
            _operationAttachmentRepository = _unitOfWork.Set<OperationAttachment>();
            _userRepository = _unitOfWork.Set<User>();
            _viewRenderService = viewRenderService;
            httpContextAccessor.HttpContext.Request.Headers.TryGetValue("companyCode", out var CompanyCode);
            _appSettings = appSettings.Value.CompanyConfig.First(a => a.CompanyCode == CompanyCode);
        }
        public async Task<ServiceResult<string>> AddOperationCommentAsync(AuthenticateDto authenticate, Guid operationId, AddOperationCommentDto model)
        {
            try
            {
                //var permission = await _authenticationService.
                //    HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);

                //if (!permission.HasPermission)
                //    return ServiceResultFactory.CreateError("", MessageId.AccessDenied);

                var dbQuery = _operationRepository
                    .Where(a => !a.IsDeleted && a.OperationId == operationId  &&
                    a.ContractCode == authenticate.ContractCode);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError("", MessageId.EntityDoesNotExist);

                //if (permission.DocumentGroupIds.Any() && !dbQuery.Any(c => permission.DocumentGroupIds.Contains(c.Document.DocumentGroupId)))
                //    return ServiceResultFactory.CreateError("", MessageId.AccessDenied);

                var operationModel = await dbQuery
                    .AsNoTracking()
                    .Select(c => new
                    {
                        OperationId = c.OperationId,
                        OperationCode = c.OperationCode,
                        OperationGroupId = c.OperationGroupId,
                        Description = c.OperationDescription,
                        adderUserId = c.AdderUserId,
                        ActivityUserIds = c.OperationActivities.Where(a => !a.IsDeleted).Select(a => a.ActivityOwnerId).ToList()
                    })
                    .FirstOrDefaultAsync();

                if (operationModel == null)
                    return ServiceResultFactory.CreateError("", MessageId.EntityDoesNotExist);

                var commentModel = new OperationComment();

                bool isSetTaskDone = false;
                if (model.ParentCommentId != null && model.ParentCommentId > 0)
                {
                    var parentComment = await _operationCommentRepository.Where(a => !a.IsDeleted && a.OperationId == operationId
                     && a.OperationCommentId == model.ParentCommentId.Value && a.ParentCommentId == null)
                        .Select(c => new
                        {
                            OperationCommentId = c.OperationCommentId,
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
                    commentModel.OperationCommentUsers = new List<OperationCommentUser>();

                    var userIds = model.UserIds.Where(a => a > 0).Select(u => u).Distinct().ToList();
                    foreach (var item in userIds)
                    {
                        commentModel.OperationCommentUsers.Add(new OperationCommentUser
                        {
                            UserId = item
                        });
                    }

                    mentionUserEmails = await _userRepository
                        .Where(a => !a.IsDeleted && a.IsActive && userIds.Contains(a.Id) && !string.IsNullOrEmpty(a.Email))
                        .Select(c => c.Email)
                        .ToListAsync();
                }

                commentModel.OperationId = operationId;
                commentModel.Message = model.Message;

                if (model.Attachments != null && model.Attachments.Count() > 0)
                {
                    if (!_fileHelper.FileExistInTemp(model.Attachments.Select(c => c.FileSrc).ToList()))
                        return ServiceResultFactory.CreateError("", MessageId.FileNotFound);
                    var attachmentResult = await AddCommentAttachmentAsync(authenticate, operationModel.OperationId, commentModel, model.Attachments);
                    if (!attachmentResult.Succeeded)
                        return ServiceResultFactory.CreateError("",
                            attachmentResult.Messages.FirstOrDefault().Message);

                    commentModel = attachmentResult.Result;
                }

                _operationCommentRepository.Add(commentModel);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var notifEvent = model.ParentCommentId == null || model.ParentCommentId <= 0 ? NotifEvent.AddCommentInOperation : NotifEvent.CommentReplyInOperation;

                    if (operationModel.adderUserId != null)
                        operationModel.ActivityUserIds.Add(operationModel.adderUserId.Value);
                    var res = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = authenticate.ContractCode,
                        RootKeyValue = operationModel.OperationId.ToString(),
                        Description = operationModel.Description,
                        FormCode = operationModel.OperationCode,
                        KeyValue = commentModel.OperationCommentId.ToString(),
                        NotifEvent = notifEvent,
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,
                        OperationGroupId = operationModel.OperationGroupId,
                        ReceiverLogUserIds = operationModel.ActivityUserIds.Distinct().ToList()
                    }, null);

                    if (commentModel.OperationCommentUsers != null && commentModel.OperationCommentUsers.Any())
                    {
                        await _scmLogAndNotificationService.AddMentionNotificationTaskAsync(new AddMentionLogDto
                        {
                            ContractCode = authenticate.ContractCode,
                            RootKeyValue = operationModel.OperationId.ToString(),
                            Description = operationModel.Description,
                            FormCode = operationModel.OperationCode,
                            KeyValue = commentModel.OperationCommentId.ToString(),
                            MentionEvent = MentionEvent.MentionOperationComment,
                            PerformerUserId = authenticate.UserId,
                            PerformerUserFullName = authenticate.UserFullName,
                            OperationGroupId = operationModel.OperationGroupId,
                            ReceiverLogUserIds = commentModel.OperationCommentUsers.Select(a => a.UserId).ToList()
                        });
                    }

                    if (mentionUserEmails != null && mentionUserEmails.Any())
                    {
                        List<CommentNotifViaEmailDTO> comments = new List<CommentNotifViaEmailDTO>();
                        if (commentModel.ParentCommentId != null)
                        {

                            comments = await _operationCommentRepository.OrderBy(c => c.OperationCommentId).Where(c => c.ParentCommentId == commentModel.ParentCommentId || c.OperationCommentId == commentModel.ParentCommentId).Select(c => new CommentNotifViaEmailDTO
                            {
                                Message = c.Message,
                                SendDate = c.CreatedDate.ToJalaliWithTime(),
                                SenderName = c.AdderUser.FullName
                            }).ToListAsync();
                        }
                        else
                        {

                            comments = await _operationCommentRepository.OrderBy(c => c.OperationCommentId).Where(c => c.ParentCommentId == commentModel.OperationCommentId || c.OperationCommentId == commentModel.OperationCommentId).Select(c => new CommentNotifViaEmailDTO
                            {
                                Message = c.Message,
                                SendDate = c.CreatedDate.ToJalaliWithTime(),
                                SenderName = c.AdderUser.FullName
                            }).ToListAsync();
                        }
                        try
                        {
                            BackgroundJob.Enqueue(() => SendEmailForUserMentionAsync(operationId, authenticate.ContractCode, operationModel.OperationCode, operationModel.Description, mentionUserEmails, comments, commentModel.OperationCommentId));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                    return ServiceResultFactory.CreateSuccess(commentModel.OperationCommentId.ToString());
                }
                return ServiceResultFactory.CreateError("", MessageId.AddEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException("", exception);
            }
        }


        public async Task<ServiceResult<List<OperationCommentListDto>>> GetOperationCommentAsync(AuthenticateDto authenticate, Guid operationId, OperationCommentQueryDto query)
        {
            try
            {
                //var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                //if (!permission.HasPermission)
                //    return ServiceResultFactory.CreateError<List<RevisionCommentListDto>>(null, MessageId.AccessDenied);

                var dbQuery = _operationCommentRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted &&
                    a.OperationId == operationId &&
                    a.ParentCommentId == null &&
                    a.Operation.ContractCode == authenticate.ContractCode)
                    .OrderByDescending(a => a.OperationId)
                    .AsQueryable();

                //if (permission.DocumentGroupIds.Any() && !dbQuery.Any(c => permission.DocumentGroupIds.Contains(c.DocumentRevision.Document.DocumentGroupId)))
                //    return ServiceResultFactory.CreateError<List<RevisionCommentListDto>>(null, MessageId.AccessDenied);

                var totalCount = dbQuery.Count();
                dbQuery = dbQuery.ApplayPageing(query);

                var result = await dbQuery.Select(c => new OperationCommentListDto
                {
                    CommentId = c.OperationCommentId,
                    Message = c.Message,
                    ParentCommentId = c.ParentCommentId,
                    Attachments = c.Attachments.Where(a => !a.IsDeleted)
                     .Select(a => new OperationAttachmentDto
                     {
                         OperationAttachmentId = a.OperationAttachmentId,
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
                    UserMentions = c.OperationCommentUsers.Select(u => new UserMiniInfoDto
                    {
                        UserFullName = u.User.FullName,
                        UserId = u.UserId,
                        UserName = u.User.UserName
                    }).ToList(),

                    ReplayComments = c.ReplayComments.Where(v => !v.IsDeleted)
                     .Select(v => new OperationCommentListDto
                     {
                         CommentId = v.OperationCommentId,
                         Message = v.Message,
                         ParentCommentId = v.ParentCommentId,
                         Attachments = v.Attachments.Where(b => !b.IsDeleted)
                         .Select(b => new OperationAttachmentDto
                         {
                             OperationAttachmentId = b.OperationAttachmentId,
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
                         UserMentions = v.OperationCommentUsers.Select(u => new UserMiniInfoDto
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
                return ServiceResultFactory.CreateException<List<OperationCommentListDto>>(null, exception);
            }
        }

        private async Task<ServiceResult<OperationComment>> AddCommentAttachmentAsync(AuthenticateDto authenticate, Guid operationId, OperationComment operationCommentModel, List<AddOperationAttachmentDto> attachment)
        {
            operationCommentModel.Attachments = new List<OperationAttachment>();

            foreach (var item in attachment)
            {
                var UploadedFile =
                    await _fileHelper.SaveDocumentFromTemp(item.FileSrc, ServiceSetting.UploadFilePathOperationComment(authenticate.ContractCode, operationId));
                if (UploadedFile == null)
                    return ServiceResultFactory.CreateError<OperationComment>(null, MessageId.UploudFailed);

                _fileHelper.DeleteDocumentFromTemp(item.FileSrc);
                operationCommentModel.Attachments.Add(new OperationAttachment
                {
                    FileName = item.FileName,
                    FileSrc = item.FileSrc,
                    FileType = UploadedFile.FileType,
                    FileSize = UploadedFile.FileSize
                });
            }
            return ServiceResultFactory.CreateSuccess(operationCommentModel);
        }

        public async Task<DownloadFileDto> DownloadCommentFileAsync(AuthenticateDto authenticate, Guid operationId, long commentId, string fileSrc)
        {
            try
            {
                //var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                //if (!permission.HasPermission)
                //    return null;

                var dbQuery = _operationAttachmentRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted &&
                    a.OperationCommentId == commentId &&
                    a.OperationComment.OperationId == operationId &&
                    a.FileSrc == fileSrc);

                //if (permission.DocumentGroupIds.Any() &&
                //    !dbQuery.Any(c => permission.ProductGroupIds.Contains(c.RevisionComment.DocumentRevision.Document.DocumentGroupId)))
                //    return null;

                if (!await dbQuery.AnyAsync())
                    return null;
                var attachment = await dbQuery.FirstOrDefaultAsync();
                return await _fileHelper.DownloadDocument(fileSrc, ServiceSetting.UploadFilePathOperationComment(authenticate.ContractCode, operationId),attachment.FileName);
            }
            catch (Exception exception)
            {
                return null;
            }
        }

      public async Task<bool>  SendEmailForUserMentionAsync(Guid operationId,string contractCode, string operationCode,string operationDescription,List<string> mentionUserEmails, List<CommentNotifViaEmailDTO> comments, long operationCommentId)
        {
            string url = "";
            string subject = "";
     
            url = $"/dashboard/OperationHandling/{operationId}?team={contractCode}&targetId={operationCommentId}";
            subject = $"Mention | {operationCode}";

            url = _appSettings.ClientHost + url;
            string message = $"در پروژه <span style='direction=ltr'>{contractCode}</span> به نام شما در پرسش و پاسخ عملیات <span style='direction=ltr'>{operationDescription}</span> با کد <span style='direction=ltr'>{operationCode}</span>" +
                $" اشاره شده است. لطفا با مراجعه به آن بخش نظرات خود را به اشتراک بگذارید.";
            CommentMentionNotif model = new CommentMentionNotif(message, url, comments,_appSettings.CompanyName,"");
            var emalRequest = new EmailRequest
            {
                Attachment = null,
                Subject = subject,
                Body = await _viewRenderService.RenderToStringAsync("_SendCommentMentionNotifViaEmail", model),

            };

            foreach (var address in mentionUserEmails)
            {
                emalRequest.To = new List<string> { address };
                await _appEmailService.SendAsync(emalRequest);
            }
            return true;
        }
    }
}
