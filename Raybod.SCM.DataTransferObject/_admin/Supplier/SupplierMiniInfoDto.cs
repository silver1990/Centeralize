using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Supplier
{
    public class SupplierMiniInfoDto
    {
        public int Id { get; set; }

        [Display(Name = "کد تامین کننده")]
        public string SupplierCode { get; set; }

        [Display(Name = "نام شرکت")]
        public string Name { get; set; }

        public List<string> ProductGroups { get; set; }

        public string TellPhone { get; set; }

        [Display(Name = "ایمیل")]
        public string Email { get; set; }

        [Display(Name = "لوگو")]
        public string Logo { get; set; }
        public string PostalCode { get; set; }

        public string EconomicCode { get; set; }

        public string NationalId { get; set; }
        public string Address { get; set; }

        [MaxLength(300)]
        public string Website { get; set; }
        public SupplierMiniInfoDto()
        {
            ProductGroups = new List<string>();
        }
    }
}
