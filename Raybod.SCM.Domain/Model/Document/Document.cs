using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class Document : BaseEntity
    {
        [Key]
        public long DocumentId { get; set; }

        [Column(TypeName = "varchar(60)")]
        public string ContractCode { get; set; }

        public int DocumentGroupId { get; set; }


        public bool IsActive { get; set; }

        [Required]
        [MaxLength(64)]
        public string DocNumber { get; set; }

        [MaxLength(100)]
        public string ClientDocNumber { get; set; }

        [Required]
        [MaxLength(250)]
        public string DocTitle { get; set; }

        [MaxLength(800)]
        public string DocRemark { get; set; }

        [Required]
        public bool IsRequiredTransmittal { get; set; }
        public DocumentClass DocClass { get; set; }

        public CommunicationCommentStatus CommunicationCommentStatus { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
        [ForeignKey("Area")]
        public int? AreaId { get; set; }

        [ForeignKey(nameof(ContractCode))]
        public virtual Contract Contract { get; set; }

        [ForeignKey(nameof(DocumentGroupId))]
        public virtual DocumentGroup DocumentGroup { get; set; }



        public virtual ICollection<DocumentProduct> DocumentProducts { get; set; }

        public virtual ICollection<DocumentRevision> DocumentRevisions { get; set; }
        public virtual Area Area { get; set; }

    }
}
