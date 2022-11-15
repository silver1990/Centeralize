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
using Raybod.SCM.DataTransferObject.PO.POComment;
using Raybod.SCM.DataTransferObject.Audit;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.DataTransferObject.Notification;
using Raybod.SCM.Domain.Struct;
using Raybod.SCM.Services.Utilitys.MailService.Model;
using Raybod.SCM.Services.Utilitys.MailService;
using Raybod.SCM.DataTransferObject.User;
using Raybod.SCM.DataTransferObject.Email;
using Raybod.SCM.DataTransferObject._PanelDocument.Communication.Comment;
using Hangfire;
using Microsoft.AspNetCore.Http;

namespace Raybod.SCM.Services.Implementation
{

    public class POCommentService : IPOCommentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITeamWorkAuthenticationService _authenticationService;
        private readonly ISCMLogAndNotificationService _scmLogAndNotificationService;
        private readonly IAppEmailService _appEmailService;
        private readonly DbSet<PO> _poRepository;
        private readonly DbSet<PAttachment> _PAttachmentRepository;
        private readonly DbSet<POComment> _poCommentRepository;
        private readonly DbSet<User> _userRepository;
        private readonly Raybod.SCM.Services.Utilitys.FileHelper _fileHelper;
        private readonly CompanyConfig _appSettings;
        private readonly IViewRenderService _viewRenderService;

        public POCommentService(IUnitOfWork unitOfWork, IWebHostEnvironment hostingEnvironmentRoot,
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
            _PAttachmentRepository = _unitOfWork.Set<PAttachment>();
            _poCommentRepository = _unitOfWork.Set<POComment>();
            _poRepository = _unitOfWork.Set<PO>();
            _userRepository = _unitOfWork.Set<User>();
            _viewRenderService = viewRenderService;
            httpContextAccessor.HttpContext.Request.Headers.TryGetValue("companyCode", out var CompanyCode);
            _appSettings = appSettings.Value.CompanyConfig.First(a => a.CompanyCode == CompanyCode);
        }

        public async Task<ServiceResult<string>> AddPOCommentAsync(AuthenticateDto authenticate, long poId, PoCommentType commentType, AddPOCommentDto model)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError("", MessageId.AccessDenied);

                var dbQuery = _poRepository
                    .Where(a => !a.IsDeleted && a.POId == poId &&
                    a.BaseContractCode == authenticate.ContractCode);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(c => permission.ProductGroupIds.Contains(c.ProductGroupId)))
                    return ServiceResultFactory.CreateError("", MessageId.AccessDenied);

                var poModel = await dbQuery
                    .Include(a => a.Supplier)
                    .FirstOrDefaultAsync();

                if (poModel == null)
                    return ServiceResultFactory.CreateError("", MessageId.EntityDoesNotExist);


                if (poModel.POStatus==POStatus.Canceled)
                    return ServiceResultFactory.CreateError("", MessageId.CantDoneBecausePOCanceled);

                var commentModel = new POComment();

                bool isSetTaskDone = false;
                if (model.ParentCommentId != null && model.ParentCommentId > 0)
                {
                    var parentComment = await _poCommentRepository.Where(a => !a.IsDeleted && a.POId == poId
                     && a.POCommentId == model.ParentCommentId.Value && a.ParentCommentId == null)
                        .Select(c => new
                        {
                            id = c.POCommentId,
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
                    //var users = await _authenticationService.GetTeamWorkRolesByRolesAndContractCode(rfpModel.ContractCode, authenticate.Roles);
                    var users = await _userRepository.Where(a => !a.IsDeleted && a.IsActive && model.UserIds.Contains(a.Id))
                        .Select(c => new
                        {
                            userId = c.Id,
                            email = c.Email
                        }).ToListAsync();

                    if (users == null || !users.Any())
                        return ServiceResultFactory.CreateError("", MessageId.DataInconsistency);

                    var userIds = users.Select(a => a.userId).ToList();
                    if (model.UserIds.Any(a => !userIds.Contains(a)))
                        return ServiceResultFactory.CreateError("", MessageId.DataInconsistency);

                    mentionUserEmails = users.Where(a => !string.IsNullOrEmpty(a.email)).Select(c => c.email).ToList();

                    commentModel.CommentUsers = new List<POCommentUser>();
                    foreach (var item in model.UserIds)
                    {
                        commentModel.CommentUsers.Add(new POCommentUser
                        {
                            UserId = item
                        });
                    }
                }

                commentModel.POId = poId;
                commentModel.CommentType = commentType;
                commentModel.Message = model.Message;

                if (model.Attachments != null && model.Attachments.Count() > 0)
                {
                    if (!_fileHelper.FileExistInTemp(model.Attachments.Select(c => c.FileSrc).ToList()))
                        return ServiceResultFactory.CreateError("", MessageId.FileNotFound);
                    var attachmentResult = await AddPOAttachmentAsync(commentModel, model.Attachments);
                    if (!attachmentResult.Succeeded)
                        return ServiceResultFactory.CreateError("",
                            attachmentResult.Messages.FirstOrDefault().Message);

                    commentModel = attachmentResult.Result;
                }

                _poCommentRepository.Add(commentModel);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var message = model.ParentCommentId == null || model.ParentCommentId <= 0 ? "پرسش" : "پاسخ";

                    var res = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = authenticate.ContractCode,
                        RootKeyValue = poModel.POId.ToString(),
                        Description = message,
                        FormCode = poModel.POCode,
                        Temp = poModel.Supplier.Name,
                        KeyValue = commentModel.POCommentId.ToString(),
                        NotifEvent =(commentType==PoCommentType.Po)? NotifEvent.AddPOComment:(commentType==PoCommentType.Inspection)?NotifEvent.AddPOInspectionComment: (commentType == PoCommentType.SupplierDocument)? NotifEvent.AddPOSupplierDocumentComment : NotifEvent.AddPOFinancialComment,
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,
                        ProductGroupId = poModel.ProductGroupId,
                    }, null);

                    if (commentModel.CommentUsers != null && commentModel.CommentUsers.Any())
                        await _scmLogAndNotificationService.AddMentionNotificationTaskAsync(new AddMentionLogDto
                        {
                            ContractCode = authenticate.ContractCode,
                            RootKeyValue = poModel.POId.ToString(),
                            Description = message,
                            FormCode = poModel.POCode,
                            KeyValue = commentModel.POCommentId.ToString(),
                            MentionEvent = (commentType == PoCommentType.Po) ? MentionEvent.MentionPOComment : (commentType == PoCommentType.Inspection) ? MentionEvent.MentionPOInspectionComment : (commentType == PoCommentType.SupplierDocument) ? MentionEvent.MentionPOSupplierDocumentComment:MentionEvent.MentionPOFinancialComment,
                            PerformerUserId = authenticate.UserId,
                            PerformerUserFullName = authenticate.UserFullName,
                            ProductGroupId = poModel.ProductGroupId,
                            ReceiverLogUserIds = commentModel.CommentUsers.Select(a => a.UserId).ToList(),
                            Temp = poModel.Supplier.Name
                        });
                    if (mentionUserEmails != null && mentionUserEmails.Any())
                    {
                        List<CommentNotifViaEmailDTO> comments = new List<CommentNotifViaEmailDTO>();
                        if (commentModel.ParentCommentId != null)
                        {

                            comments = await _poCommentRepository.OrderBy(c => c.POCommentId).Where(c => c.ParentCommentId == commentModel.ParentCommentId || c.POCommentId == commentModel.ParentCommentId).Select(c => new CommentNotifViaEmailDTO
                            {
                                Message = c.Message,
                                SendDate = c.CreatedDate.ToJalaliWithTime(),
                                SenderName = c.AdderUser.FullName
                            }).ToListAsync();
                        }
                        else
                        {

                            comments = await _poCommentRepository.OrderBy(c => c.POCommentId).Where(c => c.ParentCommentId == commentModel.POCommentId || c.POCommentId == commentModel.POCommentId).Select(c => new CommentNotifViaEmailDTO
                            {
                                Message = c.Message,
                                SendDate = c.CreatedDate.ToJalaliWithTime(),
                                SenderName = c.AdderUser.FullName
                            }).ToListAsync();
                        }
                        try
                        {
                            BackgroundJob.Enqueue(() => SendMailtoMentionUserAsync(poModel, authenticate.UserFullName, mentionUserEmails,comments, commentModel.POCommentId,commentType));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                  
                    return ServiceResultFactory.CreateSuccess(commentModel.POCommentId.ToString());
                }
                return ServiceResultFactory.CreateError("", MessageId.AddEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException("", exception);
            }
        }

        public async Task<bool> SendMailtoMentionUserAsync(PO poModel, string userFullName, List<string> mentionUserEmails, List<CommentNotifViaEmailDTO> comments,long targetId,PoCommentType commentType)
        {
            if (!mentionUserEmails.Any())
                return true;

            string link = _appSettings.ClientHost + $"/dashboard/ProcurementManagement/PurchaseOrders/PoDetails/{poModel.POId}?team={poModel.BaseContractCode}&targetId={targetId}&commentType={commentType}";
            string faMessage =(commentType==PoCommentType.Po)? $"{userFullName}، در پرسش و پاسخ سفارش خرید <span style='direction:ltr'>{poModel.POCode}</span> شرکت {poModel.Supplier.Name} به شما اشاره کرده است. لطفا با مراجعه به آن بخش نظرات خود را به اشتراک بگذارید."
            : (commentType == PoCommentType.Inspection) ? $"{userFullName}، در پرسش و پاسخ بازرسی سفارش خرید <span style='direction:ltr'>{poModel.POCode}</span> شرکت {poModel.Supplier.Name} به شما اشاره کرده است. لطفا با مراجعه به آن بخش نظرات خود را به اشتراک بگذارید."
            : (commentType == PoCommentType.SupplierDocument)? $"{userFullName}، در پرسش و پاسخ مدارک تامین کننده سفارش خرید <span style='direction:ltr'>{poModel.POCode}</span> شرکت {poModel.Supplier.Name} به شما اشاره کرده است. لطفا با مراجعه به آن بخش نظرات خود را به اشتراک بگذارید.":
            $"{userFullName}، در پرسش و پاسخ مالی سفارش خرید <span style='direction:ltr'>{poModel.POCode}</span> شرکت {poModel.Supplier.Name} به شما اشاره کرده است. لطفا با مراجعه به آن بخش نظرات خود را به اشتراک بگذارید.";

            string enMessage = "";
            CommentMentionNotif model = new CommentMentionNotif(faMessage, link, comments,_appSettings.CompanyName,enMessage);

            var emalRequest = new EmailRequest
            {
                Attachment = null,
                Subject = $"Mention | {poModel.POCode}",
                Body = await _viewRenderService.RenderToStringAsync("_SendCommentMentionNotifViaEmail", model)
            };

            foreach (var item in mentionUserEmails)
            {
                emalRequest.To = new List<string> { item };
                await _appEmailService.SendAsync(emalRequest);
            }

            return true;
        }

       
        public async Task<ServiceResult<List<POCommentListDto>>> GetPOCommentAsync(AuthenticateDto authenticate,long poId, PoCommentType commentType, POCommentQueryDto query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<POCommentListDto>>(null, MessageId.AccessDenied);

                var dbQuery = _poRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.POId == poId &&
                    a.BaseContractCode == authenticate.ContractCode);

                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(c => permission.ProductGroupIds.Contains(c.ProductGroupId));

                var rfpModel = await dbQuery.FirstOrDefaultAsync();

                if (rfpModel == null)
                    return ServiceResultFactory.CreateError<List<POCommentListDto>>(null, MessageId.EntityDoesNotExist);

                var dbComQuery = _poCommentRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.POId == poId&&a.CommentType==commentType && a.ParentCommentId == null);

                var totalCount = dbComQuery.Count();
                //dbComQuery = dbComQuery.ApplayPageing(query);

                var result = await dbComQuery.Select(c => new POCommentListDto
                {
                    CommentId = c.POCommentId,
                    Message = c.Message,
                    ParentCommentId = c.ParentCommentId,
                    attachments = c.Attachments.Where(a => !a.IsDeleted)
                     .Select(a => new POCommentAttachmentDto
                     {
                         Id = a.Id,
                         FileName = a.FileName,
                         FileSize = a.FileSize,
                         FileSrc = a.FileSrc,
                         FileType = a.FileType,
                         POCommentId = a.POCommentId
                     }).ToList(),

                    UserAudit = c.AdderUser != null ? new UserAuditLogDto
                    {
                        AdderUserId = c.AdderUserId,
                        AdderUserName = c.AdderUser.FullName,
                        CreateDate = c.CreatedDate.ToUnixTimestamp(),
                        AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + c.AdderUser.Image
                    } : null,
                    UserMentions = c.CommentUsers.Select(u => new UserMentionInfoDto
                    {
                        UserFullName = u.User.FullName,
                        UserId = u.UserId,
                        UserName = u.User.UserName
                    }).ToList(),
                    ReplayComments = c.ReplayComments.Where(v => !v.IsDeleted)
                     .Select(v => new POCommentListDto
                     {
                         CommentId = v.POCommentId,
                         Message = v.Message,
                         ParentCommentId = v.ParentCommentId,
                         attachments = v.Attachments.Where(b => !b.IsDeleted)
                         .Select(b => new POCommentAttachmentDto
                         {
                             Id = b.Id,
                             FileName = b.FileName,
                             FileSize = b.FileSize,
                             FileSrc = b.FileSrc,
                             FileType = b.FileType,
                             POCommentId = b.POCommentId
                         }).ToList(),
                         UserAudit = v.AdderUser != null ? new UserAuditLogDto
                         {
                             AdderUserId = v.AdderUserId,
                             AdderUserName = v.AdderUser.FullName,
                             CreateDate = v.CreatedDate.ToUnixTimestamp(),
                             AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + v.AdderUser.Image
                         } : null,
                         UserMentions = v.CommentUsers.Select(u => new UserMentionInfoDto
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
                return ServiceResultFactory.CreateException<List<POCommentListDto>>(null, exception);
            }
        }

       
        public async Task<ServiceResult<List<UserMentionDto>>> GetUserMentionsAsync(AuthenticateDto authenticate, long poId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<UserMentionDto>>(null, MessageId.AccessDenied);

                var productGroupId = await _poRepository
                    .Where(a => !a.IsDeleted && a.POId == poId && a.BaseContractCode == authenticate.ContractCode)
                    .Select(c => c.ProductGroupId)
                    .FirstOrDefaultAsync();

                var users = await _authenticationService.GetAllUserHasAccessPOAsync(authenticate.ContractCode, authenticate.Roles, productGroupId);
                users = users.Where(a => (a.UserType == (int)UserStatus.OrganizationUser || a.UserType == (int)UserStatus.SupperUser)).ToList();
                return ServiceResultFactory.CreateSuccess(users);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<UserMentionDto>>(null, exception);
            }
        }


        private async Task<ServiceResult<POComment>> AddPOAttachmentAsync(POComment poCommentModel, List<AddAttachmentDto> attachment)
        {
            poCommentModel.Attachments = new List<PAttachment>();

            foreach (var item in attachment)
            {
                var UploadedFile =
                    await _fileHelper.SaveDocumentFromTemp(item.FileSrc, ServiceSetting.UploadFilePath.POComment);
                if (UploadedFile == null)
                    return ServiceResultFactory.CreateError<POComment>(null, MessageId.UploudFailed);

                _fileHelper.DeleteDocumentFromTemp(item.FileSrc);
                poCommentModel.Attachments.Add(new PAttachment
                {
                    FileType = UploadedFile.FileType,
                    FileSize = UploadedFile.FileSize,
                    FileName = item.FileName,
                    FileSrc = item.FileSrc
                });
            }
            return ServiceResultFactory.CreateSuccess(poCommentModel);
        }

        public async Task<DownloadFileDto> DownloadPOCommentAttachmentAsync(AuthenticateDto authenticate, long poId, long commentId,PoCommentType commentType, string fileSrc)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return null;

                var entity = await _poCommentRepository
                   .Where(a => !a.IsDeleted &&
                   a.POCommentId == commentId &&
                   a.POId == poId &&
                   a.PO.BaseContractCode == authenticate.ContractCode &&
                   a.Attachments.Any(c => !c.IsDeleted && c.FileSrc == fileSrc))
                   .Select(c => new
                   {
                       ContractCode = c.PO.BaseContractCode,
                       ProductGroupId = c.PO.ProductGroupId,
                   }).FirstOrDefaultAsync();

                if (entity == null)
                    return null;
                
                if (permission.ProductGroupIds.Any() && !permission.ProductGroupIds.Contains(entity.ProductGroupId))
                    return null;
                var fileName = await _PAttachmentRepository.Where(a => !a.IsDeleted && a.FileSrc == fileSrc).FirstOrDefaultAsync();
                if(fileName==null)
                    return null;
                var streamResult = await _fileHelper.DownloadAttachmentDocument(fileSrc, ServiceSetting.FileSection.POComment,fileName.FileName);
                if (streamResult == null)
                    return null;
                
                return streamResult;
            }
            catch (Exception exception)
            {
                return null;
            }
        }


    }
}
