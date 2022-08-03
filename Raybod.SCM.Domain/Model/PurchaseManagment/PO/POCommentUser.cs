using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class POCommentUser
    {
        public long Id { get; set; }

        public int UserId { get; set; }

        public long POCommentId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }

        [ForeignKey(nameof(POCommentId))]
        public virtual POComment POComment { get; set; }
    }
}
