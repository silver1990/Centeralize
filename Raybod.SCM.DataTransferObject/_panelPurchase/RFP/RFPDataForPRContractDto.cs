namespace Raybod.SCM.DataTransferObject.RFP
{
    public class RFPDataForPRContractDto : ListRFPDto
    {
        public long? DateSelectWiiner { get; set; }
        public UserAuditLogDto UserAudit { get; set; }
        public RFPDataForPRContractDto()
        {
            UserAudit = new UserAuditLogDto();
        }
    }
}
