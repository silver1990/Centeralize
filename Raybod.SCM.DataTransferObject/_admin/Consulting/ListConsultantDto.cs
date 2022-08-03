using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Consultant
{
    public class ListConsultantDto : BaseConsultantDto
    {
        public List<ConsultantContractDto> ConsultantProducts { get; set; }
        public ListConsultantDto()
        {
            ConsultantProducts = new List<ConsultantContractDto>();
        }
    }
}
