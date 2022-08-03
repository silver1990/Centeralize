using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.FileDriveDirectory
{
    public class FileDriveFileAndDirectoryListDto
    {
        public List<FileDriveBreadcrumbDto> Breadcrumbs { get; set; }
        public List<FileDriverDirectoryListDto> Directories { get; set; }
        public List<FileDriveFilesListDto> Files { get; set; }
       
        public FileDriveFileAndDirectoryListDto()
        {
            Directories = new List<FileDriverDirectoryListDto>();
            Breadcrumbs = new List<FileDriveBreadcrumbDto>();
            Files = new List<FileDriveFilesListDto>();
        }
    }
}
