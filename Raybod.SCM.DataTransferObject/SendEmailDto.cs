using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject
{
    public class SendEmailDto
    {
        public string To { get; set; }
        public List<string> Tos { get; set; }

        public string CC { get; set; }

        public List<string> CCs { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }
        public SendEmailDto()
        {
            Tos = new List<string>();
            CCs = new List<string>();
        }
    }

}
