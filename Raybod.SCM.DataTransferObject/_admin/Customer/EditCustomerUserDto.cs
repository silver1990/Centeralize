using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Raybod.SCM.DataTransferObject._admin.Customer
{
    public class EditCustomerUserDto
    {


        [Required(AllowEmptyStrings = false, ErrorMessage = "الزامی می باشد")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "حداقل مقدار برای فیلد {2} و حداکثر {1} می باشد.")]
        [Display(Name = "نام")]
        public string FirstName { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "الزامی می باشد")]
        [StringLength(100, ErrorMessage = "حداقل مقدار برای فیلد {2} و حداکثر {1} می باشد.")]
        [Display(Name = "نام خانوادگی")]
        public string LastName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "الزامی می باشد")]
        [EmailAddress(ErrorMessage = "آدرس ایمیل اشتباه وارد شده است")]
        [Display(Name = "ایمیل")]
        public string Email { get; set; }

        public bool IsCustomerUser { get; set; }
    }
}
