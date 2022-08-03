using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.FileDriveDirectory
{
    public class AdvanceSearchDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool IsFile { get; set; }
    }
}
