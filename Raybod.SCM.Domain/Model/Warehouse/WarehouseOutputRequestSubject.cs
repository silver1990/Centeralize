using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Raybod.SCM.Domain.Model
{
    public class WarehouseOutputRequestSubject:BaseEntity
    {
        [Key]
        public long RequestSubjectId { get; set; }

        public int ProductId { get; set; }

        public long RequestId { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Quantity { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Delivery { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; }

        [ForeignKey(nameof(RequestId))]
        public WarehouseOutputRequest WarehouseOutputRequest { get; set; }
    }
}
