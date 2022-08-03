using System;
using System.Collections.Generic;
using System.Text;


    namespace Raybod.SCM.DataTransferObject.Contract
    {
        public class ContractDescriptionUpdateDto
        {
            public string Description { get; set; }
            public List<string> Services { get; set; }
            public ContractDescriptionUpdateDto()
            {
                Services = new List<string>();
            }
        }
    }

