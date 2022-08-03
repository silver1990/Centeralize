using Raybod.SCM.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.ProductGroup
{
    public class AddProductGroupDto
    {

        public string ProductGroupCode { get; set; }

        [Display(Name = "عنوان گروه کالا")]
        [Required(ErrorMessage = "الزامی می باشد")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "حداقل مقدار برای فیلد {2} و حداکثر {1} می باشد.")]
        public string Title { get; set; }
               
    }
}
