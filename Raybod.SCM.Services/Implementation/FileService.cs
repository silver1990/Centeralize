using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Raybod.SCM.Utility.FileHelper;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using FileHelper = Raybod.SCM.Services.Utilitys.FileHelper;
using Exon.TheWeb.Service.Core;
using Raybod.SCM.DataTransferObject;

namespace Raybod.SCM.Services.Implementation
{
    public class FileService : IFileService
    {
        private readonly FileHelper _fileHelper;
        public FileService(IWebHostEnvironment hostingEnvironmentRoot)
        {

            _fileHelper = new FileHelper(hostingEnvironmentRoot);
        }

        public async Task<ServiceResult<string>> UploadImageFile(IFormFile file)
        {
            var messages = new List<ServiceMessage>();
            string fileName = string.Empty;
            try
            {
                if (file == null)
                    return ServiceResultFactory.CreateError("", MessageId.ModelStateInvalid);

                if (!file.IsImage())
                    return ServiceResultFactory.CreateError("", MessageId.InvalidFileExtention);

                fileName = _fileHelper.SaveImages(file);
                return ServiceResultFactory.CreateSuccess(fileName);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException("", exception);
            }
        }
        public async Task<ServiceResult<string>> UploadDocumentFile(IFormFile file)
        {
            var messages = new List<ServiceMessage>();
            string fileName = string.Empty;
            try
            {
                if (file == null)
                    return ServiceResultFactory.CreateError("", MessageId.ModelStateInvalid);

                if (!file.IsDocumentExtentionValid())
                    return ServiceResultFactory.CreateError("", MessageId.InvalidFileExtention);

                if (!file.IsDocumentSizeValid())
                    return ServiceResultFactory.CreateError("", MessageId.FileSizeError);

                fileName = await _fileHelper.SaveDocument(file);

                return ServiceResultFactory.CreateSuccess(fileName);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException("", exception);
            }
        }

        public async Task<DownloadFileDto> DownloadDocumentFileAsync(string fileSrc)
        {
            return await _fileHelper.DownloadDocumentFromTempAsync(fileSrc);
        }
        public async Task<DownloadFileDto> DownloadTempDocumentFileAsync(string fileSrc,string fileName)
        {
            return await _fileHelper.DownloadDocumentFromTempAsync(fileSrc,fileName);
        }
    }
}