using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Raybod.SCM.DataAccess.Core;
using Raybod.SCM.DataAccess.Extention;
using Raybod.SCM.DataTransferObject.ContractAttachment;
using Raybod.SCM.DataTransferObject.Contract;
using Raybod.SCM.Domain.Model;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Utility.Extention;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Raybod.SCM.Utility.Common;
using Microsoft.AspNetCore.Hosting;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.DataTransferObject;
using Microsoft.Extensions.Options;
using Raybod.SCM.DataTransferObject.Customer;
using Raybod.SCM.DataTransferObject.Address;
using Raybod.SCM.DataTransferObject.Audit;
using Raybod.SCM.Domain.Struct;
using Raybod.SCM.DataTransferObject.Notification;
using Raybod.SCM.DataTransferObject.Bom;
using Raybod.SCM.DataTransferObject.Product;
using Microsoft.AspNetCore.Http;
using Exon.TheWeb.Service.Core;
using Raybod.SCM.DataTransferObject.Consultant;
using Raybod.SCM.DataTransferObject._PanelSale.Contract;
using Raybod.SCM.DataTransferObject.Authentication;
using Raybod.SCM.DataTransferObject.User;
using Raybod.SCM.Services.Utilitys;

namespace Raybod.SCM.Services.Implementation
{
    public class ContractService : IContractService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITeamWorkAuthenticationService _authenticationService;
        private readonly ISCMLogAndNotificationService _scmLogAndNotificationService;
        private readonly IFileService _fileService;
        private readonly ITeamWorkAuthenticationService _authenticationServices;
        private readonly DbSet<Contract> _contractRepository;
        private readonly DbSet<ContractFormConfig> _contractFormConfigRepository;
        private readonly DbSet<TeamWorkUser> _teamworkUserRepository;
        private readonly DbSet<UserLatestTeamWork> _latestTeamworkRepository;
        private readonly DbSet<TeamWorkUserRole> _teamworkUserRoleRepository;
        private readonly DbSet<ContractAttachment> _contractAttachmentRepository;
        private readonly DbSet<User> _usersRepository;
        private readonly DbSet<Consultant> _consultantRepository;
        private readonly DbSet<TeamWork> _teamWorkRepository;
        private readonly DbSet<SCMAuditLog> _scmAuditLogRepository;
        private readonly DbSet<Transmittal> _transmittalRepository;
        private readonly DbSet<Customer> _customerRepository;
        private readonly DbSet<PlanService> _planServiceRepository;
        private readonly FileHelper _fileHelper;
        private readonly CompanyConfig _appSettings;

        public ContractService(
            IUnitOfWork unitOfWork,
            ITeamWorkAuthenticationService authenticationService,
            IWebHostEnvironment hostingEnvironmentRoot,
            IOptions<CompanyAppSettingsDto> appSettings,
            ISCMLogAndNotificationService scmLogAndNotificationService,
            IBomProductService bomProductService,
            IMasterMrService masterMrService,
            IHttpContextAccessor httpContextAccessor,
            IFileService fileService, ITeamWorkAuthenticationService authenticationServices)
        {
            _authenticationService = authenticationService;
            _fileService = fileService;
            _unitOfWork = unitOfWork;
            _scmLogAndNotificationService = scmLogAndNotificationService;
            _contractRepository = _unitOfWork.Set<Domain.Model.Contract>();
            _contractFormConfigRepository = _unitOfWork.Set<ContractFormConfig>();
            _contractAttachmentRepository = _unitOfWork.Set<ContractAttachment>();
            _latestTeamworkRepository = _unitOfWork.Set<UserLatestTeamWork>();
            _consultantRepository = _unitOfWork.Set<Consultant>();
            _teamworkUserRepository = _unitOfWork.Set<TeamWorkUser>();
            _usersRepository = _unitOfWork.Set<User>();
            _planServiceRepository = _unitOfWork.Set<PlanService>();
            _teamworkUserRoleRepository = _unitOfWork.Set<TeamWorkUserRole>();
            _teamWorkRepository = _unitOfWork.Set<TeamWork>();
            _scmAuditLogRepository = _unitOfWork.Set<SCMAuditLog>();
            _customerRepository = _unitOfWork.Set<Customer>();
            _contractAttachmentRepository = _unitOfWork.Set<ContractAttachment>();
            _transmittalRepository = _unitOfWork.Set<Transmittal>();
            _fileHelper = new FileHelper(hostingEnvironmentRoot);
            _authenticationServices = authenticationServices;
            httpContextAccessor.HttpContext.Request.Headers.TryGetValue("companyCode", out var CompanyCode);
            _appSettings = appSettings.Value.CompanyConfig.First(a => a.CompanyCode == CompanyCode);
        }

        public async Task<ServiceResult<UserInfoApiDto>> AddContractAsync(AuthenticateDto authenticate, InsertContractDto model)
        {
            try
            {

                var permissionResult = _authenticationService.GetAccessableContract(authenticate.UserId, authenticate.Roles);
                if (!permissionResult.HasPermisson)
                    return ServiceResultFactory.CreateError<UserInfoApiDto>(null, MessageId.AccessDenied);

                if (await _contractRepository.AnyAsync(a => !a.IsDeleted && a.ContractCode.ToLower() == model.ContractCode.ToLower()))
                    return ServiceResultFactory.CreateError<UserInfoApiDto>(null, MessageId.DuplicatContractCode);

                if (!ValidateContractCodeForNonPersianCharachter(model.ContractCode))
                    return ServiceResultFactory.CreateError<UserInfoApiDto>(null, MessageId.ContractCodeHasPersianCharachter);

                if (!ValidateContractCode(model.ContractCode))
                    return ServiceResultFactory.CreateError<UserInfoApiDto>(null, MessageId.ContractCodeCharacterInvalid);

                if (model.Services == null || !model.Services.Any())
                    return ServiceResultFactory.CreateError<UserInfoApiDto>(null, MessageId.ServicesCannotBeEmpty);

                var plan = await _planServiceRepository.OrderByDescending(a => a.CreatedDate).FirstOrDefaultAsync();

                if (plan == null)
                    return ServiceResultFactory.CreateError<UserInfoApiDto>(null, MessageId.ServiceInformationNotAvailable);
                if (plan.StartDate.Date > DateTime.Now.Date)
                    return ServiceResultFactory.CreateError<UserInfoApiDto>(null, MessageId.PlanServiceNotStarted);
                if (plan.FinishDate.Date < DateTime.Now.Date)
                    return ServiceResultFactory.CreateError<UserInfoApiDto>(null, MessageId.PlanServiceExpired);

                if (plan.DocumentManagement == false && model.Services.Contains("DocumentMngService"))
                    return ServiceResultFactory.CreateError<UserInfoApiDto>(null, MessageId.DocumentServicesNotAvailable);

                if (plan.FileDrive == false && model.Services.Contains("FileDriveService"))
                    return ServiceResultFactory.CreateError<UserInfoApiDto>(null, MessageId.FileDriveServicesNotAvailable);

                if (plan.PurchaseManagement == false && model.Services.Contains("PurchasingMngService"))
                    return ServiceResultFactory.CreateError<UserInfoApiDto>(null, MessageId.PurchaseServicesNotAvailable);

                if (plan.ConstructionManagement == false && model.Services.Contains("OperationMngService"))
                    return ServiceResultFactory.CreateError<UserInfoApiDto>(null, MessageId.ConstructionServicesNotAvailable);


                if (await _contractRepository.AnyAsync(x => !x.IsDeleted && x.ContractCode == model.ContractCode))
                {
                    return ServiceResultFactory.CreateError<UserInfoApiDto>(null, MessageId.CodeExist);
                }




                var mapperConfiguration = new MapperConfiguration(configuration =>
                {
                    configuration.CreateMap<InsertContractDto, Domain.Model.Contract>();
                });


                var mapper = mapperConfiguration.CreateMapper();
                var contractModel = mapper.Map<Domain.Model.Contract>(model);
                contractModel.ContractCode = contractModel.ContractCode.Trim();
                contractModel.ContractStatus = ContractStatus.Active;
                contractModel.Details = "";
                contractModel.ContractNumber = model.ContractCode.Trim();
                contractModel = AddServices(contractModel, model.Services);


                model.FormConfig = CreateCoding(model.ContractCode).ToList();
                var formRes = await AddContractFormConfigDto(contractModel, model.FormConfig);
                if (!formRes.Succeeded)
                    return ServiceResultFactory.CreateError<UserInfoApiDto>(null, formRes.Messages.First().Message);

                contractModel = formRes.Result;
                contractModel.ContractType = ContractType.Genuine;
                contractModel.TeamWork = new TeamWork
                {
                    ContractCode = contractModel.ContractCode,
                    Title = contractModel.Description,
                    IsDeleted = false,
                    UserNotifies = new List<UserNotify>()
                };

                List<UserNotify> addUserNotifies = new List<UserNotify>();
                var teamWorkUserRoles = await _teamworkUserRoleRepository.Include(a => a.Role).Where(a => a.ContractCode == null).ToListAsync();
                foreach (var item in teamWorkUserRoles)
                {
                    await AddUserNotify(item.Role, addUserNotifies, item.UserId);
                }

                contractModel.TeamWork.UserNotifies = addUserNotifies;
                _contractRepository.Add(contractModel);

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    await AddLogAndNotificationOnAddContractAsync(authenticate, contractModel);



                    var result = await GetUserInfoAsync(authenticate.UserId);
                    if (!result.Succeeded)
                        return ServiceResultFactory.CreateError<UserInfoApiDto>(null, MessageId.OperationFailed);

                    return ServiceResultFactory.CreateSuccess(result.Result);
                }
                return ServiceResultFactory.CreateError<UserInfoApiDto>(null, MessageId.SaveFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<UserInfoApiDto>(null, exception);
            }
        }

        private async Task<ServiceResult<Contract>> AddContractFormConfigDto(Contract contractModel, List<AddContractFormConfigDto> formConfigs)
        {
            contractModel.ContractFormConfigs = new List<ContractFormConfig>();
            foreach (FormName form in Enum.GetValues(typeof(FormName)))
            {
                var newFormConfig = new ContractFormConfig();
                var currentForm = formConfigs.FirstOrDefault(a => a.FormName == form);
                if (currentForm == null)
                    return ServiceResultFactory.CreateError(new Contract(), MessageId.NotFoundFormConfig);

                if (form == FormName.DocumentRevision)
                {
                    newFormConfig.ContractCode = contractModel.ContractCode;
                    newFormConfig.FormName = form;
                    newFormConfig.CodingType = currentForm.CodingType;
                    newFormConfig.FixedPart = currentForm.FixedPart;
                    newFormConfig.LengthOfSequence = currentForm.LengthOfSequence;

                    contractModel.ContractFormConfigs.Add(newFormConfig);
                }
                else
                {
                    if (await _contractFormConfigRepository.AnyAsync(a => a.FormName == form && a.FixedPart == currentForm.FixedPart && a.ContractCode != contractModel.ContractCode))
                        return ServiceResultFactory.CreateError(new Contract(), MessageId.DuplicateFormFixPart);
                    if (currentForm.LengthOfSequence <= 0)
                        return ServiceResultFactory.CreateError(new Contract(), MessageId.InputDataValidationError);

                    newFormConfig.ContractCode = contractModel.ContractCode;
                    newFormConfig.FormName = form;
                    newFormConfig.CodingType = CodingType.Number;
                    newFormConfig.FixedPart = currentForm.FixedPart;
                    newFormConfig.LengthOfSequence = currentForm.LengthOfSequence;

                    contractModel.ContractFormConfigs.Add(newFormConfig);
                }
            }


            return ServiceResultFactory.CreateSuccess(contractModel);
        }



        private async Task AddLogAndNotificationOnAddContractAsync(AuthenticateDto authenticate, Contract contractModel)
        {
            var loginfo = new AddAuditLogDto
            {
                ContractCode = contractModel.ContractCode,
                RootKeyValue = contractModel.ContractCode,
                FormCode = contractModel.ContractCode,
                KeyValue = contractModel.ContractCode,
                NotifEvent = NotifEvent.AddContract,
                PerformerUserId = authenticate.UserId,
                PerformerUserFullName = authenticate.UserFullName
            };

            await _scmLogAndNotificationService.AddScmAuditLogAsync(loginfo, null);




        }





        public async Task<ServiceResult<List<ListContractDto>>> GetAllContractAsync(AuthenticateDto authenticate, ContractQuery query)
        {
            try
            {
                var dbQuery = _contractRepository
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .OrderByDescending(a => a.CreatedDate)
                    .AsQueryable();

                var permissionResult = _authenticationService.GetAccessableContract(authenticate.UserId, authenticate.Roles);
                if (!permissionResult.HasPermisson)
                    return ServiceResultFactory.CreateError(new List<ListContractDto>(), MessageId.AccessDenied);
                if (!permissionResult.HasOrganizationPermission)
                    dbQuery = dbQuery.Where(a => permissionResult.ContractCodes.Contains(a.ContractCode));

                dbQuery = dbQuery.Include(a => a.Customer).AsQueryable();

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(x =>
                        x.Description.Contains(query.SearchText) ||
                        x.ContractCode.Contains(query.SearchText));

                if (query.ContractType > 0)
                    dbQuery = dbQuery.Where(x => x.ContractType == query.ContractType);

                if (query.ContractStatus != null && query.ContractStatus.Count() > 0)
                    dbQuery = dbQuery.Where(x => query.ContractStatus.Contains(x.ContractStatus));

                if (query.ContractStatus != null && query.ContractStatus.Count() > 0)
                    dbQuery = dbQuery.Where(x => query.CustomerIds.Contains(x.CustomerId.Value
                    ));

                if (query.CustomerIds != null && query.CustomerIds.Count() > 0)
                    dbQuery = dbQuery.Where(x => query.CustomerIds.Contains(x.CustomerId.Value));

                var pageCount = dbQuery.Count();

                var columnsMap = new Dictionary<string, Expression<Func<Contract, object>>>
                {
                    ["ContractCode"] = v => v.ContractCode,
                    ["Description"] = v => v.Description,
                    ["Cost"] = v => v.Cost
                };

                dbQuery = dbQuery.ApplayOrdering(query, columnsMap).ApplayPageing(query);
                var result = await dbQuery.Select(x => new ListContractDto
                {
                    ContractCode = x.ContractCode,
                    ContractNumber = x.ContractNumber,
                    Description = x.Description,
                    ContractStatus = x.ContractStatus,
                    CustomerId = x.CustomerId ?? 0,
                    CustomerName = x.Customer.Name,
                    ConsultantId = x.ConsultantId,
                    ConsultantName = (x.Consultant != null) ? x.Consultant.Name : "",
                    ContractDuration = x.ContractDuration ?? 0,
                    ContractType = x.ContractType,
                    Cost = x.Cost ?? 0,
                    CostInLetters = x.Cost != null ? x.Cost.Value.NumberToText(Language.Persian) : "0",
                    CreatedDate = x.CreatedDate.ToUnixTimestamp(),
                    DateEffective = x.DateEffective.ToUnixTimestamp(),
                    DateEnd = x.DateEnd.ToUnixTimestamp(),
                    DateIssued = x.DateIssued.ToUnixTimestamp(),
                    ParentContractCode = x.ParnetContractCode,
                    UserAudit = x.AdderUser != null
                        ? new UserAuditLogDto
                        {
                            AdderUserId = x.AdderUserId,
                            AdderUserName = x.AdderUser.FullName,
                            CreateDate = x.CreatedDate.ToUnixTimestamp(),
                            AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall +
                                             x.AdderUser.Image
                        }
                        : null,
                }).ToListAsync();
                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(pageCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new List<ListContractDto>(), exception);
            }
        }

        public async Task<ServiceResult<List<ContractMiniInfoDto>>> GetAllContractMiniInfoAsync(AuthenticateDto authenticate, ContractQuery query)
        {
            try
            {
                var dbQuery = _contractRepository
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .OrderByDescending(a => a.CreatedDate)
                    .AsQueryable();

                var permissionResult = _authenticationService.GetAccessableContract(authenticate.UserId, authenticate.Roles);
                if (!permissionResult.HasPermisson)
                    return ServiceResultFactory.CreateError(new List<ContractMiniInfoDto>(), MessageId.AccessDenied);
                if (!permissionResult.HasOrganizationPermission)
                    dbQuery = dbQuery.Where(a => permissionResult.ContractCodes.Contains(a.ContractCode));

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(x =>
                        x.Description.Contains(query.SearchText) ||
                        x.ContractCode.Contains(query.SearchText));

                if (query.ContractType > 0)
                    dbQuery = dbQuery.Where(x => x.ContractType == query.ContractType);

                if (query.ContractStatus != null && query.ContractStatus.Count() > 0)
                    dbQuery = dbQuery.Where(x => query.ContractStatus.Contains(x.ContractStatus));

                if (query.CustomerIds != null && query.CustomerIds.Count() > 0)
                    dbQuery = dbQuery.Where(x => query.CustomerIds.Contains(x.CustomerId.Value));

                var totalCount = dbQuery.Count();
                var contractList = await dbQuery.ApplayPageing(query).Select(c => new ContractMiniInfoDto
                {
                    ContractCode = c.ContractCode,
                    ContractType = c.ContractType,
                    Description = c.Description,
                    CustomerId = c.CustomerId ?? 0,
                    CustomerName = c.Customer.Name,
                    ContractNumber = c.ContractNumber
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(contractList).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new List<ContractMiniInfoDto>(), exception);
            }
        }

        public async Task<ServiceResult<List<ContractMiniInfoDto>>> GetAllContractMiniInfoAsync(AuthenticateDto authenticate, string query)
        {
            try
            {
                var dbQuery = _contractRepository
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .OrderByDescending(a => a.CreatedDate)
                    .AsQueryable();

                var permissionResult = _authenticationService.GetAccessableContract(authenticate.UserId, authenticate.Roles);
                if (!permissionResult.HasPermisson)
                    return ServiceResultFactory.CreateError(new List<ContractMiniInfoDto>(), MessageId.AccessDenied);
                if (!permissionResult.HasOrganizationPermission)
                    dbQuery = dbQuery.Where(a => permissionResult.ContractCodes.Contains(a.ContractCode));

                if (!string.IsNullOrEmpty(query))
                    dbQuery = dbQuery.Where(x =>
                        x.Description.Contains(query) ||
                        x.ContractCode.Contains(query));

                var contractList = await dbQuery.Select(c => new ContractMiniInfoDto
                {
                    ContractCode = c.ContractCode,
                    ContractType = c.ContractType,
                    Description = c.Description,
                    CustomerId = c.CustomerId ?? 0,
                    CustomerName = c.Customer.Name,
                    ContractNumber = c.ContractNumber
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(contractList);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new List<ContractMiniInfoDto>(), exception);
            }
        }

        public async Task<ServiceResult<List<MiniSearchResult>>> SearchInContractAsync(AuthenticateDto authenticate, ContractQuery query)
        {
            try
            {
                var dbQuery = _contractRepository
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .AsQueryable();

                var permissionResult = _authenticationService.GetAccessableContract(authenticate.UserId, authenticate.Roles);
                if (!permissionResult.HasPermisson)
                    return ServiceResultFactory.CreateError(new List<MiniSearchResult>(), MessageId.AccessDenied);

                if (!permissionResult.HasOrganizationPermission)
                    dbQuery = dbQuery.Where(a => permissionResult.ContractCodes.Contains(a.ContractCode));

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(x =>
                        x.Description.Contains(query.SearchText) ||
                        x.ContractCode.Contains(query.SearchText));

                if (query.ContractType > 0)
                    dbQuery = dbQuery.Where(x => x.ContractType == query.ContractType);

                if (query.ContractStatus != null && query.ContractStatus.Count() > 0)
                    dbQuery = dbQuery.Where(x => query.ContractStatus.Contains(x.ContractStatus));

                if (query.CustomerIds != null && query.CustomerIds.Count() > 0)
                    dbQuery = dbQuery.Where(x => query.CustomerIds.Contains(x.CustomerId.Value));

                var totalCount = dbQuery.Count();
                dbQuery = dbQuery.ApplayPageing(query);
                var contractList = await dbQuery.Select(c => new MiniSearchResult
                {
                    Id = 0,
                    Code = c.ContractCode,
                    Description = c.Description,
                    RefCode = c.ContractCode,
                    SearchIn = "Contract"
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(contractList).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new List<MiniSearchResult>(), exception);
            }
        }

        public async Task<ServiceResult<ListContractDto>> GetContractByIdAsync(AuthenticateDto authenticate, string contractCode)
        {
            try
            {

                if (!_authenticationService.HasPermission(authenticate.UserId,
                   contractCode,
                   authenticate.Roles))
                    return ServiceResultFactory.CreateError(new ListContractDto(), MessageId.AccessDenied);

                var query = _contractRepository
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && x.ContractCode == contractCode);

                var result = await query.Select(x => new ListContractDto
                {
                    Description = x.Description,
                    ContractStatus = x.ContractStatus,
                    ContractCode = x.ContractCode,
                    CustomerId = x.CustomerId ?? 0,
                    CustomerName = x.Customer.Name,
                    ContractDuration = x.ContractDuration ?? 0,
                    ContractType = x.ContractType,
                    Cost = x.Cost,
                    CostInLetters = x.Cost != null ? x.Cost.Value.NumberToText(Language.Persian) : "0",
                    CreatedDate = x.CreatedDate.ToUnixTimestamp(),
                    DateEffective = x.DateEffective.ToUnixTimestamp(),
                    DateEnd = x.DateEnd.ToUnixTimestamp(),
                    DateIssued = x.DateIssued.ToUnixTimestamp(),
                    ParentContractCode = x.ParnetContractCode,
                    ContractNumber = x.ContractNumber,
                    UserAudit = x.AdderUser != null
                        ? new UserAuditLogDto
                        {
                            AdderUserId = x.AdderUserId,
                            AdderUserName = x.AdderUser.FullName,
                            CreateDate = x.CreatedDate.ToUnixTimestamp(),
                            AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall +
                                             x.AdderUser.Image
                        }
                        : null,
                    CustomerInfo = x.Customer != null
                        ? new BaseCustomerDto
                        {
                            Id = x.Customer.Id,
                            Name = x.Customer.Name,
                            Address = x.Customer.Address,
                            Email = x.Customer.Email,
                            PostalCode = x.Customer.PostalCode,
                            TellPhone = x.Customer.TellPhone,
                            Website = x.Customer.Website,
                            Fax = x.Customer.Fax,
                            CustomerCode = x.Customer.CustomerCode,
                            Logo = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.LogoSmall + x.Customer.Logo
                        }
                        : new BaseCustomerDto(),
                    ConsultantInfo = x.Consultant != null ?
                    new BaseConsultantDto
                    {
                        Id = x.Consultant.Id,
                        Name = x.Consultant.Name,
                        Address = x.Consultant.Address,
                        Email = x.Consultant.Email,
                        PostalCode = x.Consultant.PostalCode,
                        TellPhone = x.Consultant.TellPhone,
                        Website = x.Consultant.Website,
                        Fax = x.Consultant.Fax,
                        ConsultantCode = x.Consultant.ConsultantCode,
                        Logo = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.LogoSmall + x.Consultant.Logo
                    } : new BaseConsultantDto(),

                }).FirstOrDefaultAsync();

                if (result == null)
                    return ServiceResultFactory.CreateError(new ListContractDto(), MessageId.EntityDoesNotExist);

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new ListContractDto(), exception);
            }
        }

        public async Task<ServiceResult<bool>> EditContractAsync(AuthenticateDto authenticate, string contractCode, EditContractDto model)
        {
            try
            {

                var selectedContract = await _contractRepository
                    .Where(x => x.ContractCode == model.ContractCode)
                    .FirstOrDefaultAsync();
                if (selectedContract == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (!_authenticationService.HasPermission(authenticate.UserId,
                selectedContract.ContractCode,
                authenticate.Roles))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                if (await _contractRepository.AnyAsync(x =>
                    !x.IsDeleted && x.ContractCode != model.ContractCode && x.ContractNumber == model.ContractNumber))
                {
                    return ServiceResultFactory.CreateError(false, MessageId.CodeExist);
                }

                if (model.DateEnd < model.DateIssued)
                    return ServiceResultFactory.CreateError(false, MessageId.EndDateMustBeBiger);

                selectedContract.ContractStatus = model.ContractStatus;
                selectedContract.Description = model.Description;
                selectedContract.ContractNumber = model.ContractNumber;
                selectedContract.ContractDuration = model.ContractDuration;
                selectedContract.DateEffective = model.DateEffective.UnixTimestampToDateTime().Date;
                selectedContract.DateEnd = model.DateEnd.UnixTimestampToDateTime().Date;
                selectedContract.DateIssued = model.DateIssued.UnixTimestampToDateTime().Date;
                selectedContract.UpdateDate = DateTime.UtcNow;

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var res = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = selectedContract.ContractCode,
                        RootKeyValue = selectedContract.ContractCode,
                        FormCode = selectedContract.ContractCode,
                        KeyValue = selectedContract.ContractCode,
                        NotifEvent = NotifEvent.EditContract,
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName
                    }, null);
                    return ServiceResultFactory.CreateSuccess(true);
                }
                return ServiceResultFactory.CreateError(false, MessageId.EditEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<BaseConsultantDto>> UpdateProjectConsultantAsync(AuthenticateDto authenticate, int consultantId)
        {
            try
            {


                var selectedContract = await _contractRepository
                    .Where(x => x.ContractCode == authenticate.ContractCode)
                    .FirstOrDefaultAsync();
                if (selectedContract == null)
                    return ServiceResultFactory.CreateError(new BaseConsultantDto(), MessageId.EntityDoesNotExist);

                if (!_authenticationService.HasPermission(authenticate.UserId, selectedContract.ContractCode, authenticate.Roles))
                    return ServiceResultFactory.CreateError(new BaseConsultantDto(), MessageId.AccessDenied);

                var consultant = await _consultantRepository.FirstOrDefaultAsync(a => a.Id == consultantId && !a.IsDeleted);

                if (consultant == null && consultantId != -1)
                    return ServiceResultFactory.CreateError(new BaseConsultantDto(), MessageId.ConsultantNotFound);

                if (_teamworkUserRepository.Any(a => a.TeamWork.Contract.ContractCode == authenticate.ContractCode && a.User != null && !a.User.IsDeleted && (a.User.UserType == (int)UserStatus.ConsultantUser)))
                    return ServiceResultFactory.CreateError(new BaseConsultantDto(), MessageId.EditDontAllowedBeforeSubset);


                if (await _transmittalRepository.AnyAsync(a => a.ContractCode == authenticate.ContractCode && a.TransmittalType == TransmittalType.Consultant))
                    return ServiceResultFactory.CreateError(new BaseConsultantDto(), MessageId.EditNotAllowAfterCreateTransmittalForConsultant);



                if (consultantId != -1)
                {
                    selectedContract.ConsultantId = consultantId;
                }
                else
                {
                    selectedContract.ConsultantId = null;
                }

                selectedContract.UpdateDate = DateTime.UtcNow;


                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    BaseConsultantDto result = new BaseConsultantDto();
                    if (consultantId != -1 && consultant != null)
                    {

                        result.Id = selectedContract.Consultant.Id;
                        result.Name = selectedContract.Consultant.Name;
                        result.Address = selectedContract.Consultant.Address;
                        result.Email = selectedContract.Consultant.Email;
                        result.PostalCode = selectedContract.Consultant.PostalCode;
                        result.TellPhone = selectedContract.Consultant.TellPhone;
                        result.Website = selectedContract.Consultant.Website;
                        result.Fax = selectedContract.Consultant.Fax;
                        result.ConsultantCode = selectedContract.Consultant.ConsultantCode;
                        result.Logo = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.LogoSmall + selectedContract.Consultant.Logo;

                    }


                    var res = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = selectedContract.ContractCode,
                        RootKeyValue = selectedContract.ContractCode,
                        FormCode = selectedContract.ContractCode,
                        KeyValue = selectedContract.ContractCode,
                        NotifEvent = NotifEvent.EditContract,
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName
                    }, null);
                    return ServiceResultFactory.CreateSuccess(result);
                }
                return ServiceResultFactory.CreateError(new BaseConsultantDto(), MessageId.EditEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<BaseConsultantDto>(null, exception);
            }
        }


        public async Task<ServiceResult<BaseCustomerDto>> UpdateProjectCustomerAsync(AuthenticateDto authenticate, int customerId)
        {
            try
            {


                var selectedContract = await _contractRepository
                    .Where(x => x.ContractCode == authenticate.ContractCode)
                    .FirstOrDefaultAsync();
                if (selectedContract == null)
                    return ServiceResultFactory.CreateError(new BaseCustomerDto(), MessageId.EntityDoesNotExist);

                if (!_authenticationService.HasPermission(authenticate.UserId, selectedContract.ContractCode, authenticate.Roles))
                    return ServiceResultFactory.CreateError(new BaseCustomerDto(), MessageId.AccessDenied);

                var customer = await _customerRepository.FirstOrDefaultAsync(a => a.Id == customerId && !a.IsDeleted);

                if (customer == null && customerId != -1)
                    return ServiceResultFactory.CreateError(new BaseCustomerDto(), MessageId.CustomerNotFound);

                if (_teamworkUserRepository.Any(a => a.TeamWork.Contract.ContractCode == authenticate.ContractCode && a.User != null && !a.User.IsDeleted && (a.User.UserType == (int)UserStatus.CustomerUser)))
                    return ServiceResultFactory.CreateError(new BaseCustomerDto(), MessageId.EditDontAllowedBeforeSubset);

                if (await _transmittalRepository.AnyAsync(a => a.ContractCode == authenticate.ContractCode && a.TransmittalType == TransmittalType.Customer))
                    return ServiceResultFactory.CreateError(new BaseCustomerDto(), MessageId.EditNotAllowAfterCreateTransmittal);






                if (customerId != -1)
                {
                    selectedContract.CustomerId = customerId;
                }
                else
                {
                    selectedContract.CustomerId = null;
                }

                selectedContract.UpdateDate = DateTime.UtcNow;


                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    BaseCustomerDto result = new BaseCustomerDto();
                    if (customerId != -1 && customer != null)
                    {

                        result.Id = selectedContract.Customer.Id;
                        result.Name = selectedContract.Customer.Name;
                        result.Address = selectedContract.Customer.Address;
                        result.Email = selectedContract.Customer.Email;
                        result.PostalCode = selectedContract.Customer.PostalCode;
                        result.TellPhone = selectedContract.Customer.TellPhone;
                        result.Website = selectedContract.Customer.Website;
                        result.Fax = selectedContract.Customer.Fax;
                        result.CustomerCode = selectedContract.Customer.CustomerCode;
                        result.Logo = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.LogoSmall + selectedContract.Customer.Logo;

                    }


                    var res = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = selectedContract.ContractCode,
                        RootKeyValue = selectedContract.ContractCode,
                        FormCode = selectedContract.ContractCode,
                        KeyValue = selectedContract.ContractCode,
                        NotifEvent = NotifEvent.EditContract,
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName
                    }, null);
                    return ServiceResultFactory.CreateSuccess(result);
                }
                return ServiceResultFactory.CreateError(new BaseCustomerDto(), MessageId.EditEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<BaseCustomerDto>(null, exception);
            }
        }

        public async Task<ServiceResult<List<BaseContractAttachmentDto>>> GetContractAttachmentByIdAsync(AuthenticateDto authenticate
           )
        {
            try
            {
                if (!_authenticationService.HasPermission(authenticate.UserId, authenticate.ContractCode))
                    return ServiceResultFactory.CreateError(new List<BaseContractAttachmentDto>(), MessageId.AccessDenied);

                var result = await _contractAttachmentRepository
                    .AsNoTracking()
                    .Where(x => x.ContractCode == authenticate.ContractCode && !x.IsDeleted)
                    .Select(a => new BaseContractAttachmentDto
                    {
                        ContractCode = a.ContractCode,
                        FileName = a.FileName,
                        FileType = a.FileType,
                        FileSize = a.FileSize,
                        FileSrc = a.FileSrc,
                        Id = a.Id,
                    }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new List<BaseContractAttachmentDto>(), exception);
            }
        }

        public async Task<DownloadFileDto> DownloadContractAttachmentAsync(AuthenticateDto authenticate, string fileSrc)
        {
            try
            {
                var attachmentModel = await _contractAttachmentRepository
                   .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.FileSrc == fileSrc)
                    .FirstOrDefaultAsync();
                if (attachmentModel == null)
                    return null;
                if (!_authenticationService.HasPermission(authenticate.UserId, attachmentModel.ContractCode))
                    return null;
                var streamResult = await _fileHelper.DownloadDocumentWithContentType(fileSrc, ServiceSetting.FileSection.ContractDocument);
                if (streamResult == null)
                    return null;
                streamResult.FileName = attachmentModel.FileName;
                return streamResult;
            }
            catch (Exception exception)
            {
                return null;
            }
        }

        public async Task<ServiceResult<bool>> RemoveAttachmentAsync(AuthenticateDto authenticate, string contractCode, string fileSrc)
        {
            try
            {
                if (!_authenticationService.HasPermission(authenticate.UserId, contractCode, authenticate.Roles))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery = _contractAttachmentRepository
                    .Where(a => a.ContractCode == contractCode && a.FileSrc == fileSrc)
                    .AsQueryable();

                var attachment = await dbQuery
                    .FirstOrDefaultAsync();

                if (attachment == null)
                    return ServiceResultFactory.CreateError<bool>(false, MessageId.EntityDoesNotExist);

                attachment.IsDeleted = true;

                return await _unitOfWork.SaveChangesAsync() > 0
                    ? ServiceResultFactory.CreateSuccess(true)
                    : ServiceResultFactory.CreateError<bool>(false, MessageId.DeleteEntityFailed);
            }
            catch (Exception e)
            {
                return ServiceResultFactory.CreateException(false, e);
            }
        }


        public async Task<ServiceResult<List<BaseContractAttachmentDto>>> AddContractAttachmentAsync(AuthenticateDto authenticate, IFormFileCollection files)
        {
            try
            {
                if (files == null || !files.Any())
                    return ServiceResultFactory.CreateError<List<BaseContractAttachmentDto>>(null, MessageId.InputDataValidationError);

                if (!_authenticationService.HasPermission(authenticate.UserId, authenticate.ContractCode, authenticate.Roles))
                    return ServiceResultFactory.CreateError(new List<BaseContractAttachmentDto>(), MessageId.AccessDenied);

                var attachModels = new List<ContractAttachment>();
                foreach (var item in files)
                {
                    var fileName = item.FileName;
                    var uploadResult = await _fileService.UploadDocumentFile(item);
                    if (!uploadResult.Succeeded)
                        return ServiceResultFactory.CreateError<List<BaseContractAttachmentDto>>(null, uploadResult.Messages[0].Message);

                    var UploadedFile = await _fileHelper
                        .SaveDocumentFromTemp(uploadResult.Result, ServiceSetting.UploadFilePath.ContractDocument);

                    if (UploadedFile == null)
                        return ServiceResultFactory.CreateError<List<BaseContractAttachmentDto>>(null, MessageId.UploudFailed);

                    _fileHelper.DeleteDocumentFromTemp(uploadResult.Result);

                    attachModels.Add(new ContractAttachment
                    {
                        ContractCode = authenticate.ContractCode,
                        FileSrc = uploadResult.Result,
                        FileName = fileName,
                        FileType = UploadedFile.FileType,
                        FileSize = UploadedFile.FileSize
                    });
                }

                _contractAttachmentRepository.AddRange(attachModels);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var res = attachModels.Select(c => new BaseContractAttachmentDto
                    {
                        FileName = c.FileName,
                        FileSize = c.FileSize,
                        FileSrc = c.FileSrc,
                        FileType = c.FileType,
                        ContractCode = authenticate.ContractCode,
                        Id = c.Id
                    }).ToList();
                    return ServiceResultFactory.CreateSuccess(res);
                }
                return ServiceResultFactory.CreateError<List<BaseContractAttachmentDto>>(null, MessageId.UploudFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<BaseContractAttachmentDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<bool>> RemoveContractAsync(AuthenticateDto authenticate, string contractCode)
        {
            try
            {
                if (!_authenticationService.HasPermission(authenticate.UserId, contractCode, authenticate.Roles))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                if (await _contractRepository.AnyAsync(a => a.ContractCode == contractCode && (a.Mrps.Any() || a.PurchaseRequests.Any())))
                    return ServiceResultFactory.CreateError(false, MessageId.DeleteDontAllowedBeforeSubset);

                var selectedContract = await _contractRepository

                    .Include(x => x.Attachments)
                    .Include(a => a.SCMAuditLogs)
                    .FirstOrDefaultAsync(x => x.ContractCode == contractCode);

                _contractAttachmentRepository.RemoveRange(selectedContract.Attachments);

                _scmAuditLogRepository.RemoveRange(selectedContract.SCMAuditLogs);
                _contractRepository.Remove(selectedContract);

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    //var res = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    //{
                    //    ContractCode = selectedContract.ContractCode,
                    //    NewValues = selectedContract,
                    //    RootKeyValue = selectedContract.ContractCode,
                    //    SCMEntityEnum = SCMEntityEnum.Contracts,
                    //    FormCode = selectedContract.ContractCode,
                    //    KeyValue = selectedContract.ContractCode,
                    //    NotifEvent = NotifEvent.DeleteContract,
                    //    PerformerUserId = authenticate.UserId,
                    //    PerformerUserFullName = authenticate.UserFullName
                    //}, null);
                    return ServiceResultFactory.CreateSuccess(true);
                }

                return ServiceResultFactory.CreateError(false, MessageId.DeleteDontAllowedBeforeSubset);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateError(false, MessageId.DeleteDontAllowedBeforeSubset);
            }
        }



        #region teamWork
        public async Task<ServiceResult<List<ContractMiniInfoDto>>> GetContractForCraeteNewTeamWorkAsync(AuthenticateDto authenticate, ContractQuery query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<ContractMiniInfoDto>>(null, MessageId.AccessDenied);

                var dbQuery = _contractRepository
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && x.TeamWork == null)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(x =>
                        x.Description.Contains(query.SearchText) ||
                        x.ContractCode.Contains(query.SearchText));

                if (query.ContractType > 0)
                    dbQuery = dbQuery.Where(x => x.ContractType == query.ContractType);

                if (query.ContractStatus != null && query.ContractStatus.Count() > 0)
                    dbQuery = dbQuery.Where(x => query.ContractStatus.Contains(x.ContractStatus));

                if (query.CustomerIds != null && query.CustomerIds.Count() > 0)
                    dbQuery = dbQuery.Where(x => query.CustomerIds.Contains(x.CustomerId.Value));

                var totalCount = dbQuery.Count();
                var contractList = await dbQuery.ApplayPageing(query).Select(c => new ContractMiniInfoDto
                {
                    ContractCode = c.ContractCode,
                    ContractType = c.ContractType,
                    Description = c.Description,
                    CustomerId = c.CustomerId ?? 0,
                    CustomerName = c.Customer.Name,
                    ContractNumber = c.ContractNumber
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(contractList).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new List<ContractMiniInfoDto>(), exception);
            }
        }
        #endregion

        #region for document
        //public async Task<ServiceResult<List<ContractSubjectForEngineeringDto>>> GetContractSubjectForAddDocumentListAsync(AuthenticateDto authenticate)
        //{
        //    try
        //    {
        //        var permissionResult = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
        //        if (!permissionResult.HasPermission)
        //            return ServiceResultFactory.CreateError<List<ContractSubjectForEngineeringDto>>(null, MessageId.AccessDenied);

        //        var dbQuery = _contractSubjectRepository
        //            .AsNoTracking()
        //            .Where(a => a.ContractCode == authenticate.ContractCode && a.Contract.ContractStatus == ContractStatus.Active && a.DocumentList == null && a.Product.BomProducts.Any(c => c.ParentBomId == null))
        //            .OrderByDescending(a => a.Id)
        //            .AsQueryable();

        //        var result = await dbQuery.Select(c => new ContractSubjectForEngineeringDto
        //        {
        //            Id = c.Id,
        //            ContractCode = c.ContractCode,
        //            ContractDescription = c.Contract.Description,
        //            ProductCode = c.Product.ProductCode,
        //            ProductId = c.ProductId,
        //            ProductUnit = c.Product.Unit1,
        //            ProductName = c.Product.Description,
        //            UserAudit = c.AdderUser != null ? new UserAuditLogDto
        //            {
        //                AdderUserId = c.AdderUserId,
        //                CreateDate = c.CreatedDate.ToUnixTimestamp(),
        //                AdderUserName = c.AdderUser.FullName,
        //                AdderUserImage = c.AdderUser.Image != null ? _appSettings.ElasticHost + ServiceSetting.UploadImagesPath.UserSmall + c.AdderUser.Image : ""
        //            } : null
        //        }).ToListAsync();

        //        return ServiceResultFactory.CreateSuccess(result);
        //    }
        //    catch (Exception exception)
        //    {
        //        return ServiceResultFactory.CreateException<List<ContractSubjectForEngineeringDto>>(null, exception);
        //    }
        //}

        //public async Task<ServiceResult<ContractSubjectForEngineeringDto>> GetContractSubjectForAddDocumentListByIdAsync(AuthenticateDto authenticate, int contractSubjectId)
        //{
        //    try
        //    {
        //        var permissionResult = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
        //        if (!permissionResult.HasPermission)
        //            return ServiceResultFactory.CreateError<ContractSubjectForEngineeringDto>(null, MessageId.AccessDenied);

        //        var dbQuery = _contractSubjectRepository
        //            .AsNoTracking()
        //            .Where(a => a.Id == contractSubjectId && a.ContractCode == authenticate.ContractCode && a.Contract.ContractStatus == ContractStatus.Active && a.DocumentList == null && a.Product.BomProducts.Any(c => c.ParentBomId == null));

        //        if (dbQuery.Count() == 0)
        //            return ServiceResultFactory.CreateError<ContractSubjectForEngineeringDto>(null, MessageId.EntityDoesNotExist);

        //        //if (permissionResult.ProductGroupIds.Any())
        //        //    dbQuery = dbQuery.Where(a => permissionResult.ProductGroupIds.Contains(a.Product.ProductGroupId));

        //        if (dbQuery.Count() == 0)
        //            return ServiceResultFactory.CreateError<ContractSubjectForEngineeringDto>(null, MessageId.AccessDenied);

        //        var result = await dbQuery.Select(a => new ContractSubjectForEngineeringDto
        //        {
        //            Id = a.Id,
        //            ContractDescription = a.Contract.Description,
        //            ContractCode = a.ContractCode,
        //            ProductCode = a.Product.ProductCode,
        //            ProductId = a.ProductId,
        //            ProductUnit = a.Product.Unit1,
        //            ProductName = a.Product.Description,
        //            UserAudit = a.AdderUser != null ? new UserAuditLogDto
        //            {
        //                AdderUserId = a.AdderUserId,
        //                CreateDate = a.CreatedDate.ToUnixTimestamp(),
        //                AdderUserName = a.AdderUser.FullName,
        //                AdderUserImage = a.AdderUser.Image != null ? _appSettings.ElasticHost + ServiceSetting.UploadImagesPath.UserSmall + a.AdderUser.Image : ""
        //            } : null
        //        }).FirstOrDefaultAsync();

        //        return ServiceResultFactory.CreateSuccess(result);
        //    }
        //    catch (Exception exception)
        //    {
        //        return ServiceResultFactory.CreateException<ContractSubjectForEngineeringDto>(null, exception);
        //    }
        //}

        //public async Task<long> GetDashbourdBadgeCountOFContractSubjectForAddDocumentListAsync(AuthenticateDto authenticate)
        //{
        //    try
        //    {
        //        var dbQuery = _contractSubjectRepository
        //            .AsNoTracking()
        //            .Where(a => a.Contract.ContractStatus == ContractStatus.Active &&
        //            a.ContractCode == authenticate.ContractCode &&
        //            a.DocumentList == null && a.Product.BomProducts.Any(c => c.ParentBomId == null));

        //        var result = await dbQuery.CountAsync();
        //        return result;
        //    }
        //    catch (Exception exception)
        //    {
        //        return 0;
        //    }
        //}

        //public async Task<ServiceResult<int>> GetBadgeCountOFContractSubjectForAddDocumentListAsync(AuthenticateDto authenticate)
        //{
        //    try
        //    {
        //        var permissionResult = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
        //        if (!permissionResult.HasPermission)
        //            return ServiceResultFactory.CreateError(0, MessageId.AccessDenied);

        //        var dbQuery = _contractSubjectRepository
        //            .AsNoTracking()
        //            .Where(a => a.ContractCode == authenticate.ContractCode && a.Contract.ContractStatus == ContractStatus.Active && a.DocumentList == null && a.Product.BomProducts.Any(c => c.ParentBomId == null));

        //        //if (!permissionResult.ProductGroupIds.Any())
        //        //    dbQuery = dbQuery.Where(a => permissionResult.ProductGroupIds.Contains(a.Product.ProductGroupId));

        //        var result = await dbQuery.CountAsync();
        //        return ServiceResultFactory.CreateSuccess(result);
        //    }
        //    catch (Exception exception)
        //    {
        //        return ServiceResultFactory.CreateException(0, exception);
        //    }
        //}

        #endregion




        private async Task<ServiceResult<Contract>> AddContractAttachment(Contract contractModel,
            List<AddAttachmentDto> attachmentDtos)
        {
            contractModel.Attachments = new List<ContractAttachment>();
            foreach (var item in attachmentDtos)
            {
                if (!string.IsNullOrEmpty(item.FileSrc))
                {
                    var saveImage = await _fileHelper.SaveDocumentFromTemp(item.FileSrc, ServiceSetting.UploadFilePath.ContractDocument);
                    if (saveImage == null)
                        return ServiceResultFactory.CreateError<Contract>(null, MessageId.UploudFailed);

                    _fileHelper.DeleteDocumentFromTemp(item.FileSrc);
                    contractModel.Attachments.Add(new ContractAttachment
                    {
                        FileType = saveImage.FileType,
                        FileSize = saveImage.FileSize,
                        FileName = item.FileName,
                        FileSrc = item.FileSrc,
                    });
                }
            }

            return ServiceResultFactory.CreateSuccess(contractModel);
        }

        private IEnumerable<AddContractFormConfigDto> CreateCoding(string ContractCode)
        {
            List<AddContractFormConfigDto> result = new List<AddContractFormConfigDto>();
            result.Add(new AddContractFormConfigDto { CodingType = CodingType.Number, FixedPart = "", LengthOfSequence = 2, FormName = FormName.DocumentRevision });
            result.Add(new AddContractFormConfigDto { CodingType = CodingType.Number, FixedPart = ContractCode + "-TR-", LengthOfSequence = 1, FormName = FormName.Transmittal });
            result.Add(new AddContractFormConfigDto { CodingType = CodingType.Number, FixedPart = ContractCode + "-CO-", LengthOfSequence = 1, FormName = FormName.CommunicationComment });
            result.Add(new AddContractFormConfigDto { CodingType = CodingType.Number, FixedPart = ContractCode + "-TQ-", LengthOfSequence = 1, FormName = FormName.CommunicationTQ });
            result.Add(new AddContractFormConfigDto { CodingType = CodingType.Number, FixedPart = ContractCode + "-NCR-", LengthOfSequence = 1, FormName = FormName.CommunicationNCR });
            result.Add(new AddContractFormConfigDto { CodingType = CodingType.Number, FixedPart = ContractCode + "-MRP-", LengthOfSequence = 1, FormName = FormName.MRP });
            result.Add(new AddContractFormConfigDto { CodingType = CodingType.Number, FixedPart = ContractCode + "-PR-", LengthOfSequence = 1, FormName = FormName.PR });
            result.Add(new AddContractFormConfigDto { CodingType = CodingType.Number, FixedPart = ContractCode + "-RFP-", LengthOfSequence = 1, FormName = FormName.RFP });
            result.Add(new AddContractFormConfigDto { CodingType = CodingType.Number, FixedPart = ContractCode + "-PRC-", LengthOfSequence = 1, FormName = FormName.PRContract });
            result.Add(new AddContractFormConfigDto { CodingType = CodingType.Number, FixedPart = ContractCode + "-PO-", LengthOfSequence = 1, FormName = FormName.PO });
            result.Add(new AddContractFormConfigDto { CodingType = CodingType.Number, FixedPart = ContractCode + "-PA-", LengthOfSequence = 1, FormName = FormName.Pack });
            result.Add(new AddContractFormConfigDto { CodingType = CodingType.Number, FixedPart = ContractCode + "-RE-", LengthOfSequence = 1, FormName = FormName.Receipt });
            result.Add(new AddContractFormConfigDto { CodingType = CodingType.Number, FixedPart = ContractCode + "-RJ", LengthOfSequence = 1, FormName = FormName.ReceiptReject });
            result.Add(new AddContractFormConfigDto { CodingType = CodingType.Number, FixedPart = ContractCode + "-IN-", LengthOfSequence = 1, FormName = FormName.PendingToPayment });
            result.Add(new AddContractFormConfigDto { CodingType = CodingType.Number, FixedPart = ContractCode + "-BI-", LengthOfSequence = 1, FormName = FormName.Invoice });
            result.Add(new AddContractFormConfigDto { CodingType = CodingType.Number, FixedPart = ContractCode + "-PY-", LengthOfSequence = 1, FormName = FormName.Payment });
            result.Add(new AddContractFormConfigDto { CodingType = CodingType.Number, FixedPart = ContractCode + "-WR-", LengthOfSequence = 1, FormName = FormName.WarehouseOutput });
            result.Add(new AddContractFormConfigDto { CodingType = CodingType.Number, FixedPart = ContractCode + "-WD-", LengthOfSequence = 1, FormName = FormName.WarehouseDespatch });
            return result;
        }

        public async Task<ServiceResult<UserInfoApiDto>> GetUserInfoAsync(int userId)
        {
            try
            {


                var userModel = await _usersRepository
                    .Where(u => !u.IsDeleted && u.IsActive && u.Id == userId && u.UserType > 0)
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
                        PowerBIRoot = _appSettings.PowerBIRoot,
                        CompanyLogo = _appSettings.WepApiHost + _appSettings.CompanyLogoFront
                    }).FirstOrDefaultAsync();



                var result = await _authenticationServices.GetUserPermissionByUserIdAsync(userModel.Id);
                if (!result.Succeeded)
                    return ServiceResultFactory.CreateError(new UserInfoApiDto(), MessageId.OperationFailed);
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
        public async Task<ServiceResult<ContractDescriptionDto>> GetProjectDescriptionAsync(AuthenticateDto authenticate)
        {
            try
            {
                if (!_authenticationService.HasPermission(authenticate.UserId, authenticate.ContractCode))
                    return ServiceResultFactory.CreateError<ContractDescriptionDto>(null, MessageId.AccessDenied);
                var dbQuery = _contractRepository
                    .AsNoTracking()
                    .Include(a => a.AdderUser)
                    .Where(x => !x.IsDeleted && x.ContractCode == authenticate.ContractCode)
                    .OrderByDescending(a => a.CreatedDate);

                if (await dbQuery.CountAsync() == 0)
                    return ServiceResultFactory.CreateError<ContractDescriptionDto>(null, MessageId.EntityDoesNotExist);

                var result = await dbQuery.Select(a => new ContractDescriptionDto
                {
                    ContractCode = a.ContractCode,
                    Description = (a.Description != null) ? a.Description : "",
                    Services = CreateServiceProperty(a.DocumentManagement, a.FileDrive, a.PurchaseManagement, a.ConstructionManagement),
                    UserAudit = new UserAuditLogDto
                    {
                        AdderUserName = a.AdderUser.FullName,
                        CreateDate = a.CreatedDate.ToUnixTimestamp(),
                        AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + a.AdderUser.Image
                    }
                }).FirstOrDefaultAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<ContractDescriptionDto>(null, exception);
            }
        }


        public async Task<ServiceResult<ContractDetailsDto>> GetProjectDetailAsync(AuthenticateDto authenticate)
        {
            try
            {
                if (!_authenticationService.HasPermission(authenticate.UserId, authenticate.ContractCode))
                    return ServiceResultFactory.CreateError<ContractDetailsDto>(null, MessageId.AccessDenied);
                var dbQuery = _contractRepository
                    .AsNoTracking()
                    .Include(a => a.Customer)
                    .Include(a => a.Consultant)
                    .Where(x => !x.IsDeleted && x.ContractCode == authenticate.ContractCode)
                    .OrderByDescending(a => a.CreatedDate);

                if (await dbQuery.CountAsync() == 0)
                    return ServiceResultFactory.CreateError<ContractDetailsDto>(null, MessageId.EntityDoesNotExist);

                var result = await dbQuery.Select(a => new ContractDetailsDto
                {
                    ProjectTimeTable = new ContractDurationDto
                    {
                        ContractDuration = a.ContractDuration,
                        DateEffective = a.DateEffective.ToUnixTimestamp(),
                        DateEnd = a.DateEnd.ToUnixTimestamp(),
                        DateIssued = a.DateIssued.ToUnixTimestamp()
                    },
                    ProjectCustomer = new BaseCustomerDto
                    {
                        Id = a.Customer.Id,
                        Address = a.Customer.Address,
                        CustomerCode = a.Customer.CustomerCode,
                        Email = a.Customer.Email,
                        Fax = a.Customer.Fax,
                        Name = a.Customer.Name,
                        PostalCode = a.Customer.PostalCode,
                        TellPhone = a.Customer.TellPhone,
                        Website = a.Customer.Website,
                        Logo = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.LogoSmall + a.Customer.Logo
                    },
                    ProjectConsultant = new BaseConsultantDto
                    {
                        Id = a.Consultant.Id,
                        Address = a.Consultant.Address,
                        ConsultantCode = a.Consultant.ConsultantCode,
                        Email = a.Consultant.Email,
                        Fax = a.Consultant.Fax,
                        Name = a.Consultant.Name,
                        PostalCode = a.Consultant.PostalCode,
                        TellPhone = a.Consultant.TellPhone,
                        Website = a.Consultant.Website,
                        Logo = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.LogoSmall + a.Consultant.Logo
                    }
                }).FirstOrDefaultAsync();
                result.ProjectCustomer.Address = (result.ProjectCustomer.Address != null) ? result.ProjectCustomer.Address : "";
                result.ProjectCustomer.CustomerCode = (result.ProjectCustomer.CustomerCode != null) ? result.ProjectCustomer.CustomerCode : "";
                result.ProjectCustomer.Email = (result.ProjectCustomer.Email != null) ? result.ProjectCustomer.Email : "";
                result.ProjectCustomer.Fax = (result.ProjectCustomer.Fax != null) ? result.ProjectCustomer.Fax : "";
                result.ProjectCustomer.Name = (result.ProjectCustomer.Name != null) ? result.ProjectCustomer.Name : "";
                result.ProjectCustomer.PostalCode = (result.ProjectCustomer.PostalCode != null) ? result.ProjectCustomer.PostalCode : "";
                result.ProjectCustomer.TellPhone = (result.ProjectCustomer.TellPhone != null) ? result.ProjectCustomer.TellPhone : "";
                result.ProjectCustomer.Website = (result.ProjectCustomer.Website != null) ? result.ProjectCustomer.Website : "";
                result.ProjectCustomer.Logo = (result.ProjectCustomer.Logo != null) ? result.ProjectCustomer.Logo : "";
                result.ProjectConsultant.Address = (result.ProjectConsultant.Address != null) ? result.ProjectConsultant.Address : "";
                result.ProjectConsultant.ConsultantCode = (result.ProjectConsultant.ConsultantCode != null) ? result.ProjectConsultant.ConsultantCode : "";
                result.ProjectConsultant.Email = (result.ProjectConsultant.Email != null) ? result.ProjectConsultant.Email : "";
                result.ProjectConsultant.Fax = (result.ProjectConsultant.Fax != null) ? result.ProjectConsultant.Fax : "";
                result.ProjectConsultant.Name = (result.ProjectConsultant.Name != null) ? result.ProjectConsultant.Name : "";
                result.ProjectConsultant.PostalCode = (result.ProjectConsultant.PostalCode != null) ? result.ProjectConsultant.PostalCode : "";
                result.ProjectConsultant.TellPhone = (result.ProjectConsultant.TellPhone != null) ? result.ProjectConsultant.TellPhone : "";
                result.ProjectConsultant.Website = (result.ProjectConsultant.Website != null) ? result.ProjectConsultant.Website : "";
                result.ProjectConsultant.Logo = (result.ProjectConsultant.Logo != null) ? result.ProjectConsultant.Logo : "";
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<ContractDetailsDto>(null, exception);
            }
        }
        public async Task<ServiceResult<BaseCustomerDto>> GetContractCustomerAsync(AuthenticateDto authenticate, string contractCode)
        {
            try
            {
                var dbQuery = _contractRepository
                    .AsNoTracking()
                    .Include(a => a.Customer)
                    .Where(x => !x.IsDeleted && x.ContractCode == contractCode)
                    .OrderByDescending(a => a.CreatedDate);

                if (await dbQuery.CountAsync() == 0)
                    return ServiceResultFactory.CreateError<BaseCustomerDto>(null, MessageId.EntityDoesNotExist);

                var result = await dbQuery.Select(a => new BaseCustomerDto
                {
                    Address = a.Customer.Address,
                    CustomerCode = a.Customer.CustomerCode,
                    Email = a.Customer.Email,
                    Fax = a.Customer.Fax,
                    Name = a.Customer.Name,
                    PostalCode = a.Customer.PostalCode,
                    TellPhone = a.Customer.TellPhone,
                    Website = a.Customer.Website,
                    Logo = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.LogoSmall + a.Customer.Logo
                }).FirstOrDefaultAsync();
                result.Address = (result.Address != null) ? result.Address : "";
                result.CustomerCode = (result.CustomerCode != null) ? result.CustomerCode : "";
                result.Email = (result.Email != null) ? result.Email : "";
                result.Fax = (result.Fax != null) ? result.Fax : "";
                result.Name = (result.Name != null) ? result.Name : "";
                result.PostalCode = (result.PostalCode != null) ? result.PostalCode : "";
                result.TellPhone = (result.TellPhone != null) ? result.TellPhone : "";
                result.Website = (result.Website != null) ? result.Website : "";
                result.Logo = (result.Logo != null) ? result.Logo : "";
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<BaseCustomerDto>(null, exception);
            }
        }


        public async Task<ServiceResult<BaseConsultantDto>> GetContractConsultantAsync(AuthenticateDto authenticate, string contractCode)
        {
            try
            {
                var dbQuery = _contractRepository
                    .AsNoTracking()
                    .Include(a => a.Consultant)
                    .Where(x => !x.IsDeleted && x.ContractCode == contractCode)
                    .OrderByDescending(a => a.CreatedDate);

                if (await dbQuery.CountAsync() == 0)
                    return ServiceResultFactory.CreateError<BaseConsultantDto>(null, MessageId.EntityDoesNotExist);

                var result = await dbQuery.Select(a => new BaseConsultantDto
                {
                    Address = a.Consultant.Address,
                    ConsultantCode = a.Consultant.ConsultantCode,
                    Email = a.Consultant.Email,
                    Fax = a.Consultant.Fax,
                    Name = a.Consultant.Name,
                    PostalCode = a.Consultant.PostalCode,
                    TellPhone = a.Consultant.TellPhone,
                    Website = a.Consultant.Website,
                    Logo = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.LogoSmall + a.Consultant.Logo
                }).FirstOrDefaultAsync();
                result.Address = (result.Address != null) ? result.Address : "";
                result.ConsultantCode = (result.ConsultantCode != null) ? result.ConsultantCode : "";
                result.Email = (result.Email != null) ? result.Email : "";
                result.Fax = (result.Fax != null) ? result.Fax : "";
                result.Name = (result.Name != null) ? result.Name : "";
                result.PostalCode = (result.PostalCode != null) ? result.PostalCode : "";
                result.TellPhone = (result.TellPhone != null) ? result.TellPhone : "";
                result.Website = (result.Website != null) ? result.Website : "";
                result.Logo = (result.Logo != null) ? result.Logo : "";
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<BaseConsultantDto>(null, exception);
            }
        }
        public async Task<ServiceResult<UserInfoApiDto>> UpdateProjectDescriptionAsync(AuthenticateDto authenticate, ContractDescriptionUpdateDto model)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<UserInfoApiDto>(null, MessageId.AccessDenied);

                var dbQuery = _contractRepository
                    .Include(a => a.TeamWork)
                    .Where(x => !x.IsDeleted && x.ContractCode == authenticate.ContractCode)
                    .OrderByDescending(a => a.CreatedDate).FirstOrDefault();

                if (dbQuery == null)
                    return ServiceResultFactory.CreateError<UserInfoApiDto>(null, MessageId.EntityDoesNotExist);

                if (model.Services == null || !model.Services.Any())
                    return ServiceResultFactory.CreateError<UserInfoApiDto>(null, MessageId.ServicesCannotBeEmpty);

                var plan = await _planServiceRepository.OrderByDescending(a => a.CreatedDate).FirstOrDefaultAsync();

                if (plan == null)
                    return ServiceResultFactory.CreateError<UserInfoApiDto>(null, MessageId.ServiceInformationNotAvailable);

                if (plan.StartDate.Date > DateTime.Now.Date)
                    return ServiceResultFactory.CreateError<UserInfoApiDto>(null, MessageId.PlanServiceNotStarted);

                if (plan.FinishDate.Date < DateTime.Now.Date)
                    return ServiceResultFactory.CreateError<UserInfoApiDto>(null, MessageId.PlanServiceExpired);


                if (plan.DocumentManagement == false && model.Services.Contains("DocumentMngService"))
                    return ServiceResultFactory.CreateError<UserInfoApiDto>(null, MessageId.DocumentServicesNotAvailable);

                if (plan.FileDrive == false && model.Services.Contains("FileDriveService"))
                    return ServiceResultFactory.CreateError<UserInfoApiDto>(null, MessageId.FileDriveServicesNotAvailable);

                if (plan.PurchaseManagement == false && model.Services.Contains("PurchasingMngService"))
                    return ServiceResultFactory.CreateError<UserInfoApiDto>(null, MessageId.PurchaseServicesNotAvailable);

                if (plan.ConstructionManagement == false && model.Services.Contains("OperationMngService"))
                    return ServiceResultFactory.CreateError<UserInfoApiDto>(null, MessageId.ConstructionServicesNotAvailable);



                dbQuery.Description = (!String.IsNullOrEmpty(model.Description)) ? model.Description : "";
                if (dbQuery.TeamWork != null)
                    dbQuery.TeamWork.Title = (!String.IsNullOrEmpty(model.Description)) ? model.Description : "";


                dbQuery.DocumentManagement = false;
                dbQuery.FileDrive = false;
                dbQuery.PurchaseManagement = false;
                dbQuery.ConstructionManagement = false;

                if (model.Services.Contains("DocumentMngService"))
                    dbQuery.DocumentManagement = true;

                if (model.Services.Contains("FileDriveService"))
                    dbQuery.FileDrive = true;

                if (model.Services.Contains("PurchasingMngService"))
                    dbQuery.PurchaseManagement = true;

                if (model.Services.Contains("OperationMngService"))
                    dbQuery.ConstructionManagement = true;

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var result = await GetUserInfoAsync(authenticate.UserId);
                    if (!result.Succeeded)
                        return ServiceResultFactory.CreateError<UserInfoApiDto>(null, MessageId.OperationFailed);
                    return ServiceResultFactory.CreateSuccess(result.Result);
                }

                return ServiceResultFactory.CreateError<UserInfoApiDto>(null, MessageId.SaveFailed);


            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<UserInfoApiDto>(null, exception);
            }
        }

        public async Task<ServiceResult<ContractDurationDto>> UpdateProjectTimeTableAsync(AuthenticateDto authenticate, ContractDurationDto model)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<ContractDurationDto>(null, MessageId.AccessDenied);

                var dbQuery = _contractRepository

                    .Where(x => !x.IsDeleted && x.ContractCode == authenticate.ContractCode)
                    .OrderByDescending(a => a.CreatedDate).FirstOrDefault();

                if (dbQuery == null)
                    return ServiceResultFactory.CreateError<ContractDurationDto>(null, MessageId.EntityDoesNotExist);

                if (model.DateIssued == null || model.DateEffective == null || model.DateEnd == null || model.ContractDuration == null)
                    return ServiceResultFactory.CreateError<ContractDurationDto>(null, MessageId.InputDataValidationError);

                if (model.DateEnd < model.DateIssued || model.DateEnd < model.DateEffective)
                    return ServiceResultFactory.CreateError<ContractDurationDto>(null, MessageId.InputDataValidationError);

                dbQuery.DateEffective = model.DateEffective.UnixTimestampToDateTime();
                dbQuery.DateIssued = model.DateIssued.UnixTimestampToDateTime();
                dbQuery.DateEnd = model.DateEnd.UnixTimestampToDateTime();
                dbQuery.ContractDuration = model.ContractDuration;

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    ContractDurationDto result = new ContractDurationDto();
                    result.DateEffective = dbQuery.DateEffective.ToUnixTimestamp();
                    result.DateIssued = dbQuery.DateIssued.ToUnixTimestamp();
                    result.DateEnd = dbQuery.DateEnd.ToUnixTimestamp();
                    result.ContractDuration = dbQuery.ContractDuration;
                    return ServiceResultFactory.CreateSuccess(result);
                }

                return ServiceResultFactory.CreateError<ContractDurationDto>(null, MessageId.SaveFailed);


            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<ContractDurationDto>(null, exception);
            }
        }
        public async Task<ServiceResult<bool>> UpdateProjectVisitedAsync(AuthenticateDto authenticate)
        {
            try
            {


                var dbQuery = _latestTeamworkRepository
                    .Where(x => x.TeamWork.ContractCode == authenticate.ContractCode && x.UserId == authenticate.UserId);



                if (dbQuery == null || dbQuery.Count() == 0)
                {
                    var teamwork = await _teamWorkRepository.FirstAsync(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode);
                    if (teamwork == null)
                        return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);
                    var latestTeamwork = new UserLatestTeamWork
                    {
                        LastVisited = DateTime.Now,
                        UserId = authenticate.UserId,
                        TeamWorkId = teamwork.Id
                    };
                    await _latestTeamworkRepository.AddAsync(latestTeamwork);


                }
                else
                {
                    var latestTeamwork = await dbQuery.FirstOrDefaultAsync();
                    latestTeamwork.LastVisited = DateTime.Now;
                }


                if (await _unitOfWork.SaveChangesAsync() > 0)
                {

                    return ServiceResultFactory.CreateSuccess(true);
                }

                return ServiceResultFactory.CreateError(false, MessageId.SaveFailed);


            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }
        private Contract AddServices(Contract contractModel, List<string> services)
        {
            contractModel.DocumentManagement = false;
            contractModel.FileDrive = false;
            contractModel.PurchaseManagement = false;
            contractModel.ConstructionManagement = false;
            if (services.Contains("DocumentMngService"))
                contractModel.DocumentManagement = true;
            if (services.Contains("FileDriveService"))
                contractModel.FileDrive = true;
            if (services.Contains("PurchasingMngService"))
                contractModel.PurchaseManagement = true;
            if (services.Contains("OperationMngService"))
                contractModel.ConstructionManagement = true;
            return contractModel;
        }
        private bool ValidateContractCodeForNonPersianCharachter(string contractCode)
        {
            bool result = true;
            foreach (var ch in contractCode)
            {
                if (!(ch >= 0 && ch <= 127))
                    return false;
            }
            return result;
        }

        private bool ValidateContractCode(string contractCode)
        {
            bool result = true;
            foreach (var ch in contractCode)
            {
                if ((ch == '\\' || ch == '/' || ch == '+' || ch == '*'))
                    return false;
            }
            return result;
        }
        private static List<string> CreateServiceProperty(bool docMangement, bool fileDrive, bool purchaseManagement, bool contructionManagement)
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

        private async Task<List<UserNotify>> AddUserNotify(Role role, List<UserNotify> addNotify, int userId)
        {
            if (!role.SCMTasks.StartsWith("0"))
            {

                var notifyNumber = role.SCMTasks.Split(',', StringSplitOptions.RemoveEmptyEntries);

                foreach (var number in notifyNumber)
                {
                    if (!addNotify.Any(a => a.IsOrganization && a.NotifyNumber == Convert.ToInt32(number) && a.NotifyType == NotifyManagementType.Task && a.UserId == userId))
                        addNotify.Add(new UserNotify
                        {
                            IsActive = true,
                            IsOrganization = true,
                            NotifyNumber = Convert.ToInt32(number),
                            NotifyType = NotifyManagementType.Task,
                            UserId = userId,
                            SubModuleName = role.SubModuleName
                        });
                }
            }
            if (!role.SCMEvents.StartsWith("0"))
            {
                var notifyNumber = role.SCMEvents.Split(',', StringSplitOptions.RemoveEmptyEntries);

                foreach (var number in notifyNumber)
                {
                    if (!addNotify.Any(a => a.IsOrganization && a.NotifyNumber == Convert.ToInt32(number) && a.NotifyType == NotifyManagementType.Event && a.UserId == userId))
                        addNotify.Add(new UserNotify
                        {
                            IsActive = true,
                            IsOrganization = true,
                            NotifyNumber = Convert.ToInt32(number),
                            NotifyType = NotifyManagementType.Event,
                            UserId = userId,
                            SubModuleName = role.SubModuleName
                        });
                }
            }
            if (!role.SCMEmails.StartsWith("0"))
            {
                var notifyNumber = role.SCMEmails.Split(',', StringSplitOptions.RemoveEmptyEntries);
                var teamworks = await _teamWorkRepository.Where(a => !a.IsDeleted && !a.Contract.IsDeleted).ToListAsync();
                foreach (var number in notifyNumber)
                {
                    if (!addNotify.Any(a => a.IsOrganization && a.NotifyNumber == Convert.ToInt32(number) && a.NotifyType == NotifyManagementType.Email && a.UserId == userId))
                        addNotify.Add(new UserNotify
                        {
                            IsActive = true,
                            IsOrganization = true,
                            NotifyNumber = Convert.ToInt32(number),
                            NotifyType = NotifyManagementType.Email,
                            UserId = userId,
                            SubModuleName = role.SubModuleName
                        });
                }
            }
            return addNotify;
        }
    }
}