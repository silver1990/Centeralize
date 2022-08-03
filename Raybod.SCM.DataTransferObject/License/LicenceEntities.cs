using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.License
{
    public class LicenceEntities
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Licence { get; set; }
        public string ActiveCode { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public DateTime ExpireDate { get; set; }
    }
}
