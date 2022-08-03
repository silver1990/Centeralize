namespace Raybod.SCM.DataTransferObject.Document
{
    public class ListDocumentListDto : BaseDocumentListDto
    {
        public long Id { get; set; }

        public string ProductCode { get; set; }

        public string ProductDescription { get; set; }

        public long? LastUpdateDate { get; set; }

        public int DocumentCount { get; set; }
    }
}
