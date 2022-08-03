namespace Raybod.SCM.DataTransferObject.Document
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
