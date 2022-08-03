using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Packing;
using Raybod.SCM.DataTransferObject.PO;
using Raybod.SCM.DataTransferObject.QualityControl;
using Raybod.SCM.Services.Core.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Core
{
    public interface IPackService
    {
        Task<ServiceResult<List<WaitingPOSubjectDto>>> GetWaitingPoSubjectForAddPackingAsync(AuthenticateDto authenticate, long poId);

        Task<ServiceResult<AddPackingResultDto>> AddPackAsync(AuthenticateDto authenticate, long poId, AddPackDto model);

        Task<ServiceResult<List<PackListDto>>> GetPackListAsync(AuthenticateDto authenticate, long poId);

        Task<ServiceResult<PackDetailsDto>> GetPackByIdAsync(AuthenticateDto authenticate, long poId, long packId);

        Task<ServiceResult<PackDetailsDto>> GetPackByIdForEditAsync(AuthenticateDto authenticate, long poId, long packId);

        Task<ServiceResult<PackingQualityControlInfodto>> GetQualityControlByPackIdAsync(AuthenticateDto authenticate, long poId, long PackId);

        Task<ServiceResult<PackQcResultDto>> AddQulityControlAsync(AuthenticateDto authenticate, long poId, long packId, AddQualityControlDto model);

        Task<ServiceResult<bool>> EditPackAsync(AuthenticateDto authenticate, long poId, long packId, List<AddPackSubjectDto> model);

        Task<ServiceResult<List<POSubjectInfoDto>>> DeletePackAsync(AuthenticateDto authenticate, long poId, long packId);

        Task<ServiceResult<bool>> DeletePackAttachmentAsync(AuthenticateDto authenticate, long poId, long packId, string fileSrc);

        Task<ServiceResult<PackingAttachmentsDto>> AddPackAttachmentAsync(AuthenticateDto authenticate, long poId, long packId, AddAttachmentDto file);

        Task<DownloadFileDto> DownloadPackAttachmentAsync(AuthenticateDto authenticate, long poId, long packId, string fileSrc);

        Task<DownloadFileDto> DownloadPackQualityControlAttachmentAsync(AuthenticateDto authenticate, long poId, long packId, string fileSrc);


    }
}
