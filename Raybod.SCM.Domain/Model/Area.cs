using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Raybod.SCM.Domain.Model
{
    public class Area:BaseEntity
    {
        [Key]
        public int AreaId { get; set; }
        [Required]
        [MaxLength(200)]
        public string AreaTitle { get; set; }
        [ForeignKey("Contract")]
        public string ContractCode { get; set; }

        public virtual Contract Contract { get; set; }
        public virtual ICollection<Document> Documents { get; set; }
        public virtual ICollection<BomProduct> BomProducts { get; set; }
    }
}
