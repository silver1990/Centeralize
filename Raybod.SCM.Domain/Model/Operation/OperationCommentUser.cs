using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Raybod.SCM.Domain.Model
{
    public class OperationCommentUser
    {
        public long Id { get; set; }

        public int UserId { get; set; }

        public long OperationCommentId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        [ForeignKey(nameof(OperationCommentId))]
        public OperationComment OperationComment { get; set; }
    }
}
