namespace Raybod.SCM.DataTransferObject.ContractAttachment
{
    public class BaseContractAttachmentDto : AddAttachmentDto
    {
        public int Id { get; set; }

        public string ContractCode { get; set; }

        public string FileType { get; set; }

        public long FileSize { get; set; }
        
    }
}
