using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class Product : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public int ProductGroupId { get; set; }

        [Required]
        [MaxLength(60)]
        public string ProductCode { get; set; }

        [MaxLength(50)]
        public string TechnicalNumber { get; set; }

        [MaxLength(400)]
        [Required]
        public string Description { get; set; }

        [MaxLength(300)]
        public string Image { get; set; }

        [Required]
        [MaxLength(100)]
        public string Unit { get; set; }
        [Column(TypeName = "varchar(60)")]
        public string ContractCode { get; set; }
        //public ProductType ProductType { get; set; }



        [Timestamp]
        public byte[] RowVersion { get; set; }

        [ForeignKey(nameof(ProductGroupId))]
        public virtual ProductGroup ProductGroup { get; set; }
        [ForeignKey(nameof(ContractCode))]
        public virtual Contract Contract { get; set; }

        public virtual ICollection<BomProduct> BomProducts { get; set; }

        public virtual ICollection<DocumentProduct> DocumentProducts { get; set; }

        public virtual ICollection<WarehouseProduct> WarehouseProducts { get; set; }

        public virtual ICollection<PRContractSubject> PRContractSubjects { get; set; }

        public virtual ICollection<POSubject> POSubjects { get; set; }

        public virtual ICollection<PurchaseRequestItem> PurchaseRequestItems { get; set; }

        public virtual ICollection<MasterMR> MasterMRs { get; set; }

        public virtual ICollection<MasterMR> MasterBomProducts { get; set; }

      

    }
}
