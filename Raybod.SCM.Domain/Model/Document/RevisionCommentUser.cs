using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class RevisionCommentUser
    {
        public long Id { get; set; }

        public int UserId { get; set; }

        public long RevisionCommentId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        [ForeignKey(nameof(RevisionCommentId))]
        public RevisionComment RevisionComment { get; set; }
    }
}
