using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class PurchaseRequest : BaseEntity
    {
        [Key]
        public long Id { get; set; }

        [Column(TypeName = "varchar(60)")]
        public string ContractCode { get; set; }

        //[Column(TypeName = "varchar(64)")]
        [MaxLength(64)]
        [Required]
        public string PRCode { get; set; }

        public int ProductGroupId { get; set; }

        public long? MrpId { get; set; }

        public TypeOfInquiry TypeOfInquiry { get; set; }

        [MaxLength(int.MaxValue)]
        public string Note { get; set; }

        [MaxLength(int.MaxValue)]
        public string ConfirmNote { get; set; }

        public PRStatus PRStatus { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        [ForeignKey(nameof(MrpId))]
        public virtual Mrp Mrp { get; set; }

        [ForeignKey(nameof(ContractCode))]
        public virtual Contract Contract { get; set; }

        [ForeignKey(nameof(ProductGroupId))]
        public virtual ProductGroup ProductGroup { get; set; }


        public virtual ICollection<PurchaseRequestItem> PurchaseRequestItems { get; set; }
        public virtual ICollection<PAttachment> PRAttachments { get; set; }
        public virtual ICollection<PurchaseConfirmationWorkFlow> PurchaseConfirmationWorkFlows { get; set; }
    }
}
