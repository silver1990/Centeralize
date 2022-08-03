using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.Domain.Model
{
    public class Customer : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [Required]
        [MaxLength(64)]
        public string CustomerCode { get; set; }

        //[Required]
        [MaxLength(300)]
        public string Address { get; set; }

        [MaxLength(100)]
        public string TellPhone { get; set; }

        [MaxLength(20)]
        public string Fax { get; set; }

        [MaxLength(12)]
        public string PostalCode { get; set; }

        [MaxLength(300)]
        public string Website { get; set; }

        [MaxLength(300)]
        public string Email { get; set; }

        [MaxLength(300)]
        public string Logo { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        public virtual ICollection<CompanyUser> CustomerUsers { get; set; }

        public virtual ICollection<Contract> CustomerContracts { get; set; }

    }
}
