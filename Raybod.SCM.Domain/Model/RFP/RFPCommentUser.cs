using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class RFPCommentUser
    {
        public long Id { get; set; }

        public int UserId { get; set; }

        public long RFPCommentId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        [ForeignKey(nameof(RFPCommentId))]
        public RFPComment RFPComment { get; set; }
    }
}
