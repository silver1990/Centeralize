namespace Raybod.SCM.DataTransferObject.Document
{
    public class DocumentMiniOnfoDto
    {
        public long DocumentId { get; set; }

        public string DocNumber { get; set; }

        public string DocTitle { get; set; }

        public LastRevisionDto LastRevision { get; set; }

        public DocumentMiniOnfoDto()
        {
            LastRevision = new LastRevisionDto();
        }
    }
}
