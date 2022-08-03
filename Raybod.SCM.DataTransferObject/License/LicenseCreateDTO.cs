using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.License
{
    public class LicenceCreateDTO
    {
        public string Username { get; set; }
        public string ActivationCode { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }
    }
}
