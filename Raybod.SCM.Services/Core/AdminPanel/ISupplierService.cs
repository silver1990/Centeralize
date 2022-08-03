using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Supplier;
using Raybod.SCM.Services.Core.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Core
{
    public interface ISupplierService
    {
        Task<ServiceResult<List<SupplierMiniInfoDto>>> GetSuppliersListAsync(AuthenticateDto authenticate);
        Task<ServiceResult<List<SupplierMiniInfoDto>>> GetWinnerRFPSuppliersAsync(AuthenticateDto authenticate);
        Task<ServiceResult<ListSupplierDto>> AddSupplierAsync(AuthenticateDto authenticate, AddSupplierDto model);

        Task<ServiceResult<List<SupplierMiniInfoDto>>> GetSuppliersAsync(AuthenticateDto authenticate, SupplierQuery query);

        Task<ServiceResult<List<ListSupplierDto>>> GetSupplierAsync(AuthenticateDto authenticate, SupplierQuery query);

        Task<ServiceResult<ListSupplierDto>> EditSupplierAsync(AuthenticateDto authenticate, int supplierId, AddSupplierDto model);

        Task<ServiceResult<List<SupplierMiniInfoDto>>> GetSupplierOfSupportThisProductIdAsync(int productId);

        Task<ServiceResult<List<SupplierMiniInfoDto>>> GetSupplierOfSupportThisProductGroupsAsync(AuthenticateDto authenticate, int productGroupId);

        Task<ServiceResult<bool>> DeleteSupplierAsync(AuthenticateDto authenticate, int id);

        Task<ServiceResult<BaseSupplierUserDto>> AddSupplierUserAsync(AuthenticateDto authenticate, int supplierId, AddSupplierUserDto model);

        Task<ServiceResult<List<BaseSupplierUserDto>>> GetSupplierUserAsync(AuthenticateDto authenticate, int supplierId);

        Task<ServiceResult<bool>> DeleteSupplierUserByIdAsync(AuthenticateDto authenticate, int supplierId, int supplierUserId);

    }
}
