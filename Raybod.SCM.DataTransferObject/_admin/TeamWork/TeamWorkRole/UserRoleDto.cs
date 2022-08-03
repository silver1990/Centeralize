namespace Raybod.SCM.DataTransferObject.TeamWork
{
    public class UserRoleDto : BaseUserRoleDto
    {
        public string RoleName { get; set; }

        public bool IsGlobalGroup { get; set; }

        public string RoleDisplayName { get; set; }

        /// <summary>
        /// زیر ماژول
        /// </summary>        
        public string SubModuleName { get; set; }

      
    }
}
