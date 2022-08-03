using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Raybod.SCM.DataAccess.Core;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Document;
using Raybod.SCM.Domain.Model;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Utility.Extention;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Implementation
{
    public class ContractDocumentGroupService : IContractDocumentGroupService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly DbSet<Document> _documentRepository;
        private readonly DbSet<DocumentGroup> _documentGroupRepository;
        private readonly DbSet<Contract> _contractRepository;
        private readonly ITeamWorkAuthenticationService _authenticationServices;
        private readonly ISCMLogAndNotificationService _scmLogAndNotificationService;
        private readonly CompanyAppSettingsDto _appSettings;

        public ContractDocumentGroupService(
            IUnitOfWork unitOfWork,
            IOptions<CompanyAppSettingsDto> appSettings,
            ITeamWorkAuthenticationService authenticationServices,
            ISCMLogAndNotificationService scmLogAndNotificationService)
        {
            _unitOfWork = unitOfWork;
            _authenticationServices = authenticationServices;
            _scmLogAndNotificationService = scmLogAndNotificationService;
            _appSettings = appSettings.Value;
            _documentGroupRepository = _unitOfWork.Set<DocumentGroup>();
            _documentRepository = _unitOfWork.Set<Document>();
            _contractRepository = _unitOfWork.Set<Contract>();
        }

        public async Task<ServiceResult<ListDocumentGroupDto>> AddDocumentGroupAsync(AuthenticateDto authenticate, AddDocumentGroupDto model)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<ListDocumentGroupDto>(null, MessageId.AccessDenied);

                if (await _documentGroupRepository.AnyAsync(a => !a.IsDeleted && a.Title == model.Title || a.DocumentGroupCode == model.DocumentGroupCode))
                    return ServiceResultFactory.CreateError<ListDocumentGroupDto>(null, MessageId.DocumentGroupIsExistAllready);

                var groupModel = new DocumentGroup
                {
                    DocumentGroupCode = model.DocumentGroupCode,
                    Title = model.Title
                };

                _documentGroupRepository.Add(groupModel);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var result = new ListDocumentGroupDto
                    {
                        DocumentGroupId = groupModel.DocumentGroupId,
                        Title = groupModel.Title,
                        DocumentGroupCode = groupModel.DocumentGroupCode,
                    };
                    return ServiceResultFactory.CreateSuccess(result);
                }
                return ServiceResultFactory.CreateError<ListDocumentGroupDto>(null, MessageId.AddEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<ListDocumentGroupDto>(null, exception);
            }
        }

        public async Task<ServiceResult<bool>> EditDocumentGroupAsync(AuthenticateDto authenticate, int documentGroupId, AddDocumentGroupDto model)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                if (permission.DocumentGroupIds.Any() && !permission.DocumentGroupIds.Contains(documentGroupId))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                if (await _documentGroupRepository.AnyAsync(a => !a.IsDeleted && a.DocumentGroupId != documentGroupId && (a.Title == model.Title || a.DocumentGroupCode == model.DocumentGroupCode)))
                    return ServiceResultFactory.CreateError(false, MessageId.DocumentGroupIsExistAllready);
                
                var groupModel = await _documentGroupRepository.FirstOrDefaultAsync(a => !a.IsDeleted && a.DocumentGroupId == documentGroupId);

                if (groupModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                groupModel.DocumentGroupCode = model.DocumentGroupCode;
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
        
        public async Task<ServiceResult<bool>> DeleteDocumentGroupAsync(AuthenticateDto authenticate, int documentGroupId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                if (permission.DocumentGroupIds.Any() && !permission.DocumentGroupIds.Contains(documentGroupId))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                if (await _documentRepository.AnyAsync(a => !a.IsDeleted && a.DocumentGroupId == documentGroupId))
                    return ServiceResultFactory.CreateError(false, MessageId.DeleteDontAllowedBeforeSubset);

                var groupModel = await _documentGroupRepository.FirstOrDefaultAsync(a => !a.IsDeleted && a.DocumentGroupId == documentGroupId);

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

        public async Task<ServiceResult<List<ListDocumentGroupDto>>> GetDocumentGroupListAsync(AuthenticateDto authenticate)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<ListDocumentGroupDto>>(null, MessageId.AccessDenied);

                var dbQuery = _documentGroupRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted);

                if (permission.DocumentGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.DocumentGroupIds.Contains(a.DocumentGroupId));

                var result = await dbQuery.Select(c => new ListDocumentGroupDto
                {
                    DocumentGroupId = c.DocumentGroupId,
                    DocumentGroupCode = c.DocumentGroupCode,
                    Title = c.Title
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<ListDocumentGroupDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<ContractDocumentGroupListDto>> GetContractDocumentGroupListAsync(AuthenticateDto authenticate)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<ContractDocumentGroupListDto>(null, MessageId.AccessDenied);

                var contractDescription = await _contractRepository
                    .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode)
                    .Select(c => c.Description)
                    .FirstOrDefaultAsync();

                var result = new ContractDocumentGroupListDto();
                var dbQuery = _documentGroupRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted);

                if (permission.DocumentGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.DocumentGroupIds.Contains(a.DocumentGroupId));

                result.ContractCode = authenticate.ContractCode;
                result.ContractDescription = contractDescription;
                result.DocumentGroups = await dbQuery.Select(c => new DocumentGroupDto
                {
                    DocumentGroupId = c.DocumentGroupId,
                    DocumentGroupCode = c.DocumentGroupCode,
                    DocumentTitle = c.Title
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<ContractDocumentGroupListDto>(null, exception);
            }
        }
        public async Task<ServiceResult<List<ListDocumentGroupDto>>> GetDocumentGroupListForCustomerUserAsync(AuthenticateDto authenticate,bool accessability)
        {
            try
            {
                if (!accessability)
                    return ServiceResultFactory.CreateError<List<ListDocumentGroupDto>>(null, MessageId.AccessDenied);
                
               

                var dbQuery = _documentGroupRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted);

              

                var result = await dbQuery.Select(c => new ListDocumentGroupDto
                {
                    DocumentGroupId = c.DocumentGroupId,
                    DocumentGroupCode = c.DocumentGroupCode,
                    Title = c.Title
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<ListDocumentGroupDto>>(null, exception);
            }
        }
        public async Task<ServiceResult<List<ListDocumentGroupDto>>> GetDocumentGroupListWithoutLimitedBypermissionDocumentGroupAsync(AuthenticateDto authenticate)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<ListDocumentGroupDto>>(null, MessageId.AccessDenied);

                var dbQuery = _documentGroupRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted);

                var result = await dbQuery.Select(c => new ListDocumentGroupDto
                {
                    DocumentGroupId = c.DocumentGroupId,
                    DocumentGroupCode = c.DocumentGroupCode,
                    Title = c.Title
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<ListDocumentGroupDto>>(null, exception);
            }
        }

      

        

        

    }
}
