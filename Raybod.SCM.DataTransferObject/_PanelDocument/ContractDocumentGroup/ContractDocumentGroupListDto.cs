using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Document
{
    public class ContractDocumentGroupListDto
    {
        public string ContractCode { get; set; }

        public string ContractDescription { get; set; }

        public List<DocumentGroupDto> DocumentGroups { get; set; }

        public ContractDocumentGroupListDto()
        {
            DocumentGroups = new List<DocumentGroupDto>();
        }
    }
}
