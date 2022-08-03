using Raybod.SCM.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.RFP
{
    public class BaseRFPDto
    {
        /// <summary>
        /// نوع RFP
        /// </summary>
        public RFPType RFPType { get; set; }

        /// <summary>
        /// تاریخ سررسید
        /// </summary>
        [Required(ErrorMessage = "تاریخ سررسید را وارد کنید")]
        public long DateDue { get; set; }

        /// <summary>
        /// شرح
        /// </summary>
        [MaxLength(800)]
        public string Note { get; set; }

    }
}
