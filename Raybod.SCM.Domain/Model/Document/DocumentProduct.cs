using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class DocumentProduct
    {
        public long Id { get; set; }

        public int ProductId { get; set; }

        public long DocumentId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public virtual Product Product { get; set; }

        [ForeignKey(nameof(DocumentId))]
        public virtual Document Document { get; set; }

    }
}
