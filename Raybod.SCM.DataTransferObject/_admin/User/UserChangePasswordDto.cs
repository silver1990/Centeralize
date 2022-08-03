using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.User
{
    public class UserChangePasswordDto
    {
        [Display(Name = "گذر واژه جدید")]
        [DataType(DataType.Password)]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "حداقل مقدار برای فیلد {2} و حداکثر {1} می باشد.")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "لطفا گذر واژه جدید را وارد کنید")]
        public string NewPassword { get; set; }

        [Display(Name = "گذر واژه فعلی")]
        [DataType(DataType.Password)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "لطفا گذر واژه فعلی را وارد کنید")]
        public string CurrentPassword { get; set; }
    }
}