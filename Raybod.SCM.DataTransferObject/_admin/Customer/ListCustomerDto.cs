using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Customer
{
    public class ListCustomerDto : BaseCustomerDto
    {
        public List<CustomerContractDto> CustomerProducts { get; set; }
        public ListCustomerDto()
        {
            CustomerProducts = new List<CustomerContractDto>();
        }
    }
}
