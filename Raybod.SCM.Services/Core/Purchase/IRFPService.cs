using Raybod.SCM.DataTransferObject.RFP;
using Raybod.SCM.Services.Core.Common;
using System.Collections.Generic;
using System.Threading.Tasks;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.DataTransferObject.Supplier;

namespace Raybod.SCM.Services.Core
{
    public interface IRFPService
    {
        Task<RFPListBadgeCountDto> GetDashbourdRFPListBadgeCountAsync(AuthenticateDto authenticate);

        Task<ServiceResult<RFPListBadgeCountDto>> GetRFPListBadgeCountAsync(AuthenticateDto authenticate);

        Task<ServiceResult<long>> AddRFPAsync(AuthenticateDto authenticate, int productGroupId, AddRFPDto model);
        Task<ServiceResult<List<RFPItemInfoDto>>> RFPItemsEditAsync(AuthenticateDto authenticate, long rfpId, List<AddRFPItemDto> model);
        Task<ServiceResult<List<RFPDataForPRContractDto>>> GetInProgressRFPAsync(AuthenticateDto authenticate, RFPQueryDto query);

        Task<ServiceResult<List<ListRFPDto>>> GetRFPAsync(AuthenticateDto authenticate, RFPQueryDto query);

        Task<ServiceResult<RFPInfoDto>> GetRFPByIdAsync(AuthenticateDto authenticate, long rfpId);

        Task<ServiceResult<RFPInfoDto>> GetRFPDetailsByIdAsync(AuthenticateDto authenticate, long rfpId);
        Task<ServiceResult<SupplierEvaluationProposalInfoDto>> GetRFPEvaluationAsync(AuthenticateDto authenticate, long rfpId);

        Task<ServiceResult<List<RFPInqueryInfoDto>>> GetRFPInqueryByRFPIdAsync(AuthenticateDto authenticate, long rfpId, RFPInqueryType inqueryType);

        Task<ServiceResult<bool>> DeActiveRFPItemByIdAsync(AuthenticateDto authenticate, long rfpId, long rfpItemId);

        Task<ServiceResult<List<RFPItemInfoDto>>> AddRFPItemByRFPIdAsync(AuthenticateDto authenticate, long rfpId, List<AddRFPItemDto> model);
        Task<ServiceResult<List<RFPSupplierInfoDto>>> EditRFPInqueryByRFPIdAsync(AuthenticateDto authenticate, long rfpId, RFPInqueryType inqueryType, List<RFPInqueryInfoDto> model);

        Task<ServiceResult<bool>> DeActiveRFPSupplierBySupplierIdAsync(AuthenticateDto authenticate, long rfpId, int supplierId);

        Task<ServiceResult<List<RFPSupplierInfoDto>>> EditRFPSupplierAsync(AuthenticateDto authenticate, long rfpId, List<AddRFPSupplierDto> supplierIds);

        Task<ServiceResult<SetProposalWinnerDto>> SetRFPSupplierWinner(AuthenticateDto authenticate, long rfpId, List<int> supplierIds);

        Task<ServiceResult<RFPStatus>> GetRFPStatusByRFPIdAsync(AuthenticateDto authenticate, long rfpId);

        Task<ServiceResult<RFPEvaluationProposalInfoDto>> GetRFPSupplierInqueryAsync(AuthenticateDto authenticate, long rfpId, long SupplierId, RFPInqueryType inqueryType);

        Task<ServiceResult<AddOrEditSupplierProposalDto>> AddRFPSupplierProposalAsync(AuthenticateDto authenticate,long rfpId, int supplierId, RFPInqueryType inqueryType, List<AddSupplierProposalDto> model);

        Task<ServiceResult<AddOrEditSupplierProposalDto>> AddSupplierEvaluationProposalAsync(AuthenticateDto authenticate, long rfpId,int supplierId, RFPInqueryType inqueryType,AddRFPSupplierEvaluationDto model);

        Task<ServiceResult<int>> GetWaitingRFPItemForCreatePRContractBadgeAsync(AuthenticateDto authenticate);

        Task<ServiceResult<List<WaitingRFPItemForAddPrContractDto>>> GetWaitingRFPItemsForCreatePrContractAsync(AuthenticateDto authenticate, RFPItemQueryDto query,long? prContractId);

        Task<ServiceResult<List<RFPDataForPRContractDto>>> GetRFPForCreatePRContractAsync(AuthenticateDto authenticate, RFPQueryDto query);

        Task<ServiceResult<List<RFPItemInfoDto>>> GetRFPItemsByRFPIdForCreatePrContractAsync(AuthenticateDto authenticate, long rfpId);
               
        Task<ServiceResult<List<ListSupplierDto>>> GetWinnerSupplierRFPByRFPIdAsync(AuthenticateDto authenticate,
          long rfpId);

        Task<DownloadFileDto> DownloadRFPAttachmentAsync(AuthenticateDto authenticate, long rfpId, string fileSrc);

        Task<DownloadFileDto> DownloadRFPSupplierProposalAttachmentAsync(AuthenticateDto authenticate, long proposalId, string fileSrc);

        Task<DownloadFileDto> DownloadRFPInqueryAttachmentAsync(AuthenticateDto authenticate, long rfpId, long inqueryId, RFPInqueryType inqueryType, string fileSrc);
        Task<ServiceResult<AddOrEditProFormaDto>> AddRFPSupplierProFormaAsync(AuthenticateDto authenticate, long rfpId, long rfpSupplierId, AddRFPProFromaDto model);
        Task<ServiceResult<RFPProFormDetailDto>> GetRFPProFormaDetailAsync(AuthenticateDto authenticate, long rfpId, long rfpSupplierId);
        Task<ServiceResult<bool>> CancelRFPAsync(AuthenticateDto authenticate, long rfpId);
        Task<ServiceResult<AddOrEditProFormaDto>> EditRFPSupplierProFormaAsync(AuthenticateDto authenticate, long rfpSupplierId, long rfpProformaId, AddRFPProFromaDto model);
        Task<ServiceResult<RFPSupplierProFormListDto>> GetRFPProFormaListAsync(AuthenticateDto authenticate, long rfpId);
        Task<DownloadFileDto> DownloadRFPProFormaAttachmentAsync(AuthenticateDto authenticate, long rfpSupplierId, long proformaId, string fileSrc);
        Task<ServiceResult<AddOrEditProFormaDto>> DeleteProFormaAsync(AuthenticateDto authenticate, long rfpSupplierId, long proFormaId);
    }
}