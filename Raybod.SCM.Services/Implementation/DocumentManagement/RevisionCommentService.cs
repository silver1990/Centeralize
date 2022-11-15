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
using Raybod.SCM.DataTransferObject.Audit;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Services.Utilitys.MailService.Model;
using Raybod.SCM.Services.Utilitys.MailService;
using Raybod.SCM.DataTransferObject.Document;
using Raybod.SCM.DataTransferObject.User;
using Raybod.SCM.DataTransferObject._PanelDocument.Communication.Comment;
using Hangfire;
using Raybod.SCM.DataTransferObject.Email;
using Microsoft.AspNetCore.Http;

namespace Raybod.SCM.Services.Implementation
{
    public class RevisionCommentService : IRevisionCommentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITeamWorkAuthenticationService _authenticationService;
        private readonly ISCMLogAndNotificationService _scmLogAndNotificationService;
        private readonly IAppEmailService _appEmailService;
        private readonly DbSet<DocumentRevision> _documentRevisionRepository;
        private readonly DbSet<RevisionComment> _revisionCommentRepository;
        private readonly DbSet<RevisionAttachment> _revisionAttachmentRepository;
        private readonly DbSet<User> _userRepository;
        private readonly Raybod.SCM.Services.Utilitys.FileHelper _fileHelper;
        private readonly CompanyConfig _appSettings;
        private readonly IViewRenderService _viewRenderService;

        public RevisionCommentService(
            IUnitOfWork unitOfWork,
            IWebHostEnvironment hostingEnvironmentRoot,
            ITeamWorkAuthenticationService authenticationService,
            IOptions<CompanyAppSettingsDto> appSettings,
            ISCMLogAndNotificationService scmLogAndNotificationService,
            IHttpContextAccessor httpContextAccessor,
            IAppEmailService appEmailService, IViewRenderService viewRenderService)
        {
            _unitOfWork = unitOfWork;
            _authenticationService = authenticationService;
            _scmLogAndNotificationService = scmLogAndNotificationService;
            _appEmailService = appEmailService;
            _fileHelper = new Utilitys.FileHelper(hostingEnvironmentRoot);
            _revisionCommentRepository = _unitOfWork.Set<RevisionComment>();
            _documentRevisionRepository = _unitOfWork.Set<DocumentRevision>();
            _revisionAttachmentRepository = _unitOfWork.Set<RevisionAttachment>();
            _userRepository = _unitOfWork.Set<User>();
            _viewRenderService = viewRenderService;
            httpContextAccessor.HttpContext.Request.Headers.TryGetValue("companyCode", out var CompanyCode);
            _appSettings = appSettings.Value.CompanyConfig.First(a => a.CompanyCode == CompanyCode);
        }

        public async Task<ServiceResult<string>> AddRevisionCommentAsync(AuthenticateDto authenticate,
         long documentId, long revisonId, AddRevisionCommentDto model)
        {
            try
            {
                //var permission = await _authenticationService.
                //    HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);

                //if (!permission.HasPermission)
                //    return ServiceResultFactory.CreateError("", MessageId.AccessDenied);

                var dbQuery = _documentRevisionRepository
                    .Where(a => !a.IsDeleted && a.DocumentRevisionId == revisonId && a.DocumentId == documentId &&
                    a.Document.ContractCode == authenticate.ContractCode);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError("", MessageId.EntityDoesNotExist);

                //if (permission.DocumentGroupIds.Any() && !dbQuery.Any(c => permission.DocumentGroupIds.Contains(c.Document.DocumentGroupId)))
                //    return ServiceResultFactory.CreateError("", MessageId.AccessDenied);

                var revisionModel = await dbQuery
                    .AsNoTracking()
                    .Select(c => new
                    {
                        RevisionId = c.DocumentRevisionId,
                        DocumentRevisionCode = c.DocumentRevisionCode,
                        DocumentId = c.DocumentId,
                        DocumentGroupId = c.Document.DocumentGroupId,
                        DocTitle = c.Document.DocTitle,
                        DocNumber = c.Document.DocNumber,
                        adderUserId = c.AdderUserId,
                        ActivityUserIds = c.RevisionActivities.Where(a => !a.IsDeleted).Select(a => a.ActivityOwnerId).ToList()
                    })
                    .FirstOrDefaultAsync();

                if (revisionModel == null)
                    return ServiceResultFactory.CreateError("", MessageId.EntityDoesNotExist);

                var commentModel = new RevisionComment();

                bool isSetTaskDone = false;
                if (model.ParentCommentId != null && model.ParentCommentId > 0)
                {
                    var parentComment = await _revisionCommentRepository.Where(a => !a.IsDeleted && a.DocumentRevisionId == revisonId
                     && a.RevisionCommentId == model.ParentCommentId.Value && a.ParentCommentId == null)
                        .Select(c => new
                        {
                            revisionCommentId = c.RevisionCommentId,
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
                    commentModel.RevisionCommentUsers = new List<RevisionCommentUser>();

                    var userIds = model.UserIds.Where(a => a > 0).Select(u => u).Distinct().ToList();
                    foreach (var item in userIds)
                    {
                        commentModel.RevisionCommentUsers.Add(new RevisionCommentUser
                        {
                            UserId = item
                        });
                    }

                    mentionUserEmails = await _userRepository
                        .Where(a => !a.IsDeleted && a.IsActive && userIds.Contains(a.Id) && !string.IsNullOrEmpty(a.Email))
                        .Select(c => c.Email)
                        .ToListAsync();
                }

                commentModel.DocumentRevisionId = revisonId;
                commentModel.Message = model.Message;

                if (model.Attachments != null && model.Attachments.Count() > 0)
                {
                    if (!_fileHelper.FileExistInTemp(model.Attachments.Select(c => c.FileSrc).ToList()))
                        return ServiceResultFactory.CreateError("", MessageId.FileNotFound);
                    var attachmentResult = await AddCommentAttachmentAsync(authenticate, revisionModel.DocumentId, revisionModel.RevisionId, commentModel, model.Attachments);
                    if (!attachmentResult.Succeeded)
                        return ServiceResultFactory.CreateError("",
                            attachmentResult.Messages.FirstOrDefault().Message);

                    commentModel = attachmentResult.Result;
                }

                _revisionCommentRepository.Add(commentModel);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                   
                    var notifEvent = model.ParentCommentId == null || model.ParentCommentId <= 0 ? NotifEvent.AddRevisionComment : NotifEvent.ReplayRevisionComment;
                   
                    if (revisionModel.adderUserId != null)
                        revisionModel.ActivityUserIds.Add(revisionModel.adderUserId.Value);
                    var res = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = authenticate.ContractCode,
                        RootKeyValue = revisionModel.RevisionId.ToString(),
                        Description = revisionModel.DocTitle,
                        FormCode = revisionModel.DocumentRevisionCode,
                        KeyValue = commentModel.RevisionCommentId.ToString(),
                        NotifEvent = notifEvent,
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,
                        DocumentGroupId = revisionModel.DocumentGroupId,
                        ReceiverLogUserIds = revisionModel.ActivityUserIds.Distinct().ToList()
                    }, null);

                    if (commentModel.RevisionCommentUsers != null && commentModel.RevisionCommentUsers.Any())
                    {
                        await _scmLogAndNotificationService.AddMentionNotificationTaskAsync(new AddMentionLogDto
                        {
                            ContractCode = authenticate.ContractCode,
                            RootKeyValue = revisionModel.RevisionId.ToString(),
                            Description = revisionModel.DocTitle,
                            FormCode = revisionModel.DocumentRevisionCode,
                            KeyValue = commentModel.RevisionCommentId.ToString(),
                            MentionEvent = MentionEvent.RevisionCommentUserMention,
                            PerformerUserId = authenticate.UserId,
                            PerformerUserFullName = authenticate.UserFullName,
                            DocumentGroupId = revisionModel.DocumentGroupId,
                            ReceiverLogUserIds = commentModel.RevisionCommentUsers.Select(a => a.UserId).ToList()
                        });
                    }
                    if (mentionUserEmails != null && mentionUserEmails.Any())
                    {
                        List<CommentNotifViaEmailDTO> comments = new List<CommentNotifViaEmailDTO>();
                        if (commentModel.ParentCommentId != null)
                        {

                            comments = await _revisionCommentRepository.OrderBy(c => c.RevisionCommentId).Where(c => c.ParentCommentId == commentModel.ParentCommentId || c.RevisionCommentId == commentModel.ParentCommentId).Select(c => new CommentNotifViaEmailDTO
                            {
                                Message = c.Message,
                                SendDate = c.CreatedDate.ToJalaliWithTime(),
                                SenderName = c.AdderUser.FullName
                            }).ToListAsync();
                        }
                        else
                        {

                            comments = await _revisionCommentRepository.OrderBy(c => c.RevisionCommentId).Where(c => c.ParentCommentId == commentModel.RevisionCommentId || c.RevisionCommentId == commentModel.RevisionCommentId).Select(c => new CommentNotifViaEmailDTO
                            {
                                Message = c.Message,
                                SendDate = c.CreatedDate.ToJalaliWithTime(),
                                SenderName = c.AdderUser.FullName
                            }).ToListAsync();
                        }
                        try
                        {
                            BackgroundJob.Enqueue(() => SendEmailForUserMentionAsync(revisonId, authenticate.ContractCode, revisionModel.DocTitle, revisionModel.DocNumber, revisionModel.DocumentRevisionCode, mentionUserEmails, comments, commentModel.RevisionCommentId));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                    return ServiceResultFactory.CreateSuccess(commentModel.RevisionCommentId.ToString());
                }
                return ServiceResultFactory.CreateError("", MessageId.AddEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException("", exception);
            }
        }

        public async Task<bool> SendEmailForUserMentionAsync(long revisionId, string contractCode,
            string docTitle, string docNumber, string revisionCode, List<string> emails,List<CommentNotifViaEmailDTO> comments, long commentRevisionId)
        {
            string faMessage = $"به نام شما در ویرایش <span style='direction:ltr'>{revisionCode}</span> تهیه مدرک <span style='direction:ltr'>{docTitle}</span> به شماره <span style='direction:ltr'>{docNumber}</span> اشاره شده است.برای مشاهده و پاسخ پیام رو دکمه «مشاهده پیام» در انتهای ایمیل کلیک کنید";
            string enMessage = $"<div style='direction:ltr;text-align:left'>You are mentioned in document preparation of  {docTitle} -{docNumber} - Rev.  {revisionCode}.Click button below to view.</div>";
            string link = _appSettings.ClientHost + $"/dashboard/documents/RevDetails/{revisionId}?team={contractCode}&targetId={commentRevisionId}";
            CommentMentionNotif model = new CommentMentionNotif(faMessage, link , comments,_appSettings.CompanyName,enMessage);
            var emalRequest = new EmailRequest
            {
                Attachment = null,
                Subject = $"Mention | {docNumber}",
                Body = await _viewRenderService.RenderToStringAsync("_SendCommentMentionNotifViaEmail", model)
            };

            foreach (var address in emails)
            {
                emalRequest.To = new List<string> { address };
                await _appEmailService.SendAsync(emalRequest);
            }
            return true;
        }

        public async Task<ServiceResult<List<RevisionCommentListDto>>> GetRevisionCommentAsync(AuthenticateDto authenticate,
           long documentId, long revisonId, RevisionCommentQueryDto query)
        {
            try
            {
                //var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                //if (!permission.HasPermission)
                //    return ServiceResultFactory.CreateError<List<RevisionCommentListDto>>(null, MessageId.AccessDenied);

                var dbQuery = _revisionCommentRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted &&
                    a.DocumentRevisionId == revisonId &&
                    a.ParentCommentId == null &&
                    a.DocumentRevision.DocumentId == documentId &&
                    a.DocumentRevision.Document.ContractCode == authenticate.ContractCode)
                    .OrderByDescending(a => a.DocumentRevisionId)
                    .AsQueryable();

                //if (permission.DocumentGroupIds.Any() && !dbQuery.Any(c => permission.DocumentGroupIds.Contains(c.DocumentRevision.Document.DocumentGroupId)))
                //    return ServiceResultFactory.CreateError<List<RevisionCommentListDto>>(null, MessageId.AccessDenied);

                var totalCount = dbQuery.Count();
                dbQuery = dbQuery.ApplayPageing(query);

                var result = await dbQuery.Select(c => new RevisionCommentListDto
                {
                    CommentId = c.RevisionCommentId,
                    Message = c.Message,
                    ParentCommentId = c.ParentCommentId,
                    Attachments = c.Attachments.Where(a => !a.IsDeleted)
                     .Select(a => new RevisionAttachmentDto
                     {
                         RevisionAttachmentId = a.RevisionAttachmentId,
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
                    UserMentions = c.RevisionCommentUsers.Select(u => new UserMiniInfoDto
                    {
                        UserFullName = u.User.FullName,
                        UserId = u.UserId,
                        UserName = u.User.UserName
                    }).ToList(),

                    ReplayComments = c.ReplayComments.Where(v => !v.IsDeleted)
                     .Select(v => new RevisionCommentListDto
                     {
                         CommentId = v.RevisionCommentId,
                         Message = v.Message,
                         ParentCommentId = v.ParentCommentId,
                         Attachments = v.Attachments.Where(b => !b.IsDeleted)
                         .Select(b => new RevisionAttachmentDto
                         {
                             RevisionAttachmentId = b.RevisionAttachmentId,
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
                         UserMentions = v.RevisionCommentUsers.Select(u => new UserMiniInfoDto
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
                return ServiceResultFactory.CreateException<List<RevisionCommentListDto>>(null, exception);
            }
        }

       

        private async Task<ServiceResult<RevisionComment>> AddCommentAttachmentAsync(AuthenticateDto authenticate, long documentId,
            long revId, RevisionComment revisionCommentModel, List<AddRevisionAttachmentDto> attachment)
        {
            revisionCommentModel.Attachments = new List<RevisionAttachment>();

            foreach (var item in attachment)
            {
                var UploadedFile =
                    await _fileHelper.SaveDocumentFromTemp(item.FileSrc, ServiceSetting.UploadFilePathRevisionComment(authenticate.ContractCode, documentId, revId));
                if (UploadedFile == null)
                    return ServiceResultFactory.CreateError<RevisionComment>(null, MessageId.UploudFailed);

                _fileHelper.DeleteDocumentFromTemp(item.FileSrc);
                revisionCommentModel.Attachments.Add(new RevisionAttachment
                {
                    FileName = item.FileName,
                    FileSrc = item.FileSrc,
                    FileType = UploadedFile.FileType,
                    FileSize = UploadedFile.FileSize
                });
            }
            return ServiceResultFactory.CreateSuccess(revisionCommentModel);
        }

        public async Task<DownloadFileDto> DownloadCommentFileAsync(AuthenticateDto authenticate, long docId, long revId, long commentId, string fileSrc)
        {
            try
            {
                //var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                //if (!permission.HasPermission)
                //    return null;

                var dbQuery = _revisionAttachmentRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted &&
                    a.RevisionCommentId == commentId &&
                    a.RevisionComment.DocumentRevisionId == revId &&
                    a.FileSrc == fileSrc);

                //if (permission.DocumentGroupIds.Any() &&
                //    !dbQuery.Any(c => permission.ProductGroupIds.Contains(c.RevisionComment.DocumentRevision.Document.DocumentGroupId)))
                //    return null;

                if (!await dbQuery.AnyAsync())
                    return null;
                var attachment = await dbQuery.FirstOrDefaultAsync();
                return await _fileHelper.DownloadDocument(fileSrc, ServiceSetting.UploadFilePathRevisionComment(authenticate.ContractCode, docId, revId),attachment.FileName);
            }
            catch (Exception exception)
            {
                return null;
            }
        }

    }
}
