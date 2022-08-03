using Raybod.SCM.DataTransferObject.PRContract;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Raybod.SCM.DataTransferObject.PO.POSupplierDocuemnt
{
    public class POSupplierDocumentAttachmentDto:BasePAttachmentDto
    {
        public long? POSupplierDocumentId { get; set; }
    }
    public class POSupplierDocumentEditAttachmentDto
    {

        public long Id { get; set; }

        public long? POSupplierDocumentId { get; set; }
        /// <summary>
        ///  نام فایل
        /// </summary>
        [MaxLength(250, ErrorMessage = "حداکثر مقدار برای فیلد {1} می باشد.")]
        public string FileName { get; set; }


        /// <summary>
        ///  نام فایل
        /// </summary>
        [MaxLength(250, ErrorMessage = "حداکثر مقدار برای فیلد {1} می باشد.")]
        public string FileSrc { get; set; }
    }
}
