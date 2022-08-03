using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Document
{
    public class DocumentLogDto
    {
        public UserAuditLogDto UserAudit { get; set; }

        public NotifEvent NotifEvent { get; set; }
        public long CreateDate { get; set; }

        public DocumentLogDto()
        {
            UserAudit = new UserAuditLogDto();
        }
    }
}
