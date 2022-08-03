using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Warehouse
{
    public class ListWarehouseProductLogDto
    {
        public decimal Inputs { get; set; }

        public decimal Outputs { get; set; }

        public decimal RealStocks { get; set; }

        public List<WarehouseProductLogDto> AuditLogs { get; set; }

        public ListWarehouseProductLogDto()
        {
            AuditLogs = new List<WarehouseProductLogDto>();
        }
    }
}
