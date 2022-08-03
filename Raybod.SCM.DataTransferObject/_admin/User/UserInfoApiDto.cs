using Raybod.SCM.DataTransferObject._PanelSale.Contract;
using Raybod.SCM.DataTransferObject.Authentication;
using Raybod.SCM.DataTransferObject.Customer;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Raybod.SCM.DataTransferObject.User
{
    public class UserInfoApiDto
    {
        public int Id { get; set; }

        /// <summary>
        /// نام
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "الزامی می باشد")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "حداقل مقدار برای فیلد {2} و حداکثر {1} می باشد.")]
        [Display(Name = "نام")]
        public string FirstName { get; set; }

        /// <summary>
        /// نام خانوادگی
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "الزامی می باشد")]
        [StringLength(100, ErrorMessage = "حداقل مقدار برای فیلد {2} و حداکثر {1} می باشد.")]
        [Display(Name = "نام خانوادگی")]
        public string LastName { get; set; }

        /// <summary>
        /// کد پرسنلی
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "الزامی می باشد")]
        [Display(Name = "کد پرسنلی")]
        [StringLength(50, ErrorMessage = "حداقل مقدار برای فیلد {2} و حداکثر {1} می باشد.")]
        public string PersonalCode { get; set; }

        /// <summary>
        /// شماره موبایل
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "الزامی می باشد")]
        [Display(Name = "شماره موبایل")]
        [RegularExpression(@"0\d{10}$", ErrorMessage = "لطفا درست وارد نمایید")]
        public string Mobile { get; set; }

        /// <summary>
        /// نام کاربری
        /// </summary>
        [StringLength(50, MinimumLength = 5, ErrorMessage = "حداقل مقدار برای فیلد {2} و حداکثر {1} می باشد.")]
        [Display(Name = "نام کاربری")]
        public string UserName { get; set; }

        /// <summary>
        /// ایمیل
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "الزامی می باشد")]
        [EmailAddress(ErrorMessage = "آدرس ایمیل اشتباه وارد شده است")]
        [Display(Name = "ایمیل")]
        public string Email { get; set; }

        /// <summary>
        /// تلفن ثابت
        /// </summary>
        [Display(Name = "تلفن ثابت")]
        public string Telephone { get; set; }

        /// <summary>
        /// تصویر 
        /// </summary>
        [MaxLength(41, ErrorMessage = "حداقل مقدار برای فیلد {1} می باشد.")]
        [Display(Name = "عکس")]
        public string Image { get; set; }

        /// <summary>
        /// امضاء 
        /// </summary>
        [Display(Name = "امضاء")]
        [MaxLength(41, ErrorMessage = "حداقل مقدار برای فیلد {1} می باشد.")]
        public string Signature { get; set; }
        public int UserType { get; set; }
        public List<BaseUserTeamWorkDto> TeamWorks { get; set; }
        public string CompanyLogo { get; set; }
        public string CompanyNameFA { get; set; }
        public string CompanyNameEN { get; set; }
        public string PowerBIRoot { get; set; }
        [JsonIgnore]
        public List<int> LatestTeamworkIds { get; set; }
        public BaseCustomerDto Customer { get; set; }
        public List<string> PlanService { get; set; }
        public UserInfoApiDto()
        {            
            TeamWorks = new List<BaseUserTeamWorkDto>();
            LatestTeamworkIds = new List<int>();
            PlanService = new List<string>();
        }
    }
}
