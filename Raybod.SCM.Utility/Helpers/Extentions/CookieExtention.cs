using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Raybod.SCM.Utility.Helpers.Extentions
{
    public static class CookieExtention
    {

        #region AddCookie
        public static void AddCookie(this ActionContext httpContextBase, string cookieName, string value)
        {
            httpContextBase.HttpContext.Response.Cookies.Append(cookieName, value, new CookieOptions { Expires = DateTimeOffset.Now.AddDays(60), Secure = true });
        }
        public static void AddCookie(this ActionContext httpContextBase, string cookieName, string value, DateTime expires, bool httpOnly = false)
        {
            httpContextBase.HttpContext.Response.Cookies.Append(cookieName, value, new CookieOptions { Expires = expires, Secure = true, HttpOnly = httpOnly });

        }
        #endregion

        #region RemoveCookie
        public static void RemoveCookie(this ActionContext httpContextBase, string cookieName)
        {
            httpContextBase.HttpContext.Response.Cookies.Delete(cookieName, new CookieOptions { Expires = DateTimeOffset.Now.AddDays(-1) });

        }

        public static void RemoveAllCookies(this ActionContext httpContextBase)
        {
            foreach (var keyValuePair in httpContextBase.HttpContext.Request.Cookies)
            {
                httpContextBase.HttpContext.Response.Cookies.Delete(keyValuePair.Key, new CookieOptions { Expires = DateTimeOffset.Now.AddDays(-1) });
            }
        }

        #endregion

        #region UpdateCookie
        public static void UpdateCookie(this ActionContext httpContextBase, string cookieName, string value, bool httpOnly = false)
        {
            var requestCookie = httpContextBase.HttpContext.Request.Cookies[cookieName];
            if (!string.IsNullOrWhiteSpace(requestCookie))
            {
                httpContextBase.HttpContext.Response.Cookies.Delete(cookieName, new CookieOptions { Expires = DateTimeOffset.Now.AddDays(-1) });
            }
            httpContextBase.HttpContext.Response.Cookies.Append(cookieName, value, new CookieOptions { Expires = DateTimeOffset.Now.AddDays(60), Secure = true, HttpOnly = httpOnly });

        }
        #endregion

        #region GetCookie
        public static string GetCookieValue(this ActionContext httpContext, string cookieName)
        {
            var requestCookie = httpContext.HttpContext.Request.Cookies[cookieName];
            return requestCookie ?? string.Empty;

        }
        #endregion



    }
}