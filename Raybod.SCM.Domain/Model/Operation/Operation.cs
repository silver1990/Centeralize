using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Raybod.SCM.Domain.Model
{
    public class Operation : BaseEntity
    {
        [Key]
        public Guid OperationId { get; set; }

        [Column(TypeName = "varchar(60)")]
        public string ContractCode { get; set; }

        public int OperationGroupId { get; set; }

        [Required]
        [MaxLength(250)]
        public string OperationDescription { get; set; }

        public bool IsActive { get; set; }

        public DateTime? DueDate { get; set; }

        [Required]
        [MaxLength(64)]
        public string OperationCode { get; set; }


        [ForeignKey("Area")]
        public int? AreaId { get; set; }

        [ForeignKey(nameof(ContractCode))]
        public virtual Contract Contract { get; set; }

        [ForeignKey(nameof(OperationGroupId))]
        public virtual OperationGroup OperationGroup { get; set; }
        public virtual Area Area { get; set; }
        public virtual ICollection<StatusOperation> OperationStatuses { get; set; }
        public virtual ICollection<OperationProgress> OperationProgresses { get; set; }
        public virtual ICollection<OperationAttachment> Attachments { get; set; }
        public virtual ICollection<OperationActivity> OperationActivities { get; set; }
    }
}
