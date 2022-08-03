using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.DataTransferObject.PO
{
    public class AddPOTermsOfPaymentDto
    {
        /// <summary>
        /// مرجله پرداخت 
        /// </summary>
        public TermsOfPaymentStep PaymentStep { get; set; }

        /// <summary>
        ///  پرداخت اعتباری
        /// </summary>
        [Required(ErrorMessage = "الزامی می باشد")]
        public bool IsCreditPayment { get; set; } = false;

        /// <summary>
        /// درصد پرداخت 
        /// </summary>
        [Required(ErrorMessage = "الزامی می باشد")]
        public decimal PaymentPercentage { get; set; }

        /// <summary>
        /// اعتبار به روز 
        /// </summary>
        [Required(ErrorMessage = "الزامی می باشد")]
        public int CreditDuration { get; set; }

    }
}