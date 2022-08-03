using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Raybod.SCM.DataAccess.Core;
using Raybod.SCM.DataAccess.Extention;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Model;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Utility.Extention;
using Raybod.SCM.DataTransferObject.Invoice;
using Raybod.SCM.Utility.Common;
using Raybod.SCM.Utility.EnumType;
using Raybod.SCM.DataTransferObject.Audit;
using Microsoft.AspNetCore.Hosting;
using Raybod.SCM.DataTransferObject.PO;

namespace Raybod.SCM.Services.Implementation
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITeamWorkAuthenticationService _authenticationService;
        private readonly IPaymentService _paymentService;
        private readonly ISCMLogAndNotificationService _scmLogAndNotificationService;
        private readonly IContractFormConfigService _formConfigService;
        private readonly DbSet<POSubject> _poSubjectRepository;
        private readonly DbSet<FinancialAccount> _financialAccountRepository;
        private readonly DbSet<WarehouseOutputRequest> _warehouseOutputRepository;
        private readonly DbSet<Invoice> _invoiceRepository;
        private readonly DbSet<Receipt> _receiptRepository;
        private readonly DbSet<PO> _poRepository;
        private readonly DbSet<ReceiptReject> _receiptRejectRepository;
        private readonly DbSet<WarehouseDespatch> _warehouseDespatchRepository;
        private readonly Utilitys.FileHelper _fileHelper;
        private readonly CompanyAppSettingsDto _appSettings;

        public InvoiceService(
            IUnitOfWork unitOfWork,
            IWebHostEnvironment hostingEnvironmentRoot,
            ITeamWorkAuthenticationService authenticationService,
            IPaymentService paymentService,
            IOptions<CompanyAppSettingsDto> appSettings,
            ISCMLogAndNotificationService scmLogAndNotificationService,
            IContractFormConfigService formConfigService)
        {
            _unitOfWork = unitOfWork;
            _authenticationService = authenticationService;
            _paymentService = paymentService;
            _formConfigService=formConfigService;
            _scmLogAndNotificationService = scmLogAndNotificationService;
            _poSubjectRepository = _unitOfWork.Set<POSubject>();
            _financialAccountRepository = _unitOfWork.Set<FinancialAccount>();
            _warehouseOutputRepository = _unitOfWork.Set<WarehouseOutputRequest>();
            _warehouseDespatchRepository = _unitOfWork.Set<WarehouseDespatch>();
            _invoiceRepository = _unitOfWork.Set<Invoice>();
            _receiptRepository = _unitOfWork.Set<Receipt>();
            _poRepository = _unitOfWork.Set<PO>();
            _receiptRejectRepository = _unitOfWork.Set<ReceiptReject>();
            _fileHelper = new Utilitys.FileHelper(hostingEnvironmentRoot);
            _appSettings = appSettings.Value;
        }

        public async Task<ServiceResult<List<WaitingReceiptAndForInvoiceListDto>>> GetWaitingReceiptOrReceiptRejectForInvoiceAsync(AuthenticateDto authenticate, WaitingForInvoiceQueryDto query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<WaitingReceiptAndForInvoiceListDto>>(null, MessageId.AccessDenied);

                var resultList = new List<WaitingReceiptAndForInvoiceListDto>();

                var ServiceResultReceiptList = await GetWaitingReceiptForInvoiceAsync(authenticate, permission, query);
                if (ServiceResultReceiptList.Succeeded && ServiceResultReceiptList.Result != null && ServiceResultReceiptList.Result.Count() > 0)
                    resultList.AddRange(ServiceResultReceiptList.Result);
                else if (!ServiceResultReceiptList.Succeeded)
                    return ServiceResultReceiptList;

                var ServiceResultTransferenceList = await GetWaitingReceiptRejectForInvoiceAsync(authenticate, permission, query);
                if (ServiceResultTransferenceList.Succeeded && ServiceResultTransferenceList.Result != null && ServiceResultTransferenceList.Result.Count() > 0)
                    resultList.AddRange(ServiceResultTransferenceList.Result);
                else if (!ServiceResultTransferenceList.Succeeded)
                    return ServiceResultTransferenceList;

                //var ServiceResultPOSubjectParts = await GetWaitingPOSubjectTypePartForAddInvoiceAsync(authenticate, permission, query);
                //if (ServiceResultPOSubjectParts.Succeeded && ServiceResultPOSubjectParts.Result != null && ServiceResultPOSubjectParts.Result.Count() > 0)
                //    resultList.AddRange(ServiceResultPOSubjectParts.Result);
                //else if (!ServiceResultPOSubjectParts.Succeeded)
                //    return ServiceResultPOSubjectParts;

                var totalCount = resultList.Count();

                resultList = resultList.OrderByDescending(a => a.ReceiptId).ApplayPageing(query).ToList();

                return ServiceResultFactory.CreateSuccess(resultList).WithTotalCount(totalCount);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<WaitingReceiptAndForInvoiceListDto>>(null, exception);
            }
        }

        private async Task<ServiceResult<List<WaitingReceiptAndForInvoiceListDto>>> GetWaitingReceiptForInvoiceAsync(AuthenticateDto authenticate, PermissionResultDto permission, WaitingForInvoiceQueryDto query)
        {
            try
            {
                var dbQuery = _receiptRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.InvoiceId == null && a.PO.BaseContractCode == authenticate.ContractCode && !a.ReceiptSubjects.Any(c => c.ReceiptSubjectPartLists.Any()) )
                    .OrderByDescending(a => a.ReceiptId)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a => a.ReceiptCode.Contains(query.SearchText) || a.PO.POCode.Contains(query.SearchText));

                if (query.SupplierIds != null && query.SupplierIds.Any())
                    dbQuery = dbQuery.Where(a => query.SupplierIds.Contains(a.PO.SupplierId));

                if (query.ProductGroupIds != null && query.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(a => query.ProductGroupIds.Contains(a.PO.ProductGroupId));

                if (query.ProductIds != null && query.ProductIds.Any())
                    dbQuery = dbQuery.Where(a => a.ReceiptSubjects.Any(c => query.ProductIds.Contains(c.ProductId) ||
                     c.ReceiptSubjectPartLists.Any(v => query.ProductIds.Contains(v.ProductId))));

                var result = await dbQuery
                    .Select(r => new WaitingReceiptAndForInvoiceListDto
                    {
                        ReceiptId = r.ReceiptId,
                        WaitingForInvoiceType = WaitingForInvoiceType.Receipt,
                        POId = r.POId,
                        POCode = r.PO.POCode,
                        ReceiptCode=r.ReceiptCode,
                        InvoiceType = r.PO.PContractType == PContractType.Internal ? InvoiceType.InternalPurchase : InvoiceType.ExternalPurchase,
                        Products = r.ReceiptSubjects.Select(a => a.Product.Description).ToList(),
                        SupplierCode = r.PO.Supplier.SupplierCode,
                        SupplierId = r.PO.SupplierId,
                        SupplierName = r.PO.Supplier.Name,
                        SupplierLogo = r.Supplier != null && r.Supplier.Logo != null ? _appSettings.ClientHost + ServiceSetting.UploadImagesPath.LogoSmall + r.Supplier.Logo : null,
                        UserAudit = r.AdderUser != null ? new UserAuditLogDto
                        {
                            AdderUserName = r.AdderUser.FullName,
                            CreateDate = r.CreatedDate.ToUnixTimestamp(),
                            AdderUserImage = r.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + r.AdderUser.Image : null,
                        } : null
                    }).ToListAsync();
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<WaitingReceiptAndForInvoiceListDto>>(null, exception);
            }
        }

        private async Task<ServiceResult<List<WaitingReceiptAndForInvoiceListDto>>> GetWaitingReceiptRejectForInvoiceAsync(AuthenticateDto authenticate, PermissionResultDto permission, WaitingForInvoiceQueryDto query)
        {
            try
            {

                var dbQuery = _warehouseDespatchRepository
                    .Where(a =>  a.ContractCode == authenticate.ContractCode && !a.IsDeleted &&a.InvoiceId==null&&a.WarehouseOutputRequest.ReceiptId!=null&&a.Status!=WarehouseDespatchStatus.DespatchCanceled);

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a => a.WarehouseOutputRequest.RequestCode.Contains(query.SearchText) || a.WarehouseOutputRequest.Receipt.PO.Supplier.Name.Contains(query.SearchText)
                     || a.WarehouseOutputRequest.Receipt.PO.Supplier.SupplierCode.Contains(query.SearchText) || a.WarehouseOutputRequest.Receipt.PO.POCode.Contains(query.SearchText));

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a => a.WarehouseOutputRequest.Receipt.ReceiptCode.Contains(query.SearchText) || a.WarehouseOutputRequest.Receipt.PO.POCode.Contains(query.SearchText));

                if (query.SupplierIds != null && query.SupplierIds.Any())
                    dbQuery = dbQuery.Where(a => query.SupplierIds.Contains(a.WarehouseOutputRequest.Receipt.PO.SupplierId));

                if (query.ProductGroupIds != null && query.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(a => query.ProductGroupIds.Contains(a.WarehouseOutputRequest.Receipt.PO.ProductGroupId));

                if (query.ProductIds != null && query.ProductIds.Any())
                    dbQuery = dbQuery.Where(a => a.WarehouseProductStockLogs.Any(c => query.ProductIds.Contains(c.ProductId)));

                var result = await dbQuery
                    .Select(r => new WaitingReceiptAndForInvoiceListDto
                    {
                        ReceiptId = r.WarehouseOutputRequest.ReceiptId.Value,
                        WaitingForInvoiceType = WaitingForInvoiceType.ReceiptReject,
                        POId = r.WarehouseOutputRequest.Receipt.POId,
                        DispatchCode=r.DespatchCode,
                        POCode = r.WarehouseOutputRequest.Receipt.PO.POCode,
                        ReceiptCode=r.WarehouseOutputRequest.Receipt.ReceiptCode,
                        InvoiceType = InvoiceType.RejectPurchase,
                        Products = r.WarehouseOutputRequest.Subjects.Select(a => a.Product.Description).ToList(),
                        SupplierCode = r.WarehouseOutputRequest.Receipt.PO.Supplier.SupplierCode,
                        SupplierId = r.WarehouseOutputRequest.Receipt.PO.SupplierId,
                        SupplierName = r.WarehouseOutputRequest.Receipt.PO.Supplier.Name,
                        SupplierLogo = r.WarehouseOutputRequest.Receipt.PO.Supplier.Logo != null ? _appSettings.ClientHost + ServiceSetting.UploadImagesPath.LogoSmall + r.WarehouseOutputRequest.Receipt.PO.Supplier.Logo : null,
                        UserAudit = r.WarehouseOutputRequest.AdderUser != null ? new UserAuditLogDto
                        {
                            AdderUserName = r.AdderUser.FullName,
                            CreateDate = r.CreatedDate.ToUnixTimestamp(),
                            AdderUserImage = r.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + r.AdderUser.Image : null,
                        } : null
                    }).ToListAsync();
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<WaitingReceiptAndForInvoiceListDto>>(null, exception);
            }
        }

        private async Task<ServiceResult<List<WaitingReceiptAndReceiptRejectForInvoiceListDto>>> GetWaitingPOSubjectTypePartForAddInvoiceAsync(AuthenticateDto authenticate, PermissionResultDto permission, WaitingForInvoiceQueryDto query)
        {
            try
            {
                var dbQuery = _poSubjectRepository
                    .AsNoTracking()
                    .Where(a => a.PO.BaseContractCode == authenticate.ContractCode && a.POSubjectPartInvoiceStatus == POSubjectPartInvoiceStatus.WaitingForInvoice /*&& a.POSubjectPartLists.Any()*/)
                    .OrderByDescending(a => a.POId)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a => a.PO.POCode.Contains(query.SearchText));

                if (query.SupplierIds != null && query.SupplierIds.Any())
                    dbQuery = dbQuery.Where(a => query.SupplierIds.Contains(a.PO.SupplierId));

                if (query.ProductGroupIds != null && query.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(a => query.ProductGroupIds.Contains(a.PO.ProductGroupId));

                if (query.ProductIds != null && query.ProductIds.Any())
                    dbQuery = dbQuery.Where(a => query.ProductIds.Contains(a.ProductId));

                var result = await dbQuery
                    .Select(r => new WaitingReceiptAndReceiptRejectForInvoiceListDto
                    {
                        RefrenceId = r.ProductId,
     
                        POId = r.POId.Value,
                        POCode = r.PO.POCode,
                        InvoiceType = r.PO.PContractType == PContractType.Internal ? InvoiceType.InternalPurchase : InvoiceType.ExternalPurchase,
                        Products = new List<string> { r.Product.Description },
                        SupplierCode = r.PO.Supplier.SupplierCode,
                        SupplierId = r.PO.SupplierId,
                        SupplierName = r.PO.Supplier.Name,
                        SupplierLogo = r.PO.Supplier.Logo != null ? _appSettings.ClientHost + ServiceSetting.UploadImagesPath.LogoSmall + r.PO.Supplier.Logo : null,
                        UserAudit = r.PO.AdderUser != null ? new UserAuditLogDto
                        {
                            AdderUserName = r.PO.AdderUser.FullName,
                            CreateDate = r.PO.CreatedDate.ToUnixTimestamp(),
                            AdderUserImage = r.PO.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + r.PO.AdderUser.Image : null,
                        } : null
                    }).ToListAsync();

                foreach (var item in result)
                {
                    item.RefrenceCreateDateAndNumbers = await _receiptRepository.Where(a => !a.IsDeleted && a.Invoice == null && a.POId == item.POId &&
                     a.ReceiptSubjects.Any(c => c.ProductId == item.RefrenceId && c.ReceiptSubjectPartLists.Any()))
                        .Select(c => new WaitingRefrenceCreateDateAndNumberDto
                        {
                            Number = c.ReceiptCode,
                            DateCreate = c.CreatedDate.ToUnixTimestamp()
                        }).ToListAsync();
                }
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<WaitingReceiptAndReceiptRejectForInvoiceListDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<int>> GetWaitingForAddInvoiceBadgeCountAsync(AuthenticateDto authenticate)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(0, MessageId.AccessDenied);

                var receiptQuery = _receiptRepository
                    .AsNoTracking()
                    .Where(a => a.InvoiceId == null && a.PO.BaseContractCode == authenticate.ContractCode && !a.ReceiptSubjects.Any(c => c.ReceiptSubjectPartLists.Any()));

                var receiptRejectQuery = _receiptRejectRepository
                    .AsNoTracking()
                    .Where(a => a.InvoiceId == null && a.PO.BaseContractCode == authenticate.ContractCode && !a.ReceiptRejectSubjects.Any(c => c.ReceiptRejectSubjectPartLists.Any()));


               

                var receiptCount = await receiptQuery.CountAsync();
                var receiptRejectCount = await receiptRejectQuery.CountAsync();


                return ServiceResultFactory.CreateSuccess(receiptCount + receiptRejectCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(0, exception);
            }
        }

        public async Task<ServiceResult<WaitingReceiptAndReceiptRejectForInvoiceInfoDto>> GetReceiptByIdForAddNewInvoiceAsync(AuthenticateDto authenticate, long receiptId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<WaitingReceiptAndReceiptRejectForInvoiceInfoDto>(null, MessageId.AccessDenied);

                var receiptModel = await _receiptRepository
                    .Where(a => !a.IsDeleted &&
                    a.ReceiptId == receiptId &&
                    a.PO.BaseContractCode == authenticate.ContractCode &&
                    a.InvoiceId == null && !a.ReceiptSubjects.Any(c => c.ReceiptSubjectPartLists.Any()))
                    .Include(a => a.ReceiptSubjects)
                    .Include(a => a.Supplier)
                    .Include(a => a.PO)
                    .ThenInclude(a => a.PRContract)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                if (receiptModel == null || receiptModel.ReceiptSubjects == null || receiptModel.PO == null)
                    return ServiceResultFactory.CreateError<WaitingReceiptAndReceiptRejectForInvoiceInfoDto>(null, MessageId.EntityDoesNotExist);

                var receiptProductIds = receiptModel.ReceiptSubjects.Select(a => a.ProductId).ToList();
                var poSubjects = await _poSubjectRepository
                    .Where(a => a.POId == receiptModel.POId && receiptProductIds.Contains(a.ProductId))
                    .Include(a => a.Product)
                    .ToListAsync();

                if (poSubjects == null || poSubjects.Count() == 0)
                    return ServiceResultFactory.CreateError<WaitingReceiptAndReceiptRejectForInvoiceInfoDto>(null, MessageId.EntityDoesNotExist);

                var poSubjectProductIds = poSubjects.Select(a => a.ProductId).Distinct().ToList();
                if (receiptProductIds.Any(a => !poSubjectProductIds.Contains(a)))
                    return ServiceResultFactory.CreateError<WaitingReceiptAndReceiptRejectForInvoiceInfoDto>(null, MessageId.DataInconsistency);

                //if (receiptModel.ReceiptSubjects.Any(s => poSubjects.Where(d => d.ProductId == s.ProductId).Sum(v => v.RemainedQuantityToInvoice) < s.ReceiptQuantity))
                //    return ServiceResultFactory.CreateError<InvoiceInfoDto>(null, MessageId.ReceiptQuantityIsTooMuch);

                var result = new WaitingReceiptAndReceiptRejectForInvoiceInfoDto
                {
                    CurrencyType = receiptModel.PO.CurrencyType,
                    InvoiceType = receiptModel.PO.PContractType == PContractType.Internal ? InvoiceType.InternalPurchase : InvoiceType.ExternalPurchase,
                    POId = receiptModel.POId,
                    WaitingForInvoiceType = WaitingForInvoiceType.Receipt,
                   
                    POCode = receiptModel.PO.POCode,
                    PRContractCode = receiptModel.PO.PRContract.PRContractCode,
                    Tax = receiptModel.PO.Tax,
                    ReceiptCode=receiptModel.ReceiptCode,
                    ReceiptId = receiptModel.ReceiptId,
                    SupplierId = receiptModel.SupplierId != null ? receiptModel.SupplierId.Value : 0,
                    SupplierName = receiptModel.Supplier.Name,
                    //SupplierLogo = receiptModel.Supplier.Logo != null ? _appSettings.AdminHost + ServiceSetting.UploadImagesPath.LogoSmall + receiptModel.Supplier.Logo : null,
                    UserAudit = null
                };

                result.InvoiceProducts = CalculateInternalInvoiceProductByReceipt(receiptModel.PO, receiptModel, poSubjects);
                if (result.Tax > 0)
                    result.TotalTax = result.InvoiceProducts.Sum(a => ((result.Tax * a.TotalProductAmount) / 100));
                result.TotalProductAmount = result.InvoiceProducts.Sum(a => a.TotalProductAmount) + result.TotalTax;

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<WaitingReceiptAndReceiptRejectForInvoiceInfoDto>(null, exception);
            }
        }

        public async Task<ServiceResult<WaitingReceiptAndReceiptRejectForInvoiceInfoDto>> GetReceiptRejectByIdForAddNewInvoiceAsync(AuthenticateDto authenticate, long receiptId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<WaitingReceiptAndReceiptRejectForInvoiceInfoDto>(null, MessageId.AccessDenied);

                var rejectModel = await _warehouseDespatchRepository
                    .Where(a => a.WarehouseOutputRequest.ReceiptId == receiptId &&
                    a.InvoiceId == null &&
                    a.ContractCode == authenticate.ContractCode)
                    .Include(a => a.WarehouseProductStockLogs)
                    .ThenInclude(a => a.Product)
                    .Include(a=>a.WarehouseOutputRequest)
                    .ThenInclude(a => a.Receipt)
                    .ThenInclude(a => a.PO)
                    .ThenInclude(a=>a.PRContract)
                    .Include(a => a.WarehouseOutputRequest).ThenInclude(a=>a.Receipt).ThenInclude(a=>a.Supplier)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();


                if (rejectModel == null || rejectModel.WarehouseProductStockLogs == null || rejectModel.WarehouseOutputRequest.Receipt.PO == null)
                    return ServiceResultFactory.CreateError<WaitingReceiptAndReceiptRejectForInvoiceInfoDto>(null, MessageId.EntityDoesNotExist);

                var receiptRejectProductIds = rejectModel.WarehouseProductStockLogs
                    .Select(a => a.ProductId).ToList();

                var poSubjects = await _poSubjectRepository
                      .Where(a => a.POId == rejectModel.WarehouseOutputRequest.Receipt.POId && receiptRejectProductIds.Contains(a.ProductId))
                      .Include(a => a.Product)
                      .ToListAsync();

                if (poSubjects == null || poSubjects.Count() == 0)
                    return ServiceResultFactory.CreateError<WaitingReceiptAndReceiptRejectForInvoiceInfoDto>(null, MessageId.EntityDoesNotExist);

                var poSubjectProductIds = poSubjects.Select(a => a.ProductId).Distinct().ToList();
                if (receiptRejectProductIds.Any(a => !poSubjectProductIds.Contains(a)))
                    return ServiceResultFactory.CreateError<WaitingReceiptAndReceiptRejectForInvoiceInfoDto>(null, MessageId.DataInconsistency);

                var result = new WaitingReceiptAndReceiptRejectForInvoiceInfoDto
                {
                    CurrencyType = rejectModel.WarehouseOutputRequest.Receipt.PO.CurrencyType,
                    InvoiceType = rejectModel.WarehouseOutputRequest.Receipt.PO.PContractType == PContractType.Internal ? InvoiceType.InternalPurchase : InvoiceType.ExternalPurchase,
                    POId = rejectModel.WarehouseOutputRequest.Receipt.POId,
                    DispatchCode=rejectModel.DespatchCode,
                    WaitingForInvoiceType = WaitingForInvoiceType.ReceiptReject,
                    ReceiptCode= rejectModel.WarehouseOutputRequest.Receipt.ReceiptCode,
                    POCode = rejectModel.WarehouseOutputRequest.Receipt.PO.POCode,
                    PRContractCode = rejectModel.WarehouseOutputRequest.Receipt.PO.PRContract.PRContractCode,
                    ReceiptId = rejectModel.WarehouseOutputRequest.Receipt.ReceiptId,
                    SupplierId = rejectModel.WarehouseOutputRequest.Receipt.SupplierId.Value,
                    SupplierName = rejectModel.WarehouseOutputRequest.Receipt.Supplier.Name,
                    Tax = rejectModel.WarehouseOutputRequest.Receipt.PO.Tax,
                    //SupplierLogo = receiptModel.Supplier.Logo != null ? _appSettings.AdminHost + ServiceSetting.UploadImagesPath.LogoSmall + receiptModel.Supplier.Logo : null,
                    UserAudit = null
                };

                result.InvoiceProducts = CalculateInternalInvoiceProductByReceiptReject(rejectModel, poSubjects);
                if (result.Tax > 0)
                    result.TotalTax = result.InvoiceProducts.Sum(a => ((result.Tax * a.TotalProductAmount) / 100));
                result.TotalProductAmount = result.InvoiceProducts.Sum(a => a.TotalProductAmount) + result.TotalTax;

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<WaitingReceiptAndReceiptRejectForInvoiceInfoDto>(null, exception);
            }
        }

        public async Task<ServiceResult<bool>> AddInvoiceByReceiptAsync(AuthenticateDto authenticate, long receiptId,AddInvoiceDto model)
        {
            try
            {
                //if (model.InvoiceProducts == null)
                //    return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var receiptModel = await _receiptRepository.Where(a => a.ReceiptId == receiptId&& a.InvoiceId == null && a.PO.BaseContractCode == authenticate.ContractCode)
                  .Include(a => a.ReceiptSubjects)
                  .Include(a => a.PO)
                  .FirstOrDefaultAsync();

                if (receiptModel == null || receiptModel.ReceiptSubjects == null || receiptModel.PO == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                var receiptProductIds = receiptModel.ReceiptSubjects.Select(a => a.ProductId).ToList();

                var poSubjects = await _poSubjectRepository
                    .Where(a => a.POId == receiptModel.PO.POId && a.PO.BaseContractCode == authenticate.ContractCode && receiptProductIds.Contains(a.ProductId))
                    .ToListAsync();

                if (poSubjects == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                var poSubjectProductIds = poSubjects.Select(a => a.ProductId).Distinct().ToList();
                if (receiptProductIds.Any(a => !poSubjectProductIds.Contains(a)))
                    return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                var invoiceModel = new Invoice
                {
                    CurrencyType = receiptModel.PO.CurrencyType,
                    InvoiceType = receiptModel.PO.PContractType == PContractType.Internal ? InvoiceType.InternalPurchase : InvoiceType.ExternalPurchase,
                    POId = receiptModel.POId,
                    Note = model.Note,
                    SupplierId = receiptModel.SupplierId.Value,
                    PContractType = receiptModel.PO.PContractType,
                    Tax = receiptModel.PO.Tax,
                    InvoiceStatus = InvoiceStatus.NotPayed,
                    FinancialAccount = new FinancialAccount(),
                    InvoiceProducts = new List<InvoiceProduct>(),
                };

                invoiceModel.InvoiceProducts = CalculateInternalInvoiceProductByReceiptWithUpdatePRContractSubject(receiptModel.PO, receiptModel, poSubjects, model);
                invoiceModel.TotalProductAmount = invoiceModel.InvoiceProducts.Sum(a => a.TotalProductAmount);
                if (receiptModel.PO.Tax > 0)
                    invoiceModel.TotalTax = invoiceModel.InvoiceProducts.Sum(a => ((receiptModel.PO.Tax * a.TotalProductAmount) / 100));
                invoiceModel.TotalInvoiceAmount = invoiceModel.TotalProductAmount + invoiceModel.TotalTax  - invoiceModel.TotalDiscount;

                // add attachment

                if (model.Attachments != null && model.Attachments.Count() > 0)
                {
                    var attachmentResult = await AddInvoiceAttachmentAsync(invoiceModel, model.Attachments);
                    if (!attachmentResult.Succeeded)
                        return ServiceResultFactory.CreateError(false, attachmentResult.Messages.FirstOrDefault().Message);

                    invoiceModel = attachmentResult.Result;
                }
                // add financialAccount
                invoiceModel = AddFinancialAccountByInvoiceReceipt(invoiceModel);

                // generate form code
                var count = await _invoiceRepository.CountAsync(a => a.PO.BaseContractCode == authenticate.ContractCode);
                var codeRes = await _formConfigService.GenerateFormCodeAsync(authenticate.ContractCode, FormName.Invoice, count);
                if (!codeRes.Succeeded)
                    return ServiceResultFactory.CreateError(false, codeRes.Messages.First().Message);
                invoiceModel.InvoiceNumber = codeRes.Result;
                receiptModel.Invoice = new Invoice();
                receiptModel.Invoice = invoiceModel;
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    //await _paymentService.AddPendingToPaymentBaseOnTermsOfPaymentOfInvoice(authenticate, receiptModel.PO, invoiceModel.Id, invoiceModel.TotalInvoiceAmount);
                    await _scmLogAndNotificationService.SetDonedNotificationAsync(authenticate.UserId, receiptModel.PO.BaseContractCode, receiptModel.ReceiptId.ToString(), NotifEvent.AddInvoice);
                    var res = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = receiptModel.PO.BaseContractCode,
                        FormCode = invoiceModel.InvoiceNumber,
                        Description = receiptModel.PO.POCode,
                        Temp = receiptModel.ReceiptCode,
                        KeyValue = invoiceModel.Id.ToString(),
                        NotifEvent = NotifEvent.AddInvoice,
                        RootKeyValue = invoiceModel.Id.ToString(),
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,
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

        public async Task<ServiceResult<bool>> AddInvoiceByReceiptRejectAsync(AuthenticateDto authenticate,long receiptId, AddInvoiceDto model)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var rejectModel = await _warehouseDespatchRepository.Where(a => a.WarehouseOutputRequest.ReceiptId == receiptId && a.InvoiceId == null &&
                a.ContractCode == authenticate.ContractCode)
                  .Include(a => a.WarehouseProductStockLogs)
                  .Include(a=>a.WarehouseOutputRequest)
                  .ThenInclude(a=>a.Receipt)
                  .ThenInclude(a => a.Supplier)
                  .Include(a => a.WarehouseOutputRequest)
                  .ThenInclude(a => a.Receipt)
                  .ThenInclude(a => a.PO)
                  .ThenInclude(a => a.PRContract)
                  .FirstOrDefaultAsync();

                if (rejectModel == null || rejectModel.WarehouseProductStockLogs == null || rejectModel.WarehouseOutputRequest.Receipt.PO == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                var rejectProductIds = rejectModel.WarehouseProductStockLogs
                    .Select(a => a.ProductId).ToList();


                var poSubjects = await _poSubjectRepository
                     .Where(a => a.POId == rejectModel.WarehouseOutputRequest.Receipt.PO.POId && a.PO.BaseContractCode == authenticate.ContractCode && rejectProductIds.Contains(a.ProductId))
                     .ToListAsync();

                if (poSubjects == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                var poSubjectProductIds = poSubjects.Select(a => a.ProductId).Distinct().ToList();
                if (rejectProductIds.Any(a => !poSubjectProductIds.Contains(a)))
                    return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                var poProductsIds = poSubjects.Select(a => a.ProductId).Distinct().ToList();
                if (rejectProductIds.Any(a => !poProductsIds.Contains(a)))
                    return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                var invoiceModel = new Invoice
                {
                    CurrencyType = rejectModel.WarehouseOutputRequest.Receipt.PO.CurrencyType,
                    InvoiceType = InvoiceType.RejectPurchase,
                    POId = rejectModel.WarehouseOutputRequest.Receipt.POId,
                    Note=model.Note,
                    SupplierId = rejectModel.WarehouseOutputRequest.Receipt.SupplierId.Value,
                    PContractType = rejectModel.WarehouseOutputRequest.Receipt.PO.PContractType,
                    Tax = rejectModel.WarehouseOutputRequest.Receipt.PO.Tax,
                    InvoiceStatus = InvoiceStatus.NotPayed,
                    FinancialAccount = new FinancialAccount(),
                    InvoiceProducts = new List<InvoiceProduct>()
                };

                invoiceModel.InvoiceProducts = CalculateInternalInvoiceProductByTransferenceForAddNewInvoice(rejectModel, poSubjects);
                invoiceModel.TotalProductAmount = invoiceModel.InvoiceProducts.Sum(a => a.TotalProductAmount);
                if (rejectModel.WarehouseOutputRequest.Receipt.PO.Tax > 0)
                    invoiceModel.TotalTax = invoiceModel.InvoiceProducts.Sum(a => ((rejectModel.WarehouseOutputRequest.Receipt.PO.Tax * a.TotalProductAmount) / 100));
                invoiceModel.TotalInvoiceAmount = invoiceModel.TotalProductAmount + invoiceModel.TotalTax  - invoiceModel.TotalDiscount;

                // add attachment
                if (model.Attachments != null && model.Attachments.Count() > 0)
                {
                    var attachmentResult = await AddInvoiceAttachmentAsync(invoiceModel, model.Attachments);
                    if (!attachmentResult.Succeeded)
                        return ServiceResultFactory.CreateError(false, attachmentResult.Messages.FirstOrDefault().Message);

                    invoiceModel = attachmentResult.Result;
                }

                // add financialAccount
                invoiceModel = AddFinancialAccountByInvoiceReceipt(invoiceModel);

                // generate form code
                var count = await _invoiceRepository.CountAsync(a => a.PO.BaseContractCode == authenticate.ContractCode);
                var codeRes = await _formConfigService.GenerateFormCodeAsync(authenticate.ContractCode, FormName.Invoice, count);
                if (!codeRes.Succeeded)
                    return ServiceResultFactory.CreateError(false, codeRes.Messages.First().Message);
                invoiceModel.InvoiceNumber = codeRes.Result;
                rejectModel.Invoice = new Invoice();
                rejectModel.Invoice = invoiceModel;
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    await _scmLogAndNotificationService.SetDonedNotificationAsync(authenticate.UserId, rejectModel.WarehouseOutputRequest.Receipt.PO.BaseContractCode, rejectModel.WarehouseOutputRequest.Receipt.ReceiptId.ToString(), NotifEvent.AddInvoice);
                    var res = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = rejectModel.WarehouseOutputRequest.Receipt.PO.BaseContractCode,
                        FormCode = invoiceModel.InvoiceNumber,
                        Description = rejectModel.WarehouseOutputRequest.Receipt.PO.POCode,
                        Temp = rejectModel.WarehouseOutputRequest.Receipt.ReceiptCode,
                        KeyValue = invoiceModel.Id.ToString(),
                        NotifEvent = NotifEvent.AddInvoice,
                        RootKeyValue = invoiceModel.Id.ToString(),
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,
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

        
        private async Task<ServiceResult<Invoice>> AddInvoiceAttachmentAsync(Invoice invoice, List<AddAttachmentDto> attachment)
        {
            invoice.Attachments = new List<PaymentAttachment>();

            if (!_fileHelper.FileExistInTemp(attachment.Select(c => c.FileSrc).ToList()))
                return ServiceResultFactory.CreateError<Invoice>(null, MessageId.FileNotFound);

            foreach (var item in attachment)
            {
                var UploadedFile =
                    await _fileHelper.SaveDocumentFromTemp(item.FileSrc, ServiceSetting.UploadFilePath.Invoice);
                if (UploadedFile == null)
                    return ServiceResultFactory.CreateError<Invoice>(null, MessageId.UploudFailed);

                _fileHelper.DeleteDocumentFromTemp(item.FileSrc);
                invoice.Attachments.Add(new PaymentAttachment
                {
                    FileType = UploadedFile.FileType,
                    FileSize = UploadedFile.FileSize,
                    FileName = item.FileName,
                    FileSrc = item.FileSrc,
                });
            }

            return ServiceResultFactory.CreateSuccess(invoice);
        }


        public async Task<ServiceResult<List<ListInvoiceDto>>> GetsInvoiceAsync(AuthenticateDto authenticate, InvoiceQueryDto query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<ListInvoiceDto>>(null, MessageId.AccessDenied);

                var dbQuery = _invoiceRepository.Where(a => !a.IsDeleted && a.PO.BaseContractCode == authenticate.ContractCode)
                    .OrderByDescending(a => a.Id).AsQueryable();

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a => a.Supplier.Name.Contains(query.SearchText) || a.InvoiceNumber.Contains(query.SearchText) || a.InvoiceProducts.Any(p => p.Product.Description.Contains(query.SearchText)));

                if (query.InvoiceType == InvoiceType.ExternalPurchase)
                    dbQuery = dbQuery.Where(a => a.InvoiceType == query.InvoiceType);
                else if (query.InvoiceType == InvoiceType.InternalPurchase)
                    dbQuery = dbQuery.Where(a => a.InvoiceType == query.InvoiceType);
                else if (query.InvoiceType == InvoiceType.RejectPurchase)
                    dbQuery = dbQuery.Where(a => a.InvoiceType == query.InvoiceType);

                var totalCount = dbQuery.Count();
                var columnsMap = new Dictionary<string, Expression<Func<Invoice, object>>>
                {
                    ["Id"] = v => v.Id,
                    ["InvoiceNumber"] = v => v.InvoiceNumber,
                    ["SupplierName"] = v => v.Supplier.Name
                };
                dbQuery = dbQuery.ApplayOrdering(query, columnsMap).ApplayPageing(query);

                var result = await dbQuery.Select(c => new ListInvoiceDto
                {
                    InvoiceId = c.Id,
                    DateCreate = c.CreatedDate.ToUnixTimestamp(),
                    InvoiceNumber = c.InvoiceNumber,
                    PContractType = c.PContractType,
                    IsDispatch = (c.InvoiceType == InvoiceType.RejectPurchase) ? true : false,
                    Products = c.InvoiceProducts.Select(a => a.Product.Description).ToList(),
                    SupplierName = c.Supplier.Name
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<ListInvoiceDto>>(null, exception);
            }
        }

        //public async Task<ServiceResult<List<ListInvoiceDto>>> GetsInvoiceByPoIdAsync(AuthenticateDto authenticate, InvoiceQueryDto query)
        //{
        //    try
        //    {
        //        var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
        //        if (!permission.HasPermission)
        //            return ServiceResultFactory.CreateError<List<ListInvoiceDto>>(null, MessageId.AccessDenied);

        //        var dbQuery = _invoiceRepository.Where(a => !a.IsDeleted && a.POId == query.PoId && a.PO.BaseContractCode == authenticate.ContractCode);

        //        if (!string.IsNullOrEmpty(query.SearchText))
        //            dbQuery = dbQuery.Where(a => a.Supplier.Name.Contains(query.SearchText) || a.InvoiceNumber.Contains(query.SearchText) || a.InvoiceProducts.Any(p => p.Product.Description.Contains(query.SearchText)));

        //        if (query.InvoiceType == InvoiceType.ExternalPurchase)
        //            dbQuery = dbQuery.Where(a => a.InvoiceType == query.InvoiceType);
        //        else if (query.InvoiceType == InvoiceType.ExternalPurchase)
        //            dbQuery = dbQuery.Where(a => a.InvoiceType == query.InvoiceType);
        //        else if (query.InvoiceType == InvoiceType.ExternalPurchase)
        //            dbQuery = dbQuery.Where(a => a.InvoiceType == query.InvoiceType);

        //        dbQuery = dbQuery.OrderByDescending(a => a.Id);

        //        var result = await dbQuery.Select(c => new ListInvoiceDto
        //        {
        //            InvoiceId = c.Id,
        //            DateCreate = c.CreatedDate.ToUnixTimestamp(),
        //            InvoiceNumber = c.InvoiceNumber,
        //            PContractType = c.PContractType,
        //            Products = c.InvoiceProducts.Select(a => a.Product.Description).ToList(),
        //            SupplierName = c.Supplier.Name
        //        }).ToListAsync();

        //        return ServiceResultFactory.CreateSuccess(result);

        //    }
        //    catch (Exception exception)
        //    {
        //        return ServiceResultFactory.CreateException<List<ListInvoiceDto>>(null, exception);
        //    }
        //}

        public async Task<ServiceResult<InvoiceInfoDto>> GetInvoiceByIdAsync(AuthenticateDto authenticate, long invoiceId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<InvoiceInfoDto>(null, MessageId.AccessDenied);

                var dbQuery = _invoiceRepository.Where(a => !a.IsDeleted && a.Id == invoiceId && a.PO.BaseContractCode == authenticate.ContractCode);
                string dispatchCode = "";
                var dispatch = await _warehouseDespatchRepository.Where(a => !a.IsDeleted && a.InvoiceId == invoiceId).FirstOrDefaultAsync();
                if (dispatch != null)
                {
                    dispatchCode = dispatch.DespatchCode;
                }

                var result = await dbQuery
                    .Select(i => new InvoiceInfoDto
                    {
                        InvoiceId = i.Id,
                        CurrencyType = i.CurrencyType,
                        InvoiceType = i.InvoiceType,
                        PContractType = i.PContractType,
                        Note = !String.IsNullOrEmpty(i.Note)?i.Note:"",
                        InvoiceNumber = i.InvoiceNumber,
                        DispatchCode= dispatchCode,
                        IsDispatch=(i.InvoiceType==InvoiceType.RejectPurchase),
                        //ReferenceId = i.Receipts.Any() != null ? i.Receipt.ReceiptId : i.ReceiptReject != null ? i.ReceiptReject.ReceiptRejectId : 0,
                        SupplierId = i.SupplierId,
                        Tax = i.Tax,
                        TotalDiscount = i.TotalDiscount,
                        TotalInvoiceAmount = i.TotalInvoiceAmount,
                        TotalProductAmount = i.TotalProductAmount,
                        TotalTax = i.TotalTax,
                        OtherCosts = i.OtherCosts,
                        UserAudit = i.AdderUser != null
                       ? new UserAuditLogDto
                       {
                           AdderUserId = i.AdderUserId,
                           AdderUserName = i.AdderUser.FullName,
                           CreateDate = i.CreatedDate.ToUnixTimestamp(),
                           AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall +
                                            i.AdderUser.Image
                       }
                       : null,
                        SupplierName = i.Supplier.Name,
                        SupplierCode = i.Supplier.SupplierCode,
                        SupplierLogo = _appSettings.ClientHost + ServiceSetting.UploadImagesPath.LogoSmall + i.Supplier.Logo,
                        InvoiceProducts = i.InvoiceProducts.Select(p => new InvoiceProductInfoDto
                        {
                            Id = p.Id,
                            InvoiceId = p.InvoiceId,
                            TotalProductAmount = p.TotalProductAmount,
                            ProductCode = p.Product.ProductCode,
                            ProductId = p.ProductId,
                            ProductName = p.Product.Description,
                            ProductUnit = p.Product.Unit,
                            Quantity = p.Quantity,
                            UnitPrice = p.UnitPrice
                        }).ToList(),
                        Attachments = i.Attachments.Any(v => !v.IsDeleted)? i.Attachments.Where(v => !v.IsDeleted).Select(v => new InvoiceAttachmentDto
                        {
                            AttachmentId = v.Id,
                            FileName = v.FileName,
                            FileSrc = v.FileSrc,
                            FileSize = v.FileSize,
                            FileType = v.FileType,
                            InvoiceId = v.InvoiceId.Value
                        }).ToList():new List<InvoiceAttachmentDto>(),
                    }).FirstOrDefaultAsync();

                return ServiceResultFactory.CreateSuccess(result);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<InvoiceInfoDto>(null, exception);
            }
        }

        public async Task<ServiceResult<PoInvoiceDto>> GetInvoiceByPOIdAsync(AuthenticateDto authenticate, long poId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<PoInvoiceDto>(null, MessageId.AccessDenied);
                var poQuery = _poRepository
                .AsNoTracking()
                .Where(a => !a.IsDeleted && a.POId == poId);

                if (poQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<PoInvoiceDto>(null, MessageId.EntityDoesNotExist);

                if (permission.ProductGroupIds.Any() && !poQuery.Any(c => permission.ProductGroupIds.Contains(c.ProductGroupId)))
                    return ServiceResultFactory.CreateError<PoInvoiceDto>(null, MessageId.AccessDenied);

                var preInvoicResult = await poQuery.Select(c => new POFinancialDetailsDto
                {
                    POCode = c.POCode,
                    Tax = c.Tax,
                    TotalAmount = c.TotalAmount,
                    FinalTotalAmount = c.FinalTotalAmount,
                    TotalTax = c.FinalTotalAmount - c.TotalAmount,
                    CurrencyType = c.CurrencyType,
                    POSubjects = c.POSubjects.Select(v => new POSubjectFinancialDetailsDto
                    {
                        ProductId = v.ProductId,
                        PriceUnit = v.PriceUnit,
                        ProductCode = v.Product.ProductCode,
                        ProductName = v.Product.Description,
                        ProductUnit = v.Product.Unit,
                        Quantity = v.Quantity
                    }).ToList(),
                    UserAudit = c.AdderUser != null ? new UserAuditLogDto
                    {
                        CreateDate = c.CreatedDate.ToUnixTimestamp(),
                        AdderUserName = c.AdderUser.FullName,
                        AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall +
                                             c.AdderUser.Image
                    } : null
                }).FirstOrDefaultAsync();
                var dbQuery = _invoiceRepository.Where(a => !a.IsDeleted && a.POId == poId && a.PO.BaseContractCode == authenticate.ContractCode);

                var invoiceResult = await dbQuery
                    .Select(i => new InvoiceInfoDto
                    {
                        InvoiceId = i.Id,
                        CurrencyType = i.CurrencyType,
                        InvoiceType = i.InvoiceType,
                        PContractType = i.PContractType,
                        Note = i.Note,
                        InvoiceNumber = i.InvoiceNumber,
                        //ReferenceId = i.Receipts.Any() != null ? i.Receipt.ReceiptId : i.ReceiptReject != null ? i.ReceiptReject.ReceiptRejectId : 0,
                        SupplierId = i.SupplierId,
                        Tax = i.Tax,
                        TotalDiscount = i.TotalDiscount,
                        TotalInvoiceAmount = i.TotalInvoiceAmount,
                        TotalProductAmount = i.TotalProductAmount,
                        TotalTax = i.TotalTax,
                        OtherCosts = i.OtherCosts,
                        UserAudit = i.AdderUser != null
                       ? new UserAuditLogDto
                       {
                           AdderUserId = i.AdderUserId,
                           AdderUserName = i.AdderUser.FullName,
                           CreateDate = i.CreatedDate.ToUnixTimestamp(),
                           AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall +
                                            i.AdderUser.Image
                       }
                       : null,
                        SupplierName = i.Supplier.Name,
                        SupplierCode = i.Supplier.SupplierCode,
                        SupplierLogo = _appSettings.ClientHost + ServiceSetting.UploadImagesPath.LogoSmall + i.Supplier.Logo,
                        InvoiceProducts = i.InvoiceProducts.Select(p => new InvoiceProductInfoDto
                        {
                            Id = p.Id,
                            InvoiceId = p.InvoiceId,
                            TotalProductAmount = p.TotalProductAmount,
                            ProductCode = p.Product.ProductCode,
                            ProductId = p.ProductId,
                            ProductName = p.Product.Description,
                            ProductUnit = p.Product.Unit,
                            Quantity = p.Quantity,
                            UnitPrice = p.UnitPrice
                        }).ToList(),
                        Attachments = i.Attachments.Where(v => !v.IsDeleted).Select(v => new InvoiceAttachmentDto
                        {
                            AttachmentId = v.Id,
                            FileName = v.FileName,
                            FileSrc = v.FileSrc,
                            FileSize = v.FileSize,
                            FileType = v.FileType,
                            InvoiceId = v.InvoiceId.Value
                        }).ToList(),
                    }).ToListAsync();
                PoInvoiceDto result = new PoInvoiceDto();
                result.InvoiceInfo = invoiceResult;
                result.PreInvoice = preInvoicResult;
                return ServiceResultFactory.CreateSuccess(result);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<PoInvoiceDto>(null, exception);
            }
        }


        //public async Task<ServiceResult<InvoiceInfoDto>> GetInvoiceByIdAndPoIdAsync(AuthenticateDto authenticate, long poId, long invoiceId)
        //{
        //    try
        //    {
        //        var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
        //        if (!permission.HasPermission)
        //            return ServiceResultFactory.CreateError<InvoiceInfoDto>(null, MessageId.AccessDenied);

        //        var dbQuery = _invoiceRepository
        //            .Where(a => !a.IsDeleted && a.Id == invoiceId && a.POId == poId && a.PO.BaseContractCode == authenticate.ContractCode);

        //        if (dbQuery.Count() == 0)
        //            return ServiceResultFactory.CreateError<InvoiceInfoDto>(null, MessageId.EntityDoesNotExist);

        //        return await ReturnInvoiceById(dbQuery);

        //    }
        //    catch (Exception exception)
        //    {
        //        return ServiceResultFactory.CreateException<InvoiceInfoDto>(null, exception);
        //    }
        //}

        //private async Task<ServiceResult<InvoiceInfoDto>> ReturnInvoiceById(IQueryable<Invoice> dbQuery)
        //{
        //    try
        //    {
        //        var result = await dbQuery
        //            .Select(i => new InvoiceInfoDto
        //            {
        //                InvoiceId = i.Id,
        //                CurrencyType = i.CurrencyType,
        //                InvoiceType = i.InvoiceType,
        //                PContractType = i.PContractType,
        //                Note = i.Note,
        //                InvoiceNumber = i.InvoiceNumber,
        //                //ReferenceId = i.Receipt != null ? i.Receipt.ReceiptId : i.ReceiptReject != null ? i.ReceiptReject.ReceiptRejectId : 0,
        //                SupplierId = i.SupplierId,
        //                Tax = i.Tax,
        //                TotalDiscount = i.TotalDiscount,
        //                TotalInvoiceAmount = i.TotalInvoiceAmount,
        //                TotalProductAmount = i.TotalProductAmount,
        //                TotalTax = i.TotalTax,
        //                OtherCosts = i.OtherCosts,
        //                UserAudit = i.AdderUser != null
        //               ? new UserAuditLogDto
        //               {
        //                   AdderUserId = i.AdderUserId,
        //                   AdderUserName = i.AdderUser.FullName,
        //                   CreateDate = i.CreatedDate.ToUnixTimestamp(),
        //                   AdderUserImage = _appSettings.ElasticHost + ServiceSetting.UploadImagesPath.UserSmall +
        //                                    i.AdderUser.Image
        //               }
        //               : null,
        //                SupplierName = i.Supplier.Name,
        //                SupplierCode = i.Supplier.SupplierCode,
        //                SupplierLogo = _appSettings.AdminHost + ServiceSetting.UploadImagesPath.LogoSmall + i.Supplier.Logo,
        //                InvoiceProducts = i.InvoiceProducts.Select(p => new InvoiceProductInfoDto
        //                {
        //                    Id = p.Id,
        //                    InvoiceId = p.InvoiceId,
        //                    TotalProductAmount = p.TotalProductAmount,
        //                    ProductCode = p.Product.ProductCode,
        //                    ProductId = p.ProductId,
        //                    ProductName = p.Product.Description,
        //                    ProductUnit = p.Product.Unit,
        //                    Quantity = p.Quantity,
        //                    UnitPrice = p.UnitPrice
        //                }).ToList(),
        //                Attachments = i.Attachments.Where(v => !v.IsDeleted).Select(v => new InvoiceAttachmentDto
        //                {
        //                    AttachmentId = v.Id,
        //                    FileName = v.FileName,
        //                    FileSize = v.FileSize,
        //                    FileSrc = v.FileSrc,
        //                    FileType = v.FileType,
        //                    InvoiceId = v.InvoiceId.Value
        //                }).ToList(),
        //            }).FirstOrDefaultAsync();
        //        return ServiceResultFactory.CreateSuccess(result);
        //    }
        //    catch (Exception exception)
        //    {
        //        return ServiceResultFactory.CreateException<InvoiceInfoDto>(null, exception);
        //    }

        //}

        private Invoice AddFinancialAccountByInvoiceReceipt(Invoice invoiceModel)
        {
            FinancialAccountType financialAccountType = ReturnFinancialTypeByInvoiceType(invoiceModel.InvoiceType);
            var financialModel = new FinancialAccount
            {
                DateDone = DateTime.UtcNow,
                FinancialAccountType = financialAccountType,
                CurrencyType = invoiceModel.CurrencyType,
                POId = invoiceModel.POId.Value,
                SupplierId = invoiceModel.SupplierId
            };


            financialModel.PaymentAmount = 0;

            var someReminded = _financialAccountRepository
                .Where(a => a.SupplierId == invoiceModel.SupplierId && a.CurrencyType == invoiceModel.CurrencyType)
                .Sum(v => (v.PurchaseAmount -( v.RejectPurchaseAmount + v.PaymentAmount)));

            switch (financialAccountType)
            {
                case FinancialAccountType.Purchase:
                    financialModel.PurchaseAmount = invoiceModel.TotalInvoiceAmount;
                    financialModel.RemainedAmount = someReminded + financialModel.PurchaseAmount;
                    break;
                case FinancialAccountType.RejectPurchase:
                    financialModel.RejectPurchaseAmount = invoiceModel.TotalInvoiceAmount;
                    financialModel.RemainedAmount = someReminded - financialModel.RejectPurchaseAmount;
                    break;
                case FinancialAccountType.Payment:
                    break;
                case FinancialAccountType.InitialAccountOfYear:
                    break;
                default:
                    break;
            }

            invoiceModel.FinancialAccount = financialModel;
            return invoiceModel;
        }

        private static FinancialAccountType ReturnFinancialTypeByInvoiceType(InvoiceType invoiceType)
        {
            switch (invoiceType)
            {
                case InvoiceType.InternalPurchase:
                    return FinancialAccountType.Purchase;
                case InvoiceType.ExternalPurchase:
                    return FinancialAccountType.Purchase;
                case InvoiceType.RejectPurchase:
                    return FinancialAccountType.RejectPurchase;
                default:
                    return FinancialAccountType.RejectPurchase;
            }
        }

        //private List<InvoiceProductInfoDto> CalculateForeignInvoiceProductByReceipt(PO poModel, Receipt receiptModel, List<PRContractSubject> prContractSubjects)
        //{
        //    var result = new List<InvoiceProductInfoDto>();
        //    foreach (var receiptProduct in receiptModel.ReceiptSubjects)
        //    {
        //        var prSubjectsOfThis = prContractSubjects
        //            .Where(a => a.ProductId == receiptProduct.ProductId)
        //            .ToList();

        //        bool isDone = false;
        //        decimal thisProductReceiptQuantity = receiptProduct.ReceiptQuantity;
        //        foreach (var item in prSubjectsOfThis)
        //        {
        //            var newInvoiceProduct = new InvoiceProductInfoDto();
        //            if (isDone)
        //                continue;
        //            if (thisProductReceiptQuantity <= item.RemainedQuantityToInvoice)
        //            {
        //                newInvoiceProduct = new InvoiceProductInfoDto
        //                {
        //                    ProductCode = item.Product.ProductCode,
        //                    ProductId = item.ProductId,
        //                    ProductName = item.Product.Description,
        //                    ProductUnit = item.Product.Unit,
        //                    Quantity = thisProductReceiptQuantity,
        //                    UnitPrice = item.UnitPrice,
        //                    //TotalProductAmount = thisProductReceiptQuantity * item.UnitPrice
        //                };
        //                isDone = true;
        //            }
        //            else
        //            {
        //                thisProductReceiptQuantity = thisProductReceiptQuantity - item.RemainedQuantityToInvoice;
        //                newInvoiceProduct = new InvoiceProductInfoDto
        //                {
        //                    ProductCode = item.Product.ProductCode,
        //                    ProductId = item.ProductId,
        //                    ProductName = item.Product.Description,
        //                    ProductUnit = item.Product.Unit,
        //                    Quantity = thisProductReceiptQuantity,
        //                    UnitPrice = item.UnitPrice,
        //                    //TotalProductAmount = thisProductReceiptQuantity * item.UnitPrice
        //                };
        //            }
        //            result.Add(newInvoiceProduct);
        //        }
        //    }
        //    return result;
        //}

        private List<InvoiceProductInfoDto> CalculateInternalInvoiceProductByReceipt(PO poModel, Receipt receiptModel, List<POSubject> poSubjects)
        {
            var result = new List<InvoiceProductInfoDto>();
            foreach (var receiptProduct in receiptModel.ReceiptSubjects)
            {
                var selectedPOSubject = poSubjects
                    .Where(a => a.ProductId == receiptProduct.ProductId)
                    .FirstOrDefault();

                var newInvoiceProduct = new InvoiceProductInfoDto();

                newInvoiceProduct = new InvoiceProductInfoDto
                {
                    ProductCode = selectedPOSubject.Product.ProductCode,
                    ProductId = selectedPOSubject.ProductId,
                    ProductName = selectedPOSubject.Product.Description,
                    ProductUnit = selectedPOSubject.Product.Unit,
                    UnitPrice = selectedPOSubject.PriceUnit,
                    Quantity = receiptProduct.ReceiptQuantity,
                    TotalProductAmount = receiptProduct.ReceiptQuantity * selectedPOSubject.PriceUnit
                };

                result.Add(newInvoiceProduct);
            }

            return result;
        }

        private List<InvoiceProductInfoDto> CalculateInvoiceProductByPOSubject(PO poModel, POSubject poSubjectModel)
        {
            var result = new List<InvoiceProductInfoDto>();

            result.Add(new InvoiceProductInfoDto
            {
                ProductCode = poSubjectModel.Product.ProductCode,
                ProductId = poSubjectModel.ProductId,
                ProductName = poSubjectModel.Product.Description,
                ProductUnit = poSubjectModel.Product.Unit,
                UnitPrice = poSubjectModel.PriceUnit,
                Quantity = poSubjectModel.Quantity,
                TotalProductAmount = poSubjectModel.Quantity * poSubjectModel.PriceUnit
            });
            return result;
        }

        //private List<InvoiceProduct> CalculateForeignInvoiceProductByReceiptWithUpdatePRContractSubject(PO poModel, Receipt receiptModel, List<PRContractSubject> prContractSubjects, AddInvoiceDto model)
        //{
        //    var result = new List<InvoiceProduct>();
        //    foreach (var receiptProduct in receiptModel.ReceiptSubjects)
        //    {
        //        var prSubjectsOfThis = prContractSubjects
        //            .Where(a => a.ProductId == receiptProduct.ProductId)
        //            .ToList();

        //        //var postedProduct = model.InvoiceProducts.FirstOrDefault(a => a.ProductId == receiptProduct.ProductId);

        //        bool isDone = false;
        //        decimal thisProductReceiptQuantity = receiptProduct.ReceiptQuantity;
        //        foreach (var item in prSubjectsOfThis)
        //        {
        //            var newInvoiceProduct = new InvoiceProduct();
        //            if (isDone)
        //                continue;
        //            if (thisProductReceiptQuantity <= item.RemainedQuantityToInvoice)
        //            {
        //                newInvoiceProduct.ProductId = item.ProductId;
        //                newInvoiceProduct.Quantity = thisProductReceiptQuantity;
        //                newInvoiceProduct.UnitPrice = item.UnitPrice;
        //                //newInvoiceProduct.TotalProductAmount = thisProductReceiptQuantity * item.UnitPrice;
        //                //newInvoiceProduct.DiscountIRR = postedProduct.DiscountIRR;
        //                //newInvoiceProduct.UnitPriceIRR = postedProduct.UnitPrice * model.ExChangeRate;
        //                //newInvoiceProduct.TotalProductAmountIRR = newInvoiceProduct.UnitPriceIRR * newInvoiceProduct.Quantity;
        //                //newInvoiceProduct.FinalTotalPeymentAmountIRR = newInvoiceProduct.TotalProductAmountIRR - newInvoiceProduct.DiscountIRR;

        //                item.RemainedQuantityToInvoice -= thisProductReceiptQuantity;
        //                isDone = true;
        //            }
        //            else
        //            {
        //                thisProductReceiptQuantity = thisProductReceiptQuantity - item.RemainedQuantityToInvoice;
        //                newInvoiceProduct.ProductId = item.ProductId;
        //                newInvoiceProduct.Quantity = thisProductReceiptQuantity;
        //                newInvoiceProduct.UnitPrice = item.UnitPrice;
        //                //newInvoiceProduct.TotalProductAmount = thisProductReceiptQuantity * item.UnitPrice;
        //                //newInvoiceProduct.DiscountIRR = postedProduct.DiscountIRR;
        //                //newInvoiceProduct.UnitPriceIRR = postedProduct.UnitPrice * model.ExChangeRate;
        //                //newInvoiceProduct.TotalProductAmountIRR = newInvoiceProduct.UnitPriceIRR * newInvoiceProduct.Quantity;
        //                //newInvoiceProduct.FinalTotalPeymentAmountIRR = newInvoiceProduct.TotalProductAmountIRR - newInvoiceProduct.DiscountIRR;

        //                item.RemainedQuantityToInvoice = 0;
        //            }
        //            result.Add(newInvoiceProduct);
        //        }
        //    }
        //    return result;
        //}

        private List<InvoiceProduct> CalculateInternalInvoiceProductByReceiptWithUpdatePRContractSubject(PO poModel, Receipt receiptModel, List<POSubject> poSubjects, AddInvoiceDto model)
        {
            var result = new List<InvoiceProduct>();
            foreach (var receiptProduct in receiptModel.ReceiptSubjects)
            {
                var selectedPOSubject = poSubjects
                    .Where(a => a.ProductId == receiptProduct.ProductId)
                    .FirstOrDefault();

                decimal thisProductReceiptQuantity = receiptProduct.ReceiptQuantity;

                var newInvoiceProduct = new InvoiceProduct();

                newInvoiceProduct.ProductId = selectedPOSubject.ProductId;
                newInvoiceProduct.Quantity = receiptProduct.ReceiptQuantity;
                newInvoiceProduct.UnitPrice = selectedPOSubject.PriceUnit;
                newInvoiceProduct.TotalProductAmount = receiptProduct.ReceiptQuantity * selectedPOSubject.PriceUnit;

                result.Add(newInvoiceProduct);
            }

            return result;
        }

        private void CalculateInvoiceProductByPOSubjectPartList(Invoice invoiceModel, POSubject selectedPOSubject)
        {
            invoiceModel.InvoiceProducts = new List<InvoiceProduct>();
            invoiceModel.InvoiceProducts.Add(new InvoiceProduct
            {
                ProductId = selectedPOSubject.ProductId,
                Quantity = selectedPOSubject.Quantity,
                UnitPrice = selectedPOSubject.PriceUnit,
                TotalProductAmount = selectedPOSubject.Quantity * selectedPOSubject.PriceUnit
            });
        }

        private List<InvoiceProductInfoDto> CalculateInternalInvoiceProductByReceiptReject(WarehouseDespatch rejectModel, List<POSubject> pOSubjects)
        {
            var result = new List<InvoiceProductInfoDto>();
            foreach (var rejectSubject in rejectModel.WarehouseProductStockLogs)
            {
                var selectedPOSubject = pOSubjects
                    .Where(a => a.ProductId == rejectSubject.ProductId)
                    .FirstOrDefault();

                var newInvoiceProduct = new InvoiceProductInfoDto();

                newInvoiceProduct = new InvoiceProductInfoDto
                {
                    ProductCode = selectedPOSubject.Product.ProductCode,
                    ProductId = selectedPOSubject.ProductId,
                    ProductName = selectedPOSubject.Product.Description,
                    ProductUnit = selectedPOSubject.Product.Unit,
                    UnitPrice = selectedPOSubject.PriceUnit,
                    Quantity = rejectSubject.Output,
                    TotalProductAmount = rejectSubject.Output * selectedPOSubject.PriceUnit
                };

                result.Add(newInvoiceProduct);
            }

            return result;

        }

        //private List<InvoiceProductInfoDto> CalculateForeignInvoiceProductByTransference(PO poModel, ReceiptReject transferenceModel, List<InvoiceProduct> invoiceProducts)
        //{
        //    var result = new List<InvoiceProductInfoDto>();
        //    foreach (var transferenceProduct in transferenceModel.ReceiptRejectSubjects)
        //    {
        //        var invoiceProductsOfThis = invoiceProducts
        //            .Where(a => a.ProductId == transferenceProduct.ProductId)
        //            .ToList();

        //        bool isDone = false;
        //        decimal thisProductTransferenceQuantity = transferenceProduct.Quantity;
        //        foreach (var item in invoiceProductsOfThis)
        //        {
        //            var newInvoiceProduct = new InvoiceProductInfoDto();
        //            if (isDone)
        //                continue;
        //            if (thisProductTransferenceQuantity <= item.Quantity)
        //            {
        //                newInvoiceProduct.ProductCode = transferenceProduct.Product.ProductCode;
        //                newInvoiceProduct.ProductId = transferenceProduct.ProductId;
        //                newInvoiceProduct.ProductName = transferenceProduct.Product.Description;
        //                newInvoiceProduct.ProductUnit = transferenceProduct.Product.Unit;
        //                newInvoiceProduct.Quantity = thisProductTransferenceQuantity;
        //                newInvoiceProduct.UnitPrice = item.UnitPrice;
        //                //newInvoiceProduct.TechnicalNumber = transferenceProduct.Product.TechnicalNumber;
        //                //newInvoiceProduct.TotalProductAmount = newInvoiceProduct.Quantity * item.UnitPrice;
        //                //newInvoiceProduct.UnitPriceIRR = item.UnitPriceIRR;
        //                //newInvoiceProduct.DiscountIRR = item.DiscountIRR;
        //                //newInvoiceProduct.TotalProductAmountIRR = thisProductTransferenceQuantity * item.UnitPriceIRR;
        //                //newInvoiceProduct.FinalTotalPeymentAmountIRR = newInvoiceProduct.TotalProductAmountIRR - newInvoiceProduct.DiscountIRR;
        //                isDone = true;
        //            }
        //            else
        //            {
        //                thisProductTransferenceQuantity = thisProductTransferenceQuantity - item.Quantity;
        //                newInvoiceProduct.ProductCode = transferenceProduct.Product.ProductCode;
        //                newInvoiceProduct.ProductId = transferenceProduct.ProductId;
        //                newInvoiceProduct.ProductName = transferenceProduct.Product.Description;
        //                newInvoiceProduct.ProductUnit = transferenceProduct.Product.Unit;
        //                //newInvoiceProduct.TechnicalNumber = transferenceProduct.Product.TechnicalNumber;
        //                //newInvoiceProduct.TotalProductAmount = newInvoiceProduct.Quantity * item.UnitPrice;
        //                newInvoiceProduct.Quantity = thisProductTransferenceQuantity;
        //                newInvoiceProduct.UnitPrice = item.UnitPrice;
        //                //newInvoiceProduct.UnitPriceIRR = item.UnitPriceIRR;
        //                //newInvoiceProduct.DiscountIRR = item.DiscountIRR;
        //                //newInvoiceProduct.TotalProductAmountIRR = thisProductTransferenceQuantity * item.UnitPriceIRR;
        //                //newInvoiceProduct.FinalTotalPeymentAmountIRR = newInvoiceProduct.TotalProductAmountIRR - newInvoiceProduct.DiscountIRR;

        //            }
        //            result.Add(newInvoiceProduct);
        //        }
        //    }
        //    return result;
        //}


        private List<InvoiceProduct> CalculateInternalInvoiceProductByTransferenceForAddNewInvoice(WarehouseDespatch receiptRejectModel, List<POSubject> poSubjects)
        {
            var result = new List<InvoiceProduct>();
            foreach (var rejectProduct in receiptRejectModel.WarehouseProductStockLogs)
            {
                var selectedPOSubject = poSubjects
                    .Where(a => a.ProductId == rejectProduct.ProductId)
                    .FirstOrDefault();

                var newInvoiceProduct = new InvoiceProduct();

                newInvoiceProduct.ProductId = selectedPOSubject.ProductId;
                newInvoiceProduct.Quantity = rejectProduct.Output;
                newInvoiceProduct.UnitPrice = selectedPOSubject.PriceUnit;
                newInvoiceProduct.TotalProductAmount = rejectProduct.Output * selectedPOSubject.PriceUnit;

                result.Add(newInvoiceProduct);
            }

            return result;
        }

        //private List<InvoiceProduct> CalculateForeignInvoiceProductByTransferenceForAddNewInvoice(PO poModel, ReceiptReject transferenceModel, List<InvoiceProduct> invoiceProducts, decimal tax)
        //{
        //    var result = new List<InvoiceProduct>();
        //    foreach (var transferenceProduct in transferenceModel.ReceiptRejectSubjects)
        //    {
        //        var invoiceProductsOfThis = invoiceProducts
        //            .Where(a => a.ProductId == transferenceProduct.ProductId)
        //            .ToList();

        //        bool isDone = false;
        //        decimal thisProductTransferenceQuantity = transferenceProduct.Quantity;
        //        foreach (var item in invoiceProductsOfThis)
        //        {
        //            var newInvoiceProduct = new InvoiceProduct();
        //            if (isDone)
        //                continue;
        //            if (thisProductTransferenceQuantity <= item.Quantity)
        //            {
        //                newInvoiceProduct.ProductId = transferenceProduct.ProductId;
        //                newInvoiceProduct.Quantity = thisProductTransferenceQuantity;
        //                newInvoiceProduct.UnitPrice = item.UnitPrice;
        //                //newInvoiceProduct.TotalProductAmount = newInvoiceProduct.Quantity * item.UnitPrice;
        //                //newInvoiceProduct.UnitPriceIRR = item.UnitPriceIRR;
        //                //newInvoiceProduct.DiscountIRR = item.DiscountIRR;
        //                //newInvoiceProduct.TotalProductAmountIRR = thisProductTransferenceQuantity * item.UnitPriceIRR;
        //                //newInvoiceProduct.FinalTotalPeymentAmountIRR = newInvoiceProduct.TotalProductAmountIRR - newInvoiceProduct.DiscountIRR;

        //                isDone = true;
        //            }
        //            else
        //            {
        //                thisProductTransferenceQuantity = thisProductTransferenceQuantity - item.Quantity;
        //                newInvoiceProduct.ProductId = transferenceProduct.ProductId;
        //                newInvoiceProduct.Quantity = thisProductTransferenceQuantity;
        //                newInvoiceProduct.UnitPrice = item.UnitPrice;
        //                //newInvoiceProduct.TotalProductAmount = newInvoiceProduct.Quantity * item.UnitPrice;
        //                //newInvoiceProduct.UnitPriceIRR = item.UnitPriceIRR;
        //                //newInvoiceProduct.DiscountIRR = item.DiscountIRR;
        //                //newInvoiceProduct.TotalProductAmountIRR = thisProductTransferenceQuantity * item.UnitPriceIRR;
        //                //newInvoiceProduct.FinalTotalPeymentAmountIRR = newInvoiceProduct.TotalProductAmountIRR - newInvoiceProduct.DiscountIRR;

        //            }
        //            result.Add(newInvoiceProduct);
        //        }
        //    }
        //    return result;
        //}


        //public bool UpdatePRContractSubjectByTransference(PContractType pContractType, IEnumerable<InvoiceProduct> invoiceProducts, List<PRContractSubject> prContractSubjects)
        //{
        //    foreach (var item in invoiceProducts)
        //    {
        //        var selected = new PRContractSubject();
        //        if (pContractType == PContractType.Internal)
        //            selected = prContractSubjects.FirstOrDefault(a => a.ProductId == item.ProductId /*&& a.UnitPrice == item.UnitPriceIRR*/);
        //        else
        //            selected = prContractSubjects.FirstOrDefault(a => a.ProductId == item.ProductId && a.UnitPrice == item.UnitPrice);

        //        if (selected != null)
        //            selected.RemainedQuantityToInvoice += item.Quantity;
        //        else return false;
        //    }
        //    return true;
        //}

        public async Task<DownloadFileDto> DownloadInvoiceAttachmentAsync(AuthenticateDto authenticate, long invoiceId, string fileSrc)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return null;

                var dbQuery = _invoiceRepository
                    .Where(a => !a.IsDeleted &&
                    a.PO.BaseContractCode == authenticate.ContractCode &&
                    a.Id == invoiceId &&
                    a.Attachments.Any(c => !c.IsDeleted && c.FileSrc == fileSrc));

                var contractCode = await dbQuery
                    .Select(c => c.PO.BaseContractCode)
                    .FirstOrDefaultAsync();

                if (contractCode == null)
                    return null;
                var attachment = await dbQuery.Select(a => a.Attachments.Where(b => b.FileSrc == fileSrc && !b.IsDeleted).FirstOrDefault()).FirstOrDefaultAsync();
                var streamResult = await _fileHelper.DownloadAttachmentDocument(fileSrc, ServiceSetting.FileSection.Invoice, attachment.FileName);
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