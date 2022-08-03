using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.PO
{
    public class AddPODto
    {
        [Required(ErrorMessage = "الزامی می باشد")]
        public long DateDelivery { get; set; }

        public List<AddPOSubjectDto> POSubjects { get; set; }

    }
}
