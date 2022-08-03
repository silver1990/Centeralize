using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.PRContract
{
    public class BasePRContractSubjectPartListDto : AddPRContractPartListDto
    {
        public long PRContractSubjectPartListId { get; set; }

        [Display(Name = "کد محصول")]
        public string ProductCode { get; set; }

        [Display(Name = "نام محصول")]
        public string ProductName { get; set; }

        [Display(Name = "واحد اصلی")]
        public string ProductUnit { get; set; }

        [Display(Name = "شماره فنی")]
        public string TechnicalNumber { get; set; }

        public decimal RemainedStock { get; set; }

        public decimal ShortageQuantity { get; set; }

        public decimal ReceiptQuantity { get; set; }
    }
}
