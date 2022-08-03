using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.Domain.Model
{
    public class Supplier : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(60)]
        public string SupplierCode { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(20)]
        public string TellPhone { get; set; }

        [MaxLength(20)]
        public string Fax { get; set; }

        [MaxLength(300)]
        public string Address { get; set; }

        [MaxLength(10)]
        public string PostalCode { get; set; }

        [MaxLength(20)]
        public string EconomicCode { get; set; }

        [MaxLength(20)]
        public string NationalId { get; set; }

        [MaxLength(300)]
        public string Website { get; set; }

        [MaxLength(300)]
        public string Email { get; set; }

        [MaxLength(300)]
        public string Logo { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        public virtual ICollection<SupplierProductGroup> SupplierProductGroups { get; set; }

        public virtual ICollection<CompanyUser> SupplierUsers { get; set; }

        public virtual ICollection<RFPSupplier> RFPSuppliers { get; set; }

        public virtual ICollection<FinancialAccount> FinancialAccounts { get; set; }

        public virtual ICollection<Invoice> Invoices { get; set; }
    }
}
