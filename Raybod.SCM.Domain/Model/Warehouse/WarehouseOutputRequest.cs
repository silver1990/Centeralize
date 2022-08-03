using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Raybod.SCM.Domain.Model
{
    public class WarehouseOutputRequest:BaseEntity
    {
        [Key]
        public long RequestId { get; set; }
        [Required]
        public string RequestCode { get; set; }

        [Column(TypeName = "varchar(60)")]
        public string ContractCode { get; set; }
        public long? ReceiptId { get; set; }
        public string RecepitCode { get; set; }
        public WarehouseOutputStatus Status { get; set; }

        [ForeignKey(nameof(ContractCode))]
        public virtual Contract Contract { get; set; }

        [ForeignKey(nameof(ReceiptId))]
        public virtual Receipt Receipt { get; set; }
        public virtual ICollection<WarehouseOutputRequestSubject> Subjects { get; set; }
        public virtual ICollection<WarehouseOutputRequestWorkFlow> WarehouseOutputRequestWorkFlow { get; set; }


    }
}
