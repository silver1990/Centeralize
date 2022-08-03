using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Authentication;
using Raybod.SCM.DataTransferObject.TeamWork.authentication;
using Raybod.SCM.DataTransferObject.User;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Model;
using Raybod.SCM.Services.Core.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Core
{
    public interface ITeamWorkAuthenticationService
    {
        Task<List<UserMentionDto>> GetAllUserHasAccessDocumentAsync(string contractCode);
        Task<List<UserMentionDto>> GetAllUserHasAccessDocumentAsync(string contractCode, List<string> roles, int DocumentGroupId);
        Task<List<UserMentionDto>> GetAllUserHasAccessPurchaseAsync(string contractCode, List<string> roles, int productGroupId);
        Task<List<UserMentionDto>> GetAllUserHasAccessPurchaseAsync(string contractCode, List<string> roles);
        Task<List<UserMentionDto>> GetAllUserHasAccessPOAsync(string contractCode, List<string> roles, int productGroupId);
        Task<List<UserMentionDto>> GetAllUserHasAccessToFileAsync(string contractCode, List<string> roles);
        Task<List<UserMentionDto>> GetAllUserHasAccessOperationActivityAsync(string contractCode, long operationGroupId);
        Task<List<TeamWorkUserRole>> GetTeamWorkRolesByRolesAndContractCode(string contractCode, List<string> roles);

        Task<List<TeamWorkUserRole>> GetTeamWorkRolesByRolesAndContractCode(string contractCode, List<string> roles, int? documentGroupId, int? productGroupId);
        Task<List<TeamWorkUserRole>> GetTeamWorkRolesByRolesAndContractCode(string contractCode, List<string> roles, int? documentGroupId, int? productGroupId, int? operationGroupId);
        Task<List<TeamWorkUserRole>> GetTeamWorkRolesByRolesAndContractCode(List<string> contractCodes, List<string> roles);
        Task<List<UserInfoForAuditLogDto>> GetSCMEventLogReceiverUserByNotifEventAndContractCode(string contractCodes, NotifEvent notifEvent);

        Task<List<TeamWorkUser>> GetTeamWorkRolesByRolesAndContractCode(string contractCode, List<int> documentGroupIds, List<string> roles);

        Task<PermissionResultDto> HasUserPermissionWithProductGroup(int userId, string contractCode, List<string> roles);

        PermissionResultDto GetAccessableUserPermission(int userId, List<string> roles);

        Task<UserLogPermissonDto> GetUserLogPermissionEntitiesAsync(int userId, string contractCode);

        bool IsUserHaveAnyOfThisRoles(int userId, List<string> roles);

        Task<RoleBasePermissionResultDto> HasUserPermission(int userId, string contractCode, List<string> roles);

        Task<List<int>> HasUserLimitedByOperationGroup(int userId, string contractCode);

        bool HasPermission(int userId, string contractCode, List<string> roles);

        bool HasPermission(int userId, List<string> contractCodes, List<string> roles);

        bool HasPermission(int userId, string contractCode);
        PermissionServiceResult GetAccessableContract(int userId, List<string> roles);


        Task<ServiceResult<List<BaseUserTeamWorkDto>>> GetUserPermissionByUserIdAsync(int userId);
        Task<List<UserMentionDto>> GetAllUserHasAccessDocumentForCustomerUserAsync(string contractCode, int DocumentGroupId);

        //Task<ServiceResult<List<GlobalSearchPermisionDto>>> GetGlobalSearchPermissionByUserIdAsync(int userId, List<SCMFormPermission> SCMFormPermissions = null);
        Task<FileDriveTrashPermisionDto> HasUserPermissionForFileDriveTrash(int userId, string contractCode, List<string> roles);
    }
}
