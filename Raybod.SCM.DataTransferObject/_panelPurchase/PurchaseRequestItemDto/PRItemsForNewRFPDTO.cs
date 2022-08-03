using Raybod.SCM.Domain.Enum;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.DataTransferObject.PurchaseRequest
{
    public class PRItemsForNewRFPDTO
    {
        /// <summary>
        /// کد کالا
        /// </summary>
        public string ProductCode { get; set; }

        /// <summary>
        /// شرح کالا
        /// </summary>
        public string ProductDescription { get; set; }

        /// <summary>
        /// واحد کالا
        /// </summary>
        public string ProductUnit { get; set; }

        /// <summary>
        /// شماره فنی کالا
        /// </summary>
        public string ProductTechnicalNumber { get; set; }

        /// <summary>
        ///  گروه کالا
        /// </summary>
        public string ProductGroupName { get; set; }

        public int ProductGroupId { get; set; }

        /// <summary>
        /// وضعیت مدارک مهندسی
        /// </summary>
        public EngineeringDocumentStatus DocumentStatus { get; set; }

        public long PRItemId { get; set; }

        public int ProductId { get; set; }

        public long PurchaseRequestId { get; set; }

        public string PRCode { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Quntity { get; set; }

        public long DateStart { get; set; }

        public long DateEnd { get; set; }
    }
}
