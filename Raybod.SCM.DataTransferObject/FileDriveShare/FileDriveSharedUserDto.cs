using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.FileDriveShare
{
    public class FileDriveSharedUserDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string Image { get; set; }
        public Accessablity Accessablity { get; set; }
        
    }
}
