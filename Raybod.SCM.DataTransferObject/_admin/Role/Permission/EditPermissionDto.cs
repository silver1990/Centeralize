using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Role.Permission
{
    public class EditPermissionDto: PermissionDto
    {
        [Display(Name = "وضعیت انتخاب")]
        public bool IsSelected;
    }
}