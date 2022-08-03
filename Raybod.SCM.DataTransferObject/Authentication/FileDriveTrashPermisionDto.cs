using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Authentication
{
    public class FileDriveTrashPermisionDto
    {
        public bool HasPublicPermission { get; set; }

        public bool HasPrivatePermission { get; set; }
        public bool NoPermission { get; set; }

    }
}
