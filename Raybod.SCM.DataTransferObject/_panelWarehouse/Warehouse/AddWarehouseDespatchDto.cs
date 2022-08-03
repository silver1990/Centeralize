using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Warehouse
{
    public class AddWarehouseDespatchDto
    {
        public List<AddWarehousDespatchItemsDto> WarehouseDespatchItem { get; set; }
    }
    public class AddWarehousDespatchItemsDto
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "الزامی می باشد")]
        public decimal Delivery { get; set; }

    }
}
