using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.ProductGroup;
using Raybod.SCM.Services.Core.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Core
{
    public interface IProductGroupService
    {
        Task<ServiceResult<ListProductGroupDto>> AddProductGroupAsync(AuthenticateDto authenticate, AddProductGroupDto model);

        Task<ServiceResult<BaseProductGroupDto>> GetProductGroupByIdAsync(AuthenticateDto authenticate, int productGroupId);

        Task<ServiceResult<List<ListProductGroupDto>>> GetProductGroupAsync(AuthenticateDto authenticate, ProductGroupQuery query);

        Task<ServiceResult<List<BaseProductGroupDto>>> GetProductGroupListWithoutLimitedBypermissionProductGroupAsync(AuthenticateDto authenticate, string query);

        Task<ServiceResult<List<BaseProductGroupDto>>> GetProductGroupAsync(AuthenticateDto authenticate, string query);



        Task<ServiceResult<bool>> EditProductGroupByIdAsync(AuthenticateDto authenticate, int productGroupId, AddProductGroupDto model);

        Task<ServiceResult<bool>> DeleteProductGroupAsync(AuthenticateDto authenticate, int productGroupId);
        Task<ServiceResult<List<ProductGroupMiniInfoDto>>> GetProductGroupMiniInfoAsync(AuthenticateDto authenticate);

    }
}
