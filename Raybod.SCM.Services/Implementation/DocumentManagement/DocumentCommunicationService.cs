using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Raybod.SCM.DataAccess.Core;
using Raybod.SCM.DataAccess.Extention;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject._PanelDocument.Communication;
using Raybod.SCM.DataTransferObject.Document;
using Raybod.SCM.DataTransferObject.Document.Communication;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Model;
using Raybod.SCM.Domain.Struct;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Utility.Extention;
using Raybod.SCM.Utility.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Raybod.SCM.Services.Implementation
{
    public class DocumentCommunicationService : IDocumentCommunicationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly DbSet<Contract> _contractRepository;
        private readonly DbSet<DocumentCommunication> _communicationRepository;
        private readonly DbSet<DocumentTQNCR> _documentTQNCRRepository;
        private readonly DbSet<Customer> _customerRepository;
        private readonly DbSet<Supplier> _supplierRepository;
        private readonly DbSet<User> _userRepository;
        private readonly DbSet<Consultant> _consultantRepository;
        private readonly ITeamWorkAuthenticationService _authenticationServices;
        private readonly CompanyAppSettingsDto _appSettings;

        public DocumentCommunicationService(
            IUnitOfWork unitOfWork,
            IOptions<CompanyAppSettingsDto> appSettings,
            ITeamWorkAuthenticationService authenticationServices)
        {
            _unitOfWork = unitOfWork;
            _authenticationServices = authenticationServices;
            _appSettings = appSettings.Value;
            _contractRepository = _unitOfWork.Set<Contract>();
            _documentTQNCRRepository = _unitOfWork.Set<DocumentTQNCR>();
            _communicationRepository = _unitOfWork.Set<DocumentCommunication>();
            _customerRepository = _unitOfWork.Set<Customer>();
            _userRepository = _unitOfWork.Set<User>();
            _supplierRepository = _unitOfWork.Set<Supplier>();
            _consultantRepository = _unitOfWork.Set<Consultant>();
        }


        public async Task<ServiceResult<List<AllCompanyListDto>>> GetAllCompanyListAsync(AuthenticateDto authenticate)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<AllCompanyListDto>>(null, MessageId.AccessDenied);

                var result = new List<AllCompanyListDto>();
                result.Add(new AllCompanyListDto
                {
                    Id = 0,
                    Name =(authenticate.language=="en")?"Internal": "داخلی",
                    Type = CompanyIssue.Internal
                });
                var consultant = await _consultantRepository
                   .AsNoTracking()
                   .Where(a => !a.IsDeleted && a.ConsultantContracts.Any(b => b.ContractCode == authenticate.ContractCode))
                   .Select(c => new AllCompanyListDto
                   {
                       Id = c.Id,
                       Name = c.Name,
                       Type = CompanyIssue.Consultant
                   }).ToListAsync();
                if (consultant != null && consultant.Any())
                    result.AddRange(consultant);
                var customerInfo = await _customerRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted&&a.CustomerContracts.Any(b=>b.ContractCode==authenticate.ContractCode))
                    .Select(c => new AllCompanyListDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Type = CompanyIssue.Customer
                    }).ToListAsync();

                result.AddRange(customerInfo);

                var supplliers = await _supplierRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted)
                    .Select(c => new AllCompanyListDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Type = CompanyIssue.Supplier
                    }).ToListAsync();
                result.AddRange(supplliers);
                

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<AllCompanyListDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<CurrentContractInfoDto>> GetCurrentContractInfoAsync(AuthenticateDto authenticate)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<CurrentContractInfoDto>(null, MessageId.AccessDenied);

                var result = await _contractRepository
                     .AsNoTracking()
                     .Where(a => !a.IsDeleted &&
                     a.ContractCode == authenticate.ContractCode)
                     .Select(a => new CurrentContractInfoDto
                     {
                         ContractCode = a.ContractCode,
                         ContractDescription = a.Description,
                         CustomerCode = a.Customer.CustomerCode,
                         CustomerId = a.CustomerId??0,
                         CustomerName = a.Customer.Name
                     }).FirstOrDefaultAsync();

                if (result == null)
                    return ServiceResultFactory.CreateError<CurrentContractInfoDto>(null, MessageId.ContractNotFound);

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<CurrentContractInfoDto>(null, exception);
            }
        }
        public async Task<ServiceResult<CurrentContractInfoDto>> GetCurrentContractInfoForCustomerUserAsync(AuthenticateDto authenticate,bool accessability)
        {
            try
            {
                if (!accessability)
                    return ServiceResultFactory.CreateError<CurrentContractInfoDto>(null, MessageId.AccessDenied);


                var user = await _userRepository.FindAsync(authenticate.UserId);

                var result = await _contractRepository
                     .AsNoTracking()
                     .Where(a => !a.IsDeleted &&
                     a.ContractCode == authenticate.ContractCode)
                     .Select(a => new CurrentContractInfoDto
                     {
                         ContractCode = a.ContractCode,
                         ContractDescription = a.Description,
                         CustomerCode = (user.UserType == (int)UserStatus.CustomerUser) ? a.Customer.CustomerCode:a.Consultant.ConsultantCode,
                         CustomerId = (user.UserType == (int)UserStatus.CustomerUser)?a.CustomerId.Value:a.Consultant.Id,
                         CustomerName =(user.UserType==(int)UserStatus.CustomerUser)? a.Customer.Name:a.Consultant.Name
                     }).FirstOrDefaultAsync();

                if (result == null)
                    return ServiceResultFactory.CreateError<CurrentContractInfoDto>(null, MessageId.ContractNotFound);

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<CurrentContractInfoDto>(null, exception);
            }
        } 
        public async Task<ServiceResult<CurrentContractInfoDto>> GetCurrentContractInfoAndCustomerInfoAsync(AuthenticateDto authenticate)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<CurrentContractInfoDto>(null, MessageId.AccessDenied);

                var result = await _contractRepository
                     .AsNoTracking()
                     .Where(a => !a.IsDeleted &&
                     a.ContractCode == authenticate.ContractCode)
                     .Select(a => new CurrentContractInfoDto
                     {
                         ContractCode = a.ContractCode,
                         ContractDescription = a.Description,
                         CustomerCode = a.Customer.CustomerCode,
                         CustomerId = a.CustomerId??0,
                         CustomerName = a.Customer.Name
                     }).FirstOrDefaultAsync();

                if (result == null)
                    return ServiceResultFactory.CreateError<CurrentContractInfoDto>(null, MessageId.ContractNotFound);

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<CurrentContractInfoDto>(null, exception);
            }
        }



        public async Task<ServiceResult<List<CommunicationListDto>>> GetPendingReplyCommunicationListAsync(AuthenticateDto authenticate, CommunicationQueryDto query,CommunicationType type)
        {
            try
            {
                var result = new List<CommunicationListDto>();
                query.CommunicationType = type;
                if (type== CommunicationType.Comment)
                {
                    authenticate.Roles = new List<string> { SCMRole.ComCommentReply, SCMRole.ComCommentMng };
                    var commentList = await GetPendingCommunicationCommentListAsync(authenticate, query);
                    result.AddRange(commentList.Result);
                }
                else if(type == CommunicationType.TQ)
                {
                    authenticate.Roles = new List<string> { SCMRole.TQReply, SCMRole.TQMng };
                    var tqList = await GetPendingCommunicationTQListAsync(authenticate, query);
                    result.AddRange(tqList.Result);
                }
                else if(type== CommunicationType.NCR)
                {
                    authenticate.Roles = new List<string> { SCMRole.NCRReply, SCMRole.NCRMng };
                    var ncrList = await GetPendingCommunicationNCRListAsync(authenticate, query);
                    result.AddRange(ncrList.Result);
                   
                }
                var totalCount = result.Count;

                result = result.OrderByDescending(a => a.CreateDate).ApplayPageing(query).ToList();

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<CommunicationListDto>>(null, exception);
            }
        }

        private async Task<ServiceResult<List<CommunicationListDto>>> GetPendingCommunicationCommentListAsync(AuthenticateDto authenticate, CommunicationQueryDto query)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateSuccess(new List<CommunicationListDto>());

                var dbQuery = _communicationRepository
                    .AsNoTracking()
                    .Where(a => a.CommunicationStatus == DocumentCommunicationStatus.PendingReply && a.DocumentRevision.Document.ContractCode == authenticate.ContractCode);

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a =>
                     a.CommunicationCode.Contains(query.SearchText)
                     || a.DocumentRevision.Document.DocNumber.Contains(query.SearchText)
                     || a.DocumentRevision.Document.DocTitle.Contains(query.SearchText));

                if (permission.DocumentGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.DocumentGroupIds.Contains(a.DocumentRevision.Document.DocumentGroupId));

                if (EnumHelper.ValidateItem(query.DocClass))
                    dbQuery = dbQuery.Where(a => a.DocumentRevision.Document.DocClass == query.DocClass);

                if (EnumHelper.ValidateItem(query.CommunicationType))
                    dbQuery = dbQuery.Where(a => a.CommunicationType == query.CommunicationType);

                if (EnumHelper.ValidateItem(query.CompanyIssue))
                {
                    if (query.CompanyIssue == CompanyIssue.Customer && query.CompanyIssueId > 0)
                        dbQuery = dbQuery.Where(a => a.CustomerId == query.CompanyIssueId);
                    else if (query.CompanyIssue == CompanyIssue.Consultant && query.CompanyIssueId > 0)
                        dbQuery = dbQuery.Where(a => a.ConsultantId == query.CompanyIssueId);
                    else
                        dbQuery = dbQuery.Where(a => a.CustomerId == 0);

                   
                }

                if (query.DocumentGroupIds != null && query.DocumentGroupIds.Any())
                    dbQuery = dbQuery.Where(a => query.DocumentGroupIds.Contains(a.DocumentRevision.Document.DocumentGroupId));


                var commentList = await dbQuery.Select(c => new CommunicationListDto
                {
                    CommunicationCode = c.CommunicationCode,
                    CommunicationId = c.DocumentCommunicationId,
                    CommunicationStatus = c.CommunicationStatus,
                    CommunicationType = c.CommunicationType,
                    CreateDate = c.CreatedDate.ToUnixTimestamp(),
                    DocumentRevisionCode = c.DocumentRevision.DocumentRevisionCode,
                    CompanyIssue = c.CompanyIssue,
                    CompanyIssueName = (c.CompanyIssue==CompanyIssue.Customer)?c.Customer.Name:c.Consultant.Name,
                    DocNumber = c.DocumentRevision.Document.DocNumber,
                    DocTitle = c.DocumentRevision.Document.DocTitle,
                    DocumentGroupTitle = c.DocumentRevision.Document.DocumentGroup.Title,
                    DocumentRevisionId = c.DocumentRevisionId,
                    UserAudit = c.AdderUser != null ? new UserAuditLogDto
                    {
                        AdderUserName = c.AdderUser.FullName,
                        AdderUserImage = c.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + c.AdderUser.Image : "",
                        CreateDate = c.CreatedDate.ToUnixTimestamp(),
                    } : null,
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(commentList);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateSuccess(new List<CommunicationListDto>());
            }
        }

        private async Task<ServiceResult<List<CommunicationListDto>>> GetPendingCommunicationTQListAsync(AuthenticateDto authenticate, CommunicationQueryDto query)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateSuccess(new List<CommunicationListDto>());

                var dbQuery = _documentTQNCRRepository
                    .AsNoTracking()
                    .Where(a => a.CommunicationType == CommunicationType.TQ &&
                    a.CommunicationStatus == DocumentCommunicationStatus.PendingReply &&
                    a.DocumentRevision.Document.ContractCode == authenticate.ContractCode);

                if (permission.DocumentGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.DocumentGroupIds.Contains(a.DocumentRevision.Document.DocumentGroupId));

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a =>
                     a.CommunicationCode.Contains(query.SearchText)
                     || a.DocumentRevision.Document.DocNumber.Contains(query.SearchText)
                     || a.DocumentRevision.Document.DocTitle.Contains(query.SearchText));

                if (EnumHelper.ValidateItem(query.DocClass))
                    dbQuery = dbQuery.Where(a => a.DocumentRevision.Document.DocClass == query.DocClass);

                if (EnumHelper.ValidateItem(query.CommunicationType))
                {
                    if (query.CommunicationType != CommunicationType.Comment)
                        dbQuery = dbQuery.Where(a => a.CommunicationType == query.CommunicationType);
                    else return ServiceResultFactory.CreateSuccess(new List<CommunicationListDto>());
                }

                if (query.DocumentGroupIds != null && query.DocumentGroupIds.Any())
                    dbQuery = dbQuery.Where(a => query.DocumentGroupIds.Contains(a.DocumentRevision.Document.DocumentGroupId));

                if (EnumHelper.ValidateItem(query.CompanyIssue))
                {
                    if (query.CompanyIssue == CompanyIssue.Customer && query.CompanyIssueId > 0)
                        dbQuery = dbQuery.Where(a => a.CompanyIssue == query.CompanyIssue && a.CustomerId == query.CompanyIssueId);
                    else if (query.CompanyIssue == CompanyIssue.Supplier && query.CompanyIssueId > 0)
                        dbQuery = dbQuery.Where(a => a.CompanyIssue == query.CompanyIssue && a.SupplierId == query.CompanyIssueId);
                    else if (query.CompanyIssue == CompanyIssue.Consultant && query.CompanyIssueId > 0)
                        dbQuery = dbQuery.Where(a => a.CompanyIssue == query.CompanyIssue && a.ConsultantId == query.CompanyIssueId);
                    else if (query.CompanyIssue == CompanyIssue.Internal)
                        dbQuery = dbQuery.Where(a => a.CompanyIssue == query.CompanyIssue);
                }

                var commentList = await dbQuery.Select(c => new CommunicationListDto
                {
                    CommunicationCode = c.CommunicationCode,
                    CommunicationId = c.DocumentTQNCRId,
                    CommunicationStatus = c.CommunicationStatus,
                    CommunicationType = c.CommunicationType,
                    CompanyIssue = c.CompanyIssue,
                    CompanyIssueName = c.CompanyIssue == CompanyIssue.Customer && c.Customer != null ? c.Customer.Name :
                    c.CompanyIssue == CompanyIssue.Supplier && c.Supplier != null ? c.Supplier.Name : c.CompanyIssue == CompanyIssue.Consultant && c.Consultant != null ? c.Consultant.Name : "داخلی",
                    DocumentRevisionCode = c.DocumentRevision.DocumentRevisionCode,
                    DocNumber = c.DocumentRevision.Document.DocNumber,
                    DocTitle = c.DocumentRevision.Document.DocTitle,
                    DocumentGroupTitle = c.DocumentRevision.Document.DocumentGroup.Title,
                    DocumentRevisionId = c.DocumentRevisionId,
                    UserAudit = c.AdderUser != null ? new UserAuditLogDto
                    {
                        AdderUserName = c.AdderUser.FullName,
                        AdderUserImage = c.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + c.AdderUser.Image : "",
                        CreateDate = c.CreatedDate.ToUnixTimestamp(),
                    } : null,
                    CreateDate = c.CreatedDate.ToUnixTimestamp(),
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(commentList);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateSuccess(new List<CommunicationListDto>());
            }
        }

        private async Task<ServiceResult<List<CommunicationListDto>>> GetPendingCommunicationNCRListAsync(AuthenticateDto authenticate, CommunicationQueryDto query)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateSuccess(new List<CommunicationListDto>());

                var dbQuery = _documentTQNCRRepository
                    .AsNoTracking()
                    .Where(a => a.CommunicationType == CommunicationType.NCR &&
                    a.CommunicationStatus == DocumentCommunicationStatus.PendingReply &&
                    a.DocumentRevision.Document.ContractCode == authenticate.ContractCode);

                if (permission.DocumentGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.DocumentGroupIds.Contains(a.DocumentRevision.Document.DocumentGroupId));

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a =>
                     a.CommunicationCode.Contains(query.SearchText)
                     || a.DocumentRevision.Document.DocNumber.Contains(query.SearchText)
                     || a.DocumentRevision.Document.DocTitle.Contains(query.SearchText));

                if (EnumHelper.ValidateItem(query.DocClass))
                    dbQuery = dbQuery.Where(a => a.DocumentRevision.Document.DocClass == query.DocClass);

                if (EnumHelper.ValidateItem(query.CommunicationType))
                {
                    if (query.CommunicationType != CommunicationType.Comment)
                        dbQuery = dbQuery.Where(a => a.CommunicationType == query.CommunicationType);
                    else return ServiceResultFactory.CreateSuccess(new List<CommunicationListDto>());
                }

                if (query.DocumentGroupIds != null && query.DocumentGroupIds.Any())
                    dbQuery = dbQuery.Where(a => query.DocumentGroupIds.Contains(a.DocumentRevision.Document.DocumentGroupId));

                if (EnumHelper.ValidateItem(query.CompanyIssue))
                {
                    if (query.CompanyIssue == CompanyIssue.Customer && query.CompanyIssueId > 0)
                        dbQuery = dbQuery.Where(a => a.CompanyIssue == query.CompanyIssue && a.CustomerId == query.CompanyIssueId);
                    else if (query.CompanyIssue == CompanyIssue.Supplier && query.CompanyIssueId > 0)
                        dbQuery = dbQuery.Where(a => a.CompanyIssue == query.CompanyIssue && a.SupplierId == query.CompanyIssueId);
                    else if (query.CompanyIssue == CompanyIssue.Consultant && query.CompanyIssueId > 0)
                        dbQuery = dbQuery.Where(a => a.CompanyIssue == query.CompanyIssue && a.ConsultantId == query.CompanyIssueId);
                    else if (query.CompanyIssue == CompanyIssue.Internal)
                        dbQuery = dbQuery.Where(a => a.CompanyIssue == query.CompanyIssue);
                }

                var commentList = await dbQuery.Select(c => new CommunicationListDto
                {
                    CommunicationCode = c.CommunicationCode,
                    CommunicationId = c.DocumentTQNCRId,
                    CommunicationStatus = c.CommunicationStatus,
                    CommunicationType = c.CommunicationType,
                    CompanyIssue = c.CompanyIssue,
                    CompanyIssueName = c.CompanyIssue == CompanyIssue.Customer && c.Customer != null ? c.Customer.Name :
                    c.CompanyIssue == CompanyIssue.Supplier && c.Supplier != null ? c.Supplier.Name : c.CompanyIssue == CompanyIssue.Consultant && c.Consultant != null ? c.Consultant.Name : "داخلی",
                    DocumentRevisionCode = c.DocumentRevision.DocumentRevisionCode,
                    DocNumber = c.DocumentRevision.Document.DocNumber,
                    DocTitle = c.DocumentRevision.Document.DocTitle,
                    DocumentGroupTitle = c.DocumentRevision.Document.DocumentGroup.Title,
                    DocumentRevisionId = c.DocumentRevisionId,
                    UserAudit = c.AdderUser != null ? new UserAuditLogDto
                    {
                        AdderUserName = c.AdderUser.FullName,
                        AdderUserImage = c.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + c.AdderUser.Image : "",
                        CreateDate = c.CreatedDate.ToUnixTimestamp(),
                    } : null,
                    CreateDate = c.CreatedDate.ToUnixTimestamp(),
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(commentList);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateSuccess(new List<CommunicationListDto>());
            }
        }



        public async Task<ServiceResult<List<RevisionCommunicationListDto>>> GetAllRevisionCommunicationListAsync(AuthenticateDto authenticate, long documentRevisionId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateSuccess(new List<RevisionCommunicationListDto>());

                var resultList = new List<RevisionCommunicationListDto>();

                var commentList = await GetAllRevisionCommunicationTQAndNCRListAsync(authenticate, permission, documentRevisionId);
                var tqncrList = await GetAllRevisionCommunicationCommentListAsync(authenticate, permission, documentRevisionId);

                resultList.AddRange(commentList);
                resultList.AddRange(tqncrList);
                resultList = resultList.OrderByDescending(a => a.CreateDate).ToList();

                return ServiceResultFactory.CreateSuccess(resultList);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateSuccess(new List<RevisionCommunicationListDto>());
            }
        }

        public async Task<ServiceResult<List<RevisionCommunicationListDto>>> GetAllRevisionCommentListForCustomerUserAsync(AuthenticateDto authenticate, long documentRevisionId,bool accessability)
        {
            try
            {
                if (!accessability)
                    return ServiceResultFactory.CreateSuccess(new List<RevisionCommunicationListDto>());
               
               

                var resultList = new List<RevisionCommunicationListDto>();
                
                var commentList = await GetAllRevisionCommunicationCommentForCustomerUserListAsync(authenticate, documentRevisionId);
               

                resultList.AddRange(commentList);
            
                resultList = resultList.OrderByDescending(a => a.CreateDate).ToList();

                return ServiceResultFactory.CreateSuccess(resultList);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateSuccess(new List<RevisionCommunicationListDto>());
            }
        }

        public async Task<ServiceResult<List<RevisionCommunicationListDto>>> GetAllRevisionTQListForCustomerUserAsync(AuthenticateDto authenticate, long documentRevisionId, bool accessability)
        {
            try
            {
                if (!accessability)
                    return ServiceResultFactory.CreateSuccess(new List<RevisionCommunicationListDto>());
              
               

                var resultList = new List<RevisionCommunicationListDto>();

                var tqList = await GetAllRevisionCommunicationTQAndNCRForCustomerUserListAsync (authenticate, documentRevisionId);

                tqList = tqList.Where(a => a.CommunicationType == CommunicationType.TQ).ToList();
                resultList.AddRange(tqList);

                resultList = resultList.OrderByDescending(a => a.CreateDate).ToList();

                return ServiceResultFactory.CreateSuccess(resultList);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateSuccess(new List<RevisionCommunicationListDto>());
            }
        }


        public async Task<ServiceResult<List<RevisionCommunicationListDto>>> GetAllRevisionNCRListForCustomerUserAsync(AuthenticateDto authenticate, long documentRevisionId, bool accessability)
        {
            try
            {
                if (!accessability)
                    return ServiceResultFactory.CreateSuccess(new List<RevisionCommunicationListDto>());
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
               

                var resultList = new List<RevisionCommunicationListDto>();

                var ncrList = await GetAllRevisionCommunicationTQAndNCRForCustomerUserListAsync(authenticate, documentRevisionId);

                ncrList = ncrList.Where(a => a.CommunicationType == CommunicationType.NCR).ToList();
                resultList.AddRange(ncrList);

                resultList = resultList.OrderByDescending(a => a.CreateDate).ToList();

                return ServiceResultFactory.CreateSuccess(resultList);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateSuccess(new List<RevisionCommunicationListDto>());
            }
        }
        private async Task<List<RevisionCommunicationListDto>> GetAllRevisionCommunicationTQAndNCRListAsync(AuthenticateDto authenticate, PermissionResultDto permission, long documentRevisionId)
        {
            try
            {
                var dbQuery = _documentTQNCRRepository
                    .AsNoTracking()
                    .Where(a => a.DocumentRevision.Document.ContractCode == authenticate.ContractCode &&
                    a.DocumentRevisionId == documentRevisionId);

                if (permission.DocumentGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.DocumentGroupIds.Contains(a.DocumentRevision.Document.DocumentGroupId));

                var commentList = await dbQuery.Select(c => new RevisionCommunicationListDto
                {
                    CommunicationId = c.DocumentTQNCRId,
                    DocumentRevisionId = c.DocumentRevisionId,
                    CommunicationCode = c.CommunicationCode,
                    CommunicationStatus = c.CommunicationStatus,
                    CommunicationType = c.CommunicationType,
                    CompanyIssue = c.CompanyIssue,
                    CompanyIssueName = c.CompanyIssue == CompanyIssue.Customer && c.Customer != null ? c.Customer.Name :
                    c.CompanyIssue == CompanyIssue.Supplier && c.Supplier != null ? c.Supplier.Name :
                    c.CompanyIssue == CompanyIssue.Consultant && c.Consultant != null ? c.Consultant.Name :(authenticate.language=="en")?"Internal": "داخلی",
                    CreateDate = c.CreatedDate.ToUnixTimestamp(),

                }).ToListAsync();

                return commentList;
            }
            catch (Exception exception)
            {
                return new List<RevisionCommunicationListDto>();
            }
        }

        private async Task<List<RevisionCommunicationListDto>> GetAllRevisionCommunicationTQAndNCRForCustomerUserListAsync(AuthenticateDto authenticate, long documentRevisionId)
        {
            try
            {
                var dbQuery = _documentTQNCRRepository
                    .AsNoTracking()
                    .Where(a => a.DocumentRevision.Document.ContractCode == authenticate.ContractCode &&
                    (a.CompanyIssue == CompanyIssue.Customer || a.CompanyIssue == CompanyIssue.Consultant) &&
                    a.DocumentRevisionId == documentRevisionId);

                var commentList = await dbQuery.Select(c => new RevisionCommunicationListDto
                {
                    CommunicationId = c.DocumentTQNCRId,
                    DocumentRevisionId = c.DocumentRevisionId,
                    CommunicationCode = c.CommunicationCode,
                    CommunicationStatus = c.CommunicationStatus,
                    CommunicationType = c.CommunicationType,
                    CompanyIssue = c.CompanyIssue,
                    CompanyIssueName = c.CompanyIssue == CompanyIssue.Customer && c.Customer != null ? c.Customer.Name :
                    c.CompanyIssue == CompanyIssue.Supplier && c.Supplier != null ? c.Supplier.Name :
                     c.CompanyIssue == CompanyIssue.Consultant && c.Consultant != null ? c.Consultant.Name : "داخلی",
                    CreateDate = c.CreatedDate.ToUnixTimestamp(),

                }).ToListAsync();

                return commentList;
            }
            catch (Exception exception)
            {
                return new List<RevisionCommunicationListDto>();
            }
        }
        private async Task<List<RevisionCommunicationListDto>> GetAllRevisionCommunicationCommentListAsync(AuthenticateDto authenticate, PermissionResultDto permission, long documentRevisionId)
        {
            try
            {
                var dbQuery = _communicationRepository
                 .AsNoTracking()
                 .Where(a => a.DocumentRevision.Document.ContractCode == authenticate.ContractCode &&
                  a.DocumentRevisionId == documentRevisionId);

                if (permission.DocumentGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.DocumentGroupIds.Contains(a.DocumentRevision.Document.DocumentGroupId));

                var commentList = await dbQuery.Select(c => new RevisionCommunicationListDto
                {
                    CommunicationCode = c.CommunicationCode,
                    CommunicationId = c.DocumentCommunicationId,
                    DocumentRevisionId = c.DocumentRevisionId,
                    CommunicationStatus = c.CommunicationStatus,
                    CommunicationType = c.CommunicationType,
                    CompanyIssue = CompanyIssue.Customer,
                    CompanyIssueName = c.Customer.Name,
                    CreateDate = c.CreatedDate.ToUnixTimestamp(),
                }).ToListAsync();

                return commentList;
            }
            catch (Exception exception)
            {
                return new List<RevisionCommunicationListDto>();
            }
        }

        private async Task<List<RevisionCommunicationListDto>> GetAllRevisionCommunicationCommentForCustomerUserListAsync(AuthenticateDto authenticate, long documentRevisionId)
        {
            try
            {
                var dbQuery = _communicationRepository
                 .AsNoTracking()
                 .Where(a => a.DocumentRevision.Document.ContractCode == authenticate.ContractCode &&
                 (a.CompanyIssue==CompanyIssue.Customer||a.CompanyIssue==CompanyIssue.Consultant)&&
                  a.DocumentRevisionId == documentRevisionId);

                

                var commentList = await dbQuery.Select(c => new RevisionCommunicationListDto
                {
                    CommunicationCode = c.CommunicationCode,
                    CommunicationId = c.DocumentCommunicationId,
                    DocumentRevisionId = c.DocumentRevisionId,
                    CommunicationStatus = c.CommunicationStatus,
                    CommunicationType = c.CommunicationType,
                    CompanyIssue = CompanyIssue.Customer,
                    CompanyIssueName = c.CompanyIssue == CompanyIssue.Customer && c.Customer != null ? c.Customer.Name :
                     c.CompanyIssue == CompanyIssue.Consultant && c.Consultant != null ? c.Consultant.Name : "",
                    CreateDate = c.CreatedDate.ToUnixTimestamp(),
                }).ToListAsync();

                return commentList;
            }
            catch (Exception exception)
            {
                return new List<RevisionCommunicationListDto>();
            }
        }
        public async Task<ServiceResult<PendingReplayCommunicationDTO>> GetPendingReplyCommunicationBadgeAsync(AuthenticateDto authenticate)
        {
            try
            {
                PendingReplayCommunicationDTO result = new PendingReplayCommunicationDTO();
                authenticate.Roles = new List<string> { SCMRole.ComCommentReply, SCMRole.ComCommentMng };
                result.PendingCommentReply = await GetPendingReplyCommunicationCommentBadgeAsync(authenticate);

                authenticate.Roles = new List<string> { SCMRole.TQReply, SCMRole.TQMng };
                result.PendingTQReply = await GetPendingReplyCommunicationTQBadgeAsync(authenticate);

                authenticate.Roles = new List<string> { SCMRole.NCRReply, SCMRole.NCRMng };
                result.PendingNCRReply = await GetPendingReplyCommunicationNCRBadgeAsync(authenticate);

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<PendingReplayCommunicationDTO>(new PendingReplayCommunicationDTO(), exception);
            }
        }

        private async Task<int> GetPendingReplyCommunicationCommentBadgeAsync(AuthenticateDto authenticate)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return 0;

                var dbQuery = _communicationRepository
                   .AsNoTracking()
                   .Where(a => a.CommunicationStatus == DocumentCommunicationStatus.PendingReply &&
                   a.DocumentRevision.Document.ContractCode == authenticate.ContractCode);

                if (permission.DocumentGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.DocumentGroupIds.Contains(a.DocumentRevision.Document.DocumentGroupId));

                return await dbQuery.CountAsync();
            }
            catch (Exception exception)
            {
                return 0;
            }
        }

        private async Task<int> GetPendingReplyCommunicationTQBadgeAsync(AuthenticateDto authenticate)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return 0;

                var dbTQNCRQuery = _documentTQNCRRepository
                   .AsNoTracking()
                   .Where(a => a.CommunicationType == CommunicationType.TQ &&
                   a.CommunicationStatus == DocumentCommunicationStatus.PendingReply &&
                   a.DocumentRevision.Document.ContractCode == authenticate.ContractCode);

                if (permission.DocumentGroupIds.Any())
                    dbTQNCRQuery = dbTQNCRQuery.Where(a => permission.DocumentGroupIds.Contains(a.DocumentRevision.Document.DocumentGroupId));

                return await dbTQNCRQuery.CountAsync();

            }
            catch (Exception exception)
            {
                return 0;
            }
        }

        private async Task<int> GetPendingReplyCommunicationNCRBadgeAsync(AuthenticateDto authenticate)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return 0;

                var dbTQNCRQuery = _documentTQNCRRepository
                   .AsNoTracking()
                   .Where(a => a.CommunicationType == CommunicationType.NCR &&
                   a.CommunicationStatus == DocumentCommunicationStatus.PendingReply &&
                   a.DocumentRevision.Document.ContractCode == authenticate.ContractCode);

                if (permission.DocumentGroupIds.Any())
                    dbTQNCRQuery = dbTQNCRQuery.Where(a => permission.DocumentGroupIds.Contains(a.DocumentRevision.Document.DocumentGroupId));

                return await dbTQNCRQuery.CountAsync();

            }
            catch (Exception exception)
            {
                return 0;
            }
        }

        //public async Task<ServiceResult<int>> GetPendingReplyCommunicationBadgeForDashbourdAsync(AuthenticateDto authenticate)
        //{
        //    try
        //    {
        //        authenticate.Roles = new List<string> {
        //            SCMRole.ComCommentReply,
        //            SCMRole.ComCommentMng
        //        };
        //        var commentList = await GetPendingReplyCommunicationCommentBadgeAsync(authenticate);

        //        authenticate.Roles = new List<string> {
        //            SCMRole.TQReply,
        //            SCMRole.TQMng
        //        };
        //        var tqList = await GetPendingReplyCommunicationTQBadgeAsync(authenticate);

        //        authenticate.Roles = new List<string> {
        //            SCMRole.NCRReply,
        //            SCMRole.NCRMng
        //        };
        //        var ncrList = await GetPendingReplyCommunicationNCRBadgeAsync(authenticate);

        //        return ServiceResultFactory.CreateSuccess(commentList + tqList + ncrList);
        //    }
        //    catch (Exception exception)
        //    {
        //        return ServiceResultFactory.CreateSuccess(0);
        //    }
        //}

        public async Task<ServiceResult<Dictionary<int,int>>> GetPendingReplyCommunicationBadgeForDashbourdAsync(AuthenticateDto authenticate)
        {
            try
            {
                Dictionary<int, int> result = new Dictionary<int, int>();
                authenticate.Roles = new List<string> {
                    SCMRole.ComCommentReply,
                    SCMRole.ComCommentMng
                };

                var commentList = await GetPendingReplyCommunicationCommentBadgeAsync(authenticate);
                result.Add(1, commentList);
                authenticate.Roles = new List<string> {
                    SCMRole.TQReply,
                    SCMRole.TQMng
                };
                var tqList = await GetPendingReplyCommunicationTQBadgeAsync(authenticate);
                result.Add(2, tqList);
                authenticate.Roles = new List<string> {
                    SCMRole.NCRReply,
                    SCMRole.NCRMng
                };
                var ncrList = await GetPendingReplyCommunicationNCRBadgeAsync(authenticate);
                result.Add(3, ncrList);
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<Dictionary<int,int>>(null,exception);
            }
        }
    }
}
