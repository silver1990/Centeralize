using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Raybod.SCM.DataTransferObject.PRContract
{
    public class AddPrContractConfirmationDto
    {
        [MaxLength(800)]
        public string Note { get; set; }
        public List<AddPrContractUserConfirmationDto> Users { get; set; }
    }
}
