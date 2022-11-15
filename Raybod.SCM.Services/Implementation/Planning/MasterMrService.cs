using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Raybod.SCM.DataAccess.Core;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Model;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Raybod.SCM.DataTransferObject.Mrp;
using Raybod.SCM.DataTransferObject.MrpItem;
using Raybod.SCM.Utility.Extention;
using Raybod.SCM.DataTransferObject.Contract;
using Raybod.SCM.DataTransferObject.Bom;
using Microsoft.AspNetCore.Http;

namespace Raybod.SCM.Services.Implementation
{
    public class MasterMrService : IMasterMrService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly ITeamWorkAuthenticationService _authenticationService;
        private readonly DbSet<MasterMR> _masterMRRepository;
        private readonly DbSet<BomProduct> _bomRepository;
        private readonly DbSet<Contract> _contractRepository;
        private readonly CompanyConfig _appSettings;


        public MasterMrService(
            IUnitOfWork unitOfWork,
            IOptions<CompanyAppSettingsDto> appSettings,
            IHttpContextAccessor httpContextAccessor,
            ITeamWorkAuthenticationService authenticationService)
        {
            _unitOfWork = unitOfWork;
            _authenticationService = authenticationService;
            _bomRepository = _unitOfWork.Set<BomProduct>();
            _masterMRRepository = _unitOfWork.Set<MasterMR>();
            _contractRepository = _unitOfWork.Set<Contract>();
            httpContextAccessor.HttpContext.Request.Headers.TryGetValue("companyCode", out var CompanyCode);
            _appSettings = appSettings.Value.CompanyConfig.First(a => a.CompanyCode == CompanyCode);
        }

        
        
        private bool UpdateMasterMR(List<MasterMR> OldMasterMrs, List<MasterMR> newMasterMrs)
        {
            if (OldMasterMrs == null || !OldMasterMrs.Any())
            {
                _masterMRRepository.AddRange(newMasterMrs);
            }

            var newProductIds = newMasterMrs.Select(a => a.ProductId).ToList();
            var removeItem = OldMasterMrs
                .Where(a => !newProductIds.Contains(a.ProductId) && a.RemainedGrossRequirement == a.GrossRequirement)
                .ToList();

            foreach (var item in removeItem)
            {
                _masterMRRepository.Remove(item);
            }

            var oldProductIds = OldMasterMrs.Select(a => a.ProductId).ToList();
            var addNewMasterItems = newMasterMrs.Where(a => !oldProductIds.Contains(a.ProductId)).ToList();
            foreach (var item in addNewMasterItems)
            {
                _masterMRRepository.Add(item);
            }

            foreach (var item in newMasterMrs)
            {
                var selected = OldMasterMrs.Where(a => a.ProductId == item.ProductId).FirstOrDefault();
                if (selected == null)
                    continue;

                if (selected.GrossRequirement <= item.GrossRequirement)
                {
                    selected.RemainedGrossRequirement += item.GrossRequirement - selected.GrossRequirement;
                    selected.GrossRequirement = item.GrossRequirement;
                }
                else
                {
                    if (selected.RemainedGrossRequirement > 0)
                    {
                        selected.RemainedGrossRequirement -= selected.GrossRequirement - item.GrossRequirement;
                        selected.GrossRequirement = item.GrossRequirement;
                    }
                }
            }
            return true;
        }

        public async Task<int> DashbourdWaitingContractForMrpBadgeCountAsync(AuthenticateDto authenticate)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return 0;

                var dbQuery = _masterMRRepository
                    .Where(a => a.ContractCode == authenticate.ContractCode && a.RemainedGrossRequirement > 0);

                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.ProductGroupIds.Contains(a.Product.ProductGroupId));

                var count = await dbQuery
                    .Select(a => a.Product.ProductGroupId)
                    .Distinct()
                    .CountAsync();

                return 0;
            }
            catch (Exception exception)
            {
                return 0;
            }
        }

        public async Task<ServiceResult<int>> WaitingContractForMrpBadgeCountAsync(AuthenticateDto authenticate)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(0, MessageId.AccessDenied);

                var dbQuery = _masterMRRepository
                    .Where(a => a.ContractCode == authenticate.ContractCode && a.RemainedGrossRequirement > 0);

                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.ProductGroupIds.Contains(a.Product.ProductGroupId));

                var count = await dbQuery
                    .Select(a => a.Product.ProductGroupId)
                    .Distinct()
                    .CountAsync();

                return ServiceResultFactory.CreateSuccess(count);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(0, exception);
            }
        }

        public async Task<ServiceResult<List<WaitingContractFotMrpDto>>> WaitingMasterMrListGroupedByProductGroupIdAsync(AuthenticateDto authenticate)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<WaitingContractFotMrpDto>>(null, MessageId.AccessDenied);

                var dbQuery = _masterMRRepository
                    .Where(a => a.ContractCode == authenticate.ContractCode && a.RemainedGrossRequirement > 0);

                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.ProductGroupIds.Contains(a.Product.ProductGroupId));

                var result = await dbQuery.Select(x => new WaitingContractFotMrpDto
                {
                    ContractCode = x.ContractCode,
                    ContractNumber = x.Contract.ContractNumber,
                    Description = x.Contract.Description,
                    RemainedItem = 0,
                    ProductGroupId = x.Product.ProductGroupId,
                    ProductGroupCode = x.Product.ProductGroup.ProductGroupCode,
                    ProductGroupTitle = x.Product.ProductGroup.Title,
                    UserAudit = x.Contract.AdderUser != null ? new UserAuditLogDto
                    {
                        AdderUserId = x.Contract.AdderUserId,
                        CreateDate = x.Contract.CreatedDate.ToUnixTimestamp(),
                        AdderUserName = x.Contract.AdderUser.FullName,
                        AdderUserImage = x.Contract.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + x.Contract.AdderUser.Image : ""
                    } : null,
                }).ToListAsync();

                result = result.GroupBy(a => a.ProductGroupId)
                    .Select(c => new WaitingContractFotMrpDto
                    {
                        ContractCode = c.First().ContractCode,
                        ContractNumber = c.First().ContractNumber,
                        Description = c.First().Description,
                        RemainedItem = c.Count(),
                        ProductGroupId = c.First().ProductGroupId,
                        ProductGroupCode = c.First().ProductGroupCode,
                        ProductGroupTitle = c.First().ProductGroupTitle,
                        UserAudit = c.First().UserAudit,
                    }).ToList();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<WaitingContractFotMrpDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<WaitingContractFotMrpDto>> WaitingContractForMrpByContractCodeAsync(AuthenticateDto authenticate, int productGroupId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<WaitingContractFotMrpDto>(null, MessageId.AccessDenied);

                if (permission.ProductGroupIds.Any() && !permission.ProductGroupIds.Contains(productGroupId))
                    return ServiceResultFactory.CreateError<WaitingContractFotMrpDto>(null, MessageId.AccessDenied);

                var dbQuery = _masterMRRepository
                  .Where(a => a.ContractCode == authenticate.ContractCode && a.Product.ProductGroupId == productGroupId && a.RemainedGrossRequirement > 0);
                
                var result = await dbQuery.Select(x => new WaitingContractFotMrpDto
                {
                    ContractCode = x.ContractCode,
                    ContractNumber = x.Contract.ContractNumber,
                    Description = x.Contract.Description,
                    RemainedItem = 0,
                    ProductGroupId = x.Product.ProductGroupId,
                    ProductGroupCode = x.Product.ProductGroup.ProductGroupCode,
                    ProductGroupTitle = x.Product.ProductGroup.Title,
                    UserAudit = x.Contract.AdderUser != null ? new UserAuditLogDto
                    {
                        AdderUserId = x.Contract.AdderUserId,
                        CreateDate = x.Contract.CreatedDate.ToUnixTimestamp(),
                        AdderUserName = x.Contract.AdderUser.FullName,
                        AdderUserImage = x.Contract.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + x.Contract.AdderUser.Image : ""
                    } : null,
                }).FirstOrDefaultAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<WaitingContractFotMrpDto>(null, exception);
            }
        }

        public async Task<ServiceResult<List<MrpItemInfoDto>>> GetMasterMRBYProductGroupIdAsync(AuthenticateDto authenticate, int productGroupId, MasterMRQueryDto query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<MrpItemInfoDto>>(null, MessageId.AccessDenied);

                if (permission.ProductGroupIds.Any() && !permission.ProductGroupIds.Contains(productGroupId))
                    return ServiceResultFactory.CreateError<List<MrpItemInfoDto>>(null, MessageId.AccessDenied);

                var dbQuery = _masterMRRepository
                    .AsNoTracking()
                    .Where(a => a.ContractCode == authenticate.ContractCode && a.Product.ProductGroupId == productGroupId && a.RemainedGrossRequirement > 0);

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a => a.Product.Description.Contains(query.SearchText) ||
                                                 a.Product.ProductCode.Contains(query.SearchText));

                var totalCount = dbQuery.Count();

                var result = await dbQuery.Select(a => new MrpItemInfoDto
                {
                    ProductId = a.ProductId,
                    ProductCode = a.Product.ProductCode,
                    ProductDescription = a.Product.Description,
                    ProductTechnicalNumber = a.Product.TechnicalNumber,
                    ProductGroupName = a.Product.ProductGroup.Title,
                    Unit = a.Product.Unit,
                    GrossRequirement = a.RemainedGrossRequirement,
                    WarehouseStock = a.Product.WarehouseProducts.Sum(v => v.Inventory),
                    FreeQuantityInPO = a.Product.PRContractSubjects
                        .Where(c => c.PRContract.PRContractStatus == PRContractStatus.Active)
                        .Sum(c => c.RemainedStock),
                    DateStart = DateTime.UtcNow.ToUnixTimestamp(),
                    DateEnd=DateTime.UtcNow.ToUnixTimestamp()
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<MrpItemInfoDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<List<ExportMRPToExcelDto>>> GetMrpItemsByMrpIdForExportToExcelAsync(AuthenticateDto authenticate, int productGroupId, MasterMRQueryDto query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<ExportMRPToExcelDto>>(null, MessageId.AccessDenied);

                if (permission.ProductGroupIds.Any() && !permission.ProductGroupIds.Contains(productGroupId))
                    return ServiceResultFactory.CreateError<List<ExportMRPToExcelDto>>(null, MessageId.AccessDenied);

                var dbQuery = _masterMRRepository.Where(a => a.ContractCode == authenticate.ContractCode && a.Product.ProductGroupId == productGroupId && a.RemainedGrossRequirement > 0);

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a => a.Product.Description.Contains(query.SearchText) ||
                                                 a.Product.ProductCode.Contains(query.SearchText));

                var result = await dbQuery.Select(m => new ExportMRPToExcelDto
                {
                    ProductId = m.ProductId,
                    ProductCode = m.Product.ProductCode,
                    ProductDescription = m.Product.Description,
                    ProductTechnicalNumber = m.Product.TechnicalNumber,
                    ProductGroupName = m.Product.ProductGroup.Title,
                    Unit = m.Product.Unit,
                    GrossRequirement = m.RemainedGrossRequirement,
                    ReservedStock = 0,
                    WarehouseStock = m.Product.WarehouseProducts.Sum(v => v.Inventory),
                    FreeQuantityInPO = m.Product.PRContractSubjects
                        .Where(c => c.PRContract.PRContractStatus == PRContractStatus.Active)
                        .Sum(c => c.RemainedStock),
                    SurplusQuantity = 0,
                    DateStart = DateTime.Now.ToString("yyyy/MM/dd")
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<ExportMRPToExcelDto>>(null, exception);
            }
        }

        private async Task<ServiceResult<List<BomForMrpDto>>> GetBomForCreateMrpByProductIdAsync(int productId, decimal contractSubjectQuantity)
        {
            try
            {
                var bomModel = await _bomRepository
                    .AsNoTracking()
                    .Where(c => !c.IsDeleted && c.ProductId == productId && c.ParentBomId == null)
                    .Select(p => new BomForMrpDto
                    {
                        Id = p.Id,
                        ProductDescription = p.Product.Description,
                        ProductId = p.ProductId,
                        ProductTechnicalNumber = p.Product.TechnicalNumber,
                        ProductUnit = p.Product.Unit,
                        ProductCode = p.Product.ProductCode,
                        CoefficientUse = p.CoefficientUse,
                        GrossRequirement = contractSubjectQuantity,
                        ChildBoms = p.ChildBom.Where(x => !x.IsDeleted).Select(b => new BomForMrpDto
                        {
                            Id = b.Id,
                            ProductId = b.ProductId,
                            ProductDescription = b.Product.Description,
                            ProductUnit = b.Product.Unit,
                            ProductTechnicalNumber = b.Product.TechnicalNumber,
                            ProductCode = b.Product.ProductCode,
                            CoefficientUse = b.CoefficientUse,
                            GrossRequirement = contractSubjectQuantity * b.CoefficientUse,
                            ChildBoms = b.ChildBom.Where(c => !c.IsDeleted).Select(c => new BomForMrpDto
                            {
                                Id = c.Id,
                                ProductId = c.ProductId,
                                ProductDescription = c.Product.Description,
                                ProductUnit = c.Product.Unit,
                                ProductTechnicalNumber = c.Product.TechnicalNumber,
                                ProductCode = c.Product.ProductCode,
                                CoefficientUse = c.CoefficientUse,
                                GrossRequirement = contractSubjectQuantity * b.CoefficientUse * c.CoefficientUse,
                            }).ToList()
                        }).ToList(),
                    }).FirstOrDefaultAsync();

                if (bomModel == null)
                    return ServiceResultFactory.CreateError<List<BomForMrpDto>>(null, MessageId.EntityDoesNotExist);

                ///اگه زیرمجموعه نداشت خودشو برگردون
                if (bomModel.ChildBoms == null || bomModel.ChildBoms.Count() == 0)
                    return ServiceResultFactory.CreateSuccess(new List<BomForMrpDto> { bomModel });

                var result = new List<BomForMrpDto>();
                var LastChilds = bomModel.ChildBoms.Where(a => a.ChildBoms != null && a.ChildBoms.Count() > 0)
                    .SelectMany(a => a.ChildBoms)
                    .ToList();

                if (LastChilds != null && LastChilds.Any())
                    result.AddRange(LastChilds);

                var parentWithoutChilds = bomModel.ChildBoms
                    .Where(a => a.ChildBoms == null || a.ChildBoms.Count() == 0)
                    .ToList();

                if (parentWithoutChilds != null && parentWithoutChilds.Any())
                    result.AddRange(parentWithoutChilds);

                return ServiceResultFactory.CreateSuccess(result);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<BomForMrpDto>>(null, exception);
            }
        }

    }
}
