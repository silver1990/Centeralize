using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.TeamWork.authentication
{
    public class UserLogPermissonDto
    {
        public List<NotifEvent> GlobalPermission { get; set; }
        public List<int> ProductGroupIds { get; set; }
        public List<int> DocumentGroupIds { get; set; }

        public UserLogPermissonDto()
        {
            GlobalPermission = new List<NotifEvent>();
            ProductGroupIds = new List<int>();
            DocumentGroupIds = new List<int>();
        }
    }
}
