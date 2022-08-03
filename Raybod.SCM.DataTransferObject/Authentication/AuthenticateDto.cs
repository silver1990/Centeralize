using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject
{
    public class AuthenticateDto
    {
        public List<string> Roles { get; set; }

        public int UserId { get; set; }
        
        public string UserFullName { get; set; }
        
        public string UserName { get; set; }

        public string ContractCode { get; set; }
        
        public string UserImage { get; set; }

        public string RemoteIpAddress { get; set; }
        public string language { get; set; }
        public string CompanyCode { get; set; }

        public AuthenticateDto()
        {
            Roles = new List<string>();
        }
    }


}