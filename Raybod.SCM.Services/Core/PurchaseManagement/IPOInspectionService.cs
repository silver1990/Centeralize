using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.PO.POActivity;
using Raybod.SCM.DataTransferObject.PO.POInspection;
using Raybod.SCM.DataTransferObject.User;
using Raybod.SCM.Services.Core.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Core
{
    public interface IPOInspectionService
    {
        Task<ServiceResult<List<POInspectionDto>>> GetPoInspectionAsync(AuthenticateDto authenticate, long poId);
        Task<ServiceResult<List<UserMentionDto>>> GetInspectionUserListAsync(AuthenticateDto authenticate, long POId);
        Task<ServiceResult<POInspectionDto>> AddPoInspectionAsync(AuthenticateDto authenticate, long poId, AddPOInspectionDto model);
        Task<ServiceResult<POInspectionDto>> AddInspectionResultAsync(AuthenticateDto authenticate, long poId, long poInspectionId, AddPOInspectionResultDto model);
        Task<ServiceResult<bool>> DeletePoInspectionAsync(AuthenticateDto authenticate, long poId,long incpectionId);

        Task<DownloadFileDto> DownloadPOInspectionAttachmentAsync(AuthenticateDto authenticate, long poId,long inspectionId, string fileSrc);
    }
}
