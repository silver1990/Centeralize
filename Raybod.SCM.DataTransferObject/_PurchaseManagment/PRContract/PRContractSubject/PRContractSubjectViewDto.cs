using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.PRContract
{
    public class PRContractSubjectViewDto
    {
        public List<ListPRContractSubjectDto> PRContractSubjects { get; set; }

        public PRContractSubjectViewDto()
        {
            PRContractSubjects = new List<ListPRContractSubjectDto>();

        }
    }
}
