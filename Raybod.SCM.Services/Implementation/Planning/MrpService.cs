using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using Raybod.SCM.DataAccess.Core;
using Raybod.SCM.DataAccess.Extention;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Audit;
using Raybod.SCM.DataTransferObject.Mrp;
using Raybod.SCM.DataTransferObject.MrpItem;
using Raybod.SCM.DataTransferObject.Notification;
using Raybod.SCM.DataTransferObject.PurchaseRequest;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Model;
using Raybod.SCM.Domain.Struct;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Services.Utilitys.exportToExcel;
using Raybod.SCM.Utility.Common;
using Raybod.SCM.Utility.EnumType;
using Raybod.SCM.Utility.Extention;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Implementation
{
    public class MrpService : IMrpService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPOService _poService;
        private readonly ITeamWorkAuthenticationService _authenticationService;
        private readonly ISCMLogAndNotificationService _scmLogAndNotificationService;
        private readonly IMasterMrService _masterMrService;
        private readonly IContractFormConfigService _formConfigService;
        private readonly DbSet<Mrp> _mrpRepository;
        private readonly DbSet<ProductGroup> _productGroupRepository;
        private readonly DbSet<PO> _poRepository;
        private readonly DbSet<PRContract> _prContractRepository;
        private readonly DbSet<MrpItem> _mrpPlanningRepository;
        private readonly DbSet<MasterMR> _masterMRRepository;
        private readonly DbSet<PurchaseRequest> _purchaseRequestRepository;
        private readonly DbSet<Contract> _contractRepository;
        private readonly CompanyConfig _appSettings;

        public MrpService(
            IUnitOfWork unitOfWork,
            ITeamWorkAuthenticationService authenticationService,
            IPOService poService,
            IOptions<CompanyAppSettingsDto> appSettings,
            ISCMLogAndNotificationService scmLogAndNotificationService,
            IMasterMrService masterMrService,
            IHttpContextAccessor httpContextAccessor,
            IContractFormConfigService formConfigService)
        {
            _unitOfWork = unitOfWork;
            _poService = poService;
            _masterMrService = masterMrService;
            _scmLogAndNotificationService = scmLogAndNotificationService;
            _authenticationService = authenticationService;
            _formConfigService = formConfigService;
            _prContractRepository = _unitOfWork.Set<PRContract>();
            _poRepository = _unitOfWork.Set<PO>();
            _mrpRepository = _unitOfWork.Set<Mrp>();
            _mrpPlanningRepository = _unitOfWork.Set<MrpItem>();
            _masterMRRepository = _unitOfWork.Set<MasterMR>();
            _contractRepository = _unitOfWork.Set<Contract>();
            _purchaseRequestRepository = _unitOfWork.Set<PurchaseRequest>();
            _productGroupRepository = _unitOfWork.Set<ProductGroup>();
            httpContextAccessor.HttpContext.Request.Headers.TryGetValue("companyCode", out var CompanyCode);
            _appSettings = appSettings.Value.CompanyConfig.First(a => a.CompanyCode == CompanyCode);
        }

        public async Task<ServiceResult<string>> AddMrpAsync(AuthenticateDto authenticate, string contractCode, int productGroupId, List<AddMrpItemDto> model)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError("", MessageId.AccessDenied);

                var contractModel = await _contractRepository
                    .Where(a => a.ContractCode == authenticate.ContractCode)
                    .Select(a => new
                    {
                        contractCode = a.ContractCode,
                        description = a.Description
                    }).FirstOrDefaultAsync();

                var productGroupModel = await _productGroupRepository.Where(a => a.Id == productGroupId).FirstOrDefaultAsync();
                if (productGroupModel == null)
                    return ServiceResultFactory.CreateError("", MessageId.DataInconsistency);

                if (model == null || model.Count() == 0)
                    return ServiceResultFactory.CreateError("", MessageId.InputDataValidationError);

                if (model.Any(a => a.FinalRequirment < 0 || a.DateStart > a.DateEnd))
                    return ServiceResultFactory.CreateError("", MessageId.InputDataValidationError);

                if (model.Any(a =>
                    a.GrossRequirement <= 0 || a.WarehouseStock < 0 || a.ReservedStock < 0 || a.SurplusQuantity < 0))
                    return ServiceResultFactory.CreateError("", MessageId.InputDataValidationError);


                if (model.Any(a =>
                    a.AddPoModel != null && (a.AddPoModel.Sum(c => c.OrderAmount) > a.FinalRequirment ||
                                             a.AddPoModel.Any(c => c.OrderAmount == 0))))
                    return ServiceResultFactory.CreateError("", MessageId.InputDataValidationError);

                var postedMrpProductIds = model.Select(a => a.ProductId).ToList();

                var MasterMrs = await _masterMRRepository
                    .Where(a => a.ContractCode == contractCode && a.Product.ProductGroupId == productGroupId && postedMrpProductIds.Contains(a.ProductId) &&
                                a.RemainedGrossRequirement > 0)
                    .Include(a => a.Product)
                    .ThenInclude(a => a.BomProducts)
                    .ToListAsync();

                if (MasterMrs == null || MasterMrs.Count() != postedMrpProductIds.Count())
                    return ServiceResultFactory.CreateError("", MessageId.DataInconsistency);

                if (model.Any(a => MasterMrs.Any(c => c.ProductId == a.ProductId && c.RemainedGrossRequirement < a.GrossRequirement)))
                    return ServiceResultFactory.CreateError("", MessageId.DataInconsistency);

                var mrpModel = new Mrp
                {
                    ProductGroupId = productGroupId,
                    ContractCode = contractModel.contractCode,
                    Description = contractModel.description,
                    MrpStatus = MrpStatus.Active,
                    MrpItems = new List<MrpItem>()
                };

                var AddPoRes = await AddMrpItemAsync(mrpModel, model, MasterMrs);
                if (!AddPoRes.Succeeded)
                    return ServiceResultFactory.CreateError("", MessageId.DataInconsistency);

                mrpModel.MrpItems = AddPoRes.Result;

                // generate form code
                var count = await _mrpRepository.CountAsync(a => a.ContractCode == authenticate.ContractCode && !a.MrpItems.Any(a => !a.IsDeleted && !a.BomProduct.IsRequiredMRP));
                var codeRes = await _formConfigService.GenerateFormCodeAsync(authenticate.ContractCode, FormName.MRP, count);
                if (!codeRes.Succeeded)
                    return ServiceResultFactory.CreateError("", codeRes.Messages.First().Message);

                mrpModel.MrpNumber = codeRes.Result;

                _mrpRepository.Add(mrpModel);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    if (!await _masterMRRepository.AnyAsync(x => x.ContractCode == mrpModel.ContractCode && x.Product.ProductGroupId == mrpModel.ProductGroupId && x.RemainedGrossRequirement > 0))
                        await _scmLogAndNotificationService.SetDonedNotificationByRootKeyValueAsync(authenticate.UserId, mrpModel.ContractCode, mrpModel.ProductGroupId.ToString(), NotifEvent.AddMRP);

                    var res = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = mrpModel.ContractCode,
                        FormCode = mrpModel.MrpNumber,
                        KeyValue = mrpModel.Id.ToString(),
                        NotifEvent = NotifEvent.AddMRP,
                        Description = mrpModel.Description,
                        RootKeyValue = mrpModel.Id.ToString(),
                        ProductGroupId = mrpModel.ProductGroupId,
                        Message = productGroupModel.Title,
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName
                    },
                    mrpModel.ProductGroupId,
                    CheckSendNotification(mrpModel));
                    return ServiceResultFactory.CreateSuccess(mrpModel.Id.ToString());
                }

                return ServiceResultFactory.CreateError("", MessageId.SaveFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException("", exception);
            }
        }
        private List<NotifToDto> CheckSendNotification(Mrp mrpModel)
        {
            var result = new List<NotifToDto>();
            var sendAddDocumentNotif = mrpModel.MrpItems.Any(a => a.RemainedStock > 0);
            if (sendAddDocumentNotif)
                result.Add(new NotifToDto
                {
                    NotifEvent = NotifEvent.AddPurchaseRequest,
                    Roles = new List<string>
                    {
                      SCMRole.PurchaseRequestReg
                    }
                });

            if (!result.Any())
                return null;

            return result;
        }


        public async Task<ServiceResult<List<MrpInfoDto>>> GetMrpAsync(AuthenticateDto authenticate, MrpQuery query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<MrpInfoDto>>(null, MessageId.AccessDenied);

                var dbQuery = _mrpRepository
                    .Include(a => a.MrpItems)
                     .AsNoTracking()
                     .Where(x => !x.IsDeleted && x.ContractCode == authenticate.ContractCode && !x.MrpNumber.Contains("AutoMRP"));

                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.ProductGroupIds.Contains(a.ProductGroupId));

                if (!string.IsNullOrEmpty(query.SearchText))
                {
                    dbQuery = dbQuery
                        .Where(x => x.MrpNumber.Contains(query.SearchText)
                                    || x.ContractCode.Contains(query.SearchText)
                                    || x.Contract.Description.Contains(query.SearchText)
                                    || x.ProductGroup.Title.Contains(query.SearchText)
                                    || x.MrpItems.Any(a => a.Product.ProductCode.Contains(query.SearchText) || a.Product.Description.Contains(query.SearchText) || a.Product.TechnicalNumber.Contains(query.SearchText)));
                }
                if (!string.IsNullOrEmpty(query.MrpNumber))
                    dbQuery = dbQuery.Where(x => x.MrpNumber == query.MrpNumber);

                var columnsMap = new Dictionary<string, Expression<Func<Mrp, object>>>
                {
                    ["Id"] = v => v.Id,
                    ["MrpNumber"] = v => v.MrpNumber,
                    ["ContractCode"] = v => v.ContractCode,
                    ["CreatedDate"] = v => v.CreatedDate,
                    ["Description"] = v => v.Description,
                };

                var totalCount = dbQuery.Count();
                dbQuery = dbQuery.ApplayOrdering(query, columnsMap).ApplayPageing(query);

                var resultList = await dbQuery.Select(c => new MrpInfoDto
                {
                    Id = c.Id,
                    Description = c.Description,
                    MrpNumber = c.MrpNumber,
                    MrpStatus = c.MrpStatus,
                    ContractCode = c.ContractCode,
                    ProductGroupId = c.ProductGroupId,
                    Products = GetProductsForMrp(c.MrpItems.Where(a => !a.IsDeleted).Select(v => v.Product.Description).ToList()),
                    ProductGroupTitle = c.ProductGroup.Title,
                    MrpItemQuantity = GetMrpItemsForMrp(c.MrpItems.ToList()),
                    UserAudit = c.AdderUser != null
                        ? new UserAuditLogDto
                        {
                            AdderUserId = c.AdderUserId,
                            AdderUserName = c.AdderUser.FullName,
                            CreateDate = c.CreatedDate.ToUnixTimestamp(),
                            AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall +
                                             c.AdderUser.Image
                        }
                        : null,
                }).ToListAsync();


                return ServiceResultFactory.CreateSuccess(resultList).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new List<MrpInfoDto>(), exception);
            }
        }

        public async Task<ServiceResult<List<MrpItemInfoDto>>> GetMrpItemsByMrpIdAsync(AuthenticateDto authenticate,
            long mrpId, MrpQuery query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<MrpItemInfoDto>>(null, MessageId.AccessDenied);

                var dbQuery = _mrpPlanningRepository
                     .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.MrpId == mrpId && a.Mrp.ContractCode == authenticate.ContractCode);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.Mrp.ProductGroupId)))
                    return ServiceResultFactory.CreateError<List<MrpItemInfoDto>>(null, MessageId.AccessDenied);

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a => a.Product.Description.Contains(query.SearchText)
                                                 || a.Product.ProductCode.Contains(query.SearchText));
                List<MrpItemInfoDto> result = new List<MrpItemInfoDto>();
                var tempResult = await dbQuery
                    .Select(m => new MrpItemInfoDto
                    {
                        Id = m.Id,
                        MrpId = m.MrpId,
                        DateEnd = m.DateEnd.ToUnixTimestamp(),
                        DateStart = m.DateStart.ToUnixTimestamp(),
                        GrossRequirement = m.GrossRequirement,
                        PO = m.PO,
                        ReservedStock = m.ReservedStock,
                        WarehouseStock = m.WarehouseStock,
                        SurplusQuantity = m.SurplusQuantity,
                        Unit = m.Product.Unit,
                        ProductCode = m.Product.ProductCode,
                        ProductDescription = m.Product.Description,
                        ProductId = m.ProductId,
                        MrpItemStatus = m.MrpItemStatus,
                        ProductGroupName = m.Product.ProductGroup.Title,
                        ProductTechnicalNumber = m.Product.TechnicalNumber,
                    }).ToListAsync();
                foreach (var item in tempResult)
                {
                    if (!result.Any(a => a.ProductId == item.ProductId))
                    {
                        result.Add(new MrpItemInfoDto
                        {
                            Id = item.Id,
                            MrpId = item.MrpId,
                            DateEnd = item.DateEnd,
                            DateStart = item.DateStart,
                            GrossRequirement = tempResult.Where(a => a.ProductId == item.ProductId).Sum(a => a.GrossRequirement),
                            PO = tempResult.Where(a => a.ProductId == item.ProductId).Sum(a => a.PO),
                            ReservedStock = tempResult.Where(a => a.ProductId == item.ProductId).Sum(a => a.ReservedStock),
                            WarehouseStock = tempResult.Where(a => a.ProductId == item.ProductId).Sum(a => a.WarehouseStock),
                            SurplusQuantity = tempResult.Where(a => a.ProductId == item.ProductId).Sum(a => a.SurplusQuantity),
                            Unit = item.Unit,
                            ProductCode = item.ProductCode,
                            ProductDescription = item.ProductDescription,
                            ProductId = item.ProductId,
                            MrpItemStatus = tempResult.Where(a => a.ProductId == item.ProductId).OrderByDescending(a => a.MrpItemStatus).First().MrpItemStatus,
                            ProductGroupName = item.ProductGroupName,
                            ProductTechnicalNumber = item.ProductTechnicalNumber,
                        });
                    }
                }
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<MrpItemInfoDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<MrpForEditDto>> GetMrpByMrpIdForEditAsync(AuthenticateDto authenticate,
            long mrpId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<MrpForEditDto>(null, MessageId.AccessDenied);

                var dbQuery = _mrpRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.Id == mrpId && a.ContractCode == authenticate.ContractCode);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.ProductGroupId)))
                    return ServiceResultFactory.CreateError<MrpForEditDto>(null, MessageId.AccessDenied);

                //todo: smehdi review all service
                var result = await dbQuery
                    .Select(m => new MrpForEditDto
                    {
                        ContractCode = m.ContractCode,
                        Description = m.Description,
                        MrpNumber = m.MrpNumber,
                        ProductGroupTitle = m.ProductGroup.Title,
                        MrpId = m.Id,
                        ProductGroupId = m.ProductGroupId,
                        MrpItems = m.MrpItems
                            .Where(a => !a.IsDeleted &&
                                        (!a.Product.PurchaseRequestItems.Any(a =>!a.PurchaseRequest.IsDeleted&&!a.IsDeleted&& a.PurchaseRequest.MrpId == mrpId)
                                         || !a.Product.POSubjects.Any(a =>!a.PO.IsDeleted&&
                                              a.PO.PORefType == PORefType.MRP&&a.PO.POStatus!=POStatus.Canceled)))
                            .Select(item => new MrpItemInfoDto
                            {
                                Id = item.Id,
                                MrpId = item.MrpId,
                                DateEnd = item.DateEnd.ToUnixTimestamp(),
                                DateStart = item.DateStart.ToUnixTimestamp(),
                                GrossRequirement = item.GrossRequirement,
                                FreeQuantityInPO = item.Product.PRContractSubjects
                                    .Where(c => c.PRContract.PRContractStatus == PRContractStatus.Active)
                                    .Sum(c => c.RemainedStock),
                                PO = item.PO,
                                ReservedStock = item.ReservedStock,
                                WarehouseStock = item.WarehouseStock,
                                SurplusQuantity = item.SurplusQuantity,
                                Unit = item.Product.Unit,
                                ProductCode = item.Product.ProductCode,
                                ProductGroupName = item.Product.ProductGroup.Title,
                                ProductTechnicalNumber = item.Product.TechnicalNumber,
                                ProductDescription = item.Product.Description,
                                ProductId = item.ProductId
                            }).ToList(),
                    }).FirstOrDefaultAsync();
                List<MrpItemInfoDto> tempMrpItems = new List<MrpItemInfoDto>();
                foreach (var item in result.MrpItems)
                {
                    if (!tempMrpItems.Any(a => a.ProductId == item.ProductId))
                    {
                        tempMrpItems.Add(new MrpItemInfoDto
                        {
                            Id = item.Id,
                            MrpId = item.MrpId,
                            DateEnd = item.DateEnd,
                            DateStart = item.DateStart,
                            GrossRequirement = result.MrpItems.Where(a => a.ProductId == item.ProductId).Sum(a => a.GrossRequirement),
                            PO = result.MrpItems.Where(a => a.ProductId == item.ProductId).Sum(a => a.PO),
                            ReservedStock = result.MrpItems.Where(a => a.ProductId == item.ProductId).Sum(a => a.ReservedStock),
                            WarehouseStock = result.MrpItems.Where(a => a.ProductId == item.ProductId).Sum(a => a.WarehouseStock),
                            SurplusQuantity = result.MrpItems.Where(a => a.ProductId == item.ProductId).Sum(a => a.SurplusQuantity),
                            Unit = item.Unit,
                            ProductCode = item.ProductCode,
                            ProductDescription = item.ProductDescription,
                            ProductId = item.ProductId,
                            MrpItemStatus = result.MrpItems.Where(a => a.ProductId == item.ProductId).OrderByDescending(a => a.MrpItemStatus).First().MrpItemStatus,
                            ProductGroupName = item.ProductGroupName,
                            ProductTechnicalNumber = item.ProductTechnicalNumber,
                            FreeQuantityInPO = result.MrpItems.Where(a => a.ProductId == item.ProductId).Sum(a => a.FreeQuantityInPO),
                        });
                    }
                }

                result.MrpItems = tempMrpItems;
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<MrpForEditDto>(null, exception);
            }
        }

        public async Task<ServiceResult<bool>> EditMrpAsync(AuthenticateDto authenticate, long mrpId,
            List<AddMrpItemDto> model)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery = _mrpRepository
                    .Where(mrp => !mrp.IsDeleted && mrp.Id == mrpId && mrp.ContractCode == authenticate.ContractCode);

                var mrpModel = await dbQuery
                               .Include(a => a.ProductGroup)
                               .Include(a => a.MrpItems)
                               .ThenInclude(a => a.BomProduct)
                               .FirstOrDefaultAsync();

                if (mrpModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                var mrpItemModels = await _mrpPlanningRepository
                    .Include(a => a.BomProduct)
                    .Include(a => a.PurchaseRequestItems)
                    .Where(a => !a.IsDeleted && a.MrpId == mrpId &&
                    (!a.Product.POSubjects.Any(a =>!a.PO.IsDeleted&& a.PO.PORefType == PORefType.MRP&& a.PO.POStatus != POStatus.Canceled)))
                    .ToListAsync();
                //if (mrpItemModels == null || !mrpItemModels.Any())
                //    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (permission.ProductGroupIds.Any() && !permission.ProductGroupIds.Contains(mrpModel.ProductGroupId))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                if (model == null || model.Count() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                if (model.Any(a => a.FinalRequirment < 0))
                    return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                if (model.Any(a =>
                    a.GrossRequirement <= 0 || a.WarehouseStock < 0 || a.ReservedStock < 0 || a.SurplusQuantity < 0))
                    return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                if (model.Any(a => a.AddPoModel != null && a.AddPoModel.Sum(c => c.Quantity) > a.FinalRequirment))
                    return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                var beforeMrpItem = mrpItemModels.Select(a => a.ProductId).ToList();
                var postedMrpProductIds = model.Select(a => a.ProductId).ToList();

                var MasterMrs = await _masterMRRepository
                    .Include(a => a.Product).ThenInclude(a => a.BomProducts)
                    .Where(a => a.ContractCode == mrpModel.ContractCode)
                    .ToListAsync();

                if (MasterMrs == null || MasterMrs.Count(c => postedMrpProductIds.Contains(c.ProductId)) != postedMrpProductIds.Count)
                    return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                foreach (var item in model)
                {
                    var masterMr = MasterMrs.FirstOrDefault(a => a.ProductId == item.ProductId);
                    var beforeData = mrpItemModels.FirstOrDefault(a => a.ProductId == item.ProductId);
                    if (beforeData == null)
                    {
                        if (item.GrossRequirement > masterMr.RemainedGrossRequirement)
                            return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);
                    }
                    else if (item.GrossRequirement > masterMr.RemainedGrossRequirement + mrpItemModels.Where(a => !a.IsDeleted).Sum(a => a.GrossRequirement))
                        return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);
                }

                if (model.Any(a => MasterMrs.Any(c =>
                    c.ProductId == a.ProductId &&
                    (c.RemainedGrossRequirement + a.GrossRequirement) < a.GrossRequirement)))
                    return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                var newItem = model.Where(a => !beforeMrpItem.Contains(a.ProductId)).ToList();
                var removeItem = mrpItemModels.Where(a => !postedMrpProductIds.Contains(a.ProductId)).ToList();
                var updateItem = model.Where(a => mrpItemModels.Any(c => !c.IsDeleted && c.ProductId == a.ProductId)).ToList();

                //add new MrpItems
                if (newItem != null && newItem.Count() > 0)
                {
                    var newItemProductIds = newItem.Select(a => a.ProductId).ToList();

                    var newMrpItemsRes = await AddMrpItemAsync(mrpModel, newItem, MasterMrs);
                    if (!newMrpItemsRes.Succeeded)
                        return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                    _mrpPlanningRepository.AddRange(newMrpItemsRes.Result);
                }

                //remove MrpItems
                foreach (var item in removeItem)
                {
                    var mmr = MasterMrs.FirstOrDefault(a => a.ProductId == item.ProductId);
                    if (mmr == null)
                        return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                    mmr.RemainedGrossRequirement += item.GrossRequirement;
                    item.BomProduct.Remained += item.GrossRequirement;
                    item.IsDeleted = true;
                }

                //Update rfpItem
                if (updateItem != null && updateItem.Count() > 0)
                {
                    var updateRes =
                        await UpdateMrpItemAsync(mrpModel, mrpItemModels, updateItem, MasterMrs);
                    if (!updateRes.Succeeded)
                        return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);
                }

                // before change => for get log
                string oldObject = _scmLogAndNotificationService.SerializerObject(mrpItemModels);

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var res = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = mrpModel.ContractCode,
                        FormCode = mrpModel.MrpNumber,
                        KeyValue = mrpModel.Id.ToString(),
                        NotifEvent = NotifEvent.EditMRP,
                        ProductGroupId = mrpModel.ProductGroupId,
                        Description = mrpModel.Description,
                        Message = mrpModel.ProductGroup.Title,
                        RootKeyValue = mrpModel.Id.ToString(),
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName
                    }, null);
                    return ServiceResultFactory.CreateSuccess(true);
                }

                return ServiceResultFactory.CreateError(false, MessageId.EditEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        //public async Task<ServiceResult<List<MrpMiniInfoDto>>> GetMrpBySearchAsync(AuthenticateDto authenticate,
        //    MrpQuery query)
        //{
        //    try
        //    {
        //        var dbQuery = _mrpRepository
        //            .AsNoTracking()
        //            .Where(x => !x.IsDeleted);

        //        var permissionResult =
        //            _authenticationService.GetAccessableContract(authenticate.UserId, authenticate.Roles);

        //        if (!permissionResult.HasPermisson)
        //            return ServiceResultFactory.CreateError(new List<MrpMiniInfoDto>(), MessageId.AccessDenied);

        //        if (!permissionResult.HasOrganizationPermission)
        //            dbQuery = dbQuery.Where(a => permissionResult.ContractCodes.Contains(a.ContractCode));

        //        if (!string.IsNullOrEmpty(query.SearchText))
        //            dbQuery = dbQuery
        //                .Where(x => x.MrpNumber.Contains(query.SearchText)
        //                            || x.ContractCode.Contains(query.SearchText)
        //                            || x.Contract.Description.Contains(query.SearchText));

        //        var columnsMap = new Dictionary<string, Expression<Func<Mrp, object>>>
        //        {
        //            ["Id"] = v => v.Id,
        //            ["Description"] = v => v.Description,
        //            ["MrpNumber"] = v => v.MrpNumber,
        //        };

        //        var totalCount = dbQuery.Count();
        //        dbQuery = dbQuery.ApplayOrdering(query, columnsMap).ApplayPageing(query);

        //        var resultList = await dbQuery.Select(c => new MrpMiniInfoDto
        //        {
        //            Id = c.Id,
        //            Description = c.Description,
        //            MrpNumber = c.MrpNumber,
        //            MrpStatus = c.MrpStatus,
        //        }).ToListAsync();

        //        return ServiceResultFactory.CreateSuccess(resultList).WithTotalCount(totalCount);
        //    }
        //    catch (Exception exception)
        //    {
        //        return ServiceResultFactory.CreateException(new List<MrpMiniInfoDto>(), exception);
        //    }
        //}

        //public async Task<ServiceResult<MrpInfoWithMrpItemDto>> GetMrpByIdIncludeMrpItemAsync(
        //    AuthenticateDto authenticate, long mrpId)
        //{
        //    try
        //    {
        //        var mrpModel = await _mrpRepository
        //            .AsNoTracking()
        //            .FirstOrDefaultAsync(x => x.Id == mrpId && !x.IsDeleted);
        //        if (mrpModel == null)
        //            return ServiceResultFactory.CreateError<MrpInfoWithMrpItemDto>(new MrpInfoWithMrpItemDto(),
        //                MessageId.EntityDoesNotExist);

        //        if (!_authenticationService.HasPermission(authenticate.UserId,
        //            mrpModel.ContractCode,
        //            authenticate.Roles))
        //            return ServiceResultFactory.CreateError(new MrpInfoWithMrpItemDto(), MessageId.AccessDenied);

        //        var result = await _mrpRepository
        //            .AsNoTracking()
        //            .Where(a => a.Id == mrpId).Select(m => new MrpInfoWithMrpItemDto
        //            {
        //                Id = m.Id,
        //                ContractCode = m.ContractCode,
        //                Description = m.Description,
        //                MrpNumber = m.MrpNumber,
        //                MrpStatus = m.MrpStatus,
        //                UserAudit = m.AdderUser != null
        //                ? new UserAuditLogDto
        //                {
        //                    AdderUserId = m.AdderUserId,
        //                    AdderUserName = m.AdderUser.FullName,
        //                    CreateDate = m.CreatedDate.ToUnixTimestamp(),
        //                    AdderUserImage = _appSettings.ElasticHost + ServiceSetting.UploadImagesPath.UserSmall +
        //                                     m.AdderUser.Image
        //                }
        //                : null,
        //                MrpItems = m.MrpItems.Where(x => !x.IsDeleted).Select(p => new MrpItemInfoDto
        //                {
        //                    Id = p.Id,
        //                    MrpId = p.MrpId,
        //                    DateEnd = p.DateEnd.ToUnixTimestamp(),
        //                    GrossRequirement = p.GrossRequirement,
        //                    ProductCode = p.Product.ProductCode,
        //                    ProductDescription = p.Product.Description,
        //                    ProductId = p.ProductId,
        //                    ReservedStock = p.ReservedStock,
        //                    SurplusQuantity = p.SurplusQuantity,
        //                    WarehouseStock = p.WarehouseStock,
        //                    Unit = p.Product.Unit
        //                }).ToList()
        //            }).FirstOrDefaultAsync();


        //        return ServiceResultFactory.CreateSuccess(result);
        //    }
        //    catch (Exception exception)
        //    {
        //        return ServiceResultFactory.CreateException(new MrpInfoWithMrpItemDto(), exception);
        //    }
        //}

        //public async Task<ServiceResult<bool>> AddMrpByExcelAsync(AuthenticateDto authenticate, string contractCode, IFormFile file, bool isPersianDate)
        //{
        //    try
        //    {
        //        var contractModel = await _contractRepository
        //        .Where(a => a.ContractCode == contractCode)
        //        .Select(a => new
        //        {
        //            contractCode = a.ContractCode,
        //            description = a.Description,
        //            ContractSubjectIds = a.ContractSubjects.Where(c => !c.IsDeleted)
        //                            .Select(c => new
        //                            {
        //                                pId = c.ProductId,
        //                            }).ToList()
        //        }).FirstOrDefaultAsync();

        //        var contractSubjectIds = contractModel.ContractSubjectIds.Select(a => a.pId).ToList();

        //        if (contractModel == null || contractSubjectIds == null || contractSubjectIds.Count() == 0)
        //            return ServiceResultFactory.CreateError(false, MessageId.ContractNotFound);

        //        if (!_authenticationService.HasPermission(authenticate.UserId, contractCode, authenticate.Roles))
        //            return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

        //        var importExcelResult = await ReadExcelFileAsync(file, isPersianDate);
        //        if (!importExcelResult.Succeeded)
        //            return ServiceResultFactory.CreateError(false, importExcelResult.Messages.First().Message);

        //        var mrpItemDto = importExcelResult.Result;

        //        if (mrpItemDto.Any(a => a.FinalRequirment < 0 || a.GrossRequirement < 0 || a.WarehouseStock < 0 || a.ReservedStock < 0 || a.SurplusQuantity < 0))
        //            return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

        //        var postedMrpProductIds = mrpItemDto.Select(a => a.ProductId).ToList();

        //        var MasterMrs = await _masterMRRepository
        //            .Where(a => a.ContractCode == contractCode && postedMrpProductIds.Contains(a.ProductId) && a.RemainedGrossRequirement > 0)
        //            .ToListAsync();

        //        if (MasterMrs == null || MasterMrs.Count() != postedMrpProductIds.Count())
        //            return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

        //        if (mrpItemDto.Any(a => MasterMrs.Any(c => c.ProductId == a.ProductId && c.RemainedGrossRequirement < a.GrossRequirement)))
        //            return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

        //        var mrpModel = new Mrp
        //        {
        //            ContractCode = contractModel.contractCode,
        //            Description = contractModel.description,
        //            MrpStatus = MrpStatus.Active,
        //            MrpItems = new List<MrpItem>()
        //        };

        //        mrpModel = AddMrpItem(mrpModel, mrpItemDto);
        //        mrpModel.MrpNumber = $"MRP-{mrpModel.ContractCode}-{(await _mrpRepository.CountAsync() + 1)}";
        //        _mrpRepository.Add(mrpModel);
        //        if (await _unitOfWork.SaveChangesAsync() > 0)
        //        {
        //            var res = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
        //            {
        //                BaseContractCode = mrpModel.ContractCode,
        //                FormCode = mrpModel.MrpNumber,
        //                KeyValue = mrpModel.Id.ToString(),
        //                NotifEvent = NotifEvent.AddMRP,
        //                Description = mrpModel.Description,
        //                NewValues = mrpModel,
        //                SCMEntityEnum = SCMEntityEnum.Mrps,
        //                RootKeyValue = mrpModel.Id.ToString(),
        //                PerformerUserId = authenticate.UserId,
        //                PerformerUserFullName = authenticate.UserFullName
        //            }, false);
        //            return ServiceResultFactory.CreateSuccess(true);
        //        }
        //        return ServiceResultFactory.CreateError(false, MessageId.SaveFailed);

        //    }
        //    catch (Exception exception)
        //    {
        //        return ServiceResultFactory.CreateException(false, exception);
        //    }
        //}

        public async Task<DownloadFileDto> ExportMasterMRAsync(AuthenticateDto authenticate, int productGroupId, MasterMRQueryDto query)
        {
            var serviceResult = await _masterMrService.GetMrpItemsByMrpIdForExportToExcelAsync(authenticate, productGroupId, query);
            if (!serviceResult.Succeeded)
                return null;

            //var excelItem = serviceResult.Result.Select(a => new ExportToExcelMrpItemDto
            //{
            //    dateEnd = a.DateEnd,
            //    po = a.PO,
            //    dateStart = a.DateStart,
            //    finalRequirment = a.FinalRequirment,
            //    freeQuantityInPO = a.FreeQuantityInPO,
            //    grossRequirement = a.GrossRequirement,
            //    netRequirement = a.NetRequirement,
            //    pr = a.PR,
            //    productCode = a.ProductCode,
            //    productDescription = a.ProductDescription,
            //    productGroupName = a.ProductGroupName,
            //    productId = a.ProductId,
            //    productTechnicalNumber = a.ProductTechnicalNumber,
            //    reservedStock = a.ReservedStock,
            //    surplusQuantity = a.SurplusQuantity,
            //    unit = a.Unit,
            //    warehouseStock = a.WarehouseStock

            //}).ToList();

            return ExcelHelper.ExportToExcel(serviceResult.Result, "Mrp", "MRPItem");
        }

        public async Task<ServiceResult<List<ExportMRPToExcelDto>>> ReadExcelFileAsync(AuthenticateDto authenticate,
         string contractCode, IFormFile formFile, bool isPersianDate)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<ExportMRPToExcelDto>>(null, MessageId.AccessDenied);

                if (formFile == null || formFile.Length <= 0)
                {
                    return ServiceResultFactory.CreateError<List<ExportMRPToExcelDto>>(null, MessageId.FileNotFound);
                }

                if (!Path.GetExtension(formFile.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                {
                    return ServiceResultFactory.CreateError<List<ExportMRPToExcelDto>>(null,
                        MessageId.InvalidFileExtention);
                }

                var list = new List<ExportMRPToExcelDto>();

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
                                var item = new ExportMRPToExcelDto();

                                item.ProductCode = worksheet.Cells[row, 1].Value.ToString().Trim();
                                item.ProductDescription = worksheet.Cells[row, 2].Value.ToString().Trim();
                                item.ProductTechnicalNumber = worksheet.Cells[row, 3].Value.ToString().Trim();
                                item.ProductGroupName = worksheet.Cells[row, 4].Value.ToString().Trim();
                                item.Unit = worksheet.Cells[row, 5].Value.ToString().Trim();
                                item.FreeQuantityInPO =
                                    Convert.ToDecimal(worksheet.Cells[row, 6].Value.ToString().Trim());
                                item.ProductId = int.Parse(worksheet.Cells[row, 7].Value.ToString().Trim());
                                item.GrossRequirement =
                                    Convert.ToDecimal(worksheet.Cells[row, 8].Value.ToString().Trim());
                                item.WarehouseStock =
                                    Convert.ToDecimal(worksheet.Cells[row, 9].Value.ToString().Trim());
                                item.ReservedStock =
                                    Convert.ToDecimal(worksheet.Cells[row, 10].Value.ToString().Trim());
                                item.SurplusQuantity =
                                    Convert.ToDecimal(worksheet.Cells[row, 12].Value.ToString().Trim());
                                item.PO = 0;
                                item.DateStart = isPersianDate
                                    ? (worksheet.Cells[row, 16].Value.ToString().Trim())
                                    .ToGregorianDateFromPersianDate().ToUnixTimestamp().ToString()
                                    : (worksheet.Cells[row, 16].Value.ToString().Trim()).ToGregorianDate()
                                    .ToUnixTimestamp().ToString();

                                item.DateEnd = isPersianDate
                                    ? (worksheet.Cells[row, 17].Value.ToString().Trim())
                                    .ToGregorianDateFromPersianDate().ToUnixTimestamp().ToString()
                                    : (worksheet.Cells[row, 17].Value.ToString().Trim()).ToGregorianDate()
                                    .ToUnixTimestamp().ToString();
                                list.Add(item);
                            }
                        }
                        catch (Exception exception)
                        {
                            return ServiceResultFactory.CreateError<List<ExportMRPToExcelDto>>(null,
                                MessageId.InputDataValidationError);
                        }
                    }
                }

                if (list == null || list.Count() == 0)
                    return ServiceResultFactory.CreateError<List<ExportMRPToExcelDto>>(null,
                        MessageId.InputDataValidationError);
                else
                    return ServiceResultFactory.CreateSuccess(list);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<ExportMRPToExcelDto>>(null, exception);
            }
        }


        #region PR
        public async Task<ServiceResult<List<WaitingMrpForNewPRDto>>> GetWaitingMrpForNewPrAsync(AuthenticateDto authenticate, MrpQuery query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<WaitingMrpForNewPRDto>>(null, MessageId.AccessDenied);

                var dbQuery = _mrpRepository
                    .AsNoTracking()
                    .OrderByDescending(a => a.Id)
                    .Where(x => !x.IsDeleted && x.ContractCode == authenticate.ContractCode && x.MrpItems.Any(c => !c.IsDeleted && c.RemainedStock > 0));

                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(a => a.MrpItems.Any(c => !c.IsDeleted && c.RemainedStock > 0 && permission.ProductGroupIds.Contains(c.Product.ProductGroupId)));

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery
                        .Where(a =>
                        a.MrpNumber.Contains(query.SearchText)
                        || a.ContractCode.Contains(query.SearchText)
                        || a.MrpItems.Any(c => c.Product.ProductCode.Contains(query.SearchText) || c.Product.Description.Contains(query.SearchText) || c.Product.ProductGroup.Title.Contains(query.SearchText)));

                var totalCount = dbQuery.Count();
                dbQuery = dbQuery.ApplayPageing(query);
             
                var result = await dbQuery.Select(x => new WaitingMrpForNewPRDto
                {

                    ContractCode = x.ContractCode,
                    Description = x.Description,
                    Id = x.Id,
                    MrpNumber = x.MrpNumber,
                    MrpStatus = x.MrpStatus,
                    ProductGroupTitle = x.ProductGroup.Title,
                    ProductGroupId = x.ProductGroupId,
                    MrpItemQuantity = GetMrpItemsForPurchaseRequest(x.MrpItems.ToList()),
                    DateStart = x.MrpItems
                    .OrderBy(c => c.DateStart)
                    .Select(c => c.DateStart)
                    .FirstOrDefault()
                    .ToUnixTimestamp(),
                    DateEnd = x.MrpItems
                    .OrderByDescending(c => c.DateEnd)
                    .Select(c => c.DateEnd)
                    .FirstOrDefault()
                    .ToUnixTimestamp(),
                    Products = GetProductsForPurchaseRequest(x.MrpItems.Where(a => !a.IsDeleted && a.RemainedStock > 0)
                         .Select(v => v.Product.Description).ToList()),
                    PurchasingStream = x.MrpItems.Any(a => !a.BomProduct.IsRequiredMRP) ? PurchasingStream.WithoutMrp : PurchasingStream.WithMrp,
                    UserAudit = x.ModifierUser != null ? new UserAuditLogDto
                    {
                        AdderUserId = x.ModifierUserId,
                        AdderUserName = x.ModifierUser.FullName,
                        CreateDate = x.UpdateDate.ToUnixTimestamp(),
                        AdderUserImage = x.ModifierUser.Image != null
                             ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + x.ModifierUser.Image
                             : null,
                    } : null
                }).ToListAsync();
                
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<WaitingMrpForNewPRDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<MrpInfoDto>> GetWaitingMrpByIdForNewPrAsync(AuthenticateDto authenticate, long mrpId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<MrpInfoDto>(null, MessageId.AccessDenied);

                var dbQuery = _mrpRepository
                    .AsNoTracking()
                    .Where(x => x.Id == mrpId && !x.IsDeleted && x.ContractCode == authenticate.ContractCode);

                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(x => x.MrpItems.Any(c => !c.IsDeleted && c.RemainedStock > 0 && permission.ProductGroupIds.Contains(c.Product.ProductGroupId)));
                else
                    dbQuery = dbQuery.Where(x => x.MrpItems.Any(c => !c.IsDeleted && c.RemainedStock > 0));

                var mrpModel = await dbQuery
                    .Select(x => new MrpInfoDto
                    {
                        ContractCode = x.ContractCode,
                        Description = x.Description,
                        Id = x.Id,
                        MrpNumber = x.MrpNumber,
                        MrpStatus = x.MrpStatus,
                        ProductGroupTitle = x.ProductGroup.Title,
                        ProductGroupId = x.ProductGroupId,
                        Products = x.MrpItems.Where(a => !a.IsDeleted && a.RemainedStock > 0)
                        .Select(v => v.Product.Description).Take(10).ToList(),
                        PurchasingStream=(!x.MrpItems.Any(a=>!a.IsDeleted&&a.BomProduct.IsRequiredMRP))?PurchasingStream.WithoutMrp: PurchasingStream.WithMrp,
                        UserAudit = x.ModifierUser != null ? new UserAuditLogDto
                        {
                            AdderUserId = x.ModifierUserId,
                            AdderUserName = x.ModifierUser.FullName,
                            CreateDate = x.UpdateDate.ToUnixTimestamp(),
                            AdderUserImage = x.ModifierUser.Image != null
                            ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + x.ModifierUser.Image
                            : null,
                        } : null
                    })
                    .FirstOrDefaultAsync();

                if (mrpModel == null)
                    return ServiceResultFactory.CreateError<MrpInfoDto>(null, MessageId.AccessDenied);

                //if (!_authenticationService.HasPermission(authenticate.UserId,
                //    mrpModel.ContractCode,
                //    authenticate.Roles))
                //    return ServiceResultFactory.CreateError<MrpInfoDto>(null, MessageId.AccessDenied);

                return ServiceResultFactory.CreateSuccess(mrpModel);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<MrpInfoDto>(null, exception);
            }
        }

        public async Task<ServiceResult<List<MrpPurchaseRequestItemDto>>> GetWaitingMrpItemsByMrpIdAsync(AuthenticateDto authenticate, long mrpId, MrpQuery query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<MrpPurchaseRequestItemDto>>(null, MessageId.AccessDenied);
                IQueryable<MrpItem> dbQuery;
               
                     dbQuery = _mrpPlanningRepository
                        .AsNoTracking()
                        .Where(a => !a.IsDeleted && a.MrpId == mrpId && a.Mrp.ContractCode == authenticate.ContractCode && a.RemainedStock > 0);


                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.ProductGroupIds.Contains(a.Product.ProductGroupId));

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(c => c.Product.ProductCode.Contains(query.SearchText) || c.Product.Description.Contains(query.SearchText));

                var totalCount = dbQuery.Count();

                //dbquey = dbquey.ApplayPageing(query);
                List<MrpPurchaseRequestItemDto> result = new List<MrpPurchaseRequestItemDto>();
                var tempResult = await dbQuery.Select(m => new MrpPurchaseRequestItemDto
                {
                    ProductId = m.ProductId,
                    DateEnd = m.DateEnd.ToUnixTimestamp(),
                    DateStart = m.DateStart.ToUnixTimestamp(),
                    ProductCode = m.Product.ProductCode,
                    ProductDescription = m.Product.Description,
                    ProductUnit = m.Product.Unit,
                    ProductGroupName = m.Product.ProductGroup.Title,
                    ProductTechnicalNumber = m.Product.TechnicalNumber,
                    Quntity = m.RemainedStock,
                    DocumentStatus =
                            !m.Product.DocumentProducts.Any(v => !v.Document.IsDeleted && v.Document.IsActive && v.Document.ContractCode == authenticate.ContractCode)
                            ? EngineeringDocumentStatus.No
                            : m.Product.DocumentProducts.Any(v => !v.Document.IsDeleted && v.Document.IsActive && v.Document.ContractCode == authenticate.ContractCode &&
                            (!v.Document.DocumentRevisions.Any(a => !a.IsDeleted) || v.Document.DocumentRevisions.Any(n => !n.IsDeleted && n.RevisionStatus < RevisionStatus.Confirmed)))
                            ? EngineeringDocumentStatus.completing
                            : EngineeringDocumentStatus.Completed,
                }).ToListAsync();
                foreach (var item in tempResult)
                {
                    if (!result.Any(a => a.ProductId == item.ProductId))
                    {
                        result.Add(new MrpPurchaseRequestItemDto
                        {
                            ProductId = item.ProductId,
                            DateEnd = item.DateEnd,
                            DateStart = item.DateStart,
                            ProductCode = item.ProductCode,
                            ProductDescription = item.ProductDescription,
                            ProductUnit = item.ProductUnit,
                            ProductGroupName = item.ProductGroupName,
                            ProductTechnicalNumber = item.ProductTechnicalNumber,
                            Quntity = tempResult.Where(a => a.ProductId == item.ProductId).Sum(a => a.Quntity),
                            DocumentStatus = item.DocumentStatus
                        });
                    }
                }
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<MrpPurchaseRequestItemDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<List<MrpPurchaseRequestItemDto>>> GetWaitingMrpItemsByPurchaseRequestIdAsync(AuthenticateDto authenticate, long purchaseRequestId, MrpQuery query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<MrpPurchaseRequestItemDto>>(null, MessageId.AccessDenied);
                IQueryable<MrpItem> dbQuery;


                var purchaseRequest = await _purchaseRequestRepository.Include(a=>a.PurchaseRequestItems).FirstOrDefaultAsync(a => a.Id == purchaseRequestId);
                if(purchaseRequest==null)
                    return ServiceResultFactory.CreateError<List<MrpPurchaseRequestItemDto>>(null, MessageId.EntityDoesNotExist);
                dbQuery = _mrpPlanningRepository
                   .AsNoTracking()
                   .Where(a => !a.IsDeleted && a.MrpId == purchaseRequest.MrpId && a.Mrp.ContractCode == authenticate.ContractCode && (a.RemainedStock > 0||(a.PurchaseRequestItems.Any(b=>!b.IsDeleted&&b.PurchaseRequestId==purchaseRequestId))));


                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.ProductGroupIds.Contains(a.Product.ProductGroupId));

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(c => c.Product.ProductCode.Contains(query.SearchText) || c.Product.Description.Contains(query.SearchText));

                var totalCount = dbQuery.Count();

                //dbquey = dbquey.ApplayPageing(query);
                List<MrpPurchaseRequestItemDto> result = new List<MrpPurchaseRequestItemDto>();
                var tempResult = await dbQuery.Select(m => new MrpPurchaseRequestItemDto
                {
                    ProductId = m.ProductId,
                    DateEnd = m.DateEnd.ToUnixTimestamp(),
                    DateStart = m.DateStart.ToUnixTimestamp(),
                    ProductCode = m.Product.ProductCode,
                    ProductDescription = m.Product.Description,
                    ProductUnit = m.Product.Unit,
                    ProductGroupName = m.Product.ProductGroup.Title,
                    ProductTechnicalNumber = m.Product.TechnicalNumber,
                    Quntity = m.RemainedStock,
                    DocumentStatus =
                            !m.Product.DocumentProducts.Any(v => !v.Document.IsDeleted && v.Document.IsActive && v.Document.ContractCode == authenticate.ContractCode)
                            ? EngineeringDocumentStatus.No
                            : m.Product.DocumentProducts.Any(v => !v.Document.IsDeleted && v.Document.IsActive && v.Document.ContractCode == authenticate.ContractCode &&
                            (!v.Document.DocumentRevisions.Any(a => !a.IsDeleted) || v.Document.DocumentRevisions.Any(n => !n.IsDeleted && n.RevisionStatus < RevisionStatus.Confirmed)))
                            ? EngineeringDocumentStatus.completing
                            : EngineeringDocumentStatus.Completed,
                }).ToListAsync();
          
                foreach (var item in tempResult)
                {
                    if (!result.Any(a => a.ProductId == item.ProductId))
                    {
                        result.Add(new MrpPurchaseRequestItemDto
                        {
                            ProductId = item.ProductId,
                            DateEnd = item.DateEnd,
                            DateStart = item.DateStart,
                            ProductCode = item.ProductCode,
                            ProductDescription = item.ProductDescription,
                            ProductUnit = item.ProductUnit,
                            ProductGroupName = item.ProductGroupName,
                            ProductTechnicalNumber = item.ProductTechnicalNumber,
                            Quntity =(purchaseRequest.PurchaseRequestItems.Any(a=>!a.IsDeleted&&a.ProductId==item.ProductId))?purchaseRequest.PurchaseRequestItems.Where(a=>!a.IsDeleted&&a.ProductId==item.ProductId).Sum(a=>a.Quntity)+ tempResult.Where(a => a.ProductId == item.ProductId).Sum(a => a.Quntity) : tempResult.Where(a => a.ProductId == item.ProductId).Sum(a => a.Quntity),
                            DocumentStatus = item.DocumentStatus
                        });
                    }
                }
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<MrpPurchaseRequestItemDto>>(null, exception);
            }
        }
        #endregion

        //private Mrp AddMrpItem(Mrp mrpModel, List<ImportMrpItemFromExcelDto> mrpItemDto)
        //{
        //    foreach (var item in mrpItemDto)
        //    {
        //        var newItem = new MrpItem();

        //        newItem.ProductId = item.ProductId;
        //        newItem.GrossRequirement = item.GrossRequirement;
        //        newItem.ReservedStock = item.ReservedStock;
        //        newItem.WarehouseStock = item.WarehouseStock;
        //        newItem.NetRequirement = item.NetRequirement;
        //        newItem.SurplusQuantity = item.SurplusQuantity;
        //        newItem.FinalRequirment = item.FinalRequirment;
        //        newItem.PR = item.PR;
        //        newItem.RemainedStock = item.FinalRequirment;
        //        newItem.RemainedStock = item.FinalRequirment;
        //        newItem.DateEnd = item.DateEnd;
        //        newItem.DateStart = item.DateStart;
        //        newItem.DoneStock = 0;
        //        newItem.PO = 0;
        //        mrpModel.MrpItems.Add(newItem);
        //    }

        //    return mrpModel;
        //}

        private async Task<ServiceResult<bool>> UpdateMrpItemAsync(Mrp mrpModel, List<MrpItem> mrpItems,
            List<AddMrpItemDto> model, List<MasterMR> masterMRs)
        {
            try
            {
                foreach (var postedMrpItem in model.Where(a => !(mrpItems.Count(b => b.ProductId == a.ProductId) > 1)))
                {
                    var masterMrItem = masterMRs.FirstOrDefault(a => a.ProductId == postedMrpItem.ProductId);
                    var mrpItemModel = mrpItems.FirstOrDefault(a => a.ProductId == postedMrpItem.ProductId);
                    if (masterMrItem == null || mrpItemModel == null)
                        return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);
                    if (mrpItemModel.DoneStock > postedMrpItem.GrossRequirement)
                        return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);
                    masterMrItem.RemainedGrossRequirement += mrpItemModel.GrossRequirement;
                    masterMrItem.RemainedGrossRequirement -= postedMrpItem.GrossRequirement;
                    mrpItemModel.BomProduct.Remained += mrpItemModel.GrossRequirement;
                    postedMrpItem.PO = postedMrpItem.AddPoModel != null
                        ? postedMrpItem.AddPoModel.Sum(a => a.Quantity)
                        : 0;
                    if (postedMrpItem.GrossRequirement == 0)
                    {
                        mrpItemModel.IsDeleted = true;
                        continue;
                    }
                    mrpItemModel.GrossRequirement = postedMrpItem.GrossRequirement;
                    mrpItemModel.WarehouseStock = postedMrpItem.WarehouseStock;
                    mrpItemModel.ReservedStock = postedMrpItem.ReservedStock;
                    mrpItemModel.SurplusQuantity = postedMrpItem.SurplusQuantity;
                    mrpItemModel.NetRequirement = postedMrpItem.NetRequirement;
                    mrpItemModel.FinalRequirment = postedMrpItem.FinalRequirment;
                    mrpItemModel.PO = postedMrpItem.PO;
                    mrpItemModel.DateStart = postedMrpItem.DateStart.UnixTimestampToDateTime().Date;
                    mrpItemModel.DateEnd = postedMrpItem.DateEnd.UnixTimestampToDateTime().Date;

                    mrpItemModel.PR = mrpItemModel.FinalRequirment - mrpItemModel.PO;
                    mrpItemModel.RemainedStock = mrpItemModel.PR - mrpItemModel.DoneStock;

                    mrpItemModel.BomProduct.Remained -= mrpItemModel.GrossRequirement;
                    if (postedMrpItem.AddPoModel != null && postedMrpItem.AddPoModel.Count() > 0)
                    {
                        foreach (var item in postedMrpItem.AddPoModel)
                        {
                            var prContractModel = await _prContractRepository
                                .Where(a => a.Id == item.PRContractId)
                                .Include(a => a.PRContractSubjects)
                                .FirstOrDefaultAsync();
                            //.Include(a => a.TermsOfPayments)

                            if (prContractModel == null)
                                return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);
                            var res = _poService.AddPOToPendingByMRP(mrpModel, mrpItemModel,
                                postedMrpItem.DateEnd.UnixTimestampToDateTime(), item, prContractModel);
                            if (!res.Succeeded)
                                return ServiceResultFactory.CreateError(false, res.Messages.First().Message);

                            _poRepository.Add(res.Result);
                        }
                    }
                }


                foreach (var postedItem in model.Where(a => (mrpItems.Count(b => b.ProductId == a.ProductId) > 1)))
                {
                    var masterMrItem = masterMRs.FirstOrDefault(a => a.ProductId == postedItem.ProductId);
                    var mrpItemModel = mrpItems.Where(a => a.ProductId == postedItem.ProductId).ToList();
                    if (masterMrItem == null)
                        return ServiceResultFactory.CreateError(false,
                            MessageId.InputDataValidationError);

                    masterMrItem.RemainedGrossRequirement += mrpItemModel.Where(a => !a.IsDeleted).Sum(a => a.GrossRequirement);
                    masterMrItem.RemainedGrossRequirement -= postedItem.GrossRequirement;

                    postedItem.PO = postedItem.AddPoModel != null ? postedItem.AddPoModel.Sum(a => a.OrderAmount) : 0;
                    var neededAmount = postedItem.GrossRequirement;
                    var wareHouseStock = postedItem.WarehouseStock;
                    var reservedStock = postedItem.ReservedStock;
                    var isDeleted = false;
                    if (mrpItemModel.Sum(a => a.DoneStock) > postedItem.GrossRequirement)
                        return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);
                    foreach (var mrpItem in mrpItemModel)
                    {



                        mrpItem.BomProduct.Remained += mrpItem.GrossRequirement;
                        mrpItem.GrossRequirement = (mrpItem.BomProduct.CoefficientUse >= neededAmount) ? neededAmount : mrpItem.BomProduct.CoefficientUse;

                        mrpItem.WarehouseStock = (wareHouseStock <= mrpItem.GrossRequirement) ? wareHouseStock : wareHouseStock - mrpItem.GrossRequirement;
                        mrpItem.ReservedStock = (reservedStock <= mrpItem.GrossRequirement) ? reservedStock : reservedStock - mrpItem.GrossRequirement;
                        mrpItem.SurplusQuantity = postedItem.SurplusQuantity;
                        mrpItem.NetRequirement = (mrpItem.GrossRequirement + mrpItem.ReservedStock) - mrpItem.WarehouseStock;
                        mrpItem.FinalRequirment = mrpItem.NetRequirement + mrpItem.SurplusQuantity;
                        mrpItem.PO = postedItem.PO;
                        mrpItem.DateStart = postedItem.DateStart.UnixTimestampToDateTime().Date;
                        mrpItem.DateEnd = postedItem.DateEnd.UnixTimestampToDateTime().Date;
                        mrpItem.PR = mrpItem.FinalRequirment - mrpItem.PO;
                        mrpItem.RemainedStock = mrpItem.PR - mrpItem.DoneStock;
                        mrpItem.IsDeleted = isDeleted;

                        if (!isDeleted)
                            mrpItem.BomProduct.Remained -= mrpItem.GrossRequirement;
                        if (!isDeleted)
                            neededAmount -= mrpItem.GrossRequirement;
                        wareHouseStock = (wareHouseStock > 0) ? wareHouseStock - mrpItem.WarehouseStock : 0;
                        reservedStock = (reservedStock > 0) ? reservedStock - mrpItem.ReservedStock : 0;

                        postedItem.SurplusQuantity = (postedItem.SurplusQuantity > 0) ? postedItem.SurplusQuantity - mrpItem.SurplusQuantity : 0;

                        if (neededAmount <= 0)
                            isDeleted = true;

                    }
                    if (postedItem.AddPoModel != null && postedItem.AddPoModel.Count() > 0)
                    {
                        foreach (var item in postedItem.AddPoModel)
                        {
                            var prContractModel = await _prContractRepository
                                .Where(a => a.Id == item.PRContractId)
                                .Include(a => a.PRContractSubjects)
                                .FirstOrDefaultAsync();
                            //.Include(a => a.TermsOfPayments)

                            if (prContractModel == null)
                                return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);
                            var res = _poService.AddPOToPendingByMRP(mrpModel, mrpItemModel.First(),
                                postedItem.DateEnd.UnixTimestampToDateTime(), item, prContractModel);
                            if (!res.Succeeded)
                                return ServiceResultFactory.CreateError(false, res.Messages.First().Message);

                            _poRepository.Add(res.Result);
                        }
                    }
                    if (neededAmount > 0)
                    {
                        foreach (var bom in masterMrItem.Product.BomProducts.Where(a => !a.IsDeleted && a.Remained > 0))
                        {

                            var newMrpItem = new MrpItem();

                            newMrpItem.ProductId = postedItem.ProductId;
                            newMrpItem.GrossRequirement = (bom.Remained >= neededAmount) ? neededAmount : bom.Remained;
                            newMrpItem.BomProduct = bom;
                            newMrpItem.WarehouseStock = (wareHouseStock <= newMrpItem.GrossRequirement) ? wareHouseStock : wareHouseStock - newMrpItem.GrossRequirement;
                            newMrpItem.ReservedStock = (reservedStock <= newMrpItem.GrossRequirement) ? reservedStock : reservedStock - newMrpItem.GrossRequirement;
                            newMrpItem.SurplusQuantity = postedItem.SurplusQuantity;
                            newMrpItem.NetRequirement = (newMrpItem.GrossRequirement + newMrpItem.ReservedStock) - newMrpItem.WarehouseStock;
                            newMrpItem.FinalRequirment = newMrpItem.NetRequirement + newMrpItem.SurplusQuantity;
                            newMrpItem.MrpItemStatus = MrpItemStatus.MRP;
                            newMrpItem.MasterMRId = masterMrItem.Id;
                            newMrpItem.PO = postedItem.PO;
                            newMrpItem.DateStart = postedItem.DateStart.UnixTimestampToDateTime().Date;
                            newMrpItem.DateEnd = postedItem.DateEnd.UnixTimestampToDateTime().Date;
                            newMrpItem.DoneStock = postedItem.PO;

                            newMrpItem.PR = newMrpItem.FinalRequirment - newMrpItem.PO;
                            newMrpItem.RemainedStock = postedItem.PR;
                            if (mrpModel.Id > 0)
                                newMrpItem.MrpId = mrpModel.Id;

                            await _mrpPlanningRepository.AddAsync(newMrpItem);

                            bom.Remained -= newMrpItem.GrossRequirement;
                            neededAmount -= newMrpItem.GrossRequirement;
                            wareHouseStock = (wareHouseStock > 0) ? wareHouseStock - newMrpItem.WarehouseStock : 0;
                            reservedStock = (reservedStock > 0) ? reservedStock - newMrpItem.ReservedStock : 0;
                            postedItem.SurplusQuantity = (postedItem.SurplusQuantity > 0) ? postedItem.SurplusQuantity - newMrpItem.SurplusQuantity : 0;
                            if (neededAmount <= 0)
                                break;
                        }
                    }

                }
                return ServiceResultFactory.CreateSuccess(true);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        private async Task<ServiceResult<List<MrpItem>>> AddMrpItemAsync(Mrp mrpModel, List<AddMrpItemDto> model,
            List<MasterMR> masterMRs)
        {
            try
            {
                var result = new List<MrpItem>();
                foreach (var postedItem in model)
                {
                    var masterMrItem = masterMRs.FirstOrDefault(a => a.ProductId == postedItem.ProductId);
                    if (masterMrItem == null)
                        return ServiceResultFactory.CreateError<List<MrpItem>>(null,
                            MessageId.InputDataValidationError);

                    masterMrItem.RemainedGrossRequirement -= postedItem.GrossRequirement;

                    postedItem.PO = postedItem.AddPoModel != null ? postedItem.AddPoModel.Sum(a => a.OrderAmount) : 0;
                    var neededAmount = postedItem.GrossRequirement;
                    var wareHouseStock = postedItem.WarehouseStock;
                    var reservedStock = postedItem.ReservedStock;

                    foreach (var bom in masterMrItem.Product.BomProducts.Where(a => !a.IsDeleted && a.Remained > 0))
                    {

                        var newMrpItem = new MrpItem();

                        newMrpItem.ProductId = postedItem.ProductId;
                        newMrpItem.GrossRequirement = (bom.Remained >= neededAmount) ? neededAmount : bom.Remained;
                        newMrpItem.BomProduct = bom;
                        newMrpItem.WarehouseStock = (wareHouseStock <= newMrpItem.GrossRequirement) ? wareHouseStock : wareHouseStock - newMrpItem.GrossRequirement;
                        newMrpItem.ReservedStock = (reservedStock <= newMrpItem.GrossRequirement) ? reservedStock : reservedStock - newMrpItem.GrossRequirement;
                        newMrpItem.SurplusQuantity = postedItem.SurplusQuantity;
                        newMrpItem.NetRequirement = (newMrpItem.GrossRequirement + newMrpItem.ReservedStock) - newMrpItem.WarehouseStock;
                        newMrpItem.FinalRequirment = newMrpItem.NetRequirement + newMrpItem.SurplusQuantity;
                        newMrpItem.MrpItemStatus = MrpItemStatus.MRP;
                        newMrpItem.MasterMRId = masterMrItem.Id;
                        newMrpItem.PO = postedItem.PO;
                        newMrpItem.DateStart = postedItem.DateStart.UnixTimestampToDateTime().Date;
                        newMrpItem.DateEnd = postedItem.DateEnd.UnixTimestampToDateTime().Date;
                        newMrpItem.DoneStock = postedItem.PO;

                        newMrpItem.PR = newMrpItem.FinalRequirment - newMrpItem.PO;
                        newMrpItem.RemainedStock = postedItem.PR;
                        if (mrpModel.Id > 0)
                            newMrpItem.MrpId = mrpModel.Id;

                        result.Add(newMrpItem);
                        if (postedItem.AddPoModel != null && postedItem.AddPoModel.Count() > 0)
                        {
                            foreach (var item in postedItem.AddPoModel)
                            {
                                var prContractModel = await _prContractRepository
                                    .Where(a => a.Id == item.PRContractId)
                                    .Include(a => a.PRContractSubjects)
                                    .FirstOrDefaultAsync();
                                //.Include(a => a.TermsOfPayments)

                                if (prContractModel == null)
                                    return ServiceResultFactory.CreateError<List<MrpItem>>(null, MessageId.InputDataValidationError);

                                var res = _poService.AddPOToPendingByMRP(mrpModel, newMrpItem, postedItem.DateEnd.UnixTimestampToDateTime(), item, prContractModel);
                                if (!res.Succeeded)
                                    return ServiceResultFactory.CreateError<List<MrpItem>>(null,
                                        res.Messages.First().Message);

                                _poRepository.Add(res.Result);
                            }
                        }
                        bom.Remained -= newMrpItem.GrossRequirement;
                        neededAmount -= newMrpItem.GrossRequirement;
                        wareHouseStock = (wareHouseStock > 0) ? wareHouseStock - newMrpItem.WarehouseStock : 0;
                        reservedStock = (reservedStock > 0) ? reservedStock - newMrpItem.ReservedStock : 0;
                        postedItem.SurplusQuantity = (postedItem.SurplusQuantity > 0) ? postedItem.SurplusQuantity - newMrpItem.SurplusQuantity : 0;
                        if (neededAmount <= 0)
                            break;
                    }




                }

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<MrpItem>>(null, exception);
            }
        }

        private static int GetMrpItemsForMrp(List<MrpItem> mrpItems)
        {
            List<int> productIds = new List<int>();
            foreach (var item in mrpItems.Where(a => !a.IsDeleted))
            {
                if (!productIds.Contains(item.ProductId))
                    productIds.Add(item.ProductId);
            }
            return productIds.Count;
        }
        private static int GetMrpItemsForPurchaseRequest(List<MrpItem> mrpItems)
        {
            List<int> productIds = new List<int>();
            foreach (var item in mrpItems.Where(a => !a.IsDeleted && a.RemainedStock > 0))
            {
                if (!productIds.Contains(item.ProductId))
                    productIds.Add(item.ProductId);
            }
            return productIds.Count;
        }
        private static List<string> GetProductsForMrp(List<string> ProductTitle)
        {
            List<string> productTitles = new List<string>();
            foreach (var item in ProductTitle)
            {
                if (!productTitles.Contains(item))
                    productTitles.Add(item);
            }
            return productTitles;
        }
        private static List<string> GetProductsForPurchaseRequest(List<string> ProductTitle)
        {
            List<string> productTitles = new List<string>();
            foreach (var item in ProductTitle)
            {
                if (!productTitles.Contains(item))
                    productTitles.Add(item);
            }
            return productTitles.Take(10).ToList();
        }
    }
}