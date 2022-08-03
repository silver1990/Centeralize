using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Document.Communication;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Model;
using Raybod.SCM.Services.Core.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Core
{
    public interface ICommunicationTeamCommentService
    {
        Task<ServiceResult<string>> AddCommentAsync(AuthenticateDto authenticate,
            long communicationId, AddCommunicationTeamCommentDto model);

        Task<ServiceResult<List<CommunicationTeamCommentListDto>>> GetCommentAsync(AuthenticateDto authenticate,
            long communicationId, CommunicationTeamCommentQueryDto query);

        Task<ServiceResult<string>> AddTQAndNCRCommentAsync(AuthenticateDto authenticate,
            long communicationId, CommunicationType communicationType, AddCommunicationTeamCommentDto model);

        Task<ServiceResult<List<CommunicationTeamCommentListDto>>> GetTQAndNCRCommentAsync(AuthenticateDto authenticate,
            long communicationId, CommunicationTeamCommentQueryDto query);
    }
}
