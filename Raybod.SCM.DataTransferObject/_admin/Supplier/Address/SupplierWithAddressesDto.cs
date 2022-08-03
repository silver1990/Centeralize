using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Supplier.Address
{
    public class SupplierWithAddressesDto : BaseSupplierDto
    {
        public List<EditSupplierAddressDto> SupplierAddresses { get; set; }
        public SupplierWithAddressesDto()
        {
            SupplierAddresses = new List<EditSupplierAddressDto>();
        }
    }
}
