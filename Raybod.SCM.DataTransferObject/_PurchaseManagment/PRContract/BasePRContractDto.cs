using System.ComponentModel.DataAnnotations;
using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.DataTransferObject.PRContract
{
    public class BasePRContractDto
    {

        [Required(ErrorMessage = "الزامی می باشد")]
        public long DateIssued { get; set; }

        [Required(ErrorMessage = "الزامی می باشد")]
        public long DateEnd { get; set; }

        [Required(ErrorMessage = "الزامی می باشد")]
        public int ContractDuration { get; set; }

        public int ProductGroupId { get; set; }

        public int SupplierId { get; set; }

        public PContractType PContractType { get; set; }

        /// <summary>
        /// مکان تحویل
        /// </summary>
        public POIncoterms DeliveryLocation { get; set; }


        public CurrencyType CurrencyType { get; set; }

        public decimal Tax { get; set; }

    }
}