using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.RFP
{
    public class AddOrEditSupplierProposalDto
    {
        public RFPEvaluationProposalInfoDto SupplierPropsoal { get; set; }
        public List<RFPSupplierInfoDto> Suppliers { get; set; }
        public RFPStatus RFPStatus { get; set; }
        public AddOrEditSupplierProposalDto()
        {
            Suppliers = new List<RFPSupplierInfoDto>();
        }
    }

    public class SetProposalWinnerDto
    {
        public List<RFPSupplierInfoDto> Suppliers { get; set; }
        public RFPStatus RFPStatus { get; set; }
        public SetProposalWinnerDto()
        {
            Suppliers = new List<RFPSupplierInfoDto>();
        }
    }
}
