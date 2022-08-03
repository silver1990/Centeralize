using Microsoft.AspNetCore.Hosting;
using RestSharp;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.Services.Core;
using Microsoft.Extensions.Configuration;
using Raybod.SCM.DataTransferObject.License;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Raybod.SCM.Services.Utilitys
{
    public  class  CheckLicenseKey
    {
        private readonly ISecurity _security;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;


        public CheckLicenseKey(ISecurity security, IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _security = security;
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;

        }

        public async Task CheckLicense()
        {
            var licence = _security.DecryptFile();
            if(licence != null&&ServerInfo.AlreadySet)
            {
                LicenceCreateDTO licenceDto=new LicenceCreateDTO();
                licenceDto.IpAddress = ServerInfo.IpAddress;
                licenceDto.Port = ServerInfo.Port;
                licenceDto.Username = licence.Username;
                licenceDto.ActivationCode = licence.ActiveCode;
                var restClient = new RestClient(_configuration["LicenseUrl"]);
                var request = new RestRequest(Method.POST);
                request.Resource = "api/home/CheckLicenseKey";
                request.AddJsonBody(licenceDto);
                var response = restClient.Execute<ApiResponseDTO>(request);
                string fileName = Path.Combine(_webHostEnvironment.ContentRootPath, "Files", "SecurityFile.dll");
                if (response.StatusCode == HttpStatusCode.OK && response.Data.Status==0&&response.Data.Data != null)
                {
                    using (FileStream fsWrite = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                    {

                        var result = Convert.FromBase64String(response.Data.Data.ToString());
                        fsWrite.Write(result, 0, result.Length);
                        fsWrite.Close();


                    }

                }
                else
                {
                    if (System.IO.File.Exists(fileName))
                    {
                        System.IO.File.Delete(fileName);
                    }

                }

            }
            
        }
    }

   
    
}
