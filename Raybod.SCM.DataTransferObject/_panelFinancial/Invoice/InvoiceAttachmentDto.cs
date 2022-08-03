namespace Raybod.SCM.DataTransferObject.Invoice
{
   public class InvoiceAttachmentDto
    {
        public long AttachmentId { get; set; }
        
        public long InvoiceId { get; set; }

        public string FileName { get; set; }

        public string FileSrc { get; set; }

        public string FileType { get; set; }

        public long FileSize { get; set; }
    }
}
