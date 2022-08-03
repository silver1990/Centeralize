using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Raybod.SCM.DataTransferObject.FileDriveDirectory
{
    public class FileDriveFileRenameDto
    {
        [Required]
        [MaxLength(250)]
        public string Name { get; set; }
    }
}
