using System.Collections.Generic;
using System.Threading.Tasks;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.PO;
using Raybod.SCM.DataTransferObject.PRContract;
using Raybod.SCM.DataTransferObject.RFP;
using Raybod.SCM.DataTransferObject.User;
using Raybod.SCM.Services.Core.Common;

namespace Raybod.SCM.Services.Core
{
    public interface IPRContractService
    {
        Task<ServiceResult<string>> AddPRContractAsync(AuthenticateDto authenticate, AddPRContractDto model);
        Task<ServiceResult<string>> EditPRContractAsync(AuthenticateDto authenticate, long prContractId, AddPRContractDto model);
        Task<ServiceResult<List<ListPRContractDto>>> GetPRContractsAsync(AuthenticateDto authenticate, PRContractQuery query);

        Task<ServiceResult<bool>> ApprovePRContract(AuthenticateDto authenticate, long prContractId);

        Task<ServiceResult<PRContractSubjectViewDto>> GetPRContractSubjectsAndServiceByContractIdAsync(AuthenticateDto authenticate, long prContractId);

        Task<ServiceResult<List<EditTermsOfPaymentDto>>> GetPRContractTermOFPaymentByContractIdAsync(AuthenticateDto authenticate, long prContractId);

        Task<ServiceResult<bool>> EditPRContractBaseInfoAsync(AuthenticateDto authenticate, long prContractId, EditPRContractBaseInfoDto model);

        //Task<ServiceResult<bool>> EditPRContractSupplierAsync(AuthenticateDto authenticate, long prContractId, int supplierId);

      

        Task<ServiceResult<bool>> EditPRContractTermsOfPaymentAsync(AuthenticateDto authenticate, long prContractId, List<EditTermsOfPaymentDto> model);

        Task<ServiceResult<List<BasePRContractAttachmentDto>>> AddAttachmentAsync(AuthenticateDto authenticate, long prContractId, List<AddAttachmentDto> attachments);

        Task<ServiceResult<bool>> RemoveAttachmentAsync(AuthenticateDto authenticate, long prContractId, long attachmentId);

        Task<ServiceResult<PRContractInfoDto>> GetPRContractByPRContractIdAsync(AuthenticateDto authenticate, long prContractId);
        Task<ServiceResult<EditPrContractInfoDto>> GetPRContractDetailsByPRContractIdAsync(AuthenticateDto authenticate, long prContractId);
        Task<ServiceResult<List<BasePRContractAttachmentDto>>> GetPRContractAttachmentByPRContractIdAsync(AuthenticateDto authenticate, long prContractId);

        Task<ServiceResult<bool>> RemoveContractAsync(AuthenticateDto authenticate, long prContractId);

        Task<DownloadFileDto> DownloadAttachmentAsync(AuthenticateDto authenticate, long prContractId, string fileSrc);
        Task<DownloadFileDto> DownloadConfirmWorkFlowAttachmentAsync(AuthenticateDto authenticate, long prContractConfirmWorkFlowId, string FileSrc);

        Task<ServiceResult<bool>> IsThereAnyAvailablePRContractForThisProduct(AuthenticateDto authenticate, int productId);

        Task<ServiceResult<List<AddPOByMrpDto>>> GetAvailablePRContractForThisProduct(AuthenticateDto authenticate, int productId);

        Task<ServiceResult<List<RFPItemInfoDto>>> GetRFPItemOfThisPRContractbyprContractIdAsync(AuthenticateDto authenticate, long prContractId);

        Task<ServiceResult<List<ContractPOSubjectReportDto>>> GetReportPOSubjectbyprContractIdAsync(AuthenticateDto authenticate, long prContractId);
        Task<ServiceResult<List<UserMentionDto>>> GetConfirmationUserListAsync(AuthenticateDto authenticate, int productGroupId);
        Task<ServiceResult<int>> GetPendingPRContractBadgeAsync(AuthenticateDto authenticate);
        Task<ServiceResult<List<ListPendingPRContractDto>>> GetPendingForConfirmPrContractstAsync(AuthenticateDto authenticate, PRContractQuery query);
        Task<ServiceResult<PrContractConfirmationWorkflowDto>> GetPendingConfirmPrContractByPrContractIdAsync(AuthenticateDto authenticate, long prContractId);
        Task<ServiceResult<List<ListPendingPRContractDto>>> SetUserConfirmOwnPrContractTaskAsync(AuthenticateDto authenticate, long prContractId, AddPrContractConfirmationAnswerDto model);
        Task<ServiceResult<bool>> CancelPrContractAsync(AuthenticateDto authenticate, long prContractId);
        Task<ServiceResult<PrContractConfirmationWorkflowDto>> GetLastPrContractWorkFlowByPrContractIdAsync(AuthenticateDto authenticate, long prContractId);
    }
}