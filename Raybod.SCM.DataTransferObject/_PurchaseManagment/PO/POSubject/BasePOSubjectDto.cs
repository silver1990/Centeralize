using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.PO
{
    public class BasePOSubjectDto : EditPOSubjectDto
    {
        
        [Display(Name = "کد محصول")]
        public string ProductCode { get; set; }

        [Display(Name = "نام محصول")]
        public string ProductName { get; set; }

        [Display(Name = "واحد اصلی")]
        public string ProductUnit { get; set; }

        [Display(Name = "گروه کالا")]
        public string ProductGroupName { get; set; }

        [Display(Name = "شماره فنی")]
        public string TechnicalNumber { get; set; }
    }
}
