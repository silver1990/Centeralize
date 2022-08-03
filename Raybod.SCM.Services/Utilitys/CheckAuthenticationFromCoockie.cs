using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.Services.Utilitys
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Primitives;
    using System.Threading.Tasks;

    namespace Raybod.SCM.Services.Core
    {
        public static class CheckAuthentication
        {
            public static IApplicationBuilder UseCookieToAuthorize(this IApplicationBuilder app)
            {
                return app.UseMiddleware<CheckAuthenticationFromCoockie>();
            }
        }
        public class CheckAuthenticationFromCoockie
        {
            private readonly RequestDelegate _next;

            public CheckAuthenticationFromCoockie(RequestDelegate next)
            {
                _next = next;
            }
            public async Task InvokeAsync(HttpContext context)
            {





                if (IsDownloadPath(context.Request.Path.Value.ToLower()))
                {
                    StringValues auth;

                    if (!context.Request.Headers.TryGetValue("authorization", out auth))
                    {
                        string token;
                        if (context.Request.Cookies.TryGetValue("token", out token))
                        {
                            context.Request.Headers.Add("authorization", "Bearer " + token);
                        }

                    }
                    StringValues currentTeamWork;

                    if (!context.Request.Headers.TryGetValue("currentteamwork", out currentTeamWork))
                    {
                        string ct;
                        if (context.Request.Cookies.TryGetValue("ct", out ct))
                        {
                            context.Request.Headers.Add("currentteamwork", ct);
                        }

                    }
                }
                await _next(context);
            }
            private bool IsDownloadPath(string path)
            {
                if (path.Contains("exportdocument") ||path.Contains("downloadattachments") ||path.Contains("getfinancialaccountofsupplier") || path.Contains("exportexcelfinancialaccounts") || path.Contains("attachment/download") || path.Contains("excelexportproductlogs") || path.Contains("exportexcelproducts") || path.Contains("qualitycontrol/download") || path.Contains("attachments") || path.Contains("downloadattachment") || path.Contains("rfpattachment") || path.Contains("downloadtempfile") || path.Contains("workflowattachment") || path.Contains("downloadrevisionsattachforproduct") || path.Contains("downloadrevisionattachforproduct") || path.Contains("downloadsharefile") || path.Contains("downloadsharedirectory") || path.Contains("downloaddirectory") || path.Contains("downloadfile") || path.Contains("getfileforiframwithauthorization") || path.Contains("sale/v1/contract/attachment/download") || path.Contains("commentattachment"))
                    return true;
                return false;
            }
        }
    }

}
