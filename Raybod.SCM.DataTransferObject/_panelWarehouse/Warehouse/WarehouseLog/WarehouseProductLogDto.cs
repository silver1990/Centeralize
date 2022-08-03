using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.DataTransferObject.Warehouse
{
    public class WarehouseProductLogDto
    {
        public long Id { get; set; }
        
        public int ProductId { get; set; }
        public string RequestCode { get; set; }
        public string DespatchCode { get; set; }

        public string RecepitCode { get; set; }

        public int WarehouseId { get; set; }

        public string ProductDescription { get; set; }

        public long? ReceiptId { get; set; }

        public long? WarehouseTransferenceId { get; set; }


        public string ProductCode { get; set; }

        public string TechnicalNumber { get; set; }

        public string Unit { get; set; }

        public WarehouseStockChangeActionType WarehouseStockChangeActionType { get; set; }

        public long DateChange { get; set; }

        public decimal Input { get; set; }

        public decimal Output { get; set; }

        public decimal RealStock { get; set; }


    }

    public class WarehouseProductLogExcelDto
    {
        public string Date { get; set; }

        public string Operation { get; set; }
        public string Reference { get; set; }
        public decimal QuantityIn { get; set; }

        public decimal QuantityOut { get; set; }

        public decimal RemaindQuantity { get; set; }

       
    }
}
