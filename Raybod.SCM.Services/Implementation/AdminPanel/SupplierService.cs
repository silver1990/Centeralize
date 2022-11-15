using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Raybod.SCM.DataAccess.Core;
using Raybod.SCM.DataAccess.Extention;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.ProductGroup;
using Raybod.SCM.DataTransferObject.RFP;
using Raybod.SCM.DataTransferObject.Supplier;
using Raybod.SCM.Domain.Model;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Utility.FileHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Implementation
{
    public class SupplierService : ISupplierService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITeamWorkAuthenticationService _authenticationService;
        private readonly DbSet<Supplier> _supplierRepository;
        private readonly DbSet<RFPProForma> _proFormaRepository;
        private readonly DbSet<ProductGroup> _productGroupRepository;
        private readonly DbSet<CompanyUser> _companyUserRepository;
        private readonly Raybod.SCM.Services.Utilitys.FileHelper _fileHelper;
        private readonly CompanyConfig _appSettings;
        public SupplierService(IUnitOfWork unitOfWork,
            IWebHostEnvironment hostingEnvironmentRoot,
            IOptions<CompanyAppSettingsDto> appSettings,
            IHttpContextAccessor httpContextAccessor,
            ITeamWorkAuthenticationService teamWorkAuthenticationService)
        {
            _unitOfWork = unitOfWork;
            _authenticationService = teamWorkAuthenticationService;
            _companyUserRepository = _unitOfWork.Set<CompanyUser>();
            _productGroupRepository = _unitOfWork.Set<ProductGroup>();
            _proFormaRepository = _unitOfWork.Set<RFPProForma>();
            _supplierRepository = _unitOfWork.Set<Supplier>();
            _fileHelper = new Utilitys.FileHelper(hostingEnvironmentRoot);
            httpContextAccessor.HttpContext.Request.Headers.TryGetValue("companyCode", out var CompanyCode);
            _appSettings = appSettings.Value.CompanyConfig.First(a => a.CompanyCode == CompanyCode);
        }

        public async Task<ServiceResult<ListSupplierDto>> AddSupplierAsync(AuthenticateDto authenticate, AddSupplierDto model)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<ListSupplierDto>(null, MessageId.AccessDenied);

                if (await _supplierRepository.AnyAsync(x => x.SupplierCode == model.SupplierCode))
                    return ServiceResultFactory.CreateError<ListSupplierDto>(null, MessageId.CodeExist);

                if (!string.IsNullOrWhiteSpace(model.Logo))
                {
                    var saveImage = _fileHelper.SaveImagesFromTemp(model.Logo, ServiceSetting.UploadImagesPath.LogoLarge, (int)ImageHelper.ImageWidth.FullHD);
                    saveImage = _fileHelper.SaveImagesFromTemp(model.Logo, ServiceSetting.UploadImagesPath.LogoSmall, (int)ImageHelper.ImageWidth.Vcd);
                    if (!string.IsNullOrWhiteSpace(saveImage))
                    {
                        _fileHelper.DeleteImagesFromTemp(model.Logo);
                        model.Logo = saveImage;
                    }
                }

                var supplierModel = new Supplier
                {
                    Address = model.Address,
                    EconomicCode = model.EconomicCode,
                    Email = model.Email,
                    Fax = model.Fax,
                    Logo = model.Logo,
                    Name = model.Name,
                    NationalId = model.NationalId,
                    PostalCode = model.PostalCode,
                    Website = model.Website,
                    TellPhone = model.TellPhone,
                    SupplierCode = model.SupplierCode
                };

                if (model.ProductGroups != null && model.ProductGroups.Any())
                {
                    var productGroups = await _productGroupRepository
                         .Where(a => !a.IsDeleted && model.ProductGroups.Contains(a.Id))
                         .ToListAsync();

                    if (productGroups.Count() != model.ProductGroups.Count())
                        return ServiceResultFactory.CreateError<ListSupplierDto>(null, MessageId.DataInconsistency);

                    supplierModel.SupplierProductGroups = new List<SupplierProductGroup>();

                    foreach (var group in productGroups)
                    {
                        supplierModel.SupplierProductGroups.Add(new SupplierProductGroup
                        {
                            ProductGroup = group
                        });
                    }
                }

                _supplierRepository.Add(supplierModel);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    ListSupplierDto result = ReturnSupplierListDto(supplierModel);
                    return ServiceResultFactory.CreateSuccess(result);
                }

                return ServiceResultFactory.CreateError<ListSupplierDto>(null, MessageId.SaveFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<ListSupplierDto>(null, exception);
            }
        }

        //public async Task<ServiceResult<bool>> AddSupplierProductGroupAsync(SupplierProductGroup model)
        //{
        //    var messages = new List<ServiceMessage>();
        //    if (model.SupplierId <= 0 || model.ProductGroupId <= 0)
        //    {
        //        messages.Add(new ServiceMessage(MessageType.Error, MessageId.InputDataValidationError));
        //        return new ServiceResult<bool>(false, false, messages);
        //    }
        //    try
        //    {
        //        var supplier = await _supplierRepository
        //            .Where(x => x.Id == model.SupplierId).Include(x => x.SupplierProductGroups)
        //            .FirstOrDefaultAsync();
        //        if (supplier == null)
        //        {
        //            messages.Add(new ServiceMessage(MessageType.Error, MessageId.EntityDoesNotExist));
        //            return new ServiceResult<bool>(false, false, messages);
        //        }

        //        if (supplier.SupplierProductGroups != null && supplier.SupplierProductGroups.Any(x => x.ProductGroupId == model.ProductGroupId))
        //        {
        //            messages.Add(new ServiceMessage(MessageType.Error, MessageId.Duplicate));
        //            return new ServiceResult<bool>(false, false, messages);
        //        }
        //        var supplierProductGroup = new SupplierProductGroup { SupplierId = model.SupplierId, ProductGroupId = model.ProductGroupId };
        //        if (supplier.SupplierProductGroups != null && supplier.SupplierProductGroups.Count() > 0)
        //        {
        //            supplier.SupplierProductGroups.Add(supplierProductGroup);
        //        }
        //        else
        //        {
        //            supplier.SupplierProductGroups = new List<SupplierProductGroup> { supplierProductGroup };
        //        }

        //        if (await _unitOfWork.SaveChangesAsync() > 0)
        //        {
        //            messages.Add(new ServiceMessage(MessageType.Succeed, MessageId.Succeeded));
        //            return new ServiceResult<bool>(true, true, messages);
        //        }
        //        messages.Add(new ServiceMessage(MessageType.Error, MessageId.InternalError));
        //        return new ServiceResult<bool>(false, false, messages);
        //    }
        //    catch (Exception exception)
        //    {
        //        messages.Add(new ServiceMessage(MessageType.Error, MessageId.Exception));
        //        return new ServiceResult<bool>(false, false, messages, exception);
        //    }
        //}

        public async Task<ServiceResult<ListSupplierDto>> EditSupplierAsync(AuthenticateDto authenticate, int supplierId, AddSupplierDto model)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<ListSupplierDto>(null, MessageId.AccessDenied);

                var supplierModel = await _supplierRepository
                    .Include(a => a.SupplierProductGroups)
                    .ThenInclude(c => c.ProductGroup)
                    .FirstOrDefaultAsync(a => !a.IsDeleted && a.Id == supplierId);

                if (supplierModel == null)
                    return ServiceResultFactory.CreateError<ListSupplierDto>(null, MessageId.EntityDoesNotExist);


                if (await _supplierRepository.AnyAsync(x => x.Id != supplierId && x.SupplierCode == model.SupplierCode))
                    return ServiceResultFactory.CreateError<ListSupplierDto>(null, MessageId.CodeExist);

                if (model.ProductGroups != null && model.ProductGroups.Any())
                {
                    var productGroups = await _productGroupRepository
                       .Where(a => !a.IsDeleted && model.ProductGroups.Contains(a.Id))
                       .ToListAsync();

                    if (productGroups.Count() != model.ProductGroups.Count())
                        return ServiceResultFactory.CreateError<ListSupplierDto>(null, MessageId.DataInconsistency);

                    supplierModel = UpdateSupplierProductGroup(supplierModel, productGroups);
                }
                else
                {
                    supplierModel = UpdateSupplierProductGroup(supplierModel, new List<ProductGroup>());
                }

                if (!string.IsNullOrWhiteSpace(model.Logo) && _fileHelper.ImageExistInTemp(model.Logo))
                {
                    var saveImage = _fileHelper.SaveImagesFromTemp(model.Logo, ServiceSetting.UploadImagesPath.LogoLarge, (int)ImageHelper.ImageWidth.FullHD);
                    saveImage = _fileHelper.SaveImagesFromTemp(model.Logo, ServiceSetting.UploadImagesPath.LogoSmall, (int)ImageHelper.ImageWidth.Vcd);
                    if (!string.IsNullOrWhiteSpace(saveImage))
                    {
                        _fileHelper.DeleteImagesFromTemp(model.Logo);
                        supplierModel.Logo = saveImage ?? supplierModel.Logo;
                    }
                }
                else
                {
                    supplierModel.Logo = supplierModel.Logo;
                }

                supplierModel.Address = model.Address;
                supplierModel.EconomicCode = model.EconomicCode;
                supplierModel.Email = model.Email;
                supplierModel.Fax = model.Fax;
                supplierModel.Logo = model.Logo;
                supplierModel.Name = model.Name;
                supplierModel.NationalId = model.NationalId;
                supplierModel.PostalCode = model.PostalCode;
                supplierModel.Website = model.Website;
                supplierModel.TellPhone = model.TellPhone;
                supplierModel.SupplierCode = model.SupplierCode;

                await _unitOfWork.SaveChangesAsync();

                ListSupplierDto result = ReturnSupplierListDto(supplierModel);
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<ListSupplierDto>(null, exception);
            }
        }

        private static ListSupplierDto ReturnSupplierListDto(Supplier supplierModel)
        {
            return new ListSupplierDto
            {
                Address = supplierModel.Address,
                EconomicCode = supplierModel.EconomicCode,
                Email = supplierModel.Email,
                Fax = supplierModel.Fax,
                Logo = supplierModel.Logo,
                Name = supplierModel.Name,
                NationalId = supplierModel.NationalId,
                PostalCode = supplierModel.PostalCode,
                Website = supplierModel.Website,
                TellPhone = supplierModel.TellPhone,
                SupplierCode = supplierModel.SupplierCode,
                Id = supplierModel.Id,
                ProductGroups = supplierModel.SupplierProductGroups != null ? supplierModel.SupplierProductGroups.Select(c => new ProductGroupMiniIfoDto
                {
                    Id = c.ProductGroupId,
                    Title = c.ProductGroup.Title,
                    ProductGroupCode = c.ProductGroup.ProductGroupCode,
                }).ToList() : new List<ProductGroupMiniIfoDto>()
            };
        }

        private Supplier UpdateSupplierProductGroup(Supplier supplierModel, List<ProductGroup> productGroups)
        {
            if (productGroups == null || productGroups.Count() == 0)
            {
                if (supplierModel.SupplierProductGroups != null)
                {
                    var removeList = supplierModel.SupplierProductGroups.ToList();
                    foreach (var item in removeList)
                    {
                        supplierModel.SupplierProductGroups.Remove(item);
                    }

                }
            }
            else
            {
                if (supplierModel.SupplierProductGroups == null || supplierModel.SupplierProductGroups.Count() == 0)
                {
                    supplierModel.SupplierProductGroups = new List<SupplierProductGroup>();

                    foreach (var item in productGroups)
                    {
                        supplierModel.SupplierProductGroups.Add(new SupplierProductGroup
                        {
                            ProductGroup = item,
                            SupplierId = supplierModel.Id
                        });
                    }
                }
                else
                {
                    var removeItems = supplierModel.SupplierProductGroups.Where(a => !productGroups.Any(c => c.Id == a.ProductGroupId)).ToList();

                    var addItems = productGroups.Where(a => !supplierModel.SupplierProductGroups.Any(c => c.ProductGroupId == a.Id))
                        .Select(c => new SupplierProductGroup
                        {
                            ProductGroup = c,
                            SupplierId = supplierModel.Id
                        }).ToList();

                    foreach (var item in removeItems)
                    {
                        supplierModel.SupplierProductGroups.Remove(item);

                    }

                    foreach (var item in addItems)
                    {
                        supplierModel.SupplierProductGroups.Add(item);

                    }
                }
            }
            return supplierModel;


        }


        public async Task<ServiceResult<List<ListSupplierDto>>> GetSupplierAsync(AuthenticateDto authenticate, SupplierQuery query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<ListSupplierDto>>(null, MessageId.AccessDenied);

                var dbQuery = _supplierRepository
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted);

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(x => x.Name.Contains(query.SearchText) || x.SupplierCode.Contains(query.SearchText));

                var totalCount = dbQuery.Count();
                dbQuery = dbQuery.ApplayPageing(query.Page, query.PageSize);

                var result = await dbQuery.Select(c => new ListSupplierDto
                {
                    Id = c.Id,
                    Address = c.Address,
                    EconomicCode = c.EconomicCode,
                    Email = c.Email,
                    Fax = c.Fax,
                    Logo = c.Logo,
                    Name = c.Name,
                    NationalId = c.NationalId,
                    PostalCode = c.PostalCode,
                    Website = c.Website,
                    TellPhone = c.TellPhone,
                    SupplierCode = c.SupplierCode,
                    ProductGroups = c.SupplierProductGroups.Select(c => new ProductGroupMiniIfoDto
                    {
                        Id = c.ProductGroupId,
                        Title = c.ProductGroup.Title,
                        ProductGroupCode = c.ProductGroup.ProductGroupCode,
                    }).ToList()
                }).ToListAsync();


                return ServiceResultFactory
                    .CreateSuccess(result)
                    .WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<ListSupplierDto>>(null, exception);
            }
        }

        //public async Task<ServiceResult<BaseSupplierDto>> GetSupplierByIdAsync(int supplierId)
        //{
        //    var messages = new List<ServiceMessage>();
        //    try
        //    {
        //        var supplier = await _supplierRepository.FindAsync(supplierId);
        //        if (supplier == null)
        //        {
        //            messages.Add(new ServiceMessage(MessageType.Error, MessageId.EntityDoesNotExist));
        //            return new ServiceResult<BaseSupplierDto>(false, new BaseSupplierDto(), messages);
        //        }
        //        var mapperConfiguration = new MapperConfiguration(configuration =>
        //        {
        //            configuration.CreateMap<BaseSupplierDto, Supplier>();
        //            configuration.CreateMap<Supplier, BaseSupplierDto>()
        //             .ForMember(a => a.Logo, c => c.MapFrom(a => _appSettings.AdminHost + ServiceSetting.UploadImagesPath.LogoSmall + a.Logo));
        //        });
        //        var mapper = mapperConfiguration.CreateMapper();
        //        var result = mapper.Map<BaseSupplierDto>(supplier);
        //        messages.Add(new ServiceMessage(MessageType.Succeed, MessageId.Succeeded));
        //        return new ServiceResult<BaseSupplierDto>(true, result, messages);
        //    }
        //    catch (Exception exception)
        //    {
        //        messages.Add(new ServiceMessage(MessageType.Error, MessageId.Exception));
        //        return new ServiceResult<BaseSupplierDto>(false, new BaseSupplierDto(), messages);
        //    }
        //}

        public async Task<ServiceResult<List<SupplierMiniInfoDto>>> GetSupplierOfSupportThisProductIdAsync(int productId)
        {
            try
            {
                var dbQuery = await _supplierRepository
                    .AsNoTracking()
                    .Where(c => !c.IsDeleted && c.SupplierProductGroups
                    .Any(p => p.ProductGroup.Products.Any(a => a.Id == productId)))
                    .ToListAsync();

                if (dbQuery == null)
                    return ServiceResultFactory.CreateError(new List<SupplierMiniInfoDto>(), MessageId.EntityDoesNotExist);

                var mapperConfiguration = new MapperConfiguration(configuration =>
                {
                    configuration.CreateMap<Supplier, SupplierMiniInfoDto>()
                    .ForMember(a => a.Logo, c => c.MapFrom(a => _appSettings.ClientHost + ServiceSetting.UploadImagesPath.LogoSmall + a.Logo));
                });

                var mapper = mapperConfiguration.CreateMapper();
                var result = mapper.Map<List<SupplierMiniInfoDto>>(dbQuery);
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new List<SupplierMiniInfoDto>(), exception);
            }
        }

        public async Task<ServiceResult<List<SupplierMiniInfoDto>>> GetSupplierOfSupportThisProductGroupsAsync(AuthenticateDto authenticate, int productGroupId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<SupplierMiniInfoDto>>(null, MessageId.AccessDenied);

                var dbQuery = _supplierRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted
                    && a.SupplierProductGroups.Any(p => p.ProductGroupId == productGroupId));

                var result = await dbQuery.Select(a => new SupplierMiniInfoDto
                {
                    Id = a.Id,
                    Email = a.Email,
                    Name = a.Name,
                    SupplierCode = a.SupplierCode,
                    TellPhone = a.TellPhone,
                    Logo = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.LogoSmall + a.Logo,
                    ProductGroups = a.SupplierProductGroups.Where(c => !c.ProductGroup.IsDeleted)
                    .Select(c => c.ProductGroup.Title).ToList()
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new List<SupplierMiniInfoDto>(), exception);
            }
        }

        

        public async Task<ServiceResult<List<SupplierMiniInfoDto>>> GetSuppliersAsync(AuthenticateDto authenticate, SupplierQuery query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<SupplierMiniInfoDto>>(null, MessageId.AccessDenied);

                var dbQuery = _supplierRepository

                    .AsNoTracking()
                    .Where(a => !a.IsDeleted);

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a => a.Name.Contains(query.SearchText) || a.SupplierCode.Contains(query.SearchText));

                if (query.ProductGroupIds != null && query.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(a => a.SupplierProductGroups.Any(c => query.ProductGroupIds.Contains(c.ProductGroupId)));

                var columnsMap = new Dictionary<string, Expression<Func<Supplier, object>>>
                {
                    ["Id"] = v => v.Id,
                    ["Name"] = v => v.Name,
                    ["SupplierCode"] = v => v.SupplierCode,
                };
                var totalCount = dbQuery.Count();
                dbQuery = dbQuery.ApplayPageing(query).ApplayOrdering(query, columnsMap).AsQueryable();

                var result = await dbQuery.Select(a => new SupplierMiniInfoDto
                {
                    Id = a.Id,
                    Email = a.Email,
                    Name = a.Name,
                    SupplierCode = a.SupplierCode,
                    TellPhone = a.TellPhone,
                    Logo = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.LogoSmall + a.Logo,
                    ProductGroups = null
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new List<SupplierMiniInfoDto>(), exception);
            }
        }
        public async Task<ServiceResult<List<SupplierMiniInfoDto>>> GetSuppliersListAsync(AuthenticateDto authenticate)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<SupplierMiniInfoDto>>(null, MessageId.AccessDenied);

                var dbQuery = _supplierRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted &&
                    a.RFPSuppliers.Any(a => !a.IsDeleted &&
                    a.IsActive &&
                    !a.RFP.IsDeleted &&
                    a.RFP.ContractCode == authenticate.ContractCode
                    ));

                var totalCount = dbQuery.Count();

                var result = await dbQuery.Select(a => new SupplierMiniInfoDto
                {
                    Id = a.Id,
                    Email = a.Email,
                    Name = a.Name,
                    SupplierCode = a.SupplierCode,
                    TellPhone = a.TellPhone,
                    Logo = (!String.IsNullOrEmpty(a.Logo)) ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.LogoSmall + a.Logo : "",
                    ProductGroups = null,
                    EconomicCode = a.EconomicCode,
                    NationalId = a.NationalId,
                    PostalCode = a.PostalCode,
                    Website = a.Website,
                    Address = a.Address
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new List<SupplierMiniInfoDto>(), exception);
            }
        }
        public async Task<ServiceResult<List<SupplierMiniInfoDto>>> GetWinnerRFPSuppliersAsync(AuthenticateDto authenticate)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<SupplierMiniInfoDto>>(null, MessageId.AccessDenied);

                var dbQuery = _supplierRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted &&
                    a.RFPSuppliers.Any(a => !a.IsDeleted &&
                    a.IsActive &&
                    a.IsWinner &&
                    !a.RFP.IsDeleted &&
                    a.RFP.ContractCode== authenticate.ContractCode &&
                    a.RFP.RFPItems.Any(c => !c.IsDeleted && c.IsActive && c.RemainedStock > 0)));

                var totalCount = dbQuery.Count();

                var result = await dbQuery.Select(a => new SupplierMiniInfoDto
                {
                    Id = a.Id,
                    Email = a.Email,
                    Name = a.Name,
                    SupplierCode = a.SupplierCode,
                    TellPhone = a.TellPhone,
                    Logo = (!String.IsNullOrEmpty(a.Logo))?_appSettings.WepApiHost + ServiceSetting.UploadImagesPath.LogoSmall + a.Logo:"",
                    ProductGroups = null,
                    EconomicCode=a.EconomicCode,
                    NationalId=a.NationalId,
                    PostalCode=a.PostalCode,
                    Website=a.Website,
                    Address=a.Address
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new List<SupplierMiniInfoDto>(), exception);
            }
        }

        //public async Task<ServiceResult<bool>> DeleteSupplierProductGroupAsync(int supplierId, int productGroupId)
        //{
        //    var messages = new List<ServiceMessage>();
        //    try
        //    {
        //        var supplierProduct = await _supplierProductGroupRepository.FirstOrDefaultAsync(a => a.SupplierId == supplierId && a.ProductGroupId == productGroupId);
        //        if (supplierProduct == null)
        //        {
        //            messages.Add(new ServiceMessage(MessageType.Error, MessageId.EntityDoesNotExist));
        //            return new ServiceResult<bool>(false, false, messages);
        //        }

        //        _supplierProductGroupRepository.Remove(supplierProduct);
        //        if (await _unitOfWork.SaveChangesAsync() > 0)
        //        {
        //            messages.Add(new ServiceMessage(MessageType.Succeed, MessageId.Succeeded));
        //            return new ServiceResult<bool>(true, true, messages);
        //        }
        //        messages.Add(new ServiceMessage(MessageType.Error, MessageId.InternalError));
        //        return new ServiceResult<bool>(false, false, messages);
        //    }
        //    catch (Exception exception)
        //    {
        //        messages.Add(new ServiceMessage(MessageType.Error, MessageId.Exception));
        //        return new ServiceResult<bool>(false, false, messages, exception);
        //    }
        //}

        public async Task<ServiceResult<bool>> DeleteSupplierAsync(AuthenticateDto authenticate, int id)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                if (await _supplierRepository.AnyAsync(a => !a.IsDeleted && a.Id == id && a.RFPSuppliers.Any()))
                    return ServiceResultFactory.CreateError(false, MessageId.DeleteDontAllowedBeforeSubset);

                var supplierModel = await _supplierRepository.FirstOrDefaultAsync(a => !a.IsDeleted && a.Id == id);
                if (supplierModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);


                supplierModel.IsDeleted = true;
                return await _unitOfWork.SaveChangesAsync() > 0
                    ? ServiceResultFactory.CreateSuccess(true)
                    : ServiceResultFactory.CreateError(false, MessageId.InternalError);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }



        public async Task<ServiceResult<BaseSupplierUserDto>> AddSupplierUserAsync(AuthenticateDto authenticate, int supplierId, AddSupplierUserDto model)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<BaseSupplierUserDto>(null, MessageId.AccessDenied);

                if (!await _supplierRepository.AnyAsync(a => !a.IsDeleted && a.Id == supplierId))
                    return ServiceResultFactory.CreateError<BaseSupplierUserDto>(null, MessageId.SupplierNotFound);

                if (await _companyUserRepository.AnyAsync(a => !a.IsDeleted && a.SupplierId == supplierId && a.Email == model.Email))
                    return ServiceResultFactory.CreateError<BaseSupplierUserDto>(null, MessageId.DuplicateInformation);

                var newSupplierUserModel = new CompanyUser
                {
                    SupplierId = supplierId,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName
                };
                _companyUserRepository.Add(newSupplierUserModel);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var res = new BaseSupplierUserDto
                    {
                        SupplierUserId = newSupplierUserModel.CompanyUserId,
                        Email = model.Email,
                        FirstName = model.FirstName,
                        LastName = model.LastName
                    };
                    return ServiceResultFactory.CreateSuccess(res);
                }
                return ServiceResultFactory.CreateError<BaseSupplierUserDto>(null, MessageId.SaveFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<BaseSupplierUserDto>(null, exception);
            }
        }

        public async Task<ServiceResult<List<BaseSupplierUserDto>>> GetSupplierUserAsync(AuthenticateDto authenticate, int supplierId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<BaseSupplierUserDto>>(null, MessageId.AccessDenied);

                var dbQuery = _companyUserRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.SupplierId == supplierId);

                var result = await dbQuery.Select(c => new BaseSupplierUserDto
                {
                    SupplierUserId = c.CompanyUserId,
                    Email = c.Email,
                    FirstName = c.FirstName,
                    LastName = c.LastName
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<BaseSupplierUserDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<bool>> DeleteSupplierUserByIdAsync(AuthenticateDto authenticate, int supplierId, int supplierUserId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var userModel = await _companyUserRepository.FirstOrDefaultAsync(a => !a.IsDeleted && a.SupplierId == supplierId && a.CompanyUserId == supplierUserId);
                if (userModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                userModel.IsDeleted = true;
                return await _unitOfWork.SaveChangesAsync() > 0 ?
                    ServiceResultFactory.CreateSuccess(true) :
                    ServiceResultFactory.CreateError(false, MessageId.AccessDenied);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

    }
}
