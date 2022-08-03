using Raybod.SCM.DataTransferObject.User;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Customer
{
    public class ListNotOrgenizationUser: BaseUserDto
    {
        public int userId { get; set; }
        public MiniCompanyInfoDto Company { get; set; }
    }
    public class MiniCompanyInfoDto
    {
        public string CompanyName { get; set; }
        public int CompanyId { get; set; }
    }
}
