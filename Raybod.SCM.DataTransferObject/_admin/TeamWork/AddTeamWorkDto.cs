using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.TeamWork
{
    public class AddTeamWorkDto
    {

        public string Title { get; set; }

        public bool IsOrganization { get; set; }

        public string ContractCode { get; set; }
    }
}
