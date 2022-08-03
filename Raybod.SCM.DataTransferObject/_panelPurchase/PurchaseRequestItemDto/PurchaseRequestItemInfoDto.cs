using Raybod.SCM.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.PurchaseRequest
{
    public class PurchaseRequestItemInfoDto : BasePurchaseRequestItemDto
    {
        [Display(Name = "کد محصول")]
        public string ProductCode { get; set; }

        [Display(Name = "نام محصول")]
        public string ProductDescription { get; set; }

        [Display(Name = "واحد محصول")]
        public string ProductUnit { get; set; }

        [Display(Name = "شماره فنی")]
        public string ProductTechnicalNumber { get; set; }

        [Display(Name = "گروه کالا")]
        public string ProductGroupName { get; set; }

        public EngineeringDocumentStatus DocumentStatus { get; set; }
    }

    public class EditPurchaseRequestItemInfoDto : BasePurchaseRequestItemDto
    {
        [Display(Name = "کد محصول")]
        public string ProductCode { get; set; }

        [Display(Name = "نام محصول")]
        public string ProductDescription { get; set; }

        [Display(Name = "واحد محصول")]
        public string ProductUnit { get; set; }

        [Display(Name = "شماره فنی")]
        public string ProductTechnicalNumber { get; set; }

        [Display(Name = "گروه کالا")]
        public string ProductGroupName { get; set; }

        public EngineeringDocumentStatus DocumentStatus { get; set; }
        public decimal RequiredQuantity { get; set; }
    }
}
