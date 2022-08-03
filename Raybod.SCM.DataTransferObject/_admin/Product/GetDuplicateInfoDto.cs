using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Product
{
    public class GetDuplicateInfoDto
    {
        public string ProductCode { get; set; }
        public string Description { get; set; }
        public string TechnicalNumber { get; set; }
        public string Unit { get; set; }
        public string ProductGroupTitle { get; set; }
        public int? ProductId { get; set; }
        public List<DuplicateOptionDto> Options { get; set; }
        public GetDuplicateInfoDto()
        {
            Options = new List<DuplicateOptionDto>();
        }
    }
}
