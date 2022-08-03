using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class CommunicationTeamCommentUser
    {
        public long Id { get; set; }

        public int UserId { get; set; }

        public long CommunicationTeamCommentId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }

        [ForeignKey(nameof(CommunicationTeamCommentId))]
        public virtual CommunicationTeamComment CommunicationTeamComment { get; set; }
    }
}
