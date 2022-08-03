using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Receipt
{
    public class AddReceiptProductDto
    {
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



    }
}
