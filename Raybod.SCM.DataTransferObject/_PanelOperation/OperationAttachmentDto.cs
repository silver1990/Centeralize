using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Raybod.SCM.DataTransferObject._PanelOperation
{
    public class OperationAttachmentDto 
    {
        public long OperationAttachmentId { get; set; }
        
        /// <summary>
        /// حجم فایل
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// نوع فایل
        /// </summary>
        public string FileType { get; set; }

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
