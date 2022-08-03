using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.Domain.Model
{
    public class ProductUnit
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string Unit { get; set; }

        public bool IsDeleted { get; set; }
    }
}
