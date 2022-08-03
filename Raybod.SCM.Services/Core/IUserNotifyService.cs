using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Notification;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Services.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Core
{
    public interface IUserNotifyService
    {
        Task<ServiceResult<UserNotifyListDto>> GetUserNotifies(AuthenticateDto authenticate);
        Task<ServiceResult<UserNotifyListDto>> GetUserNotifies(AuthenticateDto authenticate,int teamworkId,int userId);
        Task<ServiceResult<bool>> UpdateUserNotifies(AuthenticateDto authenticate, UserNotifyListDto model);
        Task<ServiceResult<bool>> UpdateUserNotifies(AuthenticateDto authenticate, UserNotifyListDto model, int teamworkId,int userId);
    }
}
