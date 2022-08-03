using Raybod.SCM.Domain.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class ContractFormConfig
    {
        [Key]
        public long ContractFormConfigId { get; set; }

        [Column(TypeName = "varchar(60)")]
        public string ContractCode { get; set; }

        public FormName FormName { get; set; }

        public CodingType CodingType { get; set; }

        public string FixedPart { get; set; }

        public int LengthOfSequence { get; set; }

        [ForeignKey(nameof(ContractCode))]
        public virtual Contract Contract { get; set; }
    }
}
