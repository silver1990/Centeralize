using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Raybod.SCM.DataAccess.Core;
using Raybod.SCM.DataTransferObject.Role;
using Raybod.SCM.Domain.Model;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Microsoft.EntityFrameworkCore;

namespace Raybod.SCM.Services.Implementation
{
    public class RoleService : IRoleService
    {
        #region Constructor

        private readonly DbSet<Role> _roleRepository;
      
        private readonly DbSet<User> _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RoleService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _roleRepository = unitOfWork.Set<Role>();       
            _userRepository = unitOfWork.Set<User>();
        }

        #endregion


        public async Task<ServiceResult<bool>> AddRoleAsync(AddRoleDto addRoleDto)
        {
            var messages = new List<ServiceMessage>();
            try
            {
            
                    return ServiceResultFactory.CreateSuccess(true);
             

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        private string CalculateModulePermission(List<int> permissionIds)
        {
            permissionIds = new List<int> { 100, 200, 215, 1100, 2100, 3100, 4100, 5100, 6100, 7100, 5200, 1200, 1300, 2200, 3200 };
            string modulePermission = string.Empty;
            string moduleSectionPermission = string.Empty;
            List<int> tempList = new List<int>();

            if (permissionIds == null || !permissionIds.Any())
                return modulePermission;

            tempList = permissionIds.Select(x => x = (x / 100)).Where(x => x >= 10).Distinct().ToList();

            if (tempList == null || tempList.Count() == 0)
                return string.Empty;
            foreach (var item in tempList)
            {
                moduleSectionPermission += $"{item}'";
            }

            tempList = tempList.Select(p => p = (p / 10)).Where(p => p >= 1).Distinct().ToList();
            if (tempList == null || tempList.Count() == 0)
                return string.Empty;
            foreach (var item in tempList)
            {
                modulePermission += $"{item}";
            }

            return $"{modulePermission},{moduleSectionPermission}";
        }
        public async Task<ServiceResult<EditRoleDto>> EditRoleAsync(EditRoleDto editRoleDto)
        {
            var messages = new List<ServiceMessage>();
            try
            {
              
                    messages.Add(new ServiceMessage(MessageType.Warning, MessageId.UserMustBeHaveMoreThanZeroRole));
                    return new ServiceResult<EditRoleDto>(false, null, messages);
             
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new EditRoleDto(), exception);
            }
        }

        public async Task<ServiceResult<EditRoleDto>> GetRoleByIdAsync(int roleId)
        {
            var messages = new List<ServiceMessage>();
            try
            {
                var list = await _roleRepository.FirstOrDefaultAsync(a => a.Id == roleId);
                var mapperConfiguration = new MapperConfiguration(configuration =>
                {
                    configuration.CreateMap<Role, EditRoleDto>();
                });
                var mapper = mapperConfiguration.CreateMapper();
                var resultsList = mapper.Map<EditRoleDto>(list);
                messages.Add(new ServiceMessage(MessageType.Succeed, MessageId.Succeeded));
                return new ServiceResult<EditRoleDto>(true, resultsList, messages);
            }
            catch (Exception exception)
            {
                messages.Add(new ServiceMessage(MessageType.Error, MessageId.Exception));
                return new ServiceResult<EditRoleDto>(false, null, messages, exception);
            }
        }


        public async Task<ServiceResult<List<BaseRoleDto>>> GetAllRoleAsync()
        {
            var messages = new List<ServiceMessage>();
            try
            {
                var mapperConfiguration = new MapperConfiguration(configuration =>
                {
                    configuration.CreateMap<Role, BaseRoleDto>();
                });
                var mapper = mapperConfiguration.CreateMapper();
                var list = await _roleRepository.ToListAsync();
                var resultsList = mapper.Map<List<BaseRoleDto>>(list);
                messages.Add(new ServiceMessage(MessageType.Succeed, MessageId.Succeeded));
                return new ServiceResult<List<BaseRoleDto>>(true, resultsList, messages);
            }
            catch (Exception exception)
            {
                messages.Add(new ServiceMessage(MessageType.Error, MessageId.Exception));
                return new ServiceResult<List<BaseRoleDto>>(false, null, messages, exception);
            }
        }

        public async Task<ServiceResult> DeleteRoleAsync(int roleId)
        {
            var messages = new List<ServiceMessage>();
            try
            {
                var entity = await _roleRepository.FirstOrDefaultAsync(a => a.Id == roleId);
                //if (entity.UserRoles.Any())
                //{
                //    messages.Add(new ServiceMessage(MessageType.Error, MessageId.DeleteDontAllowedBeforeSubset));
                //    return new ServiceResult(false, messages);
                //}

                _roleRepository.Remove(entity);
                var isDeleted = await _unitOfWork.SaveChangesAsync() > 0;
                if (isDeleted)
                {
                    messages.Add(new ServiceMessage(MessageType.Succeed, MessageId.Succeeded));
                    return new ServiceResult(false, messages);
                }

                messages.Add(new ServiceMessage(MessageType.Error, MessageId.DeleteEntityFailed));
                return new ServiceResult(true, messages);
            }
            catch (Exception exception)
            {
                messages.Add(new ServiceMessage(MessageType.Error, MessageId.Exception));
                return new ServiceResult(false, messages, exception);
            }
        }



    }
}