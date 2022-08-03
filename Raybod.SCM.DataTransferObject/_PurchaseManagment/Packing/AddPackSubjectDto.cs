using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.DataTransferObject.Packing
{
    public class AddPackSubjectDto
    {
        public int ProductId { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Quantity { get; set; }

    }
}
