using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Warehouse
{
    public class WarehouseRequestConfirmationUserWorkFlowDto
    {
        public long WarehouseOutputRequestWorkFlowUserId { get; set; }

        public int UserId { get; set; }

        public string UserFullName { get; set; }

        public string UserImage { get; set; }

        public int OrderNumber { get; set; }

        public string Note { get; set; }

        public bool IsBallInCourt { get; set; }

        public bool IsAccept { get; set; }
        public long? DateEnd { get; set; }
        public long? DateStart { get; set; }

    }
}
