using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Consultant
{
    public class AddConsultantDto
    {
                
        [Required(ErrorMessage = "الزامی می باشد")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "حداقل مقدار برای فیلد {2} و حداکثر {1} می باشد.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "الزامی می باشد")]
        [MaxLength(64)]
        public string ConsultantCode { get; set; }

        [MaxLength(12)]
        public string PostalCode { get; set; }

        //[Required(ErrorMessage = "الزامی می باشد")]
        [MaxLength(300, ErrorMessage = "حداکثر مقدار برای فیلد {1} می باشد.")]
        public string Address { get; set; }

        [MaxLength(100, ErrorMessage = "حداکثر مقدار برای فیلد {1} می باشد.")]
        public string TellPhone { get; set; }

        [MaxLength(20, ErrorMessage = "حداکثر مقدار برای فیلد {1} می باشد.")]
        public string Fax { get; set; }

        [MaxLength(300, ErrorMessage = "حداکثر مقدار برای فیلد {1} می باشد.")]
        //[RegularExpression(@"^(http:\/\/www\.|https:\/\/www\.|http:\/\/|https:\/\/)?[a-z0-9]+([\-\.]{1}[a-z0-9]+)*\.[a-z]{2,5}(:[0-9]{1,5})?(\/.*)?$", ErrorMessage = "آدرس را به درستی وارد کنید")]
        public string Website { get; set; }


        [MaxLength(300, ErrorMessage = "حداکثر مقدار برای فیلد {1} می باشد.")]
        //[EmailAddress(ErrorMessage = "آدرس ایمیل اشتباه وارد شده است")]
        public string Email { get; set; }
        
        [MaxLength(300, ErrorMessage = "حداکثر مقدار برای فیلد {1} می باشد.")]
        public string Logo { get; set; }
    }
}
