using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Bom;
using Raybod.SCM.DataTransferObject.Product;
using Raybod.SCM.Services.Core.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Core
{
    public interface IBomProductService
    {
        // update master mr for test

        

        

        Task<ServiceResult<ProductMiniInfo>> GetProductByProductIdAsync(int productId);

        Task<ServiceResult<List<ProductMiniInfo>>> GetProductForCreateBomAsync(string query);



        Task<ServiceResult<bool>> EditBomProductAsync(AuthenticateDto authenticate, long bomId, List<ListBomInfoDto> model);

        Task<ServiceResult<List<BomWithChildInfo>>> GetBomAsync(AuthenticateDto authenticate, BomQueryDto query);

        Task<ServiceResult<ListBomInfoDto>> GetBomProductByIdAsync(long bomId);

        Task<ServiceResult<ListBomInfoDto>> GetBomProductByProductIdAsync(int productId);

        Task<ServiceResult<List<BomInfoDto>>> GetChildBomByBomIdAsync(long parentBomId);

        Task<ServiceResult<BomWithChildInfo>> GetBomProductByIdIncludeChildAsync(AuthenticateDto authenticate, long bomId);

        Task<ServiceResult<bool>> RemoveBomAsync(AuthenticateDto authenticate, long bomId);

        Task<ServiceResult<List<ListBomInfoDto>>> GetAllProductGroupIdsOfbomByBomIdAsync(long bomId);

        Task<ServiceResult<List<int>>> GetLastChildProductIdsOfbomByProductIdAsync(List<int> productIds);
        Task<ServiceResult<List<int>>> GetLastChildProductIdsOfContractbomAsync(string contractCode);

        //Task<ServiceResult<List<ListBomInfoDto>>> GetBomProductForDocumentByProductIdAsync(AuthenticateDto authenticate, int productId);
        Task<ServiceResult<List<ListBomInfoDto>>> GetBomProductForDocumentByContractCodeAsync(AuthenticateDto authenticate);
        Task<ServiceResult<List<ListBomInfoDto>>> GetBomProductForDocumentByContractCodeForCustomerUserAsync(AuthenticateDto authenticate,bool accessability);

    }
}
