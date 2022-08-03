using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Raybod.SCM.DataAccess.Core;
using Raybod.SCM.DataAccess.Extention;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Bom;
using Raybod.SCM.DataTransferObject.Product;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Model;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Raybod.SCM.Utility.Extention;
using Raybod.SCM.DataTransferObject.Audit;
using System.Linq.Expressions;
using Raybod.SCM.DataTransferObject.Notification;
using Raybod.SCM.Domain.Struct;
using Raybod.SCM.DataTransferObject.Mrp;
using Raybod.SCM.DataTransferObject._PanelDocument.Area;

namespace Raybod.SCM.Services.Implementation
{
    public class BomService : IBomProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISCMLogAndNotificationService _scmLogAndNotificationService;
        private readonly ITeamWorkAuthenticationService _authenticationService;
        private readonly IMasterMrService _masterMrService;
        private readonly DbSet<BomProduct> _bomProductRepository;
        private readonly DbSet<Area> _areaRepository;
        private readonly DbSet<Product> _productRepository;
        private readonly DbSet<MasterMR> _masterMRRepository;
        private readonly DbSet<Contract> _contractRepository;
        private readonly CompanyAppSettingsDto _appSettings;


        public BomService(
            IUnitOfWork unitOfWork,
            IOptions<CompanyAppSettingsDto> AppSettings,
            ISCMLogAndNotificationService scmLogAndNotificationService,
            ITeamWorkAuthenticationService teamWorkAuthenticationService,
            IMasterMrService masterMrService)
        {
            _unitOfWork = unitOfWork;
            _masterMrService = masterMrService;
            _scmLogAndNotificationService = scmLogAndNotificationService;
            _authenticationService = teamWorkAuthenticationService;
            _bomProductRepository = _unitOfWork.Set<BomProduct>();
            _productRepository = _unitOfWork.Set<Product>();
            _areaRepository = _unitOfWork.Set<Area>();
            _masterMRRepository = _unitOfWork.Set<MasterMR>();
            _contractRepository = _unitOfWork.Set<Contract>();
            _appSettings = AppSettings.Value;
        }

       
        

       
        public async Task<ServiceResult<ProductMiniInfo>> GetProductByProductIdAsync(int productId)
        {
            var messages = new List<ServiceMessage>();
            try
            {
                var dbQuery = _productRepository
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && x.Id == productId);

                dbQuery = dbQuery.Where(x => x.BomProducts == null || !x.BomProducts.Any(c => !c.IsDeleted && c.ParentBomId == null));

                var product = await dbQuery.Select(p => new ProductMiniInfo
                {
                    Id = p.Id,
                    ProductCode = p.ProductCode,
                    Description = p.Description,
                    TechnicalNumber = p.TechnicalNumber,
                    Unit = p.Unit
                }).FirstOrDefaultAsync();

                return ServiceResultFactory.CreateSuccess(product);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<ProductMiniInfo>(null, exception);
            }
        }

        public async Task<ServiceResult<List<ProductMiniInfo>>> GetProductForCreateBomAsync(string query)
        {
            var messages = new List<ServiceMessage>();
            try
            {
                if (string.IsNullOrEmpty(query))
                    return ServiceResultFactory.CreateSuccess(new List<ProductMiniInfo>());

                var dbQuery = _productRepository
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted);

                if (!string.IsNullOrEmpty(query))
                {
                    dbQuery = dbQuery
                        .Where(x =>
                        x.ProductCode.Contains(query) ||
                        (x.Description != null && x.Description.Contains(query)) ||
                        (x.TechnicalNumber != null && x.TechnicalNumber.Contains(query)));
                }

                dbQuery = dbQuery.Where(x => x.BomProducts == null || !x.BomProducts.Any(c => !c.IsDeleted && c.ParentBomId == null));

                var list = await dbQuery.Select(p => new ProductMiniInfo
                {
                    Id = p.Id,
                    ProductCode = p.ProductCode,
                    Description = p.Description,
                    TechnicalNumber = p.TechnicalNumber,
                    Unit = p.Unit,
                    Image = p.Image
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(list);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new List<ProductMiniInfo>(), exception);
            }
        }

       

        
        public async Task<ServiceResult<bool>> EditBomProductAsync(AuthenticateDto authenticate, long bomId, List<ListBomInfoDto> model)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var bomModel = await _bomProductRepository
                    .Include(a => a.Product)
                    .FirstOrDefaultAsync(a => !a.IsDeleted && a.ParentBomId == null && a.Id == bomId);
                if (bomModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                var selectedbom = await _bomProductRepository.Where(x => !x.IsDeleted && x.ParentBomId == bomId).Include(a => a.ChildBom)
                    .ToListAsync();
                if (selectedbom == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);
                // before change => for get log
                //string oldObject = _scmLogAndNotificationService.SerializerObject(selectedbom);

                foreach (var item in selectedbom)
                {
                    var postItem = model.FirstOrDefault(a => a.ProductId == item.ProductId);
                    if (postItem == null)
                    {
                        item.IsDeleted = true;
                        if (item.MaterialType != MaterialType.Component) continue;
                        foreach (var child in item.ChildBom.Where(a => !a.IsDeleted))
                        {
                            child.IsDeleted = true;
                        }
                    }
                    else
                    {
                        item.CoefficientUse = postItem.CoefficientUse;
                        item.AreaId =(postItem.Area!=null)? postItem.Area.AreaId:null;
                        switch (item.MaterialType)
                        {
                            case MaterialType.Component when postItem.MaterialType == MaterialType.Component:
                                {
                                    if (!postItem.ChildBoms.Any())
                                        return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);
                                    foreach (var child in item.ChildBom.Where(a => !a.IsDeleted))
                                    {
                                        var childPost = postItem.ChildBoms.FirstOrDefault(a => a.ProductId == child.ProductId);
                                        if (childPost == null)
                                        {
                                            child.IsDeleted = true;
                                        }
                                        else
                                        {
                                            child.CoefficientUse = childPost.CoefficientUse;
                                            child.AreaId = item.AreaId;
                                        }
                                    }
                                    // get new child
                                    var newchild = postItem.ChildBoms.Where(a => !item.ChildBom.Any(c => !c.IsDeleted && c.ProductId == a.ProductId))
                                        .Select(v => new BomProduct
                                        {
                                            IsDeleted = false,
                                            CoefficientUse = v.CoefficientUse,
                                            MaterialType = v.MaterialType,
                                            ParentBomId = item.Id,
                                            ProductId = v.ProductId,
                                            AreaId=item.AreaId
                                        }).ToList();
                                    // add new child
                                    foreach (var obj in newchild)
                                    {
                                        item.ChildBom.Add(obj);
                                    }

                                    break;
                                }
                            case MaterialType.Component:
                                {
                                    if (postItem.MaterialType == MaterialType.Part)
                                    {
                                        item.MaterialType = postItem.MaterialType;
                                        item.AreaId = (postItem.Area != null) ? postItem.Area.AreaId : null;
                                        foreach (var child in item.ChildBom.Where(a => !a.IsDeleted))
                                        {
                                            child.IsDeleted = true;
                                        }
                                    }
                                    break;
                                }
                            case MaterialType.Part when postItem.MaterialType == MaterialType.Part:
                                item.CoefficientUse = postItem.CoefficientUse;
                                item.AreaId = (postItem.Area != null) ? postItem.Area.AreaId : null;
                                break;
                            case MaterialType.Part:
                                {
                                    if (postItem.MaterialType == MaterialType.Component)
                                    {
                                        if (!postItem.ChildBoms.Any())
                                            return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                                        item.MaterialType = postItem.MaterialType;
                                        item.AreaId = (postItem.Area != null) ? postItem.Area.AreaId : null;
                                        item.ChildBom = new List<BomProduct>();
                                        item.ChildBom = postItem.ChildBoms.Select(v => new BomProduct
                                        {
                                            IsDeleted = false,
                                            CoefficientUse = v.CoefficientUse,
                                            MaterialType = v.MaterialType,
                                            ParentBomId = item.Id,
                                            ProductId = v.ProductId,
                                            AreaId=item.AreaId
                                        }).ToList();
                                    }
                                    break;
                                }
                        }
                    }
                }

                // get new parent child
                var newParentChild = model.Where(a => selectedbom.All(c => c.ProductId != a.ProductId))
                .Select(v => new BomProduct
                {
                    IsDeleted = false,
                    CoefficientUse = v.CoefficientUse,
                    MaterialType = v.MaterialType,
                    ParentBomId = bomId,
                    ProductId = v.ProductId,
                    AreaId= (v.Area != null) ? v.Area.AreaId : null,
                ChildBom = v.MaterialType == MaterialType.Component && v.ChildBoms != null && v.ChildBoms.Any() ? v.ChildBoms.Select(b => new BomProduct
                    {
                        ParentBomId = v.ProductId,
                        IsDeleted = false,
                        CoefficientUse = b.CoefficientUse,
                        MaterialType = b.MaterialType,
                        ProductId = b.ProductId,
                        AreaId= (v.Area != null) ? v.Area.AreaId : null,
                }).ToList() : null
                }).ToList();
                // add new parent child
                foreach (var obj in newParentChild)
                {
                    _bomProductRepository.Add(obj);
                }

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                   
                    var res = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = authenticate.ContractCode,
                        FormCode = bomModel.Product.ProductCode,
                        KeyValue = bomId.ToString(),
                        NotifEvent = NotifEvent.EditBOM,
                        RootKeyValue = bomId.ToString(),
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,
                        Description = bomModel.Product.Description
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

        //public async Task<ServiceResult<List<BomInfoDto>>> GetBomAsync(AuthenticateDto authenticate, BomQueryDto query)
        //{
        //    try
        //    {
        //        var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
        //        if (!permission.HasPermission)
        //            return ServiceResultFactory.CreateError<List<BomInfoDto>>(null, MessageId.AccessDenied);

        //        var dbQuery = _bomProductRepository
        //            .AsNoTracking()
        //            .Where(x => x.ParentBomId == null && !x.IsDeleted)
        //            .OrderByDescending(x => x.Id)
        //            .AsQueryable();

        //        dbQuery = dbQuery.Where(a => a.Product.ContractSubjects.Any(c => c.ContractCode == authenticate.ContractCode));

        //        if (!string.IsNullOrEmpty(query.SearchText))
        //            dbQuery = dbQuery.Where(x =>
        //              x.Product.ProductCode.Contains(query.SearchText)
        //              || x.Product.Description.Contains(query.SearchText)
        //              || x.Product.TechnicalNumber.Contains(query.SearchText)
        //            );
        //        if (query.AreaIds != null && query.AreaIds.Any())
        //           dbQuery= dbQuery.Where(a => a.ChildBom.Any(b =>b.AreaId!=null&& query.AreaIds.Contains(b.AreaId.Value)) || a.ChildBom.Any(c => c.ChildBom != null && c.ChildBom.Any(d =>d.AreaId!=null&& query.AreaIds.Contains(d.AreaId.Value))));

        //        var count = dbQuery.Count();
        //        dbQuery = dbQuery.ApplayPageing(query.Page, query.PageSize);
        //        var resultModel = await dbQuery.Select(p => new BomInfoDto
        //        {
        //            Id = p.Id,
        //            ProductId = p.ProductId,
        //            ProductDescription = p.Product.Description,
        //            ProductUnit = p.Product.Unit,
        //            ProductCode = p.Product.ProductCode,
        //            ProductTechnicalNumber = p.Product.TechnicalNumber,
        //            UserAudit = p.AdderUser != null ? new UserAuditLogDto
        //            {
        //                AdderUserId = p.AdderUserId,
        //                AdderUserName = p.AdderUser.FullName,
        //                CreateDate = p.CreatedDate.ToUnixTimestamp(),
        //                AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + p.AdderUser.Image
        //            } : null
        //        }).ToListAsync();

        //        return ServiceResultFactory.CreateSuccess(resultModel).WithTotalCount(count);
        //    }
        //    catch (Exception exception)
        //    {
        //        return ServiceResultFactory.CreateException(new List<BomInfoDto>(), exception);
        //    }
        //}
        public async Task<ServiceResult<List<BomWithChildInfo>>> GetBomAsync(AuthenticateDto authenticate, BomQueryDto query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<BomWithChildInfo>>(null, MessageId.AccessDenied);

                var dbQuery = _bomProductRepository
                    .AsNoTracking()
                    .Where(x => x.ParentBomId == null && !x.IsDeleted)
                    .OrderByDescending(x => x.Id)
                    .AsQueryable();

                dbQuery = dbQuery.Where(a => a.Product.ContractCode == authenticate.ContractCode);

                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.ProductGroupIds.Contains(a.Product.ProductGroupId));
                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(x =>
                      x.Product.ProductCode.Contains(query.SearchText)
                      || x.Product.Description.Contains(query.SearchText)
                      || x.Product.TechnicalNumber.Contains(query.SearchText)
                    );
                if (query.AreaIds != null && query.AreaIds.Any())
                    dbQuery = dbQuery.Where(a => a.ChildBom.Any(b => b.AreaId != null && query.AreaIds.Contains(b.AreaId.Value)) || a.ChildBom.Any(c => c.ChildBom != null && c.ChildBom.Any(d => d.AreaId != null && query.AreaIds.Contains(d.AreaId.Value))));

                var count = dbQuery.Count();
                dbQuery = dbQuery.ApplayPageing(query.Page, query.PageSize);
                dbQuery = dbQuery.OrderBy(a => a.Id);
                var resultModel = await dbQuery.Select(p => new BomWithChildInfo
                {
                    Id = p.Id,
                    ProductDescription = p.Product.Description,
                    ProductId = p.ProductId,
                    ProductUnit = p.Product.Unit,
                    ProductCode = p.Product.ProductCode,
                    ProductTechnicalNumber = p.Product.TechnicalNumber,
                    CoefficientUse=p.CoefficientUse,
                    MaterialType = p.MaterialType,
                    ProductGroupTitle=p.Product.ProductGroup.Title,
                    ProductGroupId = p.Product.ProductGroupId,
                    UserAudit = p.AdderUser == null ? null : new UserAuditLogDto
                    {
                        AdderUserName = p.AdderUser.FullName,
                        AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + p.AdderUser.Image,
                        CreateDate = p.CreatedDate.ToUnixTimestamp()
                    },
                    Area = (p.AreaId != null) ? new AreaReadDTO { AreaId = p.Area.AreaId, AreaTitle = p.Area.AreaTitle } : null,
                    ChildBoms = p.ChildBom.Where(x => !x.IsDeleted).Select(b => new ListBomInfoDto
                    {
                        Id = b.Id,
                        ProductId = b.ProductId,
                        ProductDescription = b.Product.Description,
                        ProductUnit = b.Product.Unit,
                        ProductCode = b.Product.ProductCode,
                        CoefficientUse = b.CoefficientUse,
                        MaterialType = b.MaterialType,
                        ProductGroupTitle = b.Product.ProductGroup.Title,
                        ProductGroupId=b.Product.ProductGroupId,
                        ProductTechnicalNumber = b.Product.TechnicalNumber,
                        Area = (b.AreaId != null) ? new AreaReadDTO { AreaId = b.Area.AreaId, AreaTitle = b.Area.AreaTitle } : null,

                    }).ToList()
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(resultModel).WithTotalCount(count);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new List<BomWithChildInfo>(), exception);
            }
        }
        public async Task<ServiceResult<ListBomInfoDto>> GetBomProductByProductIdAsync(int productId)
        {
            try
            {
                var bomModel = _bomProductRepository
                    .AsNoTracking()
                    .Where(c => !c.IsDeleted && c.ProductId == productId && c.ParentBomId == null);
                if (bomModel == null || !bomModel.Any())
                    return ServiceResultFactory.CreateError(new ListBomInfoDto(), MessageId.EntityDoesNotExist);
                var resultModel = await bomModel.Select(p => new ListBomInfoDto
                {
                    Id = p.Id,
                    ProductDescription = p.Product.Description,
                    ProductId = p.ProductId,
                    ProductUnit = p.Product.Unit,
                    ProductCode = p.Product.ProductCode,
                    ProductTechnicalNumber = p.Product.TechnicalNumber,
                    ChildBoms = p.ChildBom.Where(x => !x.IsDeleted).Select(b => new ListBomInfoDto
                    {
                        Id = b.Id,
                        ProductId = b.ProductId,
                        ProductDescription = b.Product.Description,
                        ProductUnit = b.Product.Unit,
                        ProductCode = b.Product.ProductCode,
                        CoefficientUse = b.CoefficientUse,
                        MaterialType = b.MaterialType,
                    }).ToList()

                }).FirstOrDefaultAsync();

                return ServiceResultFactory.CreateSuccess(resultModel);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new ListBomInfoDto(), exception);
            }
        }

        public async Task<ServiceResult<ListBomInfoDto>> GetBomProductByIdAsync(long bomId)
        {
            try
            {
                var bomModel = _bomProductRepository
                    .AsNoTracking()
                    .Where(c => !c.IsDeleted && c.Id == bomId && c.ParentBomId == null);
                if (bomModel == null || !bomModel.Any())
                    return ServiceResultFactory.CreateError(new ListBomInfoDto(), MessageId.EntityDoesNotExist);
                var resultModel = await bomModel.Select(p => new ListBomInfoDto
                {
                    Id = p.Id,
                    ProductDescription = p.Product.Description,
                    ProductId = p.ProductId,
                    ProductUnit = p.Product.Unit,
                    ProductCode = p.Product.ProductCode,
                    ProductTechnicalNumber = p.Product.TechnicalNumber,
                    UserAudit = p.AdderUser == null ? null : new UserAuditLogDto
                    {
                        AdderUserName = p.AdderUser.FullName,
                        AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + p.AdderUser.Image,
                        CreateDate = p.CreatedDate.ToUnixTimestamp()
                    },
                    ChildBoms = p.ChildBom.Where(x => !x.IsDeleted).Select(b => new ListBomInfoDto
                    {
                        Id = b.Id,
                        ProductId = b.ProductId,
                        ProductDescription = b.Product.Description,
                        ProductUnit = b.Product.Unit,
                        ProductCode = b.Product.ProductCode,
                        CoefficientUse = b.CoefficientUse,
                        MaterialType = b.MaterialType,
                        ProductTechnicalNumber = b.Product.TechnicalNumber,
                    }).ToList()

                }).FirstOrDefaultAsync();

                return ServiceResultFactory.CreateSuccess(resultModel);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new ListBomInfoDto(), exception);
            }
        }

        public async Task<ServiceResult<List<BomInfoDto>>> GetChildBomByBomIdAsync(long parentBomId)
        {
            try
            {
                var listChildBom = _bomProductRepository
                    .AsNoTracking()
                    .Where(x => x.ParentBomId == parentBomId);
                var resultModel = await listChildBom.Select(p => new BomInfoDto
                {
                    Id = p.Id,
                    ProductId = p.ProductId,
                    ProductDescription = p.Product.Description,
                    ProductTechnicalNumber = p.Product.TechnicalNumber,
                    ProductUnit = p.Product.Unit,
                    ProductCode = p.Product.ProductCode,
                    CoefficientUse = p.CoefficientUse,
                    MaterialType = p.MaterialType
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(resultModel ?? new List<BomInfoDto>());
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new List<BomInfoDto>(), exception);
            }
        }

        public async Task<ServiceResult<BomWithChildInfo>> GetBomProductByIdIncludeChildAsync(AuthenticateDto authenticate, long bomId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<BomWithChildInfo>(null, MessageId.AccessDenied);

                var bomModel = _bomProductRepository
                    .AsNoTracking()
                    .Where(c => !c.IsDeleted && c.Id == bomId && c.ParentBomId == null);
                if (bomModel == null || !bomModel.Any())
                    return ServiceResultFactory.CreateError(new BomWithChildInfo(), MessageId.EntityDoesNotExist);
                var resultModel = await bomModel.Select(p => new BomWithChildInfo
                {
                    Id = p.Id,
                    ProductDescription = p.Product.Description,
                    ProductId = p.ProductId,
                    ProductUnit = p.Product.Unit,
                    ProductCode = p.Product.ProductCode,
                    ProductTechnicalNumber = p.Product.TechnicalNumber,
                    UserAudit = p.AdderUser == null ? null : new UserAuditLogDto
                    {
                        AdderUserName = p.AdderUser.FullName,
                        AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + p.AdderUser.Image,
                        CreateDate = p.CreatedDate.ToUnixTimestamp()
                    },
                    ChildBoms = p.ChildBom.Where(x => !x.IsDeleted).Select(b => new ListBomInfoDto
                    {
                        Id = b.Id,
                        ProductId = b.ProductId,
                        ProductDescription = b.Product.Description,
                        ProductUnit = b.Product.Unit,
                        ProductCode = b.Product.ProductCode,
                        CoefficientUse = b.CoefficientUse,
                        MaterialType = b.MaterialType,
                        ProductTechnicalNumber = b.Product.TechnicalNumber,
                        Area=(b.AreaId!=null)?new AreaReadDTO { AreaId=b.Area.AreaId,AreaTitle=b.Area.AreaTitle}:null,
                        ChildBoms = b.ChildBom.Where(x => !x.IsDeleted).Select(g => new ListBomInfoDto
                        {
                            Id = g.Id,
                            ProductId = g.ProductId,
                            ProductDescription = g.Product.Description,
                            ProductTechnicalNumber = g.Product.TechnicalNumber,
                            ProductUnit = g.Product.Unit,
                            ProductCode = g.Product.ProductCode,
                            CoefficientUse = g.CoefficientUse,
                            MaterialType = g.MaterialType,
                            Area = (g.AreaId != null) ? new AreaReadDTO { AreaId = g.Area.AreaId, AreaTitle = g.Area.AreaTitle } : null
                        }).ToList(),

                    }).ToList()

                }).FirstOrDefaultAsync();

                return ServiceResultFactory.CreateSuccess(resultModel);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new BomWithChildInfo(), exception);
            }
        }

        public async Task<ServiceResult<bool>> RemoveBomAsync(AuthenticateDto authenticate, long bomId)
        {
            try
            {
                var model = await _bomProductRepository.FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == bomId);
                if (model == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (await _bomProductRepository.AnyAsync(x => !x.IsDeleted && x.Id == bomId && x.ChildBom.Any(c => !c.IsDeleted)))
                    return ServiceResultFactory.CreateError(false, MessageId.DeleteDontAllowedBeforeSubset);
                model.IsDeleted = true;

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var res = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = null,
                        FormCode = model.Product.ProductCode,
                        KeyValue = bomId.ToString(),
                        NotifEvent = NotifEvent.EditBOM,
                        RootKeyValue = bomId.ToString(),
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,
                        Description = model.Product.Description
                    }, null);
                    return ServiceResultFactory.CreateSuccess(true);
                }
                return ServiceResultFactory.CreateError(false, MessageId.DeleteEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<List<int>>> GetLastChildProductIdsOfbomByProductIdAsync(List<int> productIds)
        {
            try
            {
                var bomModels = await _bomProductRepository
                    .AsNoTracking()
                    .Where(c => !c.IsDeleted && productIds.Contains(c.ProductId) && c.ParentBomId == null)
                    .Select(p => new ListBomInfoDto
                    {
                        Id = p.Id,
                        ProductDescription = p.Product.Description,
                        ProductId = p.ProductId,
                        ProductTechnicalNumber = p.Product.TechnicalNumber,
                        ChildBoms = p.ChildBom.Where(x => !x.IsDeleted).Select(b => new ListBomInfoDto
                        {
                            Id = b.Id,
                            ProductId = b.ProductId,
                            ProductDescription = b.Product.Description,
                            ParentBomId = b.ParentBomId,
                            ProductTechnicalNumber = b.Product.TechnicalNumber,
                            ChildBoms = b.ChildBom.Where(c => !c.IsDeleted).Select(c => new ListBomInfoDto
                            {
                                Id = c.Id,
                                ProductId = c.ProductId,
                                ProductDescription = c.Product.Description,
                                ProductTechnicalNumber = c.Product.TechnicalNumber,
                                ParentBomId = c.ParentBomId,
                            }).ToList()
                        }).ToList(),
                    }).ToListAsync();

                if (bomModels == null)
                    return ServiceResultFactory.CreateError<List<int>>(null, MessageId.EntityDoesNotExist);

                if (bomModels.Count() != productIds.Count())
                    return ServiceResultFactory.CreateError<List<int>>(null, MessageId.BomNotFound);

                var result = new List<int>();

                foreach (var bom in bomModels)
                {
                    ///اگه زیرمجموعه نداشت خودشو برگردون
                    if (bom.ChildBoms == null || bom.ChildBoms.Count() == 0)
                    {
                        result.Add(bom.ProductId);
                        continue;
                    }

                    var pAllChildIds = bom.ChildBoms.Where(a => a.ChildBoms != null && a.ChildBoms.Any())
                        .SelectMany(a => a.ChildBoms.Select(a => a.ProductId))
                        .ToList();

                    if (pAllChildIds != null && pAllChildIds.Any())
                        result.AddRange(pAllChildIds);

                    var parentProductIds = bom.ChildBoms
                        .Where(a => a.ChildBoms == null || a.ChildBoms.Count() == 0)
                        .Select(a => a.ProductId).ToList();

                    if (parentProductIds != null && parentProductIds.Any())
                        result.AddRange(parentProductIds);
                }


                return ServiceResultFactory.CreateSuccess(result);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<int>>(null, exception);
            }
        }

        public async Task<ServiceResult<List<ListBomInfoDto>>> GetAllProductGroupIdsOfbomByBomIdAsync(long bomId)
        {
            try
            {
                var bomModel = await _bomProductRepository
                    .AsNoTracking()
                    .Where(c => !c.IsDeleted && c.Id == bomId && c.ParentBomId == null)
                    .Select(p => new ListBomInfoDto
                    {
                        Id = p.Id,
                        ProductId = p.ProductId,
                        ProductGroupId = p.Product.ProductGroupId,
                        ProductDescription = p.Product.ProductGroup.Title,
                        ChildBoms = p.ChildBom.Where(x => !x.IsDeleted).Select(b => new ListBomInfoDto
                        {
                            Id = b.Id,
                            ProductId = b.ProductId,
                            ProductGroupId = b.Product.ProductGroupId,
                            ProductDescription = b.Product.ProductGroup.Title,
                            ChildBoms = b.ChildBom.Where(c => !c.IsDeleted).Select(c => new ListBomInfoDto
                            {
                                Id = c.Id,
                                ProductId = c.ProductId,
                                ProductGroupId = c.Product.ProductGroupId,
                                ProductDescription = c.Product.ProductGroup.Title,
                            }).ToList()
                        }).ToList(),
                    }).FirstOrDefaultAsync();

                if (bomModel == null)
                    return ServiceResultFactory.CreateError<List<ListBomInfoDto>>(null, MessageId.EntityDoesNotExist);


                var temp = new List<ListBomInfoDto>();
                ///اگه زیرمجموعه نداشت خودشو برگردون
                if (bomModel.ChildBoms == null || bomModel.ChildBoms.Count() == 0)
                {
                    temp.Add(bomModel);
                    return ServiceResultFactory.CreateSuccess(temp);
                }

                var pAllChild = bomModel.ChildBoms.Where(a => a.ChildBoms != null && a.ChildBoms.Any())
                    .SelectMany(a => a.ChildBoms)
                    .ToList();

                if (pAllChild != null && pAllChild.Any())
                    temp.AddRange(pAllChild);

                var parentProduct = bomModel.ChildBoms
                    .Where(a => a.ChildBoms == null || a.ChildBoms.Count() == 0)
                    .ToList();

                if (parentProduct != null && parentProduct.Any())
                    temp.AddRange(parentProduct);


              temp = temp.GroupBy(a => a.ProductGroupId)
                    .Select(c => new ListBomInfoDto
                    {
                        ProductGroupId = c.Key,
                        ProductDescription = c.First().ProductDescription,
                    }).ToList();

                return ServiceResultFactory.CreateSuccess(temp);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<ListBomInfoDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<List<int>>> GetLastChildProductIdsOfContractbomAsync(string contractCode)
        {
            try
            {
                var bomModels = await _bomProductRepository
                    .AsNoTracking()
                    .Where(c => !c.IsDeleted && c.Product.ContractCode == contractCode && c.ParentBomId == null)
                    .Select(p => new ListBomInfoDto
                    {
                        Id = p.Id,
                        ProductId = p.ProductId,
                        ProductGroupId = p.Product.ProductGroupId,
                        ChildBoms = p.ChildBom.Where(x => !x.IsDeleted).Select(b => new ListBomInfoDto
                        {
                            Id = b.Id,
                            ProductId = b.ProductId,
                            ProductGroupId = b.Product.ProductGroupId,
                            ParentBomId = b.ParentBomId,
                            ChildBoms = b.ChildBom.Where(c => !c.IsDeleted).Select(c => new ListBomInfoDto
                            {
                                Id = c.Id,
                                ProductId = c.ProductId,
                                ProductGroupId = c.Product.ProductGroupId,
                                ParentBomId = c.ParentBomId,
                            }).ToList()
                        }).ToList(),
                    }).ToListAsync();

                if (bomModels == null || !bomModels.Any())
                    return ServiceResultFactory.CreateError<List<int>>(null, MessageId.EntityDoesNotExist);

                var result = new List<int>();
                foreach (var item in bomModels)
                {

                    var temp = new List<ListBomInfoDto>();
                    ///اگه زیرمجموعه نداشت ادامه نده
                    if (item.ChildBoms == null || item.ChildBoms.Count() == 0)
                        result.Add(item.ProductId);

                    var pAllChild = item.ChildBoms.Where(a => a.ChildBoms != null && a.ChildBoms.Any())
                        .SelectMany(a => a.ChildBoms)
                        .ToList();

                    if (pAllChild != null && pAllChild.Any())
                        temp.AddRange(pAllChild);

                    var parentProduct = item.ChildBoms
                        .Where(a => a.ChildBoms == null || a.ChildBoms.Count() == 0)
                        .ToList();

                    if (parentProduct != null && parentProduct.Any())
                        temp.AddRange(parentProduct);


                    result.AddRange(temp.Select(a => a.ProductId).ToList());
                }
                return ServiceResultFactory.CreateSuccess(result);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<int>>(null, exception);
            }
        }

        public async Task<ServiceResult<List<ListBomInfoDto>>> GetBomProductForDocumentByContractCodeAsync(AuthenticateDto authenticate)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
               
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<ListBomInfoDto>>(null, MessageId.AccessDenied);
                var bomModels = await _bomProductRepository
                    .AsNoTracking()
                    .Where(c => !c.IsDeleted &&  c.ParentBomId == null && c.Product.ContractCode == authenticate.ContractCode)
                    .Select(p => new ListBomInfoDto
                    {
                        Id = p.Id,
                        ProductDescription = p.Product.Description,
                        ProductId = p.ProductId,
                        ProductTechnicalNumber = p.Product.TechnicalNumber,
                        ProductUnit = p.Product.Unit,
                        ProductCode = p.Product.ProductCode,
                        ProductGroupId = p.Product.ProductGroupId,
                        ChildBoms = p.ChildBom.Where(x => !x.IsDeleted).Select(b => new ListBomInfoDto
                        {
                            Id = b.Id,
                            ProductId = b.ProductId,
                            ProductDescription = b.Product.Description,
                            ProductUnit = b.Product.Unit,
                            ProductTechnicalNumber = b.Product.TechnicalNumber,
                            ProductCode = b.Product.ProductCode,
                            ProductGroupId = b.Product.ProductGroupId,
                            CoefficientUse = b.CoefficientUse,
                            MaterialType = b.MaterialType,
                            ParentBomId = b.ParentBomId,
                            ChildBoms = b.ChildBom.Where(c => !c.IsDeleted).Select(c => new ListBomInfoDto
                            {
                                Id = c.Id,
                                ProductId = c.ProductId,
                                ProductDescription = c.Product.Description,
                                ProductUnit = c.Product.Unit,
                                ProductTechnicalNumber = c.Product.TechnicalNumber,
                                ProductCode = c.Product.ProductCode,
                                ProductGroupId = c.Product.ProductGroupId,
                                CoefficientUse = c.CoefficientUse,
                                MaterialType = c.MaterialType,
                                ParentBomId = c.ParentBomId,
                            }).ToList()
                        }).ToList(),
                    }).ToListAsync();

                if (bomModels == null || !bomModels.Any())
                    return ServiceResultFactory.CreateSuccess(new List<ListBomInfoDto>());

                var result = new List<ListBomInfoDto>();
                foreach (var item in bomModels)
                {

                    if (item.ChildBoms == null || item.ChildBoms.Count() == 0)
                        result.Add(item);

                    var LastChilds = item.ChildBoms.Where(a => a.ChildBoms != null && a.ChildBoms.Count() > 0)
                        .SelectMany(a => a.ChildBoms)
                        .ToList();

                    if (LastChilds != null && LastChilds.Any())
                        result.AddRange(LastChilds);

                    var parentWithoutChilds = item.ChildBoms
                        .Where(a => a.ChildBoms == null || a.ChildBoms.Count() == 0)
                        .ToList();

                    if (parentWithoutChilds != null && parentWithoutChilds.Any())
                        result.AddRange(parentWithoutChilds);
                }
                ///اگه زیرمجموعه نداشت خودشو برگردون


                return ServiceResultFactory.CreateSuccess(result);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<ListBomInfoDto>>(null, exception);
            }
        }
        public async Task<ServiceResult<List<ListBomInfoDto>>> GetBomProductForDocumentByContractCodeForCustomerUserAsync(AuthenticateDto authenticate, bool accessability)
        {
            try
            {
                if (!accessability)
                    return ServiceResultFactory.CreateError<List<ListBomInfoDto>>(null, MessageId.AccessDenied);

               
               
                var bomModels = await _bomProductRepository
                    .AsNoTracking()
                    .Where(c => !c.IsDeleted && c.ParentBomId == null && c.Product.ContractCode == authenticate.ContractCode)
                    .Select(p => new ListBomInfoDto
                    {
                        Id = p.Id,
                        ProductDescription = p.Product.Description,
                        ProductId = p.ProductId,
                        ProductTechnicalNumber = p.Product.TechnicalNumber,
                        ProductUnit = p.Product.Unit,
                        ProductCode = p.Product.ProductCode,
                        ProductGroupId = p.Product.ProductGroupId,
                        ChildBoms = p.ChildBom.Where(x => !x.IsDeleted).Select(b => new ListBomInfoDto
                        {
                            Id = b.Id,
                            ProductId = b.ProductId,
                            ProductDescription = b.Product.Description,
                            ProductUnit = b.Product.Unit,
                            ProductTechnicalNumber = b.Product.TechnicalNumber,
                            ProductCode = b.Product.ProductCode,
                            ProductGroupId = b.Product.ProductGroupId,
                            CoefficientUse = b.CoefficientUse,
                            MaterialType = b.MaterialType,
                            ParentBomId = b.ParentBomId,
                            ChildBoms = b.ChildBom.Where(c => !c.IsDeleted).Select(c => new ListBomInfoDto
                            {
                                Id = c.Id,
                                ProductId = c.ProductId,
                                ProductDescription = c.Product.Description,
                                ProductUnit = c.Product.Unit,
                                ProductTechnicalNumber = c.Product.TechnicalNumber,
                                ProductCode = c.Product.ProductCode,
                                ProductGroupId = c.Product.ProductGroupId,
                                CoefficientUse = c.CoefficientUse,
                                MaterialType = c.MaterialType,
                                ParentBomId = c.ParentBomId,
                            }).ToList()
                        }).ToList(),
                    }).ToListAsync();

                if (bomModels == null || !bomModels.Any())
                    return ServiceResultFactory.CreateSuccess(new List<ListBomInfoDto>());

                var result = new List<ListBomInfoDto>();
                foreach (var item in bomModels)
                {

                    if (item.ChildBoms == null || item.ChildBoms.Count() == 0)
                        result.Add(item);

                    var LastChilds = item.ChildBoms.Where(a => a.ChildBoms != null && a.ChildBoms.Count() > 0)
                        .SelectMany(a => a.ChildBoms)
                        .ToList();

                    if (LastChilds != null && LastChilds.Any())
                        result.AddRange(LastChilds);

                    var parentWithoutChilds = item.ChildBoms
                        .Where(a => a.ChildBoms == null || a.ChildBoms.Count() == 0)
                        .ToList();

                    if (parentWithoutChilds != null && parentWithoutChilds.Any())
                        result.AddRange(parentWithoutChilds);
                }
                ///اگه زیرمجموعه نداشت خودشو برگردون


                return ServiceResultFactory.CreateSuccess(result);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<ListBomInfoDto>>(null, exception);
            }
        }
    }
}
