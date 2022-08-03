using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Raybod.SCM.Domain.Model
{
    public class PoSupplierDocument:BaseEntity
    {
        [Key]
        public long POSupplierDocumentId { get; set; }
        [MaxLength(2048)]
        public string DocumentTitle { get; set; }

        public string DocumentCode { get; set; }
        public long POId { get; set; }
        public int ProductId { get; set; }

        [ForeignKey(nameof(POId))]
        public virtual PO PO { get; set; }

        [ForeignKey(nameof(ProductId))]
        public virtual Product Product { get; set; }
        public virtual ICollection<PAttachment> Attachments { get; set; }
    }
}
