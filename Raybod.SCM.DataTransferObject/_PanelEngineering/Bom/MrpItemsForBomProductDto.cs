using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Bom
{
    public class MrpItemsForBomProductDto
    {
        public long MrpItemId { get; set; }

        public int ProductId { get; set; }

        public long MrpId { get; set; }
        public long MasterMRId { get; set; }
        public decimal GrossRequirement { get; set; }

        public decimal NetRequirement { get; set; } // return GrossRequirement - StockWarehouse + ReservedStock;

        public decimal WarehouseStock { get; set; }

        public decimal ReservedStock { get; set; }

        public decimal SurplusQuantity { get; set; }

        public decimal FinalRequirment { get; set; }   // return ((SafetyPercent / 100) + 1) * NetRequirement;

        public decimal RemainedStock { get; set; }

        public decimal DoneStock { get; set; }

        public decimal PR { get; set; }

        public decimal PO { get; set; }
        public MrpItemStatus MrpItemStatus { get; set; }

    }
}
