using Microsoft.AspNetCore.Http;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Document.Communication;
using Raybod.SCM.DataTransferObject.Document.Communication.NCR;
using Raybod.SCM.Services.Core.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Core
{
    public interface ICommunicationNCRService
    {
        Task<ServiceResult<bool>> AddCommunicationNCRAsync(AuthenticateDto authenticate, long revisionId, AddNCRDto model);

        Task<ServiceResult<List<NCRListDto>>> GetNCRListAsync(AuthenticateDto authenticate, NCRQueryDto query);

        Task<ServiceResult<NCRQuestionDetailsDto>> GetNCRQuestionDetailsAsync(AuthenticateDto authenticate, long communicationId);

        Task<ServiceResult<NCRQuestionDetailsDto>> GetNCRQuestionDetailsForCustomerUserAsync(AuthenticateDto authenticate, long communicationId,bool accessability);

        Task<ServiceResult<NCRDetailsDto>> GetNCRDetailsAsync(AuthenticateDto authenticate, long communicationId);

        Task<ServiceResult<NCRQuestionDetailsDto>> AddReplayNCRQuestionAsync(AuthenticateDto authenticate, long communicationId, AddNCRReplyDto model);

        Task<ServiceResult<List<CommunicationAttachmentDto>>> AddCommunicationNCRAttachmentAsync(AuthenticateDto authenticate,
            long communicationId, IFormFileCollection files);

        Task<ServiceResult<bool>> DeleteCommunicationNCRAttachmentAsync(AuthenticateDto authenticate, long communicationId, string fileSrc);

        Task<DownloadFileDto> DownloadNCRFileAsync(AuthenticateDto authenticate, long communicationId, string fileSrc);

        Task<DownloadFileDto> GenerateNCRAttachmentZipAsync(AuthenticateDto authenticate, long communicationId);
        Task<ServiceResult<List<NCRListDto>>> GetNCRListForCustomerAsync(AuthenticateDto authenticate, NCRQueryDto query, bool accessibility);
        Task<ServiceResult<bool>> AddCommunicationNCRForCustomerUserAsync(AuthenticateDto authenticate, long revisionId, AddNCRDto model, bool accessability);
        Task<DownloadFileDto> GenerateNCRPdfAsync(AuthenticateDto authenticate, long communicationId);
    }
}
