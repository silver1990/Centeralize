using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class RFPCommentInquery
    {
        public long Id { get; set; }

        public long RFPInqueryId { get; set; }

        public long RFPCommentId { get; set; }

        [ForeignKey(nameof(RFPInqueryId))]
        public RFPInquery RFPInquery { get; set; }

        [ForeignKey(nameof(RFPInqueryId))]
        public RFPComment RFPSupplierComment { get; set; }
    }
}
