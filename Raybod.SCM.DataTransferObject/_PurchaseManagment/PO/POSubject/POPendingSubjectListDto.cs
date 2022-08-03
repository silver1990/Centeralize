using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.PO
{
    public class POPendingSubjectListDto : AddPOPendingSubjectDto
    {
        public long MrpItemId { get; set; }

        public long POId { get; set; }

        public string MrpCode { get; set; }

        public long DateEnd { get; set; }

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
