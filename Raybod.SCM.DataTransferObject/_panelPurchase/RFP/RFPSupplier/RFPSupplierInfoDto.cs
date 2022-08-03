using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.RFP
{
    public class RFPSupplierInfoDto
    {
        public long RFPSupplierId { get; set; }

        public long RFPId { get; set; }

        public int SupplierId { get; set; }

        [Display(Name = "کد تامین کننده")]
        public string SupplierCode { get; set; }

        [Display(Name = "نام شرکت فارسی")]
        public string SupplierName { get; set; }


        [Display(Name = "ایمیل")]
        public string SupplierEmail { get; set; }

        public string SupplierPhone { get; set; }

        public bool IsActive { get; set; }

        public List<string> SupplierProductGroups { get; set; }

        [Display(Name = "لوگو")]
        public string SupplierLogo { get; set; }

        public SupplierProposalState TechProposalState { get; set; }

        public SupplierProposalState CommercialProposalState { get; set; }
        public ProFormaStatus ProFormaStatus { get; set; }


        public bool IsWinner { get; set; }

        public RFPSupplierInfoDto()
        {
            SupplierProductGroups = new List<string>();
        }
    }
}
