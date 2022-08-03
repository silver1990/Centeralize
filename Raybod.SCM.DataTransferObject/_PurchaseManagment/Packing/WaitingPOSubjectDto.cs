using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.DataTransferObject.Packing
{
    public class WaitingPOSubjectDto
    {
        public int ProductId { get; set; }

        [Display(Name = "کد محصول")]
        public string ProductCode { get; set; }

        [Display(Name = "نام محصول")]
        public string ProductName { get; set; }

        public string TechnicalNumber { get; set; }
        
        public string ProductUnit { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Quantity { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal RemainedQuantity { get; set; }

        
    }
}
