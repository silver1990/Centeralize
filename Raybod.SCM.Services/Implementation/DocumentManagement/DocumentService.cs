using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using Raybod.SCM.DataAccess.Core;
using Raybod.SCM.DataAccess.Extention;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject._PanelDocument.Area;
using Raybod.SCM.DataTransferObject.Audit;
using Raybod.SCM.DataTransferObject.Document;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Model;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Services.Utilitys.exportToExcel;
using Raybod.SCM.Utility.Extention;
using Raybod.SCM.Utility.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib;
using Dapper.Contrib.Extensions;
using Raybod.SCM.DataTransferObject._PanelDocument.Document;
using Raybod.SCM.DataTransferObject._PanelDocument.Communication;
using Raybod.SCM.DataTransferObject._PanelDocument.DocumentRevision.Archive;

namespace Raybod.SCM.Services.Implementation
{
    public class DocumentService : IDocumentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly IBomProductService _bomProductService;
        private readonly DbSet<Document> _documentRepository;
        private readonly DbSet<Contract> _contractRepository;
        private readonly DbSet<DocumentRevision> _documentRevisionRepository;
        private readonly DbSet<DocumentGroup> _documentGroupRepository;
        private readonly DbSet<DocumentCommunication> _documentCommunicationRepository;
        private readonly DbSet<SCMAuditLog> _scmAuditLogsRepository;
        private readonly DbSet<Transmittal> _transmittalRepository;
        private readonly DbSet<Customer> _customerRepository;
        private readonly DbSet<Consultant> _consultantRepository;
        private readonly DbSet<Area> _areaRepository;
        private readonly DbSet<User> _userRepository;
        private readonly ITeamWorkAuthenticationService _authenticationServices;
        private readonly ISCMLogAndNotificationService _scmLogAndNotificationService;
        private readonly Utilitys.FileHelper _fileHelper;
        private readonly CompanyConfig _appSettings;
        private string _contentRootPath;

        public DocumentService(IUnitOfWork unitOfWork,
            IWebHostEnvironment hostingEnvironmentRoot,
            IOptions<CompanyAppSettingsDto> appSettings,
            ITeamWorkAuthenticationService authenticationServices,
            ISCMLogAndNotificationService scmLogAndNotificationService,
            IHttpContextAccessor httpContextAccessor,
            IBomProductService bomProductService, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _bomProductService = bomProductService;
            _authenticationServices = authenticationServices;
            _scmLogAndNotificationService = scmLogAndNotificationService;
            _documentRepository = _unitOfWork.Set<Document>();
            _areaRepository = _unitOfWork.Set<Area>();
            _scmAuditLogsRepository = _unitOfWork.Set<SCMAuditLog>();
            _documentCommunicationRepository = _unitOfWork.Set<DocumentCommunication>();
            _documentRevisionRepository = _unitOfWork.Set<DocumentRevision>();
            _documentGroupRepository = _unitOfWork.Set<DocumentGroup>();
            _transmittalRepository = _unitOfWork.Set<Transmittal>();
            _userRepository = _unitOfWork.Set<User>();
            _customerRepository = _unitOfWork.Set<Customer>();
            _consultantRepository = _unitOfWork.Set<Consultant>();
            _contractRepository = _unitOfWork.Set<Contract>();
            _fileHelper = new Utilitys.FileHelper(hostingEnvironmentRoot);
            _configuration = configuration;
            _contentRootPath = hostingEnvironmentRoot.ContentRootPath;
            httpContextAccessor.HttpContext.Request.Headers.TryGetValue("companyCode", out var CompanyCode);
            _appSettings = appSettings.Value.CompanyConfig.First(a => a.CompanyCode == CompanyCode);
        }


        public async Task<ServiceResult<List<DocumentViewDto>>> AddDocumentAsync(AuthenticateDto authenticate, int documentGroupId, List<AddListDocumentDto> model)
        {
            try
            {
                if (string.IsNullOrEmpty(authenticate.ContractCode) || model == null || !model.Any())
                    return ServiceResultFactory.CreateError<List<DocumentViewDto>>(null, MessageId.InputDataValidationError);


                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<DocumentViewDto>>(null, MessageId.AccessDenied);

                if (permission.DocumentGroupIds.Any() && !permission.DocumentGroupIds.Contains(documentGroupId))
                    return ServiceResultFactory.CreateError<List<DocumentViewDto>>(null, MessageId.AccessDenied);

                if (model.Any(c => !EnumHelper.ValidateItem(c.DocClass)))
                    return ServiceResultFactory.CreateError<List<DocumentViewDto>>(null, MessageId.InputDataValidationError);

                if (model.Any(c => (c.Area != null && c.Area.AreaId == null && c.Area.AreaTitle != null && c.Area.AreaTitle.Length > 20)))
                    return ServiceResultFactory.CreateError<List<DocumentViewDto>>(null, MessageId.AreaTitleLegnthOverLimited);

                var documentGroupModel = await _documentGroupRepository.FirstOrDefaultAsync(a => !a.IsDeleted && a.DocumentGroupId == documentGroupId);
                if (documentGroupModel == null)
                    return ServiceResultFactory.CreateError<List<DocumentViewDto>>(null, MessageId.EntityDoesNotExist);




                var docNumbers = model.Select(c => c.DocNumber).ToList();

                if (model.GroupBy(a => a.DocNumber).Any(c => c.Count() > 1))
                    return ServiceResultFactory.CreateError<List<DocumentViewDto>>(null, MessageId.DuplicatDocNumber);

                if (await _documentRepository.AnyAsync(a => !a.IsDeleted && docNumbers.Contains(a.DocNumber)))
                    return ServiceResultFactory.CreateError<List<DocumentViewDto>>(null, MessageId.DuplicatDocNumber);

                if (model.Any(c => c.ProductIds != null && c.ProductIds.Any()))
                {
                    var acceptBomProducts = await _bomProductService.GetLastChildProductIdsOfContractbomAsync(authenticate.ContractCode);
                    if (!acceptBomProducts.Succeeded)
                        return ServiceResultFactory.CreateError<List<DocumentViewDto>>(null, MessageId.BomNotFound);

                    if (model.Any(a => a.ProductIds.Any(p => !acceptBomProducts.Result.Contains(p))))
                        return ServiceResultFactory.CreateError<List<DocumentViewDto>>(null, MessageId.InputDataValidationError);
                }
                for (int i = 0; i < model.Count; i++)
                {
                    if (model[i].Area != null)
                    {
                        if (model[i].Area.AreaId != null)
                        {
                            model[i].AreaId = model[i].Area.AreaId;
                        }
                        else if ((!String.IsNullOrEmpty(model[i].Area.AreaTitle) && !String.IsNullOrWhiteSpace(model[i].Area.AreaTitle)))
                        {
                            var area = await _areaRepository.FirstOrDefaultAsync(a => a.AreaTitle == model[i].Area.AreaTitle.Trim() && a.ContractCode == authenticate.ContractCode && !a.IsDeleted);
                            if (area != null)
                            {
                                model[i].AreaId = area.AreaId;
                            }
                            else
                            {
                                var connectionString = _configuration["ConnectionStrings:ApplicationDbContextConnection"];
                                if (connectionString.Contains("%CONTENTROOTPATH%"))
                                {
                                    connectionString = connectionString.Replace("%CONTENTROOTPATH%", _contentRootPath);
                                }
                                try
                                {
                                    using var dbConn = new SqlConnection(connectionString);
                                    var query = @"Insert into Areas (AreaTitle,ContractCode,IsDeleted,AdderUserId,ModifierUserId,CreatedDate,UpdateDate) Values(@areaTitle,@contractCode,@isDeleted,@adderUserId,@modifierUserId,@createdDate,@updateDate);Select SCOPE_IDENTITY();";
                                    model[i].AreaId = await dbConn.ExecuteScalarAsync<int>(query, new { areaTitle = model[i].Area.AreaTitle.Trim(), contractCode = authenticate.ContractCode, isDeleted = false, adderUserId = authenticate.UserId, modifierUserId = authenticate.UserId, createdDate = DateTime.Now, updateDate = DateTime.Now });

                                }
                                catch (Exception exception)
                                {
                                    return ServiceResultFactory.CreateException<List<DocumentViewDto>>(null, exception);
                                }
                            }
                        }

                    }
                }
                var addDocumentModels = model.Select(a => new Document
                {
                    ContractCode = authenticate.ContractCode,
                    DocumentGroupId = documentGroupId,
                    DocTitle = a.DocTitle,
                    ClientDocNumber = a.ClientDocNumber,
                    DocNumber = a.DocNumber,
                    IsActive = true,
                    DocClass = a.DocClass,
                    DocRemark = a.DocRemark,
                    IsRequiredTransmittal = a.IsRequiredTransmittal,
                    AreaId = a.AreaId,
                    CommunicationCommentStatus = CommunicationCommentStatus.NotHave,
                    DocumentProducts = a.ProductIds.Select(productId => new DocumentProduct
                    {
                        ProductId = productId
                    }).ToList(),
                }).ToList();

                _documentRepository.AddRange(addDocumentModels);

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    await _scmLogAndNotificationService.SetDonedNotificationAsync(authenticate.UserId, authenticate.ContractCode, documentGroupId.ToString(), NotifEvent.AddDocument);

                    var res = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = authenticate.ContractCode,
                        FormCode = authenticate.ContractCode,
                        KeyValue = documentGroupId.ToString(),
                        Description = documentGroupModel.Title,
                        NotifEvent = Domain.Enum.NotifEvent.AddDocument,
                        RootKeyValue = documentGroupId.ToString(),
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,
                        DocumentGroupId = documentGroupId,

                    }, null);
                    var documentIds = addDocumentModels.Select(a => a.DocumentId).ToList();
                    var result = await GetDocumentListAsync(documentIds);
                    if (result.Succeeded)
                        return ServiceResultFactory.CreateSuccess(result.Result);
                    return ServiceResultFactory.CreateSuccess(new List<DocumentViewDto>());
                }
                return ServiceResultFactory.CreateError<List<DocumentViewDto>>(null, MessageId.AddEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<DocumentViewDto>>(null, exception);
            }
        }



        public async Task<ServiceResult<List<DocumentLogDto>>> GetDocumentLogAsync(AuthenticateDto authenticate, long documentId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<DocumentLogDto>>(null, MessageId.AccessDenied);

                var documentModel = await _documentRepository.Where(a => !a.IsDeleted && a.DocumentId == documentId)
                    .Select(c => new { documentGroupId = c.DocumentGroupId, documentId = c.DocumentId })
                    .FirstOrDefaultAsync();

                if (documentModel == null)
                    return ServiceResultFactory.CreateError<List<DocumentLogDto>>(null, MessageId.EntityDoesNotExist);


                var events = new List<NotifEvent> {
                    NotifEvent.AddDocument,
                    NotifEvent.EditDocument,
                    NotifEvent.DeActiveDocument,
                    NotifEvent.ActiveDocument
                };

                if (permission.DocumentGroupIds.Any() && !permission.DocumentGroupIds.Contains(documentModel.documentGroupId))
                    return ServiceResultFactory.CreateError<List<DocumentLogDto>>(null, MessageId.AccessDenied);

                var keyvalue = documentId.ToString();
                var result = await _scmAuditLogsRepository
                    .Where(a => a.KeyValue == keyvalue && events.Contains(a.NotifEvent) && a.BaseContractCode == authenticate.ContractCode)
                    .OrderBy(a => a.DateCreate)
                .Select(c => new DocumentLogDto
                {
                    NotifEvent = c.NotifEvent,
                    CreateDate = c.DateCreate.ToUnixTimestamp(),
                    UserAudit = c.PerformerUser != null ? new UserAuditLogDto
                    {
                        AdderUserName = c.PerformerUser.FullName,
                        AdderUserImage = c.PerformerUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + c.PerformerUser.FullName : ""
                    } : new UserAuditLogDto()

                })
                .ToListAsync();

                return ServiceResultFactory.CreateSuccess(result);


            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<DocumentLogDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<List<DocumentViewDto>>> GetDocumentAsync(AuthenticateDto authenticate, DocumentQueryDto query, bool? isRequireTransmittal, bool? type)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<DocumentViewDto>>(null, MessageId.AccessDenied);

                var dbQuery = _documentRepository
                     .AsNoTracking()
                     .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode);

                if (permission.DocumentGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.DocumentGroupIds.Contains(a.DocumentGroupId));

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a =>
                     a.DocTitle.Contains(query.SearchText.Trim())
                     || a.DocNumber.Contains(query.SearchText.Trim())
                     || a.DocRemark.Contains(query.SearchText.Trim())
                     || a.ClientDocNumber.Contains(query.SearchText.Trim()));
                else
                {
                    if (query.PageSize > 100 && query.PageSize != 5000)
                        query.PageSize = 100;
                }
                if (type != null && type == true)
                {
                    dbQuery = dbQuery.Where(a => a.DocumentRevisions != null && a.DocumentRevisions.Any(c => !c.IsDeleted && c.IsLastRevision && c.RevisionStatus == RevisionStatus.TransmittalIFA));
                    dbQuery = dbQuery.Where(a => !(_documentCommunicationRepository.Any(b => a.DocumentRevisions.Any(c => c.DocumentRevisionId == b.DocumentRevisionId))));

                }


                if (EnumHelper.ValidateItem(query.DocClass))
                    dbQuery = dbQuery.Where(a => a.DocClass == query.DocClass);

                if (EnumHelper.ValidateItem(query.RevisionStatuses) /*&& query.RevisionStatus != RevisionStatus.DeActive*/)
                    dbQuery = dbQuery.Where(a => a.DocumentRevisions != null && a.DocumentRevisions.Any(c => !c.IsDeleted && c.IsLastRevision && query.RevisionStatuses.Contains(c.RevisionStatus)));

                if (query.DocumentGroupIds != null && query.DocumentGroupIds.Any())
                    dbQuery = dbQuery.Where(a => query.DocumentGroupIds.Contains(a.DocumentGroupId));

                if (query.ProductIds != null && query.ProductIds.Any())
                    dbQuery = dbQuery.Where(a => a.DocumentProducts.Any(c => query.ProductIds.Contains(c.ProductId)));

                if (query.IsRequiredTransmittal != null)
                    dbQuery = dbQuery.Where(a => a.IsRequiredTransmittal == query.IsRequiredTransmittal);

                if (isRequireTransmittal != null && isRequireTransmittal == true)
                    dbQuery = dbQuery.Where(a => a.IsRequiredTransmittal == true);

                if (query.AreaIds != null && query.AreaIds.Any())
                    dbQuery = dbQuery.Where(a => query.AreaIds.Contains(a.AreaId.Value));

                if (EnumHelper.ValidateItem(query.CommunicationCommentStatus))
                    dbQuery = dbQuery.Where(a => a.CommunicationCommentStatus == query.CommunicationCommentStatus);

                var totalCount = dbQuery.Count();

                var columnsMap = new Dictionary<string, Expression<Func<Document, object>>>
                {
                    ["DocumentId"] = v => v.DocumentId
                };

                dbQuery = dbQuery.ApplayOrdering(query, columnsMap).ApplayPageing(query);

                //dbquery =>  _documentRepository
                var result = await dbQuery.Select(a => new DocumentViewDto
                {
                    DocumentId = a.DocumentId,
                    ContractCode = a.ContractCode,
                    DocTitle = a.DocTitle,
                    DocNumber = a.DocNumber,
                    ClientDocNumber = (a.ClientDocNumber != null) ? a.ClientDocNumber : "",
                    DocRemark = a.DocRemark,
                    DocClass = a.DocClass,
                    CommunicationCommentStatus = a.CommunicationCommentStatus,
                    DocumentGroupCode = a.DocumentGroup.DocumentGroupCode,
                    DocumentGroupTitle = a.DocumentGroup.Title,
                    IsActive = a.IsActive,
                    IsRequiredTransmittal = a.IsRequiredTransmittal,
                    Area = (a.Area != null) ? new AreaReadDTO
                    {
                        AreaId = a.Area.AreaId,
                        AreaTitle = a.Area.AreaTitle
                    } : null,
                    LastRevision = a.DocumentRevisions
                    .Where(c => !c.IsDeleted && c.IsLastRevision)
                    .Select(a => new LastRevisionDto
                    {
                        RevisionId = a.DocumentRevisionId,
                        RevisionCode = a.DocumentRevisionCode,
                        RevisionStatus = a.RevisionStatus
                    }).FirstOrDefault(),
                    DocumentProducts = a.DocumentProducts.Select(a => new ProductInfoDto
                    {
                        ProductDescription = a.Product.Description,
                        ProductCode = a.Product.ProductCode,
                        ProductId = a.ProductId
                    }).ToList()
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<DocumentViewDto>>(null, exception);
            }
        }


        public async Task<ServiceResult<List<DocumentViewDto>>> GetDocumentForCustomerUserAsync(AuthenticateDto authenticate, DocumentQueryDto query, bool? isRequireTransmittal, bool? type, bool accessability)
        {
            try
            {
                if (!accessability)
                    return ServiceResultFactory.CreateError<List<DocumentViewDto>>(null, MessageId.AccessDenied);



                var dbQuery = _documentRepository
                     .AsNoTracking()
                     .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.IsRequiredTransmittal);



                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a =>
                     a.DocTitle.Contains(query.SearchText.Trim())
                     || a.DocNumber.Contains(query.SearchText.Trim())
                     || a.DocRemark.Contains(query.SearchText.Trim())
                     || a.ClientDocNumber.Contains(query.SearchText.Trim()));
                else
                {
                    if (query.PageSize > 100 && query.PageSize != 5000)
                        query.PageSize = 100;
                }

                if (type != null && type == true)
                {
                    dbQuery = dbQuery.Where(a => a.DocumentRevisions != null && a.DocumentRevisions.Any(c => !c.IsDeleted && c.IsLastRevision && c.RevisionStatus == RevisionStatus.TransmittalIFA));
                    dbQuery = dbQuery.Where(a => !(_documentCommunicationRepository.Any(b => a.DocumentRevisions.Any(c => c.DocumentRevisionId == b.DocumentRevisionId))));

                }


                if (EnumHelper.ValidateItem(query.DocClass))
                    dbQuery = dbQuery.Where(a => a.DocClass == query.DocClass);

                if (EnumHelper.ValidateItem(query.RevisionStatuses) /*&& query.RevisionStatus != RevisionStatus.DeActive*/)
                    dbQuery = dbQuery.Where(a => a.DocumentRevisions != null && a.DocumentRevisions.Any(c => !c.IsDeleted && c.IsLastRevision && query.RevisionStatuses.Contains(c.RevisionStatus)));

                if (query.DocumentGroupIds != null && query.DocumentGroupIds.Any())
                    dbQuery = dbQuery.Where(a => query.DocumentGroupIds.Contains(a.DocumentGroupId));

                if (query.ProductIds != null && query.ProductIds.Any())
                    dbQuery = dbQuery.Where(a => a.DocumentProducts.Any(c => query.ProductIds.Contains(c.ProductId)));

                if (query.IsRequiredTransmittal != null)
                    dbQuery = dbQuery.Where(a => a.IsRequiredTransmittal == query.IsRequiredTransmittal);

                if (isRequireTransmittal != null && isRequireTransmittal == true)
                    dbQuery = dbQuery.Where(a => a.IsRequiredTransmittal == true);

                if (query.AreaIds != null && query.AreaIds.Any())
                    dbQuery = dbQuery.Where(a => query.AreaIds.Contains(a.AreaId.Value));

                if (EnumHelper.ValidateItem(query.CommunicationCommentStatus))
                    dbQuery = dbQuery.Where(a => a.CommunicationCommentStatus == query.CommunicationCommentStatus);

                var totalCount = dbQuery.Count();

                var columnsMap = new Dictionary<string, Expression<Func<Document, object>>>
                {
                    ["DocumentId"] = v => v.DocumentId
                };

                dbQuery = dbQuery.ApplayOrdering(query, columnsMap).ApplayPageing(query);

                //dbquery =>  _documentRepository
                var result = await dbQuery.Select(a => new DocumentViewDto
                {
                    DocumentId = a.DocumentId,
                    ContractCode = a.ContractCode,
                    DocTitle = a.DocTitle,
                    DocNumber = a.DocNumber,
                    ClientDocNumber = (a.ClientDocNumber != null) ? a.ClientDocNumber : "",
                    DocRemark = a.DocRemark,
                    DocClass = a.DocClass,
                    CommunicationCommentStatus = a.CommunicationCommentStatus,
                    DocumentGroupCode = a.DocumentGroup.DocumentGroupCode,
                    DocumentGroupTitle = a.DocumentGroup.Title,
                    IsActive = a.IsActive,
                    IsRequiredTransmittal = a.IsRequiredTransmittal,
                    Area = (a.Area != null) ? new AreaReadDTO
                    {
                        AreaId = a.Area.AreaId,
                        AreaTitle = a.Area.AreaTitle
                    } : null,
                    LastRevision = a.DocumentRevisions.OrderByDescending(b => b.CreatedDate)
                    .Where(c => !c.IsDeleted && (c.RevisionStatus==RevisionStatus.TransmittalASB||c.RevisionStatus == RevisionStatus.TransmittalIFC || c.RevisionStatus == RevisionStatus.TransmittalIFI || c.RevisionStatus == RevisionStatus.TransmittalIFA) && _transmittalRepository.Any(d => d.TransmittalRevisions.Any(e => e.DocumentRevisionId == c.DocumentRevisionId) && (d.TransmittalType == TransmittalType.Customer || d.TransmittalType == TransmittalType.Consultant)))
                    .Select(a => new LastRevisionDto
                    {
                        RevisionId = a.DocumentRevisionId,
                        RevisionCode = a.DocumentRevisionCode,
                        RevisionStatus = a.RevisionStatus
                    }).FirstOrDefault(),
                    DocumentProducts = a.DocumentProducts.Select(a => new ProductInfoDto
                    {
                        ProductDescription = a.Product.Description,
                        ProductCode = a.Product.ProductCode,
                        ProductId = a.ProductId
                    }).ToList()
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<DocumentViewDto>>(null, exception);
            }
        }
        public async Task<ServiceResult<List<PendingDocumentForCommentDto>>> GetPenndingDocumentForCommentsAsync(AuthenticateDto authenticate, DocumentQueryDto query, bool? isRequireTransmittal, bool? type, bool accessability)
        {
            try
            {
                if (!accessability)
                    return ServiceResultFactory.CreateError<List<PendingDocumentForCommentDto>>(null, MessageId.AccessDenied);



                var dbQuery = _documentRevisionRepository
                     .AsNoTracking()
                     .Include(a => a.Document)
                     .Include(a => a.DocumentCommunications)
                     .Where(a => !a.IsDeleted && a.Document.ContractCode == authenticate.ContractCode);



                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a =>
                     a.Document.DocTitle.Contains(query.SearchText)
                     || _transmittalRepository
                      .Any(d => (d.TransmittalType == TransmittalType.Customer || d.TransmittalType == TransmittalType.Consultant) &&
                      d.ContractCode == authenticate.ContractCode &&
                      d.TransmittalRevisions.Any(c => c.DocumentRevisionId == a.DocumentRevisionId && c.Transmittal.TransmittalNumber.Contains(query.SearchText)))
                     || a.Document.DocNumber.Contains(query.SearchText)
                     || a.Document.ClientDocNumber.Contains(query.SearchText));
                if (type != null && type == true)
                {
                    dbQuery = dbQuery.Where(a => !a.IsDeleted && a.RevisionStatus == RevisionStatus.TransmittalIFA && a.TransmittalRevisions.Any(d => d.Transmittal.TransmittalType == TransmittalType.Customer || d.Transmittal.TransmittalType == TransmittalType.Consultant));
                    dbQuery = dbQuery.Where(a => !a.DocumentCommunications.Any());
                }




                if (query.DocumentGroupIds != null && query.DocumentGroupIds.Any())
                    dbQuery = dbQuery.Where(a => query.DocumentGroupIds.Contains(a.Document.DocumentGroupId));



                if (isRequireTransmittal != null && isRequireTransmittal == true)
                    dbQuery = dbQuery.Where(a => a.Document.IsRequiredTransmittal == true);



                var totalCount = dbQuery.Count();

                var columnsMap = new Dictionary<string, Expression<Func<Document, object>>>
                {
                    ["DocumentId"] = v => v.DocumentId
                };
                dbQuery = dbQuery.ApplayPageing(query);
                dbQuery = dbQuery.OrderBy(a => a.DocumentId);

                //dbquery =>  _documentRepository
                var result = await dbQuery.Select(a => new PendingDocumentForCommentDto
                {
                    DocumentId = a.Document.DocumentId,
                    ContractCode = a.Document.ContractCode,
                    DocTitle = a.Document.DocTitle,
                    DocNumber = a.Document.DocNumber,
                    ClientDocNumber = a.Document.ClientDocNumber,
                    DocRemark = a.Document.DocRemark,
                    DocClass = a.Document.DocClass,
                    CommunicationCommentStatus = a.Document.CommunicationCommentStatus,
                    DocumentGroupCode = a.Document.DocumentGroup.DocumentGroupCode,
                    DocumentGroupTitle = a.Document.DocumentGroup.Title,
                    IsActive = a.Document.IsActive,
                    IsRequiredTransmittal = a.Document.IsRequiredTransmittal,
                    Area = (a.Document.AreaId != null) ? _areaRepository.Where(b => !b.IsDeleted && b.AreaId == a.Document.AreaId).Select(c => new AreaReadDTO
                    {
                        AreaId = c.AreaId,
                        AreaTitle = c.AreaTitle
                    }).FirstOrDefault()
                     : null,
                    LastRevision = new LastRevisionDto
                    {
                        RevisionId = a.DocumentRevisionId,
                        RevisionCode = a.DocumentRevisionCode,
                        RevisionStatus = a.RevisionStatus
                    }
                }).ToListAsync();

                result.ForEach(doc =>
                {
                    var transmittal = _transmittalRepository.GetTransmittlNumber(doc.LastRevision.RevisionId, authenticate.ContractCode).Result;
                    var splitDateNumber = transmittal.Split(",");
                    if (splitDateNumber.Length > 1)
                    {
                        doc.TransmittalNumber = splitDateNumber[0];
                        doc.TransmittalDate = splitDateNumber[1];
                    }
                    else
                    {
                        doc.TransmittalNumber = "";
                        doc.TransmittalDate = "";
                    }

                });
                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<PendingDocumentForCommentDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<List<DocumentMiniOnfoDto>>> GetActiveForAddRevisionDocumentAsync(AuthenticateDto authenticate, DocumentQueryDto query)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<DocumentMiniOnfoDto>>(null, MessageId.AccessDenied);

                var dbQuery = _documentRepository
                     .AsNoTracking()
                     .Where(a => !a.IsDeleted && a.IsActive && a.ContractCode == authenticate.ContractCode);

                if (permission.DocumentGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.DocumentGroupIds.Contains(a.DocumentGroupId));

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a =>
                     a.DocTitle.Contains(query.SearchText)
                     || a.DocNumber.Contains(query.SearchText)
                     || a.ClientDocNumber.Contains(query.SearchText));

                if (EnumHelper.ValidateItem(query.DocClass))
                    dbQuery = dbQuery.Where(a => a.DocClass == query.DocClass);

                if (query.DocumentGroupIds != null && query.DocumentGroupIds.Any())
                    dbQuery = dbQuery.Where(a => query.DocumentGroupIds.Contains(a.DocumentGroupId));

                var totalCount = dbQuery.Count();

                var columnsMap = new Dictionary<string, Expression<Func<Document, object>>>
                {
                    ["DocumentId"] = v => v.DocumentId
                };

                dbQuery = dbQuery
                    .ApplayOrdering(query, columnsMap)
                    .AsQueryable();

                var result = await dbQuery.Select(a => new DocumentMiniOnfoDto
                {
                    DocumentId = a.DocumentId,
                    DocTitle = a.DocTitle,
                    DocNumber = a.DocNumber,
                    LastRevision = a.DocumentRevisions
                    .Where(a => !a.IsDeleted && a.IsLastRevision == true)
                    .Select(c => new LastRevisionDto
                    {
                        RevisionId = c.DocumentRevisionId,
                        RevisionCode = c.DocumentRevisionCode,
                        RevisionStatus = c.RevisionStatus
                    }).FirstOrDefault() ?? new LastRevisionDto()
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<DocumentMiniOnfoDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<List<RevisionMiniInfoDto>>> GetLastTransmittalRevisionAsync(AuthenticateDto authenticate, DocumentQueryDto query)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<RevisionMiniInfoDto>>(null, MessageId.AccessDenied);

                var dbQuery = _documentRevisionRepository
                     .AsNoTracking()
                     .Where(a => !a.IsDeleted &&
                     a.IsLastConfirmRevision &&
                     a.Document.IsActive &&
                     !a.Document.IsDeleted &&
                     a.Document.ContractCode == authenticate.ContractCode &&
                     a.RevisionStatus >= RevisionStatus.Confirmed)
                     .OrderByDescending(a => a.DocumentRevisionId)
                     .AsQueryable();

                if (permission.DocumentGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.DocumentGroupIds.Contains(a.Document.DocumentGroupId));

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a => a.Description.Contains(query.SearchText)
                     || a.DocumentRevisionCode.Contains(query.SearchText));

                var totalCount = dbQuery.Count();

                //var columnsMap = new Dictionary<string, Expression<Func<DocumentRevision, object>>>
                //{
                //    ["DocumentRevisionId"] = v => v.DocumentRevisionId
                //};

                //dbQuery = dbQuery
                //    .ApplayPageing(query)
                //    .AsQueryable();

                var result = await dbQuery.Select(a => new RevisionMiniInfoDto
                {
                    DocTitle = a.Document.DocTitle,
                    DocNumber = a.Document.DocNumber,
                    RevisionCode = a.DocumentRevisionCode,
                    RevisionId = a.DocumentRevisionId,
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<RevisionMiniInfoDto>>(null, exception);
            }
        }
        public async Task<ServiceResult<List<RevisionMiniInfoDto>>> GetLastTransmittalRevisionForCustomerAsync(AuthenticateDto authenticate, DocumentQueryDto query, bool accessability)
        {
            try
            {
                if (!accessability)
                    return ServiceResultFactory.CreateError<List<RevisionMiniInfoDto>>(null, MessageId.AccessDenied);




                var dbQuery = _documentRevisionRepository
                     .AsNoTracking()
                     .Where(a => !a.IsDeleted &&
                     a.TransmittalRevisions.Any(b => b.Transmittal.TransmittalType == TransmittalType.Customer || b.Transmittal.TransmittalType == TransmittalType.Consultant) &&
                     a.IsLastConfirmRevision &&
                     a.Document.IsActive &&
                     !a.Document.IsDeleted &&
                     a.Document.ContractCode == authenticate.ContractCode &&
                     a.RevisionStatus >= RevisionStatus.Confirmed)
                     .OrderByDescending(a => a.DocumentRevisionId)
                     .AsQueryable();



                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a => a.Description.Contains(query.SearchText)
                     || a.DocumentRevisionCode.Contains(query.SearchText));

                var totalCount = dbQuery.Count();

                //var columnsMap = new Dictionary<string, Expression<Func<DocumentRevision, object>>>
                //{
                //    ["DocumentRevisionId"] = v => v.DocumentRevisionId
                //};

                //dbQuery = dbQuery
                //    .ApplayPageing(query)
                //    .AsQueryable();

                var result = await dbQuery.Select(a => new RevisionMiniInfoDto
                {
                    DocTitle = a.Document.DocTitle,
                    DocNumber = a.Document.DocNumber,
                    RevisionCode = a.DocumentRevisionCode,
                    RevisionId = a.DocumentRevisionId,
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<RevisionMiniInfoDto>>(null, exception);
            }
        }
        public async Task<ServiceResult<RevisionMiniInfoForCusomerUserDto>> GetLastTransmittalRevisionForDocumentAsync(AuthenticateDto authenticate, long revisionId, bool accessability)
        {
            try
            {
                if (!accessability)
                    return ServiceResultFactory.CreateError<RevisionMiniInfoForCusomerUserDto>(null, MessageId.AccessDenied);



                var dbQuery = _documentRevisionRepository
                     .AsNoTracking()
                     .Where(a => !a.IsDeleted &&
                     a.Document.IsActive &&
                     !a.Document.IsDeleted &&
                     a.Document.ContractCode == authenticate.ContractCode &&
                     a.RevisionStatus >= RevisionStatus.Confirmed &&
                     a.DocumentRevisionId == revisionId)
                     .OrderByDescending(a => a.DocumentRevisionId)
                     .AsQueryable();




                var user = await _userRepository.FindAsync(authenticate.UserId);
                var totalCount = dbQuery.Count();
                var contract = await _contractRepository
                   .AsNoTracking()
                   .Where(a => !a.IsDeleted &&
                   a.ContractCode == authenticate.ContractCode)
                   .Select(a => new CurrentContractInfoDto
                   {
                       ContractCode = a.ContractCode,
                       ContractDescription = a.Description,
                       CustomerCode = (user.UserType == (int)UserStatus.CustomerUser) ? _customerRepository.FirstOrDefault(b => !b.IsDeleted && b.CustomerUsers.Any(c => c.Email.ToLower() == authenticate.UserName.ToLower())).CustomerCode : _consultantRepository.FirstOrDefault(b => !b.IsDeleted && b.ConsultantUsers.Any(c => c.Email.ToLower() == authenticate.UserName.ToLower())).ConsultantCode,
                       CustomerId = (user.UserType == (int)UserStatus.CustomerUser) ? _customerRepository.FirstOrDefault(b => !b.IsDeleted && b.CustomerUsers.Any(c => c.Email.ToLower() == authenticate.UserName.ToLower())).Id : _consultantRepository.FirstOrDefault(b => !b.IsDeleted && b.ConsultantUsers.Any(c => c.Email.ToLower() == authenticate.UserName.ToLower())).Id,
                       CustomerName = (user.UserType == (int)UserStatus.CustomerUser) ? _customerRepository.FirstOrDefault(b => !b.IsDeleted && b.CustomerUsers.Any(c => c.Email.ToLower() == authenticate.UserName.ToLower())).Name : _consultantRepository.FirstOrDefault(b => !b.IsDeleted && b.ConsultantUsers.Any(c => c.Email.ToLower() == authenticate.UserName.ToLower())).Name
                   }).FirstOrDefaultAsync();
                //var columnsMap = new Dictionary<string, Expression<Func<DocumentRevision, object>>>
                //{
                //    ["DocumentRevisionId"] = v => v.DocumentRevisionId
                //};

                //dbQuery = dbQuery
                //    .ApplayPageing(query)
                //    .AsQueryable();

                var result = await dbQuery.Select(a => new RevisionMiniInfoForCusomerUserDto
                {
                    DocTitle = a.Document.DocTitle,
                    DocNumber = a.Document.DocNumber,
                    RevisionCode = a.DocumentRevisionCode,
                    DocumentGroupTitle = a.Document.DocumentGroup.Title,
                    DocumentGroupCode = a.Document.DocumentGroup.DocumentGroupCode,
                    DocClass = a.Document.DocClass,
                    CurrentContractInfo = contract,
                    CustomerName = (user.UserType == (int)UserStatus.CustomerUser) ? _customerRepository.FirstOrDefault(b => !b.IsDeleted && b.CustomerUsers.Any(c => c.Email.ToLower() == authenticate.UserName.ToLower())).Name : _consultantRepository.FirstOrDefault(b => !b.IsDeleted && b.ConsultantUsers.Any(c => c.Email.ToLower() == authenticate.UserName.ToLower())).Name,

                    RevisionId = a.DocumentRevisionId,
                }).FirstOrDefaultAsync();

                var transmittal = _transmittalRepository.GetTransmittlNumber(result.RevisionId, authenticate.ContractCode).Result;
                var splitDateNumber = transmittal.Split(",");
                if (splitDateNumber.Length > 1)
                {
                    result.TransmittalNumber = splitDateNumber[0];
                    result.TransmittalDate = splitDateNumber[1];
                }
                else
                {
                    result.TransmittalNumber = "";
                    result.TransmittalDate = "";
                }

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<RevisionMiniInfoForCusomerUserDto>(null, exception);
            }
        }

        public async Task<ServiceResult<DocumentDetailsDto>> GetDocumentByIdAsync(AuthenticateDto authenticate, long documentId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<DocumentDetailsDto>(null, MessageId.AccessDenied);

                var dbQuery = _documentRepository
                     .AsNoTracking()
                     .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.DocumentId == documentId);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<DocumentDetailsDto>(null, MessageId.EntityDoesNotExist);

                if (permission.DocumentGroupIds.Any() && !dbQuery.Any(a => permission.DocumentGroupIds.Contains(a.DocumentGroupId)))
                    return ServiceResultFactory.CreateError<DocumentDetailsDto>(null, MessageId.AccessDenied);

                var result = await dbQuery.Select(a => new DocumentDetailsDto
                {
                    DocumentId = a.DocumentId,
                    ContractCode = a.ContractCode,
                    DocTitle = a.DocTitle,
                    DocNumber = a.DocNumber,
                    ClientDocNumber = a.ClientDocNumber,
                    DocRemark = a.DocRemark,
                    DocClass = a.DocClass,
                    DocumentGroupCode = a.DocumentGroup.DocumentGroupCode,
                    DocumentGroupTitle = a.DocumentGroup.Title,
                    IsActive = a.IsActive,
                    DocumentProducts = a.DocumentProducts.Select(a => new ProductInfoDto
                    {
                        ProductDescription = a.Product.Description,
                        ProductCode = a.Product.ProductCode,
                        ProductId = a.ProductId
                    }).ToList()
                }).FirstOrDefaultAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<DocumentDetailsDto>(null, exception);
            }
        }

        public async Task<ServiceResult<bool>> ChangeActiveStateOfDocumentAsync(AuthenticateDto authenticate, long documentId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var documentModel = await _documentRepository
                     .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.DocumentId == documentId)
                     .Include(a => a.DocumentGroup)
                     .FirstOrDefaultAsync();

                if (documentModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (permission.DocumentGroupIds.Any() && !permission.DocumentGroupIds.Contains(documentModel.DocumentGroupId))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                documentModel.IsActive = !documentModel.IsActive;
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var res = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = authenticate.ContractCode,
                        FormCode = authenticate.ContractCode,
                        KeyValue = documentModel.DocumentId.ToString(),
                        Description = documentModel.DocTitle,
                        NotifEvent = documentModel.IsActive ? NotifEvent.ActiveDocument : NotifEvent.DeActiveDocument,
                        RootKeyValue = documentModel.DocumentId.ToString(),
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,
                        DocumentGroupId = documentModel.DocumentGroupId
                    }, null);
                    return ServiceResultFactory.CreateSuccess(true);
                }
                return ServiceResultFactory.CreateError(false, MessageId.SaveFailed);

            }
            catch (Exception exception)
            {

                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<bool>> EditDocumentByDocumentIdAsync(AuthenticateDto authenticate, long documentId, AddDocumentDto model)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var documentModel = await _documentRepository
                     .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.DocumentId == documentId)
                     .Include(a => a.DocumentProducts)
                     .Include(a => a.DocumentGroup)
                     .FirstOrDefaultAsync();

                if (documentModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (permission.DocumentGroupIds.Any() && !permission.DocumentGroupIds.Contains(documentModel.DocumentGroupId))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                if (!EnumHelper.ValidateItem(model.DocClass))
                    return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                if (await _documentRepository.AnyAsync(a => !a.IsDeleted && a.DocumentId != documentId && a.DocNumber == model.DocNumber))
                    return ServiceResultFactory.CreateError(false, MessageId.DuplicatDocNumber);

                if (model.AreaId != null)
                {

                    if (!await _areaRepository.AnyAsync(a => a.AreaId == model.AreaId && a.ContractCode == authenticate.ContractCode))
                    {
                        return ServiceResultFactory.CreateError(false, MessageId.AreaNotExist);
                    }

                }


                documentModel.DocClass = model.DocClass;
                documentModel.DocNumber = model.DocNumber;
                documentModel.DocRemark = model.DocRemark;
                documentModel.DocTitle = model.DocTitle;
                documentModel.IsRequiredTransmittal = model.IsRequiredTransmittal;
                documentModel.ClientDocNumber = model.ClientDocNumber;
                documentModel.AreaId = model.AreaId;

                if (model.ProductIds != null && model.ProductIds.Any())
                {
                    var acceptBomProducts = await _bomProductService.GetLastChildProductIdsOfContractbomAsync(authenticate.ContractCode);
                    if (!acceptBomProducts.Succeeded)
                        return ServiceResultFactory.CreateError(false, MessageId.BomNotFound);

                    if (model.ProductIds.Any(a => !acceptBomProducts.Result.Contains(a)))
                        return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                    documentModel = UpdateDocumentProducts(documentModel, model.ProductIds);
                }
                else if (documentModel.DocumentProducts != null && documentModel.DocumentProducts.Any())
                {

                    documentModel.DocumentProducts.Clear();
                }


                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var res = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = authenticate.ContractCode,
                        Message = documentModel.DocumentGroup.Title,
                        FormCode = documentModel.DocNumber,
                        KeyValue = documentModel.DocumentId.ToString(),
                        Description = documentModel.DocTitle,
                        NotifEvent = NotifEvent.EditDocument,
                        RootKeyValue = documentModel.DocumentId.ToString(),
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,
                        DocumentGroupId = documentModel.DocumentGroupId,
                    }, null);

                    return ServiceResultFactory.CreateSuccess(true);
                }
                return ServiceResultFactory.CreateError(false, MessageId.SaveFailed);

            }
            catch (Exception exception)
            {

                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        private Document UpdateDocumentProducts(Document documentModel, List<int> productIds)
        {
            if (productIds == null || productIds.Where(a => a > 0).Count() == 0)
            {
                foreach (var item in documentModel.DocumentProducts)
                {
                    documentModel.DocumentProducts.Remove(item);
                }
                return documentModel;
            }

            if (documentModel.DocumentProducts == null || documentModel.DocumentProducts.Count() == 0)
            {
                documentModel.DocumentProducts = new List<DocumentProduct>();
                documentModel.DocumentProducts = productIds.Select(p => new DocumentProduct
                {
                    DocumentId = documentModel.DocumentId,
                    ProductId = p
                }).ToList();
            }

            var beforeProductIds = documentModel.DocumentProducts.Select(a => a.ProductId).ToList();

            var removeItem = documentModel.DocumentProducts.Where(a => !productIds.Contains(a.ProductId)).ToList();
            foreach (var item in removeItem)
            {
                documentModel.DocumentProducts.Remove(item);
            }


            var addItem = productIds.Where(a => !beforeProductIds.Contains(a))
                .Select(p => new DocumentProduct
                {
                    DocumentId = documentModel.DocumentId,
                    ProductId = p
                }).ToList();

            foreach (var item in addItem)
            {
                documentModel.DocumentProducts.Add(item);
            }

            return documentModel;

        }

        public async Task<DownloadFileDto> ExportDocumentListAsync(AuthenticateDto authenticate)
        {
            var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
            if (!permission.HasPermission)
                return null;

            var dbQuery = _documentRepository
                 .AsNoTracking()
                 .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode);

            if (permission.DocumentGroupIds.Any())
                dbQuery = dbQuery.Where(a => permission.DocumentGroupIds.Contains(a.DocumentGroupId));

            dbQuery = dbQuery.OrderByDescending(a => a.DocumentId).AsQueryable();

            var result = await dbQuery.Select(a => new ExportExcelDocumentDto
            {
                DocTitle = a.DocTitle,
                DocNumber = a.DocNumber,
                ClientDocNumber = a.ClientDocNumber,
                DocRemark = a.DocRemark,
                DocClass = a.DocClass,
                DocumentGroupCode = a.DocumentGroup.DocumentGroupCode,
                DocumentGroupTitle = a.DocumentGroup.Title,
                IsActive = a.IsActive,
                AreaTitle=a.Area!=null?a.Area.AreaTitle:"",
                LastRevisionCode=a.DocumentRevisions.Any(b=>!b.IsDeleted&&b.IsLastRevision)?a.DocumentRevisions.First(b=>!b.IsDeleted&&b.IsLastRevision).DocumentRevisionCode:"",
                LastRevisionStatus= a.DocumentRevisions.Any(b => !b.IsDeleted && b.IsLastRevision) ? a.DocumentRevisions.First(b => !b.IsDeleted && b.IsLastRevision).RevisionStatus : (RevisionStatus)(-1),
                CommentStatus=a.CommunicationCommentStatus

            }).ToListAsync();

            return ExcelHelper.ExportDocumentsToExcel(result, $"{authenticate.ContractCode}-Latest Documents Status List", "document");
        }
        public async Task<DownloadFileDto> ExportDocumentsHistoryAsync(AuthenticateDto authenticate)
        {
            var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
            if (!permission.HasPermission)
                return null;

            var dbQuery = _documentRepository
                 .AsNoTracking()
                 .Include(a=>a.DocumentRevisions)
                 .ThenInclude(a=>a.TransmittalRevisions)
                 .ThenInclude(a=>a.Transmittal)
                 .Include(a=>a.DocumentRevisions)
                 .ThenInclude(a=>a.DocumentCommunications)
                 .ThenInclude(a=>a.CommunicationQuestions)
                 .ThenInclude(a=>a.CommunicationReply)
                 .Include(a=>a.Area)
                 .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode);

            if (permission.DocumentGroupIds.Any())
                dbQuery = dbQuery.Where(a => permission.DocumentGroupIds.Contains(a.DocumentGroupId));

            dbQuery = dbQuery.OrderByDescending(a => a.DocumentId).AsQueryable();

            var result = await dbQuery.ToListAsync();

            return ExcelHelper.ExportDocumentsHistoryToExcel(result, $"{authenticate.ContractCode}-Latest Revisions Status List", "document");
        }
        public async Task<DownloadFileDto> ExportDocumentsRevisionHistoryExcelAsync(AuthenticateDto authenticate)
        {
            var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
            if (!permission.HasPermission)
                return null;

            var dbQuery = _documentRevisionRepository
                 .AsNoTracking()
                 .Include(a=>a.AdderUser)
                 .Include(a => a.ConfirmationWorkFlows)
                 .ThenInclude(a=>a.ModifierUser)
                 .Include(a=>a.Document)
                 .Include(a=>a.TransmittalRevisions)
                 .ThenInclude(a=>a.Transmittal)
                 .ThenInclude(a => a.AdderUser)
                 .Include(a=>a.DocumentCommunications)
                 .ThenInclude(a=>a.CommunicationQuestions)
                 .ThenInclude(a => a.AdderUser)
                 .Include(a => a.DocumentCommunications)
                 .ThenInclude(a => a.CommunicationQuestions)
                 .ThenInclude(a=>a.CommunicationReply)
                 .ThenInclude(a => a.AdderUser)
                 .Include(a=>a.DocumentTQNCRs)
                 .ThenInclude(a=>a.AdderUser)
                 .Include(a => a.DocumentTQNCRs)
                 .ThenInclude(a => a.CommunicationQuestions)
                 .ThenInclude(a=>a.CommunicationReply)
                 .ThenInclude(a=>a.AdderUser)
                 .Where(a => !a.IsDeleted &&!a.Document.IsDeleted&& a.Document.ContractCode == authenticate.ContractCode);

            if (permission.DocumentGroupIds.Any())
                dbQuery = dbQuery.Where(a => permission.DocumentGroupIds.Contains(a.Document.DocumentGroupId));

            dbQuery = dbQuery.OrderByDescending(a => a.DocumentId).ThenBy(a=>a.CreatedDate).AsQueryable();

            var result = await dbQuery.ToListAsync();

            return ExcelHelper.ExportDocumentsRevisionHistoryToExcel(result, $"{authenticate.ContractCode}-Revisions History List", "document");
        }
        public async Task<DownloadFileDto> ExportDocumentListForCustomerUserAsync(AuthenticateDto authenticate,bool accessability)
        {
            if (!accessability)
                return null;

            var dbQuery = _documentRepository
                 .AsNoTracking()
                 .Where(a => !a.IsDeleted &&a.IsActive&&a.IsRequiredTransmittal && a.ContractCode == authenticate.ContractCode);

           

            dbQuery = dbQuery.OrderByDescending(a => a.DocumentId).AsQueryable();

            var result = await dbQuery.Select(a => new ExportExcelDocumentDto
            {
                DocTitle = a.DocTitle,
                DocNumber = a.DocNumber,
                ClientDocNumber = a.ClientDocNumber,
                DocRemark = a.DocRemark,
                DocClass = a.DocClass,
                DocumentGroupCode = a.DocumentGroup.DocumentGroupCode,
                DocumentGroupTitle = a.DocumentGroup.Title,
                IsActive = a.IsActive,
                AreaTitle = a.Area != null ? a.Area.AreaTitle : "",
                LastRevisionCode = a.DocumentRevisions.Any(b => !b.IsDeleted && b.IsLastRevision) ? a.DocumentRevisions.First(b => !b.IsDeleted && b.IsLastRevision).DocumentRevisionCode : "",
                LastRevisionStatus = a.DocumentRevisions.Any(b => !b.IsDeleted && b.IsLastRevision) ? a.DocumentRevisions.First(b => !b.IsDeleted && b.IsLastRevision).RevisionStatus : (RevisionStatus)(-1),
                CommentStatus = a.CommunicationCommentStatus

            }).ToListAsync();

            return ExcelHelper.ExportDocumentsToExcel(result, $"{authenticate.ContractCode}-Latest Documents Status List", "document");
        }
        public async Task<DownloadFileDto> ExportDocumentsHistoryForCustomerUserAsync(AuthenticateDto authenticate, bool accessability)
        {
            if(!accessability)
                return null;

            var dbQuery = _documentRepository
                 .AsNoTracking()
                 .Include(a => a.DocumentRevisions)
                 .ThenInclude(a => a.TransmittalRevisions)
                 .ThenInclude(a => a.Transmittal)
                 .Include(a => a.DocumentRevisions)
                 .ThenInclude(a => a.DocumentCommunications)
                 .ThenInclude(a => a.CommunicationQuestions)
                 .ThenInclude(a => a.CommunicationReply)
                 .Include(a => a.Area)
                 .Where(a => !a.IsDeleted && a.IsActive && a.IsRequiredTransmittal && a.ContractCode == authenticate.ContractCode);

           

            dbQuery = dbQuery.OrderByDescending(a => a.DocumentId).AsQueryable();

            var result = await dbQuery.ToListAsync();

            return ExcelHelper.ExportDocumentsHistoryToExcel(result, $"{authenticate.ContractCode}-Latest Revisions Status List", "document");
        }
        public async Task<DownloadFileDto> ExportDocumentsRevisionHistoryExcelForCustomerUserAsync(AuthenticateDto authenticate,bool accessability)
        {
            if (!accessability)
                return null;

            var dbQuery = _documentRevisionRepository
                 .AsNoTracking()
                 .Include(a => a.AdderUser)
                 .Include(a => a.ConfirmationWorkFlows)
                 .ThenInclude(a => a.ModifierUser)
                 .Include(a => a.Document)
                 .Include(a => a.TransmittalRevisions)
                 .ThenInclude(a => a.Transmittal)
                 .ThenInclude(a => a.AdderUser)
                 .Include(a => a.DocumentCommunications)
                 .ThenInclude(a => a.CommunicationQuestions)
                 .ThenInclude(a => a.AdderUser)
                 .Include(a => a.DocumentCommunications)
                 .ThenInclude(a => a.CommunicationQuestions)
                 .ThenInclude(a => a.CommunicationReply)
                 .ThenInclude(a => a.AdderUser)
                 .Include(a => a.DocumentTQNCRs)
                 .ThenInclude(a => a.AdderUser)
                 .Include(a => a.DocumentTQNCRs)
                 .ThenInclude(a => a.CommunicationQuestions)
                 .ThenInclude(a => a.CommunicationReply)
                 .ThenInclude(a => a.AdderUser)
                 .Where(a => !a.IsDeleted && a.Document.IsActive && a.Document.IsRequiredTransmittal &&!a.Document.IsDeleted && a.Document.ContractCode == authenticate.ContractCode);

           

            dbQuery = dbQuery.OrderByDescending(a => a.DocumentId).ThenBy(a => a.CreatedDate).AsQueryable();

            var result = await dbQuery.ToListAsync();

            return ExcelHelper.ExportDocumentsRevisionHistoryToExcel(result, $"{authenticate.ContractCode}-Revisions History List", "document");
        }
        public async Task<DownloadFileDto> DownloadImportTemplateAsync(AuthenticateDto authenticate)
        {
            var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
            if (!permission.HasPermission)
                return null;

            var exportModelTempelate = new List<ImportExcelDocumentDto>
            {
                new ImportExcelDocumentDto
                {
                    //DocumentId=1,
                    DocNumber="N-700",
                    DocTitle="doc-700",
                    DocClass=DocumentClass.FA,
                    ClientDocNumber="c-700",
                    DocRemark="des-700",
                    //IsActive=true,
                    //DocumentGroupId=1,
                    //ContractCode="c-0",
                }
            };

            return ExcelHelper.ExportToExcel(exportModelTempelate, "MasterMR", "document");
        }

        public async Task<ServiceResult<List<AddDocumentDto>>> ReadImportDocumentExcelFileAsync(AuthenticateDto authenticate, IFormFile formFile)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<AddDocumentDto>>(null, MessageId.AccessDenied);

                if (formFile == null || formFile.Length <= 0)
                {
                    return ServiceResultFactory.CreateError<List<AddDocumentDto>>(null, MessageId.FileNotFound);
                }

                if (!Path.GetExtension(formFile.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                {
                    return ServiceResultFactory.CreateError<List<AddDocumentDto>>(null,
                        MessageId.InvalidFileExtention);
                }

                var list = new List<AddDocumentDto>();

                using (var stream = new MemoryStream())
                {
                    await formFile.CopyToAsync(stream);

                    using (var package = new ExcelPackage(stream))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                        var rowCount = worksheet.Dimension.Rows;
                        try
                        {
                            for (int row = 2; row <= rowCount; row++)
                            {
                                var item = new AddDocumentDto();

                                item.DocNumber = worksheet.Cells[row, 1].Value.ToString().Trim();
                                item.ClientDocNumber = worksheet.Cells[row, 2].Value.ToString().Trim();
                                item.DocTitle = worksheet.Cells[row, 3].Value.ToString().Trim();
                                item.DocRemark = worksheet.Cells[row, 4].Value.ToString().Trim();
                                item.DocClass = worksheet.Cells[row, 5].Value.ToString().Trim().ToEnum<DocumentClass>();
                                list.Add(item);
                            }
                        }
                        catch (Exception exception)
                        {
                            return ServiceResultFactory.CreateException<List<AddDocumentDto>>(null,
                               exception);
                        }
                    }
                }

                if (list == null || list.Count() == 0)
                    return ServiceResultFactory.CreateError<List<AddDocumentDto>>(null,
                        MessageId.InputDataValidationError);
                else
                    return ServiceResultFactory.CreateSuccess(list);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<AddDocumentDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<DocumentArchiveInfoDto>> GetDocumentArchiveAsync(AuthenticateDto authenticate, long documentId)
        {
            try
            {

                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<DocumentArchiveInfoDto>(null, MessageId.AccessDenied);

                var dbQuery = _documentRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.DocumentId == documentId && a.ContractCode == authenticate.ContractCode);

                if (permission.DocumentGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.DocumentGroupIds.Contains(a.DocumentGroupId));

                var result = await dbQuery
                    .Select(a => new DocumentArchiveInfoDto
                    {
                        DocumentId = a.DocumentId,
                        ContractCode = a.ContractCode,
                        DocTitle = a.DocTitle,
                        DocNumber = a.DocNumber,
                        ClientDocNumber = a.ClientDocNumber,
                        DocRemark = a.DocRemark,
                        DocClass = a.DocClass,
                        DocumentGroupCode = a.DocumentGroup.DocumentGroupCode,
                        DocumentGroupTitle = a.DocumentGroup.Title,
                        IsActive = a.IsActive,
                        UserAudit = a.AdderUser != null ? new UserAuditLogDto
                        {
                            AdderUserName = a.AdderUser.FullName,
                            CreateDate = a.CreatedDate.ToUnixTimestamp(),
                            AdderUserImage = a.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + a.AdderUser.Image : "",

                        } : null,
                        Revisions = a.DocumentRevisions.Where(c => !c.IsDeleted).OrderBy(c => c.CreatedDate)
                        .Select(c => new RevisionArchiveDto
                        {
                            DocumentRevisionId = c.DocumentRevisionId,
                            DocumentRevisionCode = c.DocumentRevisionCode,
                            RevisionStatus = c.RevisionStatus,
                            Description = c.Description,
                            UserAudit = c.AdderUser != null ? new UserAuditLogDto
                            {
                                AdderUserName = c.AdderUser.FullName,
                                CreateDate = c.CreatedDate.ToUnixTimestamp(),
                                AdderUserImage = c.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + c.AdderUser.Image : "",
                            } : null,

                            ActivityUsers = c.RevisionActivities != null && c.RevisionActivities.Any() ?
                            c.RevisionActivities.Where(b => !b.IsDeleted).Select(v => v.ActivityOwnerId).Distinct().Count() : 0,

                            FinalAttachment = c.RevisionStatus >= RevisionStatus.Confirmed ?
                            c.RevisionAttachments.Count(a => !a.IsDeleted && a.RevisionAttachmentType == RevisionAttachmentType.Final) : 0,

                            NativeAttachment = c.RevisionStatus >= RevisionStatus.Confirmed ?
                            c.RevisionAttachments.Count(a => !a.IsDeleted && a.RevisionAttachmentType == RevisionAttachmentType.FinalNative) : 0,

                            TransmittalDate = (c.RevisionStatus == RevisionStatus.TransmittalIFA || c.RevisionStatus == RevisionStatus.TransmittalIFC || c.RevisionStatus == RevisionStatus.TransmittalIFI || c.RevisionStatus == RevisionStatus.TransmittalASB) && c.TransmittalRevisions.Any(v => v.Transmittal.TransmittalType == TransmittalType.Customer || v.Transmittal.TransmittalType == TransmittalType.Consultant) ?
                            c.TransmittalRevisions.Where(v => v.Transmittal.TransmittalType == TransmittalType.Customer || v.Transmittal.TransmittalType == TransmittalType.Consultant).Select(v => v.Transmittal.CreatedDate).FirstOrDefault().ToUnixTimestamp() :
                            (long?)null,

                            ConfirmWorkFlow = c.RevisionStatus >= RevisionStatus.Confirmed ? c.ConfirmationWorkFlows.Where(a => a.Status == ConfirmationWorkFlowStatus.Confirm)
                            .Select(n => new ArchiveConfirmationWorkFlowDto
                            {
                                ConfirmationUsers = n.ConfirmationWorkFlowUsers.Count(),
                                CreateDate = n.CreatedDate.ToUnixTimestamp(),
                                ConfirmDate = n.UpdateDate.ToUnixTimestamp()
                            }).FirstOrDefault() : null

                        }).ToList()
                    }).FirstOrDefaultAsync();

                if (result == null)
                    return ServiceResultFactory.CreateError<DocumentArchiveInfoDto>(null, MessageId.EntityDoesNotExist);

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<DocumentArchiveInfoDto>(null, exception);
            }
        }
        public async Task<ServiceResult<DocumentArchiveInfoForCustomerUserDto>> GetDocumentArchiveForCustomerUserAsync(AuthenticateDto authenticate, long documentId, bool accessability)
        {
            try
            {
                if (!accessability)
                    return ServiceResultFactory.CreateError<DocumentArchiveInfoForCustomerUserDto>(null, MessageId.AccessDenied);



                var dbQuery = _documentRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.DocumentId == documentId && a.ContractCode == authenticate.ContractCode);



                var result = await dbQuery
                    .Select(a => new DocumentArchiveInfoForCustomerUserDto
                    {
                        DocumentId = a.DocumentId,

                        ContractCode = a.ContractCode,
                        DocTitle = a.DocTitle,
                        DocNumber = a.DocNumber,
                        Area = (a.Area != null) ? new AreaReadDTO { AreaId = a.AreaId.Value, AreaTitle = a.Area.AreaTitle } : null,
                        ClientDocNumber = a.ClientDocNumber,
                        DocRemark = a.DocRemark,
                        DocClass = a.DocClass,
                        DocumentGroupCode = a.DocumentGroup.DocumentGroupCode,
                        DocumentGroupTitle = a.DocumentGroup.Title,
                        IsActive = a.IsActive,
                        UserAudit = a.AdderUser != null ? new UserAuditLogDto
                        {
                            AdderUserName = a.AdderUser.FullName,
                            CreateDate = a.CreatedDate.ToUnixTimestamp(),
                            AdderUserImage = a.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + a.AdderUser.Image : "",

                        } : null,
                        Revisions = a.DocumentRevisions.Where(c => !c.IsDeleted && (c.RevisionStatus == RevisionStatus.TransmittalIFA || c.RevisionStatus == RevisionStatus.TransmittalIFI || c.RevisionStatus == RevisionStatus.TransmittalIFC|| c.RevisionStatus == RevisionStatus.TransmittalASB) && _transmittalRepository.Any(d => d.TransmittalRevisions.Any(e => e.DocumentRevisionId == c.DocumentRevisionId) && (d.TransmittalType == TransmittalType.Customer || d.TransmittalType == TransmittalType.Consultant)))
                        .Select(c => new RevisionArchiveDto
                        {
                            DocumentRevisionId = c.DocumentRevisionId,
                            DocumentRevisionCode = c.DocumentRevisionCode,
                            RevisionStatus = c.RevisionStatus,
                            Description = c.Description,
                            UserAudit = c.AdderUser != null ? new UserAuditLogDto
                            {
                                AdderUserName = c.AdderUser.FullName,
                                CreateDate = c.CreatedDate.ToUnixTimestamp(),
                                AdderUserImage = c.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + c.AdderUser.Image : "",
                            } : null,

                            ActivityUsers = c.RevisionActivities != null && c.RevisionActivities.Any() ?
                            c.RevisionActivities.Where(b => !b.IsDeleted).Select(v => v.ActivityOwnerId).Distinct().Count() : 0,

                            FinalAttachment = c.RevisionStatus >= RevisionStatus.Confirmed ?
                            c.RevisionAttachments.Count(a => !a.IsDeleted && a.RevisionAttachmentType == RevisionAttachmentType.Final) : 0,

                            NativeAttachment = 0,

                            TransmittalDate = (c.RevisionStatus == RevisionStatus.TransmittalIFA || c.RevisionStatus == RevisionStatus.TransmittalIFC || c.RevisionStatus == RevisionStatus.TransmittalIFI || c.RevisionStatus == RevisionStatus.TransmittalASB) && c.TransmittalRevisions.Any(v => (v.Transmittal.TransmittalType == TransmittalType.Customer || v.Transmittal.TransmittalType == TransmittalType.Consultant)) ?
                            c.TransmittalRevisions.Where(v => v.Transmittal.TransmittalType == TransmittalType.Customer || v.Transmittal.TransmittalType == TransmittalType.Consultant).Select(v => v.Transmittal.CreatedDate).FirstOrDefault().ToUnixTimestamp() :
                            (long?)null,

                            ConfirmWorkFlow = c.RevisionStatus >= RevisionStatus.Confirmed ? c.ConfirmationWorkFlows.Where(a => a.Status == ConfirmationWorkFlowStatus.Confirm)
                            .Select(n => new ArchiveConfirmationWorkFlowDto
                            {
                                ConfirmationUsers = n.ConfirmationWorkFlowUsers.Count(),
                                CreateDate = n.CreatedDate.ToUnixTimestamp(),
                                ConfirmDate = n.UpdateDate.ToUnixTimestamp()
                            }).FirstOrDefault() : null

                        }).ToList()
                    }).FirstOrDefaultAsync();

                if (result == null)
                    return ServiceResultFactory.CreateError<DocumentArchiveInfoForCustomerUserDto>(null, MessageId.EntityDoesNotExist);

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<DocumentArchiveInfoForCustomerUserDto>(null, exception);
            }
        }
        private List<PendingDocumentForCommentDto> ApplayPageing(List<PendingDocumentForCommentDto> query, DocumentQueryDto queryDto)
        {
            if (queryDto.PageSize <= 0)
                queryDto.PageSize = 20;

            if (queryDto.Page <= 0)
                queryDto.Page = 1;

            return query.Skip((queryDto.Page - 1) * queryDto.PageSize).Take(queryDto.PageSize).ToList();
        }
        private async Task<ServiceResult<List<DocumentViewDto>>> GetDocumentListAsync(List<long> documentIds)
        {
            try
            {
                var dbQuery = _documentRepository
                     .AsNoTracking()
                     .Where(a => !a.IsDeleted && documentIds.Contains(a.DocumentId));
                var totalCount = dbQuery.Count();

                var columnsMap = new Dictionary<string, Expression<Func<Document, object>>>
                {
                    ["DocumentId"] = v => v.DocumentId
                };

                var result = await dbQuery.Select(a => new DocumentViewDto
                {
                    DocumentId = a.DocumentId,
                    ContractCode = a.ContractCode,
                    DocTitle = a.DocTitle,
                    DocNumber = a.DocNumber,
                    ClientDocNumber = (a.ClientDocNumber != null) ? a.ClientDocNumber : "",
                    DocRemark = a.DocRemark,
                    DocClass = a.DocClass,
                    CommunicationCommentStatus = a.CommunicationCommentStatus,
                    DocumentGroupCode = a.DocumentGroup.DocumentGroupCode,
                    DocumentGroupTitle = a.DocumentGroup.Title,
                    IsActive = a.IsActive,
                    IsRequiredTransmittal = a.IsRequiredTransmittal,
                    Area = (a.Area != null) ? new AreaReadDTO
                    {
                        AreaId = a.Area.AreaId,
                        AreaTitle = a.Area.AreaTitle
                    } : null,
                    LastRevision = a.DocumentRevisions
                    .Where(c => !c.IsDeleted && c.IsLastRevision)
                    .Select(a => new LastRevisionDto
                    {
                        RevisionId = a.DocumentRevisionId,
                        RevisionCode = a.DocumentRevisionCode,
                        RevisionStatus = a.RevisionStatus
                    }).FirstOrDefault(),
                    DocumentProducts = a.DocumentProducts.Select(a => new ProductInfoDto
                    {
                        ProductDescription = a.Product.Description,
                        ProductCode = a.Product.ProductCode,
                        ProductId = a.ProductId
                    }).ToList()
                }).ToListAsync();
                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<DocumentViewDto>>(null, exception);
            }
        }
    }
}
