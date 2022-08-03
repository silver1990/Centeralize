using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Raybod.SCM.Domain.Model
{
    public class EmailErrorLog
    {
        [Key]
        public long Id { get; set; }
        public string ErrorCode { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public DateTime CreatedDate { get; set; }
        public string MimMessage { get; set; }
    }
}
