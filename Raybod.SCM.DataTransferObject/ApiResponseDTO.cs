using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raybod.SCM.DataTransferObject
{
    public class ApiResponseDTO
    {
        public short Status { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }
}
