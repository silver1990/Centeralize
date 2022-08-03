using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class PackSubject
    {
        [Key]
        public long PackSubjectId { get; set; }

        public long? PackId { get; set; }

        public long? ParentSubjectId { get; set; }

        public int ProductId { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Quantity { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal ShortageQuantity { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        [ForeignKey(nameof(PackId))]
        public virtual Pack Pack { get; set; }

        [ForeignKey(nameof(ProductId))]
        public virtual Product Product { get; set; }

        [ForeignKey(nameof(ParentSubjectId))]
        public virtual PackSubject ParentSubject { get; set; }

    }
}
