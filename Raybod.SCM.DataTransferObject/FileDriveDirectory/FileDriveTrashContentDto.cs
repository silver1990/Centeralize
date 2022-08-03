using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.FileDriveDirectory
{
    public class FileDriveTrashContentDto
    {

        public List<FileDriveDirectoryTrashContentDto> Directories { get; set; }
        public List<FileDriveFileTrashContentDto> Files { get; set; }

        public FileDriveTrashContentDto()
        {
            Directories = new List<FileDriveDirectoryTrashContentDto>();
            Files = new List<FileDriveFileTrashContentDto>();
        }

    }
}
