using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.PO
{
    public class ListPODto
    {
        /// <summary>
        /// شناسه po
        /// </summary>
        public long POId { get; set; }

        public long DateDelivery { get; set; }

        public string POCode { get; set; }

        public string SupplierName { get; set; }

        /// <summary>
        /// تاریخ ثبت
        /// </summary>
        public long? CreateDate { get; set; }

        /// <summary>
        /// کالا ها
        /// </summary>
        public List<string> Products { get; set; }

        public ListPODto()
        {
            Products = new List<string>();
        }
    }
    public class ListAllPODto
    {
        /// <summary>
        /// شناسه po
        /// </summary>
        public long POId { get; set; }


        public string POCode { get; set; }

        public string SupplierName { get; set; }

        /// <summary>
        /// تاریخ ثبت
        /// </summary>
        public long? CreateDate { get; set; }

        /// <summary>
        /// کالا ها
        /// </summary>
        public List<string> Products { get; set; }
        public POStatus POStatus { get; set; }

        public bool IsPaymentDone { get; set; }
        public int  SortingRank { get; set; }
        public POShortageStatus ShortageStatus { get; set; }
        public ListAllPODto()
        {
            Products = new List<string>();
        }
    }
}
