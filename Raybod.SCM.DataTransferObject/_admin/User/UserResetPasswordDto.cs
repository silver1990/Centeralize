using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Raybod.SCM.DataTransferObject.User
{
    public class UserResetPasswordDto
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "الزامی می باشد")]
       
        public string UserName { get; set; }
        public string Code { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "الزامی می باشد")]
        [StringLength(60, MinimumLength = 1, ErrorMessage = "حداقل مقدار برای فیلد {2} و حداکثر {1} می باشد.")]
        public string NewPassword { get; set; }

        
    }
}
