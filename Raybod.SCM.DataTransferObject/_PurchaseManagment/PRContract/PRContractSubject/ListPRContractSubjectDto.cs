using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.PRContract
{
    public class ListPRContractSubjectDto : BasePRContractSubjectDto
    {
        public long PRContractSubjectId { get; set; }

        public string ProductCode { get; set; }

        public string ProductName { get; set; }

        public string ProductUnit { get; set; }

        public string RFPNumber { get; set; }

        public string ProductGroupTitle { get; set; }

        public string TechnicalNumber { get; set; }

        public decimal OrderQuantity { get; set; }

        public decimal ReceiptQuantity { get; set; }

        public decimal RemainedQuantity
        {
            get
            {
                return OrderQuantity - ReceiptQuantity;
            }
        }


    }

    public class ListPRContractSubjectToEditInfoDto 
    {
        public long PRContractSubjectId { get; set; }
        public long ProductGroupId { get; set; }
        public int ProductId { get; set; }
        public long Id { get; set; }
        public decimal Price { get; set; }

        public string ProductGroupTitle { get; set; }

        public long RFPId { get; set; }

        public long DateStart { get; set; }

        public bool IsActive { get; set; }


        public long DateEnd { get; set; }


        public decimal Quantity { get; set; }
        public string RFPNumber { get; set; }

        public long? DateWinner { get; set; }

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
        public decimal OrderQuantity { get; set; }

        public decimal ReceiptQuantity { get; set; }

        public decimal RemainedQuantity
        {
            get
            {
                return OrderQuantity - ReceiptQuantity;
            }
        }
        [Display(Name = "مبلغ کل")]
        public decimal PriceTotal
        {
            get
            {
                return Quantity * Price;
            }
        }

    }
}
