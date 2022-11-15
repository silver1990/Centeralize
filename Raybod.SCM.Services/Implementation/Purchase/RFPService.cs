using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Raybod.SCM.DataAccess.Core;
using Raybod.SCM.DataAccess.Extention;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Audit;
using Raybod.SCM.DataTransferObject.Notification;
using Raybod.SCM.DataTransferObject.RFP;
using Raybod.SCM.DataTransferObject.Supplier;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Model;
using Raybod.SCM.Domain.Struct;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Services.Utilitys.MailService;
using Raybod.SCM.Utility.Extention;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Implementation
{
    public class RFPService : IRFPService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITeamWorkAuthenticationService _authenticationService;
        private readonly ISCMLogAndNotificationService _scmLogAndNotificationService;
        private readonly IContractFormConfigService _formConfigService;
        private readonly DbSet<Supplier> _supplierRepository;
        private readonly DbSet<RFP> _rfpRepository;
        private readonly DbSet<RFPStatusLog> _rfpStatusLogRepository;
        private readonly DbSet<MrpItem> _mrpItemRepository;
        private readonly DbSet<RFPItems> _rfpItemRepository;
        private readonly DbSet<PRContract> _prContractRepository;
        private readonly DbSet<RFPInquery> _rfpInqueryRepository;
        private readonly DbSet<RFPAttachment> _rfpAttachmentRepository;
        private readonly DbSet<RFPSupplier> _rfpSupplierRepository;
        private readonly DbSet<RFPProForma> _proFromaRepository;
        private readonly DbSet<RFPSupplierProposal> _rfpSupplierProposalRepository;
        private readonly DbSet<PurchaseRequestItem> _purchaseRequestItemRepository;
        private readonly DbSet<RFPComment> _rfpCommentRepository;
        private readonly DbSet<PurchaseRequest> _purchaseRequestRepository;
        private readonly Raybod.SCM.Services.Utilitys.FileHelper _fileHelper;
        private readonly CompanyConfig _appSettings;

        public RFPService(
            IUnitOfWork unitOfWork,
            IWebHostEnvironment hostingEnvironmentRoot,
            ITeamWorkAuthenticationService authenticationService,
            IOptions<CompanyAppSettingsDto> appSettings,
            IAppEmailService appEmailService,
            ISCMLogAndNotificationService scmLogAndNotificationService,
            IHttpContextAccessor httpContextAccessor,
            IContractFormConfigService formConfigService)
        {
            _unitOfWork = unitOfWork;
            _authenticationService = authenticationService;
            _scmLogAndNotificationService = scmLogAndNotificationService;
            _formConfigService = formConfigService;
            _fileHelper = new Utilitys.FileHelper(hostingEnvironmentRoot);
            _supplierRepository = _unitOfWork.Set<Supplier>();
            _mrpItemRepository = _unitOfWork.Set<MrpItem>();
            _rfpSupplierProposalRepository = _unitOfWork.Set<RFPSupplierProposal>();
            _rfpAttachmentRepository = _unitOfWork.Set<RFPAttachment>();
            _purchaseRequestItemRepository = _unitOfWork.Set<PurchaseRequestItem>();
            _purchaseRequestRepository = _unitOfWork.Set<PurchaseRequest>();
            _rfpSupplierRepository = _unitOfWork.Set<RFPSupplier>();
            _rfpCommentRepository = _unitOfWork.Set<RFPComment>();
            _prContractRepository = _unitOfWork.Set<PRContract>();
            _proFromaRepository = _unitOfWork.Set<RFPProForma>();
            _rfpRepository = _unitOfWork.Set<RFP>();
            _rfpStatusLogRepository = _unitOfWork.Set<RFPStatusLog>();
            _rfpItemRepository = _unitOfWork.Set<RFPItems>();
            _rfpInqueryRepository = _unitOfWork.Set<RFPInquery>();
            httpContextAccessor.HttpContext.Request.Headers.TryGetValue("companyCode", out var CompanyCode);
            _appSettings = appSettings.Value.CompanyConfig.First(a => a.CompanyCode == CompanyCode);
        }

        public async Task<RFPListBadgeCountDto> GetDashbourdRFPListBadgeCountAsync(AuthenticateDto authenticate)
        {
            var result = new RFPListBadgeCountDto
            {
                PenddingRFP = 0,
                InprogressRFP = 0,
            };
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return result;

                var dbQuery = _purchaseRequestRepository
                  .AsNoTracking()
                  .Where(a => !a.IsDeleted &&
                  a.PRStatus >= PRStatus.Confirm &&
                  a.ContractCode == authenticate.ContractCode &&
                  a.PurchaseRequestItems.Any(c => !c.IsDeleted && c.PRItemStatus == PRItemStatus.NotStart));

                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.ProductGroupIds.Contains(a.ProductGroupId));

                result.PenddingRFP = await dbQuery.CountAsync();

                var rfpDbQuery = _rfpRepository
                  .AsNoTracking()
                  .Where(x => !x.IsDeleted &&
                  x.Status < RFPStatus.RFPSelection &&
                  x.Status > RFPStatus.Canceled &&
                  x.ContractCode == authenticate.ContractCode)
                  .AsQueryable();

                if (permission.ProductGroupIds.Any())
                    rfpDbQuery = rfpDbQuery.Where(a => permission.ProductGroupIds.Contains(a.ProductGroupId));

                result.InprogressRFP = await rfpDbQuery.CountAsync();


                return result;
            }
            catch (Exception exception)
            {
                return result;
            }
        }

        public async Task<ServiceResult<RFPListBadgeCountDto>> GetRFPListBadgeCountAsync(AuthenticateDto authenticate)
        {
            var result = new RFPListBadgeCountDto
            {
                PenddingRFP = 0,
                InprogressRFP = 0,
            };

            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<RFPListBadgeCountDto>(null, MessageId.AccessDenied);

                var dbQuery = _purchaseRequestRepository
                  .AsNoTracking()
                  .Where(a => !a.IsDeleted && a.PRStatus >= PRStatus.Confirm &&
                  a.ContractCode == authenticate.ContractCode &&
                  a.PurchaseRequestItems.Any(c => !c.IsDeleted && c.PRItemStatus == PRItemStatus.NotStart));

                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.ProductGroupIds.Contains(a.ProductGroupId) && a.PurchaseRequestItems.Any(c => !c.IsDeleted && c.PRItemStatus == PRItemStatus.NotStart));

                result.PenddingRFP = await dbQuery.CountAsync();

                var rfpDbQuery = _rfpRepository
                  .AsNoTracking()
                  .Where(x => !x.IsDeleted && x.Status < RFPStatus.RFPSelection && x.Status > RFPStatus.Canceled && x.ContractCode == authenticate.ContractCode)
                  .OrderByDescending(x => x.Id)
                  .AsQueryable();

                if (permission.ProductGroupIds.Any())
                    rfpDbQuery = rfpDbQuery.Where(a => permission.ProductGroupIds.Contains(a.ProductGroupId));

                result.InprogressRFP = await rfpDbQuery.CountAsync();

                return ServiceResultFactory.CreateSuccess(result);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(result, exception);
            }
        }

        public async Task<ServiceResult<long>> AddRFPAsync(AuthenticateDto authenticate, int productGroupId, AddRFPDto model)
        {
            try
            {

                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(long.MinValue, MessageId.AccessDenied);

                if (permission.ProductGroupIds.Any() && !permission.ProductGroupIds.Contains(productGroupId))
                    return ServiceResultFactory.CreateError(long.MinValue, MessageId.AccessDenied);

                if (model.Suppliers == null)
                    return ServiceResultFactory.CreateError(long.MinValue, MessageId.InputDataValidationError);


                if (model.RFPItems == null || model.RFPItems.Count() == 0)
                    return ServiceResultFactory.CreateError(long.MinValue, MessageId.InputDataValidationError);

                if (model.RFPItems.GroupBy(a => a.ProductId).Any(c => c.Count() > 1))
                    return ServiceResultFactory.CreateError(long.MinValue, MessageId.ImpossibleDuplicateProduct);

                if (model.RFPType == RFPType.Proposal)
                {
                    if (model.CommercialInquiries == null && model.CommercialInquiries.Count() <= 0)
                        return ServiceResultFactory.CreateError(long.MinValue, MessageId.InputDataValidationError);




                    if (model.CommercialInquiries.Where(a => a.Id == "delivery" || a.Id == "price").ToList().Count < 2)
                    {
                        return ServiceResultFactory.CreateError(long.MinValue, MessageId.InputDataValidationError);
                    }
                }


                var postedPurchaseRequestIds = model.RFPItems
                .Select(a => a.PurchaseRequestId)
                .ToList();
                var postedProductIds = model.RFPItems
                .Select(a => a.ProductId)
                .ToList();
                var dbQuery = _purchaseRequestItemRepository
                 .Where(x => !x.IsDeleted && postedPurchaseRequestIds.Contains(x.PurchaseRequestId) &&
                 postedProductIds.Contains(x.ProductId) &&
                 x.Product.ProductGroupId == productGroupId &&
                 x.PurchaseRequest.ContractCode == authenticate.ContractCode);

                var purchaseRequestItemModel = await dbQuery
                    .Include(a => a.PurchaseRequest)
                    .Include(a => a.MrpItem)
                    .ToListAsync();

                if (purchaseRequestItemModel == null)
                    return ServiceResultFactory.CreateError(long.MinValue, MessageId.DataInconsistency);

                //if (purchaseRequestItemModel.Any(a => model.RFPItems.Any(c => c.PurchaseRequestItemId == a.Id && c.ProductId != a.ProductId)))
                //    return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                if (purchaseRequestItemModel.Any(a => a.PRItemStatus != PRItemStatus.NotStart))
                    return ServiceResultFactory.CreateError(long.MinValue, MessageId.DataInconsistency);
                var supplierIds = model.Suppliers.Select(a => a.Id).ToList();
                if (await _supplierRepository.CountAsync(a => !a.IsDeleted && supplierIds.Contains(a.Id)) != model.Suppliers.Count())
                    return ServiceResultFactory.CreateError(long.MinValue, MessageId.DataInconsistency);

                var rfpModel = new RFP
                {
                    ProductGroupId = productGroupId,
                    DateDue = model.DateDue.UnixTimestampToDateTime().Date,
                    RFPType = model.RFPType,
                    Status = RFPStatus.Register,
                    ContractCode = authenticate.ContractCode,
                    Note = model.Note,
                    RFPStatusLogs = new List<RFPStatusLog>
                    {
                        new RFPStatusLog
                        {
                            Status=RFPLogStatus.Register,
                            DateIssued=DateTime.UtcNow
                        }
                    }
                };

                rfpModel.RFPSuppliers = new List<RFPSupplier>();
                foreach (var supplier in model.Suppliers)
                {
                    rfpModel.RFPSuppliers.Add(new RFPSupplier
                    {
                        SupplierId = supplier.Id,
                        IsActive = true
                    });
                }

                rfpModel = AddRFPItem(rfpModel, purchaseRequestItemModel, model.RFPItems);

                if (rfpModel == null)
                    return ServiceResultFactory.CreateError(long.MinValue, MessageId.DataInconsistency);


                if (model.Attachmnets != null && model.Attachmnets.Count() > 0)
                {
                    if (!_fileHelper.FileExistInTemp(model.Attachmnets.Select(c => c.FileSrc).ToList()))
                        return ServiceResultFactory.CreateError(long.MinValue, MessageId.FileNotFound);
                    var attachmentResult = await AddRFPAttachmentAsync(rfpModel, model.Attachmnets);
                    if (!attachmentResult.Succeeded)
                        return ServiceResultFactory.CreateError(long.MinValue,
                            attachmentResult.Messages.FirstOrDefault().Message);

                    rfpModel = attachmentResult.Result;
                }

                if (model.RFPType == RFPType.Proposal)
                {
                    rfpModel.RFPInqueries = new List<RFPInquery>();
                    var res = await AddCommercialInqueryAsync(rfpModel, model.CommercialInquiries);
                    if (!res.Succeeded)
                        return ServiceResultFactory.CreateError(long.MinValue, res.Messages.First().Message);
                    rfpModel.RFPInqueries = res.Result;

                    if (model.TechInquiries != null && model.TechInquiries.Any())
                    {
                        res = await AddTechInqueryAsync(rfpModel, model.TechInquiries);
                        if (!res.Succeeded)
                            return ServiceResultFactory.CreateError(long.MinValue, res.Messages.First().Message);
                        foreach (var item in (res.Result))
                            rfpModel.RFPInqueries.Add(item);
                    }
                    //create default proposal entity for each supplier
                    foreach (var supplier in rfpModel.RFPSuppliers)
                    {
                        supplier.RFPSupplierProposals = new List<RFPSupplierProposal>();

                        foreach (var inquery in rfpModel.RFPInqueries)
                        {
                            supplier.RFPSupplierProposals.Add(new RFPSupplierProposal
                            {
                                RFPInquery = inquery
                            });
                        }
                    }

                }

                // generate form code
                var count = await _rfpRepository.CountAsync(a => a.ContractCode == authenticate.ContractCode);
                var codeRes = await _formConfigService.GenerateFormCodeAsync(authenticate.ContractCode, FormName.RFP, count);
                if (!codeRes.Succeeded)
                    return ServiceResultFactory.CreateError(long.MinValue, codeRes.Messages.First().Message);
                rfpModel.RFPNumber = codeRes.Result;

                _rfpRepository.Add(rfpModel);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var prIds = purchaseRequestItemModel.Select(a => a.PurchaseRequestId).ToList();
                    var donedPrIds = await _purchaseRequestRepository
                         .Where(a => !a.IsDeleted && prIds.Contains(a.Id) && !a.PurchaseRequestItems.Any(c => c.PRItemStatus == PRItemStatus.NotStart))
                         .Select(v => v.Id.ToString()).ToListAsync();

                    if (donedPrIds.Any())
                        await _scmLogAndNotificationService.SetDonedNotificationAsync(authenticate.UserId, rfpModel.ContractCode, donedPrIds, NotifEvent.AddRFP);

                    await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = authenticate.ContractCode,
                        RootKeyValue = rfpModel.Id.ToString(),
                        FormCode = rfpModel.RFPNumber,
                        KeyValue = rfpModel.Id.ToString(),
                        ProductGroupId = rfpModel.ProductGroupId,
                        NotifEvent = NotifEvent.AddRFP,
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,

                    },
                    rfpModel.ProductGroupId
                    , SendNotifOnAddRFP(rfpModel));
                    return ServiceResultFactory.CreateSuccess(rfpModel.Id);
                }
                return ServiceResultFactory.CreateError(long.MinValue, MessageId.SaveFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(long.MinValue, exception);
            }
        }


        public async Task<ServiceResult<List<RFPItemInfoDto>>> RFPItemsEditAsync(AuthenticateDto authenticate, long rfpId, List<AddRFPItemDto> model)
        {

            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<RFPItemInfoDto>>(null, MessageId.AccessDenied);

                var dbQuery = _rfpRepository
                     .Include(a => a.RFPItems)
                    .ThenInclude(a => a.PurchaseRequestItem)
                    .ThenInclude(a => a.MrpItem)
                    .Where(a => !a.IsDeleted && a.Id == rfpId && a.ContractCode == authenticate.ContractCode);

                if (await dbQuery.CountAsync() == 0)
                    return ServiceResultFactory.CreateError<List<RFPItemInfoDto>>(null, MessageId.EntityDoesNotExist);

                var productGroupId = await dbQuery.Select(a => a.ProductGroupId).FirstOrDefaultAsync();
                if (permission.ProductGroupIds.Any() && !dbQuery.Any(c => permission.ProductGroupIds.Contains(productGroupId)))
                    return ServiceResultFactory.CreateError<List<RFPItemInfoDto>>(null, MessageId.AccessDenied);
                if (model.GroupBy(a => a.ProductId).Any(c => c.Count() > 1))
                    return ServiceResultFactory.CreateError<List<RFPItemInfoDto>>(null, MessageId.ImpossibleDuplicateProduct);

                if (await dbQuery.AnyAsync(a => a.Status > RFPStatus.RFPEvaluation))
                    return ServiceResultFactory.CreateError<List<RFPItemInfoDto>>(null, MessageId.RFPItemsCantBeEditAfterVendorSelection);

                var rfpItems = await dbQuery
                    .Include(a => a.RFPItems)
                    .ThenInclude(a => a.PurchaseRequestItem)
                    .ThenInclude(a => a.MrpItem).FirstAsync();
                var updateResult = await RFPItemEditAsync(rfpItems, model);
                if (!updateResult.Succeeded)
                    return ServiceResultFactory.CreateError<List<RFPItemInfoDto>>(null, MessageId.EditEntityFailed);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var result = await _rfpItemRepository
                   .AsNoTracking()
                   .Where(x => !x.IsDeleted && x.RFPId == rfpId && x.RFP.ContractCode == authenticate.ContractCode).Select(c => new RFPItemInfoDto
                   {
                       Id = c.Id,
                       IsActive = c.IsActive,
                       DateEnd = c.DateEnd.ToUnixTimestamp(),
                       DateStart = c.DateStart.ToUnixTimestamp(),
                       PRCode = c.PurchaseRequestItem != null ? c.PurchaseRequestItem.PurchaseRequest.PRCode : "",
                       ProductCode = c.Product.ProductCode,
                       ProductDescription = c.Product.Description,
                       ProductGroupName = c.Product.ProductGroup.Title,
                       ProductId = c.ProductId,
                       ProductTechnicalNumber = c.Product.TechnicalNumber,
                       ProductUnit = c.Product.Unit,
                       PurchaseRequestId = c.PurchaseRequestItem.PurchaseRequestId,
                       Quantity = c.Quantity,
                       DocumentStatus =
                             !c.Product.DocumentProducts.Any(v => !v.Document.IsDeleted && v.Document.IsActive && v.Document.ContractCode == authenticate.ContractCode)
                             ? EngineeringDocumentStatus.No
                             : c.Product.DocumentProducts.Any(v => !v.Document.IsDeleted && v.Document.IsActive && v.Document.ContractCode == authenticate.ContractCode &&
                             (!v.Document.DocumentRevisions.Any(a => !a.IsDeleted) || v.Document.DocumentRevisions.Any(n => !n.IsDeleted && n.RevisionStatus < RevisionStatus.Confirmed)))
                             ? EngineeringDocumentStatus.completing
                             : EngineeringDocumentStatus.Completed,
                   }).ToListAsync();

                    List<RFPItemInfoDto> items = new List<RFPItemInfoDto>();
                    foreach (var item in result)
                    {
                        if (!items.Any(a => a.ProductId == item.ProductId))
                        {
                            items.Add(new RFPItemInfoDto
                            {
                                Id = item.Id,
                                IsActive = item.IsActive,
                                DateEnd = item.DateEnd,
                                DateStart = item.DateStart,
                                PRCode = item.PRCode,
                                ProductCode = item.ProductCode,
                                ProductDescription = item.ProductDescription,
                                ProductGroupName = item.ProductGroupName,
                                ProductId = item.ProductId,
                                ProductTechnicalNumber = item.ProductTechnicalNumber,
                                ProductUnit = item.ProductUnit,
                                PurchaseRequestId = item.PurchaseRequestId,
                                Quantity = result.Where(a => a.ProductId == item.ProductId).Sum(a => a.Quantity),
                                DocumentStatus = item.DocumentStatus
                            });
                        }
                    }

                    return ServiceResultFactory.CreateSuccess(items);
                }

                return ServiceResultFactory.CreateError<List<RFPItemInfoDto>>(null, MessageId.SaveFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<RFPItemInfoDto>>(null, exception);
            }
        }

        private List<NotifToDto> SendNotifOnAddRFP(RFP rfpModel)
        {
            var result = new List<NotifToDto>();
            if (rfpModel.RFPInqueries != null && rfpModel.RFPInqueries.Any(a => a.RFPInqueryType == RFPInqueryType.TechnicalInquery))
            {
                result.Add(new NotifToDto
                {
                    NotifEvent = NotifEvent.AddTechProposal,
                    Roles = new List<string>
                    {
                        SCMRole.RFPTechMng,
                    }
                });
            }

            if (rfpModel.RFPInqueries != null && rfpModel.RFPInqueries.Any(a => a.RFPInqueryType == RFPInqueryType.CommercialInquery))
            {
                result.Add(new NotifToDto
                {
                    NotifEvent = NotifEvent.AddCommercialProposal,
                    Roles = new List<string>
                    {
                        SCMRole.RFPCommercialMng,
                    }
                });
            }
            if (rfpModel.RFPType == RFPType.Proforma)
            {
                result.Add(new NotifToDto
                {
                    NotifEvent = NotifEvent.AddRFPProForma,
                    Roles = new List<string>
                    {
                        SCMRole.RFPProFormaMng,
                    }
                });
            }
            return result.Count() > 0 ? result : null;
        }


        public async Task<ServiceResult<List<ListRFPDto>>> GetRFPAsync(AuthenticateDto authenticate, RFPQueryDto query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<ListRFPDto>>(null, MessageId.AccessDenied);

                var dbQuery = _rfpRepository
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted &&
                    x.ContractCode == authenticate.ContractCode &&
                    x.RFPItems.Any(c => c.IsActive && !c.IsDeleted))
                    .OrderByDescending(x => x.Id)
                    .AsQueryable();

                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.ProductGroupIds.Contains(a.ProductGroupId));

                dbQuery = ApplayFilterQuery(query, dbQuery);

                var totalCount = dbQuery.Count();
                dbQuery = dbQuery.ApplayPageing(query);
                var result = await dbQuery.Select(rfp => new ListRFPDto
                {
                    Id = rfp.Id,
                    RFPType = rfp.RFPType,
                    ProductGroupId = rfp.ProductGroupId,
                    ProductGroupTitle = rfp.ProductGroup.Title,
                    Status = rfp.Status,
                    DateDue = rfp.DateDue.ToUnixTimestamp(),
                    RFPNumber = rfp.RFPNumber,
                    ContractCode = rfp.ContractCode,
                    DateCreate = rfp.CreatedDate.ToUnixTimestamp(),
                    RFPItems = GetProductsForRFP(rfp.RFPItems.Where(a => !a.IsDeleted && a.IsActive)
                    .Select(c => c.Product.Description).ToList()),
                    Suppliers = rfp.RFPSuppliers.Select(c => c.Supplier.Name)
                    .ToList()
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new List<ListRFPDto>(), exception);
            }
        }

        public async Task<ServiceResult<List<RFPDataForPRContractDto>>> GetInProgressRFPAsync(AuthenticateDto authenticate, RFPQueryDto query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<RFPDataForPRContractDto>>(null, MessageId.AccessDenied);

                var dbQuery = _rfpRepository
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && x.Status < RFPStatus.RFPSelection && x.Status > RFPStatus.Canceled && x.ContractCode == authenticate.ContractCode)
                    .OrderByDescending(x => x.Id)
                    .AsQueryable();

                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.ProductGroupIds.Contains(a.ProductGroupId));

                dbQuery = ApplayFilterQuery(query, dbQuery);

                var totalCount = dbQuery.Count();
                dbQuery = dbQuery.ApplayPageing(query);
                var result = await dbQuery.Select(rfp => new RFPDataForPRContractDto
                {
                    Id = rfp.Id,
                    ProductGroupId = rfp.ProductGroupId,
                    ProductGroupTitle = rfp.ProductGroup.Title,
                    RFPType = rfp.RFPType,
                    Status = rfp.Status,
                    DateDue = rfp.DateDue.ToUnixTimestamp(),
                    RFPNumber = rfp.RFPNumber,
                    ContractCode = rfp.ContractCode,
                    DateCreate = rfp.CreatedDate.ToUnixTimestamp(),
                    UserAudit = rfp.AdderUser != null ? new UserAuditLogDto
                    {
                        AdderUserId = rfp.AdderUserId,
                        AdderUserName = rfp.AdderUser.FullName,
                        CreateDate = rfp.CreatedDate.ToUnixTimestamp(),
                        AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + rfp.AdderUser.Image
                    } : null,
                    RFPItems = GetProductsForRFP(rfp.RFPItems.Where(a => !a.IsDeleted && a.IsActive)
                    .Select(c => c.Product.Description).ToList()),
                    Suppliers = rfp.RFPSuppliers.Select(c => c.Supplier.Name)
                    .ToList()
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new List<RFPDataForPRContractDto>(), exception);
            }
        }


        public async Task<ServiceResult<RFPInfoDto>> GetRFPByIdAsync(AuthenticateDto authenticate, long rfpId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<RFPInfoDto>(null, MessageId.AccessDenied);

                var dbQuery = _rfpRepository
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted &&
                    x.Id == rfpId &&
                    x.ContractCode == authenticate.ContractCode);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<RFPInfoDto>(null, MessageId.EntityDoesNotExist);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(c => permission.ProductGroupIds.Contains(c.ProductGroupId)))
                    return ServiceResultFactory.CreateError<RFPInfoDto>(null, MessageId.AccessDenied);


                var result = await dbQuery.Select(rfp => new RFPInfoDto
                {
                    Id = rfp.Id,
                    RFPNumber = rfp.RFPNumber,
                    ContractCode = rfp.ContractCode,
                    RFPType = rfp.RFPType,
                    Status = rfp.Status,
                    ProductGroupId = rfp.ProductGroupId,
                    ProductGroupTitle = rfp.ProductGroup.Title,
                    DateDue = rfp.DateDue.ToUnixTimestamp(),
                    UserAudit = rfp.AdderUser != null ? new UserAuditLogDto
                    {
                        AdderUserId = rfp.AdderUserId,
                        AdderUserName = rfp.AdderUser.FullName,
                        CreateDate = rfp.CreatedDate.ToUnixTimestamp(),
                        AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + rfp.AdderUser.Image
                    } : null,
                    Suppliers = rfp.RFPSuppliers.Where(x => !x.IsDeleted).Select(s => new RFPSupplierInfoDto
                    {
                        RFPSupplierId = s.Id,
                        RFPId = s.RFPId,
                        IsActive = s.IsActive,
                        SupplierId = s.SupplierId,
                        SupplierCode = s.Supplier.SupplierCode,
                        SupplierEmail = s.Supplier.Email,
                        SupplierName = s.Supplier.Name,
                        SupplierLogo = _appSettings.ClientHost + ServiceSetting.UploadFilePath.RFP + s.Supplier.Logo,
                        SupplierPhone = s.Supplier.TellPhone,
                        SupplierProductGroups = s.Supplier.SupplierProductGroups
                        .Where(p => !p.ProductGroup.IsDeleted)
                        .Select(p => p.ProductGroup.Title)
                        .ToList(),
                        IsWinner = s.IsWinner
                    }).ToList(),
                    RFPItems = rfp.RFPItems.Where(a => !a.IsDeleted)
                    .Select(c => new RFPItemInfoDto
                    {
                        Id = c.Id,
                        IsActive = c.IsActive,
                        DateEnd = c.DateEnd.ToUnixTimestamp(),
                        DateStart = c.DateStart.ToUnixTimestamp(),
                        PRCode = c.PurchaseRequestItem != null ? c.PurchaseRequestItem.PurchaseRequest.PRCode : "",
                        ProductCode = c.Product.ProductCode,
                        ProductDescription = c.Product.Description,
                        ProductGroupName = c.Product.ProductGroup.Title,
                        ProductId = c.ProductId,
                        ProductTechnicalNumber = c.Product.TechnicalNumber,
                        ProductUnit = c.Product.Unit,
                        PurchaseRequestId = c.PurchaseRequestItem.PurchaseRequestId,
                        Quantity = c.Quantity,
                        DocumentStatus =
                            !c.Product.DocumentProducts.Any(v => !v.Document.IsDeleted && v.Document.IsActive && v.Document.ContractCode == authenticate.ContractCode)
                            ? EngineeringDocumentStatus.No
                            : c.Product.DocumentProducts.Any(v => !v.Document.IsDeleted && v.Document.IsActive && v.Document.ContractCode == authenticate.ContractCode &&
                            (!v.Document.DocumentRevisions.Any(a => !a.IsDeleted) || v.Document.DocumentRevisions.Any(n => !n.IsDeleted && n.RevisionStatus < RevisionStatus.Confirmed)))
                            ? EngineeringDocumentStatus.completing
                            : EngineeringDocumentStatus.Completed,
                    }).ToList(),
                    Attachments = null,
                    RfpInqueries = rfp.RFPInqueries.Where(a => !a.IsDeleted)
                    .Select(c => new RFPInqueryInfoDto
                    {
                        Id = c.Id,
                        Description = c.Description,
                        RFPInqueryType = c.RFPInqueryType,
                        DefaultInquery = c.DefaultInquery,
                        Weight = c.Weight
                    }).ToList()
                }).FirstOrDefaultAsync();
                List<RFPItemInfoDto> purchaseItems = new List<RFPItemInfoDto>();
                foreach (var item in result.RFPItems)
                {
                    if (!purchaseItems.Any(a => a.ProductId == item.ProductId))
                    {
                        purchaseItems.Add(new RFPItemInfoDto
                        {
                            Id = item.Id,
                            PRCode = item.PRCode,
                            DateEnd = item.DateEnd,
                            DateStart = item.DateStart,
                            ProductCode = item.ProductCode,
                            ProductDescription = item.ProductDescription,
                            ProductGroupName = item.ProductGroupName,
                            ProductId = item.ProductId,
                            ProductTechnicalNumber = item.ProductTechnicalNumber,
                            ProductUnit = item.ProductUnit,
                            PurchaseRequestId = item.PurchaseRequestId,
                            Quantity = result.RFPItems.Where(a => a.ProductId == item.ProductId).Sum(a => a.Quantity),
                            DocumentStatus = item.DocumentStatus,
                        });
                    }
                }
                result.RFPItems = purchaseItems;
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<RFPInfoDto>(null, exception);
            }
        }
        public async Task<ServiceResult<AddOrEditProFormaDto>> DeleteProFormaAsync(AuthenticateDto authenticate, long rfpSupplierId, long proFormaId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<AddOrEditProFormaDto>(null, MessageId.AccessDenied);

                var dbQuery = _rfpSupplierRepository
                    .Include(a => a.RFP)
                    .Include(a => a.RFPProFormas)
                    .Where(x => !x.IsDeleted &&
                    x.Id == rfpSupplierId &&
                    x.RFP.ContractCode == authenticate.ContractCode);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<AddOrEditProFormaDto>(null, MessageId.EntityDoesNotExist);
                if (permission.ProductGroupIds.Any() && !dbQuery.Any(c => permission.ProductGroupIds.Contains(c.RFP.ProductGroupId)))
                    return ServiceResultFactory.CreateError<AddOrEditProFormaDto>(null, MessageId.AccessDenied);


                var rfpSupplier = await dbQuery.Include(a => a.RFP).Include(a => a.RFPProFormas).FirstOrDefaultAsync();


                if (!rfpSupplier.RFPProFormas.Any(a => !a.IsDeleted && a.Id == proFormaId))
                    return ServiceResultFactory.CreateError<AddOrEditProFormaDto>(null, MessageId.EntityDoesNotExist);

                if (rfpSupplier.RFP.Status == RFPStatus.RFPSelection)
                    return ServiceResultFactory.CreateError<AddOrEditProFormaDto>(null, MessageId.ProFormaCantDeleteAfterVendorSelection);
                var proforma = rfpSupplier.RFPProFormas.First(a => !a.IsDeleted && a.Id == proFormaId);
                proforma.IsDeleted = true;
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var suppliers = await GetRFPDetailsByIdAsync(authenticate, rfpSupplier.RFPId);
                    var proformas = await GetRFPProFormaDetailAsync(authenticate, rfpSupplier.RFPId, rfpSupplierId);
                    AddOrEditProFormaDto result = new AddOrEditProFormaDto();
                    result.ProForma = proformas.Result;
                    result.Suppliers = suppliers.Result.Suppliers;
                    return ServiceResultFactory.CreateSuccess(result);
                }

                return ServiceResultFactory.CreateError<AddOrEditProFormaDto>(null, MessageId.SaveFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<AddOrEditProFormaDto>(null, exception);
            }
        }
        #region RFP details
        public async Task<ServiceResult<RFPInfoDto>> GetRFPDetailsByIdAsync(AuthenticateDto authenticate, long rfpId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<RFPInfoDto>(null, MessageId.AccessDenied);

                var dbQuery = _rfpRepository
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && x.Id == rfpId && x.ContractCode == authenticate.ContractCode);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<RFPInfoDto>(null, MessageId.EntityDoesNotExist);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(c => permission.ProductGroupIds.Contains(c.ProductGroupId)))
                    return ServiceResultFactory.CreateError<RFPInfoDto>(null, MessageId.AccessDenied);

                var result = await dbQuery.Select(rfp => new RFPInfoDto
                {
                    Id = rfp.Id,
                    RFPNumber = rfp.RFPNumber,
                    Note = rfp.Note,
                    ContractCode = rfp.ContractCode,
                    RFPType = rfp.RFPType,
                    Status = rfp.Status,
                    ProductGroupId = rfp.ProductGroupId,
                    ProductGroupTitle = rfp.ProductGroup.Title,
                    DateDue = rfp.DateDue.ToUnixTimestamp(),
                    UserAudit = rfp.AdderUser != null ? new UserAuditLogDto
                    {
                        AdderUserId = rfp.AdderUserId,
                        AdderUserName = rfp.AdderUser.FullName,
                        CreateDate = rfp.CreatedDate.ToUnixTimestamp(),
                        AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + rfp.AdderUser.Image
                    } : null,
                    Suppliers = rfp.RFPSuppliers.Where(x => !x.IsDeleted).Select(s => new RFPSupplierInfoDto
                    {
                        RFPSupplierId = s.Id,
                        RFPId = s.RFPId,
                        SupplierId = s.SupplierId,
                        SupplierCode = s.Supplier.SupplierCode,
                        SupplierEmail = s.Supplier.Email,
                        SupplierName = s.Supplier.Name,
                        IsWinner = s.IsWinner,
                        IsActive = s.IsActive,
                        SupplierProductGroups = null,

                        TechProposalState = (s.RFP.RFPType == RFPType.Proposal) ? !s.RFPSupplierProposals.Any(a => !a.IsDeleted && a.RFPSupplier.IsActive && !a.RFPInquery.IsDeleted && a.RFPInquery.RFPInqueryType == RFPInqueryType.TechnicalInquery)
                        ? SupplierProposalState.None
                        : s.RFPSupplierProposals.Any(a => !a.RFPSupplier.IsDeleted && a.RFPSupplier.IsActive && !a.IsDeleted && !a.IsAnswered && !a.RFPInquery.IsDeleted && a.RFPInquery.RFPInqueryType == RFPInqueryType.TechnicalInquery)
                        ? SupplierProposalState.Completing
                        : s.RFPSupplierProposals.Any(a => !a.RFPSupplier.IsDeleted && a.RFPSupplier.IsActive && !a.IsDeleted && a.IsAnswered && !a.IsEvaluated && !a.RFPInquery.IsDeleted && a.RFPInquery.RFPInqueryType == RFPInqueryType.TechnicalInquery)
                        ? SupplierProposalState.Answered
                        : SupplierProposalState.Evaluation : SupplierProposalState.None,

                        CommercialProposalState = (s.RFP.RFPType == RFPType.Proposal) ? !s.RFPSupplierProposals.Any(a => !a.IsDeleted && a.RFPSupplier.IsActive && !a.RFPInquery.IsDeleted && a.RFPInquery.RFPInqueryType == RFPInqueryType.CommercialInquery)
                        ? SupplierProposalState.None
                        : s.RFPSupplierProposals.Any(a => !a.RFPSupplier.IsDeleted && a.RFPSupplier.IsActive && !a.IsDeleted && !a.IsAnswered && !a.RFPInquery.IsDeleted && a.RFPInquery.RFPInqueryType == RFPInqueryType.CommercialInquery)
                        ? SupplierProposalState.Completing
                        : s.RFPSupplierProposals.Any(a => !a.RFPSupplier.IsDeleted && a.RFPSupplier.IsActive && !a.IsDeleted && a.IsAnswered && !a.IsEvaluated && !a.RFPInquery.IsDeleted && a.RFPInquery.RFPInqueryType == RFPInqueryType.CommercialInquery)
                        ? SupplierProposalState.Answered
                        : SupplierProposalState.Evaluation : SupplierProposalState.None,
                        ProFormaStatus = (s.RFP.RFPType == RFPType.Proforma && s.RFPProFormas.Any(a => !a.IsDeleted)) ? ProFormaStatus.HaveProForma : ProFormaStatus.NotHaveProForma
                    }).ToList(),
                    RFPItems = rfp.RFPItems.Where(a => !a.IsDeleted)
                    .Select(c => new RFPItemInfoDto
                    {
                        Id = c.Id,
                        IsActive = c.IsActive,
                        DateEnd = c.DateEnd.ToUnixTimestamp(),
                        DateStart = c.DateStart.ToUnixTimestamp(),
                        PRCode = c.PurchaseRequestItem != null ? c.PurchaseRequestItem.PurchaseRequest.PRCode : "",
                        ProductCode = c.Product.ProductCode,
                        ProductDescription = c.Product.Description,
                        ProductGroupName = c.Product.ProductGroup.Title,
                        ProductId = c.ProductId,
                        ProductTechnicalNumber = c.Product.TechnicalNumber,
                        ProductUnit = c.Product.Unit,
                        PurchaseRequestId = c.PurchaseRequestItem.PurchaseRequestId,
                        Quantity = c.Quantity,
                        DocumentStatus =
                            !c.Product.DocumentProducts.Any(v => !v.Document.IsDeleted && v.Document.IsActive && v.Document.ContractCode == authenticate.ContractCode)
                            ? EngineeringDocumentStatus.No
                            : c.Product.DocumentProducts.Any(v => !v.Document.IsDeleted && v.Document.IsActive && v.Document.ContractCode == authenticate.ContractCode &&
                            (!v.Document.DocumentRevisions.Any(a => !a.IsDeleted) || v.Document.DocumentRevisions.Any(n => !n.IsDeleted && n.RevisionStatus < RevisionStatus.Confirmed)))
                            ? EngineeringDocumentStatus.completing
                            : EngineeringDocumentStatus.Completed,
                    }).ToList(),
                    Attachments = rfp.RFPAttachments.Where(a => !a.IsDeleted)
                    .Select(c => new RFPAttachmentInfoDto
                    {
                        Id = c.Id,
                        FileName = c.FileName,
                        FileSize = c.FileSize,
                        FileType = c.FileType,
                        FileSrc = c.FileSrc
                    }).ToList(),
                    RfpInqueries = null,

                }).FirstOrDefaultAsync();
                List<RFPItemInfoDto> rfpItems = new List<RFPItemInfoDto>();
                foreach (var item in result.RFPItems)
                {
                    if (!rfpItems.Any(a => a.ProductId == item.ProductId))
                    {
                        rfpItems.Add(new RFPItemInfoDto
                        {
                            Id = item.Id,
                            IsActive = item.IsActive,
                            DateEnd = item.DateEnd,
                            DateStart = item.DateStart,
                            PRCode = item.PRCode,
                            ProductCode = item.ProductCode,
                            ProductDescription = item.ProductDescription,
                            ProductGroupName = item.ProductGroupName,
                            ProductId = item.ProductId,
                            ProductTechnicalNumber = item.ProductTechnicalNumber,
                            ProductUnit = item.ProductUnit,
                            PurchaseRequestId = item.PurchaseRequestId,
                            Quantity = result.RFPItems.Where(a => a.ProductId == item.ProductId).Sum(a => a.Quantity),
                            DocumentStatus = item.DocumentStatus
                        });
                    }
                }
                result.RFPItems = rfpItems;
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<RFPInfoDto>(null, exception);
            }
        }
        public async Task<ServiceResult<RFPSupplierProFormListDto>> GetRFPProFormaListAsync(AuthenticateDto authenticate, long rfpId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<RFPSupplierProFormListDto>(null, MessageId.AccessDenied);

                var dbQuery = _proFromaRepository
                    .AsNoTracking()
                    .Include(a => a.RFPSupplier)
                    .ThenInclude(a => a.Supplier)
                    .Include(a => a.RFPProFormaAttachments)
                    .Where(a => !a.IsDeleted
                    && a.RFPId == rfpId && a.RFPSupplier.RFPId == rfpId);
                if (await dbQuery.CountAsync() == 0)
                    return ServiceResultFactory.CreateError<RFPSupplierProFormListDto>(null, MessageId.EntityDoesNotExist);
                RFPSupplierProFormListDto result = new RFPSupplierProFormListDto();
                var proFormaList = await dbQuery.Where(a => !a.IsDeleted).Select(a => new RFPSupplierProFormDetailDto
                {
                    Email = a.RFPSupplier.Supplier.Email,
                    SupplierId = a.RFPSupplier.Supplier.Id,
                    Name = a.RFPSupplier.Supplier.Name,
                    SupplierCode = a.RFPSupplier.Supplier.SupplierCode,
                    TellPhone = a.RFPSupplier.Supplier.TellPhone,
                    Logo = (!String.IsNullOrEmpty(a.RFPSupplier.Supplier.Logo)) ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.LogoSmall + a.RFPSupplier.Supplier.Logo : "",
                    Duration = a.Duration,
                    Price = a.Price,
                    ProFormId = a.Id,
                    RFPSupplierId = a.RFPSupplierId,
                    IsWinner = a.RFPSupplier.IsWinner,
                    Attachments = a.RFPProFormaAttachments.Where(c => !c.IsDeleted).Select(c => new RFPProFormaAttachmentDto
                    {
                        RFPAttachmentId = c.Id,
                        FileName = c.FileName,
                        FileSize = c.FileSize,
                        FileSrc = c.FileSrc,
                        FileType = c.FileType
                    }).ToList()
                }).ToListAsync();
                foreach (var item in proFormaList)
                {
                    if (item.IsWinner)
                    {
                        result.Winners.Add(item.SupplierId);
                    }
                }
                result.RFPSupplierProForma = proFormaList.OrderBy(a => a.RFPSupplierId).ToList();
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<RFPSupplierProFormListDto>(null, exception);
            }
        }
        public async Task<ServiceResult<SupplierEvaluationProposalInfoDto>> GetRFPEvaluationAsync(AuthenticateDto authenticate, long rfpId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<SupplierEvaluationProposalInfoDto>(null, MessageId.AccessDenied);

                var dbQuery = _rfpSupplierRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.IsActive && a.RFPId == rfpId && a.RFP.ContractCode == authenticate.ContractCode);

                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(c => permission.ProductGroupIds.Contains(c.RFP.ProductGroupId));

                var proposals = await dbQuery
                 .Select(v => new SupplierEvaluationInfoDto
                 {
                     SupplierId = v.Supplier.Id,
                     SupplierCode = v.Supplier.SupplierCode,
                     SupplierName = v.Supplier.Name,
                     CBEScore = v.CBEScore,
                     TBEScore = v.TBEScore,
                     IsWinner = v.IsWinner,
                     Logo = (!String.IsNullOrEmpty(v.Supplier.Logo)) ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.LogoSmall + v.Supplier.Logo : "",
                     Email = v.Supplier.Email,
                     Price = v.RFPSupplierProposals.Any(a => !a.IsDeleted && a.IsAnswered && a.RFPInquery.DefaultInquery == DefaultInquery.Price) ?
                     v.RFPSupplierProposals.FirstOrDefault(a => !a.IsDeleted && a.IsAnswered && a.RFPInquery.DefaultInquery == DefaultInquery.Price).Description
                     : null,
                     DeliveryDate = v.RFPSupplierProposals.Any(a => !a.IsDeleted && a.IsAnswered && a.RFPInquery.DefaultInquery == DefaultInquery.DeliveryDate) ?
                     v.RFPSupplierProposals.FirstOrDefault(a => !a.IsDeleted && a.IsAnswered && a.RFPInquery.DefaultInquery == DefaultInquery.DeliveryDate).Description
                     : null,
                 }).ToListAsync();
                SupplierEvaluationProposalInfoDto result = new SupplierEvaluationProposalInfoDto();
                foreach (var item in proposals)
                {
                    if (item.IsWinner)
                    {
                        result.Winners.Add(item.SupplierId);
                    }
                }
                result.RFPSupplierProposal = proposals;
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<SupplierEvaluationProposalInfoDto>(null, exception);
            }
        }

        public async Task<ServiceResult<List<RFPInqueryInfoDto>>> GetRFPInqueryByRFPIdAsync(AuthenticateDto authenticate, long rfpId, RFPInqueryType inqueryType)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<RFPInqueryInfoDto>>(null, MessageId.AccessDenied);

                var dbQuery = _rfpRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.Id == rfpId && a.ContractCode == authenticate.ContractCode);

                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(c => permission.ProductGroupIds.Contains(c.ProductGroupId));

                var result = await dbQuery
                    .SelectMany(c => c.RFPInqueries.Where(v => !v.IsDeleted && v.RFPInqueryType == inqueryType))
                    .Select(p => new RFPInqueryInfoDto
                    {
                        Id = p.Id,
                        Description = p.Description,
                        RFPInqueryType = p.RFPInqueryType,
                        DefaultInquery = p.DefaultInquery,
                        Weight = p.Weight
                    }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<RFPInqueryInfoDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<bool>> DeActiveRFPItemByIdAsync(AuthenticateDto authenticate, long rfpId, long rfpItemId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery = _rfpItemRepository
                    .Where(a => !a.IsDeleted &&
                    !a.RFP.IsDeleted &&
                    a.Id == rfpItemId &&
                    a.RFP.ContractCode == authenticate.ContractCode &&
                    a.IsActive && a.RFPId == rfpId)
                    .Include(a => a.RFP)
                    .Include(a => a.PurchaseRequestItem)
                    .AsQueryable();

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(c => permission.ProductGroupIds.Contains(c.RFP.ProductGroupId)))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var rfpItemModel = await dbQuery.FirstOrDefaultAsync();

                if (rfpItemModel == null || rfpItemModel.PurchaseRequestItem == null || rfpItemModel.RFP == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (rfpItemModel.RFP.Status == RFPStatus.RFPSelection || rfpItemModel.RFP.Status == RFPStatus.Canceled)
                    return ServiceResultFactory.CreateError(false, MessageId.ImpossibleEdit);

                if (!await _rfpItemRepository.AnyAsync(a => !a.IsDeleted && a.IsActive && a.Id != rfpItemId))
                    return ServiceResultFactory.CreateError(false, MessageId.MinimumLimitRFPItem);

                rfpItemModel.IsActive = false;
                if (rfpItemModel.PurchaseRequestItem != null)
                {
                    rfpItemModel.PurchaseRequestItem.PRItemStatus = PRItemStatus.NotStart;
                }

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    return ServiceResultFactory.CreateSuccess(true);
                }

                return ServiceResultFactory.CreateError(false, MessageId.EditEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<List<RFPItemInfoDto>>> AddRFPItemByRFPIdAsync(AuthenticateDto authenticate, long rfpId, List<AddRFPItemDto> model)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<RFPItemInfoDto>>(null, MessageId.AccessDenied);

                if (model == null || model.Count() == 0)
                    return ServiceResultFactory.CreateError<List<RFPItemInfoDto>>(null, MessageId.InputDataValidationError);

                var rfpDbQuery = _rfpRepository.Where(a => !a.IsDeleted && a.Id == rfpId && a.ContractCode == authenticate.ContractCode)
                    .Include(a => a.RFPItems);

                if (rfpDbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<List<RFPItemInfoDto>>(null, MessageId.EntityDoesNotExist);

                if (permission.ProductGroupIds.Any() && !rfpDbQuery.Any(c => permission.ProductGroupIds.Contains(c.ProductGroupId)))
                    return ServiceResultFactory.CreateError<List<RFPItemInfoDto>>(null, MessageId.AccessDenied);

                var rfpModel = await rfpDbQuery
                    .FirstOrDefaultAsync();

                if (rfpModel.Status == RFPStatus.Canceled || rfpModel.Status == RFPStatus.RFPSelection)
                    return ServiceResultFactory.CreateError<List<RFPItemInfoDto>>(null, MessageId.ImpossibleEdit);


                var postedPurchaseRequestIds = model
                    .Select(a => a.PurchaseRequestId)
                    .ToList();
                var postedProductIds = model
    .Select(a => a.ProductId)
    .ToList();
                var dbQuery = _purchaseRequestItemRepository
                 .Where(x => postedPurchaseRequestIds.Contains(x.PurchaseRequestId) &&
                 postedProductIds.Contains(x.ProductId) &&
                 x.PRItemStatus == PRItemStatus.NotStart &&
                 x.PurchaseRequest.ContractCode == rfpModel.ContractCode);

                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.ProductGroupIds.Contains(a.Product.ProductGroupId));

                var purchaseRequestItemModel = await dbQuery
                    .Include(a => a.PurchaseRequest)
                    .Include(a => a.MrpItem)
                 .ToListAsync();

                if (purchaseRequestItemModel == null)
                    return ServiceResultFactory.CreateError<List<RFPItemInfoDto>>(null, MessageId.DataInconsistency);

                //if (purchaseRequestItemModel.Any(a => model.Any(c => c.PurchaseRequestItemId == a.Id && c.ProductId != a.ProductId)))
                //    return ServiceResultFactory.CreateError<List<RFPItemInfoDto>>(null, MessageId.DataInconsistency);


                var productIds = purchaseRequestItemModel
                    .Select(a => a.ProductId)
                    .ToList();

                if (rfpModel.RFPItems.Any(a => !a.IsDeleted && a.IsActive && productIds.Contains(a.ProductId)))
                    return ServiceResultFactory.CreateError<List<RFPItemInfoDto>>(null, MessageId.ImpossibleDuplicateProduct);

                if (rfpModel.RFPItems != null)
                {
                    var beforeItems = rfpModel.RFPItems.Where(a => !a.IsDeleted && a.IsActive).Select(c => c.PurchaseRequestItemId).ToList();
                    if (beforeItems != null)
                    {
                        if (beforeItems.Any(a => postedPurchaseRequestIds.Contains(a.Value)))
                            return ServiceResultFactory.CreateError<List<RFPItemInfoDto>>(null, MessageId.Duplicate);
                    }
                }

                AddRFPItem(rfpModel.Id, purchaseRequestItemModel, model);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var res = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = rfpModel.ContractCode,
                        RootKeyValue = rfpModel.Id.ToString(),
                        FormCode = rfpModel.RFPNumber,
                        KeyValue = rfpModel.Id.ToString(),
                        NotifEvent = NotifEvent.EditRFP,
                        ProductGroupId = rfpModel.ProductGroupId,
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName
                    }, null);
                    return await GetRFPItemByRFPIdAsync(rfpId);
                }
                return ServiceResultFactory.CreateError<List<RFPItemInfoDto>>(null, MessageId.EditEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<RFPItemInfoDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<List<RFPItemInfoDto>>> GetRFPItemByRFPIdAsync(long rfpId)
        {

            var result = await _rfpItemRepository.Where(a => a.RFPId == rfpId && !a.IsDeleted)
                           .Select(c => new RFPItemInfoDto
                           {
                               Id = c.Id,
                               IsActive = c.IsActive,
                               DateEnd = c.DateEnd.ToUnixTimestamp(),
                               DateStart = c.DateStart.ToUnixTimestamp(),
                               PRCode = c.PurchaseRequestItem != null ? c.PurchaseRequestItem.PurchaseRequest.PRCode : "",
                               ProductCode = c.Product.ProductCode,
                               ProductDescription = c.Product.Description,
                               ProductGroupName = c.Product.ProductGroup.Title,
                               ProductId = c.ProductId,
                               ProductTechnicalNumber = c.Product.TechnicalNumber,
                               ProductUnit = c.Product.Unit,
                               PurchaseRequestId = c.PurchaseRequestItem.PurchaseRequestId,
                               Quantity = c.Quantity,
                           }).ToListAsync();
            return ServiceResultFactory.CreateSuccess(result);
        }

        public async Task<ServiceResult<List<RFPSupplierInfoDto>>> EditRFPInqueryByRFPIdAsync(AuthenticateDto authenticate, long rfpId, RFPInqueryType inqueryType, List<RFPInqueryInfoDto> model)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<RFPSupplierInfoDto>>(null, MessageId.AccessDenied);

                var dbQuery = _rfpRepository
                    .Include(a => a.RFPSuppliers)
                    .Where(a => !a.IsDeleted && a.Id == rfpId && a.ContractCode == authenticate.ContractCode);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(c => permission.ProductGroupIds.Contains(c.ProductGroupId)))
                    return ServiceResultFactory.CreateError<List<RFPSupplierInfoDto>>(null, MessageId.AccessDenied);

                var rfpModel = await _rfpRepository
                    .Include(a => a.RFPSuppliers)
                    .FirstOrDefaultAsync(a => !a.IsDeleted && a.Id == rfpId);

                if (rfpModel == null)
                    return ServiceResultFactory.CreateError<List<RFPSupplierInfoDto>>(null, MessageId.EntityDoesNotExist);

                if (rfpModel.Status == RFPStatus.RFPSelection)
                    return ServiceResultFactory.CreateError<List<RFPSupplierInfoDto>>(null, MessageId.ImpossibleEdit);





                var inqueryModels = await _rfpInqueryRepository.Where(a => !a.IsDeleted && a.RFPId == rfpId && a.RFPInqueryType == inqueryType)
                    .Include(a => a.RFPInqueryAttachments)
                    .Include(a => a.RFPSupplierProposal)
                    .ThenInclude(c => c.RFPSupplierInqueryAttachments)
                    .ToListAsync();
                if (((model == null || !model.Any()) && (inqueryModels.Any(a => !a.IsDeleted && a.RFPSupplierProposal.Any(b => !b.IsDeleted && (b.IsAnswered || b.IsEvaluated))))) || (model != null && model.Any(a => a.Weight < 0)))
                    return ServiceResultFactory.CreateError<List<RFPSupplierInfoDto>>(null, MessageId.InputDataValidationError);

                var postedInqueryIds = model.Where(a => a.RFPInqueryType > 0).Select(a => a.Id).ToList();
                if (postedInqueryIds != null && postedInqueryIds.Any())
                {
                    var updateInqueryList = inqueryModels.Where(a => postedInqueryIds.Contains(a.Id)).ToList();
                    if (updateInqueryList == null || updateInqueryList.Count() != postedInqueryIds.Count())
                        return ServiceResultFactory.CreateError<List<RFPSupplierInfoDto>>(null, MessageId.DataInconsistency);

                    var res = await UpdateRFPInquery(updateInqueryList, model.Where(a => a.RFPInqueryType > 0).ToList());
                    if (!res.Succeeded)
                        return ServiceResultFactory.CreateError<List<RFPSupplierInfoDto>>(null, res.Messages.First().Message);
                }

                var removeInqueryList = inqueryModels.Where(a => !postedInqueryIds.Contains(a.Id)).ToList();
                RemoveRFPInquery(removeInqueryList);

                var newInqueryList = model.Where(a => a.RFPInqueryType == 0).ToList();
                if (newInqueryList != null && newInqueryList.Any())
                {

                    newInqueryList.ForEach(inquery => inquery.RFPInqueryType = inqueryType);
                    var res = await AddRFPInqueryAsync(rfpId, newInqueryList, rfpModel.RFPSuppliers.Where(a => !a.IsDeleted).ToList());
                    if (!res.Succeeded)
                        return ServiceResultFactory.CreateError<List<RFPSupplierInfoDto>>(null, res.Messages.First().Message);
                }
                UpdateRFPStatusAfterEditInquery(rfpModel, inqueryModels);

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {

                    var result = await GetRFPDetailsByIdAsync(authenticate, rfpId);

                    var res = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = rfpModel.ContractCode,
                        RootKeyValue = rfpModel.Id.ToString(),
                        FormCode = rfpModel.RFPNumber,
                        KeyValue = rfpModel.Id.ToString(),
                        NotifEvent = NotifEvent.EditRFP,
                        ProductGroupId = rfpModel.ProductGroupId,
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName
                    }, null);
                    return ServiceResultFactory.CreateSuccess(result.Result.Suppliers);
                }
                return ServiceResultFactory.CreateError<List<RFPSupplierInfoDto>>(null, MessageId.EditEntityFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<RFPSupplierInfoDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<bool>> DeActiveRFPSupplierBySupplierIdAsync(AuthenticateDto authenticate, long rfpId, int supplierId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery = _rfpRepository
                    .Where(a => !a.IsDeleted && a.Id == rfpId &&
                    a.ContractCode == authenticate.ContractCode &&
                    a.RFPSuppliers.Any(c => c.SupplierId == supplierId && c.IsActive))
                    .Include(a => a.RFPSuppliers)
                    .AsQueryable();

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(c => permission.ProductGroupIds.Contains(c.ProductGroupId)))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var rfpModel = await dbQuery
                    .FirstOrDefaultAsync();

                if (rfpModel == null || rfpModel.RFPSuppliers == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (rfpModel.Status == RFPStatus.RFPSelection || rfpModel.Status == RFPStatus.Canceled)
                    return ServiceResultFactory.CreateError(false, MessageId.ImpossibleEdit);

                var removeItem = rfpModel.RFPSuppliers.FirstOrDefault(a => a.SupplierId == supplierId);
                if (removeItem == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (!await _rfpSupplierRepository.AnyAsync(a => !a.IsDeleted && a.IsActive && a.RFPId == rfpId && a.SupplierId != supplierId))
                    return ServiceResultFactory.CreateError(false, MessageId.MinimumLimitRFPSupplier);

                removeItem.IsActive = false;

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    return ServiceResultFactory.CreateSuccess(true);
                }
                return ServiceResultFactory.CreateError(false, MessageId.AddEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<List<RFPSupplierInfoDto>>> EditRFPSupplierAsync(AuthenticateDto authenticate, long rfpId, List<AddRFPSupplierDto> model)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<RFPSupplierInfoDto>>(null, MessageId.AccessDenied);

                var dbQuery = _rfpRepository
                   .Where(a => !a.IsDeleted && a.Id == rfpId && a.ContractCode == authenticate.ContractCode)
                   .Include(a => a.RFPSuppliers)
                   .ThenInclude(a => a.RFPSupplierProposals)
                   .Include(a => a.RFPInqueries)
                   .Include(a => a.RFPStatusLogs)
                   .Include(a => a.RFPSuppliers)
                   .ThenInclude(a => a.RFPProFormas)
                  .AsQueryable();

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(c => permission.ProductGroupIds.Contains(c.ProductGroupId)))
                    return ServiceResultFactory.CreateError<List<RFPSupplierInfoDto>>(null, MessageId.AccessDenied);

                if (model == null || !model.Any())
                    return ServiceResultFactory.CreateError<List<RFPSupplierInfoDto>>(null, MessageId.EntityDoesNotExist);

                var rfpModel = await dbQuery
                   .FirstOrDefaultAsync();

                if (rfpModel == null || rfpModel.RFPSuppliers == null)
                    return ServiceResultFactory.CreateError<List<RFPSupplierInfoDto>>(null, MessageId.EntityDoesNotExist);

                if (rfpModel.Status == RFPStatus.RFPSelection || rfpModel.Status == RFPStatus.Canceled)
                    return ServiceResultFactory.CreateError<List<RFPSupplierInfoDto>>(null, MessageId.ImpossibleEdit);
                var supplierIds = model.Select(a => a.Id).ToList();
                if (await _supplierRepository.CountAsync(a => !a.IsDeleted && supplierIds.Contains(a.Id)) != supplierIds.Count())
                    return ServiceResultFactory.CreateError<List<RFPSupplierInfoDto>>(null, MessageId.DataInconsistency);

                var removeItems = rfpModel.RFPSuppliers.Where(a => !a.IsDeleted && !supplierIds.Contains(a.SupplierId)).ToList();
                var addedItems = supplierIds.Where(a => !rfpModel.RFPSuppliers.Any(b => !b.IsDeleted && b.SupplierId == a)).ToList();
                var removeReault = await RemoveRFPSupplier(rfpModel, removeItems);
                var beforeRFPStatus = rfpModel.Status;
                if (!removeReault.Succeeded)
                    return ServiceResultFactory.CreateError<List<RFPSupplierInfoDto>>(null, MessageId.EditEntityFailed);
                var addedResult = await AddRFPSupplier(rfpModel, addedItems);
                if (!addedResult.Succeeded)
                    return ServiceResultFactory.CreateError<List<RFPSupplierInfoDto>>(null, MessageId.EditEntityFailed);

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var res = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = rfpModel.ContractCode,
                        RootKeyValue = rfpModel.Id.ToString(),
                        FormCode = rfpModel.RFPNumber,
                        KeyValue = rfpModel.Id.ToString(),
                        NotifEvent = NotifEvent.EditRFP,
                        ProductGroupId = rfpModel.ProductGroupId,
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName
                    },
                    rfpModel.ProductGroupId
                    , SendNotifOnAddRfpSupplier(rfpModel, beforeRFPStatus));
                    var result = await _rfpSupplierRepository.Where(a => !a.IsDeleted && a.RFPId == rfpId).Select(s => new RFPSupplierInfoDto
                    {


                        RFPSupplierId = s.Id,
                        RFPId = s.RFPId,
                        SupplierId = s.SupplierId,
                        SupplierCode = s.Supplier.SupplierCode,
                        SupplierEmail = s.Supplier.Email,
                        SupplierName = s.Supplier.Name,
                        IsWinner = s.IsWinner,
                        IsActive = s.IsActive,
                        SupplierProductGroups = null,

                        TechProposalState = !s.RFPSupplierProposals.Any(a => !a.IsDeleted && a.RFPSupplier.IsActive && !a.RFPInquery.IsDeleted && a.RFPInquery.RFPInqueryType == RFPInqueryType.TechnicalInquery)
                               ? SupplierProposalState.None
                               : s.RFPSupplierProposals.Any(a => !a.RFPSupplier.IsDeleted && a.RFPSupplier.IsActive && !a.IsDeleted && !a.IsAnswered && !a.RFPInquery.IsDeleted && a.RFPInquery.RFPInqueryType == RFPInqueryType.TechnicalInquery)
                               ? SupplierProposalState.Completing
                               : s.RFPSupplierProposals.Any(a => !a.RFPSupplier.IsDeleted && a.RFPSupplier.IsActive && !a.IsDeleted && a.IsAnswered && !a.IsEvaluated && !a.RFPInquery.IsDeleted && a.RFPInquery.RFPInqueryType == RFPInqueryType.TechnicalInquery)
                               ? SupplierProposalState.Answered
                               : SupplierProposalState.Evaluation,

                        CommercialProposalState = !s.RFPSupplierProposals.Any(a => !a.IsDeleted && a.RFPSupplier.IsActive && !a.RFPInquery.IsDeleted && a.RFPInquery.RFPInqueryType == RFPInqueryType.CommercialInquery)
                               ? SupplierProposalState.None
                               : s.RFPSupplierProposals.Any(a => !a.RFPSupplier.IsDeleted && a.RFPSupplier.IsActive && !a.IsDeleted && !a.IsAnswered && !a.RFPInquery.IsDeleted && a.RFPInquery.RFPInqueryType == RFPInqueryType.CommercialInquery)
                               ? SupplierProposalState.Completing
                               : s.RFPSupplierProposals.Any(a => !a.RFPSupplier.IsDeleted && a.RFPSupplier.IsActive && !a.IsDeleted && a.IsAnswered && !a.IsEvaluated && !a.RFPInquery.IsDeleted && a.RFPInquery.RFPInqueryType == RFPInqueryType.CommercialInquery)
                               ? SupplierProposalState.Answered
                               : SupplierProposalState.Evaluation,

                        ProFormaStatus = !s.RFPProFormas.Any(a => !a.IsDeleted) ? ProFormaStatus.NotHaveProForma : ProFormaStatus.HaveProForma

                    }).ToListAsync();
                    return ServiceResultFactory.CreateSuccess(result);
                    //return await GetRFPSupplierByRFPIdAsync(rfpId);

                }
                return ServiceResultFactory.CreateError<List<RFPSupplierInfoDto>>(null, MessageId.AddEntityFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<RFPSupplierInfoDto>>(null, exception);
            }
        }

        private List<NotifToDto> SendNotifOnAddRfpSupplier(RFP rfpModel, RFPStatus beforeRFPStatus)
        {
            {
                var result = new List<NotifToDto>();
                if (rfpModel.RFPInqueries == null || rfpModel.RFPInqueries.Count() == 0)
                    return null;

                if (rfpModel.RFPInqueries.Any(a => !a.IsDeleted && a.RFPInqueryType == RFPInqueryType.TechnicalInquery && beforeRFPStatus >= RFPStatus.TechnicalProposal))
                {
                    result.Add(new NotifToDto
                    {
                        NotifEvent = NotifEvent.AddTechProposal,
                        Roles = new List<string>
                    {
                        SCMRole.RFPTechMng
                    }
                    });
                }

                if (rfpModel.RFPInqueries.Any(a => !a.IsDeleted && a.RFPInqueryType == RFPInqueryType.CommercialInquery) && beforeRFPStatus >= RFPStatus.CommercialProposal)
                {
                    result.Add(new NotifToDto
                    {
                        NotifEvent = NotifEvent.AddCommercialProposal,
                        Roles = new List<string>
                    {
                        SCMRole.RFPCommercialMng
                    }
                    });
                }

                return result.Count() > 0 ? result : null;
            }
        }

        public async Task<ServiceResult<SetProposalWinnerDto>> SetRFPSupplierWinner(AuthenticateDto authenticate, long rfpId, List<int> supplierIds)
        {
            try
            {
                if (supplierIds == null || !supplierIds.Any())
                    return ServiceResultFactory.CreateError<SetProposalWinnerDto>(null, MessageId.InputDataValidationError);

                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<SetProposalWinnerDto>(null, MessageId.AccessDenied);

                var dbQuery = _rfpRepository.Where(a => a.Id == rfpId && a.ContractCode == authenticate.ContractCode)
                    .Include(a => a.RFPSuppliers)
                    .Include(a => a.RFPItems)
                    .ThenInclude(c => c.PurchaseRequestItem)
                    .ThenInclude(c => c.PurchaseRequest)
                  .AsQueryable();

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(c => permission.ProductGroupIds.Contains(c.ProductGroupId)))
                    return ServiceResultFactory.CreateError<SetProposalWinnerDto>(null, MessageId.AccessDenied);

                // authentication
                var rfpModel = await dbQuery
                    .FirstOrDefaultAsync();

                if (rfpModel == null || rfpModel.RFPSuppliers == null || !rfpModel.RFPSuppliers.Any())
                    return ServiceResultFactory.CreateError<SetProposalWinnerDto>(null, MessageId.EntityDoesNotExist);
                if (rfpModel.RFPType == RFPType.Proposal)
                {
                    if (rfpModel.Status < RFPStatus.RFPEvaluation)
                        return ServiceResultFactory.CreateError<SetProposalWinnerDto>(null, MessageId.RFPSupplierStatusError);
                }



                var selectedSuppliers = rfpModel.RFPSuppliers
                    .Where(a => !a.IsDeleted && a.IsActive && supplierIds.Contains(a.SupplierId))
                    .ToList();

                if (selectedSuppliers == null || selectedSuppliers.Count() != supplierIds.Count())
                    return ServiceResultFactory.CreateError<SetProposalWinnerDto>(null, MessageId.EntityDoesNotExist);
                foreach (var item in rfpModel.RFPSuppliers)
                {
                    item.IsWinner = false;
                }


                foreach (var item in selectedSuppliers)
                {
                    item.IsWinner = true;
                }

                rfpModel.Status = RFPStatus.RFPSelection;

                if (rfpModel.RFPStatusLogs == null || !rfpModel.RFPStatusLogs.Any(a => a.Status == RFPLogStatus.RFPSelection))
                {
                    var rfpLog = new RFPStatusLog
                    {
                        DateIssued = DateTime.UtcNow,
                        RFPId = rfpId,
                        Status = RFPLogStatus.RFPSelection
                    };
                    _rfpStatusLogRepository.Add(rfpLog);
                }
                foreach (var item in rfpModel.RFPItems)
                {
                    if (item.IsActive && !item.IsDeleted)
                        item.PurchaseRequestItem.PRItemStatus = PRItemStatus.Completed;

                    var mrpItem = _mrpItemRepository
                          .Where(a => a.ProductId == item.ProductId &&
                          a.MrpId == item.PurchaseRequestItem.PurchaseRequest.MrpId && !a.IsDeleted)
                          .FirstOrDefault();

                    if (mrpItem == null)
                        return ServiceResultFactory.CreateError<SetProposalWinnerDto>(null, MessageId.DataInconsistency);

                    if (mrpItem.MrpItemStatus < MrpItemStatus.RFPDone)
                        mrpItem.MrpItemStatus = MrpItemStatus.RFPDone;

                }

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    await _scmLogAndNotificationService.SetDonedNotificationAsync(authenticate.UserId, rfpModel.ContractCode, rfpModel.Id.ToString(), NotifEvent.SetRFPWinner);
                    await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = rfpModel.ContractCode,
                        FormCode = rfpModel.RFPNumber,
                        RootKeyValue = rfpModel.Id.ToString(),
                        KeyValue = rfpModel.Id.ToString(),
                        NotifEvent = NotifEvent.SetRFPWinner,
                        ProductGroupId = rfpModel.ProductGroupId,
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,
                    },
                    rfpModel.ProductGroupId
                    , new List<NotifToDto> {
                    new NotifToDto
                    {
                        NotifEvent= NotifEvent.AddPrContract,
                        Roles= new List<string>
                        {
                           SCMRole.PrContractMng,
                           SCMRole.PrContractReg
                        }
                    }
                    });
                    SetProposalWinnerDto result = new SetProposalWinnerDto();
                    var supplier = await GetRFPDetailsByIdAsync(authenticate, rfpId);
                    result.Suppliers = supplier.Result.Suppliers;
                    result.RFPStatus = supplier.Result.Status;
                    return ServiceResultFactory.CreateSuccess(result);
                }
                return ServiceResultFactory.CreateError<SetProposalWinnerDto>(null, MessageId.SaveFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<SetProposalWinnerDto>(null, exception);
            }
        }

        public async Task<ServiceResult<RFPStatus>> GetRFPStatusByRFPIdAsync(AuthenticateDto authenticate, long rfpId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<RFPStatus>(0, MessageId.AccessDenied);

                var dbQuery = _rfpRepository
                   .Where(a => !a.IsDeleted && a.Id == rfpId && a.ContractCode == authenticate.ContractCode)
                  .AsQueryable();

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(c => permission.ProductGroupIds.Contains(c.ProductGroupId)))
                    return ServiceResultFactory.CreateError<RFPStatus>(0, MessageId.AccessDenied);

                var rfpModel = await dbQuery.FirstOrDefaultAsync();
                if (rfpModel == null)
                    return ServiceResultFactory.CreateError(RFPStatus.Canceled, MessageId.EntityDoesNotExist);

                return ServiceResultFactory.CreateSuccess(rfpModel.Status);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(RFPStatus.Register, exception);
            }
        }

        #endregion

        #region PRContract

        public async Task<ServiceResult<int>> GetWaitingRFPItemForCreatePRContractBadgeAsync(AuthenticateDto authenticate)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(0, MessageId.AccessDenied);

                var dbQuery = _rfpItemRepository
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted &&
                    x.IsActive &&
                    x.RemainedStock > 0 &&
                    !x.RFP.IsDeleted &&
                    x.RFP.Status == RFPStatus.RFPSelection &&
                    x.RFP.ContractCode == authenticate.ContractCode)
                    .AsQueryable();

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(c => permission.ProductGroupIds.Contains(c.RFP.ProductGroupId)))
                    return ServiceResultFactory.CreateError(0, MessageId.AccessDenied);

                var result = await dbQuery.CountAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(0, exception);
            }
        }

        public async Task<ServiceResult<List<RFPDataForPRContractDto>>> GetRFPForCreatePRContractAsync(AuthenticateDto authenticate, RFPQueryDto query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<RFPDataForPRContractDto>>(null, MessageId.AccessDenied);

                var dbQuery = _rfpRepository
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && x.Status == RFPStatus.RFPSelection &&
                    x.ContractCode == authenticate.ContractCode &&
                      x.RFPItems.Any(c => !c.IsDeleted && c.IsActive && c.RemainedStock > 0))
                    .OrderByDescending(x => x.Id)
                    .AsQueryable();

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(c => permission.ProductGroupIds.Contains(c.ProductGroupId)))
                    return ServiceResultFactory.CreateError<List<RFPDataForPRContractDto>>(null, MessageId.AccessDenied);

                var totalCount = dbQuery.Count();
                var result = await dbQuery.Select(rfp => new RFPDataForPRContractDto
                {
                    Id = rfp.Id,
                    RFPType = rfp.RFPType,
                    Status = rfp.Status,
                    DateDue = rfp.DateDue.ToUnixTimestamp(),
                    DateSelectWiiner = rfp.RFPStatusLogs.Where(a => a.Status == RFPLogStatus.RFPSelection).Select(c => c.DateIssued).FirstOrDefault().ToUnixTimestamp(),
                    RFPNumber = rfp.RFPNumber,
                    ContractCode = rfp.ContractCode,
                    DateCreate = rfp.CreatedDate.ToUnixTimestamp(),
                    RFPItems = rfp.RFPItems.Where(a => !a.IsDeleted && a.IsActive)
                    .Select(c => c.Product.Description).ToList(),
                    Suppliers = rfp.RFPSuppliers.Where(a => !a.IsDeleted && a.IsActive && a.IsWinner).Select(c => c.Supplier.Name).ToList(),
                    UserAudit = rfp.AdderUser != null ? new UserAuditLogDto
                    {
                        AdderUserId = rfp.AdderUserId,
                        AdderUserName = rfp.AdderUser.FullName,
                        CreateDate = rfp.CreatedDate.ToUnixTimestamp(),
                        AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + rfp.AdderUser.Image
                    } : null,
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new List<RFPDataForPRContractDto>(), exception);
            }
        }

        public async Task<ServiceResult<List<RFPItemInfoDto>>> GetRFPItemsByRFPIdForCreatePrContractAsync(AuthenticateDto authenticate, long rfpId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<RFPItemInfoDto>>(null, MessageId.AccessDenied);

                var dbQuery = _rfpItemRepository
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && x.RFPId == rfpId && x.IsActive && x.RemainedStock > 0)
                    .AsQueryable();

                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(c => permission.ProductGroupIds.Contains(c.RFP.ProductGroupId));

                var result = await dbQuery
                    .Select(c => new RFPItemInfoDto
                    {
                        Id = c.Id,
                        IsActive = c.IsActive,
                        DateEnd = c.DateEnd.ToUnixTimestamp(),
                        DateStart = c.DateStart.ToUnixTimestamp(),
                        PRCode = c.PurchaseRequestItem != null ? c.PurchaseRequestItem.PurchaseRequest.PRCode : "",
                        ProductCode = c.Product.ProductCode,
                        ProductDescription = c.Product.Description,
                        ProductGroupName = c.Product.ProductGroup.Title,
                        ProductId = c.ProductId,
                        ProductTechnicalNumber = c.Product.TechnicalNumber,
                        ProductUnit = c.Product.Unit,
                        PurchaseRequestId = c.PurchaseRequestItem.PurchaseRequestId,
                        Quantity = c.Quantity,
                        //DocumentStatus =
                        //    !c.Product.DocumentProducts.Any(v => !v.Document.IsDeleted && v.Document.IsActive && v.Document.ContractCode == authenticate.ContractCode)
                        //    ? EngineeringDocumentStatus.No
                        //    : c.Product.DocumentProducts.Any(v => !v.Document.IsDeleted && v.Document.IsActive && v.Document.ContractCode == authenticate.ContractCode &&
                        //    (!v.Document.DocumentRevisions.Any(a => !a.IsDeleted) || v.Document.DocumentRevisions.Any(n => !n.IsDeleted && n.RevisionStatus < RevisionStatus.Confirmed)))
                        //    ? EngineeringDocumentStatus.completing
                        //    : EngineeringDocumentStatus.Completed,
                    }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new List<RFPItemInfoDto>(), exception);
            }
        }

        public async Task<ServiceResult<List<WaitingRFPItemForAddPrContractDto>>> GetWaitingRFPItemsForCreatePrContractAsync(AuthenticateDto authenticate, RFPItemQueryDto query, long? prContractId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<WaitingRFPItemForAddPrContractDto>>(null, MessageId.AccessDenied);
                PRContract prContract = null;
                if (prContractId != null)
                    prContract = await _prContractRepository.Where(a => a.Id == prContractId.Value).Include(a => a.PRContractSubjects).FirstOrDefaultAsync();
                var dbQuery = _rfpItemRepository
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted &&
                    !x.RFP.IsDeleted &&
                    x.RFP.Status == RFPStatus.RFPSelection &&
                    x.RFP.ContractCode == authenticate.ContractCode &&
                    x.IsActive &&
                    (x.RemainedStock > 0 || (prContractId != null && x.PRContractSubjects.Any(a => !a.IsDeleted && a.PRContract.PRContractStatus != PRContractStatus.Canceled && a.PRContractId == prContractId))))
                    .AsQueryable();

                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(c => permission.ProductGroupIds.Contains(c.RFP.ProductGroupId));

                if (query.SupplierId > 0)
                    dbQuery = dbQuery.Where(a => a.RFP.RFPSuppliers.Any(c => c.IsActive && !c.IsDeleted && c.SupplierId == query.SupplierId && c.IsWinner));

                if (query.ProductGroupId > 0)
                    dbQuery = dbQuery.Where(a => a.RFP.ProductGroupId == query.ProductGroupId);

                var tempResult = await dbQuery
                    .Select(c => new WaitingRFPItemForAddPrContractDto
                    {
                        Id = c.Id,
                        IsActive = c.IsActive,
                        DateEnd = c.DateEnd.ToUnixTimestamp(),
                        DateStart = c.DateStart.ToUnixTimestamp(),
                        PRCode = c.PurchaseRequestItem != null ? c.PurchaseRequestItem.PurchaseRequest.PRCode : "",
                        ProductCode = c.Product.ProductCode,
                        ProductDescription = c.Product.Description,
                        ProductGroupName = c.Product.ProductGroup.Title,
                        ProductId = c.ProductId,
                        ProductTechnicalNumber = c.Product.TechnicalNumber,
                        ProductUnit = c.Product.Unit,
                        PurchaseRequestId = c.PurchaseRequestItem.PurchaseRequestId,
                        DateWinner = c.RFP.RFPStatusLogs.Where(a => a.Status == RFPLogStatus.RFPSelection).Select(v => v.DateIssued).FirstOrDefault().ToUnixTimestamp(),
                        Quantity = c.RemainedStock,
                        RFPId = c.RFPId,
                        RFPNumber = c.RFP.RFPNumber,
                        ProductGroupId = c.RFP.ProductGroupId,
                        ProductGroupTitle = c.RFP.ProductGroup.Title
                    }).ToListAsync();

                List<WaitingRFPItemForAddPrContractDto> result = new List<WaitingRFPItemForAddPrContractDto>();
                foreach (var item in tempResult)
                {
                    if (!result.Any(a => a.ProductId == item.ProductId))
                    {
                        result.Add(new WaitingRFPItemForAddPrContractDto
                        {

                            Id = item.Id,
                            IsActive = item.IsActive,
                            DateEnd = item.DateEnd,
                            DateStart = item.DateStart,
                            PRCode = item.PRCode,
                            ProductCode = item.ProductCode,
                            ProductDescription = item.ProductDescription,
                            ProductGroupName = item.ProductGroupName,
                            ProductId = item.ProductId,
                            ProductTechnicalNumber = item.ProductTechnicalNumber,
                            ProductUnit = item.ProductUnit,
                            PurchaseRequestId = item.PurchaseRequestId,
                            DateWinner = item.DateWinner,
                            Quantity = (prContract != null && prContract.PRContractSubjects.Any(a => !a.IsDeleted && a.ProductId == item.ProductId)) ? prContract.PRContractSubjects.Where(a => !a.IsDeleted && a.ProductId == item.ProductId).Sum(a => a.Quantity) + tempResult.Where(a => a.ProductId == item.ProductId).Sum(a => a.Quantity) : tempResult.Where(a => a.ProductId == item.ProductId).Sum(a => a.Quantity),
                            RFPId = item.RFPId,
                            RFPNumber = item.RFPNumber,
                            ProductGroupId = item.ProductGroupId,
                            ProductGroupTitle = item.ProductGroupTitle
                        });
                    }
                }
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new List<WaitingRFPItemForAddPrContractDto>(), exception);
            }
        }

        #endregion

        #region rfpSupplier service
        public async Task<ServiceResult<RFPEvaluationProposalInfoDto>> GetRFPSupplierInqueryAsync(AuthenticateDto authenticate, long rfpId, long SupplierId, RFPInqueryType inqueryType)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<RFPEvaluationProposalInfoDto>(null, MessageId.AccessDenied);

                var dbQuery = _rfpSupplierRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted
                    && a.RFPId == rfpId
                    && a.RFP.ContractCode == authenticate.ContractCode
                    && a.SupplierId == SupplierId)
                    .AsQueryable();

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(c => permission.ProductGroupIds.Contains(c.RFP.ProductGroupId)))
                    return ServiceResultFactory.CreateError<RFPEvaluationProposalInfoDto>(null, MessageId.AccessDenied);

                var result = await dbQuery
                    .Select(k => new
                    RFPEvaluationProposalInfoDto
                    {
                        EvaluationNote = inqueryType == RFPInqueryType.TechnicalInquery ? k.TBENote : k.CBENote,
                        EvaluationScore = inqueryType == RFPInqueryType.TechnicalInquery ? k.TBEScore : k.CBEScore,
                        InqueryType = inqueryType,
                        SupplierProposals = k.RFPSupplierProposals
                        .Where(d => !d.IsDeleted && !d.RFPInquery.IsDeleted && d.RFPInquery.RFPInqueryType == inqueryType)
                        .Select(c => new
                       ListSupplierProposalInqueryDto
                        {
                            Id = c.Id,
                            RFPInqueryId = c.RFPInqueryId,
                            IsEvaluated = c.IsEvaluated,
                            IsAnswered = c.IsAnswered,
                            Description = c.RFPInquery.Description,
                            Weight = c.RFPInquery.Weight,
                            RFPInqueryType = c.RFPInquery.RFPInqueryType,
                            EvaluationScore = GetEvaluateScore(c.IsEvaluated, c.EvaluationScore),
                            PoroposalDescription = c.Description,
                            RFPSupplierId = c.RFPSupplierId,

                            InqueryAttachments = c.RFPInquery.RFPInqueryAttachments.Where(v => !v.IsDeleted)
                       .Select(b => new RFPInqueryAttachmentDto
                       {
                           Id = b.Id,
                           FileName = b.FileName,
                           FileSize = b.FileSize,
                           FileType = b.FileType,
                           FileSrc = b.FileSrc
                       }).ToList(),
                            PoroposalAttachments = c.RFPSupplierInqueryAttachments.Where(v => !v.IsDeleted)
                       .Select(b => new RFPInqueryAttachmentDto
                       {
                           Id = b.Id,
                           FileName = b.FileName,
                           FileSize = b.FileSize,
                           FileType = b.FileType,
                           FileSrc = b.FileSrc
                       }).ToList(),

                        }).ToList()
                    }).FirstOrDefaultAsync();

                if (result != null && result.SupplierProposals != null && result.SupplierProposals.Any())
                {
                    decimal score = 0;
                    if (result.EvaluationScore <= 0)
                    {
                        var supplierScore = result.SupplierProposals.Where(a => a.EvaluationScore != null).Sum(a => (a.Weight * a.EvaluationScore.Value));
                        if (result.SupplierProposals.Any(a => !a.IsEvaluated))
                            result.EvaluationScore = null;
                        else
                        {
                            var sumWeight = result.SupplierProposals.Sum(a => a.Weight);

                            if (sumWeight == 0)
                                score = 0;
                            else
                                score = supplierScore / sumWeight;

                            result.EvaluationScore = score;
                        }

                    }
                }

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<RFPEvaluationProposalInfoDto>(null, exception);
            }
        }

        public async Task<ServiceResult<AddOrEditSupplierProposalDto>> AddRFPSupplierProposalAsync(AuthenticateDto authenticate,
            long rfpId, int supplierId, RFPInqueryType inqueryType, List<AddSupplierProposalDto> model)
        {
            try
            {
                if (model == null || !model.Any())
                    return ServiceResultFactory.CreateError<AddOrEditSupplierProposalDto>(null, MessageId.InputDataValidationError);

                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<AddOrEditSupplierProposalDto>(null, MessageId.AccessDenied);

                var dbQuery = _rfpRepository
                    .Where(a => !a.IsDeleted &&
                    a.Id == rfpId &&
                    a.ContractCode == authenticate.ContractCode &&
                    a.RFPSuppliers.Any(c => !c.IsDeleted &&
                    c.IsActive && c.SupplierId == supplierId))
                    .AsQueryable();

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(c => permission.ProductGroupIds.Contains(c.ProductGroupId)))
                    return ServiceResultFactory.CreateError<AddOrEditSupplierProposalDto>(null, MessageId.AccessDenied);

                var rfpModel = await dbQuery
                    .Include(a => a.RFPStatusLogs)
                    .FirstOrDefaultAsync();

                if (rfpModel == null)
                    return ServiceResultFactory.CreateError<AddOrEditSupplierProposalDto>(null, MessageId.EntityDoesNotExist);


                var supplierModel = await _supplierRepository.FirstOrDefaultAsync(a => a.Id == supplierId);

                if (supplierModel == null)
                    return ServiceResultFactory.CreateError<AddOrEditSupplierProposalDto>(null, MessageId.SupplierNotFound);

                if (rfpModel.Status == RFPStatus.RFPSelection)
                    return ServiceResultFactory.CreateError<AddOrEditSupplierProposalDto>(null, MessageId.ImpossibleEdit);

                var postedIds = model.Select(a => a.Id).ToList();

                var rfpSupplierProposalModels = await _rfpSupplierProposalRepository
                     .Where(a => !a.IsDeleted &&
                     a.RFPSupplier.IsActive &&
                     a.RFPSupplier.RFPId == rfpId &&
                     a.RFPInquery.RFPInqueryType == inqueryType)
                     .Include(a => a.RFPSupplier)
                     .Include(a => a.RFPSupplierInqueryAttachments)
                     .ToListAsync();

                if (rfpSupplierProposalModels == null)
                    return ServiceResultFactory.CreateError<AddOrEditSupplierProposalDto>(null, MessageId.DataInconsistency);

                var currentSupplierProposalModel = rfpSupplierProposalModels
                    .Where(a => a.RFPSupplier.SupplierId == supplierId)
                    .ToList();

                bool isBeforeAnsweredAll = false;
                if (!currentSupplierProposalModel.Any(a => a.IsAnswered == false))
                    isBeforeAnsweredAll = true;

                var supplierProposals = currentSupplierProposalModel.Where(a => postedIds.Contains(a.Id)).ToList();
                if (supplierProposals == null || supplierProposals.Count() != postedIds.Count())
                    return ServiceResultFactory.CreateError<AddOrEditSupplierProposalDto>(null, MessageId.DataInconsistency);

                foreach (var item in model)
                {
                    if (!string.IsNullOrEmpty(item.PoroposalDescription) || (item.PoroposalAttachments != null && item.PoroposalAttachments.Any()))
                    {
                        var currentProposal = supplierProposals.FirstOrDefault(a => a.Id == item.Id);
                        currentProposal.IsAnswered = true;
                        currentProposal.Description = item.PoroposalDescription;

                        var res = await AddSupplierProposalAttachmentAsync(currentProposal, item.PoroposalAttachments);
                        if (!res.Succeeded)
                            return ServiceResultFactory.CreateError<AddOrEditSupplierProposalDto>(null, res.Messages.First().Message);
                    }
                }

                bool isSetTaskDone = false;

                var evaluationTask = new List<NotifToDto>();

                if (!rfpSupplierProposalModels.Any(c => c.IsAnswered == false))
                {
                    isSetTaskDone = true;
                    UpdateRFPstatosOnAddProposal(rfpId, inqueryType, rfpModel);
                }

                if (!isBeforeAnsweredAll && !currentSupplierProposalModel.Any(c => c.IsAnswered == false))
                    evaluationTask = SendNotifOnAddproposal(inqueryType, currentSupplierProposalModel);


                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    if (isSetTaskDone)
                    {
                        var notifEvent = inqueryType == RFPInqueryType.TechnicalInquery ? NotifEvent.AddTechProposal : NotifEvent.AddCommercialProposal;
                        var res2 = await _scmLogAndNotificationService.SetDonedNotificationAsync(authenticate.UserId, authenticate.ContractCode, rfpId.ToString(), notifEvent);
                    }

                    var res = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = rfpModel.ContractCode,
                        RootKeyValue = rfpModel.Id.ToString(),
                        FormCode = rfpModel.RFPNumber,
                        Description = ((int)inqueryType).ToString(),
                        Temp = supplierModel.Name,
                        RootKeyValue2 = supplierId.ToString(),
                        KeyValue = rfpModel.Id.ToString(),
                        Quantity = supplierId.ToString(),
                        ProductGroupId = rfpModel.ProductGroupId,
                        NotifEvent = inqueryType == RFPInqueryType.TechnicalInquery ? NotifEvent.AddTechProposal : NotifEvent.AddCommercialProposal,
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName
                    },
                rfpModel.ProductGroupId,
                evaluationTask);

                    AddOrEditSupplierProposalDto result = new AddOrEditSupplierProposalDto();
                    var proposal = await GetRFPSupplierInqueryAsync(authenticate, rfpId, supplierId, inqueryType);
                    var supplier = await GetRFPDetailsByIdAsync(authenticate, rfpId);
                    result.SupplierPropsoal = proposal.Result;
                    result.Suppliers = supplier.Result.Suppliers;
                    result.RFPStatus = supplier.Result.Status;
                    return ServiceResultFactory.CreateSuccess(result);
                }



                return ServiceResultFactory.CreateError<AddOrEditSupplierProposalDto>(null, MessageId.AddEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<AddOrEditSupplierProposalDto>(null, exception);
            }
        }
        public async Task<ServiceResult<AddOrEditProFormaDto>> AddRFPSupplierProFormaAsync(AuthenticateDto authenticate, long rfpId, long rfpSupplierId, AddRFPProFromaDto model)
        {
            try
            {
                if (model == null || (model != null && (String.IsNullOrEmpty(model.Price) && (model.ProFromaAttachments == null || !model.ProFromaAttachments.Any()))))
                    return ServiceResultFactory.CreateError<AddOrEditProFormaDto>(null, MessageId.InputDataValidationError);

                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<AddOrEditProFormaDto>(null, MessageId.AccessDenied);

                var dbQuery = _rfpRepository
                    .Where(a => !a.IsDeleted &&
                    a.Id == rfpId &&
                    a.ContractCode == authenticate.ContractCode &&
                    a.RFPSuppliers.Any(c => !c.IsDeleted &&
                    c.IsActive && c.Id == rfpSupplierId))
                    .AsQueryable();

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(c => permission.ProductGroupIds.Contains(c.ProductGroupId)))
                    return ServiceResultFactory.CreateError<AddOrEditProFormaDto>(null, MessageId.AccessDenied);

                var rfpModel = await dbQuery
                    .Include(a => a.RFPStatusLogs)
                    .Include(a => a.RFPSuppliers)
                    .ThenInclude(a => a.Supplier)
                    .FirstOrDefaultAsync();

                if (rfpModel == null)
                    return ServiceResultFactory.CreateError<AddOrEditProFormaDto>(null, MessageId.EntityDoesNotExist);


                var supplierModel = rfpModel.RFPSuppliers.First(a => a.Id == rfpSupplierId).Supplier;

                if (supplierModel == null)
                    return ServiceResultFactory.CreateError<AddOrEditProFormaDto>(null, MessageId.SupplierNotFound);

                if (rfpModel.Status == RFPStatus.RFPSelection)
                    return ServiceResultFactory.CreateError<AddOrEditProFormaDto>(null, MessageId.ImpossibleEdit);
                RFPProForma proForm = null;

                proForm = new RFPProForma();
                proForm.Duration = model.Duration;
                proForm.Price = model.Price;
                proForm.RFPId = rfpId;
                proForm.RFPSupplierId = rfpSupplierId;

                if (model.ProFromaAttachments != null && model.ProFromaAttachments.Any())
                {
                    var res = await AddSupplierProFormaAttachmentAsync(model.ProFromaAttachments);
                    if (!res.Succeeded)
                        return ServiceResultFactory.CreateError<AddOrEditProFormaDto>(null, res.Messages.First().Message);
                    proForm.RFPProFormaAttachments = res.Result;
                }


                await _proFromaRepository.AddAsync(proForm);

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    if (await _rfpSupplierRepository.Where(a => a.RFPId == rfpId && !a.IsDeleted && a.RFPProFormas.Any(a => !a.IsDeleted)).CountAsync() == rfpModel.RFPSuppliers.Where(a => !a.IsDeleted).Count())
                    {

                        var notifEvent = NotifEvent.AddRFPProForma;
                        var res2 = await _scmLogAndNotificationService.SetDonedNotificationAsync(authenticate.UserId, authenticate.ContractCode, rfpId.ToString(), notifEvent);
                    }
                    var suppliers = await GetRFPDetailsByIdAsync(authenticate, rfpId);
                    var proforma = await GetRFPProFormaDetailAsync(authenticate, rfpId, rfpSupplierId);
                    var res = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = rfpModel.ContractCode,
                        RootKeyValue = rfpModel.Id.ToString(),
                        FormCode = rfpModel.RFPNumber,
                        Description = "3",
                        Temp = supplierModel.Name,
                        RootKeyValue2 = supplierModel.Id.ToString(),
                        KeyValue = rfpModel.Id.ToString(),
                        Quantity = supplierModel.Id.ToString(),
                        ProductGroupId = rfpModel.ProductGroupId,
                        NotifEvent = NotifEvent.AddRFPProForma,
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName
                    },
                rfpModel.ProductGroupId, null);
                    AddOrEditProFormaDto result = new AddOrEditProFormaDto();
                    result.ProForma = proforma.Result;
                    result.Suppliers = suppliers.Result.Suppliers;
                    return ServiceResultFactory.CreateSuccess(result);
                }
                return ServiceResultFactory.CreateError<AddOrEditProFormaDto>(null, MessageId.AddEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<AddOrEditProFormaDto>(null, exception);
            }
        }
        public async Task<ServiceResult<AddOrEditProFormaDto>> EditRFPSupplierProFormaAsync(AuthenticateDto authenticate, long rfpSupplierId, long rfpProformaId, AddRFPProFromaDto model)
        {
            try
            {
                if (model == null || (model != null && (String.IsNullOrEmpty(model.Price) && (model.ProFromaAttachments == null || !model.ProFromaAttachments.Any()))))
                    return ServiceResultFactory.CreateError<AddOrEditProFormaDto>(null, MessageId.InputDataValidationError);

                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<AddOrEditProFormaDto>(null, MessageId.AccessDenied);

                var dbQuery = _proFromaRepository
                    .Where(a => !a.IsDeleted &&
                    a.Id == rfpProformaId &&
                    a.RFP.ContractCode == authenticate.ContractCode)
                    .AsQueryable();



                var rFPProForma = await dbQuery
                    .Include(a => a.RFP)
                    .Include(a => a.RFPSupplier)
                    .ThenInclude(a => a.Supplier)
                    .Include(a => a.RFPProFormaAttachments)
                    .FirstOrDefaultAsync();

                if (rFPProForma == null)
                    return ServiceResultFactory.CreateError<AddOrEditProFormaDto>(null, MessageId.EntityDoesNotExist);
                if (permission.ProductGroupIds.Any() && !dbQuery.Any(c => permission.ProductGroupIds.Contains(rFPProForma.RFP.ProductGroupId)))
                    return ServiceResultFactory.CreateError<AddOrEditProFormaDto>(null, MessageId.AccessDenied);

                var supplierModel = rFPProForma.RFPSupplier.Supplier;

                if (supplierModel == null)
                    return ServiceResultFactory.CreateError<AddOrEditProFormaDto>(null, MessageId.SupplierNotFound);

                if (rFPProForma.RFP.Status == RFPStatus.RFPSelection)
                    return ServiceResultFactory.CreateError<AddOrEditProFormaDto>(null, MessageId.ImpossibleEdit);

                rFPProForma.Duration = model.Duration;
                rFPProForma.Price = model.Price;



                var res = await EditSupplierProFormaAttachmentAsync(model.ProFromaAttachments, rFPProForma);
                if (!res.Succeeded)
                    return ServiceResultFactory.CreateError<AddOrEditProFormaDto>(null, res.Messages.First().Message);



                if (await _unitOfWork.SaveChangesAsync() > 0)
                {


                    //    var res = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    //    {
                    //        ContractCode = rfpModel.ContractCode,
                    //        RootKeyValue = rfpModel.Id.ToString(),
                    //        FormCode = rfpModel.RFPNumber,
                    //        Description = rfpModel.Note,
                    //        Temp = supplierModel.Name,
                    //        RootKeyValue2 = supplierId.ToString(),
                    //        KeyValue = rfpModel.Id.ToString(),
                    //        Quantity = supplierId.ToString(),
                    //        ProductGroupId = rfpModel.ProductGroupId,
                    //        NotifEvent = inqueryType == RFPInqueryType.TechnicalInquery ? NotifEvent.AddTechProposal : NotifEvent.AddCommercialProposal,
                    //        PerformerUserId = authenticate.UserId,
                    //        PerformerUserFullName = authenticate.UserFullName
                    //    },
                    //rfpModel.ProductGroupId,
                    //evaluationTask);
                    var suppliers = await GetRFPDetailsByIdAsync(authenticate, rFPProForma.RFPId);
                    var proforma = await GetRFPProFormaDetailAsync(authenticate, rFPProForma.RFPId, rfpSupplierId);
                    AddOrEditProFormaDto result = new AddOrEditProFormaDto();
                    result.ProForma = proforma.Result;
                    result.Suppliers = suppliers.Result.Suppliers;
                    return ServiceResultFactory.CreateSuccess(result);
                }
                return ServiceResultFactory.CreateError<AddOrEditProFormaDto>(null, MessageId.AddEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<AddOrEditProFormaDto>(null, exception);
            }
        }
        public async Task<ServiceResult<RFPProFormDetailDto>> GetRFPProFormaDetailAsync(AuthenticateDto authenticate, long rfpId, long rfpSupplierId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<RFPProFormDetailDto>(null, MessageId.AccessDenied);

                var dbQuery = _rfpSupplierRepository
                    .AsNoTracking()
                    .Include(a => a.RFPProFormas)
                    .ThenInclude(a => a.RFPProFormaAttachments)
                    .Where(a => !a.IsDeleted
                    && a.Id == rfpSupplierId);
                if (await dbQuery.CountAsync() == 0)
                    return ServiceResultFactory.CreateError<RFPProFormDetailDto>(null, MessageId.EntityDoesNotExist);

                var result = await dbQuery.Select(a => new RFPProFormaDetailListDto
                {
                    ProFromas = a.RFPProFormas.Where(a => !a.IsDeleted).Select(b => new RFPProFormDetailDto
                    {
                        Duration = b.Duration,
                        Price = b.Price,
                        ProFormId = b.Id,
                        RFPSupplierId = b.RFPSupplierId,
                        Attachments = b.RFPProFormaAttachments.Where(c => !c.IsDeleted).Select(c => new RFPProFormaAttachmentDto
                        {
                            RFPAttachmentId = c.Id,
                            FileName = c.FileName,
                            FileSize = c.FileSize,
                            FileSrc = c.FileSrc,
                            FileType = c.FileType
                        }).ToList()

                    }).ToList(),

                }).FirstOrDefaultAsync();
                return ServiceResultFactory.CreateSuccess(result.ProFromas.FirstOrDefault());
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<RFPProFormDetailDto>(null, exception);
            }
        }

        private void UpdateRFPstatosOnAddProposal(long rfpId, RFPInqueryType inqueryType, RFP rfpModel)
        {
            rfpModel.Status = inqueryType == RFPInqueryType.TechnicalInquery ? RFPStatus.TechnicalProposal : RFPStatus.CommercialProposal;
            var logStatus = rfpModel.Status == RFPStatus.CommercialProposal ? RFPLogStatus.CommercialProposal : RFPLogStatus.TechnicalProposal;

            // add rfp status log
            if (rfpModel.RFPStatusLogs == null || !rfpModel.RFPStatusLogs.Any(a => a.Status == logStatus))
            {
                var rfpLog = new RFPStatusLog
                {
                    DateIssued = DateTime.UtcNow,
                    RFPId = rfpId,
                    Status = rfpModel.Status == RFPStatus.CommercialProposal ? RFPLogStatus.CommercialProposal : RFPLogStatus.TechnicalProposal
                };
                _rfpStatusLogRepository.Add(rfpLog);
            }
        }

        private List<NotifToDto> SendNotifOnAddproposal(RFPInqueryType inqueryType, List<RFPSupplierProposal> supplierProposals)
        {
            {
                var result = new List<NotifToDto>();

                if (supplierProposals.Any(c => c.IsAnswered == false))
                    return null;
                else if (inqueryType == RFPInqueryType.TechnicalInquery)
                {
                    result.Add(new NotifToDto
                    {
                        NotifEvent = NotifEvent.SetTechEvaluation,
                        Roles = new List<string>
                        {
                            SCMRole.RFPTechEvaluationMng

                        }
                    });
                }
                else if (inqueryType == RFPInqueryType.CommercialInquery)
                {
                    result.Add(new NotifToDto
                    {
                        NotifEvent = NotifEvent.SetCommercialEvaluation,
                        Roles = new List<string>
                        {
                            SCMRole.RFPCommercialEvaluationMng,
                        }
                    });
                }

                return result.Count() > 0 ? result : null;
            }
        }

        public async Task<ServiceResult<AddOrEditSupplierProposalDto>> AddSupplierEvaluationProposalAsync(AuthenticateDto authenticate, long rfpId,
            int supplierId, RFPInqueryType inqueryType,
            AddRFPSupplierEvaluationDto model)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<AddOrEditSupplierProposalDto>(null, MessageId.AccessDenied);

                var dbQuery = _rfpRepository
                    .Where(a => !a.IsDeleted
                    && a.Id == rfpId
                    && a.ContractCode == authenticate.ContractCode)
                    .AsQueryable();

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(c => permission.ProductGroupIds.Contains(c.ProductGroupId)))
                    return ServiceResultFactory.CreateError<AddOrEditSupplierProposalDto>(null, MessageId.AccessDenied);

                var rfpModel = await dbQuery
                    .Include(a => a.RFPStatusLogs)
                    .Include(a => a.RFPItems)
                    .ThenInclude(a => a.PurchaseRequestItem)
                    .ThenInclude(a => a.PurchaseRequest)
                    .FirstOrDefaultAsync();

                if (rfpModel == null)
                    return ServiceResultFactory.CreateError<AddOrEditSupplierProposalDto>(null, MessageId.EntityDoesNotExist);

                if (rfpModel.Status == RFPStatus.RFPSelection || rfpModel.Status == RFPStatus.Canceled)
                    return ServiceResultFactory.CreateError<AddOrEditSupplierProposalDto>(null, MessageId.ImpossibleEdit);
                var beforeRFPStatus = rfpModel.Status;

                if (model.ProposalEvaluationScore == null || !model.ProposalEvaluationScore.Any())
                    return ServiceResultFactory.CreateError<AddOrEditSupplierProposalDto>(null, MessageId.InputDataValidationError);

                var postedIds = model.ProposalEvaluationScore.Select(a => a.Id).ToList();

                var supplierProposalModels = await _rfpSupplierProposalRepository
                    .Where(a => !a.IsDeleted
                    && a.RFPSupplier.RFPId == rfpId
                    && a.RFPSupplier.IsActive
                    && !a.RFPInquery.IsDeleted)
                    .Include(a => a.RFPSupplier)
                    .Include(a => a.RFPInquery)
                    .ToListAsync();

                if (supplierProposalModels == null)
                    return ServiceResultFactory.CreateError<AddOrEditSupplierProposalDto>(null, MessageId.ImpossibleEdit);

                var currentSupplierProposalModels = supplierProposalModels
                    .Where(a => a.RFPSupplier.SupplierId == supplierId)
                    .ToList();
                if (currentSupplierProposalModels.Any(a => !a.IsDeleted && !a.IsAnswered && a.RFPInquery.RFPInqueryType == inqueryType))
                    return ServiceResultFactory.CreateError<AddOrEditSupplierProposalDto>(null, MessageId.EvaluateProposalNotpossibleBeforRegPoropsal);
                var answeredProposal = currentSupplierProposalModels
                .Where(a => a.IsAnswered && a.RFPInquery.RFPInqueryType == inqueryType)
                .ToList();

                if (answeredProposal == null)
                    return ServiceResultFactory.CreateError<AddOrEditSupplierProposalDto>(null, MessageId.ImpossibleEdit);


                var supplierModel = await _supplierRepository.FirstOrDefaultAsync(a => a.Id == supplierId);

                if (supplierModel == null)
                    return ServiceResultFactory.CreateError<AddOrEditSupplierProposalDto>(null, MessageId.SupplierNotFound);

                foreach (var item in answeredProposal)
                {
                    var postedItem = model.ProposalEvaluationScore.FirstOrDefault(a => a.Id == item.Id);
                    if (postedItem != null)
                    {
                        item.EvaluationScore = postedItem.EvaluationScore;
                        item.IsEvaluated = true;
                    }
                }


                var currentInquerySupplierProposalModels = currentSupplierProposalModels.Where(a => a.RFPInquery.RFPInqueryType == inqueryType).ToList();

                var rfpSupplier = currentSupplierProposalModels.FirstOrDefault().RFPSupplier;

                if (inqueryType == RFPInqueryType.TechnicalInquery)
                    rfpSupplier.TBENote = model.EvaluationNote;
                else
                    rfpSupplier.CBENote = model.EvaluationNote;

                // محاسبه ارزیابی این تامین کننده
                decimal evaluationScore = 0;

                var sumWeight = currentInquerySupplierProposalModels.Sum(a => a.RFPInquery.Weight);
                var supplierScore = currentInquerySupplierProposalModels.Sum(a => (a.RFPInquery.Weight * a.EvaluationScore));
                if (sumWeight != 0 && supplierScore != 0)
                    evaluationScore = supplierScore / sumWeight;

                if (inqueryType == RFPInqueryType.TechnicalInquery)
                    rfpSupplier.TBEScore = evaluationScore;
                else
                    rfpSupplier.CBEScore = evaluationScore;

                var ress = UpdateRFPStatusOnEvaluationAsync(rfpId, rfpModel, supplierProposalModels);

                if (ress == false)
                    return ServiceResultFactory.CreateError<AddOrEditSupplierProposalDto>(null, MessageId.DataInconsistency);

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    await SetEvaluationTaskDoneOnRFPEvaluationAsync(authenticate.UserId, rfpModel, inqueryType, supplierId, currentInquerySupplierProposalModels);

                    var res = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = rfpModel.ContractCode,
                        RootKeyValue = rfpModel.Id.ToString(),
                        FormCode = rfpModel.RFPNumber,
                        Temp = supplierModel.Name,
                        Description = ((int)inqueryType).ToString(),
                        Quantity = supplierId.ToString(),
                        KeyValue = rfpModel.Id.ToString(),
                        ProductGroupId = rfpModel.ProductGroupId,
                        NotifEvent = inqueryType == RFPInqueryType.TechnicalInquery ? NotifEvent.SetTechEvaluation : NotifEvent.SetCommercialEvaluation,
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName
                    },
                    rfpModel.ProductGroupId
                    , SendNotifOnProposalEvaluation(beforeRFPStatus, rfpModel));
                }
                AddOrEditSupplierProposalDto result = new AddOrEditSupplierProposalDto();
                var proposal = await GetRFPSupplierInqueryAsync(authenticate, rfpId, supplierId, inqueryType);
                var supplier = await GetRFPDetailsByIdAsync(authenticate, rfpId);
                result.SupplierPropsoal = proposal.Result;
                result.Suppliers = supplier.Result.Suppliers;
                result.RFPStatus = supplier.Result.Status;
                return ServiceResultFactory.CreateSuccess(result);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<AddOrEditSupplierProposalDto>(null, exception);
            }
        }

        private bool UpdateRFPStatusOnEvaluationAsync(long rfpId, RFP rfpModel, List<RFPSupplierProposal> supplierProposalModels)
        {
            if (!supplierProposalModels.Any(a =>
            !a.IsDeleted &&
             a.RFPSupplier.IsActive
             && !a.IsEvaluated))
            {
                rfpModel.Status = RFPStatus.RFPEvaluation;

                foreach (var item in rfpModel.RFPItems)
                {
                    var mrpItem = _mrpItemRepository
                          .Where(a => a.ProductId == item.ProductId &&
                          a.MrpId == item.PurchaseRequestItem.PurchaseRequest.MrpId && !a.IsDeleted)
                          .FirstOrDefault();

                    if (mrpItem == null)
                        return false;

                    if (mrpItem.MrpItemStatus < MrpItemStatus.RFPEvaluation)
                        mrpItem.MrpItemStatus = MrpItemStatus.RFPEvaluation;

                }


                if (rfpModel.RFPStatusLogs == null || !rfpModel.RFPStatusLogs.Any(a => a.Status == RFPLogStatus.RFPEvaluation))
                {
                    var rfpLog = new RFPStatusLog
                    {
                        DateIssued = DateTime.UtcNow,
                        RFPId = rfpId,
                        Status = RFPLogStatus.RFPEvaluation
                    };
                    _rfpStatusLogRepository.Add(rfpLog);
                }
            }

            if (!supplierProposalModels.Any(a =>
            !a.IsDeleted &&
             a.RFPSupplier.IsActive &&
            a.RFPInquery.RFPInqueryType == RFPInqueryType.TechnicalInquery &&
            !a.IsEvaluated))
            {
                if (rfpModel.RFPStatusLogs == null || !rfpModel.RFPStatusLogs.Any(a => a.Status == RFPLogStatus.TechEvaluation))
                {
                    var rfpLog = new RFPStatusLog
                    {
                        DateIssued = DateTime.UtcNow,
                        RFPId = rfpId,
                        Status = RFPLogStatus.TechEvaluation
                    };
                    _rfpStatusLogRepository.Add(rfpLog);
                }
            }

            if (!supplierProposalModels.Any(a =>
            !a.IsDeleted &&
             a.RFPSupplier.IsActive &&
             a.RFPInquery.RFPInqueryType == RFPInqueryType.CommercialInquery &&
             !a.IsEvaluated))
            {
                if (rfpModel.RFPStatusLogs == null || !rfpModel.RFPStatusLogs.Any(a => a.Status == RFPLogStatus.CommercialEvaluation))
                {
                    var rfpLog = new RFPStatusLog
                    {
                        DateIssued = DateTime.UtcNow,
                        RFPId = rfpId,
                        Status = RFPLogStatus.CommercialEvaluation
                    };
                    _rfpStatusLogRepository.Add(rfpLog);
                }
            }
            return true;
        }

        private async Task SetEvaluationTaskDoneOnRFPEvaluationAsync(int userId, RFP rfpModel, RFPInqueryType inqueryType, int supplierId, List<RFPSupplierProposal> supplierProposals)
        {
            if (!supplierProposals.Any(a => a.IsEvaluated == false))
            {
                var notifEvent = inqueryType == RFPInqueryType.TechnicalInquery ? NotifEvent.SetTechEvaluation : NotifEvent.SetCommercialEvaluation;
                await _scmLogAndNotificationService.SetDonedNotificationAsync(userId, rfpModel.ContractCode, rfpModel.Id.ToString(), supplierId.ToString(), notifEvent);
            }
        }

        private List<NotifToDto> SendNotifOnProposalEvaluation(RFPStatus beforeStatus, RFP rfpModel)
        {

            if (beforeStatus == RFPStatus.RFPEvaluation || rfpModel.Status != RFPStatus.RFPEvaluation)
                return null;

            var result = new List<NotifToDto>();
            result.Add(new NotifToDto
            {
                NotifEvent = NotifEvent.SetRFPWinner,
                Roles = new List<string>
                        {
                            SCMRole.RFPWinnerMng
                        }
            });

            return result;

        }

        private async Task<bool> UpdateRFPStatusToEvaluationAsync(long rfpId)
        {
            try
            {
                var rfpModel = await _rfpRepository.FindAsync(rfpId);

                if (rfpModel.Status == RFPStatus.Canceled)
                    return false;

                if (!await _rfpSupplierProposalRepository.AnyAsync(a => !a.IsDeleted
                && !a.RFPInquery.IsDeleted
                && a.RFPSupplier.IsActive
                && a.RFPInquery.RFPId == rfpId
                && !a.IsEvaluated))
                {
                    rfpModel.Status = RFPStatus.RFPEvaluation;
                    if (await _unitOfWork.SaveChangesAsync() > 0)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception exception)
            {
                return false;
            }
        }

        private async Task<ServiceResult<bool>> AddSupplierProposalAttachmentAsync(RFPSupplierProposal rfpSupplierProposal, List<RFPInqueryAttachmentDto> postedAttachments)
        {
            try
            {
                var attchment = rfpSupplierProposal.RFPSupplierInqueryAttachments.Where(a => !a.IsDeleted).ToList();
                var beforeAttachment = attchment == null ? null : attchment;

                if (!attchment.Any() && postedAttachments.Any())
                {
                    var postedAttach = postedAttachments.Select(a => new AddAttachmentDto { FileSrc = a.FileSrc, FileName = a.FileName }).ToList();
                    var res = await AddRFPSupplierProposalAttachmentAsync(rfpSupplierProposal.Id, postedAttach);

                    if (!res.Succeeded)
                        return ServiceResultFactory.CreateError(false, res.Messages.First().Message);
                }
                else if (attchment.Any() && !postedAttachments.Any())
                {
                    foreach (var item in attchment)
                    {
                        item.IsDeleted = true;
                    }
                }
                else if (attchment.Any() && postedAttachments.Any())
                {
                    var postedAttach = postedAttachments.Select(a => a.FileSrc).ToList();
                    var removeItem = attchment.Where(a => !postedAttach.Contains(a.FileSrc)).ToList();
                    foreach (var item in removeItem)
                    {
                        item.IsDeleted = true;
                    }

                    var beforeAttchFileNames = attchment.Select(a => a.FileSrc).ToList();
                    var newAttach = postedAttachments
                        .Where(file => !beforeAttchFileNames.Contains(file.FileSrc)).Select(c => new AddAttachmentDto { FileSrc = c.FileSrc, FileName = c.FileName })
                        .ToList();

                    var res = await AddRFPSupplierProposalAttachmentAsync(rfpSupplierProposal.Id, newAttach);

                    if (!res.Succeeded)
                        return ServiceResultFactory.CreateError(false, res.Messages.First().Message);
                }
                return ServiceResultFactory.CreateSuccess(true);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        private async Task<ServiceResult<bool>> UpdateRFPInquery(List<RFPInquery> rfpInqueries, List<RFPInqueryInfoDto> postedModel)
        {
            try
            {
                foreach (var item in postedModel)
                {
                    var updateModel = rfpInqueries.FirstOrDefault(a => a.Id == item.Id);
                    updateModel.Weight = item.Weight;
                    updateModel.Description = item.Description;


                }

                return ServiceResultFactory.CreateSuccess(true);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }



        private async Task<ServiceResult<bool>> AddRFPSupplierProposalAttachmentAsync(long supplierProposalId, List<AddAttachmentDto> attachment)
        {

            foreach (var item in attachment)
            {
                var UploadedFile =
                    await _fileHelper.SaveDocumentFromTemp(item.FileSrc, ServiceSetting.UploadFilePath.RFPSupplier);
                if (UploadedFile == null)
                    return ServiceResultFactory.CreateError(false, MessageId.UploudFailed);

                _fileHelper.DeleteDocumentFromTemp(item.FileSrc);
                var rfpInqueryAttach = new RFPAttachment
                {
                    RFPSupplierProposalId = supplierProposalId,
                    FileType = UploadedFile.FileType,
                    FileSize = UploadedFile.FileSize,
                    FileName = item.FileName,
                    FileSrc = item.FileSrc,
                };
                _rfpAttachmentRepository.Add(rfpInqueryAttach);
            }
            return ServiceResultFactory.CreateSuccess(true);
        }

        #endregion
        private async Task<ServiceResult<bool>> AddRFPInqueryAttachmentAsync(long rfpInqueryId, List<AddAttachmentDto> attachment)
        {

            foreach (var item in attachment)
            {
                var UploadedFile =
                    await _fileHelper.SaveDocumentFromTemp(item.FileSrc, ServiceSetting.UploadFilePath.RFP);
                if (UploadedFile == null)
                    return ServiceResultFactory.CreateError(false, MessageId.UploudFailed);

                _fileHelper.DeleteDocumentFromTemp(item.FileSrc);
                var rfpInqueryAttach = new RFPAttachment
                {
                    RFPInqueryId = rfpInqueryId,
                    FileType = UploadedFile.FileType,
                    FileSize = UploadedFile.FileSize,
                    FileName = item.FileName,
                    FileSrc = item.FileSrc
                };
                _rfpAttachmentRepository.Add(rfpInqueryAttach);
            }
            return ServiceResultFactory.CreateSuccess(true);
        }

        private void RemoveRFPInquery(List<RFPInquery> rfpInqueries)
        {
            foreach (var inquery in rfpInqueries)
            {
                foreach (var attach in inquery.RFPInqueryAttachments)
                {
                    if (!attach.IsDeleted)
                        attach.IsDeleted = true;
                }

                foreach (var SupplierProposal in inquery.RFPSupplierProposal)
                {
                    SupplierProposal.IsDeleted = true;
                    foreach (var item in SupplierProposal.RFPSupplierInqueryAttachments)
                    {
                        item.IsDeleted = true;
                    }
                }

                inquery.IsDeleted = true;
            }

        }

        private static IQueryable<RFP> ApplayFilterQuery(RFPQueryDto query, IQueryable<RFP> dbQuery)
        {

            if (query.Statuses != null && query.Statuses.Any())
                dbQuery = dbQuery.Where(a => query.Statuses.Contains(a.Status));

            if (query.ContractCodes != null && query.ContractCodes.Count() > 0)
                dbQuery = dbQuery.Where(a => query.ContractCodes.Contains(a.ContractCode));

            if (!string.IsNullOrEmpty(query.SearchText))
                dbQuery = dbQuery.Where(a => a.RFPNumber.Contains(query.SearchText) ||
                a.RFPItems.Any(c => !c.IsDeleted && c.IsActive &&
                (c.PurchaseRequestItem.PurchaseRequest.PRCode.Contains(query.SearchText) ||
                c.Product.Description.Contains(query.SearchText) ||
                c.Product.ProductCode.Contains(query.SearchText))) ||
                a.RFPSuppliers.Any(v => !v.IsDeleted && v.IsActive && (v.Supplier.Name.Contains(query.SearchText))));

            if (query.Suppliers != null && query.Suppliers.Count() > 0)
                dbQuery = dbQuery.Where(a => a.RFPSuppliers.Any(c => !c.IsDeleted && c.IsActive && query.Suppliers.Contains(c.SupplierId)));

            if (query.ProductGroups != null && query.ProductGroups.Count() > 0)
                dbQuery = dbQuery.Where(a => a.RFPItems.Any(c => !c.IsDeleted && c.IsActive && query.ProductGroups.Contains(c.Product.ProductGroupId)));

            if (query.Products != null && query.Products.Count() > 0)
                dbQuery = dbQuery.Where(a => a.RFPItems.Any(c => !c.IsDeleted && c.IsActive && query.Products.Contains(c.ProductId)));
            if (!string.IsNullOrEmpty(query.RfpNumber))
                dbQuery = dbQuery.Where(a => a.RFPNumber == query.RfpNumber);
            if (query.FromDateTime != null || query.ToDateTime != null)
            {
                switch (query.PRDateQuery)
                {
                    case PRDateQuery.DateCreate:
                        if (query.FromDateTime != null)
                        {
                            var date = query.FromDateTime.UnixTimestampToDateTime();
                            dbQuery = dbQuery.Where(a => a.CreatedDate != null && a.CreatedDate.Value.Date >= date.Value.Date);
                        }
                        if (query.ToDateTime != null)
                        {
                            var date = query.ToDateTime.UnixTimestampToDateTime();
                            dbQuery = dbQuery.Where(a => a.CreatedDate != null && a.CreatedDate.Value.Date <= date.Value.Date);
                        }
                        break;
                    case PRDateQuery.DateStart:
                        if (query.FromDateTime != null)
                        {
                            var date = query.FromDateTime.UnixTimestampToDateTime();
                            dbQuery = dbQuery.Where(a => a.RFPItems.Any(c => !c.IsDeleted && c.IsActive && c.DateStart.Date >= date.Value.Date));
                        }
                        if (query.ToDateTime != null)
                        {
                            var date = query.ToDateTime.UnixTimestampToDateTime();
                            dbQuery = dbQuery.Where(a => a.RFPItems.Any(c => !c.IsDeleted && c.IsActive && c.DateStart.Date <= date.Value.Date));
                        }
                        break;
                    case PRDateQuery.DateEnd:
                        if (query.FromDateTime != null)
                        {
                            var date = query.FromDateTime.UnixTimestampToDateTime();
                            dbQuery = dbQuery.Where(a => a.RFPItems.Any(c => !c.IsDeleted && c.IsActive && c.DateEnd.Date >= date.Value.Date));
                        }
                        if (query.ToDateTime != null)
                        {
                            var date = query.ToDateTime.UnixTimestampToDateTime();
                            dbQuery = dbQuery.Where(a => a.RFPItems.Any(c => !c.IsDeleted && c.IsActive && c.DateEnd.Date <= date.Value.Date));
                        }
                        break;
                    default:
                        break;
                }
            }

            return dbQuery;
        }

        private RFP AddRFPItem(RFP rfpModel, List<PurchaseRequestItem> purchaseRequestItems, List<AddRFPItemDto> postedRFPItems)
        {
            rfpModel.RFPItems = new List<RFPItems>();
            foreach (var item in postedRFPItems)
            {
                var currPRItem = purchaseRequestItems.Where(a => !a.IsDeleted && a.PurchaseRequestId == item.PurchaseRequestId && a.ProductId == item.ProductId).ToList();
                foreach (var prItem in currPRItem)
                {


                    if (prItem.MrpItem == null)
                        return null;

                    if (prItem.MrpItem.MrpItemStatus < MrpItemStatus.RFPRegister)
                        prItem.MrpItem.MrpItemStatus = MrpItemStatus.RFPRegister;

                    rfpModel.RFPItems.Add(new RFPItems
                    {
                        ProductId = item.ProductId,
                        IsActive = true,
                        PurchaseRequestItemId = prItem.Id,
                        DateStart = prItem.DateStart,
                        DateEnd = prItem.DateEnd,
                        Quantity = prItem.Quntity,
                        RemainedStock = prItem.Quntity
                    });

                    prItem.PRItemStatus = PRItemStatus.InProgress;
                }

            }
            return rfpModel;
        }

        private void AddRFPItem(long rfpId, List<PurchaseRequestItem> purchaseRequestItems, List<AddRFPItemDto> postedRFPItems)
        {
            List<RFPItems> newRfpItems = new List<RFPItems>();
            foreach (var item in postedRFPItems)
            {
                var currPRItem = purchaseRequestItems.Where(a => !a.IsDeleted && a.PurchaseRequestId == item.PurchaseRequestId && a.ProductId == item.ProductId && a.PRItemStatus == PRItemStatus.NotStart);
                foreach (var prItem in currPRItem)
                {
                    newRfpItems.Add(new RFPItems
                    {
                        RFPId = rfpId,
                        ProductId = item.ProductId,
                        IsActive = true,
                        PurchaseRequestItemId = prItem.Id,
                        DateStart = prItem.DateStart,
                        DateEnd = prItem.DateEnd,
                        Quantity = prItem.Quntity,
                        RemainedStock = prItem.Quntity
                    });
                    prItem.PRItemStatus = PRItemStatus.InProgress;
                }

                _rfpItemRepository.AddRange(newRfpItems);
            }
        }

        private async Task<ServiceResult<RFP>> AddRFPAttachmentAsync(RFP rfpModel, List<AddAttachmentDto> attachment)
        {
            rfpModel.RFPAttachments = new List<RFPAttachment>();

            foreach (var item in attachment)
            {
                var UploadedFile =
                    await _fileHelper.SaveDocumentFromTemp(item.FileSrc, ServiceSetting.UploadFilePath.RFP);
                if (UploadedFile == null)
                    return ServiceResultFactory.CreateError<RFP>(null, MessageId.UploudFailed);

                _fileHelper.DeleteDocumentFromTemp(item.FileSrc);
                rfpModel.RFPAttachments.Add(new RFPAttachment
                {
                    FileType = UploadedFile.FileType,
                    FileSize = UploadedFile.FileSize,
                    FileName = item.FileName,
                    FileSrc = item.FileSrc,
                });
            }
            return ServiceResultFactory.CreateSuccess(rfpModel);
        }

        private async Task<ServiceResult<List<RFPInquery>>> AddTechInqueryAsync(RFP rfpModel, List<AddRFPInqueryDto> techInqueries)
        {
            List<RFPInquery> teches = new List<RFPInquery>();

            foreach (var item in techInqueries)
            {
                var newInquery = new RFPInquery
                {
                    Description = item.Description,
                    DefaultInquery = DefaultInquery.None,
                    RFPInqueryType = RFPInqueryType.TechnicalInquery,
                    Weight = item.Weight,
                    RFPInqueryAttachments = new List<RFPAttachment>()
                };



                teches.Add(newInquery);
            }
            return ServiceResultFactory.CreateSuccess(teches);
        }
        private async Task<ServiceResult<List<RFPInquery>>> AddCommercialInqueryAsync(RFP rfpModel, List<AddRFPInqueryDto> commercialInqueries)
        {
            List<RFPInquery> commercials = new List<RFPInquery>();

            foreach (var item in commercialInqueries)
            {
                var newInquery = new RFPInquery
                {
                    Description = item.Description,
                    DefaultInquery = item.Id == "delivery" ? DefaultInquery.DeliveryDate : item.Id == "price" ? DefaultInquery.Price : DefaultInquery.None,
                    RFPInqueryType = RFPInqueryType.CommercialInquery,
                    Weight = item.Weight,
                    RFPInqueryAttachments = new List<RFPAttachment>()
                };



                commercials.Add(newInquery);
            }
            return ServiceResultFactory.CreateSuccess(commercials);
        }
        private async Task<ServiceResult<bool>> AddRFPInqueryAsync(long rfpId, List<RFPInqueryInfoDto> rfpInqueries, List<RFPSupplier> rfpSuppliers)
        {
            foreach (var item in rfpInqueries)
            {
                var newInquery = new RFPInquery
                {
                    RFPId = rfpId,
                    Description = item.Description,
                    DefaultInquery = item.RFPInqueryType == RFPInqueryType.TechnicalInquery ? DefaultInquery.None : item.DefaultInquery,
                    RFPInqueryType = item.RFPInqueryType,
                    Weight = item.Weight,
                    RFPInqueryAttachments = new List<RFPAttachment>()
                };



                foreach (var supplier in rfpSuppliers)
                {
                    var supplierProposal = new RFPSupplierProposal
                    {
                        RFPInquery = newInquery,
                        RFPSupplierId = supplier.Id,
                    };

                    _rfpSupplierProposalRepository.Add(supplierProposal);
                }

                _rfpInqueryRepository.Add(newInquery);
            }
            return ServiceResultFactory.CreateSuccess(true);
        }



        public async Task<ServiceResult<List<ListSupplierDto>>> GetWinnerSupplierRFPByRFPIdAsync(AuthenticateDto authenticate,
          long rfpId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<ListSupplierDto>>(null, MessageId.AccessDenied);

                var dbQuery = _rfpSupplierRepository
                    .AsNoTracking()
                    .Where(a => a.IsWinner && !a.IsDeleted && a.IsActive &&
                    a.RFPId == rfpId && !a.RFP.IsDeleted &&
                    a.RFP.Status == RFPStatus.RFPSelection &&
                    a.RFP.ContractCode == authenticate.ContractCode)
                    .AsQueryable();

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(c => permission.ProductGroupIds.Contains(c.RFP.ProductGroupId)))
                    return ServiceResultFactory.CreateError<List<ListSupplierDto>>(null, MessageId.AccessDenied);

                var result = await dbQuery.Select(s => new ListSupplierDto
                {
                    Id = s.SupplierId,
                    SupplierCode = s.Supplier.SupplierCode,
                    Email = s.Supplier.Email,
                    Name = s.Supplier.Name,
                    Address = s.Supplier.Address,
                    EconomicCode = s.Supplier.EconomicCode,
                    PostalCode = s.Supplier.PostalCode,
                    NationalId = s.Supplier.NationalId,
                    Logo = _appSettings.ClientHost + ServiceSetting.UploadImagesPath.LogoSmall + s.Supplier.Logo,
                    TellPhone = s.Supplier.TellPhone,
                    ProductGroups = null,
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception e)
            {
                return ServiceResultFactory.CreateException(new List<ListSupplierDto>(), e);
            }
        }

        public async Task<DownloadFileDto> DownloadRFPAttachmentAsync(AuthenticateDto authenticate, long rfpId, string fileSrc)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return null;

                var entity = await _rfpRepository
                   .Where(a => !a.IsDeleted && a.Id == rfpId && a.RFPAttachments.Any(c => !c.IsDeleted && c.FileSrc == fileSrc))
                   .Select(c => new
                   {
                       ContractCode = c.ContractCode,
                       ProductGroupId = c.ProductGroupId,
                   }).FirstOrDefaultAsync();

                if (entity == null)
                    return null;

                if (permission.ProductGroupIds.Any() && !permission.ProductGroupIds.Contains(entity.ProductGroupId))
                    return null;
                var attachment = await _rfpAttachmentRepository.FirstOrDefaultAsync(a => !a.IsDeleted && a.FileSrc == fileSrc);
                if (attachment == null)
                    return null;
                var streamResult = await _fileHelper.DownloadAttachmentDocument(fileSrc, ServiceSetting.FileSection.RFP, attachment.FileName);
                if (streamResult == null)
                    return null;

                return streamResult;
            }
            catch (Exception exception)
            {
                return null;
            }
        }

        public async Task<DownloadFileDto> DownloadRFPSupplierProposalAttachmentAsync(AuthenticateDto authenticate, long proposalId, string fileSrc)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return null;

                var entity = await _rfpAttachmentRepository
                    .Include(a => a.RFP)
                   .Where(a => !a.IsDeleted &&
                   a.RFPSupplierProposalId == proposalId &&
                    a.FileSrc == fileSrc
                    ).FirstOrDefaultAsync();

                if (entity == null)
                    return null;

                if (permission.ProductGroupIds.Any() && !permission.ProductGroupIds.Contains(entity.RFP.ProductGroupId))
                    return null;

                var streamResult = await _fileHelper.DownloadAttachmentDocument(fileSrc, ServiceSetting.FileSection.RFPSupplier, entity.FileName);
                if (streamResult == null)
                    return null;

                return streamResult;
            }
            catch (Exception exception)
            {
                return null;
            }
        }
        public async Task<ServiceResult<bool>> CancelRFPAsync(AuthenticateDto authenticate, long rfpId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery = _rfpRepository
                    .Include(a => a.RFPStatusLogs)
                    .Include(a => a.RFPItems)
                    .ThenInclude(a => a.PurchaseRequestItem)
                    .ThenInclude(a => a.MrpItem)
                    .Where(x => !x.IsDeleted &&
                    x.ContractCode == authenticate.ContractCode &&
                    x.Id == rfpId);
                if (await dbQuery.CountAsync() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);
                var productGroupId = await dbQuery.Select(a => a.ProductGroupId).FirstAsync();
                if (permission.ProductGroupIds.Any() && !permission.ProductGroupIds.Contains(productGroupId))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                if (await dbQuery.AnyAsync(a => a.RFPItems.Any(b => !b.IsDeleted && b.RemainedStock != b.Quantity)))
                    return ServiceResultFactory.CreateError(false, MessageId.RFPItemsUsedInPRContract);
                var rfp = await dbQuery.FirstAsync();

                var cancelRFPItems = await CancelRFPItemsAsync(rfp.RFPItems.ToList());
                if (!cancelRFPItems.Succeeded)
                    return ServiceResultFactory.CreateError(false, MessageId.EditEntityFailed);
                rfp.RFPItems = cancelRFPItems.Result;
                rfp.Status = RFPStatus.Canceled;

                rfp.RFPStatusLogs.Add(new RFPStatusLog { DateIssued = DateTime.Now, Status = RFPLogStatus.Canceled });
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    return ServiceResultFactory.CreateSuccess(true);
                }
                return ServiceResultFactory.CreateError(false, MessageId.SaveFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<DownloadFileDto> DownloadRFPInqueryAttachmentAsync(AuthenticateDto authenticate, long rfpId, long inqueryId, RFPInqueryType inqueryType, string fileSrc)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return null;
                var entity = await _rfpInqueryRepository
                    .Where(a => !a.IsDeleted &&
                    a.RFPId == rfpId &&
                    a.Id == inqueryId &&
                    a.RFPInqueryType == inqueryType &&
                    a.RFPInqueryAttachments.Any(c => !c.IsDeleted && c.FileSrc == fileSrc))
                    .Select(c => new
                    {
                        ContractCode = c.RFP.ContractCode,
                        ProductGroupId = c.RFP.ProductGroupId,

                    }).FirstOrDefaultAsync();

                if (entity == null)
                    return null;

                if (permission.ProductGroupIds.Any() && !permission.ProductGroupIds.Contains(entity.ProductGroupId))
                    return null;
                var streamResult = await _fileHelper.DownloadDocument(fileSrc, ServiceSetting.FileSection.RFP);
                if (streamResult == null)
                    return null;

                return streamResult;
            }
            catch (Exception exception)
            {
                return null;
            }
        }

        public async Task<DownloadFileDto> DownloadRFPProFormaAttachmentAsync(AuthenticateDto authenticate, long rfpSupplierId, long proformaId, string fileSrc)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return null;
                var entity = await _rfpAttachmentRepository
                    .Where(a => !a.IsDeleted &&
                    a.ProFormaId == proformaId &&
                    a.FileSrc == fileSrc).FirstOrDefaultAsync();

                if (entity == null)
                    return null;
                var productGroupId = await _rfpSupplierRepository.Where(a => a.Id == rfpSupplierId).Select(a => a.RFP.ProductGroupId).FirstAsync();
                if (permission.ProductGroupIds.Any() && !permission.ProductGroupIds.Contains(productGroupId))
                    return null;
                var streamResult = await _fileHelper.DownloadAttachmentDocument(fileSrc, ServiceSetting.FileSection.RFPSupplier, entity.FileName);
                if (streamResult == null)
                    return null;

                return streamResult;
            }
            catch (Exception exception)
            {
                return null;
            }
        }
        private async Task<ServiceResult<List<RFPItems>>> CancelRFPItemsAsync(List<RFPItems> rfpItems)
        {
            try
            {

                foreach (var item in rfpItems)
                {
                    var mrpItemId = item.PurchaseRequestItem.MrpItem.Id;
                    item.RemainedStock = item.Quantity;
                    item.IsActive = false;
                    item.PurchaseRequestItem.PRItemStatus = PRItemStatus.NotStart;
                    if (await _mrpItemRepository.AnyAsync(a => !a.IsDeleted && mrpItemId == a.Id && !a.PurchaseRequestItems.Any(b => !b.IsDeleted && b.RFPItems.Any(c => !c.IsDeleted && c.IsActive))))
                        item.PurchaseRequestItem.MrpItem.MrpItemStatus = MrpItemStatus.PR;
                }
                return ServiceResultFactory.CreateSuccess(rfpItems);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<RFPItems>>(null, exception);
            }
        }
        private async Task<ServiceResult<List<RFPAttachment>>> AddSupplierProFormaAttachmentAsync(List<RFPProFormaAttachmentDto> postedAttachments)
        {
            try
            {
                List<RFPAttachment> rfpInqueryAttach = new List<RFPAttachment>();
                foreach (var item in postedAttachments)
                {
                    var UploadedFile =
                        await _fileHelper.SaveDocumentFromTemp(item.FileSrc, ServiceSetting.UploadFilePath.RFPSupplier);
                    if (UploadedFile == null)
                        return ServiceResultFactory.CreateError<List<RFPAttachment>>(null, MessageId.UploudFailed);

                    _fileHelper.DeleteDocumentFromTemp(item.FileSrc);
                    rfpInqueryAttach.Add(new RFPAttachment
                    {
                        FileType = UploadedFile.FileType,
                        FileSize = UploadedFile.FileSize,
                        FileName = item.FileName,
                        FileSrc = item.FileSrc
                    });

                }
                return ServiceResultFactory.CreateSuccess(rfpInqueryAttach);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<RFPAttachment>>(null, exception);
            }
        }
        private async Task<ServiceResult<List<RFPAttachment>>> EditSupplierProFormaAttachmentAsync(List<RFPProFormaAttachmentDto> postedAttachments, RFPProForma rFPProForma)
        {
            try
            {
                if (postedAttachments != null && postedAttachments.Any())
                {
                    var attachmentsId = postedAttachments.Where(a => a.RFPAttachmentId != null).Select(a => a.RFPAttachmentId);
                    var attachments = rFPProForma.RFPProFormaAttachments.Where(a => !a.IsDeleted && attachmentsId.Contains(a.Id)).ToList();
                    foreach (var item in postedAttachments.Where(a => a.RFPAttachmentId == null))
                    {
                        var UploadedFile =
                            await _fileHelper.SaveDocumentFromTemp(item.FileSrc, ServiceSetting.UploadFilePath.RFPSupplier);
                        if (UploadedFile == null)
                            return ServiceResultFactory.CreateError<List<RFPAttachment>>(null, MessageId.UploudFailed);

                        _fileHelper.DeleteDocumentFromTemp(item.FileSrc);
                        attachments.Add(new RFPAttachment
                        {
                            FileType = UploadedFile.FileType,
                            FileSize = UploadedFile.FileSize,
                            FileName = item.FileName,
                            FileSrc = item.FileSrc
                        });

                    }
                    rFPProForma.RFPProFormaAttachments = attachments;
                }
                else
                {
                    rFPProForma.RFPProFormaAttachments = new List<RFPAttachment>();
                }
                return ServiceResultFactory.CreateSuccess(rFPProForma.RFPProFormaAttachments.ToList());

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<RFPAttachment>>(null, exception);
            }
        }
        private async Task<ServiceResult<RFP>> RFPItemEditAsync(RFP rfpModel, List<AddRFPItemDto> postedRFPItems)
        {
            try
            {

                var prItemProductIds = rfpModel.RFPItems.Select(a => a.ProductId).ToList();
                var postedPRItemProductIds = postedRFPItems.Select(a => a.ProductId).ToList();
                var prItemPurchaseRequestIds = rfpModel.RFPItems.Select(a => a.PurchaseRequestItem.PurchaseRequestId).ToList();
                var postedPurchaseRequestIds = postedRFPItems.Select(a => a.PurchaseRequestId).ToList();
                var purchaseRequests = await _purchaseRequestItemRepository
                    .Include(a => a.MrpItem)
                    .Where(a => !a.IsDeleted && (prItemPurchaseRequestIds.Contains(a.PurchaseRequestId) || postedPurchaseRequestIds.Contains(a.PurchaseRequestId)))
                    .ToListAsync();

                if (purchaseRequests == null)
                    return ServiceResultFactory.CreateError<RFP>(null, MessageId.DataInconsistency);

                // remove prItems
                var removePRItem = rfpModel.RFPItems.Where(a => !postedRFPItems.Any(b => b.PurchaseRequestId == a.PurchaseRequestItem.PurchaseRequestId && b.ProductId == a.ProductId)).ToList();
                var removeResult = await RemoveRFPItems(rfpModel, purchaseRequests, removePRItem);
                if (!removeResult.Succeeded)
                    return ServiceResultFactory.CreateError<RFP>(null, MessageId.DeleteEntityFailed);
                // add new prItem
                var addnewPRItems = postedRFPItems.Where(a => !rfpModel.RFPItems.Any(b => !b.IsDeleted && b.ProductId == a.ProductId && a.PurchaseRequestId == b.PurchaseRequestItem.PurchaseRequestId))
                     .ToList();
                var addResult = await AddRFPItems(rfpModel, purchaseRequests, addnewPRItems);
                if (!addResult.Succeeded)
                    return ServiceResultFactory.CreateError<RFP>(null, addResult.Messages.First().Message);


                return ServiceResultFactory.CreateSuccess(rfpModel);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<RFP>(null, exception);
            }


        }

        private async Task<ServiceResult<RFP>> AddRFPItems(RFP rfpModel, List<PurchaseRequestItem> purchaseRequestItems, List<AddRFPItemDto> postedRFPItems)
        {
            try
            {
                foreach (var item in postedRFPItems)
                {
                    var currPRItem = purchaseRequestItems.Where(a => !a.IsDeleted && a.PurchaseRequestId == item.PurchaseRequestId && a.ProductId == item.ProductId).ToList();
                    foreach (var prItem in currPRItem)
                    {

                        if (prItem.MrpItem == null)
                            return ServiceResultFactory.CreateError<RFP>(null, MessageId.EditEntityFailed);

                        if (prItem.MrpItem.MrpItemStatus < MrpItemStatus.RFPRegister)
                            prItem.MrpItem.MrpItemStatus = MrpItemStatus.RFPRegister;

                        rfpModel.RFPItems.Add(new RFPItems
                        {
                            ProductId = item.ProductId,
                            IsActive = true,
                            PurchaseRequestItemId = prItem.Id,
                            DateStart = prItem.DateStart,
                            DateEnd = prItem.DateEnd,
                            Quantity = prItem.Quntity,
                            RemainedStock = prItem.Quntity
                        });

                        prItem.PRItemStatus = PRItemStatus.InProgress;
                    }

                }
                return ServiceResultFactory.CreateSuccess(rfpModel);
            }
            catch (Exception ex)
            {
                return ServiceResultFactory.CreateException<RFP>(null, ex);
            }
        }

        private async Task<ServiceResult<RFP>> RemoveRFPItems(RFP rfpModel, List<PurchaseRequestItem> purchaseRequestItems, List<RFPItems> postedRFPItems)
        {
            try
            {
                foreach (var item in postedRFPItems)
                {



                    if (item.PurchaseRequestItem.MrpItem == null)
                        return ServiceResultFactory.CreateError<RFP>(null, MessageId.EditEntityFailed);

                    if (!await _mrpItemRepository.AnyAsync(a => !a.IsDeleted && a.Id == item.PurchaseRequestItem.MrpItemId && a.PurchaseRequestItems.Any(b => !b.IsDeleted && b.RFPItems.Any(c => !c.IsDeleted))))
                        item.PurchaseRequestItem.MrpItem.MrpItemStatus = MrpItemStatus.PR;
                    item.IsDeleted = true;
                    item.PurchaseRequestItem.PRItemStatus = PRItemStatus.NotStart;



                }
                return ServiceResultFactory.CreateSuccess(rfpModel);
            }
            catch (Exception ex)
            {
                return ServiceResultFactory.CreateException<RFP>(null, ex);
            }
        }
        private async Task<ServiceResult<RFP>> RemoveRFPSupplier(RFP rfpModel, List<RFPSupplier> postedSupplierIds)
        {
            try
            {
                foreach (var item in postedSupplierIds)
                {
                    item.IsDeleted = true;

                    if (rfpModel.RFPType == RFPType.Proposal)
                    {


                        if (rfpModel.RFPSuppliers.Any(a => !a.IsDeleted && a.Id != item.Id && a.RFPSupplierProposals.Any(b => !b.IsDeleted && b.RFPInquery.RFPInqueryType == RFPInqueryType.TechnicalInquery)) && !rfpModel.RFPSuppliers.Any(a => !a.IsDeleted && a.Id != item.Id && a.RFPSupplierProposals.Any(b => !b.IsDeleted && b.RFPInquery.RFPInqueryType == RFPInqueryType.TechnicalInquery && (!b.IsAnswered))) && rfpModel.Status < RFPStatus.TechnicalProposal)
                        {
                            rfpModel.Status = RFPStatus.TechnicalProposal;
                            rfpModel.RFPStatusLogs.Add(new RFPStatusLog { DateIssued = DateTime.Now, Status = RFPLogStatus.TechEvaluation });
                        }
                        if (rfpModel.RFPSuppliers.Any(a => !a.IsDeleted && a.Id != item.Id && a.RFPSupplierProposals.Any(b => !b.IsDeleted && b.RFPInquery.RFPInqueryType == RFPInqueryType.CommercialInquery)) && (!rfpModel.RFPSuppliers.Any(a => !a.IsDeleted && a.Id != item.Id && a.RFPSupplierProposals.Any(b => b.RFPInquery.RFPInqueryType == RFPInqueryType.CommercialInquery && (!b.IsAnswered))) && rfpModel.Status < RFPStatus.CommercialProposal))
                        {

                            rfpModel.Status = RFPStatus.CommercialProposal;
                            rfpModel.RFPStatusLogs.Add(new RFPStatusLog { DateIssued = DateTime.Now, Status = RFPLogStatus.CommercialProposal });

                        }
                        if (!rfpModel.RFPSuppliers.Any(a => !a.IsDeleted && a.Id != item.Id && a.RFPSupplierProposals.Any(b => (b.RFPInquery.RFPInqueryType == RFPInqueryType.CommercialInquery && (!b.IsEvaluated))) && a.RFPSupplierProposals.Any(b => ((b.RFPInquery.RFPInqueryType == RFPInqueryType.TechnicalInquery && (!b.IsEvaluated))))) && rfpModel.Status < RFPStatus.RFPEvaluation)
                        {
                            rfpModel.Status = RFPStatus.RFPEvaluation;
                            rfpModel.RFPStatusLogs.Add(new RFPStatusLog { DateIssued = DateTime.Now, Status = RFPLogStatus.RFPEvaluation });
                        }



                        foreach (var proposal in item.RFPSupplierProposals)
                        {
                            proposal.IsDeleted = true;
                        }
                    }
                    else if (rfpModel.RFPType == RFPType.Proforma)
                    {

                        foreach (var proforma in item.RFPProFormas)
                        {
                            proforma.IsDeleted = true;
                        }
                    }
                }
                return ServiceResultFactory.CreateSuccess(rfpModel);
            }
            catch (Exception ex)
            {
                return ServiceResultFactory.CreateException<RFP>(null, ex);
            }
        }
        private async Task<ServiceResult<RFP>> AddRFPSupplier(RFP rfpModel, List<int> postedSupplierIds)
        {
            try
            {
                if (postedSupplierIds != null && postedSupplierIds.Any())
                {
                    List<RFPSupplier> newSupplier = new List<RFPSupplier>();
                    RFPSupplier rfpSupplier = null;
                    foreach (var item in postedSupplierIds)
                    {
                        rfpSupplier = new RFPSupplier();
                        rfpSupplier.SupplierId = item;
                        rfpSupplier.IsActive = true;
                        rfpSupplier.RFPId = rfpModel.Id;
                        rfpSupplier.RFPSupplierProposals = new List<RFPSupplierProposal>();
                        foreach (var inquiry in rfpModel.RFPInqueries.Where(a => !a.IsDeleted))
                            rfpSupplier.RFPSupplierProposals.Add(new RFPSupplierProposal { RFPInqueryId = inquiry.Id });
                        newSupplier.Add(rfpSupplier);

                    }
                    await _rfpSupplierRepository.AddRangeAsync(newSupplier);
                    rfpModel.Status = RFPStatus.Register;
                }

                return ServiceResultFactory.CreateSuccess(rfpModel);
            }
            catch (Exception ex)
            {
                return ServiceResultFactory.CreateException<RFP>(null, ex);
            }
        }

        private static List<string> GetProductsForRFP(List<string> ProductTitle)
        {
            List<string> productTitles = new List<string>();
            foreach (var item in ProductTitle)
            {
                if (!productTitles.Contains(item))
                    productTitles.Add(item);
            }
            return productTitles;
        }
        private static decimal? GetEvaluateScore(bool isEvaluate, decimal score)
        {
            if (isEvaluate)
                return score;
            return null;
        }
        private void UpdateRFPStatusAfterEditInquery(RFP rfpModel, List<RFPInquery> inqueries)
        {
            List<SupplierProposalState> techProposalState = new List<SupplierProposalState>();
            List<SupplierProposalState> commercialProposalState = new List<SupplierProposalState>();
            foreach (var item in inqueries.Where(a => !a.IsDeleted))
            {
                techProposalState.Add(!item.RFPSupplierProposal.Any(a => !a.IsDeleted && a.RFPSupplier.IsActive && !a.RFPInquery.IsDeleted && a.RFPInquery.RFPInqueryType == RFPInqueryType.TechnicalInquery)
               ? SupplierProposalState.None
               : item.RFPSupplierProposal.Any(a => !a.RFPSupplier.IsDeleted && a.RFPSupplier.IsActive && !a.IsDeleted && !a.IsAnswered && !a.RFPInquery.IsDeleted && a.RFPInquery.RFPInqueryType == RFPInqueryType.TechnicalInquery)
               ? SupplierProposalState.Completing
               : item.RFPSupplierProposal.Any(a => !a.RFPSupplier.IsDeleted && a.RFPSupplier.IsActive && !a.IsDeleted && a.IsAnswered && !a.IsEvaluated && !a.RFPInquery.IsDeleted && a.RFPInquery.RFPInqueryType == RFPInqueryType.TechnicalInquery)
               ? SupplierProposalState.Answered
               : SupplierProposalState.Evaluation);

                commercialProposalState.Add(!item.RFPSupplierProposal.Any(a => !a.IsDeleted && a.RFPSupplier.IsActive && !a.RFPInquery.IsDeleted && a.RFPInquery.RFPInqueryType == RFPInqueryType.CommercialInquery)
               ? SupplierProposalState.None
               : item.RFPSupplierProposal.Any(a => !a.RFPSupplier.IsDeleted && a.RFPSupplier.IsActive && !a.IsDeleted && !a.IsAnswered && !a.RFPInquery.IsDeleted && a.RFPInquery.RFPInqueryType == RFPInqueryType.CommercialInquery)
               ? SupplierProposalState.Completing
               : item.RFPSupplierProposal.Any(a => !a.RFPSupplier.IsDeleted && a.RFPSupplier.IsActive && !a.IsDeleted && a.IsAnswered && !a.IsEvaluated && !a.RFPInquery.IsDeleted && a.RFPInquery.RFPInqueryType == RFPInqueryType.CommercialInquery)
               ? SupplierProposalState.Answered
               : SupplierProposalState.Evaluation);
            }
            if (techProposalState.Any(a => a != SupplierProposalState.Answered) && rfpModel.Status == RFPStatus.TechnicalProposal)
            {
                if (commercialProposalState.Any(a => a != SupplierProposalState.Answered))
                {
                    rfpModel.Status = RFPStatus.Register;
                }
                else
                {
                    rfpModel.Status = RFPStatus.CommercialProposal;
                }
            }
            if (commercialProposalState.Any(a => a != SupplierProposalState.Answered) && rfpModel.Status == RFPStatus.CommercialProposal)
            {
                if (techProposalState.Any(a => a != SupplierProposalState.Answered))
                {
                    rfpModel.Status = RFPStatus.Register;
                }
                else
                {
                    rfpModel.Status = RFPStatus.TechnicalProposal;
                }
            }
            if (!commercialProposalState.Any(a => a != SupplierProposalState.Evaluation) && (!techProposalState.Any(a => a != SupplierProposalState.Evaluation)) && rfpModel.Status != RFPStatus.RFPEvaluation)
            {

                rfpModel.Status = RFPStatus.RFPEvaluation;

            }
        }
    }
}
