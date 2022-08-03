using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.Services.Utilitys.MailService.Model
{
    public class EmailRequest
    {
        [Required]
        public List<string> To { get; set; }
        public List<string> Bcc { get; set; }

        [Required]
        public string Body { get; set; }

        [Required]
        public string Subject { get; set; }

        public IFormFile Attachment { get; set; }

        public EmailRequest()
        {
            To = new List<string>();
            Bcc = new List<string>();
        }
    }
}
