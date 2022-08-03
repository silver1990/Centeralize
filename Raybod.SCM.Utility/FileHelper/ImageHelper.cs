using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Drawing;
using System.IO;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Raybod.SCM.Utility.EnumType;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Processing;

namespace Raybod.SCM.Utility.FileHelper
{
    public static class ImageHelper
    {
        public enum ImageWidth : int
        {
            QVcd = 320,
            Vcd = 360,
            Sd = 720,
            Hd = 1280,
            FullHD = 1920,
        }
        public enum ImageHeigth : int
        {
            QVcd = 200,
            Vcd = 240,
            Sd = 480,
            Hd = 720,
            FullHD = 1080,
        }

        //public void ResizeImage(Stream inputStream, int width, int height)
        //{
        //    Image<Rgba32> img = Image.Load(inputStream);
        //    MediaTypeNames.Image result = new Bitmap(width, height, PixelFormat.Format24bppRgb);
        //    using (var g = Graphics.FromImage(result))
        //    {
        //        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
        //        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        //        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
        //        g.DrawImage(img, 0, 0, width, height);
        //    }
        //    //  CompressImage(result, savePath, ic);
        //}

        //public void ResizeImage(MediaTypeNames.Image img, int width, int height)
        //{
        //    MediaTypeNames.Image result = new Bitmap(width, height, PixelFormat.Format24bppRgb);
        //    using (var g = Graphics.FromImage(result))
        //    {
        //        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
        //        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        //        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
        //        g.DrawImage(img, 0, 0, width, height);
        //    }
        //    //  CompressImage(result, savePath, ic);
        //} 

        public static bool IsImage(this IFormFile postedFile)
        {
            //-------------------------------------------
            //  Check the image mime types
            //-------------------------------------------
            if (!string.Equals(postedFile.ContentType, "image/jpg", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(postedFile.ContentType, "image/jpeg", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(postedFile.ContentType, "image/pjpeg", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(postedFile.ContentType, "image/gif", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(postedFile.ContentType, "image/x-png", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(postedFile.ContentType, "image/png", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            //-------------------------------------------
            //  Check the image extension
            //-------------------------------------------
            var postedFileExtension = Path.GetExtension(postedFile.FileName);
            if (!string.Equals(postedFileExtension, ".jpg", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(postedFileExtension, ".png", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(postedFileExtension, ".gif", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(postedFileExtension, ".jpeg", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }

        public static bool IsDocumentExtentionValid(this IFormFile document)
        {
            var extention = Path.GetExtension(document.FileName)?.ToLower();
            return extention == ".msg" || extention == ".dwg" || extention == ".jpeg" ||
                extention == ".jpg" || extention == ".png" || extention == ".rar" ||
                extention == ".zip" || extention == ".pdf" || extention == ".txt" ||
                extention == ".doc" || extention == ".docx" || extention == ".pdf" ||
                extention == ".xls" || extention == ".xlsx" || extention == ".xer" ||
                extention == ".pptx" || extention == ".pptm" || extention == ".ppt" ||
                extention == ".mpp"||extention==".sdb";
        }


        public static bool IsDocumentSizeValid(this IFormFile file)
        {
            return file.Length / 1024 <= 819200;
        }

        public static bool IsDocument(this IFormFile postedFile)
        {
            //-------------------------------------------
            //  Check the image mime types
            //-------------------------------------------
            //if (!string.Equals(postedFile.ContentType, "text/plain", StringComparison.OrdinalIgnoreCase) &&
            //    !string.Equals(postedFile.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase) &&
            //    !string.Equals(postedFile.ContentType, "application/vnd.ms-word", StringComparison.OrdinalIgnoreCase) &&
            //    !string.Equals(postedFile.ContentType, "application/vnd.ms-excel", StringComparison.OrdinalIgnoreCase) &&
            //    !string.Equals(postedFile.ContentType, "application/vnd.openxmlformats", StringComparison.OrdinalIgnoreCase) 
            //{
            //    return false;
            //}

            //-------------------------------------------
            //  Check the image extension
            //-------------------------------------------
            var postedFileExtension = Path.GetExtension(postedFile.FileName);
            if (!string.Equals(postedFileExtension, ".txt", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(postedFileExtension, ".png", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(postedFileExtension, ".jpeg", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(postedFileExtension, ".jpg", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(postedFileExtension, ".zip", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(postedFileExtension, ".pdf", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(postedFileExtension, ".doc", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(postedFileExtension, ".docx", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(postedFileExtension, ".xls", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(postedFileExtension, ".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }
        public static ImageFormat GetImageFormat(this IFormFile postedFile)
        {
            var postedFileExtension = Path.GetExtension(postedFile.FileName);
            if (string.Equals(postedFileExtension, ".jpg", StringComparison.OrdinalIgnoreCase)
                || string.Equals(postedFileExtension, ".jpeg", StringComparison.OrdinalIgnoreCase))
            {
                return ImageFormat.Jpeg;
            }
            return string.Equals(postedFileExtension, ".png", StringComparison.OrdinalIgnoreCase) ? ImageFormat.Png : ImageFormat.Jpeg;
        }

        public static MemoryStream ResizeImageByWidthAndSaveJpeg(Stream inputStream, int width)
        {
            using (Image<Rgba32> image = SixLabors.ImageSharp.Image.Load(inputStream))
            {
                var height = (image.Height * width) / image.Width;
                image.Mutate(x => x
                    .Resize(width, height));
                MemoryStream outputs = new MemoryStream();
                image.Save(outputs, ImageFormats.Jpeg);
                return outputs;
            }
        }

        //public void ResizeImageByWidth(MediaTypeNames.Image img, int width)
        //{
        //    var height = img.Height * width / img.Width;
        //    MediaTypeNames.Image result = new Bitmap(width, height, PixelFormat.Format24bppRgb);
        //    using (var g = Graphics.FromImage(result))
        //    {
        //        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
        //        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        //        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
        //        g.DrawImage(img, 0, 0, width, height);
        //    }
        //    //   CompressImage(result, savePath, ic);
        //}

        //public void ResizeImageByHeight(Stream inputStream, int height)
        //{
        //    MediaTypeNames.Image img = new Bitmap(inputStream);
        //    var width = img.Width * height / img.Height;
        //    MediaTypeNames.Image result = new Bitmap(width, height, PixelFormat.Format24bppRgb);
        //    using (var g = Graphics.FromImage(result))
        //    {
        //        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
        //        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        //        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
        //        g.DrawImage(img, 0, 0, width, height);
        //    }
        //    //   CompressImage(result, savePath, ic);
        //}

        //public void ResizeImageByHeight(MediaTypeNames.Image img, int height)
        //{
        //    var width = img.Width * height / img.Height;
        //    MediaTypeNames.Image result = new Bitmap(width, height, PixelFormat.Format24bppRgb);
        //    using (var g = Graphics.FromImage(result))
        //    {
        //        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
        //        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        //        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
        //        g.DrawImage(img, 0, 0, width, height);
        //    }
        //    //   CompressImage(result, savePath, ic);
        //}

        //public MediaTypeNames.Image ByteArrayToImage(byte[] arr, int width, int height)
        //{
        //    var output = new Bitmap(width, height);
        //    var rect = new Rectangle(0, 0, width, height);
        //    var bmpData = output.LockBits(rect, ImageLockMode.ReadWrite, output.PixelFormat);
        //    var ptr = bmpData.Scan0;
        //    Marshal.Copy(arr, 0, ptr, arr.Length);
        //    output.UnlockBits(bmpData);
        //    return output;
        //}
    }
}

