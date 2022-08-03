using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Document;
using Raybod.SCM.Services.Core.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Core
{
    public interface IContractDocumentGroupService
    {
        #region DocumentGroup
        Task<ServiceResult<ListDocumentGroupDto>> AddDocumentGroupAsync(AuthenticateDto authenticate, AddDocumentGroupDto model);

        Task<ServiceResult<bool>> EditDocumentGroupAsync(AuthenticateDto authenticate, int documentGroupId, AddDocumentGroupDto model);

        Task<ServiceResult<List<ListDocumentGroupDto>>> GetDocumentGroupListAsync(AuthenticateDto authenticate);
        Task<ServiceResult<List<ListDocumentGroupDto>>> GetDocumentGroupListWithoutLimitedBypermissionDocumentGroupAsync(AuthenticateDto authenticate);

        Task<ServiceResult<bool>> DeleteDocumentGroupAsync(AuthenticateDto authenticate, int documentGroupId);
        #endregion


        Task<ServiceResult<ContractDocumentGroupListDto>> GetContractDocumentGroupListAsync(AuthenticateDto authenticate);


        Task<ServiceResult<List<ListDocumentGroupDto>>> GetDocumentGroupListForCustomerUserAsync(AuthenticateDto authenticate, bool accessability);
    }
}
