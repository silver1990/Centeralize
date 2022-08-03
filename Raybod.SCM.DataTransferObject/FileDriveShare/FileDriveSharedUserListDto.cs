using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.FileDriveShare
{
    public class FileDriveSharedUserListDto
    {
        public List<FileDriveSharedUserDto> Users { get; set; }
        public List<FileDriveSharedUserDto> Owners { get; set; }
        public FileDriveSharedUserListDto()
        {
            Users = new List<FileDriveSharedUserDto>();
            Owners = new List<FileDriveSharedUserDto>();
        }
    }
}
