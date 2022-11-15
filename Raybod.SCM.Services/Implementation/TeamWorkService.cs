using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Raybod.SCM.DataAccess.Core;
using Raybod.SCM.DataAccess.Extention;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject._admin;
using Raybod.SCM.DataTransferObject._PanelOperation.OperationGroup;
using Raybod.SCM.DataTransferObject.Authentication;
using Raybod.SCM.DataTransferObject.Customer;
using Raybod.SCM.DataTransferObject.Document;
using Raybod.SCM.DataTransferObject.ProductGroup.Group;
using Raybod.SCM.DataTransferObject.Role;
using Raybod.SCM.DataTransferObject.TeamWork;
using Raybod.SCM.DataTransferObject.User;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Model;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Implementation
{
    public class TeamWorkService : ITeamWorkService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITeamWorkAuthenticationService _authenticationService;
        private readonly DbSet<TeamWork> _teamWorkRepository;
        private readonly DbSet<TeamWorkUser> _teamWorkUserRepository;
        private readonly DbSet<TeamWorkUserProductGroup> _teamWorkUserProductGroupRepository;
        private readonly DbSet<TeamWorkUserDocumentGroup> _teamWorkUserDocumentGroupRepository;
        private readonly DbSet<TeamWorkUserOperationGroup> _teamWorkUserOperationGroupRepository;
        private readonly DbSet<TeamWorkUserRole> _teamWorkUserRoleRepository;
        private readonly DbSet<ProductGroup> _productGroupRepository;
        private readonly DbSet<DocumentGroup> _documentGroupRepository;
        private readonly DbSet<OperationGroup> _operationGroupRepository;
        private readonly DbSet<Customer> _customerRepository;
        private readonly DbSet<Consultant> _consultantRepository;
        private readonly DbSet<CompanyUser> _companyUserRepository;
        private readonly DbSet<Contract> _contractRepository;
        private readonly DbSet<Role> _roleRepository;
        private readonly DbSet<User> _userRepository;
        private readonly DbSet<UserNotify> _notifyRepository;
        private readonly DbSet<FileDriveShare> _shareRepository;
        private readonly DbSet<UserLatestTeamWork> _latestTeamworkRepository;
        private readonly CompanyConfig _appSettings;

        public TeamWorkService(IUnitOfWork unitOfWork,
            IOptions<CompanyAppSettingsDto> appSettings,
            IHttpContextAccessor httpContextAccessor,
            ITeamWorkAuthenticationService authenticationService)
        {
            _unitOfWork = unitOfWork;
            _authenticationService = authenticationService;
            _teamWorkRepository = _unitOfWork.Set<TeamWork>();
            _teamWorkUserRepository = _unitOfWork.Set<TeamWorkUser>();
            _teamWorkUserProductGroupRepository = _unitOfWork.Set<TeamWorkUserProductGroup>();
            _teamWorkUserDocumentGroupRepository = _unitOfWork.Set<TeamWorkUserDocumentGroup>();
            _teamWorkUserOperationGroupRepository = _unitOfWork.Set<TeamWorkUserOperationGroup>();
            _teamWorkUserRoleRepository = _unitOfWork.Set<TeamWorkUserRole>();
            _productGroupRepository = _unitOfWork.Set<ProductGroup>();
            _documentGroupRepository = _unitOfWork.Set<DocumentGroup>();
            _operationGroupRepository = _unitOfWork.Set<OperationGroup>();
            _contractRepository = _unitOfWork.Set<Contract>();
            _notifyRepository = _unitOfWork.Set<UserNotify>();
            _customerRepository = _unitOfWork.Set<Customer>();
            _companyUserRepository = _unitOfWork.Set<CompanyUser>();
            _latestTeamworkRepository = _unitOfWork.Set<UserLatestTeamWork>();
            _consultantRepository = _unitOfWork.Set<Consultant>();
            _roleRepository = _unitOfWork.Set<Role>();
            _userRepository = _unitOfWork.Set<User>();
            _shareRepository = _unitOfWork.Set<FileDriveShare>();
            httpContextAccessor.HttpContext.Request.Headers.TryGetValue("companyCode", out var CompanyCode);
            _appSettings = appSettings.Value.CompanyConfig.First(a => a.CompanyCode == CompanyCode);
        }

        #region teamWork service

        public async Task<ServiceResult<string>> AddTeamWorkAsync(AuthenticateDto authenticate, string contractCode)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError("", MessageId.AccessDenied);

                if (string.IsNullOrEmpty(contractCode))
                    return ServiceResultFactory.CreateError("", MessageId.InputDataValidationError);

                var contractModel = await _contractRepository
                    .FirstOrDefaultAsync(x => !x.IsDeleted && x.ContractCode == contractCode);

                if (contractModel == null)
                    return ServiceResultFactory.CreateError("", MessageId.EntityDoesNotExist);

                if (await _teamWorkRepository.AnyAsync(x => x.ContractCode == contractCode))
                    return ServiceResultFactory.CreateError("", MessageId.TeamWorkDuplicatContract);

                var teamWorkModel = new TeamWork
                {
                    ContractCode = contractModel.ContractCode,
                    Title = contractModel.Description,
                    IsDeleted = false,
                };

                _teamWorkRepository.Add(teamWorkModel);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    return ServiceResultFactory.CreateSuccess(teamWorkModel.Id.ToString());
                }

                return ServiceResultFactory.CreateError("", MessageId.InternalError);
            }
            catch (System.Exception exception)
            {
                return ServiceResultFactory.CreateException("", exception);
            }
        }

        //public async Task<ServiceResult<bool>> EditTeamWorkAsync(BaseTeamWorkDto model)
        //{
        //    try
        //    {
        //        var selectedTeamWork = await _teamWorkRepository.FindAsync(model.Id);
        //        if (selectedTeamWork == null)
        //            return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

        //        if (model.IsDefult)
        //        {
        //            model.ContractCode = null;
        //            if (await _teamWorkRepository.AnyAsync(x => !x.IsDeleted && x.ContractCode == null))
        //                return ServiceResultFactory.CreateError(false, MessageId.OrganizationTeamWorkError);
        //        }

        //        if (!model.IsDefult)
        //        {
        //            if (string.IsNullOrEmpty(model.ContractCode))
        //                return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

        //            if (selectedTeamWork.ContractCode != model.ContractCode && await _teamWorkRepository.AnyAsync(x =>
        //                    !x.IsDeleted && x.Id != model.Id && x.ContractCode == model.ContractCode))
        //                return ServiceResultFactory.CreateError(false, MessageId.TeamWorkDuplicatContract);
        //        }

        //        var mapperConfiguration = new MapperConfiguration(configuration =>
        //        {
        //            configuration.CreateMap<TeamWork, BaseTeamWorkDto>();
        //            configuration.CreateMap<BaseTeamWorkDto, TeamWork>();
        //        });

        //        var mapper = mapperConfiguration.CreateMapper();
        //        var teamWorkModel = mapper.Map<TeamWork>(model);
        //        var mergWith = selectedTeamWork.MergeWith(teamWorkModel);

        //        return await _unitOfWork.SaveChangesAsync() > 0
        //            ? ServiceResultFactory.CreateSuccess(true)
        //            : ServiceResultFactory.CreateError(false, MessageId.EditEntityFailed);
        //    }
        //    catch (System.Exception exception)
        //    {
        //        return ServiceResultFactory.CreateException(false, exception);
        //    }
        //}

        public async Task<ServiceResult<List<TeamWorkInfoDto>>> GetAllTeamWorkAsync(AuthenticateDto authenticate, TeamWorkQueryDto query)
        {

            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<TeamWorkInfoDto>>(null, MessageId.AccessDenied);

                var dbQuery = _teamWorkRepository.Where(a => !a.IsDeleted).AsQueryable();
                if (!string.IsNullOrEmpty(query.SearchText))
                {
                    dbQuery = dbQuery.Where(x =>
                        x.Title.Contains(query.SearchText) || (x.ContractCode != null && x.ContractCode.Contains(query.SearchText)));
                }

                var totalCount = dbQuery.Count();

                var columnsMap = new Dictionary<string, Expression<Func<TeamWork, object>>>
                {
                    ["ContractCode"] = v => v.ContractCode,
                    ["Title"] = v => v.Title,
                    ["DateCreat"] = v => v.DateCreat
                };

                dbQuery = dbQuery.ApplayOrdering(query, columnsMap).ApplayPageing(query);

                var result = await dbQuery.Select(a => new TeamWorkInfoDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    ContractCode = a.ContractCode,
                    IsOrganization = a.ContractCode == null ? true : false,
                    UserCount = a.TeamWorkUsers.Count(),
                    users = a.TeamWorkUsers.Select(v => new UserAuditLogDto
                    {
                        AdderUserId = v.UserId,
                        AdderUserName = v.User.FirstName + " " + v.User.LastName,
                        AdderUserImage = v.User.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + v.User.Image : "",
                    }).ToList()
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (System.Exception exception)
            {
                return ServiceResultFactory.CreateException(new List<TeamWorkInfoDto>(), exception);
            }
        }

        public async Task<ServiceResult<BaseTeamWorkDto>> GetTeamWorkByIdAsync(AuthenticateDto authenticate, int teamWorkId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<BaseTeamWorkDto>(null, MessageId.AccessDenied);

                var teamWorkIdModel = await _teamWorkRepository.FindAsync(teamWorkId);
                if (teamWorkIdModel == null)
                {
                    return ServiceResultFactory.CreateError(new BaseTeamWorkDto(), MessageId.EntityDoesNotExist);
                }

                var mapperConfiguration = new MapperConfiguration(configuration =>
                {
                    configuration.CreateMap<TeamWork, BaseTeamWorkDto>();
                    configuration.CreateMap<BaseTeamWorkDto, TeamWork>();
                });

                var mapper = mapperConfiguration.CreateMapper();
                var result = mapper.Map<BaseTeamWorkDto>(teamWorkIdModel);
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (System.Exception exception)
            {
                return ServiceResultFactory.CreateException(new BaseTeamWorkDto(), exception);
            }
        }

        public async Task<ServiceResult<bool>> RemoveTeamWorkAsync(AuthenticateDto authenticate, int teamWorkId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var teamWorkModel = await _teamWorkRepository.Include(c => c.TeamWorkUsers)
                    .FirstOrDefaultAsync(x => x.Id == teamWorkId);
                if (teamWorkModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (teamWorkModel.TeamWorkUsers.Any())
                    return ServiceResultFactory.CreateError(false, MessageId.DeleteDontAllowedBeforeSubset);

                teamWorkModel.IsDeleted = true;
                return await _unitOfWork.SaveChangesAsync() > 0
                    ? ServiceResultFactory.CreateSuccess(true)
                    : ServiceResultFactory.CreateError(false, MessageId.DeleteEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<List<TeamWorkUserPermissionsDto>>> AddUserToTeamWorkAsync(AuthenticateDto authenticate, int teamWorkId, List<int> userIds)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<TeamWorkUserPermissionsDto>>(null, MessageId.AccessDenied);

                if (userIds == null || userIds.Count() == 0)
                    return ServiceResultFactory.CreateError<List<TeamWorkUserPermissionsDto>>(null, MessageId.InputDataValidationError);


                var teamWorkModel = await _teamWorkRepository
                    .Where(a => a.Id == teamWorkId)
                    .Include(a => a.TeamWorkUsers)
                    .FirstOrDefaultAsync();

                if (teamWorkModel == null)
                    return ServiceResultFactory.CreateError<List<TeamWorkUserPermissionsDto>>(null, MessageId.EntityDoesNotExist);

                var newUsers = await _userRepository.Where(a => !a.IsDeleted && userIds.Contains(a.Id))
                    .Select(t => new TeamWorkUserPermissionsDto
                    {
                        UserId = t.Id,
                        UserImage = t.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + t.Image : "",
                        UserName = t.FirstName + " " + t.LastName,
                        IsActive = t.IsActive,
                        UserType=t.UserType,
                        UserEmail = t.Email,
                        UserFullName = t.UserName,
                        UserMobile = t.Mobile,
                        Company = (t.UserType == (int)UserStatus.CustomerUser) ? _companyUserRepository.Where(b => b.Email == t.Email).Select(d => new MiniCompanyInfoDto { CompanyId = d.Customer.Id, CompanyName = d.Customer.Name }).FirstOrDefault() : (t.UserType == (int)UserStatus.ConsultantUser) ? _companyUserRepository.Where(b => b.Email == t.Email).Select(d => new MiniCompanyInfoDto { CompanyId = d.Consultant.Id, CompanyName = d.Consultant.Name }).FirstOrDefault() : null,
                        UserProductGroups = new List<ProductGroupInfoDto>(),
                        DocumentGroups = new List<ListDocumentGroupDto>(),
                        UserRoles = new List<UserRoleDto>(),
                    }).ToListAsync();

                if (newUsers.Count() != userIds.Count())
                    return ServiceResultFactory.CreateError<List<TeamWorkUserPermissionsDto>>(null, MessageId.UserNotExist);

                if (teamWorkModel.TeamWorkUsers != null && teamWorkModel.TeamWorkUsers.Any(a => userIds.Contains(a.UserId)))
                    return ServiceResultFactory.CreateError<List<TeamWorkUserPermissionsDto>>(null, MessageId.Duplicate);
                List<TeamWorkUser> addModels = new List<TeamWorkUser>();
                if (String.IsNullOrEmpty(teamWorkModel.ContractCode))
                {
                    addModels = _userRepository.Where(u => u.UserType == (int)UserStatus.OrganizationUser && userIds.Contains(u.Id)).Select(userId => new TeamWorkUser
                    {
                        TeamWorkId = teamWorkId,
                        UserId = userId.Id,
                    }).ToList();
                }
                else
                {
                    
                    addModels = userIds.Select(userId => new TeamWorkUser
                    {
                        TeamWorkId = teamWorkId,
                        UserId = userId,

                    }).ToList();
                    var latestVisited = userIds.Select(userId => new UserLatestTeamWork
                    {
                        TeamWorkId = teamWorkId,
                        UserId = userId,
                        LastVisited = DateTime.Now

                    }).ToList();
                    if(newUsers.Any((a => a.UserType == (int)UserStatus.CustomerUser || a.UserType == (int)UserStatus.ConsultantUser)))
                    {
                        List<UserNotify> userNotifies = new List<UserNotify>();
                        foreach (var user in newUsers.Where(a => a.UserType == (int)UserStatus.CustomerUser || a.UserType == (int)UserStatus.ConsultantUser))
                        {
                            userNotifies.Add(new UserNotify { UserId=user.UserId,IsActive = true, IsOrganization = false, NotifyType = NotifyManagementType.Event, TeamWorkId = teamWorkId, NotifyNumber = (int)NotifEvent.AddTransmittal,SubModuleName= "Transmittal" });
                            userNotifies.Add(new UserNotify { UserId = user.UserId, IsActive = true, IsOrganization = false, NotifyType = NotifyManagementType.Event, TeamWorkId = teamWorkId, NotifyNumber = (int)NotifEvent.AddComComment,SubModuleName="Comment" });
                            userNotifies.Add(new UserNotify { UserId = user.UserId, IsActive = true, IsOrganization = false, NotifyType = NotifyManagementType.Event, TeamWorkId = teamWorkId, NotifyNumber = (int)NotifEvent.ReplyComComment,SubModuleName="Comment" });
                        }
                        _notifyRepository.AddRange(userNotifies);
                    }
                    
                    _latestTeamworkRepository.AddRange(latestVisited);

                }


                _teamWorkUserRepository.AddRange(addModels);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {

                    return ServiceResultFactory.CreateSuccess(newUsers);

                }
                return ServiceResultFactory.CreateError<List<TeamWorkUserPermissionsDto>>(null, MessageId.SaveFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<TeamWorkUserPermissionsDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<bool>> SetTeamWorkUserProductGroupAsync(AuthenticateDto authenticate, int teamWorkId, int userId, List<int> productGroupIds)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var teamWorkUserModel = await _teamWorkUserRepository
                    .Where(a => a.TeamWorkId == teamWorkId && a.UserId == userId)
                    .Include(c => c.TeamWork)
                    .Include(c => c.TeamWorkUserProductGroups)
                    .FirstOrDefaultAsync();

                if (teamWorkUserModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                var addNewProductGroup = new List<TeamWorkUserProductGroup>();
                var removeProductGroup = new List<TeamWorkUserProductGroup>();
                if (productGroupIds == null || productGroupIds.Count() == 0)
                    removeProductGroup = teamWorkUserModel.TeamWorkUserProductGroups.ToList();
                else
                {
                    if (await _productGroupRepository.CountAsync(a => !a.IsDeleted && productGroupIds.Contains(a.Id)) != productGroupIds.Count())
                        return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                    if (teamWorkUserModel.TeamWorkUserProductGroups != null && teamWorkUserModel.TeamWorkUserProductGroups.Any())
                    {
                        removeProductGroup = teamWorkUserModel.TeamWorkUserProductGroups
                        .Where(a => !productGroupIds.Contains(a.ProductGroupId))
                        .ToList();

                        var beforeProductGroupIds = teamWorkUserModel.TeamWorkUserProductGroups
                            .Select(a => a.ProductGroupId).ToList();

                        addNewProductGroup = productGroupIds.Where(productGroupId => !beforeProductGroupIds.Contains(productGroupId))
                            .Select(c => new TeamWorkUserProductGroup
                            {
                                ProductGroupId = c,
                                TeamWorkUserId = teamWorkUserModel.Id,
                                ContractCode = teamWorkUserModel.TeamWork.ContractCode,
                                UserId = userId
                            }).ToList();
                    }
                    else
                    {
                        addNewProductGroup = productGroupIds.Select(productGroupId => new TeamWorkUserProductGroup
                        {
                            ProductGroupId = productGroupId,
                            TeamWorkUserId = teamWorkUserModel.Id,
                            ContractCode = teamWorkUserModel.TeamWork.ContractCode,
                            UserId = userId
                        }).ToList();
                    }

                }

                foreach (var item in addNewProductGroup)
                {
                    _teamWorkUserProductGroupRepository.Add(item);
                }
                foreach (var item in removeProductGroup)
                {
                    _teamWorkUserProductGroupRepository.Remove(item);
                }

                await _unitOfWork.SaveChangesAsync();
                return ServiceResultFactory.CreateSuccess(true);


            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<bool>> SetTeamWorkUserDocumentGroupAsync(AuthenticateDto authenticate, int teamWorkId, int userId, List<int> documentGroupIds)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var teamWorkUserModel = await _teamWorkUserRepository
                    .Where(a => a.TeamWorkId == teamWorkId && a.UserId == userId)
                    .Include(c => c.TeamWork)
                    .Include(c => c.TeamWorkUserDocumentGroups)
                    .FirstOrDefaultAsync();

                if (teamWorkUserModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                var addNewProductGroup = new List<TeamWorkUserDocumentGroup>();
                var removeProductGroup = new List<TeamWorkUserDocumentGroup>();
                if (documentGroupIds == null || documentGroupIds.Count() == 0)
                {
                    removeProductGroup = teamWorkUserModel.TeamWorkUserDocumentGroups.ToList();
                }
                else
                {
                    if (await _documentGroupRepository.CountAsync(a => !a.IsDeleted && documentGroupIds.Contains(a.DocumentGroupId)) != documentGroupIds.Count())
                        return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                    if (teamWorkUserModel.TeamWorkUserDocumentGroups != null && teamWorkUserModel.TeamWorkUserDocumentGroups.Any())
                    {
                        removeProductGroup = teamWorkUserModel.TeamWorkUserDocumentGroups
                             .Where(a => !documentGroupIds.Contains(a.DocumentGroupId))
                             .ToList();

                        var beforeDocumentGroupIds = teamWorkUserModel.TeamWorkUserDocumentGroups
                            .Select(a => a.DocumentGroupId).ToList();

                        addNewProductGroup = documentGroupIds.Where(documentGroupId => !beforeDocumentGroupIds.Contains(documentGroupId))
                            .Select(c => new TeamWorkUserDocumentGroup
                            {
                                DocumentGroupId = c,
                                TeamWorkUserId = teamWorkUserModel.Id,
                                ContractCode = teamWorkUserModel.TeamWork.ContractCode,
                                UserId = userId
                            }).ToList();
                    }
                    else
                    {
                        addNewProductGroup = documentGroupIds.Select(documentGroupId => new TeamWorkUserDocumentGroup
                        {
                            DocumentGroupId = documentGroupId,
                            TeamWorkUserId = teamWorkUserModel.Id,
                            ContractCode = teamWorkUserModel.TeamWork.ContractCode,
                            UserId = userId
                        }).ToList();
                    }

                }

                foreach (var item in addNewProductGroup)
                {
                    _teamWorkUserDocumentGroupRepository.Add(item);
                }
                foreach (var item in removeProductGroup)
                {
                    _teamWorkUserDocumentGroupRepository.Remove(item);
                }

                await _unitOfWork.SaveChangesAsync();
                return ServiceResultFactory.CreateSuccess(true);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }


        public async Task<ServiceResult<bool>> SetTeamWorkUserOperationGroupAsync(AuthenticateDto authenticate, int teamWorkId, int userId, List<int> operationGroupIds)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var teamWorkUserModel = await _teamWorkUserRepository
                    .Where(a => a.TeamWorkId == teamWorkId && a.UserId == userId)
                    .Include(c => c.TeamWork)
                    .Include(c => c.TeamWorkUserOperationGroups)
                    .FirstOrDefaultAsync();

                if (teamWorkUserModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                var addNewOperationGroup = new List<TeamWorkUserOperationGroup>();
                var removeOperationGroup = new List<TeamWorkUserOperationGroup>();
                if (operationGroupIds == null || operationGroupIds.Count() == 0)
                {
                    removeOperationGroup = teamWorkUserModel.TeamWorkUserOperationGroups.ToList();
                }
                else
                {
                    if (await _operationGroupRepository.CountAsync(a => !a.IsDeleted && operationGroupIds.Contains(a.OperationGroupId)) != operationGroupIds.Count())
                        return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                    if (teamWorkUserModel.TeamWorkUserOperationGroups != null && teamWorkUserModel.TeamWorkUserOperationGroups.Any())
                    {
                        removeOperationGroup = teamWorkUserModel.TeamWorkUserOperationGroups
                             .Where(a => !operationGroupIds.Contains(a.OperationGroupId))
                             .ToList();

                        var beforeOperationGroupIds = teamWorkUserModel.TeamWorkUserOperationGroups
                            .Select(a => a.OperationGroupId).ToList();

                        addNewOperationGroup = operationGroupIds.Where(operationGroupId => !beforeOperationGroupIds.Contains(operationGroupId))
                            .Select(c => new TeamWorkUserOperationGroup
                            {
                                OperationGroupId = c,
                                TeamWorkUserId = teamWorkUserModel.Id,
                                ContractCode = teamWorkUserModel.TeamWork.ContractCode,
                                UserId = userId
                            }).ToList();
                    }
                    else
                    {
                        addNewOperationGroup = operationGroupIds.Select(operationGroupId => new TeamWorkUserOperationGroup
                        {
                            OperationGroupId = operationGroupId,
                            TeamWorkUserId = teamWorkUserModel.Id,
                            ContractCode = teamWorkUserModel.TeamWork.ContractCode,
                            UserId = userId
                        }).ToList();
                    }

                }

                foreach (var item in addNewOperationGroup)
                {
                    _teamWorkUserOperationGroupRepository.Add(item);
                }
                foreach (var item in removeOperationGroup)
                {
                    _teamWorkUserOperationGroupRepository.Remove(item);
                }

                await _unitOfWork.SaveChangesAsync();
                return ServiceResultFactory.CreateSuccess(true);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<bool>> SetUserTeamWorkRoleAsync(AuthenticateDto authenticate, int teamWorkId, int userId, List<BaseUserRoleDto> roles)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var teamWorkUserModel = await _teamWorkUserRepository
                      .Where(a => a.TeamWorkId == teamWorkId && a.UserId == userId)
                      .Include(c => c.TeamWork)
                      .Include(c => c.TeamWorkUserRoles)
                      .ThenInclude(c=>c.Role)
                      .FirstOrDefaultAsync();

                if (teamWorkUserModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                var addNewRoles = new List<TeamWorkUserRole>();
                var removeRoles = new List<TeamWorkUserRole>();
                var addNotify = new List<UserNotify>();
                var removeNotify = new List<UserNotify>();

                if (roles == null || roles.Count() == 0)
                {
                    if (teamWorkUserModel.TeamWorkUserRoles == null || !teamWorkUserModel.TeamWorkUserRoles.Any())
                        return ServiceResultFactory.CreateSuccess(true);
                    else
                    {
                        removeRoles.AddRange(teamWorkUserModel.TeamWorkUserRoles);
                        if (teamWorkUserModel.TeamWork.ContractCode == null)
                        {
                            var userNotifies = await _notifyRepository.Where(a => a.UserId == userId && a.IsOrganization).ToListAsync();
                            removeNotify.AddRange(userNotifies);
                        }
                        else
                        {
                            var userNotifies = await _notifyRepository.Where(a => a.UserId == userId && a.TeamWorkId == teamWorkId).ToListAsync();
                            removeNotify.AddRange(userNotifies);
                        }
                    }

                }
                else
                {

                    if (teamWorkUserModel.TeamWork.ContractCode != null)
                    {
                        var exceptionRole = new List<int> { 117 };

                        roles = roles.Where(a => !exceptionRole.Contains(a.RoleId)).ToList();
                    }

                    var postedRolesIds = roles.Select(a => a.RoleId).Distinct().ToList();
                    var roleModels = await _roleRepository
                        .Where(a => postedRolesIds.Contains(a.Id))
                        .ToListAsync();

                    if (roleModels == null || roleModels.Count() != postedRolesIds.Count())
                        return ServiceResultFactory.CreateError(false, MessageId.RoleNotFount);

                    if (teamWorkUserModel.TeamWorkUserRoles == null || !teamWorkUserModel.TeamWorkUserRoles.Any())
                    {
                        foreach (var role in roleModels)
                        {
                            var postedRole = roles.FirstOrDefault(a => a.RoleId == role.Id);
                            addNewRoles.Add(new TeamWorkUserRole
                            {
                                IsGlobalGroup = role.IsGlobalGroup,
                                ContractCode = teamWorkUserModel.TeamWork.ContractCode,
                                RoleDisplayName = role.DisplayName,
                                RoleId = role.Id,
                                RoleName = role.Name,
                                SCMEvents = role.SCMEvents,
                                SCMTasks = role.SCMTasks,
                                SCMEmails = role.SCMEmails,
                                SubModuleName = role.SubModuleName,
                                TeamWorkId = teamWorkId,
                                TeamWorkUserId = teamWorkUserModel.Id,
                                UserId = userId,
                                IsSendNotification = role.IsSendNotification,
                                IsSendMailNotification = role.IsSendMailNotification
                            });

                            await AddUserNotify(role, addNotify, userId, teamWorkId, teamWorkUserModel);

                        }
                    }
                    else
                    {
                        removeRoles = teamWorkUserModel.TeamWorkUserRoles
                                                   .Where(a => !postedRolesIds.Contains(a.RoleId))
                                                   .ToList();
                        foreach(var role in removeRoles)
                        {
                            await RemoveUserNotify(role, removeNotify, userId, teamWorkId, teamWorkUserModel);
                        }
                        var beforeRoleIds = teamWorkUserModel.TeamWorkUserRoles
                            .Select(a => a.RoleId).ToList();

                        addNewRoles = roleModels.Where(role => !beforeRoleIds.Contains(role.Id))
                            .Select(c => new TeamWorkUserRole
                            {
                                IsGlobalGroup = c.IsGlobalGroup,
                                ContractCode = teamWorkUserModel.TeamWork.ContractCode,
                                RoleDisplayName = c.DisplayName,
                                RoleId = c.Id,
                                RoleName = c.Name,
                                SCMEvents = c.SCMEvents,
                                SCMEmails = c.SCMEmails,
                                SCMTasks = c.SCMTasks,
                                SubModuleName = c.SubModuleName,
                                TeamWorkId = teamWorkId,
                                TeamWorkUserId = teamWorkUserModel.Id,
                                UserId = userId,
                                IsSendNotification = c.IsSendNotification,
                                IsSendMailNotification = c.IsSendMailNotification,
                            }).ToList();

                        foreach(var item in roleModels.Where(role => !beforeRoleIds.Contains(role.Id)))
                        {
                            await AddUserNotify(item, addNotify, userId, teamWorkId, teamWorkUserModel);
                        }
                    }
                }

                foreach (var item in removeRoles)
                {
                    _teamWorkUserRoleRepository.Remove(item);
                }
                foreach (var item in addNewRoles)
                {
                    _teamWorkUserRoleRepository.Add(item);
                    
                }
                foreach (var item in removeNotify)
                {
                    _notifyRepository.Remove(item);
                }
                foreach (var item in addNotify)
                {
                    _notifyRepository.Add(item);

                }
                await _unitOfWork.SaveChangesAsync();

                return ServiceResultFactory.CreateSuccess(true);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<bool>> DeleteUserFromTeamWorkAsync(AuthenticateDto authenticate, int teamWorkId, int userId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var teamWorkUserModel = await _teamWorkUserRepository.Where(a => a.UserId == userId && a.TeamWorkId == teamWorkId)
                    .Include(a => a.TeamWorkUserProductGroups)
                    .Include(a => a.TeamWorkUserRoles)
                    .Include(a=>a.TeamWork)
                    .FirstOrDefaultAsync();

                if (teamWorkUserModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (teamWorkUserModel.TeamWork.ContractCode == null)
                {
                    var userNotifies=await _notifyRepository.Where(a => a.UserId == userId && a.IsOrganization).ToListAsync();
                    if(userNotifies!=null&&userNotifies.Any())
                        _notifyRepository.RemoveRange(userNotifies);
                }
                else
                {
                    var userNotifies = await _notifyRepository.Where(a => a.UserId == userId && a.TeamWorkId==teamWorkId&&!a.IsOrganization).ToListAsync();
                    if (userNotifies != null && userNotifies.Any())
                        _notifyRepository.RemoveRange(userNotifies);
                }
                   

                if (teamWorkUserModel.TeamWorkUserRoles != null)
                    _teamWorkUserRoleRepository.RemoveRange(teamWorkUserModel.TeamWorkUserRoles);



                if (teamWorkUserModel.TeamWorkUserProductGroups != null)
                    _teamWorkUserProductGroupRepository.RemoveRange(teamWorkUserModel.TeamWorkUserProductGroups);

                _teamWorkUserRepository.RemoveRange(teamWorkUserModel);

                return await _unitOfWork.SaveChangesAsync() > 0
                    ? ServiceResultFactory.CreateSuccess(true)
                    : ServiceResultFactory.CreateError(false, MessageId.EditEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        //public async Task<ServiceResult<bool>> SetTeamWorkUserConfigAsync(AuthenticateDto authenticate, List<SetTeamWorkUserConfigDto> teamWorks)
        //{
        //    try
        //    {
        //        //var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
        //        //if (!permission.HasPermission)
        //        //    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

        //        if (teamWorks == null || !teamWorks.Any() || teamWorks.Any(c => c.TeamWorkId <= 0))
        //            return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

        //        var userModel = await _userRepository.Where(a => !a.IsDeleted && a.IsActive && a.Id == authenticate.UserId)
        //            .Include(c => c.UserLatestTeamWorks)
        //            .FirstOrDefaultAsync();

        //        if (userModel == null)
        //            return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

        //        var teamWorkIds = teamWorks.Where(a => a.IsInvisible).Select(a => a.TeamWorkId).ToList();
        //        if (!teamWorkIds.Any())
        //        {
        //            var removeItems = userModel.UserLatestTeamWorks.ToList();
        //            foreach (var item in removeItems)
        //            {
        //                userModel.u.Remove(item);
        //            }
        //            userModel.UserInvisibleTeamWorks = new List<UserInvisibleTeamWork>();
        //        }
        //        else
        //        {
        //            var removeItems = userModel.UserInvisibleTeamWorks.Where(a => !teamWorkIds.Contains(a.TeamWorkId)).ToList();

        //            foreach (var item in removeItems)
        //            {
        //                userModel.UserInvisibleTeamWorks.Remove(item);
        //            }

        //            var beforeItemId = userModel.UserInvisibleTeamWorks.Select(a => a.TeamWorkId).ToList();

        //            var addItems = teamWorkIds.Where(c => !beforeItemId.Contains(c)).ToList();
        //            foreach (var item in addItems)
        //            {
        //                userModel.UserInvisibleTeamWorks.Add(new UserInvisibleTeamWork
        //                {
        //                    TeamWorkId = item
        //                });
        //            }
        //        }

        //        //if (!userModel.Any(a => !a.IsInvisible))
        //        //    return ServiceResultFactory.CreateError(false, MessageId.ImpossibleInVisibleAllTeamWork);

        //        await _unitOfWork.SaveChangesAsync();

        //        return ServiceResultFactory.CreateSuccess(true);
        //    }
        //    catch (Exception exception)
        //    {
        //        return ServiceResultFactory.CreateException(false, exception);
        //    }
        //}

        public async Task<ServiceResult<List<BaseUserTeamWorkDto>>> GetUserTeamWorkForCustomerUserAsync(AuthenticateDto authenticate)
        {
            var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
            if (!permission.HasPermission)
                return ServiceResultFactory.CreateError<List<BaseUserTeamWorkDto>>(null, MessageId.AccessDenied);


            var result = await _userRepository.Where(a => !a.IsDeleted && a.IsActive && a.Id == authenticate.UserId)
                .Select(c => new UserInfoApiDto
                {
                    LatestTeamworkIds = c.UserLatestTeamWorks.Select(c => c.TeamWorkId).ToList(),
                    TeamWorks = c.TeamWorkUsers.Where(a => !a.TeamWork.IsDeleted)
                    .Select(v => new BaseUserTeamWorkDto
                    {

                        TeamWorkCode = v.TeamWork.ContractCode,
                        TeamWorkId = v.TeamWorkId,
                        Title = v.TeamWork.Title
                    }).ToList()
                }).FirstOrDefaultAsync();

            if (result == null)
                return ServiceResultFactory.CreateError<List<BaseUserTeamWorkDto>>(null, MessageId.EntityDoesNotExist);

            if (result.TeamWorks.Any())
            {
                var allTeamWorks = await _teamWorkRepository.Where(a => !a.IsDeleted && a.ContractCode != null)
                        .Select(c => new BaseUserTeamWorkDto
                        {

                            TeamWorkCode = c.ContractCode,
                            TeamWorkId = c.Id,
                            Title = c.Title
                        }).ToListAsync();
                var latestTeamwork = await _latestTeamworkRepository.Where(a => a.UserId == authenticate.UserId).OrderByDescending(a => a.LastVisited).ToListAsync();
                List<int> latestOrderTeamwork = new List<int>();
                if (latestTeamwork != null && latestTeamwork.Any())
                    latestOrderTeamwork = latestTeamwork.Take(4).Select(a => a.TeamWorkId).ToList();



                int topFour = 1;
                foreach (var item in allTeamWorks.Where(a => latestOrderTeamwork.Contains(a.TeamWorkId)))
                {
                    item.IsLatest = (latestOrderTeamwork != null && latestOrderTeamwork.Any() && latestOrderTeamwork.Contains(item.TeamWorkId) || topFour <= 4) ? true : false;
                    topFour++;
                    if (latestTeamwork != null && latestTeamwork.Any(a => a.TeamWorkId == item.TeamWorkId))
                        item.LastVisited = latestTeamwork.First(a => a.TeamWorkId == item.TeamWorkId).LastVisited;
                }
                foreach (var item in allTeamWorks.Where(a => !latestOrderTeamwork.Contains(a.TeamWorkId)))
                {
                    item.IsLatest = (latestOrderTeamwork != null && latestOrderTeamwork.Any() && latestOrderTeamwork.Contains(item.TeamWorkId) || topFour <= 4) ? true : false;
                    topFour++;
                    if (latestTeamwork != null && latestTeamwork.Any(a => a.TeamWorkId == item.TeamWorkId))
                        item.LastVisited = latestTeamwork.First(a => a.TeamWorkId == item.TeamWorkId).LastVisited;
                }
                result.TeamWorks = allTeamWorks.OrderByDescending(a => a.LastVisited).ToList();
            }

            return ServiceResultFactory.CreateSuccess(result.TeamWorks);
        }


        public async Task<ServiceResult<List<BaseUserTeamWorkDto>>> GetUserTeamWorkAsync(AuthenticateDto authenticate)
        {
            var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);



            var result = await _userRepository.Where(a => !a.IsDeleted && a.IsActive && a.Id == authenticate.UserId)
                .Select(c => new UserInfoApiDto
                {
                    LatestTeamworkIds = c.UserLatestTeamWorks.Select(c => c.TeamWorkId).ToList(),
                    TeamWorks = c.TeamWorkUsers.Where(a => !a.TeamWork.IsDeleted)
                    .Select(v => new BaseUserTeamWorkDto
                    {

                        TeamWorkCode = v.TeamWork.ContractCode,
                        TeamWorkId = v.TeamWorkId,
                        Title = v.TeamWork.Title
                    }).ToList()
                }).FirstOrDefaultAsync();

            if (result == null)
                return ServiceResultFactory.CreateError<List<BaseUserTeamWorkDto>>(null, MessageId.EntityDoesNotExist);

            if (result.TeamWorks.Any())
            {
                var latestTeamwork = await _latestTeamworkRepository.Where(a => a.UserId == authenticate.UserId).OrderByDescending(a => a.LastVisited).ToListAsync();
                List<int> latestOrderTeamwork = new List<int>();
                if (latestTeamwork != null && latestTeamwork.Any())
                    latestOrderTeamwork = latestTeamwork.Take(4).Select(a => a.TeamWorkId).ToList();



                int topFour = 1;
                var allTeamWorks = await _teamWorkRepository.Where(a => !a.IsDeleted && a.ContractCode != null)
                        .Select(c => new BaseUserTeamWorkDto
                        {

                            TeamWorkCode = c.ContractCode,
                            TeamWorkId = c.Id,
                            Title = c.Title
                        }).ToListAsync();

                foreach (var item in allTeamWorks.Where(a => latestOrderTeamwork.Contains(a.TeamWorkId)))
                {
                    item.IsLatest = (latestOrderTeamwork != null && latestOrderTeamwork.Any() && latestOrderTeamwork.Contains(item.TeamWorkId) || topFour <= 4) ? true : false;
                    topFour++;
                    if (latestTeamwork != null && latestTeamwork.Any(a => a.TeamWorkId == item.TeamWorkId))
                        item.LastVisited = latestTeamwork.First(a => a.TeamWorkId == item.TeamWorkId).LastVisited;
                }
                foreach (var item in allTeamWorks.Where(a => !latestOrderTeamwork.Contains(a.TeamWorkId)))
                {
                    item.IsLatest = (latestOrderTeamwork != null && latestOrderTeamwork.Any() && latestOrderTeamwork.Contains(item.TeamWorkId) || topFour <= 4) ? true : false;
                    topFour++;
                    if (latestTeamwork != null && latestTeamwork.Any(a => a.TeamWorkId == item.TeamWorkId))
                        item.LastVisited = latestTeamwork.First(a => a.TeamWorkId == item.TeamWorkId).LastVisited;
                }
                result.TeamWorks = allTeamWorks.OrderByDescending(a => a.LastVisited).ToList();
            }

            return ServiceResultFactory.CreateSuccess(result.TeamWorks);
        }


        public async Task<ServiceResult<List<TeamWorkForFileDriveShareDto>>> GetUserTeamWorkForFileShareAsync(AuthenticateDto authenticate, Guid entityId, EntityType entityType)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionForFileDriveTrash(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPrivatePermission)
                {
                    if (entityType == EntityType.Directory && !await _shareRepository.AnyAsync(a => !a.IsDeleted && a.DirectoryId == entityId && a.UserId == authenticate.UserId && a.Accessablity == Accessablity.Editor && a.Status == ShareEntityStatus.Active))
                        return ServiceResultFactory.CreateError<List<TeamWorkForFileDriveShareDto>>(null, MessageId.AccessDenied);
                    if (entityType == EntityType.File && !await _shareRepository.AnyAsync(a => !a.IsDeleted && a.FileId == entityId && a.UserId == authenticate.UserId && a.Accessablity == Accessablity.Editor && a.Status == ShareEntityStatus.Active))
                        return ServiceResultFactory.CreateError<List<TeamWorkForFileDriveShareDto>>(null, MessageId.AccessDenied);
                }


                var dbQuery = (entityType == EntityType.Directory) ? _teamWorkUserRepository.Include(a => a.User).Where(a => (a.TeamWork.ContractCode == authenticate.ContractCode || a.TeamWork.ContractCode == null) && !_shareRepository.Any(b => !b.IsDeleted && b.DirectoryId == entityId && b.UserId == a.User.Id)) :
                    _teamWorkUserRepository.Include(a => a.User).Where(a => (a.TeamWork.ContractCode == authenticate.ContractCode || a.TeamWork.ContractCode == null) && !_shareRepository.Any(b => !b.IsDeleted && b.FileId == entityId && b.UserId == a.User.Id));

                var users = await dbQuery.Where(a => a.User.Id != authenticate.UserId && a.User.UserType != (int)UserStatus.ConsultantUser && a.User.UserType != (int)UserStatus.CustomerUser).Select(a => new TeamWorkForFileDriveShareDto
                {
                    FullName = a.User.FullName,
                    Image = (!String.IsNullOrEmpty(a.User.Image)) ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + a.User.Image : null,
                    UserId = a.User.Id,
                    UserName = a.User.UserName
                }).ToListAsync();
                List<TeamWorkForFileDriveShareDto> result = new List<TeamWorkForFileDriveShareDto>();
                foreach (var item in users)
                {
                    if (!result.Any(a => a.UserId == item.UserId))
                        result.Add(item);
                }

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception ex)
            {
                return ServiceResultFactory.CreateException<List<TeamWorkForFileDriveShareDto>>(null, ex);
            }




        }
        #endregion

        #region TeamWorkRole services

        //public async Task<ServiceResult<List<ListRoleDto>>> GetRoleBySCMModuleAsync(AuthenticateDto authenticate, SCMModule module)
        //{
        //    try
        //    {
        //        var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
        //        if (!permission.HasPermission)
        //            return ServiceResultFactory.CreateError<List<ListRoleDto>>(null, MessageId.AccessDenied);

        //        var dbQuery = await _roleRepository
        //            .Where(x => x.SCMModule == module)
        //            .GroupBy(x => x.SubModuleName)
        //            .ToListAsync();
        //        if (dbQuery == null)
        //            return ServiceResultFactory.CreateSuccess(new List<ListRoleDto>());

        //        var result = dbQuery.Select(r => new ListRoleDto
        //        {
        //            SubModuleName = r.Key,
        //            Roles = r.Select(c => new RoleInfoDto
        //            {
        //                RoleId = c.Id,
        //                Description = c.Description,
        //                DisplayName = c.DisplayName,
        //                IsParalel = c.IsParallel,
        //                SCMWorkFlow = c.SCMWorkFlow,
        //                WorkFlowStateId = c.WorkFlowStateId
        //            }).ToList()
        //        }).ToList();

        //        return ServiceResultFactory.CreateSuccess(result);
        //    }
        //    catch (Exception exception)
        //    {
        //        return ServiceResultFactory.CreateException(new List<ListRoleDto>(), exception);
        //    }
        //}

        public async Task<ServiceResult<TeamWorkDetailsDto>> GetTeamWorkUserListByTeamWorkIdAsync(AuthenticateDto authenticate, int teamWorkId, TeamWorkQueryDto query)
        {
            try
            {
                //var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                //if (!permission.HasPermission)
                //    return ServiceResultFactory.CreateError<TeamWorkDetailsDto>(null, MessageId.AccessDenied);
                if (!_authenticationService.HasPermission(authenticate.UserId, authenticate.ContractCode))
                    return ServiceResultFactory.CreateError<TeamWorkDetailsDto>(null, MessageId.AccessDenied);
                var teamWorkModel = await _teamWorkRepository
                    .Where(a => a.Id == teamWorkId)
                    .Select(c => new BaseTeamWorkDto
                    {
                        Id = c.Id,
                        ContractCode = c.ContractCode,
                        Title = c.Title,
                        IsOrganization = c.ContractCode == null ? true : false
                    }).FirstOrDefaultAsync();

                if (teamWorkModel == null)
                    return ServiceResultFactory.CreateError<TeamWorkDetailsDto>(null, MessageId.EntityDoesNotExist);

                var dbQuery = _teamWorkUserRepository.Where(a => a.TeamWorkId == teamWorkId);

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a => a.User.UserName.Contains(query.SearchText)
                     || a.User.FirstName.Contains(query.SearchText)
                     || a.TeamWorkUserRoles.Any(c => c.SubModuleName.Contains(query.SearchText))
                     || a.TeamWorkUserProductGroups.Any(c => c.ProductGroup.Title.Contains(query.SearchText)));

                //dbQuery = dbQuery.ApplayPageing(query);

                var teamWorkUsers = await dbQuery
                    .Select(t => new TeamWorkUserPermissionsDto
                    {
                        UserId = t.UserId,
                        UserImage = t.User.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + t.User.Image : "",
                        UserName = t.User.FirstName + " " + t.User.LastName,
                        IsActive = t.User.IsActive,
                        UserEmail = t.User.Email,
                        UserFullName = t.User.UserName,
                        UserMobile = t.User.Mobile,
                        UserType = t.User.UserType,
                        Company = (t.User.UserType == (int)UserStatus.CustomerUser) ? _companyUserRepository.Where(b => b.Email == t.User.Email).Select(d => new MiniCompanyInfoDto { CompanyId = d.Customer.Id, CompanyName = d.Customer.Name }).FirstOrDefault() : (t.User.UserType == (int)UserStatus.ConsultantUser) ? _companyUserRepository.Where(b => b.Email == t.User.Email).Select(d => new MiniCompanyInfoDto { CompanyId = d.Consultant.Id, CompanyName = d.Consultant.Name }).FirstOrDefault() : null,
                        UserProductGroups = t.TeamWorkUserProductGroups.Select(p => new ProductGroupInfoDto
                        {
                            Id = p.ProductGroupId,
                            Title = p.ProductGroup.Title
                        }).ToList(),
                        DocumentGroups = t.TeamWorkUserDocumentGroups != null ? t.TeamWorkUserDocumentGroups.Select(p => new ListDocumentGroupDto
                        {
                            DocumentGroupId = p.DocumentGroupId,
                            Title = p.DocumentGroup.Title,
                            DocumentGroupCode = p.DocumentGroup.DocumentGroupCode
                        }).ToList() : new List<ListDocumentGroupDto>(),
                        OperationGroups = t.TeamWorkUserOperationGroups != null ? t.TeamWorkUserOperationGroups.Select(p => new ListOperationGroupDto
                        {
                            OperationGroupId = p.OperationGroupId,
                            Title = p.OperationGroup.Title,
                            OperationGroupCode = p.OperationGroup.OperationGroupCode
                        }).ToList() : new List<ListOperationGroupDto>(),
                        UserRoles = t.TeamWorkUserRoles.OrderBy(a => a.RoleId)
                    .Select(r => new UserRoleDto
                    {
                        RoleId = r.RoleId,
                        IsGlobalGroup = r.IsGlobalGroup,
                        RoleName = r.RoleName,
                        RoleDisplayName = r.RoleDisplayName,
                        SubModuleName = r.SubModuleName,
                        IsSendNotification = r.IsSendNotification,
                        IsSendMailNotification = r.IsSendMailNotification
                    }).ToList(),
                    }).ToListAsync();

                var result = new TeamWorkDetailsDto();
                result.TeamWork = teamWorkModel;
                result.Users = teamWorkUsers.Where(u => u.UserType == (int)UserStatus.OrganizationUser || u.UserType == (int)UserStatus.SupperUser).ToList();
                result.CustomerUsers = teamWorkUsers.Where(u => u.UserType == (int)UserStatus.CustomerUser || u.UserType == (int)UserStatus.ConsultantUser).ToList();
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new TeamWorkDetailsDto(), exception);
            }
        }

        private async Task<List<UserNotify>> AddUserNotify(Role role, List<UserNotify> addNotify, int userId, int teamWorkId, TeamWorkUser teamWorkUserModel)
        {
            if (!role.SCMTasks.StartsWith("0"))
            {
                if (teamWorkUserModel.TeamWork.ContractCode == null)
                {
                    var notifyNumber = role.SCMTasks.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    var teamworks = await _teamWorkRepository.Where(a => !a.IsDeleted && !a.Contract.IsDeleted).ToListAsync();
                    foreach (var teamwork in teamworks)
                    {
                        foreach (var number in notifyNumber)
                        {
                            if (!addNotify.Any(a => a.IsOrganization && a.NotifyNumber == Convert.ToInt32(number) && a.NotifyType == NotifyManagementType.Task && a.UserId == userId && a.TeamWorkId == teamwork.Id))
                                addNotify.Add(new UserNotify
                                {
                                    IsActive = true,
                                    IsOrganization = true,
                                    NotifyNumber = Convert.ToInt32(number),
                                    NotifyType = NotifyManagementType.Task,
                                    TeamWorkId = teamwork.Id,
                                    UserId = userId,
                                    SubModuleName=role.SubModuleName
                                });
                        }
                    }


                }
                else
                {
                    var notifyNumber = role.SCMTasks.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var number in notifyNumber)
                    {
                        if (!addNotify.Any(a => !a.IsOrganization && a.NotifyNumber == Convert.ToInt32(number) && a.NotifyType == NotifyManagementType.Task && a.UserId == userId && a.TeamWorkId == teamWorkId))
                            addNotify.Add(new UserNotify
                            {
                                IsActive = true,
                                IsOrganization = false,
                                NotifyNumber = Convert.ToInt32(number),
                                NotifyType = NotifyManagementType.Task,
                                TeamWorkId = teamWorkId,
                                UserId = userId,
                                SubModuleName=role.SubModuleName
                            });
                    }
                }
            }
            if (!role.SCMEvents.StartsWith("0"))
            {
                if (teamWorkUserModel.TeamWork.ContractCode == null)
                {
                    var notifyNumber = role.SCMEvents.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    var teamworks = await _teamWorkRepository.Where(a => !a.IsDeleted && !a.Contract.IsDeleted).ToListAsync();
                    foreach (var teamwork in teamworks)
                    {
                        foreach (var number in notifyNumber)
                        {
                            if (!addNotify.Any(a => a.IsOrganization && a.NotifyNumber == Convert.ToInt32(number) && a.NotifyType == NotifyManagementType.Event && a.UserId == userId && a.TeamWorkId == teamwork.Id))
                                addNotify.Add(new UserNotify
                                {
                                    IsActive = true,
                                    IsOrganization = true,
                                    NotifyNumber = Convert.ToInt32(number),
                                    NotifyType = NotifyManagementType.Event,
                                    TeamWorkId = teamwork.Id,
                                    UserId = userId,
                                    SubModuleName=role.SubModuleName
                                });
                        }
                    }


                }
                else
                {
                    var notifyNumber = role.SCMEvents.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var number in notifyNumber)
                    {
                        if (!addNotify.Any(a => !a.IsOrganization && a.NotifyNumber == Convert.ToInt32(number) && a.NotifyType == NotifyManagementType.Event && a.UserId == userId && a.TeamWorkId == teamWorkId))
                            addNotify.Add(new UserNotify
                            {
                                IsActive = true,
                                IsOrganization = false,
                                NotifyNumber = Convert.ToInt32(number),
                                NotifyType = NotifyManagementType.Event,
                                TeamWorkId = teamWorkId,
                                UserId = userId,
                                SubModuleName=role.SubModuleName
                            });
                    }
                }
            }
            if (!role.SCMEmails.StartsWith("0"))
            {
                if (teamWorkUserModel.TeamWork.ContractCode == null)
                {
                    var notifyNumber = role.SCMEmails.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    var teamworks = await _teamWorkRepository.Where(a => !a.IsDeleted && !a.Contract.IsDeleted).ToListAsync();
                    foreach (var teamwork in teamworks)
                    {
                        foreach (var number in notifyNumber)
                        {
                            if (!addNotify.Any(a => a.IsOrganization && a.NotifyNumber == Convert.ToInt32(number) && a.NotifyType == NotifyManagementType.Email && a.UserId == userId && a.TeamWorkId == teamwork.Id))
                                addNotify.Add(new UserNotify
                            {
                                IsActive = true,
                                IsOrganization = true,
                                NotifyNumber = Convert.ToInt32(number),
                                NotifyType = NotifyManagementType.Email,
                                TeamWorkId = teamwork.Id,
                                UserId = userId,
                                SubModuleName=role.SubModuleName
                            });
                        }
                    }


                }
                else
                {
                    var notifyNumber = role.SCMEmails.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var number in notifyNumber)
                    {
                        if (!addNotify.Any(a => !a.IsOrganization && a.NotifyNumber == Convert.ToInt32(number) && a.NotifyType == NotifyManagementType.Email && a.UserId == userId && a.TeamWorkId == teamWorkId))
                            addNotify.Add(new UserNotify
                            {
                                IsActive = true,
                                IsOrganization = false,
                                NotifyNumber = Convert.ToInt32(number),
                                NotifyType = NotifyManagementType.Email,
                                TeamWorkId = teamWorkId,
                                UserId = userId,
                                SubModuleName=role.SubModuleName
                            });
                    }
                }
            }
            return addNotify;
        }

        private async Task<List<UserNotify>> RemoveUserNotify(TeamWorkUserRole userRole,List<UserNotify> removeNotify, int userId, int teamWorkId, TeamWorkUser teamWorkUserModel)
        {
            if (!userRole.Role.SCMTasks.StartsWith("0"))
            {
                if (teamWorkUserModel.TeamWork.ContractCode == null)
                {
                    var notifyNumber = userRole.Role.SCMTasks.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    var notifyNumbers = notifyNumber.Select(a => Convert.ToInt32(a));
                    var userNotify = await _notifyRepository.Where(a => a.UserId == userId && a.IsOrganization && a.NotifyType == NotifyManagementType.Task && notifyNumbers.Contains(a.NotifyNumber)).ToListAsync();
                    removeNotify.AddRange(userNotify);


                }
                else
                {
                    var notifyNumber = userRole.Role.SCMTasks.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    var notifyNumbers = notifyNumber.Select(a => Convert.ToInt32(a));
                    var userNotify = await _notifyRepository.Where(a => a.UserId == userId && !a.IsOrganization &&a.TeamWorkId==teamWorkId&& a.NotifyType == NotifyManagementType.Task && notifyNumbers.Contains(a.NotifyNumber)).ToListAsync();
                    removeNotify.AddRange(userNotify);
                }
            }
            if (!userRole.Role.SCMEvents.StartsWith("0"))
            {
                if (teamWorkUserModel.TeamWork.ContractCode == null)
                {
                    var notifyNumber = userRole.Role.SCMEvents.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    var notifyNumbers = notifyNumber.Select(a => Convert.ToInt32(a));
                    var userNotify = await _notifyRepository.Where(a => a.UserId == userId && a.IsOrganization  && a.NotifyType == NotifyManagementType.Event && notifyNumbers.Contains(a.NotifyNumber)).ToListAsync();
                    removeNotify.AddRange(userNotify);

                }
                else
                {
                    var notifyNumber = userRole.Role.SCMEvents.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    var notifyNumbers = notifyNumber.Select(a => Convert.ToInt32(a));
                    var userNotify = await _notifyRepository.Where(a => a.UserId == userId && !a.IsOrganization && a.TeamWorkId == teamWorkId && a.NotifyType == NotifyManagementType.Event && notifyNumbers.Contains(a.NotifyNumber)).ToListAsync();
                    removeNotify.AddRange(userNotify);
                }
            }
            if (!userRole.Role.SCMEmails.StartsWith("0"))
            {
                if (teamWorkUserModel.TeamWork.ContractCode == null)
                {
                    var notifyNumber = userRole.Role.SCMEmails.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    var notifyNumbers = notifyNumber.Select(a => Convert.ToInt32(a));
                    var userNotify = await _notifyRepository.Where(a => a.UserId == userId && a.IsOrganization && a.NotifyType == NotifyManagementType.Email && notifyNumbers.Contains(a.NotifyNumber)).ToListAsync();
                    removeNotify.AddRange(userNotify);

                }
                else
                {
                    var notifyNumber = userRole.Role.SCMEmails.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    var notifyNumbers = notifyNumber.Select(a => Convert.ToInt32(a));
                    var userNotify = await _notifyRepository.Where(a => a.UserId == userId && !a.IsOrganization && a.TeamWorkId == teamWorkId && a.NotifyType == NotifyManagementType.Email && notifyNumbers.Contains(a.NotifyNumber)).ToListAsync();
                    removeNotify.AddRange(userNotify);
                }
            }
            return removeNotify;
        }
        #endregion
    }
}