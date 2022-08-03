using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.FileDriveDirectory
{
    public class FileDriveDirectoryTrashContentDto
    {
        
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string CreateDate { get; set; }
            public string ModifiedDate { get; set; }
            public string Size { get; set; }
            public string Path { get; set; }
            public UserAuditLogDto UserAudit { get; set; }
    }
}
