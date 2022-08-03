using Raybod.SCM.Domain.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.DataTransferObject.PurchaseRequest
{
    public class BasePurchaseRequestDto
    {
        public long Id { get; set; }

        public string ContractCode { get; set; }

        [Required]
        public string PRCode { get; set; }

        public long? MrpId { get; set; }

        public TypeOfInquiry TypeOfInquiry { get; set; }

        [MaxLength(800)]
        public string Note { get; set; }

        [MaxLength(800)]
        public string ConfirmNote { get; set; }

        public PRStatus PRStatus { get; set; }
    }
}
