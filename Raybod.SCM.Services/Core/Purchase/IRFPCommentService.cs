using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.RFP;
using Raybod.SCM.DataTransferObject.RFP.RFPComment;
using Raybod.SCM.DataTransferObject.User;
using Raybod.SCM.Services.Core.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Core
{
    public interface IRFPCommentService
    {
        Task<ServiceResult<string>> AddRFPCommentAsync(AuthenticateDto authenticate,
            long rfpId, long rfpSupplierId, AddRFPCommentDto model);
        Task<ServiceResult<string>> AddRFPProFormaCommentAsync(AuthenticateDto authenticate, long rfpId, long rfpSupplierId, AddProFromaCommentDto model);
        Task<ServiceResult<List<RFPCommentListDto>>> GetRFPCommentAsync(AuthenticateDto authenticate,
            long rfpId, long rfpSupplierId, RFPCommentQueryDto query);
        Task<ServiceResult<List<BaseRFPProFormaCommentDto>>> GetRFPProFormaCommentAsync(AuthenticateDto authenticate, long rfpId, long rfpSupplierId, RFPCommentQueryDto query);
        Task<ServiceResult<List<UserMentionDto>>> GetUserMentionsAsync(AuthenticateDto authenticate, long rfpId);

       

        Task<DownloadFileDto> DownloadRFPCommentAttachmentAsync(AuthenticateDto authenticate, long rfpId, long rfpSupplierId, long commentId, string fileSrc);

    }
}
