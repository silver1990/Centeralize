using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Document;
using Raybod.SCM.DataTransferObject.User;
using Raybod.SCM.Services.Core.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Core
{
    public interface IRevisionActivityService
    {
        Task<ServiceResult<List<UserMentionDto>>> GetActivityUserListAsync(AuthenticateDto authenticate, long revisionId);

        Task<ServiceResult<BaseRevisionAvtivityDto>> AddRevisionActivityAsync(AuthenticateDto authenticate, long documentId, long revisionId,
        AddRevisionActivityDto model);

        Task<ServiceResult<bool>> DeleteRevisionActivityAsync(AuthenticateDto authenticate, long documentId, long revisionId,
            long revisionActivityId);

        Task<ServiceResult<bool>> SetRevisionActivityStatusAsync(AuthenticateDto authenticate, long documentId, long revisionId,
            long revisionActivityId);

        Task<ServiceResult<BaseRevisionAvtivityDto>> EditRevisionActivityAsync(AuthenticateDto authenticate, long documentId,
            long revisionId, long revisionActivityId, AddRevisionActivityDto model);

        Task<ServiceResult<ResultAddActivityTimeSheetDto>> AddActivityTimeSheetAsync(AuthenticateDto authenticate, long documentId, long revisionId,
            long revisionActivityId, AddActivityTimeSheetDto model);

        Task<ServiceResult<List<ListActivityTimeSheetDto>>> GetActivityTimeSheetAsync(AuthenticateDto authenticate, long documentId, long revisionId,
            long revisionActivityId);

        Task<ServiceResult<string>> DeleteActivityTimeSheetAsync(AuthenticateDto authenticate, long documentId, long revisionId,
            long revisionActivityId, long activityTimeSheetId);
    }
}
