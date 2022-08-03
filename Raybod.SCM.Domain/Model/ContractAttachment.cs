using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class ContractAttachment : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(300)]
        public string FileName { get; set; }
        /// <summary>
        ///  نام فایل
        /// </summary>
        [MaxLength(250)]
        public string FileSrc { get; set; }

        public string FileType { get; set; }

        public long FileSize { get; set; }

        [Column(TypeName = "varchar(60)")]
        public string ContractCode { get; set; }

        [ForeignKey(nameof(ContractCode))]
        public Contract Contract { get; set; }
    }
}
