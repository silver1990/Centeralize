using Raybod.SCM.Domain.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class PDFTemplate
    {
        [Key]
        public int Id { get; set; }

        public string ContractCode { get; set; }

        public PDFTemplateType PDFTemplateType { get; set; }

        [MaxLength]
        public string Section1 { get; set; }

        [MaxLength]
        public string Section2 { get; set; }

        [MaxLength]
        public string Section3 { get; set; }

        [MaxLength]
        public string Section4 { get; set; }

        [MaxLength]
        public string Section5 { get; set; }

        [ForeignKey(nameof(ContractCode))]
        public Contract Contract { get; set; }
    }
}
