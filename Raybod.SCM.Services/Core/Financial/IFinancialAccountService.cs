using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.FinancialAccount;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.View;
using Raybod.SCM.Services.Core.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Core
{
    public interface IFinancialAccountService
    {
        Task<ServiceResult<List<FinancialAccountBaseOnSupplier>>> GetFinancialAccountBaseONSupplierAsync();

        Task<DownloadFileDto> GetFinancialAccountBySupplierIdAsync(int supplierId, CurrencyType currencyType);

        //Task<ServiceResult<List<FinancialAccountOfSupplierDto>>> GetFinancialAccountByPoIdAsync(AuthenticateDto authenticate, long poId);

        Task<DownloadFileDto> ExportExcelFinancialAccountBaseONSupplierAsync();
        //Task<ServiceResult<bool>> AddFinancialAccountByPayment(long? poId, Payment paymentModel);
    }
}
