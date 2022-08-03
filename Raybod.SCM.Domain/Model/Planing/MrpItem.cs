using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class MrpItem : BaseEntity
    {
        [Key]
        public long Id { get; set; }

        public int ProductId { get; set; }

        public long MrpId { get; set; }
        public long? BomProductId { get; set; }
        public long MasterMRId { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal GrossRequirement { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal NetRequirement { get; set; } // return GrossRequirement - StockWarehouse + ReservedStock;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal WarehouseStock { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal ReservedStock { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal SurplusQuantity { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal FinalRequirment { get; set; }   // return ((SafetyPercent / 100) + 1) * NetRequirement;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal RemainedStock { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal DoneStock { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal PR { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal PO { get; set; }

        [Required]
        public DateTime DateStart { get; set; }

        [Required]
        public DateTime DateEnd { get; set; }

        public MrpItemStatus MrpItemStatus { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        [ForeignKey(nameof(MrpId))]
        public Mrp Mrp { get; set; }

        [ForeignKey(nameof(MasterMRId))]
        public MasterMR MasterMR { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; }

        [ForeignKey(nameof(BomProductId))]
        public BomProduct BomProduct { get; set; }
        public virtual ICollection<POSubject> POSubjects { get; set; }
        public virtual ICollection<PurchaseRequestItem> PurchaseRequestItems { get; set; }

    }
}
