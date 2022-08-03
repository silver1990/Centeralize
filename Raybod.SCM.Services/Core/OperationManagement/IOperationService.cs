using Microsoft.AspNetCore.Http;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject._PanelOperation;
using Raybod.SCM.DataTransferObject.Document;
using Raybod.SCM.DataTransferObject.Operation;
using Raybod.SCM.Services.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Core
{
    public interface IOperationService
    {
        Task<ServiceResult<List<OperationViewDto>>> AddOperationAsync(AuthenticateDto authenticate, int operationGroupId, List<AddOperationDto> model);
        Task<ServiceResult<OperationViewDto>> AddOperationAsync(AuthenticateDto authenticate, int operationGroupId, AddOperationDto model);
        Task<ServiceResult<List<OperationViewDto>>> GetOperationAsync(AuthenticateDto authenticate, OperationQueryDto query);
        Task<ServiceResult<OperationDetailDto>> GetOperationByIdAsync(AuthenticateDto authenticate, Guid operationId);
        Task<ServiceResult<List<NotStartedOperationListDto>>> GetNotStartedOperationAsync(AuthenticateDto authenticate, OperationQueryDto query);
        Task<ServiceResult<List<StartedOperationViewDto>>> GetStartedOperationAsync(AuthenticateDto authenticate, OperationQueryDto query);
        Task<ServiceResult<OperationViewDto>> EditOperationByOperationIdAsync(AuthenticateDto authenticate, Guid operationId, EditOperationDto model);
        Task<ServiceResult<bool>> ActiveOrDeactiveOperationByOperationIdAsync(AuthenticateDto authenticate, Guid operationId);

        Task<ServiceResult<StartedOperationViewDto>> StartOperation(AuthenticateDto authenticate, Guid operationId, StartOperationDto model);

        Task<ServiceResult<List<OperationViewDto>>> StartOperation(AuthenticateDto authenticate, List<StartOperationsDto> model);

        Task<ServiceResult<int>> GetInProgressOperationBadgeCountAsync(AuthenticateDto authenticate);

        Task<ServiceResult<int>> GetPendingForConfirmOperationBadgeCountAsync(AuthenticateDto authenticate);

        Task<ServiceResult<bool>> RestartOperationByOperationIdAsync(AuthenticateDto authenticate, Guid operationId);
        Task<ServiceResult<List<OperationAttachmentDto>>> AddOperationAttachmentAsync(AuthenticateDto authenticate, Guid operationId, IFormFileCollection files);
        Task<ServiceResult<List<OperationAttachmentDto>>> GetOperationAttachmentAsync(AuthenticateDto authenticate, Guid operationId);
        Task<ServiceResult<bool>> DeleteOperationAttachmentAsync(AuthenticateDto authenticate, Guid operationId, string fileSrc);
        Task<DownloadFileDto> DownloadOperationFileAsync(AuthenticateDto authenticate, Guid operationId, string fileSrc);
        Task<ServiceResult<bool>> AbortOperationByOperationIdAsync(AuthenticateDto authenticate, Guid operationId);
        Task<ServiceResult<bool>> ConfirmOperationByOperationIdAsync(AuthenticateDto authenticate, Guid operationId);
    }
}
