namespace Raybod.SCM.DataTransferObject.PO.POActivity
{
    public class ListActivityTimeSheetDto : BaseActivityTimeSheetDto
    {
        public UserAuditLogDto UserAudit { get; set; }

        public ListActivityTimeSheetDto()
        {
            UserAudit = new UserAuditLogDto();
        }
    }
}
