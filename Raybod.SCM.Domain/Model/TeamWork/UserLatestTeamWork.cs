using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class UserLatestTeamWork
    {
        [Key]
        public long Id { get; set; }

        public int TeamWorkId { get; set; }

        public int UserId { get; set; }

        public DateTime? LastVisited { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }
        [ForeignKey(nameof(TeamWorkId))]
        public virtual TeamWork TeamWork { get; set; }
    }
}
