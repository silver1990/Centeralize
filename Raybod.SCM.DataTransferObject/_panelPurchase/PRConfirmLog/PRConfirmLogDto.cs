namespace Raybod.SCM.DataTransferObject.PurchaseRequest
{
    public class PRConfirmLogDto
    {
        public string ConfirmNote { get; set; }

        public bool IsConfirm { get; set; }

        public UserAuditLogDto UserAudit { get; set; }
    }
}
