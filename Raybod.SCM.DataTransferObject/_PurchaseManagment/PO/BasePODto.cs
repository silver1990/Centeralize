using Raybod.SCM.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.PO
{
    public class BasePODto
    {
        /// <summary>
        /// شناسه PO
        /// </summary>
        public long POId { get; set; }

        /// <summary>
        /// کد PO
        /// </summary>
        public string POCode { get; set; }

        /// <summary>
        /// وضعیت
        /// </summary>
        public POStatus POStatus { get; set; }

        /// <summary>
        /// واحد پول
        /// </summary>
        public CurrencyType CurrencyType { get; set; }

        /// <summary>
        /// نوع قرارداد
        /// </summary>
        public PContractType PContractType { get; set; }

        /// <summary>
        /// مکان تحویل
        /// </summary>
        public POIncoterms DeliveryLocation { get; set; }

        /// <summary>
        /// تاریخ تحویل
        /// </summary>
        [Required]
        public long DateDelivery { get; set; }

    }
}
