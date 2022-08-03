using System.Collections.Generic;
using System.Threading.Tasks;
using Raybod.SCM.DataTransferObject.Role;
using Raybod.SCM.DataTransferObject.Role.Permission;
using Raybod.SCM.Services.Core.Common;

namespace Raybod.SCM.Services.Core
{
    public interface IRoleService
    {
        /// <summary>
        /// ایجاد گروه جدید
        /// </summary>
        /// <param name="addRoleDto"></param>
        /// <returns></returns>
        Task<ServiceResult<bool>> AddRoleAsync(AddRoleDto addRoleDto);

        /// <summary>
        /// وبرایش گروه
        /// </summary>
        /// <param name="editRoleDto"></param>
        /// <returns></returns>
        Task<ServiceResult<EditRoleDto>> EditRoleAsync(EditRoleDto editRoleDto);

 
        /// <summary>
        /// نمایش یک گروه
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        Task<ServiceResult<EditRoleDto>> GetRoleByIdAsync(int roleId);

        /// <summary>
        /// حذف گروه
        /// </summary> 
        /// <param name="roleId"></param>
        /// <returns></returns>
        Task<ServiceResult> DeleteRoleAsync(int roleId);


      
    }
}