using Raybod.SCM.DataTransferObject._PanelOperation.OperationGroup;
using Raybod.SCM.DataTransferObject.Customer;
using Raybod.SCM.DataTransferObject.Document;
using Raybod.SCM.DataTransferObject.ProductGroup.Group;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.TeamWork
{
    public class TeamWorkUserPermissionsDto
    {
        public int UserId { get; set; }

        public string UserFullName { get; set; }

        public string UserName { get; set; }

        public string UserImage { get; set; }

        public string UserEmail { get; set; }

        public string UserMobile { get; set; }

        public bool IsActive { get; set; }
        public int UserType { get; set; }

        public List<ListDocumentGroupDto> DocumentGroups { get; set; }
        public List<ListOperationGroupDto> OperationGroups { get; set; }

        public List<ProductGroupInfoDto> UserProductGroups { get; set; }

        public List<UserRoleDto> UserRoles { get; set; }
        public MiniCompanyInfoDto Company { get; set; }
        public TeamWorkUserPermissionsDto()
        {
            UserProductGroups = new List<ProductGroupInfoDto>();
            DocumentGroups = new List<ListDocumentGroupDto>();
            UserRoles = new List<UserRoleDto>();
        }

    }
}
