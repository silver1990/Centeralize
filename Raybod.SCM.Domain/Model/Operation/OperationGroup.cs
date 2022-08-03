using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Raybod.SCM.Domain.Model
{
    public class OperationGroup:BaseEntity
    {
        [Key]
        public int OperationGroupId { get; set; }

        [Required]
        [MaxLength(64)]
        public string OperationGroupCode { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; }

        public virtual ICollection<Operation> Operations { get; set; }
    }
}
