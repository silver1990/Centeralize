using Exon.TheWeb.Service.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Raybod.SCM.DataAccess.Core;
using Raybod.SCM.DataAccess.Extention;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject._PanelDocument.Area;
using Raybod.SCM.DataTransferObject.Audit;
using Raybod.SCM.DataTransferObject.Document;
using Raybod.SCM.DataTransferObject.Notification;
using Raybod.SCM.DataTransferObject.User;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Model;
using Raybod.SCM.Domain.Struct;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Utility.Common;
using Raybod.SCM.Utility.EnumType;
using Raybod.SCM.Utility.Extention;
using Raybod.SCM.Utility.FileHelper;
using Raybod.SCM.Utility.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;


namespace Raybod.SCM.Services.Implementation
{
    public class DocumentRevisionService : IDocumentRevisionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;
        private readonly IContractFormConfigService _formConfigService;
        private readonly DbSet<Document> _documentRepository;
        private readonly DbSet<DocumentRevision> _documentRevisionRepository;
        private readonly DbSet<RevisionAttachment> _revisionAttachmentRepository;
        private readonly DbSet<Transmittal> _transmittalRepository;
        private readonly DbSet<Product> _productRepository;
        private readonly ITeamWorkAuthenticationService _authenticationServices;
        private readonly ISCMLogAndNotificationService _scmLogAndNotificationService;
        private readonly Utilitys.FileHelper _fileHelper;
        private readonly CompanyConfig _appSettings;
        private readonly IWebHostEnvironment _hostingEnvironmentRoot;

        public DocumentRevisionService(IUnitOfWork unitOfWork,
            IWebHostEnvironment hostingEnvironmentRoot,
            IOptions<CompanyAppSettingsDto> appSettings,
            ITeamWorkAuthenticationService authenticationServices,
            IHttpContextAccessor httpContextAccessor,
            ISCMLogAndNotificationService scmLogAndNotificationService,
            IFileService fileService,
            IContractFormConfigService formConfigService)
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
            _authenticationServices = authenticationServices;
            _formConfigService = formConfigService;
            _scmLogAndNotificationService = scmLogAndNotificationService;
            _documentRepository = _unitOfWork.Set<Document>();
            _documentRevisionRepository = _unitOfWork.Set<DocumentRevision>();
            _transmittalRepository = _unitOfWork.Set<Transmittal>();
            _productRepository = _unitOfWork.Set<Product>();
            _revisionAttachmentRepository = _unitOfWork.Set<RevisionAttachment>();
            _fileHelper = new Utilitys.FileHelper(hostingEnvironmentRoot);
            _hostingEnvironmentRoot = hostingEnvironmentRoot;
            httpContextAccessor.HttpContext.Request.Headers.TryGetValue("companyCode", out var CompanyCode);
            _appSettings = appSettings.Value.CompanyConfig.First(a => a.CompanyCode == CompanyCode);
        }

        public async Task<RevisionDashboardBadgeDto> GetRevisionDashboardBadgeAsync(AuthenticateDto authenticate)
        {
            var result = new RevisionDashboardBadgeDto();
            try
            {
                authenticate.Roles = new List<string> {
                    SCMRole.RevisionActivityMng,
                    SCMRole.RevisionMng,
                    SCMRole.RevisionObs,
                    SCMRole.RevisionGlbObs,
                    SCMRole.RevisionCreator,
                };

                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (permission.HasPermission)
                {

                    var dbQuery = _documentRevisionRepository
                        .AsNoTracking()
                        .Where(a => !a.IsDeleted &&
                        a.Document.IsActive &&
                        a.Document.ContractCode == authenticate.ContractCode);

                    if (permission.DocumentGroupIds.Any())
                        dbQuery = dbQuery.Where(c => permission.DocumentGroupIds.Contains(c.Document.DocumentGroupId));

                    result.InProgressRevision = await dbQuery
                        .CountAsync(a => a.RevisionStatus == RevisionStatus.InProgress ||
                        a.RevisionStatus == RevisionStatus.PendingForModify);
                }

                authenticate.Roles = new List<string> {
                    SCMRole.RevisionMng,
                    SCMRole.RevisionObs,
                    SCMRole.RevisionGlbObs,
                    SCMRole.RevisionConfirmMng,
                    SCMRole.RevisionConfirmGlbMng,
                    SCMRole.RevisionCreator,
                };

                var permission3 = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (permission3.HasPermission)
                {

                    var dbQuery = _documentRevisionRepository
                        .AsNoTracking()
                        .Where(a => !a.IsDeleted &&
                        a.Document.IsActive &&
                        a.Document.ContractCode == authenticate.ContractCode);

                    if (permission3.DocumentGroupIds.Any())
                        dbQuery = dbQuery.Where(c => permission3.DocumentGroupIds.Contains(c.Document.DocumentGroupId));

                    result.PendingConfirmationRevision = await dbQuery
                        .CountAsync(a => a.RevisionStatus == RevisionStatus.PendingConfirm);

                }
                authenticate.Roles = new List<string> {
                    SCMRole.TransmittalMng,
                    SCMRole.TransmittalObs,
                    SCMRole.TransmittalLimitedObs,
                    SCMRole.TransmittalLimitedGlbObs
                };

                var permission2 = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (permission2.HasPermission)
                {
                    var dbQuery = _documentRevisionRepository
                        .AsNoTracking()
                        .Where(a => !a.IsDeleted &&
                        a.Document.IsRequiredTransmittal &&
                        a.Document.IsActive &&
                        a.RevisionStatus == RevisionStatus.Confirmed &&
                        (a.TransmittalRevisions == null || !a.TransmittalRevisions.Any()) &&
                        a.Document.ContractCode == authenticate.ContractCode);

                    if (permission2.DocumentGroupIds.Any())
                        dbQuery = dbQuery.Where(c => permission.DocumentGroupIds.Contains(c.Document.DocumentGroupId));

                    result.PendingTransmittalRevision = await dbQuery
                        .CountAsync();
                }

                return result;
            }
            catch (Exception exception)
            {
                return result;
            }
        }

        public async Task<ServiceResult<PendingRevisionBadgeCountDto>> GetPendingRevisonBadgeCountAsync(AuthenticateDto authenticate)
        {
            try
            {
                var result = new PendingRevisionBadgeCountDto();

                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<PendingRevisionBadgeCountDto>(null, MessageId.AccessDenied);

                var dbcQuery = _documentRevisionRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted &&
                    !a.Document.IsDeleted &&
                    a.Document.IsActive &&
                    a.Document.ContractCode == authenticate.ContractCode);

                if (permission.DocumentGroupIds.Any())
                    dbcQuery = dbcQuery.Where(c => permission.DocumentGroupIds.Contains(c.Document.DocumentGroupId));

                result.InProgressRevisionCount = await dbcQuery
                    .CountAsync(a => a.RevisionStatus == RevisionStatus.InProgress ||
                    a.RevisionStatus == Domain.Enum.RevisionStatus.PendingForModify);

                result.PendingConfirmRevisionCount = await dbcQuery
                    .CountAsync(a => a.RevisionStatus == RevisionStatus.PendingConfirm);

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<PendingRevisionBadgeCountDto>(null, exception);
            }
        }

        #region revision
        public async Task<ServiceResult<InProgressRevisionListDto>> AddDocumentRevisionAsync(AuthenticateDto authenticate, long documentId, AddDocumentRevisionDto model)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<InProgressRevisionListDto>(null, MessageId.AccessDenied);

                var dbQuery = _documentRepository
                    .Where(a => !a.IsDeleted &&
                    a.ContractCode == authenticate.ContractCode &&
                    a.DocumentId == documentId);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<InProgressRevisionListDto>(null, MessageId.EntityDoesNotExist);

                if (dbQuery.Any(a => a.DocumentRevisions.Any(c => !c.IsDeleted && (c.RevisionStatus == RevisionStatus.InProgress || c.RevisionStatus == RevisionStatus.PendingForModify || c.RevisionStatus == RevisionStatus.PendingConfirm))))
                    return ServiceResultFactory.CreateError<InProgressRevisionListDto>(null, MessageId.ImposibleAddRevision);

                if (permission.DocumentGroupIds.Any() && !dbQuery.Any(a => permission.DocumentGroupIds.Contains(a.DocumentGroupId)))
                    return ServiceResultFactory.CreateError<InProgressRevisionListDto>(null, MessageId.AccessDenied);

                var documentModel = await dbQuery
                    .Include(a => a.DocumentRevisions)
                .FirstOrDefaultAsync();

                if (documentModel == null)
                    return ServiceResultFactory.CreateError<InProgressRevisionListDto>(null, MessageId.EntityDoesNotExist);

                if (documentModel.DocumentRevisions.Any(a => !a.IsDeleted && a.RevisionStatus == RevisionStatus.InProgress))
                    return ServiceResultFactory.CreateError<InProgressRevisionListDto>(null, MessageId.DataInconsistency);

                if (documentModel.DocumentRevisions.Any(a => !a.IsDeleted) && string.IsNullOrEmpty(model.Reason))
                    return ServiceResultFactory.CreateError<InProgressRevisionListDto>(null, MessageId.RequiredRevisionDescription);
                else if (string.IsNullOrEmpty(model.Reason))
                    model.Reason = "ویرایش اولیه";

                var revisionModel = new DocumentRevision();

                var deactiveRevision = await _documentRevisionRepository
                    .Where(a => !a.IsDeleted &&
                    a.DocumentId == documentId &&
                    a.RevisionStatus == RevisionStatus.DeActive)
                    .FirstOrDefaultAsync();

                if (deactiveRevision != null)
                {
                    revisionModel = deactiveRevision;

                    revisionModel.AdderUserId = authenticate.UserId;
                    revisionModel.CreatedDate = DateTime.UtcNow;
                    revisionModel.IsLastRevision = true;
                    revisionModel.RevisionStatus = RevisionStatus.InProgress;
                    revisionModel.Description = model.Reason;
                    revisionModel.DateEnd = model.DateEnd.UnixTimestampToDateTime();

                    var lastRevisionModels = await _documentRevisionRepository
                    .Where(a => !a.IsDeleted &&
                    a.DocumentId == documentId &&
                    a.IsLastRevision)
                    .ToListAsync();
                    foreach (var item in lastRevisionModels)
                    {
                        item.IsLastRevision = false;
                    }
                }
                else
                {
                    revisionModel.DocumentId = documentId;
                    revisionModel.IsLastRevision = true;
                    revisionModel.Document = documentModel;
                    revisionModel.RevisionStatus = RevisionStatus.InProgress;
                    revisionModel.Description = model.Reason;
                    revisionModel.DateEnd = model.DateEnd.UnixTimestampToDateTime();

                    var beforeLasrRevisions = await _documentRevisionRepository
                        .Where(a => !a.IsDeleted && a.IsLastRevision && a.DocumentId == documentId)
                        .ToListAsync();
                    foreach (var item in beforeLasrRevisions)
                    {
                        item.IsLastRevision = false;
                    }
                    // generate ReceiptNumber 
                    var totalCount = await _documentRevisionRepository
                        .CountAsync(a => !a.IsDeleted && a.DocumentId == documentId);
                    ServiceResult<string> codeRes;
                    var count = totalCount;
                    while (true)
                    {
                        codeRes = await _formConfigService.GenerateFormCodeAsync(authenticate.ContractCode, FormName.DocumentRevision, count);
                        if (!codeRes.Succeeded)
                            return ServiceResultFactory.CreateError(new InProgressRevisionListDto(), codeRes.Messages.First().Message);
                        if (documentModel.DocumentRevisions.Any(a =>!a.IsDeleted && a.DocumentRevisionCode == codeRes.Result))
                            count++;
                        else
                        {
                            break;
                        }
                        
                    }

                    revisionModel.DocumentRevisionCode = codeRes.Result;
                    _documentRevisionRepository.Add(revisionModel);
                }

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var res = await GetDocumentRevisionAsync(revisionModel.DocumentRevisionId);
                    var res1 = await _scmLogAndNotificationService.AddDocumentAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = documentModel.ContractCode,
                        Description = documentModel.DocTitle,
                        FormCode = revisionModel.DocumentRevisionCode,
                        Message = documentModel.DocNumber,
                        KeyValue = revisionModel.DocumentRevisionId.ToString(),
                        NotifEvent = NotifEvent.AddRevision,
                        RootKeyValue = revisionModel.DocumentRevisionId.ToString(),
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,
                        DocumentGroupId = documentModel.DocumentGroupId
                    },
                    documentModel.DocumentGroupId,
                    new List<NotifToDto> {
                    new NotifToDto
                    {
                        NotifEvent= NotifEvent.AddRevision,
                        Roles= new List<string>
                        {
                           SCMRole.RevisionMng
                        }
                    }
                    });
                    return ServiceResultFactory.CreateSuccess(res.Result);
                }
                return ServiceResultFactory.CreateError<InProgressRevisionListDto>(null, MessageId.AddEntityFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<InProgressRevisionListDto>(null, exception);
            }
        }
        public async Task<ServiceResult<DocumentViewDto>> AddDocumentRevisionFromListAsync(AuthenticateDto authenticate, long documentId, AddDocumentRevisionDto model)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<DocumentViewDto>(null, MessageId.AccessDenied);

                var dbQuery = _documentRepository
                    .Where(a => !a.IsDeleted &&
                    a.ContractCode == authenticate.ContractCode &&
                    a.DocumentId == documentId);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<DocumentViewDto>(null, MessageId.EntityDoesNotExist);

                if (dbQuery.Any(a => a.DocumentRevisions.Any(c => !c.IsDeleted && (c.RevisionStatus == RevisionStatus.InProgress || c.RevisionStatus == RevisionStatus.PendingForModify || c.RevisionStatus == RevisionStatus.PendingConfirm))))
                    return ServiceResultFactory.CreateError<DocumentViewDto>(null, MessageId.ImposibleAddRevision);

                if (permission.DocumentGroupIds.Any() && !dbQuery.Any(a => permission.DocumentGroupIds.Contains(a.DocumentGroupId)))
                    return ServiceResultFactory.CreateError<DocumentViewDto>(null, MessageId.AccessDenied);

                var documentModel = await dbQuery
                    .Include(a => a.DocumentRevisions)
                .FirstOrDefaultAsync();

                if (documentModel == null)
                    return ServiceResultFactory.CreateError<DocumentViewDto>(null, MessageId.EntityDoesNotExist);

                if (documentModel.DocumentRevisions.Any(a => !a.IsDeleted && a.RevisionStatus == RevisionStatus.InProgress))
                    return ServiceResultFactory.CreateError<DocumentViewDto>(null, MessageId.DataInconsistency);

                if (documentModel.DocumentRevisions.Any(a => !a.IsDeleted) && string.IsNullOrEmpty(model.Reason))
                    return ServiceResultFactory.CreateError<DocumentViewDto>(null, MessageId.RequiredRevisionDescription);
                else if (string.IsNullOrEmpty(model.Reason))
                    model.Reason = "ویرایش اولیه";

                var revisionModel = new DocumentRevision();

                var deactiveRevision = await _documentRevisionRepository
                    .Where(a => !a.IsDeleted &&
                    a.DocumentId == documentId &&
                    a.RevisionStatus == RevisionStatus.DeActive)
                    .FirstOrDefaultAsync();

                if (deactiveRevision != null)
                {
                    revisionModel = deactiveRevision;

                    revisionModel.AdderUserId = authenticate.UserId;
                    revisionModel.CreatedDate = DateTime.UtcNow;
                    revisionModel.IsLastRevision = true;
                    revisionModel.RevisionStatus = RevisionStatus.InProgress;
                    revisionModel.Description = model.Reason;
                    revisionModel.DateEnd = model.DateEnd.UnixTimestampToDateTime();

                    var lastRevisionModels = await _documentRevisionRepository
                    .Where(a => !a.IsDeleted &&
                    a.DocumentId == documentId &&
                    a.IsLastRevision)
                    .ToListAsync();
                    foreach (var item in lastRevisionModels)
                    {
                        item.IsLastRevision = false;
                    }
                }
                else
                {
                    revisionModel.DocumentId = documentId;
                    revisionModel.IsLastRevision = true;
                    revisionModel.Document = documentModel;
                    revisionModel.RevisionStatus = RevisionStatus.InProgress;
                    revisionModel.Description = model.Reason;
                    revisionModel.DateEnd = model.DateEnd.UnixTimestampToDateTime();

                    var beforeLasrRevisions = await _documentRevisionRepository
                        .Where(a => !a.IsDeleted && a.IsLastRevision && a.DocumentId == documentId)
                        .ToListAsync();
                    foreach (var item in beforeLasrRevisions)
                    {
                        item.IsLastRevision = false;
                    }
                    // generate ReceiptNumber 
                    var totalCount = await _documentRevisionRepository
                        .CountAsync(a => !a.IsDeleted && a.DocumentId == documentId);
                    ServiceResult<string> codeRes;
                    var count = totalCount;
                    while (true)
                    {
                        codeRes = await _formConfigService.GenerateFormCodeAsync(authenticate.ContractCode, FormName.DocumentRevision, count);
                        if (!codeRes.Succeeded)
                            return ServiceResultFactory.CreateError(new DocumentViewDto(), codeRes.Messages.First().Message);
                        if (documentModel.DocumentRevisions.Any(a =>!a.IsDeleted && a.DocumentRevisionCode == codeRes.Result))
                            count++;
                        else
                        {
                            break;
                        }
                        
                    }

                    revisionModel.DocumentRevisionCode = codeRes.Result;
                    _documentRevisionRepository.Add(revisionModel);
                }

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var res = await GetDocumentRevisionAsync(revisionModel.DocumentRevisionId);
                    var res1 = await _scmLogAndNotificationService.AddDocumentAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = documentModel.ContractCode,
                        Description = documentModel.DocTitle,
                        FormCode = revisionModel.DocumentRevisionCode,
                        Message = documentModel.DocNumber,
                        KeyValue = revisionModel.DocumentRevisionId.ToString(),
                        NotifEvent = NotifEvent.AddRevision,
                        RootKeyValue = revisionModel.DocumentRevisionId.ToString(),
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,
                        DocumentGroupId = documentModel.DocumentGroupId
                    },
                    documentModel.DocumentGroupId,
                    new List<NotifToDto> {
                    new NotifToDto
                    {
                        NotifEvent= NotifEvent.AddRevision,
                        Roles= new List<string>
                        {
                           SCMRole.RevisionMng
                        }
                    }
                    });
                    var result = await GetDocumentByIdAsync(authenticate, documentId);
                    if(result.Succeeded)
                        return ServiceResultFactory.CreateSuccess(result.Result);
                    return ServiceResultFactory.CreateSuccess(new DocumentViewDto());
                }
                return ServiceResultFactory.CreateError<DocumentViewDto>(null, MessageId.AddEntityFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<DocumentViewDto>(null, exception);
            }
        }

        public async Task<ServiceResult<bool>> EditDocumentRevisionAsync(AuthenticateDto authenticate,long documentRevisionId, EditRevisionDto model)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery =  _documentRevisionRepository
                    .Include(a=>a.Document)
                    .Where(a => !a.IsDeleted &&
                    a.Document.ContractCode == authenticate.ContractCode &&
                    a.DocumentRevisionId == documentRevisionId);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (dbQuery.Any(a => a.Document.DocumentRevisions.Any(b=>!b.IsDeleted&&b.DocumentRevisionCode==model.Code&&b.DocumentRevisionId!=documentRevisionId)))
                    return ServiceResultFactory.CreateError(false, MessageId.DuplicatRevisionNumber);

                if (permission.DocumentGroupIds.Any() && !dbQuery.Any(a => permission.DocumentGroupIds.Contains(a.Document.DocumentGroupId)))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var revisionModel = await dbQuery.FirstOrDefaultAsync();

                revisionModel.DocumentRevisionCode = model.Code;
                revisionModel.Description = model.Reason;
                revisionModel.DateEnd = model.DateEnd.UnixTimestampToDateTime();
                

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

       

        public async Task<ServiceResult<List<InProgressRevisionListDto>>> GetInprogressDocumentRevisionAsync(AuthenticateDto authenticate, DocRevisionQueryDto query)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<InProgressRevisionListDto>>(null, MessageId.AccessDenied);

                var dbQuery = _documentRevisionRepository
                     .AsNoTracking()
                     .Where(a => !a.IsDeleted &&
                     a.Document.IsActive &&
                    (a.RevisionStatus == RevisionStatus.InProgress || a.RevisionStatus == RevisionStatus.PendingForModify) &&
                     a.Document.ContractCode == authenticate.ContractCode);

                if (query.JustPendigModify)
                    dbQuery = dbQuery.Where(a => a.RevisionStatus == RevisionStatus.PendingForModify);

                if (permission.DocumentGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.DocumentGroupIds.Contains(a.Document.DocumentGroupId));

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a =>
                     a.Document.DocTitle.Contains(query.SearchText)
                     || a.Document.DocNumber.Contains(query.SearchText)
                     || a.Description.Contains(query.SearchText));

                if (EnumHelper.ValidateItem(query.DocClass))
                    dbQuery = dbQuery.Where(a => a.Document.DocClass == query.DocClass);


                if (query.DocumentGroupIds != null && query.DocumentGroupIds.Any())
                    dbQuery = dbQuery.Where(a => query.DocumentGroupIds.Contains(a.Document.DocumentGroupId));

                var totalCount = dbQuery.Count();

                var columnsMap = new Dictionary<string, Expression<Func<DocumentRevision, object>>>
                {
                    ["DocumentRevisionId"] = v => v.DocumentRevisionId
                };

                dbQuery = dbQuery.ApplayOrdering(query, columnsMap).ApplayPageing(query);

                var result = await dbQuery.Select(a => new InProgressRevisionListDto
                {
                    DocumentId = a.DocumentId,
                    DocumentRevisionId = a.DocumentRevisionId,
                    DocumentRevisionCode = a.DocumentRevisionCode,
                    Description = a.Description,
                    RevisionStatus = a.RevisionStatus,
                    DateEnd = a.DateEnd.ToUnixTimestamp(),
                    DocTitle = a.Document.DocTitle,
                    DocNumber = a.Document.DocNumber,
                    DocClass = a.Document.DocClass,
                    DocumentGroupCode = a.Document.DocumentGroup.DocumentGroupCode,
                    DocumentGroupTitle = a.Document.DocumentGroup.Title,
                    UserAudit = a.AdderUser != null ? new UserAuditLogDto
                    {
                        CreateDate = a.CreatedDate.ToUnixTimestamp(),
                        AdderUserName = a.AdderUser.FullName,
                        AdderUserImage = a.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + a.AdderUser.Image : ""
                    } : null,
                    ActivityUsers = a.RevisionActivities != null ? a.RevisionActivities
                    .Where(a => !a.IsDeleted)
                    .Select(c => new UserMentionDto
                    {
                        Id = c.ActivityOwnerId,
                        Display = c.ActivityOwner.FullName,
                        Image = c.ActivityOwner.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + c.ActivityOwner.Image : ""
                    }).ToList() : new List<UserMentionDto>()
                }).ToListAsync();


                foreach (var item in result)
                {
                    if (item.ActivityUsers != null && item.ActivityUsers.Any())
                        item.ActivityUsers = item.ActivityUsers.GroupBy(a => a.Id).Select(v => v.First()).ToList();
                }

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<InProgressRevisionListDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<bool>> DeActiveRevisionAsync(AuthenticateDto authenticate, long documentId, long revisoinId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var revisionModel = await _documentRevisionRepository
                     .Where(a => !a.IsDeleted &&
                     a.DocumentRevisionId == revisoinId &&
                     a.DocumentId == documentId &&
                     a.Document.ContractCode == authenticate.ContractCode &&
                     a.Document.IsActive &&
                     (a.RevisionStatus == RevisionStatus.InProgress || a.RevisionStatus == RevisionStatus.PendingForModify))
                     .Include(a => a.Document)
                     .FirstOrDefaultAsync();

                if (revisionModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (permission.DocumentGroupIds.Any() && !permission.DocumentGroupIds.Contains(revisionModel.Document.DocumentGroupId))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                revisionModel.RevisionStatus = RevisionStatus.DeActive;

                revisionModel.IsLastRevision = false;


                var getLastRevision = await _documentRevisionRepository.Where(a => !a.IsDeleted && a.DocumentId == documentId && a.IsLastRevision == false)
                    .OrderByDescending(a => a.DocumentRevisionId)
                    .FirstOrDefaultAsync();

                if (getLastRevision != null)
                    getLastRevision.IsLastRevision = true;

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    await _scmLogAndNotificationService
                        .SetDonedNotificationAsync(authenticate.UserId,
                        authenticate.ContractCode,
                        revisionModel.DocumentRevisionId.ToString(),
                        revisionModel.RevisionStatus == RevisionStatus.PendingForModify ? NotifEvent.RevisionReject : NotifEvent.AddRevision);

                    var res = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = authenticate.ContractCode,
                        FormCode = revisionModel.DocumentRevisionCode,
                        KeyValue = revisionModel.DocumentRevisionId.ToString(),
                        Description = revisionModel.Document.DocTitle,
                        NotifEvent = NotifEvent.RevisionDeActive,
                        RootKeyValue = revisionModel.DocumentId.ToString(),
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,
                        DocumentGroupId = revisionModel.Document.DocumentGroupId
                    }, null);
                    return ServiceResultFactory.CreateSuccess(true);
                }
                return ServiceResultFactory.CreateError(false, MessageId.SaveFailed);

            }
            catch (Exception exception)
            {

                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<DocumentRevisionDetailsDto>> GetDocumentRevisionByIdAsync(AuthenticateDto authenticate, long revisionId)
        {
            try
            {
                //var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                //if (!permission.HasPermission)
                //    return ServiceResultFactory.CreateError<DocumentRevisionDetailsDto>(null, MessageId.AccessDenied);

                var dbQuery = _documentRevisionRepository
                     .AsNoTracking()
                     .Where(a => !a.IsDeleted &&
                     a.Document.IsActive &&
                     a.Document.ContractCode == authenticate.ContractCode &&
                     a.DocumentRevisionId == revisionId);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<DocumentRevisionDetailsDto>(null, MessageId.EntityDoesNotExist);

                //if (permission.DocumentGroupIds.Any() && !dbQuery.Any(a => permission.DocumentGroupIds.Contains(a.Document.DocumentGroupId)))
                //    return ServiceResultFactory.CreateError<DocumentRevisionDetailsDto>(null, MessageId.AccessDenied);

                var result = await dbQuery.Select(a => new DocumentRevisionDetailsDto
                {
                    DocumentId = a.DocumentId,
                    ClientDocNumber = (a.Document.ClientDocNumber != null) ? a.Document.ClientDocNumber : "",
                    DocumentRevisionId = a.DocumentRevisionId,
                    DocumentRevisionCode = a.DocumentRevisionCode,
                    Description = a.Description,
                    RevisionStatus = a.RevisionStatus,
                    DateEnd = a.DateEnd.ToUnixTimestamp(),
                    DocTitle = a.Document.DocTitle,
                    DocNumber = a.Document.DocNumber,
                    DocClass = a.Document.DocClass,
                    DocumentGroupCode = a.Document.DocumentGroup.DocumentGroupCode,
                    DocumentGroupTitle = a.Document.DocumentGroup.Title,
                    UserAudit = a.AdderUser != null ? new UserAuditLogDto
                    {
                        CreateDate = a.CreatedDate.ToUnixTimestamp(),
                        AdderUserName = a.AdderUser.FullName,
                        AdderUserImage = a.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + a.AdderUser.Image : ""
                    } : null,
                    RevisionAvtivities = a.RevisionActivities.Where(a => !a.IsDeleted)
                    .Select(c => new BaseRevisionAvtivityDto
                    {
                        ActivityOwnerId = c.ActivityOwnerId,
                        DateEnd = c.DateEnd.ToUnixTimestamp(),
                        Description = c.Description,
                        Duration = $"{Math.Floor(c.Duration)}:{TimeSpan.FromHours(c.Duration).Minutes}",
                        RevisionActivityId = c.RevisionActivityId,
                        RevisionActivityStatus = c.RevisionActivityStatus,
                        ActivityOwner = c.ActivityOwner != null ? new UserAuditLogDto
                        {
                            AdderUserName = c.ActivityOwner.FullName,
                            AdderUserImage = c.ActivityOwner.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + c.ActivityOwner.Image : ""
                        } : null,
                    }).ToList()
                }).FirstOrDefaultAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<DocumentRevisionDetailsDto>(null, exception);
            }
        }

        private async Task<ServiceResult<InProgressRevisionListDto>> GetDocumentRevisionAsync(long revisionId)
        {
            try
            {
                var dbQuery = _documentRevisionRepository
                     .AsNoTracking()
                     .Where(a => a.DocumentRevisionId == revisionId);

                var result = await dbQuery.Select(a => new InProgressRevisionListDto
                {
                    DocumentId = a.DocumentId,
                    DocumentRevisionId = a.DocumentRevisionId,
                    DocumentRevisionCode = a.DocumentRevisionCode,
                    Description = a.Description,
                    RevisionStatus = a.RevisionStatus,
                    DateEnd = a.DateEnd.ToUnixTimestamp(),
                    DocTitle = a.Document.DocTitle,
                    DocNumber = a.Document.DocNumber,
                    DocClass = a.Document.DocClass,
                    DocumentGroupCode = a.Document.DocumentGroup.DocumentGroupCode,
                    DocumentGroupTitle = a.Document.DocumentGroup.Title,
                    ActivityUsers = new List<UserMentionDto>(),
                    UserAudit = a.AdderUser != null ? new UserAuditLogDto
                    {
                        CreateDate = a.CreatedDate.ToUnixTimestamp(),
                        AdderUserName = a.AdderUser.FullName,
                        AdderUserImage = a.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + a.AdderUser.Image : ""
                    } : null,
                }).FirstOrDefaultAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<InProgressRevisionListDto>(null, exception);
            }
        }

        public async Task<ServiceResult<List<RevisionAttachmentDto>>> AddDocumentRevisionAttachmentAsync(AuthenticateDto authenticate, long documentId,long revisionId, IFormFileCollection files)
        {
            try
            {
                if (files == null || !files.Any())
                    return ServiceResultFactory.CreateError<List<RevisionAttachmentDto>>(null, MessageId.FileNotFound);

                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<RevisionAttachmentDto>>(null, MessageId.AccessDenied);

                var dbQuery = _documentRevisionRepository
                    .AsNoTracking()
                     .Where(a => !a.IsDeleted &&
                     a.Document.IsActive &&
                     a.DocumentId == documentId &&
                     a.Document.ContractCode == authenticate.ContractCode &&
                     a.DocumentRevisionId == revisionId);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<List<RevisionAttachmentDto>>(null, MessageId.EntityDoesNotExist);

                if (permission.DocumentGroupIds.Any() && !dbQuery.Any(a => permission.DocumentGroupIds.Contains(a.Document.DocumentGroupId)))
                    return ServiceResultFactory.CreateError<List<RevisionAttachmentDto>>(null, MessageId.AccessDenied);

                var attachModels = new List<RevisionAttachment>();
                foreach (var item in files)
                {
                    var fileName = item.FileName;
                    var uploadResult = await _fileService.UploadDocumentFile(item);
                    if (!uploadResult.Succeeded)
                        return ServiceResultFactory.CreateError<List<RevisionAttachmentDto>>(null, uploadResult.Messages[0].Message);

                    var UploadedFile = await _fileHelper
                        .SaveDocumentFromTemp(uploadResult.Result, ServiceSetting.UploadFilePathDocument(authenticate.ContractCode, documentId, revisionId));

                    if (UploadedFile == null)
                        return ServiceResultFactory.CreateError<List<RevisionAttachmentDto>>(null, MessageId.UploudFailed);

                    _fileHelper.DeleteDocumentFromTemp(uploadResult.Result);

                    attachModels.Add(new RevisionAttachment
                    {
                        DocumentRevisionId = revisionId,
                        FileSrc = uploadResult.Result,
                        FileName = fileName,
                        RevisionAttachmentType = RevisionAttachmentType.Preparation,
                        FileType = UploadedFile.FileType,
                        FileSize = UploadedFile.FileSize
                    });
                }

                _revisionAttachmentRepository.AddRange(attachModels);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var res = attachModels.Select(c => new RevisionAttachmentDto
                    {
                        FileName = c.FileName,
                        FileSize = c.FileSize,
                        FileSrc = c.FileSrc,
                        FileType = c.FileType,
                        RevisionAttachmentId = c.RevisionAttachmentId
                    }).ToList();
                    return ServiceResultFactory.CreateSuccess(res);
                }
                return ServiceResultFactory.CreateError<List<RevisionAttachmentDto>>(null, MessageId.UploudFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<RevisionAttachmentDto>>(null, exception);

            }
        }

        public async Task<ServiceResult<List<RevisionAttachmentDto>>> GetDocumentRevisionAttachmentAsync(AuthenticateDto authenticate,
            long documentId, long revisionId, RevisionAttachmentType type)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(new List<RevisionAttachmentDto>(), MessageId.AccessDenied);

                if (type == RevisionAttachmentType.FinalNative && _appSettings.IsCheckedIP && !_appSettings.RemoteIPs.Contains(authenticate.RemoteIpAddress))
                    return ServiceResultFactory.CreateError(new List<RevisionAttachmentDto>(), MessageId.AccessDenied);

                var dbQuery = _revisionAttachmentRepository
                   .AsNoTracking()
                   .Where(a => !a.IsDeleted &&
                   a.DocumentRevisionId == revisionId &&
            
                   a.DocumentRevision.Document.IsActive &&
                   a.DocumentRevision.DocumentId == documentId &&
                   a.DocumentRevision.Document.ContractCode == authenticate.ContractCode);

                if (permission.DocumentGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.DocumentGroupIds.Contains(a.DocumentRevision.Document.DocumentGroupId));



                switch (type)
                {
                    case RevisionAttachmentType.Preparation:
                        dbQuery = dbQuery.Where(a => a.RevisionAttachmentType == type);
                        break;
                    case RevisionAttachmentType.Final:
                        dbQuery = dbQuery.Where(a => a.RevisionAttachmentType == type);
                        break;
                    case RevisionAttachmentType.FinalNative:
                        dbQuery = dbQuery.Where(a => a.RevisionAttachmentType == type);
                        break;
                    default:
                        dbQuery = dbQuery.Where(a => a.RevisionAttachmentType == RevisionAttachmentType.Preparation);
                        break;
                }

                var res = await dbQuery.Select(c => new RevisionAttachmentDto
                {
                    FileName = c.FileName,
                    FileSize = c.FileSize,
                    AttachType = c.RevisionAttachmentType,
                    FileSrc = c.FileSrc,
                    FileType = c.FileType,
                    RevisionAttachmentId = c.RevisionAttachmentId
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(res);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<RevisionAttachmentDto>>(null, exception);
            }
        }
        public async Task<ServiceResult<List<RevisionAttachmentDto>>> GetDocumentRevisionAttachmentForCustomerUserAsync(AuthenticateDto authenticate,
            long documentId, long revisionId, RevisionAttachmentType type,bool accessability)
        {
            try
            {
                if (!accessability)
                    return ServiceResultFactory.CreateError(new List<RevisionAttachmentDto>(), MessageId.AccessDenied);
               
                

                if (type == RevisionAttachmentType.FinalNative && _appSettings.IsCheckedIP && !_appSettings.RemoteIPs.Contains(authenticate.RemoteIpAddress))
                    return ServiceResultFactory.CreateError(new List<RevisionAttachmentDto>(), MessageId.AccessDenied);

                var dbQuery = _revisionAttachmentRepository
                   .AsNoTracking()
                   .Where(a => !a.IsDeleted &&
                   a.DocumentRevisionId == revisionId &&
            
                   a.DocumentRevision.Document.IsActive &&
                   a.DocumentRevision.DocumentId == documentId &&
                   a.DocumentRevision.Document.ContractCode == authenticate.ContractCode);

               



                switch (type)
                {
                    case RevisionAttachmentType.Preparation:
                        dbQuery = dbQuery.Where(a => a.RevisionAttachmentType == type);
                        break;
                    case RevisionAttachmentType.Final:
                        dbQuery = dbQuery.Where(a => a.RevisionAttachmentType == type);
                        break;
                    case RevisionAttachmentType.FinalNative:
                        dbQuery = dbQuery.Where(a => a.RevisionAttachmentType == type);
                        break;
                    default:
                        dbQuery = dbQuery.Where(a => a.RevisionAttachmentType == RevisionAttachmentType.Preparation);
                        break;
                }

                var res = await dbQuery.Select(c => new RevisionAttachmentDto
                {
                    FileName = c.FileName,
                    FileSize = c.FileSize,
                    AttachType = c.RevisionAttachmentType,
                    FileSrc = c.FileSrc,
                    FileType = c.FileType,
                    RevisionAttachmentId = c.RevisionAttachmentId
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(res);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<RevisionAttachmentDto>>(null, exception);
            }
        }

        public async Task<DownloadFileDto> DownloadRevisionFileAsync(AuthenticateDto authenticate, long docId, long revId, string fileSrc)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return null;

                var dbQuery = _revisionAttachmentRepository
                    .AsNoTracking()
                    .Where(a =>
                    !a.IsDeleted
                    && a.DocumentRevisionId == revId
                    && a.RevisionAttachmentType == RevisionAttachmentType.Preparation
                    && a.DocumentRevision.DocumentId == docId
                    && a.FileSrc == fileSrc);

                if (permission.DocumentGroupIds.Any() && !dbQuery.Any(a => permission.DocumentGroupIds.Contains(a.DocumentRevision.Document.DocumentGroupId)))
                    return null;

                if (!await dbQuery.AnyAsync())
                    return null;

                return await _fileHelper.DownloadDocument(fileSrc, ServiceSetting.UploadFilePathDocument(authenticate.ContractCode, docId, revId));
            }
            catch (Exception exception)
            {
                return null;
            }
        }
        

        public async Task<DownloadFileDto> DownloadRevisionFileAsync(AuthenticateDto authenticate, long revId, string fileSrc, RevisionAttachmentType type)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return null;

                if (type == RevisionAttachmentType.FinalNative && _appSettings.IsCheckedIP && !_appSettings.RemoteIPs.Contains(authenticate.RemoteIpAddress))
                    return null;

                var dbQuery = _revisionAttachmentRepository
                    .AsNoTracking()
                    .Where(a =>
                    !a.IsDeleted &&
                     a.DocumentRevisionId == revId &&
                    a.DocumentRevision.Document.ContractCode == authenticate.ContractCode &&
                    a.FileSrc == fileSrc);

                if (permission.DocumentGroupIds.Any() && !dbQuery.Any(a => permission.DocumentGroupIds.Contains(a.DocumentRevision.Document.DocumentGroupId)))
                    return null;
                switch (type)
                {
                    case RevisionAttachmentType.Preparation:
                        dbQuery = dbQuery.Where(a => a.RevisionAttachmentType == type);
                        break;
                    case RevisionAttachmentType.Final:
                        dbQuery = dbQuery.Where(a => a.RevisionAttachmentType == type);
                        break;
                    case RevisionAttachmentType.FinalNative:
                        dbQuery = dbQuery.Where(a => a.RevisionAttachmentType == type);
                        break;
                    default:
                        break;
                }


                if (!dbQuery.Any())
                    return null;

                var docuemntId = await dbQuery
                    .Select(a => a.DocumentRevision.DocumentId)
                    .FirstOrDefaultAsync();
                return await _fileHelper.DownloadDocument(fileSrc, ServiceSetting.UploadFilePathDocument(authenticate.ContractCode, docuemntId, revId));
            }
            catch (Exception exception)
            {
                return null;
            }
        }

        public async Task<DownloadFileDto> DownloadRevisionFileAsync(AuthenticateDto authenticate, long revId)
        {
            try
            {
                authenticate.Roles = new List<string> {SCMRole.DocumentArchiveObs};
                PermissionResultDto permissionNative = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                PermissionResultDto permissionFinal=null;
                if (!permissionNative.HasPermission)
                {
                    authenticate.Roles = new List<string> {SCMRole.DocumentArchiveLimitedObs,SCMRole.DocumentArchiveLimitedGlbObs};
                    permissionFinal = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                    if (!permissionFinal.HasPermission)
                        return null;
                }
                    

                //if (type == RevisionAttachmentType.FinalNative && _appSettings.IsCheckedIP && !_appSettings.RemoteIPs.Contains(authenticate.RemoteIpAddress))
                //    return null;

                var dbQuery = _revisionAttachmentRepository
                    .AsNoTracking()
                    .Where(a =>
                    !a.IsDeleted &&
                     a.DocumentRevisionId == revId &&
                    a.DocumentRevision.Document.ContractCode == authenticate.ContractCode );

                var revision = await _documentRevisionRepository.FirstOrDefaultAsync(a => a.DocumentRevisionId == revId);
                if (revision == null || (revision.RevisionStatus != RevisionStatus.Confirmed && revision.RevisionStatus != RevisionStatus.TransmittalASB && revision.RevisionStatus != RevisionStatus.TransmittalIFA && revision.RevisionStatus != RevisionStatus.TransmittalIFC && revision.RevisionStatus != RevisionStatus.TransmittalIFI))
                    return null;
                if (permissionNative.HasPermission&& permissionNative.DocumentGroupIds.Any() && !dbQuery.Any(a => permissionNative.DocumentGroupIds.Contains(a.DocumentRevision.Document.DocumentGroupId)))
                    return null;

                if (!permissionNative.HasPermission&&permissionFinal.HasPermission && permissionFinal.DocumentGroupIds.Any() && !dbQuery.Any(a => permissionFinal.DocumentGroupIds.Contains(a.DocumentRevision.Document.DocumentGroupId)))
                    return null;
               
                if(permissionNative.HasPermission)
                        dbQuery = dbQuery.Where(a => (a.RevisionAttachmentType == RevisionAttachmentType.FinalNative||a.RevisionAttachmentType==RevisionAttachmentType.Final));

                if (!permissionNative.HasPermission&&permissionFinal.HasPermission)
                    dbQuery = dbQuery.Where(a =>  a.RevisionAttachmentType == RevisionAttachmentType.Final);


                if (!dbQuery.Any())
                    return null;

                var docuemntId = await dbQuery
                    .Select(a => a.DocumentRevision.DocumentId)
                    .FirstOrDefaultAsync();
                List<InMemoryFileDto> nativeFile = new List<InMemoryFileDto>();
                List<InMemoryFileDto> files = new List<InMemoryFileDto>();
                string root = _hostingEnvironmentRoot.ContentRootPath;
                if (permissionNative.HasPermission)
                {
                    foreach(var native in dbQuery.Where(a => a.RevisionAttachmentType == RevisionAttachmentType.FinalNative))
                    {
                        nativeFile.Add(new InMemoryFileDto { FileSrc = root + ServiceSetting.UploadFilePathDocument(authenticate.ContractCode, docuemntId, revId) + native.FileSrc, FileName = native.FileName });
                    }

                    foreach (var file in dbQuery.Where(a => a.RevisionAttachmentType == RevisionAttachmentType.Final))
                    {
                        files.Add(new InMemoryFileDto { FileSrc = root + ServiceSetting.UploadFilePathDocument(authenticate.ContractCode, docuemntId, revId) + file.FileSrc, FileName = file.FileName });
                    }
                    return await _fileHelper.DownloadDocument(nativeFile,files);
                }

                foreach (var file in dbQuery.Where(a => a.RevisionAttachmentType == RevisionAttachmentType.Final))
                {
                    files.Add(new InMemoryFileDto { FileSrc = root + ServiceSetting.UploadFilePathDocument(authenticate.ContractCode, docuemntId, revId) + file.FileSrc, FileName = file.FileName });
                }
                return await _fileHelper.DownloadDocument(null, files);
            }
            catch (Exception exception)
            {
                return null;
            }
        }


        public async Task<DownloadFileDto> DownloadRevisionFileForCustomerAsync(AuthenticateDto authenticate, long revId,bool accessability)
        {
            try
            {
                if (!accessability)
                    return null;


                //if (type == RevisionAttachmentType.FinalNative && _appSettings.IsCheckedIP && !_appSettings.RemoteIPs.Contains(authenticate.RemoteIpAddress))
                //    return null;

                var dbQuery = _revisionAttachmentRepository
                    .AsNoTracking()
                    .Where(a =>
                    !a.IsDeleted &&
                     a.DocumentRevisionId == revId &&
                    a.DocumentRevision.Document.ContractCode == authenticate.ContractCode);

                var revision = await _documentRevisionRepository.FirstOrDefaultAsync(a => a.DocumentRevisionId == revId);
                if (revision == null || (revision.RevisionStatus != RevisionStatus.Confirmed && revision.RevisionStatus != RevisionStatus.TransmittalASB &&  revision.RevisionStatus != RevisionStatus.TransmittalIFA && revision.RevisionStatus != RevisionStatus.TransmittalIFC && revision.RevisionStatus != RevisionStatus.TransmittalIFI))
                    return null;
                

                    dbQuery = dbQuery.Where(a => a.RevisionAttachmentType == RevisionAttachmentType.Final);


                if (!dbQuery.Any())
                    return null;

                var docuemntId = await dbQuery
                    .Select(a => a.DocumentRevision.DocumentId)
                    .FirstOrDefaultAsync();
                List<InMemoryFileDto> nativeFile = new List<InMemoryFileDto>();
                List<InMemoryFileDto> files = new List<InMemoryFileDto>();
                string root = _hostingEnvironmentRoot.ContentRootPath;
               
                foreach (var file in dbQuery.Where(a => a.RevisionAttachmentType == RevisionAttachmentType.Final))
                {
                    files.Add(new InMemoryFileDto { FileSrc = root + ServiceSetting.UploadFilePathDocument(authenticate.ContractCode, docuemntId, revId) + file.FileSrc, FileName = file.FileName });
                }
                return await _fileHelper.DownloadDocument(null, files);
            }
            catch (Exception exception)
            {
                return null;
            }
        }



        private async Task<bool> SaveAchmentForTaransmittal(List<RevisionAttachmentTemp> attachments,string contractCode,string transmittalNumber, List<RevisionAttachmentTemp> documentDetails)
        {
            var result = new List<InMemoryFileDto>();
            try
            {


                foreach (var item in documentDetails)
                {
                    result.Add(new InMemoryFileDto
                    {
                        FileName = item.Revision.FileName,
                        FileSrc = item.Revision.FileSrc,
                        FileUrl = ServiceSetting.UploadFilePathDocumentTransMittal(contractCode),
                    });
                }

       
                    foreach (var item in attachments)
                    {
                        if (!item.Revision.IsDeleted && item.Revision.RevisionAttachmentType == RevisionAttachmentType.Final)
                        {
                            result.Add(new InMemoryFileDto
                            {
                                FileName = item.Revision.FileName,
                                FileSrc = item.Revision.FileSrc,
                                FileUrl = ServiceSetting.UploadFilePathDocument(contractCode, item.DocumentId, item.Revision.DocumentRevisionId.Value)
                            });

                        }
                    }
                
                string filePath = ServiceSetting.UploadFilePathDocumentTransMittal(contractCode);
                string fileName = transmittalNumber + ".zip";
                string fileSource = _fileHelper.FileReadSrc(fileName, filePath);
                return await _fileHelper.ToMemoryStreamZipFileAsync(result, fileSource);

            }
            catch (Exception ex)
            {
                //DebugLog log = new DebugLog();
                //log.StatusCode = (int)MessageId.FileNotFound;
                //log.Message = ex.Message;
                //log.InnerMessage = ex.InnerException != null ? ex.InnerException.Message : "";
                //log.StackTrace = ex.StackTrace;
                //log.CreateDate = DateTime.Now;
                //await _debugLogRepository.AddAsync(log);
                //await _unitOfWork.SaveChangesAsync();
                return false;
            }

        }
        public async Task<DownloadFileDto> DownloadRevisionFileForCustomerUserAsync(AuthenticateDto authenticate, long revId, string fileSrc, RevisionAttachmentType type,bool accessability)
        {
            try
            {
                if (!accessability)
                    return null;
                
                

                if (type == RevisionAttachmentType.FinalNative && _appSettings.IsCheckedIP && !_appSettings.RemoteIPs.Contains(authenticate.RemoteIpAddress))
                    return null;

                var dbQuery = _revisionAttachmentRepository
                    .AsNoTracking()
                    .Where(a =>
                    !a.IsDeleted &&
                     a.DocumentRevisionId == revId &&
                    a.DocumentRevision.Document.ContractCode == authenticate.ContractCode &&
                    a.FileSrc == fileSrc);

                
                switch (type)
                {
                    case RevisionAttachmentType.Preparation:
                        dbQuery = dbQuery.Where(a => a.RevisionAttachmentType == type);
                        break;
                    case RevisionAttachmentType.Final:
                        dbQuery = dbQuery.Where(a => a.RevisionAttachmentType == type);
                        break;
                    case RevisionAttachmentType.FinalNative:
                        dbQuery = dbQuery.Where(a => a.RevisionAttachmentType == type);
                        break;
                    default:
                        break;
                }


                if (!dbQuery.Any())
                    return null;

                var docuemntId = await dbQuery
                    .Select(a => a.DocumentRevision.DocumentId)
                    .FirstOrDefaultAsync();
                return await _fileHelper.DownloadDocument(fileSrc, ServiceSetting.UploadFilePathDocument(authenticate.ContractCode, docuemntId, revId));
            }
            catch (Exception exception)
            {
                return null;
            }
        }

        //public async Task<DownloadFileDto> DownloadRevisionFileAsync(AuthenticateDto authenticate, long docId, long revId, RevisionAttachmentType attachType, string fileSrc)
        //{
        //    try
        //    {
        //        //var permission = await _authenticationService.HasPermission(authenticate, teamWorkId);
        //        //if (!permission.HasPermission)
        //        //    return null;

        //        var dbQuery = _revisionAttachmentRepository
        //            .AsNoTracking()
        //            .Where(a =>
        //            !a.IsDeleted
        //            && a.DocumentRevisionId == revId
        //            && a.DocumentRevision.DocumentId == docId
        //            && a.RevisionAttachmentType == attachType
        //            && a.FileSrc == fileSrc);

        //        //if (permission.ProductGroupIds.Any())
        //        //    dbQuery = dbQuery.Where(a => permission.ProductGroupIds.Contains(a.RFQComment.RFQ.PR.ProductGroupId));

        //        if (!await dbQuery.AnyAsync())
        //            return null;

        //        return await _fileHelper.DownloadDocument(fileSrc, ServiceSetting.UploadFilePathDocument(authenticate.ContractCode, docId, revId));
        //    }
        //    catch (Exception exception)
        //    {
        //        return null;
        //    }
        //}


        public async Task<ServiceResult<bool>> DeleteDocumentRevisionAttachmentAsync(AuthenticateDto authenticate, long documentId, long revisionId, string fileSrc)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery = _revisionAttachmentRepository
                     .Where(a => !a.IsDeleted &&
                     a.DocumentRevisionId == revisionId &&
                     a.FileSrc == fileSrc &&
                     a.DocumentRevision.Document.IsActive &&
                     a.RevisionAttachmentType == RevisionAttachmentType.Preparation &&
                     a.DocumentRevision.DocumentId == documentId &&
                     a.DocumentRevision.Document.ContractCode == authenticate.ContractCode);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (permission.DocumentGroupIds.Any() && !dbQuery.Any(a => permission.DocumentGroupIds.Contains(a.DocumentRevision.Document.DocumentGroupId)))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var attachModel = await dbQuery.FirstOrDefaultAsync();
                if (attachModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                attachModel.IsDeleted = true;

                await _unitOfWork.SaveChangesAsync();

                return ServiceResultFactory.CreateSuccess(true);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);

            }
        }

        #endregion

        public async Task<ServiceResult<List<ProductDocumentDto>>> GetDocumentByProductIdBaseOnContractAsync(AuthenticateDto authenticate, int productId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<ProductDocumentDto>>(null, MessageId.AccessDenied);

                var dbQuery = _documentRepository
                     .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.DocumentProducts.Any(a => a.ProductId == productId));

                if (permission.ProductGroupIds.Any() && dbQuery.Any(c => c.DocumentProducts.Any()) &&
                    !dbQuery.Any(a => a.DocumentProducts.Any(c => permission.ProductGroupIds.Contains(c.Product.ProductGroupId))))
                    return ServiceResultFactory.CreateError<List<ProductDocumentDto>>(null, MessageId.AccessDenied);


                var result = await dbQuery
                    .Select(d => new ProductDocumentDto
                    {
                        DocumentId = d.DocumentId,
                        DocTitle = d.DocTitle,
                        DocNumber = d.DocNumber,
                        DocumentGroupTitle = d.DocumentGroup.Title,
                        ClientDocNumber = d.ClientDocNumber,
                        DocClass = d.DocClass,
                        HasRevision = d.DocumentRevisions.Any(a => !a.IsDeleted),
                        DocumentRevisionCode = d.DocumentRevisions
                        .Where(a => a.IsLastConfirmRevision)
                        .Select(c => c.DocumentRevisionCode)
                        .FirstOrDefault(),
                        LastRevisionDate = d.DocumentRevisions
                        .Where(a => a.IsLastConfirmRevision)
                        .Select(c => c.ConfirmationWorkFlows.Where(n => !n.IsDeleted && n.Status == ConfirmationWorkFlowStatus.Confirm).Select(n => n.UpdateDate).FirstOrDefault())
                        .FirstOrDefault()
                        .ToUnixTimestamp()
                    }).ToListAsync();
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<ProductDocumentDto>>(null, exception);
            }
        }

        public async Task<DownloadFileDto> GetLastDocumentRevisionAttachAzZipFileByProductIdAsync(AuthenticateDto authenticate, long documentId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return null;

                var dbQuery = _revisionAttachmentRepository
                     .AsNoTracking()
                    .Where(a => !a.IsDeleted &&
                    a.RevisionAttachmentType == RevisionAttachmentType.Final &&
                    a.DocumentRevision.DocumentId == documentId &&
                    a.DocumentRevision.IsLastConfirmRevision &&
                    !a.DocumentRevision.Document.IsDeleted &&
                    a.DocumentRevision.Document.IsActive &&
                    a.DocumentRevision.Document.ContractCode == authenticate.ContractCode &&
                    a.DocumentRevision.Document.DocumentProducts.Any());

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(c => c.DocumentRevision.Document.DocumentProducts.Any(v => permission.ProductGroupIds.Contains(v.Product.ProductGroupId))))
                    return null;

                var lasrRevision = await dbQuery
                    .Select(c => new
                    {
                        FileName = c.FileName,
                        FileSrc = c.FileSrc,
                        DocumentId = c.DocumentRevision.DocumentId,
                        DocumentRevisionId = c.DocumentRevisionId.Value,
                    }).ToListAsync();

                if (lasrRevision == null || !lasrRevision.Any())
                    return null;

                var files = new List<FinalRevisionAttachmentToZipDto>();
                foreach (var item in lasrRevision)
                {
                    files.Add(new FinalRevisionAttachmentToZipDto
                    {
                        FileName = item.FileName,
                        FileSrc = item.FileSrc,
                        FilePath = ServiceSetting.UploadFilePathDocument(authenticate.ContractCode, item.DocumentId, item.DocumentRevisionId)
                    });
                }


                return await _fileHelper.DownloadZipFileAsync(files);

            }
            catch (Exception exception)
            {
                return null;
            }
        }
        public async Task<DownloadFileDto> GetLastDocumentRevisionAttachAsZipFileByProductIdAsync(AuthenticateDto authenticate, long documentId,int productId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return null;

                var dbQuery = _revisionAttachmentRepository
                     .AsNoTracking()
                   
                    .Where(a => !a.IsDeleted &&
                    a.RevisionAttachmentType == RevisionAttachmentType.Final &&
                    a.DocumentRevision.DocumentId == documentId &&
                    a.DocumentRevision.IsLastConfirmRevision &&
                    !a.DocumentRevision.Document.IsDeleted &&
                    a.DocumentRevision.Document.IsActive &&
                    a.DocumentRevision.Document.ContractCode == authenticate.ContractCode &&
                    a.DocumentRevision.Document.DocumentProducts.Any());

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(c => c.DocumentRevision.Document.DocumentProducts.Any(v => permission.ProductGroupIds.Contains(v.Product.ProductGroupId))))
                    return null;

                var lasrRevision = await dbQuery
                    .Select(c => new
                    {
                        FileName = c.FileName,
                        FileSrc = c.FileSrc,
                        DocumentId = c.DocumentRevision.DocumentId,
                        DocumentRevisionId = c.DocumentRevisionId.Value,
                    }).ToListAsync();

                if (lasrRevision == null || !lasrRevision.Any())
                    return null;

                var files = new List<FinalRevisionAttachmentToZipDto>();
                foreach (var item in lasrRevision)
                {
                    files.Add(new FinalRevisionAttachmentToZipDto
                    {
                        FileName = item.FileName,
                        FileSrc = item.FileSrc,
                        FilePath = ServiceSetting.UploadFilePathDocument(authenticate.ContractCode, item.DocumentId, item.DocumentRevisionId)
                    });
                }


                var result= await _fileHelper.DownloadZipFileAsync(files);
                var product = await _productRepository.FirstOrDefaultAsync(a => a.Id == productId);
                if (product != null &&result!=null)
                    result.FileName = product.ProductCode + ".zip";
                return result;
            }
            catch (Exception exception)
            {
                return null;
            }
        }

        public async Task<DownloadFileDto> GetLastDocumentRevisionsAttachAsZipFileByProductIdAsync(AuthenticateDto authenticate,  int productId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return null;

                var dbQuery = _revisionAttachmentRepository
                     .AsNoTracking()
                    .Where(a => !a.IsDeleted &&
                    a.RevisionAttachmentType == RevisionAttachmentType.Final &&
                    a.DocumentRevision.IsLastConfirmRevision &&
                    !a.DocumentRevision.Document.IsDeleted &&
                    a.DocumentRevision.Document.IsActive &&
                    a.DocumentRevision.Document.ContractCode == authenticate.ContractCode &&
                    a.DocumentRevision.Document.DocumentProducts.Any(a=>a.ProductId==productId));

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(c => c.DocumentRevision.Document.DocumentProducts.Any(v => permission.ProductGroupIds.Contains(v.Product.ProductGroupId))))
                    return null;

                var lasrRevision = await dbQuery
                    .Select(c => new
                    {
                        FileName = c.FileName,
                        FileSrc = c.FileSrc,
                        DocumentId = c.DocumentRevision.DocumentId,
                        DocumentRevisionId = c.DocumentRevisionId.Value,
                        DocNumber=c.DocumentRevision.Document.DocNumber
                    }).ToListAsync();

                if (lasrRevision == null || !lasrRevision.Any())
                    return null;

                var files = new List<FinalRevisionsAttachmentToZipDto>();
                foreach (var item in lasrRevision)
                {
                    files.Add(new FinalRevisionsAttachmentToZipDto
                    {
                        FileName = item.FileName,
                        FileSrc = item.FileSrc,
                        FilePath = ServiceSetting.UploadFilePathDocument(authenticate.ContractCode, item.DocumentId, item.DocumentRevisionId),
                        DocNumber=item.DocNumber
                    });
                }


                var result = await _fileHelper.DownloadProductDocument(files);
                var product = await _productRepository.FirstOrDefaultAsync(a => a.Id == productId);
                if (product != null && result != null)
                    result.FileName = product.ProductCode + ".zip";
                return result;
            }
            catch (Exception exception)
            {
                return null;
            }
        }
        public async Task<ServiceResult< string>> ImportFileFromSharing(AuthenticateDto authenticate, long documentId, long revisionId, string fileSrc) {
            try
            {

                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                {
                    return ServiceResultFactory.CreateError<string>(null, MessageId.AccessDenied);

                }

                var sourceFile = _hostingEnvironmentRoot.ContentRootPath+ ServiceSetting.UploadFilePathDocument(authenticate.ContractCode, documentId, revisionId)+fileSrc;
                var extension = Path.GetExtension(fileSrc);
                var name = "doc-" + Guid.NewGuid().ToString("N") + extension;

                var destinationFile = _hostingEnvironmentRoot.ContentRootPath+ ServiceSetting.UploadFilePath.Temp + name;

                File.Copy(sourceFile, destinationFile, true);





                return ServiceResultFactory.CreateSuccess(name);

            }
            catch(Exception exception)
            {
                return ServiceResultFactory.CreateException<string>(null, exception);


            }


             
        }
        public async Task<ServiceResult<DocumentViewDto>> GetDocumentByIdAsync(AuthenticateDto authenticate,long documentId)
        {
            try
            {


                var dbQuery = _documentRepository
                     .AsNoTracking()
                     .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.DocumentId==documentId);


                //dbquery =>  _documentRepository
                var result = await dbQuery.Select(a => new DocumentViewDto
                {
                    DocumentId = a.DocumentId,
                    ContractCode = a.ContractCode,
                    DocTitle = a.DocTitle,
                    DocNumber = a.DocNumber,
                    ClientDocNumber = (a.ClientDocNumber != null) ? a.ClientDocNumber : "",
                    DocRemark = a.DocRemark,
                    DocClass = a.DocClass,
                    CommunicationCommentStatus = a.CommunicationCommentStatus,
                    DocumentGroupCode = a.DocumentGroup.DocumentGroupCode,
                    DocumentGroupTitle = a.DocumentGroup.Title,
                    IsActive = a.IsActive,
                    IsRequiredTransmittal = a.IsRequiredTransmittal,
                    Area = (a.Area != null) ? new AreaReadDTO
                    {
                        AreaId = a.Area.AreaId,
                        AreaTitle = a.Area.AreaTitle
                    } : null,
                    LastRevision = a.DocumentRevisions
                    .Where(c => !c.IsDeleted && c.IsLastRevision)
                    .Select(a => new LastRevisionDto
                    {
                        RevisionId = a.DocumentRevisionId,
                        RevisionCode = a.DocumentRevisionCode,
                        RevisionStatus = a.RevisionStatus
                    }).FirstOrDefault(),
                    DocumentProducts = a.DocumentProducts.Select(a => new ProductInfoDto
                    {
                        ProductDescription = a.Product.Description,
                        ProductCode = a.Product.ProductCode,
                        ProductId = a.ProductId
                    }).ToList()
                }).FirstOrDefaultAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<DocumentViewDto>(null, exception);
            }
        }
    }

    public class RevisionAttachmentTemp
    {
        public RevisionAttachment Revision { get; set; }
        public long DocumentId { get; set; }
    }
}
