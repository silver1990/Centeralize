using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.Services.Utilitys
{
    public class ServerInfo
    {
        public static int Port { get; set; } = 0;
        public static string IpAddress { get; set; } = "";
        public static bool AlreadySet { get; set; }
    }
}
