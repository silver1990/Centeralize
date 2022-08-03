using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.User
{
    public class AddUserDto : BaseUserDto
    {
        /// <summary>
        /// گذرواژه
        /// </summary>
       
        [StringLength(60, MinimumLength = 6, ErrorMessage = "حداقل مقدار برای فیلد {2} و حداکثر {1} می باشد.")]
        [Display(Name = "کلمه عبور")]
        public string Password { get; set; }

    }
}