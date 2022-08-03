using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject._PanelDocument.Communication;
using Raybod.SCM.DataTransferObject.Document;
using Raybod.SCM.DataTransferObject.Document.Communication;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Services.Core.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Core
{
    public interface IDocumentCommunicationService
    {
        Task<ServiceResult<CurrentContractInfoDto>> GetCurrentContractInfoAsync(AuthenticateDto authenticate);
        Task<ServiceResult<CurrentContractInfoDto>> GetCurrentContractInfoForCustomerUserAsync(AuthenticateDto authenticate,bool accessability);
      
        Task<ServiceResult<List<CommunicationListDto>>> GetPendingReplyCommunicationListAsync(AuthenticateDto authenticate, CommunicationQueryDto query, CommunicationType type);

        Task<ServiceResult<PendingReplayCommunicationDTO>> GetPendingReplyCommunicationBadgeAsync(AuthenticateDto authenticate);


        //Task<ServiceResult<int>> GetPendingReplyCommunicationBadgeForDashbourdAsync(AuthenticateDto authenticate);
        Task<ServiceResult<Dictionary<int, int>>> GetPendingReplyCommunicationBadgeForDashbourdAsync(AuthenticateDto authenticate);

        Task<ServiceResult<List<AllCompanyListDto>>> GetAllCompanyListAsync(AuthenticateDto authenticate);

        Task<ServiceResult<List<RevisionCommunicationListDto>>> GetAllRevisionCommunicationListAsync(AuthenticateDto authenticate, long documentRevisionId);

        Task<ServiceResult<CurrentContractInfoDto>> GetCurrentContractInfoAndCustomerInfoAsync(AuthenticateDto authenticate);

        Task<ServiceResult<List<RevisionCommunicationListDto>>> GetAllRevisionNCRListForCustomerUserAsync(AuthenticateDto authenticate, long documentRevisionId, bool accessability);

        Task<ServiceResult<List<RevisionCommunicationListDto>>> GetAllRevisionTQListForCustomerUserAsync(AuthenticateDto authenticate, long documentRevisionId, bool accessability);

        Task<ServiceResult<List<RevisionCommunicationListDto>>> GetAllRevisionCommentListForCustomerUserAsync(AuthenticateDto authenticate, long documentRevisionId, bool accessability);
    }
}
