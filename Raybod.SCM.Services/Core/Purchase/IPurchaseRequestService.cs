using Raybod.SCM.DataTransferObject.PurchaseRequest;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Services.Core.Common;
using System.Collections.Generic;
using System.Threading.Tasks;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.User;
using Raybod.SCM.DataTransferObject._panelPurchase.PurchaseRequestConfirmation;

namespace Raybod.SCM.Services.Core
{
    public interface IPurchaseRequestService
    {
        Task<PRWaitingListBadgeCountDto> GetDashbourdWaitingListBadgeCountAsync(AuthenticateDto authenticate);

        Task<ServiceResult<PRWaitingListBadgeCountDto>> GetWaitingListBadgeCountAsync(AuthenticateDto authenticate, List<string> registerRoles, List<string> confirmRoles);

        Task<ServiceResult<bool>> AddPurchaseRequestAsync(AuthenticateDto authenticate, AddPurchaseRequestDto model);

        Task<ServiceResult<List<ListPendingForConfirmPurchaseRequestDto>>> GetPendingForConfirmPurchaseRequestAsync(AuthenticateDto authenticate, PurchaseRequestQueryDto query);

        Task<ServiceResult<ListPendingForConfirmPurchaseRequestDto>> GetPendingForConfirmPurchaseRequestByIdAsync(AuthenticateDto authenticate, long purchaseRequsetId);

        Task<ServiceResult<List<ListPurchaseRequestDto>>> GetPurchaseRequestAsync(AuthenticateDto authenticate, PurchaseRequestQueryDto query);

        Task<ServiceResult<PurchaseRequestInfoDto>> GetPurchaseRequestByIdAsync(AuthenticateDto authenticate, long purchaseRequestId);
        Task<ServiceResult<PurchaseRequestEditInfoDto>> GetPurchaseRequestByIdForEditAsync(AuthenticateDto authenticate, long purchaseRequestId);
        Task<DownloadFileDto> DownloadPRAttachmentAsync(AuthenticateDto authenticate, long purchaseRequestId, string fileSrc);
        Task<DownloadFileDto> DownloadPRWorkFlowAttachmentAsync(AuthenticateDto authenticate, long purchaseRequestId, string fileSrc);
        Task<ServiceResult<List<PurchaseRequestItemInfoDto>>> GetPurchaseRequestItemsByPRIdAsync(AuthenticateDto authenticate,long purchaseRequestId);

        Task<ServiceResult<bool>> EditPurchaseRequestAsync(AuthenticateDto authenticate,long purchaseRequestId, EditPurchaseRequestDto model);

        Task<ServiceResult<bool>> ConfirmPurchaseRequestAsync(AuthenticateDto authenticate,long purchaseRequestId, AddPrConfirmDto model);

        Task<ServiceResult<bool>> RejectPurchaseRequestAsync(AuthenticateDto authenticate,long purchaseRequestId, AddPrConfirmDto model);

        //Task<ServiceResult<bool>> DeletePurchaseRequestAsync(AuthenticateDto authenticate,
        //     long purchaseRequestId);

        //Task<ServiceResult<List<PurchaseRequestMiniInfoDto>>> GetPurchaseRequestMiniInfoAsync(AuthenticateDto authenticate,
        //    PurchaseRequestQueryDto query);

        Task<ServiceResult<bool>> SetStatusPurchaseRequestAsync(AuthenticateDto authenticate, long purchaseRequestId, PRStatus status);


        #region rfp

        Task<ServiceResult<List<WaitingPRForNewRFPListDto>>> GetWaitingPRForNewRFPListAsync(AuthenticateDto authenticate, PurchaseRequestQueryDto query);

        Task<ServiceResult<PRForNewRFPDto>> GetWaitingPRForAddRFPByPRIdAsync(AuthenticateDto authenticate, long prId);

        Task<ServiceResult<List<PRItemsForNewRFPDTO>>> GetWaitingPRItemForAddRFPByProductGroupIdAsync(AuthenticateDto authenticate, int productGroupId,
            PurchaseRequestQueryDto query);

        #endregion

        Task<ServiceResult<List<UserMentionDto>>> GetConfirmationUserListAsync(AuthenticateDto authenticate, long purchaseId);
        
        Task<ServiceResult<ListPendingForConfirmPurchaseRequestDto>> SetUserConfirmOwnPurchaseRequestTaskAsync(AuthenticateDto authenticate, long purchaseRequsetId, AddPurchaseRequestConfirmationAnswerDto model);
        Task<ServiceResult<PurchaseRequestConfirmationWorkflowDto>> GetPendingConfirmPurchaseByPurchaseRequestIdAsync(AuthenticateDto authenticate, long purchaseRequestId);
        Task<ServiceResult<bool>> EditPurchaseRequestBySysAdminAsync(AuthenticateDto authenticate, long purchaseRequestId, EditPurchaseRequestBySysAdminDto model);
    }
}
