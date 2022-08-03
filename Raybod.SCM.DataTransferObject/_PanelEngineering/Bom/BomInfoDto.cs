using System;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Bom
{
    public class BomInfoDto : BaseBomDto
    {
        public long Id { get; set; }

        [Display(Name = "کد کالا")]
        public string ProductCode { get; set; }

        [Display(Name = "شرح کالا")]
        public string ProductDescription { get; set; }

        [Display(Name = "واحد کالا")]
        public string ProductUnit { get; set; }

        [Display(Name = "شماره فنی کالا")]
        public string ProductTechnicalNumber { get; set; }
        
        public UserAuditLogDto UserAudit { get; set; }
    }
}
