using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Document
{
    public class AddContractDocumentGroupDto
    {
        [Required]
        public string ContractCode { get; set; }

        public int DocumentGroupId { get; set; }
    }
}
