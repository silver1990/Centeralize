using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class Mrp : BaseEntity
    {
        [Key]
        public long Id { get; set; }

        [Column(TypeName = "varchar(60)")]
        public string ContractCode { get; set; }

        public int ProductGroupId { get; set; }

        [Required]
        //[Column(TypeName = "varchar(64)")]
        [MaxLength(64)]
        public string MrpNumber { get; set; }

        [Required]
        [MaxLength(300)]
        public string Description { get; set; }

        public MrpStatus MrpStatus { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        [ForeignKey(nameof(ContractCode))]
        public virtual Contract Contract { get; set; }

        [ForeignKey(nameof(ProductGroupId))]
        public virtual ProductGroup ProductGroup { get; set; }

        public virtual ICollection<MrpItem> MrpItems { get; set; }
    }
}
