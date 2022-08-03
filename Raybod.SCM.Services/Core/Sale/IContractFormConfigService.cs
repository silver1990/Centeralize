using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Contract;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Model;
using Raybod.SCM.Services.Core.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Core
{
    public interface IContractFormConfigService
    {
        Task<ServiceResult<bool>> IsValidCurrentFixedPart(AuthenticateDto authenticate, string contractCode, string fixedPart);

        Task<ServiceResult<List<ContractFormConfigDto>>> GetContractFormConfigListAsync(AuthenticateDto authenticate, string contractCode);

        Task<ServiceResult<ContractFormConfigDto>> EditFormConfigListAsync(AuthenticateDto authenticate, string contractCode, ContractFormConfigDto model);

        Task<ServiceResult<string>> GenerateFormCodeAsync(string contractCode, FormName formName, int count);
        Task<ServiceResult<ContractFormConfig>> GetFormCodeAsync(string contractCode, FormName formName);
        Task<ServiceResult<string>> GenerateFormCodeAsync(string contractCode, FormName formName, int count, string counter);

    }
}
