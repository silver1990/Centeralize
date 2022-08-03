using System.Collections.Generic;
using System.Threading.Tasks;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Authentication;
using Raybod.SCM.DataTransferObject.Customer;
using Raybod.SCM.DataTransferObject.User;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Services.Core.Common;

namespace Raybod.SCM.Services.Core
{
    public interface IUserService
    {
        Task<ServiceResult<ListUserDto>> AddUserAsync(AuthenticateDto authenticate, AddUserDto model,int type);

        Task<ServiceResult<bool>> EdiUserAsync(AuthenticateDto authenticate, int userId, EditUserDto model);
    

        Task<ServiceResult<List<ListUserDto>>> GetUserAsync(AuthenticateDto authenticate, UserQueryDto query,int type=(int)UserStatus.OrganizationUser);

        Task<ServiceResult<List<UserMentionDto>>> GetUserMiniInfoAsync(AuthenticateDto authenticate, UserQueryDto query);

        Task<ServiceResult<List<UserMentionDto>>> GetUserMiniInfoWithoutAuthenticationAsync(UserQueryDto query);

        Task<ServiceResult<bool>> DeleteUserAsync(AuthenticateDto authenticate, int userId);

        Task<ServiceResult<bool>> ChangePasswordAsync(int userId, UserChangePasswordDto model);
        Task<ServiceResult<bool>> ResetPassworAsync(UserResetPasswordDto model);

        Task<ServiceResult<ListUserDto>> SigninAsync(SigningDto model);

        Task<ServiceResult<UserInfoApiDto>> SigninWithApiAsync(SigningApiDto model);
        Task<ServiceResult<UserInfoApiDto>> UserInfoInCheckAuthentication(AuthenticateDto authenticate);

        ServiceResult<string> SetRefreshToken(string username, string refreshToken, bool isRememberMe);

        Task<ServiceResult<bool>> CheckAndSetRefreshTokenAsync(string username, string refreshToken, string newRefreshToken);

        Task<ServiceResult<bool>> ActivatedUserAsync(AuthenticateDto authenticate, int userId);

        Task<ServiceResult<List<BaseUserTeamWorkDto>>> GetUserTeamWork(AuthenticateDto authenticate, int userId);

        Task<ServiceResult<ListUserDto>> AddUserForCustomerAsync(AuthenticateDto authenticate, AddUserDto model, int type, bool active);

        Task<ServiceResult<bool>> IsUserCustomerAccess(AuthenticateDto authenticate);

        Task<ServiceResult<bool>> IsUserCustomerOrSupperUserAccess(AuthenticateDto authenticate);

        Task<ServiceResult<List<ListNotOrgenizationUser>>> GetUserForCustomerUsersAsync(AuthenticateDto authenticate, UserQueryDto query, List<int> type);
        Task<ServiceResult<bool>> ForgetPassword(ForgetPasswordModel model,string lang);
        Task<ServiceResult<bool>> ValidatePasswordRecoveryRequest(ValidateRecoveryPasswordDto model);
    }
}