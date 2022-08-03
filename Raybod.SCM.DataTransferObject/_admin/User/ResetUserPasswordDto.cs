using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.User
{
    public class ResetUserPasswordDto
    {
        public int Id { get; set; }

        [Display(Name = "گذر واژه جدید")]
        [DataType(DataType.Password)]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "حداقل مقدار برای فیلد {2} و حداکثر {1} می باشد.")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "لطفا گذر واژه جدید را وارد کنید")]
        public string NewPassword { get; set; }

        [Display(Name = "تایید گذر واژه جدید")]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "تایید گذر واژه نامعتبر است")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "لطفا تایید گذر واژه جدید را وارد کنید")]
        public string CofirmNewPassword { get; set; }
    }
}