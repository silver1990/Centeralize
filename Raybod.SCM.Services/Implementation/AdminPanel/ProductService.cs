using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Raybod.SCM.DataAccess.Core;
using Raybod.SCM.DataAccess.Extention;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject._PanelDocument.Area;
using Raybod.SCM.DataTransferObject.Bom;
using Raybod.SCM.DataTransferObject.MasterMrpReport;
using Raybod.SCM.DataTransferObject.Mrp;
using Raybod.SCM.DataTransferObject.Product;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Model;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Utility.Common;
using Raybod.SCM.Utility.Extention;
using Raybod.SCM.Utility.FileHelper;
using Raybod.SCM.Utility.Helpers;
using System;
using System.Collections.Generic;
using Raybod.SCM.Utility.Utility;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Raybod.SCM.Services.Implementation
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITeamWorkAuthenticationService _authenticationServices;
        private readonly IContractFormConfigService _formConfigService;
        private readonly IMasterMrReportService _masterMrReportService;
        private readonly DbSet<Product> _productRepository;
        private readonly DbSet<ProductUnit> _productUnitRepository;
        private readonly DbSet<ProductGroup> _productGroupRepository;
        private readonly DbSet<Contract> _contractRepository;
        private readonly DbSet<Mrp> _mrpRepository;
        private readonly DbSet<MrpItem> _mrpItemRepository;
        private readonly DbSet<RFPItems> _rfpItemRepository;
        private readonly DbSet<POSubject> _poSubjectRepository;
        private readonly DbSet<PRContractSubject> _prContractSubjectRepository;
        private readonly DbSet<BomProduct> _bomProductRepository;
        private readonly DbSet<MasterMR> _masterMrRepository;
        private readonly DbSet<PurchaseRequestItem> _prItemRepository;
        private readonly DbSet<WarehouseProduct> _warehouseProductRepository;
        private readonly DbSet<Area> _areaRepository;
        private readonly Raybod.SCM.Services.Utilitys.FileHelper _fileHelper;
        private readonly CompanyConfig _appSettings;
        public ProductService(IUnitOfWork unitOfWork,
            IWebHostEnvironment hostingEnvironmentRoot,
            IOptions<CompanyAppSettingsDto> appSettings,
            IHttpContextAccessor httpContextAccessor,
            ITeamWorkAuthenticationService authenticationServices, IContractFormConfigService formConfigService, IMasterMrReportService masterMrReportService)
        {
            _unitOfWork = unitOfWork;
            _authenticationServices = authenticationServices;
            _fileHelper = new Utilitys.FileHelper(hostingEnvironmentRoot);
            _productRepository = _unitOfWork.Set<Product>();
            _productUnitRepository = _unitOfWork.Set<ProductUnit>();
            _productGroupRepository = _unitOfWork.Set<ProductGroup>();
            _contractRepository = _unitOfWork.Set<Contract>();
            _mrpRepository = _unitOfWork.Set<Mrp>();
            _mrpItemRepository = _unitOfWork.Set<MrpItem>();
            _rfpItemRepository = _unitOfWork.Set<RFPItems>();
            _poSubjectRepository = _unitOfWork.Set<POSubject>();
            _prContractSubjectRepository = _unitOfWork.Set<PRContractSubject>();
            _masterMrRepository = _unitOfWork.Set<MasterMR>();
            _bomProductRepository = _unitOfWork.Set<BomProduct>();
            _prItemRepository = _unitOfWork.Set<PurchaseRequestItem>();
            _warehouseProductRepository = _unitOfWork.Set<WarehouseProduct>();
            _areaRepository = _unitOfWork.Set<Area>();
            _formConfigService = formConfigService;
            _masterMrReportService = masterMrReportService;
            httpContextAccessor.HttpContext.Request.Headers.TryGetValue("companyCode", out var CompanyCode);
            _appSettings = appSettings.Value.CompanyConfig.First(a => a.CompanyCode == CompanyCode);
        }

        #region product unit
        public async Task<ServiceResult<List<string>>> GetProductUnitAsync(AuthenticateDto authenticate)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<string>>(null, MessageId.AccessDenied);

                var result = await _productUnitRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted)
                    .Select(a => a.Unit)
                    .ToListAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<string>>(null, exception);
            }
        }

        public async Task<ServiceResult<bool>> AddProductUnitAsync(AuthenticateDto authenticate, string unit)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                if (unit == null || unit.Length == 0 || unit.Length > 20)
                    return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                var result = await _productUnitRepository
                    .Where(a => !a.IsDeleted)
                    .Select(a => a.Unit)
                    .ToListAsync();

                if (result.Any(c => c == unit))
                    return ServiceResultFactory.CreateError(false, MessageId.Duplicate);

                var unitModel = new ProductUnit
                {
                    Unit = unit
                };

                _productUnitRepository.Add(unitModel);
                return await _unitOfWork.SaveChangesAsync() > 0
                    ? ServiceResultFactory.CreateSuccess(true)
                    : ServiceResultFactory.CreateError(false, MessageId.SaveFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<bool>> DeleteProductUnitAsync(AuthenticateDto authenticate, string unit)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var unitModel = await _productUnitRepository
                        .FirstOrDefaultAsync(a => !a.IsDeleted && a.Unit == unit);

                if (unitModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                //if (unitModel.CustomerId == null)
                //    return ServiceResultFactory.CreateError(false, MessageId.GlobalUnitNotDelete);

                unitModel.IsDeleted = true;

                return await _unitOfWork.SaveChangesAsync() > 0
                    ? ServiceResultFactory.CreateSuccess(true)
                    : ServiceResultFactory.CreateError(false, MessageId.SaveFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }

        }

        #endregion

        public async Task<ServiceResult<BaseProductDto>> AddProductAsync(AuthenticateDto authenticate, int productGroupId, AddProductDto model)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<BaseProductDto>(null, MessageId.AccessDenied);

                if (!EnumHelper.ValidateItem(model.ProductType))
                    return ServiceResultFactory.CreateError<BaseProductDto>(null, MessageId.InputDataValidationError);

                var productGroup = await _productGroupRepository
                    .Where(x => !x.IsDeleted && x.Id == productGroupId)
                    .FirstOrDefaultAsync();
                if (productGroup == null)
                    return ServiceResultFactory.CreateError<BaseProductDto>(null, MessageId.InputDataValidationError);

                //var productCount = _productRepository
                //    .Count(x => x.ProductGroupId == productGroupId);

                if (!await _productUnitRepository.AnyAsync(a => !a.IsDeleted && a.Unit == model.Unit))
                    return ServiceResultFactory.CreateError<BaseProductDto>(null, MessageId.InputDataValidationError);

                if (await _productRepository
                    .AnyAsync(a => !a.IsDeleted && a.ProductCode == model.ProductCode))
                    return ServiceResultFactory.CreateError<BaseProductDto>(null, MessageId.DuplicateInformation);

                //model.ProductCode = CodeGenerator.ProductCodeGenerator(productCount, productGroup.ProductGroupCode);

                var productModel = new Product
                {
                    Description = model.Description,

                    ProductCode = model.ProductCode,
                    Unit = model.Unit,
                    TechnicalNumber = model.TechnicalNumber,
                    ProductGroupId = productGroupId
                };

                _productRepository.Add(productModel);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var res = new BaseProductDto
                    {
                        Id = productModel.Id,
                        ProductType = model.ProductType,
                        Description = productModel.Description,
                        TechnicalNumber = productModel.TechnicalNumber,
                        ProductCode = productModel.ProductCode,
                        Unit = productModel.Unit,
                    };
                    return ServiceResultFactory.CreateSuccess(res);
                }

                return ServiceResultFactory.CreateError(new BaseProductDto(), MessageId.SaveFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new BaseProductDto(), exception);
            }
        }
        public async Task<ServiceResult<List<MasterMrProductListDto>>> AddProductWithBomAsync(AuthenticateDto authenticate, int productGroupId, List<AddProductWithBomDto> model)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<MasterMrProductListDto>>(null, MessageId.AccessDenied);

                //if (!EnumHelper.ValidateItem(model.ProductType))
                //    return ServiceResultFactory.CreateError<BaseProductDto>(null, MessageId.InputDataValidationError);

                var productGroup = await _productGroupRepository
                    .Where(x => !x.IsDeleted && x.Id == productGroupId)
                    .FirstOrDefaultAsync();
                if (productGroup == null)
                    return ServiceResultFactory.CreateError<List<MasterMrProductListDto>>(null, MessageId.InputDataValidationError);

                //var productCount = _productRepository
                //    .Count(x => x.ProductGroupId == productGroupId);
                var contract = await _contractRepository.FirstOrDefaultAsync(a => a.ContractCode == authenticate.ContractCode);
                if (contract == null)
                    return ServiceResultFactory.CreateError<List<MasterMrProductListDto>>(null, MessageId.InputDataValidationError);
                List<Product> productModel = new List<Product>();
                Mrp mrpModel = null;
                List<BomProduct> bomProducts = new List<BomProduct>();
                if (model.Any(a => a.MaterialType != MaterialType.Component && a.IsRequiredMRP == false))
                {
                    mrpModel = await _mrpRepository.Include(a=>a.MrpItems).FirstOrDefaultAsync(a => !a.IsDeleted&&a.ContractCode==authenticate.ContractCode && a.ProductGroupId == productGroupId && !a.MrpItems.Any(b => b.BomProduct.IsRequiredMRP));
                    if (mrpModel == null)
                    {
                        mrpModel = new Mrp
                        {
                            ProductGroupId = productGroupId,
                            ContractCode = contract.ContractCode,
                            Description = contract.Description,
                            MrpStatus = MrpStatus.Active,
                            MrpItems = new List<MrpItem>()
                        };
                        var count = await _mrpRepository.CountAsync(a => a.ContractCode == authenticate.ContractCode && a.MrpNumber.Contains("AutoMRP"));
                        mrpModel.MrpNumber = "AutoMRP" + count.ToString("00");
                    }
                    
                }
                List<ProductUnit> unitModel = new List<ProductUnit>();
                foreach (var item in model.Where(a => !a.IsRegisterd))
                {
                    if (!await _productUnitRepository.AnyAsync(a => !a.IsDeleted && a.Unit == item.Unit) && !unitModel.Any(a => a.Unit == item.Unit))
                    {
                        unitModel.Add(new ProductUnit
                        {
                            Unit = item.Unit
                        });
                        _productUnitRepository.AddRange(unitModel);
                    }
                    if (model.Where(a => a.ProductCode == item.ProductCode).Count() > 1)
                        return ServiceResultFactory.CreateError<List<MasterMrProductListDto>>(null, MessageId.DuplicateInformation);

                    if (await _productRepository
                        .AnyAsync(a => !a.IsDeleted && a.ProductCode == item.ProductCode))
                        return ServiceResultFactory.CreateError<List<MasterMrProductListDto>>(null, MessageId.DuplicateInformation);

                    //model.ProductCode = CodeGenerator.ProductCodeGenerator(productCount, productGroup.ProductGroupCode);
                    var area = (item.Area != null) ? await _areaRepository.FirstOrDefaultAsync(a => a.AreaTitle == item.Area.AreaTitle&&a.ContractCode==authenticate.ContractCode) : null;

                    productModel.Add(new Product
                    {
                        Description = item.Description,

                        ProductCode = item.ProductCode,
                        Unit = item.Unit,
                        ContractCode = authenticate.ContractCode,
                        TechnicalNumber = !String.IsNullOrEmpty(item.TechnicalNumber) ? item.TechnicalNumber : "",
                        ProductGroupId = productGroupId,
                        BomProducts = new List<BomProduct> { new BomProduct { Area = ((item.Area != null && !String.IsNullOrEmpty(item.Area.AreaTitle)) && area == null) ? new Area { AreaTitle = item.Area.AreaTitle, ContractCode = authenticate.ContractCode } : (item.Area != null && area != null) ? area : null, IsRequiredMRP = item.IsRequiredMRP, CoefficientUse = item.CoefficientUse, MaterialType = item.MaterialType, Remained = item.CoefficientUse } },
                        MasterMRs = (item.MaterialType != MaterialType.Component) ? new List<MasterMR> { new MasterMR { ContractCode = authenticate.ContractCode, GrossRequirement = item.CoefficientUse, RemainedGrossRequirement = item.CoefficientUse } } : null

                    });



                }

                var registerd = model.Where(a => a.IsRegisterd).Select(a => a.ProductCode).ToList();
                if (registerd != null && registerd.Any())
                {
                    var registerProducts = await _productRepository.Include(a => a.MasterMRs).Where(a => !a.IsDeleted && registerd.Contains(a.ProductCode)).ToListAsync();
                    foreach (var item in registerProducts)
                    {
                        var modelItem = model.First(a => a.ProductCode == item.ProductCode);
                        var area = (modelItem.Area != null) ? await _areaRepository.FirstOrDefaultAsync(a => a.AreaTitle == modelItem.Area.AreaTitle&&a.ContractCode==authenticate.ContractCode) : null;


                        if (item.BomProducts == null)
                        {
                            item.BomProducts = new List<BomProduct> { new BomProduct { Area = (area != null) ? area : null, IsRequiredMRP = modelItem.IsRequiredMRP, CoefficientUse = modelItem.CoefficientUse, MaterialType = modelItem.MaterialType, Remained = modelItem.CoefficientUse } };
                        }

                        else
                        {

                            item.BomProducts.Add(new BomProduct { Area = (area != null) ? area : null, IsRequiredMRP = modelItem.IsRequiredMRP, CoefficientUse = modelItem.CoefficientUse, MaterialType = modelItem.MaterialType, Remained = modelItem.CoefficientUse });
                        }
                        if (item.MasterMRs == null || !item.MasterMRs.Any())
                        {
                            item.MasterMRs = new List<MasterMR> { new MasterMR { ContractCode = authenticate.ContractCode, GrossRequirement = modelItem.CoefficientUse, RemainedGrossRequirement = modelItem.CoefficientUse } };
                        }

                        else
                        {

                            item.MasterMRs.First().GrossRequirement += modelItem.CoefficientUse;
                            item.MasterMRs.First().RemainedGrossRequirement += modelItem.CoefficientUse;
                        }
                    }
                }
                _productRepository.AddRange(productModel);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    if (model.Any(a => a.MaterialType != MaterialType.Component && a.IsRequiredMRP == false))
                    {
                        foreach (var item in productModel)
                            bomProducts.AddRange(item.BomProducts);
                        if (registerd != null && registerd.Any())
                        {
                            var registerProducts = await _productRepository.Include(a => a.MasterMRs).Where(a => !a.IsDeleted && registerd.Contains(a.ProductCode)).ToListAsync();
                            foreach (var item in registerProducts)
                            {
                                bomProducts.Add(item.BomProducts.OrderByDescending(a => a.CreatedDate).First());
                            }
                        }
                        await AddMrpAuto(bomProducts, mrpModel);
                    }

                    var result = await _masterMrReportService.GetMasterMrByContractCodeAsync(authenticate, new MasterMRQueryDto { Page = 1, PageSize = 9999 });
                    return ServiceResultFactory.CreateSuccess(result.Result);
                }

                return ServiceResultFactory.CreateError<List<MasterMrProductListDto>>(null, MessageId.SaveFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<MasterMrProductListDto>>(null, exception);
            }
        }
        public async Task<ServiceResult<List<MasterMrProductListDto>>> AddSubsetProductAsync(AuthenticateDto authenticate, int productId, List<AddProductSubsetDto> model)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<MasterMrProductListDto>>(null, MessageId.AccessDenied);

                //if (!EnumHelper.ValidateItem(model.ProductType))
                //    return ServiceResultFactory.CreateError<BaseProductDto>(null, MessageId.InputDataValidationError);

                var product = await _productRepository
                    .Include(a => a.BomProducts)
                    .ThenInclude(a => a.Area)
                    .Where(x => !x.IsDeleted && x.Id == productId)
                    .FirstOrDefaultAsync();
                if (product == null)
                    return ServiceResultFactory.CreateError<List<MasterMrProductListDto>>(null, MessageId.EntityDoesNotExist);

                //var productCount = _productRepository
                //    .Count(x => x.ProductGroupId == productGroupId);
                var contract = await _contractRepository.FirstOrDefaultAsync(a => a.ContractCode == authenticate.ContractCode);
                if (contract == null)
                    return ServiceResultFactory.CreateError<List<MasterMrProductListDto>>(null, MessageId.InputDataValidationError);
                List<Product> productModel = new List<Product>();
                Mrp mrpModel = null;
                List<BomProduct> bomProducts = new List<BomProduct>();
                var bom = product.BomProducts.FirstOrDefault(a => !a.IsDeleted && a.MaterialType == MaterialType.Component);
                if (bom == null)
                    return ServiceResultFactory.CreateError<List<MasterMrProductListDto>>(null, MessageId.EntityDoesNotExist);
                if (!bom.IsRequiredMRP)
                {
                    mrpModel = await _mrpRepository.Include(a => a.MrpItems).FirstOrDefaultAsync(a => !a.IsDeleted && a.ProductGroupId == product.ProductGroupId &&a.ContractCode==authenticate.ContractCode&& !a.MrpItems.Any(b => b.BomProduct.IsRequiredMRP));
                    if (mrpModel == null)
                    {
                        mrpModel = new Mrp
                        {
                            ProductGroupId = product.ProductGroupId,
                            ContractCode = contract.ContractCode,
                            Description = contract.Description,
                            MrpStatus = MrpStatus.Active,
                            MrpItems = new List<MrpItem>()
                        };
                        var count = await _mrpRepository.CountAsync(a => a.ContractCode == authenticate.ContractCode && a.MrpNumber.Contains("AutoMRP"));
                        mrpModel.MrpNumber = "AutoMRP" + count.ToString("00");
                    }
                    
                }
                List<ProductUnit> unitModel = new List<ProductUnit>();
                var area = (bom.Area != null) ? bom.Area : null;
                foreach (var item in model.Where(a => !a.IsRegisterd))
                {
                    if (model.Where(a => a.ProductCode == item.ProductCode).Count() > 1)
                        return ServiceResultFactory.CreateError<List<MasterMrProductListDto>>(null, MessageId.ImpossibleDuplicateProduct);
                    if (!await _productUnitRepository.AnyAsync(a => !a.IsDeleted && a.Unit == item.Unit) && !unitModel.Any(a => a.Unit == item.Unit))
                    {
                        unitModel.Add(new ProductUnit
                        {
                            Unit = item.Unit
                        });
                        _productUnitRepository.AddRange(unitModel);
                    }


                    if (await _productRepository.AnyAsync(a => !a.IsDeleted && a.ProductCode == item.ProductCode))
                        return ServiceResultFactory.CreateError<List<MasterMrProductListDto>>(null, MessageId.DuplicateInformation);

                    //model.ProductCode = CodeGenerator.ProductCodeGenerator(productCount, productGroup.ProductGroupCode);


                    productModel.Add(new Product
                    {
                        Description = item.Description,

                        ProductCode = item.ProductCode,
                        Unit = item.Unit,
                        ContractCode = authenticate.ContractCode,
                        TechnicalNumber = !String.IsNullOrEmpty(item.TechnicalNumber) ? item.TechnicalNumber : "",
                        ProductGroupId = product.ProductGroupId,
                        BomProducts = new List<BomProduct> { new BomProduct { Area = (area != null) ? area : null, IsRequiredMRP = bom.IsRequiredMRP, CoefficientUse = item.CoefficientUse*bom.CoefficientUse, MaterialType = MaterialType.RawMaterial, ParentBomId = bom.Id, Remained = item.CoefficientUse*bom.CoefficientUse } },
                        MasterMRs = new List<MasterMR> { new MasterMR { ContractCode = authenticate.ContractCode, GrossRequirement = (item.CoefficientUse * bom.CoefficientUse), RemainedGrossRequirement = (item.CoefficientUse * bom.CoefficientUse) } }

                    });



                }
                var registerd = model.Where(a => a.IsRegisterd).Select(a => a.ProductCode).ToList();
                if (registerd != null && registerd.Any())
                {
                    var registerProducts = await _productRepository.Include(a => a.MasterMRs).Where(a => !a.IsDeleted && registerd.Contains(a.ProductCode)).ToListAsync();
                    foreach (var item in registerProducts)
                    {


                        if (item.BomProducts == null)
                        {
                            item.BomProducts = new List<BomProduct> { new BomProduct { Area = (area != null) ? area : null, IsRequiredMRP = bom.IsRequiredMRP, CoefficientUse = model.First(a => a.ProductCode == item.ProductCode).CoefficientUse*bom.CoefficientUse, MaterialType = MaterialType.RawMaterial, ParentBomId = bom.Id, Remained = model.First(a => a.ProductCode == item.ProductCode).CoefficientUse*bom.CoefficientUse } };
                        }

                        else
                        {

                            item.BomProducts.Add(new BomProduct { Area = (area != null) ? area : null, IsRequiredMRP = bom.IsRequiredMRP, CoefficientUse = model.First(a => a.ProductCode == item.ProductCode).CoefficientUse*bom.CoefficientUse, MaterialType = MaterialType.RawMaterial, ParentBomId = bom.Id, Remained = model.First(a => a.ProductCode == item.ProductCode).CoefficientUse*bom.CoefficientUse });
                        }
                        if (item.MasterMRs == null || !item.MasterMRs.Any())
                        {
                            item.MasterMRs = new List<MasterMR> { new MasterMR { ContractCode = authenticate.ContractCode, GrossRequirement = (model.First(a => a.ProductCode == item.ProductCode).CoefficientUse * bom.CoefficientUse), RemainedGrossRequirement = (model.First(a => a.ProductCode == item.ProductCode).CoefficientUse * bom.CoefficientUse) } };
                        }

                        else
                        {

                            item.MasterMRs.First().GrossRequirement += (model.First(a => a.ProductCode == item.ProductCode).CoefficientUse * bom.CoefficientUse);
                            item.MasterMRs.First().RemainedGrossRequirement += (model.First(a => a.ProductCode == item.ProductCode).CoefficientUse * bom.CoefficientUse);
                        }
                    }
                }


                _productRepository.AddRange(productModel);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    if (!bom.IsRequiredMRP)
                    {
                        foreach (var item in productModel)
                            bomProducts.AddRange(item.BomProducts);
                        if (registerd != null && registerd.Any())
                        {
                            var registerProducts = await _productRepository.Include(a => a.MasterMRs).Where(a => !a.IsDeleted && registerd.Contains(a.ProductCode)).ToListAsync();
                            foreach (var item in registerProducts)
                            {
                                bomProducts.Add(item.BomProducts.OrderByDescending(a => a.CreatedDate).First());
                            }
                        }
                        await AddMrpAuto(bomProducts, mrpModel, 1);
                    }
                    //var res = new BaseProductDto
                    //{
                    //    Id = productModel.Id,
                    //    ProductType = model.ProductType,
                    //    Description = productModel.Description,
                    //    TechnicalNumber = productModel.TechnicalNumber,
                    //    ProductCode = productModel.ProductCode,
                    //    Unit = productModel.Unit,
                    //};
                    var result = await _masterMrReportService.GetMasterMrByContractCodeAsync(authenticate, new MasterMRQueryDto { Page = 1, PageSize = 99999 });
                    return ServiceResultFactory.CreateSuccess(result.Result);
                }

                return ServiceResultFactory.CreateError<List<MasterMrProductListDto>>(null, MessageId.SaveFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<MasterMrProductListDto>>(null, exception);
            }
        }
        public async Task<ServiceResult<List<BaseProductDto>>> AddProductAsync(AuthenticateDto authenticate, int productGroupId, List<AddProductDto> model)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<BaseProductDto>>(null, MessageId.AccessDenied);

                if (model == null || !model.Any() || model.Any(c => string.IsNullOrEmpty(c.ProductCode) || string.IsNullOrEmpty(c.Description)))
                    return ServiceResultFactory.CreateError<List<BaseProductDto>>(null, MessageId.InputDataValidationError);

                if (model.Any(c => !EnumHelper.ValidateItem(c.ProductType)))
                    return ServiceResultFactory.CreateError<List<BaseProductDto>>(null, MessageId.InputDataValidationError);

                var productGroup = await _productGroupRepository
                    .Where(x => !x.IsDeleted && x.Id == productGroupId)
                    .FirstOrDefaultAsync();
                if (productGroup == null)
                    return ServiceResultFactory.CreateError<List<BaseProductDto>>(null, MessageId.InputDataValidationError);

                //var productCount = _productRepository
                //    .Count(x => x.ProductGroupId == productGroupId);

                var units = await _productUnitRepository.Where(a => !a.IsDeleted).Select(c => c.Unit).ToListAsync();
                if (!units.Any())
                    return ServiceResultFactory.CreateError<List<BaseProductDto>>(null, MessageId.DataInconsistency);

                if (model.Any(c => !units.Contains(c.Unit)))
                    return ServiceResultFactory.CreateError<List<BaseProductDto>>(null, MessageId.DataInconsistency);

                var newProductCodes = model.Select(c => c.ProductCode).ToList();
                if (newProductCodes.GroupBy(v => v).Any(c => c.Count() > 1))
                    return ServiceResultFactory.CreateError<List<BaseProductDto>>(null, MessageId.DuplicateInformation);

                var serverCodes = await _productRepository
                    .Where(a => !a.IsDeleted && newProductCodes.Contains(a.ProductCode))
                    .Select(v => v.ProductCode)
                    .ToListAsync();

                if (serverCodes.Any() && serverCodes.Any(v => newProductCodes.Any(c => c == v)))
                    return ServiceResultFactory.CreateError<List<BaseProductDto>>(null, MessageId.DuplicateInformation);

                //model.ProductCode = CodeGenerator.ProductCodeGenerator(productCount, productGroup.ProductGroupCode);

                var productModels = model.Select(c => new Product
                {
                    Description = c.Description,

                    ProductCode = c.ProductCode,
                    Unit = c.Unit,
                    TechnicalNumber = c.TechnicalNumber,
                    ProductGroupId = productGroupId
                }).ToList();

                _productRepository.AddRange(productModels);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var res = productModels.Select(c => new BaseProductDto
                    {
                        Id = c.Id,
                        Description = c.Description,
                        TechnicalNumber = c.TechnicalNumber,
                        ProductCode = c.ProductCode,
                        Unit = c.Unit,
                    }).ToList();
                    return ServiceResultFactory.CreateSuccess(res);
                }

                return ServiceResultFactory.CreateError<List<BaseProductDto>>(null, MessageId.SaveFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<BaseProductDto>>(null, exception);
            }
        }


        public async Task<ServiceResult<bool>> EditProductAsync(AuthenticateDto authenticate, int productId, EditProductDto model)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);



                var selectedProduct = await _productRepository
                    .Where(x => x.Id == productId)
                    .Include(a=>a.BomProducts)
                    .ThenInclude(a=>a.MrpItems)
                    .ThenInclude(a=>a.PurchaseRequestItems)
                    .FirstOrDefaultAsync();
                if (selectedProduct == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (await _productRepository.AnyAsync(x => x.Id != productId && x.ProductCode == model.ProductCode))
                    return ServiceResultFactory.CreateError(false, MessageId.CodeExist);

                if (!await _productUnitRepository.AnyAsync(x => !x.IsDeleted && x.Unit == model.Unit))
                    return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                if (model.ProductGroupId != selectedProduct.ProductGroupId)
                {
                    if (!((selectedProduct.BomProducts.Any(a=>!a.IsDeleted&&!a.IsRequiredMRP&&a.MrpItems.Any(b=>!b.IsDeleted&&!b.PurchaseRequestItems.Any(c=>!c.IsDeleted)))&& !selectedProduct.BomProducts.Any(a => !a.IsDeleted && a.IsRequiredMRP && a.MrpItems.Any(b => !b.IsDeleted))) ||(selectedProduct.BomProducts.Any(a => !a.IsDeleted && a.IsRequiredMRP && !a.MrpItems.Any(b => !b.IsDeleted))&&!selectedProduct.BomProducts.Any(a => !a.IsDeleted && !a.IsRequiredMRP && a.MrpItems.Any(b => !b.IsDeleted && b.PurchaseRequestItems.Any(c => !c.IsDeleted))))))
                        return ServiceResultFactory.CreateError(false, MessageId.ProductGroupCantBeEdit);
                    var contract = await _contractRepository.FirstOrDefaultAsync(a => a.ContractCode == authenticate.ContractCode);
                    if (selectedProduct.BomProducts.Any(a => !a.IsDeleted && !a.IsRequiredMRP))
                    {
                        var mrp= await _mrpRepository.Include(a => a.MrpItems).FirstOrDefaultAsync(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.ProductGroupId == model.ProductGroupId && !a.MrpItems.Any(b => b.BomProduct.IsRequiredMRP));
                        if (mrp == null)
                        {
                            mrp = new Mrp
                            {
                                ProductGroupId = model.ProductGroupId,
                                ContractCode = authenticate.ContractCode,
                                Description = contract.Description,
                                MrpStatus = MrpStatus.Active,
                                MrpItems = new List<MrpItem>()
                            };
                            var count = await _mrpRepository.CountAsync(a => a.ContractCode == authenticate.ContractCode && a.MrpNumber.Contains("AutoMRP"));
                            mrp.MrpNumber = "AutoMRP" + count.ToString("00");
                            _mrpRepository.Add(mrp);
                        }
                         
                        foreach (var item in selectedProduct.BomProducts.Where(a => !a.IsDeleted))
                            foreach (var mrpItem in item.MrpItems)
                            {
                                mrp.MrpItems.Add(mrpItem);
                            }
                    }
                        
                }
                
                selectedProduct.Description = model.ProductDescription;
                selectedProduct.ProductCode = model.ProductCode;

                selectedProduct.TechnicalNumber = model.ProductTechnicalNumber;
                selectedProduct.Unit = model.Unit;
                selectedProduct.ProductGroupId = model.ProductGroupId;

                await _unitOfWork.SaveChangesAsync();
                return ServiceResultFactory.CreateSuccess(true);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<string>> GenerateProductCode(AuthenticateDto authenticate, int productGroupId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError("", MessageId.AccessDenied);

                var dbQuery = _productGroupRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.Id == productGroupId);

                var res = await dbQuery.Select(a => new { code = a.ProductGroupCode, pCount = a.Products.Count() })
                    .FirstOrDefaultAsync();

                if (res == null)
                    return ServiceResultFactory.CreateError("", MessageId.EntityDoesNotExist);

                //var result = CodeGenerator.ProductCodeGenerator(groupDetails.productCount, groupDetails.code);
                return ServiceResultFactory.CreateSuccess($"{res.code}-{res.pCount}");
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException("", exception);
            }
        }

        public async Task<ServiceResult<List<BaseProductDto>>> GetProductAsync(AuthenticateDto authenticate, long productGroupId, ProductQuery query)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<BaseProductDto>>(null, MessageId.AccessDenied);

                var dbQuery = _productRepository
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && x.ProductGroupId == productGroupId);

                if (!string.IsNullOrEmpty(query.SearchText))
                {
                    dbQuery = dbQuery
                        .Where(x =>
                        x.ProductCode.Contains(query.SearchText) ||
                        (x.Description != null && x.Description.ToLower().Contains(query.SearchText)) ||
                          (x.TechnicalNumber != null && x.TechnicalNumber.ToLower().Contains(query.SearchText)));
                }

                var columnsMap = new Dictionary<string, Expression<Func<Product, object>>>
                {
                    ["ProductId"] = v => v.Id,
                    ["Description"] = v => v.Description,
                    ["ProductCode"] = v => v.ProductCode,
                };
                var totalCount = dbQuery.Count();
                var list = await dbQuery.ApplayPageing(query).ApplayOrdering(query, columnsMap).Select(p => new BaseProductDto
                {
                    Id = p.Id,
                    ProductCode = p.ProductCode,
                    TechnicalNumber = p.TechnicalNumber,
                    Description = p.Description,

                    Unit = p.Unit,
                }).ToListAsync();


                return ServiceResultFactory.CreateSuccess(list).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new List<BaseProductDto>(), exception);
            }
        }

        public async Task<ServiceResult<List<ProductMiniInfo>>> GetProductMiniInfoAsync(AuthenticateDto authenticate, ProductQuery query)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<ProductMiniInfo>>(null, MessageId.AccessDenied);
                
                var dbQuery = _prContractSubjectRepository
                    .AsNoTracking()
                    .OrderByDescending(a => a.Id)
                    .Where(x => !x.IsDeleted&&x.PRContract.BaseContractCode==authenticate.ContractCode);

                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.ProductGroupIds.Contains(a.Product.ProductGroupId));

                //if (EnumHelper.ValidateItem(query.ProductType))
                //    dbQuery = dbQuery.Where(x => x.ProductType == query.ProductType);

                if (query.ProductGroupIds != null && query.ProductGroupIds.Count() > 0)
                    dbQuery = dbQuery.Where(x => query.ProductGroupIds.Contains(x.Product.ProductGroupId));

                if (!string.IsNullOrEmpty(query.SearchText))
                {
                    dbQuery = dbQuery
                        .Where(x =>
                        x.Product.ProductCode.Contains(query.SearchText) ||
                        (x.Product.Description != null && x.Product.Description.ToLower().Contains(query.SearchText)) ||
                        (x.Product.TechnicalNumber != null && x.Product.TechnicalNumber.ToLower().Contains(query.SearchText)));
                }

                var list = await dbQuery.Select(p => new ProductMiniInfo
                {
                    Id = p.Product.Id,
                    ProductCode = p.Product.ProductCode,
                    TechnicalNumber = p.Product.TechnicalNumber,
                    Description = p.Product.Description,
                    Unit = p.Product.Unit,
                }).ToListAsync();
                IEqualityComparer<ProductMiniInfo> customComparer = new ProductInfoEqualityCompare();
                list = list.Distinct(customComparer).ToList();
                return ServiceResultFactory.CreateSuccess(list);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new List<ProductMiniInfo>(), exception);
            }
        }

        public async Task<ServiceResult<List<ProductMiniInfo>>> GetProductMiniInfoWithoutLimitedPermisionGroupAsync(AuthenticateDto authenticate, ProductQuery query)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<ProductMiniInfo>>(null, MessageId.AccessDenied);

                var dbQuery = _productRepository
                    .AsNoTracking()
                    .OrderByDescending(a => a.Id)
                    .Where(x => !x.IsDeleted && x.ContractCode == authenticate.ContractCode);

                //if (EnumHelper.ValidateItem(query.ProductType))
                //    dbQuery = dbQuery.Where(x => x.ProductType == query.ProductType);

                if (query.ProductGroupIds != null && query.ProductGroupIds.Count() > 0)
                    dbQuery = dbQuery.Where(x => query.ProductGroupIds.Contains(x.ProductGroupId));

                if (!string.IsNullOrEmpty(query.SearchText))
                {
                    dbQuery = dbQuery
                        .Where(x =>
                        x.ProductCode.Contains(query.SearchText) ||
                        (x.Description != null && x.Description.ToLower().Contains(query.SearchText)) ||
                        (x.TechnicalNumber != null && x.TechnicalNumber.ToLower().Contains(query.SearchText)));
                }

                var list = await dbQuery.Select(p => new ProductMiniInfo
                {
                    Id = p.Id,
                    ProductCode = p.ProductCode,
                    TechnicalNumber = p.TechnicalNumber,
                    Description = p.Description,
                    Unit = p.Unit,
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(list);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new List<ProductMiniInfo>(), exception);
            }
        }

        public async Task<ServiceResult<List<ProductMiniInfo>>> GetProductMiniInfoAsync(AuthenticateDto authenticate, string query, List<int> productGroupIds)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<ProductMiniInfo>>(null, MessageId.AccessDenied);

                var dbQuery = _productRepository
                    .AsNoTracking()
                    .OrderByDescending(a => a.Id)
                    .Where(x => !x.IsDeleted);

                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.ProductGroupIds.Contains(a.ProductGroupId));

                if (productGroupIds != null && productGroupIds.Count() > 0)
                    dbQuery = dbQuery.Where(x => productGroupIds.Contains(x.ProductGroupId));

                if (!string.IsNullOrEmpty(query))
                {
                    dbQuery = dbQuery
                        .Where(x =>
                        x.ProductCode.Contains(query) ||
                        (x.Description != null && x.Description.ToLower().Contains(query)) ||
                        (x.TechnicalNumber != null && x.TechnicalNumber.ToLower().Contains(query)));
                }

                var list = await dbQuery.Select(p => new ProductMiniInfo
                {
                    Id = p.Id,
                    ProductCode = p.ProductCode,
                    TechnicalNumber = p.TechnicalNumber,
                    Description = p.Description,
                    Unit = p.Unit,
                }).Take(100).ToListAsync();

                return ServiceResultFactory.CreateSuccess(list);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new List<ProductMiniInfo>(), exception);
            }
        }

        public async Task<ServiceResult<List<ProductMiniInfo>>> GetProductMiniInfoWithPaginationAsync(AuthenticateDto authenticate, ProductQuery query)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<ProductMiniInfo>>(null, MessageId.AccessDenied);

                var dbQuery = _productRepository
                    .AsNoTracking()
                    .OrderByDescending(a => a.Id)
                    .Where(x => !x.IsDeleted);

                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.ProductGroupIds.Contains(a.ProductGroupId));

                //if (EnumHelper.ValidateItem(query.ProductType))
                //    dbQuery = dbQuery.Where(x => x.ProductType == query.ProductType);

                if (query.ProductGroupIds != null && query.ProductGroupIds.Count() > 0)
                    dbQuery = dbQuery.Where(x => query.ProductGroupIds.Contains(x.ProductGroupId));

                if (!string.IsNullOrEmpty(query.SearchText))
                {
                    dbQuery = dbQuery
                        .Where(x =>
                        x.ProductCode.Contains(query.SearchText) ||
                        (x.Description != null && x.Description.ToLower().Contains(query.SearchText)) ||
                        (x.TechnicalNumber != null && x.TechnicalNumber.ToLower().Contains(query.SearchText)));
                }

                dbQuery = dbQuery.ApplayPageing(query).AsQueryable();
                var list = await dbQuery.Select(p => new ProductMiniInfo
                {
                    Id = p.Id,
                    ProductCode = p.ProductCode,
                    TechnicalNumber = p.TechnicalNumber,
                    Description = p.Description,
                    Unit = p.Unit,
                }).Take(100).ToListAsync();

                return ServiceResultFactory.CreateSuccess(list);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new List<ProductMiniInfo>(), exception);
            }
        }

        public async Task<ServiceResult<BaseProductDto>> GetProductByIdAsync(AuthenticateDto authenticate, int productId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<BaseProductDto>(null, MessageId.AccessDenied);

                var result = await _productRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.Id == productId)
                    .Select(c => new BaseProductDto
                    {
                        Description = c.Description,
                        Id = c.Id,
                        ProductCode = c.ProductCode,
                        TechnicalNumber = c.TechnicalNumber,
                        Unit = c.Unit
                    }).FirstOrDefaultAsync();

                if (result == null)
                    return ServiceResultFactory.CreateError<BaseProductDto>(null, MessageId.EntityDoesNotExist);

                return ServiceResultFactory.CreateSuccess(result);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<BaseProductDto>(null, exception);
            }
        }

        public async Task<ServiceResult<bool>> RemoveProductAsync(AuthenticateDto authenticate, int productId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                if (await _productRepository
                    .AsNoTracking()
                    .AnyAsync(c => c.Id == productId && c.BomProducts.Any() ||
                    c.DocumentProducts.Any() ||
                    c.MasterBomProducts.Any() ||
                    c.MasterMRs.Any() ||
                    c.POSubjects.Any() ||
                    c.PurchaseRequestItems.Any() ||
                    c.PRContractSubjects.Any() ||
                    c.WarehouseProducts.Any()))
                    return ServiceResultFactory.CreateError(false, MessageId.DeleteDontAllowedBeforeSubset);

                var ProductGroupModel = await _productRepository
                    .Where(a => !a.IsDeleted && a.Id == productId)
                    .FirstOrDefaultAsync();

                if (ProductGroupModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                ProductGroupModel.IsDeleted = true;
                return await _unitOfWork.SaveChangesAsync() > 0
                    ? ServiceResultFactory.CreateSuccess(true)
                    : ServiceResultFactory.CreateError(false, MessageId.InternalError);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<bool>> AddProductImageAsync(AuthenticateDto authenticate, int ProductId, string imageName)
        {
            try
            {
                var product = await _productRepository.FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == ProductId);
                if (product == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);
                if (string.IsNullOrEmpty(imageName))
                    return ServiceResultFactory.CreateError(false, MessageId.ModelStateInvalid);

                var saveImage = _fileHelper.SaveImagesFromTemp(imageName, ServiceSetting.UploadImagesPath.ProductLarge, (int)ImageHelper.ImageWidth.FullHD);
                saveImage = _fileHelper.SaveImagesFromTemp(imageName, ServiceSetting.UploadImagesPath.ProductSmall, (int)ImageHelper.ImageWidth.Vcd);
                if (!string.IsNullOrWhiteSpace(saveImage))
                {
                    _fileHelper.DeleteImagesFromTemp(imageName);
                    product.Image = saveImage;
                }
                return await _unitOfWork.SaveChangesAsync() > 0
                    ? ServiceResultFactory.CreateSuccess(true)
                    : ServiceResultFactory.CreateError(false, MessageId.SaveFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }
        public async Task<ServiceResult<GetProductInfoForAddSubsetDto>> GetProductInfoAsync(AuthenticateDto authenticate, int productId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<GetProductInfoForAddSubsetDto>(null, MessageId.AccessDenied);

                var dbQuery = _productRepository
                    .Include(a => a.ProductGroup)
                    .Include(a => a.BomProducts)
                    .ThenInclude(a => a.Area)
                    .Where(a => !a.IsDeleted && a.Id == productId && a.ContractCode == authenticate.ContractCode);
                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.ProductGroupId)))
                    return ServiceResultFactory.CreateError<GetProductInfoForAddSubsetDto>(null, MessageId.AccessDenied);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<GetProductInfoForAddSubsetDto>(null, MessageId.EntityDoesNotExist);

                var product = await dbQuery.FirstOrDefaultAsync();
                var bom = product.BomProducts.FirstOrDefault(a => !a.IsDeleted && a.MaterialType == MaterialType.Component);
                if (bom == null)
                    return ServiceResultFactory.CreateError<GetProductInfoForAddSubsetDto>(null, MessageId.EntityDoesNotExist);

                var products = await _productRepository.Include(a => a.ProductGroup).Where(a => !a.IsDeleted && a.ProductGroupId == product.ProductGroupId && a.Id != product.Id && !a.BomProducts.Any(a => !a.IsDeleted && a.ProductId == product.Id && a.MaterialType == MaterialType.RawMaterial))
                    .Select(a => new ValidProductForAddingSubsetDto
                    {
                        Description = a.Description,
                        IsRegisterd = true,
                        ProductCode = a.ProductCode,
                        ProductGroupTitle = a.ProductGroup.Title,
                        ProductId = a.Id,
                        TechnicalNumber = a.TechnicalNumber,
                        Unit = a.Unit
                    }).ToListAsync();
                var result = await dbQuery.Select(a => new GetProductInfoForAddSubsetDto
                {
                    CoefficientUse = a.BomProducts.FirstOrDefault(a => !a.IsDeleted && a.MaterialType == MaterialType.Component).CoefficientUse,
                    Area = (bom.Area != null) ? new AreaReadDTO { AreaId = bom.Area.AreaId, AreaTitle = bom.Area.AreaTitle } : null,
                    Description = a.Description,
                    IsRequiredMRP = bom.IsRequiredMRP,
                    MaterialType = bom.MaterialType,
                    ProductCode = a.ProductCode,
                    ProductId = a.Id,
                    TechnicalNumber = a.TechnicalNumber,
                    Unit = a.Unit,
                    ProductGroupTitle = a.ProductGroup.Title,
                    Products = products

                }).FirstOrDefaultAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<GetProductInfoForAddSubsetDto>(null, exception);
            }
        }
        //public async Task<ServiceResult<GetDuplicateInfoDto>> GetDuplicateInfoAsync(AuthenticateDto authenticate, int productId)
        //{
        //    try
        //    {
        //        var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
        //        if (!permission.HasPermission)
        //            return ServiceResultFactory.CreateError<GetDuplicateInfoDto>(null, MessageId.AccessDenied);

        //        var dbQuery = _productRepository
        //            .Include(a => a.ProductGroup)
        //            .Include(a => a.BomProducts)
        //            .ThenInclude(a => a.Area)
        //            .Where(a => !a.IsDeleted && a.Id == productId && a.ContractCode == authenticate.ContractCode);
        //        if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.ProductGroupId)))
        //            return ServiceResultFactory.CreateError<GetDuplicateInfoDto>(null, MessageId.AccessDenied);

        //        if (dbQuery.Count() == 0)
        //            return ServiceResultFactory.CreateError<GetDuplicateInfoDto>(null, MessageId.EntityDoesNotExist);
        //        var product = await dbQuery.FirstOrDefaultAsync();
        //        var result = new GetDuplicateInfoDto
        //        {
        //            Description = product.Description,
        //            ProductCode = product.ProductCode,
        //            ProductGroupTitle = product.ProductGroup.Title,
        //            ProductId = product.Id,
        //            TechnicalNumber = product.TechnicalNumber,
        //            Unit = product.Unit,
        //            Options = (product.BomProducts.Any(a => !a.IsDeleted && a.MaterialType == MaterialType.Part)) ?
        //            new List<DuplicateOptionDto> { new DuplicateOptionDto { Label = "مجموعه", Value = MaterialType.Component } } :
        //            (product.BomProducts.Any(a => !a.IsDeleted && a.MaterialType == MaterialType.Component)) ?
        //            new List<DuplicateOptionDto> { new DuplicateOptionDto { Label = "تجهیز", Value = MaterialType.Part } } :
        //            (!product.BomProducts.Any(a => !a.IsDeleted && (a.MaterialType == MaterialType.Part || a.MaterialType == MaterialType.Component))) ?
        //            new List<DuplicateOptionDto> { new DuplicateOptionDto { Label = "تجهیز", Value = MaterialType.Part }, new DuplicateOptionDto { Label = "مجموعه", Value = MaterialType.Component } } :
        //            new List<DuplicateOptionDto>()
        //        };

        //        return ServiceResultFactory.CreateSuccess(result);
        //    }
        //    catch (Exception exception)
        //    {
        //        return ServiceResultFactory.CreateException<GetDuplicateInfoDto>(null, exception);
        //    }
        //}
        public async Task<ServiceResult<List<GetProductListInfoDto>>> GetProductListInfoAsync(AuthenticateDto authenticate, int productGroupId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<GetProductListInfoDto>>(null, MessageId.AccessDenied);

                var dbQuery = _productRepository
                    .Include(a => a.ProductGroup)
                    .Include(a => a.BomProducts)
                    .ThenInclude(a => a.Area)
                    .Where(a => !a.IsDeleted && a.ProductGroupId == productGroupId && a.ContractCode == authenticate.ContractCode );
                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.ProductGroupId)))
                    return ServiceResultFactory.CreateError<List<GetProductListInfoDto>>(null, MessageId.AccessDenied);

                var result = await dbQuery.Select(a => new GetProductListInfoDto
                {
                    Description = a.Description,
                    IsRegisterd = true,
                    IsSet = a.BomProducts.Any(b => !b.IsDeleted && b.MaterialType == MaterialType.Component),
                    ProductCode = a.ProductCode,
                    ProductGroupTitle = a.ProductGroup.Title,
                    ProductId = a.Id,
                    TechnicalNumber = a.TechnicalNumber,
                    Unit = a.Unit,
                    Inventory=a.WarehouseProducts.Sum(a=>a.Inventory),
                    IsEquipment= a.BomProducts.Any(b => !b.IsDeleted && b.MaterialType == MaterialType.Part)
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<GetProductListInfoDto>>(null, exception);
            }
        }
       
        //public async Task<ServiceResult<List<MasterMrProductListDto>>> AddDuplicateAsync(AuthenticateDto authenticate, int productId, CreateDuplicateDto model)
        //{
        //    try
        //    {
        //        var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
        //        if (!permission.HasPermission)
        //            return ServiceResultFactory.CreateError<List<MasterMrProductListDto>>(null, MessageId.AccessDenied);



        //        var product = await _productRepository
        //            .Include(a => a.ProductGroup)
        //            .Include(a => a.BomProducts)
        //            .Include(a => a.MasterMRs)
        //            .Where(a => !a.IsDeleted && a.Id == productId).FirstOrDefaultAsync();


        //        if (permission.ProductGroupIds.Any() && !permission.ProductGroupIds.Contains(product.ProductGroupId))
        //            return ServiceResultFactory.CreateError<List<MasterMrProductListDto>>(null, MessageId.AccessDenied);
        //        if ((product.BomProducts.Any(a => !a.IsDeleted && a.MaterialType == MaterialType.Component)) && (product.BomProducts.Any(a => !a.IsDeleted && a.MaterialType == MaterialType.Part)))
        //            return ServiceResultFactory.CreateError<List<MasterMrProductListDto>>(null, MessageId.HasBothTypeBom);
        //        if ((model.MaterialType == MaterialType.Component) && product.BomProducts.Any(a => !a.IsDeleted && a.MaterialType == MaterialType.Component))
        //            return ServiceResultFactory.CreateError<List<MasterMrProductListDto>>(null, MessageId.HasComponentTypeBom);
        //        if ((model.MaterialType == MaterialType.Part) && product.BomProducts.Any(a => !a.IsDeleted && a.MaterialType == MaterialType.Part))
        //            return ServiceResultFactory.CreateError<List<MasterMrProductListDto>>(null, MessageId.HasPartTypeBom);
        //        //var productCount = _productRepository
        //        //    .Count(x => x.ProductGroupId == productGroupId);
        //        var contract = await _contractRepository.FirstOrDefaultAsync(a => a.ContractCode == authenticate.ContractCode);
        //        if (contract == null)
        //            return ServiceResultFactory.CreateError<List<MasterMrProductListDto>>(null, MessageId.InputDataValidationError);

        //        Mrp mrpModel = null;
        //        if (model.MaterialType != MaterialType.Component && model.IsRequiredMRP == false)
        //        {

        //            mrpModel = new Mrp
        //            {
        //                ProductGroupId = product.ProductGroupId,
        //                ContractCode = contract.ContractCode,
        //                Description = contract.Description,
        //                MrpStatus = MrpStatus.Active,
        //                MrpItems = new List<MrpItem>()
        //            };
        //            var count = await _mrpRepository.CountAsync(a => a.ContractCode == authenticate.ContractCode);
        //            var codeRes = await _formConfigService.GenerateFormCodeAsync(authenticate.ContractCode, FormName.MRP, count);
        //            if (!codeRes.Succeeded)
        //                return ServiceResultFactory.CreateError<List<MasterMrProductListDto>>(null, codeRes.Messages.First().Message);
        //            mrpModel.MrpNumber = codeRes.Result;
        //        }

        //        var area = (model.Area != null) ? await _areaRepository.FirstOrDefaultAsync(a => a.AreaTitle == model.Area.AreaTitle) : null;
        //        List<BomProduct> bomProducts = new List<BomProduct>();
        //        if (product.BomProducts == null || !product.BomProducts.Any())
        //            product.BomProducts = new List<BomProduct> { new BomProduct { Area = ((model.Area != null && !String.IsNullOrEmpty(model.Area.AreaTitle)) && area == null) ? new Area { AreaTitle = model.Area.AreaTitle, ContractCode = authenticate.ContractCode } : (model.Area != null && area != null) ? area : null, IsRequiredMRP = model.IsRequiredMRP, CoefficientUse = model.CoefficientUse, MaterialType = model.MaterialType } };
        //        else
        //            product.BomProducts.Add(new BomProduct { Area = ((model.Area != null && !String.IsNullOrEmpty(model.Area.AreaTitle)) && area == null) ? new Area { AreaTitle = model.Area.AreaTitle, ContractCode = authenticate.ContractCode } : (model.Area != null && area != null) ? area : null, IsRequiredMRP = model.IsRequiredMRP, CoefficientUse = model.CoefficientUse, MaterialType = model.MaterialType });
        //        bomProducts.Add(new BomProduct { Area = ((model.Area != null && !String.IsNullOrEmpty(model.Area.AreaTitle)) && area == null) ? new Area { AreaTitle = model.Area.AreaTitle, ContractCode = authenticate.ContractCode } : (model.Area != null && area != null) ? area : null, IsRequiredMRP = model.IsRequiredMRP, CoefficientUse = model.CoefficientUse, MaterialType = model.MaterialType ,ProductId=product.Id});
        //        if (model.MaterialType == MaterialType.Part)
        //        {
        //            if (product.MasterMRs == null || !product.MasterMRs.Any())
        //                product.MasterMRs = new List<MasterMR> { new MasterMR { ContractCode = authenticate.ContractCode, GrossRequirement = model.CoefficientUse, RemainedGrossRequirement = model.CoefficientUse } };
        //            if (product.MasterMRs != null && product.MasterMRs.Any())
        //            {
        //                product.MasterMRs.First().GrossRequirement = product.MasterMRs.First().GrossRequirement + model.CoefficientUse;
        //                product.MasterMRs.First().RemainedGrossRequirement = product.MasterMRs.First().RemainedGrossRequirement + model.CoefficientUse;
        //            }

        //        }

        //        if (await _unitOfWork.SaveChangesAsync() > 0)
        //        {
        //            if (model.MaterialType == MaterialType.Part && model.IsRequiredMRP == false)
        //            {
        //                await AddMrpAuto(bomProducts, mrpModel);
        //            }

        //            var result = await _masterMrReportService.GetMasterMrByContractCodeAsync(authenticate, new MasterMRQueryDto { Page = 1, PageSize = 9999 });
        //            return ServiceResultFactory.CreateSuccess(result.Result);
        //        }

        //        return ServiceResultFactory.CreateError<List<MasterMrProductListDto>>(null, MessageId.SaveFailed);

        //    }
        //    catch (Exception exception)
        //    {
        //        return ServiceResultFactory.CreateException<List<MasterMrProductListDto>>(null, exception);
        //    }
        //}

        public async Task<ServiceResult<List<EditBomProductInfoDto>>> GetBomProductByProductIdAsync(AuthenticateDto authenticate, int productId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<EditBomProductInfoDto>>(null, MessageId.AccessDenied);

                var dbQuery = _bomProductRepository
                    .Include(a => a.Area)
                    .Include(a => a.ParentBom)
                    .ThenInclude(a => a.Product)
                    .Include(a=>a.MrpItems)
                    .ThenInclude(a=>a.PurchaseRequestItems)
                    .Where(a => !a.IsDeleted && a.ProductId == productId && a.Product.ContractCode == authenticate.ContractCode && a.MaterialType != MaterialType.Component);
                //if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.ProductGroupId)))
                //    return ServiceResultFactory.CreateError<GetDuplicateInfoDto>(null, MessageId.AccessDenied);

                //if (dbQuery.Count() == 0)
                //    return ServiceResultFactory.CreateError<GetDuplicateInfoDto>(null, MessageId.EntityDoesNotExist);
                //var product = await dbQuery.FirstOrDefaultAsync();
                var result = await dbQuery.Select(a => new EditBomProductInfoDto
                {
                    Area = (a.Area != null) ? new AreaReadDTO { AreaId = a.Area.AreaId, AreaTitle = a.Area.AreaTitle } : null,
                    BomProductId = a.Id,
                    Quantity = a.CoefficientUse,
                    IsRequiredMrp=a.IsRequiredMRP,
                    Remained=(!a.IsRequiredMRP)?a.MrpItems.Where(a=>!a.IsDeleted).Sum(a=>a.RemainedStock):a.Remained,
                    UserAudit = new UserAuditLogDto
                    {
                        AdderUserId = a.ModifierUserId,
                        AdderUserName = a.ModifierUser.FullName,
                        CreateDate = a.UpdateDate.ToUnixTimestamp(),
                        AdderUserImage = !String.IsNullOrEmpty(a.ModifierUser.Image) ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + a.ModifierUser.Image : "",
                    },
                    BomReference = (a.MaterialType == MaterialType.RawMaterial) ? "زیر مجموعه - "+a.ParentBom.Product.Description : "تجهیز"
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<EditBomProductInfoDto>>(null, exception);
            }
        }
        public async Task<ServiceResult<BomProductArchiveDto>> GetBomProductArchiveByBomIdAsync(AuthenticateDto authenticate, long bomProductId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<BomProductArchiveDto>(null, MessageId.AccessDenied);

                var dbQuery = _bomProductRepository
                    .Where(a => !a.IsDeleted && a.Id == bomProductId&&a.Product.ContractCode==authenticate.ContractCode);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.Product.ProductGroupId)))
                    return ServiceResultFactory.CreateError<BomProductArchiveDto>(null, MessageId.AccessDenied);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<BomProductArchiveDto>(null, MessageId.EntityDoesNotExist);

                var bomProduct = await dbQuery.FirstOrDefaultAsync();
                BomProductArchiveDto result = new BomProductArchiveDto();
                if (bomProduct.IsRequiredMRP)
                {
                    var mrps = _mrpItemRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted &&
                    a.BomProductId == bomProductId &&
                    !a.Mrp.IsDeleted &&
                    a.Mrp.ContractCode == authenticate.ContractCode);
                    var mrpList = await mrps.Select(m => new MrpReportListDto
                    {
                        MRPItemId = m.Id,
                        MRPId = m.MrpId,
                        CreateDate = m.Mrp.CreatedDate.ToUnixTimestamp(),
                        DateEnd = m.DateEnd.ToUnixTimestamp(),
                        DateStart = m.DateStart.ToUnixTimestamp(),
                        GrossRequirement = m.GrossRequirement,
                        PO = m.PO,
                        ReservedStock = m.ReservedStock,
                        WarehouseStock = m.WarehouseStock,
                        SurplusQuantity = m.SurplusQuantity,
                        MrpNumber = m.Mrp.MrpNumber,

                    }).ToListAsync();
                    result.Mrps =(mrpList!=null&&mrpList.Any())? MergeMrpsForArchive(mrpList):null;
                }

                var purchaseRequests = _prItemRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted &&
                    a.MrpItem.BomProductId == bomProductId &&
                    !a.PurchaseRequest.IsDeleted &&a.PurchaseRequest.PRStatus!=PRStatus.Reject&&
                    a.PurchaseRequest.ContractCode == authenticate.ContractCode);
                var purchaseRequestList = await purchaseRequests.Select(a => new PRReportListDto
                {
                    CreateDate = a.PurchaseRequest.CreatedDate.ToUnixTimestamp(),
                    MRPNumber =(a.MrpItem.BomProduct.IsRequiredMRP)? a.PurchaseRequest.Mrp.MrpNumber:"",
                    PRCode = a.PurchaseRequest.PRCode,
                    PRId = a.PurchaseRequestId,
                    PRItemId = a.Id,
                    Quntity = a.Quntity
                }).ToListAsync();
                
                result.PurchaseRequests = (purchaseRequestList!=null&&purchaseRequestList.Any())?MergePurchaseRequestsForArchive(purchaseRequestList):null;

                var rfps = _rfpItemRepository
                   .AsNoTracking()
                   .Where(a => !a.IsDeleted &&
                   a.IsActive &&
                   a.PurchaseRequestItem.MrpItem.BomProductId == bomProductId &&
                   !a.RFP.IsDeleted &&a.RFP.Status!=RFPStatus.Canceled&&
                   a.RFP.ContractCode == authenticate.ContractCode);

                var rfpList = await rfps.Select(a => new RFPReportListDto
                {
                    RFPId = a.RFPId,
                    RFPItemId = a.Id,
                    RFPStatus = a.RFP.Status,
                    CreateDate = a.RFP.CreatedDate.ToUnixTimestamp(),
                    RFPNumber = a.RFP.RFPNumber,
                    PRCode = a.PurchaseRequestItem.PurchaseRequest.PRCode,
                    Quntity = a.Quantity,
                }).ToListAsync();

                result.RFPs = (rfpList != null && rfpList.Any()) ? MergeRFPsForArchive(rfpList) : null;

                var prContracts = _prContractSubjectRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted &&
                    a.PRContract.PRContractStatus!=PRContractStatus.Canceled&&
                    a.RFPItem.PurchaseRequestItem.MrpItem.BomProductId == bomProductId &&
                    a.PRContract.BaseContractCode == authenticate.ContractCode);

                var prContractList = await prContracts.Select(a => new PRCReportListDto
                {
                    PRCSubjectId = a.Id,
                    DateEnd = a.PRContract.DateEnd.ToUnixTimestamp(),
                    DateIssued = a.PRContract.DateIssued.ToUnixTimestamp(),
                    CreateDate = a.PRContract.CreatedDate.ToUnixTimestamp(),
                    RFPNumber = a.RFPItem.RFP.RFPNumber,
                    PRContractCode = a.PRContract.PRContractCode,
                    PRContractId = a.PRContractId,
                    Quntity = a.Quantity,
                    RemainedQuantity=a.RemainedStock,
                    OrderQuantity = a.Product.POSubjects.Where(c =>!c.PO.IsDeleted&& c.PO.POStatus != POStatus.Pending && c.PO.PRContractId == a.PRContractId).Sum(v => v.Quantity),
                    ReceiptQuantity = a.Product.POSubjects.Where(c => !c.PO.IsDeleted && c.PO.POStatus != POStatus.Pending && c.PO.PRContractId == a.PRContractId).Sum(v => v.ReceiptedQuantity),
                }).ToListAsync();

                result.PrContracts = (prContractList != null && prContractList.Any()) ? MergePrContractsForArchive(prContractList) : null;

                var pos = _poSubjectRepository
                   .AsNoTracking()
                   .Where(a => !a.PO.IsDeleted &&a.PO.POStatus!=POStatus.Canceled&&
                   a.MrpItem.BomProductId == bomProductId &&
                   a.PO.POStatus != POStatus.Pending &&
                   a.PO.BaseContractCode == authenticate.ContractCode);


                var poList = await pos.Select(a => new POReportListDto
                {
                    POId = a.POId.Value,
                    DateDelivery = a.PO.DateDelivery.ToUnixTimestamp(),
                    MrpNumber =(a.MrpItem.BomProduct.IsRequiredMRP)? a.MrpItem.Mrp.MrpNumber:"",
                    POCode = a.PO.POCode,
                    POSubjectId = a.POSubjectId,
                    PRContractCode = a.PO.PRContract.PRContractCode,
                    Quantity = a.Quantity,
                    ReceiptedQuantity = a.ReceiptedQuantity,
                    RemainedQuantity = a.RemainedQuantity,
                    CreateDate = a.PO.CreatedDate.ToUnixTimestamp(),
                }).ToListAsync();
                result.Pos = (poList!=null&&poList.Any())?MergePosForArchive(poList):null;

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<BomProductArchiveDto>(null, exception);
            }
        }
        private async Task AddMrpAuto(List<BomProduct> model, Mrp mrp, decimal CoefficientUse = 1)
        {
            foreach (var item in model)
            {
                if (item.MaterialType != MaterialType.Component && !item.IsRequiredMRP)
                {
                    item.Remained = 0;
                    var products = await _productRepository.Include(a => a.MasterMRs).FirstAsync(a => a.Id == item.ProductId);
                    products.MasterMRs.First().RemainedGrossRequirement -= (item.CoefficientUse * CoefficientUse);
                    mrp.MrpItems.Add(new MrpItem { DateEnd = DateTime.Now.AddDays(7), DateStart = DateTime.Now, DoneStock = 0, FinalRequirment = item.CoefficientUse, GrossRequirement = item.CoefficientUse, MasterMRId = products.MasterMRs.First().Id, MrpItemStatus = MrpItemStatus.MRP, NetRequirement = item.CoefficientUse, PO = 0, PR = 0, RemainedStock = item.CoefficientUse, ReservedStock = 0, SurplusQuantity = 0, WarehouseStock = 0, ProductId = item.ProductId, BomProductId = item.Id });

                }
            }
            if (mrp.Id > 0)
            {
                await _unitOfWork.SaveChangesAsync();
            }
            else
            {
                await _mrpRepository.AddAsync(mrp);
                await _unitOfWork.SaveChangesAsync();
            }
            
            
        }

        public async Task<ServiceResult<BomProductEditResultDto>> EditBomProductAsync(AuthenticateDto authenticate, long bomId, CreateBomFormAnotherBomDto model)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<BomProductEditResultDto>(null, MessageId.AccessDenied);

                var dbQuery = _bomProductRepository
                    .Include(a => a.Area)
                    .Include(a=>a.ModifierUser)
                    .Include(a => a.ParentBom)
                    .ThenInclude(a=>a.Product)
                    .Include(a => a.Product)
                    .ThenInclude(a=>a.MasterMRs)
                    .Include(a => a.MrpItems)
                    .ThenInclude(a => a.PurchaseRequestItems)
                    .Where(a => !a.IsDeleted && a.Id == bomId && a.Product.ContractCode == authenticate.ContractCode && a.MaterialType != MaterialType.Component);

                if (await dbQuery.CountAsync() == 0)
                    return ServiceResultFactory.CreateError<BomProductEditResultDto>(null, MessageId.EntityDoesNotExist);

                var bom = await dbQuery.FirstAsync();
                
                if(model.Quantity<bom.CoefficientUse-bom.Remained&&bom.IsRequiredMRP)
                    return ServiceResultFactory.CreateError<BomProductEditResultDto>(null, MessageId.QuantityGreaterThanRemaind);
                if(!bom.IsRequiredMRP&&bom.MrpItems.Where(a=>!a.IsDeleted).Sum(a=>a.DoneStock)>model.Quantity)
                    return ServiceResultFactory.CreateError<BomProductEditResultDto>(null, MessageId.QuantityGreaterThanDoneStock);
                if (model.Quantity<=0)
                    return ServiceResultFactory.CreateError<BomProductEditResultDto>(null, MessageId.InputDataValidationError);
                var diffQuantity = -(bom.CoefficientUse - model.Quantity);
               var bomPlanned= bom.MrpItems.Where(a => !a.IsDeleted).Sum(a => a.GrossRequirement);
               
                if (!bom.IsRequiredMRP)
                {
                    if (model.Quantity == 0)
                    {
                        bom.Remained = 0;
                        bom.MrpItems.First(a => !a.IsDeleted).GrossRequirement =0;
                        bom.MrpItems.First(a => !a.IsDeleted).FinalRequirment =0;
                        bom.MrpItems.First(a => !a.IsDeleted).NetRequirement =0;
                        bom.MrpItems.First(a => !a.IsDeleted).RemainedStock =0;
                    }
                    else
                    {
                        bom.Remained = 0;
                        bom.MrpItems.First(a => !a.IsDeleted).GrossRequirement += diffQuantity;
                        bom.MrpItems.First(a => !a.IsDeleted).FinalRequirment += diffQuantity;
                        bom.MrpItems.First(a => !a.IsDeleted).NetRequirement += diffQuantity;
                        bom.MrpItems.First(a => !a.IsDeleted).RemainedStock += diffQuantity;
                    }
                    

                }
                var planned = bom.MrpItems.Where(a => !a.IsDeleted).Sum(a => a.GrossRequirement);
                var masterMrPlanned = bom.Product.MasterMRs.First().GrossRequirement - bom.Product.MasterMRs.First().RemainedGrossRequirement;
                bom.Product.MasterMRs.First().GrossRequirement -= bom.CoefficientUse;
                bom.CoefficientUse = model.Quantity;
                bom.Remained = bom.CoefficientUse - planned;
                bom.Product.MasterMRs.First().GrossRequirement += bom.CoefficientUse;
                bom.Product.MasterMRs.First().RemainedGrossRequirement = bom.Product.MasterMRs.First().GrossRequirement-((masterMrPlanned - bomPlanned) + planned);
                bom.AreaId = (model.Area != null) ? model.Area.AreaId : null;
                
                if(await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var masterMr = await _masterMrReportService.GetMasterMrDetailByProductIdAsync(authenticate,bom.ProductId);
                    var result = new BomProductEditResultDto
                    {
                        BomProduct = new EditBomProductInfoDto
                        {
                            Area =(bom.AreaId!=null)? model.Area:null,
                            BomProductId = bom.Id,
                            BomReference = (bom.MaterialType == MaterialType.RawMaterial) ? "زیر مجموعه - " + bom.ParentBom.Product.Description : "تجهیز",
                            IsRequiredMrp = bom.IsRequiredMRP,
                            Quantity = bom.CoefficientUse,
                            Remained = (!bom.IsRequiredMRP) ? bom.MrpItems.Where(a => !a.IsDeleted).Sum(a => a.RemainedStock) : bom.Remained,
                            UserAudit = new UserAuditLogDto
                            {
                                AdderUserId = authenticate.UserId,
                                AdderUserName = authenticate.UserFullName,
                                CreateDate = bom.UpdateDate.ToUnixTimestamp(),
                                AdderUserImage = !String.IsNullOrEmpty(authenticate.UserImage) ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + authenticate.UserImage : "",
                            },

                        },
                        MasterMr = masterMr.Result
                    };
                    return ServiceResultFactory.CreateSuccess(result);
                }
                return ServiceResultFactory.CreateError<BomProductEditResultDto>(null, MessageId.SaveFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<BomProductEditResultDto>(null, exception);
            }
        }


        //public async Task<ServiceResult<bool>> AddBomProductsAsync(AuthenticateDto authenticate, long bomId, CreateBomFormAnotherBomDto model)
        //{
        //    try
        //    {
        //        var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
        //        if (!permission.HasPermission)
        //            return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

        //        var dbQuery = _bomProductRepository
        //            .Include(a => a.Area)
        //            .Include(a => a.ParentBom)
        //            .ThenInclude(a => a.Product)
        //            .Include(a=>a.MrpItems)
        //            .ThenInclude(a=>a.PurchaseRequestItems)
        //            .Where(a => !a.IsDeleted && a.Id == bomId && a.Product.ContractCode == authenticate.ContractCode && a.MaterialType != MaterialType.Component);

        //        if (await dbQuery.CountAsync() == 0)
        //            return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

        //        var bom = await dbQuery.FirstAsync();
        //        if((bom.MrpItems==null||!bom.MrpItems.Any(a=>!a.IsDeleted))||(!bom.IsRequiredMRP&&!bom.MrpItems.Any(a=>!a.IsDeleted&&a.PurchaseRequestItems.Any(b=>!b.IsDeleted))))
        //            return ServiceResultFactory.CreateError(false, MessageId.BomQuantityCanAdjust);
        //        Mrp mrpModel = null;
        //        if (!model.IsRequiredMRP)
        //        {

        //            mrpModel = new Mrp
        //            {
        //                ProductGroupId = bom.Product.ProductGroupId,
        //                ContractCode = bom.Product.ContractCode,
        //                Description = bom.Product.Description,
        //                MrpStatus = MrpStatus.Active,
        //                MrpItems = new List<MrpItem>()
        //            };
        //            var count = await _mrpRepository.CountAsync(a => a.ContractCode == authenticate.ContractCode);
        //            var codeRes = await _formConfigService.GenerateFormCodeAsync(authenticate.ContractCode, FormName.MRP, count);
        //            if (!codeRes.Succeeded)
        //                return ServiceResultFactory.CreateError(false, codeRes.Messages.First().Message);
        //            mrpModel.MrpNumber = codeRes.Result;
        //        }

        //        BomProduct bomProduct = new BomProduct { CoefficientUse = model.Quantity, IsRequiredMRP = model.IsRequiredMRP, MaterialType = bom.MaterialType, ProductId = bom.ProductId, Remained = model.Quantity };

        //        await _bomProductRepository.AddAsync(bomProduct);
        //        if(await _unitOfWork.SaveChangesAsync() > 0)
        //        {
        //            if (!model.IsRequiredMRP)
        //                await AddMrpAuto(new List<BomProduct> { bomProduct }, mrpModel);
        //            return ServiceResultFactory.CreateSuccess(true);
        //        }
        //        return ServiceResultFactory.CreateError(false, MessageId.SaveFailed);
        //    }
        //    catch (Exception exception)
        //    {
        //        return ServiceResultFactory.CreateException(false, exception);
        //    }
        //}

        private MrpReportDto MergeMrpsForArchive(List<MrpReportListDto> mrps)
        {
            MrpReportDto result = new MrpReportDto();
            result.MrpList = new List<MrpReportListDto>();
            foreach (var item in mrps)
            {
                if (!result.MrpList.Any(a => a.MRPId == item.MRPId))
                {
                    result.MrpList.Add(new MrpReportListDto
                    {
                        MRPItemId = item.MRPItemId,
                        MRPId = item.MRPId,
                        CreateDate = item.CreateDate,
                        DateEnd = item.DateEnd,
                        DateStart = item.DateStart,
                        GrossRequirement = mrps.Where(a => a.MRPId == item.MRPId).Sum(a => a.GrossRequirement),
                        PO = mrps.Where(a => a.MRPId == item.MRPId).Sum(a => a.PO),
                        ReservedStock = mrps.Where(a => a.MRPId == item.MRPId).Sum(a => a.ReservedStock),
                        WarehouseStock = mrps.Where(a => a.MRPId == item.MRPId).Sum(a => a.WarehouseStock),
                        SurplusQuantity = mrps.Where(a => a.MRPId == item.MRPId).Sum(a => a.SurplusQuantity),
                        MrpNumber = item.MrpNumber,
                    });
                }
            }
            result.Quantity = result.MrpList.Sum(a => a.GrossRequirement);
            return result;
        }
        private PurchaseRequestReportDto MergePurchaseRequestsForArchive(List<PRReportListDto> purchaseRequests)
        {
            PurchaseRequestReportDto result = new PurchaseRequestReportDto();
            result.PurchaseRequestList = new List<PRReportListDto>();
            foreach (var item in purchaseRequests)
            {
                if (!result.PurchaseRequestList.Any(a => a.PRId == item.PRId))
                {
                    result.PurchaseRequestList.Add(new PRReportListDto
                    {
                        CreateDate = item.CreateDate,
                        MRPNumber = item.MRPNumber,
                        PRCode = item.PRCode,
                        PRId = item.PRId,
                        PRItemId = item.PRItemId,
                        Quntity = purchaseRequests.Where(a=>a.PRId==item.PRId).Sum(a=>a.Quntity)
                    });
                }
            }
            result.Quantity = result.PurchaseRequestList.Sum(a => a.Quntity);
            return result;
        }
        private RFPReportDto MergeRFPsForArchive(List<RFPReportListDto> rfps)
        {
            RFPReportDto result = new RFPReportDto();
            result.RFPList = new List<RFPReportListDto>();
            foreach (var item in rfps)
            {
                if (!result.RFPList.Any(a => a.RFPId == item.RFPId))
                {
                    result.RFPList.Add(new RFPReportListDto
                    {
                        RFPId = item.RFPId,
                        RFPItemId = item.RFPItemId,
                        RFPStatus = item.RFPStatus,
                        CreateDate = item.CreateDate,
                        RFPNumber = item.RFPNumber,
                        PRCode = item.PRCode,
                        Quntity = rfps.Where(a=>a.RFPId==item.RFPId).Sum(a=>a.Quntity),
                    });
                }
            }
            result.Quantity = result.RFPList.Sum(a => a.Quntity);
            return result;
        }
        private PrContractReportDto MergePrContractsForArchive(List<PRCReportListDto> prContracts)
        {
            PrContractReportDto result = new PrContractReportDto();
            result.PrContractList = new List<PRCReportListDto>();
            foreach (var item in prContracts)
            {
                if (!result.PrContractList.Any(a => a.PRContractId == item.PRContractId))
                {
                    result.PrContractList.Add(new PRCReportListDto
                    {
                        PRCSubjectId = item.PRCSubjectId,
                        DateEnd = item.DateEnd,
                        DateIssued = item.DateIssued,
                        CreateDate = item.CreateDate,
                        RFPNumber = item.RFPNumber,
                        PRContractCode = item.PRContractCode,
                        PRContractId = item.PRContractId,
                        Quntity = prContracts.Where(a=>a.PRContractId==item.PRContractId).Sum(a=>a.Quntity),
                        OrderQuantity = item.OrderQuantity,
                        ReceiptQuantity = item.ReceiptQuantity,
                    });
                }
            }
            result.Quantity = result.PrContractList.Sum(a => a.Quntity);
            return result;
        }

        private POReportDto MergePosForArchive(List<POReportListDto> pos)
        {
            POReportDto result = new POReportDto();
            result.PoList = new List<POReportListDto>();
            foreach (var item in pos)
            {
                if (!result.PoList.Any(a => a.POId == item.POId))
                {
                    result.PoList.Add(new POReportListDto
                    {
                        POId = item.POId,
                        DateDelivery = item.DateDelivery,
                        MrpNumber = item.MrpNumber,
                        POCode = item.POCode,
                        POSubjectId = item.POSubjectId,
                        PRContractCode = item.PRContractCode,
                        Quantity =pos.Where(a=>a.POId==item.POId).Sum(a=> a.Quantity),
                        ReceiptedQuantity = pos.Where(a => a.POId == item.POId).Sum(a => a.ReceiptedQuantity),
                        RemainedQuantity = pos.Where(a => a.POId == item.POId).Sum(a => a.RemainedQuantity),
                        CreateDate = item.CreateDate,
                    });
                }
            }
            result.Quantity = result.PoList.Sum(a => a.Quantity);
            return result;
        }
    }
}
