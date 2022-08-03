using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.Domain.Model
{
    public class DocumentGroup : BaseEntity
    {
        [Key]
        public int DocumentGroupId { get; set; }

        [Required]
        [MaxLength(64)]
        public string DocumentGroupCode { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; }

        public virtual ICollection<Document> Documents { get; set; }
    }
}
