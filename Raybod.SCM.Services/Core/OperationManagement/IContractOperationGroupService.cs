using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject._PanelOperation.OperationGroup;
using Raybod.SCM.Services.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Core
{
    public interface IContractOperationGroupService
    {
        #region DocumentGroup
        Task<ServiceResult<ListOperationGroupDto>> AddOperationGroupAsync(AuthenticateDto authenticate, AddOperationGroupDto model);

        Task<ServiceResult<bool>> EditOperationGroupAsync(AuthenticateDto authenticate, int operationGroupId, AddOperationGroupDto model);

        Task<ServiceResult<List<ListOperationGroupDto>>> GetOperationGroupListAsync(AuthenticateDto authenticate);
        Task<ServiceResult<List<ListOperationGroupDto>>> GetOperationGroupListWithoutLimitedBypermissionOperationGroupAsync(AuthenticateDto authenticate);

        Task<ServiceResult<bool>> DeleteOperationGroupAsync(AuthenticateDto authenticate, int operationGroupId);

        Task<ServiceResult<List<OperationGroupDto>>> GetContractOperationGroupListAsync(AuthenticateDto authenticate);

        #endregion
    }
}
