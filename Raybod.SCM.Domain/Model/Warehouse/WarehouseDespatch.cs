using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Raybod.SCM.Domain.Model
{
    public class WarehouseDespatch:BaseEntity
    {
        [Key]
        public long DespatchId  { get; set; }

        public long RequestId { get; set; }
        public long? InvoiceId { get; set; }
        public string DespatchCode { get; set; }
        public WarehouseDespatchStatus Status { get; set; }

        [Column(TypeName = "varchar(60)")]
        public string ContractCode { get; set; }

        [ForeignKey(nameof(ContractCode))]
        public virtual Contract Contract { get; set; }

        [ForeignKey(nameof(RequestId))]
        public WarehouseOutputRequest WarehouseOutputRequest { get; set; }

        [ForeignKey(nameof(InvoiceId))]
        public Invoice Invoice { get; set; }

        public virtual ICollection<WarehouseProductStockLogs> WarehouseProductStockLogs { get; set; }
    }
}
