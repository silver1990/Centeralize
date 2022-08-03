using Microsoft.AspNetCore.Http;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.PO;
using Raybod.SCM.Domain.Model;
using Raybod.SCM.Services.Core.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Core
{
    public interface IPOService
    {
        Task<POListBadgeDto> GetDashbourdPOListBadgeAsync(AuthenticateDto authenticate);

        Task<ServiceResult<POListBadgeDto>> GetPOListBadgeAsync(AuthenticateDto authenticate);

        Task<ServiceResult<List<PO>>> AddPOToPendingByPRContractAsync(PRContract prContract);
        Task<ServiceResult<List<PO>>> AddPOToConfirmedByPRContractAsync(PRContract prContract, List<RFPItems> rfpItems);
        ServiceResult<PO> AddPOToPendingByMRP(Mrp mrp, MrpItem mrpItem, DateTime dateDelivery, AddPOByMrpDto addPoModel, PRContract prContractModel);

        Task<ServiceResult<List<POPendingListDto>>> GetPOPendingAsync(AuthenticateDto authenticate, POQueryDto query);

        Task<ServiceResult<POPendingInfoDto>> GetPOPendingByPOIdAsync(AuthenticateDto authenticate, long poId);

        Task<ServiceResult<List<POPendingSubjectListDto>>> GetPOPendingSubjectByPRContractIdAsync(AuthenticateDto authenticate, long prContractId);

        Task<ServiceResult<List<InprogressPOListDto>>> GetInprogressPOAsync(AuthenticateDto authenticate, POQueryDto query);

        Task<ServiceResult<List<ListPODto>>> GetDeliverdPOAsync(AuthenticateDto authenticate, POQueryDto query);
        Task<ServiceResult<List<ListAllPODto>>> GetAllPOAsync(AuthenticateDto authenticate, POQueryDto query);

        Task<ServiceResult<bool>> AddPOAsync(AuthenticateDto authenticate, long prContractId, AddPODto model);

        Task<ServiceResult<PODetailsDto>> GetPODetailsByPOIdAsync(AuthenticateDto authenticate, long poId);

        Task<ServiceResult<List<POStatusLogDto>>> GetPoStatusLogsAsync(AuthenticateDto authenticate, long poId);

        Task<ServiceResult<List<POSubjectWithListPartDto>>> GetPOSubjectsByPOIdAsync(AuthenticateDto authenticate, long poId);

        Task<ServiceResult<List<BasePOAttachmentDto>>> AddPOAttachmentAsync(AuthenticateDto authenticate, long poId, IFormFileCollection files);

        Task<ServiceResult<bool>> RemovePOAttachmentByPoIdAsync(AuthenticateDto authenticate, long poId, string fileSrc);

        Task<ServiceResult<List<BasePOAttachmentDto>>> GetPoAttachmentByPOIdAsync(AuthenticateDto authenticate, long poId);

        Task<DownloadFileDto> DownloadPOAttachmentAsync(AuthenticateDto authenticate, long poId, string FileSrc);

        Task<ServiceResult<PODetailsForCancelDto>> CancelPoAsync(AuthenticateDto authenticate, long poId);
    }
}
