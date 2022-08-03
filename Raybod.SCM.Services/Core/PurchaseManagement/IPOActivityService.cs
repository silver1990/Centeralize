using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.PO.POActivity;
using Raybod.SCM.DataTransferObject.User;
using Raybod.SCM.Services.Core.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Core
{
    public interface IPOActivityService
    {
        Task<ServiceResult<BasePOAvtivityDto>> AddPOActivityAsync(AuthenticateDto authenticate, long poId,
                    AddPOActivityDto model);

        Task<ServiceResult<List<BasePOAvtivityDto>>> GetPOActivityListAsync(AuthenticateDto authenticate, long poId);

        Task<ServiceResult<BasePOAvtivityDto>> SetActivityStatusAsync(AuthenticateDto authenticate, long poId, long poActivityId);

        Task<ServiceResult<double>> DeletePOActivityAsync(AuthenticateDto authenticate, long poId,
            long poActivityId);

        Task<ServiceResult<BasePOAvtivityDto>> EditPOActivityAsync(AuthenticateDto authenticate, long poId, long poActivityId, AddPOActivityDto model);

        Task<ServiceResult<ResultAddActivityTimeSheetDto>> AddActivityTimeSheetAsync(AuthenticateDto authenticate, long poId,
            long poActivityId, AddActivityTimeSheetDto model);

        Task<ServiceResult<List<ListActivityTimeSheetDto>>> GetActivityTimeSheetAsync(AuthenticateDto authenticate, long poId,
            long poActivityId);

        Task<ServiceResult<ActivityTimeSheetDto>> DeleteActivityTimeSheetAsync(AuthenticateDto authenticate, long poId,
            long poActivityId, long activityTimeSheetId);

        Task<ServiceResult<List<UserMentionDto>>> GetActivityUserListAsync(AuthenticateDto authenticate, long POId);

    }
}
