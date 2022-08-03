using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class CompanyUser : BaseEntity
    {
        [Key]
        public int CompanyUserId { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [StringLength(100)]
        public string LastName { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public int? CustomerId { get; set; }

        public int? SupplierId { get; set; }
        public int? ConsultantId { get; set; }

        [ForeignKey(nameof(CustomerId))]
        public virtual Customer Customer { get; set; }

        [ForeignKey(nameof(SupplierId))]
        public virtual Supplier Supplier { get; set; }

        [ForeignKey(nameof(ConsultantId))]
        public virtual Consultant Consultant { get; set; }
    }
}
