using Raybod.SCM.DataTransferObject.Customer;
using Raybod.SCM.Services.Core.Common;
using System.Collections.Generic;
using System.Threading.Tasks;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject._admin.Customer;
using Raybod.SCM.DataTransferObject.Consultant;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.DataTransferObject.User;

namespace Raybod.SCM.Services.Core
{
    public interface ICustomerService
    {
        Task<ServiceResult<BaseCustomerDto>> AddCustomerAsync(AuthenticateDto authenticate, AddCustomerDto model);

        Task<ServiceResult<bool>> EditCustomerAsync(AuthenticateDto authenticate, int customerId, AddCustomerDto model);

        Task<ServiceResult<List<BaseCustomerDto>>> GetCustomerAsync(AuthenticateDto authenticate, CustomerQuery query);

        Task<ServiceResult<List<CustomerMiniInfoDto>>> GetCustomerMiniInfoAsync(AuthenticateDto authenticate, CustomerQuery query);

        Task<ServiceResult<List<CustomerMiniInfoForCommentDto>>> GetCustomerMiniInfoWithoutPageingAsync(AuthenticateDto authenticate, CustomerQuery query);

        Task<ServiceResult<BaseCustomerDto>> GetCustomerByIdAsync(AuthenticateDto authenticate, int customerId);

        Task<ServiceResult<bool>> DeleteCustomerAsync(AuthenticateDto authenticate, int customerid);

        Task<ServiceResult<BaseCustomerUserDto>> AddCustomerUserAsync(AuthenticateDto authenticate, int customerId, AddCustomerUserDto model, bool? isUserSystem);
        Task<ServiceResult<BaseCustomerUserDto>> AddCustomerUserAsync(AuthenticateDto authenticate, int customerId, AddCustomerUserDto model, bool? isUserSystem, AddUserDto userModel, UserStatus type);
        Task<ServiceResult<BaseCustomerUserDto>> EditCustomerUserAsync(AuthenticateDto authenticate, int customerId, EditCustomerUserDto model, int companyUserId, AddUserDto userModel, UserStatus type, bool active);

        Task<ServiceResult<List<BaseCustomerUserDto>>> GetCustomerUserAsync(AuthenticateDto authenticate, int customerId);

        Task<ServiceResult<bool>> DeleteCustomerUserByIdAsync(AuthenticateDto authenticate, int customerId, int customerUserId);
       
    }
}
