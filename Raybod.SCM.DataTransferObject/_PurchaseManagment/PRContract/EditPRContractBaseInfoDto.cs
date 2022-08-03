using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Raybod.SCM.DataTransferObject.PRContract
{
    public class EditPRContractBaseInfoDto
    {
        [Required(ErrorMessage = "الزامی می باشد")]
        public long DateIssued { get; set; }

        /// <summary>
        /// تاریخ شروع قرارداد
        /// </summary>
        [Required]
        public int ContractDuration { get; set; }

        [Required(ErrorMessage = "الزامی می باشد")]
        public long DateEnd { get; set; }

    }
}
