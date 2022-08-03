using System.ComponentModel.DataAnnotations;
using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.DataTransferObject.PRContract
{
    public class AddTermsOfPaymentDto
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
    public class PaymentSteps
    {
        public PaymentStep OrderPayment { get; set; }
        public PaymentStep PrepPayment { get; set; }
        public PaymentStep PackPayment { get; set; }
        public PaymentStep InvoicePayment { get; set; }

    }
    public class PaymentStep
    {
        public int Percent { get; set; }
        public int Credit { get; set; }

    }
}