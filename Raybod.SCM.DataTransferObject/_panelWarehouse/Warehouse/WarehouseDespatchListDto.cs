using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Warehouse
{
    public class WarehouseDespatchListDto
    {
        public long DespatchId { get; set; }
        public long RequestId { get; set; }
        public string RequestCode { get; set; }
        public string RecepitCode { get; set; }
        public string DespatchCode { get; set; }
        public string ProductGroupTitle { get; set; }
        public List<string> Products { get; set; }
        public WarehouseDespatchStatus Status { get; set; }
        public long? CreatedDate { get; set; }
    }
}
