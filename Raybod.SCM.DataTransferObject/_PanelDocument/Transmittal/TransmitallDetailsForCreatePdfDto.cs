using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Model;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Transmittal
{
    public class TransmitallDetailsForCreatePdfDto
    {
        public string CompanyName { get; set; }

        public string CompanyLogo { get; set; }

        public POI POI { get; set; }

        public int CustomerId { get; set; }

        public string CustomerName { get; set; }

        public string CustomerEmail { get; set; }

        public string CustomerLogo { get; set; }

        public string TransmittalNumber { get; set; }

        public string ContractDescription { get; set; }

        public int DocumentGroupId { get; set; }

        public string DocumentGroupTitle { get; set; }

        public string DocumentGroupCode { get; set; }
        
        public List<DocumentRevision> Revisions { get; set; }
    }
}
