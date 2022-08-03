using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Supplier
{
    public class AddSupplierDto : BaseSupplierDto
    {
        public List<int> ProductGroups { get; set; }
    }
}
