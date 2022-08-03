using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.DataTransferObject.Contract
{
    public class AddContractFormConfigDto
    {
        public FormName FormName { get; set; }

        public CodingType CodingType { get; set; }

        public string FixedPart { get; set; }

        public int LengthOfSequence { get; set; }
    }
}
