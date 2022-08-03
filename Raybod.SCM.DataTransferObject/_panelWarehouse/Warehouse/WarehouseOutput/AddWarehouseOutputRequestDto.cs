using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Warehouse
{
    public class AddWarehouseOutputRequestDto
    {
        public long? RecepitId { get; set; }
        public string RecepitCode { get; set; }
        public AddWarehouseRequestConfirmationDto WorkFlow { get; set; }
        public List<WarehouseOutputRequestSubjecDto> Subjects { get; set; }
    }
}
