using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.Utility.EnumType;
using Raybod.SCM.Utility.Extention;
using System;
using System.Linq;
using System.Security.Claims;

namespace Raybod.SCM.Utility.Security
{
    public class LicenseAuthorizationAttribute : IAuthorizationFilter
    {
        private readonly short _permission;
        public LicenseAuthorizationAttribute(short permission)
        {
            _permission = permission;
        }
        public void OnAuthorization(AuthorizationFilterContext filterContext)
        {
            if (filterContext.HttpContext.Request.Headers.Any(h => h.Key == "license header"))
            {
                ApiResponseDTO result = new ApiResponseDTO();
                result.Status = Convert.ToInt16(filterContext.HttpContext.Request.Headers.First(h => h.Key == "license header").Value);
                result.Message = ((LicenseStatus)result.Status).GetDisplayName();
                filterContext.HttpContext.Response.StatusCode = 601;
                filterContext.Result = new JsonResult(result);
            }
        }
    }
    
    public class AuthorizeAttribute : TypeFilterAttribute
    {
        public AuthorizeAttribute(short permission)
            : base(typeof(LicenseAuthorizationAttribute))
        {
            Arguments = new object[] { permission };
        }
    }
}
