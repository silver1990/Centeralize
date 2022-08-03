using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Contract
{
    public class ContractSubjectMiniInfoDto
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public string ProductCode { get; set; }

        public string Productdescription { get; set; }

        [Display(Name = "واحد اصلی")]
        public string ProductUnit { get; set; }
    }
}
