using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.PO.POSupplierDocuemnt;
using Raybod.SCM.Services.Core.Common;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace Raybod.SCM.Services.Core
{
    public interface IPOSupplierDocumentService
    {
        Task<ServiceResult<List<POSupplierDocumentDto>>> GetPOSupplierDocumentAsync(AuthenticateDto authenticate, long poId);
        Task<ServiceResult<List<POSupplierDocumentProductListDto>>> GetPOSupplierDocumentProductListAsync(AuthenticateDto authenticate, long poId);
        Task<ServiceResult<POSupplierDocumentDto>> AddPOSupplierDocumentAsync(AuthenticateDto authenticate, long poId, AddPOSupplierDocumentDto model);
        Task<ServiceResult<POSupplierDocumentDto>> EditPOSupplierDocumentAsync(AuthenticateDto authenticate, long poId, long poSupplierDocumentId, EditPOSupplierDocumentDto model);
        Task<ServiceResult<bool>> DeletePOSupplierDocumentAsync(AuthenticateDto authenticate, long poId, long poSupplierDocumentId);
        Task<DownloadFileDto> DownloadPOSupplierDocumentAttachmentAsync(AuthenticateDto authenticate, long poId, long poSupplierDocumentId, string fileSrc);
    }
}
