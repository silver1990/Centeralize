using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Raybod.SCM.DataTransferObject
{
    public class AddOperationAttachmentDto
    {
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
