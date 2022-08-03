using Microsoft.EntityFrameworkCore;
using Raybod.SCM.DataAccess.Core;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Contract;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Model;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Implementation
{
    public class ContractFormConfigService : IContractFormConfigService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITeamWorkAuthenticationService _authenticationService;
        private readonly ISCMLogAndNotificationService _scmLogAndNotificationService;
        private readonly DbSet<Domain.Model.Contract> _contractRepository;
        private readonly DbSet<ContractFormConfig> _contractFormConfigRepository;
        private readonly DbSet<DocumentCommunication> _documentCommunicationRepository;
        private readonly DbSet<DocumentTQNCR> _documentTQNCRRepository;
        private readonly DbSet<DocumentRevision> _documentRevisionRepository;
        private readonly DbSet<PRContract> _prContractRepository;
        private readonly DbSet<PO> _poRepository;
        private readonly DbSet<Invoice> _invoiceRepository;
        public ContractFormConfigService(
            IUnitOfWork unitOfWork,
            ITeamWorkAuthenticationService authenticationService,
            ISCMLogAndNotificationService scmLogAndNotificationService)
        {
            _authenticationService = authenticationService;
            _unitOfWork = unitOfWork;
            _scmLogAndNotificationService = scmLogAndNotificationService;
            _contractRepository = _unitOfWork.Set<Domain.Model.Contract>();
            _contractFormConfigRepository = _unitOfWork.Set<ContractFormConfig>();
            _documentCommunicationRepository = _unitOfWork.Set<DocumentCommunication>();
            _documentTQNCRRepository = _unitOfWork.Set<DocumentTQNCR>();
            _prContractRepository = _unitOfWork.Set<PRContract>();
            _documentRevisionRepository = _unitOfWork.Set<DocumentRevision>();
            _poRepository = _unitOfWork.Set<PO>();
            _invoiceRepository = _unitOfWork.Set<Invoice>();
        }

        public async Task<ServiceResult<bool>> IsValidCurrentFixedPart(AuthenticateDto authenticate, string contractCode, string fixedPart)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var res = await _contractFormConfigRepository
                    .AsNoTracking()
                    .AnyAsync(x => x.FixedPart == fixedPart && x.ContractCode != contractCode);

                if (res)
                    return ServiceResultFactory.CreateError(false, Core.Common.Message.MessageId.DuplicateFormFixPart);
                else
                    return ServiceResultFactory.CreateSuccess(true);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<List<ContractFormConfigDto>>> GetContractFormConfigListAsync(AuthenticateDto authenticate, string contractCode)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(new List<ContractFormConfigDto>(), MessageId.AccessDenied);

                var res = await _contractFormConfigRepository
                    .Where(x => x.ContractCode == contractCode)
                    .Select(c => new ContractFormConfigDto
                    {
                        CodingType = c.CodingType,
                        ContractCode = c.ContractCode,
                        ContractFormConfigId = c.ContractFormConfigId,
                        FixedPart = c.FixedPart,
                        FormName = c.FormName,
                        LengthOfSequence = c.LengthOfSequence
                    }).ToListAsync();
                if (res.Any())
                    res = await CheckIsFormNameEditAbleAsync(contractCode, res);

                return ServiceResultFactory.CreateSuccess(res);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new List<ContractFormConfigDto>(), exception);
            }
        }

        public async Task<ServiceResult<ContractFormConfigDto>> EditFormConfigListAsync(AuthenticateDto authenticate, string contractCode, ContractFormConfigDto model)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(new ContractFormConfigDto(), MessageId.AccessDenied);

                var configModel = await _contractFormConfigRepository
                    .Where(x => x.ContractCode == contractCode &&
                    x.ContractFormConfigId == model.ContractFormConfigId && x.FormName == model.FormName)
                    .FirstOrDefaultAsync();

                if (configModel == null)
                    return ServiceResultFactory.CreateError(new ContractFormConfigDto(), MessageId.EntityDoesNotExist);

                var isPossibleEdit = await IsPossibleEditFormConfigAsync(contractCode, model.FormName);
                if (!isPossibleEdit)
                    return ServiceResultFactory.CreateError(new ContractFormConfigDto(), MessageId.ImpossibleEditFormConfig);

                if (model.FormName != FormName.DocumentRevision)
                {
                    if (await _contractFormConfigRepository.AnyAsync(a => a.ContractCode != contractCode && a.FormName == model.FormName && a.FixedPart == model.FixedPart))
                        return ServiceResultFactory.CreateError(new ContractFormConfigDto(), MessageId.DuplicateFormFixPart);
                }

                configModel.FixedPart = model.FixedPart;
                configModel.LengthOfSequence = model.LengthOfSequence;
                configModel.CodingType = model.CodingType;

                await _unitOfWork.SaveChangesAsync();

                var res = new ContractFormConfigDto
                {
                    CodingType = configModel.CodingType,
                    FixedPart = configModel.FixedPart,
                    ContractCode = configModel.ContractCode,
                    ContractFormConfigId = configModel.ContractFormConfigId,
                    FormName = configModel.FormName,
                    LengthOfSequence = configModel.LengthOfSequence,
                    IsEditable = model.IsEditable
                };

                return ServiceResultFactory.CreateSuccess(res);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new ContractFormConfigDto(), exception);
            }
        }

        private async Task<bool> IsPossibleEditFormConfigAsync(string contractCode, FormName formName)
        {
            try
            {
                switch (formName)
                {
                    case FormName.DocumentRevision:
                        return !await _documentRevisionRepository.AnyAsync(c => c.Document.ContractCode == contractCode && c.IsDeleted==false);
                    case FormName.Transmittal:
                        return !await _contractRepository.AnyAsync(c => c.ContractCode == contractCode && c.Transmittals.Any());
                    case FormName.CommunicationComment:
                        return !await _documentCommunicationRepository.AnyAsync(c => c.DocumentRevision.Document.ContractCode == contractCode);
                    case FormName.CommunicationTQ:
                        return !await _documentTQNCRRepository.AnyAsync(c => c.DocumentRevision.Document.ContractCode == contractCode && c.CommunicationType == CommunicationType.TQ);
                    case FormName.CommunicationNCR:
                        return !await _documentTQNCRRepository.AnyAsync(c => c.DocumentRevision.Document.ContractCode == contractCode && c.CommunicationType == CommunicationType.NCR);
                    case FormName.MRP:
                        return !await _contractRepository.AnyAsync(c => c.ContractCode == contractCode && c.Mrps.Any());
                    case FormName.PR:
                        return !await _contractRepository.AnyAsync(c => c.ContractCode == contractCode && c.PurchaseRequests.Any());
                    case FormName.RFP:
                        return !await _contractRepository.AnyAsync(c => c.ContractCode == contractCode && c.RFPs.Any());
                    case FormName.PRContract:
                        return !await _prContractRepository.AnyAsync(c => c.BaseContractCode == contractCode);
                    case FormName.PO:
                        return !await _poRepository.AnyAsync(c => c.BaseContractCode == contractCode);
                    case FormName.Pack:
                        return !await _poRepository.AnyAsync(c => c.BaseContractCode == contractCode && c.Packs.Any());
                    case FormName.Receipt:
                        return !await _poRepository.AnyAsync(c => c.BaseContractCode == contractCode && c.Receipts.Any());
                    case FormName.ReceiptReject:
                        return !await _poRepository.AnyAsync(c => c.BaseContractCode == contractCode && c.ReceiptRejects.Any());
                    case FormName.PendingToPayment:
                        return !await _contractRepository.AnyAsync(c => c.ContractCode == contractCode && c.PendingForPayments.Any());
                    case FormName.Invoice:
                        return !await _invoiceRepository.AnyAsync(c => c.PO.BaseContractCode == contractCode);
                    case FormName.Payment:
                        return !await _contractRepository.AnyAsync(c => c.ContractCode == contractCode && c.Payments.Any());
                    case FormName.WarehouseOutput:
                        return !await _contractRepository.AnyAsync(c => c.ContractCode == contractCode && c.WarehouseOutputRequests.Any());
                    case FormName.WarehouseDespatch:
                        return !await _contractRepository.AnyAsync(c => c.ContractCode == contractCode && c.WarehouseDespatches.Any());
                    default:
                        return false;
                }
            }
            catch (Exception exception)
            {
                return false;
            }
        }

        public async Task<List<ContractFormConfigDto>> CheckIsFormNameEditAbleAsync(string contractCode, List<ContractFormConfigDto> names)
        {
            foreach (var item in names)
            {
                switch (item.FormName)
                {
                    case FormName.DocumentRevision:
                        item.IsEditable = !await _documentRevisionRepository.AnyAsync(c => c.Document.ContractCode == contractCode);
                        break;
                    case FormName.Transmittal:
                        item.IsEditable = !await _contractRepository.AnyAsync(c => c.ContractCode == contractCode && c.Transmittals.Any());
                        break;
                    case FormName.CommunicationComment:
                        item.IsEditable = !await _documentCommunicationRepository.AnyAsync(c => c.DocumentRevision.Document.ContractCode == contractCode);
                        break;
                    case FormName.CommunicationTQ:
                        item.IsEditable = !await _documentTQNCRRepository.AnyAsync(c => c.DocumentRevision.Document.ContractCode == contractCode && c.CommunicationType == CommunicationType.TQ);
                        break;
                    case FormName.CommunicationNCR:
                        item.IsEditable = !await _documentTQNCRRepository.AnyAsync(c => c.DocumentRevision.Document.ContractCode == contractCode && c.CommunicationType == CommunicationType.NCR);
                        break;
                    case FormName.MRP:
                        item.IsEditable = !await _contractRepository.AnyAsync(c => c.ContractCode == contractCode && c.Mrps.Any());
                        break;
                    case FormName.PR:
                        item.IsEditable = !await _contractRepository.AnyAsync(c => c.ContractCode == contractCode && c.PurchaseRequests.Any());
                        break;
                    case FormName.RFP:
                        item.IsEditable = !await _contractRepository.AnyAsync(c => c.ContractCode == contractCode && c.RFPs.Any());
                        break;
                    case FormName.PRContract:
                        item.IsEditable = !await _prContractRepository.AnyAsync(c => c.BaseContractCode == contractCode);
                        break;
                    case FormName.PO:
                        item.IsEditable = !await _poRepository.AnyAsync(c => c.BaseContractCode == contractCode);
                        break;
                    case FormName.Pack:
                        item.IsEditable = !await _poRepository.AnyAsync(c => c.BaseContractCode == contractCode && c.Packs.Any());
                        break;
                    case FormName.Receipt:
                        item.IsEditable = !await _poRepository.AnyAsync(c => c.BaseContractCode == contractCode && c.Receipts.Any());
                        break;
                    case FormName.ReceiptReject:
                        item.IsEditable = !await _poRepository.AnyAsync(c => c.BaseContractCode == contractCode && c.ReceiptRejects.Any());
                        break;
                    case FormName.PendingToPayment:
                        item.IsEditable = !await _contractRepository.AnyAsync(c => c.ContractCode == contractCode && c.PendingForPayments.Any());
                        break;
                    case FormName.Invoice:
                        item.IsEditable = !await _invoiceRepository.AnyAsync(c => c.PO.BaseContractCode == contractCode);
                        break;
                    case FormName.Payment:
                        item.IsEditable = !await _contractRepository.AnyAsync(c => c.ContractCode == contractCode && c.Payments.Any());
                        break;
                    default:
                        item.IsEditable = false;
                        break;
                }
            }
            return names;
        }

        public async Task<ServiceResult<string>> GenerateFormCodeAsync(string contractCode, FormName formName, int count)
        {
            try
            {
                var formConfig = await _contractFormConfigRepository
                    .Where(a => a.ContractCode == contractCode && a.FormName == formName)
                    .FirstOrDefaultAsync();

                if (formConfig == null)
                    return ServiceResultFactory.CreateError("", MessageId.NotFoundFormConfig);

                if (formName == FormName.DocumentRevision)
                    return ServiceResultFactory.CreateSuccess(GenerateRevisionFormCode(formConfig, count));
                else
                    return ServiceResultFactory.CreateSuccess(GenerateFormCodeExceptRevision(formConfig, count));

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException("", exception);
            }
        }

        public async Task<ServiceResult<ContractFormConfig>> GetFormCodeAsync(string contractCode, FormName formName)
        {
            try
            {
                var formConfig = await _contractFormConfigRepository
                    .Where(a => a.ContractCode == contractCode && a.FormName == formName)
                    .FirstOrDefaultAsync();

                if (formConfig == null)
                    return ServiceResultFactory.CreateError<ContractFormConfig>(null, MessageId.NotFoundFormConfig);

               
                    return ServiceResultFactory.CreateSuccess(formConfig);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<ContractFormConfig>(null, exception);
            }
        }
        public async Task<ServiceResult<string>> GenerateFormCodeAsync(string contractCode, FormName formName, int count,string counter)
        {
            try
            {
                var formConfig = await _contractFormConfigRepository
                    .Where(a => a.ContractCode == contractCode && a.FormName == formName)
                    .FirstOrDefaultAsync();

                if (formConfig == null)
                    return ServiceResultFactory.CreateError("", MessageId.NotFoundFormConfig);

                if (formName == FormName.DocumentRevision)
                    return ServiceResultFactory.CreateSuccess(GenerateRevisionFormCode(formConfig, count));
                else
                    return ServiceResultFactory.CreateSuccess(GenerateFormCodeExceptRevision(formConfig, counter));

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException("", exception);
            }
        }
        private string GenerateRevisionFormCode(ContractFormConfig formConfig, int count)
        {
            if (formConfig.CodingType == CodingType.Number)
            {
                if (formConfig.LengthOfSequence == 1)
                {
                    return $"{formConfig.FixedPart}{count}";

                }
                else
                {
                    int length = (count + 1).ToString().Length;
                    string minimum = "";
                    for (int i = 1; i <= (formConfig.LengthOfSequence - length); i++)
                    {
                        minimum += "0";
                    }

                    return $"{formConfig.FixedPart}{minimum}{count}";
                }

            }
            else
            {
                var alphabet = new List<string>
                {
                    "A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P",
                    "Q","R","S","T","U","V","W","X",
                    "Y","Z"
                };
                return $"{formConfig.FixedPart}{alphabet[count]}";
            }
        }

        private string GenerateFormCodeExceptRevision(ContractFormConfig formConfig, int count)
        {
            if (formConfig.LengthOfSequence == 1)
                return $"{formConfig.FixedPart}{++count}";
            else
            {
                int length = (count + 1).ToString().Length;
                string minimum = "";
                for (int i = 1; i <= (formConfig.LengthOfSequence - length); i++)
                {
                    minimum += "0";
                }
                return $"{formConfig.FixedPart}{minimum}{++count}";
            }
        }
        private string GenerateFormCodeExceptRevision(ContractFormConfig formConfig, string counter)
        {
            var count = Convert.ToInt32(counter);
            if (formConfig.LengthOfSequence == 1)
                return $"{formConfig.FixedPart}{++count}";
            else
            {
                int length = (count + 1).ToString().Length;
                string minimum = "";
                for (int i = 1; i <= (formConfig.LengthOfSequence - length); i++)
                {
                    minimum += "0";
                }
                return $"{formConfig.FixedPart}{minimum}{++count}";
            }
        }
        private async Task<bool> generaqteCode()
        {
            var contracts = await _contractRepository
                .ToListAsync();

            foreach (var cnt in contracts)
            {

                cnt.ContractFormConfigs = new List<ContractFormConfig>();
                foreach (FormName form in Enum.GetValues(typeof(FormName)))
                {
                    var newFormConfig = new ContractFormConfig();

                    string fixedPart = "";

                    switch (form)
                    {
                        case FormName.Transmittal:
                            fixedPart = cnt.ContractCode + "-TR-";
                            break;
                        case FormName.CommunicationComment:
                            fixedPart = cnt.ContractCode + "-CO-";
                            break;
                        case FormName.CommunicationTQ:
                            fixedPart = cnt.ContractCode + "-TQ-";
                            break;
                        case FormName.CommunicationNCR:
                            fixedPart = cnt.ContractCode + "-NCR-";
                            break;
                        case FormName.MRP:
                            fixedPart = cnt.ContractCode + "-MRP-";
                            break;
                        case FormName.PR:
                            fixedPart = cnt.ContractCode + "-PR-";
                            break;
                        case FormName.RFP:
                            fixedPart = cnt.ContractCode + "-RFP-";
                            break;
                        case FormName.PRContract:
                            fixedPart = cnt.ContractCode + "-PRC-";
                            break;
                        case FormName.PO:
                            fixedPart = cnt.ContractCode + "-PO-";
                            break;
                        case FormName.Pack:
                            fixedPart = cnt.ContractCode + "-PA-";
                            break;
                        case FormName.Receipt:
                            fixedPart = cnt.ContractCode + "-RE-";
                            break;
                        case FormName.ReceiptReject:
                            fixedPart = cnt.ContractCode + "-RJ-";
                            break;
                        case FormName.PendingToPayment:
                            fixedPart = cnt.ContractCode + "-IN-";
                            break;
                        case FormName.Invoice:
                            fixedPart = cnt.ContractCode + "-BI-";
                            break;
                        case FormName.Payment:
                            fixedPart = cnt.ContractCode + "-PY-";
                            break;
                        default:
                            fixedPart = "";
                            break;
                    }

                    if (form == FormName.DocumentRevision)
                    {
                        newFormConfig.ContractCode = cnt.ContractCode;
                        newFormConfig.FormName = form;
                        newFormConfig.CodingType = CodingType.Number;
                        newFormConfig.FixedPart = "";
                        newFormConfig.LengthOfSequence = 2;

                        cnt.ContractFormConfigs.Add(newFormConfig);
                    }
                    else if (form == FormName.Transmittal)
                    {
                        newFormConfig.ContractCode = cnt.ContractCode;
                        newFormConfig.FormName = form;
                        newFormConfig.CodingType = CodingType.Number;
                        newFormConfig.FixedPart = cnt.ContractCode + "-TR-";
                        newFormConfig.LengthOfSequence = 4;
                        cnt.ContractFormConfigs.Add(newFormConfig);
                    }
                    else
                    {
                        newFormConfig.ContractCode = cnt.ContractCode;
                        newFormConfig.FormName = form;
                        newFormConfig.CodingType = CodingType.Number;
                        newFormConfig.FixedPart = fixedPart;
                        newFormConfig.LengthOfSequence = 1;

                        cnt.ContractFormConfigs.Add(newFormConfig);
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync();

            return true;

        }
    }
}
