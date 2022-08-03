using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Raybod.SCM.DataTransferObject._panelPurchase.PurchaseRequestConfirmation
{
    public class AddPurchaseConfirmationAttachmentDto
    {
        public long PAttachmentId { get; set; }
        /// <summary>
        ///  نام فایل
        /// </summary>
        [MaxLength(250)]
        public string FileName { get; set; }

        /// <summary>
        ///  نام فایل
        /// </summary>
        [MaxLength(50)]
        public string FileSrc { get; set; }

       
    }
}
