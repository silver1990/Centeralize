using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.FileDriveShare
{
    public class FileDriveShareUserAccessablityDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Image { get; set; }
        public Access Access { get; set; }
    }
    public class Access
    {
        public Accessablity Value { get; set; }
        public string Label { get; set; }
    }

    public class FileDriveShareOwnerUserAccessablityDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Image { get; set; }
        public Accessablity Accessablity { get; set; }
    }
}
