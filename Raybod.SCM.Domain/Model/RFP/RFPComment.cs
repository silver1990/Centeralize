using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class RFPComment : BaseEntity
    {
        [Key]
        public long Id { get; set; }

        public long RFPSupplierId { get; set; }

        public RFPInqueryType RFPInqueryType { get; set; }

        public string Message { get; set; }

        public long? ParentCommentId { get; set; }

        [ForeignKey(nameof(ParentCommentId))]
        public RFPComment ParentComment { get; set; }

        [ForeignKey(nameof(RFPSupplierId))]
        public RFPSupplier RFPSupplier { get; set; }

        public ICollection<RFPComment> ReplayComments { get; set; }

        public ICollection<RFPCommentUser> RFPCommentUsers { get; set; }

        public ICollection<RFPCommentInquery> RFPCommentInqueries { get; set; }
        public ICollection<RFPAttachment> Attachments { get; set; }
    }
}
