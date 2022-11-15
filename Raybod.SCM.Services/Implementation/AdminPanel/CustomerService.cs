using AutoMapper;
using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Raybod.SCM.DataAccess.Core;
using Raybod.SCM.DataAccess.Extention;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject._admin.Customer;
using Raybod.SCM.DataTransferObject._PanelDocument.Communication.Comment;
using Raybod.SCM.DataTransferObject.Consultant;
using Raybod.SCM.DataTransferObject.Customer;
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
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Implementation
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITeamWorkAuthenticationService _authenticationService;
        private readonly DbSet<Customer> _customerRepository;
        private readonly DbSet<Consultant> _consultantRepository;
        private readonly DbSet<TeamWorkUser> _teamWorkUsersRepository;
        private readonly DbSet<User> _userRepository;
        private readonly DbSet<CompanyUser> _companyUserRepository;
        private readonly DbSet<Contract> _contractRepository;
        private readonly Raybod.SCM.Services.Utilitys.FileHelper _fileHelper;
        private readonly CompanyConfig _appSettings;
        private readonly IViewRenderService _viewRenderService;
        private readonly IAppEmailService _appEmailService;

        public CustomerService(
            IUnitOfWork unitOfWork,
            IWebHostEnvironment hostingEnvironmentRoot,
            ITeamWorkAuthenticationService authenticationService,
            IHttpContextAccessor httpContextAccessor,
            IOptions<CompanyAppSettingsDto> appSettings, IViewRenderService viewRenderService, IAppEmailService appEmailService)
        {
            _unitOfWork = unitOfWork;
            _authenticationService = authenticationService;
            _customerRepository = _unitOfWork.Set<Customer>();
            _consultantRepository = _unitOfWork.Set<Consultant>();
            _companyUserRepository = _unitOfWork.Set<CompanyUser>();
            _userRepository = _unitOfWork.Set<User>();
            _contractRepository = _unitOfWork.Set<Contract>();
            _fileHelper = new Utilitys.FileHelper(hostingEnvironmentRoot);
            _viewRenderService = viewRenderService;
            _appEmailService = appEmailService;
            _teamWorkUsersRepository = _unitOfWork.Set<TeamWorkUser>();
            httpContextAccessor.HttpContext.Request.Headers.TryGetValue("companyCode", out var CompanyCode);
            _appSettings = appSettings.Value.CompanyConfig.First(a => a.CompanyCode == CompanyCode);
        }

        public async Task<ServiceResult<BaseCustomerDto>> AddCustomerAsync(AuthenticateDto authenticate, AddCustomerDto model)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<BaseCustomerDto>(null, MessageId.AccessDenied);

                if (!string.IsNullOrEmpty(model.Email) && !RegexHelpers.IsValidEmail(model.Email))
                    return ServiceResultFactory.CreateError<BaseCustomerDto>(null, MessageId.EmailNotCorrect);

                if (await _customerRepository.AnyAsync(x => !x.IsDeleted && x.CustomerCode == model.CustomerCode))
                    return ServiceResultFactory.CreateError(new BaseCustomerDto(), MessageId.CodeExist);

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

                var customerModel = new Customer
                {
                    Name = model.Name,
                    CustomerCode = model.CustomerCode,
                    Address = model.Address,
                    Email = model.Email,
                    Fax = model.Fax,
                    Logo = model.Logo,
                    PostalCode = model.PostalCode,
                    TellPhone = model.TellPhone,
                    Website = model.Website
                };
                _customerRepository.Add(customerModel);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var result = new BaseCustomerDto
                    {
                        Id = customerModel.Id,
                        Address = customerModel.Address,
                        Email = customerModel.Email,
                        Fax = customerModel.Fax,
                        Logo = customerModel.Logo,
                        Name = customerModel.Name,
                        CustomerCode = customerModel.CustomerCode,
                        PostalCode = customerModel.PostalCode,
                        TellPhone = customerModel.TellPhone,
                        Website = customerModel.Website
                    };

                    return ServiceResultFactory.CreateSuccess(result);
                }

                return ServiceResultFactory.CreateError(new BaseCustomerDto(), MessageId.SaveFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new BaseCustomerDto(), exception);
            }
        }

        public async Task<ServiceResult<bool>> EditCustomerAsync(AuthenticateDto authenticate, int customerId, AddCustomerDto model)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);


                if (!string.IsNullOrEmpty(model.Email) && !RegexHelpers.IsValidEmail(model.Email))
                    return ServiceResultFactory.CreateError(false, MessageId.EmailNotCorrect);

                var selectedCustomer = await _customerRepository.FirstOrDefaultAsync(a => !a.IsDeleted && a.Id == customerId);
                if (selectedCustomer == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (await _customerRepository.AnyAsync(x => !x.IsDeleted && x.Id != customerId && x.CustomerCode == model.CustomerCode))
                    return ServiceResultFactory.CreateError(false, MessageId.CodeExist);

                selectedCustomer.Address = model.Address;
                selectedCustomer.CustomerCode = model.CustomerCode;
                selectedCustomer.Email = model.Email;
                selectedCustomer.Fax = model.Fax;
                selectedCustomer.Name = model.Name;
                selectedCustomer.PostalCode = model.PostalCode;
                selectedCustomer.TellPhone = model.TellPhone;
                selectedCustomer.Website = model.Website;

                if (!string.IsNullOrWhiteSpace(model.Logo) && _fileHelper.ImageExistInTemp(model.Logo))
                {
                    var saveImage = _fileHelper.SaveImagesFromTemp(model.Logo, ServiceSetting.UploadImagesPath.LogoLarge, (int)ImageHelper.ImageWidth.FullHD);
                    saveImage = _fileHelper.SaveImagesFromTemp(model.Logo, ServiceSetting.UploadImagesPath.LogoSmall, (int)ImageHelper.ImageWidth.Vcd);
                    if (!string.IsNullOrWhiteSpace(saveImage))
                    {
                        _fileHelper.DeleteImagesFromTemp(model.Logo);
                        selectedCustomer.Logo = saveImage ?? selectedCustomer.Logo;
                    }
                }
                else
                {
                    selectedCustomer.Logo = selectedCustomer.Logo;
                }

                await _unitOfWork.SaveChangesAsync();
                return ServiceResultFactory.CreateSuccess(true);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<List<BaseCustomerDto>>> GetCustomerAsync(AuthenticateDto authenticate, CustomerQuery query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<BaseCustomerDto>>(null, MessageId.AccessDenied);

                var dbQuery = _customerRepository
                    .AsNoTracking()
                    .OrderByDescending(a => a.Id)
                    .Where(x => !x.IsDeleted);

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(x => x.Name.Contains(query.SearchText) || x.CustomerCode.Contains(query.SearchText));

                var totalCount = dbQuery.Count();
                dbQuery = dbQuery.ApplayPageing(query);

                var result = await dbQuery.Select(c => new BaseCustomerDto
                {
                    Id = c.Id,
                    Address = c.Address,
                    CustomerCode = c.CustomerCode,
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
                return ServiceResultFactory.CreateException(new List<BaseCustomerDto>(), exception);
            }
        }

        public async Task<ServiceResult<List<CustomerMiniInfoDto>>> GetCustomerMiniInfoAsync(AuthenticateDto authenticate, CustomerQuery query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<CustomerMiniInfoDto>>(null, MessageId.AccessDenied);

                var dbQuery = _customerRepository
                   .AsNoTracking()
                   .OrderByDescending(a => a.Id)
                   .Where(x => !x.IsDeleted);

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(x => x.Name.Contains(query.SearchText) || x.CustomerCode.Contains(x.CustomerCode));

                var totalCount = dbQuery.Count();
                dbQuery = dbQuery.ApplayPageing(query);

                var result = await dbQuery.Select(c => new CustomerMiniInfoDto
                {
                    Id = c.Id,
                    Logo = c.Logo != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.LogoSmall + c.Logo : "",
                    CustomerCode = c.CustomerCode,
                    Name = c.Name,
                }).ToListAsync();

                //_appSettings.AdminHost + ServiceSetting.UploadImagesPath.LogoSmall + c.Logo :

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new List<CustomerMiniInfoDto>(), exception);
            }
        }




        public async Task<ServiceResult<List<CustomerMiniInfoForCommentDto>>> GetCustomerMiniInfoWithoutPageingAsync(AuthenticateDto authenticate, CustomerQuery query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<CustomerMiniInfoForCommentDto>>(null, MessageId.AccessDenied);

                var dbCustomer = _customerRepository
                   .AsNoTracking()
                   .OrderByDescending(a => a.Id)
                   .Where(x => !x.IsDeleted && (x.CustomerContracts.Any(b => b.ContractCode == authenticate.ContractCode)));

                var dbConsultant = _consultantRepository
                   .AsNoTracking()
                   .OrderByDescending(a => a.Id)
                   .Where(x => !x.IsDeleted && (x.ConsultantContracts.Any(b => b.ContractCode == authenticate.ContractCode)));

                if (!string.IsNullOrEmpty(query.SearchText))
                {

                    dbConsultant = dbConsultant.Where(x => x.Name.Contains(query.SearchText) || x.ConsultantCode.Contains(x.ConsultantCode));
                    dbCustomer = dbCustomer.Where(x => x.Name.Contains(query.SearchText) || x.CustomerCode.Contains(x.CustomerCode));
                }

                List<CustomerMiniInfoForCommentDto> result = new List<CustomerMiniInfoForCommentDto>();
                var totalCount = dbCustomer.Count();
                if (dbConsultant != null && dbConsultant.Any())
                {
                    var consultant = await dbConsultant.Select(c => new CustomerMiniInfoForCommentDto
                    {
                        Id = c.Id,
                        CompanyIssue = CompanyIssue.Consultant,
                        Logo = c.Logo != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.LogoSmall + c.Logo : "",
                        CustomerCode = c.ConsultantCode,
                        Name = c.Name,
                    }).FirstOrDefaultAsync();
                    result.Add(consultant);
                }
                var Custoemr = await dbCustomer.Select(c => new CustomerMiniInfoForCommentDto
                {
                    Id = c.Id,
                    CompanyIssue = CompanyIssue.Customer,
                    Logo = c.Logo != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.LogoSmall + c.Logo : "",
                    CustomerCode = c.CustomerCode,
                    Name = c.Name,
                }).FirstOrDefaultAsync();
                result.Add(Custoemr);
                //_appSettings.AdminHost + ServiceSetting.UploadImagesPath.LogoSmall + c.Logo :

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new List<CustomerMiniInfoForCommentDto>(), exception);
            }
        }


        public async Task<ServiceResult<BaseCustomerDto>> GetCustomerByIdAsync(AuthenticateDto authenticate, int customerId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<BaseCustomerDto>(null, MessageId.AccessDenied);

                var customerModel = await _customerRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.Id == customerId)
                    .Select(c => new BaseCustomerDto
                    {
                        Id = c.Id,
                        Address = c.Address,
                        CustomerCode = c.CustomerCode,
                        Email = c.Email,
                        Fax = c.Fax,
                        Logo = c.Logo,
                        Name = c.Name,
                        PostalCode = c.PostalCode,
                        TellPhone = c.TellPhone,
                        Website = c.Website
                    }).FirstOrDefaultAsync();

                if (customerModel == null)
                    return ServiceResultFactory.CreateError<BaseCustomerDto>(null, MessageId.EntityDoesNotExist);

                return ServiceResultFactory.CreateSuccess(customerModel);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new BaseCustomerDto(), exception);
            }
        }

        public async Task<ServiceResult<bool>> DeleteCustomerAsync(AuthenticateDto authenticate, int customerid)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var customerModel = await _customerRepository
                    .FirstOrDefaultAsync(a => !a.IsDeleted && a.Id == customerid);

                if (customerModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (await _contractRepository.AnyAsync(a => !a.IsDeleted && a.CustomerId == customerid))
                    return ServiceResultFactory.CreateError(false, MessageId.DeleteDontAllowedBeforeSubset);
                if (await _userRepository.AnyAsync(a => !a.IsDeleted && _customerRepository.Any(b => b.CustomerUsers.Any(c => c.Email == a.Email))))
                    return ServiceResultFactory.CreateError(false, MessageId.DeleteDontAllowedBeforeSubset);
                customerModel.IsDeleted = true;
                return await _unitOfWork.SaveChangesAsync() > 0
                    ? ServiceResultFactory.CreateSuccess(true)
                    : ServiceResultFactory.CreateError(false, MessageId.SaveFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<BaseCustomerUserDto>> AddCustomerUserAsync(AuthenticateDto authenticate, int customerId, AddCustomerUserDto model, bool? isUserSystem)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<BaseCustomerUserDto>(null, MessageId.AccessDenied);

                if (!await _customerRepository.AnyAsync(a => !a.IsDeleted && a.Id == customerId))
                    return ServiceResultFactory.CreateError<BaseCustomerUserDto>(null, MessageId.CustomerNotFound);

                if (await _companyUserRepository.AnyAsync(a => !a.IsDeleted && a.Email == model.Email))
                    return ServiceResultFactory.CreateError<BaseCustomerUserDto>(null, MessageId.EmailExist);

                if (await _userRepository.AnyAsync(a => !a.IsDeleted && a.Email == model.Email) && isUserSystem != null && isUserSystem == true)
                    return ServiceResultFactory.CreateError<BaseCustomerUserDto>(null, MessageId.EmailExist);

                var newCustomerUserModel = new CompanyUser
                {
                    CustomerId = customerId,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName
                };
                _companyUserRepository.Add(newCustomerUserModel);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var res = new BaseCustomerUserDto
                    {
                        CustomerUserId = newCustomerUserModel.CompanyUserId,
                        Email = model.Email,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        UserType = _userRepository.Any(u => u.Email == model.Email) ? _userRepository.First(u => u.Email == model.Email).UserType : 0,
                    };
                    return ServiceResultFactory.CreateSuccess(res);
                }
                return ServiceResultFactory.CreateError<BaseCustomerUserDto>(null, MessageId.SaveFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<BaseCustomerUserDto>(null, exception);
            }
        }

        public async Task<ServiceResult<BaseCustomerUserDto>> AddCustomerUserAsync(AuthenticateDto authenticate, int customerId, AddCustomerUserDto model, bool? isUserSystem, AddUserDto userModel, UserStatus type)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<BaseCustomerUserDto>(null, MessageId.AccessDenied);

                if (!await _customerRepository.AnyAsync(a => !a.IsDeleted && a.Id == customerId))
                    return ServiceResultFactory.CreateError<BaseCustomerUserDto>(null, MessageId.CustomerNotFound);

                if (await _companyUserRepository.AnyAsync(a => !a.IsDeleted && a.Email == model.Email))
                    return ServiceResultFactory.CreateError<BaseCustomerUserDto>(null, MessageId.EmailExist);

                if (await _userRepository.AnyAsync(a => !a.IsDeleted && a.Email == model.Email) && isUserSystem != null && isUserSystem == true)
                    return ServiceResultFactory.CreateError<BaseCustomerUserDto>(null, MessageId.EmailExist);

                var newCustomerUserModel = new CompanyUser
                {
                    CustomerId = customerId,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName
                };
                var userResult = await AddUserAsync(userModel, (int)type);
                if (!userResult.Succeeded)
                    return ServiceResultFactory.CreateError<BaseCustomerUserDto>(null, userResult.Messages[0].Message);
                _companyUserRepository.Add(newCustomerUserModel);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var res = new BaseCustomerUserDto
                    {
                        CustomerUserId = newCustomerUserModel.CompanyUserId,
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
                return ServiceResultFactory.CreateError<BaseCustomerUserDto>(null, MessageId.SaveFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<BaseCustomerUserDto>(null, exception);
            }
        }
        public async Task<ServiceResult<BaseCustomerUserDto>> EditCustomerUserAsync(AuthenticateDto authenticate, int customerId, EditCustomerUserDto model, int companyUserId,AddUserDto userModel,UserStatus type,bool active)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<BaseCustomerUserDto>(null, MessageId.AccessDenied);

                if (!await _customerRepository.AnyAsync(a => !a.IsDeleted && a.Id == customerId))
                    return ServiceResultFactory.CreateError<BaseCustomerUserDto>(null, MessageId.CustomerNotFound);

                if (!await _companyUserRepository.AnyAsync(a => !a.IsDeleted && a.CompanyUserId == companyUserId))
                    return ServiceResultFactory.CreateError<BaseCustomerUserDto>(null, MessageId.CompanyUserNotFound);

                var newCustomerUserModel = await _companyUserRepository.FirstAsync(a => !a.IsDeleted && a.CompanyUserId == companyUserId);
                newCustomerUserModel.FirstName = model.FirstName;
                newCustomerUserModel.LastName = model.LastName;
                newCustomerUserModel.UpdateDate = DateTime.Now;
                var userResult = await AddUserForCustomerAsync(userModel, (int)type, active);
                if(!userResult.Succeeded)
                    return ServiceResultFactory.CreateError<BaseCustomerUserDto>(null, userResult.Messages[0].Message);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var res = new BaseCustomerUserDto
                    {
                        CustomerUserId = newCustomerUserModel.CompanyUserId,
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
                return ServiceResultFactory.CreateError<BaseCustomerUserDto>(null, MessageId.SaveFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<BaseCustomerUserDto>(null, exception);
            }
        }
        public async Task<ServiceResult<List<BaseCustomerUserDto>>> GetCustomerUserAsync(AuthenticateDto authenticate, int customerId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<BaseCustomerUserDto>>(null, MessageId.AccessDenied);

                var dbQuery = _companyUserRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.CustomerId == customerId);

                var result = await dbQuery.Select(c => new BaseCustomerUserDto
                {
                    CustomerUserId = c.CompanyUserId,
                    Email = c.Email,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    UserType = (_userRepository.Any(a => a.Email == c.Email)) ? _userRepository.FirstOrDefault(a => a.Email == c.Email).UserType : 0
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<BaseCustomerUserDto>>(null, exception);
            }
        }
        
        public async Task<ServiceResult<bool>> DeleteCustomerUserByIdAsync(AuthenticateDto authenticate, int customerId, int customerUserId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var userModel = await _companyUserRepository.FirstOrDefaultAsync(a => !a.IsDeleted && a.CustomerId == customerId && a.CompanyUserId == customerUserId);
                if (userModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);
                var user = await _userRepository.FirstOrDefaultAsync(u => !u.IsDeleted && u.Email == userModel.Email);
                if (user != null)
                {
                    if (await _teamWorkUsersRepository.AnyAsync(a => a.UserId == user.Id))
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

        private async Task<ServiceResult<EditCustomerUserResultDto>> AddUserForCustomerAsync( AddUserDto model, int type, bool active)
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
                var user = await _userRepository.FirstOrDefaultAsync(u => !u.IsDeleted &&  (u.Email == model.Email));
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
