using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Payment
{
    public class AddPaymentConfirmationAttachmentDto
    {
        public long PaymentAttachmentId { get; set; }
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
