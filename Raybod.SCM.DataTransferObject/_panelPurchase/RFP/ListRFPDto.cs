using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.DataTransferObject.RFP
{
    public class ListRFPDto : BaseRFPDto
    {
        public long Id { get; set; }

        public string ProductGroupTitle { get; set; }

        public int ProductGroupId { get; set; }

        [Required]        
        public string RFPNumber { get; set; }
                
        public string ContractCode { get; set; }

        public RFPStatus Status { get; set; }

        public List<string> RFPItems { get; set; }

        public List<string> Suppliers { get; set; }

        public long? DateCreate { get; set; }
    }
}
