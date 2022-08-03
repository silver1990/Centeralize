using System;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;

namespace Raybod.SCM.Utility.Extention
{
    /// <summary>
    /// مزین کردن اکشن ها به این فیلتر از درخواست اسپم گونه به اکشن جلوگیری میکند 
    /// </summary>
    public class StopSpamAttribute : IActionFilter
    {
        /// <summary>
        ///  حداقل زمان مجاز بین درخواست‌ها برحسب ثانیه
        /// </summary>
        public int DelayRequest = 10;

        /// <summary>
        /// پیام خطایی که در صورت رسیدن درخواست غیرمجاز باید صادر کنیم
        /// </summary>
        public string ErrorMessage = "درخواست‌های شما در مدت زمان معقولی صورت نگرفته است.";

        /// <summary>
        ///خصوصیتی برای تعیین اینکه آدرس درخواست هم به شناسه یکتا افزوده شود یا خیر
        /// <br/>
        /// درصورت true بودن آدرس درخواستی هم به شناسه یکتا اضفه میگردد که از در خواست های یکسان به یک اکشن جلوگیری کند
        /// </summary>
        public bool AddAddress = true;


        public void OnActionExecuting(ActionExecutingContext actionContext)
        {
            // درسترسی به شئی درخواست
            var request = actionContext.HttpContext.Request;

            // دسترسی به شیئ کش 
            var cache = new MemoryCache(new MemoryCacheOptions());


            // کاربر IP بدست آوردن
            var ip = IpExtentions.GetIPAddress(actionContext);

            // مشخصات مرورگر
            var browser = actionContext.HttpContext.Request.Headers["UserAgent"].ToString();
            var absolutePath = string.Concat(
                   request.Scheme,
                   "://",
                   request.Host.ToUriComponent(),
                   request.PathBase.ToUriComponent(),
                   request.Path.ToUriComponent(),
                   request.QueryString.ToUriComponent());
            // در اینجا آدرس درخواست جاری را تعیین می‌کنیم 
            var targetInfo = (AddAddress) ? (absolutePath + request.QueryString) : "";

            // شناسه یکتای درخواست
            var uniquely = string.Concat(ip, browser, targetInfo);


            //در اینجا با کمک هش یک امضا از شناسه‌ی درخواست ایجاد می‌کنیم
            //It formats the string as two uppercase hexadecimal characters.
            //In more depth, the argument "X2" is a "format string" that tells the ToString() method how it should format the string. byte.ToString() without any arguments returns the number in its natural decimal representation, with no padding.
            var hashValue = string.Join("", MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(uniquely)).Select(s => s.ToString("x2")));

            // ابتدا چک می‌کنیم که آیا شناسه‌ی یکتای درخواست در کش موجود نباشد
            if (cache.Get(hashValue) != null)
            {
                actionContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                actionContext.HttpContext.Response.Body.Flush(); // Sends all currently buffered output to the client.
                                                                 //  actionContext.HttpContext.Response.SuppressContent = true;  // Gets or sets a value indicating whether to send HTTP content to the client.
                                                                 //actionContext.HttpContext.ApplicationInstance.CompleteRequest();
                                                                 //actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden);
                                                                 //if (actionContext.Result is ObjectResult objectResult)
                                                                 //{
                                                                 //    objectResult.Value = HttpResponseMessage(HttpStatusCode.Forbidden);
                                                                 //}
                                                                 // Causes ASP.NET to bypass all events and filtering in the HTTP pipeline chain of execution and directly execute the EndRequest event.
            }
            else
            {
                // اگر موجود نبود یعنی درخواست با زمانی بیشتر از مقداری که تعیین کرده‌ایم انجام شده
                // پس شناسه درخواست جدید را با پارامتر زمانی که تعیین کرده بودیم به شیئ کش اضافه می‌کنیم

                var expiration = DateTimeOffset.UtcNow.AddSeconds(DelayRequest);
                var cacheEntry = cache.CreateEntry(hashValue);
                cacheEntry.AbsoluteExpiration = expiration;
                cacheEntry.Value = true;
                cacheEntry.Priority = CacheItemPriority.Normal;
                return;
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            return;
        }
    }
}