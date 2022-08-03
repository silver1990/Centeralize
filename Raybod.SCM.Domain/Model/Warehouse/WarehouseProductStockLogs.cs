using Raybod.SCM.Domain.Enum;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class WarehouseProductStockLogs
    {
        [Key]
        public long Id { get; set; }

        public int ProductId { get; set; }
        public long? DespatchId { get; set; }

        public DateTime DateChange { get; set; }

        public long? ReceiptId { get; set; }

        public long? WarehouseTransferenceId { get; set; }
        
        public WarehouseStockChangeActionType WarehouseStockChangeActionType { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Input { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Output { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal RealStock { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; }
        
        [ForeignKey(nameof(ReceiptId))]
        public Receipt Receipt { get; set; }
        [ForeignKey(nameof(DespatchId))]
        public WarehouseDespatch WarehouseDespatch { get; set; }


        [ForeignKey(nameof(WarehouseTransferenceId))]
        public ReceiptReject WarehouseTransference { get; set; }
    }
}
