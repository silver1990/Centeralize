using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.RFP
{
    public class SupplierEvaluationInfoDto
    {
        public int SupplierId { get; set; }

        [Display(Name = "ایمیل")]
        public string Email { get; set; }

        [Display(Name = "لوگو")]
        public string Logo { get; set; }
        /// <summary>
        /// نام تامین کننده
        /// </summary>
        public string SupplierName { get; set; }

        /// <summary>
        /// کد تامین کننده
        /// </summary>
        public string SupplierCode { get; set; }

        /// <summary>
        /// برنده پیشنهاد
        /// </summary>
        public bool IsWinner { get; set; }

        /// <summary>
        /// نمزه پیشنهاد فنی
        /// </summary>
        public decimal TBEScore { get; set; }

        /// <summary>
        /// نمره پیشنهاد بازرگانی
        /// </summary>
        public decimal CBEScore { get; set; }

        /// <summary>
        /// قیمت
        /// </summary>
        public string Price { get; set; }

        /// <summary>
        /// زمان تحویل
        /// </summary>
        public string DeliveryDate { get; set; }
    }
    public class SupplierEvaluationProposalInfoDto
    {
        public List<SupplierEvaluationInfoDto> RFPSupplierProposal { get; set; }
        public List<int> Winners { get; set; }
        public SupplierEvaluationProposalInfoDto()
        {
            RFPSupplierProposal = new List<SupplierEvaluationInfoDto>();
            Winners = new List<int>();
        }
    }
}
