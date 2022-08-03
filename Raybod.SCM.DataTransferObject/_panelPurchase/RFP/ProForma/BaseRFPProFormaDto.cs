using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Raybod.SCM.DataTransferObject.RFP
{
    public class BaseRFPProFormaDto
    {
        [MaxLength(500)]
        public string Duration { get; set; }

        /// <summary>
        /// قیمت
        /// </summary>
        
        public string Price { get; set; }
    }
}
