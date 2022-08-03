using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Dapper;
using System.Xml.Serialization;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using RestSharp;
using Raybod.SCM.DataTransferObject;
using System.Net;
using Raybod.SCM.Utility.Filters;
using Raybod.SCM.DataTransferObject.License;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Builder;
using Raybod.SCM.Services.Utilitys;
using Raybod.SCM.Utility.EnumType;
using Raybod.SCM.Utility.Extention;
using Raybod.SCM.ModuleApi.Helper;
using Newtonsoft.Json;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;

namespace Raybod.SCM.ModuleApi
{
    [Route("api/SCMManagement/v1")]
    [ApiController]
    [SwaggerArea(AreaName = "SCMManagement")]
    public class LicensesController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;


        public LicensesController(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
        }

        // GET: SeriLogs

       
        [HttpPost]
        [Route("Licenses/GetLicense")]
        public async Task<IActionResult> GetLicence(LicenceDTO licenceDto)
        {
           
            ApiResponseDTO result = new ApiResponseDTO();
            LicenceCreateDTO createLicense = new LicenceCreateDTO();
            createLicense.ActivationCode = licenceDto.ActivationCode;
            createLicense.Username = licenceDto.Username;
            createLicense.IpAddress = ServerInfo.IpAddress;
            createLicense.Port = ServerInfo.Port;
            var restClient = new RestClient(_configuration["LicenseUrl"]);
            var request = new RestRequest(Method.POST);
            request.Resource = "api/home/generatekey";
            request.AddJsonBody(createLicense);
            var response = restClient.Execute<ApiResponseDTO>(request);
            string fileName = Path.Combine(_webHostEnvironment.ContentRootPath, "Files", "SecurityFile.dll");
            if (System.IO.File.Exists(fileName))
            {
                System.IO.File.Delete(fileName);
            }
           

            if (response.StatusCode == HttpStatusCode.OK  )
            {
                if(response.Data.Status == (short)LicenseStatus.OprationSuccess && response.Data.Data != null)
                {
                    using (FileStream fsWrite = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                    {

                        var responseEncod = Convert.FromBase64String(response.Data.Data.ToString());
                        fsWrite.Write(responseEncod, 0, responseEncod.Length);
                        fsWrite.Close();
                        result.Status = (short)LicenseStatus.OprationSuccess;
                        result.Message = LicenseStatus.OprationSuccess.GetDisplayName();
                       
                        return new JsonResult(result);

                    }
                }
                else
                {
                    if (System.IO.File.Exists(fileName))
                    {
                        System.IO.File.Delete(fileName);
                    }
                    result.Status = response.Data.Status;
                    result.Message = ((LicenseStatus)(response.Data.Status)).GetDisplayName();
                    
                }
                

            }
            else
            {
                if (System.IO.File.Exists(fileName))
                {
                    System.IO.File.Delete(fileName);
                }
               
                result.Status = (short)LicenseStatus.ServerError;
                result.Message = LicenseStatus.ServerError.GetDisplayName();
                
            }
            return new JsonResult(result);
        }

    }
}
