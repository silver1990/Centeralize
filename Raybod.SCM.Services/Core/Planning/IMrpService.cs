using Microsoft.AspNetCore.Http;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Mrp;
using Raybod.SCM.DataTransferObject.MrpItem;
using Raybod.SCM.DataTransferObject.PurchaseRequest;
using Raybod.SCM.Services.Core.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Core
{
    public interface IMrpService
    {

        Task<ServiceResult<string>> AddMrpAsync(AuthenticateDto authenticate, string contractCode, int productGroupId, List<AddMrpItemDto> model);

        Task<ServiceResult<List<MrpInfoDto>>> GetMrpAsync(AuthenticateDto authenticate, MrpQuery query);

        Task<ServiceResult<List<MrpItemInfoDto>>> GetMrpItemsByMrpIdAsync(AuthenticateDto authenticate, long mrpId, MrpQuery query);

        Task<ServiceResult<MrpForEditDto>> GetMrpByMrpIdForEditAsync(AuthenticateDto authenticate, long mrpId);

        Task<ServiceResult<bool>> EditMrpAsync(AuthenticateDto authenticate, long mrpId, List<AddMrpItemDto> model);

        //Task<ServiceResult<List<MrpMiniInfoDto>>> GetMrpBySearchAsync(AuthenticateDto authenticate, MrpQuery query);

        //Task<ServiceResult<MrpInfoWithMrpItemDto>> GetMrpByIdIncludeMrpItemAsync(AuthenticateDto authenticate, long mrpId);

        Task<ServiceResult<List<ExportMRPToExcelDto>>> ReadExcelFileAsync(AuthenticateDto authenticate, string contractCode, IFormFile formFile, bool isPersianDate);

        Task<DownloadFileDto> ExportMasterMRAsync(AuthenticateDto authenticate, int productGroupId, MasterMRQueryDto query);

        Task<ServiceResult<List<WaitingMrpForNewPRDto>>> GetWaitingMrpForNewPrAsync(AuthenticateDto authenticate, MrpQuery query);

        Task<ServiceResult<MrpInfoDto>> GetWaitingMrpByIdForNewPrAsync(AuthenticateDto authenticate, long mrpId);

        Task<ServiceResult<List<MrpPurchaseRequestItemDto>>> GetWaitingMrpItemsByMrpIdAsync(AuthenticateDto authenticate, long mrpId, MrpQuery query);
        Task<ServiceResult<List<MrpPurchaseRequestItemDto>>> GetWaitingMrpItemsByPurchaseRequestIdAsync(AuthenticateDto authenticate, long purchaseRequestId, MrpQuery query);
    }
}
