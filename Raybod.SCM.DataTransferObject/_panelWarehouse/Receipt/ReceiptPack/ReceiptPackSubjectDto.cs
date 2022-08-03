using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Receipt
{
    public class ReceiptPackSubjectDto
    {
        public long ReceiptSubjectId { get; set; }

        /// <summary>
        /// شناسه کالا 
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// میزان اعلام شده
        /// </summary>
        public decimal PackQuantity { get; set; }

        /// <summary>
        /// میزان دریافت شده
        /// </summary>
        public decimal ReceiptQuantity { get; set; }

        /// <summary>
        /// میزان تایید شده
        /// </summary>
        public decimal QCReceiptQuantity { get; set; }

        /// <summary>
        /// کد کالا
        /// </summary>
        [Display(Name = "کد محصول")]
        public string ProductCode { get; set; }

        /// <summary>
        /// نام کالا
        /// </summary>
        [Display(Name = "نام محصول")]
        public string ProductName { get; set; }

        /// <summary>
        /// واحد کالا
        /// </summary>
        [Display(Name = "واحد اصلی")]
        public string ProductUnit { get; set; }

        public List<ReceiptPackSubjectDto> ReceiptPackSubjects { get; set; }

        public ReceiptPackSubjectDto()
        {
            ReceiptPackSubjects = new List<ReceiptPackSubjectDto>();
        }

    }
}
