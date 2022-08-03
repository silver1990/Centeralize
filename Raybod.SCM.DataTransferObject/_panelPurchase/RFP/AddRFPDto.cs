using Raybod.SCM.DataTransferObject.Supplier;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.RFP
{
    public class AddRFPDto : BaseRFPDto
    {
        /// <summary>
        /// تامین کننده ها
        /// </summary>
        [Required]
        public List<SupplierMiniInfoDto> Suppliers { get; set; }
        
        /// <summary>
        /// ایتم های RFP
        /// </summary>
        [Required]
        public virtual List<AddRFPItemDto> RFPItems { get; set; }

        /// <summary>
        /// استعلام ها
        /// </summary>
        public virtual List<AddRFPInqueryDto> TechInquiries { get; set; }
        public virtual List<AddRFPInqueryDto> CommercialInquiries { get; set; }

        /// <summary>
        /// فایل ضمیمه ها
        /// </summary>
        public virtual List<AddAttachmentDto> Attachmnets { get; set; }

    }
}
