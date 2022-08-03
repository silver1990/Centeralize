using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Warehouse
{
    public class WarehouseOutputRequestSubjectListDto
    {
        [Display(Name = "کد محصول")]
        public string ProductCode { get; set; }

        [Display(Name = "نام محصول")]
        public string ProductDescription { get; set; }

        [Display(Name = "واحد محصول")]
        public string ProductUnit { get; set; }

        [Display(Name = "شماره فنی")]
        public string ProductTechnicalNumber { get; set; }

        [Display(Name = "گروه کالا")]
        public string ProductGroupName { get; set; }
        public int ProductId { get; set; }
        public long RequestId { get; set; }
        public long RequestSubjectId { get; set; }

        [Required(ErrorMessage = "الزامی می باشد")]
        public decimal Quntity { get; set; }

        [Required(ErrorMessage = "الزامی می باشد")]
        public decimal Delivery { get; set; }
        [Required(ErrorMessage = "الزامی می باشد")]
        public decimal Inventory { get; set; }

    }
}
