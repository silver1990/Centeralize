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
using Raybod.SCM.DataTransferObject.RFP.RFPComment;
using Raybod.SCM.DataTransferObject.Audit;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Services.Utilitys.MailService.Model;
using Raybod.SCM.Services.Utilitys.MailService;
using Raybod.SCM.DataTransferObject._PanelDocument.Communication.Comment;
using Hangfire;
using Raybod.SCM.DataTransferObject.Email;
using Raybod.SCM.DataTransferObject.RFP;
using Raybod.SCM.DataTransferObject.User;
using Microsoft.AspNetCore.Http;

namespace Raybod.SCM.Services.Implementation
{

    public class RFPCommentService : IRFPCommentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITeamWorkAuthenticationService _authenticationService;
        private readonly ISCMLogAndNotificationService _scmLogAndNotificationService;
        private readonly IAppEmailService _appEmailService;
        private readonly DbSet<RFP> _RFPRepository;
        private readonly DbSet<RFPSupplier> _RFPSupplierRepository;
        private readonly DbSet<RFPComment> _RFPCommentRepository;
        private readonly DbSet<User> _userRepository;
        private readonly Raybod.SCM.Services.Utilitys.FileHelper _fileHelper;
        private readonly CompanyConfig _appSettings;
        private readonly IViewRenderService _viewRenderService;
        public RFPCommentService(IUnitOfWork unitOfWork, IWebHostEnvironment hostingEnvironmentRoot,
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
            _RFPSupplierRepository = _unitOfWork.Set<RFPSupplier>();
            _RFPCommentRepository = _unitOfWork.Set<RFPComment>();
            _RFPRepository = _unitOfWork.Set<RFP>();
            _userRepository = _unitOfWork.Set<User>();
            _viewRenderService = viewRenderService;
            httpContextAccessor.HttpContext.Request.Headers.TryGetValue("companyCode", out var CompanyCode);
            _appSettings = appSettings.Value.CompanyConfig.First(a => a.CompanyCode == CompanyCode);
        }

        public async Task<ServiceResult<string>> AddRFPCommentAsync(AuthenticateDto authenticate,long rfpId, long rfpSupplierId, AddRFPCommentDto model)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError("", MessageId.AccessDenied);

                var rfpModel = await _RFPSupplierRepository
                     .Where(a => !a.RFP.IsDeleted &&
                     !a.IsDeleted &&
                     a.IsActive &&
                     a.RFPId == rfpId &&
                     a.Id == rfpSupplierId &&
                     a.RFP.ContractCode == authenticate.ContractCode)
                     .Select(c => new
                     {
                         ProductGroupId = c.RFP.ProductGroupId,
                         ContractCode = c.RFP.ContractCode,
                         SupplierName = c.Supplier.Name,
                         RFPNumber = c.RFP.RFPNumber,
                         RFPId = c.RFPId,
                         SupplierId = c.SupplierId
                     }).FirstOrDefaultAsync();

                if (permission.ProductGroupIds.Any() && !permission.ProductGroupIds.Contains(rfpModel.ProductGroupId))
                    return ServiceResultFactory.CreateError("", MessageId.AccessDenied);

                if (rfpModel == null)
                    return ServiceResultFactory.CreateError("", MessageId.EntityDoesNotExist);

                var commentModel = new RFPComment();

                if (model.ParentCommentId != null && model.ParentCommentId > 0)
                {
                    var parentComment = await _RFPCommentRepository.Where(a => !a.IsDeleted && a.RFPSupplierId == rfpSupplierId
                     && a.Id == model.ParentCommentId.Value && a.ParentCommentId == null && a.RFPInqueryType == model.InqueryType)
                        .Select(c => new
                        {
                            id = c.Id,
                            isHasAnswer = c.ReplayComments.Any()
                        }).FirstOrDefaultAsync();

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

                    commentModel.RFPCommentUsers = new List<RFPCommentUser>();
                    foreach (var item in model.UserIds)
                    {
                        commentModel.RFPCommentUsers.Add(new RFPCommentUser
                        {
                            UserId = item
                        });
                    }
                }

                if (model.RFPInqueryIds != null && model.RFPInqueryIds.Any(a => a > 0))
                {
                    var rfpInqueryIds = await _RFPRepository.Where(a => a.Id == rfpId)
                        .SelectMany(c => c.RFPInqueries.Where(a => !a.IsDeleted).Select(v => v.Id).ToList())
                        .ToListAsync();

                    if (rfpInqueryIds == null)
                        return ServiceResultFactory.CreateError("", MessageId.DataInconsistency);
                    commentModel.RFPCommentInqueries = new List<RFPCommentInquery>();
                    foreach (var item in model.RFPInqueryIds)
                    {
                        commentModel.RFPCommentInqueries.Add(new RFPCommentInquery
                        {
                            RFPInqueryId = item
                        });
                    }
                }

                commentModel.RFPSupplierId = rfpSupplierId;
                commentModel.Message = model.Message;
                commentModel.RFPInqueryType = model.InqueryType;

                if (model.Attachments != null && model.Attachments.Count() > 0)
                {
                    if (!_fileHelper.FileExistInTemp(model.Attachments.Select(c => c.FileSrc).ToList()))
                        return ServiceResultFactory.CreateError("", MessageId.FileNotFound);
                    var attachmentResult = await AddRFPAttachmentAsync(commentModel, model.Attachments);
                    if (!attachmentResult.Succeeded)
                        return ServiceResultFactory.CreateError("",
                            attachmentResult.Messages.FirstOrDefault().Message);

                    commentModel = attachmentResult.Result;
                }

                _RFPCommentRepository.Add(commentModel);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var notifEvent = model.InqueryType == RFPInqueryType.TechnicalInquery ? NotifEvent.AddTechRFPComment :  NotifEvent.AddCommercialRFPComment;

                    var res = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = rfpModel.ContractCode,
                        KeyValue = rfpModel.RFPId.ToString(),
                        RootKeyValue = rfpModel.SupplierId.ToString(),
                        RootKeyValue2 = ((int)model.InqueryType).ToString(),
                        Quantity = commentModel.Id.ToString(),
                        Temp = rfpModel.SupplierName,
                        Description = commentModel.ParentCommentId == null ? "پرسش" : "پاسخ",
                        FormCode = rfpModel.RFPNumber,
                        ProductGroupId = rfpModel.ProductGroupId,
                        NotifEvent = notifEvent,
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,
                        Message = commentModel.Id.ToString()
                    }, null);

                    if (commentModel.RFPCommentUsers != null && commentModel.RFPCommentUsers.Any())
                    {
                        var res2 = await _scmLogAndNotificationService.AddMentionNotificationTaskAsync(new AddMentionLogDto
                        {
                            ContractCode = rfpModel.ContractCode,
                            KeyValue = rfpModel.RFPId.ToString(),
                            RootKeyValue = rfpModel.SupplierId.ToString(),
                            RootKeyValue2 = ((int)model.InqueryType).ToString(),
                            Description = commentModel.ParentCommentId == null ? "پرسش" : "پاسخ",
                            FormCode = rfpModel.RFPNumber,
                            ProductGroupId = rfpModel.ProductGroupId,
                            MentionEvent = model.InqueryType == RFPInqueryType.TechnicalInquery ? MentionEvent.AddTechRFPCommentMention : MentionEvent.AddCommercialRFPCommentMention,
                            PerformerUserId = authenticate.UserId,
                            PerformerUserFullName = authenticate.UserFullName,
                            ReceiverLogUserIds = commentModel.RFPCommentUsers.Select(a => a.UserId).ToList(),
                            Message= commentModel.Id.ToString(),
                            Temp = rfpModel.SupplierName
                        });
                    }
                    if (mentionUserEmails != null && mentionUserEmails.Any())
                    {
                        List<CommentNotifViaEmailDTO> comments = new List<CommentNotifViaEmailDTO>();
                        if (commentModel.ParentCommentId != null)
                        {

                            comments = await _RFPCommentRepository.OrderBy(c => c.Id).Where(c => c.ParentCommentId == commentModel.ParentCommentId || c.Id == commentModel.ParentCommentId).Select(c => new CommentNotifViaEmailDTO
                            {
                                Message = c.Message,
                                SendDate = c.CreatedDate.ToJalaliWithTime(),
                                SenderName = c.AdderUser.FullName
                            }).ToListAsync();
                        }
                        else
                        {

                            comments = await _RFPCommentRepository.OrderBy(c => c.Id).Where(c => c.ParentCommentId == commentModel.Id || c.Id == commentModel.Id).Select(c => new CommentNotifViaEmailDTO
                            {
                                Message = c.Message,
                                SendDate = c.CreatedDate.ToJalaliWithTime(),
                                SenderName = c.AdderUser.FullName
                            }).ToListAsync();
                        }
                        try
                        {
                            BackgroundJob.Enqueue(() => SendMailtoMentionUserAsync(rfpModel.RFPId, rfpModel.SupplierId, rfpModel.RFPNumber,rfpModel.ContractCode, model.InqueryType, rfpModel.SupplierName, authenticate.UserFullName, mentionUserEmails, comments, commentModel.Id));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }

                    return ServiceResultFactory.CreateSuccess(commentModel.Id.ToString());
                }
                return ServiceResultFactory.CreateError("", MessageId.AddEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException("", exception);
            }
        }


        public async Task<ServiceResult<string>> AddRFPProFormaCommentAsync(AuthenticateDto authenticate, long rfpId, long rfpSupplierId, AddProFromaCommentDto model)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError("", MessageId.AccessDenied);

                var rfpModel = await _RFPSupplierRepository
                     .Where(a => !a.RFP.IsDeleted &&
                     !a.IsDeleted &&
                     a.IsActive &&
                     a.RFPId == rfpId &&
                     a.Id == rfpSupplierId &&
                     a.RFP.ContractCode == authenticate.ContractCode)
                     .Select(c => new
                     {
                         ProductGroupId = c.RFP.ProductGroupId,
                         ContractCode = c.RFP.ContractCode,
                         SupplierName = c.Supplier.Name,
                         RFPNumber = c.RFP.RFPNumber,
                         RFPId = c.RFPId,
                         SupplierId = c.SupplierId
                     }).FirstOrDefaultAsync();

                if (permission.ProductGroupIds.Any() && !permission.ProductGroupIds.Contains(rfpModel.ProductGroupId))
                    return ServiceResultFactory.CreateError("", MessageId.AccessDenied);

                if (rfpModel == null)
                    return ServiceResultFactory.CreateError("", MessageId.EntityDoesNotExist);

                var commentModel = new RFPComment();

                if (model.ParentCommentId != null && model.ParentCommentId > 0)
                {
                    var parentComment = await _RFPCommentRepository.Where(a => !a.IsDeleted && a.RFPSupplierId == rfpSupplierId
                     && a.Id == model.ParentCommentId.Value && a.ParentCommentId == null && a.RFPInqueryType == (RFPInqueryType)3)
                        .Select(c => new
                        {
                            id = c.Id,
                            isHasAnswer = c.ReplayComments.Any()
                        }).FirstOrDefaultAsync();

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

                    commentModel.RFPCommentUsers = new List<RFPCommentUser>();
                    foreach (var item in model.UserIds)
                    {
                        commentModel.RFPCommentUsers.Add(new RFPCommentUser
                        {
                            UserId = item
                        });
                    }
                }

                

                commentModel.RFPSupplierId = rfpSupplierId;
                commentModel.Message = model.Message;
                commentModel.RFPInqueryType = (RFPInqueryType)3;

                if (model.Attachments != null && model.Attachments.Count() > 0)
                {
                    if (!_fileHelper.FileExistInTemp(model.Attachments.Select(c => c.FileSrc).ToList()))
                        return ServiceResultFactory.CreateError("", MessageId.FileNotFound);
                    var attachmentResult = await AddRFPAttachmentAsync(commentModel, model.Attachments);
                    if (!attachmentResult.Succeeded)
                        return ServiceResultFactory.CreateError("",
                            attachmentResult.Messages.FirstOrDefault().Message);

                    commentModel = attachmentResult.Result;
                }

                _RFPCommentRepository.Add(commentModel);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    

                    var res = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = rfpModel.ContractCode,
                        KeyValue = rfpModel.RFPId.ToString(),
                        RootKeyValue = rfpModel.SupplierId.ToString(),
                        RootKeyValue2 = (3).ToString(),
                        Quantity = commentModel.Id.ToString(),
                        Temp = rfpModel.SupplierName,
                        Description = commentModel.ParentCommentId == null ? "پرسش" : "پاسخ",
                        FormCode = rfpModel.RFPNumber,
                        ProductGroupId = rfpModel.ProductGroupId,
                        NotifEvent = NotifEvent.AddFPProFormaComment,
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,
                        Message = commentModel.Id.ToString()
                    }, null);

                    if (commentModel.RFPCommentUsers != null && commentModel.RFPCommentUsers.Any())
                    {
                        var res2 = await _scmLogAndNotificationService.AddMentionNotificationTaskAsync(new AddMentionLogDto
                        {
                            ContractCode = rfpModel.ContractCode,
                            KeyValue = rfpModel.RFPId.ToString(),
                            RootKeyValue = rfpModel.SupplierId.ToString(),
                            RootKeyValue2 = (3).ToString(),
                            Description = commentModel.ParentCommentId == null ? "پرسش" : "پاسخ",
                            FormCode = rfpModel.RFPNumber,
                            ProductGroupId = rfpModel.ProductGroupId,
                            MentionEvent = MentionEvent.AddRFPProFormaCommentMention,
                            PerformerUserId = authenticate.UserId,
                            PerformerUserFullName = authenticate.UserFullName,
                            ReceiverLogUserIds = commentModel.RFPCommentUsers.Select(a => a.UserId).ToList(),
                            Message = commentModel.Id.ToString(),
                            Temp = rfpModel.SupplierName
                        });
                    }
                    if (mentionUserEmails != null && mentionUserEmails.Any())
                    {
                        List<CommentNotifViaEmailDTO> comments = new List<CommentNotifViaEmailDTO>();
                        if (commentModel.ParentCommentId != null)
                        {

                            comments = await _RFPCommentRepository.OrderBy(c => c.Id).Where(c => c.ParentCommentId == commentModel.ParentCommentId || c.Id == commentModel.ParentCommentId).Select(c => new CommentNotifViaEmailDTO
                            {
                                Message = c.Message,
                                SendDate = c.CreatedDate.ToJalaliWithTime(),
                                SenderName = c.AdderUser.FullName
                            }).ToListAsync();
                        }
                        else
                        {

                            comments = await _RFPCommentRepository.OrderBy(c => c.Id).Where(c => c.ParentCommentId == commentModel.Id || c.Id == commentModel.Id).Select(c => new CommentNotifViaEmailDTO
                            {
                                Message = c.Message,
                                SendDate = c.CreatedDate.ToJalaliWithTime(),
                                SenderName = c.AdderUser.FullName
                            }).ToListAsync();
                        }
                        try
                        {
                            BackgroundJob.Enqueue(() => SendMailtoMentionUserAsync(rfpModel.RFPId, rfpModel.SupplierId, rfpModel.RFPNumber, rfpModel.ContractCode, (RFPInqueryType)3, rfpModel.SupplierName, authenticate.UserFullName, mentionUserEmails, comments, commentModel.Id));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }

                    return ServiceResultFactory.CreateSuccess(commentModel.Id.ToString());
                }
                return ServiceResultFactory.CreateError("", MessageId.AddEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException("", exception);
            }
        }
        public async Task<bool> SendMailtoMentionUserAsync(long rfpId, int supplierId, string rfpNumber, string contractCode,
           RFPInqueryType inqueryType, string supplierName, string userFullName, List<string> mentionUserEmails, List<CommentNotifViaEmailDTO> comments,long targetId)
        {
            if (!mentionUserEmails.Any())
                return true;

            string faInqryTypeDisply = inqueryType == RFPInqueryType.TechnicalInquery ? "درخواست پیشنهاد فنی" : inqueryType==RFPInqueryType.CommercialInquery? "درخواست پیشنهاد بازرگانی" : "درخواست پیش فاکتور";
            string enInqryTypeDisply = inqueryType == RFPInqueryType.TechnicalInquery ? "Technical Inquery" : inqueryType==RFPInqueryType.CommercialInquery? "Commercial Inquery" : "Proforma";
            string link = _appSettings.ClientHost + $"/dashboard/PurchaseEngineering/RFP/Details/{rfpId}?supplierid={supplierId}&inquiry={(int)inqueryType}&team={contractCode}&targetId={targetId}";
            string faMessage = $"{userFullName}، در پرسش و پاسخ {faInqryTypeDisply} <span style='direction:ltr'>{rfpNumber}</span> شرکت {supplierName} به شما اشاره کرده است. لطفا با مراجعه به آن بخش نظرات خود را به اشتراک بگذارید.";
            string enMessage = "";
            CommentMentionNotif model = new CommentMentionNotif(faMessage, link, comments,_appSettings.CompanyName,enMessage);
            var emalRequest = new EmailRequest
            {
                Attachment = null,
                Subject = $"Mention | {rfpNumber}",
                Body = await _viewRenderService.RenderToStringAsync("_SendCommentMentionNotifViaEmail", model)
            };

            foreach (var address in mentionUserEmails)
            {
                emalRequest.To = new List<string> { address };
                await _appEmailService.SendAsync(emalRequest);
            }

            

            return true;
        }

        public async Task<ServiceResult<List<RFPCommentListDto>>> GetRFPCommentAsync(AuthenticateDto authenticate,
            long rfpId, long rfpSupplierId, RFPCommentQueryDto query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<RFPCommentListDto>>(null, MessageId.AccessDenied);

                var dbQuery = _RFPRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.Id == rfpId &&
                    a.ContractCode == authenticate.ContractCode &&
                    a.RFPSuppliers.Any(c => !c.IsDeleted && c.IsActive && c.Id == rfpSupplierId));

                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(c => permission.ProductGroupIds.Contains(c.ProductGroupId));

                var rfpModel = await dbQuery.FirstOrDefaultAsync();

                if (rfpModel == null)
                    return ServiceResultFactory.CreateError<List<RFPCommentListDto>>(null, MessageId.EntityDoesNotExist);

                var dbComQuery = _RFPCommentRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.RFPSupplierId == rfpSupplierId && a.RFPInqueryType == query.InqueryType && a.ParentCommentId == null);

                var totalCount = dbComQuery.Count();
                //dbComQuery = dbComQuery.ApplayPageing(query);

                var result = await dbComQuery.Select(c => new RFPCommentListDto
                {
                    CommentId = c.Id,
                    Message = c.Message,
                    ParentCommentId = c.ParentCommentId,
                    Attachments = c.Attachments.Where(a => !a.IsDeleted)
                     .Select(a => new RFPCommentAttachmentDto
                     {
                         Id = a.Id,
                         FileName = a.FileName,
                         FileSize = a.FileSize,
                         FileSrc = a.FileSrc,
                         FileType = a.FileType,
                         RFPCommentId = a.RFPCommentId
                     }).ToList(),

                    UserAudit = c.AdderUser != null ? new UserAuditLogDto
                    {
                        AdderUserId = c.AdderUserId,
                        AdderUserName = c.AdderUser.FullName,
                        CreateDate = c.CreatedDate.ToUnixTimestamp(),
                        AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + c.AdderUser.Image
                    } : null,
                    UserMentions = c.RFPCommentUsers.Select(u => new UserMentionInfoDto
                    {
                        UserFullName = u.User.FullName,
                        UserId = u.UserId,
                        UserName = u.User.UserName
                    }).ToList(),
                    InqueryMentions = c.RFPCommentInqueries.Select(i => new InqueryMentionDto
                    {
                        InqueryId = i.RFPInqueryId,
                        Description = i.RFPInquery.Description
                    }).ToList(),
                    ReplayComments = c.ReplayComments.Where(v => !v.IsDeleted)
                     .Select(v => new RFPCommentListDto
                     {
                         CommentId = v.Id,
                         Message = v.Message,
                         ParentCommentId = v.ParentCommentId,
                         Attachments = v.Attachments.Where(b => !b.IsDeleted)
                         .Select(b => new RFPCommentAttachmentDto
                         {
                             Id = b.Id,
                             FileName = b.FileName,
                             FileSize = b.FileSize,
                             FileSrc = b.FileSrc,
                             FileType = b.FileType,
                             RFPCommentId = b.RFPCommentId
                         }).ToList(),
                         UserAudit = v.AdderUser != null ? new UserAuditLogDto
                         {
                             AdderUserId = v.AdderUserId,
                             AdderUserName = v.AdderUser.FullName,
                             CreateDate = v.CreatedDate.ToUnixTimestamp(),
                             AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + v.AdderUser.Image
                         } : null,
                         UserMentions = v.RFPCommentUsers.Select(u => new UserMentionInfoDto
                         {
                             UserFullName = u.User.FullName,
                             UserId = u.UserId,
                             UserName = u.User.UserName
                         }).ToList(),
                         InqueryMentions = v.RFPCommentInqueries.Select(i => new InqueryMentionDto
                         {
                             InqueryId = i.RFPInqueryId,
                             Description = i.RFPInquery.Description
                         }).ToList(),
                     }).ToList()
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<RFPCommentListDto>>(null, exception);
            }
        }
        public async Task<ServiceResult<List<BaseRFPProFormaCommentDto>>> GetRFPProFormaCommentAsync(AuthenticateDto authenticate,long rfpId, long rfpSupplierId, RFPCommentQueryDto query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<BaseRFPProFormaCommentDto>>(null, MessageId.AccessDenied);

                var dbQuery = _RFPRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.Id == rfpId &&
                    a.ContractCode == authenticate.ContractCode &&
                    a.RFPSuppliers.Any(c => !c.IsDeleted && c.IsActive && c.Id == rfpSupplierId));

                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(c => permission.ProductGroupIds.Contains(c.ProductGroupId));

                var rfpModel = await dbQuery.FirstOrDefaultAsync();

                if (rfpModel == null)
                    return ServiceResultFactory.CreateError<List<BaseRFPProFormaCommentDto>>(null, MessageId.EntityDoesNotExist);

                var dbComQuery = _RFPCommentRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.ParentCommentId==null&&a.RFPSupplierId == rfpSupplierId && a.RFPInqueryType == (RFPInqueryType)3).Include(a => a.Attachments);

                var totalCount = dbComQuery.Count();
                //dbComQuery = dbComQuery.ApplayPageing(query);

                var result = await dbComQuery.Select(c => new BaseRFPProFormaCommentDto
                {
                    CommentId = c.Id,
                    Message = c.Message,
                    ParentCommentId = c.ParentCommentId,
                    attachments = c.Attachments.Where(a => !a.IsDeleted)
                     .Select(a => new RFPCommentAttachmentDto
                     {
                         Id = a.Id,
                         FileName = a.FileName,
                         FileSize = a.FileSize,
                         FileSrc = a.FileSrc,
                         FileType = a.FileType,
                         RFPCommentId = a.RFPCommentId
                     }).ToList(),

                    UserAudit = c.AdderUser != null ? new UserAuditLogDto
                    {
                        AdderUserId = c.AdderUserId,
                        AdderUserName = c.AdderUser.FullName,
                        CreateDate = c.CreatedDate.ToUnixTimestamp(),
                        AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + c.AdderUser.Image
                    } : null,
                    UserMentions = c.RFPCommentUsers.Select(u => new UserMentionInfoDto
                    {
                        UserFullName = u.User.FullName,
                        UserId = u.UserId,
                        UserName = u.User.UserName
                    }).ToList(),
                    ReplayComments = c.ReplayComments.Where(v => !v.IsDeleted)
                     .Select(v => new RFPProFormaCommentListDto
                     {
                         CommentId = v.Id,
                         Message = v.Message,
                         ParentCommentId = v.ParentCommentId,
                         Attachments = v.Attachments.Where(b => !b.IsDeleted)
                         .Select(b => new RFPCommentAttachmentDto
                         {
                             Id = b.Id,
                             FileName = b.FileName,
                             FileSize = b.FileSize,
                             FileSrc = b.FileSrc,
                             FileType = b.FileType,
                             RFPCommentId = b.RFPCommentId
                         }).ToList(),
                         UserAudit = v.AdderUser != null ? new UserAuditLogDto
                         {
                             AdderUserId = v.AdderUserId,
                             AdderUserName = v.AdderUser.FullName,
                             CreateDate = v.CreatedDate.ToUnixTimestamp(),
                             AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + v.AdderUser.Image
                         } : null,
                         UserMentions = v.RFPCommentUsers.Select(u => new UserMentionInfoDto
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
                return ServiceResultFactory.CreateException<List<BaseRFPProFormaCommentDto>>(null, exception);
            }
        }

       


        public async Task<ServiceResult<List<UserMentionDto>>> GetUserMentionsAsync(AuthenticateDto authenticate, long rfpId)
        {
            try
            {
                var rfpModel = await _RFPRepository.FindAsync(rfpId);
                if (rfpModel == null)
                    return ServiceResultFactory.CreateError<List<UserMentionDto>>(null, MessageId.EntityDoesNotExist);

                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(new List<UserMentionDto>(), MessageId.AccessDenied);

                if (permission.ProductGroupIds.Any() && !permission.ProductGroupIds.Contains(rfpModel.ProductGroupId))
                    return ServiceResultFactory.CreateError(new List<UserMentionDto>(), MessageId.AccessDenied);

                var users = await _authenticationService.GetAllUserHasAccessPOAsync(rfpModel.ContractCode, authenticate.Roles, rfpModel.ProductGroupId);
                users = users.Where(a => (a.UserType == (int)UserStatus.OrganizationUser || a.UserType == (int)UserStatus.SupperUser)).ToList();
                if (users == null)
                    return ServiceResultFactory.CreateSuccess(new List<UserMentionDto>());

                var result = users.Select(u => new UserMentionDto
                {
                    Id = u.Id,
                    Display = u.Display,
                    Image =(!String.IsNullOrEmpty(u.Image))? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + u.Image:"",
                }).ToList();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<UserMentionDto>>(null, exception);
            }
        }


        private async Task<ServiceResult<RFPComment>> AddRFPAttachmentAsync(RFPComment rfpCommentModel, List<AddAttachmentDto> attachment)
        {
            rfpCommentModel.Attachments = new List<RFPAttachment>();

            foreach (var item in attachment)
            {
                var UploadedFile =
                    await _fileHelper.SaveDocumentFromTemp(item.FileSrc, ServiceSetting.UploadFilePath.RFPComment);
                if (UploadedFile == null)
                    return ServiceResultFactory.CreateError<RFPComment>(null, MessageId.UploudFailed);

                _fileHelper.DeleteDocumentFromTemp(item.FileSrc);
                rfpCommentModel.Attachments.Add(new RFPAttachment
                {
                    FileType = UploadedFile.FileType,
                    FileSize = UploadedFile.FileSize,
                    FileName = item.FileName,
                    FileSrc = item.FileSrc
                });
            }
            return ServiceResultFactory.CreateSuccess(rfpCommentModel);
        }

        public async Task<DownloadFileDto> DownloadRFPCommentAttachmentAsync(AuthenticateDto authenticate, long rfpId, long rfpSupplierId, long commentId, string fileSrc)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return null;

                var entity = await _RFPCommentRepository
                   .Where(a => !a.IsDeleted &&
                   a.RFPSupplier.RFPId == rfpId &&
                   a.RFPSupplier.RFP.ContractCode == authenticate.ContractCode &&
                   a.RFPSupplierId == rfpSupplierId &&
                   a.Id == commentId &&
                   a.Attachments.Any(c => !c.IsDeleted && c.FileSrc == fileSrc))
                   .Select(c => new
                   {
                       ContractCode = c.RFPSupplier.RFP.ContractCode,
                       ProductGroupId = c.RFPSupplier.RFP.ProductGroupId,
                       FileName=c.Attachments.Where(d=>d.FileSrc==fileSrc).Select(d=>d.FileName).FirstOrDefault()
                   }).FirstOrDefaultAsync();

                if (entity == null)
                    return null;

                if (permission.ProductGroupIds.Any() && !permission.ProductGroupIds.Contains(entity.ProductGroupId))
                    return null;

                var streamResult = await _fileHelper.DownloadAttachmentDocument(fileSrc, ServiceSetting.FileSection.RFPComment,entity.FileName);
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
