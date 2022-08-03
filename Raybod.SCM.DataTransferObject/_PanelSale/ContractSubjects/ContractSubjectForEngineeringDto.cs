using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.ContractSubjects
{
    public class ContractSubjectForEngineeringDto
    {
        public int Id { get; set; }

        [Display(Name = "کد قرارداد")]
        public string ContractCode { get; set; }

        [Display(Name = "شرح قرارداد")]
        public string ContractDescription { get; set; }

        [Display(Name = "شناسه محصول")]
        public int ProductId { get; set; }

        [Display(Name = "کد محصول")]
        public string ProductCode { get; set; }

        [Display(Name = "نام محصول")]
        public string ProductName { get; set; }

        [Display(Name = "واحد اصلی")]
        public string ProductUnit { get; set; }

        public UserAuditLogDto UserAudit { get; set; }
    }
}
