using System;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace Raybod.SCM.Utility.Helpers
{
    public static class RegexHelpers
    {
        private static readonly Regex HtmlRegex = new Regex("<.*?>", RegexOptions.Compiled);
        private static readonly Regex ContentRegex = new Regex(@"<\/?script[^>]*?>",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex SafeStrRegex = new Regex(@"<script[^>]*?>[\s\S]*?<\/script>",
           RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// حذف تمامی تگ‌های موجود
        /// </summary>
        /// <param name="html">ورودی اچ تی ام ال</param>
        /// <returns></returns>
        public static string CleanTags(this string html)
        {
            HtmlRegex.Replace(html, string.Empty);

            html = html.Replace("&nbsp;", " ");
            html = html.Replace("&zwnj;", " ");
            html = html.Replace("&quot;", " ");
            html = html.Replace("amp;", "");
            html = html.Replace("&laquo;", "«");
            html = html.Replace("&raquo;", "»");
            return html;
        }


        /// <summary>
        /// تنها حذف یک تگ ویژه
        /// </summary>
        /// <param name="html">ورودی اچ تی ام ال</param>
        /// <returns></returns>
        public static string CleanScriptTags(this string html)
        {
            return ContentRegex.Replace(html, string.Empty);
        }


        /// <summary>
        /// حذف یک تگ ویژه به همراه محتویات آن
        /// </summary>
        /// <param name="html">ورودی اچ تی ام ال</param>
        /// <returns></returns>
        public static string CleanScriptsTagsAndContents(this string html)
        {
            return SafeStrRegex.Replace(html, "");
        }


        /// <summary>
        /// بررسی صحیح بودن url 
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        public static bool IsUrlValid(string returnUrl)
        {
            return !string.IsNullOrWhiteSpace(returnUrl)
                   && Uri.IsWellFormedUriString(returnUrl, UriKind.Relative);
        }

        /// <summary>
        /// بررسی صحیح بودن ایمیل
        /// </summary>
        /// <param name="emailaddress"></param>
        /// <returns></returns>
        public static bool IsValidEmail(string emailaddress)
        {
            try
            {
                MailAddress m = new MailAddress(emailaddress);

                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
