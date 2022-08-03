using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Payment;
using Raybod.SCM.DataTransferObject.PendingForPayment;
using Raybod.SCM.DataTransferObject.Supplier;
using Raybod.SCM.DataTransferObject.User;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Model;
using Raybod.SCM.Services.Core.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Core
{
    public interface IPaymentService
    {
        #region po
        Task<ServiceResult<bool>> AddPendingToPaymentBaseOnTermsOfApprovePOAsync(AuthenticateDto authenticate, PO poModel);
        Task<ServiceResult<AddPendingForPaymentResultDto>> AddPendingToPaymentBaseOnTermsOfPaymentExceptInvoiceAsync(AuthenticateDto authenticate, long poId, AddPendingForPaymentDto model);
        
        Task<ServiceResult<bool>> DeletePendingForPaymentByPOIdAsync(AuthenticateDto authenticate, long poId, long pendingForPaymentId);
        #endregion

        #region prContract
        Task<ServiceResult<List<PendingForPaymentReportForContractDto>>> GetReportPendingForPaymentByPRContractIdAsync(AuthenticateDto authenticate, long prContractId);
        #endregion

        Task<ServiceResult<bool>> AddPendingToPaymentBaseOnTermsOfPaymentOfInvoice(AuthenticateDto authenticate, PO poModel, long invoiceId, decimal amount);


        Task<ServiceResult<int>> GetPendingForPaymentBadgeCountAsync(AuthenticateDto authenticate);

        Task<ServiceResult<List<ListPendingForPaymentDto>>> GetNotSettledPendingForPaymentAsync(AuthenticateDto authenticate, PendingForPaymentQueryDto query);

        Task<ServiceResult<PendingForPaymentInfoForPayDto>> GetPendingOfPaymentForPayByPendingOfPaymentIdAsync(AuthenticateDto authenticate, long pendingForPaymentId);

        Task<ServiceResult<List<PendingForPaymentForPayedDto>>> GetPendingOfPaymentForPayBySupplierIdAsync(AuthenticateDto authenticate, int supplierId);

        Task<ServiceResult<List<ListPendingForPaymentDto>>> GetPendingForPaymentAsync(AuthenticateDto authenticate, PendingForPaymentQueryDto query);

        Task<ServiceResult<bool>> AddPaymentByPendingForPaymentAsync(AuthenticateDto authenticate, int supplierId, AddPaymentDto model);

        Task<ServiceResult<List<PaymentListDto>>> GetPaymentAsync(AuthenticateDto authenticate, PaymentQueryDto query);
        Task<ServiceResult<List<PaymentListDto>>> GetPaymentListAsync(AuthenticateDto authenticate, PaymentQueryDto query);
        Task<ServiceResult<List<PendingForConfirmPaymentListDto>>> GetPendingForConfirmPaymentAsync(AuthenticateDto authenticate, PaymentQueryDto query);

        Task<ServiceResult<PaymentInfoDto>> GetPaymentByIdAsync(AuthenticateDto authenticate, long paymentId);

        Task<DownloadFileDto> DownloadPaymentAttachmentAsync(AuthenticateDto authenticate, long paymentId, string fileSrc);

        // for po tracking
        Task<ServiceResult<PendingForPaymentDto>> GetPendingForPaymentByPOIdAsync(AuthenticateDto authenticate, long poId);
        Task<ServiceResult<bool>> CancelPendingForPaymentByPendingForPaymentIdAsync(AuthenticateDto authenticate, long pendingForPaymentId);
        Task<ServiceResult<List<UserMentionDto>>> GetConfirmationUserListAsync(AuthenticateDto authenticate);
        Task<ServiceResult<List<SupplierMiniInfoDto>>> GetSuppliersAsync(AuthenticateDto authenticate, SupplierQuery query);
        Task<ServiceResult<PaymentConfirmationWorkflowDto>> GetPendingConfirmPaymentByPaymentIdAsync(AuthenticateDto authenticate, long paymentId);
        Task<ServiceResult<List<PendingForConfirmPaymentListDto>>> SetUserConfirmOwnPurchaseRequestTaskAsync(AuthenticateDto authenticate, long paymentId, AddPaymentConfirmationAnswerDto model);
        Task<ServiceResult<PaymentInfoWithWorkFlowDto>> GetPaymentInfoByIdAsync(AuthenticateDto authenticate, long paymentId);
        //Task<ServiceResult<List<PaymentListDto>>> GetPaymentsByPoIdAsync(AuthenticateDto authenticate, long poId);

        //Task<ServiceResult<PaymentInfoDto>> GetPaymentByIdAndPoIdAsync(AuthenticateDto authenticate, long poId, long paymentId);
    }
}
