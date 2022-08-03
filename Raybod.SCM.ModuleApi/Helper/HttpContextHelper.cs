using Microsoft.AspNetCore.Http;

using Raybod.SCM.DataTransferObject;
using System.Linq;
using System.Security.Claims;

namespace Raybod.SCM.ModuleApi.Helper
{
    public static class HttpContextHelper
    {
        public static AuthenticateDto GetUserAuthenticateInfo(HttpContext httpContext)
        {          
            var authenticate = new AuthenticateDto
            {
                UserId = int.Parse(httpContext.User.Identity.Name),
                UserFullName = httpContext.User.Claims.Where(c => c.Type == ClaimTypes.GivenName)
               .Select(a => a.Value).FirstOrDefault(),
                UserImage = httpContext.User.Claims.Where(c => c.Type == ClaimTypes.UserData)
               .Select(a => a.Value).FirstOrDefault(),
                UserName = httpContext.User.Claims.Where(c => c.Type == ClaimTypes.Surname)
               .Select(a => a.Value).FirstOrDefault()
            };

            authenticate.RemoteIpAddress = httpContext.Connection.RemoteIpAddress.ToString();
            string contractCode = string.Empty;
            if (httpContext.Request.Headers.TryGetValue("CurrentTeamWork", out var traceValue))
            {
                contractCode = traceValue;
            }
            if (httpContext.Request.Headers.TryGetValue("lang", out var lang))
            {
                authenticate.language = lang;
            }
            authenticate.ContractCode = contractCode;
            string companyCode = "";
            if (httpContext.Request.Headers.TryGetValue("companyCode", out var code))
            {
                companyCode = code;
            }
            authenticate.CompanyCode = companyCode;
            return authenticate;
        }
        public static AuthenticateDto GetLanguageWithoutAuthenticateInfo(HttpContext httpContext)
        {
            string language = "fa";
            if (httpContext.Request.Headers.TryGetValue("lang", out var lang))
            {
                language = lang;
            }
            var authenticate = new AuthenticateDto
            {
                language = language
            };
            string companyCode = "";
            if (httpContext.Request.Headers.TryGetValue("companyCode", out var code))
            {
                companyCode = code;
            }
            authenticate.CompanyCode = companyCode;
            return authenticate;
        }
    }
    
}
