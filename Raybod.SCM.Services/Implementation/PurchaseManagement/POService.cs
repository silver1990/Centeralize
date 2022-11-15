using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Exon.TheWeb.Service.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Raybod.SCM.DataAccess.Core;
using Raybod.SCM.DataAccess.Extention;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Audit;
using Raybod.SCM.DataTransferObject.Notification;
using Raybod.SCM.DataTransferObject.PO;
using Raybod.SCM.DataTransferObject.User;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Model;
using Raybod.SCM.Domain.Struct;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Utility.Common;
using Raybod.SCM.Utility.EnumType;
using Raybod.SCM.Utility.Extention;
using Raybod.SCM.Utility.Utility;

namespace Raybod.SCM.Services.Implementation
{
    public class POService : IPOService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentService _paymentService;
        private readonly ISCMLogAndNotificationService _scmLogAndNotificationService;
        private readonly IContractFormConfigService _formConfigService;
        private readonly DbSet<PRContract> _prContractRepository;
        private readonly DbSet<PO> _poRepository;
        private readonly DbSet<POSubject> _poSubjectRepository;
        private readonly DbSet<Receipt> _receiptRepository;
        private readonly DbSet<PAttachment> _pAttachmentRepository;
        private readonly DbSet<PurchaseRequestItem> _purchaseRequestItemRepository;
        private readonly ITeamWorkAuthenticationService _authenticationServices;
        private readonly IFileService _fileService;
        private readonly Utilitys.FileHelper _fileHelper;
        private readonly CompanyConfig _appSettings;

        public POService(
            IUnitOfWork unitOfWork,
            IWebHostEnvironment hostingEnvironmentRoot,
            IOptions<CompanyAppSettingsDto> appSettings,
            ITeamWorkAuthenticationService authenticationServices,
            IPaymentService paymentService,
            ISCMLogAndNotificationService scmLogAndNotificationService,
            IFileService fileService,
            IHttpContextAccessor httpContextAccessor,
            IContractFormConfigService formConfigService)
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
            _paymentService = paymentService;
            _authenticationServices = authenticationServices;
            _scmLogAndNotificationService = scmLogAndNotificationService;
            _formConfigService = formConfigService;
            _poRepository = _unitOfWork.Set<PO>();
            _poSubjectRepository = _unitOfWork.Set<POSubject>();
            _receiptRepository = _unitOfWork.Set<Receipt>();
            _prContractRepository = _unitOfWork.Set<PRContract>();
            _pAttachmentRepository = _unitOfWork.Set<PAttachment>();
            _purchaseRequestItemRepository = _unitOfWork.Set<PurchaseRequestItem>();
            _fileHelper = new Utilitys.FileHelper(hostingEnvironmentRoot);
            httpContextAccessor.HttpContext.Request.Headers.TryGetValue("companyCode", out var CompanyCode);
            _appSettings = appSettings.Value.CompanyConfig.First(a => a.CompanyCode == CompanyCode);
        }

        public async Task<POListBadgeDto> GetDashbourdPOListBadgeAsync(AuthenticateDto authenticate)
        {
            var result = new POListBadgeDto();
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return result;

                var dbQuery = _poRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted &&
                    a.BaseContractCode == authenticate.ContractCode);

                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.ProductGroupIds.Contains(a.ProductGroupId));

                result.PenddingPo = dbQuery.Count(a => a.POStatus == POStatus.Pending);

                result.InprogressPo = await dbQuery.CountAsync(a => a.POStatus > POStatus.Pending && a.POStatus < POStatus.Delivered);

                return result;
            }
            catch (Exception exception)
            {
                return result;
            }
        }

        //todo: smehdi most be review 
        public async Task<ServiceResult<List<PO>>> AddPOToPendingByPRContractAsync(PRContract prContract)
        {
            try
            {
                var prcSubjectProductIds = prContract.PRContractSubjects
                    .Where(a => !a.IsDeleted)
                    .Select(a => a.ProductId)
                    .ToList();

                var rfpItemPrItemIds = prContract.PRContractSubjects.Where(a => !a.IsDeleted)
                    .Select(a => a.RFPItem.PurchaseRequestItemId)
                    .ToList();

                var selectedItemPrs = await _purchaseRequestItemRepository
                    .Where(a => rfpItemPrItemIds.Contains(a.Id))
                    .Select(c => new
                    {
                        productId = c.ProductId,
                        mrpId = c.PurchaseRequest.MrpId,
                        prItemId = c.Id,
                        mrpItemId = c.PurchaseRequest.Mrp.MrpItems.FirstOrDefault(v => !v.IsDeleted && v.ProductId == c.ProductId).Id,
                        dateEnd = c.DateEnd.Date,
                        quantity = c.Quntity,
                    })
                    .ToListAsync();

                if (selectedItemPrs == null)
                    return ServiceResultFactory.CreateError<List<PO>>(null, MessageId.DataInconsistency);

                var selectedItemGroupByMrp = selectedItemPrs.GroupBy(c => c.mrpId).ToList();

                var result = new List<PO>();

                foreach (var item in selectedItemGroupByMrp)
                {
                    var poModel = new PO
                    {
                        IsDeleted = false,
                        SupplierId = prContract.SupplierId,
                        BaseContractCode = prContract.BaseContractCode,
                        POStatus = POStatus.Pending,
                        CurrencyType = prContract.CurrencyType,
                        Tax = prContract.Tax,
                        PContractType = prContract.PContractType,
                        ProductGroupId = prContract.ProductGroupId,
                        PRContractId = prContract.Id,
                        DateDelivery = item.Select(c => c.dateEnd).OrderByDescending(c => c).First(),
                        PORefType = PORefType.PRContract,
                        DeliveryLocation = prContract.DeliveryLocation,
                        POSubjects = new List<POSubject>(),
                        POTermsOfPayments = new List<POTermsOfPayment>(),
                    };

                    foreach (var rfpItem in item.ToList())
                    {
                        var prcSubjectForThisProduct = prContract.PRContractSubjects
                            .Where(a => !a.IsDeleted && a.ProductId == rfpItem.productId && a.RemainedStock > 0)
                            .FirstOrDefault();

                        if (prcSubjectForThisProduct == null)
                            return ServiceResultFactory.CreateError(new List<PO>(), MessageId.DataInconsistency);
                        //todo
                        var purchaseItemOrder = rfpItem.quantity;
                        if (prcSubjectForThisProduct.RemainedStock < rfpItem.quantity)
                        {
                            purchaseItemOrder = prcSubjectForThisProduct.RemainedStock;
                        }

                        var poSubject = new POSubject();

                        poSubject.Quantity = purchaseItemOrder;
                        prcSubjectForThisProduct.ReservedStock += purchaseItemOrder;
                        prcSubjectForThisProduct.RemainedStock -= purchaseItemOrder;

                        poSubject.ProductId = prcSubjectForThisProduct.ProductId;
                        poSubject.PriceUnit = prcSubjectForThisProduct.UnitPrice;
                        poSubject.MrpItemId = rfpItem.mrpItemId;
                        poSubject.RemainedQuantity = poSubject.Quantity;
                        poModel.POSubjects.Add(poSubject);
                    }

                    CalculatePOAmount(poModel.Tax, poModel);


                    result.Add(poModel);
                }
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception e)
            {
                return ServiceResultFactory.CreateException(new List<PO>(), e);
            }
        }
        public async Task<ServiceResult<List<PO>>> AddPOToConfirmedByPRContractAsync(PRContract prContract, List<RFPItems> rfpItems)
        {
            try
            {
                var prcSubjectProductIds = prContract.PRContractSubjects
                    .Where(a => !a.IsDeleted)
                    .Select(a => a.ProductId)
                    .ToList();
                var rpfItemIds = prContract.PRContractSubjects
                    .Select(a => a.RFPItemId)
                    .ToList();


                var selectedItemPrs = rfpItems
                    .Where(a => rpfItemIds.Contains(a.Id))
                    .Select(c => new
                    {
                        productId = c.ProductId,
                        mrpId = c.PurchaseRequestItem.PurchaseRequest.MrpId,
                        prItemId = c.Id,
                        mrpItemId = c.PurchaseRequestItem.MrpItem.Id,
                        dateEnd = c.DateEnd.Date,
                        quantity = c.PurchaseRequestItem.Quntity,
                    })
                    .ToList();

                if (selectedItemPrs == null)
                    return ServiceResultFactory.CreateError<List<PO>>(null, MessageId.DataInconsistency);

                var selectedItemGroupByMrp = selectedItemPrs.GroupBy(c => c.mrpId).ToList();

                var result = new List<PO>();

                foreach (var item in selectedItemGroupByMrp)
                {
                    var poModel = new PO
                    {
                        IsDeleted = false,
                        SupplierId = prContract.SupplierId,
                        BaseContractCode = prContract.BaseContractCode,
                        POStatus = POStatus.Pending,
                        CurrencyType = prContract.CurrencyType,
                        Tax = prContract.Tax,
                        PContractType = prContract.PContractType,
                        ProductGroupId = prContract.ProductGroupId,
                        PRContract = prContract,
                        DateDelivery = item.Select(c => c.dateEnd).OrderByDescending(c => c).First(),
                        PORefType = PORefType.PRContract,
                        DeliveryLocation = prContract.DeliveryLocation,
                        POSubjects = new List<POSubject>(),
                        POTermsOfPayments = new List<POTermsOfPayment>(),
                    };

                    foreach (var rfpItem in item.ToList())
                    {
                        var prcSubjectForThisProduct = prContract.PRContractSubjects
                            .Where(a => !a.IsDeleted && a.ProductId == rfpItem.productId && a.RemainedStock > 0)
                            .FirstOrDefault();

                        if (prcSubjectForThisProduct == null)
                            return ServiceResultFactory.CreateError(new List<PO>(), MessageId.DataInconsistency);
                        //todo
                        var purchaseItemOrder = rfpItem.quantity;
                        if (prcSubjectForThisProduct.RemainedStock < rfpItem.quantity)
                        {
                            purchaseItemOrder = prcSubjectForThisProduct.RemainedStock;
                        }

                        var poSubject = new POSubject();

                        poSubject.Quantity = purchaseItemOrder;
                        prcSubjectForThisProduct.ReservedStock += purchaseItemOrder;
                        prcSubjectForThisProduct.RemainedStock -= purchaseItemOrder;

                        poSubject.ProductId = prcSubjectForThisProduct.ProductId;
                        poSubject.PriceUnit = prcSubjectForThisProduct.UnitPrice;
                        poSubject.MrpItemId = rfpItem.mrpItemId;
                        poSubject.RemainedQuantity = poSubject.Quantity;
                        poModel.POSubjects.Add(poSubject);
                    }

                    CalculatePOAmount(poModel.Tax, poModel);


                    result.Add(poModel);
                }
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception e)
            {
                return ServiceResultFactory.CreateException(new List<PO>(), e);
            }
        }

        //todo: smehdi most be review 
        public ServiceResult<PO> AddPOToPendingByMRP(Mrp mrpModel, MrpItem mrpItemModel, DateTime dateDelivery, AddPOByMrpDto addPoModel, PRContract prContractModel)
        {
            try
            {
                var result = new PO();
                //prContractModel.TermsOfPayments == null ||
                //  !prContractModel.TermsOfPayments.Any(a => !a.IsDeleted)
                if (prContractModel == null)
                    return ServiceResultFactory.CreateError<PO>(null, MessageId.InputDataValidationError);

                if (prContractModel.PRContractSubjects == null ||
                    !prContractModel.PRContractSubjects.Any(a => !a.IsDeleted))
                    return ServiceResultFactory.CreateError<PO>(null, MessageId.InputDataValidationError);

                var prContractSubject = prContractModel.PRContractSubjects
                    .Where(a => !a.IsDeleted && a.ProductId == addPoModel.ProductId)
                    .FirstOrDefault();

                if (prContractSubject == null || prContractSubject.RemainedStock < addPoModel.OrderAmount)
                    return ServiceResultFactory.CreateError<PO>(null, MessageId.DataInconsistency);

                var poModel = new PO
                {
                    IsDeleted = false,
                    SupplierId = prContractModel.SupplierId,
                    BaseContractCode = mrpModel.ContractCode,
                    POStatus = POStatus.Pending,
                    CurrencyType = prContractModel.CurrencyType,
                    DeliveryLocation = prContractModel.DeliveryLocation,
                    PContractType = prContractModel.PContractType,
                    Tax = prContractModel.Tax,
                    ProductGroupId = mrpModel.ProductGroupId,
                    PRContractId = prContractModel.Id,
                    DateDelivery = dateDelivery,
                    PORefType = PORefType.MRP,
                    POSubjects = new List<POSubject>(),
                    POTermsOfPayments = new List<POTermsOfPayment>(),
                };

                // import term Of Payment
                //var poTermOfPayment = prContractModel.TermsOfPayments.Select(a => new POTermsOfPayment
                //{
                //    CreditDuration = a.CreditDuration,
                //    IsDeleted = false,
                //    IsCreditPayment = a.IsCreditPayment,
                //    PaymentPercentage = a.PaymentPercentage,
                //    PaymentStep = a.PaymentStep,
                //}).ToList();

                //poModel.POTermsOfPayments = poTermOfPayment;

                var poSubject = new POSubject();

                poSubject.Quantity = addPoModel.OrderAmount;
                prContractSubject.ReservedStock += addPoModel.OrderAmount;
                prContractSubject.RemainedStock -= addPoModel.OrderAmount;
                poSubject.MrpItem = mrpItemModel;
                poSubject.ProductId = prContractSubject.ProductId;
                poSubject.PriceUnit = prContractSubject.UnitPrice;
                poSubject.RemainedQuantity = poSubject.Quantity;

                poModel.POSubjects.Add(poSubject);
                CalculatePOAmount(poModel.Tax, poModel);
                return ServiceResultFactory.CreateSuccess(poModel);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<PO>(null, exception);
            }
        }

        private static void CalculatePOAmount(decimal tax, PO poModel)
        {
            var totalAmount = poModel.POSubjects.Sum(a => a.PriceTotal);


            poModel.TotalAmount = totalAmount;
            if (tax <= 0)
                poModel.FinalTotalAmount = totalAmount;
            else
                poModel.FinalTotalAmount = totalAmount + (totalAmount * (tax / 100));
        }

        public async Task<ServiceResult<POListBadgeDto>> GetPOListBadgeAsync(AuthenticateDto authenticate)
        {
            var result = new POListBadgeDto();
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<POListBadgeDto>(null, MessageId.AccessDenied);

                var dbQuery = _poRepository.AsNoTracking()
                    .Where(a => !a.IsDeleted && a.BaseContractCode == authenticate.ContractCode);

                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.ProductGroupIds.Contains(a.ProductGroupId));

                result.PenddingPo = dbQuery.Count(a => !a.IsDeleted && a.POStatus == POStatus.Pending);

                result.InprogressPo = dbQuery.Count(a => !a.IsDeleted && a.POStatus > POStatus.Pending && a.POStatus < POStatus.Delivered);

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<POListBadgeDto>(null, exception);
            }
        }

        public async Task<ServiceResult<List<POPendingSubjectListDto>>> GetPOPendingSubjectByPRContractIdAsync(AuthenticateDto authenticate, long prContractId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<POPendingSubjectListDto>>(null, MessageId.AccessDenied);

                var dbQuery = _poSubjectRepository
                    .AsNoTracking()
                    .Where(a => !a.PO.IsDeleted && a.PO.BaseContractCode == authenticate.ContractCode && a.ParentSubject == null && a.PO.PRContractId == prContractId && a.PO.POStatus == POStatus.Pending && a.RemainedQuantity > 0);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<List<POPendingSubjectListDto>>(null, MessageId.EntityDoesNotExist);

                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.ProductGroupIds.Contains(a.PO.ProductGroupId));

                var result = await dbQuery.Select(a => new POPendingSubjectListDto
                {
                    POId = a.POId.Value,
                    ProductId = a.ProductId,
                    MrpItemId = a.MrpItemId.Value,
                    ProductName = a.Product.Description,
                    ProductCode = a.Product.ProductCode,
                    MrpCode = a.MrpItem.Mrp.MrpNumber,
                    DateEnd = a.PO.PRContract.PRContractSubjects.Where(c => c.ProductId == a.ProductId).Select(c => c.RFPItem.DateEnd).FirstOrDefault().ToUnixTimestamp(),
                    ProductGroupName = a.Product.ProductGroup.Title,
                    TechnicalNumber = a.Product.TechnicalNumber,
                    ProductUnit = a.Product.Unit,
                    PriceUnit = a.PriceUnit,
                    Quantity = a.RemainedQuantity
                }).ToListAsync();
                List<POPendingSubjectListDto> pOPendingListDtos = new List<POPendingSubjectListDto>();
                foreach (var item in result)
                {
                    if (!pOPendingListDtos.Any(a => a.ProductId == item.ProductId && a.POId == item.POId))
                    {
                        pOPendingListDtos.Add(new POPendingSubjectListDto
                        {
                            POId = item.POId,
                            ProductId = item.ProductId,
                            MrpItemId = item.MrpItemId,
                            ProductName = item.ProductName,
                            ProductCode = item.ProductCode,
                            MrpCode = item.MrpCode,
                            DateEnd = item.DateEnd,
                            ProductGroupName = item.ProductGroupName,
                            TechnicalNumber = item.TechnicalNumber,
                            ProductUnit = item.ProductUnit,
                            PriceUnit = item.PriceUnit,
                            Quantity = result.Where(a => a.ProductId == item.ProductId && a.POId == item.POId).Sum(a => a.Quantity)
                        });
                    }
                }
                return ServiceResultFactory.CreateSuccess(pOPendingListDtos);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<POPendingSubjectListDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<POPendingInfoDto>> GetPOPendingByPOIdAsync(AuthenticateDto authenticate, long poId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<POPendingInfoDto>(null, MessageId.AccessDenied);

                var dbQuery = _poRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.POId == poId && a.POStatus == POStatus.Pending);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<POPendingInfoDto>(null, MessageId.EntityDoesNotExist);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(c => permission.ProductGroupIds.Contains(c.ProductGroupId)))
                    return ServiceResultFactory.CreateError<POPendingInfoDto>(null, MessageId.AccessDenied);

                var result = await dbQuery.Select(po => new POPendingInfoDto
                {
                    POId = po.POId,
                    PRContractId = po.PRContractId,
                    TotalAmount = po.TotalAmount,
                    FinalTotalAmount = po.FinalTotalAmount,
                    CurrencyType = po.PRContract.CurrencyType,
                    DateDelivery = po.DateDelivery.ToUnixTimestamp(),
                    POCode = po.POCode,
                    PContractType = po.PRContract.PContractType,
                    PRContractDateIssued = po.PRContract.DateIssued.ToUnixTimestamp(),
                    PRContractDateEnd = po.PRContract.DateEnd.ToUnixTimestamp(),
                    POStatus = po.POStatus,
                    Tax = po.Tax,
                    ProductGroup = po.ProductGroup.Title,

                    PRContractCode = po.PRContract.PRContractCode,
                    DeliveryLocation = po.DeliveryLocation,
                    SupplierName = po.Supplier.Name,
                    SupplierCode = po.Supplier.Name,
                    SupplierLogo = po.Supplier.Logo != null ? _appSettings.ClientHost + ServiceSetting.UploadImagesPath.LogoSmall + po.Supplier.Logo : null,
                    POTermsOfPayments = po.PRContract.TermsOfPayments.Where(a => !a.IsDeleted)
                        .Select(c => new EditPOTermsOfPaymentDto
                        {
                            Id = c.Id,
                            CreditDuration = c.CreditDuration,
                            PaymentPercentage = c.PaymentPercentage,
                            IsCreditPayment = c.IsCreditPayment,
                            PaymentStep = c.PaymentStep,
                            POId = c.Id
                        }).ToList(),
                    UserAudit = po.AdderUser != null
                        ? new UserAuditLogDto
                        {
                            AdderUserId = po.AdderUserId,
                            AdderUserName = po.AdderUser.FullName,
                            CreateDate = po.CreatedDate.ToUnixTimestamp(),
                            AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall +
                                             po.AdderUser.Image
                        }
                        : null,
                    POSubjects = po.POSubjects
                    .Where(a => a.ParentSubject == null)
                    .Select(s => new POPendingSubjectListDto
                    {
                        ProductCode = s.Product.ProductCode,
                        ProductGroupName = s.Product.ProductGroup.Title,
                        ProductName = s.Product.Description,
                        ProductUnit = s.Product.Unit,
                        TechnicalNumber = s.Product.TechnicalNumber,
                        ProductId = s.ProductId,
                        Quantity = s.RemainedQuantity,
                        PriceUnit = s.PriceUnit,
                        DateEnd = s.MrpItem.DateEnd.ToUnixTimestamp(),
                        MrpCode = (s.MrpItem.BomProduct.IsRequiredMRP) ? s.MrpItem.Mrp.MrpNumber : "",
                        MrpItemId = s.MrpItemId.Value,
                        POId = s.POId.Value
                    }).ToList(),

                }).FirstOrDefaultAsync();
                List<POPendingSubjectListDto> pOPendingSubjectLists = new List<POPendingSubjectListDto>();

                foreach (var item in result.POSubjects)
                {
                    if (!pOPendingSubjectLists.Any(a => a.ProductId == item.ProductId && a.POId == item.POId))
                    {
                        pOPendingSubjectLists.Add(new POPendingSubjectListDto
                        {
                            ProductCode = item.ProductCode,
                            ProductGroupName = item.ProductGroupName,
                            ProductName = item.ProductName,
                            ProductUnit = item.ProductUnit,
                            TechnicalNumber = item.TechnicalNumber,
                            ProductId = item.ProductId,
                            Quantity = result.POSubjects.Where(a => a.ProductId == item.ProductId && a.POId == item.POId).Sum(a => a.Quantity),
                            PriceUnit = item.PriceUnit,
                            DateEnd = item.DateEnd,
                            MrpCode = item.MrpCode,
                            MrpItemId = item.MrpItemId,
                            POId = item.POId
                        });
                    }
                }
                result.POSubjects = pOPendingSubjectLists;
                result.MrpCode = (result.POSubjects != null && result.POSubjects.Any()) ? result.POSubjects.First().MrpCode : "";
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<POPendingInfoDto>(null, exception);
            }
        }

        public async Task<ServiceResult<bool>> AddPOAsync(AuthenticateDto authenticate, long prContractId, AddPODto model)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery = _prContractRepository
                    .Where(a => a.Id == prContractId && a.BaseContractCode == authenticate.ContractCode)
                    .Include(a => a.Supplier)
                    .Include(a => a.TermsOfPayments)
                    .Include(a => a.PRContractSubjects)
                    .AsQueryable();

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(c => permission.ProductGroupIds.Contains(c.ProductGroupId)))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var prContractModel = await dbQuery
                    .FirstOrDefaultAsync();

                if (prContractModel == null ||
                    prContractModel.PRContractSubjects == null ||
                    !prContractModel.PRContractSubjects.Any())
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (model.DateDelivery <= 0)
                    return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                if(model.DateDelivery.UnixTimestampToDateTime()<prContractModel.DateIssued)
                    return ServiceResultFactory.CreateError(false, MessageId.DeliverDateCantBelessThenContractStartDate);
                if (model.POSubjects == null || !model.POSubjects.Any())
                    return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                if (model.POSubjects.Any(a => a.Quantity <= 0))
                    return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                if (model.POSubjects.GroupBy(a => a.ProductId).Any(c => c.Count() > 1))
                    return ServiceResultFactory.CreateError(false, MessageId.ImpossibleDuplicateProduct);

                var postedProductIds = model.POSubjects.Select(a => a.ProductId).Distinct().ToList();

                var postedpoIds = model.POSubjects.Select(a => a.POId)
                    .Distinct()
                    .ToList();

                var poModels = await _poRepository.Where(a => !a.IsDeleted && a.POStatus == POStatus.Pending &&
                postedpoIds.Contains(a.POId))
                    .Include(a => a.POSubjects)
                    .ThenInclude(a => a.MrpItem)
                    .ToListAsync();

                if (poModels == null || poModels.Count() != postedpoIds.Count())
                    return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                var poSubjectModels = new List<POSubject>();
                decimal neededQuantity = 0;
                foreach (var item in model.POSubjects)
                {
                    var selectedpo = poModels
                        .Where(a => a.POId == item.POId && a.POSubjects.Any(s => s.ProductId == item.ProductId && s.MrpItemId == item.MrpItemId))
                        .FirstOrDefault();

                    if (selectedpo == null)
                        return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                    var selectedPOSubject = selectedpo.POSubjects.Where(s => s.ProductId == item.ProductId).ToList();
                    if (selectedPOSubject.Sum(a => a.RemainedQuantity) <= 0 || selectedPOSubject.Sum(a => a.RemainedQuantity) < item.Quantity)
                        return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                    neededQuantity = item.Quantity;

                    var prSubjectModel = prContractModel.PRContractSubjects.Where(a => a.ProductId == item.ProductId).ToList();
                    if (prSubjectModel == null)
                        return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);
                    foreach (var pOSubject in selectedPOSubject)
                    {
                        if (neededQuantity > 0)
                        {

                            if (pOSubject.MrpItem.MrpItemStatus < MrpItemStatus.PO)
                                pOSubject.MrpItem.MrpItemStatus = MrpItemStatus.PO;

                            poSubjectModels.Add(new POSubject
                            {
                                MrpItemId = pOSubject.MrpItemId,
                                PriceUnit = pOSubject.PriceUnit,
                                ProductId = pOSubject.ProductId,
                                Quantity = (neededQuantity >= pOSubject.Quantity) ? pOSubject.Quantity : neededQuantity,
                                RemainedQuantity = (neededQuantity >= pOSubject.Quantity) ? pOSubject.Quantity : neededQuantity,

                            });

                            pOSubject.RemainedQuantity -= (neededQuantity >= pOSubject.Quantity) ? pOSubject.Quantity : neededQuantity;
                            if (pOSubject.RemainedQuantity <= 0)
                                selectedpo.POSubjects.Remove(pOSubject);

                            neededQuantity -= pOSubject.Quantity;
                        }

                    }

                }




                var freePOModel = poModels.Where(a => a.POSubjects == null || !a.POSubjects.Any()).FirstOrDefault();
                if (freePOModel == null)
                {
                    freePOModel = await AddNewPOModelAsync(model, prContractModel, poSubjectModels);
                }
                else
                {
                    AddPOModelUseFreePOModel(freePOModel, poModels, prContractModel, poSubjectModels);
                }

                CalculatePOAmount(freePOModel.Tax, freePOModel);
                // generate form code
                var count = await _poRepository.CountAsync(a => a.POStatus != POStatus.Pending && a.BaseContractCode == authenticate.ContractCode);
                var codeRes = await _formConfigService.GenerateFormCodeAsync(authenticate.ContractCode, FormName.PO, count);
                if (!codeRes.Succeeded)
                    return ServiceResultFactory.CreateError(false, codeRes.Messages.First().Message);
                freePOModel.POCode = codeRes.Result;

                freePOModel.POStatusLogs = new List<POStatusLog>();
                freePOModel.POStatusLogs.Add(new POStatusLog
                {
                    BeforeStatus = POStatus.Pending,
                    IsDone = true,
                    Status = POStatus.Approved
                });
                //var notifReceiptUserRole = new List<string>
                //{
                //        SCMRole.POPrepManagement,
                //        SCMRole.POPrepObserver,
                //};

                freePOModel.CreatedDate = DateTime.UtcNow;
                freePOModel.AdderUserId = authenticate.UserId;

                if (freePOModel.POId <= 0)
                    _poRepository.Add(freePOModel);

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {

                    //await _paymentService.AddPendingToPaymentBaseOnTermsOfApprovePOAsync(authenticate, freePOModel);
                    await AddLogAndTaskOnAddPOAsync(authenticate, prContractModel, postedpoIds, freePOModel);

                    return ServiceResultFactory.CreateSuccess(true);
                }

                return ServiceResultFactory.CreateError(false, MessageId.AddEntityFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        private async Task AddLogAndTaskOnAddPOAsync(AuthenticateDto authenticate, PRContract prContractModel, List<long> postedpoIds, PO freePOModel)
        {
            var poIds = await _poRepository
                .Where(a => !a.IsDeleted && a.POStatus == POStatus.Pending && postedpoIds.Contains(a.POId))
                .Select(c => c.POId)
                .ToListAsync();

            var donedPoIds = new List<string>();
            if (poIds.Any())
                donedPoIds = postedpoIds.Where(c => !poIds.Contains(c)).Select(a => a.ToString()).ToList();
            else
                donedPoIds = postedpoIds.Select(a => a.ToString()).ToList();

            if (donedPoIds.Any())
                await _scmLogAndNotificationService.SetDonedNotificationAsync(authenticate.UserId, authenticate.ContractCode, donedPoIds, NotifEvent.AddPOPending);

            var res = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
            {
                ContractCode = freePOModel.BaseContractCode,
                FormCode = freePOModel.POCode,
                Description = prContractModel.Supplier.Name,
                KeyValue = freePOModel.POId.ToString(),
                NotifEvent = NotifEvent.AddPO,
                ProductGroupId = freePOModel.ProductGroupId,
                RootKeyValue = freePOModel.POId.ToString(),
                PerformerUserId = authenticate.UserId,
                PerformerUserFullName = authenticate.UserFullName
            },
            freePOModel.ProductGroupId,
            new List<NotifToDto> {
                    new NotifToDto
                    {
                        NotifEvent= NotifEvent.AddPO,
                        Roles= new List<string>
                        {
                           SCMRole.POMng,
                        }
                    }
            });
        }

        private void AddPOModelUseFreePOModel(PO freePOModel, List<PO> poModels, PRContract prContractModel, List<POSubject> poSubjectModels)
        {
            freePOModel.POStatus = POStatus.Approved;
            freePOModel.POSubjects = new List<POSubject>();
            foreach (var item in poSubjectModels)
            {
                freePOModel.POSubjects.Add(item);
            }
            var removePOModels = poModels
                .Where(a => a.POId != freePOModel.POId && (a.POSubjects == null || !a.POSubjects.Any()))
                .ToList();

            foreach (var item in removePOModels)
            {
                _poRepository.Remove(item);
            }


            freePOModel.POTermsOfPayments = new List<POTermsOfPayment>();
            freePOModel.POTermsOfPayments = prContractModel.TermsOfPayments.Select(c => new POTermsOfPayment
            {
                CreditDuration = c.CreditDuration,
                IsCreditPayment = c.IsCreditPayment,
                PaymentPercentage = c.PaymentPercentage,
                PaymentStatus = c.PaymentStatus,
                PaymentStep = c.PaymentStep
            }).ToList();
        }

        private async Task<PO> AddNewPOModelAsync(AddPODto model, PRContract prContractModel, List<POSubject> poSubjects)
        {
            PO freePOModel = new PO
            {
                BaseContractCode = prContractModel.BaseContractCode,
                CurrencyType = prContractModel.CurrencyType,
                DateDelivery = model.DateDelivery.UnixTimestampToDateTime(),
                DeliveryLocation = prContractModel.DeliveryLocation,
                PContractType = prContractModel.PContractType,
                ProductGroupId = prContractModel.ProductGroupId,
                POStatus = POStatus.Approved,
                PRContractId = prContractModel.Id,
                SupplierId = prContractModel.SupplierId,
                Tax = prContractModel.Tax,
                POTermsOfPayments = new List<POTermsOfPayment>(),
                POSubjects = new List<POSubject>(),

            };

            foreach (var item in poSubjects)
            {
                freePOModel.POSubjects.Add(item);
            }



            freePOModel.POTermsOfPayments = prContractModel.TermsOfPayments.Select(c => new POTermsOfPayment
            {
                CreditDuration = c.CreditDuration,
                IsCreditPayment = c.IsCreditPayment,
                PaymentPercentage = c.PaymentPercentage,
                PaymentStatus = c.PaymentStatus,
                PaymentStep = c.PaymentStep
            }).ToList();
            await _poRepository.AddAsync(freePOModel);
            return freePOModel;
        }

        public async Task<ServiceResult<List<POPendingListDto>>> GetPOPendingAsync(AuthenticateDto authenticate,
            POQueryDto query)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<POPendingListDto>>(null, MessageId.AccessDenied);

                var dbQuery = _poRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.BaseContractCode == authenticate.ContractCode && a.POStatus == POStatus.Pending)
                    .AsQueryable();

                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(c => permission.ProductGroupIds.Contains(c.ProductGroupId));

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a => a.PRContract.PRContractCode.Contains(query.SearchText) ||
                      a.POSubjects.Any(c => c.RemainedQuantity > 0 && c.MrpItem.Mrp.MrpNumber.Contains(query.SearchText)));

                if (query.SupplierIds != null && query.SupplierIds.Any())
                    dbQuery = dbQuery.Where(a => query.SupplierIds.Contains(a.PRContract.SupplierId));

                if (query.ProductGroupIds != null && query.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(a => a.POSubjects.Any(c => c.RemainedQuantity > 0 && query.ProductGroupIds.Contains(c.Product.ProductGroupId)));

                if (query.ProductIds != null && query.ProductIds.Any())
                    dbQuery = dbQuery.Where(a => a.POSubjects.Any(c => c.RemainedQuantity > 0 && query.ProductIds.Contains(c.ProductId)));

                var pageCount = dbQuery.Count();

                var columnsMap = new Dictionary<string, Expression<Func<PO, object>>>
                {
                    ["POCode"] = v => v.POCode,
                    ["POId"] = v => v.POId,
                    ["CreatedDate"] = v => v.CreatedDate,
                };

                dbQuery = dbQuery.ApplayOrdering(query, columnsMap).ApplayPageing(query);

                var result = await dbQuery.Select(po => new POPendingListDto
                {
                    POId = po.POId,
                    DateDelivery = po.DateDelivery.ToUnixTimestamp(),
                    MrpCode = (po.POSubjects.Any(a => a.MrpItem.BomProduct.IsRequiredMRP)) ? po.POSubjects.Select(a => a.MrpItem.Mrp.MrpNumber).FirstOrDefault() : "",
                    PRContractCode = po.PRContract.PRContractCode,
                    PRContractDateIssued = po.PRContract.DateIssued.ToUnixTimestamp(),
                    PRContractDateEnd = po.PRContract.DateEnd.ToUnixTimestamp(),
                    Products = MergePoSubjects(po.POSubjects.Select(a => a.Product.Description).ToList()),
                    SupplierCode = po.Supplier.SupplierCode,
                    SupplierName = po.Supplier.Name,
                    SupplierLogo = _appSettings.ClientHost + ServiceSetting.UploadImagesPath.LogoSmall + po.Supplier.Logo,
                    UserAudit = po.AdderUser != null
                        ? new UserAuditLogDto
                        {
                            AdderUserId = po.AdderUserId,
                            AdderUserName = po.AdderUser.FullName,
                            CreateDate = po.CreatedDate.ToUnixTimestamp(),
                            AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall +
                                             po.AdderUser.Image
                        }
                        : null,
                }).ToListAsync();
                List<POPendingListDto> pOPendings = new List<POPendingListDto>();

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(pageCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new List<POPendingListDto>(), exception);
            }
        }

        public async Task<ServiceResult<List<ListPODto>>> GetDeliverdPOAsync(AuthenticateDto authenticate, POQueryDto query)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<ListPODto>>(null, MessageId.AccessDenied);

                var dbQuery = _poRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.BaseContractCode == authenticate.ContractCode && a.POStatus == POStatus.Delivered)
                    .AsQueryable();

                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(c => permission.ProductGroupIds.Contains(c.ProductGroupId));


                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a => a.Supplier.Name.Contains(query.SearchText) ||
                   a.Supplier.SupplierCode.Contains(query.SearchText) ||
                   a.POCode.Contains(query.SearchText) ||
                   a.PRContract.PRContractCode.Contains(query.SearchText) ||
                   a.POSubjects.Any(c => c.Product.Description.Contains(query.SearchText) ||
                   c.Product.ProductCode.Contains(query.SearchText)));

                var pageCount = dbQuery.Count();

                var columnsMap = new Dictionary<string, Expression<Func<PO, object>>>
                {
                    ["POCode"] = v => v.POCode,
                    ["Id"] = v => v.POId
                };

                dbQuery = dbQuery.ApplayOrdering(query, columnsMap).ApplayPageing(query);

                var result = await dbQuery.Select(po => new ListPODto
                {
                    POId = po.POId,
                    DateDelivery = po.DateDelivery.ToUnixTimestamp(),
                    POCode = po.POCode,
                    SupplierName = po.Supplier.Name,
                    Products = po.POSubjects.Select(a => a.Product.Description).ToList(),
                    CreateDate = po.CreatedDate.ToUnixTimestamp()
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(pageCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new List<ListPODto>(), exception);
            }
        }
        public async Task<ServiceResult<List<ListAllPODto>>> GetAllPOAsync(AuthenticateDto authenticate, POQueryDto query)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<ListAllPODto>>(null, MessageId.AccessDenied);

                var dbQuery = _poRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.BaseContractCode == authenticate.ContractCode && a.POStatus != POStatus.Pending)
                    .AsQueryable();

                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(c => permission.ProductGroupIds.Contains(c.ProductGroupId));

               
                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a => a.Supplier.Name.Contains(query.SearchText) ||
                   a.Supplier.SupplierCode.Contains(query.SearchText) ||
                   a.POCode.Contains(query.SearchText) ||
                   a.PRContract.PRContractCode.Contains(query.SearchText) ||
                   a.POSubjects.Any(c => c.Product.Description.Contains(query.SearchText) ||
                   c.Product.ProductCode.Contains(query.SearchText)));

                var pageCount = dbQuery.Count();

                var columnsMap = new Dictionary<string, Expression<Func<PO, object>>>
                {
                    ["POCode"] = v => v.POCode
                };
                //dbQuery = dbQuery.ApplayOrdering(query, columnsMap).ApplayPageing(query);
                var resultQuery =await dbQuery.ToListAsync();
                resultQuery = resultQuery.OrderByDescending(a => a.POCode, new CompareFormNumbers()).ToList();
                resultQuery = resultQuery.ApplayPageing(query).ToList();
                var poIds = resultQuery.Select(a => a.POId).ToList();
                var result = await dbQuery.Where(a=>poIds.Contains(a.POId)).Select(po => new ListAllPODto
                {
                    POId = po.POId,
                    POCode = po.POCode,
                    SupplierName = po.Supplier.Name,
                    Products = MergePoSubjects(po.POSubjects.Select(a => a.Product.Description).ToList()),
                    CreateDate = po.CreatedDate.ToUnixTimestamp(),
                    POStatus = po.POStatus,
                    IsPaymentDone = po.IsPaymentDone,
                    ShortageStatus = po.ShortageStatus

                }).ToListAsync();

                result = result.OrderByDescending(a => a.POCode, new CompareFormNumbers()).ToList();
                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(pageCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new List<ListAllPODto>(), exception);
            }
        }
        public async Task<ServiceResult<List<InprogressPOListDto>>> GetInprogressPOAsync(AuthenticateDto authenticate, POQueryDto query)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<InprogressPOListDto>>(null, MessageId.AccessDenied);

                var dbQuery = _poRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.BaseContractCode == authenticate.ContractCode && a.POStatus != POStatus.Pending && a.POStatus != POStatus.Delivered&&a.POStatus!=POStatus.Canceled)
                    .AsQueryable();

                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(c => permission.ProductGroupIds.Contains(c.ProductGroupId));

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a =>
                    a.POCode.Contains(query.SearchText) ||
                    a.PRContract.PRContractCode.Contains(query.SearchText));

                if (query.SupplierIds != null && query.SupplierIds.Any())
                    dbQuery = dbQuery.Where(a => query.SupplierIds.Contains(a.PRContract.SupplierId));

                if (query.ProductGroupIds != null && query.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(a => a.POSubjects.Any(c => query.ProductGroupIds.Contains(c.Product.ProductGroupId)));

                if (query.ProductIds != null && query.ProductIds.Any())
                    dbQuery = dbQuery.Where(a => a.POSubjects.Any(c => query.ProductIds.Contains(c.ProductId)));

                var pageCount = dbQuery.Count();

                //var columnsMap = new Dictionary<string, Expression<Func<PO, object>>>
                //{
                //    ["POCode"] = v => v.POCode,
                //    ["POId"] = v => v.POId
                //};

                dbQuery = dbQuery.OrderByDescending(a => a.CreatedDate).ApplayPageing(query);

                var result = await dbQuery.Select(po => new InprogressPOListDto
                {
                    POId = po.POId,
                    DateDelivery = po.DateDelivery.ToUnixTimestamp(),
                    POCode = po.POCode,
                    Products = MergePoSubjects(po.POSubjects.Select(a => a.Product.Description).ToList()),
                    CreateDate = po.CreatedDate.ToUnixTimestamp(),
                    SupplierName = po.Supplier.Name,
                    SupplierCode = po.Supplier.SupplierCode,
                    SupplierLogo = po.Supplier.Logo != null ? _appSettings.ClientHost + ServiceSetting.UploadImagesPath.LogoSmall + po.Supplier.Logo : "",
                    PRContractCode = po.PRContract.PRContractCode,
                    PContractType = po.PContractType,
                    PRContractDateIssued = po.PRContract.DateIssued.ToUnixTimestamp(),
                    PRContractDateEnd = po.PRContract.DateEnd.ToUnixTimestamp(),
                    UserAudit = po.AdderUser != null
                        ? new UserAuditLogDto
                        {
                            AdderUserId = po.AdderUserId,
                            AdderUserName = po.AdderUser.FullName,
                            CreateDate = po.CreatedDate.ToUnixTimestamp(),
                            AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall +
                                             po.AdderUser.Image
                        }
                        : null,

                    ActivityUsers = po.POActivities != null ? po.POActivities
                    .Where(a => !a.IsDeleted)
                    .Select(c => new UserMentionDto
                    {
                        Id = c.ActivityOwnerId,
                        Display = c.ActivityOwner.FullName,
                        Image = c.ActivityOwner.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + c.ActivityOwner.Image : ""
                    }).ToList() : new List<UserMentionDto>()
                }).ToListAsync();


                foreach (var item in result)
                {
                    if (item.ActivityUsers != null && item.ActivityUsers.Any())
                        item.ActivityUsers = item.ActivityUsers.GroupBy(a => a.Id).Select(v => v.First()).ToList();
                }

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(pageCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new List<InprogressPOListDto>(), exception);
            }
        }

        public async Task<ServiceResult<PODetailsDto>> GetPODetailsByPOIdAsync(AuthenticateDto authenticate, long poId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<PODetailsDto>(null, MessageId.AccessDenied);

                var dbQuery = _poRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.BaseContractCode == authenticate.ContractCode && a.POId == poId && (a.POStatus != POStatus.Pending));

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError(new PODetailsDto(), MessageId.EntityDoesNotExist);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(c => permission.ProductGroupIds.Contains(c.ProductGroupId)))
                    return ServiceResultFactory.CreateError<PODetailsDto>(null, MessageId.AccessDenied);

                var result = await dbQuery.Select(po => new PODetailsDto
                {
                    POId = po.POId,
                    PRContractId = po.PRContractId,
                    PRContractDateIssued = po.PRContract.DateIssued.ToUnixTimestamp(),
                    PRContractDateEnd = po.PRContract.DateEnd.ToUnixTimestamp(),
                    SupplierCode = po.Supplier.SupplierCode,
                    SupplierName = po.Supplier.Name,
                    SupplierLogo = po.Supplier.Logo != null ? _appSettings.ClientHost + ServiceSetting.UploadImagesPath.LogoSmall + po.Supplier.Logo : null,
                    CurrencyType = po.PRContract.CurrencyType,
                    DateDelivery = po.DateDelivery.ToUnixTimestamp(),
                    POCode = po.POCode,
                    PContractType = po.PRContract.PContractType,
                    POStatus = po.POStatus,
                    PRContractCode = po.PRContract.PRContractCode,
                    DeliveryLocation = po.DeliveryLocation,
                    SupplierId = po.SupplierId,
                    
                    PoProgressPercent = po.POProgresses.Where(b => !b.IsDeleted).OrderByDescending(b => b.Id).Select(b => b.ProgressPercent).FirstOrDefault(),
                    POSubjects = po.POSubjects.Select(c => new POSubjectInfoDto
                    {
                        POSubjectId = c.POSubjectId,
                        PriceUnit = 0,
                        Quantity = c.Quantity,
                        MrpCode = (c.MrpItem.BomProduct.IsRequiredMRP) ? c.MrpItem.Mrp.MrpNumber : "",
                        DateRequired = c.MrpItem.DateEnd.ToUnixTimestamp(),
                        ReceiptQuantity = c.ReceiptedQuantity,
                        RemainedStock = c.RemainedQuantity,
                        ShortageQuantity = c.ShortageQuantity,
                        ProductId = c.ProductId,
                        ProductCode = c.Product.ProductCode,
                        ProductName = c.Product.Description,
                        ProductUnit = c.Product.Unit,
                        TechnicalNumber = c.Product.TechnicalNumber,
                        ProductGroupName = c.Product.ProductGroup.Title,

                    }).ToList(),
                    UserAudit = po.AdderUser != null
                        ? new UserAuditLogDto
                        {
                            AdderUserId = po.AdderUserId,
                            AdderUserName = po.AdderUser.FullName,
                            CreateDate = po.CreatedDate.ToUnixTimestamp(),
                            AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall +
                                             po.AdderUser.Image
                        }
                        : null,

                }).FirstOrDefaultAsync();
                List<POSubjectInfoDto> pOSubjectInfoDtos = new List<POSubjectInfoDto>();
                foreach (var item in result.POSubjects)
                {
                    if (!pOSubjectInfoDtos.Any(a => a.ProductId == item.ProductId))
                    {
                        pOSubjectInfoDtos.Add(new POSubjectInfoDto
                        {
                            POSubjectId = item.POSubjectId,
                            PriceUnit = item.PriceUnit,
                            Quantity = result.POSubjects.Where(a => a.ProductId == item.ProductId).Sum(a => a.Quantity),
                            MrpCode = item.MrpCode,
                            DateRequired = item.DateRequired,
                            ReceiptQuantity = result.POSubjects.Where(a => a.ProductId == item.ProductId).Sum(a => a.ReceiptQuantity),
                            RemainedStock = result.POSubjects.Where(a => a.ProductId == item.ProductId).Sum(a => a.RemainedStock),
                            ShortageQuantity = result.POSubjects.Where(a => a.ProductId == item.ProductId).Sum(a => a.ShortageQuantity),
                            ProductId = item.ProductId,
                            ProductCode = item.ProductCode,
                            ProductName = item.ProductName,
                            ProductUnit = item.ProductUnit,
                            TechnicalNumber = item.TechnicalNumber,
                            ProductGroupName = item.ProductGroupName,
                        });
                    }
                }
                result.POSubjects = pOSubjectInfoDtos;
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new PODetailsDto(), exception);
            }
        }

        public async Task<ServiceResult<List<POStatusLogDto>>> GetPoStatusLogsAsync(AuthenticateDto authenticate,
            long poId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<POStatusLogDto>>(null, MessageId.AccessDenied);

                var dbQuery = _poRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.BaseContractCode == authenticate.ContractCode && a.POId == poId);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<List<POStatusLogDto>>(null, MessageId.EntityDoesNotExist);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(c => permission.ProductGroupIds.Contains(c.ProductGroupId)))
                    return ServiceResultFactory.CreateError<List<POStatusLogDto>>(null, MessageId.AccessDenied);


                var poStatusLogs = await dbQuery
                    .SelectMany(a => a.POStatusLogs)
                    .Select(c => new POStatusLogDto
                    {
                        IsDone = c.IsDone,
                        POStatus = c.Status,
                        DateDone = c.UpdateDate.ToUnixTimestamp()
                    })
                    .ToListAsync();

                var result = GetPOStatusLog(poStatusLogs);
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception e)
            {
                return ServiceResultFactory.CreateException(new List<POStatusLogDto>(), e);
            }
        }

        public async Task<ServiceResult<List<POSubjectWithListPartDto>>> GetPOSubjectsByPOIdAsync(AuthenticateDto authenticate, long poId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<POSubjectWithListPartDto>>(null, MessageId.AccessDenied);

                var dbQuery = _poSubjectRepository
                    .AsNoTracking()
                    .Where(a => a.POId == poId && a.PO.BaseContractCode == authenticate.ContractCode && a.PO.POStatus > POStatus.Pending);

                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(c => permission.ProductGroupIds.Contains(c.PO.ProductGroupId));

                var result = await dbQuery.Select(c => new POSubjectWithListPartDto
                {
                    POSubjectId = c.POSubjectId,
                    PriceUnit = 0,
                    Quantity = c.Quantity,
                    RemainedStock = c.RemainedQuantity,
                    ProductId = c.ProductId,
                    ProductCode = c.Product.ProductCode,
                    ProductName = c.Product.Description,
                    ProductUnit = c.Product.Unit,
                    TechnicalNumber = c.Product.TechnicalNumber,
                    ProductGroupName = c.Product.ProductGroup.Title,

                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new List<POSubjectWithListPartDto>(), exception);
            }
        }

        public async Task<ServiceResult<List<BasePOAttachmentDto>>> AddPOAttachmentAsync(AuthenticateDto authenticate, long poId, IFormFileCollection files)
        {
            try
            {
                if (files == null || !files.Any())
                    return ServiceResultFactory.CreateError<List<BasePOAttachmentDto>>(null, MessageId.InputDataValidationError);

                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<BasePOAttachmentDto>>(null, MessageId.AccessDenied);

                var poModel = await _poRepository
                   .Where(a => a.POId == poId && !a.IsDeleted && a.BaseContractCode == authenticate.ContractCode)
                   .Select(c => new
                   {
                       poId = c.POId,
                       baseContractCode = c.BaseContractCode,
                       ProductGroupId = c.ProductGroupId,
                       POStatus=c.POStatus
                   }).FirstOrDefaultAsync();

                if (poModel == null)
                    return ServiceResultFactory.CreateError<List<BasePOAttachmentDto>>(null, MessageId.EntityDoesNotExist);

                if (poModel.POStatus==POStatus.Canceled)
                    return ServiceResultFactory.CreateError<List<BasePOAttachmentDto>>(null, MessageId.CantDoneBecausePOCanceled);

                if (permission.ProductGroupIds.Any() && !permission.ProductGroupIds.Contains(poModel.ProductGroupId))
                    return ServiceResultFactory.CreateError<List<BasePOAttachmentDto>>(null, MessageId.AccessDenied);

                var attachModels = new List<PAttachment>();
                foreach (var item in files)
                {
                    var fileName = item.FileName;
                    var uploadResult = await _fileService.UploadDocumentFile(item);
                    if (!uploadResult.Succeeded)
                        return ServiceResultFactory.CreateError<List<BasePOAttachmentDto>>(null, uploadResult.Messages[0].Message);

                    var UploadedFile = await _fileHelper
                        .SaveDocumentFromTemp(uploadResult.Result, ServiceSetting.UploadFilePath.PO);

                    if (UploadedFile == null)
                        return ServiceResultFactory.CreateError<List<BasePOAttachmentDto>>(null, MessageId.UploudFailed);

                    _fileHelper.DeleteDocumentFromTemp(uploadResult.Result);

                    attachModels.Add(new PAttachment
                    {
                        POId = poId,
                        FileSrc = uploadResult.Result,
                        FileName = fileName,
                        FileType = UploadedFile.FileType,
                        FileSize = UploadedFile.FileSize
                    });
                }

                _pAttachmentRepository.AddRange(attachModels);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var res = attachModels.Select(c => new BasePOAttachmentDto
                    {
                        Id = c.Id,
                        FileName = c.FileName,
                        FileSize = c.FileSize,
                        FileSrc = c.FileSrc,
                        FileType = c.FileType,
                        POId = poId
                    }).ToList();
                    return ServiceResultFactory.CreateSuccess(res);
                }

                return ServiceResultFactory.CreateError<List<BasePOAttachmentDto>>(null, MessageId.DeleteEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new List<BasePOAttachmentDto>(), exception);
            }
        }

        public async Task<ServiceResult<bool>> RemovePOAttachmentByPoIdAsync(AuthenticateDto authenticate, long poId, string fileSrc)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var poModel = await _poRepository
                  .Where(a => a.POId == poId && !a.IsDeleted && a.BaseContractCode == authenticate.ContractCode)
                 .Select(c => new
                 {
                     poId = c.POId,
                     baseContractCode = c.BaseContractCode,
                     ProductGroupId = c.ProductGroupId
                 }).FirstOrDefaultAsync();

                if (poModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (permission.ProductGroupIds.Any() && !permission.ProductGroupIds.Contains(poModel.ProductGroupId))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var attachmentModel = await _pAttachmentRepository
                    .FirstOrDefaultAsync(a => !a.IsDeleted && a.POId == poId && a.FileSrc == fileSrc);
                if (attachmentModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                attachmentModel.IsDeleted = true;

                return await _unitOfWork.SaveChangesAsync() > 0
                    ? ServiceResultFactory.CreateSuccess(true)
                    : ServiceResultFactory.CreateError(false, MessageId.DeleteEntityFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<List<BasePOAttachmentDto>>> GetPoAttachmentByPOIdAsync(AuthenticateDto authenticate, long poId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<BasePOAttachmentDto>>(null, MessageId.AccessDenied);

                var dbQuery = _pAttachmentRepository
                    .Where(a => !a.IsDeleted && a.POId == poId && a.PO.BaseContractCode == authenticate.ContractCode);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(c => permission.ProductGroupIds.Contains(c.PO.ProductGroupId)))
                    return ServiceResultFactory.CreateError<List<BasePOAttachmentDto>>(null, MessageId.AccessDenied);

                if (dbQuery == null)
                    return ServiceResultFactory.CreateError(new List<BasePOAttachmentDto>(), MessageId.EntityDoesNotExist);

                var result = await dbQuery
                    .Select(c => new BasePOAttachmentDto
                    {
                        Id = c.Id,
                        POId = c.POId.Value,
                        FileName = c.FileName,
                        FileType = c.FileType,
                        FileSize = c.FileSize,
                        FileSrc = c.FileSrc
                    }).ToListAsync();
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new List<BasePOAttachmentDto>(), exception);
            }
        }

        public async Task<DownloadFileDto> DownloadPOAttachmentAsync(AuthenticateDto authenticate, long poId, string fileSrc)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return null;

                if (!await _poRepository.AnyAsync(a => a.POId == poId && a.BaseContractCode == authenticate.ContractCode &&
                a.POAttachments.Any(c => !c.IsDeleted && c.FileSrc == fileSrc)))
                    return null;

                var streamResult = await _fileHelper.DownloadDocument(fileSrc, ServiceSetting.FileSection.PO);
                if (streamResult == null)
                    return null;

                return streamResult;
            }
            catch (Exception exception)
            {
                return null;
            }
        }
        public async Task<ServiceResult<PODetailsForCancelDto>> CancelPoAsync(AuthenticateDto authenticate, long poId)
        {
            try
            {

                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<PODetailsForCancelDto>(null, MessageId.AccessDenied);

                var poModel = await _poRepository.Include(a => a.Packs).Include(a=>a.PRContract).Include(a=>a.Supplier).Include(a=>a.POProgresses).Include(a => a.POSubjects).ThenInclude(a => a.MrpItem)
                   .Where(a => a.POId == poId && !a.IsDeleted && a.BaseContractCode == authenticate.ContractCode).FirstOrDefaultAsync();

                if (poModel == null)
                    return ServiceResultFactory.CreateError<PODetailsForCancelDto>(null, MessageId.EntityDoesNotExist);

                if (permission.ProductGroupIds.Any() && !permission.ProductGroupIds.Contains(poModel.ProductGroupId))
                    return ServiceResultFactory.CreateError<PODetailsForCancelDto>(null, MessageId.AccessDenied);
                if (poModel.Packs.Any(a => !a.IsDeleted && a.PackStatus > PackStatus.RejectQC))
                    return ServiceResultFactory.CreateError<PODetailsForCancelDto>(null, MessageId.PoCantCancelIfHasConfirmedQCPack);
                poModel.POStatus = POStatus.Canceled;
                var mrpId = poModel.POSubjects.Select(a => a.MrpItem.MrpId).FirstOrDefault();
                var poPending = await _poRepository.Include(a => a.POSubjects).Where(a => !a.IsDeleted && a.PRContractId == poModel.PRContractId && a.POStatus == POStatus.Pending && a.POSubjects.Any(a => a.MrpItem.MrpId == mrpId)).FirstOrDefaultAsync();
                if (poPending != null)
                {
                    foreach (var item in poModel.POSubjects)
                    {
                        if (poPending.POSubjects.Any(a => a.ProductId == item.ProductId && a.MrpItemId == item.MrpItemId))
                        {
                            var subject = poPending.POSubjects.First(a => a.ProductId == item.ProductId && a.MrpItemId == item.MrpItemId);
                            subject.Quantity += item.Quantity;
                            subject.RemainedQuantity += item.Quantity;
                            item.MrpItem.MrpItemStatus = MrpItemStatus.PRC;
                        }
                        else
                        {
                            poPending.POSubjects.Add(new POSubject
                            {
                                Quantity = item.Quantity,
                                RemainedQuantity = item.Quantity,
                                ProductId = item.ProductId,
                                PriceUnit = item.PriceUnit,
                                MrpItemId = item.MrpItemId,
                            });
                        }
                    }

                    CalculatePOAmount(poPending.Tax, poPending);
                }
                else
                {
                    var poPendingModel = new PO
                    {
                        IsDeleted = false,
                        SupplierId = poModel.SupplierId,
                        BaseContractCode = poModel.BaseContractCode,
                        POStatus = POStatus.Pending,
                        CurrencyType = poModel.CurrencyType,
                        Tax = poModel.Tax,
                        PContractType = poModel.PContractType,
                        ProductGroupId = poModel.ProductGroupId,
                        PRContractId = poModel.PRContractId,
                        DateDelivery = poModel.DateDelivery,
                        PORefType = poModel.PORefType,
                        DeliveryLocation = poModel.DeliveryLocation,
                        POSubjects = new List<POSubject>(),
                        POTermsOfPayments = new List<POTermsOfPayment>()
                    };
                    foreach (var item in poModel.POSubjects)
                    {
                        poPendingModel.POSubjects.Add(new POSubject
                        {
                            Quantity = item.Quantity,

                            ProductId = item.ProductId,
                            PriceUnit = item.PriceUnit,
                            MrpItemId = item.MrpItemId,
                            RemainedQuantity = item.Quantity

                        });
                    }

                    CalculatePOAmount(poPendingModel.Tax, poPendingModel);
                    await _poRepository.AddAsync(poPendingModel);
                }

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var result = new PODetailsForCancelDto
                    {
                        POId = poModel.POId,
                        PRContractId = poModel.PRContractId,
                        PRContractDateIssued = poModel.PRContract.DateIssued.ToUnixTimestamp(),
                        PRContractDateEnd = poModel.PRContract.DateEnd.ToUnixTimestamp(),
                        SupplierCode = poModel.Supplier.SupplierCode,
                        SupplierName = poModel.Supplier.Name,
                        SupplierLogo = poModel.Supplier.Logo != null ? _appSettings.ClientHost + ServiceSetting.UploadImagesPath.LogoSmall + poModel.Supplier.Logo : null,
                        CurrencyType = poModel.PRContract.CurrencyType,
                        DateDelivery = poModel.DateDelivery.ToUnixTimestamp(),
                        POCode = poModel.POCode,
                        PContractType = poModel.PRContract.PContractType,
                        POStatus = poModel.POStatus,
                        PRContractCode = poModel.PRContract.PRContractCode,
                        DeliveryLocation = poModel.DeliveryLocation,
                        SupplierId = poModel.SupplierId,
                        PoProgressPercent = poModel.POProgresses.Where(b => !b.IsDeleted).OrderByDescending(b => b.Id).Select(b => b.ProgressPercent).FirstOrDefault(),
                    };

                    return ServiceResultFactory.CreateSuccess(result);
                }

                return ServiceResultFactory.CreateError<PODetailsForCancelDto>(null, MessageId.DeleteEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<PODetailsForCancelDto>(null, exception);
            }
        }
        private List<POStatusLogDto> GetPOStatusLog(List<POStatusLogDto> poStatusLog)
        {
            var result = new List<POStatusLogDto>();
            foreach (POStatus status in (POStatus[])Enum.GetValues(typeof(POStatus)))
            {
                if (status == POStatus.Canceled)
                    continue;

                var step = poStatusLog.FirstOrDefault(a => a.POStatus == status);
                if (step != null)
                {
                    result.Add(new POStatusLogDto
                    {
                        DateDone = step.DateDone,
                        IsDone = step.IsDone,
                        IsDoing = true,
                        POStatus = step.POStatus,
                        DisplayName = status.GetDisplayName()
                    });
                }
                else
                {
                    result.Add(new POStatusLogDto
                    {
                        DateDone = null,
                        IsDone = false,
                        IsDoing = false,
                        POStatus = status,
                        DisplayName = status.GetDisplayName()
                    });
                }
            }

            return result;
        }

       
        #region packing
        public async Task<ServiceResult<List<POSubjectWithListPartDto>>> GetRemainedPOSubjectsByPOIdAsync(AuthenticateDto authenticate, long poId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<POSubjectWithListPartDto>>(null, MessageId.AccessDenied);

                var dbQuery = _poSubjectRepository
                    .AsNoTracking()
                    .Where(a => a.POId == poId && a.PO.BaseContractCode == authenticate.ContractCode && a.PO.POStatus > POStatus.Pending && a.RemainedQuantity > 0);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<List<POSubjectWithListPartDto>>(null, MessageId.EntityDoesNotExist);

                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.ProductGroupIds.Contains(a.PO.ProductGroupId));

                var result = await dbQuery.Select(c => new POSubjectWithListPartDto
                {
                    POSubjectId = c.POSubjectId,
                    PriceUnit = 0,
                    Quantity = c.Quantity,
                    RemainedStock = c.RemainedQuantity,
                    ProductId = c.ProductId,
                    ProductCode = c.Product.ProductCode,
                    ProductName = c.Product.Description,
                    ProductUnit = c.Product.Unit,
                    TechnicalNumber = c.Product.TechnicalNumber,
                    ProductGroupName = c.Product.ProductGroup.Title,

                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new List<POSubjectWithListPartDto>(), exception);
            }
        }
        #endregion

        private static List<string> MergePoSubjects(List<string> products)
        {
            List<string> result = new List<string>();
            foreach (var item in products)
            {
                if (!result.Contains(item))
                    result.Add(item);
            }
            return result;
        }
    }
}