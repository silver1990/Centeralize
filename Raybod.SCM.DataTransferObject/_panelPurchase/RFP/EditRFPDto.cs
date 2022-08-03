using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.RFP
{
    public class EditRFPDto
    {
        public List<int> SupplierIds { get; set; }

        [Required(ErrorMessage ="تاریخ سررسید الزامی می باشد")]
        public long DateDue { get; set; }
    }
}
