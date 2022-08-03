namespace Raybod.SCM.DataTransferObject.Document
{
    public class FinalRevisionAttachmentToZipDto
    {
        public string FileName { get; set; }

        public string FileSrc { get; set; }

        public string FilePath { get; set; }
        
    }

    public class FinalRevisionsAttachmentToZipDto
    {
        public string FileName { get; set; }

        public string FileSrc { get; set; }

        public string FilePath { get; set; }
        public string DocNumber { get; set; }

    }
}
