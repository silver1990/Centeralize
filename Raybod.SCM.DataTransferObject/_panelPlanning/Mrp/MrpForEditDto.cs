using Raybod.SCM.DataTransferObject.Contract;
using Raybod.SCM.DataTransferObject.MrpItem;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Mrp
{
    public class MrpForEditDto
    {
        public long MrpId { get; set; }

        public string MrpNumber { get; set; }

        public string Description { get; set; }

        public string ContractCode { get; set; }

        public string ProductGroupTitle { get; set; }

        public int ProductGroupId { get; set; }

        //public List<ContractSubjectMiniInfoDto> ContractSubjects { get; set; }

        public List<MrpItemInfoDto> MrpItems { get; set; }

        public MrpForEditDto()
        {
            //ContractSubjects = new List<ContractSubjectMiniInfoDto>();
            MrpItems = new List<MrpItemInfoDto>();
        }
    }
}
