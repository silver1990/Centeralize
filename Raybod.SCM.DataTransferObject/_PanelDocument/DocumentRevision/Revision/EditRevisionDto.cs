using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Document
{
    public class EditRevisionDto
    {
        [MaxLength(300)]
        public string Reason { get; set; }

        public long? DateEnd { get; set; }

        [MaxLength(64)]
        public string Code { get; set; }
    }
}
