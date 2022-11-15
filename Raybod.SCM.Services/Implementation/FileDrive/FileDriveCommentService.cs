using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Raybod.SCM.DataAccess.Core;
using Raybod.SCM.DataAccess.Extention;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject._PanelDocument.Communication.Comment;
using Raybod.SCM.DataTransferObject.Audit;
using Raybod.SCM.DataTransferObject.Email;
using Raybod.SCM.DataTransferObject.FileDriveDirectory;
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
    public class FileDriveCommentService : IFileDriveCommentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITeamWorkAuthenticationService _authenticationService;
        private readonly ISCMLogAndNotificationService _scmLogAndNotificationService;
        private readonly IAppEmailService _appEmailService;
        private readonly DbSet<FileDriveFile> _filedriveFileRepository;
        private readonly DbSet<FileDriveShare> _filedriveShareRepository;
        private readonly DbSet<FileDriveComment> _filedriveCommentRepository;
        private readonly DbSet<FileDriveCommentAttachment> _filedriveCommentAttachmentRepository;
        private readonly DbSet<User> _userRepository;
        private readonly Raybod.SCM.Services.Utilitys.FileHelper _fileHelper;
        private readonly CompanyConfig _appSettings;
        private readonly IViewRenderService _viewRenderService;
        public FileDriveCommentService(
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
            _filedriveFileRepository = _unitOfWork.Set<FileDriveFile>();
            _filedriveCommentRepository = _unitOfWork.Set<FileDriveComment>();
            _filedriveShareRepository = _unitOfWork.Set<FileDriveShare>();
            _filedriveCommentAttachmentRepository = _unitOfWork.Set<FileDriveCommentAttachment>();
            _userRepository = _unitOfWork.Set<User>();
            _viewRenderService = viewRenderService;
            httpContextAccessor.HttpContext.Request.Headers.TryGetValue("companyCode", out var CompanyCode);
            _appSettings = appSettings.Value.CompanyConfig.First(a => a.CompanyCode == CompanyCode);
        }
        public async Task<ServiceResult<string>> AddFileDriveCommentAsync(AuthenticateDto authenticate, Guid fileId, AddFileDriveCommentDto model)
        {
            try
            {

                var dbQuery = _filedriveFileRepository
                    .Where(a => !a.IsDeleted && a.FileId == fileId &&
                    a.Directory.ContractCode == authenticate.ContractCode);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError("", MessageId.EntityDoesNotExist);

                //if (permission.DocumentGroupIds.Any() && !dbQuery.Any(c => permission.DocumentGroupIds.Contains(c.Document.DocumentGroupId)))
                //    return ServiceResultFactory.CreateError("", MessageId.AccessDenied);

                var fileModel = await dbQuery
                    .AsNoTracking()
                    .Select(c => new
                    {
                        FileId = c.FileId,
                        DirectoryId = c.DirectoryId,
                        FileName = c.FileName,
                        adderUserId = c.AdderUserId,
                        userId=c.UserId,
                        directoryName=c.Directory.DirectoryName
                    })
                    .FirstOrDefaultAsync();

                if (fileModel == null)
                    return ServiceResultFactory.CreateError("", MessageId.EntityDoesNotExist);

                var commentModel = new FileDriveComment();

                bool isSetTaskDone = false;
                if (model.ParentCommentId != null && model.ParentCommentId > 0)
                {
                    var parentComment = await _filedriveCommentRepository.Where(a => !a.IsDeleted && a.FileId == fileId
                     && a.CommentId == model.ParentCommentId.Value && a.ParentCommentId == null)
                        .Select(c => new
                        {
                            CommentId = c.CommentId,
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
                    commentModel.CommentUsers = new List<FileDriveCommentUser>();

                    var userIds = model.UserIds.Where(a => a > 0).Select(u => u).Distinct().ToList();
                    foreach (var item in userIds)
                    {
                        commentModel.CommentUsers.Add(new FileDriveCommentUser
                        {
                            UserId = item
                        });
                    }

                    mentionUserEmails = await _userRepository
                        .Where(a => !a.IsDeleted && a.IsActive && userIds.Contains(a.Id) && !string.IsNullOrEmpty(a.Email))
                        .Select(c => c.Email)
                        .ToListAsync();
                }

                commentModel.FileId = fileId;
                commentModel.Message = model.Message;

                if (model.Attachments != null && model.Attachments.Count() > 0)
                {
                    if (!_fileHelper.FileExistInTemp(model.Attachments.Select(c => c.FileSrc).ToList()))
                        return ServiceResultFactory.CreateError("", MessageId.FileNotFound);
                    var attachmentResult = await AddCommentAttachmentAsync(authenticate, commentModel, model.Attachments);
                    if (!attachmentResult.Succeeded)
                        return ServiceResultFactory.CreateError("",
                            attachmentResult.Messages.FirstOrDefault().Message);

                    commentModel = attachmentResult.Result;
                }

                _filedriveCommentRepository.Add(commentModel);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {

                    var notifEvent = model.ParentCommentId == null || model.ParentCommentId <= 0 ? NotifEvent.AddFileDriveComment : NotifEvent.ReplyFileDriveComment;
                    var link = (fileModel.userId == null && fileModel.directoryName == "FileDrive") ? "root" : (fileModel.userId == null && fileModel.directoryName != "FileDrive") ? $"{fileModel.DirectoryId}" : (fileModel.userId != null && !await _filedriveShareRepository.AnyAsync(a => !a.IsDeleted && a.Status == ShareEntityStatus.Active && a.DirectoryId == fileModel.DirectoryId)) ? "shared/root" :
                               (fileModel.userId != null && await _filedriveShareRepository.AnyAsync(a => !a.IsDeleted && a.Status == ShareEntityStatus.Active && a.DirectoryId == fileModel.DirectoryId)) ? $"shared/{fileModel.DirectoryId}" : "";
                    
                    //var res = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    //{
                    //    ContractCode = authenticate.ContractCode,
                    //    RootKeyValue = fileModel.FileId.ToString(),
                    //    Description = fileModel.FileName,
                    //    FormCode = fileModel.DirectoryId.ToString(),
                    //    KeyValue = commentModel.CommentId.ToString(),
                    //    NotifEvent = notifEvent,
                    //    PerformerUserId = authenticate.UserId,
                    //    PerformerUserFullName = authenticate.UserFullName,
                    //    Temp= $"/dashboard/FileStorage/{link}"

                    //}, null);

                    if (commentModel.CommentUsers != null && commentModel.CommentUsers.Any())
                    {
                        await _scmLogAndNotificationService.AddMentionNotificationTaskAsync(new AddMentionLogDto
                        {
                            ContractCode = authenticate.ContractCode,
                            RootKeyValue = fileModel.FileId.ToString(),
                            Description = fileModel.FileName,
                            FormCode = fileModel.DirectoryId.ToString(),
                            KeyValue = commentModel.CommentId.ToString(),
                            MentionEvent = MentionEvent.MentionFileDriveComment,
                            PerformerUserId = authenticate.UserId,
                            PerformerUserFullName = authenticate.UserFullName,
                            Temp = $"/dashboard/FileStorage/{link}",
                            ReceiverLogUserIds = commentModel.CommentUsers.Select(a => a.UserId).ToList()
                        });
                    }
                    if (mentionUserEmails != null && mentionUserEmails.Any())
                    {
                        List<CommentNotifViaEmailDTO> comments = new List<CommentNotifViaEmailDTO>();
                        if (commentModel.ParentCommentId != null)
                        {

                            comments = await _filedriveCommentRepository.OrderBy(c => c.CommentId).Where(c => c.ParentCommentId == commentModel.ParentCommentId || c.CommentId == commentModel.ParentCommentId).Select(c => new CommentNotifViaEmailDTO
                            {
                                Message = c.Message,
                                SendDate = c.CreatedDate.ToJalaliWithTime(),
                                SenderName = c.AdderUser.FullName
                            }).ToListAsync();
                        }
                        else
                        {

                            comments = await _filedriveCommentRepository.OrderBy(c => c.CommentId).Where(c => c.ParentCommentId == commentModel.CommentId || c.CommentId == commentModel.CommentId).Select(c => new CommentNotifViaEmailDTO
                            {
                                Message = c.Message,
                                SendDate = c.CreatedDate.ToJalaliWithTime(),
                                SenderName = c.AdderUser.FullName
                            }).ToListAsync();
                        }
                        try
                        {
                           if(fileModel.userId!=null && await _filedriveShareRepository.AnyAsync(a => !a.IsDeleted && a.Status == ShareEntityStatus.Active && a.FileId == fileModel.FileId)||fileModel.userId==null)
                            BackgroundJob.Enqueue(() => SendEmailForUserMentionAsync(fileId, authenticate.ContractCode, fileModel.FileName, fileModel.DirectoryId, mentionUserEmails, comments, commentModel.CommentId,link));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                    return ServiceResultFactory.CreateSuccess(commentModel.CommentId.ToString());
                }
                return ServiceResultFactory.CreateError("", MessageId.AddEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException("", exception);
            }
        }

        public async Task<DownloadFileDto> DownloadCommentFileAsync(AuthenticateDto authenticate, long commentId, string fileSrc)
        {
            try
            {
                //var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                //if (!permission.HasPermission)
                //    return null;

                var dbQuery = _filedriveCommentAttachmentRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted &&
                    a.FileDriveCommentId == commentId &&
                    a.FileSrc == fileSrc);


                if (!await dbQuery.AnyAsync())
                    return null;
                var attachment = await dbQuery.FirstOrDefaultAsync();
                return await _fileHelper.DownloadDocument(fileSrc, ServiceSetting.UploadFilePathFileDriveComment(authenticate.ContractCode), attachment.FileName);
            }
            catch (Exception exception)
            {
                return null;
            }
        }

        public async Task<ServiceResult<List<FileDriveCommentListDto>>> GetFileDriveCommentAsync(AuthenticateDto authenticate, Guid fileId, FileDriveCommentQueryDto query)
        {
            try
            {


                var dbQuery = _filedriveCommentRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted &&
                    a.FileId == fileId &&
                    a.ParentCommentId == null &&
                    a.File.Directory.ContractCode == authenticate.ContractCode)
                    .OrderByDescending(a => a.FileId)
                    .AsQueryable();



                var totalCount = dbQuery.Count();
                dbQuery = dbQuery.ApplayPageing(query);

                var result = await dbQuery.Select(c => new FileDriveCommentListDto
                {
                    CommentId = c.CommentId,
                    Message = c.Message,
                    ParentCommentId = c.ParentCommentId,
                    Attachments = c.Attachments.Where(a => !a.IsDeleted)
                     .Select(a => new FileDriveCommentAttachmentDto
                     {
                         CommentAttachmentId = a.CommentAttachmentId,
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
                     .Select(v => new FileDriveCommentListDto
                     {
                         CommentId = v.CommentId,
                         Message = v.Message,
                         ParentCommentId = v.ParentCommentId,
                         Attachments = v.Attachments.Where(b => !b.IsDeleted)
                         .Select(b => new FileDriveCommentAttachmentDto
                         {
                             CommentAttachmentId = b.CommentAttachmentId,
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
                return ServiceResultFactory.CreateException<List<FileDriveCommentListDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<List<UserMentionDto>>> GetUserMentionsAsync(AuthenticateDto authenticate, Guid fileId)
        {
            try
            {
                //var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                //if (!permission.HasPermission)
                //    return ServiceResultFactory.CreateError<List<UserMentionDto>>(null, MessageId.AccessDenied);

                var file = await _filedriveFileRepository
                    .Where(a => !a.IsDeleted && a.FileId == fileId && a.Directory.ContractCode == authenticate.ContractCode)
                    .FirstOrDefaultAsync();
                if (file == null)
                    return ServiceResultFactory.CreateError<List<UserMentionDto>>(null, MessageId.EntityDoesNotExist);
                List<UserMentionDto> users = new List<UserMentionDto>();
                if (file.UserId == null)
                {
                    users = await _authenticationService.GetAllUserHasAccessToFileAsync(authenticate.ContractCode, authenticate.Roles);
                    users = users.Where(a => (a.UserType == (int)UserStatus.OrganizationUser || a.UserType == (int)UserStatus.SupperUser)).ToList();
                }
                else
                {
                    List<int> userIds = new List<int>();
                    userIds = await _filedriveShareRepository.Where(a => !a.IsDeleted && a.Status == ShareEntityStatus.Active && a.FileId == file.FileId).Select(a => a.UserId).ToListAsync();
                    userIds.Add(file.UserId.Value);
                    users = await _userRepository.Where(a => !a.IsDeleted && a.IsActive && userIds.Contains(a.Id)).Select(a => new UserMentionDto
                    {
                        Id = a.Id,
                        Image =!String.IsNullOrEmpty(a.Image) ?_appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + a.Image : "",
                        Display = a.FullName,
                        UserType = a.UserType
                    }).ToListAsync();

                }
                return ServiceResultFactory.CreateSuccess(users);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<UserMentionDto>>(null, exception);
            }
        }
        public async Task<bool> SendEmailForUserMentionAsync(Guid fileId, string contractCode,
           string fileName, Guid directoryId, List<string> emails, List<CommentNotifViaEmailDTO> comments, long commentId,string linkPart)
        {
            string faMessage = $"به نام شما در فایل <span style='direction:ltr'>{fileName}</span> در فایل درایو پروژه <span style='direction:ltr'>{contractCode}</span>  اشاره شده است.برای مشاهده و پاسخ پیام رو دکمه «مشاهده پیام» در انتهای ایمیل کلیک کنید";
            string enMessage = $"";
            string link = _appSettings.ClientHost + $"/dashboard/FileStorage/{linkPart}?team={contractCode}&file={fileId.ToString()}&targetId={commentId}";
            CommentMentionNotif model = new CommentMentionNotif(faMessage, link, comments, _appSettings.CompanyName, enMessage);
            var emalRequest = new EmailRequest
            {
                Attachment = null,
                Subject = $"Mention | {fileName}",
                Body = await _viewRenderService.RenderToStringAsync("_SendCommentMentionNotifViaEmail", model)
            };

            foreach (var address in emails)
            {
                emalRequest.To = new List<string> { address };
                await _appEmailService.SendAsync(emalRequest);
            }
            return true;
        }

        private async Task<ServiceResult<FileDriveComment>> AddCommentAttachmentAsync(AuthenticateDto authenticate, FileDriveComment filedriveCommentModel, List<FileDriveCommentAttachmentDto> attachment)
        {
            filedriveCommentModel.Attachments = new List<FileDriveCommentAttachment>();

            foreach (var item in attachment)
            {
                var UploadedFile =
                    await _fileHelper.SaveDocumentFromTemp(item.FileSrc, ServiceSetting.UploadFilePathFileDriveComment(authenticate.ContractCode));
                if (UploadedFile == null)
                    return ServiceResultFactory.CreateError<FileDriveComment>(null, MessageId.UploudFailed);

                _fileHelper.DeleteDocumentFromTemp(item.FileSrc);
                filedriveCommentModel.Attachments.Add(new FileDriveCommentAttachment
                {
                    FileName = item.FileName,
                    FileSrc = item.FileSrc,
                    FileType = UploadedFile.FileType,
                    FileSize = UploadedFile.FileSize
                });
            }
            return ServiceResultFactory.CreateSuccess(filedriveCommentModel);
        }
    }
}
