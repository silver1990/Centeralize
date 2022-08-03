using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject._PanelOperation;
using Raybod.SCM.DataTransferObject.User;
using Raybod.SCM.Services.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Core
{
    public interface IOperationActivityService
    {
        Task<ServiceResult<List<UserMentionDto>>> GetActivityUserListAsync(AuthenticateDto authenticate, Guid operationId);
        Task<ServiceResult<List<BaseOperationActivityDto>>> GetOperationActivityListAsync(AuthenticateDto authenticate, Guid operationId);

        Task<ServiceResult<OperationActivityDto>> AddOperationActivityAsync(AuthenticateDto authenticate, Guid operationId,AddOperationActivityDto model);

        Task<ServiceResult<double>> DeleteOperationActivityAsync(AuthenticateDto authenticate, Guid operationId,long operationActivityId);

        Task<ServiceResult<OperationActivityDto>> SetOperationActivityStatusAsync(AuthenticateDto authenticate, Guid operationId,long revisionActivityId);

        Task<ServiceResult<OperationActivityDto>> EditOperationActivityAsync(AuthenticateDto authenticate, Guid operationId, long revisionActivityId, AddOperationActivityDto model);

        Task<ServiceResult<ResultAddActivityTimeSheetDto>> AddActivityTimeSheetAsync(AuthenticateDto authenticate, Guid operationId, long revisionActivityId, AddActivityTimeSheetDto model);

        Task<ServiceResult<List<ListActivityTimeSheetDto>>> GetActivityTimeSheetAsync(AuthenticateDto authenticate, Guid operationId, long revisionActivityId);

        Task<ServiceResult<ActivityTimeSheetDto>> DeleteActivityTimeSheetAsync(AuthenticateDto authenticate, Guid operationId,long revisionActivityId, long activityTimeSheetId);
    }
}
