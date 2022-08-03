using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class SupplierProductGroup
    {

        public int Id { get; set; }

        public int SupplierId { get; set; }

        public int ProductGroupId { get; set; }

        [ForeignKey(nameof(ProductGroupId))]
        public virtual ProductGroup ProductGroup { get; set; }

        [ForeignKey(nameof(SupplierId))]
        public virtual Supplier Supplier { get; set; }
    }
}
