using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Contract;
using Raybod.SCM.DataTransferObject.Mrp;
using Raybod.SCM.DataTransferObject.MrpItem;
using Raybod.SCM.Domain.Model;
using Raybod.SCM.Services.Core.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Core
{
    public interface IMasterMrService
    {
        /// <summary>
        /// add or update all contract where  contractSubject contain this product
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>



        /// <summary>
        /// update all contract where  contractSubject contain this product
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>


        /// <summary>
        /// add all contract where  contractSubject contain this product
        /// </summary>
        /// <param name="contractCode"></param>
        /// <param name="contractSubjects"></param>
        /// <returns></returns>
      

        Task<int> DashbourdWaitingContractForMrpBadgeCountAsync(AuthenticateDto authenticate);

        Task<ServiceResult<int>> WaitingContractForMrpBadgeCountAsync(AuthenticateDto authenticate);

        /// <summary>
        /// get waiting masterMr List grouped by productGroupId
        /// </summary>
        /// <param name="authenticate"></param>
        /// <returns></returns>
        Task<ServiceResult<List<WaitingContractFotMrpDto>>> WaitingMasterMrListGroupedByProductGroupIdAsync(AuthenticateDto authenticate);

        /// <summary>
        /// get waiting contract details
        /// </summary>
        /// <param name="authenticate"></param>
        /// <param name="productGroupId"></param>
        /// <returns></returns>
        Task<ServiceResult<WaitingContractFotMrpDto>> WaitingContractForMrpByContractCodeAsync(AuthenticateDto authenticate, int productGroupId);

        /// <summary>
        /// get waiting masterMr 
        /// </summary>
        /// <param name="authenticate"></param>
        /// <param name="productGroupId"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<ServiceResult<List<MrpItemInfoDto>>> GetMasterMRBYProductGroupIdAsync(AuthenticateDto authenticate,
            int productGroupId, MasterMRQueryDto query);

        Task<ServiceResult<List<ExportMRPToExcelDto>>> GetMrpItemsByMrpIdForExportToExcelAsync(AuthenticateDto authenticate, int productGroupId, MasterMRQueryDto query);
    }
}
