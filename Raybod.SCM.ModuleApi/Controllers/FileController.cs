using System.Threading.Tasks;
using Exon.TheWeb.Service.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Raybod.SCM.ModuleApi.Helper;
using Raybod.SCM.Utility.Filters;

namespace Raybod.SCM.ModuleApi.Controllers
{
    [Route("api/raybodSCM/file")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "raybodPanel")]
    public class FileController : ControllerBase
    {
        private readonly IFileService _fileService;

        public FileController(IFileService fileService,IHttpContextAccessor httpContextAccessor)
        {
            _fileService = fileService;
           
               
           
        }

        /// <summary>
        /// upload image file to temp
        /// </summary>
        /// <returns></returns>
        [Route("uploadImage")]
        [HttpPost]
        public async Task<object> PostImage()
        {
           
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            var file = HttpContext.Request.Form.Files[0];
            var result = await _fileService.UploadImageFile(file);
            return result.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// upload document file to temp
        /// </summary>
        /// <returns></returns>
        [Route("uploadDocument")]
        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 104857600)]
        [RequestSizeLimit(104857600)]
        public async Task<object> PostDocument()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            var file = HttpContext.Request.Form.Files[0];
            var result = await _fileService.UploadDocumentFile(file);
            return result.ToWebApiResultVCore(authenticate.language);
        }
        /// <summary>
        /// download document temp file
        /// </summary>
        /// <param name="fileSrc"></param>
        /// <returns></returns>
        [Route("downloadFile")]
        [HttpGet]
        public async Task<object> DownloadFileAsync(string fileSrc)
        {
            var streamResult = await _fileService.DownloadDocumentFileAsync(fileSrc);
            if (streamResult == null)
                return NotFound();

            return File(streamResult.Stream, "application/octet-stream");            
        }
        [Route("downloadTempFile")]
        [HttpGet]
        public async Task<object> DownloadTempFileAsync(string fileSrc,string fileName)
        {
            var streamResult = await _fileService.DownloadTempDocumentFileAsync(fileSrc,fileName);
            if (streamResult == null)
                return NotFound();

            return File(streamResult.Stream, streamResult.ContentType,streamResult.FileName);
        }
    }
}