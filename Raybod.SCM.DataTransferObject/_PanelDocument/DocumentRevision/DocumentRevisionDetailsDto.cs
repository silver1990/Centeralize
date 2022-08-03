using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Document
{
    public class DocumentRevisionDetailsDto : DocumentRevisionListDto
    {
        public List<BaseRevisionAvtivityDto> RevisionAvtivities { get; set; }
        public DocumentRevisionDetailsDto()
        {
            RevisionAvtivities = new List<BaseRevisionAvtivityDto>();
        }
        public string ClientDocNumber { get; set; }
    }
}
