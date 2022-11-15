using Exon.TheWeb.Service.Core;
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
using Raybod.SCM.DataTransferObject.Document.Communication;
using Raybod.SCM.DataTransferObject.Email;
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
using Raybod.SCM.Utility.FileHelper;
using Raybod.SCM.Utility.Helpers;
using Raybod.SCM.Utility.Utility;
using SelectPdf;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Implementation
{
    public class CommunicationCommentService : ICommunicationCommentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;
        private readonly IAppEmailService _appEmailService;
        private readonly IContractFormConfigService _formConfigService;
        private readonly DbSet<DocumentRevision> _documentRevisionRepository;
        private readonly DbSet<Customer> _customerRepository;
        private readonly DbSet<Consultant> _consultantRepository;
        private readonly DbSet<DocumentCommunication> _documentCommunicationRepository;
        private readonly DbSet<CommunicationAttachment> _communicationAttachmentRepository;
        private readonly DbSet<Transmittal> _transmittalRepository;
        private readonly DbSet<CompanyUser> _companyUserRepository;
        private readonly DbSet<UserNotify> _notifyRepository;
        private readonly DbSet<User> _userRepository;
        private readonly DbSet<PDFTemplate> _pdfTemplateRepository;
        private readonly ITeamWorkAuthenticationService _authenticationServices;
        private readonly ISCMLogAndNotificationService _scmLogAndNotificationService;
        private readonly Utilitys.FileHelper _fileHelper;
        private readonly CompanyConfig _appSettings;
        private readonly IViewRenderService _viewRenderService;

        public CommunicationCommentService(
            IUnitOfWork unitOfWork,
            IWebHostEnvironment hostingEnvironmentRoot,
            IOptions<CompanyAppSettingsDto> appSettings,
            ITeamWorkAuthenticationService authenticationServices,
            ISCMLogAndNotificationService scmLogAndNotificationService,
            IFileService fileService,
            IHttpContextAccessor httpContextAccessor,
            IAppEmailService appEmailService,
            IContractFormConfigService formConfigService, IViewRenderService viewRenderService)
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
            _authenticationServices = authenticationServices;
            _formConfigService = formConfigService;
            _appEmailService = appEmailService;
            _scmLogAndNotificationService = scmLogAndNotificationService;
            _documentRevisionRepository = _unitOfWork.Set<DocumentRevision>();
            _documentCommunicationRepository = _unitOfWork.Set<DocumentCommunication>();
            _communicationAttachmentRepository = _unitOfWork.Set<CommunicationAttachment>();
            _transmittalRepository = _unitOfWork.Set<Transmittal>();
            _customerRepository = _unitOfWork.Set<Customer>();
            _userRepository = _unitOfWork.Set<User>();
            _companyUserRepository = _unitOfWork.Set<CompanyUser>();
            _consultantRepository = _unitOfWork.Set<Consultant>();
            _notifyRepository = _unitOfWork.Set<UserNotify>();
            _pdfTemplateRepository = _unitOfWork.Set<PDFTemplate>();
            _fileHelper = new Utilitys.FileHelper(hostingEnvironmentRoot);
            _viewRenderService = viewRenderService;
            httpContextAccessor.HttpContext.Request.Headers.TryGetValue("companyCode", out var CompanyCode);
            _appSettings = appSettings.Value.CompanyConfig.First(a => a.CompanyCode == CompanyCode);
        }

        public async Task<ServiceResult<bool>> AddCommunicationCommentAsync(AuthenticateDto authenticate, long revisionId, AddCommunicationCommentDto model)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery = _documentRevisionRepository
                    .Where(a => !a.IsDeleted &&
                    a.Document.ContractCode == authenticate.ContractCode &&
                    a.DocumentRevisionId == revisionId &&
                    a.IsLastConfirmRevision &&
                    a.RevisionStatus >= RevisionStatus.Confirmed);

                var revisionModel = await dbQuery
                    .Include(a => a.Document)
                    .FirstOrDefaultAsync();

                if (revisionModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (permission.DocumentGroupIds.Any() && !permission.DocumentGroupIds.Contains(revisionModel.Document.DocumentGroupId))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                if (model.Questions == null || !model.Questions.Any() || model.Questions.Any(c => string.IsNullOrEmpty(c.Description)))
                    return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                if (!EnumHelper.ValidateItem(model.CommentStatus))
                    return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);


                if (!await _customerRepository.AnyAsync(a => !a.IsDeleted && a.Id == model.CustomerId) && model.CompanyIssue == CompanyIssue.Customer)
                    return ServiceResultFactory.CreateError(false, MessageId.CustomerNotFound);
                if (!await _consultantRepository.AnyAsync(a => !a.IsDeleted && a.Id == model.CustomerId) && model.CompanyIssue == CompanyIssue.Consultant)
                    return ServiceResultFactory.CreateError(false, MessageId.ConsultantNotFound);
                var communicationModel = new DocumentCommunication
                {
                    CommentStatus = model.CommentStatus,
                    CommunicationType = CommunicationType.Comment,
                    CommunicationStatus = model.CommentStatus == CommunicationCommentStatus.Commented ? DocumentCommunicationStatus.PendingReply : DocumentCommunicationStatus.Closed,
                    CustomerId = model.CompanyIssue == CompanyIssue.Customer ? model.CustomerId : (int?)null,
                    ConsultantId = model.CompanyIssue == CompanyIssue.Consultant ? model.CustomerId : (int?)null,
                    CompanyIssue = model.CompanyIssue,
                    DocumentRevisionId = revisionId,
                    CommunicationQuestions = new List<CommunicationQuestion>()
                };

                foreach (var item in model.Questions)
                {
                    communicationModel.CommunicationQuestions.Add(new CommunicationQuestion
                    {
                        Description = item.Description,
                        Attachments = new List<CommunicationAttachment>()
                    });
                }

                // add attachment
                if (model.Attachments != null && model.Attachments.Any())
                {
                    var res = await AddCommunicationAttachmentAsync(authenticate, revisionModel.DocumentId, revisionId, model.Attachments);
                    if (!res.Succeeded)
                        return ServiceResultFactory.CreateError(false, res.Messages[0].Message);
                    else
                    {
                        communicationModel.CommunicationQuestions.First().Attachments = res.Result;
                    }
                }

                // generate form code
                string counter = "";
                var communications = await _documentCommunicationRepository
                    .Where(a => a.DocumentRevision.Document.ContractCode == authenticate.ContractCode).ToListAsync();
               
                if (communications != null && communications.Any())
                {
                    var lastCommunication = communications.OrderByDescending(a => a.CommunicationCode, new CompareFormNumbers()).FirstOrDefault();
                    if (lastCommunication == null)
                        counter = "0";
                    else
                    {
                        var codeConfig = await _formConfigService.GetFormCodeAsync(authenticate.ContractCode, FormName.CommunicationComment);
                        var codePattern = codeConfig.Result.FixedPart;
                         counter = lastCommunication.CommunicationCode.Substring(codePattern.Length);

                    }
                }

                else
                {
                    counter = "0";
                }

                var codeRes = await _formConfigService.GenerateFormCodeAsync(authenticate.ContractCode, FormName.CommunicationComment, 0, counter);
                if (!codeRes.Succeeded)
                    return ServiceResultFactory.CreateError(false, codeRes.Messages.First().Message);

                communicationModel.CommunicationCode = codeRes.Result;

                revisionModel.Document.CommunicationCommentStatus = model.CommentStatus;

                _documentCommunicationRepository.Add(communicationModel);

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    await _scmLogAndNotificationService.SetDonedNotificationAsync(authenticate.ContractCode, revisionModel.DocumentRevisionCode, revisionId.ToString(), NotifEvent.TransmittalPendingForComment);
                    var task = new List<NotifToDto> {
                    new NotifToDto
                    {
                        NotifEvent= NotifEvent.ReplyComComment,
                        Roles= new List<string>
                        {
                           SCMRole.ComCommentMng,
                           SCMRole.ComCommentReply,
                        }
                    }
                  };

                    if (communicationModel.CommentStatus != CommunicationCommentStatus.Commented)
                        task = null;

                    var res1 = await _scmLogAndNotificationService.AddDocumentAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = authenticate.ContractCode,
                        Description = revisionModel.DocumentRevisionCode,
                        FormCode = communicationModel.CommunicationCode,
                        Message = revisionModel.Document.DocTitle,
                        KeyValue = communicationModel.DocumentCommunicationId.ToString(),
                        NotifEvent = NotifEvent.AddComComment,
                        RootKeyValue = revisionModel.DocumentRevisionId.ToString(),
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,
                        DocumentGroupId = revisionModel.Document.DocumentGroupId
                    },
                    revisionModel.Document.DocumentGroupId, task);
                    try
                    {
                        BackgroundJob.Enqueue(() => SendEmailOnAddCommunicationAsync(authenticate, communicationModel, revisionModel));
                        //BackgroundJob.Enqueue(()=> SendEmailOnAddCommunicationForCustomerUserAsync(authenticate, communicationModel, revisionModel));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.StackTrace);
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
        public async Task<ServiceResult<bool>> AddCommunicationCommentForCustomerUserAsync(AuthenticateDto authenticate, long revisionId, AddCommunicationCommentDto model, bool accessability)
        {
            try
            {
                if (!accessability)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery = _documentRevisionRepository
                    .Where(a => !a.IsDeleted &&
                    a.Document.ContractCode == authenticate.ContractCode &&
                    a.DocumentRevisionId == revisionId &&
                    a.RevisionStatus >= RevisionStatus.Confirmed);

                var revisionModel = await dbQuery
                    .Include(a => a.Document)
                    .FirstOrDefaultAsync();

                if (revisionModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);



                if (model.Questions == null || !model.Questions.Any() || model.Questions.Any(c => string.IsNullOrEmpty(c.Description)))
                    return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                if (!EnumHelper.ValidateItem(model.CommentStatus))
                    return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);


                var customer = await _customerRepository.FirstOrDefaultAsync(a => !a.IsDeleted && a.CustomerUsers.Any(b => !b.IsDeleted && b.Email.ToLower() == authenticate.UserName.ToLower()));
                if (customer == null)
                {
                    var consultant = await _consultantRepository.FirstOrDefaultAsync(a => !a.IsDeleted && a.ConsultantUsers.Any(b => !b.IsDeleted && b.Email.ToLower() == authenticate.UserName.ToLower()));
                    if (consultant == null)
                    {
                        return ServiceResultFactory.CreateError(false, MessageId.CustomerNotFound);
                    }

                    else
                    {
                        model.CustomerId = consultant.Id;
                        model.CompanyIssue = CompanyIssue.Consultant;
                    }

                }

                else
                {
                    model.CustomerId = customer.Id;
                    model.CompanyIssue = CompanyIssue.Customer;
                }



                if (!await _customerRepository.AnyAsync(a => !a.IsDeleted && a.Id == model.CustomerId) && model.CompanyIssue == CompanyIssue.Customer)
                    return ServiceResultFactory.CreateError(false, MessageId.CustomerNotFound);
                if (!await _consultantRepository.AnyAsync(a => !a.IsDeleted && a.Id == model.CustomerId) && model.CompanyIssue == CompanyIssue.Consultant)
                    return ServiceResultFactory.CreateError(false, MessageId.ConsultantNotFound);

                var communicationModel = new DocumentCommunication
                {
                    CommentStatus = model.CommentStatus,
                    CommunicationType = CommunicationType.Comment,
                    CommunicationStatus = model.CommentStatus == CommunicationCommentStatus.Commented ? DocumentCommunicationStatus.PendingReply : DocumentCommunicationStatus.Closed,
                    CustomerId = model.CompanyIssue == CompanyIssue.Customer ? model.CustomerId : (int?)null,
                    ConsultantId = model.CompanyIssue == CompanyIssue.Consultant ? model.CustomerId : (int?)null,
                    CompanyIssue = model.CompanyIssue,
                    DocumentRevisionId = revisionId,
                    CommunicationQuestions = new List<CommunicationQuestion>()
                };

                foreach (var item in model.Questions)
                {
                    communicationModel.CommunicationQuestions.Add(new CommunicationQuestion
                    {
                        Description = item.Description,
                        Attachments = new List<CommunicationAttachment>()
                    });
                }

                // add attachment
                if (model.Attachments != null && model.Attachments.Any())
                {
                    var res = await AddCommunicationAttachmentAsync(authenticate, revisionModel.DocumentId, revisionId, model.Attachments);
                    if (!res.Succeeded)
                        return ServiceResultFactory.CreateError(false, res.Messages[0].Message);
                    else
                    {
                        communicationModel.CommunicationQuestions.First().Attachments = res.Result;
                    }
                }

                string counter = "";
                var communications = await _documentCommunicationRepository
                     .Where(a => a.DocumentRevision.Document.ContractCode == authenticate.ContractCode).ToListAsync();
                if(communications!=null&& communications.Any())
                {
                    var lastCommunication = communications.OrderByDescending(a => a.CommunicationCode, new CompareFormNumbers()).FirstOrDefault();
                    if (lastCommunication == null)
                        counter = "0";
                    else
                    {
                        var codeConfig = await _formConfigService.GetFormCodeAsync(authenticate.ContractCode, FormName.CommunicationComment);
                        var codePattern = codeConfig.Result.FixedPart;
                        counter = lastCommunication.CommunicationCode.Substring(codePattern.Length);
                    }
                }
                
                else
                {
                    counter = "0";
                }
                // generate form code
                var count = await _documentCommunicationRepository
                    .CountAsync(a => a.DocumentRevision.Document.ContractCode == authenticate.ContractCode);
                var codeRes = await _formConfigService.GenerateFormCodeAsync(authenticate.ContractCode, FormName.CommunicationComment, 0, counter);
                if (!codeRes.Succeeded)
                    return ServiceResultFactory.CreateError(false, codeRes.Messages.First().Message);

                communicationModel.CommunicationCode = codeRes.Result;
                if (revisionModel.IsLastConfirmRevision)
                {
                    revisionModel.Document.CommunicationCommentStatus = model.CommentStatus;
                }


                _documentCommunicationRepository.Add(communicationModel);

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    await _scmLogAndNotificationService.SetDonedNotificationAsync(authenticate.ContractCode, revisionModel.DocumentRevisionCode, revisionId.ToString(), NotifEvent.TransmittalPendingForComment);
                    var task = new List<NotifToDto> {
                    new NotifToDto
                    {
                        NotifEvent= NotifEvent.ReplyComComment,
                        Roles= new List<string>
                        {
                           SCMRole.ComCommentMng,
                           SCMRole.ComCommentReply,
                        }
                    }
                  };

                    if (communicationModel.CommentStatus != CommunicationCommentStatus.Commented)
                        task = null;

                    var res1 = await _scmLogAndNotificationService.AddDocumentAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = authenticate.ContractCode,
                        Description = revisionModel.DocumentRevisionCode,
                        FormCode = communicationModel.CommunicationCode,
                        Message = revisionModel.Document.DocTitle,
                        KeyValue = communicationModel.DocumentCommunicationId.ToString(),
                        NotifEvent = NotifEvent.AddComComment,
                        RootKeyValue = revisionModel.DocumentRevisionId.ToString(),
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,
                        DocumentGroupId = revisionModel.Document.DocumentGroupId
                    },
                 revisionModel.Document.DocumentGroupId, task);
                    try
                    {
                        BackgroundJob.Enqueue(() => SendEmailOnAddCommunicationAsync(authenticate, communicationModel, revisionModel));
                        //BackgroundJob.Enqueue(() => SendEmailOnAddCommunicationForCustomerUserAsync(authenticate, communicationModel, revisionModel));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.StackTrace);
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
        public async Task<ServiceResult<bool>> SendEmailOnAddCommunicationAsync(AuthenticateDto authenticate, DocumentCommunication communicationModel, DocumentRevision revisionModel)
        {
            var toRoles = new List<string> {
                SCMRole.ComCommentReply,
                SCMRole.ComCommentMng,
                SCMRole.ComCommentObs,
                SCMRole.ComCommentReg
                };

            var toUsers = await _authenticationServices
                .GetAllUserHasAccessDocumentAsync(authenticate.ContractCode, toRoles, revisionModel.Document.DocumentGroupId);
            var commentEmailNotify = await _notifyRepository.Where(a => a.TeamWork.ContractCode == authenticate.ContractCode && a.NotifyType == NotifyManagementType.Email && a.NotifyNumber == (int)EmailNotify.Comment && a.IsActive).Select(a => a.UserId).ToListAsync();
            if (commentEmailNotify != null && commentEmailNotify.Any())
                toUsers = toUsers.Where(a => commentEmailNotify.Contains(a.Id)).ToList();
            else
                toUsers = new List<UserMentionDto>();
            var toUserEmails = (toUsers != null && toUsers.Any()) ? toUsers
                .Where(a => !string.IsNullOrEmpty(a.Email))
                .GroupBy(a => a.Email)
                .Select(a => a.Key)
                .ToList() : new List<string>();

            if (!toUserEmails.Any())
                return ServiceResultFactory.CreateSuccess(true);

            var attachments = await GenerateCommentAttachmentZipAsync(authenticate, communicationModel.DocumentCommunicationId);


            string faMessage = $"در پروژه <span style='direction:ltr'>{authenticate.ContractCode}</span> یک کامنت جدید به شماره <span style='direction:ltr'>{communicationModel.CommunicationCode}</span> مربوط به مدرک <span style='direction:ltr'>{revisionModel.Document.DocTitle}</span> به شماره <span style='direction:ltr'>{revisionModel.Document.DocNumber}</span> توسط {authenticate.UserFullName} به شرح زیر ثبت گردید. ";
            string enMessage = $"<div style='direction:ltr;text-align:left'>Comment No. {communicationModel.CommunicationCode} related to document titled {revisionModel.Document.DocTitle} - {revisionModel.Document.DocNumber} has been registered  by {authenticate.UserFullName} as following.</div>";
            List<CommentNotifViaEmailDTO> comments = new List<CommentNotifViaEmailDTO>();

            CommentNotifViaEmailDTO comment = null;
            foreach (var item in communicationModel.CommunicationQuestions)
            {
                comment = new CommentNotifViaEmailDTO { Message = item.Description, SendDate = item.CreatedDate.ToJalaliWithTime(), SenderName = authenticate.UserFullName };
                comments.Add(comment);
            }
            CommentMentionNotif model = new CommentMentionNotif(faMessage, "", comments, _appSettings.CompanyName, enMessage);



            var emailRequest = new SendEmailDto
            {
                Tos = toUserEmails,
                Body = await _viewRenderService.RenderToStringAsync("_CommentCreateNotifEmail", model),
                Subject = $"Comment | {communicationModel.CommunicationCode}"
            };

            await _appEmailService.SendAsync(emailRequest, (attachments != null) ? attachments.Stream : new MemoryStream(), communicationModel.CommunicationCode + ".zip");

            return ServiceResultFactory.CreateSuccess(true);
        }
        public async Task<ServiceResult<bool>> SendEmailOnAddCommunicationForCustomerUserAsync(AuthenticateDto authenticate, DocumentCommunication communicationModel, DocumentRevision revisionModel)
        {


            var toUsers = await _authenticationServices
                .GetAllUserHasAccessDocumentForCustomerUserAsync(authenticate.ContractCode, revisionModel.Document.DocumentGroupId);

            var toUserEmails = toUsers
                .Where(a => !string.IsNullOrEmpty(a.Email))
                .GroupBy(a => a.Email)
                .Select(a => a.Key)
                .ToList();

            if (!toUserEmails.Any())
                return ServiceResultFactory.CreateSuccess(true);

            var emailAttachs = PreParationAchmentForSendInEmail(communicationModel, revisionModel);


            string message = $"در پروژه {authenticate.ContractCode} یک کامنت جدید به شماره {communicationModel.CommunicationCode} مربوط به مدرک {revisionModel.Document.DocTitle} به شماره {revisionModel.Document.DocNumber} توسط {authenticate.UserFullName} به شرح زیر ثبت گردید. ";
            List<CommentNotifViaEmailDTO> comments = new List<CommentNotifViaEmailDTO>();

            CommentNotifViaEmailDTO comment = null;
            foreach (var item in communicationModel.CommunicationQuestions)
            {
                comment = new CommentNotifViaEmailDTO { Message = item.Description, SendDate = item.CreatedDate.ToJalaliWithTime(), SenderName = authenticate.UserFullName };
                comments.Add(comment);
            }
            CommentMentionNotif model = new CommentMentionNotif(message, "", comments, _appSettings.CompanyName);



            var emailRequest = new SendEmailDto
            {
                Tos = toUserEmails,
                Body = await _viewRenderService.RenderToStringAsync("_CommentCreateNotifEmail", model),
                Subject = $"کامنت {communicationModel.CommunicationCode}"
            };

            await _appEmailService.SendAsync(emailRequest, emailAttachs, true);

            return ServiceResultFactory.CreateSuccess(true);
        }
        private List<InMemoryFileDto> PreParationAchmentForSendInEmail(DocumentCommunication communication, DocumentRevision revisionModel)
        {
            var result = new List<InMemoryFileDto>();

            foreach (var item in communication.CommunicationQuestions.First().Attachments)
            {
                result.Add(new InMemoryFileDto
                {
                    FileName = item.FileName,
                    FileSrc = item.FileSrc,
                    FileUrl = ServiceSetting.UploadFilePathRevisionCommunication(revisionModel.Document.ContractCode, revisionModel.DocumentId, revisionModel.DocumentRevisionId),
                });
            }

            return result;
        }



        private async Task<ServiceResult<List<CommunicationAttachment>>> AddCommunicationAttachmentAsync(AuthenticateDto authenticate, long docId, long revId,
            List<AddRevisionAttachmentDto> files)
        {
            var attachModels = new List<CommunicationAttachment>();
            foreach (var item in files)
            {
                var UploadedFile = await _fileHelper
                    .SaveDocumentFromTemp(item.FileSrc, ServiceSetting.UploadFilePathRevisionCommunication(authenticate.ContractCode, docId, revId));

                if (UploadedFile == null)
                    return ServiceResultFactory.CreateError<List<CommunicationAttachment>>(null, MessageId.UploudFailed);

                _fileHelper.DeleteDocumentFromTemp(item.FileSrc);

                attachModels.Add(new CommunicationAttachment
                {
                    FileSrc = item.FileSrc,
                    FileName = item.FileName,
                    FileType = UploadedFile.FileType,
                    FileSize = UploadedFile.FileSize
                });
            }

            return ServiceResultFactory.CreateSuccess(attachModels);
        }

        public async Task<ServiceResult<List<CommunicationListDto>>> GetCommunicationCommentListAsync(AuthenticateDto authenticate, COMQueryDto query)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<CommunicationListDto>>(null, MessageId.AccessDenied);

                var dbQuery = _documentCommunicationRepository
                    .AsNoTracking()
                    .Where(a => a.DocumentRevision.Document.ContractCode == authenticate.ContractCode);

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a =>
                     a.CommunicationCode.Contains(query.SearchText)
                     || a.DocumentRevision.Document.DocNumber.Contains(query.SearchText)
                     || a.DocumentRevision.Document.DocTitle.Contains(query.SearchText)
                     || a.Customer.Name.Contains(query.SearchText)
                      || a.Consultant.Name.Contains(query.SearchText));

                if (EnumHelper.ValidateItem(query.DocClass))
                    dbQuery = dbQuery.Where(a => a.DocumentRevision.Document.DocClass == query.DocClass);

                if (EnumHelper.ValidateItem(query.CommunicationStatus))
                    dbQuery = dbQuery.Where(a => a.CommunicationStatus == query.CommunicationStatus);

                if (EnumHelper.ValidateItem(query.CompanyIssue))
                {
                    if (query.CompanyIssue == CompanyIssue.Customer && query.CompanyIssueId > 0)
                        dbQuery = dbQuery.Where(a => a.CustomerId == query.CompanyIssueId);
                    if (query.CompanyIssue == CompanyIssue.Consultant && query.CompanyIssueId > 0)
                        dbQuery = dbQuery.Where(a => a.ConsultantId == query.CompanyIssueId);
                }

                if (query.DocumentGroupIds != null && query.DocumentGroupIds.Any())
                    dbQuery = dbQuery.Where(a => query.DocumentGroupIds.Contains(a.DocumentRevision.Document.DocumentGroupId));

                if (permission.DocumentGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.DocumentGroupIds.Contains(a.DocumentRevision.Document.DocumentGroupId));

                var columnsMap = new Dictionary<string, Expression<Func<DocumentCommunication, object>>>
                {
                    ["DocumentCommunicationId"] = v => v.DocumentCommunicationId
                };

                var totalCount = dbQuery.Count();
                var queryResult =await dbQuery.ToListAsync();
                queryResult=queryResult.OrderByDescending(a => a.CommunicationCode, new CompareFormNumbers()).ToList();
                queryResult = queryResult.ApplayPageing(query).ToList();
                var documentCommunicationIds = queryResult.Select(a => a.DocumentCommunicationId);
                var result = await dbQuery.Where(a=>documentCommunicationIds.Contains(a.DocumentCommunicationId)).Select(c => new CommunicationListDto
                {
                    DocumentRevisionCode = c.DocumentRevision.DocumentRevisionCode,
                    CommunicationId = c.DocumentCommunicationId,
                    CommunicationCode = c.CommunicationCode,
                    CommunicationStatus = c.CommunicationStatus,
                    CommunicationType = c.CommunicationType,
                    CompanyIssue = c.CompanyIssue,
                    CompanyIssueName = c.CompanyIssue == CompanyIssue.Customer ? c.Customer.Name : c.Consultant.Name,
                    DocNumber = c.DocumentRevision.Document.DocNumber,
                    DocTitle = c.DocumentRevision.Document.DocTitle,
                    DocumentGroupTitle = c.DocumentRevision.Document.DocumentGroup.Title,
                    DocumentRevisionId = c.DocumentRevisionId,
                    UserAudit = c.AdderUser != null ? new UserAuditLogDto
                    {
                        AdderUserName = c.AdderUser.FullName,
                        AdderUserImage = c.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + c.AdderUser.Image : "",
                        CreateDate = c.CreatedDate.ToUnixTimestamp(),
                    } : null,
                    CommentStatus = c.CommentStatus,
                    Replayer = (c.CommunicationQuestions.FirstOrDefault() != null && c.CommunicationQuestions.FirstOrDefault().CommunicationReply != null) ? c.CommunicationQuestions.OrderByDescending(c => c.CommunicationQuestionId).FirstOrDefault().CommunicationReply.AdderUser.FullName : "",
                    ReplayDate = (c.CommunicationQuestions.FirstOrDefault() != null && c.CommunicationQuestions.FirstOrDefault().CommunicationReply != null) ? c.CommunicationQuestions.OrderByDescending(c => c.CommunicationQuestionId).FirstOrDefault().CommunicationReply.CreatedDate.ToUnixTimestamp() : null,
                }).ToListAsync();
                result = result.OrderByDescending(a => a.CommunicationCode, new CompareFormNumbers()).ToList();
                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<CommunicationListDto>>(null, exception);
            }
        }
        public async Task<ServiceResult<List<CommunicationListDto>>> GetCommunicationCommentListForCustomerUserAsync(AuthenticateDto authenticate, COMQueryDto query, bool accessability)
        {
            try
            {
                if (!accessability)
                    return ServiceResultFactory.CreateError<List<CommunicationListDto>>(null, MessageId.AccessDenied);



                var dbQuery = _documentCommunicationRepository
                    .AsNoTracking()
                    .Where(a => a.DocumentRevision.Document.ContractCode == authenticate.ContractCode && (a.CompanyIssue == CompanyIssue.Consultant || a.CompanyIssue == CompanyIssue.Customer));

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a =>
                     a.CommunicationCode.Contains(query.SearchText)
                     || a.DocumentRevision.Document.DocNumber.Contains(query.SearchText)
                     || a.DocumentRevision.Document.DocTitle.Contains(query.SearchText)
                     || a.Customer.Name.Contains(query.SearchText));

                if (EnumHelper.ValidateItem(query.DocClass))
                    dbQuery = dbQuery.Where(a => a.DocumentRevision.Document.DocClass == query.DocClass);

                if (EnumHelper.ValidateItem(query.CommunicationStatus))
                    dbQuery = dbQuery.Where(a => a.CommunicationStatus == query.CommunicationStatus);

                if (EnumHelper.ValidateItem(query.CompanyIssue))
                {
                    if (query.CompanyIssue == CompanyIssue.Customer && query.CompanyIssueId > 0)
                        dbQuery = dbQuery.Where(a => a.CustomerId == query.CompanyIssueId);
                }

                if (query.DocumentGroupIds != null && query.DocumentGroupIds.Any())
                    dbQuery = dbQuery.Where(a => query.DocumentGroupIds.Contains(a.DocumentRevision.Document.DocumentGroupId));



                var columnsMap = new Dictionary<string, Expression<Func<DocumentCommunication, object>>>
                {
                    ["DocumentCommunicationId"] = v => v.DocumentCommunicationId
                };

                var totalCount = dbQuery.Count();
                var queryResult = await dbQuery.ToListAsync();
                queryResult = queryResult.OrderByDescending(a => a.CommunicationCode, new CompareFormNumbers()).ToList();
                queryResult = queryResult.ApplayPageing(query).ToList();
                var documentCommunicationIds = queryResult.Select(a => a.DocumentCommunicationId);
                var result = await dbQuery.Where(a => documentCommunicationIds.Contains(a.DocumentCommunicationId)).Select(c => new CommunicationListDto
                {
                    DocumentRevisionCode = c.DocumentRevision.DocumentRevisionCode,
                    CommunicationId = c.DocumentCommunicationId,
                    CommunicationCode = c.CommunicationCode,
                    CommunicationStatus = c.CommunicationStatus,
                    CommunicationType = c.CommunicationType,
                    CompanyIssue = c.CompanyIssue,
                    CompanyIssueName = c.CompanyIssue == CompanyIssue.Customer ? c.Customer.Name : c.Consultant.Name,
                    DocNumber = c.DocumentRevision.Document.DocNumber,
                    DocTitle = c.DocumentRevision.Document.DocTitle,
                    DocumentGroupTitle = c.DocumentRevision.Document.DocumentGroup.Title,
                    DocumentRevisionId = c.DocumentRevisionId,
                    UserAudit = c.AdderUser != null ? new UserAuditLogDto
                    {
                        AdderUserName = c.AdderUser.FullName,
                        AdderUserImage = c.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + c.AdderUser.Image : "",
                        CreateDate = c.CreatedDate.ToUnixTimestamp(),
                    } : null,
                    CommentStatus = c.CommentStatus,
                    Replayer = (c.CommunicationQuestions.FirstOrDefault() != null && c.CommunicationQuestions.FirstOrDefault().CommunicationReply != null) ? c.CommunicationQuestions.OrderByDescending(c => c.CommunicationQuestionId).FirstOrDefault().CommunicationReply.AdderUser.FullName : "",
                    ReplayDate = (c.CommunicationQuestions.FirstOrDefault() != null && c.CommunicationQuestions.FirstOrDefault().CommunicationReply != null) ? c.CommunicationQuestions.OrderByDescending(c => c.CommunicationQuestionId).FirstOrDefault().CommunicationReply.CreatedDate.ToUnixTimestamp() : null,
                }).ToListAsync();
                result = result.OrderByDescending(a => a.CommunicationCode, new CompareFormNumbers()).ToList();
                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<CommunicationListDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<CommnetQuestionDetailsDto>> GetCommentQuestionDetailsAsync(AuthenticateDto authenticate, long communicationId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<CommnetQuestionDetailsDto>(null, MessageId.AccessDenied);

                var dbQuery = _documentCommunicationRepository
                    .AsNoTracking()
                    .Where(a => a.DocumentCommunicationId == communicationId && a.DocumentRevision.Document.ContractCode == authenticate.ContractCode);

                if (permission.DocumentGroupIds.Any() && !dbQuery.Any(a => permission.DocumentGroupIds.Contains(a.DocumentRevision.Document.DocumentGroupId)))
                    return ServiceResultFactory.CreateError<CommnetQuestionDetailsDto>(null, MessageId.AccessDenied);


                var result = await dbQuery.Select(c => new CommnetQuestionDetailsDto
                {
                    CommentStatus = c.CommentStatus,
                    Questions = c.CommunicationQuestions.Select(v => new QuestionDto
                    {
                        QuestionId = v.CommunicationQuestionId,
                        Description = v.Description,
                        IsReplyed = v.IsReplyed,
                        UserAudit = v.AdderUser != null ? new UserAuditLogDto
                        {
                            AdderUserName = v.AdderUser.FullName,
                            AdderUserImage = "",
                            CreateDate = v.CreatedDate.ToUnixTimestamp(),
                        } : null,
                        Attachments = v.Attachments.Where(b => !b.IsDeleted)
                        .Select(n => new CommunicationAttachmentDto
                        {
                            AttachmentId = n.CommunicationAttachmentId,
                            FileName = n.FileName,
                            FileSize = n.FileSize,
                            FileSrc = n.FileSrc,
                            FileType = n.FileType
                        }).ToList(),
                        Reply = v.CommunicationReply != null ? new ReplyDto
                        {
                            Description = v.CommunicationReply.Description,
                            Attachments = v.CommunicationReply.Attachments.Where(b => !b.IsDeleted)
                            .Select(n => new CommunicationAttachmentDto
                            {
                                AttachmentId = n.CommunicationAttachmentId,
                                FileName = n.FileName,
                                FileSize = n.FileSize,
                                FileSrc = n.FileSrc,
                                FileType = n.FileType
                            }).ToList(),
                            UserAudit = v.CommunicationReply.AdderUser != null ? new UserAuditLogDto
                            {
                                AdderUserName = v.CommunicationReply.AdderUser.FullName,
                                AdderUserImage = "",
                                CreateDate = v.CommunicationReply.CreatedDate.ToUnixTimestamp(),
                            } : null,
                        } : new ReplyDto()
                    }).ToList(),
                }).FirstOrDefaultAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<CommnetQuestionDetailsDto>(null, exception);
            }
        }
        public async Task<ServiceResult<CommnetQuestionDetailsDto>> GetCommentQuestionDetailsForCustomerUserAsync(AuthenticateDto authenticate, long communicationId, bool accessability)
        {
            try
            {
                if (!accessability)
                    return ServiceResultFactory.CreateError<CommnetQuestionDetailsDto>(null, MessageId.AccessDenied);



                var dbQuery = _documentCommunicationRepository
                    .AsNoTracking()
                    .Where(a => a.DocumentCommunicationId == communicationId && a.DocumentRevision.Document.ContractCode == authenticate.ContractCode);




                var result = await dbQuery.Select(c => new CommnetQuestionDetailsDto
                {
                    CommentStatus = c.CommentStatus,
                    Questions = c.CommunicationQuestions.Select(v => new QuestionDto
                    {
                        QuestionId = v.CommunicationQuestionId,
                        Description = v.Description,
                        IsReplyed = v.IsReplyed,
                        UserAudit = v.AdderUser != null ? new UserAuditLogDto
                        {
                            AdderUserName = v.AdderUser.FullName,
                            AdderUserImage = "",
                            CreateDate = v.CreatedDate.ToUnixTimestamp(),
                        } : null,
                        Attachments = v.Attachments.Where(b => !b.IsDeleted)
                        .Select(n => new CommunicationAttachmentDto
                        {
                            AttachmentId = n.CommunicationAttachmentId,
                            FileName = n.FileName,
                            FileSize = n.FileSize,
                            FileSrc = n.FileSrc,
                            FileType = n.FileType
                        }).ToList(),
                        Reply = v.CommunicationReply != null ? new ReplyDto
                        {
                            Description = v.CommunicationReply.Description,
                            Attachments = v.CommunicationReply.Attachments.Where(b => !b.IsDeleted)
                            .Select(n => new CommunicationAttachmentDto
                            {
                                AttachmentId = n.CommunicationAttachmentId,
                                FileName = n.FileName,
                                FileSize = n.FileSize,
                                FileSrc = n.FileSrc,
                                FileType = n.FileType
                            }).ToList(),
                            UserAudit = v.CommunicationReply.AdderUser != null ? new UserAuditLogDto
                            {
                                AdderUserName = v.CommunicationReply.AdderUser.FullName,
                                AdderUserImage = "",
                                CreateDate = v.CommunicationReply.CreatedDate.ToUnixTimestamp(),
                            } : null,
                        } : new ReplyDto()
                    }).ToList(),
                }).FirstOrDefaultAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<CommnetQuestionDetailsDto>(null, exception);
            }
        }

        public async Task<ServiceResult<CommentDetailsDto>> GetCommentDetailsAsync(AuthenticateDto authenticate, long communicationId)
        {
            try
            {
                //var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                //if (!permission.HasPermission)
                //    return ServiceResultFactory.CreateError<CurrentContractInfoDto>(null, MessageId.AccessDenied);

                var dbQuery = _documentCommunicationRepository
                    .AsNoTracking()
                    .Where(a => a.DocumentCommunicationId == communicationId && a.DocumentRevision.Document.ContractCode == authenticate.ContractCode);


                var result = await dbQuery.Select(c => new CommentDetailsDto
                {
                    CommunicationId = c.DocumentCommunicationId,
                    CommunicationCode = c.CommunicationCode,
                    CommunicationStatus = c.CommunicationStatus,
                    CommunicationType = c.CommunicationType,
                    CompanyIssue = c.CompanyIssue,
                    CompanyIssueName = c.CompanyIssue == CompanyIssue.Customer ? c.Customer.Name : c.Consultant.Name,
                    DocumentRevisionCode = c.DocumentRevision.DocumentRevisionCode,
                    DocNumber = c.DocumentRevision.Document.DocNumber,
                    DocTitle = c.DocumentRevision.Document.DocTitle,
                    DocumentGroupTitle = c.DocumentRevision.Document.DocumentGroup.Title,
                    DocumentRevisionId = c.DocumentRevisionId,
                    UserAudit = c.AdderUser != null ? new UserAuditLogDto
                    {
                        AdderUserName = c.AdderUser.FullName,
                        AdderUserImage = c.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + c.AdderUser.Image : "",
                        CreateDate = c.CreatedDate.ToUnixTimestamp(),
                    } : null,
                    CommentStatus = c.CommentStatus,
                    Attachments = c.Attachments.Where(b => !b.IsDeleted)
                        .Select(n => new CommunicationAttachmentDto
                        {
                            AttachmentId = n.CommunicationAttachmentId,
                            FileName = n.FileName,
                            FileSize = n.FileSize,
                            FileSrc = n.FileSrc,
                            FileType = n.FileType
                        }).ToList(),
                    Questions = c.CommunicationQuestions.Select(v => new QuestionDto
                    {
                        QuestionId = v.CommunicationQuestionId,
                        Description = v.Description,
                        IsReplyed = v.IsReplyed,
                        UserAudit = v.AdderUser != null ? new UserAuditLogDto
                        {
                            AdderUserName = v.AdderUser.FullName,
                            AdderUserImage = "",
                            CreateDate = v.CreatedDate.ToUnixTimestamp(),
                        } : null,
                        Attachments = v.Attachments.Where(b => !b.IsDeleted)
                        .Select(n => new CommunicationAttachmentDto
                        {
                            AttachmentId = n.CommunicationAttachmentId,
                            FileName = n.FileName,
                            FileSize = n.FileSize,
                            FileSrc = n.FileSrc,
                            FileType = n.FileType
                        }).ToList(),
                        Reply = v.CommunicationReply != null ? new ReplyDto
                        {
                            Description = v.CommunicationReply.Description,
                            UserAudit = v.CommunicationReply.AdderUser != null ? new UserAuditLogDto
                            {
                                AdderUserName = v.CommunicationReply.AdderUser.FullName,
                                AdderUserImage = "",
                                CreateDate = v.CommunicationReply.CreatedDate.ToUnixTimestamp(),
                            } : null,
                            Attachments = v.CommunicationReply.Attachments.Where(b => !b.IsDeleted)
                            .Select(n => new CommunicationAttachmentDto
                            {
                                AttachmentId = n.CommunicationAttachmentId,
                                FileName = n.FileName,
                                FileSize = n.FileSize,
                                FileSrc = n.FileSrc,
                                FileType = n.FileType
                            }).ToList(),
                        } : new ReplyDto()
                    }).ToList(),
                }).FirstOrDefaultAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<CommentDetailsDto>(null, exception);
            }
        }

        public async Task<ServiceResult<List<CommunicationAttachmentDto>>> AddCommunicationCommentAttachmentAsync(AuthenticateDto authenticate,
            long communicationId, IFormFileCollection files)
        {
            try
            {
                if (files == null || !files.Any())
                    return ServiceResultFactory.CreateError<List<CommunicationAttachmentDto>>(null, MessageId.InputDataValidationError);

                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<CommunicationAttachmentDto>>(null, MessageId.AccessDenied);

                var dbQuery = _documentCommunicationRepository
                    .AsNoTracking()
                     .Where(a => a.DocumentCommunicationId == communicationId &&
                     a.DocumentRevision.Document.ContractCode == authenticate.ContractCode);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<List<CommunicationAttachmentDto>>(null, MessageId.EntityDoesNotExist);

                if (permission.DocumentGroupIds.Any() && !dbQuery.Any(a => permission.DocumentGroupIds.Contains(a.DocumentRevision.Document.DocumentGroupId)))
                    return ServiceResultFactory.CreateError<List<CommunicationAttachmentDto>>(null, MessageId.AccessDenied);

                var communicationModel = await dbQuery.Select(c => new
                {
                    communicationId = c.DocumentCommunicationId,
                    revId = c.DocumentRevisionId,
                    docId = c.DocumentRevision.DocumentId
                }).FirstOrDefaultAsync();

                if (communicationModel == null)
                    return ServiceResultFactory.CreateError<List<CommunicationAttachmentDto>>(null, MessageId.EntityDoesNotExist);

                var attachModels = new List<CommunicationAttachment>();
                foreach (var item in files)
                {
                    var fileName = item.FileName;
                    var uploadResult = await _fileService.UploadDocumentFile(item);
                    if (!uploadResult.Succeeded)
                        return ServiceResultFactory.CreateError<List<CommunicationAttachmentDto>>(null, uploadResult.Messages[0].Message);

                    var UploadedFile = await _fileHelper
                        .SaveDocumentFromTemp(uploadResult.Result, ServiceSetting.UploadFilePathRevisionCommunication(authenticate.ContractCode, communicationModel.docId, communicationModel.revId));

                    if (UploadedFile == null)
                        return ServiceResultFactory.CreateError<List<CommunicationAttachmentDto>>(null, MessageId.UploudFailed);

                    _fileHelper.DeleteDocumentFromTemp(uploadResult.Result);

                    attachModels.Add(new CommunicationAttachment
                    {
                        DocumentCommunicationId = communicationId,
                        FileSrc = uploadResult.Result,
                        FileName = fileName,
                        FileType = UploadedFile.FileType,
                        FileSize = UploadedFile.FileSize
                    });
                }

                _communicationAttachmentRepository.AddRange(attachModels);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var res = attachModels.Select(c => new CommunicationAttachmentDto
                    {
                        FileName = c.FileName,
                        FileSize = c.FileSize,
                        FileSrc = c.FileSrc,
                        FileType = c.FileType,
                        AttachmentId = c.CommunicationAttachmentId
                    }).ToList();
                    return ServiceResultFactory.CreateSuccess(res);
                }
                return ServiceResultFactory.CreateError<List<CommunicationAttachmentDto>>(null, MessageId.UploudFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<CommunicationAttachmentDto>>(null, exception);

            }
        }

        public async Task<ServiceResult<bool>> DeleteCommunicationCommentAttachmentAsync(AuthenticateDto authenticate, long communicationId, string fileSrc)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery = _documentCommunicationRepository
                 .AsNoTracking()
                  .Where(a => a.DocumentCommunicationId == communicationId &&
                  a.DocumentRevision.Document.ContractCode == authenticate.ContractCode);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (permission.DocumentGroupIds.Any() && !dbQuery.Any(a => permission.DocumentGroupIds.Contains(a.DocumentRevision.Document.DocumentGroupId)))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var communicationModel = await dbQuery.Select(c => new
                {
                    communicationId = c.DocumentCommunicationId,
                    revId = c.DocumentRevisionId,
                    docId = c.DocumentRevision.DocumentId
                }).FirstOrDefaultAsync();

                var dbQueryAttch = _communicationAttachmentRepository
                     .Where(a => !a.IsDeleted &&
                     a.DocumentCommunicationId == communicationId &&
                     a.FileSrc == fileSrc);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                var attachModel = await dbQueryAttch.FirstOrDefaultAsync();
                if (attachModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                attachModel.IsDeleted = true;

                string path = ServiceSetting.UploadFilePathRevisionCommunication(authenticate.ContractCode, communicationModel.docId, communicationModel.revId) + fileSrc;
                await _unitOfWork.SaveChangesAsync();
                _fileHelper.DeleteDocumentFromPath(path);

                return ServiceResultFactory.CreateSuccess(true);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);

            }
        }
        public async Task<DownloadFileDto> GenerateCommentPdfAsync(AuthenticateDto authenticate, long communicationId)
        {
            try
            {
                //var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                //if (!permission.HasPermission)
                //    return null;


                var dbQuery = _documentCommunicationRepository
                    .Where(a => a.DocumentCommunicationId == communicationId &&
                    a.DocumentRevision.Document.ContractCode == authenticate.ContractCode);

                //if (permission.DocumentGroupIds.Any() && !dbQuery.Any(c => permission.DocumentGroupIds.Contains(c.DocumentTQNCR.DocumentRevision.Document.DocumentGroupId)))
                //    return null;

                var commentModel = await dbQuery.Select(a => new
                {
                    CommentCode = a.CommunicationCode,
                    DocumentRevisionId = a.DocumentRevisionId,
                    DocNumber = a.DocumentRevision.Document.DocNumber,
                    DocTitle = a.DocumentRevision.Document.DocTitle,
                    DocClientNumber = a.DocumentRevision.Document.ClientDocNumber,
                    RevisionCode = a.DocumentRevision.DocumentRevisionCode,
                    ProjectDescription = a.DocumentRevision.Document.Contract.Description,
                    CommentStatus = a.CommentStatus,
                    CommunicationStatus = a.CommunicationStatus,
                    StartDate = a.CreatedDate,
                    EndDate = a.UpdateDate,
                    CustomerLogo = a.DocumentRevision.Document.Contract.Customer.Logo,
                    customerId = a.DocumentRevision.Document.Contract.Customer.Id,
                    QuestionReplys = a.CommunicationQuestions.Select(c => new
                    {
                        questionDescription = c.Description,
                        replyDescription = c.CommunicationReply != null ? c.CommunicationReply.Description : "",
                    }).ToList()
                }).FirstOrDefaultAsync();

                if (commentModel == null)
                    return null;

                var selectedTemplate = new PDFTemplate();
                var templates = _pdfTemplateRepository
                    .Where(a => (a.ContractCode == null || a.ContractCode == authenticate.ContractCode) &&
                     a.PDFTemplateType == PDFTemplateType.Communication)
                    .ToList();

                if (!templates.Any())
                    return null;

                if (templates.Any(c => c.ContractCode == authenticate.ContractCode))
                    selectedTemplate = templates.FirstOrDefault(c => c.ContractCode == authenticate.ContractCode);
                else if (templates.Any(c => c.ContractCode == null))
                    selectedTemplate = templates.FirstOrDefault(c => c.ContractCode == null);
                else
                    selectedTemplate = templates
                        .OrderByDescending(a => a.Id)
                        .FirstOrDefault();

                var commentDetails = new CommentDetailsForCreatePdfDto
                {
                    CommentCode = commentModel.CommentCode,
                    DocNumber = commentModel.DocNumber,
                    DocTitle = commentModel.DocTitle,
                    DocClientNumber = commentModel.DocClientNumber,
                    RevisionCode = commentModel.RevisionCode,
                    ProjectDescription = commentModel.ProjectDescription,
                    CommentStatus = commentModel.CommentStatus,
                    StartDate = commentModel.StartDate != null ? commentModel.StartDate.ToPersianDateString() : "",
                    EndDate = commentModel.CommunicationStatus == DocumentCommunicationStatus.Replyed && commentModel.EndDate != null ? commentModel.EndDate.ToPersianDateString() : "",
                    CustomerLogo = commentModel.CustomerLogo,
                };

                if (commentModel.QuestionReplys != null && commentModel.QuestionReplys.Any())
                    commentDetails.QuestionReplys = commentModel.QuestionReplys.Select(c => new CommentQuestionReplyDto
                    {
                        QuestionDescription = c.questionDescription,
                        ReplyDescription = c.replyDescription
                    }).ToList();

                var lastCustomerTransmittalInfo = await _transmittalRepository
                    .AsNoTracking()
                    .Where(a => (a.TransmittalType == TransmittalType.Customer || a.TransmittalType == TransmittalType.Consultant) &&
                    a.ContractCode == authenticate.ContractCode &&
                    a.TransmittalRevisions.Any(c => c.DocumentRevisionId == commentModel.DocumentRevisionId))
                     .OrderByDescending(a => a.CreatedDate)
                     .Select(v => new
                     {
                         TransmittalNumber = v.TransmittalNumber,
                         TransmittalDate = v.CreatedDate,
                     }).FirstOrDefaultAsync();

                if (lastCustomerTransmittalInfo != null)
                {
                    commentDetails.TransmittalNumber = lastCustomerTransmittalInfo.TransmittalNumber;
                    commentDetails.TransmittalDate = lastCustomerTransmittalInfo.TransmittalDate != null ?
                        lastCustomerTransmittalInfo.TransmittalDate.ToPersianDateString() : "";
                }

                commentDetails.CompanyLogo = _fileHelper.FileReadSrc(_appSettings.CompanyLogo);

                commentDetails.CustomerLogo = _fileHelper.FileReadSrc(commentModel.CustomerLogo, ServiceSetting.UploadImagesPath.LogoSmall);

                // instantiate a html to pdf converter object
                HtmlToPdf converter = new HtmlToPdf();

                converter.Options.MarginTop = 15;
                converter.Options.MarginBottom = 15;
                converter.Options.MarginLeft = 15;
                converter.Options.MarginRight = 15;

                // set converter options
                converter.Options.PdfPageSize = PdfPageSize.A4;
                converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;

                // create a new pdf document converting an url
                var stringHeader = CommentPdfHeaderTemplate(commentDetails, selectedTemplate);

                converter.Header.Height = 170;
                converter.Header.DisplayOnFirstPage = true;
                converter.Header.DisplayOnEvenPages = true;
                converter.Header.DisplayOnOddPages = true;
                converter.Options.DisplayHeader = true;
                PdfHtmlSection headerHtml = new PdfHtmlSection(stringHeader, "");
                headerHtml.AutoFitHeight = HtmlToPdfPageFitMode.AutoFit;
                converter.Header.Add(headerHtml);

                var stringFooter = CommentPdfFooterTemplate(selectedTemplate);
                converter.Footer.Height = 50;
                converter.Footer.DisplayOnFirstPage = true;
                converter.Footer.DisplayOnEvenPages = true;
                converter.Footer.DisplayOnOddPages = true;
                converter.Options.DisplayFooter = true;
                PdfHtmlSection footerHtml = new PdfHtmlSection(stringFooter, "");
                headerHtml.AutoFitHeight = HtmlToPdfPageFitMode.AutoFit;
                converter.Footer.Add(footerHtml);

                var stringBody = CommentPdfBodyTemplate(commentDetails, selectedTemplate);
                PdfDocument doc = converter.ConvertHtmlString(stringBody);

                var gggg = doc.Save();

                var result = new DownloadFileDto
                {
                    ArchiveFile = gggg,
                    ContentType = "application/pdf",
                    FileName = commentModel.DocNumber + "-" + commentModel.CommentCode + ".pdf",
                };
                return result;

            }
            catch (Exception exception)
            {
                throw;
            }
        }
        public async Task<DownloadFileDto> DownloadCommentFileAsync(AuthenticateDto authenticate, long communicationId, string fileSrc)
        {
            try
            {
                //var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                //if (!permission.HasPermission)
                //    return null;

                var info = await _documentCommunicationRepository
                    .AsNoTracking()
                    .Where(a =>
                    a.DocumentCommunicationId == communicationId &&
                    a.DocumentRevision.Document.ContractCode == authenticate.ContractCode)
                    .Select(c => new
                    {
                        revisionId = c.DocumentRevisionId,
                        docId = c.DocumentRevision.DocumentId,
                        docGroupId = c.DocumentRevision.Document.DocumentGroupId
                    }).FirstOrDefaultAsync();

                if (info == null)
                    return null;

                //if (permission.DocumentGroupIds.Any() && !permission.DocumentGroupIds.Contains(info.docGroupId))
                //    return null;

                var dbQuery = _communicationAttachmentRepository
                    .AsNoTracking()
                    .Where(a =>
                    !a.IsDeleted &&
                    a.FileSrc == fileSrc);

                if (!await dbQuery.AnyAsync())
                    return null;
                var attachment = await dbQuery.FirstOrDefaultAsync();
                return await _fileHelper.DownloadDocument(fileSrc, ServiceSetting.UploadFilePathRevisionCommunication(authenticate.ContractCode, info.docId, info.revisionId), attachment.FileName);
            }
            catch (Exception exception)
            {
                return null;
            }
        }

        public async Task<ServiceResult<CommnetQuestionDetailsDto>> AddReplyCommentAsync(AuthenticateDto authenticate, long communicationId, ReplyCommunicationCommentDto model)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<CommnetQuestionDetailsDto>(null, MessageId.AccessDenied);

                if (model == null || model.Replys == null || !model.Replys.Any())
                    return ServiceResultFactory.CreateError<CommnetQuestionDetailsDto>(null, MessageId.InputDataValidationError);

                if (model.Replys.Any(a => string.IsNullOrEmpty(a.Description)))
                    return ServiceResultFactory.CreateError<CommnetQuestionDetailsDto>(null, MessageId.InputDataValidationError);

                var dbQuery = _documentCommunicationRepository
                    .Where(a => a.DocumentCommunicationId == communicationId &&
                    a.CommunicationStatus == DocumentCommunicationStatus.PendingReply &&
                    a.DocumentRevision.Document.ContractCode == authenticate.ContractCode);

                if (permission.DocumentGroupIds.Any() && !dbQuery.Any(c => permission.DocumentGroupIds.Contains(c.DocumentRevision.Document.DocumentGroupId)))
                    return ServiceResultFactory.CreateError<CommnetQuestionDetailsDto>(null, MessageId.AccessDenied);

                var communicationModel = await dbQuery
                    .Include(a => a.AdderUser)
                    .Include(a => a.DocumentRevision)
                    .ThenInclude(a => a.Document)
                    .Include(a => a.CommunicationQuestions)
                    .ThenInclude(c => c.CommunicationReply)
                    .FirstOrDefaultAsync();

                var questionIds = model.Replys.Select(a => a.QuestionId).ToList();

                var questionModels = communicationModel.CommunicationQuestions.Where(a => !a.IsReplyed).ToList();

                if (questionModels.Count() != questionIds.Count() || questionModels.Any(c => !questionIds.Contains(c.CommunicationQuestionId)))
                    return ServiceResultFactory.CreateError<CommnetQuestionDetailsDto>(null, MessageId.ReplyCountCantLessThenQuestion);

                foreach (var item in questionModels)
                {
                    var currentReply = model.Replys.FirstOrDefault(a => a.QuestionId == item.CommunicationQuestionId);
                    if (currentReply == null)
                        return ServiceResultFactory.CreateError<CommnetQuestionDetailsDto>(null, MessageId.EntityDoesNotExist);

                    item.IsReplyed = true;
                    item.CommunicationReply = new CommunicationReply
                    {
                        Description = currentReply.Description,
                        Attachments = new List<CommunicationAttachment>()
                    };
                }

                if (model.Attachments != null && model.Attachments.Any())
                {
                    var res = await AddCommunicationAttachmentAsync(authenticate, communicationModel.DocumentRevision.DocumentId, communicationModel.DocumentRevisionId, model.Attachments);
                    if (!res.Succeeded)
                        return ServiceResultFactory.CreateError<CommnetQuestionDetailsDto>(null, res.Messages[0].Message);
                    else
                    {
                        questionModels.First().CommunicationReply.Attachments = res.Result;
                    }
                }

                communicationModel.CommunicationStatus = DocumentCommunicationStatus.Replyed;

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    await _scmLogAndNotificationService.SetDonedNotificationAsync(authenticate.UserId, authenticate.ContractCode, communicationModel.DocumentCommunicationId.ToString(), NotifEvent.ReplyComComment);
                    var res1 = await _scmLogAndNotificationService.AddDocumentAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = authenticate.ContractCode,
                        FormCode = communicationModel.CommunicationCode,
                        Message = "",
                        KeyValue = communicationModel.DocumentCommunicationId.ToString(),
                        NotifEvent = NotifEvent.ReplyComComment,
                        RootKeyValue = communicationModel.DocumentCommunicationId.ToString(),
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,
                        DocumentGroupId = communicationModel.DocumentRevision.Document.DocumentGroupId,

                    },
                    communicationModel.DocumentRevision.Document.DocumentGroupId,
                    null);
                    try
                    {
                        BackgroundJob.Enqueue(() => SendEmailOnReplyCommunicationAsync(authenticate, communicationModel));
                        BackgroundJob.Enqueue(() => SendEmailOnReplyCommunicationToNotOrginizeUserAsync(authenticate, communicationModel));


                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.StackTrace);
                    }


                    //if (communicationModel.AdderUser != null)
                    //    await SendEmailForAdderUserAsync(authenticate, communicationModel);
                    var res = await ReturnCommentQuestionDetailsAsync(communicationId);
                    return ServiceResultFactory.CreateSuccess(res);
                }
                return ServiceResultFactory.CreateError<CommnetQuestionDetailsDto>(null, MessageId.SaveFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<CommnetQuestionDetailsDto>(null, exception);
            }
        }

        public async Task<ServiceResult<bool>> SendEmailOnReplyCommunicationAsync(AuthenticateDto authenticate, DocumentCommunication communicationModel)
        {

            var toRoles = new List<string> {
                SCMRole.ComCommentReg,
                SCMRole.ComCommentMng,
                SCMRole.ComCommentObs,
                SCMRole.ComCommentReply
                };

            var toUsers = await _authenticationServices
                .GetAllUserHasAccessDocumentAsync(authenticate.ContractCode, toRoles, communicationModel.DocumentRevision.Document.DocumentGroupId);
            var commentEmailNotify = await _notifyRepository.Where(a => a.TeamWork.ContractCode == authenticate.ContractCode && a.NotifyType == NotifyManagementType.Email && a.NotifyNumber == (int)EmailNotify.Comment && a.IsActive).Select(a => a.UserId).ToListAsync();
            if (commentEmailNotify != null && commentEmailNotify.Any())
                toUsers = toUsers.Where(a => commentEmailNotify.Contains(a.Id)).ToList();
            else
                toUsers = new List<UserMentionDto>();
            var toUserEmails = (toUsers != null && toUsers.Any()) ? toUsers
                .Where(a => !string.IsNullOrEmpty(a.Email))
                .GroupBy(a => a.Email)
                .Select(a => a.Key)
                .ToList() : new List<string>();

            if (!toUserEmails.Any())
                return ServiceResultFactory.CreateSuccess(true);

            var attachments = await GenerateCommentAttachmentZipAsync(authenticate, communicationModel.DocumentCommunicationId);
            string faMessage = $"در پروژه <span style='direction:ltr'>{authenticate.ContractCode}</span> پاسخ به کامنت جدید به شماره <span style='direction:ltr'>{communicationModel.CommunicationCode}</span> مربوط به مدرک <span style='direction:ltr'>{communicationModel.DocumentRevision.Document.DocTitle}</span> به شماره <span style='direction:ltr'>{communicationModel.DocumentRevision.Document.DocNumber}</span> توسط {authenticate.UserFullName} به شرح زیر ثبت گردید. ";
            string enMessage = $"<div style='direction:ltr;text-align:left'>Replying to Comment No. {communicationModel.CommunicationCode} related to document titled <span style='direction:ltr'>{communicationModel.DocumentRevision.Document.DocTitle} </span>  has been registered  by {authenticate.UserFullName} as following.</div>";
            List<CommentNotifViaEmailDTO> comments = new List<CommentNotifViaEmailDTO>();

            CommentNotifViaEmailDTO comment = null;



            foreach (var item in communicationModel.CommunicationQuestions)
            {
                comment = new CommentNotifViaEmailDTO { Message = item.Description, Discription = item.CommunicationReply.Description, SendDate = item.CreatedDate.ToJalaliWithTime(), SenderName = authenticate.UserFullName };
                comments.Add(comment);
            }
            CommentMentionNotif model = new CommentMentionNotif(faMessage, "", comments, _appSettings.CompanyName, enMessage);


            var emailRequest = new SendEmailDto
            {
                Tos = toUserEmails,
                Body = await _viewRenderService.RenderToStringAsync("_CommentCreateNotifEmail", model),
                Subject = $"Reply to Comment | {communicationModel.CommunicationCode}"
            };
            await _appEmailService.SendAsync(emailRequest, (attachments != null) ? attachments.Stream : new MemoryStream(), communicationModel.CommunicationCode + ".zip");

            return ServiceResultFactory.CreateSuccess(true);
        }
        public async Task<ServiceResult<bool>> SendEmailOnReplyCommunicationToNotOrginizeUserAsync(AuthenticateDto authenticate, DocumentCommunication communicationModel)
        {
            List<string> users = new List<string>();


            users.Add(communicationModel.AdderUser.Email);

            if (users != null && users.Any())
            {
                var attachments = await GenerateCommentAttachmentZipAsync(authenticate, communicationModel.DocumentCommunicationId);
                string faMessage = $"در پروژه <span style='direction:ltr'>{authenticate.ContractCode}</span> پاسخ به کامنت جدید به شماره <span style='direction:ltr'>{communicationModel.CommunicationCode}</sapn> مربوط به مدرک <span style='direction:ltr'>{communicationModel.DocumentRevision.Document.DocTitle}</span> به شماره <span style='direction:ltr'>{communicationModel.DocumentRevision.Document.DocNumber}</span> توسط {authenticate.UserFullName} به شرح زیر ثبت گردید. ";
                string enMessage = $"<div style='direction:ltr;text-align:left'>Replying to Comment No. {communicationModel.CommunicationCode} related to document titled <span style='direction:ltr'>{communicationModel.DocumentRevision.Document.DocTitle} </span>  has been registered  by {authenticate.UserFullName} as following.</div>";
                List<CommentNotifViaEmailDTO> comments = new List<CommentNotifViaEmailDTO>();

                CommentNotifViaEmailDTO comment = null;



                foreach (var item in communicationModel.CommunicationQuestions)
                {
                    comment = new CommentNotifViaEmailDTO { Message = item.Description, Discription = item.CommunicationReply.Description, SendDate = item.CreatedDate.ToJalaliWithTime(), SenderName = authenticate.UserFullName };
                    comments.Add(comment);
                }
                CommentMentionNotif model = new CommentMentionNotif(faMessage, "", comments, _appSettings.CompanyName, enMessage);


                var emailRequest = new SendEmailDto
                {
                    Tos = users,
                    Body = await _viewRenderService.RenderToStringAsync("_CommentCreateNotifEmail", model),
                    Subject = $"Reply to Comment | {communicationModel.CommunicationCode}"
                };
                await _appEmailService.SendAsync(emailRequest, (attachments != null) ? attachments.Stream : new MemoryStream(), communicationModel.CommunicationCode + ".zip");
            }




            return ServiceResultFactory.CreateSuccess(true);
        }

        private List<InMemoryFileDto> PreParationAchmentForSendInEmail(DocumentCommunication communication)
        {
            var result = new List<InMemoryFileDto>();

            foreach (var item in communication.CommunicationQuestions.First().CommunicationReply.Attachments)
            {
                result.Add(new InMemoryFileDto
                {
                    FileName = item.FileName,
                    FileSrc = item.FileSrc,
                    FileUrl = ServiceSetting.UploadFilePathRevisionCommunication(communication.DocumentRevision.Document.ContractCode, communication.DocumentRevision.DocumentId, communication.DocumentRevision.DocumentRevisionId),
                });
            }

            return result;
        }


        private async Task<bool> SendEmailForAdderUserAsync(AuthenticateDto authenticate, DocumentCommunication communicationModel)
        {
            string url = $"/dashboard/documents/communication?action=Comment";

            url = _appSettings.ClientHost + url;

            var dat = DateTime.UtcNow.ToPersianDate();
            var emalRequest = new EmailRequest
            {
                Attachment = null,
                Subject = "اطلاعیه",
                Body = EmailTemplate.ReplyCommunicationEmailTemplate(communicationModel.CommunicationCode, authenticate.UserFullName, dat, url)
            };

            emalRequest.To = new List<string> { communicationModel.AdderUser.Email };
            await _appEmailService.SendAsync(emalRequest);
            return true;

        }

        private async Task<CommnetQuestionDetailsDto> ReturnCommentQuestionDetailsAsync(long communicationId)
        {
            try
            {
                var dbQuery = _documentCommunicationRepository
                    .AsNoTracking()
                    .Where(a => a.DocumentCommunicationId == communicationId);


                var result = await dbQuery.Select(c => new CommnetQuestionDetailsDto
                {
                    CommentStatus = c.CommentStatus,
                    Questions = c.CommunicationQuestions.Select(v => new QuestionDto
                    {
                        QuestionId = v.CommunicationQuestionId,
                        Description = v.Description,
                        IsReplyed = v.IsReplyed,
                        UserAudit = v.AdderUser != null ? new UserAuditLogDto
                        {
                            AdderUserName = v.AdderUser.FullName,
                            AdderUserImage = "",
                            CreateDate = v.CreatedDate.ToUnixTimestamp(),
                        } : null,
                        Attachments = v.Attachments.Where(b => !b.IsDeleted)
                        .Select(n => new CommunicationAttachmentDto
                        {
                            AttachmentId = n.CommunicationAttachmentId,
                            FileName = n.FileName,
                            FileSize = n.FileSize,
                            FileSrc = n.FileSrc,
                            FileType = n.FileType
                        }).ToList(),
                        Reply = v.CommunicationReply != null ? new ReplyDto
                        {
                            Description = v.CommunicationReply.Description,
                            UserAudit = v.CommunicationReply.AdderUser != null ? new UserAuditLogDto
                            {
                                AdderUserName = v.CommunicationReply.AdderUser.FullName,
                                AdderUserImage = "",
                                CreateDate = v.CommunicationReply.CreatedDate.ToUnixTimestamp(),
                            } : null,
                        } : new ReplyDto()
                    }).ToList(),
                }).FirstOrDefaultAsync();

                return result;
            }
            catch (Exception exception)
            {
                return new CommnetQuestionDetailsDto();
            }
        }

        public async Task<DownloadFileDto> GenerateCommentAttachmentZipAsync(AuthenticateDto authenticate, long communicationId)
        {
            try
            {
                //var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                //if (!permission.HasPermission)
                //    return null;


                var dbQuery = _documentCommunicationRepository

                    .Where(a => a.DocumentCommunicationId == communicationId &&
                    a.DocumentRevision.Document.ContractCode == authenticate.ContractCode);

                //if (permission.DocumentGroupIds.Any() && !dbQuery.Any(c => permission.DocumentGroupIds.Contains(c.DocumentTQNCR.DocumentRevision.Document.DocumentGroupId)))
                //    return null;

                var commentModel = await dbQuery.Select(a => new
                {
                    CommentCode = a.CommunicationCode,
                    DocumentRevisionId = a.DocumentRevisionId,
                    DocNumber = a.DocumentRevision.Document.DocNumber,
                    DocumentId = a.DocumentRevision.DocumentId,
                    DocTitle = a.DocumentRevision.Document.DocTitle,
                    DocClientNumber = a.DocumentRevision.Document.ClientDocNumber,
                    RevisionCode = a.DocumentRevision.DocumentRevisionCode,
                    ProjectDescription = a.DocumentRevision.Document.Contract.Description,
                    CommentStatus = a.CommentStatus,
                    CommunicationStatus = a.CommunicationStatus,
                    StartDate = a.CreatedDate,
                    EndDate = a.UpdateDate,
                    CustomerLogo = a.DocumentRevision.Document.Contract.Customer.Logo,
                    customerId = a.DocumentRevision.Document.Contract.Customer.Id,
                    QuestionReplys = a.CommunicationQuestions.Select(c => new
                    {
                        questionDescription = c.Description,
                        replyDescription = c.CommunicationReply != null ? c.CommunicationReply.Description : "",
                    }).ToList()
                }).FirstOrDefaultAsync();

                if (commentModel == null)
                    return null;
                var questions = await dbQuery.Include(a => a.CommunicationQuestions).ThenInclude(a => a.Attachments).Include(a => a.CommunicationQuestions).ThenInclude(a => a.CommunicationReply).ThenInclude(a => a.Attachments).ToListAsync();
                var attachments = questions.Select(a => new CommentTQNCRAttachmentDto
                {
                    CommentAttachment = a.CommunicationQuestions.First().Attachments.Where(b => !b.IsDeleted).Select(b => new BasicAttachmentDownloadDto
                    {
                        FileName = b.FileName,
                        FileSrc = b.FileSrc
                    }).ToList(),
                    ReplyAttachment = (a.CommunicationQuestions.First().CommunicationReply != null) ? a.CommunicationQuestions.First().CommunicationReply.Attachments.Where(b => !b.IsDeleted).Select(b => new BasicAttachmentDownloadDto
                    {
                        FileName = b.FileName,
                        FileSrc = b.FileSrc
                    }).ToList() : new List<BasicAttachmentDownloadDto>()
                }).FirstOrDefault();
                var selectedTemplate = new PDFTemplate();
                var templates = _pdfTemplateRepository
                    .Where(a => (a.ContractCode == null || a.ContractCode == authenticate.ContractCode) &&
                     a.PDFTemplateType == PDFTemplateType.Communication)
                    .ToList();

                if (!templates.Any())
                    return null;

                if (templates.Any(c => c.ContractCode == authenticate.ContractCode))
                    selectedTemplate = templates.FirstOrDefault(c => c.ContractCode == authenticate.ContractCode);
                else if (templates.Any(c => c.ContractCode == null))
                    selectedTemplate = templates.FirstOrDefault(c => c.ContractCode == null);
                else
                    selectedTemplate = templates
                        .OrderByDescending(a => a.Id)
                        .FirstOrDefault();

                var commentDetails = new CommentDetailsForCreatePdfDto
                {
                    CommentCode = commentModel.CommentCode,
                    DocNumber = commentModel.DocNumber,
                    DocTitle = commentModel.DocTitle,
                    DocClientNumber = commentModel.DocClientNumber,
                    RevisionCode = commentModel.RevisionCode,
                    ProjectDescription = commentModel.ProjectDescription,
                    CommentStatus = commentModel.CommentStatus,
                    StartDate = commentModel.StartDate != null ? commentModel.StartDate.ToPersianDateString() : "",
                    EndDate = commentModel.CommunicationStatus == DocumentCommunicationStatus.Replyed && commentModel.EndDate != null ? commentModel.EndDate.ToPersianDateString() : "",
                    CustomerLogo = commentModel.CustomerLogo,
                };

                if (commentModel.QuestionReplys != null && commentModel.QuestionReplys.Any())
                    commentDetails.QuestionReplys = commentModel.QuestionReplys.Select(c => new CommentQuestionReplyDto
                    {
                        QuestionDescription = c.questionDescription,
                        ReplyDescription = c.replyDescription
                    }).ToList();

                var lastCustomerTransmittalInfo = await _transmittalRepository
                    .AsNoTracking()
                    .Where(a => (a.TransmittalType == TransmittalType.Customer || a.TransmittalType == TransmittalType.Consultant) &&
                    a.ContractCode == authenticate.ContractCode &&
                    a.TransmittalRevisions.Any(c => c.DocumentRevisionId == commentModel.DocumentRevisionId))
                     .OrderByDescending(a => a.CreatedDate)
                     .Select(v => new
                     {
                         TransmittalNumber = v.TransmittalNumber,
                         TransmittalDate = v.CreatedDate,
                     }).FirstOrDefaultAsync();

                if (lastCustomerTransmittalInfo != null)
                {
                    commentDetails.TransmittalNumber = lastCustomerTransmittalInfo.TransmittalNumber;
                    commentDetails.TransmittalDate = lastCustomerTransmittalInfo.TransmittalDate != null ?
                        lastCustomerTransmittalInfo.TransmittalDate.ToPersianDateString() : "";
                }

                commentDetails.CompanyLogo = _fileHelper.FileReadSrc(_appSettings.CompanyLogo);

                commentDetails.CustomerLogo = _fileHelper.FileReadSrc(commentModel.CustomerLogo, ServiceSetting.UploadImagesPath.LogoSmall);

                // instantiate a html to pdf converter object
                HtmlToPdf converter = new HtmlToPdf();

                converter.Options.MarginTop = 15;
                converter.Options.MarginBottom = 15;
                converter.Options.MarginLeft = 15;
                converter.Options.MarginRight = 15;

                // set converter options
                converter.Options.PdfPageSize = PdfPageSize.A4;
                converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;

                // create a new pdf document converting an url
                var stringHeader = CommentPdfHeaderTemplate(commentDetails, selectedTemplate);

                converter.Header.Height = 170;
                converter.Header.DisplayOnFirstPage = true;
                converter.Header.DisplayOnEvenPages = true;
                converter.Header.DisplayOnOddPages = true;
                converter.Options.DisplayHeader = true;
                PdfHtmlSection headerHtml = new PdfHtmlSection(stringHeader, "");
                headerHtml.AutoFitHeight = HtmlToPdfPageFitMode.AutoFit;
                converter.Header.Add(headerHtml);

                var stringFooter = CommentPdfFooterTemplate(selectedTemplate);
                converter.Footer.Height = 50;
                converter.Footer.DisplayOnFirstPage = true;
                converter.Footer.DisplayOnEvenPages = true;
                converter.Footer.DisplayOnOddPages = true;
                converter.Options.DisplayFooter = true;
                PdfHtmlSection footerHtml = new PdfHtmlSection(stringFooter, "");
                headerHtml.AutoFitHeight = HtmlToPdfPageFitMode.AutoFit;
                converter.Footer.Add(footerHtml);

                var stringBody = CommentPdfBodyTemplate(commentDetails, selectedTemplate);
                PdfDocument doc = converter.ConvertHtmlString(stringBody);

                var gggg = doc.Save();

                var result = new DownloadFileDto
                {
                    ArchiveFile = gggg,
                    ContentType = "application/pdf",
                    FileName = commentModel.DocNumber + "-" + commentModel.CommentCode + ".pdf",
                };



                MemoryStream zipStream = new MemoryStream();

                using (ZipArchive zipFile = new ZipArchive(zipStream, ZipArchiveMode.Update, true))
                {

                    var zipPdfEntry = zipFile.CreateEntry(commentModel.CommentCode + ".pdf");

                    //Get the stream of the attachment
                    using (var originalFileStream = new MemoryStream(result.ArchiveFile))
                    {
                        using (var zipEntryStream = zipPdfEntry.Open())
                        {
                            //Copy the attachment stream to the zip entry stream
                            await originalFileStream.CopyToAsync(zipEntryStream);
                        }
                    }
                    //await ExcelHelper.CreateWordprocessingDocument(stringHeader, tqModel.TQCode, zipFile);
                    await AddAttachmentToZipFile(attachments, zipFile, authenticate.ContractCode, commentModel.DocumentId, commentModel.DocumentRevisionId);
                }
                zipStream.Seek(0, SeekOrigin.Begin);
                var finalResult = new DownloadFileDto
                {
                    Stream = zipStream,
                    ContentType = "application/zip",
                    FileName = commentModel.DocNumber + "-" + commentModel.CommentCode + ".zip",
                };
                return finalResult;
            }
            catch (Exception exception)
            {
                throw;
            }
        }

        private string CommentPdfHeaderTemplate(CommentDetailsForCreatePdfDto commentDetails, PDFTemplate template)
        {
            var html = new StringBuilder();
            var commentStatus = commentDetails.CommentStatus == CommunicationCommentStatus.Approved ? "تایید شده" : commentDetails.CommentStatus == CommunicationCommentStatus.ApproveAsNote ? "تایید مشروط" : commentDetails.CommentStatus == CommunicationCommentStatus.Commented ? "تایید نشده" : commentDetails.CommentStatus == CommunicationCommentStatus.Rejected ? "غیر قابل بررسی" : "";
            html.AppendFormat(template.Section1,
            commentDetails.CustomerLogo,
            commentDetails.ProjectDescription,
            commentDetails.CompanyLogo,
            commentDetails.CommentCode,
            commentDetails.StartDate,
            commentDetails.EndDate,
            "",
            commentDetails.TransmittalNumber,
            commentDetails.DocTitle,
            commentDetails.RevisionCode,
            commentDetails.TransmittalDate,
            commentDetails.DocNumber,
            commentDetails.DocClientNumber,
            commentStatus
            );

            return html.ToString();
        }

        private string CommentPdfBodyTemplate(CommentDetailsForCreatePdfDto commentDetails, PDFTemplate template)
        {
            var html = new StringBuilder();

            html.AppendFormat(template.Section2);

            int i = 0;
            foreach (var item in commentDetails.QuestionReplys)
            {

                html.AppendFormat(template.Section3,
                          ++i,
                          item.QuestionDescription,
                          item.ReplyDescription);
            }

            html.AppendFormat(template.Section4);

            return html.ToString();
        }

        private string CommentPdfFooterTemplate(PDFTemplate template)
        {
            var html = new StringBuilder();

            html.AppendFormat(template.Section5);
            return html.ToString();
        }

        private async Task AddAttachmentToZipFile(CommentTQNCRAttachmentDto attachments, ZipArchive zipFile, string contractCode, long documentId, long revisionId)
        {
            foreach (var item in attachments.CommentAttachment)
            {
                var file = await _fileHelper.DownloadDocument(item.FileSrc, ServiceSetting.UploadFilePathRevisionCommunication(contractCode, documentId, revisionId));
                if (file != null)
                {
                    var zipCommentEntry = zipFile.CreateEntry("Comment\\" + item.FileName);
                    using (var zipEntryStream = zipCommentEntry.Open())
                    {

                        await file.Stream.CopyToAsync(zipEntryStream);
                    }
                }
            }
            foreach (var item in attachments.ReplyAttachment)
            {
                var file = await _fileHelper.DownloadDocument(item.FileSrc, ServiceSetting.UploadFilePathRevisionCommunication(contractCode, documentId, revisionId));
                if (file != null)
                {
                    var zipReplyEntry = zipFile.CreateEntry("Reply\\" + item.FileName);
                    using (var zipEntryStream = zipReplyEntry.Open())
                    {

                        await file.Stream.CopyToAsync(zipEntryStream);
                    }
                }
            }
        }

    }
   
}
