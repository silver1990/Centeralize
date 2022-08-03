using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Raybod.SCM.Domain.Model
{
    public class UserNotify
    {
        [Key]
        public long Id { get; set; }
        public NotifyManagementType NotifyType { get; set; }
        public int NotifyNumber { get; set; }
        public bool IsActive { get; set; }
        public bool IsOrganization { get; set; }
        public int TeamWorkId { get; set; }
        public string SubModuleName { get; set; }
        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        [ForeignKey(nameof(TeamWorkId))]
        public TeamWork TeamWork { get; set; }
    }
}
