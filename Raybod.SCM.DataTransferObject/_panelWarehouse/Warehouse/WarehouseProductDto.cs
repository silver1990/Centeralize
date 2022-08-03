using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Warehouse
{
    public class WarehouseProductDto : AddWarehouseProduct
    {
        public long Id { get; set; }

        /// <summary>
        /// کد کالا
        /// </summary>
        [Display(Name = "کد محصول")]
        public string ProductCode { get; set; }

        /// <summary>
        /// نام کالا
        /// </summary>
        [Display(Name = "نام محصول")]
        public string ProductName { get; set; }


        public int ProductGroupId { get; set; }

        /// <summary>
        /// واحد کالا
        /// </summary>
        [Display(Name = "واحد اصلی")]
        public string ProductUnit { get; set; }

        /// <summary>
        /// نام گروه کالا
        /// </summary>
        [Display(Name = "گروه کالا")]
        public string ProductGroupName { get; set; }

        /// <summary>
        /// شماره فنی کالا
        /// </summary>
        [Display(Name = "شماره فنی")]
        public string TechnicalNumber { get; set; }

        public long? LastUpdateDate { get; set; }
    }
}
