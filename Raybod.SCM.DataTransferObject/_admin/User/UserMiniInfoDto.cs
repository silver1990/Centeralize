using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.User
{
    public class UserMiniInfoDto
    {
        public int UserId { get; set; }
        public string UserFullName { get; set; }

        /// <summary>
        /// نام کاربری
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// عکس
        /// </summary>
        public string Image { get; set; }
    }

    public class UserInfoForAuditLogDto
    {
        public int UserId { get; set; }
        public List<int> DocumentGroupIds { get; set; }
        public List<int> ProductGroupIds { get; set; }
        public List<int> OperationGroupIds { get; set; }
      
    }
}