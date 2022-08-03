using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Product;
using Raybod.SCM.DataTransferObject.User;
using Raybod.SCM.DataTransferObject.Warehouse;
using Raybod.SCM.Services.Core.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Core
{
    public interface IWarehouseService
    {
        Task<ServiceResult<BaseWarehouseDto>> AddWarehouseAsync(AuthenticateDto authenticate, AddWarehouseDto model);

        Task<ServiceResult<BaseWarehouseDto>> EditWarehouseAsync(AuthenticateDto authenticate, int warehouseId, AddWarehouseDto model);

        Task<ServiceResult<List<BaseWarehouseDto>>> GetWarehouseListAsync(AuthenticateDto authenticate, WarehouseQueryDto query);

        Task<ServiceResult<BaseWarehouseDto>> GetWarehouseByIdAsync(AuthenticateDto authenticate, int warehouseId);

        Task<ServiceResult<bool>> RemoveWarehouseAsync(AuthenticateDto authenticate, int warehouseId);

        Task<ServiceResult<List<WarehouseProductDto>>> GetWarehouseProductAsync(AuthenticateDto authenticate, WarehouseProductQueryDto query);

        Task<DownloadFileDto> ExportExcelWarehouseProductAsync(AuthenticateDto authenticate, WarehouseProductQueryDto query);

        Task<DownloadFileDto> GetWarehouseProductLogsAsync(AuthenticateDto authenticate, int productId, WarehouseProductLogQueryDto query);
        Task<ServiceResult<PendingForConfirmWarehouseOutputRequestDto>> AddWarehouseOutputRequest(AuthenticateDto authenticate, AddWarehouseOutputRequestDto model);
        Task<ServiceResult<WarehouseRequestConfirmationWorkflowDto>> GetPendingConfirmWarehouseOutputRequestByPurchaseRequestIdAsync(AuthenticateDto authenticate, long warehouseOutputRequestId);
        Task<ServiceResult<List<PendingForConfirmWarehouseOutputRequestDto>>> SetUserConfirmOwnWarehouseOutputRequestTaskAsync(AuthenticateDto authenticate, long warehouseRequestId, AddWarehouseRequestConfirmationAnswerDto model);
        Task<ServiceResult<List<PendingForConfirmWarehouseOutputRequestDto>>> GetPendingwarehouseRequisitionListAsync(AuthenticateDto authenticate, WarehouseOutputQueryDto query);
        Task<ServiceResult<List<GetProductListInfoDto>>> GetWarehouseProductListInfoAsync(AuthenticateDto authenticate, int productGroupId);
        Task<ServiceResult<List<UserMentionDto>>> GetConfirmationUserListAsync(AuthenticateDto authenticate,int productGroupId);
        Task<ServiceResult<List<WarehouseOutputRequestListDto>>> GetWarehouseRequisitionListAsync(AuthenticateDto authenticate, WarehouseOutputQueryDto query);
        Task<ServiceResult<WarehouseOutputRequestDetailsDto>> GetWarehouseRequisitionByRequestIdAsync(AuthenticateDto authenticate, long requestId);
        Task<ServiceResult<List<PendingForConfirmWarehouseOutputRequestDto>>> GetConfrimWarehouseRequisitionListAsync(AuthenticateDto authenticate, WarehouseOutputQueryDto query);
        Task<ServiceResult<WarehouseOutputRequestDespatchInfoDto>> GetWaitingRequestForDespatchInfoByRequestIdAsync(AuthenticateDto authenticate, long requestId);
        Task<ServiceResult<bool>> AddWarehouseDespatchAsync(AuthenticateDto authenticate, long requestId, AddWarehouseDespatchDto model);
        Task<ServiceResult<bool>> CancelWarehouseDespatchAsync(AuthenticateDto authenticate, long requestId);
        Task<ServiceResult<List<WarehouseDespatchListDto>>> GetWarehouseDespatchListAsync(AuthenticateDto authenticate, WarehouseDespatchQueryDto query);
        Task<ServiceResult<WarehouseDespatchDetailDto>> GetWarehouseDespatchByDespatchIdAsync(AuthenticateDto authenticate, long despatchId);
    }
}
