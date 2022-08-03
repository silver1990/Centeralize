using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Raybod.SCM.Utility.FileHelper;
using Raybod.SCM.Services.Core.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using static Raybod.SCM.Services.Core.Common.ServiceSetting;
using Raybod.SCM.DataTransferObject;
using Microsoft.AspNetCore.StaticFiles;
using System.IO.Compression;
using System;
using Raybod.SCM.DataTransferObject.Document;
using Raybod.SCM.Domain.Model;
using Raybod.SCM.DataAccess.Core;
using Raybod.SCM.Services.Core.Common.Message;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using MimeMapping;

namespace Raybod.SCM.Services.Utilitys
{
    internal class FileHelper
    {
        private readonly Utility.FileHelper.FileHelper _fileHelper;
        private readonly IWebHostEnvironment _hostingEnvironmentRoot;


        internal FileHelper(IWebHostEnvironment hostingEnvironmentRoot)
        {
            _fileHelper = new Utility.FileHelper.FileHelper(hostingEnvironmentRoot);
            _hostingEnvironmentRoot = hostingEnvironmentRoot;

        }

        #region Image
        internal string SaveImages(IFormFile image)
        {
            if (image == null) return null;
            string imageName;
            using (var stream = image.OpenReadStream())
            {
                // image.CopyTo(stream); 
                ImageHelper.ResizeImageByWidthAndSaveJpeg(stream, (int)ImageHelper.ImageWidth.FullHD);
                imageName = _fileHelper.SaveCompressImage(image, ServiceSetting.UploadImagesPath.Temp, Utility.FileHelper.FileHelper.ImageComperssion.None);
            }
            return imageName;
        }


        internal string SaveImagesFromTemp(string imageName, string path, int imageSize)
        {
            if (!ImageExistInTemp(imageName))
            {
                return null;
            }
            var image = File.ReadAllBytes(_hostingEnvironmentRoot.ContentRootPath + ServiceSetting.UploadImagesPath.Temp + imageName);
            if (image == null) return null;
            var imageInputStream = new MemoryStream(image);

            ImageHelper.ResizeImageByWidthAndSaveJpeg(imageInputStream, imageSize);
            var file = new FormFile(imageInputStream, 0, image.Length, imageName, imageName);
            imageName = _fileHelper.SaveCompressImage(file, path, imageName, Utility.FileHelper.FileHelper.ImageComperssion.None);
            return imageName;
        }

        internal string SaveImagesFromTemp(string imageName, string path)
        {
            if (!ImageExistInTemp(imageName))
            {
                return null;
            }
            var image = File.ReadAllBytes(_hostingEnvironmentRoot.ContentRootPath + ServiceSetting.UploadImagesPath.Temp + imageName);
            if (image == null) return null;
            var imageInputStream = new MemoryStream(image);

            ImageHelper.ResizeImageByWidthAndSaveJpeg(imageInputStream, (int)ImageHelper.ImageWidth.FullHD);
            var file = new FormFile(imageInputStream, 0, image.Length, imageName, imageName);
            imageName = _fileHelper.SaveCompressImage(file, path, imageName, Utility.FileHelper.FileHelper.ImageComperssion.None);


            return imageName;
        }

        internal string SaveImagesFromTemp(string imageName)
        {
            if (!ImageExistInTemp(imageName))
            {
                return null;
            }
            var image = File.ReadAllBytes(_hostingEnvironmentRoot.ContentRootPath + ServiceSetting.UploadImagesPath.Temp + imageName);
            if (image == null) return null;
            var imageInputStream = new MemoryStream(image);

            ImageHelper.ResizeImageByWidthAndSaveJpeg(imageInputStream, (int)ImageHelper.ImageWidth.FullHD);
            var file = new FormFile(imageInputStream, 0, image.Length, imageName, imageName);
            imageName = _fileHelper.SaveCompressImage(file, ServiceSetting.UploadImagesPath.Fhd, imageName, Utility.FileHelper.FileHelper.ImageComperssion.None);

            return imageName;
        }

        //internal async Task<string> SaveDocumentFromTemp(string documentName)
        //{
        //    if (!FileExistInTemp(documentName))
        //    {
        //        return null;
        //    }
        //    var document = File.ReadAllBytes(_hostingEnvironmentRoot.ContentRootPath + ServiceSetting.UploadFilePath.Temp + documentName);
        //    if (document == null) return null;
        //    var imageInputStream = new MemoryStream(document);

        //    var file = new FormFile(imageInputStream, 0, document.Length, documentName, documentName);
        //    documentName = await _fileHelper.SaveDocumentAsync(file, ServiceSetting.UploadFilePath.document, documentName);

        //    return documentName;
        //}

        internal async Task<FileInfoDto> SaveDocumentFromTemp(string documentName, string path)
        {
            if (!FileExistInTemp(documentName))
            {
                return null;
            }
            var document = File.ReadAllBytes(_hostingEnvironmentRoot.ContentRootPath + ServiceSetting.UploadFilePath.Temp + documentName);
            if (document == null) return null;
            var imageInputStream = new MemoryStream(document);

            var file = new FormFile(imageInputStream, 0, document.Length, documentName, documentName);
            long fileSize = file.Length;
            string fileType = Path.GetExtension(documentName);
            documentName = await _fileHelper.SaveDocumentAsync(file, path, documentName);

            return string.IsNullOrEmpty(documentName)
                ? null
                : new FileInfoDto
                {
                    FileName = documentName,
                    FileSize = fileSize,
                    FileType = fileType
                };
        }

        internal async Task<FileInfoDto> SaveDocument(byte[] files, string documentName, string path)
        {
            var result = new FileInfoDto
            {
                FileName = documentName,
                FileSize = files.Length,
                FileType = Path.GetExtension(documentName)
            };
            documentName = await _fileHelper.SaveFileAsync(files, path, documentName);

            return string.IsNullOrEmpty(documentName)
                ? null
                : result;
        }

        public async Task<DownloadFileDto> DownloadZipFileAsync(List<FinalRevisionAttachmentToZipDto> attachs)
        {
            try
            {
                var files = new List<InMemoryFileDto>();
                foreach (var item in attachs)
                {
                    if (!IsFileExist(item.FileSrc, item.FilePath))
                        return null;

                    files.Add(new InMemoryFileDto
                    {
                        FileName = item.FileName,
                        FileSrc = FileReadSrc(item.FileSrc, item.FilePath)
                    });
                }

                using (MemoryStream zipStream = new MemoryStream())
                {
                    using (ZipArchive zipFile = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                    {
                        foreach (var item in files)
                        {
                            zipFile.CreateEntryFromFile(item.FileSrc, item.FileName, CompressionLevel.Fastest);
                        }
                    }

                    return new DownloadFileDto
                    {
                        Stream = null,
                        // ContentType = GetContentType("attach.zip"),
                        // ContentType = "application/zip",
                        ContentType = "application/octet-stream",
                        FileName = "Attachments",
                        ArchiveFile = zipStream.ToArray()
                    };
                }
            }
            catch (Exception exception)
            {

                return null;
            }
        }

        public async Task<DownloadFileDto> ToMemoryStreamZipFileAsync(List<InMemoryFileDto> attachs)
        {
            try
            {
                foreach (var item in attachs)
                {
                    if (!IsFileExist(item.FileUrl + item.FileSrc))
                        return null;
                }

                using (MemoryStream zipStream = new MemoryStream())
                {
                    using (ZipArchive zipFile = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                    {
                        foreach (var item in attachs)
                        {
                            zipFile.CreateEntryFromFile(FileReadSrc(item.FileSrc, item.FileUrl), item.FileName, CompressionLevel.Fastest);
                        }
                    }

                    return new DownloadFileDto
                    {
                        Stream = zipStream,
                        // ContentType = GetContentType("attach.zip"),
                        // ContentType = "application/zip",
                        ContentType = "application/octet-stream",
                        FileName = "Attachments"
                    };
                }
            }
            catch (Exception exception)
            {

                return null;
            }
        }

        public async Task<DownloadFileDto> ToMemoryStreamZipFileForTransmittalAsync(TransmittalFilesDto attachs)
        {
            try
            {
                if(attachs.RevisionFiles!=null && attachs.RevisionFiles.Count() > 0)
                {
                    foreach (var item in attachs.RevisionFiles)
                    {
                        if (!IsFileExist(item.FileUrl + item.FileSrc))
                            return null;
                    }
                }
                

                using (MemoryStream zipStream = new MemoryStream())
                {
                    using (ZipArchive zipFile = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                    {
                        if (attachs.RevisionFiles != null && attachs.RevisionFiles.Count() > 0)
                        {
                            foreach (var item in attachs.RevisionFiles)
                            {
                                zipFile.CreateEntryFromFile(FileReadSrc(item.FileSrc, item.FileUrl), item.FileName, CompressionLevel.Fastest);
                            }
                        }
                        var zipEntry = zipFile.CreateEntry(attachs.TransmitallFile.FileName);

                        //Get the stream of the attachment
                        using (var originalFileStream = new MemoryStream(attachs.TransmitallFile.ArchiveFile))
                        using (var zipEntryStream = zipEntry.Open())
                        {
                            //Copy the attachment stream to the zip entry stream
                            originalFileStream.CopyTo(zipEntryStream);
                        }
                    }

                    return new DownloadFileDto
                    {
                        Stream = zipStream,
                        // ContentType = GetContentType("attach.zip"),
                        // ContentType = "application/zip",
                        ContentType = "application/octet-stream",
                        FileName = "Attachments"
                    };
                }
            }
            catch (Exception exception)
            {

                return null;
            }
        }
        public async Task<bool> ToMemoryStreamZipFileAsync(List<InMemoryFileDto> attachs, string fileSource)
        {
            try
            {
                foreach (var item in attachs)
                {
                    if (!IsFileExist(item.FileUrl + item.FileSrc))
                        return false;
                }

                using (MemoryStream zipStream = new MemoryStream())
                {
                    using (ZipArchive zipFile = new ZipArchive(zipStream, ZipArchiveMode.Update, true))
                    {
                        foreach (var item in attachs)
                        {
                            zipFile.CreateEntryFromFile(FileReadSrc(item.FileSrc, item.FileUrl), item.FileName, CompressionLevel.Fastest);
                        }
                    }
                    zipStream.Seek(0, SeekOrigin.Begin);
                    using (var stream = new FileStream(fileSource, FileMode.Create))
                    {
                        await zipStream.CopyToAsync(stream);
                    }

                }
                return true;
            }
            catch (Exception ex)
            {
                //DebugLog log = new DebugLog();
                //log.StatusCode = (int)MessageId.FileNotFound;
                //log.Message = ex.Message;
                //log.InnerMessage = ex.InnerException != null ? ex.InnerException.Message : "";
                //log.StackTrace = ex.StackTrace;
                //log.CreateDate = DateTime.Now;
                //await _debugLogRepository.AddAsync(log);
                //await _unitOfWork.SaveChangesAsync();
                return false;
            }
        }

        public async Task<bool> ToMemoryStreamZipFileTransmittalAsync(List<InMemoryFileDto> attachs, string fileSource, DownloadFileDto transmittalFile)
        {
            try
            {
                foreach (var item in attachs)
                {
                    if (!IsFileExist(item.FileUrl + item.FileSrc))
                        return false;
                }

                using (MemoryStream zipStream = new MemoryStream())
                {
                    using (ZipArchive zipFile = new ZipArchive(zipStream, ZipArchiveMode.Update, true))
                    {
                        foreach (var item in attachs)
                        {
                            zipFile.CreateEntryFromFile(FileReadSrc(item.FileSrc, item.FileUrl), item.FileName, CompressionLevel.Fastest);
                        }


                        var zipEntry = zipFile.CreateEntry(transmittalFile.FileName);

                        //Get the stream of the attachment
                        using (var originalFileStream = new MemoryStream(transmittalFile.ArchiveFile))
                        {
                            using (var zipEntryStream = zipEntry.Open())
                            {
                                //Copy the attachment stream to the zip entry stream
                                await originalFileStream.CopyToAsync(zipEntryStream);
                            }


                        }
                       
                    }
                    zipStream.Seek(0, SeekOrigin.Begin);
                    using (var stream = new FileStream(fileSource, FileMode.Create))
                    {
                        await zipStream.CopyToAsync(stream);
                    }

                }
                return true;
            }
            catch (Exception ex)
            {

                return false;
            }
        }
        public async Task<DownloadFileDto> DownloadDocument(string documentName, string path)
        {
            if (!IsFileExist(documentName, path))
                return null;

            var filesrc = FileReadSrc(documentName, path);

            var memory = new MemoryStream();
            using (var stream = new FileStream(filesrc, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            //return new DownloadFileDto { Stream = memory, ContentType = GetContentType(filesrc), FileName = documentName };
            return new DownloadFileDto { Stream = memory, ContentType = "application/octet-stream" };
        }
        public async Task<DownloadFileDto> DownloadDocument(string documentName, string path, string fileName)
        {
            if (!IsFileExist(documentName, path))
                return null;

            var filesrc = FileReadSrc(documentName, path);

            var memory = new MemoryStream();
            using (var stream = new FileStream(filesrc, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            string contentType = MimeUtility.GetMimeMapping(filesrc);
            //return new DownloadFileDto { Stream = memory, ContentType = GetContentType(filesrc), FileName = documentName };
            return new DownloadFileDto { Stream = memory, ContentType = contentType, FileName = fileName };
        }
        public async Task<DownloadFileDto> DownloadDocumentForPreview(string documentName, string path)
        {
            if (!IsFileExist(documentName, path))
                return null;

            var filesrc = FileReadSrc(documentName, path);

            var memory = new MemoryStream();
            using (var stream = new FileStream(filesrc, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            string contentType = MimeUtility.GetMimeMapping(filesrc);
            //return new DownloadFileDto { Stream = memory, ContentType = GetContentType(filesrc), FileName = documentName };
            return new DownloadFileDto { Stream = memory, ContentType = contentType };
        }
        public async Task<DownloadFileDto> DownloadFileDriveDocument(string path, string documentName)
        {
            if (!IsFileExist(documentName, path))
                return null;

            var filesrc = FileReadSrc(documentName, path);

            var memory = new MemoryStream();
            using (var stream = new FileStream(filesrc, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            string contentType = MimeUtility.GetMimeMapping(filesrc);
            //return new DownloadFileDto { Stream = memory, ContentType = GetContentType(filesrc), FileName = documentName };
            return new DownloadFileDto { Stream = memory, ContentType = contentType, FileName = documentName };
        }

        public string PreviewFileDriveDocument(string path, string documentName)
        {
            if (!IsFileExist(documentName, path))
                return null;

            var filesrc = FileReadSrc(documentName, path);


            //return new DownloadFileDto { Stream = memory, ContentType = GetContentType(filesrc), FileName = documentName };
            return filesrc;
        }

        public async Task<DownloadFileDto> DownloadFileDriveFolder(string path)
        {
            if (!IsDirectoryExist(path))
                return null;

            var filesrc = FileReadSrc(path);
            DirectoryInfo root = new DirectoryInfo(filesrc);
            var memory = new MemoryStream();
            using (var zipArchive = new ZipArchive(memory, ZipArchiveMode.Update, true))
            {
                await ZipUpDirectory(zipArchive, filesrc, root.Name + "\\");
            }


            memory.Position = 0;
            //return new DownloadFileDto { Stream = memory, ContentType = GetContentType(filesrc), FileName = documentName };
            return new DownloadFileDto { Stream = memory, ContentType = "application/zip", FileName = root.Name + ".zip" };
        }
        public async Task<DownloadFileDto> DownloadProductDocument(List<FinalRevisionsAttachmentToZipDto> attachs)
        {
            try
            {
                var files = new List<InMemoryFileDto>();
                foreach (var item in attachs)
                {
                    if (!IsFileExist(item.FileSrc, item.FilePath))
                        return null;

                    files.Add(new InMemoryFileDto
                    {
                        FileName = item.FileName,
                        FileSrc = FileReadSrc(item.FileSrc, item.FilePath)
                    });
                }

                MemoryStream zipStream = new MemoryStream();
                {
                    using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                    {
                        foreach (var item in attachs)
                        {
                            var entry = archive.CreateEntry(item.DocNumber + "\\" + item.FileName, CompressionLevel.Fastest);
                            using (var entryStream = entry.Open())
                            await using (var fileStream = System.IO.File.OpenRead(FileReadSrc(item.FileSrc, item.FilePath)))
                            {
                                await fileStream.CopyToAsync(entryStream);
                            }
                        }
                    }

                }
                zipStream.Position = 0;
                return new DownloadFileDto { Stream = zipStream, ContentType = "application/zip", FileName = "Attachment.zip" };
            }
            catch (Exception exception)
            {

                return null;
            }


            //return new DownloadFileDto { Stream = memory, ContentType = GetContentType(filesrc), FileName = documentName };

        }
        public async Task<DownloadFileDto> DownloadFileDriveShareFolder(string path, DbSet<FileDriveDirectory> directoryRepository, DbSet<FileDriveShare> shareRepository, DbSet<FileDriveFile> fileRepository, int userId)
        {
            if (!IsDirectoryExist(path))
                return null;

            var filesrc = FileReadSrc(path);
            DirectoryInfo root = new DirectoryInfo(filesrc);
            var memory = new MemoryStream();
            using (var zipArchive = new ZipArchive(memory, ZipArchiveMode.Update, true))
            {
                await ZipUpShareDirectory(zipArchive, filesrc, root.Name + "\\", directoryRepository, shareRepository, fileRepository, path, userId);
            }


            memory.Position = 0;
            //return new DownloadFileDto { Stream = memory, ContentType = GetContentType(filesrc), FileName = documentName };
            return new DownloadFileDto { Stream = memory, ContentType = "application/zip", FileName = root.Name + ".zip" };
        }
        public async Task<DownloadFileDto> DownloadDocument(List<InMemoryFileDto> native, List<InMemoryFileDto> files)
        {
            if (native != null)
                foreach (var item in native)
                {
                    if (!File.Exists(item.FileSrc))
                        return null;
                }
            foreach (var item in files)
            {
                if (!File.Exists(item.FileSrc))
                    return null;
            }



            var zipFileMemoryStream = new MemoryStream();
            using (ZipArchive archive = new ZipArchive(zipFileMemoryStream, ZipArchiveMode.Update, leaveOpen: true))
            {
                if (native != null)
                {


                    foreach (var file in native)
                    {

                        archive.CreateEntryFromFile(file.FileSrc, file.FileName, CompressionLevel.Fastest);
                    }

                    foreach (var file in files)
                    {

                        archive.CreateEntryFromFile(file.FileSrc, file.FileName, CompressionLevel.Fastest);
                    }

                }
                else
                {
                    foreach (var file in files)
                    {

                        archive.CreateEntryFromFile(file.FileSrc, file.FileName, CompressionLevel.Fastest);
                    }
                }

            }

            zipFileMemoryStream.Seek(0, SeekOrigin.Begin);
            //return new DownloadFileDto { Stream = memory, ContentType = GetContentType(filesrc), FileName = documentName };
            return new DownloadFileDto { Stream = zipFileMemoryStream, ContentType = "application/octet-stream" };
        }
        public async Task<DownloadFileDto> DownloadDocumentFromTempAsync(string documentName)
        {
            if (!FileExistInTemp(documentName))
                return null;

            var filesrc = FileReadSrc() + documentName;

            var memory = new MemoryStream();
            using (var stream = new FileStream(filesrc, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return new DownloadFileDto { Stream = memory, ContentType = "application/octet-stream" };
        }
        public async Task<DownloadFileDto> DownloadDocumentFromTempAsync(string documentName, string fileName)
        {
            if (!FileExistInTemp(documentName))
                return null;

            var filesrc = FileReadSrc() + documentName;

            var memory = new MemoryStream();
            using (var stream = new FileStream(filesrc, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            string contentType = MimeUtility.GetMimeMapping(filesrc);
            return new DownloadFileDto { Stream = memory, ContentType = contentType, FileName = fileName };
        }
        public async Task<DownloadFileDto> DownloadAttachmentDocument(string documentName, FileSection fileSection, string fileName)
        {
            if (!IsFileExist(documentName, fileSection))
                return null;

            var filesrc = FileReadSrc(fileSection) + documentName;

            var memory = new MemoryStream();
            using (var stream = new FileStream(filesrc, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }

            memory.Position = 0;
            string contentType = MimeUtility.GetMimeMapping(filesrc);
            return new DownloadFileDto { Stream = memory, ContentType = contentType, FileName = fileName };
        }
        
        public async Task<DownloadFileDto> DownloadDocument(string documentName, FileSection fileSection)
        {
            if (!IsFileExist(documentName, fileSection))
                return null;

            var filesrc = FileReadSrc(fileSection) + documentName;

            var memory = new MemoryStream();
            using (var stream = new FileStream(filesrc, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            //return new DownloadFileDto { Stream = memory, ContentType = GetContentType(filesrc), FileName = documentName };
            return new DownloadFileDto { Stream = memory, ContentType = "application/octet-stream" };
        }
        public async Task<DownloadFileDto> DownloadDocumentWithContentType(string documentName, FileSection fileSection)
        {
            if (!IsFileExist(documentName, fileSection))
                return null;

            var filesrc = FileReadSrc(fileSection) + documentName;

            var memory = new MemoryStream();
            using (var stream = new FileStream(filesrc, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            string contentType = MimeUtility.GetMimeMapping(filesrc);
            //return new DownloadFileDto { Stream = memory, ContentType = GetContentType(filesrc), FileName = documentName };
            return new DownloadFileDto { Stream = memory, ContentType = contentType, FileName = documentName };
            //return new DownloadFileDto { Stream = memory, ContentType = GetContentType(filesrc), FileName = documentName };

        }

        private string GetContentType(string path)
        {
            var provider = new FileExtensionContentTypeProvider();
            string contentType;
            if (!provider.TryGetContentType(path, out contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
        }

        public bool ImageExistInTemp(string imageName)
        {
            return File.Exists(_hostingEnvironmentRoot.ContentRootPath + ServiceSetting.UploadImagesPath.Temp + imageName);
        }

        internal bool FileExistInTemp(string imageName)
        {
            return File.Exists(_hostingEnvironmentRoot.ContentRootPath + ServiceSetting.UploadFilePath.Temp + imageName);
        }

        public bool FileExistInTemp(List<string> documents)
        {
            foreach (var imageName in documents)
            {
                if (!File.Exists(
                    _hostingEnvironmentRoot.ContentRootPath + ServiceSetting.UploadFilePath.Temp + imageName))
                    return false;
            }

            return true;
        }

        public bool IsFileExist(string imageName, string path)
        {
            return File.Exists(_hostingEnvironmentRoot.ContentRootPath + path + imageName);
        }
        public bool IsDirectoryExist(string path)
        {
            return Directory.Exists(_hostingEnvironmentRoot.ContentRootPath + path);
        }
        public bool IsFileExist(string fullPath)
        {
            return File.Exists(_hostingEnvironmentRoot.ContentRootPath + fullPath);
        }

        public bool IsFileExist(string imageName, FileSection sectionFile)
        {
            switch (sectionFile)
            {
                //case FileSection.EngineeringDocument:
                //    return File.Exists(_hostingEnvironmentRoot.ContentRootPath + ServiceSetting.UploadFilePath.EngineeringDocument + imageName);
                case FileSection.RFP:
                    return File.Exists(_hostingEnvironmentRoot.ContentRootPath + ServiceSetting.UploadFilePath.RFP + imageName);
                case FileSection.RFPSupplier:
                    return File.Exists(_hostingEnvironmentRoot.ContentRootPath + ServiceSetting.UploadFilePath.RFPSupplier + imageName);
                case FileSection.PRContract:
                    return File.Exists(_hostingEnvironmentRoot.ContentRootPath + ServiceSetting.UploadFilePath.PrContract + imageName);
                case FileSection.PO:
                    return File.Exists(_hostingEnvironmentRoot.ContentRootPath + ServiceSetting.UploadFilePath.PO + imageName);
                case FileSection.POComment:
                    return File.Exists(_hostingEnvironmentRoot.ContentRootPath + ServiceSetting.UploadFilePath.POComment + imageName);
                case FileSection.ContractDocument:
                    return File.Exists(_hostingEnvironmentRoot.ContentRootPath + ServiceSetting.UploadFilePath.ContractDocument + imageName);
                case FileSection.PR:
                    return File.Exists(_hostingEnvironmentRoot.ContentRootPath + ServiceSetting.UploadFilePath.PR + imageName);
                case FileSection.RFPComment:
                    return File.Exists(_hostingEnvironmentRoot.ContentRootPath + ServiceSetting.UploadFilePath.RFPComment + imageName);
                case FileSection.Invoice:
                    return File.Exists(_hostingEnvironmentRoot.ContentRootPath + ServiceSetting.UploadFilePath.Invoice + imageName);
                case FileSection.Payment:
                    return File.Exists(_hostingEnvironmentRoot.ContentRootPath + ServiceSetting.UploadFilePath.Payment + imageName);
                case FileSection.PoIncpection:
                    return File.Exists(_hostingEnvironmentRoot.ContentRootPath + ServiceSetting.UploadFilePath.POInspection + imageName);
                case FileSection.ManufactureDocument:
                    return File.Exists(_hostingEnvironmentRoot.ContentRootPath + ServiceSetting.UploadFilePath.POManufactureDocument + imageName);
                default:
                    return false;
            }

        }

        internal void DeleteImages(string imagePath)
        {
            _fileHelper.DeleteFile(_hostingEnvironmentRoot.ContentRootPath + ServiceSetting.UploadImagesPath.Large + imagePath);
            _fileHelper.DeleteFile(_hostingEnvironmentRoot.ContentRootPath + ServiceSetting.UploadImagesPath.Medium + imagePath);
            _fileHelper.DeleteFile(_hostingEnvironmentRoot.ContentRootPath + ServiceSetting.UploadImagesPath.Small + imagePath);
            _fileHelper.DeleteFile(_hostingEnvironmentRoot.ContentRootPath + ServiceSetting.UploadImagesPath.Fhd + imagePath);
        }

        //internal void DeleteImageSupplier(string imagePath)
        //{
        //    _fileHelper.DeleteFile(_hostingEnvironmentRoot.ContentRootPath + ServiceSetting.UploadImagesPath.SupplierSmall + imagePath);
        //    _fileHelper.DeleteFile(_hostingEnvironmentRoot.ContentRootPath + ServiceSetting.UploadImagesPath.SupplierLarge + imagePath);
        //    _fileHelper.DeleteFile(_hostingEnvironmentRoot.ContentRootPath + ServiceSetting.UploadImagesPath.SupplierSignatureLarge + imagePath);
        //}

        internal void DeleteImageUser(string imagePath)
        {
            _fileHelper.DeleteFile(_hostingEnvironmentRoot.ContentRootPath + ServiceSetting.UploadImagesPath.UserLarge + imagePath);
            _fileHelper.DeleteFile(_hostingEnvironmentRoot.ContentRootPath + ServiceSetting.UploadImagesPath.UserSmall + imagePath);
        }
        internal void DeleteImagesFromTemp(string imagePath)
        {
            _fileHelper.DeleteFile(_hostingEnvironmentRoot.ContentRootPath + ServiceSetting.UploadImagesPath.Temp + imagePath);
        }
        #endregion

        #region document

        internal async Task<string> SaveDocument(IFormFile document)
        {
            if (document == null) return null;
            string fileName;
            fileName = await _fileHelper.SaveDocumentFile(document, ServiceSetting.UploadFilePath.Temp);
            //using (var stream = document.OpenReadStream())
            //{
            //}
            return fileName;
        }

        public string FileReadSrc()
        {
            var directory = _hostingEnvironmentRoot.ContentRootPath + ServiceSetting.UploadFilePath.Temp;
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            return directory;
        }

        public string CreateDirectory(string rootPath, string directoryName)
        {
            var directory = _hostingEnvironmentRoot.ContentRootPath + rootPath + directoryName;
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            return rootPath + directoryName + "/";
        }
        public string CreateRootDirectory(string rootPath)
        {
            var directory = _hostingEnvironmentRoot.ContentRootPath + rootPath.Substring(0, rootPath.Length - 1);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            return rootPath;
        }
        public string RemoveDirectory(string rootPath, string directoryName)
        {
            var directory = _hostingEnvironmentRoot.ContentRootPath + rootPath + directoryName;
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory);
            }
            return directory;
        }
        public bool ValidateTitle(string title)
        {
            if (title.Contains("\\")
                || title.Contains("/")
                || title.Contains("\"")
                || title.Contains(":")
                || title.Contains("*")
                || title.Contains("?")
                || title.Contains("<")
                || title.Contains(">")
                || title.Contains("|"))
            {
                return false;
            }
            else
                return true;
        }
        public string RenameDirectory(string rootPath, string sourceDirectory, string destinationDirectory, string oldName)
        {
            var source = _hostingEnvironmentRoot.ContentRootPath + sourceDirectory;
            var paths = sourceDirectory.Split('/', StringSplitOptions.RemoveEmptyEntries);
            paths[paths.Length - 1] = destinationDirectory;

            var destination = _hostingEnvironmentRoot.ContentRootPath + "/" + String.Join('/', paths) + "/";

            if (Directory.Exists(source))
            {
                Directory.Move(source, destination);
            }
            return "/" + String.Join('/', paths) + "/";
        }

        public string MoveDirectory(string sourceDirectory, string destinationDirectory, string name)
        {
            var source = _hostingEnvironmentRoot.ContentRootPath + sourceDirectory;
            var destination = _hostingEnvironmentRoot.ContentRootPath + destinationDirectory + name + "/";

            if (Directory.Exists(source))
            {
                Directory.Move(source, destination);
            }
            return destinationDirectory + name + "/";
        }
        public string CopyDirectory(string sourceDirectory, string destinationDirectory, string name)
        {
            var source = _hostingEnvironmentRoot.ContentRootPath + sourceDirectory;
            var destination = _hostingEnvironmentRoot.ContentRootPath + destinationDirectory + name + "/";

            if (Directory.Exists(source))
            {
                DirectoryCopy(source, destination, true);
            }
            return destinationDirectory + name + "/";
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.       
            Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }


        private async Task ZipUpDirectory(ZipArchive archive, string path, string root)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(path);

            archive.CreateEntry(root, CompressionLevel.Fastest);
            DirectoryInfo[] dirs = dir.GetDirectories();
            FileInfo[] files = dir.GetFiles();
            foreach (var file in files)
            {

                var entry = archive.CreateEntry(root + file.Name, CompressionLevel.Fastest);
                using (var entryStream = entry.Open())
                await using (var fileStream = System.IO.File.OpenRead(file.FullName))
                {
                    await fileStream.CopyToAsync(entryStream);
                }
            }
            foreach (DirectoryInfo subdir in dirs)
            {

                await ZipUpDirectory(archive, subdir.FullName, root + subdir.Name + "\\");
            }


        }
        private async Task ZipUpShareDirectory(ZipArchive archive, string path, string root, DbSet<FileDriveDirectory> directoryRepository, DbSet<FileDriveShare> shareRepository, DbSet<FileDriveFile> fileRepository, string directoryPath, int userId)
        {
            // Get the subdirectories for the specified directory.
            var directory = await directoryRepository.Include(a => a.Shares).Include(a => a.Directories).FirstOrDefaultAsync(a => !a.IsDeleted && a.DirectoryPath == directoryPath);
            if (directory.Shares != null && directory.Shares.Any(a => !a.IsDeleted && a.UserId == userId))
            {
                DirectoryInfo dir = new DirectoryInfo(path);

                archive.CreateEntry(root, CompressionLevel.Fastest);
                DirectoryInfo[] dirs = dir.GetDirectories();
                FileInfo[] files = dir.GetFiles();
                var shareFiles = await fileRepository.Include(a => a.Shares).Where(a => !a.IsDeleted && a.DirectoryId == directory.DirectoryId).ToListAsync();
                foreach (var file in files)
                {

                    if (shareFiles.Any(a => file.Name == a.FileName && a.Shares.Any(a => !a.IsDeleted && a.UserId == userId)))
                    {
                        var entry = archive.CreateEntry(root + file.Name + file.Extension, CompressionLevel.Fastest);
                        using (var entryStream = entry.Open())
                        await using (var fileStream = System.IO.File.OpenRead(file.FullName))
                        {
                            await fileStream.CopyToAsync(entryStream);
                        }
                    }

                }
                foreach (DirectoryInfo subdir in dirs)
                {
                    directoryPath = directory.Directories.FirstOrDefault(a => a.DirectoryName == subdir.Name && !a.IsDeleted).DirectoryPath;
                    await ZipUpShareDirectory(archive, subdir.FullName, root + subdir.Name + "\\", directoryRepository, shareRepository, fileRepository, directoryPath, userId);
                }
            }



        }
        public string MoveDirectory(string sourceDirectory, string destinationDirectory)
        {
            var source = _hostingEnvironmentRoot.ContentRootPath + sourceDirectory;
            var destination = _hostingEnvironmentRoot.ContentRootPath + destinationDirectory;

            if (Directory.Exists(source))
            {
                Directory.Move(source, destination);
            }
            return destination + "/";
        }
        public string RemoveRootDirectory(string rootPath)
        {
            var directory = _hostingEnvironmentRoot.ContentRootPath + rootPath.Substring(rootPath.Length - 1);
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory);
            }
            return directory;
        }


        public bool RestoreDirectoryFromTrash(string destinationPath, string name, string contractCode)
        {
            try
            {
                var source = _hostingEnvironmentRoot.ContentRootPath + ServiceSetting.FileDriveTrashPath(contractCode) + name;
                var destination = _hostingEnvironmentRoot.ContentRootPath + destinationPath;

                if (Directory.Exists(source))
                {
                    Directory.Move(source, destination);
                }
                return true;
            }
            catch
            {
                return false;
            }

        }
        public string MoveDirectoryToTrash(string sourceDirectory, string name, string contractCode)
        {
            var source = _hostingEnvironmentRoot.ContentRootPath + sourceDirectory;
            var trashCan = _hostingEnvironmentRoot.ContentRootPath + ServiceSetting.FileDriveTrashPath(contractCode);
            if (!Directory.Exists(trashCan))
                Directory.CreateDirectory(trashCan);
            var destination = trashCan + name;

            if (Directory.Exists(source))
            {
                Directory.Move(source, destination);
            }
            return ServiceSetting.FileDriveTrashPath(contractCode) + name;
        }
        public string FileReadSrc(string fileName, string path)
        {
            var directory = _hostingEnvironmentRoot.ContentRootPath + path;
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            return directory + fileName;
        }
        public string FileReadSrcForEmailAttachemnt(string fileName, string path)
        {
            var directory = _hostingEnvironmentRoot.ContentRootPath + path;

            return directory + fileName;
        }
        public string FileReadSrc(string fullPath)
        {
            var directory = _hostingEnvironmentRoot.ContentRootPath + fullPath;

            return directory;
        }

        public string ReturnConentRootPath()
        {
            return _hostingEnvironmentRoot.ContentRootPath;

        }

        public string FileReadSrc(FileSection fileSection)
        {
            switch (fileSection)
            {
                //case FileSection.EngineeringDocument:
                //    return _hostingEnvironmentRoot.ContentRootPath + ServiceSetting.UploadFilePath.EngineeringDocument;
                case FileSection.RFP:
                    return _hostingEnvironmentRoot.ContentRootPath + ServiceSetting.UploadFilePath.RFP;
                case FileSection.RFPSupplier:
                    return _hostingEnvironmentRoot.ContentRootPath + ServiceSetting.UploadFilePath.RFPSupplier;
                case FileSection.PRContract:
                    return _hostingEnvironmentRoot.ContentRootPath + ServiceSetting.UploadFilePath.PrContract;
                case FileSection.PO:
                    return _hostingEnvironmentRoot.ContentRootPath + ServiceSetting.UploadFilePath.PO;
                case FileSection.POComment:
                    return _hostingEnvironmentRoot.ContentRootPath + ServiceSetting.UploadFilePath.POComment;
                case FileSection.ContractDocument:
                    return _hostingEnvironmentRoot.ContentRootPath + ServiceSetting.UploadFilePath.ContractDocument;
                case FileSection.PR:
                    return _hostingEnvironmentRoot.ContentRootPath + ServiceSetting.UploadFilePath.PR;
                case FileSection.RFPComment:
                    return _hostingEnvironmentRoot.ContentRootPath + ServiceSetting.UploadFilePath.RFPComment;
                case FileSection.Payment:
                    return _hostingEnvironmentRoot.ContentRootPath + ServiceSetting.UploadFilePath.Payment;
                case FileSection.Invoice:
                    return _hostingEnvironmentRoot.ContentRootPath + ServiceSetting.UploadFilePath.Invoice;
                case FileSection.PoIncpection:
                    return _hostingEnvironmentRoot.ContentRootPath + ServiceSetting.UploadFilePath.POInspection;
                case FileSection.ManufactureDocument:
                    return _hostingEnvironmentRoot.ContentRootPath + ServiceSetting.UploadFilePath.POManufactureDocument;
                default:
                    break;
            }
            return _hostingEnvironmentRoot.ContentRootPath + ServiceSetting.UploadFilePath.Temp;
        }

        internal void DeleteDocumentFromTemp(string documentPath)
        {
            _fileHelper.DeleteFile(_hostingEnvironmentRoot.ContentRootPath + ServiceSetting.UploadFilePath.Temp + documentPath);
        }

        internal void DeleteDocumentFromPath(string documentPath)
        {
            _fileHelper.DeleteFile(_hostingEnvironmentRoot.ContentRootPath + documentPath);
        }
        internal void DeleteDirectoryFromPath(string documentPath)
        {
            _fileHelper.DeleteDirectory(_hostingEnvironmentRoot.ContentRootPath + documentPath);
        }
        #endregion
        public long GetFileSizeSumFromDirectory(string searchDirectory)
        {
            return GetFileSizeSumFromDirectoryRec(_hostingEnvironmentRoot.ContentRootPath + searchDirectory);
        }
        private long GetFileSizeSumFromDirectoryRec(string searchDirectory)
        {
            long currentSize = 0;
            long subDirSize = 0;
            try
            {
                var files = Directory.EnumerateFiles(searchDirectory);

                // get the sizeof all files in the current directory
                currentSize = (from file in files let fileInfo = new FileInfo(file) select fileInfo.Length).Sum();

                var directories = Directory.EnumerateDirectories(searchDirectory);

                // get the size of all files in all subdirectories
                subDirSize = (from directory in directories select GetFileSizeSumFromDirectoryRec(directory)).Sum();

            }
            catch
            {

            }
            return currentSize + subDirSize;
        }
        public async Task<bool> FileDriveSaveDocument(IFormFile file, string path, string name)
        {
            if (!file.IsDocumentExtentionValid())
                return false;

            if (!file.IsDocumentSizeValid())
                return false;
            return await _fileHelper.FileDriveSaveDocumentAsync(file, path, name);
        }
        public async Task<bool> FileDriveSaveDocumentInUploadFolder(IFormFile file, string path, string name)
        {
            if (!file.IsDocumentExtentionValid())
                return false;

            if (!file.IsDocumentSizeValid())
                return false;
            return await _fileHelper.FileDriveSaveDocumentUploadFolderAsync(file, path, name);
        }
        public bool FileDriveRemoveDocument(string path, string name)
        {
            return _fileHelper.FileDriveRemoveDocument(path, name);
        }
        public int MyProperty { get; set; }


        public bool MoveFileToTrash(string sourceDirectory, string name, string contractCode)
        {
            try
            {
                var source = _hostingEnvironmentRoot.ContentRootPath + sourceDirectory;
                var trashCan = _hostingEnvironmentRoot.ContentRootPath + ServiceSetting.FileDriveTrashPath(contractCode);
                if (!Directory.Exists(trashCan))
                    Directory.CreateDirectory(trashCan);
                var destination = trashCan + name;

                if (File.Exists(source))
                {
                    File.Move(source, destination);
                }
                return true;
            }
            catch
            {
                return false;
            }

        }
        public bool RestoreFileFromTrash(string destinationPath, string name, string contractCode)
        {
            try
            {
                var source = _hostingEnvironmentRoot.ContentRootPath + ServiceSetting.FileDriveTrashPath(contractCode) + name;
                var destination = _hostingEnvironmentRoot.ContentRootPath + destinationPath;

                if (File.Exists(source))
                {
                    File.Move(source, destination);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool RenameFile(string rootPath, string oldName, string newName)
        {
            try
            {
                var source = _hostingEnvironmentRoot.ContentRootPath + rootPath + oldName;
                var destination = _hostingEnvironmentRoot.ContentRootPath + rootPath + newName + Path.GetExtension(oldName);

                if (File.Exists(source))
                {
                    File.Move(source, destination);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public bool MoveFile(string sourceDirectory, string destinationDirectory, string distinationName,string sourceName)
        {
            try
            {
                var source = _hostingEnvironmentRoot.ContentRootPath + sourceDirectory + sourceName;
                var destination = _hostingEnvironmentRoot.ContentRootPath + destinationDirectory + distinationName;

                if (File.Exists(source))
                {
                    File.Move(source, destination);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool CopyFile(string sourceDirectory, string destinationDirectory, string distinationName,string sourceName)
        {
            try
            {
                var source = _hostingEnvironmentRoot.ContentRootPath + sourceDirectory + sourceName;
                var destination = _hostingEnvironmentRoot.ContentRootPath + destinationDirectory + distinationName;

                if (File.Exists(source))
                {
                    File.Copy(source, destination);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
