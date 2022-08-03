using Raybod.SCM.DataTransferObject.User;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Document
{
    public class InProgressRevisionListDto : DocumentRevisionListDto
    {
        public List<UserMentionDto> ActivityUsers { get; set; }

        public InProgressRevisionListDto()
        {
            ActivityUsers = new List<UserMentionDto>();
        }
    }
}
