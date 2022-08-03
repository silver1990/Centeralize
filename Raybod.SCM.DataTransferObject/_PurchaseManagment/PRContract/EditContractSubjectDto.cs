using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.PRContract
{
    public class EditContractSubjectDto
    {
        public List<EditPRContractSubjectDto> ContractSubjects { get; set; }

        public List<EditPrContractServiceDto> ContractService { get; set; }

    }
}
