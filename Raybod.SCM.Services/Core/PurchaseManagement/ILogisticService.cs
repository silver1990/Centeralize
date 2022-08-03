using Microsoft.AspNetCore.Http;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Logistic;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Services.Core.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Core
{
    public interface ILogisticService
    {
        Task<ServiceResult<List<PackLogisticListDto>>> GetPoPackLogisticAsync(AuthenticateDto authenticate, long poId);

        Task<ServiceResult<List<BaseLogisticDto>>> GetPackLogisticByPackIdAsync(AuthenticateDto authenticate, long poId, long packId);

        Task<ServiceResult<bool>> StartTransportationAsync(AuthenticateDto authenticate, long poId, long packId, LogisticStep logisticStep);

        Task<ServiceResult<bool>> CompeleteTransportationAsync(AuthenticateDto authenticate, long poId, long packId, LogisticStep logisticStep);

        Task<ServiceResult<bool>> StartClearancePortAsync(AuthenticateDto authenticate, long poId, long packId, LogisticStep logisticStep);

        Task<ServiceResult<bool>> CompeleteClearancePortAsync(AuthenticateDto authenticate, long poId, long packId, LogisticStep logisticStep);

        Task<ServiceResult<List<LogisticAttachmentDto>>> GetLogisticAttachmentAsync(AuthenticateDto authenticate, long poId, long packId, LogisticStep logisticStep);

        Task<ServiceResult<List<LogisticAttachmentDto>>> AddLogisticAttachmentAsync(AuthenticateDto authenticate, long poId,
           long packId, LogisticStep logisticStep, IFormFileCollection files);

        Task<DownloadFileDto> DownloadLogisticAttachmentAsync(AuthenticateDto authenticate, long poId, long packId, LogisticStep logisticStep, string fileSrc);

        Task<ServiceResult<bool>> DeleteLogisticAttachmentAsync(AuthenticateDto authenticate, long poId, long packId, LogisticStep step, string fileSrc);

    }
}
