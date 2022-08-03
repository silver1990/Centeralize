using Raybod.SCM.DataTransferObject.ProductGroup;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Supplier
{
    public class ListSupplierDto : BaseSupplierDto
    {
        public int Id { get; set; }

        public List<ProductGroupMiniIfoDto> ProductGroups { get; set; }

        public ListSupplierDto()
        {
            ProductGroups = new List<ProductGroupMiniIfoDto>();
        }
    }
}
