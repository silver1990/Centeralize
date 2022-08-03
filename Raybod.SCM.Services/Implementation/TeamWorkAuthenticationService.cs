using EFSecondLevelCache.Core;
using Microsoft.EntityFrameworkCore;
using Raybod.SCM.DataAccess.Core;
using Raybod.SCM.DataTransferObject.TeamWork.authentication;
using Raybod.SCM.Domain.Model;
using Raybod.SCM.Services.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using Raybod.SCM.Services.Core.Common;
using System.Threading.Tasks;
using Raybod.SCM.Utility.Helpers;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.User;
using Raybod.SCM.DataTransferObject.Authentication;
using Raybod.SCM.Domain.Struct;

namespace Raybod.SCM.Services.Implementation
{
    public class TeamWorkAuthenticationService : ITeamWorkAuthenticationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly DbSet<TeamWork> _teamWorkRepository;
        private readonly DbSet<TeamWorkUser> _teamWorkUserRepository;
        private readonly DbSet<UserLatestTeamWork> _userLatestTeamWorkRepository;
        private readonly DbSet<TeamWorkUserRole> _teamWorkRoleRepository;
        private readonly DbSet<Contract> _contractRepository;

        public TeamWorkAuthenticationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _teamWorkRoleRepository = _unitOfWork.Set<TeamWorkUserRole>();
            _teamWorkUserRepository = _unitOfWork.Set<TeamWorkUser>();
            _contractRepository = _unitOfWork.Set<Contract>();
            _teamWorkRepository = _unitOfWork.Set<TeamWork>();
            _userLatestTeamWorkRepository = _unitOfWork.Set<UserLatestTeamWork>();
        }


        public async Task<List<TeamWorkUserRole>> GetTeamWorkRolesByRolesAndContractCode(string contractCode, List<string> roles)
        {
            try
            {
                return await _teamWorkRoleRepository
                    .Where(a => (a.ContractCode == null || a.ContractCode == contractCode) && roles.Contains(a.RoleName))
                    .Cacheable()
                    .ToListAsync();
            }
            catch (Exception exception)
            {
                return null;
            }
        }

        public async Task<List<TeamWorkUserRole>> GetTeamWorkRolesByRolesAndContractCode(string contractCode, List<string> roles, int? documentGroupId, int? productGroupId)
        {
            try
            {
                var permission = await _teamWorkRoleRepository
                    .Where(a => (a.ContractCode == null || a.ContractCode == contractCode))
                    .Include(a => a.TeamWorkUser)
                    .ThenInclude(a => a.TeamWorkUserProductGroups)
                    .Include(a => a.TeamWorkUser)
                    .ThenInclude(a => a.TeamWorkUserDocumentGroups)
                    .Cacheable()
                    .ToListAsync();

                if (documentGroupId != null && documentGroupId > 0)
                    permission = permission
                        .Where(a =>
                        (!a.TeamWorkUser.TeamWorkUserDocumentGroups.Any() || a.TeamWorkUser.TeamWorkUserDocumentGroups.Any(v => v.DocumentGroupId == documentGroupId)) &&
                         a.TeamWorkUser.TeamWorkUserRoles.Any(c => roles.Contains(c.RoleName)))
                        .ToList();

                if (productGroupId != null && productGroupId > 0)
                    permission = permission
                        .Where(a =>
                        (!a.TeamWorkUser.TeamWorkUserProductGroups.Any() || a.TeamWorkUser.TeamWorkUserProductGroups.Any(v => v.ProductGroupId == productGroupId)) &&
                         a.TeamWorkUser.TeamWorkUserRoles.Any(c => roles.Contains(c.RoleName)))
                        .ToList();

                return permission;
            }
            catch (Exception exception)
            {
                return null;
            }
        }

        public async Task<List<TeamWorkUserRole>> GetTeamWorkRolesByRolesAndContractCode(string contractCode, List<string> roles, int? documentGroupId, int? productGroupId,int?operationGroupId)
        {
            try
            {
                var permission = await _teamWorkRoleRepository
                    .Where(a => (a.ContractCode == null || a.ContractCode == contractCode))
                    .Include(a => a.TeamWorkUser)
                    .ThenInclude(a => a.TeamWorkUserProductGroups)
                    .Include(a => a.TeamWorkUser)
                    .ThenInclude(a => a.TeamWorkUserDocumentGroups)
                    .Include(a => a.TeamWorkUser)
                    .ThenInclude(a => a.TeamWorkUserOperationGroups)
                    .Cacheable()
                    .ToListAsync();

                if (documentGroupId != null && documentGroupId > 0)
                    permission = permission
                        .Where(a =>
                        (!a.TeamWorkUser.TeamWorkUserDocumentGroups.Any() || a.TeamWorkUser.TeamWorkUserDocumentGroups.Any(v => v.DocumentGroupId == documentGroupId)) &&
                         a.TeamWorkUser.TeamWorkUserRoles.Any(c => roles.Contains(c.RoleName)))
                        .ToList();

                if (operationGroupId != null && operationGroupId > 0)
                    permission = permission
                        .Where(a =>
                        (!a.TeamWorkUser.TeamWorkUserOperationGroups.Any() || a.TeamWorkUser.TeamWorkUserOperationGroups.Any(v => v.OperationGroupId == operationGroupId)) &&
                         a.TeamWorkUser.TeamWorkUserRoles.Any(c => roles.Contains(c.RoleName)))
                        .ToList();
                if (productGroupId != null && productGroupId > 0)
                    permission = permission
                        .Where(a =>
                        (!a.TeamWorkUser.TeamWorkUserProductGroups.Any() || a.TeamWorkUser.TeamWorkUserProductGroups.Any(v => v.ProductGroupId == productGroupId)) &&
                         a.TeamWorkUser.TeamWorkUserRoles.Any(c => roles.Contains(c.RoleName)))
                        .ToList();

                return permission;
            }
            catch (Exception exception)
            {
                return null;
            }
        }
        public async Task<List<UserMentionDto>> GetAllUserHasAccessDocumentAsync(string contractCode, List<string> roles, int DocumentGroupId)
        {
            try
            {
                var res = await _teamWorkUserRepository
                    .Where(a => (!a.User.IsDeleted && a.User.IsActive) && (a.TeamWork.ContractCode == null || a.TeamWork.ContractCode == contractCode) &&
                    (!a.TeamWorkUserDocumentGroups.Any() ||
                    a.TeamWorkUserDocumentGroups.Any(v => v.DocumentGroupId == DocumentGroupId) ||
                    (a.TeamWorkUserRoles.Any(c => roles.Contains(c.RoleName) && c.IsGlobalGroup))) &&
                    a.TeamWorkUserRoles.Any(c => roles.Contains(c.RoleName)))
                    .Select(x => new UserMentionDto
                    {
                        Id = x.UserId,
                        Image = x.User.Image,
                        Display = x.User.FullName,
                        Email = x.User.Email,
                    }).ToListAsync();

                return res.GroupBy(a => a.Id).Select(x => new UserMentionDto
                {
                    Id = x.Key,
                    Image = x.First().Image,
                    Display = x.First().Display,
                    Email = x.First().Email
                }).ToList();
            }
            catch (Exception exception)
            {
                return new List<UserMentionDto>();
            }
        }
        public async Task<List<UserMentionDto>> GetAllUserHasAccessPurchaseAsync(string contractCode, List<string> roles, int productGroupId)
        {
            try
            {
                var res = await _teamWorkUserRepository
                    .Where(a => (!a.User.IsDeleted && a.User.IsActive) && (a.TeamWork.ContractCode == null || a.TeamWork.ContractCode == contractCode) &&
                    (!a.TeamWorkUserProductGroups.Any() ||
                    a.TeamWorkUserProductGroups.Any(v => v.ProductGroupId == productGroupId) ||
                    (a.TeamWorkUserRoles.Any(c => roles.Contains(c.RoleName) && c.IsGlobalGroup))) &&
                    a.TeamWorkUserRoles.Any(c => roles.Contains(c.RoleName)))
                    .Select(x => new UserMentionDto
                    {
                        Id = x.UserId,
                        Image = x.User.Image,
                        Display = x.User.FullName,
                        Email = x.User.Email,
                    }).ToListAsync();

                return res.GroupBy(a => a.Id).Select(x => new UserMentionDto
                {
                    Id = x.Key,
                    Image = x.First().Image,
                    Display = x.First().Display,
                    Email = x.First().Email
                }).ToList();
            }
            catch (Exception exception)
            {
                return new List<UserMentionDto>();
            }
        }

        public async Task<List<UserMentionDto>> GetAllUserHasAccessPurchaseAsync(string contractCode, List<string> roles)
        {
            try
            {
                var res = await _teamWorkUserRepository
                    .Where(a => (!a.User.IsDeleted && a.User.IsActive) && (a.TeamWork.ContractCode == null || a.TeamWork.ContractCode == contractCode) &&
                    (!a.TeamWorkUserProductGroups.Any() ||
                    
                    (a.TeamWorkUserRoles.Any(c => roles.Contains(c.RoleName) && c.IsGlobalGroup))) &&
                    a.TeamWorkUserRoles.Any(c => roles.Contains(c.RoleName)))
                    .Select(x => new UserMentionDto
                    {
                        Id = x.UserId,
                        Image = x.User.Image,
                        Display = x.User.FullName,
                        Email = x.User.Email,
                    }).ToListAsync();

                return res.GroupBy(a => a.Id).Select(x => new UserMentionDto
                {
                    Id = x.Key,
                    Image = x.First().Image,
                    Display = x.First().Display,
                    Email = x.First().Email
                }).ToList();
            }
            catch (Exception exception)
            {
                return new List<UserMentionDto>();
            }
        }
        public async Task<List<UserMentionDto>> GetAllUserHasAccessDocumentAsync(string contractCode)
        {
            try
            {
                var res = await _teamWorkUserRepository
                    .Where(a => (!a.User.IsDeleted && a.User.IsActive) && (a.TeamWork.ContractCode == null || a.TeamWork.ContractCode == contractCode))
                    .Select(x => new UserMentionDto
                    {
                        Id = x.UserId,
                        Image = x.User.Image,
                        Display = x.User.FullName,
                        Email = x.User.Email,
                    }).ToListAsync();

                return res.GroupBy(a => a.Id).Select(x => new UserMentionDto
                {
                    Id = x.Key,
                    Image = x.First().Image,
                    Display = x.First().Display,
                    Email = x.First().Email
                }).ToList();
            }
            catch (Exception exception)
            {
                return new List<UserMentionDto>();
            }
        }

        public async Task<List<UserMentionDto>> GetAllUserHasAccessOperationActivityAsync(string contractCode,long operationGroupId)
        {
            try
            {
                var res = await _teamWorkUserRepository
                    .Where(a => (!a.User.IsDeleted && a.User.IsActive) && (a.TeamWork.ContractCode == null || a.TeamWork.ContractCode == contractCode)&&a.TeamWorkUserRoles.Any(b=>b.RoleName==SCMRole.OperationActivityUpd||b.RoleName==SCMRole.OperationInProgressMng)&&(!a.TeamWorkUserOperationGroups.Any()||a.TeamWorkUserOperationGroups.Any(b=>b.OperationGroupId==operationGroupId)))
                    .Select(x => new UserMentionDto
                    {
                        Id = x.UserId,
                        Image = x.User.Image,
                        Display = x.User.FullName,
                        Email = x.User.Email,
                    }).ToListAsync();

                return res.GroupBy(a => a.Id).Select(x => new UserMentionDto
                {
                    Id = x.Key,
                    Image = x.First().Image,
                    Display = x.First().Display,
                    Email = x.First().Email
                }).ToList();
            }
            catch (Exception exception)
            {
                return new List<UserMentionDto>();
            }
        }
        public async Task<List<UserMentionDto>> GetAllUserHasAccessDocumentForCustomerUserAsync(string contractCode, int DocumentGroupId)
        {
            try
            {
                var res = await _teamWorkUserRepository
                    .Where(a => (a.TeamWork.ContractCode == null || a.TeamWork.ContractCode == contractCode) &&
                    (!a.TeamWorkUserDocumentGroups.Any() ||
                    a.TeamWorkUserDocumentGroups.Any(v => v.DocumentGroupId == DocumentGroupId) || (!a.User.IsDeleted && a.User.UserType == (int)UserStatus.CustomerUser)))
                    .Select(x => new UserMentionDto
                    {
                        Id = x.UserId,
                        Image = x.User.Image,
                        Display = x.User.FullName,
                        Email = x.User.Email,
                    }).ToListAsync();

                return res.GroupBy(a => a.Id).Select(x => new UserMentionDto
                {
                    Id = x.Key,
                    Image = x.First().Image,
                    Display = x.First().Display,
                    Email = x.First().Email
                }).ToList();
            }
            catch (Exception exception)
            {
                return new List<UserMentionDto>();
            }
        }
        public async Task<List<UserMentionDto>> GetAllUserHasAccessPOAsync(string contractCode, List<string> roles, int productGroupId)
        {
            try
            {
                var res = await _teamWorkUserRepository
                    .Where(a => (a.TeamWork.ContractCode == null || a.TeamWork.ContractCode == contractCode) &&
                    (!a.TeamWorkUserProductGroups.Any() ||
                    a.TeamWorkUserProductGroups.Any(v => v.ProductGroupId == productGroupId) ||
                    (a.TeamWorkUserRoles.Any(c => roles.Contains(c.RoleName) && c.IsGlobalGroup))) &&
                    a.TeamWorkUserRoles.Any(c => roles.Contains(c.RoleName)) && a.User.IsActive && !a.User.IsDeleted)

                    .Select(x => new UserMentionDto
                    {
                        Id = x.UserId,
                        Image = x.User.Image,
                        Display = x.User.FullName,
                        UserType = x.User.UserType
                    }).ToListAsync();

                res = res.GroupBy(a => a.Id).Select(x => new UserMentionDto
                {
                    Id = x.Key,
                    Image = x.First().Image,
                    Display = x.First().Display,
                    UserType = x.First().UserType
                }).ToList();

                return res;
            }
            catch (Exception exception)
            {
                return new List<UserMentionDto>();
            }
        }
        public async Task<List<UserMentionDto>> GetAllUserHasAccessToFileAsync(string contractCode, List<string> roles)
        {
            try
            {
                var res = await _teamWorkUserRepository
                    .Where(a => (a.TeamWork.ContractCode == null || a.TeamWork.ContractCode == contractCode) &&
                    (!a.TeamWorkUserProductGroups.Any() ||
                    (a.TeamWorkUserRoles.Any(c => roles.Contains(c.RoleName) && c.IsGlobalGroup))) &&
                    a.TeamWorkUserRoles.Any(c => roles.Contains(c.RoleName)) && a.User.IsActive && !a.User.IsDeleted)
                    .Select(x => new UserMentionDto
                    {
                        Id = x.UserId,
                        Image = x.User.Image,
                        Display = x.User.FullName,
                        UserType = x.User.UserType
                    }).ToListAsync();

                res = res.GroupBy(a => a.Id).Select(x => new UserMentionDto
                {
                    Id = x.Key,
                    Image = x.First().Image,
                    Display = x.First().Display,
                    UserType = x.First().UserType
                }).ToList();

                return res;
            }
            catch (Exception exception)
            {
                return new List<UserMentionDto>();
            }
        }
        public async Task<List<TeamWorkUser>> GetTeamWorkRolesByRolesAndContractCode(string contractCode, List<int> documentGroupIds, List<string> roles)
        {
            try
            {
                return await _teamWorkUserRepository
                    .Where(a => (a.TeamWork.ContractCode == null || a.TeamWork.ContractCode == contractCode) &&
                    (!a.TeamWorkUserDocumentGroups.Any() || a.TeamWorkUserDocumentGroups.Any(v => documentGroupIds.Contains(v.DocumentGroupId))) &&
                    a.TeamWorkUserRoles.Any(c => roles.Contains(c.RoleName)))
                    .Include(a => a.TeamWorkUserDocumentGroups)
                    .Cacheable()
                    .ToListAsync();
            }
            catch (Exception exception)
            {
                return null;
            }
        }

        public async Task<List<TeamWorkUserRole>> GetTeamWorkRolesByRolesAndContractCode(List<string> contractCodes, List<string> roles)
        {
            try
            {
                return await _teamWorkRoleRepository
                    .Where(a => (a.ContractCode == null || contractCodes.Contains(a.ContractCode)) && roles.Contains(a.RoleName))
                    .Cacheable()
                    .ToListAsync();
            }
            catch (Exception exception)
            {
                return null;
            }
        }
        public async Task<List<UserInfoForAuditLogDto>> GetSCMEventLogReceiverUserByNotifEventAndContractCode(string contractCodes, NotifEvent notifEvent)
        {
            try
            {
                var userIds= await _teamWorkRoleRepository
                    .Where(a => (a.ContractCode == null || contractCodes.Contains(a.ContractCode)) && a.SCMEvents.Contains(","+((int)notifEvent).ToString()+","))
                    .Select(a=>new UserInfoForAuditLogDto
                    {
                        UserId=a.UserId,
                        DocumentGroupIds=a.TeamWorkUser.TeamWorkUserDocumentGroups.Select(a=>a.DocumentGroupId).ToList(),
                        OperationGroupIds=a.TeamWorkUser.TeamWorkUserOperationGroups.Select(a=>a.OperationGroupId).ToList(),
                        ProductGroupIds=a.TeamWorkUser.TeamWorkUserProductGroups.Select(a=>a.ProductGroupId).ToList()
                    }).ToListAsync();
                return userIds;
            }
            catch (Exception exception)
            {
                return null;
            }
        }
        public async Task<UserLogPermissonDto> GetUserLogPermissionEntitiesAsync(int userId, string contractCode)
        {
            try
            {
                var permission = new UserLogPermissonDto();

                var dbQuery = await _teamWorkUserRepository.Where(a => a.UserId == userId)
                    .Include(a => a.TeamWorkUserRoles)
                    .Include(a => a.TeamWorkUserProductGroups)
                    .Include(a => a.TeamWorkUserDocumentGroups)
                    .Cacheable()
                    .ToListAsync();

                if (dbQuery == null || !dbQuery.Any())
                    return permission;


                var userTeamWorks = dbQuery
                    .Where(a => a.TeamWorkUserRoles.Any(c => (c.ContractCode == null || c.ContractCode == contractCode)))
                    .ToList();

                // age roli tarif shode nadasht befrest bere nadare
                if (userTeamWorks == null || !userTeamWorks.Any())
                    return permission;

                permission.ProductGroupIds = userTeamWorks
                    .Where(a => a.TeamWorkUserProductGroups != null)
                    .SelectMany(a => a.TeamWorkUserProductGroups.Select(p => p.ProductGroupId))
                    .Distinct()
                    .ToList();

                permission.DocumentGroupIds = userTeamWorks
                    .Where(a => a.TeamWorkUserDocumentGroups != null)
                    .SelectMany(a => a.TeamWorkUserDocumentGroups.Select(p => p.DocumentGroupId))
                    .Distinct()
                    .ToList();

                permission.GlobalPermission = userTeamWorks
                    .SelectMany(a => a.TeamWorkUserRoles)
                    .SelectMany(c => c.SCMEvents.SplitHelper())
                    .Distinct()
                    .Select(v => (NotifEvent)Enum.Parse(typeof(NotifEvent), v))
                    .ToList();

                return permission;
            }
            catch (Exception exception)
            {
                return null;
            }
        }

        public bool IsUserHaveAnyOfThisRoles(int userId, List<string> roles)
        {
            try
            {
                return _teamWorkRoleRepository.Where(a =>
                    a.UserId == userId)
                    .Cacheable()
                    .Any(a => roles.Any(role => role == a.RoleName));
            }
            catch (Exception exception)
            {
                return false;
            }
        }

        public async Task<PermissionResultDto> HasUserPermissionWithProductGroup(int userId, string contractCode, List<string> roles)
        {
            var result = new PermissionResultDto();
            result.HasPermission = false;
            result.HasGlobalPermission = false;

            try
            {
                var userTeamWorks = await _teamWorkUserRepository
                .AsNoTracking()
                .Where(a => a.UserId == userId)
                .Include(a => a.TeamWork)
                .Include(a => a.TeamWorkUserRoles)
                .Include(a => a.TeamWorkUserProductGroups)
                .Include(a => a.TeamWorkUserDocumentGroups)
                .Cacheable()
                .ToListAsync();

                // age chizi nabud befrest bere nadare
                if (userTeamWorks == null || !userTeamWorks.Any())
                    return result;

                userTeamWorks = userTeamWorks
                    .Where(a => a.TeamWorkUserRoles.Any(c => c.ContractCode == null || c.ContractCode == contractCode))
                    .ToList();
                // age roli tarif shode nadasht befrest bere nadare
                if (userTeamWorks == null || !userTeamWorks.Any())
                    return result;

                result.ProductGroupIds = userTeamWorks
                    .Where(a => a.TeamWorkUserProductGroups != null)
                    .SelectMany(a => a.TeamWorkUserProductGroups.Select(p => p.ProductGroupId))
                    .Distinct()
                    .ToList();

                result.DocumentGroupIds = userTeamWorks
                    .Where(a => a.TeamWorkUserDocumentGroups != null)
                    .SelectMany(a => a.TeamWorkUserDocumentGroups.Select(p => p.DocumentGroupId))
                    .Distinct()
                    .ToList();

                var acceptedRoles = userTeamWorks
                     .SelectMany(a => a.TeamWorkUserRoles)
                     .Where(v => roles.Contains(v.RoleName))
                     .ToList();

                if (acceptedRoles == null || !acceptedRoles.Any())
                {
                    if (roles != null && roles.Any())
                    {
                        return result;
                    }
                }



                // pas dasresi dare
                result.HasPermission = true;

                if (acceptedRoles.Any(c => c.IsGlobalGroup))
                {
                    result.ProductGroupIds = new List<int>();
                    result.DocumentGroupIds = new List<int>();
                }

                result.ProductGroupIds ??= new List<int>();
                result.DocumentGroupIds ??= new List<int>();


                return result;
            }
            catch (Exception exception)
            {
                return result;
            }
        }

        public async Task<FileDriveTrashPermisionDto> HasUserPermissionForFileDriveTrash(int userId, string contractCode, List<string> roles)
        {
            var result = new FileDriveTrashPermisionDto();
            result.HasPublicPermission = false;
            result.HasPrivatePermission = false;

            try
            {
                var userTeamWorks = await _teamWorkUserRepository
                .AsNoTracking()
                .Where(a => a.UserId == userId)
                .Include(a => a.TeamWork)
                .Include(a => a.TeamWorkUserRoles)
                .Include(a => a.TeamWorkUserProductGroups)
                .Include(a => a.TeamWorkUserDocumentGroups)
                .Cacheable()
                .ToListAsync();

                // age chizi nabud befrest bere nadare
                if (userTeamWorks == null || !userTeamWorks.Any())
                    return result;

                userTeamWorks = userTeamWorks
                    .Where(a => a.TeamWorkUserRoles.Any(c => c.ContractCode == null || c.ContractCode == contractCode))
                    .ToList();
                // age roli tarif shode nadasht befrest bere nadare
                if (userTeamWorks == null || !userTeamWorks.Any())
                    return result;

               

                var acceptedRoles = userTeamWorks
                     .SelectMany(a => a.TeamWorkUserRoles)
                     .Where(v => roles.Contains(v.RoleName))
                     .ToList();

                if (acceptedRoles == null || !acceptedRoles.Any())
                {
                    
                        return result;

                }

                if (acceptedRoles.Any(a => a.RoleName == SCMRole.FileDriveMng ))
                    result.HasPublicPermission=true;

                if (acceptedRoles.Any(a => a.RoleName == SCMRole.PrivateMng))
                    result.HasPrivatePermission = true;
                // pas dasresi dare
               
                return result;
            }
            catch (Exception exception)
            {
                return result;
            }
        }
        public async Task<List<int>> HasUserLimitedByOperationGroup(int userId, string contractCode)
        {
            var result = new List<int>();
          

            try
            {
                var userTeamWorks = await _teamWorkUserRepository
                .AsNoTracking()
                .Where(a => a.UserId == userId)
                .Include(a => a.TeamWork)
                .Include(a => a.TeamWorkUserRoles)
                .Include(a => a.TeamWorkUserOperationGroups)
                .Cacheable()
                .ToListAsync();

                // age chizi nabud befrest bere nadare
                if (userTeamWorks == null || !userTeamWorks.Any())
                    return null;

                userTeamWorks = userTeamWorks
                    .Where(a => a.TeamWork.ContractCode == null || a.TeamWork.ContractCode == contractCode)
                    .ToList();
                // age roli tarif shode nadasht befrest bere nadare
                if (userTeamWorks == null || !userTeamWorks.Any())
                    return null;



                result = userTeamWorks
                    .Where(a => a.TeamWorkUserOperationGroups != null)
                    .SelectMany(a => a.TeamWorkUserOperationGroups.Select(p => p.OperationGroupId))
                    .Distinct()
                    .ToList();

                // pas dasresi dare
                var acceptedRoles = userTeamWorks
                     .SelectMany(a => a.TeamWorkUserRoles)
                     .Where(v => v.IsGlobalGroup)
                     .ToList();

                if (acceptedRoles.Any(c => c.IsGlobalGroup))
                {
                    result = new List<int>();
                }

                result ??= new List<int>();


                return result;
            }
            catch (Exception exception)
            {
                return result;
            }
        }
        public async Task<RoleBasePermissionResultDto> HasUserPermission(int userId, string contractCode, List<string> roles)
        {
            var result = new RoleBasePermissionResultDto();
            result.HasPermission = false;
            result.HasGlobalPermission = false;

            try
            {
                var userTeamWorks = await _teamWorkUserRepository
                .AsNoTracking()
                .Where(a => a.UserId == userId)
                .Include(a => a.TeamWork)
                .Include(a => a.TeamWorkUserRoles)
                .Include(a=>a.TeamWorkUserOperationGroups)
                .Cacheable()
                .ToListAsync();

                // age chizi nabud befrest bere nadare
                if (userTeamWorks == null || !userTeamWorks.Any())
                    return result;

                userTeamWorks = userTeamWorks
                    .Where(a => a.TeamWorkUserRoles.Any(c => c.ContractCode == null || c.ContractCode == contractCode))
                    .ToList();
                // age roli tarif shode nadasht befrest bere nadare
                if (userTeamWorks == null || !userTeamWorks.Any())
                    return result;

                result.OperationGroupList = userTeamWorks
                   .Where(a => a.TeamWorkUserOperationGroups != null)
                   .SelectMany(a => a.TeamWorkUserOperationGroups.Select(p => p.OperationGroupId))
                   .Distinct()
                   .ToList();


                var acceptedRoles = userTeamWorks
                     .SelectMany(a => a.TeamWorkUserRoles)
                     .Where(v => roles.Contains(v.RoleName))
                     .ToList();

                if (acceptedRoles == null || !acceptedRoles.Any())
                {
                    return result;
                }

                // pas dasresi dare
                result.HasPermission = true;

                if (acceptedRoles.Any(c => c.IsGlobalGroup))
                {
                    result.OperationGroupList = new List<int>();
                }

              
                return result;
            }
            catch (Exception exception)
            {
                return result;
            }
        }
       
        public bool HasPermission(int userId, string contractCode, List<string> roles)
        {
            try
            {
                var dbQuery = _teamWorkRoleRepository.Where(a => a.UserId == userId)
                    .Cacheable()
                    .AsQueryable();
                //چک کن ایا برای این کاربر در این نقش و در این قرارداد سطح دسترسی تعریف شده است  یا نه
                return dbQuery
                    .Any(a => (a.ContractCode == null || a.ContractCode == contractCode) && roles.Any(role => role == a.RoleName));

            }
            catch (Exception exception)
            {
                return false;
            }
        }

        public bool HasPermission(int userId, string contractCode)
        {
            try
            {
                var dbQuery = _teamWorkRoleRepository.Where(a => a.UserId == userId)
                    .Cacheable()
                    .AsQueryable();
                //چک کن ایا برای این کاربر در این نقش و در این قرارداد سطح دسترسی تعریف شده است  یا نه
                return dbQuery
                    .Any(a => (a.ContractCode == null || a.ContractCode == contractCode));

            }
            catch (Exception exception)
            {
                return false;
            }
        }
        public bool HasPermission(int userId, List<string> contractCodes, List<string> roles)
        {
            try
            {
                var dbQuery = _teamWorkRoleRepository.Where(a => a.UserId == userId)
                    .Cacheable()
                    .AsQueryable();
                //چک کن ایا برای این کاربر در این نقش و در این قرارداد سطح دسترسی تعریف شده است  یا نه
                return dbQuery
                    .Any(a => (a.ContractCode == null || contractCodes.Contains(a.ContractCode)) && roles.Any(role => role == a.RoleName));

            }
            catch (Exception exception)
            {
                return false;
            }
        }


        public PermissionResultDto GetAccessableUserPermission(int userId, List<string> roles)
        {
            var userTeamWorks = _teamWorkUserRepository
                .AsNoTracking()
                .Where(a => a.UserId == userId)
                .Include(a => a.TeamWorkUserRoles)
                .Include(a => a.TeamWorkUserProductGroups)
                .Cacheable()
                .ToList();

            var result = new PermissionResultDto();
            result.HasPermission = false;
            result.HasGlobalPermission = false;

            // age chizi nabud befrest bere nadare
            if (userTeamWorks == null || !userTeamWorks.Any())
                return result;


            userTeamWorks = userTeamWorks
                .Where(a => a.TeamWorkUserRoles.Any(c => roles.Contains(c.RoleName)))
                .ToList();
            // age roli tarif shode nadasht befrest bere nadare
            if (userTeamWorks == null || !userTeamWorks.Any())
                return result;

            // pas dasresi dare
            result.HasPermission = true;

            /// age dasresei global dasht ba in shive bere
            if (userTeamWorks.Any(a => a.TeamWorkUserRoles.Any(c => c.ContractCode == null)))
            {
                result.HasGlobalPermission = true;
                result.ProductGroupIds = userTeamWorks
                    .SelectMany(c => c.TeamWorkUserProductGroups.Select(a => a.ProductGroupId))
                    .ToList();

                return result;
            }

            /// vaghti global nadasht ba in shive nadasht
            result.TeamWorkData = userTeamWorks.Select(a => new TeamWorkDataDto
            {
                ContractCode = a.TeamWorkUserRoles.First().ContractCode,
                ProductGroupIds = a.TeamWorkUserProductGroups
                 .Select(a => a.ProductGroupId)
                 .ToList()
            }).ToList();

            return result;
        }

        public PermissionServiceResult GetAccessableContract(int userId, List<string> roles)
        {
            try
            {
                var rolePermission = _teamWorkRoleRepository
                    .AsNoTracking()
                    .Where(a => a.UserId == userId)
                    .Cacheable()
                    .Where(a => roles.Contains(a.RoleName))
                    .ToList();

                if (rolePermission == null||!rolePermission.Any())
                    return new PermissionServiceResult { HasPermisson = false };

                if (rolePermission.Any(a => a.ContractCode == null))
                    return new PermissionServiceResult { HasPermisson = true, HasOrganizationPermission = true };

                return new PermissionServiceResult
                {
                    HasPermisson = true,
                    HasOrganizationPermission = false,
                    ContractCodes = rolePermission
                    .Where(a => a.ContractCode != null)
                    .Select(a => a.ContractCode)
                    .Distinct()
                    .ToList()
                };
            }
            catch (Exception e)
            {
                return new PermissionServiceResult { HasPermisson = false };
            }
        }


        public async Task<ServiceResult<List<BaseUserTeamWorkDto>>> GetUserPermissionByUserIdAsync(int userId)
        {
            try
            {
                var userTeamWorks = await _teamWorkUserRepository
                  .AsNoTracking()
                  .Where(a => a.UserId == userId)
                  .Include(a => a.TeamWork)
                  .Include(a => a.TeamWorkUserRoles)
                  .Include(a => a.TeamWorkUserProductGroups)
                  .Include(a => a.TeamWorkUserDocumentGroups)
                  .Cacheable()
                  .ToListAsync();

                var latestTeamwork = await _userLatestTeamWorkRepository.Where(a => a.UserId == userId).OrderByDescending(a => a.LastVisited).ToListAsync();
                List<int> latestOrderTeamwork = new List<int>() ;
                if (latestTeamwork != null && latestTeamwork.Any())
                    latestOrderTeamwork = latestTeamwork.Select(a=>a.TeamWorkId).Take(4).ToList();

                var result = new List<BaseUserTeamWorkDto>();
               
                int topFour = 1;
                if (userTeamWorks == null || !userTeamWorks.Any())
                    return ServiceResultFactory.CreateSuccess(new List<BaseUserTeamWorkDto>());
                if (userTeamWorks.Any(a => a.TeamWork.ContractCode == null))
                {
                    result = await _teamWorkRepository.Include(a=>a.Contract).Where(a => !a.IsDeleted && a.ContractCode != null)
                         .Select(c => new BaseUserTeamWorkDto
                         {
                             
                             TeamWorkCode = c.ContractCode,
                             TeamWorkId = c.Id,
                             Title = c.Title,
                             Services= CreateServiceProperty(c.Contract.DocumentManagement,c.Contract.FileDrive,c.Contract.PurchaseManagement,c.Contract.ConstructionManagement),
                             UserPermissions = new List<UserPermissionDto>()
                         }).ToListAsync();

                    var allUserRoles = userTeamWorks.SelectMany(a => a.TeamWorkUserRoles).ToList();

                    foreach (var item in result.Where(a=> latestOrderTeamwork.Contains(a.TeamWorkId)))
                    {
                        item.IsLatest = (latestOrderTeamwork != null && latestOrderTeamwork.Any() && latestOrderTeamwork.Contains(item.TeamWorkId)||topFour<=4) ? true : false;
                        if (item.IsLatest)
                            topFour++;
                        if (latestTeamwork!=null&&latestTeamwork.Any(a => a.TeamWorkId == item.TeamWorkId))
                            item.LastVisited = latestTeamwork.First(a => a.TeamWorkId == item.TeamWorkId).LastVisited;
                        item.UserPermissions = allUserRoles
                            .Where(a => a.ContractCode == null || a.ContractCode == item.TeamWorkCode)
                            .GroupBy(a => a.SubModuleName)
                            .Select(v => new UserPermissionDto
                            {
                                Permission = v.Key,
                                PermissionIds = v.Select(v => v.RoleId).Distinct().ToList()
                            }).ToList();
                    }
                    foreach (var item in result.Where(a => !latestOrderTeamwork.Contains(a.TeamWorkId)))
                    {
                        item.IsLatest = (latestOrderTeamwork != null && latestOrderTeamwork.Any() && latestOrderTeamwork.Contains(item.TeamWorkId) || topFour <= 4) ? true : false;
                        if (item.IsLatest)
                            topFour++;
                        if (latestTeamwork != null && latestTeamwork.Any(a => a.TeamWorkId == item.TeamWorkId))
                            item.LastVisited = latestTeamwork.First(a => a.TeamWorkId == item.TeamWorkId).LastVisited;
                        item.UserPermissions = allUserRoles
                            .Where(a => a.ContractCode == null || a.ContractCode == item.TeamWorkCode)
                            .GroupBy(a => a.SubModuleName)
                            .Select(v => new UserPermissionDto
                            {
                                Permission = v.Key,
                                PermissionIds = v.Select(v => v.RoleId).Distinct().ToList()
                            }).ToList();
                    }
                }
                else
                {

                    result = userTeamWorks.Select(c => new BaseUserTeamWorkDto
                    {
                        TeamWorkId = c.TeamWorkId,
                        TeamWorkCode = c.TeamWork.ContractCode,
                        Title = c.TeamWork.Title,
                        Services = CreateServiceProperty(_contractRepository.First(a => a.ContractCode == c.TeamWork.ContractCode).DocumentManagement, _contractRepository.First(a => a.ContractCode == c.TeamWork.ContractCode).FileDrive, _contractRepository.First(a => a.ContractCode == c.TeamWork.ContractCode).PurchaseManagement, _contractRepository.First(a => a.ContractCode == c.TeamWork.ContractCode).ConstructionManagement),
                        
                        UserPermissions = c.TeamWorkUserRoles.GroupBy(c => c.SubModuleName)
                                          .Select(v => new UserPermissionDto
                                          {
                                              Permission = v.Key,
                                              PermissionIds = v.Select(v => v.RoleId).Distinct().ToList()
                                          }).ToList(),
                    }).ToList();
                   foreach(var item in result.Where(a=> latestOrderTeamwork.Contains(a.TeamWorkId)))
                    {
                        item.IsLatest = (latestOrderTeamwork != null && latestOrderTeamwork.Any() && latestOrderTeamwork.Contains(item.TeamWorkId) || topFour <= 4) ? true : false;
                        if (item.IsLatest)
                            topFour++;
                        if (latestTeamwork != null && latestTeamwork.Any(a => a.TeamWorkId == item.TeamWorkId))
                            item.LastVisited = latestTeamwork.First(a => a.TeamWorkId == item.TeamWorkId).LastVisited;
                    }
                    foreach (var item in result.Where(a => !latestOrderTeamwork.Contains(a.TeamWorkId)))
                    {
                        item.IsLatest = (latestOrderTeamwork != null && latestOrderTeamwork.Any() && latestOrderTeamwork.Contains(item.TeamWorkId) || topFour <= 4) ? true : false;
                        if (item.IsLatest)
                            topFour++;
                        if (latestTeamwork != null && latestTeamwork.Any(a => a.TeamWorkId == item.TeamWorkId))
                            item.LastVisited = latestTeamwork.First(a => a.TeamWorkId == item.TeamWorkId).LastVisited;
                    }
                }

                return ServiceResultFactory.CreateSuccess(result.OrderByDescending(a => a.LastVisited).ToList());
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<BaseUserTeamWorkDto>>(null, exception);
            }
        }

        private static List<string> CreateServiceProperty(bool docMangement,bool fileDrive,bool purchaseManagement, bool contructionManagement)
        {
            List<string> result = new List<string>();
            if (fileDrive)
                result.Add("FileDriveService");
            if (docMangement)
                result.Add("DocumentMngService");
            if (purchaseManagement)
                result.Add("PurchasingMngService");
            if (contructionManagement)
                result.Add("OperationMngService");
            return result;
        }
        //public async Task<ServiceResult<List<GlobalSearchPermisionDto>>> GetGlobalSearchPermissionByUserIdAsync(int userId, List<SCMFormPermission> SCMFormPermissions)
        //{
        //    try
        //    {
        //        var SCMSearchForm = new List<SCMFormPermission>
        //        {
        //            SCMFormPermission.Contract,
        //            SCMFormPermission.Bom,
        //            SCMFormPermission.DocumentList,
        //            SCMFormPermission.MRP,
        //            SCMFormPermission.PurchaseRequest
        //        };

        //        var dbQuery = _teamWorkRoleRepository
        //            .Where(a => a.UserId == userId)
        //            .Cacheable()
        //            .Where(c => SCMSearchForm.Contains(c.SCMFormPermission));

        //        if (SCMFormPermissions != null)
        //            dbQuery = dbQuery.Where(a => SCMFormPermissions.Contains(a.SCMFormPermission));

        //        var dbQueryList = await dbQuery
        //           .ToListAsync();

        //        var groupbyDCMForm = dbQueryList.GroupBy(a => a.SCMFormPermission);

        //        var result = new List<GlobalSearchPermisionDto>();
        //        foreach (var item in groupbyDCMForm)
        //        {
        //            var newForm = new GlobalSearchPermisionDto();

        //            newForm.SCMForm = item.Key;
        //            newForm.IsHaveGlobalPermision = item.Any(c => c.ContractCode == null);
        //            newForm.ContractCodes = newForm.IsHaveGlobalPermision ? null : item.Select(a => a.ContractCode).ToList();

        //            result.Add(newForm);
        //        }
        //        return ServiceResultFactory.CreateSuccess(result);

        //    }
        //    catch (Exception exception)
        //    {
        //        return ServiceResultFactory.CreateException<List<GlobalSearchPermisionDto>>(null, exception);
        //    }

        //}


    }
}