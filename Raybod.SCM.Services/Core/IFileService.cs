using Microsoft.AspNetCore.Http;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.Services.Core.Common;
using System.Threading.Tasks;

namespace Exon.TheWeb.Service.Core
{
    public interface IFileService
    {
        Task<ServiceResult<string>> UploadImageFile(IFormFile file);

        Task<ServiceResult<string>> UploadDocumentFile(IFormFile file);

        Task<DownloadFileDto> DownloadDocumentFileAsync(string fileSrc);
        Task<DownloadFileDto> DownloadTempDocumentFileAsync(string fileSrc,string fileName);
    }
}