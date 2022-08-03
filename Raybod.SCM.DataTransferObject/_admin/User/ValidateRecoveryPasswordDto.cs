using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.User
{
    public class ValidateRecoveryPasswordDto
    {
        public string UserName { get; set; }
        public string Code { get; set; }
    }
}
