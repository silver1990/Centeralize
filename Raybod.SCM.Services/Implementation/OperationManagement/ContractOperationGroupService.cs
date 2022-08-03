using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Raybod.SCM.DataAccess.Core;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject._PanelOperation.OperationGroup;
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
    public class ContractOperationGroupService : IContractOperationGroupService
    {
        private readonly IUnitOfWork _unitOfWork;
       
        private readonly DbSet<OperationGroup> _operationGroupRepository;
        private readonly DbSet<Contract> _contractRepository;
        private readonly ITeamWorkAuthenticationService _authenticationServices;
        private readonly ISCMLogAndNotificationService _scmLogAndNotificationService;
        private readonly CompanyAppSettingsDto _appSettings;

        public ContractOperationGroupService(
            IUnitOfWork unitOfWork,
            IOptions<CompanyAppSettingsDto> appSettings,
            ITeamWorkAuthenticationService authenticationServices,
            ISCMLogAndNotificationService scmLogAndNotificationService)
        {
            _unitOfWork = unitOfWork;
            _authenticationServices = authenticationServices;
            _scmLogAndNotificationService = scmLogAndNotificationService;
            _appSettings = appSettings.Value;
            _operationGroupRepository = _unitOfWork.Set<OperationGroup>();
            _contractRepository = _unitOfWork.Set<Contract>();
        }

        public async Task<ServiceResult<ListOperationGroupDto>> AddOperationGroupAsync(AuthenticateDto authenticate, AddOperationGroupDto model)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermission(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<ListOperationGroupDto>(null, MessageId.AccessDenied);

                if (await _operationGroupRepository.AnyAsync(a => !a.IsDeleted && (a.Title == model.Title || a.OperationGroupCode == model.OperationGroupCode)))
                    return ServiceResultFactory.CreateError<ListOperationGroupDto>(null, MessageId.DuplicateInformation);

                var groupModel = new OperationGroup
                {
                    OperationGroupCode = model.OperationGroupCode,
                    Title = model.Title
                };

                _operationGroupRepository.Add(groupModel);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var result = new ListOperationGroupDto
                    {
                        OperationGroupId = groupModel.OperationGroupId,
                        Title = groupModel.Title,
                        OperationGroupCode = groupModel.OperationGroupCode,
                    };
                    return ServiceResultFactory.CreateSuccess(result);
                }
                return ServiceResultFactory.CreateError<ListOperationGroupDto>(null, MessageId.AddEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<ListOperationGroupDto>(null, exception);
            }
        }

        public async Task<ServiceResult<bool>> DeleteOperationGroupAsync(AuthenticateDto authenticate, int operationGroupId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermission(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                if (permission.OperationGroupList.Any() && !permission.OperationGroupList.Contains(operationGroupId))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                if (await _operationGroupRepository.AnyAsync(a => !a.IsDeleted && a.OperationGroupId == operationGroupId&&a.Operations.Any(b=>!b.IsDeleted)))
                    return ServiceResultFactory.CreateError(false, MessageId.DeleteDontAllowedBeforeSubset);

                var groupModel = await _operationGroupRepository.FirstOrDefaultAsync(a => !a.IsDeleted && a.OperationGroupId == operationGroupId);

                if (groupModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);
                groupModel.IsDeleted = true;


                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    return ServiceResultFactory.CreateSuccess(true);
                }
                return ServiceResultFactory.CreateError(false, MessageId.AddEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<bool>> EditOperationGroupAsync(AuthenticateDto authenticate, int operationGroupId, AddOperationGroupDto model)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermission(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                if (permission.OperationGroupList.Any() && !permission.OperationGroupList.Contains(operationGroupId))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                if (await _operationGroupRepository.AnyAsync(a => !a.IsDeleted && a.OperationGroupId != operationGroupId && (a.Title == model.Title || a.OperationGroupCode == model.OperationGroupCode)))
                    return ServiceResultFactory.CreateError(false, MessageId.DuplicateInformation);

                var groupModel = await _operationGroupRepository.FirstOrDefaultAsync(a => !a.IsDeleted && a.OperationGroupId == operationGroupId);

                if (groupModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                groupModel.OperationGroupCode = model.OperationGroupCode;
                groupModel.Title = model.Title;

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    return ServiceResultFactory.CreateSuccess(true);
                }
                return ServiceResultFactory.CreateError(false, MessageId.AddEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<List<ListOperationGroupDto>>> GetOperationGroupListAsync(AuthenticateDto authenticate)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermission(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<ListOperationGroupDto>>(null, MessageId.AccessDenied);

                var dbQuery = _operationGroupRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted);

                if (permission.OperationGroupList.Any())
                    dbQuery = dbQuery.Where(a => permission.OperationGroupList.Contains(a.OperationGroupId));

                var result = await dbQuery.Select(c => new ListOperationGroupDto
                {
                    OperationGroupId = c.OperationGroupId,
                    OperationGroupCode = c.OperationGroupCode,
                    Title = c.Title
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<ListOperationGroupDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<List<ListOperationGroupDto>>> GetOperationGroupListWithoutLimitedBypermissionOperationGroupAsync(AuthenticateDto authenticate)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermission(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<ListOperationGroupDto>>(null, MessageId.AccessDenied);

                var dbQuery = _operationGroupRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted);

                var result = await dbQuery.Select(c => new ListOperationGroupDto
                {
                    OperationGroupId = c.OperationGroupId,
                    OperationGroupCode = c.OperationGroupCode,
                    Title = c.Title
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<ListOperationGroupDto>>(null, exception);
            }
        }


        public async Task<ServiceResult<List<OperationGroupDto>>> GetContractOperationGroupListAsync(AuthenticateDto authenticate)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermission(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<OperationGroupDto>>(null, MessageId.AccessDenied);

                var contractDescription = await _contractRepository
                    .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode)
                    .Select(c => c.Description)
                    .FirstOrDefaultAsync();

               
                var dbQuery = _operationGroupRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted);

                if (permission.OperationGroupList.Any())
                    dbQuery = dbQuery.Where(a => permission.OperationGroupList.Contains(a.OperationGroupId));


                var  result = await dbQuery.Select(c => new OperationGroupDto
                {
                    OperationGroupId = c.OperationGroupId,
                    OperationGroupCode = c.OperationGroupCode,
                    OperationTitle = c.Title
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<OperationGroupDto>>(null, exception);
            }
        }

    }
}
