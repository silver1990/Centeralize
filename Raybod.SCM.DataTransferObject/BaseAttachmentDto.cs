using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject
{
   public class BaseAttachmentDto  : AddRevisionAttachmentDto
    {

        /// <summary>
        /// حجم فایل
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// نوع فایل
        /// </summary>
        public string FileType { get; set; }
    }
}
