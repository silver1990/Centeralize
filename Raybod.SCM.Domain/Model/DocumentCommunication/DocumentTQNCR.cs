using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class DocumentTQNCR : BaseAuditEntity
    {
        [Key]
        public long DocumentTQNCRId { get; set; }

        public long DocumentRevisionId { get; set; }

        [MaxLength(64)]
        [Required]
        public string CommunicationCode { get; set; }

        [MaxLength(250)]
        public string Subject { get; set; }

        public CommunicationType CommunicationType { get; set; }

        public DocumentCommunicationStatus CommunicationStatus { get; set; }

        public NCRReason NCRReason { get; set; }

        public CompanyIssue CompanyIssue { get; set; }

        public int? CustomerId { get; set; }

        public int? SupplierId { get; set; }

        public int? ConsultantId { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        [ForeignKey(nameof(DocumentRevisionId))]
        public DocumentRevision DocumentRevision { get; set; }

        [ForeignKey(nameof(SupplierId))]
        public Supplier Supplier { get; set; }

        [ForeignKey(nameof(CustomerId))]
        public Customer Customer { get; set; }

        [ForeignKey(nameof(ConsultantId))]
        public Consultant Consultant { get; set; }

        public virtual ICollection<CommunicationQuestion> CommunicationQuestions { get; set; }

        public virtual ICollection<CommunicationAttachment> Attachments { get; set; }

        public virtual ICollection<CommunicationTeamComment> Comments { get; set; }
    }
}
