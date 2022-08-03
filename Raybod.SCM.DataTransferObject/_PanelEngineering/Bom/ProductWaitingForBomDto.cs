namespace Raybod.SCM.DataTransferObject.Bom
{
    public class ProductWaitingForBomDto
    {
        public int ProductId { get; set; }

        public string ContractCode { get; set; }

        public string ContractNumber { get; set; }

        public string ContractDescription { get; set; }

        public long? DateCreate { get; set; }

        public string ProductCode { get; set; }

        public string ProductDescription { get; set; }
        public string ProductTechnicalNumber { get; set; }

        public UserAuditLogDto UserAudit { get; set; }
    }
}
