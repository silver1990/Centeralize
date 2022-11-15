using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Raybod.SCM.DataAccess.Core;
using Raybod.SCM.DataAccess.Extention;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Audit;
using Raybod.SCM.DataTransferObject.Notification;
using Raybod.SCM.DataTransferObject.QualityControl;
using Raybod.SCM.DataTransferObject.Receipt;
using Raybod.SCM.DataTransferObject.ReportReceiptProduct;
using Raybod.SCM.DataTransferObject.Warehouse;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Model;
using Raybod.SCM.Domain.Struct;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Utility.Common;
using Raybod.SCM.Utility.Extention;

namespace Raybod.SCM.Services.Implementation
{
    public class ReceiptService : IReceiptService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITeamWorkAuthenticationService _authenticationService;
        private readonly ISCMLogAndNotificationService _scmLogAndNotificationService;
        private readonly IContractFormConfigService _formConfigService;
        private readonly DbSet<WarehouseOutputRequest> _warehouseOutputRequestRepository;
        private readonly DbSet<ReceiptReject> _receiptRejectRepository;
        private readonly DbSet<WarehouseProduct> _warehouseProductRepository;
        private readonly DbSet<WarehouseProductStockLogs> _warehouseProductStockLogsRepository;
        private readonly DbSet<QualityControl> _qualityControlRepository;
        private readonly DbSet<PAttachment> _pAttachmentRepository;
        private readonly DbSet<ProductGroup> _prodcutGroupRepository;
        private readonly DbSet<Pack> _packRepository;
        private readonly DbSet<MrpItem> _mrpItemRepository;
        private readonly DbSet<Receipt> _receiptRepository;
        private readonly DbSet<ReceiptSubject> _receiptSubjectRepository;
        private readonly DbSet<PO> _poRepository;
        private readonly DbSet<POStatusLog> _poStatusLogRepository;
        private readonly Raybod.SCM.Services.Utilitys.FileHelper _fileHelper;
        private readonly CompanyConfig _appSettings;
        public ReceiptService(
            IUnitOfWork unitOfWork,
            IWebHostEnvironment hostingEnvironmentRoot,
            ITeamWorkAuthenticationService authenticationService,
            IOptions<CompanyAppSettingsDto> appSettings,
            IHttpContextAccessor httpContextAccessor,
            ISCMLogAndNotificationService scmLogAndNotificationService,
            IContractFormConfigService formConfigService)
        {
            _unitOfWork = unitOfWork;
            _authenticationService = authenticationService;
            _scmLogAndNotificationService = scmLogAndNotificationService;
            _formConfigService = formConfigService;
            _warehouseProductRepository = _unitOfWork.Set<WarehouseProduct>();
            _packRepository = _unitOfWork.Set<Pack>();
            _receiptRejectRepository = _unitOfWork.Set<ReceiptReject>();
            _qualityControlRepository = _unitOfWork.Set<QualityControl>();
            _pAttachmentRepository = _unitOfWork.Set<PAttachment>();
            _warehouseProductStockLogsRepository = _unitOfWork.Set<WarehouseProductStockLogs>();
            _receiptRepository = _unitOfWork.Set<Receipt>();
            _prodcutGroupRepository = _unitOfWork.Set<ProductGroup>();
            _receiptSubjectRepository = _unitOfWork.Set<ReceiptSubject>();
            _poRepository = _unitOfWork.Set<PO>();
            _mrpItemRepository = _unitOfWork.Set<MrpItem>();
            _poStatusLogRepository = _unitOfWork.Set<POStatusLog>();
            httpContextAccessor.HttpContext.Request.Headers.TryGetValue("companyCode", out var CompanyCode);
            _appSettings = appSettings.Value.CompanyConfig.First(a => a.CompanyCode == CompanyCode);
            _fileHelper = new Utilitys.FileHelper(hostingEnvironmentRoot);
            _warehouseOutputRequestRepository = _unitOfWork.Set<WarehouseOutputRequest>(); 
        }

        public async Task<ServiceResult<ReceiptBadgeNotificationDto>> GetReceiptWaitingBadgeCountAsync(AuthenticateDto authenticate)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateSuccess(new ReceiptBadgeNotificationDto());

                var result = new ReceiptBadgeNotificationDto();

                var receiptQuery = _receiptRepository.Where(a => a.PO.BaseContractCode == authenticate.ContractCode && a.ReceiptStatus == ReceiptStatus.PendingForQC);


                result.WaitingReceiptforQC = await receiptQuery.CountAsync();

                var packQuery = _packRepository
                    .Where(a => !a.IsDeleted &&
                    a.PO.BaseContractCode == authenticate.ContractCode &&
                    (a.PackStatus == PackStatus.PendingDelivered || a.PackStatus == PackStatus.T3Inprogress));

                var receiptmodels = await packQuery
                    .Select(p => new ListWaitingPackForReceiptDto
                    {
                        PackId = p.PackId,
                        ReceiptProducts = p.PackSubjects.Select(c => new ReceiptSubjectDto
                        {
                            ProductName = c.Product.Description,

                        }).ToList(),
                    }).ToListAsync();

                var lastResult = new List<ListWaitingPackForReceiptDto>();

                foreach (var receipt in receiptmodels)
                {
                    foreach (var subject in receipt.ReceiptProducts)
                    {
                        var newReceipt = new ListWaitingPackForReceiptDto
                        {
                            PackId = receipt.PackId,
                            ReceiptProducts = new List<ReceiptSubjectDto>()
                        };

                        if (subject.PartProductNames != null && subject.PartProductNames.Count() > 0)
                        {
                            newReceipt.ReceiptProducts.Add(new ReceiptSubjectDto
                            {
                                ProductName = subject.ProductName,
                                PartProductNames = subject.PartProductNames
                            });
                            lastResult.Add(newReceipt);
                        }
                    }

                    var realSubject = receipt.ReceiptProducts.Where(a => a.PartProductNames == null || a.PartProductNames.Count() == 0).ToList();
                    if (realSubject != null && realSubject.Count() > 0)
                    {
                        var receiptmodel = new ListWaitingPackForReceiptDto
                        {
                            PackId = receipt.PackId,
                            ReceiptProducts = new List<ReceiptSubjectDto>()
                        };
                        receiptmodel.ReceiptProducts = realSubject;
                        lastResult.Add(receiptmodel);
                    }
                }

                result.WaitingforAddReceipt = lastResult.Count();
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<ReceiptBadgeNotificationDto>(null, exception);
            }
        }

        public async Task<ServiceResult<List<ListWaitingPackForReceiptDto>>> GetWaitingPackForReceiptAsync(AuthenticateDto authenticate, WaitingPackQueryDto query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<ListWaitingPackForReceiptDto>>(null, MessageId.AccessDenied);

                var dbQuery = _packRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted &&
                    a.PO.BaseContractCode == authenticate.ContractCode &&
                    (a.PackStatus == PackStatus.PendingDelivered || a.PackStatus == PackStatus.T3Inprogress))
                    .AsQueryable();

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a => a.PackCode.Contains(query.SearchText) || a.PO.POCode.Contains(query.SearchText));

                if (query.SupplierIds != null && query.SupplierIds.Any())
                    dbQuery = dbQuery.Where(a => query.SupplierIds.Contains(a.PO.SupplierId));

                if (query.ProductGroupIds != null && query.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(a => query.ProductGroupIds.Contains(a.PO.ProductGroupId));

                if (query.ProductIds != null && query.ProductIds.Any())
                    dbQuery = dbQuery.Where(a => a.PackSubjects.Any(c => query.ProductIds.Contains(c.ProductId)));

                var pageCount = dbQuery.Count();

                var columnsMap = new Dictionary<string, Expression<Func<Pack, object>>>
                {
                    ["PackCode"] = v => v.PackCode,
                    ["PackId"] = v => v.PackId,
                    ["CreatedDate"] = v => v.CreatedDate,
                };

                //dbQuery = dbQuery.ApplayOrdering(query, columnsMap);
                //.ApplayPageing(query);

                var result = await dbQuery.Select(p => new ListWaitingPackForReceiptDto
                {
                    PackId = p.PackId,
                    POId = p.POId,
                    PackCode = p.PackCode,
                    POCode = p.PO.POCode,
                    SupplierCode = p.PO.Supplier.SupplierCode,
                    SupplierName = p.PO.Supplier.Name,
                    SupplierLogo = _appSettings.ClientHost + ServiceSetting.UploadImagesPath.LogoSmall + p.PO.Supplier.Logo,
                    LogisticDateEnd = p.Logistics.Any(a => a.Step == LogisticStep.T3 && a.LogisticStatus == LogisticStatus.Compeleted) ?
                    p.Logistics.FirstOrDefault(a => a.Step == LogisticStep.T3 && a.LogisticStatus == LogisticStatus.Compeleted).DateEnd.ToUnixTimestamp()
                    : null,
                    ReceiptProducts = p.PackSubjects.Select(c => new ReceiptSubjectDto
                    {
                        ProductName = c.Product.Description,
                        ProductId = c.ProductId,

                    }).ToList(),
                    UserAudit = p.AdderUser != null ? new UserAuditLogDto
                    {
                        AdderUserName = p.AdderUser.FullName,
                        AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + p.AdderUser.Image,
                        CreateDate = p.CreatedDate.ToUnixTimestamp()
                    } : null,
                }).ToListAsync();

                var lastResult = new List<ListWaitingPackForReceiptDto>();

                foreach (var receipt in result)
                {
                    foreach (var subject in receipt.ReceiptProducts)
                    {
                        var newReceipt = new ListWaitingPackForReceiptDto
                        {
                            PackId = receipt.PackId,
                            POId = receipt.POId,
                            PackCode = receipt.PackCode,
                            POCode = receipt.POCode,
                            LogisticDateEnd = receipt.LogisticDateEnd,
                            SupplierCode = receipt.SupplierCode,
                            SupplierName = receipt.SupplierName,
                            SupplierLogo = receipt.SupplierLogo,
                            UserAudit = receipt.UserAudit,
                            ReceiptProducts = new List<ReceiptSubjectDto>()
                        };

                        if (subject.PartProductNames.Any() && subject.PartProductNames.Count() > 0)
                        {
                            newReceipt.IsPart = true;
                            newReceipt.subjectProductId = subject.ProductId;
                            newReceipt.ReceiptProducts.Add(new ReceiptSubjectDto
                            {
                                ProductName = subject.ProductName,
                                PartProductNames = subject.PartProductNames
                            });
                            lastResult.Add(newReceipt);
                        }
                    }

                    var realSubject = receipt.ReceiptProducts.Where(a => !a.PartProductNames.Any()).ToList();
                    if (realSubject != null && realSubject.Count() > 0)
                    {
                        var receiptmodel = new ListWaitingPackForReceiptDto
                        {
                            PackId = receipt.PackId,
                            POId = receipt.POId,
                            PackCode = receipt.PackCode,
                            POCode = receipt.POCode,
                            IsPart = false,
                            LogisticDateEnd = receipt.LogisticDateEnd,
                            SupplierCode = receipt.SupplierCode,
                            SupplierName = receipt.SupplierName,
                            SupplierLogo = receipt.SupplierLogo,
                            UserAudit = receipt.UserAudit,
                            ReceiptProducts = new List<ReceiptSubjectDto>()
                        };
                        receiptmodel.ReceiptProducts = realSubject;
                        lastResult.Add(receiptmodel);
                    }
                }
                lastResult = lastResult.OrderByDescending(c => c.LogisticDateEnd).ToList();
                return ServiceResultFactory.CreateSuccess(lastResult);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<ListWaitingPackForReceiptDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<ReceiptPackInfoDto>> GetWaitingPackInfoByIdAsync(AuthenticateDto authenticate, long packId, bool isPart, long? subjectProductId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<ReceiptPackInfoDto>(null, MessageId.AccessDenied);

                if (isPart && subjectProductId == null)
                    return ServiceResultFactory.CreateError<ReceiptPackInfoDto>(null, MessageId.EntityDoesNotExist);

                var dbQuery = _packRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted &&
                    a.PO.BaseContractCode == authenticate.ContractCode &&
                    a.PackId == packId &&
                    (a.PackStatus == PackStatus.PendingDelivered || a.PackStatus == PackStatus.T3Inprogress))
                    .OrderByDescending(a => a.PackId)
                    .AsQueryable();

                var result = await dbQuery
                     .Select(p => new ReceiptPackInfoDto
                     {
                         PackId = p.PackId,
                         PackCode = p.PackCode,
                         SupplierCode = p.PO.Supplier.SupplierCode,
                         SupplierName = p.PO.Supplier.Name,
                         //SupplierImage = _appSettings.AdminHost + ServiceSetting.UploadImagesPath.LogoSmall + p.PO.Supplier.Logo,
                         POCode = p.PO.POCode,
                         PRContractCode = p.PO.PRContract.PRContractCode,
                         LogisticDateEnd =
                         p.Logistics.Any(c => c.Step == LogisticStep.T3 && c.LogisticStatus == LogisticStatus.Compeleted) ?
                         p.Logistics.First(c => c.Step == LogisticStep.T3).DateEnd.ToUnixTimestamp() :
                         null,
                         ReceiptSubjects = p.PackSubjects
                         .Select(a => new ReceiptPackSubjectDto
                         {
                             ReceiptSubjectId = a.PackSubjectId,
                             PackQuantity = a.Quantity,
                             ReceiptQuantity = a.Quantity,
                             ProductId = a.ProductId,
                             ProductName = a.Product.Description,
                             ProductUnit = a.Product.Unit,
                             ProductCode = a.Product.ProductCode,
                             QCReceiptQuantity = 0,

                         }).ToList()
                     }).FirstOrDefaultAsync();

                if (result == null)
                    return ServiceResultFactory.CreateError<ReceiptPackInfoDto>(null, MessageId.EntityDoesNotExist);

                return ServiceResultFactory.CreateSuccess(result);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<ReceiptPackInfoDto>(null, exception);
            }
        }


        public async Task<ServiceResult<bool>> AddReceiptForPackAsync(AuthenticateDto authenticate, long packId, List<AddReceiptProductDto> model)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                if (model == null)
                    return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                // age part bod har part bayad be tour monhaser be fard sabt shavad


                //age nabud hich subject parti nabayad bash, faghat subject hay sade
                if ((model.Any(v => v.ReceiptQuantity <= 0)))
                    return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                var postedProductIds = new List<int>();
                foreach (var item in model)
                {

                    postedProductIds.Add(item.ProductId);

                }

                postedProductIds = postedProductIds.Distinct().ToList();

                var warehouseProducts = await _warehouseProductRepository
                    .Where(c => postedProductIds.Contains(c.ProductId))
                    .ToListAsync();

                var packQuery = _packRepository
                    .Where(a => !a.IsDeleted &&
                    a.PO.BaseContractCode == authenticate.ContractCode &&
                    a.PackId == packId &&
                    (a.PackStatus == PackStatus.PendingDelivered || a.PackStatus == PackStatus.T3Inprogress));

                var PackModel = await packQuery
                    .Include(a => a.Logistics)
                    .Include(a => a.PackSubjects)

                    .FirstOrDefaultAsync();

                if (PackModel == null || PackModel.PackSubjects == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (PackModel.PackSubjects.Count() != model.Count)
                    return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                var poModel = await _poRepository
                    .Where(a => a.BaseContractCode == authenticate.ContractCode && a.POId == PackModel.POId)
                    .Include(a => a.Supplier)
                    .Include(a => a.POSubjects)
                     .Include(a => a.POSubjects)
                    .ThenInclude(a => a.MrpItem)
                    .FirstOrDefaultAsync();

                if (poModel == null || poModel.POSubjects == null)
                    return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                var mrpItemIds = poModel.POSubjects
                    .Select(a => a.MrpItemId)
                    .ToList();

                var mrpItems = await _mrpItemRepository
                    .Where(a => !a.IsDeleted && mrpItemIds.Contains(a.Id))
                    .ToListAsync();

                // add new receipt
                var receiptModel = new Receipt
                {
                    POId = poModel.POId,
                    PackId = PackModel.PackId,
                    ReceiptStatus = ReceiptStatus.PendingForQC,
                    SupplierId = poModel.SupplierId,
                    ReceiptSubjects = new List<ReceiptSubject>(),
                };

                // update poSubject
                var UpdatePoSubjectResult = AddReceiptSubject(model, poModel.POSubjects, mrpItems, receiptModel, PackModel.PackSubjects);
                if (!UpdatePoSubjectResult.Succeeded)
                    return UpdatePoSubjectResult;


                // update warhouse
                await UpdateWarehouseStockQuantity(receiptModel, model, warehouseProducts);


                if (PackModel.PackStatus == PackStatus.T3Inprogress)
                {
                    var lastLogistic = PackModel.Logistics.FirstOrDefault(a => a.Step == LogisticStep.T3);
                    if (lastLogistic == null)
                        return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                    lastLogistic.DateEnd = DateTime.UtcNow;
                    lastLogistic.LogisticStatus = LogisticStatus.Compeleted;
                    if(poModel.POStatus!=POStatus.Delivered)
                        await UpdatePoStatus(authenticate.UserId, poModel, lastLogistic.LogisticId);
                }


                PackModel.PackStatus = PackStatus.Delivered;



                // generate form code
                var count = await _receiptRepository.CountAsync(a => a.PO.BaseContractCode == authenticate.ContractCode);
                var codeRes = await _formConfigService.GenerateFormCodeAsync(authenticate.ContractCode, FormName.Receipt, count);
                if (!codeRes.Succeeded)
                    return ServiceResultFactory.CreateError(false, codeRes.Messages.First().Message);

                receiptModel.ReceiptCode = codeRes.Result;
                if (poModel.POStatus == POStatus.Delivered)
                {
                    if (poModel.POSubjects.Any(a => a.RemainedQuantity > 0))
                        poModel.ShortageStatus = POShortageStatus.RecepitShortage;
                    else
                        poModel.ShortageStatus = POShortageStatus.NoShortage;
                }

                await _receiptRepository.AddAsync(receiptModel);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    await _scmLogAndNotificationService.SetDonedNotificationAsync(authenticate.UserId, poModel.BaseContractCode, packId.ToString(), NotifEvent.AddReceipt);

                    await SendNotificationOnAddReceiptAsync(authenticate, false, model, poModel, receiptModel, PackModel.PackCode);

                    return ServiceResultFactory.CreateSuccess(true);
                }
                return ServiceResultFactory.CreateError(false, MessageId.SaveFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        private async Task SendNotificationOnAddReceiptAsync(AuthenticateDto authenticate, bool isPart, List<AddReceiptProductDto> model,
            PO poModel, Receipt receiptModel, string packCode)
        {

            int productId = 0;
            var task = new List<NotifToDto> {
                            new NotifToDto
                            {
                                NotifEvent=NotifEvent.AddReceiptQC,
                                Roles= new List<string>
                                {
                                    SCMRole.WarehouseQCMng,
                                }
                            },
                            new NotifToDto
                            {
                                NotifEvent = NotifEvent.AddInvoice,
                                Roles = new List<string>
                                {
                                    SCMRole.InvoiceMng,
                                }
                            }
                        };

            if (isPart)
            {
                productId = model.Select(a => a.ProductId).FirstOrDefault();
                if (!poModel.POSubjects.Any(c => c.ProductId == productId && c.POSubjectPartInvoiceStatus == POSubjectPartInvoiceStatus.WaitingForInvoice))
                {
                    var removeTask = task.FirstOrDefault(a => a.NotifEvent == NotifEvent.AddInvoice);
                    task.Remove(removeTask);
                }
            }

            var res = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
            {
                ContractCode = poModel.BaseContractCode,
                FormCode = receiptModel.ReceiptCode,
                KeyValue = receiptModel.ReceiptId.ToString(),
                RootKeyValue = poModel.POId.ToString(),
                RootKeyValue2 = productId.ToString(),
                Description = poModel.POCode,
                Temp = packCode,
                Message = poModel.Supplier.Name,
                Quantity =  ((int)WaitingForInvoiceType.Receipt).ToString(),
                NotifEvent = NotifEvent.AddReceipt,
                PerformerUserId = authenticate.UserId,
                PerformerUserFullName = authenticate.UserFullName,
            },
            task);
        }

        public async Task<ServiceResult<List<ListReceiptDto>>> GetWaitingReceiptForAddQCListAsync(AuthenticateDto authenticate, ReceiptQueryDto query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<ListReceiptDto>>(null, MessageId.AccessDenied);

                var dbQuery = _receiptRepository
                    .AsNoTracking()
                    .Where(a => a.PO.BaseContractCode == authenticate.ContractCode && a.ReceiptStatus == ReceiptStatus.PendingForQC)
                    .OrderByDescending(a => a.ReceiptId)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a => a.ReceiptCode.Contains(query.SearchText) || (a.Pack != null && a.Pack.PackCode.Contains(query.SearchText)));

                if (query.SupplierIds != null && query.SupplierIds.Any())
                    dbQuery = dbQuery.Where(a => a.Supplier != null && query.SupplierIds.Contains(a.PO.SupplierId));

                if (query.ProductGroupIds != null && query.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(a => query.ProductGroupIds.Contains(a.Pack.PO.ProductGroupId));

                if (query.ProductIds != null && query.ProductIds.Any())
                    dbQuery = dbQuery.Where(a => a.ReceiptSubjects.Any(c => query.ProductIds.Contains(c.ProductId) ||
                    c.ReceiptSubjectPartLists.Any(v => query.ProductIds.Contains(v.ProductId))));

                var totalCount = dbQuery.Count();

                dbQuery = dbQuery.ApplayPageing(query);

                var result = await dbQuery.Select(r => new ListReceiptDto
                {
                    PackCode = r.Pack != null ? r.Pack.PackCode : "",
                    PackId = r.PackId,
                    ReceiptId = r.ReceiptId,
                    ReceiptCode = r.ReceiptCode,
                    SupplierId = r.SupplierId,
                    SupplierCode = r.Supplier != null ? r.Supplier.SupplierCode : "",
                    SupplierName = r.Supplier != null ? r.Supplier.Name : "",
                    SupplierImage = r.Supplier != null ? _appSettings.ClientHost + ServiceSetting.UploadImagesPath.LogoSmall + r.Supplier.Logo : "",
                    ReceiptStatus = r.ReceiptStatus,
                    UserAudit = r.AdderUser != null ? new UserAuditLogDto
                    {
                        AdderUserName = r.AdderUser.FullName,
                        AdderUserImage = r.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + r.AdderUser.Image : "",
                        CreateDate = r.CreatedDate.ToUnixTimestamp()
                    } : null,
                    ReceiptProducts = r.ReceiptSubjects.Select(c => new ReceiptSubjectDto
                    {
                        ProductName = c.Product.Description,
                        PartProductNames = c.ReceiptSubjectPartLists != null ? c.ReceiptSubjectPartLists
                       .Select(v => new ReceiptSubjectDto
                       {
                           ProductName = v.Product.Description,
                       }).ToList() : new List<ReceiptSubjectDto>()
                    }).ToList(),
                }).ToListAsync();
                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<ListReceiptDto>>(null, exception);
            }
        }

        #region receiptReject
        public async Task<ServiceResult<List<ListReceiptDto>>> GetWaitingReceiptForRejectListAsync(AuthenticateDto authenticate, ReceiptQueryDto query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<ListReceiptDto>>(null, MessageId.AccessDenied);

                var dbQuery = _receiptRepository
                    .AsNoTracking()
                    .Where(a => a.PO.BaseContractCode == authenticate.ContractCode && (a.ReceiptStatus == ReceiptStatus.QCRejected || a.ReceiptStatus == ReceiptStatus.ConditionalAcceptance) &&
                   !a.ReceiptSubjects.Any(c => c.ReceiptSubjectPartLists.Any()) &&
                   a.ReceiptSubjects.Any(v => v.PurchaseRejectRemainedQuantity > 0))
                    .OrderByDescending(c => c.ReceiptId).AsQueryable();

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a => a.ReceiptCode.Contains(query.SearchText) || (a.Pack != null && a.Pack.PackCode.Contains(query.SearchText)));

                if (query.SupplierIds != null && query.SupplierIds.Any())
                    dbQuery = dbQuery.Where(a => a.Supplier != null && query.SupplierIds.Contains(a.PO.SupplierId));

                if (query.ProductGroupIds != null && query.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(a => a.ReceiptSubjects.Any(c => query.ProductGroupIds.Contains(c.Product.ProductGroupId)));

                if (query.ProductIds != null && query.ProductIds.Any())
                    dbQuery = dbQuery.Where(a => a.ReceiptSubjects.Any(c => query.ProductIds.Contains(c.ProductId)));

                var totalCount = dbQuery.Count();

                dbQuery = dbQuery.ApplayPageing(query);

                var result = await dbQuery.Select(r => new ListReceiptDto
                {
                    PackCode = r.Pack != null ? r.Pack.PackCode : "",
                    PackId = r.PackId,
                    ReceiptId = r.ReceiptId,
                    ReceiptCode = r.ReceiptCode,
                    SupplierId = r.SupplierId,
                    SupplierCode = r.Supplier != null ? r.Supplier.SupplierCode : "",
                    SupplierName = r.Supplier != null ? r.Supplier.Name : "",
                    SupplierImage = r.Supplier != null ? _appSettings.ClientHost + ServiceSetting.UploadImagesPath.LogoSmall + r.Supplier.Logo : "",
                    ReceiptStatus = r.ReceiptStatus,
                    UserAudit = r.AdderUser != null ? new UserAuditLogDto
                    {
                        AdderUserName = r.AdderUser.FullName,
                        AdderUserImage = r.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + r.AdderUser.Image : "",
                        CreateDate = r.CreatedDate.ToUnixTimestamp()
                    } : null,
                    ReceiptProducts = r.ReceiptSubjects.Select(c => new ReceiptSubjectDto
                    {
                        ProductName = c.Product.Description,
                        PartProductNames = c.ReceiptSubjectPartLists != null ? c.ReceiptSubjectPartLists
                       .Select(v => new ReceiptSubjectDto
                       {
                           ProductName = v.Product.Description
                       }).ToList() : new List<ReceiptSubjectDto>()
                    }).ToList(),
                }).ToListAsync();
                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<ListReceiptDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<ReceiptRejectInfoDto>> GetWaitingReceiptForRejectInfoByReceiptIdAsync(AuthenticateDto authenticate, long receiptId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<ReceiptRejectInfoDto>(null, MessageId.AccessDenied);

                var dbQuery = _receiptRepository
                    .AsNoTracking()
                   .Where(a => a.ReceiptId == receiptId &&
                   a.PO.BaseContractCode == authenticate.ContractCode &&
                   (a.ReceiptStatus == ReceiptStatus.QCRejected || a.ReceiptStatus == ReceiptStatus.ConditionalAcceptance) &&
                   !a.ReceiptSubjects.Any(c => c.ReceiptSubjectPartLists.Any()) &&
                   a.ReceiptSubjects.Any(v => v.PurchaseRejectRemainedQuantity > 0));

                var result = await dbQuery
                    .Select(r => new ReceiptRejectInfoDto
                    {
                        POCode = r.PO.POCode,
                        ReceiptId = r.ReceiptId,
                        ReceiptCode = r.ReceiptCode,
                        SupplierCode = r.Supplier != null ? r.Supplier.SupplierCode : "",
                        SupplierName = r.Supplier != null ? r.Supplier.Name : "",
                        SupplierImage = r.Supplier != null ? _appSettings.ClientHost + ServiceSetting.UploadImagesPath.LogoSmall + r.Supplier.Logo : "",
                        UserAudit = r.AdderUser != null
                         ? new UserAuditLogDto
                         {
                             AdderUserId = r.AdderUserId,
                             AdderUserName = r.AdderUser.FullName,
                             CreateDate = r.CreatedDate.ToUnixTimestamp(),
                             AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall +
                                              r.AdderUser.Image
                         }
                         : null,
                        ReceiptRejectSubjects = r.ReceiptSubjects.Where(b => b.PurchaseRejectRemainedQuantity > 0)
                        .Select(p => new ReceiptRejectSubjectDto
                        {
                            ProductId = p.ProductId,
                            ReceiptQuantity = p.ReceiptQuantity,
                            Quantity = p.PurchaseRejectRemainedQuantity,
                            PurchaseRejectRemainedQuantity = p.PurchaseRejectRemainedQuantity,
                            ProductCode = p.Product.ProductCode,
                            ProductName = p.Product.Description,
                            ProductUnit = p.Product.Unit,
                            ReceiptRejectSubjects = p.ReceiptSubjectPartLists.Where(v => v.PurchaseRejectRemainedQuantity > 0).Select(c => new ReceiptRejectSubjectDto
                            {
                                ProductId = c.ProductId,
                                Quantity = c.PurchaseRejectRemainedQuantity,
                                PurchaseRejectRemainedQuantity = c.PurchaseRejectRemainedQuantity,
                                ReceiptQuantity = c.ReceiptQuantity,
                                ProductCode = c.Product.ProductCode,
                                ProductName = c.Product.Description,
                                ProductUnit = c.Product.Unit,
                            }).ToList()
                        }).ToList(),
                        QualityControl = null,
                        ReceiptRejectAttachments = null,
                    }).FirstOrDefaultAsync();
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<ReceiptRejectInfoDto>(null, exception);
            }
        }

        public async Task<ServiceResult<bool>> AddReceiptRejectAsync(AuthenticateDto authenticate, long receiptId, AddReceiptRejectDto model)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                if (model == null || model.RejectSubjects == null)
                    return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                var receiptModel = await _receiptRepository
                    .Where(a => a.ReceiptId == receiptId &&
                    a.PO.BaseContractCode == authenticate.ContractCode &&
                    (a.ReceiptStatus == ReceiptStatus.ConditionalAcceptance || a.ReceiptStatus == ReceiptStatus.QCRejected))
                    .Include(a => a.Pack)
                    .Include(a => a.PO)
                    .ThenInclude(a => a.Supplier)
                    .FirstOrDefaultAsync();

                var receiptSubjectModel = await _receiptSubjectRepository
                    .Where(a => a.ReceiptId == receiptId && a.Receipt.PO.BaseContractCode == authenticate.ContractCode)
                    .Include(a => a.ReceiptSubjectPartLists)
                    .ToListAsync();

                if (receiptModel == null || receiptSubjectModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                var postedSubjectProductIds = model.RejectSubjects.Select(a => a.ProductId).ToList();

                var receiptSubjectProductIds = receiptSubjectModel.Select(a => a.ProductId).ToList();

                if (postedSubjectProductIds.Any(c => !receiptSubjectProductIds.Contains(c)))
                    return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                var postReceiptProductIds = new List<int>();
                foreach (var item in model.RejectSubjects)
                {
                    if (item.Parts == null || !item.Parts.Any())
                        postReceiptProductIds.Add(item.ProductId);
                    else
                    {
                        foreach (var part in item.Parts)
                        {
                            postReceiptProductIds.Add(part.ProductId);
                        }
                    }
                }
                postReceiptProductIds = postReceiptProductIds.Distinct().ToList();

                var warehouseProducts = await _warehouseProductRepository
                    .Where(a => postReceiptProductIds.Contains(a.ProductId)).ToListAsync();

                if (warehouseProducts == null || warehouseProducts.Count() != postReceiptProductIds.Count())
                    return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                var receiptRejectModel = new ReceiptReject
                {
                    ReceiptId = receiptModel.ReceiptId,
                    PackId = receiptModel.PackId,
                    POId = receiptModel.POId,
                    SupplierId = receiptModel.SupplierId,
                    Note = model.Note,
                    ReceiptRejectAttachments = new List<PAttachment>(),
                    ReceiptRejectSubjects = new List<ReceiptRejectSubject>()
                };

                var receiptRejectSubjects = new List<ReceiptRejectSubject>();
                foreach (var postedSubject in model.RejectSubjects)
                {
                    var selectedReceiptSubjectModel = receiptSubjectModel.FirstOrDefault(a => a.ProductId == postedSubject.ProductId);
                    if (selectedReceiptSubjectModel == null)
                        return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);
                    var newRejectSubjectMiodel = new ReceiptRejectSubject();
                    newRejectSubjectMiodel.ProductId = postedSubject.ProductId;
                    if (selectedReceiptSubjectModel.ReceiptSubjectPartLists == null || !selectedReceiptSubjectModel.ReceiptSubjectPartLists.Any())
                    {
                        if (selectedReceiptSubjectModel.PurchaseRejectRemainedQuantity <= 0)
                            return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                        if (postedSubject.Quantity > selectedReceiptSubjectModel.PurchaseRejectRemainedQuantity)
                            return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                        selectedReceiptSubjectModel.PurchaseRejectRemainedQuantity -= postedSubject.Quantity;
                        newRejectSubjectMiodel.Quantity = postedSubject.Quantity;
                        newRejectSubjectMiodel.ReceiptQuantity = selectedReceiptSubjectModel.ReceiptQuantity;

                        var wProduct = warehouseProducts.FirstOrDefault(a => a.ProductId == postedSubject.ProductId);
                        if (wProduct == null)
                            return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                        wProduct.Inventory -= postedSubject.Quantity;

                        _warehouseProductStockLogsRepository.Add(new WarehouseProductStockLogs
                        {
                            Input = 0,
                            Output = postedSubject.Quantity,
                            WarehouseStockChangeActionType = WarehouseStockChangeActionType.RejectReceipt,
                            DateChange = DateTime.UtcNow,
                            RealStock = wProduct.Inventory,
                            ProductId = newRejectSubjectMiodel.ProductId,
                            WarehouseTransference = receiptRejectModel,
                        });

                        receiptRejectSubjects.Add(newRejectSubjectMiodel);
                    }
                    else
                    {
                        if (postedSubject.Parts == null || !postedSubject.Parts.Any())
                            return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                        newRejectSubjectMiodel.ReceiptRejectSubjectPartLists = new List<ReceiptRejectSubject>();
                        foreach (var postedItem in postedSubject.Parts)
                        {
                            var selectedpart = selectedReceiptSubjectModel.ReceiptSubjectPartLists.FirstOrDefault(a => a.ProductId == postedItem.ProductId);
                            if (selectedpart == null)
                                return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                            if (selectedpart.PurchaseRejectRemainedQuantity <= 0)
                                return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                            if (postedItem.Quantity <= 0)
                                return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                            if (postedItem.Quantity > selectedpart.PurchaseRejectRemainedQuantity)
                                return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                            selectedpart.PurchaseRejectRemainedQuantity -= postedItem.Quantity;

                            newRejectSubjectMiodel.ReceiptRejectSubjectPartLists.Add(new ReceiptRejectSubject
                            {
                                Quantity = postedItem.Quantity,
                                ProductId = postedItem.ProductId,
                                ReceiptQuantity = selectedpart.ReceiptQuantity
                            });

                            var wProduct = warehouseProducts.FirstOrDefault(a => a.ProductId == postedItem.ProductId);
                            if (wProduct == null)
                                return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                            wProduct.Inventory -= postedItem.Quantity;

                            _warehouseProductStockLogsRepository.Add(new WarehouseProductStockLogs
                            {
                                Input = 0,
                                Output = postedSubject.Quantity,
                                WarehouseStockChangeActionType = WarehouseStockChangeActionType.RejectReceipt,
                                DateChange = DateTime.UtcNow,
                                RealStock = wProduct.Inventory,
                                ProductId = newRejectSubjectMiodel.ProductId,
                                WarehouseTransference = receiptRejectModel,
                            });

                        }
                        receiptRejectSubjects.Add(newRejectSubjectMiodel);
                    }
                }

                receiptRejectModel.ReceiptRejectSubjects = receiptRejectSubjects;

                //add attachment
                if (model.Attachments != null && model.Attachments.Any())
                {
                    if (!_fileHelper.FileExistInTemp(model.Attachments.Select(c => c.FileSrc).ToList()))
                        return ServiceResultFactory.CreateError(false, MessageId.FileNotFound);
                    var attachmentResult = await AddReceiptRejectAttachmentAsync(receiptRejectModel, model.Attachments);
                    if (!attachmentResult.Succeeded)
                        return ServiceResultFactory.CreateError(false,
                            attachmentResult.Messages.FirstOrDefault().Message);

                    receiptRejectModel = attachmentResult.Result;
                }

                receiptRejectModel.Note = model.Note;

                // generate form code
                var count = await _receiptRejectRepository.CountAsync(a => a.PO.BaseContractCode == authenticate.ContractCode);
                var codeRes = await _formConfigService.GenerateFormCodeAsync(authenticate.ContractCode, FormName.ReceiptReject, count);
                if (!codeRes.Succeeded)
                    return ServiceResultFactory.CreateError(false, codeRes.Messages.First().Message);

                receiptRejectModel.ReceiptRejectCode = codeRes.Result;

                _receiptRejectRepository.Add(receiptRejectModel);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    if (!receiptSubjectModel.Any(a => a.PurchaseRejectRemainedQuantity > 0))
                        await _scmLogAndNotificationService.SetDonedNotificationByRootKeyValueAsync(authenticate.UserId, receiptModel.PO.BaseContractCode, receiptModel.ReceiptId.ToString(), NotifEvent.AddReceiptReject);
                    var res = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = receiptModel.PO.BaseContractCode,
                        FormCode = receiptModel.ReceiptCode,
                        RootKeyValue2 = receiptRejectModel.ReceiptRejectCode,
                        KeyValue = receiptRejectModel.ReceiptRejectId.ToString(),
                        RootKeyValue = receiptModel.PO.POId.ToString(),
                        Description = receiptModel.PO.POCode,
                        Temp = receiptModel.Pack.PackCode,
                        Message = receiptModel.PO.Supplier.Name,
                        Quantity = ((int)WaitingForInvoiceType.ReceiptReject).ToString(),
                        NotifEvent = NotifEvent.AddReceiptReject,
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,
                    },
                    new List<NotifToDto> {
                            new NotifToDto
                            {
                                NotifEvent=NotifEvent.AddInvoice,
                                Roles= new List<string>
                                {
                                    SCMRole.InvoiceMng,
                                }
                            }
                    });

                    return ServiceResultFactory.CreateSuccess(true);
                }

                return ServiceResultFactory.CreateError(false, MessageId.SaveFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<List<ListReceiptRejectDto>>> GetReceiptRejectListAsync(AuthenticateDto authenticate, ReceiptQueryDto query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(new List<ListReceiptRejectDto>(), MessageId.AccessDenied);

                var dbQuery = _receiptRejectRepository
                    .AsNoTracking()
                    .OrderByDescending(a => a.ReceiptRejectId)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(query.SearchText))
                {
                    dbQuery = dbQuery.Where(a => a.ReceiptRejectCode.Contains(query.SearchText) ||
                    ((a.Supplier != null && a.Supplier.Name.Contains(query.SearchText)) || a.Supplier == null) ||
                    a.ReceiptRejectSubjects.Any(b => (b.ReceiptRejectSubjectPartLists == null && b.Product.Description.Contains(query.SearchText)) ||
                    (b.ReceiptRejectSubjectPartLists != null && b.ReceiptRejectSubjectPartLists.Any(v => v.Product.Description.Contains(query.SearchText)))));
                }

                var totalCount = dbQuery.Count();
                var columnsMap = new Dictionary<string, Expression<Func<ReceiptReject, object>>>
                {
                    ["ReceiptRejectId"] = v => v.ReceiptRejectId,
                    ["ReceiptRejectCode"] = v => v.ReceiptRejectCode,
                };

                dbQuery = dbQuery.ApplayOrdering(query, columnsMap).ApplayPageing(query);

                var result = await dbQuery.Select(r => new ListReceiptRejectDto
                {
                    ReceiptRejectId = r.ReceiptRejectId,
                    ReceiptRejectCode = r.ReceiptRejectCode,
                    ReceiptCode = r.Receipt.ReceiptCode,
                    SupplierName = r.Supplier != null ? r.Supplier.Name : "",
                    UserAudit = r.AdderUser != null ? new UserAuditLogDto
                    {
                        AdderUserName = r.AdderUser.FullName,
                        AdderUserImage = r.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + r.AdderUser.Image : "",
                        CreateDate = r.CreatedDate.ToUnixTimestamp()
                    } : null,
                    ReceiptProducts = r.ReceiptRejectSubjects.Select(c => new ReceiptSubjectDto
                    {
                        ProductName = c.Product.Description,
                        PartProductNames = c.ReceiptRejectSubjectPartLists != null ? c.ReceiptRejectSubjectPartLists
                       .Select(v => new ReceiptSubjectDto
                       {
                           ProductName = v.Product.Description
                       }).ToList() : new List<ReceiptSubjectDto>()
                    }).ToList(),
                }).ToListAsync();
                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<ListReceiptRejectDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<ReceiptRejectInfoDto>> GetReceiptRejectInfoByIdAsync(AuthenticateDto authenticate, long ReceiptRejectId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<ReceiptRejectInfoDto>(null, MessageId.AccessDenied);

                var dbQuery = _receiptRejectRepository
                    .AsNoTracking()
                    .Where(a => a.ReceiptRejectId == ReceiptRejectId && a.PO.BaseContractCode == authenticate.ContractCode);

                var result = await dbQuery
                    .Select(r => new ReceiptRejectInfoDto
                    {
                        ReceiptId = r.Receipt.ReceiptId,
                        ReceiptRejectId = r.ReceiptRejectId,
                        POCode = r.PO.POCode,
                        ReceiptCode = r.Receipt.ReceiptCode,
                        Note = r.Note,
                        DateReceipted = r.Receipt.CreatedDate.ToUnixTimestamp(),
                        ReceiptRejectCode = r.ReceiptRejectCode,
                        SupplierCode = r.Supplier != null ? r.Supplier.SupplierCode : "",
                        SupplierName = r.Supplier != null ? r.Supplier.Name : "",
                        SupplierImage = r.Supplier != null ? _appSettings.ClientHost + ServiceSetting.UploadImagesPath.LogoSmall + r.Supplier.Logo : "",
                        UserAudit = r.AdderUser != null
                         ? new UserAuditLogDto
                         {
                             AdderUserId = r.AdderUserId,
                             AdderUserName = r.AdderUser.FullName,
                             CreateDate = r.CreatedDate.ToUnixTimestamp(),
                             AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall +
                                              r.AdderUser.Image
                         }
                         : null,
                        ReceiptRejectSubjects = r.ReceiptRejectSubjects
                        .Select(p => new ReceiptRejectSubjectDto
                        {
                            ReceiptRejectSubjectId = p.ReceiptRejectSubjectId,
                            ProductId = p.ProductId,
                            ReceiptQuantity = p.ReceiptQuantity,
                            ProductCode = p.Product.ProductCode,
                            ProductName = p.Product.Description,
                            ProductUnit = p.Product.Unit,
                            Quantity = p.Quantity,
                            ReceiptRejectSubjects = p.ReceiptRejectSubjectPartLists.Select(c => new ReceiptRejectSubjectDto
                            {
                                ReceiptRejectSubjectId = c.ReceiptRejectSubjectId,
                                ProductId = c.ProductId,
                                ReceiptQuantity = c.ReceiptQuantity,
                                ProductCode = c.Product.ProductCode,
                                ProductName = c.Product.Description,
                                ProductUnit = c.Product.Unit,
                                Quantity = c.Quantity,
                            }).ToList()
                        }).ToList(),
                        ReceiptRejectAttachments = r.ReceiptRejectAttachments.Select(a => new ReceiptAttachmentDto
                        {
                            Id = a.Id,
                            FileName = a.FileName,
                            FileSize = a.FileSize,
                            FileSrc = a.FileSrc,
                            FileType = a.FileType,
                            ReceiptId = a.ReceiptId.Value,
                        }).ToList(),
                        QualityControl = r.Receipt.QualityControl != null ? new ReceiptQualityControlDto
                        {
                            Id = r.Receipt.QualityControl.Id,
                            Note = r.Receipt.QualityControl.Note,
                            QCResult = r.Receipt.QualityControl.QCResult,
                            ReceiptId = r.Receipt.QualityControl.ReceiptId.Value,
                            UserAudit = r.Receipt.QualityControl.AdderUser != null ? new UserAuditLogDto
                            {
                                AdderUserId = r.Receipt.QualityControl.AdderUserId,
                                AdderUserName = r.Receipt.QualityControl.AdderUser.FullName,
                                CreateDate = r.Receipt.QualityControl.CreatedDate.ToUnixTimestamp(),
                                AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall +
                                              r.Receipt.QualityControl.AdderUser.Image
                            } : null,
                        } : null
                    }).FirstOrDefaultAsync();

                if (result != null && result.QualityControl != null)
                {
                    result.QualityControl.Attachments = await _pAttachmentRepository
                        .AsNoTracking()
                        .Where(a => !a.IsDeleted && a.QualityControlId == result.QualityControl.Id)
                        .Select(at => new BaseQualityControlAttachmentDto
                        {
                            Id = at.Id,
                            FileName = at.FileName,
                            FileSize = at.FileSize,
                            FileType = at.FileType,
                            FileSrc = at.FileSrc,
                            QualityControlId = at.QualityControlId.Value
                        })
                        .ToListAsync();
                }


                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<ReceiptRejectInfoDto>(null, exception);
            }
        }

        #endregion

        public async Task<ServiceResult<List<ListReceiptDto>>> GetReceiptListAsync(AuthenticateDto authenticate, ReceiptQueryDto query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<ListReceiptDto>>(null, MessageId.AccessDenied);

                var dbQuery = _receiptRepository
                    .AsNoTracking()
                    .Where(a => a.PO.BaseContractCode == authenticate.ContractCode)
                    .OrderByDescending(a => a.ReceiptId)
                    .AsQueryable();


                if (!string.IsNullOrEmpty(query.SearchText))
                {
                    dbQuery = dbQuery.Where(a => a.ReceiptCode.Contains(query.SearchText) ||
                    ((a.Supplier != null && a.Supplier.Name.Contains(query.SearchText)) || a.Supplier == null) ||
                    a.ReceiptSubjects.Any(b => b.Product.Description.Contains(query.SearchText) || b.Product.ProductCode.Contains(query.SearchText)));
                }

                var totalCount = dbQuery.Count();
                var columnsMap = new Dictionary<string, Expression<Func<Receipt, object>>>
                {
                    ["ReceiptId"] = v => v.ReceiptId,
                    ["ReceiptCode"] = v => v.ReceiptCode,
                };

                dbQuery = dbQuery.ApplayOrdering(query, columnsMap).ApplayPageing(query);

                var result = await dbQuery.Select(r => new ListReceiptDto
                {
                    ReceiptId = r.ReceiptId,
                    ReceiptCode = r.ReceiptCode,
                    SupplierName = r.Supplier != null ? r.Supplier.Name : "",
                    ReceiptStatus = r.ReceiptStatus,
                    UserAudit = r.AdderUser != null ? new UserAuditLogDto
                    {
                        AdderUserName = r.AdderUser.FullName,
                        AdderUserImage = r.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + r.AdderUser.Image : "",
                        CreateDate = r.CreatedDate.ToUnixTimestamp()
                    } : null,
                    ReceiptProducts = r.ReceiptSubjects.Select(c => new ReceiptSubjectDto
                    {
                        ProductName = c.Product.Description,
                        PartProductNames = c.ReceiptSubjectPartLists != null ? c.ReceiptSubjectPartLists
                       .Select(v => new ReceiptSubjectDto
                       {
                           ProductName = v.Product.Description
                       }).ToList() : new List<ReceiptSubjectDto>()
                    }).ToList(),
                }).ToListAsync();
                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<ListReceiptDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<ReceiptInfoDto>> GetReceiptInfoByIdForAddQCAsync(AuthenticateDto authenticate, long receiptId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<ReceiptInfoDto>(null, MessageId.AccessDenied);

                var dbQuery = _receiptRepository
                    .AsNoTracking()
                    .Where(a => a.ReceiptId == receiptId &&
                    a.ReceiptStatus == ReceiptStatus.PendingForQC &&
                    a.PO.BaseContractCode == authenticate.ContractCode);

                var result = await dbQuery
                    .Select(r => new ReceiptInfoDto
                    {
                        POCode = r.PO.POCode,
                        PackCode = r.Pack != null ? r.Pack.PackCode : "",
                        PackId = r.PackId,
                        ReceiptId = r.ReceiptId,
                        ReceiptCode = r.ReceiptCode,
                        SupplierId = r.SupplierId,
                        SupplierCode = r.Supplier != null ? r.Supplier.SupplierCode : "",
                        SupplierName = r.Supplier != null ? r.Supplier.Name : "",
                        SupplierImage = r.Supplier != null ? _appSettings.ClientHost + ServiceSetting.UploadImagesPath.LogoSmall + r.Supplier.Logo : "",
                        ReceiptStatus = r.ReceiptStatus,
                        UserAudit = r.AdderUser != null
                         ? new UserAuditLogDto
                         {
                             AdderUserId = r.AdderUserId,
                             AdderUserName = r.AdderUser.FullName,
                             CreateDate = r.CreatedDate.ToUnixTimestamp(),
                             AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall +
                                              r.AdderUser.Image
                         }
                         : null,
                        ReceiptSubjects = r.ReceiptSubjects
                        .Select(p => new ReceiptPackSubjectDto
                        {
                            ReceiptSubjectId = p.ReceiptSubjectId,
                            ProductId = p.ProductId,
                            PackQuantity = p.PackQuantity,
                            QCReceiptQuantity = p.ReceiptQuantity,
                            ReceiptQuantity = p.ReceiptQuantity,
                            ProductCode = p.Product.ProductCode,
                            ProductName = p.Product.Description,
                            ProductUnit = p.Product.Unit,
                            ReceiptPackSubjects = p.ReceiptSubjectPartLists.Select(c => new ReceiptPackSubjectDto
                            {
                                ReceiptSubjectId = c.ReceiptSubjectId,
                                ProductId = c.ProductId,
                                PackQuantity = c.PackQuantity,
                                QCReceiptQuantity = c.ReceiptQuantity,
                                ReceiptQuantity = c.ReceiptQuantity,
                                ProductCode = c.Product.ProductCode,
                                ProductName = c.Product.Description,
                                ProductUnit = c.Product.Unit,
                            }).ToList()
                        }).ToList(),
                        ReceiptAttachments = null,
                        QualityControl = null,

                    }).FirstOrDefaultAsync();
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<ReceiptInfoDto>(null, exception);
            }
        }


        public async Task<ServiceResult<ReceiptInfoDto>> GetReceiptInfoByIdAsync(AuthenticateDto authenticate, long ReceiptId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<ReceiptInfoDto>(null, MessageId.AccessDenied);

                var dbQuery = _receiptRepository
                    .AsNoTracking()
                    .Where(a => a.ReceiptId == ReceiptId && a.PO.BaseContractCode == authenticate.ContractCode);

                var result = await dbQuery
                    .Select(r => new ReceiptInfoDto
                    {
                        PackCode = r.Pack != null ? r.Pack.PackCode : "",
                        PackId = r.PackId,
                        ReceiptId = r.ReceiptId,
                        ReceiptCode = r.ReceiptCode,
                        Note = r.Note,
                        POCode=r.PO.POCode,
                        ProductGroupTitle=r.PO.ProductGroup.Title,
                        ReceiptStatus = r.ReceiptStatus,
                        SupplierId = r.SupplierId,
                        SupplierCode = r.Supplier != null ? r.Supplier.SupplierCode : "",
                        SupplierName = r.Supplier != null ? r.Supplier.Name : "",
                        SupplierImage = r.Supplier != null ? _appSettings.ClientHost + ServiceSetting.UploadImagesPath.LogoSmall + r.Supplier.Logo : "",
                        UserAudit = r.AdderUser != null
                         ? new UserAuditLogDto
                         {
                             AdderUserId = r.AdderUserId,
                             AdderUserName = r.AdderUser.FullName,
                             CreateDate = r.CreatedDate.ToUnixTimestamp(),
                             AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall +
                                              r.AdderUser.Image
                         }
                         : null,
                        ReceiptSubjects = r.ReceiptSubjects
                        .Select(p => new ReceiptPackSubjectDto
                        {
                            ReceiptSubjectId = p.ReceiptSubjectId,
                            ProductId = p.ProductId,
                            PackQuantity = p.PackQuantity,
                            QCReceiptQuantity = p.QCAcceptQuantity,
                            ReceiptQuantity = p.ReceiptQuantity,
                            ProductCode = p.Product.ProductCode,
                            ProductName = p.Product.Description,
                            ProductUnit = p.Product.Unit,
                            ReceiptPackSubjects = p.ReceiptSubjectPartLists.Select(c => new ReceiptPackSubjectDto
                            {
                                ReceiptSubjectId = c.ReceiptSubjectId,
                                ProductId = c.ProductId,
                                PackQuantity = c.PackQuantity,
                                QCReceiptQuantity = c.QCAcceptQuantity,
                                ReceiptQuantity = c.ReceiptQuantity,
                                ProductCode = c.Product.ProductCode,
                                ProductName = c.Product.Description,
                                ProductUnit = c.Product.Unit,
                            }).ToList()
                        }).ToList(),
                        QualityControl = r.QualityControl != null ? new ReceiptQualityControlDto
                        {
                            Id = r.QualityControl.Id,
                            Note = r.QualityControl.Note,
                            QCResult = (r.ReceiptStatus== ReceiptStatus.QCRejected)?(QCResult)0: (r.ReceiptStatus == ReceiptStatus.QCPassed)?(QCResult)1:(r.ReceiptStatus== ReceiptStatus.ConditionalAcceptance)?(QCResult.ConditionalAcceptance):(QCResult)3,
                            ReceiptId = r.QualityControl.ReceiptId.Value,
                            UserAudit = r.QualityControl.AdderUser != null ? new UserAuditLogDto
                            {
                                AdderUserId = r.QualityControl.AdderUserId,
                                AdderUserName = r.QualityControl.AdderUser.FullName,
                                CreateDate = r.QualityControl.CreatedDate.ToUnixTimestamp(),
                                AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall +
                                              r.QualityControl.AdderUser.Image
                            } : null,
                        } : null
                    }).FirstOrDefaultAsync();

                if (result != null && result.QualityControl != null)
                {
                    result.QualityControl.Attachments = await _pAttachmentRepository
                        .AsNoTracking()
                        .Where(a => !a.IsDeleted && a.QualityControlId == result.QualityControl.Id)
                        .Select(at => new BaseQualityControlAttachmentDto
                        {
                            Id = at.Id,
                            FileName = at.FileName,
                            FileSize = at.FileSize,
                            FileType = at.FileType,
                            FileSrc = at.FileSrc,
                            QualityControlId = at.QualityControlId.Value
                        })
                        .ToListAsync();
                }


                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<ReceiptInfoDto>(null, exception);
            }
        }

        public async Task<ServiceResult<bool>> AddReceiptQualityControlAsync(AuthenticateDto authenticate, long receiptId, AddQCReceiptDto model)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var receiptModel = await _receiptRepository
                    .Where(a => a.ReceiptId == receiptId && a.ReceiptStatus == ReceiptStatus.PendingForQC && a.PO.BaseContractCode == authenticate.ContractCode)
                    .Include(a => a.Pack)
                    .Include(a => a.ReceiptSubjects)
                    .ThenInclude(a => a.ReceiptSubjectPartLists)
                    .FirstOrDefaultAsync();

                if (receiptModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                var poModel = await _poRepository.Where(a => a.POId == receiptModel.POId)
                    .Include(a => a.Supplier)
                    .Include(a => a.POSubjects)
                    .Include(a => a.POSubjects)
                    .ThenInclude(c => c.MrpItem)
                    .FirstOrDefaultAsync();

                if (poModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (receiptModel.ReceiptStatus != ReceiptStatus.PendingForQC)
                    return ServiceResultFactory.CreateError(false, MessageId.ImpossibleAddQC);

                if (receiptModel.ReceiptSubjects == null)
                    return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                if (receiptModel.ReceiptSubjects.Count() != model.ReceiptSubjects.Count())
                    return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                var postReceiptProductIds = new List<int>();
                foreach (var item in model.ReceiptSubjects)
                {

                    postReceiptProductIds.Add(item.ProductId);

                }
                postReceiptProductIds = postReceiptProductIds.Distinct().ToList();
                var prodcutGroup = await _prodcutGroupRepository.Where(a => a.Products.Any(a => a.Id == postReceiptProductIds.First())).FirstOrDefaultAsync();
                var qcResult = QCResult.Accept;

                if (!model.ReceiptSubjects.Any(c => (c.QCReceiptQuantity > 0)))
                    qcResult = QCResult.Reject;
                else if (model.ReceiptSubjects.Any(c => (c.QCReceiptQuantity != c.ReceiptQuantity)))
                    qcResult = QCResult.ConditionalAcceptance;

                if (receiptModel.ReceiptSubjects.Any(c => c.ReceiptSubjectPartLists.Any()) && qcResult != QCResult.Accept)
                    return ServiceResultFactory.CreateError(false, MessageId.ImpossibleRejectPartListReceipt);

                var warehouseProducts = await _warehouseProductRepository
                    .Where(a => postReceiptProductIds.Contains(a.ProductId)).ToListAsync();

                if (warehouseProducts == null || warehouseProducts.Count() != postReceiptProductIds.Count())
                    return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                //before change => for get log
                //string oldObject = _scmLogAndNotificationService.SerializerObject(receiptModel);
                WarehouseOutputRequest requsetModel = new WarehouseOutputRequest();
                var qualityControlEntity = new QualityControl
                {
                    ReceiptId = receiptModel.ReceiptId,
                    QCResult = qcResult,
                    Note = model.Note,
                    QCAttachments = new List<PAttachment>()
                };
                if (poModel.POStatus == POStatus.Delivered)

                    //add attachment
                    if (model.Attachments != null && model.Attachments.Any())
                    {
                        if (!_fileHelper.FileExistInTemp(model.Attachments.Select(v => v.FileSrc).ToList()))
                            return ServiceResultFactory.CreateError(false, MessageId.FileNotFound);
                        var attachmentResult = await AddQualityControlAttachmentAsync(qualityControlEntity, model.Attachments);
                        if (!attachmentResult.Succeeded)
                            return ServiceResultFactory.CreateError(false,
                                attachmentResult.Messages.FirstOrDefault().Message);

                        qualityControlEntity = attachmentResult.Result;
                    }

                var warehouseTransferenceProducts = new List<ReceiptRejectSubject>();
                if (qcResult == QCResult.Accept)
                {
                    var res = UpdateWarhouseProductByAcceptQualityControl(receiptModel, warehouseProducts);
                    if (res == false)
                        return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);
                }
                else if (qcResult == QCResult.Reject)
                {
                    var res = UpdateWarehouseProductAndReceiptByRejectQualityControlAsync(receiptModel, warehouseProducts, poModel.POSubjects);
                    if (!res.Succeeded)
                        return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);
                    if(poModel.POStatus==POStatus.Delivered)
                        poModel.ShortageStatus = POShortageStatus.RecepitQcShortage;
                    AddWarehouseOutputRequestDto wharehouseOutputRequest = new AddWarehouseOutputRequestDto();
                    wharehouseOutputRequest.Subjects = receiptModel.ReceiptSubjects.Select(a => new WarehouseOutputRequestSubjecDto
                    {
                        ProductId = a.ProductId,
                        Quantity = a.PurchaseRejectRemainedQuantity,
                    }).ToList();
                    wharehouseOutputRequest.RecepitId = receiptModel.ReceiptId;
                    wharehouseOutputRequest.RecepitCode = receiptModel.ReceiptCode;
                    var addWharehouseOutputRequest = await AddWarehouseOutputRequest(authenticate, wharehouseOutputRequest);
                    if(!addWharehouseOutputRequest.Succeeded)
                        return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);
                    requsetModel = addWharehouseOutputRequest.Result;
                    await _warehouseOutputRequestRepository.AddAsync(addWharehouseOutputRequest.Result);
                }
                else if (qcResult == QCResult.ConditionalAcceptance)
                {
                    var res = UpdatePOSubjectAndWarehouseProductAndReceiptByConditionalQualityControlAsync(receiptModel, warehouseProducts, model.ReceiptSubjects, poModel.POSubjects);
                    if (!res.Succeeded)
                        return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);
                    if (poModel.POStatus == POStatus.Delivered)
                        poModel.ShortageStatus = POShortageStatus.RecepitQcShortage;

                    AddWarehouseOutputRequestDto wharehouseOutputRequest = new AddWarehouseOutputRequestDto();
                    wharehouseOutputRequest.Subjects = receiptModel.ReceiptSubjects.Select(a => new WarehouseOutputRequestSubjecDto
                    {
                        ProductId = a.ProductId,
                        Quantity = a.PurchaseRejectRemainedQuantity,
                    }).ToList();
                    wharehouseOutputRequest.RecepitId = receiptModel.ReceiptId;
                    wharehouseOutputRequest.RecepitCode = receiptModel.ReceiptCode;
                    var addWharehouseOutputRequest = await AddWarehouseOutputRequest(authenticate, wharehouseOutputRequest);
                    if (!addWharehouseOutputRequest.Succeeded)
                        return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);
                    requsetModel = addWharehouseOutputRequest.Result;
                    await _warehouseOutputRequestRepository.AddAsync(addWharehouseOutputRequest.Result);

                }
                else
                {
                    return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);
                }
                _qualityControlRepository.Add(qualityControlEntity);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    await _scmLogAndNotificationService.SetDonedNotificationAsync(authenticate.UserId, poModel.BaseContractCode, receiptModel.ReceiptId.ToString(), NotifEvent.AddReceiptQC);
                    var res = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = poModel.BaseContractCode,
                        FormCode = receiptModel.ReceiptCode,
                        KeyValue = receiptModel.ReceiptId.ToString(),
                        RootKeyValue = receiptModel.ReceiptId.ToString(),
                        NotifEvent = NotifEvent.AddReceiptQC,
                        Description = poModel.POCode,
                        Temp = receiptModel.Pack.PackCode,
                        Message = poModel.Supplier.Name,
                        Quantity = qcResult.ToString(),
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,
                    },
                    null);

                    if (qcResult == QCResult.Reject || qcResult == QCResult.ConditionalAcceptance)
                    {
                       
                            await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                            {
                                ContractCode = requsetModel.ContractCode,
                                FormCode = requsetModel.RequestCode,
                                KeyValue = requsetModel.RequestId.ToString(),
                                NotifEvent = NotifEvent.ConfirmWarehouseOutputRequest,
                                Description = "2",
                                ProductGroupId = prodcutGroup.Id,
                                RootKeyValue = requsetModel.RequestId.ToString(),
                                Message = prodcutGroup.Title,
                                PerformerUserId = authenticate.UserId,
                                PerformerUserFullName = authenticate.UserFullName,

                            },
                        prodcutGroup.Id
                        , ConfirmSendNotification());
                        
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

        public async Task<DownloadFileDto> DownloadReceiptRejectAttachmentAsync(AuthenticateDto authenticate, long receiptRejectId, string fileSrc)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return null;

                var dbQuery = _receiptRejectRepository
                    .Where(a => !a.IsDeleted && a.ReceiptRejectId == receiptRejectId &&
                    a.ReceiptRejectAttachments.Any(c => !c.IsDeleted && c.FileSrc == fileSrc));

                var contractCode = await dbQuery
                    .Select(c => c.Pack.PO.BaseContractCode)
                    .FirstOrDefaultAsync();

                if (contractCode == null)
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

        public async Task<DownloadFileDto> DownloadReceiptQualityControleAttachmentAsync(AuthenticateDto authenticate, long receiptId, string fileSrc)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return null;

                var dbQuery = _receiptRepository
                    
                    .Where(a => !a.IsDeleted && a.ReceiptId == receiptId && a.QualityControl != null &&
                    a.QualityControl.QCAttachments.Any(c => !c.IsDeleted && c.FileSrc == fileSrc));

                var contractCode = await dbQuery
                    .Select(c => c.Pack.PO.BaseContractCode)
                    .FirstOrDefaultAsync();

                if (contractCode == null)
                    return null;
                var receipt = await dbQuery.Include(a=>a.QualityControl).ThenInclude(a=>a.QCAttachments).FirstOrDefaultAsync();
                if (receipt == null)
                    return null;
                var targetFile = receipt.QualityControl.QCAttachments.Where(a => a.FileSrc == fileSrc).FirstOrDefault();
                if (targetFile == null)
                    return null;
                var streamResult = await _fileHelper.DownloadAttachmentDocument(fileSrc, ServiceSetting.FileSection.PO,targetFile.FileName);
                if (streamResult == null)
                    return null;

                return streamResult;
            }
            catch (Exception exception)
            {
                return null;
            }
        }

        private static List<NotifToDto> ReturnNotificationOnAddReceiptQc(ReceiptStatus status)
        {
            if (status == ReceiptStatus.QCPassed)
                return null;
            else
                return new List<NotifToDto> {
                    new NotifToDto
                    {
                        NotifEvent=NotifEvent.AddReceiptReject,
                        Roles= new List<string>
                        {
                           SCMRole.WarehouseMng,
                        }
                    }
                    };
        }

        public async Task<ServiceResult<List<ReportReceiptProductDto>>> GetReportReceiptProductByPoIdAsync(AuthenticateDto authenticate, long poId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<ReportReceiptProductDto>>(null, MessageId.AccessDenied);

                var poModel = await _poRepository.Where(a => !a.IsDeleted && a.POId == poId)
                    .FirstOrDefaultAsync();
                if (poModel == null)
                    return ServiceResultFactory.CreateError<List<ReportReceiptProductDto>>(null, MessageId.EntityDoesNotExist);

                var dbQuery = _receiptRepository
                    .Where(a => a.POId == poId);

                var result = await dbQuery
                    .Select(r => new ReportReceiptProductDto
                    {
                        PackCode = r != null ? r.Pack.PackCode : "",
                        ReceiptId = r.ReceiptId,
                        ReceiptCode = r.ReceiptCode,
                        ReceiptStatus = r.ReceiptStatus,
                        UserAudit = r.AdderUser != null
                         ? new UserAuditLogDto
                         {
                             AdderUserId = r.AdderUserId,
                             AdderUserName = r.AdderUser.FullName,
                             CreateDate = r.CreatedDate.ToUnixTimestamp(),
                             AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall +
                                              r.AdderUser.Image
                         }
                         : null,
                        ReceiptSubjects = r.ReceiptSubjects
                        .Select(p => new ReceiptPackSubjectDto
                        {
                            ReceiptSubjectId = p.ReceiptSubjectId,
                            ProductId = p.ProductId,
                            PackQuantity = p.PackQuantity,
                            QCReceiptQuantity = p.QCAcceptQuantity,
                            ReceiptQuantity = p.ReceiptQuantity,
                            ProductCode = p.Product.ProductCode,
                            ProductName = p.Product.Description,
                            ProductUnit = p.Product.Unit,
                            ReceiptPackSubjects = p.ReceiptSubjectPartLists.Select(c => new ReceiptPackSubjectDto
                            {
                                ReceiptSubjectId = c.ReceiptSubjectId,
                                ProductId = c.ProductId,
                                PackQuantity = c.PackQuantity,
                                QCReceiptQuantity = c.QCAcceptQuantity,
                                ReceiptQuantity = c.ReceiptQuantity,
                                ProductCode = c.Product.ProductCode,
                                ProductName = c.Product.Description,
                                ProductUnit = c.Product.Unit,
                            }).ToList()
                        }).ToList(),

                    }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<ReportReceiptProductDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<List<ReportReceiptProductDto>>> GetReportReceiptProductByPackIdAsync(AuthenticateDto authenticate, long poId, long packId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<ReportReceiptProductDto>>(null, MessageId.AccessDenied);

                var poModel = await _poRepository.Where(a => !a.IsDeleted && a.POId == poId)
                    .FirstOrDefaultAsync();
                if (poModel == null)
                    return ServiceResultFactory.CreateError<List<ReportReceiptProductDto>>(null, MessageId.EntityDoesNotExist);

                var dbQuery = _receiptRepository
                    .Where(a => a.POId == poId && a.PackId == packId);

                var result = await dbQuery
                    .Select(r => new ReportReceiptProductDto
                    {
                        PackCode = r != null ? r.Pack.PackCode : "",
                        ReceiptId = r.ReceiptId,
                        ReceiptCode = r.ReceiptCode,
                        ReceiptStatus = r.ReceiptStatus,
                        UserAudit = r.AdderUser != null
                         ? new UserAuditLogDto
                         {
                             AdderUserId = r.AdderUserId,
                             AdderUserName = r.AdderUser.FullName,
                             CreateDate = r.CreatedDate.ToUnixTimestamp(),
                             AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall +
                                              r.AdderUser.Image
                         }
                         : null,
                        ReceiptSubjects = r.ReceiptSubjects
                        .Select(p => new ReceiptPackSubjectDto
                        {
                            ReceiptSubjectId = p.ReceiptSubjectId,
                            ProductId = p.ProductId,
                            PackQuantity = p.PackQuantity,
                            QCReceiptQuantity = p.QCAcceptQuantity,
                            ReceiptQuantity = p.ReceiptQuantity,
                            ProductCode = p.Product.ProductCode,
                            ProductName = p.Product.Description,
                            ProductUnit = p.Product.Unit,
                            ReceiptPackSubjects = p.ReceiptSubjectPartLists.Select(c => new ReceiptPackSubjectDto
                            {
                                ReceiptSubjectId = c.ReceiptSubjectId,
                                ProductId = c.ProductId,
                                PackQuantity = c.PackQuantity,
                                QCReceiptQuantity = c.QCAcceptQuantity,
                                ReceiptQuantity = c.ReceiptQuantity,
                                ProductCode = c.Product.ProductCode,
                                ProductName = c.Product.Description,
                                ProductUnit = c.Product.Unit,
                            }).ToList()
                        }).ToList(),

                    }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<ReportReceiptProductDto>>(null, exception);
            }
        }
        //#endregion

        //Warehouse Product Stock change Logs
        // #region WarehouseProductLogs

        //private async Task<bool> AddWarehouseProductStockLogsOnAddReceipt(Receipt receipt, IEnumerable<WarehouseProduct> warehouseProducts)
        //{
        //    var WProductLogs = new List<WarehouseProductStockLogs>();
        //    foreach (var item in receipt.ReceiptSubjects)
        //    {
        //        var wProduct = warehouseProducts.FirstOrDefault(a => a.ProductId == item.ProductId);
        //        WProductLogs.Add(new WarehouseProductStockLogs
        //        {
        //            Input = item.ReceiptQuantity,
        //            Output = 0,
        //            WarehouseStockChangeActionType = WarehouseStockChangeActionType.addReceipt,
        //            DateChange = DateTime.UtcNow,
        //            RealStock = wProduct == null ? item.ReceiptQuantity : wProduct.Inventory,
        //            ProductId = item.ProductId,
        //            ReceiptId = receipt.ReceiptId,
        //        });
        //    }

        //    await _warehouseProductStockLogsRepository.AddRangeAsync(WProductLogs);
        //    return _unitOfWork.SaveChange() > 0 ? true : false;
        //}

        private ServiceResult<bool> UpdatePOSubjectAndWarehouseProductAndReceiptByConditionalQualityControlAsync(Receipt receiptModel,
         IEnumerable<WarehouseProduct> warehouseProducts, List<AddReceiptProductDto> postedModel, IEnumerable<POSubject> poSubjectModels)
        {
            try
            {
                receiptModel.ReceiptStatus = ReceiptStatus.ConditionalAcceptance;

                foreach (var receiptSubject in receiptModel.ReceiptSubjects)
                {
                    var poProduct = poSubjectModels.Where(a => a.ProductId == receiptSubject.ProductId).ToList();
                    var postedProduct = postedModel.FirstOrDefault(a => a.ProductId == receiptSubject.ProductId);

                    if (postedProduct == null || poProduct == null)
                        return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);
                    decimal neededQuantity = 0;
                    decimal shortageQuantity = 0;
                    if (receiptSubject.ReceiptSubjectPartLists == null || !receiptSubject.ReceiptSubjectPartLists.Any())
                    {
                        neededQuantity = receiptSubject.PurchaseRejectRemainedQuantity;
                        shortageQuantity = receiptSubject.ReceiptQuantity - postedProduct.QCReceiptQuantity;
                        if (receiptSubject.ReceiptQuantity < postedProduct.QCReceiptQuantity)
                            return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                        var wProduct = warehouseProducts.FirstOrDefault(a => a.ProductId == receiptSubject.ProductId);
                        if (wProduct == null)
                            return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                        if (receiptSubject.ReceiptQuantity == postedProduct.QCReceiptQuantity)
                        {
                            wProduct.AcceptQuantity += receiptSubject.ReceiptQuantity;
                            receiptSubject.QCAcceptQuantity = receiptSubject.ReceiptQuantity;
                        }
                        else
                        {
                            receiptSubject.QCAcceptQuantity = postedProduct.QCReceiptQuantity;
                            receiptSubject.PurchaseRejectRemainedQuantity = receiptSubject.ReceiptQuantity - postedProduct.QCReceiptQuantity;
                            foreach (var item in poProduct.OrderByDescending(a => a.POSubjectId))
                            {
                                if (shortageQuantity > 0)
                                {
                                    if (item.RemainedQuantity == item.Quantity)
                                        continue;
                                    if (item.RemainedQuantity + shortageQuantity <= item.Quantity)
                                    {
                                        item.RemainedQuantity += shortageQuantity;
                                        item.ReceiptedQuantity -= shortageQuantity;
                                        item.ShortageQuantity += shortageQuantity;
                                        if (item.ReceiptedQuantity < 0)
                                        {
                                            item.ReceiptedQuantity = 0;
                                        }
                                        item.MrpItem.MrpItemStatus = (item.ReceiptedQuantity == 0) ? MrpItemStatus.PO : item.MrpItem.MrpItemStatus;

                                        shortageQuantity = 0;
                                    }

                                    else
                                    {
                                        shortageQuantity -= (item.Quantity - item.RemainedQuantity);
                                        item.ShortageQuantity += (item.Quantity - item.RemainedQuantity);
                                        item.RemainedQuantity = item.Quantity;
                                        item.ReceiptedQuantity = 0;
                                        item.MrpItem.MrpItemStatus = MrpItemStatus.PO;
                                    }
                                }


                            }
                            //foreach (var item in poProduct.OrderByDescending(a => a.POSubjectId))
                            //{
                            //    if (item.RemainedQuantity == item.Quantity)
                            //        continue;
                            //    if (item.RemainedQuantity + neededQuantity <= item.Quantity)
                            //    {
                            //        item.RemainedQuantity += neededQuantity;
                            //        item.ShortageQuantity += neededQuantity;
                            //        neededQuantity = 0;

                            //    }

                            //    else
                            //    {
                            //        neededQuantity -= (item.Quantity - item.RemainedQuantity);
                            //        item.RemainedQuantity = item.Quantity;
                            //        item.ShortageQuantity += item.Quantity;
                            //    }

                            //}


                            wProduct.AcceptQuantity += postedProduct.QCReceiptQuantity;
                        }
                    }
                    //else
                    //{
                    //    if (receiptSubject.ReceiptSubjectPartLists.Count() != postedProduct.Parts.Count())
                    //        return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);
                    //    foreach (var item in receiptSubject.ReceiptSubjectPartLists)
                    //    {
                    //        var postedPart = postedProduct.Parts.FirstOrDefault(a => a.ProductId == item.ProductId);
                    //        var selectedPOSubjectPart = poProduct.POSubjectPartLists.FirstOrDefault(a => a.ProductId == item.ProductId);
                    //        if (postedPart == null || selectedPOSubjectPart == null)
                    //            return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                    //        if (item.ReceiptQuantity < postedPart.QCReceiptQuantity)
                    //            return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                    //        var wProduct = warehouseProducts.FirstOrDefault(a => a.ProductId == item.ProductId);
                    //        if (wProduct == null)
                    //            return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                    //        if (item.ReceiptQuantity == postedPart.QCReceiptQuantity)
                    //        {
                    //            wProduct.AcceptQuantity += item.ReceiptQuantity;
                    //            item.QCAcceptQuantity = item.ReceiptQuantity;
                    //        }
                    //        else
                    //        {
                    //            item.QCAcceptQuantity = postedPart.QCReceiptQuantity;
                    //            item.PurchaseRejectRemainedQuantity = item.ReceiptQuantity - postedPart.QCReceiptQuantity;

                    //            selectedPOSubjectPart.ShortageQuantity += item.PurchaseRejectRemainedQuantity;
                    //            selectedPOSubjectPart.RemainedQuantity += item.PurchaseRejectRemainedQuantity;

                    //            wProduct.AcceptQuantity += postedPart.QCReceiptQuantity;
                    //        }

                    //    }

                    //}
                }
                return ServiceResultFactory.CreateSuccess(true);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        private ServiceResult<bool> UpdateWarehouseProductAndReceiptByRejectQualityControlAsync(Receipt receiptModel,
            IEnumerable<WarehouseProduct> warehouseProducts, IEnumerable<POSubject> poSubjectModels)
        {
            try
            {
                receiptModel.ReceiptStatus = ReceiptStatus.QCRejected;

                foreach (var receiptSubject in receiptModel.ReceiptSubjects)
                {
                    var poProduct = poSubjectModels.Where(a => a.ProductId == receiptSubject.ProductId).ToList();
                    if (poProduct == null)
                        return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);
                    decimal neededQuantity = 0;
                    if (receiptSubject.ReceiptSubjectPartLists == null || !receiptSubject.ReceiptSubjectPartLists.Any())
                    {
                        neededQuantity = receiptSubject.ReceiptQuantity;
                        var wProduct = warehouseProducts.FirstOrDefault(a => a.ProductId == receiptSubject.ProductId);
                        if (wProduct == null || poProduct == null)
                            return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                        receiptSubject.QCAcceptQuantity = 0;
                        receiptSubject.ShortageQuantity = receiptSubject.ReceiptQuantity;
                        receiptSubject.PurchaseRejectRemainedQuantity = receiptSubject.ReceiptQuantity;
                        foreach (var item in poProduct.OrderByDescending(a => a.POSubjectId))
                        {
                            if (item.RemainedQuantity == item.Quantity)
                                continue;
                            if (item.RemainedQuantity + neededQuantity <= item.Quantity)
                            {
                                item.RemainedQuantity += neededQuantity;
                                item.ShortageQuantity += neededQuantity;
                                item.ReceiptedQuantity -= neededQuantity;
                                item.ReceiptedQuantity = (item.ReceiptedQuantity >= 0) ? item.ReceiptedQuantity : 0;
                                neededQuantity = 0;
                                if (item.ReceiptedQuantity == 0)
                                    item.MrpItem.MrpItemStatus = MrpItemStatus.PO;

                            }

                            else
                            {
                                neededQuantity -= (item.Quantity - item.RemainedQuantity);
                                item.RemainedQuantity = item.Quantity;
                                item.ShortageQuantity += item.Quantity;
                                item.ReceiptedQuantity -= (item.Quantity - item.RemainedQuantity);
                                item.ReceiptedQuantity = (item.ReceiptedQuantity >= 0) ? item.ReceiptedQuantity : 0;
                                if (item.ReceiptedQuantity == 0)
                                    item.MrpItem.MrpItemStatus = MrpItemStatus.PO;
                            }

                        }

                    }
                    //else
                    //{
                    //    foreach (var item in receiptSubject.ReceiptSubjectPartLists)
                    //    {
                    //        var wProduct = warehouseProducts.FirstOrDefault(a => a.ProductId == item.ProductId);
                    //        var selectedPOSubjectPart = poProduct.POSubjectPartLists.FirstOrDefault(a => a.ProductId == item.ProductId);
                    //        if (wProduct == null || selectedPOSubjectPart == null)
                    //            return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                    //        item.QCAcceptQuantity = 0;
                    //        item.PurchaseRejectRemainedQuantity = item.ReceiptQuantity;

                    //        selectedPOSubjectPart.ShortageQuantity += receiptSubject.ReceiptQuantity;
                    //        selectedPOSubjectPart.RemainedQuantity += receiptSubject.ReceiptQuantity;
                    //    }
                    //}

                }

                return ServiceResultFactory.CreateSuccess(true);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        private bool UpdateWarhouseProductByAcceptQualityControl(Receipt receipt, IEnumerable<WarehouseProduct> warehouseProduct)
        {
            receipt.ReceiptStatus = ReceiptStatus.QCPassed;
            foreach (var subject in receipt.ReceiptSubjects)
            {
                if (subject.ReceiptSubjectPartLists == null || !subject.ReceiptSubjectPartLists.Any())
                {
                    var wProduct = warehouseProduct.FirstOrDefault(a => a.ProductId == subject.ProductId);
                    if (wProduct == null)
                        return false;
                    wProduct.AcceptQuantity = subject.ReceiptQuantity;
                    subject.QCAcceptQuantity = subject.ReceiptQuantity;
                }
                else
                {
                    foreach (var item in subject.ReceiptSubjectPartLists)
                    {
                        var wProduct = warehouseProduct.FirstOrDefault(a => a.ProductId == item.ProductId);
                        if (wProduct == null)
                            return false;
                        wProduct.AcceptQuantity = item.ReceiptQuantity;
                        item.QCAcceptQuantity = item.ReceiptQuantity;
                    }
                }
            }
            return true;
        }

        private async Task<ServiceResult<ReceiptReject>> AddReceiptRejectAttachmentAsync(ReceiptReject receiptRejectModel, List<AddAttachmentDto> attachment)
        {
            receiptRejectModel.ReceiptRejectAttachments = new List<PAttachment>();

            foreach (var item in attachment)
            {
                var UploadedFile =
                    await _fileHelper.SaveDocumentFromTemp(item.FileSrc, ServiceSetting.UploadFilePath.PO);
                if (UploadedFile == null)
                    return ServiceResultFactory.CreateError<ReceiptReject>(null, MessageId.UploudFailed);

                _fileHelper.DeleteDocumentFromTemp(item.FileSrc);
                receiptRejectModel.ReceiptRejectAttachments.Add(new PAttachment
                {
                    FileType = UploadedFile.FileType,
                    FileSize = UploadedFile.FileSize,
                    FileName = item.FileName,
                    FileSrc = item.FileSrc
                });
            }
            return ServiceResultFactory.CreateSuccess(receiptRejectModel);
        }

        private async Task<ServiceResult<QualityControl>> AddQualityControlAttachmentAsync(QualityControl qualityControl, List<AddAttachmentDto> attachment)
        {
            qualityControl.QCAttachments = new List<PAttachment>();

            foreach (var item in attachment)
            {
                var UploadedFile =
                    await _fileHelper.SaveDocumentFromTemp(item.FileSrc, ServiceSetting.UploadFilePath.PO);
                if (UploadedFile == null)
                    return ServiceResultFactory.CreateError<QualityControl>(null, MessageId.UploudFailed);

                _fileHelper.DeleteDocumentFromTemp(item.FileSrc);
                qualityControl.QCAttachments.Add(new PAttachment
                {
                    FileType = UploadedFile.FileType,
                    FileSize = UploadedFile.FileSize,
                    FileName = item.FileName,
                    FileSrc = item.FileSrc
                });
            }
            return ServiceResultFactory.CreateSuccess(qualityControl);
        }


        private async Task<bool> UpdateWarehouseStockQuantity(Receipt receiptModel, List<AddReceiptProductDto> receiptSubjectModels, IEnumerable<WarehouseProduct> warehouseProducts)
        {
            if (warehouseProducts == null || warehouseProducts.Count() == 0)
                warehouseProducts = new List<WarehouseProduct>();
            // update warhouse stock
            foreach (var subject in receiptSubjectModels)
            {

                if (subject.ReceiptQuantity <= 0)
                    continue;

                var warehouseProductOfThis = warehouseProducts.FirstOrDefault(a => a.ProductId == subject.ProductId);
                if (warehouseProductOfThis == null)
                {
                    await _warehouseProductRepository.AddAsync(new WarehouseProduct
                    {
                        AcceptQuantity = 0,
                        ProductId = subject.ProductId,
                        Inventory = subject.ReceiptQuantity,
                        ReceiptQuantity = subject.ReceiptQuantity,
                    });
                }
                else
                {
                    warehouseProductOfThis.ReceiptQuantity += subject.ReceiptQuantity;
                    warehouseProductOfThis.Inventory += subject.ReceiptQuantity;

                }
                _warehouseProductStockLogsRepository.Add(new WarehouseProductStockLogs
                {
                    Input = subject.ReceiptQuantity,
                    Output = 0,
                    WarehouseStockChangeActionType = WarehouseStockChangeActionType.addReceipt,
                    DateChange = DateTime.UtcNow,
                    RealStock = warehouseProductOfThis == null ? subject.ReceiptQuantity : warehouseProductOfThis.Inventory,
                    ProductId = subject.ProductId,
                    Receipt = receiptModel,
                });

            }
            return true;
        }

        private ServiceResult<bool> AddReceiptSubject(List<AddReceiptProductDto> receiptSubjectModels, IEnumerable<POSubject> poSubjects, List<MrpItem> mrpItems, Receipt receiptModel, IEnumerable<PackSubject> packSubjects)
        {
            foreach (var addSubject in receiptSubjectModels)
            {
                var selectedPackSubject = packSubjects
                     .Where(a => a.ProductId == addSubject.ProductId)
                     .FirstOrDefault();

                var selectedPOSubject = poSubjects.Where(a => a.ProductId == addSubject.ProductId).ToList();

                //var selectedMrp = mrpItems.FirstOrDefault(a => a.Id == selectedPOSubject.MrpItemId);

                if (selectedPackSubject == null || selectedPOSubject == null)
                    return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);






                if (addSubject.ReceiptQuantity < 0)
                    return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                if (addSubject.ReceiptQuantity > selectedPackSubject.Quantity)
                    return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                receiptModel.ReceiptSubjects.Add(new ReceiptSubject
                {
                    ProductId = addSubject.ProductId,
                    ReceiptQuantity = addSubject.ReceiptQuantity,
                    PackQuantity = addSubject.PackQuantity,
                });
                decimal neededQuantity = 0;
                neededQuantity = addSubject.ReceiptQuantity;
                decimal shortageQuantity = 0;
                if (addSubject.ReceiptQuantity < selectedPackSubject.Quantity)
                {
                    selectedPackSubject.ShortageQuantity = selectedPackSubject.Quantity - addSubject.ReceiptQuantity;
                    shortageQuantity = selectedPackSubject.ShortageQuantity;
                }
                foreach (var item in selectedPOSubject)
                {
                    if (neededQuantity > 0)
                    {
                        if (item.MrpItem.MrpItemStatus < MrpItemStatus.Receipt)
                            item.MrpItem.MrpItemStatus = MrpItemStatus.Receipt;

                        if ((item.Quantity - item.ReceiptedQuantity) >= neededQuantity)
                        {
                            item.ReceiptedQuantity += neededQuantity;
                            neededQuantity = 0;
                        }
                        else if ((item.Quantity - item.ReceiptedQuantity) > 0 && (item.Quantity - item.ReceiptedQuantity) < neededQuantity)
                        {
                            neededQuantity -= item.Quantity - item.ReceiptedQuantity;
                            item.ReceiptedQuantity += item.Quantity - item.ReceiptedQuantity;
                        }

                        // age kambod dasht
                        if (addSubject.ReceiptQuantity < selectedPackSubject.Quantity && shortageQuantity > 0)
                        {

                            item.ShortageQuantity += (item.Quantity - item.RemainedQuantity - item.ReceiptedQuantity >= shortageQuantity) ? shortageQuantity : item.Quantity - item.RemainedQuantity - item.ReceiptedQuantity;
                            shortageQuantity -= (item.Quantity - item.RemainedQuantity - item.ReceiptedQuantity >= shortageQuantity) ? shortageQuantity : item.Quantity - item.RemainedQuantity - item.ReceiptedQuantity;
                            item.RemainedQuantity += (item.Quantity - item.RemainedQuantity) - item.ReceiptedQuantity;


                        }
                    }
                    else
                    {
                        if (addSubject.ReceiptQuantity < selectedPackSubject.Quantity && shortageQuantity > 0)
                        {

                            item.ShortageQuantity += (item.Quantity - item.RemainedQuantity - item.ReceiptedQuantity >= shortageQuantity) ? shortageQuantity : item.Quantity - item.RemainedQuantity - item.ReceiptedQuantity;
                            item.RemainedQuantity += (item.Quantity - item.RemainedQuantity) - item.ReceiptedQuantity;
                            shortageQuantity -= (item.Quantity - item.RemainedQuantity - item.ReceiptedQuantity >= shortageQuantity) ? shortageQuantity : item.Quantity - item.RemainedQuantity - item.ReceiptedQuantity;
                            if (shortageQuantity <= 0)
                                break;
                        }
                    }


                }


            }

            return ServiceResultFactory.CreateSuccess(true);
        }

        private async Task<bool> UpdatePoStatus(int userId, PO poModel, long logisticId)
        {
            var poStatusLogs = await _poStatusLogRepository.FirstOrDefaultAsync(a => a.POId == poModel.POId && a.Status == POStatus.Delivered);
            if (poStatusLogs == null)
            {
                var newStatusLogs = new POStatusLog
                {
                    IsDone = false,
                    POId = poModel.POId,
                    BeforeStatus = poModel.POStatus,
                    Status = POStatus.Delivered,
                };

                if (poModel.POStatus >= POStatus.packing && !await _packRepository.AnyAsync(a => !a.IsDeleted && a.POId == poModel.POId && a.Logistics.Any(a => a.LogisticId != logisticId && a.LogisticStatus != LogisticStatus.Compeleted)))
                {
                    poModel.POStatus = POStatus.Delivered;
                    newStatusLogs.IsDone = true;
                    //await _scmLogAndNotificationService.SetDonedNotificationAsync(userId, poModel.BaseContractCode, poModel.POId.ToString(), NotifEvent.AddPO);
                }

                await _poStatusLogRepository.AddAsync(newStatusLogs);
            }
            else
            {
                if (poModel.POStatus >= POStatus.packing && !await _packRepository.AnyAsync(a => !a.IsDeleted && a.POId == poModel.POId && a.Logistics.Any(a => a.LogisticId != logisticId && a.LogisticStatus != LogisticStatus.Compeleted)))
                {
                    poStatusLogs.IsDone = true;
                    poModel.POStatus = POStatus.Delivered;
                    //await _scmLogAndNotificationService.SetDonedNotificationAsync(userId, poModel.BaseContractCode, poModel.POId.ToString(), NotifEvent.AddPO);
                }
            }

            return true;
        }

        private async Task<ServiceResult<WarehouseOutputRequest>> AddWarehouseOutputRequest(AuthenticateDto authenticate, AddWarehouseOutputRequestDto model)
        {
            try
            {
                
                if (model.Subjects == null || !model.Subjects.Any())
                    return ServiceResultFactory.CreateError<WarehouseOutputRequest>(null, MessageId.EntityDoesNotExist);
                var subjectIds = model.Subjects.Select(a => a.ProductId).Distinct().ToList();
                var dbQuery = _warehouseProductRepository.Where(a => subjectIds.Contains(a.ProductId) && a.Product.ContractCode == authenticate.ContractCode);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<WarehouseOutputRequest>(null, MessageId.EntityDoesNotExist);

                if (subjectIds.Count != dbQuery.Count())
                    return ServiceResultFactory.CreateError<WarehouseOutputRequest>(null, MessageId.InputDataValidationError);

                var warehouseProducts = await dbQuery.ToListAsync();

                if (warehouseProducts == null)
                    return ServiceResultFactory.CreateError<WarehouseOutputRequest>(null, MessageId.EntityDoesNotExist);

                foreach (var item in warehouseProducts)
                {
                    if (item.Inventory < model.Subjects.First(a => a.ProductId == item.ProductId).Quantity)
                        return ServiceResultFactory.CreateError<WarehouseOutputRequest>(null, MessageId.QuantityCantBeGreaterThenInventory);
                }


                WarehouseOutputRequest request = new WarehouseOutputRequest
                {
                    ReceiptId = model.RecepitId,
                    RecepitCode = model.RecepitCode,
                    Status = WarehouseOutputStatus.Confirmed,
                    ContractCode = authenticate.ContractCode,
                    Subjects = new List<WarehouseOutputRequestSubject>(),
                    WarehouseOutputRequestWorkFlow = new List<WarehouseOutputRequestWorkFlow>()
                };
                request.WarehouseOutputRequestWorkFlow.Add(new WarehouseOutputRequestWorkFlow
                {
                    ConfirmNote = "",
                    Status = ConfirmationWorkFlowStatus.Confirm,
                    WarehouseOutputRequestWorkFlowUsers = new List<WarehouseOutputRequestWorkFlowUser>(),
                });
                var insertSubjectResult = AddWharehouseOutputRequestSubject(model, request);
                if (!insertSubjectResult.Succeeded)
                    return ServiceResultFactory.CreateError<WarehouseOutputRequest>(null, MessageId.EntityDoesNotExist);
                request = insertSubjectResult.Result;

                var count = await _warehouseOutputRequestRepository.CountAsync(a => a.ContractCode == authenticate.ContractCode);
                var codeRes = await _formConfigService.GenerateFormCodeAsync(authenticate.ContractCode, FormName.WarehouseOutput, count);
                if (!codeRes.Succeeded)
                    return ServiceResultFactory.CreateError<WarehouseOutputRequest>(null, codeRes.Messages.First().Message);
                request.RequestCode = codeRes.Result;
                return ServiceResultFactory.CreateSuccess(request);

            }
            catch (Exception ex)
            {
                return ServiceResultFactory.CreateException<WarehouseOutputRequest>(null, ex);
            }

        }

        private ServiceResult<WarehouseOutputRequest> AddWharehouseOutputRequestSubject(AddWarehouseOutputRequestDto model, WarehouseOutputRequest requstModel)
        {
            foreach (var item in model.Subjects)
            {
                if (item.Quantity > 0)
                {
                    requstModel.Subjects.Add(new WarehouseOutputRequestSubject
                    {
                        Quantity = item.Quantity,
                        ProductId = item.ProductId
                    });
                }
                
            }
            return ServiceResultFactory.CreateSuccess(requstModel);
        }
        private List<NotifToDto> ConfirmSendNotification()
        {
            return new List<NotifToDto>{ new NotifToDto
            {
                        NotifEvent = NotifEvent.AddWarehouseDispatch,
                        Roles = new List<string>
                        {
                                  SCMRole.WarehouseDispatchMng,
                        }
            }};
        }
    }
}
