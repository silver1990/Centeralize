using System.IO;

namespace Raybod.SCM.DataTransferObject
{
    public class DownloadFileDto
    {
        public MemoryStream Stream { get; set; }

        public string ContentType { get; set; }

        public string FileName { get; set; }
        public byte[] ArchiveFile { get; set; } = null;
    }
}
