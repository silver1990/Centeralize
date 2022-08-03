using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Customer
{
    public class CustomerMiniInfoDto
    {
        public int Id { get; set; }

        [Display(Name = "کد مشتری")]
        public string CustomerCode { get; set; }

        [Display(Name = "نام شرکت")]
        public string Name { get; set; }

        public string Logo { get; set; }
    }
    public class CustomerMiniInfoForCommentDto
    {
        public int Id { get; set; }
        public CompanyIssue CompanyIssue { get; set; }

        [Display(Name = "کد مشتری")]
        public string CustomerCode { get; set; }

        [Display(Name = "نام شرکت")]
        public string Name { get; set; }

        public string Logo { get; set; }
    }
}
