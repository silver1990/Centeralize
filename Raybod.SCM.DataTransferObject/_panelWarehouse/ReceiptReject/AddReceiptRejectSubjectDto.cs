using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Receipt
{
    public class AddReceiptRejectSubjectDto
    {
        /// <summary>
        /// شناسه کالا 
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// میزان دریافت شده
        /// </summary>
        public decimal Quantity { get; set; }

        public List<AddReceiptRejectSubjectDto> Parts { get; set; }

    }
}
