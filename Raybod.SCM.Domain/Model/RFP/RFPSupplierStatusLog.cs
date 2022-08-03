using Raybod.SCM.Domain.Enum;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class RFPSupplierStatusLog : BaseAuditEntity
    {
        /// <summary>
        /// شناسه
        /// </summary>
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// وضعیت درخواست
        /// </summary>
        public RFPStatus Status { get; set; }

        /// <summary>
        /// تاریخ انجام
        /// </summary>

        [Required]
        public DateTime DateIssued { get; set; }

        public long RFPSupplierId { get; set; }

        //[ForeignKey(nameof(RFPSupplierId))]
        //public RFPSupplier RFPSupplier { get; set; }

    }
}
