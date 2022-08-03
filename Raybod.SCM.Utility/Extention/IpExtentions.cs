using Microsoft.AspNetCore.Mvc;

namespace Raybod.SCM.Utility.Extention
{
    public static class IpExtentions
    {
        public static string GetIPAddress(ActionContext context)
        {
            return context.HttpContext.Request.HttpContext.Connection.RemoteIpAddress.IsIPv4MappedToIPv6.ToString();
        }
    }
}
