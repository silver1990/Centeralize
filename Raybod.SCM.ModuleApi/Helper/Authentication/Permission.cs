using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Utility.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace Raybod.SCM.ModuleApi.Helper.Authentication
{
    public class Permission : ActionFilterAttribute
    {
        /// <summary>
        /// نام نقش ها
        /// </summary>
        public string Role { get; set; }

        ///// <summary>
        ///// شناسه سطح دسترسی 
        ///// </summary>
        //public bool Is { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {

            var identityName = filterContext.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (identityName == null)
            {
                filterContext.Result = new EmptyResult();
                filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return;
            }

            List<string> roles = new List<string>();
            if (Role.Contains(","))
                roles.AddRange(Role.SplitHelper());
            else
                roles.Add(Role);

            var userId = int.Parse(identityName.Value);

            var _teamWorkAuthenticationService = filterContext.HttpContext.RequestServices.GetService<ITeamWorkAuthenticationService>();

            var result = _teamWorkAuthenticationService.IsUserHaveAnyOfThisRoles(userId, roles);
            // age login bud vali access be in safhe nadsht bere be action 403
            if (!result)
            {
                filterContext.Result = new EmptyResult();
                filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }
            return;
        }
    }
}
