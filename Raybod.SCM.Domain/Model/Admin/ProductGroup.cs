using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.Domain.Model
{
    public class ProductGroup : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(60)]
        public string ProductGroupCode { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        public virtual ICollection<SupplierProductGroup> SupplierProductGroups { get; set; }

        public virtual ICollection<TeamWorkUserProductGroup> TeamWorkUserProductGroups { get; set; }

        public virtual ICollection<Product> Products { get; set; }

    }
}
