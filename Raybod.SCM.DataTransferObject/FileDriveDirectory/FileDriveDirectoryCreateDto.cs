using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Raybod.SCM.DataTransferObject.FileDriveDirectory
{
    public class FileDriveDirectoryCreateDto
    {
        public Guid? DirectoryId { get; set; }
        [Required]
        [MaxLength(60)]
        public string Name { get; set; }
    }
}
