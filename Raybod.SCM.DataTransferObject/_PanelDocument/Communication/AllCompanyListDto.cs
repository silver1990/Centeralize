using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Document.Communication
{
    public class AllCompanyListDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public CompanyIssue Type { get; set; }
    }
}
