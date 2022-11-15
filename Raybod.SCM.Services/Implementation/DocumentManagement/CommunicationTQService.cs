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
using Raybod.SCM.DataTransferObject.Document.Communication.TQ;
using Raybod.SCM.DataTransferObject.Email;
using Raybod.SCM.DataTransferObject.Notification;
using Raybod.SCM.DataTransferObject.User;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Model;
using Raybod.SCM.Domain.Struct;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Services.Utilitys.exportToExcel;
using Raybod.SCM.Services.Utilitys.MailService;
using Raybod.SCM.Services.Utilitys.MailService.Model;
using Raybod.SCM.Utility.Common;
using Raybod.SCM.Utility.Extention;
using Raybod.SCM.Utility.FileHelper;
using Raybod.SCM.Utility.Helpers;
using SelectPdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Raybod.SCM.Services.Implementation
{
    public class CommunicationTQService : ICommunicationTQService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;
        private readonly ITeamWorkAuthenticationService _authenticationServices;
        private readonly IAppEmailService _appEmailService;
        private readonly ISCMLogAndNotificationService _scmLogAndNotificationService;
        private readonly IContractFormConfigService _formConfigService;
        private readonly DbSet<DocumentRevision> _documentRevisionRepository;
        private readonly DbSet<Customer> _customerRepository;
        private readonly DbSet<Consultant> _consultantRepository;
        private readonly DbSet<Supplier> _supplierRepository;
        private readonly DbSet<User> _userRepository;
        private readonly DbSet<UserNotify> _notifyRepository;
        private readonly DbSet<DocumentTQNCR> _documentTQNCRRepository;
        private readonly DbSet<CommunicationQuestion> _communicationQuestionRepository;
        private readonly DbSet<CommunicationAttachment> _communicationAttachmentRepository;
        private readonly DbSet<PDFTemplate> _pdfTemplateRepository;
        private readonly Utilitys.FileHelper _fileHelper;
        private readonly CompanyConfig _appSettings;
        private readonly IViewRenderService _viewRenderService;

        public CommunicationTQService(
            IUnitOfWork unitOfWork,
            IWebHostEnvironment hostingEnvironmentRoot,
            IAppEmailService appEmailService,
            IOptions<CompanyAppSettingsDto> appSettings,
            ITeamWorkAuthenticationService authenticationServices,
            ISCMLogAndNotificationService scmLogAndNotificationService,
            IFileService fileService,
            IHttpContextAccessor httpContextAccessor,
            IContractFormConfigService formConfigService, IViewRenderService viewRenderService)
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
            _authenticationServices = authenticationServices;
            _scmLogAndNotificationService = scmLogAndNotificationService;
            _appEmailService = appEmailService;
            _formConfigService = formConfigService;
            _documentRevisionRepository = _unitOfWork.Set<DocumentRevision>();
            _documentTQNCRRepository = _unitOfWork.Set<DocumentTQNCR>();
            _communicationQuestionRepository = _unitOfWork.Set<CommunicationQuestion>();
            _communicationAttachmentRepository = _unitOfWork.Set<CommunicationAttachment>();
            _customerRepository = _unitOfWork.Set<Customer>();
            _notifyRepository = _unitOfWork.Set<UserNotify>();
            _consultantRepository = _unitOfWork.Set<Consultant>();
            _userRepository = _unitOfWork.Set<User>();
            _supplierRepository = _unitOfWork.Set<Supplier>();
            _pdfTemplateRepository = _unitOfWork.Set<PDFTemplate>();
            _fileHelper = new Utilitys.FileHelper(hostingEnvironmentRoot);
            _viewRenderService = viewRenderService;
            httpContextAccessor.HttpContext.Request.Headers.TryGetValue("companyCode", out var CompanyCode);
            _appSettings = appSettings.Value.CompanyConfig.First(a => a.CompanyCode == CompanyCode);
        }

        public async Task<ServiceResult<bool>> AddCommunicationTQAsync(AuthenticateDto authenticate, long revisionId, AddTQDto model)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                if (model.Question == null || string.IsNullOrEmpty(model.Question.Description))
                    return ServiceResultFactory.CreateError(false, MessageId.QuestionDescriptionCantBeNullOrEmpty);

                if (!EnumHelper.ValidateItem(model.CompanyIssue))
                    return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                if (model.CompanyIssue == CompanyIssue.Customer)
                {
                    if (!await _customerRepository.AnyAsync(a => a.Id == model.CompanyIssueId))
                        return ServiceResultFactory.CreateError(false, MessageId.CustomerNotFound);

                }
                else if (model.CompanyIssue == CompanyIssue.Supplier)
                {
                    if (!await _supplierRepository.AnyAsync(a => a.Id == model.CompanyIssueId))
                        return ServiceResultFactory.CreateError(false, MessageId.SupplierNotFound);
                }
                else if (model.CompanyIssue == CompanyIssue.Consultant)
                {
                    if (!await _consultantRepository.AnyAsync(a => a.Id == model.CompanyIssueId))
                        return ServiceResultFactory.CreateError(false, MessageId.ConsultantNotFound);
                }
                var dbQuery = _documentRevisionRepository
                    .Where(a => !a.IsDeleted &&
                    a.Document.ContractCode == authenticate.ContractCode &&
                    a.DocumentRevisionId == revisionId &&
                    a.IsLastConfirmRevision &&
                    a.RevisionStatus >= RevisionStatus.Confirmed);

                var revisionModel = await dbQuery
                    .Select(c => new
                    {
                        revisionId = c.DocumentRevisionId,
                        revisionCode = c.DocumentRevisionCode,
                        documentId = c.DocumentId,
                        DocNumber = c.Document.DocNumber,
                        DocTitle = c.Document.DocTitle,
                        documentGroupId = c.Document.DocumentGroupId,
                        customerId = c.Document.Contract.CustomerId
                    }).FirstOrDefaultAsync();

                if (revisionModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (permission.DocumentGroupIds.Any() && !permission.DocumentGroupIds.Contains(revisionModel.documentGroupId))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var communicationModel = new DocumentTQNCR
                {
                    CommunicationType = CommunicationType.TQ,
                    CompanyIssue = model.CompanyIssue,
                    Subject = model.Subject,
                    CommunicationStatus = DocumentCommunicationStatus.PendingReply,
                    CustomerId = model.CompanyIssue == CompanyIssue.Customer ? model.CompanyIssueId : (int?)null,
                    SupplierId = model.CompanyIssue == CompanyIssue.Supplier ? model.CompanyIssueId : (int?)null,
                    ConsultantId = model.CompanyIssue == CompanyIssue.Consultant ? model.CompanyIssueId : (int?)null,
                    DocumentRevisionId = revisionId,
                    CommunicationQuestions = new List<CommunicationQuestion>()
                };

                var questionResult = await AddTQQuestion(authenticate, revisionModel.documentId, revisionModel.revisionId, model.Question);
                if (!questionResult.Succeeded)
                    return ServiceResultFactory.CreateError(false, questionResult.Messages.First().Message);
                else
                    communicationModel.CommunicationQuestions.Add(questionResult.Result);

                // add attachment
                if (model.Attachments != null && model.Attachments.Any())
                {
                    var res = await AddCommunicationAttachmentAsync(authenticate, revisionModel.documentId, revisionId, communicationModel, model.Attachments);
                    if (!res.Succeeded)
                        return ServiceResultFactory.CreateError(false, res.Messages[0].Message);
                }

                // generate form code
                var count = await _documentTQNCRRepository
                    .CountAsync(a => a.CommunicationType == CommunicationType.TQ &&
                    a.DocumentRevision.Document.ContractCode == authenticate.ContractCode);

                var codeRes = await _formConfigService.GenerateFormCodeAsync(authenticate.ContractCode, FormName.CommunicationTQ, count);
                if (!codeRes.Succeeded)
                    return ServiceResultFactory.CreateError(false, codeRes.Messages.First().Message);

                communicationModel.CommunicationCode = codeRes.Result;

                _documentTQNCRRepository.Add(communicationModel);

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var res1 = await _scmLogAndNotificationService.AddDocumentAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = authenticate.ContractCode,
                        Description = revisionModel.revisionCode,
                        FormCode = communicationModel.CommunicationCode,
                        Message = revisionModel.DocTitle,
                        KeyValue = communicationModel.DocumentTQNCRId.ToString(),
                        NotifEvent = NotifEvent.AddTQ,
                        RootKeyValue = revisionModel.revisionId.ToString(),
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,
                        DocumentGroupId = revisionModel.documentGroupId
                    },
                 revisionModel.documentGroupId,
                  new List<NotifToDto> {
                    new NotifToDto
                    {
                        NotifEvent= NotifEvent.ReplyTQComment,
                        Roles= new List<string>
                        {
                           SCMRole.TQMng,
                           SCMRole.TQReply,
                        }
                    }
                  });
                    try
                    {
                        BackgroundJob.Enqueue(() => SendEmailOnAddCommunicationAsync(authenticate, communicationModel, revisionModel.documentGroupId,
                        revisionModel.documentId, revisionModel.DocTitle, revisionModel.DocNumber));
                        //if(communicationModel.CompanyIssue==CompanyIssue.Consultant||communicationModel.CompanyIssue==CompanyIssue.Customer)
                        //BackgroundJob.Enqueue(()=> SendEmailOnAddCommunicationForCustomerUserAsync(authenticate, communicationModel, revisionModel.documentGroupId,
                        //revisionModel.documentId, revisionModel.DocTitle, revisionModel.DocNumber));
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

        public async Task<ServiceResult<bool>> AddCommunicationTQForCustomerAsync(AuthenticateDto authenticate, long revisionId, AddTQDto model, bool accessability)
        {
            try
            {
                if (!accessability)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);



                if (model.Question == null || string.IsNullOrEmpty(model.Question.Description))
                    return ServiceResultFactory.CreateError(false, MessageId.QuestionDescriptionCantBeNullOrEmpty);

                //if (!EnumHelper.ValidateItem(model.CompanyIssue))
                //    return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                //if (model.CompanyIssue == CompanyIssue.Customer)
                //{
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
                        model.CompanyIssue = CompanyIssue.Consultant;
                        model.CompanyIssueId = consultant.Id;
                    }
                }

                else
                {
                    model.CompanyIssue = CompanyIssue.Customer;
                    model.CompanyIssueId = customer.Id;
                }

                if (!await _customerRepository.AnyAsync(a => a.Id == model.CompanyIssueId) && model.CompanyIssue == CompanyIssue.Customer)
                    return ServiceResultFactory.CreateError(false, MessageId.CustomerNotFound);
                if (!await _consultantRepository.AnyAsync(a => a.Id == model.CompanyIssueId) && model.CompanyIssue == CompanyIssue.Consultant)
                    return ServiceResultFactory.CreateError(false, MessageId.ConsultantNotFound);
                //}
                //else if (model.CompanyIssue == CompanyIssue.Supplier)
                //{
                //    if (!await _supplierRepository.AnyAsync(a => a.Id == model.CompanyIssueId))
                //        return ServiceResultFactory.CreateError(false, MessageId.SupplierNotFound);
                //}

                var dbQuery = _documentRevisionRepository
                    .Where(a => !a.IsDeleted &&
                    a.Document.ContractCode == authenticate.ContractCode &&
                    a.DocumentRevisionId == revisionId &&
                    a.IsLastConfirmRevision &&
                    a.RevisionStatus >= RevisionStatus.Confirmed);

                var revisionModel = await dbQuery
                    .Select(c => new
                    {
                        revisionId = c.DocumentRevisionId,
                        revisionCode = c.DocumentRevisionCode,
                        documentId = c.DocumentId,
                        DocNumber = c.Document.DocNumber,
                        DocTitle = c.Document.DocTitle,
                        documentGroupId = c.Document.DocumentGroupId,
                        customerId = c.Document.Contract.CustomerId
                    }).FirstOrDefaultAsync();

                if (revisionModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);



                var communicationModel = new DocumentTQNCR
                {
                    CommunicationType = CommunicationType.TQ,
                    CompanyIssue = model.CompanyIssue,
                    Subject = model.Subject,
                    CommunicationStatus = DocumentCommunicationStatus.PendingReply,
                    CustomerId = model.CompanyIssue == CompanyIssue.Customer ? model.CompanyIssueId : (int?)null,
                    SupplierId = (int?)null,
                    ConsultantId = model.CompanyIssue == CompanyIssue.Consultant ? model.CompanyIssueId : (int?)null,
                    DocumentRevisionId = revisionId,
                    CommunicationQuestions = new List<CommunicationQuestion>()
                };

                var questionResult = await AddTQQuestion(authenticate, revisionModel.documentId, revisionModel.revisionId, model.Question);
                if (!questionResult.Succeeded)
                    return ServiceResultFactory.CreateError(false, questionResult.Messages.First().Message);
                else
                    communicationModel.CommunicationQuestions.Add(questionResult.Result);

                // add attachment
                if (model.Attachments != null && model.Attachments.Any())
                {
                    var res = await AddCommunicationAttachmentAsync(authenticate, revisionModel.documentId, revisionId, communicationModel, model.Attachments);
                    if (!res.Succeeded)
                        return ServiceResultFactory.CreateError(false, res.Messages[0].Message);
                }

                // generate form code
                var count = await _documentTQNCRRepository
                    .CountAsync(a => a.CommunicationType == CommunicationType.TQ &&
                    a.DocumentRevision.Document.ContractCode == authenticate.ContractCode);

                var codeRes = await _formConfigService.GenerateFormCodeAsync(authenticate.ContractCode, FormName.CommunicationTQ, count);
                if (!codeRes.Succeeded)
                    return ServiceResultFactory.CreateError(false, codeRes.Messages.First().Message);

                communicationModel.CommunicationCode = codeRes.Result;

                _documentTQNCRRepository.Add(communicationModel);

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var res1 = await _scmLogAndNotificationService.AddDocumentAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = authenticate.ContractCode,
                        Description = revisionModel.revisionCode,
                        FormCode = communicationModel.CommunicationCode,
                        Message = revisionModel.DocTitle,
                        KeyValue = communicationModel.DocumentTQNCRId.ToString(),
                        NotifEvent = NotifEvent.AddTQ,
                        RootKeyValue = revisionModel.revisionId.ToString(),
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,
                        DocumentGroupId = revisionModel.documentGroupId
                    },
                 revisionModel.documentGroupId,
                  new List<NotifToDto> {
                    new NotifToDto
                    {
                        NotifEvent= NotifEvent.ReplyTQComment,
                        Roles= new List<string>
                        {
                           SCMRole.TQMng,
                           SCMRole.TQReply,
                        }
                    }
                  });
                    try
                    {
                        BackgroundJob.Enqueue(() => SendEmailOnAddCommunicationAsync(authenticate, communicationModel, revisionModel.documentGroupId,
                        revisionModel.documentId, revisionModel.DocTitle, revisionModel.DocNumber));
                        //if(communicationModel.CompanyIssue==CompanyIssue.Consultant||communicationModel.CompanyIssue==CompanyIssue.Customer)
                        //BackgroundJob.Enqueue(() => SendEmailOnAddCommunicationForCustomerUserAsync(authenticate, communicationModel, revisionModel.documentGroupId,
                        //revisionModel.documentId, revisionModel.DocTitle, revisionModel.DocNumber));
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

        public async Task<ServiceResult<bool>> SendEmailOnAddCommunicationAsync(AuthenticateDto authenticate, DocumentTQNCR communicationModel,
            int documentGroupId, long documentId, string docTitle, string docNumber)
        {
            var toRoles = new List<string> {
                SCMRole.TQMng,
                SCMRole.TQObs,
               SCMRole.TQReg,
               SCMRole.TQReply
               };

            var toUsers = await _authenticationServices
                .GetAllUserHasAccessDocumentAsync(authenticate.ContractCode, toRoles, documentGroupId);
            var tqEmailNotify = await _notifyRepository.Where(a => a.TeamWork.ContractCode == authenticate.ContractCode && a.NotifyType == NotifyManagementType.Email && a.NotifyNumber == (int)EmailNotify.TQ && a.IsActive).Select(a => a.UserId).ToListAsync();
            if (tqEmailNotify != null && tqEmailNotify.Any())
                toUsers = toUsers.Where(a => tqEmailNotify.Contains(a.Id)).ToList();
            else
                toUsers = new List<UserMentionDto>();
            var toUserEmails =(toUsers!=null&&toUsers.Any())? toUsers
                .Where(a => !string.IsNullOrEmpty(a.Email))
                .Select(a => a.Email)
                .ToList():new List<string>();

            if (!toUserEmails.Any())
                return ServiceResultFactory.CreateSuccess(true);
            var attachments = await GenerateTQAttachmentZipAsync(authenticate, communicationModel.DocumentTQNCRId);

            string faMessage = $"در پروژه <span style='direction:ltr'>{authenticate.ContractCode}</span> یک TQ جدید به شماره <span style='direction:ltr'>{communicationModel.CommunicationCode}</span> مربوط به مدرک <span style='direction:ltr'>{docTitle}</span> به شماره <span style='direction:ltr'>{docNumber}</span> توسط {authenticate.UserFullName} به شرح زیر ثبت گردید. ";
            string enMessage = $"<div style='direction:ltr;text-align:left'>TQ No. {communicationModel.CommunicationCode} related to document titled {docTitle} - {docNumber} has been registered  by {authenticate.UserFullName} as following.</div>";
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
                Subject = $"TQ | {communicationModel.CommunicationCode}"
            };
            await _appEmailService.SendAsync(emailRequest, attachments!=null?attachments.Stream:new MemoryStream(), communicationModel.CommunicationCode+".zip");

            return ServiceResultFactory.CreateSuccess(true);
        }
        public async Task<ServiceResult<bool>> SendEmailOnAddCommunicationForCustomerUserAsync(AuthenticateDto authenticate, DocumentTQNCR communicationModel,
            int documentGroupId, long documentId, string docTitle, string docNumber)
        {
            List<string> users = new List<string>();
            if (communicationModel.CompanyIssue == CompanyIssue.Customer)
            {
                users = await _userRepository.Where(a => _customerRepository.Any(b => b.Id == communicationModel.CustomerId && b.CustomerUsers.Any(c => c.Email == a.Email))).Select(d => d.Email).ToListAsync();
            }
            else if
                (communicationModel.CompanyIssue == CompanyIssue.Consultant)
            {
                users = await _userRepository.Where(a => _consultantRepository.Any(b => b.Id == communicationModel.ConsultantId && b.ConsultantUsers.Any(c => c.Email == a.Email))).Select(d => d.Email).ToListAsync();
            }

            if (users == null || !users.Any())
                return ServiceResultFactory.CreateSuccess(true);

            var emailAttachs = PreParationAchmentForSendInEmail(communicationModel, documentId, authenticate.ContractCode);

            string message = $"در پروژه {authenticate.ContractCode} یک TQ جدید به شماره {communicationModel.CommunicationCode} مربوط به مدرک {docTitle} به شماره {docNumber} توسط {authenticate.UserFullName} به شرح زیر ثبت گردید. ";
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
                Tos = users,
                Body = await _viewRenderService.RenderToStringAsync("_CommentCreateNotifEmail", model),
                Subject = $"TQ {communicationModel.CommunicationCode}"
            };
            await _appEmailService.SendAsync(emailRequest, emailAttachs, true);

            return ServiceResultFactory.CreateSuccess(true);
        }

        private List<InMemoryFileDto> PreParationAchmentForSendInEmail(DocumentTQNCR communication, long documentId,
            string contractCode)
        {
            var result = new List<InMemoryFileDto>();

            foreach (var item in communication.CommunicationQuestions.First().Attachments)
            {
                result.Add(new InMemoryFileDto
                {
                    FileName = item.FileName,
                    FileSrc = item.FileSrc,
                    FileUrl = ServiceSetting.UploadFilePathRevisionCommunication(contractCode, documentId, communication.DocumentRevisionId),
                });
            }

            return result;
        }


        private async Task<ServiceResult<CommunicationQuestion>> AddTQQuestion(AuthenticateDto authenticate, long docId,
            long revId, AddTQQuestionDto model)
        {
            try
            {
                var questionModel = new CommunicationQuestion
                {
                    Description = model.Description,
                    Attachments = new List<CommunicationAttachment>()
                };
                return await AddQuestionAttachmentAsync(authenticate, docId, revId, model, questionModel);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<CommunicationQuestion>(null, exception);
            }
        }

        private async Task<ServiceResult<CommunicationQuestion>> AddQuestionAttachmentAsync(AuthenticateDto authenticate, long docId, long revId, AddTQQuestionDto model, CommunicationQuestion questionModel)
        {
            if (model.Attachments != null && model.Attachments.Any())
            {
                foreach (var item in model.Attachments)
                {
                    var UploadedFile = await _fileHelper
                        .SaveDocumentFromTemp(item.FileSrc, ServiceSetting.UploadFilePathRevisionCommunication(authenticate.ContractCode, docId, revId));

                    if (UploadedFile == null)
                        return ServiceResultFactory.CreateError<CommunicationQuestion>(null, MessageId.UploudFailed);

                    _fileHelper.DeleteDocumentFromTemp(item.FileSrc);

                    questionModel.Attachments.Add(new CommunicationAttachment
                    {
                        FileSrc = item.FileSrc,
                        FileName = item.FileName,
                        FileType = UploadedFile.FileType,
                        FileSize = UploadedFile.FileSize
                    });
                }
            }
            return ServiceResultFactory.CreateSuccess(questionModel);
        }

        private async Task<ServiceResult<bool>> AddCommunicationAttachmentAsync(AuthenticateDto authenticate, long docId, long revId,
            DocumentTQNCR tqNCRModel, List<AddRevisionAttachmentDto> files)
        {
            var attachModels = new List<CommunicationAttachment>();
            foreach (var item in files)
            {
                var UploadedFile = await _fileHelper
                    .SaveDocumentFromTemp(item.FileSrc, ServiceSetting.UploadFilePathRevisionCommunication(authenticate.ContractCode, docId, revId));

                if (UploadedFile == null)
                    return ServiceResultFactory.CreateError(false, MessageId.UploudFailed);

                _fileHelper.DeleteDocumentFromTemp(item.FileSrc);

                attachModels.Add(new CommunicationAttachment
                {
                    DocumentTQNCR = tqNCRModel,
                    FileSrc = item.FileSrc,
                    FileName = item.FileName,
                    FileType = UploadedFile.FileType,
                    FileSize = UploadedFile.FileSize
                });

            }
            _communicationAttachmentRepository.AddRange(attachModels);
            return ServiceResultFactory.CreateSuccess(true);

        }

        public async Task<ServiceResult<List<TQListDto>>> GetTQListAsync(AuthenticateDto authenticate, TQQueryDto query)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<TQListDto>>(null, MessageId.AccessDenied);

                var dbQuery = _documentTQNCRRepository
                    .AsNoTracking()
                    .Where(a => a.CommunicationType == CommunicationType.TQ && a.DocumentRevision.Document.ContractCode == authenticate.ContractCode);

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a =>
                     a.CommunicationCode.Contains(query.SearchText)
                     || a.DocumentRevision.Document.DocNumber.Contains(query.SearchText)
                     || a.DocumentRevision.Document.DocTitle.Contains(query.SearchText));

                if (EnumHelper.ValidateItem(query.DocClass))
                    dbQuery = dbQuery.Where(a => a.DocumentRevision.Document.DocClass == query.DocClass);

                if (EnumHelper.ValidateItem(query.CommunicationStatus))
                    dbQuery = dbQuery.Where(a => a.CommunicationStatus == query.CommunicationStatus);

                if (EnumHelper.ValidateItem(query.CompanyIssue))
                {
                    if (query.CompanyIssue == CompanyIssue.Customer && query.CompanyIssueId > 0)
                        dbQuery = dbQuery.Where(a => a.CompanyIssue == query.CompanyIssue && a.CustomerId == query.CompanyIssueId);
                    else if (query.CompanyIssue == CompanyIssue.Supplier && query.CompanyIssueId > 0)
                        dbQuery = dbQuery.Where(a => a.CompanyIssue == query.CompanyIssue && a.SupplierId == query.CompanyIssueId);
                    else if (query.CompanyIssue == CompanyIssue.Consultant && query.CompanyIssueId > 0)
                        dbQuery = dbQuery.Where(a => a.CompanyIssue == query.CompanyIssue && a.ConsultantId == query.CompanyIssueId);
                    else if (query.CompanyIssue == CompanyIssue.Internal)
                        dbQuery = dbQuery.Where(a => a.CompanyIssue == query.CompanyIssue);
                }

                if (query.DocumentGroupIds != null && query.DocumentGroupIds.Any())
                    dbQuery = dbQuery.Where(a => query.DocumentGroupIds.Contains(a.DocumentRevision.Document.DocumentGroupId));

                if (permission.DocumentGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.DocumentGroupIds.Contains(a.DocumentRevision.Document.DocumentGroupId));

                var columnsMap = new Dictionary<string, Expression<Func<DocumentTQNCR, object>>>
                {
                    ["DocumentTQNCRId"] = v => v.DocumentTQNCRId
                };

                var totalCount = dbQuery.Count();
                dbQuery = dbQuery.ApplayOrdering(query, columnsMap).ApplayPageing(query);

                var result = await dbQuery.Select(c => new TQListDto
                {
                    CommunicationId = c.DocumentTQNCRId,
                    CommunicationCode = c.CommunicationCode,
                    CommunicationStatus = c.CommunicationStatus,
                    CompanyIssue = c.CompanyIssue,
                    CompanyIssueName = c.CompanyIssue == CompanyIssue.Customer && c.Customer != null ? c.Customer.Name :
                    c.CompanyIssue == CompanyIssue.Supplier && c.Supplier != null ? c.Supplier.Name : c.CompanyIssue == CompanyIssue.Consultant && c.Consultant != null ? c.Consultant.Name : "داخلی",
                    DocumentRevisionCode = c.DocumentRevision.DocumentRevisionCode,
                    DocNumber = c.DocumentRevision.Document.DocNumber,
                    DocTitle = c.DocumentRevision.Document.DocTitle,
                    DocumentGroupTitle = c.DocumentRevision.Document.DocumentGroup.Title,
                    DocumentRevisionId = c.DocumentRevisionId,
                    Replayer = (c.CommunicationQuestions.FirstOrDefault() != null && c.CommunicationQuestions.FirstOrDefault().CommunicationReply != null) ? c.CommunicationQuestions.OrderByDescending(c => c.CommunicationQuestionId).FirstOrDefault().CommunicationReply.AdderUser.FullName : "",
                    ReplayDate = (c.CommunicationQuestions.FirstOrDefault() != null && c.CommunicationQuestions.FirstOrDefault().CommunicationReply != null) ? c.CommunicationQuestions.OrderByDescending(c => c.CommunicationQuestionId).FirstOrDefault().CommunicationReply.CreatedDate.ToUnixTimestamp() : null,
                    Subject = c.Subject,
                    UserAudit = c.AdderUser != null ? new UserAuditLogDto
                    {
                        AdderUserName = c.AdderUser.FullName,
                        AdderUserImage = c.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + c.AdderUser.Image : "",
                        CreateDate = c.CreatedDate.ToUnixTimestamp(),
                    } : null,
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<TQListDto>>(null, exception);
            }
        }
        public async Task<ServiceResult<List<TQListDto>>> GetTQListForCustomerAsync(AuthenticateDto authenticate, TQQueryDto query, bool accessability)
        {
            try
            {
                if (!accessability)
                    return ServiceResultFactory.CreateError<List<TQListDto>>(null, MessageId.AccessDenied);



                var dbQuery = _documentTQNCRRepository
                    .AsNoTracking()
                    .Where(a => a.CommunicationType == CommunicationType.TQ && a.DocumentRevision.Document.ContractCode == authenticate.ContractCode && (a.CompanyIssue == CompanyIssue.Customer || a.CompanyIssue == CompanyIssue.Consultant));

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a =>
                     a.CommunicationCode.Contains(query.SearchText)
                     || a.DocumentRevision.Document.DocNumber.Contains(query.SearchText)
                     || a.DocumentRevision.Document.DocTitle.Contains(query.SearchText));

                if (EnumHelper.ValidateItem(query.DocClass))
                    dbQuery = dbQuery.Where(a => a.DocumentRevision.Document.DocClass == query.DocClass);

                if (EnumHelper.ValidateItem(query.CommunicationStatus))
                    dbQuery = dbQuery.Where(a => a.CommunicationStatus == query.CommunicationStatus);

                if (EnumHelper.ValidateItem(query.CompanyIssue))
                {
                    if (query.CompanyIssue == CompanyIssue.Customer && query.CompanyIssueId > 0)
                        dbQuery = dbQuery.Where(a => a.CompanyIssue == query.CompanyIssue && a.CustomerId == query.CompanyIssueId);
                    else if (query.CompanyIssue == CompanyIssue.Supplier && query.CompanyIssueId > 0)
                        dbQuery = dbQuery.Where(a => a.CompanyIssue == query.CompanyIssue && a.SupplierId == query.CompanyIssueId);
                    else if (query.CompanyIssue == CompanyIssue.Consultant && query.CompanyIssueId > 0)
                        dbQuery = dbQuery.Where(a => a.CompanyIssue == query.CompanyIssue && a.ConsultantId == query.CompanyIssueId);
                    else if (query.CompanyIssue == CompanyIssue.Internal)
                        dbQuery = dbQuery.Where(a => a.CompanyIssue == query.CompanyIssue);
                }

                if (query.DocumentGroupIds != null && query.DocumentGroupIds.Any())
                    dbQuery = dbQuery.Where(a => query.DocumentGroupIds.Contains(a.DocumentRevision.Document.DocumentGroupId));



                var columnsMap = new Dictionary<string, Expression<Func<DocumentTQNCR, object>>>
                {
                    ["DocumentTQNCRId"] = v => v.DocumentTQNCRId
                };

                var totalCount = dbQuery.Count();
                dbQuery = dbQuery.ApplayOrdering(query, columnsMap).ApplayPageing(query);

                var result = await dbQuery.Select(c => new TQListDto
                {
                    CommunicationId = c.DocumentTQNCRId,
                    CommunicationCode = c.CommunicationCode,
                    CommunicationStatus = c.CommunicationStatus,
                    CompanyIssue = c.CompanyIssue,

                    CompanyIssueName = c.CompanyIssue == CompanyIssue.Customer && c.Customer != null ? c.Customer.Name :
                    c.CompanyIssue == CompanyIssue.Supplier && c.Supplier != null ? c.Supplier.Name : c.CompanyIssue == CompanyIssue.Consultant && c.Consultant != null ? c.Consultant.Name : "داخلی",
                    DocumentRevisionCode = c.DocumentRevision.DocumentRevisionCode,
                    DocNumber = c.DocumentRevision.Document.DocNumber,
                    DocTitle = c.DocumentRevision.Document.DocTitle,
                    DocumentGroupTitle = c.DocumentRevision.Document.DocumentGroup.Title,
                    DocumentRevisionId = c.DocumentRevisionId,
                    Replayer = (c.CommunicationQuestions.FirstOrDefault() != null && c.CommunicationQuestions.FirstOrDefault().CommunicationReply != null) ? c.CommunicationQuestions.OrderByDescending(c => c.CommunicationQuestionId).FirstOrDefault().CommunicationReply.AdderUser.FullName : "",
                    ReplayDate = (c.CommunicationQuestions.FirstOrDefault() != null && c.CommunicationQuestions.FirstOrDefault().CommunicationReply != null) ? c.CommunicationQuestions.OrderByDescending(c => c.CommunicationQuestionId).FirstOrDefault().CommunicationReply.CreatedDate.ToUnixTimestamp() : null,
                    Subject = c.Subject,
                    UserAudit = c.AdderUser != null ? new UserAuditLogDto
                    {
                        AdderUserName = c.AdderUser.FullName,
                        AdderUserImage = c.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + c.AdderUser.Image : "",
                        CreateDate = c.CreatedDate.ToUnixTimestamp(),
                    } : null,
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<TQListDto>>(null, exception);
            }
        }
        public async Task<ServiceResult<TQQuestionDetailsDto>> GetTQQuestionDetailsAsync(AuthenticateDto authenticate, long communicationId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<TQQuestionDetailsDto>(null, MessageId.AccessDenied);

                var dbQuery = _documentTQNCRRepository
                    .AsNoTracking()
                    .Where(a => a.DocumentTQNCRId == communicationId &&
                    a.CommunicationType == CommunicationType.TQ &&
                    a.DocumentRevision.Document.ContractCode == authenticate.ContractCode);

                if (permission.DocumentGroupIds.Any() && !dbQuery.Any(c => permission.DocumentGroupIds.Contains(c.DocumentRevision.Document.DocumentGroupId)))
                    return ServiceResultFactory.CreateError<TQQuestionDetailsDto>(null, MessageId.AccessDenied);

                var result = await dbQuery.Select(c => new TQQuestionDetailsDto
                {
                    Subject = c.Subject,
                    Questions = c.CommunicationQuestions.Select(v => new QuestionDto
                    {
                        QuestionId = v.CommunicationQuestionId,
                        Description = v.Description,
                        IsReplyed = v.IsReplyed,
                        Attachments = v.Attachments.Where(b => !b.IsDeleted)
                        .Select(n => new CommunicationAttachmentDto
                        {
                            AttachmentId = n.CommunicationAttachmentId,
                            FileName = n.FileName,
                            FileSize = n.FileSize,
                            FileSrc = n.FileSrc,
                            FileType = n.FileType
                        }).ToList(),
                        UserAudit = v.AdderUser != null ? new UserAuditLogDto
                        {
                            AdderUserName = v.AdderUser.FullName,
                            AdderUserImage = "",
                            CreateDate = v.CreatedDate.ToUnixTimestamp(),
                        } : null,
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
                return ServiceResultFactory.CreateException<TQQuestionDetailsDto>(null, exception);
            }
        }
        public async Task<DownloadFileDto> GenerateTQPdfAsync(AuthenticateDto authenticate, long communicationId)
        {
            try
            {
                //var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                //if (!permission.HasPermission)
                //    return null;


                var dbQuery = _communicationQuestionRepository
                    .AsNoTracking()
                    .Where(a => a.DocumentTQNCRId == communicationId &&
                    a.DocumentTQNCR.CommunicationType == CommunicationType.TQ &&
                    a.DocumentTQNCR.DocumentRevision.Document.ContractCode == authenticate.ContractCode);

                //if (permission.DocumentGroupIds.Any() && !dbQuery.Any(c => permission.DocumentGroupIds.Contains(c.DocumentTQNCR.DocumentRevision.Document.DocumentGroupId)))
                //    return null;

                var tqModel = await dbQuery.Select(a => new TQDetailsForCreatePdfDto
                {

                    TQCode = a.DocumentTQNCR.CommunicationCode,
                    DocNumber = a.DocumentTQNCR.DocumentRevision.Document.DocNumber,
                    RevisionCode = a.DocumentTQNCR.DocumentRevision.DocumentRevisionCode,
                    ProjectDescription = a.DocumentTQNCR.DocumentRevision.Document.Contract.Description,
                    TQSubject = a.DocumentTQNCR.Subject,
                    QuestionDescription = a.Description,
                    RegisterQuestionDate = a.CreatedDate,
                    RegisterQuestionUser = a.AdderUser != null ? a.AdderUser.FullName : "",
                    IsReplyed = a.IsReplyed,
                    ReplyDescription = a.IsReplyed ? a.CommunicationReply.Description : "",
                    RegisterReplyDate = a.IsReplyed ? a.CommunicationReply.CreatedDate : (DateTime?)null,
                    RegisterReplyUser = a.IsReplyed && a.CommunicationReply.AdderUser != null ? a.CommunicationReply.AdderUser.FullName : "",
                    CompanyIssue = a.DocumentTQNCR.CompanyIssue,
                    CompanyIssueName = a.DocumentTQNCR.CompanyIssue == CompanyIssue.Customer && a.DocumentTQNCR.Customer != null ? a.DocumentTQNCR.Customer.Name :
                    a.DocumentTQNCR.CompanyIssue == CompanyIssue.Supplier && a.DocumentTQNCR.Supplier != null ? a.DocumentTQNCR.Supplier.Name : a.DocumentTQNCR.CompanyIssue == CompanyIssue.Consultant && a.DocumentTQNCR.Consultant != null ? a.DocumentTQNCR.Consultant.Name : "داخلی",
                    CustomerLogo = a.DocumentTQNCR.DocumentRevision.Document.Contract.Customer.Logo,
                    CompanyLogo = a.DocumentTQNCR.Customer != null ? a.DocumentTQNCR.Customer.Logo : a.DocumentTQNCR.Supplier != null ? a.DocumentTQNCR.Supplier.Logo : a.DocumentTQNCR.Consultant != null ? a.DocumentTQNCR.Consultant.Logo : ""
                }).FirstOrDefaultAsync();

                if (tqModel == null)
                    return null;

                var selectedTemplate = new PDFTemplate();
                var templates = _pdfTemplateRepository
                    .Where(a => (a.ContractCode == null || a.ContractCode == authenticate.ContractCode) &&
                     a.PDFTemplateType == PDFTemplateType.TQ)
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

                tqModel.CustomerLogo = _fileHelper.FileReadSrc(tqModel.CustomerLogo, ServiceSetting.UploadImagesPath.LogoSmall);

                tqModel.CompanyLogo = _fileHelper.FileReadSrc(_appSettings.CompanyLogo);
                tqModel.ReplyerCompany = (tqModel.IsReplyed) ? _appSettings.CompanyNameFA : "";
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
                var stringHeader = TQPdfHeaderTemplate(tqModel, selectedTemplate);
                converter.Header.Height = 150;
                converter.Header.DisplayOnFirstPage = true;
                converter.Header.DisplayOnEvenPages = true;
                converter.Header.DisplayOnOddPages = true;
                converter.Options.DisplayHeader = true;
                PdfHtmlSection headerHtml = new PdfHtmlSection(stringHeader, "");
                headerHtml.AutoFitHeight = HtmlToPdfPageFitMode.AutoFit;
                converter.Header.Add(headerHtml);

                //converter.Footer.Height = 40;
                //converter.Footer.DisplayOnFirstPage = true;
                //converter.Footer.DisplayOnEvenPages = true;
                //converter.Footer.DisplayOnOddPages = true;
                //converter.Options.DisplayFooter = true;
                //PdfHtmlSection footerHtml = new PdfHtmlSection(stringFooter, "");
                //headerHtml.AutoFitHeight = HtmlToPdfPageFitMode.AutoFit;
                //converter.Footer.Add(footerHtml);
                var htmlBody = TQPdfBodyTemplate(tqModel, selectedTemplate);
                PdfDocument doc = converter.ConvertHtmlString(htmlBody);

                var gggg = doc.Save();

                var result = new DownloadFileDto
                {
                    ArchiveFile = gggg,
                    ContentType = "application/pdf",
                    FileName = tqModel.DocNumber + "-" + tqModel.TQCode + ".pdf",
                };
                return result;

            }
            catch (Exception exception)
            {
                throw;
            }
        }
        public async Task<ServiceResult<TQQuestionDetailsDto>> GetTQQuestionDetailsForCustomerAsync(AuthenticateDto authenticate, long communicationId, bool accessability)
        {
            try
            {
                if (!accessability)
                    return ServiceResultFactory.CreateError<TQQuestionDetailsDto>(null, MessageId.AccessDenied);




                var dbQuery = _documentTQNCRRepository
                    .AsNoTracking()
                    .Where(a => a.DocumentTQNCRId == communicationId &&
                    a.CommunicationType == CommunicationType.TQ &&
                    a.DocumentRevision.Document.ContractCode == authenticate.ContractCode);



                var result = await dbQuery.Select(c => new TQQuestionDetailsDto
                {
                    Subject = c.Subject,
                    Questions = c.CommunicationQuestions.Select(v => new QuestionDto
                    {
                        QuestionId = v.CommunicationQuestionId,
                        Description = v.Description,
                        IsReplyed = v.IsReplyed,
                        Attachments = v.Attachments.Where(b => !b.IsDeleted)
                        .Select(n => new CommunicationAttachmentDto
                        {
                            AttachmentId = n.CommunicationAttachmentId,
                            FileName = n.FileName,
                            FileSize = n.FileSize,
                            FileSrc = n.FileSrc,
                            FileType = n.FileType
                        }).ToList(),
                        UserAudit = v.AdderUser != null ? new UserAuditLogDto
                        {
                            AdderUserName = v.AdderUser.FullName,
                            AdderUserImage = "",
                            CreateDate = v.CreatedDate.ToUnixTimestamp(),
                        } : null,
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
                return ServiceResultFactory.CreateException<TQQuestionDetailsDto>(null, exception);
            }
        }
        public async Task<ServiceResult<TQDetailsDto>> GetTQDetailsAsync(AuthenticateDto authenticate, long communicationId)
        {
            try
            {
                //var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                //if (!permission.HasPermission)
                //    return ServiceResultFactory.CreateError<CurrentContractInfoDto>(null, MessageId.AccessDenied);

                var dbQuery = _documentTQNCRRepository
                    .AsNoTracking()
                    .Where(a => a.DocumentTQNCRId == communicationId &&
                    a.CommunicationType == CommunicationType.TQ &&
                    a.DocumentRevision.Document.ContractCode == authenticate.ContractCode);


                var result = await dbQuery.Select(c => new TQDetailsDto
                {
                    CommunicationId = c.DocumentTQNCRId,
                    CommunicationCode = c.CommunicationCode,
                    CommunicationStatus = c.CommunicationStatus,
                    CompanyIssue = c.CompanyIssue,
                    CompanyIssueName = c.CompanyIssue == CompanyIssue.Customer && c.Customer != null ? c.Customer.Name :
                    c.CompanyIssue == CompanyIssue.Supplier && c.Supplier != null ? c.Supplier.Name : c.CompanyIssue == CompanyIssue.Consultant && c.Consultant != null ? c.Consultant.Name : "داخلی",
                    DocumentRevisionCode = c.DocumentRevision.DocumentRevisionCode,
                    DocNumber = c.DocumentRevision.Document.DocNumber,
                    DocTitle = c.DocumentRevision.Document.DocTitle,
                    DocumentGroupTitle = c.DocumentRevision.Document.DocumentGroup.Title,
                    DocumentRevisionId = c.DocumentRevisionId,
                    Subject = c.Subject,
                    Attachments = c.Attachments.Where(b => !b.IsDeleted)
                        .Select(n => new CommunicationAttachmentDto
                        {
                            AttachmentId = n.CommunicationAttachmentId,
                            FileName = n.FileName,
                            FileSize = n.FileSize,
                            FileSrc = n.FileSrc,
                            FileType = n.FileType
                        }).ToList(),
                    UserAudit = c.AdderUser != null ? new UserAuditLogDto
                    {
                        AdderUserName = c.AdderUser.FullName,
                        AdderUserImage = c.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + c.AdderUser.Image : "",
                        CreateDate = c.CreatedDate.ToUnixTimestamp(),
                    } : null,
                    Questions = c.CommunicationQuestions.Select(v => new QuestionDto
                    {
                        QuestionId = v.CommunicationQuestionId,
                        Description = v.Description,
                        IsReplyed = v.IsReplyed,
                        Attachments = v.Attachments.Where(b => !b.IsDeleted)
                        .Select(n => new CommunicationAttachmentDto
                        {
                            AttachmentId = n.CommunicationAttachmentId,
                            FileName = n.FileName,
                            FileSize = n.FileSize,
                            FileSrc = n.FileSrc,
                            FileType = n.FileType
                        }).ToList(),
                        UserAudit = v.AdderUser != null ? new UserAuditLogDto
                        {
                            AdderUserName = v.AdderUser.FullName,
                            AdderUserImage = "",
                            CreateDate = v.CreatedDate.ToUnixTimestamp(),
                        } : null,
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
                return ServiceResultFactory.CreateException<TQDetailsDto>(null, exception);
            }
        }

        public async Task<ServiceResult<TQQuestionDetailsDto>> AddReplayTQQuestionAsync(AuthenticateDto authenticate, long communicationId, AddTQReplyDto model)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<TQQuestionDetailsDto>(null, MessageId.AccessDenied);

                if (model == null || string.IsNullOrEmpty(model.Description))
                    return ServiceResultFactory.CreateError<TQQuestionDetailsDto>(null, MessageId.ReplyDescriptionCantBeNullOrEmpty);

                var dbQuery = _documentTQNCRRepository
                    .Where(a => a.DocumentTQNCRId == communicationId &&
                    a.CommunicationType == CommunicationType.TQ &&
                    a.CommunicationStatus == DocumentCommunicationStatus.PendingReply &&
                    a.DocumentRevision.Document.ContractCode == authenticate.ContractCode);

                if (permission.DocumentGroupIds.Any() && !dbQuery.Any(c => permission.DocumentGroupIds.Contains(c.DocumentRevision.Document.DocumentGroupId)))
                    return ServiceResultFactory.CreateError<TQQuestionDetailsDto>(null, MessageId.AccessDenied);

                var communicationModel = await dbQuery
                    .Include(a => a.AdderUser)
                    .Include(a => a.DocumentRevision)
                    .ThenInclude(a => a.Document)
                    .Include(a => a.CommunicationQuestions)
                    .ThenInclude(c => c.CommunicationReply)
                    .FirstOrDefaultAsync();

                var questionModel = communicationModel.CommunicationQuestions.Where(a => !a.IsReplyed).FirstOrDefault();


                if (questionModel == null || questionModel.CommunicationQuestionId != model.QuestionId)
                    return ServiceResultFactory.CreateError<TQQuestionDetailsDto>(null, MessageId.DataInconsistency);

                questionModel.IsReplyed = true;
                questionModel.CommunicationReply = new CommunicationReply
                {
                    Description = model.Description,
                    Attachments = new List<CommunicationAttachment>()
                };

                var attachResult = await AddReplayAttachmentAsync(authenticate, communicationModel.DocumentRevision.DocumentId,
                    communicationModel.DocumentRevisionId, questionModel.CommunicationReply, model.Attachments);
                if (!attachResult.Succeeded)
                    return ServiceResultFactory.CreateError<TQQuestionDetailsDto>(null, attachResult.Messages.First().Message);
                else
                    questionModel.CommunicationReply = attachResult.Result;

                communicationModel.CommunicationStatus = DocumentCommunicationStatus.Replyed;

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    await _scmLogAndNotificationService.SetDonedNotificationAsync(authenticate.UserId, authenticate.ContractCode, communicationModel.DocumentTQNCRId.ToString(), NotifEvent.ReplyTQComment);
                    var res1 = await _scmLogAndNotificationService.AddDocumentAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = authenticate.ContractCode,
                        FormCode = communicationModel.CommunicationCode,
                        Message = "",
                        KeyValue = communicationModel.DocumentTQNCRId.ToString(),
                        NotifEvent = NotifEvent.ReplyTQComment,
                        RootKeyValue = communicationModel.DocumentTQNCRId.ToString(),
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,
                        DocumentGroupId = communicationModel.DocumentRevision.Document.DocumentGroupId
                    },
                    communicationModel.DocumentRevision.Document.DocumentGroupId,
                    null);

                    try
                    {
                        BackgroundJob.Enqueue(() => SendEmailOnReplyTQAsync(authenticate, communicationModel));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.StackTrace);
                    }

                    //if (communicationModel.AdderUser != null)
                    //    await SendEmailForAdderUserAsync(authenticate, communicationModel);
                    var res = await ReturnTQQuestionDetailsAsync(communicationId);
                    return ServiceResultFactory.CreateSuccess(res);
                }
                return ServiceResultFactory.CreateError<TQQuestionDetailsDto>(null, MessageId.SaveFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<TQQuestionDetailsDto>(null, exception);
            }
        }

        public async Task<ServiceResult<bool>> SendEmailOnReplyTQAsync(AuthenticateDto authenticate, DocumentTQNCR communicationModel)
        {
            var toRoles = new List<string> {
                SCMRole.TQMng,
                SCMRole.TQObs,
               SCMRole.TQReg,
               SCMRole.TQReply
               };

            var toUsers = await _authenticationServices
                .GetAllUserHasAccessDocumentAsync(authenticate.ContractCode, toRoles, communicationModel.DocumentRevision.Document.DocumentGroupId);
            var tqEmailNotify = await _notifyRepository.Where(a => a.TeamWork.ContractCode == authenticate.ContractCode && a.NotifyType == NotifyManagementType.Email && a.NotifyNumber == (int)EmailNotify.TQ && a.IsActive).Select(a => a.UserId).ToListAsync();
            if (tqEmailNotify != null && tqEmailNotify.Any())
                toUsers = toUsers.Where(a => tqEmailNotify.Contains(a.Id)).ToList();
            else
                toUsers = new List<UserMentionDto>();
            var toUserEmails =(toUsers!=null&&toUsers.Any())? toUsers
                .Where(a => !string.IsNullOrEmpty(a.Email))
                .Select(a => a.Email)
                .ToList():new List<string>();

            if (!toUserEmails.Any())
                return ServiceResultFactory.CreateSuccess(true);

            var attachments = await GenerateTQAttachmentZipAsync(authenticate, communicationModel.DocumentTQNCRId);

            string faMessage = $"در پروژه <span style='direction:ltr'>{authenticate.ContractCode}</span> پاسخ به TQ به شماره <span style='direction:ltr'>{communicationModel.CommunicationCode}</span> مربوط به مدرک <span style='direction:ltr'>{communicationModel.DocumentRevision.Document.DocTitle}</span> به شماره <span style='direction:ltr'>{communicationModel.DocumentRevision.Document.DocNumber}</span> توسط {authenticate.UserFullName} به شرح زیر ثبت گردید. ";
            string enMessage = $"<div style='direction:ltr;text-align:left'>Replying to TQ No. {communicationModel.CommunicationCode} related to document titled {communicationModel.DocumentRevision.Document.DocTitle} - {communicationModel.DocumentRevision.Document.DocNumber} has been registered  by {authenticate.UserFullName} as following. </div>";
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
                Subject = $"Reply to TQ | {communicationModel.CommunicationCode}"
            };
            await _appEmailService.SendAsync(emailRequest, attachments != null ? attachments.Stream : new MemoryStream(), communicationModel.CommunicationCode + ".zip" );

            return ServiceResultFactory.CreateSuccess(true);
        }

        private List<InMemoryFileDto> PreParationAchmentForSendInEmail(DocumentTQNCR communication)
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


        private async Task<TQQuestionDetailsDto> ReturnTQQuestionDetailsAsync(long communicationId)
        {
            try
            {
                var dbQuery = _documentTQNCRRepository
                    .AsNoTracking()
                    .Where(a => a.CommunicationType == CommunicationType.NCR && a.DocumentTQNCRId == communicationId);


                var result = await dbQuery.Select(c => new TQQuestionDetailsDto
                {
                    Subject = c.Subject,
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
                return new TQQuestionDetailsDto();
            }
        }

        private async Task<bool> SendEmailForAdderUserAsync(AuthenticateDto authenticate, DocumentTQNCR communicationModel)
        {
            string url = $"/dashboard/documents/communication?action=TQ";

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

        public async Task<ServiceResult<List<CommunicationAttachmentDto>>> AddCommunicationTQAttachmentAsync(AuthenticateDto authenticate,
          long communicationId, IFormFileCollection files)
        {
            try
            {
                if (files == null || !files.Any())
                    return ServiceResultFactory.CreateError<List<CommunicationAttachmentDto>>(null, MessageId.FileNotFound);

                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<CommunicationAttachmentDto>>(null, MessageId.AccessDenied);

                var dbQuery = _documentTQNCRRepository
                    .AsNoTracking()
                     .Where(a =>
                     a.CommunicationType == CommunicationType.TQ &&
                     a.DocumentTQNCRId == communicationId &&
                     a.DocumentRevision.Document.ContractCode == authenticate.ContractCode);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<List<CommunicationAttachmentDto>>(null, MessageId.EntityDoesNotExist);

                if (permission.DocumentGroupIds.Any() && !dbQuery.Any(a => permission.DocumentGroupIds.Contains(a.DocumentRevision.Document.DocumentGroupId)))
                    return ServiceResultFactory.CreateError<List<CommunicationAttachmentDto>>(null, MessageId.AccessDenied);

                var communicationModel = await dbQuery.Select(c => new
                {
                    communicationId = c.DocumentTQNCRId,
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
                        DocumentTQNCRId = communicationId,
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

        public async Task<ServiceResult<bool>> DeleteCommunicationTQAttachmentAsync(AuthenticateDto authenticate, long communicationId, string fileSrc)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery = _documentTQNCRRepository
                 .AsNoTracking()
                  .Where(a =>
                  a.CommunicationType == CommunicationType.TQ &&
                  a.DocumentTQNCRId == communicationId &&
                  a.DocumentRevision.Document.ContractCode == authenticate.ContractCode);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (permission.DocumentGroupIds.Any() && !dbQuery.Any(a => permission.DocumentGroupIds.Contains(a.DocumentRevision.Document.DocumentGroupId)))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var communicationModel = await dbQuery.Select(c => new
                {
                    communicationId = c.DocumentTQNCRId,
                    revId = c.DocumentRevisionId,
                    docId = c.DocumentRevision.DocumentId
                }).FirstOrDefaultAsync();

                var dbQueryAttch = _communicationAttachmentRepository
                     .Where(a => !a.IsDeleted &&
                     a.DocumentTQNCRId == communicationId &&
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

        public async Task<DownloadFileDto> DownloadTQFileAsync(AuthenticateDto authenticate, long communicationId, string fileSrc)
        {
            try
            {
                //var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                //if (!permission.HasPermission)
                //    return null;

                var info = await _documentTQNCRRepository
                    .AsNoTracking()
                    .Where(a =>
                    a.CommunicationType == CommunicationType.TQ &&
                    a.DocumentTQNCRId == communicationId &&
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
                return await _fileHelper.DownloadDocument(fileSrc, ServiceSetting.UploadFilePathRevisionCommunication(authenticate.ContractCode, info.docId, info.revisionId),attachment.FileName);
            }
            catch (Exception exception)
            {
                return null;
            }
        }

        private async Task<ServiceResult<CommunicationReply>> AddReplayAttachmentAsync(AuthenticateDto authenticate,
           long docId, long revId, CommunicationReply replyModel, List<AddRevisionAttachmentDto> model)
        {
            if (model != null && model.Any())
            {
                foreach (var item in model)
                {
                    var UploadedFile = await _fileHelper
                        .SaveDocumentFromTemp(item.FileSrc, ServiceSetting.UploadFilePathRevisionCommunication(authenticate.ContractCode, docId, revId));

                    if (UploadedFile == null)
                        return ServiceResultFactory.CreateError<CommunicationReply>(null, MessageId.UploudFailed);

                    _fileHelper.DeleteDocumentFromTemp(item.FileSrc);

                    replyModel.Attachments.Add(new CommunicationAttachment
                    {
                        FileSrc = item.FileSrc,
                        FileName = item.FileName,
                        FileType = UploadedFile.FileType,
                        FileSize = UploadedFile.FileSize
                    });
                }
            }
            return ServiceResultFactory.CreateSuccess(replyModel);
        }


        public async Task<DownloadFileDto> GenerateTQAttachmentZipAsync(AuthenticateDto authenticate, long communicationId)
        {
            try
            {
                //var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                //if (!permission.HasPermission)
                //    return null;


                var dbQuery = _communicationQuestionRepository
                    .AsNoTracking()
                    .Where(a => a.DocumentTQNCRId == communicationId &&
                    a.DocumentTQNCR.CommunicationType == CommunicationType.TQ &&
                    a.DocumentTQNCR.DocumentRevision.Document.ContractCode == authenticate.ContractCode);

                //if (permission.DocumentGroupIds.Any() && !dbQuery.Any(c => permission.DocumentGroupIds.Contains(c.DocumentTQNCR.DocumentRevision.Document.DocumentGroupId)))
                //    return null;

                var tqModel = await dbQuery.Select(a => new TQDetailsForCreatePdfDto
                {
                    TQCode = a.DocumentTQNCR.CommunicationCode,
                    DocNumber = a.DocumentTQNCR.DocumentRevision.Document.DocNumber,
                    DocumentId = a.DocumentTQNCR.DocumentRevision.DocumentId,
                    RevisionId = a.DocumentTQNCR.DocumentRevision.DocumentRevisionId,
                    RevisionCode = a.DocumentTQNCR.DocumentRevision.DocumentRevisionCode,
                    ProjectDescription = a.DocumentTQNCR.DocumentRevision.Document.Contract.Description,
                    TQSubject = a.DocumentTQNCR.Subject,
                    QuestionDescription = a.Description,
                    RegisterQuestionDate = a.CreatedDate,
                    RegisterQuestionUser = a.AdderUser != null ? a.AdderUser.FullName : "",
                    IsReplyed = a.IsReplyed,
                    ReplyDescription = a.IsReplyed ? a.CommunicationReply.Description : "",
                    RegisterReplyDate = a.IsReplyed ? a.CommunicationReply.CreatedDate : (DateTime?)null,
                    RegisterReplyUser = a.IsReplyed && a.CommunicationReply.AdderUser != null ? a.CommunicationReply.AdderUser.FullName : "",
                    CompanyIssue = a.DocumentTQNCR.CompanyIssue,
                    CustomerLogo = a.DocumentTQNCR.DocumentRevision.Document.Contract.Customer.Logo,
                    CompanyLogo = a.DocumentTQNCR.Customer != null ? a.DocumentTQNCR.Customer.Logo : a.DocumentTQNCR.Supplier != null ? a.DocumentTQNCR.Supplier.Logo : a.DocumentTQNCR.Consultant != null ? a.DocumentTQNCR.Consultant.Logo : "",
                    CompanyIssueName = a.DocumentTQNCR.CompanyIssue == CompanyIssue.Customer && a.DocumentTQNCR.Customer != null ? a.DocumentTQNCR.Customer.Name :
                    a.DocumentTQNCR.CompanyIssue == CompanyIssue.Supplier && a.DocumentTQNCR.Supplier != null ? a.DocumentTQNCR.Supplier.Name : a.DocumentTQNCR.CompanyIssue == CompanyIssue.Consultant && a.DocumentTQNCR.Consultant != null ? a.DocumentTQNCR.Consultant.Name : "داخلی",
                }).FirstOrDefaultAsync();
                var attachments = await dbQuery.Select(a => new CommentTQNCRAttachmentDto
                {
                    CommentAttachment = a.Attachments.Where(b => !b.IsDeleted).Select(b => new BasicAttachmentDownloadDto
                    {
                        FileName = b.FileName,
                        FileSrc = b.FileSrc
                    }).ToList(),
                    ReplyAttachment = a.CommunicationReply.Attachments.Where(b => !b.IsDeleted).Select(b => new BasicAttachmentDownloadDto
                    {
                        FileName = b.FileName,
                        FileSrc = b.FileSrc
                    }).ToList()
                }).FirstOrDefaultAsync();
                if (tqModel == null)
                    return null;

                var selectedTemplate = new PDFTemplate();
                var templates = _pdfTemplateRepository
                    .Where(a => (a.ContractCode == null || a.ContractCode == authenticate.ContractCode) &&
                     a.PDFTemplateType == PDFTemplateType.TQ)
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
                
                tqModel.CustomerLogo = _fileHelper.FileReadSrc(tqModel.CustomerLogo, ServiceSetting.UploadImagesPath.LogoSmall);

                tqModel.CompanyLogo = _fileHelper.FileReadSrc(_appSettings.CompanyLogo);
                tqModel.ReplyerCompany=(tqModel.IsReplyed)? _appSettings.CompanyNameFA:"";
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
                var stringHeader = TQPdfHeaderTemplate(tqModel, selectedTemplate);
                converter.Header.Height = 150;
                converter.Header.DisplayOnFirstPage = true;
                converter.Header.DisplayOnEvenPages = true;
                converter.Header.DisplayOnOddPages = true;
                converter.Options.DisplayHeader = true;
                PdfHtmlSection headerHtml = new PdfHtmlSection(stringHeader, "");
                headerHtml.AutoFitHeight = HtmlToPdfPageFitMode.AutoFit;
                converter.Header.Add(headerHtml);

                var htmlBody = TQPdfBodyTemplate(tqModel, selectedTemplate);


                PdfDocument doc = converter.ConvertHtmlString(htmlBody);

                var gggg = doc.Save();

                var result = new DownloadFileDto
                {
                    ArchiveFile = gggg,
                    ContentType = "application/pdf",
                    FileName = tqModel.DocNumber + "-" + tqModel.TQCode + ".pdf",
                };
                MemoryStream zipStream = new MemoryStream();

                using (ZipArchive zipFile = new ZipArchive(zipStream, ZipArchiveMode.Update, true))
                {

                    var zipPdfEntry = zipFile.CreateEntry(tqModel.TQCode + ".pdf");

                    //Get the stream of the attachment
                    using (var originalFileStream = new MemoryStream(result.ArchiveFile))
                    {
                        using (var zipEntryStream = zipPdfEntry.Open())
                        {
                            //Copy the attachment stream to the zip entry stream
                            await originalFileStream.CopyToAsync(zipEntryStream);
                        }
                    }
                    
                    await AddAttachmentToZipFile(attachments, zipFile, authenticate.ContractCode, tqModel.DocumentId, tqModel.RevisionId);
                }
                zipStream.Seek(0, SeekOrigin.Begin);
                var finalResult = new DownloadFileDto
                {
                    Stream = zipStream,
                    ContentType = "application/zip",
                    FileName = tqModel.DocNumber + "-" + tqModel.TQCode + ".zip",
                };
                return finalResult;



            }
            catch (Exception exception)
            {
                throw;
            }
        }

        private string TQPdfHeaderTemplate(TQDetailsForCreatePdfDto tqDetails, PDFTemplate template)
        {
            var html = new StringBuilder();

            html.AppendFormat(template.Section1,
            tqDetails.CustomerLogo,
            tqDetails.CompanyLogo,
            tqDetails.ProjectDescription,
            tqDetails.TQCode,
            tqDetails.DocNumber,
            tqDetails.RevisionCode);

            return html.ToString();
        }

        private string TQPdfBodyTemplate(TQDetailsForCreatePdfDto tqDetails, PDFTemplate template)
        {
            var html = new StringBuilder();

            html.AppendFormat(template.Section2,
                                tqDetails.TQSubject,
                                tqDetails.QuestionDescription,
                                tqDetails.CompanyIssueName,
                                tqDetails.RegisterQuestionDate.ToPersianDateString(),
                                tqDetails.ReplyDescription,
                                tqDetails.ReplyerCompany,
                                tqDetails.RegisterReplyDate.ToPersianDateString());

            return html.ToString();
        }
        private async Task AddAttachmentToZipFile(CommentTQNCRAttachmentDto attachments,ZipArchive zipFile,string contractCode,long documentId,long revisionId)
        {
            foreach (var item in attachments.CommentAttachment)
            {
                var file = await _fileHelper.DownloadDocument(item.FileSrc, ServiceSetting.UploadFilePathRevisionCommunication(contractCode, documentId, revisionId));
                if (file != null)
                {
                    var zipCommentEntry = zipFile.CreateEntry("TQ\\" + item.FileName);
                    using (var zipEntryStream = zipCommentEntry.Open())
                    {

                        await file.Stream.CopyToAsync(zipEntryStream);
                    }
                }
            }
            foreach (var item in attachments.ReplyAttachment)
            {
                var file = await _fileHelper.DownloadDocument(item.FileSrc, ServiceSetting.UploadFilePathRevisionCommunication(contractCode, documentId,revisionId));
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
