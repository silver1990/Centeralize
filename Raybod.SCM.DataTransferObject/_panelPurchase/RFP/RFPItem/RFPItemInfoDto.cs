using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.DataTransferObject.RFP
{
    public class RFPItemInfoDto : BaseRFPItemDto
    {
        /// <summary>
        /// کد درخواست خرید
        /// </summary>
        public string PRCode { get; set; }

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

        /// <summary>
        /// وضعیت مدارک مهندسی
        /// </summary>
        public EngineeringDocumentStatus DocumentStatus { get; set; }

    }
}
