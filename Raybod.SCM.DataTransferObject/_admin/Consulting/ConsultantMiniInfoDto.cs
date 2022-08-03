using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Consultant
{
    public class ConsultantMiniInfoDto
    {
        public int Id { get; set; }

        [Display(Name = "کد مشاور")]
        public string ConsultantCode { get; set; }

        [Display(Name = "نام شرکت")]
        public string Name { get; set; }

        public string Logo { get; set; }

    }
}
