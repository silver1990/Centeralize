using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.User
{
    public class SigningDto
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "الزامی می باشد")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "حداقل مقدار برای فیلد {2} و حداکثر {1} می باشد.")]
        public string UserName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "الزامی می باشد")]
        [StringLength(60, MinimumLength = 6, ErrorMessage = "حداقل مقدار برای فیلد {2} و حداکثر {1} می باشد.")]
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}