using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Email
{
    public class TransmittalEmailContentDto
    {
        [Required]
        public string Subject { get; set; }
        [Required]
        [EmailAddress]
        public string To { get; set; }
        public List<string> CC { get; set; }
        [Required]
        public string Message { get; set; }
    }
}
