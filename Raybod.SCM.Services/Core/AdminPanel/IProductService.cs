using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Bom;
using Raybod.SCM.DataTransferObject.MasterMrpReport;
using Raybod.SCM.DataTransferObject.Product;
using Raybod.SCM.Services.Core.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Core
{
    public interface IProductService
    {
        Task<ServiceResult<List<string>>> GetProductUnitAsync(AuthenticateDto authenticate);

        Task<ServiceResult<List<ProductMiniInfo>>> GetProductMiniInfoWithoutLimitedPermisionGroupAsync(AuthenticateDto authenticate, ProductQuery query);
        Task<ServiceResult<bool>> AddProductUnitAsync(AuthenticateDto authenticate, string unit);

        Task<ServiceResult<bool>> DeleteProductUnitAsync(AuthenticateDto authenticate, string unit);

        Task<ServiceResult<BaseProductDto>> AddProductAsync(AuthenticateDto authenticate, int productGroupId, AddProductDto model);
        Task<ServiceResult<List<MasterMrProductListDto>>> AddProductWithBomAsync(AuthenticateDto authenticate, int productGroupId, List<AddProductWithBomDto> model);
        Task<ServiceResult<List<MasterMrProductListDto>>> AddSubsetProductAsync(AuthenticateDto authenticate, int productId, List<AddProductSubsetDto> model);
        Task<ServiceResult<List<BaseProductDto>>> AddProductAsync(AuthenticateDto authenticate, int productGroupId, List<AddProductDto> model);

        //Task<ServiceResult<bool>> EditProductAsync(AuthenticateDto authenticate, int productGroupId, int productId, AddProductDto model);
        Task<ServiceResult<bool>> EditProductAsync(AuthenticateDto authenticate, int productId, EditProductDto model);

        Task<ServiceResult<string>> GenerateProductCode(AuthenticateDto authenticate, int productGroupId);

        Task<ServiceResult<List<BaseProductDto>>> GetProductAsync(AuthenticateDto authenticate, long productGroupId, ProductQuery query);

        Task<ServiceResult<List<ProductMiniInfo>>> GetProductMiniInfoAsync(AuthenticateDto authenticate, ProductQuery query);

        Task<ServiceResult<List<ProductMiniInfo>>> GetProductMiniInfoAsync(AuthenticateDto authenticate, string query, List<int> productGroupIds);

        Task<ServiceResult<List<ProductMiniInfo>>> GetProductMiniInfoWithPaginationAsync(AuthenticateDto authenticate, ProductQuery query);

        Task<ServiceResult<BaseProductDto>> GetProductByIdAsync(AuthenticateDto authenticate, int productId);

        Task<ServiceResult<bool>> RemoveProductAsync(AuthenticateDto authenticate, int productId);

        Task<ServiceResult<bool>> AddProductImageAsync(AuthenticateDto authenticate, int ProductId, string imageName);
        Task<ServiceResult<GetProductInfoForAddSubsetDto>> GetProductInfoAsync(AuthenticateDto authenticate, int productId);
        //Task<ServiceResult<GetDuplicateInfoDto>> GetDuplicateInfoAsync(AuthenticateDto authenticate, int productId);
        //Task<ServiceResult<List<MasterMrProductListDto>>> AddDuplicateAsync(AuthenticateDto authenticate, int productId, CreateDuplicateDto model);
        Task<ServiceResult<List<GetProductListInfoDto>>> GetProductListInfoAsync(AuthenticateDto authenticate, int productGroupId);
        Task<ServiceResult<List<EditBomProductInfoDto>>> GetBomProductByProductIdAsync(AuthenticateDto authenticate, int productId);
        Task<ServiceResult<BomProductEditResultDto>> EditBomProductAsync(AuthenticateDto authenticate, long bomId, CreateBomFormAnotherBomDto model);
        //Task<ServiceResult<bool>> AddBomProductsAsync(AuthenticateDto authenticate, long bomId, CreateBomFormAnotherBomDto model);
        Task<ServiceResult<BomProductArchiveDto>> GetBomProductArchiveByBomIdAsync(AuthenticateDto authenticate, long bomProductId);
    }
}
