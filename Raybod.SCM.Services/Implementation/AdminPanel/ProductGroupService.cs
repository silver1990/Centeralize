using Microsoft.EntityFrameworkCore;
using Raybod.SCM.DataAccess.Core;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.ProductGroup;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Model;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Utility.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Implementation
{
    public class ProductGroupService : IProductGroupService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITeamWorkAuthenticationService _authenticationService;
        private readonly DbSet<ProductGroup> _productGroupRepository;

        public ProductGroupService(IUnitOfWork unitOfWork, ITeamWorkAuthenticationService authenticationService)
        {
            _unitOfWork = unitOfWork;
            _authenticationService = authenticationService;
            _productGroupRepository = _unitOfWork.Set<ProductGroup>();
        }

        public async Task<ServiceResult<ListProductGroupDto>> AddProductGroupAsync(AuthenticateDto authenticate, AddProductGroupDto model)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<ListProductGroupDto>(null, MessageId.AccessDenied);

                var dbQuery = _productGroupRepository.AsQueryable();

                if (await dbQuery.AnyAsync(a => a.ProductGroupCode == model.ProductGroupCode))
                    return ServiceResultFactory.CreateError<ListProductGroupDto>(null, MessageId.CodeExist);

                if (await dbQuery.AnyAsync(a => !a.IsDeleted && a.Title == model.Title))
                    return ServiceResultFactory.CreateError<ListProductGroupDto>(null, MessageId.DuplicateInformation);

                var productGroupModel = new ProductGroup
                {
                    Title = model.Title,
                    ProductGroupCode = model.ProductGroupCode
                };

                _productGroupRepository.Add(productGroupModel);

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var result = new ListProductGroupDto
                    {
                        ProductGroupCode = productGroupModel.ProductGroupCode,
                        Id = productGroupModel.Id,
                        Title = productGroupModel.Title,
                        ProductCount = 0

                    };
                    return ServiceResultFactory.CreateSuccess(result);
                }

                return ServiceResultFactory.CreateError<ListProductGroupDto>(null, MessageId.InternalError);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<ListProductGroupDto>(null, exception);
            }
        }

        public async Task<ServiceResult<BaseProductGroupDto>> GetProductGroupByIdAsync(AuthenticateDto authenticate, int productGroupId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<BaseProductGroupDto>(null, MessageId.AccessDenied);

                var dbQuery = _productGroupRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.Id == productGroupId);

                if (!dbQuery.Any())
                    return ServiceResultFactory.CreateError<BaseProductGroupDto>(null, MessageId.EntityDoesNotExist);

                var result = await dbQuery.Select(c => new BaseProductGroupDto
                {
                    ProductGroupCode = c.ProductGroupCode,
                    Id = c.Id,
                    Title = c.Title
                }).FirstOrDefaultAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<BaseProductGroupDto>(null, exception);
            }
        }

        public async Task<ServiceResult<List<ListProductGroupDto>>> GetProductGroupAsync(AuthenticateDto authenticate, ProductGroupQuery query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<ListProductGroupDto>>(null, MessageId.AccessDenied);

                var dbQuery = _productGroupRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted);

                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.ProductGroupIds.Contains(a.Id));

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a => a.ProductGroupCode.Contains(query.SearchText) || a.Title.Contains(query.SearchText));

                var result = await dbQuery.Select(c => new ListProductGroupDto
                {
                    Id = c.Id,
                    ProductGroupCode = c.ProductGroupCode,
                    Title = c.Title,
                    ProductCount = c.Products.Count(a => !a.IsDeleted)
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<ListProductGroupDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<List<BaseProductGroupDto>>> GetProductGroupAsync(AuthenticateDto authenticate, string query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<BaseProductGroupDto>>(null, MessageId.AccessDenied);

                var dbQuery = _productGroupRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted);

                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.ProductGroupIds.Contains(a.Id));

                if (!string.IsNullOrEmpty(query))
                    dbQuery = dbQuery.Where(a => a.ProductGroupCode.Contains(query) || a.Title.Contains(query));


                var result = await dbQuery.Select(c => new BaseProductGroupDto
                {
                    Id = c.Id,
                    ProductGroupCode = c.ProductGroupCode,
                    Title = c.Title,
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<BaseProductGroupDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<List<BaseProductGroupDto>>> GetProductGroupListWithoutLimitedBypermissionProductGroupAsync(AuthenticateDto authenticate, string query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<BaseProductGroupDto>>(null, MessageId.AccessDenied);

                var dbQuery = _productGroupRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted);

                if (!string.IsNullOrEmpty(query))
                    dbQuery = dbQuery.Where(a => a.ProductGroupCode.Contains(query) || a.Title.Contains(query));


                var result = await dbQuery.Select(c => new BaseProductGroupDto
                {
                    Id = c.Id,
                    ProductGroupCode = c.ProductGroupCode,
                    Title = c.Title,
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<BaseProductGroupDto>>(null, exception);
            }
        }


       
        public async Task<ServiceResult<bool>> EditProductGroupByIdAsync(AuthenticateDto authenticate, int productGroupId, AddProductGroupDto model)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var groupModel = await _productGroupRepository
                    .FirstOrDefaultAsync(a => !a.IsDeleted && a.Id == productGroupId);

                if (groupModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                var dbQuery = _productGroupRepository.AsQueryable();

                if (groupModel.ProductGroupCode != model.ProductGroupCode)
                {

                    if (await dbQuery.AnyAsync(a => a.Id != productGroupId && a.ProductGroupCode == model.ProductGroupCode))
                        return ServiceResultFactory.CreateError(false, MessageId.CodeExist);
                }

                if (groupModel.Title != model.Title)
                {
                    if (await dbQuery.AnyAsync(a => !a.IsDeleted && a.Id != productGroupId && a.Title == model.Title))
                        return ServiceResultFactory.CreateError(false, MessageId.DuplicateInformation);
                }

                groupModel.ProductGroupCode = model.ProductGroupCode;
                groupModel.Title = model.Title;

                await _unitOfWork.SaveChangesAsync();
                return ServiceResultFactory.CreateSuccess(true);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<bool>> DeleteProductGroupAsync(AuthenticateDto authenticate, int productGroupId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var groupModel = await _productGroupRepository
                    .Where(a => !a.IsDeleted && a.Id == productGroupId)
                    .FirstOrDefaultAsync();

                if (await _productGroupRepository.AnyAsync(a => a.Id == productGroupId && a.Products.Any(c => !c.IsDeleted)))
                    return ServiceResultFactory.CreateError(false, MessageId.DeleteDontAllowedBeforeSubset);

                if (groupModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                groupModel.IsDeleted = true;

                await _unitOfWork.SaveChangesAsync();
                return ServiceResultFactory.CreateSuccess(true);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }
        public async Task<ServiceResult<List<ProductGroupMiniInfoDto>>> GetProductGroupMiniInfoAsync(AuthenticateDto authenticate)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<ProductGroupMiniInfoDto>>(null, MessageId.AccessDenied);

                var dbQuery = _productGroupRepository
                    .Where(a => !a.IsDeleted);

                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.ProductGroupIds.Contains(a.Id));

                var result = await dbQuery.Select(a => new ProductGroupMiniInfoDto
                {
                    ProductGroupId = a.Id,
                    ProductGroupTitle = a.Title
                }).ToListAsync();
                return ServiceResultFactory.CreateSuccess(result);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<ProductGroupMiniInfoDto>>(null, exception);
            }
        }
    }
}
