using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using Raybod.SCM.DataAccess.Core;
using Raybod.SCM.DataTransferObject.User;
using Raybod.SCM.Domain.Model;
using Raybod.SCM.Utility.Security;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Raybod.SCM.Utility.FileHelper;
using Raybod.SCM.DataAccess.Extention;
using Microsoft.Extensions.Options;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Authentication;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Services.Utilitys.MailService;
using Raybod.SCM.DataTransferObject.Email;
using Raybod.SCM.DataTransferObject._PanelDocument.Communication.Comment;
using Hangfire;
using Raybod.SCM.DataTransferObject.Customer;
using Raybod.SCM.Utility.Extention;
using System.Security.Cryptography;

namespace Raybod.SCM.Services.Implementation
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITeamWorkAuthenticationService _authenticationServices;
        private readonly CompanyAppSettingsDto _appSettings;
        private readonly IAppEmailService _appEmailService;
        private readonly IContractService _contractService;
        private readonly DbSet<User> _usersRepository;
        private readonly DbSet<CompanyUser> _companyUserRepository;
        private readonly DbSet<Customer> _customerRepository;
        private readonly DbSet<Consultant> _consultantRepository;
        private readonly DbSet<PlanService> _planServiceRepository;
        private readonly DbSet<TeamWork> _teamWorkRepository;
        private readonly DbSet<TeamWorkUser> _teamWorkUsersRepository;
        private readonly Raybod.SCM.Services.Utilitys.FileHelper _fileHelper;
        private readonly IViewRenderService _viewRenderService;
        public UserService(IUnitOfWork unitOfWork, IWebHostEnvironment hostingEnvironmentRoot,
            IOptions<CompanyAppSettingsDto> appSettings,
            ITeamWorkAuthenticationService teamWorkAuthenticationService, IViewRenderService viewRenderService, IAppEmailService appEmailService, IContractService contractService)
        {
            _unitOfWork = unitOfWork;
            _appSettings = appSettings.Value;
            _authenticationServices = teamWorkAuthenticationService;
            _usersRepository = _unitOfWork.Set<User>();
            _teamWorkRepository = _unitOfWork.Set<TeamWork>();
            _companyUserRepository = _unitOfWork.Set<CompanyUser>();
            _customerRepository = _unitOfWork.Set<Customer>();
            _consultantRepository = _unitOfWork.Set<Consultant>();
            _teamWorkUsersRepository = _unitOfWork.Set<TeamWorkUser>();
            _planServiceRepository = _unitOfWork.Set<PlanService>();
            _fileHelper = new Utilitys.FileHelper(hostingEnvironmentRoot);
            _viewRenderService = viewRenderService;
            _appEmailService = appEmailService;
            _contractService = contractService;
        }

        public async Task<ServiceResult<ListUserDto>> AddUserAsync(AuthenticateDto authenticate, AddUserDto model, int type)
        {
            try
            {
                model.UserType = type;
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<ListUserDto>(null, MessageId.AccessDenied);

                if (!string.IsNullOrWhiteSpace(model.Email) && Regex.IsMatch(model.Email, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$") == false)
                    return ServiceResultFactory.CreateError<ListUserDto>(null, MessageId.EmailNotCorrect);

                if (await _usersRepository.AnyAsync(a => !a.IsDeleted && a.Email == model.Email))
                    return ServiceResultFactory.CreateError<ListUserDto>(null, MessageId.EmailExist);

                if (await _companyUserRepository.AnyAsync(a => !a.IsDeleted && a.Email == model.Email))
                    return ServiceResultFactory.CreateError<ListUserDto>(null, MessageId.EmailExist);

                
                if (string.IsNullOrEmpty(model.UserName) || string.IsNullOrEmpty(model.Password))
                    return ServiceResultFactory.CreateError<ListUserDto>(null, MessageId.UserNameOrPasswordNull);

                if (await _usersRepository.AnyAsync(a => !a.IsDeleted && a.UserName == model.UserName))
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
                var user = await _usersRepository.FirstOrDefaultAsync(u => (u.IsDeleted && u.UserName == model.UserName) || (u.IsDeleted && u.Email == model.Email));
                //if (user != null && (model.UserType == (int)UserStatus.ConsultantUser || model.UserType == (int)UserStatus.CustomerUser))
                //{
                //    var hasher = new PasswordHasher();
                //    user.IsDeleted = false;
                //    user.UserType = model.UserType;
                //    user.FullName = model.FirstName + " " + model.LastName;
                //    user.FirstName = model.FirstName;
                //    user.LastName = model.LastName;
                //    user.UserName = model.Email;
                //    user.Password = hasher.HashPassword(model.Password) ;
                //    userResult = user;
                //}
                 if (user != null&&(model.UserType == (int)UserStatus.OrganizationUser || model.UserType == (int)UserStatus.SupperUser))
                {
                    user.IsDeleted = false;
                    user.UserType = model.UserType;
                    userResult = user;
                }
                else if (user != null && ((model.UserType == (int)UserStatus.ConsultantUser || model.UserType == (int)UserStatus.CustomerUser)&&user.UserType != (int)UserStatus.OrganizationUser && user.UserType != (int)UserStatus.SupperUser)&& !_teamWorkUsersRepository.Any(a=>a.UserId==user.Id))
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
                    _usersRepository.Add(UserModel);
                    userResult = UserModel;
                }
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    if (type == (int)UserStatus.CustomerUser||type==(int)UserStatus.ConsultantUser)
                    {
                        try
                        {
                            BackgroundJob.Enqueue(() => SendEmailOnAddCustomerUser(authenticate.UserFullName, model.Email, fullName, model.Password));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.StackTrace);
                        }

                    }
                    var result = mapper.Map<ListUserDto>(userResult);
                    return ServiceResultFactory.CreateSuccess(result);
                }
                return ServiceResultFactory.CreateError<ListUserDto>(null, MessageId.SaveFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<ListUserDto>(null, exception);
            }
        }
       

        public async Task<ServiceResult<ListUserDto>> AddUserForCustomerAsync(AuthenticateDto authenticate, AddUserDto model, int type, bool active)
        {
            try
            {
                model.UserType = type;
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<ListUserDto>(null, MessageId.AccessDenied);

                if (!string.IsNullOrWhiteSpace(model.Email) && Regex.IsMatch(model.Email, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$") == false)
                    return ServiceResultFactory.CreateError<ListUserDto>(null, MessageId.EmailNotCorrect);



                if (string.IsNullOrEmpty(model.UserName) || string.IsNullOrEmpty(model.Password))
                    return ServiceResultFactory.CreateError<ListUserDto>(null, MessageId.UserNameOrPasswordNull);


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
                var user = await _usersRepository.FirstOrDefaultAsync(u =>!u.IsDeleted&& (u.UserName == model.UserName) || (u.Email == model.Email));
                var deletedUser = await _usersRepository.FirstOrDefaultAsync(u => u.IsDeleted && (u.UserName == model.UserName) || (u.Email == model.Email)&&(u.UserType!=(int)UserStatus.OrganizationUser&&u.UserType!=(int)UserStatus.SupperUser)&&!_teamWorkUsersRepository.Any(t=>t.UserId==u.Id));
                if (user!=null&&(user.UserType==(int)UserStatus.OrganizationUser||user.UserType==(int)UserStatus.SupperUser))
                    return ServiceResultFactory.CreateError<ListUserDto>(null, MessageId.DuplicateInformation);
                if (user != null)
                {
                    user.IsActive = active;
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.FullName = model.FirstName + " " + model.LastName;
                    user.UserType = (active) ? type : 0;
                    userResult = user;
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
                    _usersRepository.Add(UserModel);
                    userResult = UserModel;

                    newUser = true;
                }
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    if (newUser)
                    {
                        try
                        {
                            BackgroundJob.Enqueue(() => SendEmailOnAddCustomerUser(authenticate.UserFullName, model.Email, fullName, model.Password));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.StackTrace);
                        }

                    }

                    var result = mapper.Map<ListUserDto>(userResult);
                    return ServiceResultFactory.CreateSuccess(result);


                }
                else if (!active && user == null)
                {
                    return ServiceResultFactory.CreateSuccess(new ListUserDto());
                }
                return ServiceResultFactory.CreateError<ListUserDto>(null, MessageId.SaveFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<ListUserDto>(null, exception);
            }
        }
        public async Task<ServiceResult<bool>> SendEmailOnAddCustomerUser(string senderName, string email, string userFullName, string password)
        {



            string faMessage = $"{senderName}  شما را به سامانه مدیریت پروژه رایبد دعوت نموده است. میتوانید پس از کلیک بر روی دکمه ورود با نام کاربری و کلمه عبور زیر وارد سایت شوید.";
            string enMessage = $"<div style:'direction:ltr;text-align:left'>{senderName} invited you to Raybod Project Management System. You can use the user name and password in below for enter to application</div>.";


            CommentMentionNotif model = new CommentMentionNotif(faMessage, _appSettings.ClientHost, new List<CommentNotifViaEmailDTO> { new CommentNotifViaEmailDTO { Discription = userFullName, Message = email, SendDate = DateTime.Now.ToString(), SenderName = password } }, _appSettings.CompanyName,enMessage);
            var emailRequest = new SendEmailDto
            {
                Tos = new List<string> { email },
                Body = await _viewRenderService.RenderToStringAsync("_AddCustomerUserEmailNotif", model),
                Subject = "دعوتنامه"
            };
            await _appEmailService.SendAsync(emailRequest);

            return ServiceResultFactory.CreateSuccess(true);
        }
        public async Task<ServiceResult<bool>> IsUserCustomerAccess(AuthenticateDto authenticate)
        {

            var customer = await _customerRepository.Include(a => a.CustomerUsers).FirstOrDefaultAsync(a => a.CustomerContracts.Any(b => b.ContractCode == authenticate.ContractCode));
            if (customer != null)
            {
                if (customer.CustomerUsers.Any(a => a.Email == authenticate.UserName))
                {
                    var teamwork = await _teamWorkRepository.Include(a => a.TeamWorkUsers).FirstOrDefaultAsync(a => a.ContractCode == authenticate.ContractCode);
                    if (teamwork.TeamWorkUsers.Any(a => a.UserId == authenticate.UserId))
                    {
                        return ServiceResultFactory.CreateSuccess(true);
                    }

                }
            }
            var consultant = await _consultantRepository.Include(a => a.ConsultantUsers).FirstOrDefaultAsync(a => a.ConsultantContracts.Any(b => b.ContractCode == authenticate.ContractCode));
            if (consultant != null)
            {
                if (consultant.ConsultantUsers.Any(a => a.Email == authenticate.UserName))
                {
                    var teamwork = await _teamWorkRepository.Include(a => a.TeamWorkUsers).FirstOrDefaultAsync(a => a.ContractCode == authenticate.ContractCode);
                    if (teamwork.TeamWorkUsers.Any(a => a.UserId == authenticate.UserId))
                    {
                        return ServiceResultFactory.CreateSuccess(true);
                    }

                }
            }
            return ServiceResultFactory.CreateSuccess(false);


        }

        public async Task<ServiceResult<bool>> IsUserCustomerOrSupperUserAccess(AuthenticateDto authenticate)
        {

            var customer = await _customerRepository.Include(a => a.CustomerUsers).FirstOrDefaultAsync(a => a.CustomerContracts.Any(b => b.ContractCode == authenticate.ContractCode));
            if (customer != null)
            {
                if (customer.CustomerUsers.Any(a => a.Email == authenticate.UserName))
                {
                    var teamwork = await _teamWorkRepository.Include(a => a.TeamWorkUsers).FirstOrDefaultAsync(a => a.ContractCode == authenticate.ContractCode);
                    if (teamwork.TeamWorkUsers.Any(a => a.UserId == authenticate.UserId))
                    {
                        return ServiceResultFactory.CreateSuccess(true);
                    }

                }
            }
            var consultant = await _consultantRepository.Include(a => a.ConsultantUsers).FirstOrDefaultAsync(a => a.ConsultantContracts.Any(b => b.ContractCode == authenticate.ContractCode));
            if (consultant != null)
            {
                if (consultant.ConsultantUsers.Any(a => a.Email == authenticate.UserName))
                {
                    var teamwork = await _teamWorkRepository.Include(a => a.TeamWorkUsers).FirstOrDefaultAsync(a => a.ContractCode == authenticate.ContractCode);
                    if (teamwork.TeamWorkUsers.Any(a => a.UserId == authenticate.UserId))
                    {
                        return ServiceResultFactory.CreateSuccess(true);
                    }

                }
            }
            var superUser = await _usersRepository.FindAsync(authenticate.UserId);

            if (superUser != null && superUser.UserType == (int)UserStatus.SupperUser)
                return ServiceResultFactory.CreateSuccess(true);

            return ServiceResultFactory.CreateSuccess(false);


        }
        public async Task<ServiceResult<bool>> EdiUserAsync(AuthenticateDto authenticate, int userId, EditUserDto model)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var selectedUser = await _usersRepository.FirstOrDefaultAsync(a => !a.IsDeleted && a.Id == userId);
                if (selectedUser == null)
                    return ServiceResultFactory.CreateError(false, MessageId.UserNotExist);


                if (await _usersRepository.AnyAsync(s => !s.IsDeleted && s.Email == model.Email && s.Id != userId))
                    return ServiceResultFactory.CreateError(false, MessageId.EmailExist);


                if (string.IsNullOrEmpty(model.UserName))
                    return ServiceResultFactory.CreateError(false, MessageId.UserNameOrPasswordNull);

                if (await _usersRepository.AnyAsync(s => !s.IsDeleted && s.UserName == model.UserName && s.Id != userId))
                    return ServiceResultFactory.CreateError(false, MessageId.UserNameExist);


                selectedUser.LastName = model.LastName;
                selectedUser.Email = model.Email;
                selectedUser.Mobile = model.Mobile;
                selectedUser.Telephone = model.Telephone;
                selectedUser.UserName = model.UserName;
                selectedUser.FirstName = model.FirstName;
                selectedUser.FullName = model.FirstName + " " + model.LastName; ;

                if (!string.IsNullOrWhiteSpace(model.Image) && _fileHelper.ImageExistInTemp(model.Image))
                {
                    var saveImage = _fileHelper.SaveImagesFromTemp(model.Image, ServiceSetting.UploadImagesPath.UserLarge, (int)ImageHelper.ImageWidth.FullHD);
                    saveImage = _fileHelper.SaveImagesFromTemp(model.Image, ServiceSetting.UploadImagesPath.UserSmall, (int)ImageHelper.ImageWidth.Vcd);
                    _fileHelper.DeleteImagesFromTemp(model.Image);
                    selectedUser.Image = saveImage ?? selectedUser.Image;
                }
                else
                {
                    selectedUser.Image = selectedUser.Image;
                }

                if (!string.IsNullOrEmpty(model.Pass))
                {
                    var hasher = new PasswordHasher();
                    selectedUser.Password = hasher.HashPassword(model.Pass);
                }

                await _unitOfWork.SaveChangesAsync();
                return ServiceResultFactory.CreateSuccess(true);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);

            }
        }

        //public async Task<ServiceResult<List<ListUserDto>>> GetAllUserAsync()
        //{
        //    var messages = new List<ServiceMessage>();
        //    try
        //    {
        //        var queryable = _usersRepository.Where(a => !a.IsDeleted);

        //        var count = await queryable.CountAsync();
        //        //queryable = queryable.ApplyAllOrderBy(orderProperties).DeferredPaginate(offset, limit);
        //        var list = await queryable.ToListAsync();
        //        var mapperConfiguration = new MapperConfiguration(configuration =>
        //        {
        //            configuration.CreateMap<User, ListUserDto>()
        //            .ForMember(u => u.Image, m => m.MapFrom(c => _appSettings.ElasticHost + ServiceSetting.UploadImagesPath.UserSmall + c.Image))
        //            .ForMember(u => u.Signature, m => m.MapFrom(c => _appSettings.AdminHost + ServiceSetting.UploadImagesPath.UserSignatureLarge + c.Signature));
        //        });
        //        var mapper = mapperConfiguration.CreateMapper();
        //        var resultsList = mapper.Map<List<ListUserDto>>(list);
        //        messages.Add(new ServiceMessage(MessageType.Succeed, MessageId.Succeeded));
        //        return new ServiceResult<List<ListUserDto>>(true, resultsList ?? new List<ListUserDto>(), messages);
        //    }
        //    catch (Exception exception)
        //    {
        //        messages.Add(new ServiceMessage(MessageType.Error, MessageId.Exception));
        //        return new ServiceResult<List<ListUserDto>>(false, null, messages, exception);
        //    }
        //}

        public async Task<ServiceResult<List<UserMentionDto>>> GetUserMiniInfoAsync(AuthenticateDto authenticate, UserQueryDto query)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<UserMentionDto>>(null, MessageId.AccessDenied);

                var dbQuery = _usersRepository
                    .AsNoTracking()
                    .OrderByDescending(x => x.Id).Where(a => !a.IsDeleted);

                if (!string.IsNullOrWhiteSpace(query.SearchText))
                {
                    dbQuery = dbQuery.Where(a =>
                    a.FirstName.Contains(query.SearchText) ||
                    a.LastName.Contains(query.SearchText) ||
                    a.UserName.Contains(query.SearchText));
                }

                var count = dbQuery.Count();

                var result = await dbQuery.Select(c => new UserMentionDto
                {
                    Id = c.Id,
                    Display = c.FullName,
                    //UserName = c.UserName,
                    Image = c.Image == null ? "" : _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + c.Image
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(count);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<UserMentionDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<List<UserMentionDto>>> GetUserMiniInfoWithoutAuthenticationAsync(UserQueryDto query)
        {
            try
            {
                //var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                //if (!permission.HasPermission)
                //    return ServiceResultFactory.CreateError<List<UserMentionDto>>(null, MessageId.AccessDenied);

                var dbQuery = _usersRepository
                    .AsNoTracking()
                    .OrderByDescending(x => x.Id).Where(a => !a.IsDeleted && a.IsActive&&(a.UserType==(int)UserStatus.OrganizationUser||a.UserType== (int)UserStatus.SupperUser));

                if (!string.IsNullOrWhiteSpace(query.SearchText))
                {
                    dbQuery = dbQuery.Where(a =>
                    a.FirstName.Contains(query.SearchText) ||
                    a.LastName.Contains(query.SearchText) ||
                    a.UserName.Contains(query.SearchText));
                }

                var count = dbQuery.Count();

                var result = await dbQuery.Select(c => new UserMentionDto
                {
                    Id = c.Id,
                    Display = c.FullName,
                    //UserName = c.UserName,
                    Image = c.Image == null ? "" : _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + c.Image,
                    UserType=c.UserType
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(count);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<UserMentionDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<List<ListUserDto>>> GetUserAsync(AuthenticateDto authenticate, UserQueryDto query, int type = (int)UserStatus.OrganizationUser)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<ListUserDto>>(null, MessageId.AccessDenied);

                var dbQuery = _usersRepository
                    .AsNoTracking()
                    .OrderByDescending(x => x.Id).Where(a => !a.IsDeleted && (a.UserType == type||a.UserType==(int)UserStatus.SupperUser));

                if (!string.IsNullOrWhiteSpace(query.SearchText))
                {
                    dbQuery = dbQuery.Where(a =>
                    a.FirstName.Contains(query.SearchText) ||
                    a.LastName.Contains(query.SearchText) ||
                    a.UserName.Contains(query.SearchText));
                }

                var count = await dbQuery.CountAsync();
                var list = await dbQuery
                    .ApplayPageing(query)
                    .ToListAsync();
                var mapperConfiguration = new MapperConfiguration(configuration =>
                {
                    configuration.CreateMap<User, ListUserDto>()
                    .ForMember(u => u.userId, m => m.MapFrom(c => c.Id))
                    .ForMember(u => u.Image, m => m.MapFrom(c => c.Image == null ? "" : _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + c.Image));
                });
                var mapper = mapperConfiguration.CreateMapper();
                var resultsList = mapper.Map<List<ListUserDto>>(list);

                return ServiceResultFactory.CreateSuccess(resultsList).WithTotalCount(count);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<ListUserDto>>(null, exception);
            }
        }

        
        public async Task<ServiceResult<List<ListNotOrgenizationUser>>> GetUserForCustomerUsersAsync(AuthenticateDto authenticate, UserQueryDto query, List<int> type )
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<ListNotOrgenizationUser>>(null, MessageId.AccessDenied);
                //var customer = await _customerRepository.Include(a => a.CustomerUsers).FirstOrDefaultAsync(a => a.CustomerContracts.Any(b => b.ContractCode == authenticate.ContractCode));
                var dbQuery = _usersRepository
                    .AsNoTracking()
                    .OrderByDescending(x => x.Id).Where(a => !a.IsDeleted);
                if (type != null && type.Any())
                    dbQuery = dbQuery.Where(a => type.Contains(a.UserType));
                
                dbQuery = dbQuery.Where(a => (_customerRepository.Any(b => b.CustomerContracts.Any(c => c.ContractCode == authenticate.ContractCode) && b.CustomerUsers.Any(d =>!d.IsDeleted && d.Email == a.Email)))|| (_consultantRepository.Any(b => b.ConsultantContracts.Any(c => c.ContractCode == authenticate.ContractCode) && b.ConsultantUsers.Any(d => !d.IsDeleted&& d.Email == a.Email))));
                if (!string.IsNullOrWhiteSpace(query.SearchText))
                {
                    dbQuery = dbQuery.Where(a =>
                    a.FirstName.Contains(query.SearchText) ||
                    a.LastName.Contains(query.SearchText) ||
                    a.UserName.Contains(query.SearchText));
                }

                var count = await dbQuery.CountAsync();
                var list = await dbQuery
                    .ApplayPageing(query)
                    .Select(a=>new ListNotOrgenizationUser 
                    {
                        userId=a.Id,
                        Email=a.Email,
                        Image=(a.Image==null)?"": _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + a.Image,
                        FirstName=a.FirstName,
                        IsActive=a.IsActive,
                        LastName=a.LastName,
                        Mobile=a.Mobile,
                        Telephone=a.Telephone,
                        UserName=a.UserName,
                        UserType=a.UserType,
                        Company= (a.UserType == (int)UserStatus.CustomerUser) ? _companyUserRepository.Where(b => b.Email == a.Email).Select(d => new MiniCompanyInfoDto { CompanyId = d.Customer.Id, CompanyName = d.Customer.Name }).FirstOrDefault() : (a.UserType == (int)UserStatus.ConsultantUser) ? _companyUserRepository.Where(b => b.Email == a.Email).Select(d => new MiniCompanyInfoDto { CompanyId = d.Consultant.Id, CompanyName = d.Consultant.Name }).FirstOrDefault() : null,
                    })
                    .ToListAsync();
               

                return ServiceResultFactory.CreateSuccess(list).WithTotalCount(count);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<ListNotOrgenizationUser>>(null, exception);
            }
        }
        //public async Task<ServiceResult<List<ListUserDto>>> GetUserAsync(SearchUserDto query, int offset, int limit, string orderProperties)
        //{
        //    var messages = new List<ServiceMessage>();
        //    try
        //    {
        //        var queryable = _usersRepository.OrderByDescending(x => x.Id).Where(a => !a.IsDeleted);
        //        if (query != null)
        //        {
        //            if (!string.IsNullOrWhiteSpace(query.Mobile))
        //            {
        //                queryable = queryable.Where(a => a.Mobile.Contains(query.Mobile));
        //            }
        //            if (!string.IsNullOrWhiteSpace(query.FirstName))
        //            {
        //                queryable = queryable.Where(a => a.FirstName.Contains(query.FirstName));
        //            }
        //            if (!string.IsNullOrWhiteSpace(query.LastName))
        //            {
        //                queryable = queryable.Where(a => a.LastName.Contains(query.LastName));
        //            }
        //            if (!string.IsNullOrWhiteSpace(query.UserName))
        //            {
        //                queryable = queryable.Where(a => a.UserName.Contains(query.UserName));
        //            }
        //            if (!string.IsNullOrWhiteSpace(query.Email))
        //            {
        //                queryable = queryable.Where(a => a.Email.Contains(query.Email));
        //            }
        //        }
        //        var count = await queryable.CountAsync();
        //        // queryable = queryable.ApplyAllOrderBy(orderProperties).DeferredPaginate(offset, limit);
        //        var list = await queryable.ApplayPageing(offset, limit).ToListAsync();
        //        var mapperConfiguration = new MapperConfiguration(configuration =>
        //        {
        //            configuration.CreateMap<User, ListUserDto>()
        //            .ForMember(u => u.Image, m => m.MapFrom(c => _appSettings.ElasticHost + ServiceSetting.UploadImagesPath.UserSmall + c.Image))
        //            .ForMember(u => u.Signature, m => m.MapFrom(c => _appSettings.AdminHost + ServiceSetting.UploadImagesPath.UserSignatureLarge + c.Signature));
        //        });
        //        var mapper = mapperConfiguration.CreateMapper();
        //        var resultsList = mapper.Map<List<ListUserDto>>(list);
        //        messages.Add(new ServiceMessage(MessageType.Succeed, MessageId.Succeeded));
        //        var pageCount = (int)Math.Ceiling(count / (double)limit);
        //        return new ServiceResult<List<ListUserDto>>(true, resultsList ?? new List<ListUserDto>(), messages, null, pageCount);
        //    }
        //    catch (Exception exception)
        //    {
        //        messages.Add(new ServiceMessage(MessageType.Error, MessageId.Exception));
        //        return new ServiceResult<List<ListUserDto>>(false, null, messages, exception);
        //    }
        //}

        //public async Task<ServiceResult<ListUserDto>> GetUserByIdAsync(int UserId)
        //{
        //    var messages = new List<ServiceMessage>();
        //    try
        //    {
        //        var User = await _usersRepository.FirstOrDefaultAsync(a => a.Id == UserId);
        //        var mapperConfiguration = new MapperConfiguration(configuration =>
        //        {
        //            configuration.CreateMap<User, ListUserDto>()
        //            .ForMember(u => u.Image, m => m.MapFrom(c => _appSettings.ElasticHost + ServiceSetting.UploadImagesPath.UserSmall + c.Image));
        //        });
        //        var mapper = mapperConfiguration.CreateMapper();
        //        var resultsList = mapper.Map<ListUserDto>(User);
        //        messages.Add(new ServiceMessage(MessageType.Succeed, MessageId.Succeeded));
        //        return new ServiceResult<ListUserDto>(true, resultsList, messages);
        //    }
        //    catch (Exception exception)
        //    {
        //        messages.Add(new ServiceMessage(MessageType.Error, MessageId.Exception));
        //        return new ServiceResult<ListUserDto>(false, null, messages, exception);
        //    }
        //}

        //public async Task<ServiceResult<EditUserDto>> GetUserByIdForEditAsync(int UserId)
        //{
        //    var messages = new List<ServiceMessage>();
        //    try
        //    {
        //        var User = await _usersRepository.FirstOrDefaultAsync(a => a.Id == UserId);
        //        var mapperConfiguration = new MapperConfiguration(configuration =>
        //        {
        //            configuration.CreateMap<User, EditUserDto>()
        //            .ForMember(u => u.Image, m => m.MapFrom(c => _appSettings.ElasticHost + ServiceSetting.UploadImagesPath.UserSmall + c.Image))
        //            .ForMember(u => u.Signature, m => m.MapFrom(c => _appSettings.AdminHost + ServiceSetting.UploadImagesPath.UserSignatureLarge + c.Signature));
        //        });
        //        var mapper = mapperConfiguration.CreateMapper();
        //        var resultsList = mapper.Map<EditUserDto>(User);
        //        messages.Add(new ServiceMessage(MessageType.Succeed, MessageId.Succeeded));
        //        return new ServiceResult<EditUserDto>(true, resultsList, messages);
        //    }
        //    catch (Exception exception)
        //    {
        //        messages.Add(new ServiceMessage(MessageType.Error, MessageId.Exception));
        //        return new ServiceResult<EditUserDto>(false, null, messages, exception);
        //    }
        //}

        public async Task<ServiceResult<bool>> DeleteUserAsync(AuthenticateDto authenticate, int userId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var userModel = await _usersRepository.FirstOrDefaultAsync(a => !a.IsDeleted && a.Id == userId);
                if (userModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.UserNotExist);

                userModel.IsDeleted = true;
                userModel.IsActive = false;

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    return ServiceResultFactory.CreateSuccess(true);
                }
                return ServiceResultFactory.CreateError(false, MessageId.DeleteEntityFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<bool>> ChangePasswordAsync(int userId, UserChangePasswordDto model)
        {
            try
            {
                //var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                //if (!permission.HasPermission)
                //    return ServiceResultFactory.CreateError<ListDocumentGroupDto>(null, MessageId.AccessDenied);

                var User = await _usersRepository.FirstOrDefaultAsync(a => !a.IsDeleted && a.IsActive && a.Id == userId);
                if (User == null)
                    return ServiceResultFactory.CreateError(false, MessageId.UserNotExist);


                var hasher = new PasswordHasher();
                var currentPassword = hasher.HashPassword(model.CurrentPassword);
                if (User.Password != currentPassword)
                    return ServiceResultFactory.CreateError(false, MessageId.OldPasswordNotCorect);

                User.Password = hasher.HashPassword(model.NewPassword);
                await _unitOfWork.SaveChangesAsync();
                return ServiceResultFactory.CreateSuccess(true);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }
        public async Task<ServiceResult<bool>> ResetPassworAsync( UserResetPasswordDto model)
        {
            try
            {
                
                var User = await _usersRepository.FirstOrDefaultAsync(a => !a.IsDeleted && a.IsActive && a.UserName==model.UserName&&a.RefreshToken==model.Code);
                if (User == null)
                    return ServiceResultFactory.CreateError(false, MessageId.UserNotExist);


                var hasher = new PasswordHasher();

                User.RefreshToken = GenerateRefreshToken();
                User.Password = hasher.HashPassword(model.NewPassword);
                if(await _unitOfWork.SaveChangesAsync()>0)
                    return ServiceResultFactory.CreateSuccess(true);
                return ServiceResultFactory.CreateError(false, MessageId.RecoveryPasswordFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        
        public async Task<ServiceResult<bool>> ActivatedUserAsync(AuthenticateDto authenticate, int UserId)
        {
            var messages = new List<ServiceMessage>();
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);


                var User = await _usersRepository.FirstOrDefaultAsync(a => !a.IsDeleted && a.Id == UserId);
                if (User == null)
                {
                    messages.Add(new ServiceMessage(MessageType.Error, MessageId.EntityDoesNotExist));
                    return new ServiceResult<bool>(false, false, messages);
                }
                User.IsActive = !User.IsActive;
                await _unitOfWork.SaveChangesAsync();
                messages.Add(new ServiceMessage(MessageType.Succeed, MessageId.Succeeded));
                return new ServiceResult<bool>(true, true, messages);
            }
            catch (Exception exception)
            {
                messages.Add(new ServiceMessage(MessageType.Error, MessageId.Exception));
                return new ServiceResult<bool>(false, false, messages, exception);
            }
        }

        public async Task<ServiceResult<ListUserDto>> SigninAsync(SigningDto model)
        {
            var messages = new List<ServiceMessage>();
            try
            {
                var hasher = new PasswordHasher();
                model.Password = hasher.HashPassword(model.Password);
                var User = await _usersRepository
                    .FirstOrDefaultAsync(u => !u.IsDeleted && u.IsActive && u.UserName == model.UserName && u.Password == model.Password);
                if (User == null)
                {
                    messages.Add(new ServiceMessage(MessageType.Error, MessageId.SigninFailed));
                    return new ServiceResult<ListUserDto>(false, null, messages);
                }
                var mapperConfiguration = new MapperConfiguration(configuration =>
                {
                    configuration.CreateMap<User, ListUserDto>();
                });
                var mapper = mapperConfiguration.CreateMapper();
                var UserResult = mapper.Map<User, ListUserDto>(User);
                messages.Add(new ServiceMessage(MessageType.Succeed, MessageId.Succeeded));
                return new ServiceResult<ListUserDto>(true, UserResult, messages);
            }
            catch (Exception exception)
            {
                messages.Add(new ServiceMessage(MessageType.Error, MessageId.Exception));
                return new ServiceResult<ListUserDto>(false, null, messages, exception);
            }
        }

        public async Task<ServiceResult<UserInfoApiDto>> SigninWithApiAsync(SigningApiDto model)
        {
            try
            {
                var hasher = new PasswordHasher();
                model.Password = hasher.HashPassword(model.Password);
                
                var userModel = await _usersRepository
                    .Where(u => !u.IsDeleted && u.IsActive && u.UserName == model.UserName && u.Password == model.Password && u.UserType > 0)
                    .Select(a => new UserInfoApiDto
                    {
                        Id = a.Id,
                        Email = a.Email,
                        LastName = a.LastName,
                        Telephone = a.Telephone,
                        UserName = a.UserName,
                        Mobile = a.Mobile,
                        FirstName = a.FirstName,
                        Customer = (a.UserType==(int)UserStatus.ConsultantUser)? _consultantRepository.Where(c => c.ConsultantUsers.Any(d => !d.IsDeleted && d.Email == a.Email)).ToList().GetBaseCustomerDto(): _customerRepository.Where(c => c.CustomerUsers.Any(d => !d.IsDeleted && d.Email == a.Email)).ToList().GetBaseCustomerDto(),
                        Image = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + a.Image,

                        UserType = a.UserType,
                        CompanyNameFA=_appSettings.CompanyNameFA,
                        CompanyNameEN=_appSettings.CompanyNameEN,
                        PowerBIRoot=_appSettings.PowerBIRoot,
                        CompanyLogo=_appSettings.WepApiHost + _appSettings.CompanyLogoFront
                    }).FirstOrDefaultAsync();
                
                if (userModel == null&& await _usersRepository.AnyAsync(u => !u.IsDeleted && u.IsActive && u.UserName == model.UserName  && u.UserType > 0))
                    return ServiceResultFactory.CreateError(new UserInfoApiDto(), MessageId.UserNameOrPasswordNotCorrect);
                if (userModel == null)
                    return ServiceResultFactory.CreateError(new UserInfoApiDto(), MessageId.SigninFailed);
                var result = await _authenticationServices.GetUserPermissionByUserIdAsync(userModel.Id);
                if (!result.Succeeded)
                    return ServiceResultFactory.CreateError(new UserInfoApiDto(), MessageId.SigninFailed);
                userModel.TeamWorks = result.Result;
                var planService= await _planServiceRepository.OrderByDescending(a => a.CreatedDate).FirstOrDefaultAsync();
                
                if (planService != null)
                {
                    if (planService.FileDrive)
                        userModel.PlanService.Add("FileDriveService");
                    if (planService.DocumentManagement)
                        userModel.PlanService.Add("DocumentMngService");
                    if (planService.PurchaseManagement)
                        userModel.PlanService.Add("PurchasingMngService");
                    if (planService.ConstructionManagement)
                        userModel.PlanService.Add("OperationMngService");
                }
                return ServiceResultFactory.CreateSuccess(userModel);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<UserInfoApiDto>(null, exception);
            }
        }
        public async Task<ServiceResult<UserInfoApiDto>> UserInfoInCheckAuthentication(AuthenticateDto authenticate)
        {
            try
            {


                var userModel = await _usersRepository
                    .Where(u => !u.IsDeleted && u.IsActive && u.UserName == authenticate.UserName&&  u.UserType > 0)
                    .Select(a => new UserInfoApiDto
                    {
                        Id = a.Id,
                        Email = a.Email,
                        LastName = a.LastName,
                        Telephone = a.Telephone,
                        UserName = a.UserName,
                        Mobile = a.Mobile,
                        FirstName = a.FirstName,
                        Customer = (a.UserType == (int)UserStatus.ConsultantUser) ? _consultantRepository.Where(c => c.ConsultantUsers.Any(d => !d.IsDeleted && d.Email == a.Email)).ToList().GetBaseCustomerDto() : _customerRepository.Where(c => c.CustomerUsers.Any(d => !d.IsDeleted && d.Email == a.Email)).ToList().GetBaseCustomerDto(),
                        Image = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + a.Image,
                        LatestTeamworkIds = a.UserLatestTeamWorks.Select(c => c.TeamWorkId).ToList(),
                        UserType = a.UserType,
                        CompanyNameFA = _appSettings.CompanyNameFA,
                        CompanyNameEN = _appSettings.CompanyNameEN,
                        PowerBIRoot = _appSettings.PowerBIRoot,
                        CompanyLogo = _appSettings.WepApiHost + _appSettings.CompanyLogoFront
                    }).FirstOrDefaultAsync();

                if (userModel == null && await _usersRepository.AnyAsync(u => !u.IsDeleted && u.IsActive && u.UserName == authenticate.UserName && u.UserType > 0))
                    return ServiceResultFactory.CreateError(new UserInfoApiDto(), MessageId.UserNameOrPasswordNotCorrect);
                if (userModel == null)
                    return ServiceResultFactory.CreateError(new UserInfoApiDto(), MessageId.SigninFailed);
                var result = await _authenticationServices.GetUserPermissionByUserIdAsync(userModel.Id);
                if (!result.Succeeded)
                    return ServiceResultFactory.CreateError(new UserInfoApiDto(), MessageId.SigninFailed);
                userModel.TeamWorks = result.Result;
                var planService = await _planServiceRepository.OrderByDescending(a => a.CreatedDate).FirstOrDefaultAsync();

                if (planService != null)
                {
                    if (planService.FileDrive)
                        userModel.PlanService.Add("FileDriveService");
                    if (planService.DocumentManagement)
                        userModel.PlanService.Add("DocumentMngService");
                    if (planService.PurchaseManagement)
                        userModel.PlanService.Add("PurchasingMngService");
                    if (planService.ConstructionManagement)
                        userModel.PlanService.Add("OperationMngService");
                }
                return ServiceResultFactory.CreateSuccess(userModel);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<UserInfoApiDto>(null, exception);
            }
        }

        public async Task<ServiceResult<bool>> CheckAndSetRefreshTokenAsync(string username, string refreshToken, string newRefreshToken)
        {
            try
            {
                var userModel = await _usersRepository
                    .Where(x => x.IsActive && !x.IsDeleted && x.UserName == username && x.RefreshToken == refreshToken && x.DateExpireRefreshToken > DateTime.UtcNow)
                    .FirstOrDefaultAsync();

                if (userModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.TokenNotValid);

                userModel.RefreshToken = newRefreshToken;

                return await _unitOfWork.SaveChangesAsync() > 0
                     ? ServiceResultFactory.CreateSuccess(true)
                     : ServiceResultFactory.CreateError(false, MessageId.TokenNotValid);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }
    
        public ServiceResult<string> SetRefreshToken(string username, string refreshToken, bool isRememberMe)
        {
            try
            {
                var user = _usersRepository
                    .Where(x => x.IsActive && !x.IsDeleted && x.UserName == username)
                    .FirstOrDefault();

                user.RefreshToken = refreshToken;
                user.DateExpireRefreshToken = isRememberMe ? DateTime.UtcNow.AddDays(15) : DateTime.UtcNow.AddDays(1);
                return _unitOfWork.SaveChange() > 0
                    ? ServiceResultFactory.CreateSuccess(refreshToken)
                    : ServiceResultFactory.CreateError(string.Empty, MessageId.InternalError);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(string.Empty, exception);
            }
        }

        private string GetUserModulePermission(List<string> rolemodulePermissionList)
        {
            string modulePermision = string.Empty;
            string moduleSectionPermision = string.Empty;
            foreach (var item in rolemodulePermissionList)
            {
                var temp = item.Split(",").ToList();
                string temp1 = temp[0].Trim();
                string temp2 = temp[1].Trim();

                if (!string.IsNullOrEmpty(temp1))
                    modulePermision += temp1;

                if (!string.IsNullOrEmpty(temp2))
                    moduleSectionPermision += temp2;
            }
            return $"{modulePermision},{moduleSectionPermision}";
        }

        //public async Task<ServiceResult<List<SearchUserDto>>> GetUserBySearchQuery(string text)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(text))
        //            return ServiceResultFactory.CreateSuccess(new List<SearchUserDto>());
        //        var query = await _usersRepository
        //            .Where(x => !x.IsDeleted && x.IsActive && (x.FirstName.Contains(text) || x.LastName.Contains(text) || x.UserName.Contains(text) || x.PersonalCode.Contains(text))).ToListAsync();
        //        if (query == null)
        //            return ServiceResultFactory.CreateSuccess(new List<SearchUserDto>());

        //        var mapperConfiguration = new MapperConfiguration(configuration =>
        //         {
        //             configuration.CreateMap<User, SearchUserDto>()
        //             .ForMember(u => u.Image, m => m.MapFrom(c => _appSettings.ElasticHost + ServiceSetting.UploadImagesPath.UserSmall + c.Image));
        //         });
        //        var mapper = mapperConfiguration.CreateMapper();
        //        var userlist = mapper.Map<List<SearchUserDto>>(query);
        //        return ServiceResultFactory.CreateSuccess(userlist);

        //    }
        //    catch (Exception exception)
        //    {
        //        return ServiceResultFactory.CreateException<List<SearchUserDto>>(null, exception);
        //    }
        //}


        //public async Task<ServiceResult<List<SearchUserDto>>> GetUserForSelectPosition(string query)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(query))
        //            return ServiceResultFactory.CreateSuccess(new List<SearchUserDto>());
        //        var userQuery = await _usersRepository
        //            .Where(x => !x.IsDeleted && x.IsActive && (x.Position == null || x.Position.IsDeleted == true) && (x.FirstName.Contains(query) || x.LastName.Contains(query) || x.UserName.Contains(query) || x.PersonalCode.Contains(query))).ToListAsync();
        //        if (userQuery == null)
        //            return ServiceResultFactory.CreateSuccess(new List<SearchUserDto>());

        //        var mapperConfiguration = new MapperConfiguration(configuration =>
        //        {
        //            configuration.CreateMap<User, SearchUserDto>()
        //            .ForMember(u => u.Image, m => m.MapFrom(c => _appSettings.ElasticHost + ServiceSetting.UploadImagesPath.UserSmall + c.Image));
        //            configuration.CreateMap<SearchUserDto, User>();
        //        });
        //        var mapper = mapperConfiguration.CreateMapper();
        //        var userlist = mapper.Map<List<SearchUserDto>>(userQuery);
        //        return ServiceResultFactory.CreateSuccess(userlist);

        //    }
        //    catch (Exception exception)
        //    {
        //        return ServiceResultFactory.CreateException<List<SearchUserDto>>(null, exception);
        //    }
        //}

        //public async Task<ServiceResult<bool>> AddUserPositionAsync(int userId, int positionId)
        //{
        //    try
        //    {
        //        var userModel = await _usersRepository.FindAsync(userId);
        //        if (userModel == null)
        //            return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);
        //        userModel.PositionId = positionId;
        //        return await _unitOfWork.SaveChangesAsync() > 0
        //            ? ServiceResultFactory.CreateSuccess(true)
        //            : ServiceResultFactory.CreateError(false, MessageId.SaveFailed);
        //    }
        //    catch (Exception exception)
        //    {
        //        return ServiceResultFactory.CreateException(false, exception);
        //    }
        //}

        //public async Task<ServiceResult<bool>> DeleteUserPositionAsync(int userId, int positionId)
        //{
        //    try
        //    {
        //        var userModel = await _usersRepository.FindAsync(userId);
        //        if (userModel == null)
        //            return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);
        //        if (userModel.PositionId == positionId)
        //        {
        //            userModel.PositionId = null;
        //        }
        //        else { return ServiceResultFactory.CreateError(false, MessageId.UnsuccessfulDelete); }

        //        return await _unitOfWork.SaveChangesAsync() > 0
        //            ? ServiceResultFactory.CreateSuccess(true)
        //            : ServiceResultFactory.CreateError(false, MessageId.UnsuccessfulDelete);
        //    }
        //    catch (Exception exception)
        //    {
        //        return ServiceResultFactory.CreateException(false, exception);
        //    }
        //}
        public async Task<ServiceResult<bool>> ForgetPassword(ForgetPasswordModel model,string lang)
        {
            try
            {


                var dbQuery = await _usersRepository.Where(a => !a.IsDeleted && a.UserName == model.UserName && a.IsActive).FirstOrDefaultAsync();

                if (dbQuery == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);
                dbQuery.RefreshToken = GenerateRefreshToken();
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    try
                    {
                        BackgroundJob.Enqueue(() => SendEmailOnUserForgetPassword(dbQuery.UserName, dbQuery.Email, dbQuery.FullName, dbQuery.RefreshToken,lang));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.StackTrace);
                    }
                    return ServiceResultFactory.CreateSuccess(true);
                }
                return ServiceResultFactory.CreateError(false, MessageId.OperationFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }
        public async Task<ServiceResult<bool>> ValidatePasswordRecoveryRequest(ValidateRecoveryPasswordDto model)
        {
            try
            {


                var dbQuery = await _usersRepository.Where(a => !a.IsDeleted && a.UserName == model.UserName && a.IsActive).FirstOrDefaultAsync();

                if (dbQuery == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);
               if(dbQuery.RefreshToken!=model.Code)
                    return ServiceResultFactory.CreateError(false, MessageId.RecoveryLinkExpired);

                return ServiceResultFactory.CreateSuccess(true);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }
        public async Task<ServiceResult<bool>> SendEmailOnUserForgetPassword(string userName, string email, string userFullName, string refreshToken,string lang)
        {



            string faMessage = $"برای بازیابی کلمه عبور خود بر روی لینک زیر کلیک کنید";
            string enMessage = "To recover your password please click on the button below";

            string linkUrl = _appSettings.ClientHost + $"/passwordRecovery?userName={userName}&code={refreshToken}";
            CommentMentionNotif model = new CommentMentionNotif(faMessage, linkUrl, new List<CommentNotifViaEmailDTO> { new CommentNotifViaEmailDTO { Discription = userFullName, Message = email, SendDate = DateTime.Now.ToString(), SenderName = lang } }, _appSettings.CompanyName, enMessage);
            var emailRequest = new SendEmailDto
            {
                Tos = new List<string> { email },
                Body = await _viewRenderService.RenderToStringAsync("_ForgetPasswordEmail", model),
                Subject = "Password Recovery"
            };
            await _appEmailService.SendAsync(emailRequest);

            return ServiceResultFactory.CreateSuccess(true);
        }
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                string base64string = Convert.ToBase64String(randomNumber);
                return base64string.Replace("+", "");
            }
        }
        public async Task<ServiceResult<List<BaseUserTeamWorkDto>>> GetUserTeamWork(AuthenticateDto authenticate, int userId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<BaseUserTeamWorkDto>>(null, MessageId.AccessDenied);




                var result = await _usersRepository.Where(a => !a.IsDeleted && a.IsActive && a.Id == userId)
             .Select(c => new UserInfoApiDto
             {
                 LatestTeamworkIds = null,
                 TeamWorks = c.TeamWorkUsers.Where(a => !a.TeamWork.IsDeleted)
                 .Select(v => new BaseUserTeamWorkDto
                 {

                     TeamWorkCode = v.TeamWork.ContractCode,
                     TeamWorkId = v.TeamWorkId,
                     Title = v.TeamWork.Title
                 }).ToList()
             }).FirstOrDefaultAsync();

                var count = result.TeamWorks.Count();

                if (result == null)
                    return ServiceResultFactory.CreateError<List<BaseUserTeamWorkDto>>(null, MessageId.EntityDoesNotExist);


                return ServiceResultFactory.CreateSuccess(result.TeamWorks).WithTotalCount(count);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<BaseUserTeamWorkDto>>(null, exception);
            }
        }


    }
}