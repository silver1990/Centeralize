using AutoMapper;
using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Raybod.SCM.DataAccess.Core;
using Raybod.SCM.DataAccess.Extention;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject._PanelDocument.Communication.Comment;
using Raybod.SCM.DataTransferObject.Consultant;
using Raybod.SCM.DataTransferObject.Email;
using Raybod.SCM.DataTransferObject.User;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Model;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Services.Utilitys.MailService;
using Raybod.SCM.Utility.FileHelper;
using Raybod.SCM.Utility.Helpers;
using Raybod.SCM.Utility.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Implementation.AdminPanel
{
    public class ConsultantService : IConsultantService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITeamWorkAuthenticationService _authenticationService;
        private readonly DbSet<Consultant> _consultantRepository;
        private readonly DbSet<User> _userRepository;
        private readonly DbSet<CompanyUser> _companyUserRepository;
        private readonly DbSet<Contract> _contractRepository;
        private readonly Raybod.SCM.Services.Utilitys.FileHelper _fileHelper;
        private readonly CompanyAppSettingsDto _appSettings;
        private readonly IViewRenderService _viewRenderService;
        private readonly IAppEmailService _appEmailService;
        private readonly DbSet<TeamWorkUser> _teamWorkUsersRepository;
        public ConsultantService(
            IUnitOfWork unitOfWork,
            IWebHostEnvironment hostingEnvironmentRoot,
            ITeamWorkAuthenticationService authenticationService,
            IOptions<CompanyAppSettingsDto> appSettings, IViewRenderService viewRenderService, IAppEmailService appEmailService)
        {
            _appSettings = appSettings.Value;
            _unitOfWork = unitOfWork;
            _authenticationService = authenticationService;
            _consultantRepository = _unitOfWork.Set<Consultant>();
            _companyUserRepository = _unitOfWork.Set<CompanyUser>();
            _userRepository = _unitOfWork.Set<User>();
            _contractRepository = _unitOfWork.Set<Contract>();
            _teamWorkUsersRepository = _unitOfWork.Set<TeamWorkUser>();
            _fileHelper = new Utilitys.FileHelper(hostingEnvironmentRoot);
            _viewRenderService = viewRenderService;
            _appEmailService = appEmailService;
        }

        public async Task<ServiceResult<BaseConsultantDto>> AddConsultantAsync(AuthenticateDto authenticate, AddConsultantDto model)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<BaseConsultantDto>(null, MessageId.AccessDenied);

                if (!string.IsNullOrEmpty(model.Email) && !RegexHelpers.IsValidEmail(model.Email))
                    return ServiceResultFactory.CreateError<BaseConsultantDto>(null, MessageId.EmailNotCorrect);

                if (await _consultantRepository.AnyAsync(x => !x.IsDeleted && x.ConsultantCode == model.ConsultantCode))
                    return ServiceResultFactory.CreateError(new BaseConsultantDto(), MessageId.CodeExist);

               

                if (!string.IsNullOrWhiteSpace(model.Logo))
                {
                    var saveImage = _fileHelper.SaveImagesFromTemp(model.Logo, ServiceSetting.UploadImagesPath.LogoLarge, (int)ImageHelper.ImageWidth.FullHD);
                    saveImage = _fileHelper.SaveImagesFromTemp(model.Logo, ServiceSetting.UploadImagesPath.LogoSmall, (int)ImageHelper.ImageWidth.Vcd);
                    if (!string.IsNullOrWhiteSpace(saveImage))
                    {
                        _fileHelper.DeleteImagesFromTemp(model.Logo);
                        model.Logo = saveImage;
                    }
                }

                var consultantModel = new Consultant
                {
                    Name = model.Name,
                    ConsultantCode = model.ConsultantCode,
                    Address = model.Address,
                    Email = model.Email,
                    Fax = model.Fax,
                    Logo = model.Logo,
                    PostalCode = model.PostalCode,
                    TellPhone = model.TellPhone,
                    Website = model.Website
                };
                _consultantRepository.Add(consultantModel);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var result = new BaseConsultantDto
                    {
                        Id = consultantModel.Id,
                        Address = consultantModel.Address,
                        Email = consultantModel.Email,
                        Fax = consultantModel.Fax,
                        Logo = consultantModel.Logo,
                        Name = consultantModel.Name,
                        ConsultantCode = consultantModel.ConsultantCode,
                        PostalCode = consultantModel.PostalCode,
                        TellPhone = consultantModel.TellPhone,
                        Website = consultantModel.Website
                    };

                    return ServiceResultFactory.CreateSuccess(result);
                }

                return ServiceResultFactory.CreateError(new BaseConsultantDto(), MessageId.SaveFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new BaseConsultantDto(), exception);
            }
        }

        public async Task<ServiceResult<bool>> EditConsultantAsync(AuthenticateDto authenticate, int consultantId, AddConsultantDto model)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);


                if (!string.IsNullOrEmpty(model.Email) && !RegexHelpers.IsValidEmail(model.Email))
                    return ServiceResultFactory.CreateError(false, MessageId.EmailNotCorrect);

                var selectedConsultant = await _consultantRepository.FirstOrDefaultAsync(a => !a.IsDeleted && a.Id == consultantId);
                if (selectedConsultant == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (await _consultantRepository.AnyAsync(x => !x.IsDeleted && x.Id != consultantId && x.ConsultantCode == model.ConsultantCode))
                    return ServiceResultFactory.CreateError(false, MessageId.CodeExist);

                selectedConsultant.Address = model.Address;
                selectedConsultant.ConsultantCode = model.ConsultantCode;
                selectedConsultant.Email = model.Email;
                selectedConsultant.Fax = model.Fax;
                selectedConsultant.Name = model.Name;
                selectedConsultant.PostalCode = model.PostalCode;
                selectedConsultant.TellPhone = model.TellPhone;
                selectedConsultant.Website = model.Website;

                if (!string.IsNullOrWhiteSpace(model.Logo) && _fileHelper.ImageExistInTemp(model.Logo))
                {
                    var saveImage = _fileHelper.SaveImagesFromTemp(model.Logo, ServiceSetting.UploadImagesPath.LogoLarge, (int)ImageHelper.ImageWidth.FullHD);
                    saveImage = _fileHelper.SaveImagesFromTemp(model.Logo, ServiceSetting.UploadImagesPath.LogoSmall, (int)ImageHelper.ImageWidth.Vcd);
                    if (!string.IsNullOrWhiteSpace(saveImage))
                    {
                        _fileHelper.DeleteImagesFromTemp(model.Logo);
                        selectedConsultant.Logo = saveImage ?? selectedConsultant.Logo;
                    }
                }
                else
                {
                    selectedConsultant.Logo = selectedConsultant.Logo;
                }

                await _unitOfWork.SaveChangesAsync();
                return ServiceResultFactory.CreateSuccess(true);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<List<BaseConsultantDto>>> GetConsultantAsync(AuthenticateDto authenticate, ConsultantQuery query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<BaseConsultantDto>>(null, MessageId.AccessDenied);

                var dbQuery = _consultantRepository
                    .AsNoTracking()
                    .OrderByDescending(a => a.Id)
                    .Where(x => !x.IsDeleted);

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(x => x.Name.Contains(query.SearchText) || x.ConsultantCode.Contains(query.SearchText));

                var totalCount = dbQuery.Count();
                dbQuery = dbQuery.ApplayPageing(query);

                var result = await dbQuery.Select(c => new BaseConsultantDto
                {
                    Id = c.Id,
                    Address = c.Address,
                    ConsultantCode = c.ConsultantCode,
                    Email = c.Email,
                    Fax = c.Fax,
                    Logo = c.Logo,
                    Name = c.Name,
                    PostalCode = c.PostalCode,
                    TellPhone = c.TellPhone,
                    Website = c.Website
                }).ToListAsync();

                //_appSettings.AdminHost + ServiceSetting.UploadImagesPath.LogoSmall + c.Logo :

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new List<BaseConsultantDto>(), exception);
            }
        }

        public async Task<ServiceResult<List<ConsultantMiniInfoDto>>> GetConsultantMiniInfoAsync(AuthenticateDto authenticate, ConsultantQuery query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<ConsultantMiniInfoDto>>(null, MessageId.AccessDenied);

                var dbQuery = _consultantRepository
                   .AsNoTracking()
                   .OrderByDescending(a => a.Id)
                   .Where(x => !x.IsDeleted);

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(x => x.Name.Contains(query.SearchText) || x.ConsultantCode.Contains(x.ConsultantCode));

                var totalCount = dbQuery.Count();
                dbQuery = dbQuery.ApplayPageing(query);

                var result = await dbQuery.Select(c => new ConsultantMiniInfoDto
                {
                    Id = c.Id,
                    Logo = c.Logo != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.LogoSmall + c.Logo : "",
                    ConsultantCode = c.ConsultantCode,
                    Name = c.Name,
                }).ToListAsync();

                //_appSettings.AdminHost + ServiceSetting.UploadImagesPath.LogoSmall + c.Logo :

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new List<ConsultantMiniInfoDto>(), exception);
            }
        }

        public async Task<ServiceResult<List<ConsultantMiniInfoDto>>> GetConsultantMiniInfoWithoutPageingAsync(AuthenticateDto authenticate, ConsultantQuery query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<ConsultantMiniInfoDto>>(null, MessageId.AccessDenied);

                var dbQuery = _consultantRepository
                   .AsNoTracking()
                   .OrderByDescending(a => a.Id)
                   .Where(x => !x.IsDeleted && (x.ConsultantContracts.Any(b => b.ContractCode == authenticate.ContractCode)));

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(x => x.Name.Contains(query.SearchText) || x.ConsultantCode.Contains(x.ConsultantCode));

                var totalCount = dbQuery.Count();

                var result = await dbQuery.Select(c => new ConsultantMiniInfoDto
                {
                    Id = c.Id,
                    Logo = c.Logo != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.LogoSmall + c.Logo : "",
                    ConsultantCode = c.ConsultantCode,
                    Name = c.Name,
                }).ToListAsync();

                //_appSettings.AdminHost + ServiceSetting.UploadImagesPath.LogoSmall + c.Logo :

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new List<ConsultantMiniInfoDto>(), exception);
            }
        }


        public async Task<ServiceResult<BaseConsultantDto>> GetConsultantByIdAsync(AuthenticateDto authenticate, int consultantId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<BaseConsultantDto>(null, MessageId.AccessDenied);

                var consultantModel = await _consultantRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.Id == consultantId)
                    .Select(c => new BaseConsultantDto
                    {
                        Id = c.Id,
                        Address = c.Address,
                        ConsultantCode = c.ConsultantCode,
                        Email = c.Email,
                        Fax = c.Fax,
                        Logo = c.Logo,
                        Name = c.Name,
                        PostalCode = c.PostalCode,
                        TellPhone = c.TellPhone,
                        Website = c.Website
                    }).FirstOrDefaultAsync();

                if (consultantModel == null)
                    return ServiceResultFactory.CreateError<BaseConsultantDto>(null, MessageId.EntityDoesNotExist);

                return ServiceResultFactory.CreateSuccess(consultantModel);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new BaseConsultantDto(), exception);
            }
        }

        public async Task<ServiceResult<bool>> DeleteConsultantAsync(AuthenticateDto authenticate, int consultantid)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var consultantModel = await _consultantRepository
                    .FirstOrDefaultAsync(a => !a.IsDeleted && a.Id == consultantid);

                if (consultantModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (await _contractRepository.AnyAsync(a => !a.IsDeleted && a.ConsultantId == consultantid))
                    return ServiceResultFactory.CreateError(false, MessageId.DeleteDontAllowedBeforeSubset);

                if (await _userRepository.AnyAsync(a => !a.IsDeleted && _consultantRepository.Any(b=>b.ConsultantUsers.Any(c=>c.Email==a.Email))))
                    return ServiceResultFactory.CreateError(false, MessageId.DeleteDontAllowedBeforeSubset);

                consultantModel.IsDeleted = true;
                return await _unitOfWork.SaveChangesAsync() > 0
                    ? ServiceResultFactory.CreateSuccess(true)
                    : ServiceResultFactory.CreateError(false, MessageId.InternalError);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<BaseConsultantUserDto>> AddConsultantUserAsync(AuthenticateDto authenticate, int consultantId, AddConsultantUserDto model,bool? isUserSystem)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<BaseConsultantUserDto>(null, MessageId.AccessDenied);

                if (!await _consultantRepository.AnyAsync(a => !a.IsDeleted && a.Id == consultantId))
                    return ServiceResultFactory.CreateError<BaseConsultantUserDto>(null, MessageId.ConsultantNotFound);


                if (await _companyUserRepository.AnyAsync(a => !a.IsDeleted && a.Email == model.Email))
                    return ServiceResultFactory.CreateError<BaseConsultantUserDto>(null, MessageId.DuplicateInformation);

                if (await _userRepository.AnyAsync(a => !a.IsDeleted && a.Email == model.Email) && isUserSystem != null && isUserSystem == true)
                    return ServiceResultFactory.CreateError<BaseConsultantUserDto>(null, MessageId.DuplicateInformation);

                var newConsultantUserModel = new CompanyUser
                {
                    ConsultantId = consultantId,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName
                };
                _companyUserRepository.Add(newConsultantUserModel);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var res = new BaseConsultantUserDto
                    {
                        ConsultantUserId = newConsultantUserModel.CompanyUserId,
                        Email = model.Email,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        UserType = _userRepository.Any(u => u.Email == model.Email) ? _userRepository.First(u => u.Email == model.Email).UserType : 0,
                    };
                    return ServiceResultFactory.CreateSuccess(res);
                }
                return ServiceResultFactory.CreateError<BaseConsultantUserDto>(null, MessageId.SaveFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<BaseConsultantUserDto>(null, exception);
            }
        }
        public async Task<ServiceResult<BaseConsultantUserDto>> AddConsultantUserAsync(AuthenticateDto authenticate, int consultantId, AddConsultantUserDto model,bool? isUserSystem,AddUserDto userModel,UserStatus type)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<BaseConsultantUserDto>(null, MessageId.AccessDenied);

                if (!await _consultantRepository.AnyAsync(a => !a.IsDeleted && a.Id == consultantId))
                    return ServiceResultFactory.CreateError<BaseConsultantUserDto>(null, MessageId.ConsultantNotFound);


                if (await _companyUserRepository.AnyAsync(a => !a.IsDeleted && a.Email == model.Email))
                    return ServiceResultFactory.CreateError<BaseConsultantUserDto>(null, MessageId.DuplicateInformation);

                if (await _userRepository.AnyAsync(a => !a.IsDeleted && a.Email == model.Email) && isUserSystem != null && isUserSystem == true)
                    return ServiceResultFactory.CreateError<BaseConsultantUserDto>(null, MessageId.DuplicateInformation);

                var newConsultantUserModel = new CompanyUser
                {
                    ConsultantId = consultantId,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName
                };
                var userResult = await AddUserAsync(userModel, (int)type);
                if (!userResult.Succeeded)
                    return ServiceResultFactory.CreateError<BaseConsultantUserDto>(null, userResult.Messages[0].Message);
                _companyUserRepository.Add(newConsultantUserModel);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var res = new BaseConsultantUserDto
                    {
                        ConsultantUserId = newConsultantUserModel.CompanyUserId,
                        Email = model.Email,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        UserType = _userRepository.Any(u => u.Email == model.Email) ? _userRepository.First(u => u.Email == model.Email).UserType : 0,
                    };
                    try
                    {
                        BackgroundJob.Enqueue(() => SendEmailOnAddCustomerUser(authenticate.UserFullName, model.Email, model.FirstName + " " + model.LastName, userModel.Password));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.StackTrace);
                    }
                    return ServiceResultFactory.CreateSuccess(res);
                }
                return ServiceResultFactory.CreateError<BaseConsultantUserDto>(null, MessageId.SaveFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<BaseConsultantUserDto>(null, exception);
            }
        }



        public async Task<ServiceResult<BaseConsultantUserDto>> EditConsultantUserAsync(AuthenticateDto authenticate, int consultantId, EditConsultantUserDto model, int companyUserId, AddUserDto userModel, UserStatus type, bool active)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<BaseConsultantUserDto>(null, MessageId.AccessDenied);

                if (!await _consultantRepository.AnyAsync(a => !a.IsDeleted && a.Id == consultantId))
                    return ServiceResultFactory.CreateError<BaseConsultantUserDto>(null, MessageId.ConsultantNotFound);

                if (!await _companyUserRepository.AnyAsync(a => !a.IsDeleted && a.CompanyUserId == companyUserId))
                    return ServiceResultFactory.CreateError<BaseConsultantUserDto>(null, MessageId.CompanyUserNotFound);

                var newConsultantUserModel = await _companyUserRepository.FirstAsync(a => !a.IsDeleted && a.CompanyUserId == companyUserId);
                newConsultantUserModel.FirstName = model.FirstName;
                newConsultantUserModel.LastName = model.LastName;
                newConsultantUserModel.UpdateDate = DateTime.Now;
                var userResult = await AddUserForCustomerAsync(userModel, (int)type, active);
                if (!userResult.Succeeded)
                    return ServiceResultFactory.CreateError<BaseConsultantUserDto>(null, userResult.Messages[0].Message);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var res = new BaseConsultantUserDto
                    {
                        ConsultantUserId = newConsultantUserModel.CompanyUserId,
                        Email = model.Email,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        UserType = _userRepository.Any(u => u.Email == model.Email) ? _userRepository.First(u => u.Email == model.Email).UserType : 0,
                    };
                    if (userResult.Result.NewUser)
                    {
                        try
                        {
                            BackgroundJob.Enqueue(() => SendEmailOnAddCustomerUser(authenticate.UserFullName, model.Email, userResult.Result.FullName, userResult.Result.Password));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.StackTrace);
                        }

                    }
                    return ServiceResultFactory.CreateSuccess(res);
                }
                return ServiceResultFactory.CreateError<BaseConsultantUserDto>(null, MessageId.SaveFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<BaseConsultantUserDto>(null, exception);
            }
        }
        public async Task<ServiceResult<List<BaseConsultantUserDto>>> GetConsultantUserAsync(AuthenticateDto authenticate, int consultantId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<BaseConsultantUserDto>>(null, MessageId.AccessDenied);

                var dbQuery = _companyUserRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.ConsultantId == consultantId);

                var result = await dbQuery.Select(c => new BaseConsultantUserDto
                {
                    ConsultantUserId = c.CompanyUserId,
                    Email = c.Email,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    UserType = (_userRepository.Any(a => a.Email == c.Email)) ? _userRepository.FirstOrDefault(a => a.Email == c.Email).UserType : 0
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<BaseConsultantUserDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<bool>> DeleteConsultantUserByIdAsync(AuthenticateDto authenticate, int consultantId, int consultantUserId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var userModel = await _companyUserRepository.FirstOrDefaultAsync(a => !a.IsDeleted && a.ConsultantId == consultantId && a.CompanyUserId == consultantUserId);
                if (userModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);
               
                var user = await _userRepository.FirstOrDefaultAsync(u =>!u.IsDeleted&& u.Email == userModel.Email);
                if (user != null)
                {
                    if (await _teamWorkUsersRepository.AnyAsync(a => a.UserId ==user.Id))
                        return ServiceResultFactory.CreateError(false, MessageId.DeleteDontAllowedBeforeSubset);
                    user.IsDeleted = true;
                }
                    

                userModel.IsDeleted = true;
                return await _unitOfWork.SaveChangesAsync() > 0 ?
                    ServiceResultFactory.CreateSuccess(true) :
                    ServiceResultFactory.CreateError(false, MessageId.AccessDenied);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        private async Task<ServiceResult<ListUserDto>> AddUserAsync(AddUserDto model, int type)
        {
            try
            {
                model.UserType = type;


                if (!string.IsNullOrWhiteSpace(model.Email) && Regex.IsMatch(model.Email, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$") == false)
                    return ServiceResultFactory.CreateError<ListUserDto>(null, MessageId.EmailNotCorrect);

                if (await _userRepository.AnyAsync(a => !a.IsDeleted && a.Email == model.Email))
                    return ServiceResultFactory.CreateError<ListUserDto>(null, MessageId.EmailExist);

                if (await _companyUserRepository.AnyAsync(a => !a.IsDeleted && a.Email == model.Email))
                    return ServiceResultFactory.CreateError<ListUserDto>(null, MessageId.EmailExist);


                if (string.IsNullOrEmpty(model.UserName) || string.IsNullOrEmpty(model.Password))
                    return ServiceResultFactory.CreateError<ListUserDto>(null, MessageId.UserNameOrPasswordNull);

                if (await _userRepository.AnyAsync(a => !a.IsDeleted && a.UserName == model.UserName))
                    return ServiceResultFactory.CreateError<ListUserDto>(null, MessageId.UserNameExist);
                var mapperConfiguration = new MapperConfiguration(configuration =>
                {
                    configuration.CreateMap<AddUserDto, User>();
                    configuration.CreateMap<User, AddUserDto>();
                    configuration.CreateMap<User, ListUserDto>()
                    .ForMember(u => u.userId, m => m.MapFrom(c => c.Id))
                    .ForMember(u => u.Image, m => m.MapFrom(c => c.Image == null ? "" : _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + c.Image));
                });
                string fullName = "";
                User userResult = null;
                var mapper = mapperConfiguration.CreateMapper();
                var user = await _userRepository.FirstOrDefaultAsync(u => (u.IsDeleted && u.UserName == model.UserName) || (u.IsDeleted && u.Email == model.Email));

                if (user != null && ((model.UserType == (int)UserStatus.ConsultantUser || model.UserType == (int)UserStatus.CustomerUser) && user.UserType != (int)UserStatus.OrganizationUser && user.UserType != (int)UserStatus.SupperUser) && !_teamWorkUsersRepository.Any(a => a.UserId == user.Id))
                {
                    var UserModel = mapper.Map<User>(model);
                    if (!string.IsNullOrEmpty(UserModel.Password))
                    {
                        var hasher = new PasswordHasher();
                        UserModel.Password = hasher.HashPassword(UserModel.Password);
                    }
                    user.Password = UserModel.Password;
                    user.UserName = UserModel.Email;
                    user.FullName = UserModel.FirstName + " " + UserModel.LastName;
                    user.FirstName = UserModel.FirstName;
                    user.LastName = UserModel.LastName;
                    user.IsDeleted = false;
                    user.UserType = model.UserType;
                    userResult = user;
                }
                else
                {
                    var UserModel = mapper.Map<User>(model);

                    UserModel.FullName = UserModel.FirstName + " " + UserModel.LastName;
                    UserModel.IsActive = true;
                    fullName = UserModel.FullName;
                    if (!string.IsNullOrWhiteSpace(model.Image))
                    {
                        var saveImage = _fileHelper.SaveImagesFromTemp(model.Image, ServiceSetting.UploadImagesPath.UserSmall, (int)ImageHelper.ImageWidth.FullHD);
                        saveImage = _fileHelper.SaveImagesFromTemp(model.Image, ServiceSetting.UploadImagesPath.UserLarge, (int)ImageHelper.ImageWidth.Vcd);
                        if (!string.IsNullOrWhiteSpace(saveImage))
                        {
                            _fileHelper.DeleteImagesFromTemp(model.Image);
                            UserModel.Image = saveImage;
                        }
                    }

                    if (!string.IsNullOrEmpty(UserModel.Password))
                    {
                        var hasher = new PasswordHasher();
                        UserModel.Password = hasher.HashPassword(UserModel.Password);
                    }
                    _userRepository.Add(UserModel);
                    userResult = UserModel;
                }


                var result = mapper.Map<ListUserDto>(userResult);
                return ServiceResultFactory.CreateSuccess(result);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<ListUserDto>(null, exception);
            }
        }

        private async Task<ServiceResult<EditCustomerUserResultDto>> AddUserForCustomerAsync(AddUserDto model, int type, bool active)
        {
            try
            {
                model.UserType = type;

                if (!string.IsNullOrWhiteSpace(model.Email) && Regex.IsMatch(model.Email, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$") == false)
                    return ServiceResultFactory.CreateError<EditCustomerUserResultDto>(null, MessageId.EmailNotCorrect);

                if (string.IsNullOrEmpty(model.UserName) || string.IsNullOrEmpty(model.Password))
                    return ServiceResultFactory.CreateError<EditCustomerUserResultDto>(null, MessageId.UserNameOrPasswordNull);


                var mapperConfiguration = new MapperConfiguration(configuration =>
                {
                    configuration.CreateMap<AddUserDto, User>();
                    configuration.CreateMap<User, AddUserDto>();
                    configuration.CreateMap<User, ListUserDto>()
                    .ForMember(u => u.userId, m => m.MapFrom(c => c.Id))
                    .ForMember(u => u.Image, m => m.MapFrom(c => c.Image == null ? "" : _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + c.Image));
                });
                string fullName = "";
                bool newUser = false;
                User userResult = null;
                var mapper = mapperConfiguration.CreateMapper();
                var user = await _userRepository.FirstOrDefaultAsync(u => !u.IsDeleted && (u.Email == model.Email));
                var deletedUser = await _userRepository.FirstOrDefaultAsync(u => u.IsDeleted &&  (u.Email == model.Email) && (u.UserType != (int)UserStatus.OrganizationUser && u.UserType != (int)UserStatus.SupperUser) && !_teamWorkUsersRepository.Any(t => t.UserId == u.Id));

                if (user != null)
                {
                    user.IsActive = active;
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.FullName = model.FirstName + " " + model.LastName;
                    user.UserType = (active) ? type : 0;
                    userResult = user;
                    _userRepository.Update(user);
                }
                else if (deletedUser != null)
                {
                    var hasher = new PasswordHasher();
                    deletedUser.Password = hasher.HashPassword(model.Password);
                    deletedUser.IsDeleted = false;
                    deletedUser.FullName = model.FirstName + " " + model.LastName;
                    deletedUser.IsActive = active;
                    deletedUser.UserName = model.Email;
                    deletedUser.FirstName = model.FirstName;
                    deletedUser.LastName = model.LastName;
                    deletedUser.UserType = (active) ? type : 0;
                    userResult = deletedUser;
                    _userRepository.Update(deletedUser);
                }
                else if (active)
                {

                    var UserModel = mapper.Map<User>(model);

                    UserModel.FullName = UserModel.FirstName + " " + UserModel.LastName;
                    UserModel.IsActive = true;
                    fullName = UserModel.FullName;
                    if (!string.IsNullOrWhiteSpace(model.Image))
                    {
                        var saveImage = _fileHelper.SaveImagesFromTemp(model.Image, ServiceSetting.UploadImagesPath.UserSmall, (int)ImageHelper.ImageWidth.FullHD);
                        saveImage = _fileHelper.SaveImagesFromTemp(model.Image, ServiceSetting.UploadImagesPath.UserLarge, (int)ImageHelper.ImageWidth.Vcd);
                        if (!string.IsNullOrWhiteSpace(saveImage))
                        {
                            _fileHelper.DeleteImagesFromTemp(model.Image);
                            UserModel.Image = saveImage;
                        }
                    }

                    if (!string.IsNullOrEmpty(UserModel.Password))
                    {
                        var hasher = new PasswordHasher();
                        UserModel.Password = hasher.HashPassword(UserModel.Password);
                    }
                    UserModel.UserType = type;
                    _userRepository.Add(UserModel);
                    userResult = UserModel;

                    newUser = true;
                }



                var result = new EditCustomerUserResultDto
                {
                    NewUser = newUser,
                    FullName = model.FirstName + " " + model.LastName,
                    Password = model.Password
                };
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<EditCustomerUserResultDto>(null, exception);
            }
        }
        public async Task<ServiceResult<bool>> SendEmailOnAddCustomerUser(string senderName, string email, string userFullName, string password)
        {



            string faMessage = $"{senderName}  شما را به سامانه مدیریت پروژه رایبد دعوت نموده است. میتوانید پس از کلیک بر روی دکمه ورود با نام کاربری و کلمه عبور زیر وارد سایت شوید.";
            string enMessage = "";


            CommentMentionNotif model = new CommentMentionNotif(faMessage, _appSettings.ClientHost, new List<CommentNotifViaEmailDTO> { new CommentNotifViaEmailDTO { Discription = userFullName, Message = email, SendDate = DateTime.Now.ToString(), SenderName = password } }, _appSettings.CompanyName, enMessage);
            var emailRequest = new SendEmailDto
            {
                Tos = new List<string> { email },
                Body = await _viewRenderService.RenderToStringAsync("_AddCustomerUserEmailNotif", model),
                Subject = "دعوتنامه"
            };
            await _appEmailService.SendAsync(emailRequest);

            return ServiceResultFactory.CreateSuccess(true);
        }
    }
}
