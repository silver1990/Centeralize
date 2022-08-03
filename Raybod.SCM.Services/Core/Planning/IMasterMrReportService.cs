using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.MasterMrpReport;
using Raybod.SCM.DataTransferObject.Mrp;
using Raybod.SCM.Services.Core.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Core
{
    public interface IMasterMrReportService
    {
        Task<ServiceResult<List<MasterMrProductListDto>>> GetMasterMrByContractCodeAsync(AuthenticateDto authenticate, MasterMRQueryDto query);

        Task<ServiceResult<MasterMrProductDetailsDto>> GetMasterMrByProductIdAsync(AuthenticateDto authenticate, int productId);
        Task<ServiceResult<MasterMrProductListDto>> GetMasterMrDetailByProductIdAsync(AuthenticateDto authenticate, int productId);
        Task<ServiceResult<List<MrpReportListDto>>> GetMrpReportListByProductIdAsync(AuthenticateDto authenticate, int productId, MasterMRQueryDto query);

        Task<ServiceResult<List<PRReportListDto>>> GetPRReportListByProductIdAsync(AuthenticateDto authenticate, int productId, MasterMRQueryDto query);

        Task<ServiceResult<List<RFPReportListDto>>> GetRFPReportListByProductIdAsync(AuthenticateDto authenticate, int productId, MasterMRQueryDto query);

        Task<ServiceResult<List<PRCReportListDto>>> GetPRContractReportListByProductIdAsync(AuthenticateDto authenticate, int productId, MasterMRQueryDto query);

        Task<ServiceResult<List<POReportListDto>>> GetPOReportListByProductIdAsync(AuthenticateDto authenticate, int productId, MasterMRQueryDto query);
    }
}
