using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Raybod.SCM.DataAccess.Core;
using Raybod.SCM.DataAccess.Extention;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Warehouse;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Model;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Utility.Extention;
using Raybod.SCM.Services.Utilitys.exportToExcel;
using Raybod.SCM.DataTransferObject.Receipt;
using Raybod.SCM.DataTransferObject.Product;
using Raybod.SCM.DataTransferObject.User;
using Raybod.SCM.Domain.Struct;
using Raybod.SCM.DataTransferObject.Audit;
using Raybod.SCM.DataTransferObject.Notification;

namespace Raybod.SCM.Services.Implementation
{
    public class WarehouseService : IWarehouseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITeamWorkAuthenticationService _authenticationServices;
        private readonly IContractFormConfigService _formConfigService;
        private readonly ISCMLogAndNotificationService _scmLogAndNotificationService;
        private readonly DbSet<Warehouse> _WarehouseRepository;
        private readonly DbSet<WarehouseProduct> _warehouseProductRepository;
        private readonly DbSet<WarehouseProductStockLogs> _warehouseProductStockLogsRepository;
        private readonly DbSet<WarehouseOutputRequest> _warehouseOutputRequestRepository;
        private readonly DbSet<ProductGroup> _productGroupRepository;

        private readonly DbSet<WarehouseDespatch> _warehouseDespatchRepository;
        private readonly DbSet<WarehouseOutputRequestWorkFlow> _warehouseOutputRequestWorkFlowRepository;
        private readonly DbSet<User> _userRepository;
        private readonly CompanyAppSettingsDto _appSettings;

        public WarehouseService(
            IUnitOfWork unitOfWork,
            IWebHostEnvironment hostingEnvironmentRoot,
            ITeamWorkAuthenticationService authenticationService,
            IOptions<CompanyAppSettingsDto> appSettings, IContractFormConfigService formConfigService, ISCMLogAndNotificationService scmLogAndNotificationService)
        {
            _unitOfWork = unitOfWork;
            _authenticationServices = authenticationService;
            _WarehouseRepository = _unitOfWork.Set<Warehouse>();
            _warehouseProductRepository = _unitOfWork.Set<WarehouseProduct>();
            _warehouseProductStockLogsRepository = _unitOfWork.Set<WarehouseProductStockLogs>();
            _warehouseOutputRequestRepository = _unitOfWork.Set<WarehouseOutputRequest>();
            _userRepository = _unitOfWork.Set<User>();
            _warehouseOutputRequestWorkFlowRepository = _unitOfWork.Set<WarehouseOutputRequestWorkFlow>();
            _warehouseDespatchRepository = _unitOfWork.Set<WarehouseDespatch>();
            _productGroupRepository = _unitOfWork.Set<ProductGroup>();
            _appSettings = appSettings.Value;
            _formConfigService = formConfigService;
            _scmLogAndNotificationService = scmLogAndNotificationService;
        }

        public async Task<ServiceResult<BaseWarehouseDto>> AddWarehouseAsync(AuthenticateDto authenticate, AddWarehouseDto model)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<BaseWarehouseDto>(null, MessageId.AccessDenied);

                if (await _WarehouseRepository.AnyAsync(x => !x.IsDeleted && x.WarehouseCode == model.WarehouseCode))
                    return ServiceResultFactory.CreateError(new BaseWarehouseDto(), MessageId.CodeExist);

                var warehouseModel = new Warehouse
                {

                    IsDeleted = false,
                    Name = model.Name,
                    WarehouseCode = model.WarehouseCode,
                    Address = model.Address,
                    Phone = model.Phone
                };

                _WarehouseRepository.Add(warehouseModel);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var result = new BaseWarehouseDto
                    {
                        WarehouseId = warehouseModel.Id,
                        Name = model.Name,
                        WarehouseCode = model.WarehouseCode,
                        Address = model.Address,
                        Phone = model.Phone
                    };

                    return ServiceResultFactory.CreateSuccess(result);
                }

                return ServiceResultFactory.CreateError(new BaseWarehouseDto(), MessageId.InternalError);
            }
            catch (System.Exception exception)
            {
                return ServiceResultFactory.CreateException(new BaseWarehouseDto(), exception);
            }
        }

        public async Task<ServiceResult<BaseWarehouseDto>> EditWarehouseAsync(AuthenticateDto authenticate, int warehouseId, AddWarehouseDto model)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<BaseWarehouseDto>(null, MessageId.AccessDenied);

                var selectedWarehouse = await _WarehouseRepository
                    .Where(a => a.Id == warehouseId)
                    .FirstOrDefaultAsync();
                if (selectedWarehouse == null)
                    return ServiceResultFactory.CreateError(new BaseWarehouseDto(), MessageId.EntityDoesNotExist);

                if (selectedWarehouse.WarehouseCode != model.WarehouseCode && await _WarehouseRepository.AnyAsync(x => !x.IsDeleted && x.Id != warehouseId && x.WarehouseCode == model.WarehouseCode))
                    return ServiceResultFactory.CreateError(new BaseWarehouseDto(), MessageId.CodeExist);


                selectedWarehouse.WarehouseCode = model.WarehouseCode;
                selectedWarehouse.Name = model.Name;
                selectedWarehouse.Address = model.Address;
                selectedWarehouse.Phone = model.Phone;

                await _unitOfWork.SaveChangesAsync();

                var result = new BaseWarehouseDto
                {
                    WarehouseId = selectedWarehouse.Id,
                    Name = selectedWarehouse.Name,
                    WarehouseCode = selectedWarehouse.WarehouseCode,
                    Address = selectedWarehouse.Address,
                    Phone = selectedWarehouse.Phone
                };
                return ServiceResultFactory.CreateSuccess(result);

            }
            catch (System.Exception exception)
            {

                return ServiceResultFactory.CreateException(new BaseWarehouseDto(), exception);
            }
        }

        public async Task<ServiceResult<List<BaseWarehouseDto>>> GetWarehouseListAsync(AuthenticateDto authenticate, WarehouseQueryDto query)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<BaseWarehouseDto>>(null, MessageId.AccessDenied);

                var dbQuery = _WarehouseRepository.Where(x => !x.IsDeleted)
                    .OrderByDescending(x => x.Id)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(x =>
                    x.Name.Contains(query.SearchText) ||
                    x.Phone.Contains(query.SearchText) ||
                    x.WarehouseCode.Contains(query.SearchText));

                dbQuery = dbQuery.ApplayPageing(query);
                var list = await dbQuery.Select(c => new BaseWarehouseDto
                {
                    WarehouseId = c.Id,
                    Name = c.Name,
                    WarehouseCode = c.WarehouseCode,
                    Address = c.Address,
                    Phone = c.Phone
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(list);

            }
            catch (System.Exception exception)
            {
                return ServiceResultFactory.CreateException(new List<BaseWarehouseDto>(), exception);
            }
        }

        public async Task<ServiceResult<BaseWarehouseDto>> GetWarehouseByIdAsync(AuthenticateDto authenticate, int warehouseId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<BaseWarehouseDto>(null, MessageId.AccessDenied);

                var result = await _WarehouseRepository
                    .Where(a => a.Id == warehouseId)
                    .Select(c => new BaseWarehouseDto
                    {
                        WarehouseId = c.Id,
                        Name = c.Name,
                        WarehouseCode = c.WarehouseCode,
                        Address = c.Address,
                        Phone = c.Phone
                    }).FirstOrDefaultAsync();

                if (result == null)
                    return ServiceResultFactory.CreateError(new BaseWarehouseDto(), MessageId.EntityDoesNotExist);

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (System.Exception exception)
            {
                return ServiceResultFactory.CreateException(new BaseWarehouseDto(), exception);
            }
        }

        public async Task<ServiceResult<bool>> RemoveWarehouseAsync(AuthenticateDto authenticate, int warehouseId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var WarehouseModel = await _WarehouseRepository.FindAsync(warehouseId);
                if (WarehouseModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                WarehouseModel.IsDeleted = true;
                return await _unitOfWork.SaveChangesAsync() > 0
                    ? ServiceResultFactory.CreateSuccess(true)
                    : ServiceResultFactory.CreateError(false, MessageId.InternalError);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<List<WarehouseProductDto>>> GetWarehouseProductAsync(AuthenticateDto authenticate, WarehouseProductQueryDto query)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(new List<WarehouseProductDto>(), MessageId.AccessDenied);

                var dbQuery = _warehouseProductRepository.Where(a => a.Product.ContractCode == authenticate.ContractCode).AsQueryable();

                if (query.ProductGroupId > 0)
                    dbQuery = dbQuery.Where(a => a.Product.ProductGroupId == query.ProductGroupId);

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(x =>
                       x.Product.ProductCode.Contains(query.SearchText) || x.Product.TechnicalNumber.Contains(query.SearchText) ||
                       (x.Product.Description != null && x.Product.Description.ToLower().Contains(query.SearchText)));

                if (query.ProductGroupIds != null && query.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(a => query.ProductGroupIds.Contains(a.Product.ProductGroupId));

                if (query.ProductIds != null && query.ProductIds.Any())
                    dbQuery = dbQuery.Where(a => query.ProductIds.Contains(a.ProductId));


                var totalCount = dbQuery.Count();
                var columnsMap = new Dictionary<string, Expression<Func<WarehouseProduct, object>>>
                {
                    ["Id"] = v => v.WarehouseProductId,
                    ["ProductId"] = v => v.ProductId,
                    ["ProductCode"] = v => v.Product.ProductCode,
                    ["Description"] = v => v.Product.Description,
                };

                dbQuery = dbQuery.ApplayOrdering(query, columnsMap).ApplayPageing(query);
                var result = await dbQuery.Select(p => new WarehouseProductDto
                {
                    Id = p.WarehouseProductId,
                    ProductId = p.ProductId,
                    TemporaryStock = p.ReceiptQuantity,
                    InitialStock = p.AcceptQuantity,
                    RealStock = p.Inventory,
                    //WarehouseId = p.WarehouseId,
                    ProductGroupId=p.Product.ProductGroupId,
                    ProductCode = p.Product.ProductCode,
                    ProductName = p.Product.Description,
                    ProductUnit = p.Product.Unit,
                    ProductGroupName = p.Product.ProductGroup.Title,
                    TechnicalNumber = p.Product.TechnicalNumber,
                    LastUpdateDate = p.UpdateDate.ToUnixTimestamp()
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<WarehouseProductDto>>(null, exception);
            }
        }

        public async Task<DownloadFileDto> ExportExcelWarehouseProductAsync(AuthenticateDto authenticate, WarehouseProductQueryDto query)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return null;

                var dbQuery = _warehouseProductRepository.AsQueryable();

                if (query.ProductGroupId > 0)
                    dbQuery = dbQuery.Where(a => a.Product.ProductGroupId == query.ProductGroupId);

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(x =>
                       x.Product.ProductCode.Contains(query.SearchText) ||
                       (x.Product.Description != null && x.Product.Description.ToLower().Contains(query.SearchText)));

                var totalCount = dbQuery.Count();
                var columnsMap = new Dictionary<string, Expression<Func<WarehouseProduct, object>>>
                {
                    ["Id"] = v => v.WarehouseProductId,
                    ["ProductId"] = v => v.ProductId,
                    ["ProductCode"] = v => v.Product.ProductCode,
                    ["Description"] = v => v.Product.Description,
                };

                dbQuery = dbQuery.ApplayOrdering(query, columnsMap).ApplayPageing(query);

                var result = await dbQuery.Select(p => new ExportExcelWarehouseProductDto
                {
                    Inventory = p.Inventory,
                    EquipmentCode = p.Product.ProductCode,
                    EquipmentName = p.Product.Description,
                    Unit = p.Product.Unit,
                    Group = p.Product.ProductGroup.Title,
                    TechnicalNumber = p.Product.TechnicalNumber,
                    LastUpdated = p.UpdateDate.ToPersianDateString() != "" ? p.UpdateDate.ToPersianDateString() : "",
                }).ToListAsync();


                return ExcelHelper.InventoryExportToExcelWithStyle(result, "Inventrory", authenticate.ContractCode);
            }
            catch (Exception exception)
            {
                return null;
            }
        }

        public async Task<DownloadFileDto> GetWarehouseProductLogsAsync(AuthenticateDto authenticate, int productId, WarehouseProductLogQueryDto query)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return null;

                var dbQuery = _warehouseProductStockLogsRepository
                .Where(a => a.ProductId == productId)
                .OrderBy(a => a.Id)
                .AsQueryable();

                if (dbQuery.Count() == 0)
                    return null;


                //dbQuery = dbQuery.ApplayPageing(query);

                var LogList = await dbQuery.Select(a => new WarehouseProductLogExcelDto
                {
                    Date = a.DateChange.ToPersianDate(),
                    Operation = (a.WarehouseStockChangeActionType == WarehouseStockChangeActionType.addReceipt) ? "Receipt" : (!String.IsNullOrEmpty(a.WarehouseDespatch.WarehouseOutputRequest.RecepitCode)) ? "Warehouse Dispatch(Rejection)" : "Warehouse Dispatch",
                    Reference = (a.WarehouseStockChangeActionType == WarehouseStockChangeActionType.addReceipt) ? a.Receipt.ReceiptCode : a.WarehouseDespatch.DespatchCode,
                    QuantityIn = a.Input,
                    QuantityOut = a.Output,
                    RemaindQuantity = a.RealStock
                }).ToListAsync();
                var product = await dbQuery.Include(a => a.Product).FirstOrDefaultAsync();

                var result = ExcelHelper.KardexExportToExcelWithStyle(LogList, product.Product.Description + "-Kardex", product.Product.ProductCode);

                return result;

            }
            catch (Exception exception)
            {
                return null;
            }
        }
        public async Task<ServiceResult<List<GetProductListInfoDto>>> GetWarehouseProductListInfoAsync(AuthenticateDto authenticate, int productGroupId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<GetProductListInfoDto>>(null, MessageId.AccessDenied);

                var dbQuery = _warehouseProductRepository
                    .Include(a => a.Product)
                    .ThenInclude(a => a.ProductGroup)
                    .Include(a => a.Product)
                    .ThenInclude(a => a.BomProducts)
                    .ThenInclude(a => a.Area)
                    .Where(a => !a.Product.IsDeleted && a.Product.ProductGroupId == productGroupId && a.Product.ContractCode == authenticate.ContractCode);
                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.Product.ProductGroupId)))
                    return ServiceResultFactory.CreateError<List<GetProductListInfoDto>>(null, MessageId.AccessDenied);

                var result = await dbQuery.Select(a => new GetProductListInfoDto
                {
                    Description = a.Product.Description,
                    IsRegisterd = true,
                    IsSet = a.Product.BomProducts.Any(b => !b.IsDeleted && b.MaterialType == MaterialType.Component),
                    ProductCode = a.Product.ProductCode,
                    ProductGroupTitle = a.Product.ProductGroup.Title,
                    ProductId = a.Product.Id,
                    TechnicalNumber = a.Product.TechnicalNumber,
                    Unit = a.Product.Unit,
                    Inventory = a.Inventory,
                    IsEquipment = a.Product.BomProducts.Any(b => !b.IsDeleted && b.MaterialType == MaterialType.Part)
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<GetProductListInfoDto>>(null, exception);
            }
        }
        public async Task<ServiceResult<List<UserMentionDto>>> GetConfirmationUserListAsync(AuthenticateDto authenticate, int productGroupId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<UserMentionDto>>(null, MessageId.AccessDenied);



                var roles = new List<string> { SCMRole.WareHouseOutputRequestConfirm };
                var list = await _authenticationServices.GetAllUserHasAccessPurchaseAsync(authenticate.ContractCode, roles, productGroupId);

                foreach (var item in list)
                {
                    item.Image = item.Image != null ?
                        _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + item.Image : "";
                }

                return ServiceResultFactory.CreateSuccess(list);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<UserMentionDto>>(null, exception);
            }
        }
        public async Task<ServiceResult<PendingForConfirmWarehouseOutputRequestDto>> AddWarehouseOutputRequest(AuthenticateDto authenticate, AddWarehouseOutputRequestDto model)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<PendingForConfirmWarehouseOutputRequestDto>(null, MessageId.AccessDenied);
                if (model.Subjects == null || !model.Subjects.Any())
                    return ServiceResultFactory.CreateError<PendingForConfirmWarehouseOutputRequestDto>(null, MessageId.EntityDoesNotExist);
                var subjectIds = model.Subjects.Select(a => a.ProductId).Distinct().ToList();
                var dbQuery = _warehouseProductRepository.Where(a => subjectIds.Contains(a.ProductId) && a.Product.ContractCode == authenticate.ContractCode);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<PendingForConfirmWarehouseOutputRequestDto>(null, MessageId.EntityDoesNotExist);

                var productGroupTitle=await dbQuery.Select(a => a.Product.ProductGroup.Title).FirstOrDefaultAsync();
                var productGroupId=await dbQuery.Select(a => a.Product.ProductGroupId).FirstOrDefaultAsync();

                if (subjectIds.Count != dbQuery.Count())
                    return ServiceResultFactory.CreateError<PendingForConfirmWarehouseOutputRequestDto>(null, MessageId.InputDataValidationError);

                var warehouseProducts = await dbQuery.ToListAsync();

                if (warehouseProducts == null)
                    return ServiceResultFactory.CreateError<PendingForConfirmWarehouseOutputRequestDto>(null, MessageId.EntityDoesNotExist);

                //foreach (var item in warehouseProducts)
                //{
                //    if (item.Inventory < model.Subjects.First(a => a.ProductId == item.ProductId).Quantity)
                //        return ServiceResultFactory.CreateError<PendingForConfirmWarehouseOutputRequestDto>(null, MessageId.QuantityCantBeGreaterThenInventory);
                //}


                WarehouseOutputRequest request = new WarehouseOutputRequest
                {
                    ReceiptId = model.RecepitId,
                    RecepitCode = model.RecepitCode,
                    Status = WarehouseOutputStatus.PendingForConfirm,
                    ContractCode = authenticate.ContractCode,
                    Subjects = new List<WarehouseOutputRequestSubject>(),
                    WarehouseOutputRequestWorkFlow = new List<WarehouseOutputRequestWorkFlow>()
                };

                var confirmWorkFlow = await AddWarehouseOutputRequestConfirmationAsync(model.WorkFlow);
                if (!confirmWorkFlow.Succeeded)
                    return ServiceResultFactory.CreateError<PendingForConfirmWarehouseOutputRequestDto>(null, MessageId.OperationFailed);
                request.WarehouseOutputRequestWorkFlow.Add(confirmWorkFlow.Result);
                if (confirmWorkFlow.Result.Status == ConfirmationWorkFlowStatus.Confirm)
                {
                    request.Status = WarehouseOutputStatus.Confirmed;
                }
                var insertSubjectResult = AddWharehouseOutputRequestSubject(model, request);
                if (!insertSubjectResult.Succeeded)
                    return ServiceResultFactory.CreateError<PendingForConfirmWarehouseOutputRequestDto>(null, MessageId.EntityDoesNotExist);
                request = insertSubjectResult.Result;

                var count = await _warehouseOutputRequestRepository.CountAsync(a => a.ContractCode == authenticate.ContractCode);
                var codeRes = await _formConfigService.GenerateFormCodeAsync(authenticate.ContractCode, FormName.WarehouseOutput, count);
                if (!codeRes.Succeeded)
                    return ServiceResultFactory.CreateError<PendingForConfirmWarehouseOutputRequestDto>(null, codeRes.Messages.First().Message);
                request.RequestCode = codeRes.Result;
                await _warehouseOutputRequestRepository.AddAsync(request);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    int? userId = null;
                    if (request.Status != WarehouseOutputStatus.Confirmed)
                    {
                        userId = request.WarehouseOutputRequestWorkFlow.First().WarehouseOutputRequestWorkFlowUsers.First(a => a.IsBallInCourt).UserId;
                    }
                    if (request.Status != WarehouseOutputStatus.Confirmed)
                    {
                        await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                        {
                            ContractCode = request.ContractCode,
                            FormCode = request.RequestCode,
                            KeyValue = request.RequestId.ToString(),
                            NotifEvent = NotifEvent.AddWarehouseOutputRequest,
                            ProductGroupId =productGroupId,
                            RootKeyValue = request.RequestId.ToString(),
                            PerformerUserId = authenticate.UserId,
                            Message = productGroupTitle,
                            PerformerUserFullName = authenticate.UserFullName,
                        },
                    productGroupId,
                    NotifEvent.ConfirmWarehouseOutputRequest, userId);
                    }
                    if (request.Status == WarehouseOutputStatus.Confirmed)
                    {
                        await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                        {
                            ContractCode = request.ContractCode,
                            FormCode = request.RequestCode,
                            KeyValue = request.RequestId.ToString(),
                            NotifEvent = NotifEvent.ConfirmWarehouseOutputRequest,
                            Description="2",
                            ProductGroupId = productGroupId,
                            RootKeyValue = request.RequestId.ToString(),
                            Message = productGroupTitle,
                            PerformerUserId = authenticate.UserId,
                            PerformerUserFullName = authenticate.UserFullName,

                        },
                    productGroupId
                    , ConfirmSendNotification());
                    }

                    var result = await GetPendingwarehouseRequisitionByRequestIdAsync(authenticate, request.RequestId);
                    if (result.Succeeded)
                        return ServiceResultFactory.CreateSuccess(result.Result);
                    else
                        return ServiceResultFactory.CreateSuccess(new PendingForConfirmWarehouseOutputRequestDto());

                }
                return ServiceResultFactory.CreateError<PendingForConfirmWarehouseOutputRequestDto>(null, MessageId.SaveFailed);

            }
            catch (Exception ex)
            {
                return ServiceResultFactory.CreateException<PendingForConfirmWarehouseOutputRequestDto>(null, ex);
            }

        }


        public async Task<ServiceResult<WarehouseRequestConfirmationWorkflowDto>> GetPendingConfirmWarehouseOutputRequestByPurchaseRequestIdAsync(AuthenticateDto authenticate, long warehouseOutputRequestId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<WarehouseRequestConfirmationWorkflowDto>(null, MessageId.AccessDenied);

                var dbQuery = _warehouseOutputRequestWorkFlowRepository
                     .AsNoTracking()
                     .Where(a => !a.IsDeleted &&
                   a.RequestId == warehouseOutputRequestId &&
                    a.Status == ConfirmationWorkFlowStatus.Pending &&
                     a.WarehouseOutputRequest.Status == WarehouseOutputStatus.PendingForConfirm);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<WarehouseRequestConfirmationWorkflowDto>(null, MessageId.EntityDoesNotExist);

                //if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.WarehouseOutputRequest.rece.ProductGroupId)))
                //    return ServiceResultFactory.CreateError<WarehouseRequestConfirmationWorkflowDto>(null, MessageId.AccessDenied);

                var result = await dbQuery
                        .Select(x => new WarehouseRequestConfirmationWorkflowDto
                        {
                            ConfirmNote = x.ConfirmNote,
                            RequestCode = x.WarehouseOutputRequest.RequestCode,
                            WarehouseOutputRequestConfirmUserAudit = x.AdderUserId != null ? new UserAuditLogDto
                            {
                                CreateDate = x.CreatedDate.ToUnixTimestamp(),
                                AdderUserName = x.AdderUser.FullName,
                                AdderUserImage = x.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + x.AdderUser.Image : ""
                            } : null,

                            WarehouseOutputRequestSubjects = x.WarehouseOutputRequest.Subjects.Where(a => !a.IsDeleted).Select(b => new WarehouseOutputRequestSubjectListDto
                            {
                                ProductCode = b.Product.ProductCode,
                                Inventory = b.Product.WarehouseProducts.Sum(a => a.Inventory),
                                RequestSubjectId = b.RequestSubjectId,
                                ProductDescription = b.Product.Description,
                                ProductGroupName = b.Product.ProductGroup.Title,
                                ProductTechnicalNumber = b.Product.TechnicalNumber,
                                ProductId = b.ProductId,
                                ProductUnit = b.Product.Unit,
                                Quntity = b.Quantity,
                                RequestId = b.RequestId,

                            }).ToList(),
                            WarehouseOutputRequestConfirmationUserWorkFlows = x.WarehouseOutputRequestWorkFlowUsers.Where(a => a.UserId > 0)
                            .Select(e => new WarehouseRequestConfirmationUserWorkFlowDto
                            {
                                WarehouseOutputRequestWorkFlowUserId = e.WarehouseOutputRequestWorkFlowUserId,
                                IsAccept = e.IsAccept,
                                IsBallInCourt = e.IsBallInCourt,
                                Note = e.Note,
                                OrderNumber = e.OrderNumber,
                                UserId = e.UserId,
                                UserFullName = e.User != null ? e.User.FullName : "",
                                UserImage = e.User != null && e.User.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + e.User.Image : "",
                                DateEnd = e.DateEnd.ToUnixTimestamp(),
                                DateStart = e.DateStart.ToUnixTimestamp(),
                            }).ToList()
                        }).FirstOrDefaultAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<WarehouseRequestConfirmationWorkflowDto>(null, exception);
            }
        }
        public async Task<ServiceResult<List<PendingForConfirmWarehouseOutputRequestDto>>> SetUserConfirmOwnWarehouseOutputRequestTaskAsync(AuthenticateDto authenticate, long warehouseRequestId, AddWarehouseRequestConfirmationAnswerDto model)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<PendingForConfirmWarehouseOutputRequestDto>>(null, MessageId.AccessDenied);

                var dbQuery = _warehouseOutputRequestWorkFlowRepository
                     .Where(a => !a.IsDeleted &&
                    a.RequestId == warehouseRequestId &&
                    a.Status == ConfirmationWorkFlowStatus.Pending &&
                    a.WarehouseOutputRequest.Status == WarehouseOutputStatus.PendingForConfirm)
                     .Include(a => a.WarehouseOutputRequestWorkFlowUsers)
                     .ThenInclude(c => c.User)
                     .Include(a => a.WarehouseOutputRequest)
                     .ThenInclude(a => a.Subjects)
                     .AsQueryable();


                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<List<PendingForConfirmWarehouseOutputRequestDto>>(null, MessageId.EntityDoesNotExist);


                var productGroup = await dbQuery.Select(a => a.WarehouseOutputRequest.Subjects.Select(a => a.Product.ProductGroup).FirstOrDefault()).FirstOrDefaultAsync();

                var confirmationModel = await dbQuery.FirstOrDefaultAsync();

                if (confirmationModel == null)
                    return ServiceResultFactory.CreateError<List<PendingForConfirmWarehouseOutputRequestDto>>(null, MessageId.EntityDoesNotExist);

                if (confirmationModel.WarehouseOutputRequestWorkFlowUsers == null && !confirmationModel.WarehouseOutputRequestWorkFlowUsers.Any())
                    return ServiceResultFactory.CreateError<List<PendingForConfirmWarehouseOutputRequestDto>>(null, MessageId.DataInconsistency);

                if (!confirmationModel.WarehouseOutputRequestWorkFlowUsers.Any(c => c.UserId == authenticate.UserId && c.IsBallInCourt))
                    return ServiceResultFactory.CreateError<List<PendingForConfirmWarehouseOutputRequestDto>>(null, MessageId.AccessDenied);


                var userBallInCourtModel = confirmationModel.WarehouseOutputRequestWorkFlowUsers.FirstOrDefault(a => a.IsBallInCourt && a.UserId == authenticate.UserId);
                userBallInCourtModel.DateEnd = DateTime.UtcNow;
                if (model.IsAccept)
                {
                    userBallInCourtModel.IsBallInCourt = false;
                    userBallInCourtModel.IsAccept = true;
                    userBallInCourtModel.Note = model.Note;
                    if (!confirmationModel.WarehouseOutputRequestWorkFlowUsers.Any(a => a.IsAccept == false))
                    {
                        confirmationModel.Status = ConfirmationWorkFlowStatus.Confirm;
                        confirmationModel.WarehouseOutputRequest.Status = WarehouseOutputStatus.Confirmed;

                    }
                    else
                    {
                        var nextBallInCourtModel = confirmationModel.WarehouseOutputRequestWorkFlowUsers.Where(a => !a.IsAccept)
                             .OrderBy(a => a.OrderNumber)
                             .FirstOrDefault();

                        nextBallInCourtModel.IsBallInCourt = true;
                        userBallInCourtModel.DateStart = DateTime.UtcNow;
                    }
                }
                else
                {
                    userBallInCourtModel.IsAccept = false;
                    userBallInCourtModel.Note = model.Note;
                    confirmationModel.Status = ConfirmationWorkFlowStatus.Reject;

                    confirmationModel.WarehouseOutputRequest.Status = WarehouseOutputStatus.Rejected;

                }

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    await _scmLogAndNotificationService.SetDonedNotificationAsync(authenticate.UserId, confirmationModel.WarehouseOutputRequest.ContractCode, confirmationModel.RequestId.ToString(), NotifEvent.ConfirmWarehouseOutputRequest);
                   
                    int? userId = null;
                    if (model.IsAccept && confirmationModel.WarehouseOutputRequest.Status != WarehouseOutputStatus.Confirmed)
                    {
                        userId = confirmationModel.WarehouseOutputRequestWorkFlowUsers.First(a => a.IsBallInCourt).UserId;
                    }
                    if (confirmationModel.WarehouseOutputRequest.Status != WarehouseOutputStatus.Confirmed)
                    {
                        await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                        {
                            ContractCode = confirmationModel.WarehouseOutputRequest.ContractCode,
                            FormCode = confirmationModel.WarehouseOutputRequest.RequestCode,
                            KeyValue = confirmationModel.WarehouseOutputRequest.RequestId.ToString(),
                            NotifEvent = NotifEvent.ConfirmWarehouseOutputRequest,
                            Description= userId==null?"4":"2",
                            ProductGroupId = productGroup.Id,
                            RootKeyValue = confirmationModel.RequestId.ToString(),
                            Message = productGroup.Title,
                            PerformerUserId = authenticate.UserId,
                            PerformerUserFullName = authenticate.UserFullName,

                        },
                     productGroup.Id
                    , NotifEvent.ConfirmWarehouseOutputRequest, userId);
                    }
                    if (confirmationModel.WarehouseOutputRequest.Status == WarehouseOutputStatus.Confirmed)
                    {
                        await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                        {
                            ContractCode = confirmationModel.WarehouseOutputRequest.ContractCode,
                            FormCode = confirmationModel.WarehouseOutputRequest.RequestCode,
                            KeyValue = confirmationModel.WarehouseOutputRequest.RequestId.ToString(),
                            NotifEvent = NotifEvent.ConfirmWarehouseOutputRequest,
                            ProductGroupId = productGroup.Id,
                            RootKeyValue = confirmationModel.RequestId.ToString(),
                            Description ="2",
                            Message = productGroup.Title,
                            PerformerUserId = authenticate.UserId,
                            PerformerUserFullName = authenticate.UserFullName,

                        },
                    productGroup.Id
                    , ConfirmSendNotification());
                    }
                    var result = await GetPendingwarehouseRequisitionListAsync(authenticate, new WarehouseOutputQueryDto { Page = 1, PageSize = 9999 });
                    if (result.Succeeded)
                        return ServiceResultFactory.CreateSuccess(result.Result);
                    else
                        return ServiceResultFactory.CreateSuccess(new List<PendingForConfirmWarehouseOutputRequestDto>());
                }
                return ServiceResultFactory.CreateError<List<PendingForConfirmWarehouseOutputRequestDto>>(null, MessageId.SaveFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<PendingForConfirmWarehouseOutputRequestDto>>(null, exception);
            }
        }
        public async Task<ServiceResult<List<PendingForConfirmWarehouseOutputRequestDto>>> GetPendingwarehouseRequisitionListAsync(AuthenticateDto authenticate, WarehouseOutputQueryDto query)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<PendingForConfirmWarehouseOutputRequestDto>>(null, MessageId.AccessDenied);

                var dbQuery = _warehouseOutputRequestWorkFlowRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.WarehouseOutputRequest.ContractCode == authenticate.ContractCode && !a.WarehouseOutputRequest.IsDeleted && a.Status == ConfirmationWorkFlowStatus.Pending)
                    .OrderByDescending(c => c.RequestId).AsQueryable();
                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a =>
                   a.WarehouseOutputRequest.RequestCode.Contains(query.SearchText) ||
                   a.WarehouseOutputRequest.Subjects.Any(c => c.Product.Description.Contains(query.SearchText) ||
                   c.Product.ProductCode.Contains(query.SearchText)));
                var totalCount = dbQuery.Count();

                dbQuery = dbQuery.ApplayPageing(query);

                var result = await dbQuery.Select(r => new PendingForConfirmWarehouseOutputRequestDto
                {
                    RequestId = r.RequestId,
                    ProductGroupTitle = r.WarehouseOutputRequest.Subjects.First().Product.ProductGroup.Title,
                    Products = r.WarehouseOutputRequest.Subjects.Select(a => a.Product.Description).ToList(),
                    RecepitCode = (!String.IsNullOrEmpty(r.WarehouseOutputRequest.RecepitCode)) ? r.WarehouseOutputRequest.RecepitCode : "",
                    RequestCode = r.WarehouseOutputRequest.RequestCode,
                    BallInCourtUser = r.WarehouseOutputRequestWorkFlowUsers.Any() ?
                    r.WarehouseOutputRequestWorkFlowUsers.Where(a => a.IsBallInCourt)
                    .Select(c => new UserAuditLogDto
                    {
                        AdderUserId = c.UserId,
                        AdderUserName = c.User.FirstName + " " + c.User.LastName,
                        AdderUserImage = c.User.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + c.User.Image : ""
                    }).FirstOrDefault() : new UserAuditLogDto(),
                    UserAudit = r.AdderUser != null ? new UserAuditLogDto
                    {
                        AdderUserName = r.AdderUser.FullName,
                        AdderUserImage = r.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + r.AdderUser.Image : "",
                        CreateDate = r.CreatedDate.ToUnixTimestamp()
                    } : null,

                }).ToListAsync();
                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<PendingForConfirmWarehouseOutputRequestDto>>(null, exception);
            }
        }
        public async Task<ServiceResult<List<PendingForConfirmWarehouseOutputRequestDto>>> GetConfrimWarehouseRequisitionListAsync(AuthenticateDto authenticate, WarehouseOutputQueryDto query)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<PendingForConfirmWarehouseOutputRequestDto>>(null, MessageId.AccessDenied);

                var dbQuery = _warehouseOutputRequestWorkFlowRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.WarehouseOutputRequest.ContractCode == authenticate.ContractCode && !a.WarehouseOutputRequest.IsDeleted && a.WarehouseOutputRequest.Status == WarehouseOutputStatus.Confirmed)
                    .OrderByDescending(c => c.RequestId).AsQueryable();

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a =>
                   a.WarehouseOutputRequest.RequestCode.Contains(query.SearchText) ||
                   a.WarehouseOutputRequest.Subjects.Any(c => c.Product.Description.Contains(query.SearchText) ||
                   c.Product.ProductCode.Contains(query.SearchText)));

                var totalCount = dbQuery.Count();

                dbQuery = dbQuery.ApplayPageing(query);

                var result = await dbQuery.Select(r => new PendingForConfirmWarehouseOutputRequestDto
                {
                    RequestId = r.RequestId,
                    ProductGroupTitle = r.WarehouseOutputRequest.Subjects.First().Product.ProductGroup.Title,
                    Products = r.WarehouseOutputRequest.Subjects.Select(a => a.Product.Description).ToList(),
                    RecepitCode = (!String.IsNullOrEmpty(r.WarehouseOutputRequest.RecepitCode)) ? r.WarehouseOutputRequest.RecepitCode : "",
                    RequestCode = r.WarehouseOutputRequest.RequestCode,
                    BallInCourtUser = r.WarehouseOutputRequestWorkFlowUsers.Any() ?
                    r.WarehouseOutputRequestWorkFlowUsers.Where(a => a.IsBallInCourt)
                    .Select(c => new UserAuditLogDto
                    {
                        AdderUserId = c.UserId,
                        AdderUserName = c.User.FirstName + " " + c.User.LastName,
                        AdderUserImage = c.User.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + c.User.Image : ""
                    }).FirstOrDefault() : new UserAuditLogDto(),
                    UserAudit = r.AdderUser != null ? new UserAuditLogDto
                    {
                        AdderUserName = r.AdderUser.FullName,
                        AdderUserImage = r.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + r.AdderUser.Image : "",
                        CreateDate = r.CreatedDate.ToUnixTimestamp()
                    } : null,

                }).ToListAsync();
                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<PendingForConfirmWarehouseOutputRequestDto>>(null, exception);
            }
        }
        public async Task<ServiceResult<List<WarehouseOutputRequestListDto>>> GetWarehouseRequisitionListAsync(AuthenticateDto authenticate, WarehouseOutputQueryDto query)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<WarehouseOutputRequestListDto>>(null, MessageId.AccessDenied);

                var dbQuery = _warehouseOutputRequestRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode)
                    .OrderByDescending(c => c.RequestId).AsQueryable();
                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a =>
                   a.RequestCode.Contains(query.SearchText) ||
                   a.Subjects.Any(c => c.Product.Description.Contains(query.SearchText) ||
                   c.Product.ProductCode.Contains(query.SearchText)));

                var totalCount = dbQuery.Count();

                dbQuery = dbQuery.ApplayPageing(query);

                var result = await dbQuery.Select(r => new WarehouseOutputRequestListDto
                {
                    RequestId = r.RequestId,
                    ProductGroupTitle = r.Subjects.First().Product.ProductGroup.Title,
                    Products = r.Subjects.Select(a => a.Product.Description).ToList(),
                    RecepitCode = (!String.IsNullOrEmpty(r.RecepitCode)) ? r.RecepitCode : "",
                    RequestCode = r.RequestCode,
                    Status = r.Status,
                    CreatedDate = r.CreatedDate.ToUnixTimestamp()

                }).ToListAsync();
                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<WarehouseOutputRequestListDto>>(null, exception);
            }
        }


        public async Task<ServiceResult<WarehouseOutputRequestDetailsDto>> GetWarehouseRequisitionByRequestIdAsync(AuthenticateDto authenticate, long requestId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<WarehouseOutputRequestDetailsDto>(null, MessageId.AccessDenied);

                var dbQuery = _warehouseOutputRequestRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.RequestId == requestId)
                    .OrderByDescending(c => c.RequestId).AsQueryable();

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<WarehouseOutputRequestDetailsDto>(null, MessageId.EntityDoesNotExist);
                var result = await dbQuery.Select(r => new WarehouseOutputRequestDetailsDto
                {
                    RequestId = r.RequestId,
                    ProductGroupTitle = r.Subjects.First().Product.ProductGroup.Title,
                    RequestCode = r.RequestCode,
                    Status = r.Status,
                    Note = (r.WarehouseOutputRequestWorkFlow.FirstOrDefault() != null) ? r.WarehouseOutputRequestWorkFlow.First().ConfirmNote : "",
                    UserAudit = r.AdderUser != null ? new UserAuditLogDto
                    {
                        AdderUserName = r.AdderUser.FullName,
                        AdderUserImage = r.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + r.AdderUser.Image : "",
                        CreateDate = r.CreatedDate.ToUnixTimestamp()
                    } : null,
                    Subjects = r.Subjects.Where(a => !a.IsDeleted).Select(a => new WarehouseOutputRequestSubjectListDto
                    {
                        ProductCode = a.Product.ProductCode,
                        ProductDescription = a.Product.Description,
                        ProductGroupName = a.Product.ProductGroup.Title,
                        ProductId = a.Product.Id,
                        ProductTechnicalNumber = a.Product.TechnicalNumber,
                        ProductUnit = a.Product.Unit,
                        Quntity = a.Quantity,
                        RequestId = a.RequestId,
                        RequestSubjectId = a.RequestSubjectId,
                        Delivery = a.Delivery
                    }).ToList(),
                    WarehouseRequestConfirmationUserWorkFlows = (r.WarehouseOutputRequestWorkFlow.FirstOrDefault() != null) ? r.WarehouseOutputRequestWorkFlow.First().WarehouseOutputRequestWorkFlowUsers.Where(a => a.UserId > 0)
                            .Select(e => new WarehouseRequestConfirmationUserWorkFlowDto
                            {
                                WarehouseOutputRequestWorkFlowUserId = e.WarehouseOutputRequestWorkFlowUserId,
                                IsAccept = e.IsAccept,
                                IsBallInCourt = e.IsBallInCourt,
                                Note = e.Note,
                                OrderNumber = e.OrderNumber,
                                UserId = e.UserId,
                                UserFullName = e.User != null ? e.User.FullName : "",
                                UserImage = e.User != null && e.User.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + e.User.Image : "",
                                DateStart = e.DateStart.ToUnixTimestamp(),
                                DateEnd = e.DateEnd.ToUnixTimestamp()
                            }).ToList() : new List<WarehouseRequestConfirmationUserWorkFlowDto>(),

                }).FirstOrDefaultAsync();
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<WarehouseOutputRequestDetailsDto>(null, exception);
            }
        }
        public async Task<ServiceResult<WarehouseDespatchDetailDto>> GetWarehouseDespatchByDespatchIdAsync(AuthenticateDto authenticate, long despatchId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<WarehouseDespatchDetailDto>(null, MessageId.AccessDenied);

                var dbQuery = _warehouseDespatchRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.DespatchId == despatchId)
                    .OrderByDescending(c => c.RequestId).AsQueryable();

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<WarehouseDespatchDetailDto>(null, MessageId.EntityDoesNotExist);
                var result = await dbQuery.Select(r => new WarehouseDespatchDetailDto
                {
                    RequestId = r.RequestId,
                    ProductGroupTitle = r.WarehouseOutputRequest.Subjects.First().Product.ProductGroup.Title,
                    RequestCode = r.WarehouseOutputRequest.RequestCode,
                    Status = r.Status,
                    DespatchCode = r.DespatchCode,
                    DespatchId = r.DespatchId,
                    UserAudit = r.AdderUser != null ? new UserAuditLogDto
                    {
                        AdderUserName = r.AdderUser.FullName,
                        AdderUserImage = r.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + r.AdderUser.Image : "",
                        CreateDate = r.CreatedDate.ToUnixTimestamp()
                    } : null,
                    Subjects = r.WarehouseOutputRequest.Subjects.Where(a => !a.IsDeleted).Select(a => new WarehouseOutputRequestSubjectListDto
                    {
                        ProductCode = a.Product.ProductCode,
                        ProductDescription = a.Product.Description,
                        ProductGroupName = a.Product.ProductGroup.Title,
                        ProductId = a.Product.Id,
                        ProductTechnicalNumber = a.Product.TechnicalNumber,
                        ProductUnit = a.Product.Unit,
                        Quntity = a.Quantity,
                        RequestId = a.RequestId,
                        RequestSubjectId = a.RequestSubjectId,
                        Delivery = a.Delivery,
                    }).ToList(),


                }).FirstOrDefaultAsync();
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<WarehouseDespatchDetailDto>(null, exception);
            }
        }
        public async Task<ServiceResult<WarehouseOutputRequestDespatchInfoDto>> GetWaitingRequestForDespatchInfoByRequestIdAsync(AuthenticateDto authenticate, long requestId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<WarehouseOutputRequestDespatchInfoDto>(null, MessageId.AccessDenied);

                var dbQuery = _warehouseOutputRequestRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.RequestId == requestId)
                    .OrderByDescending(c => c.RequestId).AsQueryable();

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<WarehouseOutputRequestDespatchInfoDto>(null, MessageId.EntityDoesNotExist);
                var result = await dbQuery.Select(r => new WarehouseOutputRequestDespatchInfoDto
                {
                    RequestId = r.RequestId,
                    ProductGroupTitle = r.Subjects.First().Product.ProductGroup.Title,
                    RequestCode = r.RequestCode,
                    Status = r.Status,
                    RecepitCode = r.RecepitCode,
                    RecepitId = r.RequestId,
                    UserAudit = r.AdderUser != null ? new UserAuditLogDto
                    {
                        AdderUserName = r.AdderUser.FullName,
                        AdderUserImage = r.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + r.AdderUser.Image : "",
                        CreateDate = r.CreatedDate.ToUnixTimestamp()
                    } : null,
                    Subjects = r.Subjects.Where(a => !a.IsDeleted).Select(a => new WarehouseOutputRequestSubjectListDto
                    {
                        ProductCode = a.Product.ProductCode,
                        ProductDescription = a.Product.Description,
                        ProductGroupName = a.Product.ProductGroup.Title,
                        ProductId = a.Product.Id,
                        ProductTechnicalNumber = a.Product.TechnicalNumber,
                        ProductUnit = a.Product.Unit,
                        Quntity = a.Quantity,
                        RequestId = a.RequestId,
                        RequestSubjectId = a.RequestSubjectId,
                        Delivery = (a.Product.WarehouseProducts.Sum(a => a.Inventory) > a.Quantity) ? a.Quantity : a.Product.WarehouseProducts.Sum(a => a.Inventory),
                        Inventory = a.Product.WarehouseProducts.Sum(a => a.Inventory)
                    }).ToList(),


                }).FirstOrDefaultAsync();
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<WarehouseOutputRequestDespatchInfoDto>(null, exception);
            }
        }

        public async Task<ServiceResult<bool>> AddWarehouseDespatchAsync(AuthenticateDto authenticate, long requestId, AddWarehouseDespatchDto model)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                if (model == null || model.WarehouseDespatchItem == null || !model.WarehouseDespatchItem.Any())
                    return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                var requestModel = await _warehouseOutputRequestRepository
                    .Include(a => a.Subjects)
                    .Where(a => !a.IsDeleted && a.RequestId == requestId &&
                    a.ContractCode == authenticate.ContractCode &&
                    a.Status == WarehouseOutputStatus.Confirmed).FirstOrDefaultAsync();

                if (requestModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                var requestSubjectModels = requestModel.Subjects.Where(a => !a.IsDeleted).ToList();

                if (model.WarehouseDespatchItem.Count>1&&!model.WarehouseDespatchItem.Any(a => a.Delivery > 0))
                    return ServiceResultFactory.CreateError(false, MessageId.AllQuantityCantBeZeroInWarehouseDispatch);

                if (model.WarehouseDespatchItem.Count == 1 && !model.WarehouseDespatchItem.Any(a => a.Delivery > 0))
                    return ServiceResultFactory.CreateError(false, MessageId.InventoryIsZero);

                if (requestSubjectModels == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);


                var postedSubjectProductIds = model.WarehouseDespatchItem.Select(a => a.ProductId).ToList();

                var receiptSubjectProductIds = requestSubjectModels.Select(a => a.ProductId).ToList();
                var productId = postedSubjectProductIds.First();
                var productGroup = await _productGroupRepository.Where(a => a.Products.Any(a => a.Id == productId)).FirstOrDefaultAsync();
                if (postedSubjectProductIds.Any(c => !receiptSubjectProductIds.Contains(c)))
                    return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                var postReceiptProductIds = new List<int>();
                foreach (var item in model.WarehouseDespatchItem)
                {
                    postReceiptProductIds.Add(item.ProductId);
                }
                postReceiptProductIds = postReceiptProductIds.Distinct().ToList();

                var warehouseProducts = await _warehouseProductRepository
                    .Where(a => postReceiptProductIds.Contains(a.ProductId)).ToListAsync();

                if (warehouseProducts == null || warehouseProducts.Count() != postReceiptProductIds.Count())
                    return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                var warehouseDespatchModel = new WarehouseDespatch
                {
                    ContractCode = authenticate.ContractCode,
                    RequestId = requestId,
                    Status = WarehouseDespatchStatus.DespatchCompletely,
                    WarehouseProductStockLogs = new List<WarehouseProductStockLogs>()
                };

                var receiptRejectSubjects = new List<ReceiptRejectSubject>();
                foreach (var postedSubject in model.WarehouseDespatchItem)
                {
                    var selectedRequestSubjectModel = requestSubjectModels.FirstOrDefault(a => a.ProductId == postedSubject.ProductId);
                    if (selectedRequestSubjectModel == null)
                        return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);



                    if (selectedRequestSubjectModel.Quantity < 0)
                        return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                    if (postedSubject.Delivery > selectedRequestSubjectModel.Quantity)
                        return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                    var wProduct = warehouseProducts.FirstOrDefault(a => a.ProductId == postedSubject.ProductId);
                    if (wProduct == null)
                        return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                    wProduct.Inventory -= postedSubject.Delivery;
                    if (postedSubject.Delivery < selectedRequestSubjectModel.Quantity)
                        warehouseDespatchModel.Status = WarehouseDespatchStatus.DespatchPartially;
                    selectedRequestSubjectModel.Delivery = postedSubject.Delivery;
                    requestModel.Status = WarehouseOutputStatus.Done;
                    if (postedSubject.Delivery > 0)
                    {
                        warehouseDespatchModel.WarehouseProductStockLogs.Add(new WarehouseProductStockLogs
                        {
                            Input = 0,
                            Output = postedSubject.Delivery,
                            WarehouseStockChangeActionType = WarehouseStockChangeActionType.ExitedTheWarehouse,
                            DateChange = DateTime.UtcNow,
                            RealStock = wProduct.Inventory,
                            ProductId = selectedRequestSubjectModel.ProductId,
                            ReceiptId = requestModel.ReceiptId,
                        });
                    }

                }

                var count = await _warehouseDespatchRepository.CountAsync(a => a.ContractCode == authenticate.ContractCode);
                var codeRes = await _formConfigService.GenerateFormCodeAsync(authenticate.ContractCode, FormName.WarehouseDespatch, count);
                if (!codeRes.Succeeded)
                    return ServiceResultFactory.CreateError(false, codeRes.Messages.First().Message);

                warehouseDespatchModel.DespatchCode = codeRes.Result;

                _warehouseDespatchRepository.Add(warehouseDespatchModel);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = warehouseDespatchModel.ContractCode,
                        FormCode = warehouseDespatchModel.DespatchCode,
                        KeyValue = warehouseDespatchModel.DespatchId.ToString(),
                        NotifEvent = NotifEvent.AddWarehouseDispatch,
                        ProductGroupId = productGroup.Id,
                        Description= requestModel.RequestCode,
                        RootKeyValue = warehouseDespatchModel.DespatchId.ToString(),
                        RootKeyValue2 = (warehouseDespatchModel.Status==WarehouseDespatchStatus.DespatchCanceled)?"3": (warehouseDespatchModel.Status == WarehouseDespatchStatus.DespatchPartially) ?"2":"1",
                        Message = productGroup.Title,
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
        public async Task<ServiceResult<bool>> CancelWarehouseDespatchAsync(AuthenticateDto authenticate, long requestId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);


                var requestModel = await _warehouseOutputRequestRepository
                    .Include(a => a.Subjects)
                    .Where(a => !a.IsDeleted && a.RequestId == requestId &&
                    a.ContractCode == authenticate.ContractCode &&
                    a.Status == WarehouseOutputStatus.Confirmed).FirstOrDefaultAsync();
                var productId =  requestModel.Subjects.Select(a => a.ProductId).FirstOrDefault();
                if (requestModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);
                var productGroup = await _productGroupRepository.Where(a => a.Products.Any(a => a.Id == productId)).FirstOrDefaultAsync();
                var requestSubjectModels = requestModel.Subjects.Where(a => !a.IsDeleted).ToList();
                if (requestSubjectModels == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                var warehouseDespatchModel = new WarehouseDespatch
                {
                    ContractCode = authenticate.ContractCode,
                    RequestId = requestId,
                    Status = WarehouseDespatchStatus.DespatchCanceled,
                };
                requestModel.Status = WarehouseOutputStatus.Done;


                var count = await _warehouseDespatchRepository.CountAsync(a => a.ContractCode == authenticate.ContractCode);
                var codeRes = await _formConfigService.GenerateFormCodeAsync(authenticate.ContractCode, FormName.WarehouseDespatch, count);
                if (!codeRes.Succeeded)
                    return ServiceResultFactory.CreateError(false, codeRes.Messages.First().Message);

                warehouseDespatchModel.DespatchCode = codeRes.Result;

                _warehouseDespatchRepository.Add(warehouseDespatchModel);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = warehouseDespatchModel.ContractCode,
                        FormCode = warehouseDespatchModel.DespatchCode,
                        KeyValue = warehouseDespatchModel.DespatchId.ToString(),
                        NotifEvent = NotifEvent.AddWarehouseDispatch,
                        ProductGroupId = productGroup.Id,
                        Description = requestModel.RequestCode,
                        RootKeyValue = warehouseDespatchModel.DespatchId.ToString(),
                        RootKeyValue2 ="3",
                        Message = productGroup.Title,
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,
                    }, null);;

                    return ServiceResultFactory.CreateSuccess(true);
                }

                return ServiceResultFactory.CreateError(false, MessageId.SaveFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<List<WarehouseDespatchListDto>>> GetWarehouseDespatchListAsync(AuthenticateDto authenticate, WarehouseDespatchQueryDto query)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<WarehouseDespatchListDto>>(null, MessageId.AccessDenied);

                var dbQuery = _warehouseDespatchRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode)
                    .OrderByDescending(c => c.RequestId).AsQueryable();

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a =>
                    a.DespatchCode.Contains(query.SearchText) ||
                   a.WarehouseOutputRequest.Subjects.Any(c => c.Product.Description.Contains(query.SearchText) ||
                   c.Product.ProductCode.Contains(query.SearchText)));

                var totalCount = dbQuery.Count();

                dbQuery = dbQuery.ApplayPageing(query);

                var result = await dbQuery.Select(r => new WarehouseDespatchListDto
                {
                    RequestId = r.RequestId,
                    DespatchCode = r.DespatchCode,
                    DespatchId = r.DespatchId,
                    ProductGroupTitle = r.WarehouseOutputRequest.Subjects.First().Product.ProductGroup.Title,
                    Products = r.WarehouseOutputRequest.Subjects.Select(a => a.Product.Description).ToList(),
                    RequestCode = r.WarehouseOutputRequest.RequestCode,
                    RecepitCode = r.WarehouseOutputRequest.RecepitCode,
                    Status = r.Status,
                    CreatedDate = r.CreatedDate.ToUnixTimestamp()

                }).ToListAsync();
                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<WarehouseDespatchListDto>>(null, exception);
            }
        }

        private async Task<ServiceResult<PendingForConfirmWarehouseOutputRequestDto>> GetPendingwarehouseRequisitionByRequestIdAsync(AuthenticateDto authenticate, long requestId)
        {
            try
            {


                var dbQuery = _warehouseOutputRequestWorkFlowRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.WarehouseOutputRequest.ContractCode == authenticate.ContractCode && !a.WarehouseOutputRequest.IsDeleted && a.WarehouseOutputRequest.RequestId == requestId)
                    .OrderByDescending(c => c.RequestId).AsQueryable();



                var result = await dbQuery.Select(r => new PendingForConfirmWarehouseOutputRequestDto
                {
                    RequestId = r.RequestId,
                    ProductGroupTitle = r.WarehouseOutputRequest.Subjects.First().Product.ProductGroup.Title,
                    Products = r.WarehouseOutputRequest.Subjects.Select(a => a.Product.Description).ToList(),
                    RecepitCode = (!String.IsNullOrEmpty(r.WarehouseOutputRequest.RecepitCode)) ? r.WarehouseOutputRequest.RecepitCode : "",
                    RequestCode = r.WarehouseOutputRequest.RequestCode,
                    Status = r.WarehouseOutputRequest.Status,
                    BallInCourtUser = r.WarehouseOutputRequestWorkFlowUsers.Any() ?
                    r.WarehouseOutputRequestWorkFlowUsers.Where(a => a.IsBallInCourt)
                    .Select(c => new UserAuditLogDto
                    {
                        AdderUserId = c.UserId,
                        AdderUserName = c.User.FirstName + " " + c.User.LastName,
                        AdderUserImage = c.User.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + c.User.Image : ""
                    }).FirstOrDefault() : new UserAuditLogDto(),
                    UserAudit = r.AdderUser != null ? new UserAuditLogDto
                    {
                        AdderUserName = r.AdderUser.FullName,
                        AdderUserImage = r.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + r.AdderUser.Image : "",
                        CreateDate = r.CreatedDate.ToUnixTimestamp()
                    } : null,

                }).FirstOrDefaultAsync();
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<PendingForConfirmWarehouseOutputRequestDto>(null, exception);
            }
        }
        private ServiceResult<WarehouseOutputRequest> AddWharehouseOutputRequestSubject(AddWarehouseOutputRequestDto model, WarehouseOutputRequest requstModel)
        {
            foreach (var item in model.Subjects)
            {
                requstModel.Subjects.Add(new WarehouseOutputRequestSubject
                {
                    Quantity = item.Quantity,
                    ProductId = item.ProductId
                });
            }
            return ServiceResultFactory.CreateSuccess(requstModel);
        }

        private async Task<ServiceResult<WarehouseOutputRequestWorkFlow>> AddWarehouseOutputRequestConfirmationAsync(AddWarehouseRequestConfirmationDto model)
        {
            //if (usersIds.Count() == 0)
            //    return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);
            var warehouseRequestConfirmationModel = new WarehouseOutputRequestWorkFlow
            {

                ConfirmNote = model.Note,
                Status = ConfirmationWorkFlowStatus.Pending,
                WarehouseOutputRequestWorkFlowUsers = new List<WarehouseOutputRequestWorkFlowUser>(),

            };

            if (model.Users != null && model.Users.Any())
            {
                var usersIds = model.Users.Select(a => a.UserId).Distinct().ToList();
                if (await _userRepository.CountAsync(a => !a.IsDeleted && a.IsActive && usersIds.Contains(a.Id)) != usersIds.Count())
                    return ServiceResultFactory.CreateError<WarehouseOutputRequestWorkFlow>(null, MessageId.DataInconsistency);

                foreach (var item in model.Users)
                {
                    warehouseRequestConfirmationModel.WarehouseOutputRequestWorkFlowUsers.Add(new WarehouseOutputRequestWorkFlowUser
                    {
                        UserId = item.UserId,
                        OrderNumber = item.OrderNumber
                    });
                }
                if (warehouseRequestConfirmationModel.WarehouseOutputRequestWorkFlowUsers.Any())
                {
                    var bollincourtUser = warehouseRequestConfirmationModel.WarehouseOutputRequestWorkFlowUsers.OrderBy(a => a.OrderNumber).First();
                    bollincourtUser.IsBallInCourt = true;
                    bollincourtUser.DateStart = DateTime.UtcNow;
                }
            }
            else
            {
                warehouseRequestConfirmationModel.Status = ConfirmationWorkFlowStatus.Confirm;
            }



            return ServiceResultFactory.CreateSuccess(warehouseRequestConfirmationModel);
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
