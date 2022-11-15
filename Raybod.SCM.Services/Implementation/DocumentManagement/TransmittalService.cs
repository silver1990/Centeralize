
using Exon.TheWeb.Service.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Raybod.SCM.DataAccess.Core;
using Raybod.SCM.DataAccess.Extention;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Audit;
using Raybod.SCM.DataTransferObject.Document;
using Raybod.SCM.DataTransferObject.Transmittal;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Model;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Services.Utilitys.MailService;
using Raybod.SCM.Utility.Extention;
using Raybod.SCM.Utility.FileHelper;
using Raybod.SCM.Utility.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DinkToPdf;
using DinkToPdf.Contracts;
using System.Threading.Tasks;
using Raybod.SCM.Domain.Struct;
using System.Threading;
using Raybod.SCM.DataTransferObject.Email;
using Raybod.SCM.DataTransferObject._PanelDocument.Communication.Comment;
using Hangfire;
using Microsoft.Data.SqlClient;
using Dapper;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.IO.Compression;
using Raybod.SCM.DataTransferObject.User;
using Microsoft.AspNetCore.Http;

namespace Raybod.SCM.Services.Implementation
{
    public class TransmittalService : ITransmittalService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;
        private readonly IAppEmailService _appEmailService;
        private readonly IContractFormConfigService _formConfigService;
        private readonly DbSet<Document> _documentRepository;
        private readonly DbSet<User> _userRepository;
        private readonly IConverter _converter;
        private readonly DbSet<Customer> _customerRepository;
        private readonly DbSet<UserNotify> _notifyRepository;
        private readonly DbSet<Consultant> _consultantRepository;
        private readonly DbSet<Supplier> _supplierRepository;
        private readonly DbSet<PDFTemplate> _pdfTemplateRepository;
        private readonly DbSet<CompanyUser> _companyUserRepository;
        private readonly DbSet<DocumentRevision> _documentRevisionRepository;
        private readonly DbSet<RevisionAttachment> _revisionAttachmentRepository;
        private readonly DbSet<Transmittal> _transmittalRepository;
        private readonly DbSet<TransmittalRevision> _transmittalRevisionRepository;
        private readonly ITeamWorkAuthenticationService _authenticationServices;
        private readonly ISCMLogAndNotificationService _scmLogAndNotificationService;
        private readonly Utilitys.FileHelper _fileHelper;
        private readonly CompanyConfig _appSettings;
        private readonly IViewRenderService _viewRenderService;
        private readonly IConfiguration _configuration;
        private string _contentRootPath;
        private readonly DbSet<DocumentCommunication> _documentCommunicationRepository;
        public TransmittalService(IUnitOfWork unitOfWork,
            IWebHostEnvironment hostingEnvironmentRoot,
            IAppEmailService appEmailService,
            IOptions<CompanyAppSettingsDto> appSettings,
            ITeamWorkAuthenticationService authenticationServices,
            ISCMLogAndNotificationService scmLogAndNotificationService,
            IFileService fileService,
            IConverter converter,
            IHttpContextAccessor httpContextAccessor,
            IContractFormConfigService formConfigService,
            IViewRenderService viewRenderService, IConfiguration configuration
            )
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
            _converter = converter;
            _appEmailService = appEmailService;
            _authenticationServices = authenticationServices;
            _formConfigService = formConfigService;
            _scmLogAndNotificationService = scmLogAndNotificationService;
            _documentRepository = _unitOfWork.Set<Document>();
            _customerRepository = _unitOfWork.Set<Customer>();
            _consultantRepository = _unitOfWork.Set<Consultant>();
            _supplierRepository = _unitOfWork.Set<Supplier>();
            _pdfTemplateRepository = _unitOfWork.Set<PDFTemplate>();
            _companyUserRepository = _unitOfWork.Set<CompanyUser>();
            _notifyRepository = _unitOfWork.Set<UserNotify>();
            _userRepository = _unitOfWork.Set<User>();
            _documentRevisionRepository = _unitOfWork.Set<DocumentRevision>();
            _revisionAttachmentRepository = _unitOfWork.Set<RevisionAttachment>();
            _transmittalRepository = _unitOfWork.Set<Transmittal>();
            _transmittalRevisionRepository = _unitOfWork.Set<TransmittalRevision>();
            _documentCommunicationRepository = _unitOfWork.Set<DocumentCommunication>();
            _fileHelper = new Utilitys.FileHelper(hostingEnvironmentRoot);
            _viewRenderService = viewRenderService;
            _configuration = configuration;
            _contentRootPath = hostingEnvironmentRoot.ContentRootPath;
            httpContextAccessor.HttpContext.Request.Headers.TryGetValue("companyCode", out var CompanyCode);
            _appSettings = appSettings.Value.CompanyConfig.First(a => a.CompanyCode == CompanyCode);
        }

        public async Task<ServiceResult<List<TransmittalCompanyListDto>>> GetTransmittalCompanyListAsync(AuthenticateDto authenticate)
        {
            try
            {
                //var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                //if (!permission.HasPermission)
                //    return ServiceResultFactory.CreateError<List<TransmittalCompanyListDto>>(null, MessageId.AccessDenied);

                var result = new List<TransmittalCompanyListDto>();
                var consultant = await _consultantRepository
                   .AsNoTracking()
                   .Where(a => !a.IsDeleted && a.ConsultantContracts.Any(c => c.ContractCode == authenticate.ContractCode))
                   .Select(c => new TransmittalCompanyListDto
                   {
                       Id = c.Id,
                       Name = c.Name,
                       Type = TransmittalType.Consultant
                   }).FirstOrDefaultAsync();

                if (consultant != null)
                    result.Add(consultant);
                var customerInfo = await _customerRepository
                   .AsNoTracking()
                   .Where(a => !a.IsDeleted && a.CustomerContracts.Any(c => c.ContractCode == authenticate.ContractCode))
                   .Select(c => new TransmittalCompanyListDto
                   {
                       Id = c.Id,
                       Name = c.Name,
                       Type = TransmittalType.Customer
                   }).FirstOrDefaultAsync();

                if (customerInfo != null)
                    result.Add(customerInfo);
                //return ServiceResultFactory.CreateError<List<TransmittalCompanyListDto>>(null, MessageId.CustomerNotFound);




                result.Add(new TransmittalCompanyListDto
                {
                    Id = 0,
                    Name = (authenticate.language=="en")?"Internal":"داخلی",
                    Type = TransmittalType.Internal
                });

                var supplliers = await _supplierRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted)
                    .Select(c => new TransmittalCompanyListDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Type = TransmittalType.Supplier
                    }).ToListAsync();

                result.AddRange(supplliers);

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<TransmittalCompanyListDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<List<TransmittalUserListDto>>> GetTransmittalSupplierUserAsync(AuthenticateDto authenticate, int supplierId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<TransmittalUserListDto>>(null, MessageId.AccessDenied);

                var supplliers = await _companyUserRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.SupplierId == supplierId)
                    .Select(c => new TransmittalUserListDto
                    {
                        Id = c.CompanyUserId,
                        FullName = c.FirstName + " " + c.LastName,
                        Email = c.Email
                    }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(supplliers);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<TransmittalUserListDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<List<TransmittalUserListDto>>> GetTransmittalConsultantUserAsync(AuthenticateDto authenticate, int consultantId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<TransmittalUserListDto>>(null, MessageId.AccessDenied);

                var consultant = await _companyUserRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.ConsultantId == consultantId)
                    .Select(c => new TransmittalUserListDto
                    {
                        Id = c.CompanyUserId,
                        FullName = c.FirstName + " " + c.LastName,
                        Email = c.Email
                    }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(consultant);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<TransmittalUserListDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<List<TransmittalUserListDto>>> GetTransmittalCustomerUserAsync(AuthenticateDto authenticate, int customerId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<TransmittalUserListDto>>(null, MessageId.AccessDenied);

                var customers = await _companyUserRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.CustomerId == customerId)
                    .Select(c => new TransmittalUserListDto
                    {
                        Id = c.CompanyUserId,
                        FullName = c.FirstName + " " + c.LastName,
                        Email = c.Email
                    }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(customers);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<TransmittalUserListDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<List<TransmittalUserListDto>>> GetTransmittalInternalUserAsync(AuthenticateDto authenticate)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<TransmittalUserListDto>>(null, MessageId.AccessDenied);

                var customers = await _userRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.IsActive && a.UserType != (int)UserStatus.CustomerUser && a.UserType != (int)UserStatus.ConsultantUser)
                    .Select(c => new TransmittalUserListDto
                    {
                        Id = c.Id,
                        FullName = c.FirstName + " " + c.LastName,
                        Email = c.Email
                    }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(customers);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<TransmittalUserListDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<List<PenddingTransmittalListDto>>> GetPendingRevisionGroupByDocumentGroupIdAsync(AuthenticateDto authenticate)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<PenddingTransmittalListDto>>(null, MessageId.AccessDenied);

                var dbQuery = _documentRevisionRepository
                     .AsNoTracking()
                     .Where(a => !a.IsDeleted && a.Document.IsRequiredTransmittal &&
                     a.Document.IsActive &&
                     a.RevisionStatus == RevisionStatus.Confirmed &&
                      (a.TransmittalRevisions == null || !a.TransmittalRevisions.Any()) &&
                     a.Document.ContractCode == authenticate.ContractCode);

                if (permission.DocumentGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.DocumentGroupIds.Contains(a.Document.DocumentGroupId));

                var revisionList = await dbQuery.Select(a => new
                {
                    DocumentId = a.DocumentId,
                    DocumentGroupId = a.Document.DocumentGroupId,
                    RevisionCode = a.DocumentRevisionCode,
                    DocTitle = a.Document.DocTitle,
                    DocumentGroupCode = a.Document.DocumentGroup.DocumentGroupCode,
                    DocumentGroupTitle = a.Document.DocumentGroup.Title,
                    DocNumber = a.Document.DocNumber,
                    DocClass = a.Document.DocClass,
                    RevisionId = a.DocumentRevisionId,
                    PageNumber = a.RevisionPageNumber,
                    PageSize = a.RevisionPageSize

                }).ToListAsync();

                var totalCount = revisionList.Count();

                if (revisionList == null || revisionList.Count() <= 0)
                    return ServiceResultFactory.CreateSuccess(new List<PenddingTransmittalListDto>());

                var result = revisionList.GroupBy(a => a.DocumentGroupId)
                    .Select(c => new PenddingTransmittalListDto
                    {
                        DocumentGroupCode = c.First().DocumentGroupCode,
                        DocumentGroupTitle = c.First().DocumentGroupTitle,
                        DocumentGroupId = c.Key,
                        Revisions = c.Select(v => new PendingTransmitalRevisionInfoDo
                        {
                            DocTitle = v.DocTitle,
                            RevisionCode = v.RevisionCode,
                            DocumentId = v.DocumentId,
                            DocClass = v.DocClass,
                            DocNumber = v.DocNumber,
                            RevisionId = v.RevisionId,
                            PageNumber = v.PageNumber,
                            PageSize = v.PageSize
                        }).ToList()
                    }).ToList();

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<PenddingTransmittalListDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<List<PendingTransmitalRevisionInfoDo>>> GetRevisionForAddTransmittalAsync(AuthenticateDto authenticate, int documentGroupId, DocRevisionQueryDto query)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<PendingTransmitalRevisionInfoDo>>(null, MessageId.AccessDenied);

                var dbQuery = _documentRevisionRepository
                     .AsNoTracking()
                     .Where(a => !a.IsDeleted &&
                     a.RevisionStatus >= RevisionStatus.Confirmed &&
                     a.IsLastConfirmRevision &&
                     !a.Document.IsDeleted &&
                     a.Document.IsActive &&
                     a.Document.DocumentGroupId == documentGroupId &&
                     a.Document.ContractCode == authenticate.ContractCode);

                if (permission.DocumentGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.DocumentGroupIds.Contains(a.Document.DocumentGroupId));

                var totalCount = dbQuery.Count();

                var columnsMap = new Dictionary<string, Expression<Func<DocumentRevision, object>>>
                {
                    ["DocumentRevisionId"] = v => v.DocumentRevisionId
                };

                dbQuery = dbQuery.ApplayOrdering(query, columnsMap).ApplayPageing(query);

                var result = await dbQuery.Select(a => new PendingTransmitalRevisionInfoDo
                {
                    RevisionCode = a.DocumentRevisionCode,
                    DocTitle = a.Document.DocTitle,
                    DocNumber = a.Document.DocNumber,
                    DocClass = a.Document.DocClass,
                    DocumentId = a.DocumentId,
                    RevisionId = a.DocumentRevisionId,
                    PageNumber = a.RevisionPageNumber,
                    PageSize = a.RevisionPageSize,
                    RevisionAttachments = a.RevisionAttachments.Where(a => !a.IsDeleted && a.RevisionAttachmentType == RevisionAttachmentType.Final)
                    .Select(c => new RevisionAttachmentDto
                    {
                        AttachType = c.RevisionAttachmentType,
                        FileType = c.FileType,
                        FileName = c.FileName,
                        FileSize = c.FileSize,
                        FileSrc = c.FileSrc,
                        RevisionAttachmentId = c.RevisionAttachmentId
                    }).ToList()
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<PendingTransmitalRevisionInfoDo>>(null, exception);
            }
        }

        public async Task<ServiceResult<PendingTransmittalDetailsDto>> GetPendingTransmittalDetailsAsync(AuthenticateDto authenticate, int documentGroupId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<PendingTransmittalDetailsDto>(null, MessageId.AccessDenied);

                if (permission.DocumentGroupIds.Any() && !permission.DocumentGroupIds.Contains(documentGroupId))
                    return ServiceResultFactory.CreateError<PendingTransmittalDetailsDto>(null, MessageId.AccessDenied);

                var dbQuery = _documentRevisionRepository
                     .AsNoTracking()
                     .Where(a => !a.IsDeleted &&
                     a.RevisionStatus == RevisionStatus.Confirmed &&
                     a.Document.IsRequiredTransmittal &&
                     (a.TransmittalRevisions == null || !a.TransmittalRevisions.Any()) &&
                     !a.Document.IsDeleted &&
                     a.Document.IsActive &&
                     a.Document.DocumentGroupId == documentGroupId &&
                     a.Document.ContractCode == authenticate.ContractCode);

                var revisionList = await dbQuery.Select(a => new PendingTransmitalRevisionInfoDo
                {
                    RevisionCode = a.DocumentRevisionCode,
                    DocTitle = a.Document.DocTitle,
                    DocNumber = a.Document.DocNumber,
                    DocClass = a.Document.DocClass,
                    DocumentId = a.DocumentId,
                    RevisionId = a.DocumentRevisionId,
                    PageNumber = a.RevisionPageNumber,
                    PageSize = a.RevisionPageSize,
                    RevisionAttachments = a.RevisionAttachments.Where(a => !a.IsDeleted && a.RevisionAttachmentType == RevisionAttachmentType.Final)
                    .Select(c => new RevisionAttachmentDto
                    {
                        AttachType = c.RevisionAttachmentType,
                        FileType = c.FileType,
                        FileName = c.FileName,
                        FileSize = c.FileSize,
                        FileSrc = c.FileSrc,
                        RevisionAttachmentId = c.RevisionAttachmentId
                    }).ToList()
                }).ToListAsync();

                if (revisionList == null || revisionList.Count() == 0)
                    return ServiceResultFactory.CreateError<PendingTransmittalDetailsDto>(null, MessageId.EntityDoesNotExist);

                var result = await _documentRepository
                    .Where(a => !a.IsDeleted && a.IsActive && a.ContractCode == authenticate.ContractCode && a.DocumentGroupId == documentGroupId)
                    .Select(c => new PendingTransmittalDetailsDto
                    {
                        DocumentGroupId = c.DocumentGroupId,
                        DocumentGroupCode = c.DocumentGroup.DocumentGroupCode,
                        DocumentGroupTitle = c.DocumentGroup.Title,
                        ContractDescription = c.Contract.Description,
                        Revisions = new List<PendingTransmitalRevisionInfoDo>()
                    }).FirstOrDefaultAsync();

                if (result == null)
                    return ServiceResultFactory.CreateError<PendingTransmittalDetailsDto>(null, MessageId.EntityDoesNotExist);

                result.Revisions = revisionList;

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<PendingTransmittalDetailsDto>(null, exception);
            }
        }

        public async Task<ServiceResult<bool>> AddTransmittalByPendingRevisionAsync(AuthenticateDto authenticate, int documentGroupId, AddTransmittalDto model)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                if (permission.DocumentGroupIds.Any() && !permission.DocumentGroupIds.Contains(documentGroupId))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                if (model == null || model.Revisions == null || !model.Revisions.Any())
                    return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                if (!EnumHelper.ValidateItem(model.TransmittalType))
                    return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                if (model.Revisions.Any(a => !EnumHelper.ValidateItem(a.POI)))
                    return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                var revisionIds = model.Revisions.Select(c => c.DocumentRevisionId).ToList();

                var dbQuery = _documentRevisionRepository
                  .Where(a => !a.IsDeleted &&
                  a.RevisionStatus == RevisionStatus.Confirmed &&
                   (a.TransmittalRevisions == null || !a.TransmittalRevisions.Any()) &&
                  !a.Document.IsDeleted &&
                  a.Document.IsActive &&
                  revisionIds.Contains(a.DocumentRevisionId) &&
                  a.Document.DocumentGroupId == documentGroupId &&
                  a.Document.ContractCode == authenticate.ContractCode);

                var selectedRevisionModel = await dbQuery
                    .Include(c => c.Document)
                    .Include(v => v.RevisionAttachments)
                    .ToListAsync();

                if (selectedRevisionModel.Count() != model.Revisions.Count())
                    return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                var documentDetails = await _documentRepository
                    .Where(a => !a.IsDeleted && a.IsActive && a.ContractCode == authenticate.ContractCode && a.DocumentGroupId == documentGroupId)
                    .Select(c => new TransmitallDetailsForCreatePdfDto
                    {
                        DocumentGroupId = c.DocumentGroupId,
                        DocumentGroupCode = c.DocumentGroup.DocumentGroupCode,
                        DocumentGroupTitle = c.DocumentGroup.Title,
                        ContractDescription = c.Contract.Description,
                        CustomerId = c.Contract.CustomerId ?? 0,
                        CustomerName = c.Contract.Customer.Name,
                        CustomerEmail = c.Contract.Customer.Email,
                        CustomerLogo = c.Contract.Customer.Logo,
                        Revisions = new List<DocumentRevision>()
                    }).FirstOrDefaultAsync();

                if (documentDetails == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                documentDetails.Revisions = selectedRevisionModel;

                var transmittalModel = new Transmittal
                {
                    ContractCode = authenticate.ContractCode,
                    Description = model.Description,
                    DocumentGroupId = documentGroupId,
                    CreatedDate = DateTime.UtcNow,
                    TransmittalType = model.TransmittalType,
                    Attachments = new List<RevisionAttachment>(),
                    TransmittalRevisions = new List<TransmittalRevision>()
                };

                transmittalModel.TransmittalRevisions = model.Revisions.Select(c => new TransmittalRevision
                {
                    DocumentRevisionId = c.DocumentRevisionId,
                    POI = c.POI
                }).ToList();

                foreach (var item in selectedRevisionModel)
                {
                    var transmittalStatus = transmittalModel.TransmittalRevisions.First(c => c.DocumentRevisionId == item.DocumentRevisionId);
                    switch (transmittalStatus.POI)
                    {
                        case POI.IFA:
                            item.RevisionStatus = RevisionStatus.TransmittalIFA;
                            break;
                        case POI.IFI:
                            item.RevisionStatus = RevisionStatus.TransmittalIFI;
                            break;
                        case POI.IFC:
                            item.RevisionStatus = RevisionStatus.TransmittalIFC;
                            break;
                        case POI.ASB:
                            item.RevisionStatus = RevisionStatus.TransmittalASB;
                            break;
                    }
                }

                var addReciverResult = await AddTransmittalReceiverUserAsync(transmittalModel, documentDetails.CustomerId, model);
                if (!addReciverResult.Succeeded)
                    return ServiceResultFactory.CreateError(false, addReciverResult.Messages.First().Message);

                transmittalModel = addReciverResult.Result;

                // generate form code
                var count = await _transmittalRepository.CountAsync(a => a.ContractCode == authenticate.ContractCode);
                var codeRes = await _formConfigService.GenerateFormCodeAsync(authenticate.ContractCode, FormName.Transmittal, count);
                if (!codeRes.Succeeded)
                    return ServiceResultFactory.CreateError(false, codeRes.Messages.First().Message);
                var transmittalNumber = codeRes.Result;
                transmittalModel.TransmittalNumber = transmittalNumber;

                var pdfResult = CreateAndSaveTransmittalsPdfAsync(transmittalModel, documentDetails,authenticate.UserFullName);
                if (!pdfResult.Succeeded)
                    return ServiceResultFactory.CreateError(false, pdfResult.Messages.First().Message);



                _transmittalRepository.Add(transmittalModel);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    if (!string.IsNullOrEmpty(transmittalModel.Email))
                    {
                        try
                        {
                            BackgroundJob.Enqueue(() => SendEmailToReceiverUserAsync(authenticate, transmittalModel, documentDetails, pdfResult.Result));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.StackTrace);
                        }


                    }

                    var keyValues = model.Revisions.Select(c => c.DocumentRevisionId.ToString()).ToList();
                    await _scmLogAndNotificationService
                    .SetDonedNotificationAsync(authenticate.UserId, authenticate.ContractCode, keyValues, NotifEvent.AddTransmittal);

                    var company = transmittalModel.TransmittalType == TransmittalType.Internal ? "ترانسمیتال داخلی" :
                        transmittalModel.TransmittalType == TransmittalType.Customer ? documentDetails.CustomerName : transmittalModel.TransmittalType == TransmittalType.Supplier ? transmittalModel.Supplier.Name : transmittalModel.Consultant.Name;
                    var logModels = selectedRevisionModel.Select(c => new AddAuditLogDto
                    {
                        ContractCode = authenticate.ContractCode,
                        Description = c.Document.DocTitle,
                        FormCode = c.DocumentRevisionCode,
                        Message = company,
                        KeyValue = transmittalModel.TransmittalId.ToString(),
                        NotifEvent = NotifEvent.AddTransmittal,
                        RootKeyValue = c.DocumentRevisionId.ToString(),
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,
                        DocumentGroupId = documentDetails.DocumentGroupId
                    }).ToList();
                    if (!String.IsNullOrEmpty(model.Email) && !String.IsNullOrWhiteSpace(model.Email) && (transmittalModel.TransmittalType == TransmittalType.Consultant || transmittalModel.TransmittalType == TransmittalType.Customer))
                    {
                        var user = await _userRepository.FirstOrDefaultAsync(d => d.Email == model.Email && !d.IsDeleted && d.IsActive);
                        if (user != null)
                        {
                            var taskModel = selectedRevisionModel.Where(a => a.RevisionStatus == RevisionStatus.TransmittalIFA && !_documentCommunicationRepository.Any(b => b.DocumentRevisionId == a.DocumentRevisionId)).Select(c => new AddTaskNotificationDto
                            {
                                ContractCode = authenticate.ContractCode,
                                Description = c.Document.DocTitle,
                                Message = transmittalModel.TransmittalNumber,
                                FormCode = c.DocumentRevisionCode,
                                KeyValue = c.DocumentRevisionId.ToString(),
                                NotifEvent = NotifEvent.TransmittalPendingForComment,
                                RootKeyValue = c.DocumentRevisionId.ToString(),
                                PerformerUserId = authenticate.UserId,
                                PerformerUserFullName = authenticate.UserFullName,
                                Users = new List<int> { user.Id }
                            }).ToList();
                            await _scmLogAndNotificationService.AddScmAuditLogAsync(logModels, taskModel, false);
                        }
                        else
                        {
                            var res1 = await _scmLogAndNotificationService.AddScmAuditLogAsync(logModels, null);
                        }
                    }
                    else
                    {
                        var res1 = await _scmLogAndNotificationService.AddScmAuditLogAsync(logModels, null);
                    }


                    return ServiceResultFactory.CreateSuccess(true);
                }

                return ServiceResultFactory.CreateSuccess(true);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<bool>> AddTransmittalAsync(AuthenticateDto authenticate, int documentGroupId, AddTransmittalDto model)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                if (permission.DocumentGroupIds.Any() && !permission.DocumentGroupIds.Contains(documentGroupId))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                if (model == null || model.Revisions == null || !model.Revisions.Any())
                    return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                if (!EnumHelper.ValidateItem(model.TransmittalType))
                    return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                if (model.Revisions.Any(a => !EnumHelper.ValidateItem(a.POI)))
                    return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                var revisionIds = model.Revisions.Select(a => a.DocumentRevisionId).ToList();
                var dbQuery = _documentRevisionRepository
                  .Where(a => !a.IsDeleted &&
                  a.RevisionStatus >= RevisionStatus.Confirmed &&
                  a.IsLastConfirmRevision &&
                  !a.Document.IsDeleted &&
                  a.Document.IsActive &&
                  revisionIds.Contains(a.DocumentRevisionId) &&
                  a.Document.DocumentGroupId == documentGroupId &&
                  a.Document.ContractCode == authenticate.ContractCode);

                var selectedRevisionModel = await dbQuery
                    .Include(c => c.Document)
                    .Include(v => v.RevisionAttachments)
                    .ToListAsync();

                if (selectedRevisionModel.Count() != model.Revisions.Count())
                    return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                var documentDetails = await _documentRepository
                    .Where(a => !a.IsDeleted && a.IsActive && a.ContractCode == authenticate.ContractCode && a.DocumentGroupId == documentGroupId)
                    .Select(c => new TransmitallDetailsForCreatePdfDto
                    {
                        DocumentGroupId = c.DocumentGroupId,
                        DocumentGroupCode = c.DocumentGroup.DocumentGroupCode,
                        DocumentGroupTitle = c.DocumentGroup.Title,
                        ContractDescription = c.Contract.Description,
                        CustomerId = c.Contract.CustomerId ?? 0,
                        CustomerName = c.Contract.Customer.Name,
                        CustomerEmail = c.Contract.Customer.Email,
                        CustomerLogo = c.Contract.Customer.Logo,
                        Revisions = new List<DocumentRevision>()
                    }).FirstOrDefaultAsync();

                if (documentDetails == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                documentDetails.Revisions = selectedRevisionModel;

                var transmittalModel = new Transmittal
                {
                    ContractCode = authenticate.ContractCode,
                    Description = model.Description,
                    DocumentGroupId = documentGroupId,
                    CreatedDate = DateTime.UtcNow,
                    TransmittalType = model.TransmittalType,
                    //SupplierId= model.TransmittalType==TransmittalType.Supplier?model.SupplierId:null,
                    //ConsultantId= model.TransmittalType==TransmittalType.Consultant?model.ConsultantId:null,
                    Attachments = new List<RevisionAttachment>(),
                    TransmittalRevisions = new List<TransmittalRevision>()
                };

                transmittalModel.TransmittalRevisions = model.Revisions.Select(c => new TransmittalRevision
                {
                    DocumentRevisionId = c.DocumentRevisionId,
                    POI = c.POI
                }).ToList();

                //if (transmittalModel.TransmittalType == TransmittalType.Customer)
                //{
                foreach (var item in selectedRevisionModel)
                {
                    var transmittalStatus = transmittalModel.TransmittalRevisions.First(c => c.DocumentRevisionId == item.DocumentRevisionId);
                    switch (transmittalStatus.POI)
                    {
                        case POI.IFA:
                            item.RevisionStatus = RevisionStatus.TransmittalIFA;
                            break;
                        case POI.IFI:
                            item.RevisionStatus = RevisionStatus.TransmittalIFI;
                            break;
                        case POI.IFC:
                            item.RevisionStatus = RevisionStatus.TransmittalIFC;
                            break;
                        case POI.ASB:
                            item.RevisionStatus = RevisionStatus.TransmittalASB;
                            break;
                    }
                }
                //}

                var addReciverResult = await AddTransmittalReceiverUserAsync(transmittalModel, documentDetails.CustomerId, model);
                if (!addReciverResult.Succeeded)
                    return ServiceResultFactory.CreateError(false, addReciverResult.Messages.First().Message);

                transmittalModel = addReciverResult.Result;

                // generate form code
                var count = await _transmittalRepository.CountAsync(a => a.ContractCode == authenticate.ContractCode);
                var codeRes = await _formConfigService.GenerateFormCodeAsync(authenticate.ContractCode, FormName.Transmittal, count);
                if (!codeRes.Succeeded)
                    return ServiceResultFactory.CreateError(false, codeRes.Messages.First().Message);
                var transmittalNumber = codeRes.Result;
                transmittalModel.TransmittalNumber = transmittalNumber;

                //generate pdf
                var pdfResult = CreateAndSaveTransmittalsPdfAsync(transmittalModel, documentDetails,authenticate.UserFullName);
                if (!pdfResult.Succeeded)
                    return ServiceResultFactory.CreateError(false, pdfResult.Messages.First().Message);

                //transmittalModel.Attachments.Add(pdfResult.Result);

                _transmittalRepository.Add(transmittalModel);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    if (!string.IsNullOrEmpty(transmittalModel.Email))
                    {
                        try
                        {
                            BackgroundJob.Enqueue(() => SendEmailToReceiverUserAsync(authenticate, transmittalModel, documentDetails, pdfResult.Result));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.StackTrace);
                        }
                    }

                    var keyValues = model.Revisions.Select(c => c.DocumentRevisionId.ToString()).ToList();
                    await _scmLogAndNotificationService
                    .SetDonedNotificationAsync(authenticate.UserId, authenticate.ContractCode, keyValues, NotifEvent.AddTransmittal);

                    var company = transmittalModel.TransmittalType == TransmittalType.Internal ? "ترانسمیتال داخلی" :
                        transmittalModel.TransmittalType == TransmittalType.Customer ? documentDetails.CustomerName : transmittalModel.TransmittalType == TransmittalType.Supplier ? transmittalModel.Supplier.Name : transmittalModel.Consultant.Name;
                    var logModels = selectedRevisionModel.Select(c => new AddAuditLogDto
                    {
                        ContractCode = authenticate.ContractCode,
                        Description = c.Document.DocTitle,
                        FormCode = c.DocumentRevisionCode,
                        Message = company,
                        KeyValue = transmittalModel.TransmittalId.ToString(),
                        NotifEvent = NotifEvent.AddTransmittal,
                        RootKeyValue = c.DocumentRevisionId.ToString(),
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,
                        DocumentGroupId = documentDetails.DocumentGroupId
                    }).ToList();
                    if (!String.IsNullOrEmpty(model.Email) && !String.IsNullOrWhiteSpace(model.Email) && (transmittalModel.TransmittalType == TransmittalType.Consultant || transmittalModel.TransmittalType == TransmittalType.Customer))
                    {
                        var user = await _userRepository.FirstOrDefaultAsync(d => d.Email == model.Email && !d.IsDeleted && d.IsActive);
                        if (user != null)
                        {
                            var taskModel = selectedRevisionModel.Where(a => a.RevisionStatus == RevisionStatus.TransmittalIFA && !_documentCommunicationRepository.Any(b => b.DocumentRevisionId == a.DocumentRevisionId)).Select(c => new AddTaskNotificationDto
                            {
                                ContractCode = authenticate.ContractCode,
                                Description = c.Document.DocTitle,
                                Message = transmittalModel.TransmittalNumber,
                                FormCode = c.DocumentRevisionCode,
                                KeyValue = c.DocumentRevisionId.ToString(),
                                NotifEvent = NotifEvent.TransmittalPendingForComment,
                                RootKeyValue = c.DocumentRevisionId.ToString(),
                                PerformerUserId = authenticate.UserId,
                                PerformerUserFullName = authenticate.UserFullName,
                                Users = new List<int> { user.Id }
                            }).ToList();
                            await _scmLogAndNotificationService.AddScmAuditLogAsync(logModels, taskModel, false);
                        }
                        else
                        {
                            var res1 = await _scmLogAndNotificationService.AddScmAuditLogAsync(logModels, null);
                        }
                    }
                    else
                    {
                        var res1 = await _scmLogAndNotificationService.AddScmAuditLogAsync(logModels, null);
                    }


                    return ServiceResultFactory.CreateSuccess(true);
                }

                return ServiceResultFactory.CreateSuccess(true);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<bool>> SendEmailToReceiverUserAsync(AuthenticateDto authenticate, Transmittal transmittalModel, TransmitallDetailsForCreatePdfDto documentDetails, DownloadFileDto transmittalFile)
        {

            var roles = new List<string> { SCMRole.TransmittalMng, SCMRole.TransmittalObs };

            var users = await _authenticationServices.GetAllUserHasAccessDocumentAsync(authenticate.ContractCode, roles, transmittalModel.DocumentGroupId);
            var transmittalEmailNotify = await _notifyRepository.Where(a => a.TeamWork.ContractCode == authenticate.ContractCode && a.NotifyType == NotifyManagementType.Email && a.NotifyNumber == (int)EmailNotify.Transmittal && a.IsActive).Select(a=>a.UserId).ToListAsync();
            if (transmittalEmailNotify != null && transmittalEmailNotify.Any())
                users = users.Where(a => transmittalEmailNotify.Contains(a.Id)).ToList();
            else
                users = new List<UserMentionDto>();
            var ccEmails =(users!=null&&users.Any())? users
                .Where(a => !string.IsNullOrEmpty(a.Email) && a.Email != transmittalModel.Email)
                .Select(a => a.Email)
                .ToList():new List<string>();

            var emailAttachs = await PreParationAchmentForSendInEmail(transmittalModel, documentDetails, transmittalFile);
            await SaveAchmentForTaransmittal(transmittalModel, documentDetails, transmittalFile);
            string faBody = $"یک ترانسمیتال جدید به شماره <span style='direction:ltr'>{transmittalModel.TransmittalNumber}</span> مربوط به گروه مدارک {documentDetails.DocumentGroupTitle} در سیستم مدیریت پروژه رایبد ثبت گردید.";
            
            string enBody= $"<div  style='direction:ltr;text-align:left'>Transmittal No. {transmittalModel.TransmittalNumber} related to {documentDetails.DocumentGroupTitle} has been registered in Raybod App</div>";
            
            CommentMentionNotif model = new CommentMentionNotif(faBody, null, new List<CommentNotifViaEmailDTO>(), _appSettings.CompanyName,enBody);

            var emailRequest = new SendEmailDto
            {
                To = transmittalModel.Email,
                Body = await _viewRenderService.RenderToStringAsync("_TransmittlaNotifEmail", model),
                CCs = ccEmails,
                Subject = $"Transmittal | {transmittalModel.TransmittalNumber}"
            };
            await _appEmailService.SendTransmittalEmailAsync(emailRequest, emailAttachs, transmittalModel.TransmittalNumber + ".zip");

            return ServiceResultFactory.CreateSuccess(true);
        }

        private async Task<ServiceResult<Transmittal>> AddTransmittalReceiverUserAsync(Transmittal transmittalModel, int customerId, AddTransmittalDto model)
        {
            if ((model.TransmittalType == TransmittalType.Customer && !string.IsNullOrEmpty(model.Email)))
            {
                if (!await _companyUserRepository.AnyAsync(a => !a.IsDeleted && a.CustomerId == customerId && a.Email == model.Email))
                    return ServiceResultFactory.CreateError<Transmittal>(null, MessageId.UserNotExist);

                transmittalModel.FullName = model.FullName;
                transmittalModel.Email = model.Email;
            }
            else if (model.TransmittalType == TransmittalType.Internal && !string.IsNullOrEmpty(model.Email))
            {
                if (!await _userRepository.AnyAsync(a => !a.IsDeleted && a.IsActive && a.Email == model.Email))
                    return ServiceResultFactory.CreateError<Transmittal>(null, MessageId.UserNotExist);

                transmittalModel.FullName = model.FullName;
                transmittalModel.Email = model.Email;
            }
            else if (model.TransmittalType == TransmittalType.Supplier && model.SupplierId != null && model.SupplierId > 0)
            {
                var supplierModel = await _supplierRepository.FirstOrDefaultAsync(a => !a.IsDeleted && a.Id == model.SupplierId);
                if (supplierModel == null)
                    return ServiceResultFactory.CreateError<Transmittal>(null, MessageId.SupplierNotFound);

                transmittalModel.SupplierId = supplierModel.Id;
                transmittalModel.Supplier = supplierModel;

                if (!string.IsNullOrEmpty(model.Email))
                {
                    if (!await _companyUserRepository.AnyAsync(a => !a.IsDeleted && a.SupplierId == model.SupplierId && a.Email == model.Email))
                        return ServiceResultFactory.CreateError<Transmittal>(null, MessageId.UserNotExist);

                    transmittalModel.FullName = model.FullName;
                    transmittalModel.Email = model.Email;
                }
            }
            else if (model.TransmittalType == TransmittalType.Consultant && model.ConsultantId != null && model.ConsultantId > 0)
            {
                var consultantModel = await _consultantRepository.FirstOrDefaultAsync(a => !a.IsDeleted && a.Id == model.ConsultantId);
                if (consultantModel == null)
                    return ServiceResultFactory.CreateError<Transmittal>(null, MessageId.ConsultantNotFound);

                transmittalModel.ConsultantId = consultantModel.Id;
                transmittalModel.Consultant = consultantModel;

                if (!string.IsNullOrEmpty(model.Email))
                {
                    if (!await _companyUserRepository.AnyAsync(a => !a.IsDeleted && a.ConsultantId == model.ConsultantId && a.Email == model.Email))
                        return ServiceResultFactory.CreateError<Transmittal>(null, MessageId.UserNotExist);

                    transmittalModel.FullName = model.FullName;
                    transmittalModel.Email = model.Email;
                }
            }
            return ServiceResultFactory.CreateSuccess(transmittalModel);
        }

        private async Task<TransmittalFilesDto> PreParationAchmentForSendInEmail(Transmittal transmittalModel, TransmitallDetailsForCreatePdfDto documentDetails, DownloadFileDto transmittalFile)
        {
            var result = new TransmittalFilesDto();
            try
            {


                result.TransmitallFile = transmittalFile;

                foreach (var revison in documentDetails.Revisions)
                {
                    foreach (var item in revison.RevisionAttachments)
                    {
                        if (!item.IsDeleted && item.RevisionAttachmentType == RevisionAttachmentType.Final)
                        {
                            result.RevisionFiles.Add(new InMemoryFileDto
                            {
                                FileName = item.FileName,
                                FileSrc = item.FileSrc,
                                FileUrl = ServiceSetting.UploadFilePathDocument(transmittalModel.ContractCode, revison.DocumentId, revison.DocumentRevisionId)
                            });

                        }
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                return result;
            }

        }
        private async Task<bool> SaveAchmentForTaransmittal(Transmittal transmittalModel, TransmitallDetailsForCreatePdfDto documentDetails, DownloadFileDto transmittalFile)
        {
            var result = new List<InMemoryFileDto>();
            try
            {


                foreach (var revison in documentDetails.Revisions)
                {
                    foreach (var item in revison.RevisionAttachments)
                    {
                        if (!item.IsDeleted && item.RevisionAttachmentType == RevisionAttachmentType.Final)
                        {
                            result.Add(new InMemoryFileDto
                            {
                                FileName = item.FileName,
                                FileSrc = item.FileSrc,
                                FileUrl = ServiceSetting.UploadFilePathDocument(transmittalModel.ContractCode, revison.DocumentId, revison.DocumentRevisionId)
                            });

                        }
                    }
                }
                string filePath = ServiceSetting.UploadFilePathDocumentTransMittal(transmittalModel.ContractCode);
                string fileName = transmittalModel.TransmittalNumber + ".zip";
                string fileSource = _fileHelper.FileReadSrc(fileName, filePath);
                return await _fileHelper.ToMemoryStreamZipFileTransmittalAsync(result, fileSource, transmittalFile);

            }
            catch (Exception ex)
            {
                return false;
            }

        }


        public async Task<DownloadFileDto> DownloadTransmitalFileAsync(AuthenticateDto authenticate, long revId, RevisionAttachmentType type = RevisionAttachmentType.Final)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return null;

                if (type == RevisionAttachmentType.FinalNative && _appSettings.IsCheckedIP && !_appSettings.RemoteIPs.Contains(authenticate.RemoteIpAddress))
                    return null;

                var dbQuery = _revisionAttachmentRepository
                    .AsNoTracking()
                    .Where(a =>
                    !a.IsDeleted &&
                     (_transmittalRepository.Any(b => b.TransmittalId == revId && b.TransmittalRevisions.Any(c => c.DocumentRevisionId == a.DocumentRevisionId)) && a.DocumentRevision.Document.ContractCode == authenticate.ContractCode)
                    );

                if (permission.DocumentGroupIds.Any() && !dbQuery.Any(a => permission.DocumentGroupIds.Contains(a.DocumentRevision.Document.DocumentGroupId)))
                    return null;
                switch (type)
                {
                    case RevisionAttachmentType.Preparation:
                        dbQuery = dbQuery.Where(a => a.RevisionAttachmentType == type);
                        break;
                    case RevisionAttachmentType.Final:
                        dbQuery = dbQuery.Where(a => (a.RevisionAttachmentType == type));
                        break;
                    case RevisionAttachmentType.FinalNative:
                        dbQuery = dbQuery.Where(a => a.RevisionAttachmentType == type);
                        break;
                    default:
                        break;
                }


                if (!dbQuery.Any())
                    return null;

                var docuemntId = await dbQuery
                    .Select(a => a.DocumentRevision.DocumentId)
                    .FirstOrDefaultAsync();
                var transmittal = await _transmittalRepository.Include(a=>a.AdderUser).Include(a => a.TransmittalRevisions).ThenInclude(a => a.DocumentRevision).ThenInclude(a => a.RevisionAttachments).Include(a => a.TransmittalRevisions).ThenInclude(a => a.DocumentRevision).ThenInclude(a => a.Document).Include(a => a.Supplier).Include(a => a.Consultant).FirstOrDefaultAsync(a => a.TransmittalId == revId);
                string fileSrc = transmittal.TransmittalNumber + ".zip";
                var result = await _fileHelper.DownloadDocument(fileSrc, ServiceSetting.UploadFilePathTransmittal(authenticate.ContractCode));
                if (result == null)
                {
                    var revisionAttachment = await dbQuery.Where(a => a.DocumentRevisionId != null).Select(a => new RevisionAttachmentTemp { Revision = a, DocumentId = a.DocumentRevision.DocumentId }).ToListAsync();

                    var documentDetails = await _documentRepository
                   .Where(a => !a.IsDeleted && a.IsActive && a.ContractCode == authenticate.ContractCode && a.DocumentGroupId == transmittal.DocumentGroupId)
                   .Select(c => new TransmitallDetailsForCreatePdfDto
                   {
                       DocumentGroupId = c.DocumentGroupId,
                       DocumentGroupCode = c.DocumentGroup.DocumentGroupCode,
                       DocumentGroupTitle = c.DocumentGroup.Title,
                       ContractDescription = c.Contract.Description,
                       CustomerId = c.Contract.CustomerId ?? 0,
                       CustomerName = c.Contract.Customer.Name,
                       CustomerEmail = c.Contract.Customer.Email,
                       CustomerLogo = c.Contract.Customer.Logo,
                       Revisions = new List<DocumentRevision>()
                   }).FirstOrDefaultAsync();
                    documentDetails.Revisions = transmittal.TransmittalRevisions.Select(a => a.DocumentRevision).ToList();
                    var pdfResut = CreateAndSaveTransmittalsPdfAsync(transmittal, documentDetails,transmittal.AdderUser.FullName);
                    if (!pdfResut.Succeeded)
                        return null;

                    var crateResult = await SaveAchmentForTaransmittal(revisionAttachment, authenticate.ContractCode, transmittal.TransmittalNumber, pdfResut.Result);
                    if (!crateResult)
                        return null;
                    else
                    {
                        return await _fileHelper.DownloadDocument(fileSrc, ServiceSetting.UploadFilePathTransmittal(authenticate.ContractCode));
                    }
                }
                return result;
            }
            catch (Exception exception)
            {
                return null;
            }
        }
        private async Task<bool> SaveAchmentForTaransmittal(List<RevisionAttachmentTemp> attachments, string contractCode, string transmittalNumber, DownloadFileDto transmittalFile)
        {
            var result = new List<InMemoryFileDto>();
            try
            {
                foreach (var item in attachments)
                {
                    if (!item.Revision.IsDeleted && item.Revision.RevisionAttachmentType == RevisionAttachmentType.Final)
                    {
                        result.Add(new InMemoryFileDto
                        {
                            FileName = item.Revision.FileName,
                            FileSrc = item.Revision.FileSrc,
                            FileUrl = ServiceSetting.UploadFilePathDocument(contractCode, item.DocumentId, item.Revision.DocumentRevisionId.Value)
                        });

                    }
                }

                string filePath = ServiceSetting.UploadFilePathDocumentTransMittal(contractCode);
                string fileName = transmittalNumber + ".zip";
                string fileSource = _fileHelper.FileReadSrc(fileName, filePath);
                return await _fileHelper.ToMemoryStreamZipFileTransmittalAsync(result, fileSource, transmittalFile);

            }
            catch (Exception ex)
            {
                
                return false;
            }

        }
        
        public ServiceResult<DownloadFileDto> CreateAndSaveTransmittalsPdfAsync(Transmittal transmittalModel, TransmitallDetailsForCreatePdfDto documentDetails,string issure)
        {
            try
            {


                var selectedTemplate = new PDFTemplate();
                var templates = _pdfTemplateRepository
                    .Where(a => (a.ContractCode == null || a.ContractCode == transmittalModel.ContractCode) &&
                     a.PDFTemplateType == PDFTemplateType.Transmittal)
                    .ToList();

                if (!templates.Any())
                    return ServiceResultFactory.CreateError<DownloadFileDto>(null, MessageId.PdfTemplateNotFound);

                if (templates.Any(c => c.ContractCode == transmittalModel.ContractCode))
                    selectedTemplate = templates.FirstOrDefault(c => c.ContractCode == transmittalModel.ContractCode);
                else if (templates.Any(c => c.ContractCode == null))
                    selectedTemplate = templates.FirstOrDefault(c => c.ContractCode == null);
                else
                    selectedTemplate = templates
                        .OrderByDescending(a => a.Id)
                        .FirstOrDefault();

                string fileSrc = "doc-" + Guid.NewGuid().ToString("N") + ".pdf";
                var filePath = ServiceSetting.UploadFilePathDocumentTransMittal(transmittalModel.ContractCode);

                var globalSettings = new GlobalSettings
                {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4,
                    Margins = new MarginSettings { Top = 10, Bottom = 10 },

                };

                string companyLogo =
                documentDetails.CompanyName = _appSettings.CompanyName;
                documentDetails.CompanyLogo = _fileHelper.FileReadSrc(_appSettings.CompanyLogo);

                documentDetails.CustomerLogo = _fileHelper.FileReadSrc(documentDetails.CustomerLogo, ServiceSetting.UploadImagesPath.LogoSmall);

                var contentRootPath = _fileHelper.ReturnConentRootPath();
                var htmlContent = TransmittalPdfTemplate(transmittalModel, documentDetails, selectedTemplate, contentRootPath,issure);
                var objectSettings = new ObjectSettings
                {
                    // PagesCount = true,
                    HtmlContent = htmlContent,
                    WebSettings = { DefaultEncoding = "utf-8" },
                    //HeaderSettings = { FontName = "Arial", FontSize = 9, Right = "Page [page] of [toPage]", Line = true },
                    //FooterSettings = { FontName = "Arial", FontSize = 9, Line = true, Center = "Report Footer" }
                };
                var pdf = new HtmlToPdfDocument()
                {
                    GlobalSettings = globalSettings,
                    Objects = { objectSettings }
                };
                var file = _converter.Convert(pdf);

                var fileExtension = ".pdf";
                var result = new DownloadFileDto
                {
                    ArchiveFile = file,
                    FileName = transmittalModel.TransmittalNumber + fileExtension,
                    ContentType = "application/pdf",
                };

                return ServiceResultFactory.CreateSuccess(result);

                //string fileSrc = "doc-" + Guid.NewGuid().ToString("N") + ".pdf";


                //HtmlToPdf converter = new HtmlToPdf();

                //converter.Options.MarginTop = 15;
                //converter.Options.MarginBottom = 15;
                //converter.Options.MarginLeft = 15;
                //converter.Options.MarginRight = 15;

                //// set converter options
                //converter.Options.PdfPageSize = PdfPageSize.A4;
                //converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;

                //string companyLogo = _fileHelper.FileReadSrc("logo.png", "/Files/Pic/");
                //documentDetails.CustomerLogo = _fileHelper.FileReadSrc(documentDetails.CustomerLogo, ServiceSetting.UploadImagesPath.LogoSmall);

                //var htmlContent = Utilitys.PdfTemplate.TransmittalPdfTemplate(transmittalModel, documentDetails, companyLogo);
                //PdfDocument doc = converter.ConvertHtmlString(htmlContent);

                //var filePath = ServiceSetting.UploadFilePathDocumentTransMittal(transmittalModel.ContractCode);


                //var fff = doc.Save();

                //var rrr = await _fileHelper.SaveDocument(fff, fileSrc, filePath);
                //if (rrr == null)
                //    return ServiceResultFactory.CreateError<RevisionAttachment>(null, MessageId.SaveFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<DownloadFileDto>(null, exception);
            }
        }
        public async Task<ServiceResult<List<TransmittalListDto>>> GetTransmittalListAsync(AuthenticateDto authenticate, TransmittalQueryDto query, bool? type)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<TransmittalListDto>>(null, MessageId.AccessDenied);

                var dbQuery = _transmittalRepository
                     .AsNoTracking()
                     .Where(a =>
                     a.ContractCode == authenticate.ContractCode);

                if (type != null && type == true)
                    dbQuery = dbQuery.Where(a => a.TransmittalType == TransmittalType.Customer);

                if (permission.DocumentGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.DocumentGroupIds.Contains(a.DocumentGroupId));

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a =>
                    a.Description.Contains(query.SearchText) ||
                    a.TransmittalNumber.Contains(query.SearchText)
                    || a.TransmittalRevisions.Any(c => c.DocumentRevision.Document.DocNumber.Contains(query.SearchText) || c.DocumentRevision.Document.DocTitle.Contains(query.SearchText))
                    || (a.FullName != null && a.FullName.Contains(query.SearchText))
                    || (a.Email != null && a.Email.Contains(query.SearchText))
                    || (a.TransmittalType == TransmittalType.Customer && a.Contract.Customer.Name.Contains(query.SearchText))
                    || (a.SupplierId != null && a.Supplier.Name.Contains(query.SearchText))
                    || (a.ConsultantId != null && a.Consultant.Name.Contains(query.SearchText))
                    || (a.AdderUser != null && a.AdderUser.FullName.Contains(query.SearchText)));

                if (query.DocumentGroupIds != null && query.DocumentGroupIds.Any())
                    dbQuery = dbQuery.Where(a => query.DocumentGroupIds.Contains(a.DocumentGroupId));

                if (query.RevisionIds != null && query.RevisionIds.Any())
                    dbQuery = dbQuery.Where(a => a.TransmittalRevisions.Any(c => query.RevisionIds.Contains(c.DocumentRevisionId)));

                var totalCount = dbQuery.Count();

                var columnsMap = new Dictionary<string, Expression<Func<Transmittal, object>>>
                {
                    ["TransmittalId"] = v => v.TransmittalId
                };

                dbQuery = dbQuery.ApplayOrdering(query, columnsMap).ApplayPageing(query);

                var result = await dbQuery.Select(a => new TransmittalListDto
                {
                    Description = a.Description,
                    DocumentGroupTitle = a.DocumentGroup.Title,
                    Email = a.Email,
                    FullName = a.FullName,
                    RevisionCount = a.TransmittalRevisions.Count(),
                    TransmittalNumber = a.TransmittalNumber,
                    TransmittalType = a.TransmittalType,
                    TransmittalId = a.TransmittalId,
                    Attachment = a.Attachments.Select(c => new RevisionAttachmentDto
                    {
                        AttachType = c.RevisionAttachmentType,
                        FileName = c.FileName,
                        FileSize = c.FileSize,
                        FileSrc = c.FileSrc,
                        FileType = c.FileType,
                        RevisionAttachmentId = c.RevisionAttachmentId
                    }).FirstOrDefault(),
                    CompanyReceiver =
                    a.TransmittalType == TransmittalType.Supplier && a.SupplierId != null ? a.Supplier.Name :
                    a.TransmittalType == TransmittalType.Consultant && a.ConsultantId != null ? a.Consultant.Name :
                    a.TransmittalType == TransmittalType.Customer ? a.Contract.Customer.Name : "ترانسمیتال داخلی",
                    UserAudit = a.AdderUser != null ? new UserAuditLogDto
                    {
                        CreateDate = a.CreatedDate.ToUnixTimestamp(),
                        AdderUserName = a.AdderUser.FullName,
                        AdderUserImage = a.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + a.AdderUser.Image : ""
                    } : null
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<TransmittalListDto>>(null, exception);
            }
        }
        public async Task<ServiceResult<List<TransmittalListDto>>> GetTransmittalLisForCustomerUsertAsync(AuthenticateDto authenticate, TransmittalQueryDto query, bool? type, bool accessability)
        {
            try
            {
                if (!accessability)
                    return ServiceResultFactory.CreateError<List<TransmittalListDto>>(null, MessageId.AccessDenied);



                var dbQuery = _transmittalRepository
                     .AsNoTracking()
                     .Where(a =>
                     a.ContractCode == authenticate.ContractCode);

                if (type != null && type == true)
                    dbQuery = dbQuery.Where(a => (a.TransmittalType == TransmittalType.Customer || a.TransmittalType == TransmittalType.Consultant));



                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a =>
                    a.Description.Contains(query.SearchText) ||
                    a.TransmittalNumber.Contains(query.SearchText)
                    || a.TransmittalRevisions.Any(c => c.DocumentRevision.Document.DocNumber.Contains(query.SearchText) || c.DocumentRevision.Document.DocTitle.Contains(query.SearchText))
                    || (a.FullName != null && a.FullName.Contains(query.SearchText))
                    || (a.Email != null && a.Email.Contains(query.SearchText))
                    || (a.TransmittalType == TransmittalType.Customer && a.Contract.Customer.Name.Contains(query.SearchText))
                    || (a.TransmittalType == TransmittalType.Consultant && a.Contract.Consultant.Name.Contains(query.SearchText))
                    || (a.SupplierId != null && a.Supplier.Name.Contains(query.SearchText))
                    || (a.ConsultantId != null && a.Consultant.Name.Contains(query.SearchText))
                    || (a.AdderUser != null && a.AdderUser.FullName.Contains(query.SearchText)));

                if (query.DocumentGroupIds != null && query.DocumentGroupIds.Any())
                    dbQuery = dbQuery.Where(a => query.DocumentGroupIds.Contains(a.DocumentGroupId));

                if (query.RevisionIds != null && query.RevisionIds.Any())
                    dbQuery = dbQuery.Where(a => a.TransmittalRevisions.Any(c => query.RevisionIds.Contains(c.DocumentRevisionId)));

                var totalCount = dbQuery.Count();

                var columnsMap = new Dictionary<string, Expression<Func<Transmittal, object>>>
                {
                    ["TransmittalId"] = v => v.TransmittalId
                };

                dbQuery = dbQuery.ApplayOrdering(query, columnsMap).ApplayPageing(query);

                var result = await dbQuery.Select(a => new TransmittalListDto
                {
                    Description = a.Description,
                    DocumentGroupTitle = a.DocumentGroup.Title,
                    Email = a.Email,
                    FullName = a.FullName,
                    RevisionCount = a.TransmittalRevisions.Count(),
                    TransmittalNumber = a.TransmittalNumber,
                    TransmittalType = a.TransmittalType,
                    TransmittalId = a.TransmittalId,
                    Attachment = a.Attachments.Select(c => new RevisionAttachmentDto
                    {
                        AttachType = c.RevisionAttachmentType,
                        FileName = c.FileName,
                        FileSize = c.FileSize,
                        FileSrc = c.FileSrc,
                        FileType = c.FileType,
                        RevisionAttachmentId = c.RevisionAttachmentId
                    }).FirstOrDefault(),
                    CompanyReceiver =
                    a.TransmittalType == TransmittalType.Supplier && a.SupplierId != null ? a.Supplier.Name :
                    a.TransmittalType == TransmittalType.Consultant && a.ConsultantId != null ? a.Consultant.Name :
                    a.TransmittalType == TransmittalType.Customer ? a.Contract.Customer.Name : "ترانسمیتال داخلی",
                    UserAudit = a.AdderUser != null ? new UserAuditLogDto
                    {
                        CreateDate = a.CreatedDate.ToUnixTimestamp(),
                        AdderUserName = a.AdderUser.FullName,
                        AdderUserImage = a.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + a.AdderUser.Image : ""
                    } : null
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<TransmittalListDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<List<TransmittalExportExcelDto>>> GetTransmittaledRevisionListForExportToExcelAsync(AuthenticateDto authenticate, bool? type)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<TransmittalExportExcelDto>>(null, MessageId.AccessDenied);

                var dbQuery = _transmittalRevisionRepository
                     .AsNoTracking()
                     .Where(a =>
                     a.Transmittal.ContractCode == authenticate.ContractCode)
                     .OrderByDescending(a => a.TransmittalId)
                     .AsQueryable();

                if (permission.DocumentGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.DocumentGroupIds.Contains(a.Transmittal.DocumentGroupId));

                if (type != null && type == true)
                    dbQuery = dbQuery.Where(a => a.Transmittal.TransmittalType == TransmittalType.Customer);

                var result = await dbQuery.Select(a => new TransmittalExportExcelDto
                {
                    DocumentGroupTitle = a.Transmittal.DocumentGroup.Title,
                    FullName = a.Transmittal.FullName,
                    TransmittalNumber = a.Transmittal.TransmittalNumber,
                    CompanyReceiver =
                    a.Transmittal.TransmittalType == TransmittalType.Supplier && a.Transmittal.SupplierId != null ? a.Transmittal.Supplier.Name :
                    a.Transmittal.TransmittalType == TransmittalType.Consultant && a.Transmittal.ConsultantId != null ? a.Transmittal.Consultant.Name :
                    a.Transmittal.TransmittalType == TransmittalType.Customer ? a.Transmittal.Contract.Customer.Name : "ترانسمیتال داخلی",
                    CreateDate = a.Transmittal.CreatedDate.ToUnixTimestamp(),
                    DocClass = a.DocumentRevision.Document.DocClass,
                    DocNumber = a.DocumentRevision.Document.DocNumber,
                    DocTitle = a.DocumentRevision.Document.DocTitle,
                    ClientDocNumber = a.DocumentRevision.Document.ClientDocNumber,
                    PageNumber = a.DocumentRevision.RevisionPageNumber,
                    PageSize = a.DocumentRevision.RevisionPageSize,
                    RevisionCode = a.DocumentRevision.DocumentRevisionCode,
                    UserSenderName = a.Transmittal.AdderUser != null ? a.Transmittal.AdderUser.FullName : "",
                    POI = a.POI == POI.IFA ? "IFA" : a.POI == POI.IFI ? "IFI" : a.POI == POI.IFC ? "IFC":"ASB"

                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<TransmittalExportExcelDto>>(null, exception);
            }
        }
        public async Task<ServiceResult<List<TransmittalExportExcelDto>>> GetTransmittaledRevisionListForExportToExcelCustomerUserlAsync(AuthenticateDto authenticate, bool? type, bool accessability)
        {
            try
            {
                if (!accessability)
                    return ServiceResultFactory.CreateError<List<TransmittalExportExcelDto>>(null, MessageId.AccessDenied);



                var dbQuery = _transmittalRevisionRepository
                     .AsNoTracking()
                     .Where(a =>
                     a.Transmittal.ContractCode == authenticate.ContractCode)
                     .OrderByDescending(a => a.TransmittalId)
                     .AsQueryable();



                if (type != null && type == true)
                    dbQuery = dbQuery.Where(a => a.Transmittal.TransmittalType == TransmittalType.Customer || a.Transmittal.TransmittalType == TransmittalType.Consultant);

                var result = await dbQuery.Select(a => new TransmittalExportExcelDto
                {
                    DocumentGroupTitle = a.Transmittal.DocumentGroup.Title,
                    FullName = a.Transmittal.FullName,
                    TransmittalNumber = a.Transmittal.TransmittalNumber,
                    CompanyReceiver =
                    a.Transmittal.TransmittalType == TransmittalType.Supplier && a.Transmittal.SupplierId != null ? a.Transmittal.Supplier.Name :
                    a.Transmittal.TransmittalType == TransmittalType.Consultant && a.Transmittal.ConsultantId != null ? a.Transmittal.Consultant.Name :
                    a.Transmittal.TransmittalType == TransmittalType.Customer ? a.Transmittal.Contract.Customer.Name : "ترانسمیتال داخلی",
                    CreateDate = a.Transmittal.CreatedDate.ToUnixTimestamp(),
                    DocClass = a.DocumentRevision.Document.DocClass,
                    DocNumber = a.DocumentRevision.Document.DocNumber,
                    DocTitle = a.DocumentRevision.Document.DocTitle,
                    ClientDocNumber = a.DocumentRevision.Document.ClientDocNumber,
                    PageNumber = a.DocumentRevision.RevisionPageNumber,
                    PageSize = a.DocumentRevision.RevisionPageSize,
                    RevisionCode = a.DocumentRevision.DocumentRevisionCode,
                    UserSenderName = a.Transmittal.AdderUser != null ? a.Transmittal.AdderUser.FullName : "",
                    POI = a.POI == POI.IFA ? "IFA" : a.POI == POI.IFI ? "IFI" : a.POI == POI.IFC ? "IFC":"ASB",
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<TransmittalExportExcelDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<List<TransmittalListDto>>> GetTransmittalListByRevisionIdAsync(AuthenticateDto authenticate, long revisionId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<TransmittalListDto>>(null, MessageId.AccessDenied);

                var dbQuery = _transmittalRepository
                     .AsNoTracking()
                     .Where(a =>
                     a.TransmittalRevisions.Any(c => c.DocumentRevisionId == revisionId) &&
                     a.ContractCode == authenticate.ContractCode);

                if (permission.DocumentGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.DocumentGroupIds.Contains(a.DocumentGroupId));

                var result = await dbQuery.Select(a => new TransmittalListDto
                {
                    Description = a.Description,
                    DocumentGroupTitle = a.DocumentGroup.Title,
                    Email = a.Email,
                    FullName = a.FullName,
                    RevisionCount = a.TransmittalRevisions.Count(),
                    TransmittalNumber = a.TransmittalNumber,
                    TransmittalType = a.TransmittalType,
                    TransmittalId = a.TransmittalId,
                    Attachment = a.Attachments.Select(c => new RevisionAttachmentDto
                    {
                        AttachType = c.RevisionAttachmentType,
                        FileName = c.FileName,
                        FileSize = c.FileSize,
                        FileSrc = c.FileSrc,
                        FileType = c.FileType,
                        RevisionAttachmentId = c.RevisionAttachmentId
                    }).FirstOrDefault(),

                    CompanyReceiver =
                    a.TransmittalType == TransmittalType.Supplier && a.SupplierId != null ? a.Supplier.Name :
                    a.TransmittalType == TransmittalType.Consultant && a.ConsultantId != null ? a.Consultant.Name :
                    a.TransmittalType == TransmittalType.Customer ? a.Contract.Customer.Name :(authenticate.language=="en")?"Internal Transmittal": "ترانسمیتال داخلی",
                    UserAudit = a.AdderUser != null ? new UserAuditLogDto
                    {
                        CreateDate = a.CreatedDate.ToUnixTimestamp(),
                        AdderUserName = a.AdderUser.FullName,
                        AdderUserImage = a.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + a.AdderUser.Image : ""
                    } : null
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<TransmittalListDto>>(null, exception);
            }
        }
        public async Task<ServiceResult<List<TransmittalListDto>>> GetTransmittalListByRevisionIdForCustomerUserAsync(AuthenticateDto authenticate, long revisionId, bool accessability)
        {
            try
            {
                if (!accessability)
                    return ServiceResultFactory.CreateError<List<TransmittalListDto>>(null, MessageId.AccessDenied);



                var dbQuery = _transmittalRepository
                     .AsNoTracking()
                     .Where(a =>
                     a.TransmittalRevisions.Any(c => c.DocumentRevisionId == revisionId) &&
                     a.ContractCode == authenticate.ContractCode && (a.TransmittalType == TransmittalType.Customer || a.TransmittalType == TransmittalType.Consultant));



                var result = await dbQuery.Select(a => new TransmittalListDto
                {
                    Description = a.Description,
                    DocumentGroupTitle = a.DocumentGroup.Title,
                    Email = a.Email,
                    FullName = a.FullName,
                    RevisionCount = a.TransmittalRevisions.Count(),
                    TransmittalNumber = a.TransmittalNumber,
                    TransmittalType = a.TransmittalType,
                    TransmittalId = a.TransmittalId,
                    Attachment = a.Attachments.Select(c => new RevisionAttachmentDto
                    {
                        AttachType = c.RevisionAttachmentType,
                        FileName = c.FileName,
                        FileSize = c.FileSize,
                        FileSrc = c.FileSrc,
                        FileType = c.FileType,
                        RevisionAttachmentId = c.RevisionAttachmentId
                    }).FirstOrDefault(),

                    CompanyReceiver =
                    a.TransmittalType == TransmittalType.Supplier && a.SupplierId != null ? a.Supplier.Name :
                    a.TransmittalType == TransmittalType.Consultant && a.ConsultantId != null ? a.Consultant.Name :
                    a.TransmittalType == TransmittalType.Customer ? a.Contract.Customer.Name : "ترانسمیتال داخلی",
                    UserAudit = a.AdderUser != null ? new UserAuditLogDto
                    {
                        CreateDate = a.CreatedDate.ToUnixTimestamp(),
                        AdderUserName = a.AdderUser.FullName,
                        AdderUserImage = a.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + a.AdderUser.Image : ""
                    } : null
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<TransmittalListDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<List<PendingTransmitalRevisionInfoDo>>> GetTransmittalDetailsAsync(AuthenticateDto authenticate, long transmittalId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<PendingTransmitalRevisionInfoDo>>(null, MessageId.AccessDenied);

                var dbQuery = _documentRevisionRepository
                     .AsNoTracking()
                     .Where(a => !a.IsDeleted &&
                     a.Document.ContractCode == authenticate.ContractCode &&
                     a.TransmittalRevisions.Any(c => c.TransmittalId == transmittalId));

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<List<PendingTransmitalRevisionInfoDo>>(null, MessageId.EntityDoesNotExist);

                if (permission.DocumentGroupIds.Any() && !dbQuery.Any(a => permission.DocumentGroupIds.Contains(a.Document.DocumentGroupId)))
                    return ServiceResultFactory.CreateError<List<PendingTransmitalRevisionInfoDo>>(null, MessageId.AccessDenied);

                var result = await dbQuery.Select(a => new PendingTransmitalRevisionInfoDo
                {
                    RevisionCode = a.DocumentRevisionCode,
                    DocTitle = a.Document.DocTitle,
                    DocNumber = a.Document.DocNumber,
                    DocClass = a.Document.DocClass,
                    POI = a.TransmittalRevisions.FirstOrDefault(a => a.TransmittalId == transmittalId).POI,
                    DocumentId = a.DocumentId,
                    RevisionId = a.DocumentRevisionId,
                    PageNumber = a.RevisionPageNumber,
                    PageSize = a.RevisionPageSize,
                    RevisionAttachments = a.RevisionAttachments.Where(a => !a.IsDeleted && a.RevisionAttachmentType == RevisionAttachmentType.Final)
                    .Select(c => new RevisionAttachmentDto
                    {
                        AttachType = c.RevisionAttachmentType,
                        FileType = c.FileType,
                        FileName = c.FileName,
                        FileSize = c.FileSize,
                        FileSrc = c.FileSrc,
                        RevisionAttachmentId = c.RevisionAttachmentId
                    }).ToList()
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<PendingTransmitalRevisionInfoDo>>(null, exception);
            }
        }
        public async Task<ServiceResult<List<PendingTransmitalRevisionInfoDo>>> GetTransmittalDetailsForCustomerUserAsync(AuthenticateDto authenticate, long transmittalId, bool accessability)
        {
            try
            {
                if (!accessability)
                    return ServiceResultFactory.CreateError<List<PendingTransmitalRevisionInfoDo>>(null, MessageId.AccessDenied);

                var dbQuery = _documentRevisionRepository
                     .AsNoTracking()
                     .Where(a => !a.IsDeleted &&
                     a.Document.ContractCode == authenticate.ContractCode &&
                     a.TransmittalRevisions.Any(c => c.TransmittalId == transmittalId));

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<List<PendingTransmitalRevisionInfoDo>>(null, MessageId.EntityDoesNotExist);



                var result = await dbQuery.Select(a => new PendingTransmitalRevisionInfoDo
                {
                    RevisionCode = a.DocumentRevisionCode,
                    DocTitle = a.Document.DocTitle,
                    DocNumber = a.Document.DocNumber,
                    DocClass = a.Document.DocClass,
                    POI = a.TransmittalRevisions.FirstOrDefault(a => a.TransmittalId == transmittalId).POI,
                    DocumentId = a.DocumentId,
                    RevisionId = a.DocumentRevisionId,
                    PageNumber = a.RevisionPageNumber,
                    PageSize = a.RevisionPageSize,
                    RevisionAttachments = a.RevisionAttachments.Where(a => !a.IsDeleted && a.RevisionAttachmentType == RevisionAttachmentType.Final)
                    .Select(c => new RevisionAttachmentDto
                    {
                        AttachType = c.RevisionAttachmentType,
                        FileType = c.FileType,
                        FileName = c.FileName,
                        FileSize = c.FileSize,
                        FileSrc = c.FileSrc,
                        RevisionAttachmentId = c.RevisionAttachmentId
                    }).ToList()
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<PendingTransmitalRevisionInfoDo>>(null, exception);
            }
        }
        public async Task<DownloadFileDto> DownloadTransmitalFileAsync(AuthenticateDto authenticate, long transmittalId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return null;

                var transmittal = await _transmittalRepository.Include(a=>a.AdderUser).Include(a => a.TransmittalRevisions).ThenInclude(a => a.DocumentRevision).ThenInclude(a => a.RevisionAttachments).Include(a => a.TransmittalRevisions).ThenInclude(a => a.DocumentRevision).ThenInclude(a => a.Document).Include(a => a.Supplier).Include(a => a.Consultant).FirstOrDefaultAsync(a => a.TransmittalId == transmittalId);

                var documentDetails = await _documentRepository
               .Where(a => !a.IsDeleted && a.IsActive && a.ContractCode == authenticate.ContractCode && a.DocumentGroupId == transmittal.DocumentGroupId)
               .Select(c => new TransmitallDetailsForCreatePdfDto
               {
                   DocumentGroupId = c.DocumentGroupId,
                   DocumentGroupCode = c.DocumentGroup.DocumentGroupCode,
                   DocumentGroupTitle = c.DocumentGroup.Title,
                   ContractDescription = c.Contract.Description,
                   CustomerId = c.Contract.CustomerId ?? 0,
                   CustomerName = c.Contract.Customer.Name,
                   CustomerEmail = c.Contract.Customer.Email,
                   CustomerLogo = c.Contract.Customer.Logo,
                   Revisions = new List<DocumentRevision>()
               }).FirstOrDefaultAsync();
                documentDetails.Revisions = transmittal.TransmittalRevisions.Select(a => a.DocumentRevision).ToList();
                var pdfResut = CreateAndSaveTransmittalsPdfAsync(transmittal, documentDetails, transmittal.AdderUser.FullName);
                if (!pdfResut.Succeeded)
                    return null;


                return pdfResut.Result;
            }
            catch (Exception exception)
            {
                return null;
            }
        }
        public async Task<DownloadFileDto> DownloadTransmitalFileForCustomerUserAsync(AuthenticateDto authenticate, long transmittalId, bool accessability)
        {
            try
            {
                if (!accessability)
                    return null;



                var transmittal = await _transmittalRepository.Include(a=>a.AdderUser).Include(a => a.TransmittalRevisions).ThenInclude(a => a.DocumentRevision).ThenInclude(a => a.RevisionAttachments).Include(a => a.TransmittalRevisions).ThenInclude(a => a.DocumentRevision).ThenInclude(a => a.Document).Include(a => a.Supplier).Include(a => a.Consultant).FirstOrDefaultAsync(a => a.TransmittalId == transmittalId);

                var documentDetails = await _documentRepository
               .Where(a => !a.IsDeleted && a.IsActive && a.ContractCode == authenticate.ContractCode && a.DocumentGroupId == transmittal.DocumentGroupId)
               .Select(c => new TransmitallDetailsForCreatePdfDto
               {
                   DocumentGroupId = c.DocumentGroupId,
                   DocumentGroupCode = c.DocumentGroup.DocumentGroupCode,
                   DocumentGroupTitle = c.DocumentGroup.Title,
                   ContractDescription = c.Contract.Description,
                   CustomerId = c.Contract.CustomerId ?? 0,
                   CustomerName = c.Contract.Customer.Name,
                   CustomerEmail = c.Contract.Customer.Email,
                   CustomerLogo = c.Contract.Customer.Logo,
                   Revisions = new List<DocumentRevision>()
               }).FirstOrDefaultAsync();
                documentDetails.Revisions = transmittal.TransmittalRevisions.Select(a => a.DocumentRevision).ToList();
                var pdfResut = CreateAndSaveTransmittalsPdfAsync(transmittal, documentDetails, transmittal.AdderUser.FullName);
                if (!pdfResut.Succeeded)
                    return null;


                return pdfResut.Result;
            }
            catch (Exception exception)
            {
                return null;
            }
        }

        public async Task<DownloadFileDto> DownloadTransmitalFileForCustomerUserAsync(AuthenticateDto authenticate, long transmittalId, bool accessability, RevisionAttachmentType type = RevisionAttachmentType.Final)
        {
            try
            {
                if (!accessability)
                    return null;



                if (type == RevisionAttachmentType.FinalNative && _appSettings.IsCheckedIP && !_appSettings.RemoteIPs.Contains(authenticate.RemoteIpAddress))
                    return null;

                var dbQuery = _revisionAttachmentRepository
                    .AsNoTracking()
                    .Where(a =>
                    !a.IsDeleted &&
                     (_transmittalRepository.Any(b => b.TransmittalId == transmittalId && b.TransmittalRevisions.Any(c => c.DocumentRevisionId == a.DocumentRevisionId)) && a.DocumentRevision.Document.ContractCode == authenticate.ContractCode)
                    );


                switch (type)
                {
                    case RevisionAttachmentType.Preparation:
                        dbQuery = dbQuery.Where(a => a.RevisionAttachmentType == type);
                        break;
                    case RevisionAttachmentType.Final:
                        dbQuery = dbQuery.Where(a => (a.RevisionAttachmentType == type) || (a.TransmittalId != null && a.RevisionAttachmentType == 0));
                        break;
                    case RevisionAttachmentType.FinalNative:
                        dbQuery = dbQuery.Where(a => a.RevisionAttachmentType == type);
                        break;
                    default:
                        break;
                }


                if (!dbQuery.Any())
                    return null;

                var docuemntId = await dbQuery
                    .Select(a => a.DocumentRevision.DocumentId)
                    .FirstOrDefaultAsync();
                var transmittal = await _transmittalRepository.Include(a=>a.AdderUser).Include(a => a.TransmittalRevisions).ThenInclude(a => a.DocumentRevision).ThenInclude(a => a.RevisionAttachments).Include(a => a.TransmittalRevisions).ThenInclude(a => a.DocumentRevision).ThenInclude(a => a.Document).Include(a => a.Supplier).Include(a => a.Consultant).FirstOrDefaultAsync(a => a.TransmittalId == transmittalId);
                string fileSrc = transmittal.TransmittalNumber + ".zip";
                var result = await _fileHelper.DownloadDocument(fileSrc, ServiceSetting.UploadFilePathTransmittal(authenticate.ContractCode));
                if (result == null)
                {
                    var revisionAttachment = await dbQuery.Where(a => a.DocumentRevisionId != null).Select(a => new RevisionAttachmentTemp { Revision = a, DocumentId = a.DocumentRevision.DocumentId }).ToListAsync();
                    var documentDetails = await _documentRepository
                    .Where(a => !a.IsDeleted && a.IsActive && a.ContractCode == authenticate.ContractCode && a.DocumentGroupId == transmittal.DocumentGroupId)
                    .Select(c => new TransmitallDetailsForCreatePdfDto
                    {
                        DocumentGroupId = c.DocumentGroupId,
                        DocumentGroupCode = c.DocumentGroup.DocumentGroupCode,
                        DocumentGroupTitle = c.DocumentGroup.Title,
                        ContractDescription = c.Contract.Description,
                        CustomerId = c.Contract.CustomerId ?? 0,
                        CustomerName = c.Contract.Customer.Name,
                        CustomerEmail = c.Contract.Customer.Email,
                        CustomerLogo = c.Contract.Customer.Logo,
                        Revisions = new List<DocumentRevision>()
                    }).FirstOrDefaultAsync();
                    documentDetails.Revisions = transmittal.TransmittalRevisions.Select(a => a.DocumentRevision).ToList();
                    var pdfResut = CreateAndSaveTransmittalsPdfAsync(transmittal, documentDetails, transmittal.AdderUser.FullName);
                    if (!pdfResut.Succeeded)
                        return null;
                    var crateResult = await SaveAchmentForTaransmittal(revisionAttachment, authenticate.ContractCode, transmittal.TransmittalNumber, pdfResut.Result);
                    if (!crateResult)
                        return null;
                    else
                    {
                        return await _fileHelper.DownloadDocument(fileSrc, ServiceSetting.UploadFilePathTransmittal(authenticate.ContractCode));
                    }
                }
                return result;
            }
            catch (Exception exception)
            {
                return null;
            }
        }
        public static string TransmittalPdfTemplate(Transmittal transmittal, TransmitallDetailsForCreatePdfDto documentDetails,
            PDFTemplate template, string contentRootPath,string issure)
        {
            var html = new StringBuilder();
            html.AppendFormat(template.Section1,
                                documentDetails.ContractDescription,
                                documentDetails.CompanyLogo,
                                documentDetails.CustomerLogo,
                                transmittal.TransmittalNumber,
                                transmittal.CreatedDate.Value.ToPersianDate(),
                                transmittal.TransmittalType == TransmittalType.Customer ?
                                    documentDetails.CustomerName : transmittal.TransmittalType == TransmittalType.Internal ? documentDetails.CompanyName : transmittal.TransmittalType == TransmittalType.Supplier ? transmittal.Supplier.Name : transmittal.Consultant.Name,
                                documentDetails.CustomerName,
                                transmittal.FullName,
                                documentDetails.CompanyName,
                                documentDetails.DocumentGroupTitle,
                                contentRootPath,
                                issure
                                );
            int i = 1;
            foreach (var item in documentDetails.Revisions)
            {
                html.AppendFormat(template.Section2,
                 i++,
                 item.Document.DocNumber,
                 item.DocumentRevisionCode,
                 item.Document.DocTitle,
                 transmittal.TransmittalRevisions.First(a => a.DocumentRevisionId == item.DocumentRevisionId).POI == POI.IFA ? "IFA" : transmittal.TransmittalRevisions.First(a => a.DocumentRevisionId == item.DocumentRevisionId).POI == POI.IFI ? "IFI" : transmittal.TransmittalRevisions.First(a => a.DocumentRevisionId == item.DocumentRevisionId).POI == POI.IFC ? "IFC":"ASB",
                 item.RevisionPageSize,
                 item.RevisionPageNumber,
                 item.Document.ClientDocNumber,
                 (item.Document.DocClass==DocumentClass.FA)?"FA":"FI");
            }

            html.AppendFormat(template.Section3,
                documentDetails.Revisions.Where(a => a.RevisionPageNumber != null).Sum(v => v.RevisionPageNumber),
                transmittal.Description,
                contentRootPath);

            return html.ToString();
        }

        public async Task<ServiceResult<TransmittalEmailContentDto>> GetTransmittalEmailContent(AuthenticateDto authenticate, long transmittalId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<TransmittalEmailContentDto>(null, MessageId.AccessDenied);

                var dbQuery = _transmittalRepository
                     .AsNoTracking().Include(a => a.DocumentGroup)
                     .Where(a => a.TransmittalId == transmittalId);

                if (permission.DocumentGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.DocumentGroupIds.Contains(a.DocumentGroupId));
                var transmittal = await dbQuery.FirstOrDefaultAsync();
                if (transmittal == null)
                {
                    return ServiceResultFactory.CreateError<TransmittalEmailContentDto>(null, MessageId.EntityDoesNotExist);
                }

                var roles = new List<string> { SCMRole.TransmittalMng, SCMRole.TransmittalObs };

                var users = await _authenticationServices.GetAllUserHasAccessDocumentAsync(authenticate.ContractCode, roles, transmittal.DocumentGroupId);
                var transmittalEmailNotify = await _notifyRepository.Where(a => a.TeamWork.ContractCode == authenticate.ContractCode && a.NotifyType == NotifyManagementType.Email && a.NotifyNumber == (int)EmailNotify.Transmittal && a.IsActive).Select(a => a.UserId).ToListAsync();
                if (transmittalEmailNotify != null && transmittalEmailNotify.Any())
                    users = users.Where(a => transmittalEmailNotify.Contains(a.Id)).ToList();
                else
                    users = new List<UserMentionDto>();
                var ccEmails =(users!=null&&users.Any())? users
               .Where(a => !string.IsNullOrEmpty(a.Email) && a.Email != transmittal.Email)
               .Select(a => a.Email)
               .ToList(): new List<string>();
                string faBody = $"یک ترانسمیتال جدید به شماره {transmittal.TransmittalNumber} مربوط به گروه مدارک {transmittal.DocumentGroup.Title} در سیستم مدیریت پروژه رایبد ثبت گردید.";

                string enBody = $"Transmittal No. {transmittal.TransmittalNumber} related to {transmittal.DocumentGroup.Title} has been registered in Raybod App";
                var result = new TransmittalEmailContentDto
                {
                    Message = (authenticate.language=="en")?enBody:faBody,
                    CC = ccEmails,
                    To = !String.IsNullOrEmpty(transmittal.Email) ? transmittal.Email : "",
                    Subject = $"Transmittal | {transmittal.TransmittalNumber}"
                };

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<TransmittalEmailContentDto>(null, exception);
            }
        }
        public async Task<ServiceResult<bool>> SendTransmittalEmail(AuthenticateDto authenticate, long transmittalId, TransmittalEmailContentDto model)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery = _transmittalRepository
                     .AsNoTracking().Include(a => a.DocumentGroup)
                     .Where(a => a.TransmittalId == transmittalId);

                if (permission.DocumentGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.DocumentGroupIds.Contains(a.DocumentGroupId));
                var transmittal = await dbQuery.FirstOrDefaultAsync();
                if (transmittal == null)
                {
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);
                }


                string fileSrc = transmittal.TransmittalNumber + ".zip";
                DownloadFileDto result = await _fileHelper.DownloadDocument(fileSrc, ServiceSetting.UploadFilePathTransmittal(authenticate.ContractCode));
                if (result == null)
                {
                    result = await DownloadTransmitalFileAsync(authenticate, transmittalId,RevisionAttachmentType.Final);
                    if (result == null)
                        return ServiceResultFactory.CreateError(false, MessageId.FileNotFound);
                    else
                    {
                        result = await _fileHelper.DownloadDocument(fileSrc, ServiceSetting.UploadFilePathTransmittal(authenticate.ContractCode));
                    }
                }
                CommentMentionNotif emaiBody = new CommentMentionNotif(model.Message, null, new List<CommentNotifViaEmailDTO>(), _appSettings.CompanyName);
                var emailRequest = new SendEmailDto
                {
                    To = model.To,
                    Body = await _viewRenderService.RenderToStringAsync("_TransmittlaNotifEmailFree", emaiBody),
                    CCs = model.CC,
                    Subject = model.Subject
                };


                return await _appEmailService.SendAsync(emailRequest, _fileHelper.FileReadSrcForEmailAttachemnt(fileSrc, ServiceSetting.UploadFilePathTransmittal(authenticate.ContractCode)));



            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<bool>(false, exception);
            }
        }

    }
}
