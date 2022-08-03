using Raybod.SCM.DataTransferObject.Consultant;
using Raybod.SCM.DataTransferObject.Customer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Contract
{
    public class ContractDetailsDto
    {
        public ContractDurationDto ProjectTimeTable { get; set; }
        public BaseCustomerDto ProjectCustomer { get; set; }
        public BaseConsultantDto ProjectConsultant { get; set; }
    }
}
