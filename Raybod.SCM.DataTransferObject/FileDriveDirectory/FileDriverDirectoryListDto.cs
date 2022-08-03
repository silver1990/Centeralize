using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.FileDriveDirectory
{
    public class FileDriverDirectoryListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string CreateDate{ get; set; }
        public string ModifiedDate{ get; set; }
        public string Size{ get; set; }
        public UserAuditLogDto UserAudit { get; set; }
    }
}
