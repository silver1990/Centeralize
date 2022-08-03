using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.DataTransferObject.Contract
{
    public class ContractFormConfigDto
    {
        public long ContractFormConfigId { get; set; }

        public string ContractCode { get; set; }

        public FormName FormName { get; set; }

        public CodingType CodingType { get; set; }

        public string FixedPart { get; set; }

        public int LengthOfSequence { get; set; }

        public bool IsEditable { get; set; } = false;
    }
}
