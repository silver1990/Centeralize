using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Consultant;
using Raybod.SCM.DataTransferObject.User;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Services.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Core
{
    public interface IConsultantService
    {
        Task<ServiceResult<BaseConsultantDto>> AddConsultantAsync(AuthenticateDto authenticate, AddConsultantDto model);

        Task<ServiceResult<bool>> EditConsultantAsync(AuthenticateDto authenticate, int customerId, AddConsultantDto model);

        Task<ServiceResult<List<BaseConsultantDto>>> GetConsultantAsync(AuthenticateDto authenticate, ConsultantQuery query);

        Task<ServiceResult<List<ConsultantMiniInfoDto>>> GetConsultantMiniInfoAsync(AuthenticateDto authenticate, ConsultantQuery query);

        Task<ServiceResult<List<ConsultantMiniInfoDto>>> GetConsultantMiniInfoWithoutPageingAsync(AuthenticateDto authenticate, ConsultantQuery query);

        Task<ServiceResult<BaseConsultantDto>> GetConsultantByIdAsync(AuthenticateDto authenticate, int consultantId);

        Task<ServiceResult<bool>> DeleteConsultantAsync(AuthenticateDto authenticate, int consultantid);

        Task<ServiceResult<BaseConsultantUserDto>> AddConsultantUserAsync(AuthenticateDto authenticate, int customerId, AddConsultantUserDto model, bool? isUserSystem);
        Task<ServiceResult<BaseConsultantUserDto>> AddConsultantUserAsync(AuthenticateDto authenticate, int consultantId, AddConsultantUserDto model, bool? isUserSystem, AddUserDto userModel, UserStatus type);
        Task<ServiceResult<BaseConsultantUserDto>> EditConsultantUserAsync(AuthenticateDto authenticate, int consultantId, EditConsultantUserDto model, int companyUserId,AddUserDto userModel,UserStatus type,bool active);

        Task<ServiceResult<List<BaseConsultantUserDto>>> GetConsultantUserAsync(AuthenticateDto authenticate, int consultantId);

        Task<ServiceResult<bool>> DeleteConsultantUserByIdAsync(AuthenticateDto authenticate, int consultantId, int consultantUserId);
    }
}
