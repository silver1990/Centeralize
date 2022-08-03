using Microsoft.AspNetCore.Http;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Document.Communication;
using Raybod.SCM.DataTransferObject.Document.Communication.TQ;
using Raybod.SCM.Services.Core.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Core
{
    public interface ICommunicationTQService
    {       

        Task<ServiceResult<bool>> AddCommunicationTQAsync(AuthenticateDto authenticate, long revisionId, AddTQDto model);

        Task<ServiceResult<List<TQListDto>>> GetTQListAsync(AuthenticateDto authenticate, TQQueryDto query);

        Task<ServiceResult<TQQuestionDetailsDto>> GetTQQuestionDetailsAsync(AuthenticateDto authenticate, long communicationId);

        Task<ServiceResult<TQDetailsDto>> GetTQDetailsAsync(AuthenticateDto authenticate, long communicationId);

        Task<ServiceResult<TQQuestionDetailsDto>> AddReplayTQQuestionAsync(AuthenticateDto authenticate, long communicationId, AddTQReplyDto model);

        Task<ServiceResult<List<CommunicationAttachmentDto>>> AddCommunicationTQAttachmentAsync(AuthenticateDto authenticate,
          long communicationId, IFormFileCollection files);

        Task<ServiceResult<bool>> DeleteCommunicationTQAttachmentAsync(AuthenticateDto authenticate, long communicationId, string fileSrc);

        Task<DownloadFileDto> DownloadTQFileAsync(AuthenticateDto authenticate, long communicationId, string fileSrc);

        Task<DownloadFileDto> GenerateTQAttachmentZipAsync(AuthenticateDto authenticate, long communicationId);
        Task<ServiceResult<List<TQListDto>>> GetTQListForCustomerAsync(AuthenticateDto authenticate, TQQueryDto query,bool accessability);
        Task<ServiceResult<bool>> AddCommunicationTQForCustomerAsync(AuthenticateDto authenticate, long revisionId, AddTQDto model, bool accessability);
        Task<ServiceResult<TQQuestionDetailsDto>> GetTQQuestionDetailsForCustomerAsync(AuthenticateDto authenticate, long communicationId, bool accessability);
        Task<DownloadFileDto> GenerateTQPdfAsync(AuthenticateDto authenticate, long communicationId);
    }
}
