using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Raybod.SCM.Domain.Model
{ 
    public class RFPProForma:BaseEntity
    {
        [Key]
        public long Id { get; set; }

        public long RFPId { get; set; }
        public long RFPSupplierId { get; set; }


        /// <summary>
        /// شرح استعلام
        /// </summary>
        [MaxLength(500)]
        public string Duration { get; set; }

        [MaxLength(100)]
        public string Price { get; set; }

        public virtual ICollection<RFPAttachment> RFPProFormaAttachments { get; set; }

        [ForeignKey(nameof(RFPId))]
        public RFP RFP { get; set; }
        [ForeignKey(nameof(RFPSupplierId))]
        public RFPSupplier RFPSupplier { get; set; }
    }
}
