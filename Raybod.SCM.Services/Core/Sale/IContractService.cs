using Raybod.SCM.DataTransferObject.ContractAttachment;
using Raybod.SCM.DataTransferObject.Contract;
using Raybod.SCM.Services.Core.Common;
using System.Collections.Generic;
using System.Threading.Tasks;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Address;
using Raybod.SCM.DataTransferObject.ContractSubjects;
using Microsoft.AspNetCore.Http;
using Raybod.SCM.DataTransferObject.Consultant;
using Raybod.SCM.DataTransferObject._PanelSale.Contract;
using Raybod.SCM.DataTransferObject.Authentication;
using Raybod.SCM.DataTransferObject.User;
using Raybod.SCM.DataTransferObject.Customer;

namespace Raybod.SCM.Services.Core
{
    public interface IContractService
    {
        Task<ServiceResult<UserInfoApiDto>> AddContractAsync(AuthenticateDto authenticate, InsertContractDto model);

        Task<ServiceResult<bool>> EditContractAsync(AuthenticateDto authenticate, string contractCode, EditContractDto model);

        Task<ServiceResult<List<ListContractDto>>> GetAllContractAsync(AuthenticateDto authenticate, ContractQuery query);

        Task<ServiceResult<List<ContractMiniInfoDto>>> GetAllContractMiniInfoAsync(AuthenticateDto authenticate, ContractQuery query);

        Task<ServiceResult<List<ContractMiniInfoDto>>> GetAllContractMiniInfoAsync(AuthenticateDto authenticate, string query);

        Task<ServiceResult<ListContractDto>> GetContractByIdAsync(AuthenticateDto authenticate, string contractCode);

        Task<ServiceResult<List<BaseContractAttachmentDto>>> GetContractAttachmentByIdAsync(AuthenticateDto authenticate);

        Task<DownloadFileDto> DownloadContractAttachmentAsync(AuthenticateDto authenticate, string fileSrc);

       

        

       

        Task<ServiceResult<bool>> RemoveContractAsync(AuthenticateDto authenticate, string contractCode);

        Task<ServiceResult<bool>> RemoveAttachmentAsync(AuthenticateDto authenticate, string contractCode, string fileSrc);

        Task<ServiceResult<List<BaseContractAttachmentDto>>> AddContractAttachmentAsync(AuthenticateDto authenticate, IFormFileCollection files);

        Task<ServiceResult<List<MiniSearchResult>>> SearchInContractAsync(AuthenticateDto authenticate, ContractQuery query);

        Task<ServiceResult<List<ContractMiniInfoDto>>> GetContractForCraeteNewTeamWorkAsync(AuthenticateDto authenticate, ContractQuery query);

        Task<ServiceResult<BaseConsultantDto>> UpdateProjectConsultantAsync(AuthenticateDto authenticate, int consultantId);
        Task<ServiceResult<BaseCustomerDto>> UpdateProjectCustomerAsync(AuthenticateDto authenticate, int customerId);
        Task<ServiceResult<ContractDescriptionDto>> GetProjectDescriptionAsync(AuthenticateDto authenticate);
        Task<ServiceResult<ContractDetailsDto>> GetProjectDetailAsync(AuthenticateDto authenticate);
        Task<ServiceResult<BaseCustomerDto>> GetContractCustomerAsync(AuthenticateDto authenticate, string contractCode);
        Task<ServiceResult<BaseConsultantDto>> GetContractConsultantAsync(AuthenticateDto authenticate, string contractCode);
        Task<ServiceResult<UserInfoApiDto>> UpdateProjectDescriptionAsync(AuthenticateDto authenticate, ContractDescriptionUpdateDto model);
        Task<ServiceResult<ContractDurationDto>> UpdateProjectTimeTableAsync(AuthenticateDto authenticate, ContractDurationDto model);
        Task<ServiceResult<bool>> UpdateProjectVisitedAsync(AuthenticateDto authenticate);
    }
}
