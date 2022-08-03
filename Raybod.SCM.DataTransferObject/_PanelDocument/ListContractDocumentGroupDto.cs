namespace Raybod.SCM.DataTransferObject.Document
{
    public class ListContractDocumentGroupDto : EditContractDocumentGroupDto
    {
        public string DocumentGroupTitle { get; set; }

        public string DocumentGroupCode { get; set; }

        public string ContractDescription { get; set; }

        public UserAuditLogDto UserAudit { get; set; }

        public ListContractDocumentGroupDto()
        {
            UserAudit = new UserAuditLogDto();
        }
    }
}
