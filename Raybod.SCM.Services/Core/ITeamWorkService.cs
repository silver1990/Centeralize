using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject._admin;
using Raybod.SCM.DataTransferObject.Authentication;
using Raybod.SCM.DataTransferObject.Role;
using Raybod.SCM.DataTransferObject.TeamWork;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Services.Core.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Core
{
    public interface ITeamWorkService
    {
        Task<ServiceResult<string>> AddTeamWorkAsync(AuthenticateDto authenticate, string contractCode);

        //Task<ServiceResult<bool>> SetTeamWorkUserConfigAsync(AuthenticateDto authenticate, List<SetTeamWorkUserConfigDto> teamWorks);

        Task<ServiceResult<List<TeamWorkInfoDto>>> GetAllTeamWorkAsync(AuthenticateDto authenticate, TeamWorkQueryDto query);

        Task<ServiceResult<List<TeamWorkUserPermissionsDto>>> AddUserToTeamWorkAsync(AuthenticateDto authenticate, int teamWorkId, List<int> userIds);

        Task<ServiceResult<bool>> SetTeamWorkUserProductGroupAsync(AuthenticateDto authenticate, int teamWorkId, int userId, List<int> productGroupIds);

        Task<ServiceResult<bool>> SetTeamWorkUserDocumentGroupAsync(AuthenticateDto authenticate, int teamWorkId, int userId, List<int> documentGroupIds);

        Task<ServiceResult<bool>> SetTeamWorkUserOperationGroupAsync(AuthenticateDto authenticate, int teamWorkId, int userId, List<int> operationGroupIds);

        Task<ServiceResult<bool>> SetUserTeamWorkRoleAsync(AuthenticateDto authenticate, int teamWorkId, int userId, List<BaseUserRoleDto> roles);

        Task<ServiceResult<BaseTeamWorkDto>> GetTeamWorkByIdAsync(AuthenticateDto authenticate, int teamWorkId);

        Task<ServiceResult<bool>> RemoveTeamWorkAsync(AuthenticateDto authenticate, int teamWorkId);

        Task<ServiceResult<TeamWorkDetailsDto>> GetTeamWorkUserListByTeamWorkIdAsync(AuthenticateDto authenticate, int teamWorkId, TeamWorkQueryDto query);

        Task<ServiceResult<bool>> DeleteUserFromTeamWorkAsync(AuthenticateDto authenticate, int userId, int teamWorkId);

        Task<ServiceResult<List<BaseUserTeamWorkDto>>> GetUserTeamWorkAsync(AuthenticateDto authenticate);

        Task<ServiceResult<List<BaseUserTeamWorkDto>>> GetUserTeamWorkForCustomerUserAsync(AuthenticateDto authenticate);

        Task<ServiceResult<List<TeamWorkForFileDriveShareDto>>> GetUserTeamWorkForFileShareAsync(AuthenticateDto authenticate,Guid entityId, EntityType entityType);
    }
}
