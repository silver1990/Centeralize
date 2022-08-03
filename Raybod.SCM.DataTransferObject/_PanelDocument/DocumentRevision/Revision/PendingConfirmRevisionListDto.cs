namespace Raybod.SCM.DataTransferObject.Document
{
    public class PendingConfirmRevisionListDto : DocumentRevisionListDto
    {
        public UserAuditLogDto BallInCourtUser { get; set; }

        public PendingConfirmRevisionListDto()
        {
            BallInCourtUser = new UserAuditLogDto();
        }
        public string ClientDocNumber { get; set; }
    }
}
