using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.DataTransferObject.PRContract
{
    public class AddPRContractPartListDto
    {
        public int ProductId { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Quantity { get; set; }

    }
}
