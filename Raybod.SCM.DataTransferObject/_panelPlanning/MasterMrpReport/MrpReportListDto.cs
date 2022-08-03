namespace Raybod.SCM.DataTransferObject.MasterMrpReport
{
   public class MrpReportListDto
    {
        public long MRPItemId { get; set; }

        public long MRPId { get; set; }

        public string MrpNumber { get; set; }

        public long? CreateDate { get; set; }

        public decimal GrossRequirement { get; set; }

        public decimal WarehouseStock { get; set; }

        public decimal ReservedStock { get; set; } = 0;

        public decimal NetRequirement
        {
            get
            {
                return (GrossRequirement - WarehouseStock + ReservedStock) < 0 ? 0 : GrossRequirement - WarehouseStock + ReservedStock;
            }
        }

        public decimal SurplusQuantity { get; set; } = 0;

        public decimal FinalRequirment
        {
            get
            {
                return SurplusQuantity + NetRequirement;
            }
        }

        public decimal PO { get; set; }

        public decimal PR
        {
            get
            {
                return FinalRequirment - PO;
            }
        }

        public long DateStart { get; set; }

        public long DateEnd { get; set; }
    }
}
