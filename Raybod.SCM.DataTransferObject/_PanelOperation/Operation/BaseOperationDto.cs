using Raybod.SCM.DataTransferObject._PanelDocument.Area;
using Raybod.SCM.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Operation
{
    public class BaseOperationDto
    {
        /// <summary>
        /// کدفعالیت
        /// </summary>
        [Required]
        [MaxLength(64)]
        public string OperationCode { get; set; }

       
        [Required]
        [MaxLength(250)]
        public string OperationDescription { get; set; }

        

        public AreaReadDTO Area { get; set; }
    }
}
