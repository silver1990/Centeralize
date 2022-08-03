using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.RFP
{
    public class SupplierProposalInfoDto
    {
        public int SupplierId { get; set; }

        public string SupplierCode { get; set; }

        public string SupplierEmail { get; set; }

        public string SupplierName { get; set; }

        public string SupplierLogo { get; set; }

        public List<ListSupplierProposalInqueryDto> SupplierProposals { get; set; }
        public SupplierProposalInfoDto()
        {
            SupplierProposals = new List<ListSupplierProposalInqueryDto>();
        }

    }
}
