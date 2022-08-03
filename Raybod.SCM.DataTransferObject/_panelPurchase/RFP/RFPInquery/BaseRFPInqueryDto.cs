using Raybod.SCM.Domain.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.DataTransferObject.RFP
{
    public class BaseRFPInqueryDto
    {
  

        /// <summary>
        /// شرح استعلام
        /// </summary>
        [MaxLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// وزن
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Weight { get; set; }
    }
}
