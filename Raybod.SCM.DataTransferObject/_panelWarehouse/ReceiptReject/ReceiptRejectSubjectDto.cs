using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.DataTransferObject.Receipt
{
    public class ReceiptRejectSubjectDto
    {
        public long ReceiptRejectSubjectId { get; set; }

        /// <summary>
        /// شناسه کالا 
        /// </summary>
        public int ProductId { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal ReceiptQuantity { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal PurchaseRejectRemainedQuantity { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Quantity { get; set; }

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

        public List<ReceiptRejectSubjectDto> ReceiptRejectSubjects { get; set; }

        public ReceiptRejectSubjectDto()
        {
            ReceiptRejectSubjects = new List<ReceiptRejectSubjectDto>();
        }
    }
}
