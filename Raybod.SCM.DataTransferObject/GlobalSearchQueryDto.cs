using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject
{
    public class GlobalSearchQueryDto
    {
        public string Text { get; set; }

        public List<SCMFormPermission> SCMFormPermissions { get; set; }
    }
}
