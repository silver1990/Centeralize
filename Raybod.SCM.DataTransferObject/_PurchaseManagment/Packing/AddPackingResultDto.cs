using Raybod.SCM.DataTransferObject.PO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Packing
{
    public class AddPackingResultDto
    {
        public PackListDto PackInfo { get; set; }
        public List<POSubjectInfoDto> POSubjects { get; set; }
        public AddPackingResultDto()
        {
            POSubjects = new List<POSubjectInfoDto>();

        }
    }
}
