using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.Domain.Model
{
    public class Warehouse : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(250)]
        public string Name { get; set; }

        [MaxLength(60)]
        public string WarehouseCode { get; set; }

        [MaxLength(300)]
        public string Address { get; set; }

        [MaxLength(20)]
        public string Phone { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

    }
}
