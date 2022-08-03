using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Raybod.SCM.Services.Utilitys;
using Raybod.SCM.Utility.EnumType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Core
{
    public class CheckLicence
    {
        private readonly RequestDelegate _next;
        private readonly ISecurity _security;

        public CheckLicence(RequestDelegate next, ISecurity security)
        {
            _next = next;
            _security = security;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!ServerInfo.AlreadySet)
            {
                var feature = context.Features.Get<IHttpConnectionFeature>();
                ServerInfo.IpAddress = feature?.LocalIpAddress?.ToString();
                ServerInfo.Port = (feature != null) ? feature.LocalPort : 0;
                ServerInfo.AlreadySet = true;
            }
            
                var licence = _security.DecryptFile();
                if (licence == null)
                {
                    try
                    {
                        context.Request.Headers.Add("license header", ((short)LicenseStatus.LicenseNotValid).ToString());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }

                else
                {
                   
                    if (licence.IpAddress != ServerInfo.IpAddress)
                    {
                        context.Request.Headers.Add("license header", ((short)LicenseStatus.IpAndPortNotValid).ToString());
                    }
                if (licence.Port != ServerInfo.Port)
                {
                    context.Request.Headers.Add("license header", ((short)LicenseStatus.IpAndPortNotValid).ToString());
                }
                if (licence.ExpireDate < DateTime.Now)
                    {
                        context.Request.Headers.Add("license header", ((short)LicenseStatus.LicenseExpire).ToString());
                    }
                    //else if (licence.Licence != licence.Username)
                    //{
                    //    context.Request.Headers.Add("license header", ((short)LicenseStatus.LicenseNotValid).ToString());
                    //}
                }

            
         
            await _next(context);
        }
        
    }
}
