using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Raybod.SCM.Utility.FileHelper
{
    public class FileHelper
    {
        private const string TempPath = "/Files/Temp/";
        private readonly IWebHostEnvironment _hostingEnvironmentRoot;

        public FileHelper(IWebHostEnvironment hostingEnvironmentRoot)
        {
            _hostingEnvironmentRoot = hostingEnvironmentRoot;
        }

        public enum ImageComperssion
        {
            Maximum = 50,
            Good = 60,
            Normal = 70,
            Fast = 80,
            Minimum = 90,
            None = 100,
        }

        public async Task<string> SaveImageAsync(IFormFile image)
        {
            if (!IsImageSizeValid(image) || !IsImageExtentionValid(image))
            {
                return "";
            }
            var renamedFile = "";
            var extension = Path.GetExtension(image.FileName);
            string name;
            do
            {
                name = "file-" + Guid.NewGuid().ToString("N") + extension;
                renamedFile = _hostingEnvironmentRoot.ContentRootPath + "/Files/UploadImages/" + name;

            } while (File.Exists(renamedFile));
            try
            {
                using (var fileStream = new FileStream(renamedFile, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                    fileStream.Flush();
                }
            }
            catch (Exception)
            {
                return "";
            }
            return name;
        }

        public async Task<string> SaveImageAsync(IFormFile image, string path)
        {
            if (!IsImageSizeValid(image) || !IsImageExtentionValid(image))
            {
                return "";
            }
            var renamedFile = "";
            var extension = Path.GetExtension(image.FileName);
            string name;
            do
            {
                name = "file-" + Guid.NewGuid().ToString("N") + extension;
                renamedFile = _hostingEnvironmentRoot.ContentRootPath + path + name;

            } while (File.Exists(renamedFile));
            try
            {
                using (var fileStream = new FileStream(renamedFile, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                    fileStream.Flush();
                }
            }
            catch (Exception)
            {
                return "";
            }
            return name;
        }

        public async Task<string> SaveImageAsync(IFormFile image, string path, string name)
        {
            var renamedFile = _hostingEnvironmentRoot.ContentRootPath + path + name + ".jpg";
            try
            {
                using (var fileStream = new FileStream(renamedFile, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                    fileStream.Flush();
                }
                //image.Save(renamedFile + ".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            catch (Exception)
            {
                return "";
            }
            return name;
        }

        //public string SaveCompressImage(FormFile image, ImageComperssion ic)
        //{
        //    if (!IsImageSizeValid(image) || !IsImageExtentionValid(image))
        //    {
        //        return "";
        //    }
        //    var renamedFile = "";
        //    var extension = Path.GetExtension(image.FileName);
        //    string name;
        //    do
        //    {
        //        name = "img-" + Guid.NewGuid().ToString("N") + extension;
        //        renamedFile = _hostingEnvironmentRoot + "/Files/UploadImages/" + name;

        //    } while (File.Exists(renamedFile));
        //    try
        //    {
        //        System.Drawing.Image sourceimage = Image.FromStream(image.InputStream);
        //        CompressImage(sourceimage, renamedFile, ic);
        //    }
        //    catch (Exception)
        //    {
        //        return "";
        //    }
        //    return name;
        //}

        public string SaveCompressImage(IFormFile image, string path, ImageComperssion ic)
        {
            if (!IsImageSizeValid(image) || !IsImageExtentionValid(image))
            {
                return null;
            }
            var renamedFile = "";
            string name = string.Empty;
            var extension = Path.GetExtension(image.FileName);
            //string name = Path.GetFileNameWithoutExtension(image.FileName).Trim().Replace(" ", "-") + "-" + DateTime.UtcNow.Ticks.ToString();
            var directory = _hostingEnvironmentRoot.ContentRootPath + path;
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            do
            {
                name = "img-" + Guid.NewGuid().ToString("N") + extension;
                //name = name + extension;
                renamedFile = directory + name;

            } while (File.Exists(renamedFile));
            try
            {
                using (var sourceimage = Image.Load(image.OpenReadStream()))
                {
                    //sourceimage.Mutate(x => x
                    //    .Resize(sourceimage.Width / 2, sourceimage.Height / 2)
                    //    .Grayscale());
                    sourceimage.Save($"{renamedFile}");
                }

            }
            catch (Exception ex)
            {
                return null;
            }
            return name;
        }

        public string SaveCompressImage(IFormFile image, string path, string name, ImageComperssion ic)
        {
            if (!IsImageSizeValid(image) || !IsImageExtentionValid(image))
            {
                return null;
            }

            try
            {
                using (Image<Rgba32> sourceimage = Image.Load(image.OpenReadStream()))
                {
                    //sourceimage.Mutate(x => x
                    //    .Resize(sourceimage.Width / 2, sourceimage.Height / 2)
                    //    .Grayscale());
                    var directory = _hostingEnvironmentRoot.ContentRootPath + path;
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                    sourceimage.Save($"{_hostingEnvironmentRoot.ContentRootPath + path + name}");
                }
            }
            catch (Exception exception)
            {
                return null;
            }

            return name;
        }

        //private static void CompressImage(Image<Rgba32> img, string path, ImageComperssion ic)
        //{
        //    var qualityParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, Convert.ToInt32(ic));
        //    var format = img.RawFormat;
        //    var codec = ImageCodecInfo.GetImageDecoders().FirstOrDefault(c => c.FormatID == format.Guid);
        //    var mimeType = codec == null ? "image/jpeg" : codec.MimeType;
        //    var codecs = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders();
        //    var jpegCodec = codecs.FirstOrDefault(t => t.MimeType == mimeType);
        //    var encoderParams = new EncoderParameters(1) { Param = { [0] = qualityParam } };
        //    img.Save(path, jpegCodec, encoderParams);
        //}

        public async Task<string> SaveDocumentFile(IFormFile document, string path)
        {
            //if (!IsDocumentSizeValid(document) || !IsDocumentExtentionValid(document))
            //{
            //    return null;
            //}
            var renamedFile = "";
            var name = "";
            var extension = Path.GetExtension(document.FileName);
            //string name = Path.GetFileNameWithoutExtension(document.FileName) + "-" + DateTime.UtcNow.Ticks.ToString();
            var directory = _hostingEnvironmentRoot.ContentRootPath + path;
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            do
            {
                name = "doc-" + Guid.NewGuid().ToString("N") + extension;
                //name = name + extension;
                renamedFile = directory + name;

            } while (File.Exists(renamedFile));
            try
            {
                using (var stream = new FileStream(renamedFile, FileMode.Create))
                {
                    await document.CopyToAsync(stream);
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            return name;
        }
        public async Task<string> SaveDocumentAsync(IFormFile document, string path, string name)
        {
            try
            {
                var directory = _hostingEnvironmentRoot.ContentRootPath + path;
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                var renamedFile = directory + name;
                using (var fileStream = new FileStream(renamedFile, FileMode.Create))
                {
                    await document.CopyToAsync(fileStream);
                    fileStream.Flush();
                }
            }
            catch (Exception)
            {
                return "";
            }
            return name;
        }
        public async Task<bool> FileDriveSaveDocumentAsync(IFormFile document, string path, string name)
        {
            try
            {

       
                var directory = _hostingEnvironmentRoot.ContentRootPath + path;
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
               
                using (var fileStream = new FileStream(directory + name, FileMode.Create))
                {
                    await document.CopyToAsync(fileStream);
                    fileStream.Flush();
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        public async Task<bool> FileDriveSaveDocumentUploadFolderAsync(IFormFile document, string path, string name)
        {
            try
            {

                var fileParrent = document.Name.Substring(0, document.Name.LastIndexOf('/'));
               
                var directory = _hostingEnvironmentRoot.ContentRootPath + path + "/" + fileParrent + "/";
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using (var fileStream = new FileStream(directory + document.FileName, FileMode.Create))
                {
                    await document.CopyToAsync(fileStream);
                    fileStream.Flush();
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        public  bool FileDriveRemoveDocument( string path, string name)
        {
            try
            {
                var directory = _hostingEnvironmentRoot.ContentRootPath + path+name;
                if (File.Exists(directory))
                {
                    File.Delete(directory);
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        public async Task<string> SaveFileAsync(byte[] file, string path, string name)
        {
            var renamedFile = _hostingEnvironmentRoot.ContentRootPath + path + name;
            try
            {
                await Task.Run(() => File.WriteAllBytes(renamedFile, file));
            }
            catch (Exception)
            {
                return "";
            }
            return name;
        }

        public void DeleteFile(string path)
        {
            if (!File.Exists(path)) return;
            try
            {
                File.Delete(path);
            }
            catch (Exception ex)
            {
                // ignored
            }
        } 
        public void DeleteDirectory(string path)
        {
            if (!Directory.Exists(path)) return;
            try
            {
                Directory.Delete(path,true);
            }
            catch(Exception ex)
            {
                // ignored
            }
        }


        private static bool IsImageMimeTypeValid(IFormFile image)
        {
            var mimeType = image.ContentType.ToLower();
            return mimeType == "image/jpg" || mimeType == "image/jpeg" || mimeType == "image/pjpeg" || mimeType == "image/gif" || mimeType == "image/x-png" || mimeType == "image/png";
        }

        private static bool IsImageExtentionValid(IFormFile image)
        {
            var extention = Path.GetExtension(image.FileName)?.ToLower();
            return extention == ".jpg" || extention == ".png" || extention == ".gif" || extention == ".jpeg";
        }

        private static bool IsImageSizeValid(IFormFile image)
        {
            return image.Length / 1024 <= 10240;
        }

        private static bool IsFileExtentionValid(IFormFile file)
        {
            string[] validExt = { ".jpg", ".gif", ".png", ".rar", ".pdf", ".zip", ".mp4", ".flv", ".avi", ".wmv", ".mp3", ".wav", ".aac", ".3gp", ".xls", ".xlsx", ".doc", ".docx", ".ppt", ".pptx" };
            var extention = Path.GetExtension(file.FileName).ToLower();
            return Array.IndexOf(validExt, extention) >= 0;
        }

        private static bool IsVideoExtentionValid(IFormFile video)
        {
            string[] validExt = { ".mp4", ".flv", ".avi", ".wmv", ".3gp", ".mov" };
            var extention = Path.GetExtension(video.FileName)?.ToLower();
            return Array.IndexOf(validExt, extention) >= 0;
        }

        private static bool IsDocumentExtentionValid(IFormFile document)
        {
            var extention = Path.GetExtension(document.FileName)?.ToLower();
            return
                extention == ".msg" || extention == ".dwg" ||
                extention == ".jpeg" || extention == ".jpg" ||
                extention == ".png" || extention == ".rar" ||
                extention == ".zip" || extention == ".pdf" ||
                extention == ".txt" || extention == ".doc" ||
                extention == ".docx" || extention == ".pdf" ||
                extention == ".xls" || extention == ".xlsx" ||
                extention == ".xer" || extention == ".mpp";
        }

        private static bool IsVideoSizeValid(IFormFile video)
        {
            return video.Length / 1024 <= 1024000;
        }
        private static bool IsDocumentSizeValid(IFormFile file)
        {
            return file.Length / 1024 <= 819200;
        }
        public async Task<string> ConvertEntityToJsonFileAsync<TEntity>(TEntity entity, string name)
        {
            var data = await Task.Run(() => JsonConvert.SerializeObject(entity, Formatting.None));
            var path = _hostingEnvironmentRoot.ContentRootPath + name + ".json";
            System.IO.File.WriteAllBytes(path, Encoding.UTF8.GetBytes(data));
            return path;
        }

        public async Task<TEntity> ConvertJsonFileToEntityAsync<TEntity>(string name)
        {
            var path = _hostingEnvironmentRoot.ContentRootPath + TempPath + name + ".json";
            var json = File.ReadAllBytes(path);
            var items = await Task.Run(() => JsonConvert.DeserializeObject<TEntity>(Encoding.UTF8.GetString(json)));
            return items;
        }

    }
}
