using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.PO
{
    public class EditPODto
    {
        /// <summary>
        /// تاریخ تحویل
        /// </summary>
        [Required(ErrorMessage = "الزامی می باشد")]
        public long DateDelivery { get; set; }

        /// <summary>
        /// موضوع سفارش
        /// </summary>
        [Required(ErrorMessage = "الزامی می باشد")]
        public List<EditPOSubjectDto> POSubjects { get; set; }

    }
}
