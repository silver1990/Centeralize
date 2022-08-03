using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class MasterMR
    {
        [Key]
        public long Id { get; set; }

        [Column(TypeName = "varchar(60)")]
        public string ContractCode { get; set; }

        public int ProductId { get; set; }

        public int? BomProductId { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal GrossRequirement { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal RemainedGrossRequirement { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        [ForeignKey(nameof(ContractCode))]
        public virtual Contract Contract { get; set; }

        [ForeignKey(nameof(BomProductId))]
        public virtual Product BomProduct { get; set; }

        [ForeignKey(nameof(ProductId))]
        public virtual Product Product { get; set; }

        public virtual ICollection<MrpItem> MrpItems { get; set; }

    }
}
