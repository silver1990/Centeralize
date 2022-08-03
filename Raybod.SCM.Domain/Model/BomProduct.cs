using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class BomProduct : BaseEntity
    {

        public long Id { get; set; }

        public int ProductId { get; set; }

        public long? ParentBomId { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal CoefficientUse { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Remained { get; set; }

        public MaterialType MaterialType { get; set; }
        public bool IsRequiredMRP { get; set; }
        [ForeignKey("Area")]
        public int? AreaId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public virtual Product Product { get; set; }

        [ForeignKey(nameof(ParentBomId))]
        public virtual BomProduct ParentBom { get; set; }

        public virtual ICollection<BomProduct> ChildBom { get; set; }

        public virtual Area Area { get; set; }

        public virtual ICollection<MrpItem> MrpItems { get; set; }
    }
}
