using Dapper;
using Exon.TheWeb.Service.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Raybod.SCM.DataAccess.Core;
using Raybod.SCM.DataAccess.Extention;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject._PanelDocument.Area;
using Raybod.SCM.DataTransferObject._PanelOperation;
using Raybod.SCM.DataTransferObject.Audit;
using Raybod.SCM.DataTransferObject.Notification;
using Raybod.SCM.DataTransferObject.Operation;
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
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Implementation
{
    public class OperationService : IOperationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly DbSet<Operation> _operationRepository;
        private readonly DbSet<OperationGroup> _operationGroupRepository;
        private readonly DbSet<StatusOperation> _operationStatusGroupRepository;
        private readonly DbSet<OperationProgress> _operationProgressGroupRepository;
        private readonly DbSet<OperationActivity> _operationActivityRepository;
        private readonly DbSet<OperationActivityTimesheet> _activityTimesheetRepository;
        private readonly DbSet<OperationAttachment> _operationAttachmentRepository;
        private readonly DbSet<Area> _areaRepository;
        private readonly Utilitys.FileHelper _fileHelper;
        private readonly CompanyAppSettingsDto _appSettings;
        private readonly IWebHostEnvironment _hostingEnvironmentRoot;
        private readonly ITeamWorkAuthenticationService _authenticationService;
        private readonly ISCMLogAndNotificationService _scmLogAndNotificationService;
        private readonly IFileService _fileService;
        private string _contentRootPath;

        public OperationService(IUnitOfWork unitOfWork,
            IOptions<CompanyAppSettingsDto> appSettings,
            IWebHostEnvironment hostingEnvironmentRoot,
            IFileService fileService,
            IConfiguration configuration,
            ITeamWorkAuthenticationService authenticationService,
            ISCMLogAndNotificationService scmLogAndNotificationService)
        {
            _unitOfWork = unitOfWork;
            _operationRepository = _unitOfWork.Set<Operation>();
            _areaRepository = _unitOfWork.Set<Area>();
            _appSettings = appSettings.Value;
            _operationGroupRepository = _unitOfWork.Set<OperationGroup>();
            _operationStatusGroupRepository = _unitOfWork.Set<StatusOperation>();
            _operationProgressGroupRepository = _unitOfWork.Set<OperationProgress>();
            _operationAttachmentRepository = _unitOfWork.Set<OperationAttachment>();
            _activityTimesheetRepository = _unitOfWork.Set<OperationActivityTimesheet>();
            _operationActivityRepository = _unitOfWork.Set<OperationActivity>();
            _configuration = configuration;
            _contentRootPath = hostingEnvironmentRoot.ContentRootPath;
            _fileService = fileService;
            _fileHelper = new Utilitys.FileHelper(hostingEnvironmentRoot);
            _authenticationService = authenticationService;
            _scmLogAndNotificationService = scmLogAndNotificationService;
        }

        public async Task<ServiceResult<List<OperationViewDto>>> AddOperationAsync(AuthenticateDto authenticate, int operationGroupId, List<AddOperationDto> model)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermission(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if(!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<OperationViewDto>>(null, MessageId.AccessDenied);
        
                if (string.IsNullOrEmpty(authenticate.ContractCode) || model == null || !model.Any())
                    return ServiceResultFactory.CreateError<List<OperationViewDto>>(null, MessageId.InputDataValidationError);
              

                var operationGroupModel = await _operationGroupRepository.FirstOrDefaultAsync(a => !a.IsDeleted && a.OperationGroupId == operationGroupId);
                if (operationGroupModel == null)
                    return ServiceResultFactory.CreateError<List<OperationViewDto>>(null, MessageId.EntityDoesNotExist);

               
                if ( permission.OperationGroupList.Any() && !permission.OperationGroupList.Contains(operationGroupModel.OperationGroupId))
                    return ServiceResultFactory.CreateError<List<OperationViewDto>>(null, MessageId.AccessDenied);

                var operationCodes = model.Select(c => c.OperationCode).ToList();

                if (model.GroupBy(a => a.OperationCode).Any(c => c.Count() > 1))
                    return ServiceResultFactory.CreateError<List<OperationViewDto>>(null, MessageId.DuplicatOperationCode);

                if (await _operationRepository.AnyAsync(a => !a.IsDeleted && operationCodes.Contains(a.OperationCode)))
                    return ServiceResultFactory.CreateError<List<OperationViewDto>>(null, MessageId.DuplicatOperationCode);



                for (int i = 0; i < model.Count; i++)
                {
                    if (model[i].Area != null)
                    {
                        if (model[i].Area.AreaId != null)
                        {
                            model[i].AreaId = model[i].Area.AreaId;
                        }
                        else if ((!String.IsNullOrEmpty(model[i].Area.AreaTitle) && !String.IsNullOrWhiteSpace(model[i].Area.AreaTitle)))
                        {
                            var area = await _areaRepository.FirstOrDefaultAsync(a => a.AreaTitle == model[i].Area.AreaTitle.Trim() && a.ContractCode == authenticate.ContractCode && !a.IsDeleted);
                            if (area != null)
                            {
                                model[i].AreaId = area.AreaId;
                            }
                            else
                            {
                                var connectionString = _configuration["ConnectionStrings:ApplicationDbContextConnection"];
                                if (connectionString.Contains("%CONTENTROOTPATH%"))
                                {
                                    connectionString = connectionString.Replace("%CONTENTROOTPATH%", _contentRootPath);
                                }
                                try
                                {
                                    using var dbConn = new SqlConnection(connectionString);
                                    var query = @"Insert into Areas (AreaTitle,ContractCode,IsDeleted,AdderUserId,ModifierUserId,CreatedDate,UpdateDate) Values(@areaTitle,@contractCode,@isDeleted,@adderUserId,@modifierUserId,@createdDate,@updateDate);Select SCOPE_IDENTITY();";
                                    model[i].AreaId = await dbConn.ExecuteScalarAsync<int>(query, new { areaTitle = model[i].Area.AreaTitle.Trim(), contractCode = authenticate.ContractCode, isDeleted = false, adderUserId = authenticate.UserId, modifierUserId = authenticate.UserId, createdDate = DateTime.Now, updateDate = DateTime.Now });

                                }
                                catch (Exception exception)
                                {
                                    return ServiceResultFactory.CreateException<List<OperationViewDto>>(null, exception);
                                }
                            }
                        }

                    }
                }
                var addOperationModels = model.Select(a => new Operation
                {
                    ContractCode = authenticate.ContractCode,
                    OperationGroupId = operationGroupId,
                    OperationDescription = a.OperationDescription,
                    OperationCode = a.OperationCode,
                    AreaId = a.AreaId,
                    IsActive = true,
                    OperationStatuses = new List<StatusOperation> { new StatusOperation { IsDeleted = false, OperationStatus = OperationStatus.NotStarted } },
                    OperationProgresses = new List<OperationProgress> { new OperationProgress { IsDeleted = false, ProgressPercent = 0 } }
                }).ToList();

                _operationRepository.AddRange(addOperationModels);

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    await _scmLogAndNotificationService.SetDonedNotificationAsync(authenticate.UserId, authenticate.ContractCode, operationGroupId.ToString(), NotifEvent.AddOperation);

                    var res = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = authenticate.ContractCode,
                        FormCode = authenticate.ContractCode,
                        KeyValue = operationGroupId.ToString(),
                        Description = operationGroupModel.Title,
                        NotifEvent = Domain.Enum.NotifEvent.AddOperation,
                        RootKeyValue = operationGroupId.ToString(),
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,
                        OperationGroupId = operationGroupId,

                    }, null);
                    var result = addOperationModels.Select(a => new OperationViewDto
                    {
                        OperationId = a.OperationId,
                        ContractCode = a.ContractCode,
                        OperationCode = a.OperationCode,
                        OperationDescription = a.OperationDescription,
                        OperationGroupCode = a.OperationGroup.OperationGroupCode,
                        OperationGroupTitle = a.OperationGroup.Title,
                        IsActive = a.IsActive,
                        OperationProgress = new OperationProgressDto { ProgressPercent = 0, OperationStatus = OperationStatus.NotStarted },
                        Area = _areaRepository.Where(b => !b.IsDeleted && b.AreaId == a.AreaId).Select(b => new AreaReadDTO { AreaId = b.AreaId, AreaTitle = b.AreaTitle }).FirstOrDefault(),
                    }).ToList();


                    return ServiceResultFactory.CreateSuccess(result);
                }
                return ServiceResultFactory.CreateError<List<OperationViewDto>>(null, MessageId.AddEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<OperationViewDto>>(null, exception);
            }
        }


        public async Task<ServiceResult<OperationViewDto>> AddOperationAsync(AuthenticateDto authenticate, int operationGroupId, AddOperationDto model)
        {
            try
            {

                var permission = await _authenticationService.HasUserPermission(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<OperationViewDto>(null, MessageId.AccessDenied);

                if (string.IsNullOrEmpty(authenticate.ContractCode) || model == null)
                    return ServiceResultFactory.CreateError<OperationViewDto>(null, MessageId.InputDataValidationError);

                var operationGroupModel = await _operationGroupRepository.FirstOrDefaultAsync(a => !a.IsDeleted && a.OperationGroupId == operationGroupId);
                if (operationGroupModel == null)
                    return ServiceResultFactory.CreateError<OperationViewDto>(null, MessageId.EntityDoesNotExist);

                if (permission.OperationGroupList.Any() && !permission.OperationGroupList.Contains(operationGroupModel.OperationGroupId))
                    return ServiceResultFactory.CreateError<OperationViewDto>(null, MessageId.AccessDenied);

                if (model.Area != null)
                {

                    if (model.Area.AreaId != null)
                    {
                        var area = await _areaRepository.FirstOrDefaultAsync(a => a.AreaId == model.Area.AreaId.Value && a.ContractCode == authenticate.ContractCode && !a.IsDeleted);
                        if (area == null)
                            return ServiceResultFactory.CreateError<OperationViewDto>(null, MessageId.AreaNotExistInContract);
                        model.AreaId = model.Area.AreaId;
                    }

                }

                if (await _operationRepository.AnyAsync(a => !a.IsDeleted && a.OperationCode == model.OperationCode))
                    return ServiceResultFactory.CreateError<OperationViewDto>(null, MessageId.DuplicatOperationCode);
                var addOperationModel = new Operation
                {
                    ContractCode = authenticate.ContractCode,
                    OperationGroupId = operationGroupId,
                    OperationDescription = model.OperationDescription,
                    OperationCode = model.OperationCode,
                    AreaId = model.AreaId,
                    IsActive = true,
                    OperationStatuses = new List<StatusOperation> { new StatusOperation { IsDeleted = false, OperationStatus = Domain.Enum.OperationStatus.NotStarted } },
                    OperationProgresses = new List<OperationProgress> { new OperationProgress { IsDeleted = false, ProgressPercent = 0 } }
                };

                _operationRepository.Add(addOperationModel);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    await _scmLogAndNotificationService.SetDonedNotificationAsync(authenticate.UserId, authenticate.ContractCode, operationGroupId.ToString(), NotifEvent.AddOperation);

                    var res = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = authenticate.ContractCode,
                        FormCode = authenticate.ContractCode,
                        KeyValue = addOperationModel.OperationGroupId.ToString(),
                        Description = operationGroupModel.Title,
                        NotifEvent = Domain.Enum.NotifEvent.AddOperation,
                        RootKeyValue = operationGroupId.ToString(),
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,
                        OperationGroupId = operationGroupId
                    }, null);
                    var result = new OperationViewDto
                    {
                        OperationId = addOperationModel.OperationId,
                        ContractCode = addOperationModel.ContractCode,
                        OperationCode = addOperationModel.OperationCode,
                        OperationDescription = addOperationModel.OperationDescription,
                        OperationGroupCode = addOperationModel.OperationGroup.OperationGroupCode,
                        OperationGroupTitle = addOperationModel.OperationGroup.Title,
                        IsActive = addOperationModel.IsActive,
                        OperationProgress = new OperationProgressDto { ProgressPercent = 0, OperationStatus = Domain.Enum.OperationStatus.NotStarted },
                        Area = _areaRepository.Where(b => !b.IsDeleted && b.AreaId == addOperationModel.AreaId).Select(b => new AreaReadDTO { AreaId = b.AreaId, AreaTitle = b.AreaTitle }).FirstOrDefault(),
                    };
                    return ServiceResultFactory.CreateSuccess(result);
                }
                return ServiceResultFactory.CreateError<OperationViewDto>(null, MessageId.AddEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<OperationViewDto>(null, exception);
            }
        }

        public async Task<ServiceResult<List<OperationViewDto>>> GetOperationAsync(AuthenticateDto authenticate, OperationQueryDto query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermission(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<OperationViewDto>>(null, MessageId.AccessDenied);

                var dbQuery = _operationRepository
                     .AsNoTracking()
                     .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode);

                if (permission.OperationGroupList.Any())
                    dbQuery = dbQuery.Where(a => permission.OperationGroupList.Contains(a.OperationGroupId));

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a =>
                     a.OperationCode.Contains(query.SearchText)
                     || a.OperationDescription.Contains(query.SearchText));






                if (EnumHelper.ValidateItem(query.OperationStatuses) /*&& query.RevisionStatus != RevisionStatus.DeActive*/)
                    dbQuery = dbQuery.Where(a => query.OperationStatuses.Contains(a.OperationStatuses.Where(b => !b.IsDeleted).OrderByDescending(b => b.Id).FirstOrDefault().OperationStatus));

                if (query.OperationGroupIds != null && query.OperationGroupIds.Any())
                    dbQuery = dbQuery.Where(a => query.OperationGroupIds.Contains(a.OperationGroupId));



                if (query.AreaIds != null && query.AreaIds.Any())
                    dbQuery = dbQuery.Where(a => query.AreaIds.Contains(a.AreaId.Value));


                var totalCount = dbQuery.Count();

                var columnsMap = new Dictionary<string, Expression<Func<Operation, object>>>
                {
                    ["OperationId"] = v => v.OperationId
                };

                dbQuery = dbQuery.ApplayOrdering(query, columnsMap).ApplayPageing(query);

                //dbquery =>  _documentRepository
                var result = await dbQuery.Select(a => new OperationViewDto
                {
                    OperationId = a.OperationId,
                    ContractCode = a.ContractCode,
                    OperationCode = a.OperationCode,
                    OperationDescription = a.OperationDescription,
                    OperationGroupCode = a.OperationGroup.OperationGroupCode,
                    OperationGroupTitle = a.OperationGroup.Title,
                    IsActive = a.IsActive,
                    OperationProgress = new OperationProgressDto { OperationStatus = a.OperationStatuses.Where(b => !b.IsDeleted).OrderByDescending(b => b.Id).Select(b => b.OperationStatus).FirstOrDefault(), ProgressPercent = a.OperationProgresses.Where(b => !b.IsDeleted).OrderByDescending(b => b.Id).Select(b => b.ProgressPercent).FirstOrDefault() },
                    Area = (a.Area != null) ? new AreaReadDTO
                    {
                        AreaId = a.Area.AreaId,
                        AreaTitle = a.Area.AreaTitle
                    } : null,
                    DueDate = a.DueDate.ToUnixTimestamp()

                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<OperationViewDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<List<StartedOperationViewDto>>> GetStartedOperationAsync(AuthenticateDto authenticate, OperationQueryDto query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermission(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<StartedOperationViewDto>>(null, MessageId.AccessDenied);

                var dbQuery = _operationRepository
                     .AsNoTracking()
                     .Where(a => !a.IsDeleted &&a.IsActive&& a.ContractCode == authenticate.ContractCode && (a.OperationStatuses.Where(b => !b.IsDeleted).OrderByDescending(b => b.Id).FirstOrDefault() != null && (a.OperationStatuses.Where(c => !c.IsDeleted).OrderByDescending(c => c.Id).FirstOrDefault().OperationStatus == Domain.Enum.OperationStatus.Rejected || a.OperationStatuses.Where(d => !d.IsDeleted).OrderByDescending(d => d.Id).FirstOrDefault().OperationStatus == Domain.Enum.OperationStatus.InProgress)));

                if (permission.OperationGroupList.Any())
                    dbQuery = dbQuery.Where(a => permission.OperationGroupList.Contains(a.OperationGroupId));



                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a =>
                     a.OperationCode.Contains(query.SearchText)
                     || a.OperationDescription.Contains(query.SearchText));






                if (EnumHelper.ValidateItem(query.OperationStatuses) /*&& query.RevisionStatus != RevisionStatus.DeActive*/)
                    dbQuery = dbQuery.Where(a => query.OperationStatuses.Contains(a.OperationStatuses.Where(b => !b.IsDeleted).OrderByDescending(b => b.Id).FirstOrDefault().OperationStatus));

                if (query.OperationGroupIds != null && query.OperationGroupIds.Any())
                    dbQuery = dbQuery.Where(a => query.OperationGroupIds.Contains(a.OperationGroupId));



                if (query.AreaIds != null && query.AreaIds.Any())
                    dbQuery = dbQuery.Where(a => query.AreaIds.Contains(a.AreaId.Value));


                var totalCount = dbQuery.Count();

                var columnsMap = new Dictionary<string, Expression<Func<Operation, object>>>
                {
                    ["CreatedDate"] = v => v.OperationId
                };

                dbQuery = dbQuery.ApplayOrdering(query, columnsMap).ApplayPageing(query);

                //dbquery =>  _documentRepository
                var result = await dbQuery.Select(a => new StartedOperationViewDto
                {
                    OperationId = a.OperationId,
                    ContractCode = a.ContractCode,
                    OperationCode = a.OperationCode,
                    OperationDescription = a.OperationDescription,
                    OperationGroupCode = a.OperationGroup.OperationGroupCode,
                    OperationGroupTitle = a.OperationGroup.Title,
                    IsActive = a.IsActive,
                    OperationProgress = new OperationProgressDto { OperationStatus = a.OperationStatuses.Where(b => !b.IsDeleted).OrderByDescending(b => b.Id).Select(b => b.OperationStatus).FirstOrDefault(), ProgressPercent = a.OperationProgresses.Where(b => !b.IsDeleted).OrderByDescending(b => b.Id).Select(b => b.ProgressPercent).FirstOrDefault() },
                    Area = (a.Area != null) ? new AreaReadDTO
                    {
                        AreaId = a.Area.AreaId,
                        AreaTitle = a.Area.AreaTitle
                    } : null,
                    WhomStarted = a.OperationStatuses.Where(b => !b.IsDeleted && b.OperationStatus == OperationStatus.InProgress).OrderByDescending(b => b.Id).Select(b => new UserAuditLogDto
                    {
                        AdderUserImage = b.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + b.AdderUser.Image : "",
                        AdderUserName = b.AdderUser.FullName,
                        CreateDate = b.CreatedDate.ToUnixTimestamp()
                    }).FirstOrDefault(),
                    DueDate = a.DueDate.ToUnixTimestamp()

                }).ToListAsync();
                result = result.OrderByDescending(a => a.WhomStarted.CreateDate).ToList();
                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<StartedOperationViewDto>>(null, exception);
            }
        }
        public async Task<ServiceResult<OperationDetailDto>> GetOperationByIdAsync(AuthenticateDto authenticate, Guid operationId)
        {
            try
            {
                //var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                //if (!permission.HasPermission)
                //    return ServiceResultFactory.CreateError<List<DocumentViewDto>>(null, MessageId.AccessDenied);

                var dbQuery = _operationRepository
                     .AsNoTracking()
                     .Include(a => a.OperationStatuses)
                     .Include(a => a.OperationProgresses)
                     .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.OperationId == operationId);

                //if (permission.OperationGroupIds.Any())
                //    dbQuery = dbQuery.Where(a => permission.OperationGroupIds.Contains(a.OperationGroupId));


                if (dbQuery == null)
                    return ServiceResultFactory.CreateError<OperationDetailDto>(null, MessageId.EntityDoesNotExist);


                //dbquery =>  _documentRepository
                var result = await dbQuery.Select(a => new OperationDetailDto
                {
                    OperationId = a.OperationId,
                    ContractCode = a.ContractCode,
                    OperationCode = a.OperationCode,
                    OperationDescription = a.OperationDescription,
                    OperationGroupCode = a.OperationGroup.OperationGroupCode,
                    OperationGroupTitle = a.OperationGroup.Title,
                    IsActive = a.IsActive,
                    OperationProgress = new OperationProgressDto { OperationStatus = a.OperationStatuses.Where(b => !b.IsDeleted).OrderByDescending(b => b.Id).Select(b => b.OperationStatus).FirstOrDefault(), ProgressPercent = a.OperationProgresses.Where(b => !b.IsDeleted).OrderByDescending(b => b.Id).Select(b => b.ProgressPercent).FirstOrDefault() },
                    Area = (a.Area != null) ? new AreaReadDTO
                    {
                        AreaId = a.Area.AreaId,
                        AreaTitle = a.Area.AreaTitle
                    } : null,
                    WhomStarted = a.OperationStatuses.Where(b => !b.IsDeleted && b.OperationStatus == OperationStatus.InProgress).OrderBy(b => b.Id).Select(b => new UserAuditLogDto
                    {
                        AdderUserImage = b.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + b.AdderUser.Image : "",
                        AdderUserName = b.AdderUser.FullName,
                        CreateDate = b.CreatedDate.ToUnixTimestamp()
                    }).FirstOrDefault(),
                    UserAudit = a.AdderUser != null ? new UserAuditLogDto { AdderUserImage = a.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + a.AdderUser.Image : "", AdderUserName = a.AdderUser.FullName, CreateDate = a.CreatedDate.ToUnixTimestamp() } : null,
                    DueDate = a.DueDate.ToUnixTimestamp()
                }).FirstOrDefaultAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<OperationDetailDto>(null, exception);
            }
        }

        public async Task<ServiceResult<List<NotStartedOperationListDto>>> GetNotStartedOperationAsync(AuthenticateDto authenticate, OperationQueryDto query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermission(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<NotStartedOperationListDto>>(null, MessageId.AccessDenied);

                var dbQuery = _operationRepository
                     .AsNoTracking()
                     .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && (a.OperationStatuses.Where(b => !b.IsDeleted).OrderByDescending(b => b.Id).FirstOrDefault() != null && (a.OperationStatuses.Where(b => !b.IsDeleted).OrderByDescending(b => b.Id).FirstOrDefault().OperationStatus == Domain.Enum.OperationStatus.NotStarted)));

                if (permission.OperationGroupList.Any())
                    dbQuery = dbQuery.Where(a => permission.OperationGroupList.Contains(a.OperationGroupId));



                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a =>
                     a.OperationCode.Contains(query.SearchText)
                     || a.OperationDescription.Contains(query.SearchText));






                if (EnumHelper.ValidateItem(query.OperationStatuses) /*&& query.RevisionStatus != RevisionStatus.DeActive*/)
                    dbQuery = dbQuery.Where(a => query.OperationStatuses.Contains(a.OperationStatuses.Where(b => !b.IsDeleted).OrderByDescending(b => b.Id).FirstOrDefault().OperationStatus));

                if (query.OperationGroupIds != null && query.OperationGroupIds.Any())
                    dbQuery = dbQuery.Where(a => query.OperationGroupIds.Contains(a.OperationGroupId));



                if (query.AreaIds != null && query.AreaIds.Any())
                    dbQuery = dbQuery.Where(a => query.AreaIds.Contains(a.AreaId.Value));


                var totalCount = dbQuery.Count();

                var columnsMap = new Dictionary<string, Expression<Func<Operation, object>>>
                {
                    ["OperationId"] = v => v.OperationId
                };

                dbQuery = dbQuery.ApplayOrdering(query, columnsMap).ApplayPageing(query);

                //dbquery =>  _documentRepository
                var result = await dbQuery.Select(a => new NotStartedOperationListDto
                {
                    OperationId = a.OperationId,
                    OperationCode = a.OperationCode,
                    OperationDescription = a.OperationDescription,

                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<NotStartedOperationListDto>>(null, exception);
            }
        }
        public async Task<ServiceResult<OperationViewDto>> EditOperationByOperationIdAsync(AuthenticateDto authenticate, Guid operationId, EditOperationDto model)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermission(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<OperationViewDto>(null, MessageId.AccessDenied);

                var operationModel = await _operationRepository
                     .Where(a => !a.IsDeleted&&a.IsActive && a.ContractCode == authenticate.ContractCode && a.OperationId == operationId)
                     .Include(a => a.OperationGroup)
                     .Include(a=>a.OperationStatuses)
                     .Include(a=>a.OperationProgresses)
                     .FirstOrDefaultAsync();

                if (operationModel == null)
                    return ServiceResultFactory.CreateError<OperationViewDto>(null, MessageId.EntityDoesNotExist);

              
                if (permission.OperationGroupList.Any() && !permission.OperationGroupList.Contains(operationModel.OperationGroupId))
                    return ServiceResultFactory.CreateError<OperationViewDto>(null, MessageId.AccessDenied);

                if (!operationModel.IsActive)
                    return ServiceResultFactory.CreateError<OperationViewDto>(null, MessageId.OperationIsDeactive);

                if (await _operationRepository.AnyAsync(a => !a.IsDeleted && a.OperationId != operationId && a.OperationCode == model.OperationCode))
                    return ServiceResultFactory.CreateError<OperationViewDto>(null, MessageId.DuplicatOperationCode);

                var operationGroupModel = await _operationGroupRepository.FirstOrDefaultAsync(a => !a.IsDeleted && a.OperationGroupId == model.OperationGroupId);
                if (operationGroupModel == null)
                    return ServiceResultFactory.CreateError<OperationViewDto>(null, MessageId.OperationGroupNotExist);

                if (model.Area != null)
                {
                    if (model.Area.AreaId != null)
                    {
                        if (!await _areaRepository.AnyAsync(a => a.AreaId == model.Area.AreaId.Value && a.ContractCode == authenticate.ContractCode))
                        {
                            return ServiceResultFactory.CreateError<OperationViewDto>(null, MessageId.AreaNotExist);
                        }
                    }
                }

                operationModel.OperationCode = model.OperationCode;
                operationModel.OperationDescription = model.OperationDescription;
                operationModel.AreaId = (model.Area != null) ? model.Area.AreaId : null;
                operationModel.OperationGroupId = model.OperationGroupId;

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var result = new OperationViewDto
                    {
                        OperationId = operationModel.OperationId,
                        ContractCode = operationModel.ContractCode,
                        OperationCode = operationModel.OperationCode,
                        OperationDescription = operationModel.OperationDescription,
                        OperationGroupCode = operationModel.OperationGroup.OperationGroupCode,
                        OperationGroupTitle = operationModel.OperationGroup.Title,
                        IsActive = operationModel.IsActive,
                        OperationProgress = new OperationProgressDto { OperationStatus = operationModel.OperationStatuses.Where(b => !b.IsDeleted).OrderByDescending(b => b.Id).Select(b => b.OperationStatus).FirstOrDefault(), ProgressPercent = operationModel.OperationProgresses.Where(b => !b.IsDeleted).OrderByDescending(b => b.Id).Select(b => b.ProgressPercent).FirstOrDefault() },
                        Area = _areaRepository.Where(a => !a.IsDeleted && a.AreaId == operationModel.AreaId).Select(b => new AreaReadDTO { AreaId = b.AreaId, AreaTitle = b.AreaTitle }).FirstOrDefault(),

                    };
                    return ServiceResultFactory.CreateSuccess(result);
                }
                return ServiceResultFactory.CreateError<OperationViewDto>(null, MessageId.SaveFailed);

            }
            catch (Exception exception)
            {

                return ServiceResultFactory.CreateException<OperationViewDto>(null, exception);
            }
        }
        public async Task<ServiceResult<StartedOperationViewDto>> StartOperation(AuthenticateDto authenticate, Guid operationId, StartOperationDto model)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermission(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<StartedOperationViewDto>(null, MessageId.AccessDenied);

                var operationModel = await _operationRepository
                     .Where(a => !a.IsDeleted && a.IsActive && a.ContractCode == authenticate.ContractCode && a.OperationId == operationId)
                     .Include(a => a.OperationGroup)
                     .Include(a => a.OperationStatuses)
                     .Include(a => a.OperationProgresses)
                     .FirstOrDefaultAsync();

                if (operationModel == null)
                    return ServiceResultFactory.CreateError<StartedOperationViewDto>(null, MessageId.EntityDoesNotExist);

                if(permission.OperationGroupList.Any()&&!permission.OperationGroupList.Contains(operationModel.OperationGroupId))
                    return ServiceResultFactory.CreateError<StartedOperationViewDto>(null, MessageId.AccessDenied);

                if (!operationModel.IsActive)
                    return ServiceResultFactory.CreateError<StartedOperationViewDto>(null, MessageId.OperationIsDeactive);

                if (operationModel.OperationStatuses.Where(a => !a.IsDeleted).OrderByDescending(a => a.Id).FirstOrDefault() == null || operationModel.OperationStatuses.Where(a => !a.IsDeleted).OrderByDescending(a => a.Id).FirstOrDefault().OperationStatus != Domain.Enum.OperationStatus.NotStarted)
                    return ServiceResultFactory.CreateError<StartedOperationViewDto>(null, MessageId.OperationStartedAlready);
                //if (permission.OperationGroupIds.Any() && !permission.OperationGroupIds.Contains(operationModel.OperationGroupId))
                //    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);







                operationModel.DueDate = model.DueDate.UnixTimestampToDateTime();
                operationModel.OperationStatuses.Add(new Domain.Model.StatusOperation { OperationStatus = Domain.Enum.OperationStatus.InProgress, IsDeleted = false });

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var res1 = await _scmLogAndNotificationService.AddOperationAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = operationModel.ContractCode,
                        Description = operationModel.OperationDescription,
                        FormCode = operationModel.OperationCode,
                        Message = operationModel.OperationCode,
                        KeyValue = operationModel.OperationId.ToString(),
                        NotifEvent = NotifEvent.StartOperation,
                        RootKeyValue = operationModel.OperationId.ToString(),
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,
                        OperationGroupId = operationModel.OperationGroupId
                    },
                    operationModel.OperationGroupId,
                    new List<NotifToDto> {
                    new NotifToDto
                    {
                        NotifEvent= NotifEvent.StartOperation,
                        Roles= new List<string>
                        {
                           SCMRole.OperationInProgressMng
                        }
                    }
                    });

                    var result = new StartedOperationViewDto
                    {
                        OperationId = operationModel.OperationId,
                        ContractCode = operationModel.ContractCode,
                        OperationCode = operationModel.OperationCode,
                        OperationDescription = operationModel.OperationDescription,
                        OperationGroupCode = operationModel.OperationGroup.OperationGroupCode,
                        OperationGroupTitle = operationModel.OperationGroup.Title,
                        IsActive = operationModel.IsActive,
                        OperationProgress = new OperationProgressDto { OperationStatus = operationModel.OperationStatuses.Where(b => !b.IsDeleted).OrderByDescending(b => b.Id).Select(b => b.OperationStatus).FirstOrDefault(), ProgressPercent = operationModel.OperationProgresses.Where(b => !b.IsDeleted).OrderByDescending(b => b.Id).Select(b => b.ProgressPercent).FirstOrDefault() },
                        Area = _areaRepository.Where(a => !a.IsDeleted && a.AreaId == operationModel.AreaId).Select(b => new AreaReadDTO { AreaId = b.AreaId, AreaTitle = b.AreaTitle }).FirstOrDefault(),
                        WhomStarted = new UserAuditLogDto
                        {

                            AdderUserImage = authenticate.UserImage,
                            AdderUserName = authenticate.UserFullName,
                            CreateDate = DateTime.Now.ToUnixTimestamp(),
                        },
                        DueDate = operationModel.DueDate.ToUnixTimestamp()
                    };
                    return ServiceResultFactory.CreateSuccess(result);
                }
                return ServiceResultFactory.CreateError<StartedOperationViewDto>(null, MessageId.SaveFailed);

            }
            catch (Exception exception)
            {

                return ServiceResultFactory.CreateException<StartedOperationViewDto>(null, exception);
            }
        }
        public async Task<ServiceResult<List<OperationViewDto>>> StartOperation(AuthenticateDto authenticate, List<StartOperationsDto> model)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermission(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<OperationViewDto>>(null, MessageId.AccessDenied);

                
                foreach (var item in model)
                    if (!await _operationRepository.AnyAsync(a => !a.IsDeleted && a.OperationId == item.OperationId))
                        return ServiceResultFactory.CreateError<List<OperationViewDto>>(null, MessageId.EntityDoesNotExist);
                foreach (var item in model)
                    if (!await _operationRepository.AnyAsync(a => a.IsActive && a.OperationId == item.OperationId))
                        return ServiceResultFactory.CreateError<List<OperationViewDto>>(null, MessageId.OperationIsDeactive);

                foreach (var item in model)
                    if (await _operationRepository.AnyAsync(a => a.IsActive && !a.IsDeleted && a.OperationId == item.OperationId && (a.OperationStatuses.Where(b => !b.IsDeleted).OrderByDescending(b => b.Id).FirstOrDefault() == null || a.OperationStatuses.Where(b => !b.IsDeleted).OrderByDescending(b => b.Id).FirstOrDefault().OperationStatus != Domain.Enum.OperationStatus.NotStarted)))
                        return ServiceResultFactory.CreateError<List<OperationViewDto>>(null, MessageId.OperationStartedAlready);
                //if (permission.OperationGroupIds.Any() && !permission.OperationGroupIds.Contains(operationModel.OperationGroupId))
                //    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);






                var dbQuery = await _operationRepository.Include(a => a.OperationStatuses).Include(a => a.OperationProgresses).Where(a => !a.IsDeleted && a.IsActive && model.Select(b => b.OperationId).Contains(a.OperationId)).ToListAsync();
                if(permission.OperationGroupList.Any()&&dbQuery.Any(a=>!permission.OperationGroupList.Contains(a.OperationGroupId)))
                    return ServiceResultFactory.CreateError<List<OperationViewDto>>(null, MessageId.AccessDenied);
                foreach (var item in dbQuery)
                {
                    item.DueDate = model.First(a => a.OperationId == item.OperationId).OperationDueDate.UnixTimestampToDateTime();
                    item.OperationStatuses.Add(new Domain.Model.StatusOperation { OperationStatus = Domain.Enum.OperationStatus.InProgress, IsDeleted = false });
                }


                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    foreach(var operationModel in dbQuery)
                    {
                        var res1 = await _scmLogAndNotificationService.AddOperationAuditLogAsync(new AddAuditLogDto
                        {
                            ContractCode = operationModel.ContractCode,
                            Description = operationModel.OperationDescription,
                            FormCode = operationModel.OperationCode,
                            Message = operationModel.OperationCode,
                            KeyValue = operationModel.OperationId.ToString(),
                            NotifEvent = NotifEvent.StartOperation,
                            RootKeyValue = operationModel.OperationId.ToString(),
                            PerformerUserId = authenticate.UserId,
                            PerformerUserFullName = authenticate.UserFullName,
                            OperationGroupId = operationModel.OperationGroupId
                        },
                     operationModel.OperationGroupId,
                     new List<NotifToDto> {
                    new NotifToDto
                    {
                        NotifEvent= NotifEvent.StartOperation,
                        Roles= new List<string>
                        {
                           SCMRole.OperationInProgressMng
                        }
                    }
                     });
                    }
                    
                    var result = await _operationRepository.Where(a => !a.IsDeleted && a.IsActive && model.Select(b => b.OperationId).Contains(a.OperationId)).Select(a => new OperationViewDto
                    {
                        OperationId = a.OperationId,
                        ContractCode = a.ContractCode,
                        OperationCode = a.OperationCode,
                        OperationDescription = a.OperationDescription,
                        OperationGroupCode = a.OperationGroup.OperationGroupCode,
                        OperationGroupTitle = a.OperationGroup.Title,
                        IsActive = a.IsActive,
                        OperationProgress = new OperationProgressDto { OperationStatus = a.OperationStatuses.Where(b => !b.IsDeleted).OrderByDescending(b => b.Id).Select(b => b.OperationStatus).FirstOrDefault(), ProgressPercent = a.OperationProgresses.Where(b => !b.IsDeleted).OrderByDescending(b => b.Id).Select(b => b.ProgressPercent).FirstOrDefault() },
                        Area = _areaRepository.Where(b => !b.IsDeleted && b.AreaId == a.AreaId).Select(b => new AreaReadDTO { AreaId = b.AreaId, AreaTitle = b.AreaTitle }).FirstOrDefault(),

                    }).ToListAsync();
                    return ServiceResultFactory.CreateSuccess(result);
                }
                return ServiceResultFactory.CreateError<List<OperationViewDto>>(null, MessageId.SaveFailed);

            }
            catch (Exception exception)
            {

                return ServiceResultFactory.CreateException<List<OperationViewDto>>(null, exception);
            }
        }
        public async Task<ServiceResult<bool>> ActiveOrDeactiveOperationByOperationIdAsync(AuthenticateDto authenticate, Guid operationId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermission(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var operationModel = await _operationRepository
                     .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.OperationId == operationId)
                     .FirstOrDefaultAsync();

                if (operationModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);



                if (permission.OperationGroupList.Any() && !permission.OperationGroupList.Contains(operationModel.OperationGroupId))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);
                operationModel.IsActive = !operationModel.IsActive;

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

        public async Task<ServiceResult<bool>> ConfirmOperationByOperationIdAsync(AuthenticateDto authenticate, Guid operationId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermission(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var operationModel = await _operationRepository
                    .Include(a=>a.OperationActivities)
                    .Include(a=>a.OperationStatuses)
                    .Include(a=>a.OperationProgresses)
                     .Where(a => !a.IsDeleted &&a.IsActive&& a.ContractCode == authenticate.ContractCode && a.OperationId == operationId)
                     .FirstOrDefaultAsync();

                if (operationModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

               
                if (permission.OperationGroupList.Any() && !permission.OperationGroupList.Contains(operationModel.OperationGroupId))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                if (operationModel.OperationStatuses.Where(a=>!a.IsDeleted).OrderByDescending(a=>a.Id).FirstOrDefault()==null||
                    (operationModel.OperationStatuses.Where(a => !a.IsDeleted).OrderByDescending(a => a.Id).FirstOrDefault().OperationStatus!=OperationStatus.InProgress&&
                    operationModel.OperationStatuses.Where(a => !a.IsDeleted).OrderByDescending(a => a.Id).FirstOrDefault().OperationStatus != OperationStatus.Rejected))
                    return ServiceResultFactory.CreateError(false, MessageId.OperationCannotBeConfirm);

                //if (operationModel.OperationActivities!=null&&operationModel.OperationActivities.Any(a=>!a.IsDeleted)&&operationModel.OperationProgresses.Where(a => !a.IsDeleted).OrderByDescending(a => a.Id).FirstOrDefault() == null ||
                //   (operationModel.OperationProgresses.Where(a => !a.IsDeleted).OrderByDescending(a => a.Id).FirstOrDefault().ProgressPercent > 100 ))
                //    return ServiceResultFactory.CreateError(false, MessageId.OperationProgressNotCompleted);

                if (operationModel.OperationActivities!=null&&operationModel.OperationActivities.Any(a=>!a.IsDeleted&&a.OperationActivityStatus!=OperationActivityStatus.Done))
                    return ServiceResultFactory.CreateError(false, MessageId.OperationHasNotCompletedActivity);

                if (operationModel.OperationActivities == null || operationModel.OperationActivities.Count()==0 || !operationModel.OperationActivities.Any(a=>!a.IsDeleted))
                {
                    operationModel.OperationProgresses.Add(new OperationProgress { IsDeleted = false, ProgressPercent = 100 });
                }
                //if (permission.OperationGroupIds.Any() && !permission.OperationGroupIds.Contains(operationModel.OperationGroupId))
                //    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);
                operationModel.OperationStatuses.Add(new StatusOperation { OperationStatus = OperationStatus.Confirmed, IsDeleted = false });
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    await SendLogAndNotitificationTaskOnSetConfirmOperationAsync(authenticate, operationModel);
                    return ServiceResultFactory.CreateSuccess(true);
                }
                return ServiceResultFactory.CreateError(false, MessageId.SaveFailed);

            }
            catch (Exception exception)
            {

                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<bool>> AbortOperationByOperationIdAsync(AuthenticateDto authenticate, Guid operationId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermission(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var operationModel = await _operationRepository
                    .Include(a => a.OperationActivities)
                    .Include(a => a.OperationStatuses)
                     .Where(a => !a.IsDeleted &&a.IsActive && a.ContractCode == authenticate.ContractCode && a.OperationId == operationId)
                     .FirstOrDefaultAsync();

                if (operationModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);



                if (operationModel.OperationStatuses.Where(a => !a.IsDeleted).OrderByDescending(a => a.Id).FirstOrDefault() == null ||
                    (operationModel.OperationStatuses.Where(a => !a.IsDeleted).OrderByDescending(a => a.Id).FirstOrDefault().OperationStatus != OperationStatus.InProgress &&
                    operationModel.OperationStatuses.Where(a => !a.IsDeleted).OrderByDescending(a => a.Id).FirstOrDefault().OperationStatus != OperationStatus.Rejected))
                    return ServiceResultFactory.CreateError(false, MessageId.OperationCannotBeConfirm);

                if (permission.OperationGroupList.Any() && !permission.OperationGroupList.Contains(operationModel.OperationGroupId))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                operationModel.OperationStatuses.Add(new StatusOperation { OperationStatus = OperationStatus.NotStarted, IsDeleted = false });

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

        public async Task<ServiceResult<bool>> RestartOperationByOperationIdAsync(AuthenticateDto authenticate, Guid operationId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermission(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var operationModel = await _operationRepository
                    .Include(a => a.OperationActivities)
                    .Include(a => a.OperationStatuses)
                    .Include(a => a.OperationProgresses)
                     .Where(a => !a.IsDeleted &&a.IsActive&& a.ContractCode == authenticate.ContractCode && a.OperationId == operationId)
                     .FirstOrDefaultAsync();

                if (operationModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (operationModel.OperationStatuses.Where(a => !a.IsDeleted).OrderByDescending(a => a.Id).FirstOrDefault() == null ||
                    (operationModel.OperationStatuses.Where(a => !a.IsDeleted).OrderByDescending(a => a.Id).FirstOrDefault().OperationStatus != OperationStatus.Confirmed))
                    return ServiceResultFactory.CreateError(false, MessageId.OperationNotConfirm);

                //if (operationModel.OperationProgresses.Where(a => !a.IsDeleted).OrderByDescending(a => a.Id).FirstOrDefault() == null ||
                //   (operationModel.OperationProgresses.Where(a => !a.IsDeleted).OrderByDescending(a => a.Id).FirstOrDefault().ProgressPercent > 100))
                //    return ServiceResultFactory.CreateError(false, MessageId.OperationProgressNotCompleted);

                //if (operationModel.OperationActivities != null && operationModel.OperationActivities.Any(a => !a.IsDeleted && a.OperationActivityStatus != OperationActivityStatus.Done))
                //    return ServiceResultFactory.CreateError(false, MessageId.OperationHasNotCompletedActivity);
                if (permission.OperationGroupList.Any() && !permission.OperationGroupList.Contains(operationModel.OperationGroupId))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                if (operationModel.OperationActivities == null || operationModel.OperationActivities.Count() == 0 || !operationModel.OperationActivities.Any(a => !a.IsDeleted))
                {
                    operationModel.OperationProgresses.Remove(operationModel.OperationProgresses.Last());
                }

                operationModel.OperationStatuses.Add(new StatusOperation { OperationStatus = OperationStatus.InProgress, IsDeleted = false });
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


        public async Task<ServiceResult<int>> GetPendingForConfirmOperationBadgeCountAsync(AuthenticateDto authenticate)
        {
            try
            {


                //var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                //if (!permission.HasPermission)
                //    return ServiceResultFactory.CreateError<DocumentBadgeCountDto>(null, MessageId.AccessDenied);
                var dbQuery = _operationRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted &&a.IsActive&& a.ContractCode == authenticate.ContractCode && (a.OperationStatuses.Where(b => !b.IsDeleted).OrderByDescending(b => b.Id).FirstOrDefault() != null && (a.OperationStatuses.Where(b => !b.IsDeleted).OrderByDescending(b => b.Id).FirstOrDefault().OperationStatus == Domain.Enum.OperationStatus.PendingConfirm)));


                //if (permission.DocumentGroupIds.Any())
                //    dbQuery = dbQuery.Where(c => permission.DocumentGroupIds.Contains(c.DocumentGroupId));

                var result = await dbQuery.CountAsync();
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<int>(0, exception);
            }
        }

        public async Task<ServiceResult<int>> GetInProgressOperationBadgeCountAsync(AuthenticateDto authenticate)
        {
            try
            {


                var permission = await _authenticationService.HasUserPermission(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(0, MessageId.AccessDenied);
                var dbQuery = _operationRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted &&a.IsActive&& a.ContractCode == authenticate.ContractCode && (a.OperationStatuses.Where(b => !b.IsDeleted).OrderByDescending(b => b.Id).FirstOrDefault() != null && (a.OperationStatuses.Where(c => !c.IsDeleted).OrderByDescending(c => c.Id).FirstOrDefault().OperationStatus == Domain.Enum.OperationStatus.Rejected || a.OperationStatuses.Where(d => !d.IsDeleted).OrderByDescending(d => d.Id).FirstOrDefault().OperationStatus == Domain.Enum.OperationStatus.InProgress)));


                if (permission.OperationGroupList.Any())
                    dbQuery = dbQuery.Where(c => permission.OperationGroupList.Contains(c.OperationGroupId));

                var result = await dbQuery.CountAsync();
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<int>(0, exception);
            }
        }


        public async Task<ServiceResult<List<OperationAttachmentDto>>> AddOperationAttachmentAsync(AuthenticateDto authenticate, Guid operationId, IFormFileCollection files)
        {
            try
            {
                if (files == null || !files.Any())
                    return ServiceResultFactory.CreateError<List<OperationAttachmentDto>>(null, MessageId.InputDataValidationError);

                var permission = await _authenticationService.HasUserPermission(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<OperationAttachmentDto>>(null, MessageId.AccessDenied);

                var dbQuery = _operationRepository
                    .AsNoTracking()
                     .Where(a => !a.IsDeleted &&
                     a.IsActive &&
                     a.OperationId == operationId &&
                     a.ContractCode == authenticate.ContractCode);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<List<OperationAttachmentDto>>(null, MessageId.EntityDoesNotExist);

                if (permission.OperationGroupList.Any() && !dbQuery.Any(a => permission.OperationGroupList.Contains(a.OperationGroupId)))
                    return ServiceResultFactory.CreateError<List<OperationAttachmentDto>>(null, MessageId.AccessDenied);

                var attachModels = new List<OperationAttachment>();
                foreach (var item in files)
                {
                    var fileName = item.FileName;
                    var uploadResult = await _fileService.UploadDocumentFile(item);
                    if (!uploadResult.Succeeded)
                        return ServiceResultFactory.CreateError<List<OperationAttachmentDto>>(null, uploadResult.Messages[0].Message);

                    var UploadedFile = await _fileHelper
                        .SaveDocumentFromTemp(uploadResult.Result, ServiceSetting.UploadFilePathOperation(authenticate.ContractCode, operationId));

                    if (UploadedFile == null)
                        return ServiceResultFactory.CreateError<List<OperationAttachmentDto>>(null, MessageId.UploudFailed);

                    _fileHelper.DeleteDocumentFromTemp(uploadResult.Result);

                    attachModels.Add(new OperationAttachment
                    {
                        OperationId = operationId,
                        FileSrc = uploadResult.Result,
                        FileName = fileName,
                        FileType = UploadedFile.FileType,
                        FileSize = UploadedFile.FileSize
                    });
                }

                _operationAttachmentRepository.AddRange(attachModels);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var res = attachModels.Select(c => new OperationAttachmentDto
                    {
                        FileName = c.FileName,
                        FileSize = c.FileSize,
                        FileSrc = c.FileSrc,
                        FileType = c.FileType,
                        OperationAttachmentId = c.OperationAttachmentId
                    }).ToList();
                    return ServiceResultFactory.CreateSuccess(res);
                }
                return ServiceResultFactory.CreateError<List<OperationAttachmentDto>>(null, MessageId.UploudFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<OperationAttachmentDto>>(null, exception);

            }
        }
        public async Task<ServiceResult<List<OperationAttachmentDto>>> GetOperationAttachmentAsync(AuthenticateDto authenticate, Guid operationId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermission(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(new List<OperationAttachmentDto>(), MessageId.AccessDenied);



                var dbQuery = _operationAttachmentRepository
                   .AsNoTracking()
                   .Where(a => !a.IsDeleted &&
                   a.OperationId == operationId &&
                   a.Operation.IsActive &&
                   !a.Operation.IsDeleted&&
                   a.OperationCommentId == null &&
                   a.Operation.ContractCode == authenticate.ContractCode);

                if (permission.OperationGroupList.Any())
                    dbQuery = dbQuery.Where(a => permission.OperationGroupList.Contains(a.Operation.OperationGroupId));




                var res = await dbQuery.Select(c => new OperationAttachmentDto
                {
                    FileName = c.FileName,
                    FileSize = c.FileSize,
                    FileSrc = c.FileSrc,
                    FileType = c.FileType,
                    OperationAttachmentId = c.OperationAttachmentId
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(res);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<OperationAttachmentDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<bool>> DeleteOperationAttachmentAsync(AuthenticateDto authenticate, Guid operationId, string fileSrc)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermission(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery = _operationAttachmentRepository
                     .Where(a => !a.IsDeleted &&
                     a.OperationId == operationId &&
                     a.FileSrc == fileSrc &&
                     a.Operation.IsActive &&
                     !a.Operation.IsDeleted &&
                     a.OperationCommentId == null &&
                     a.Operation.ContractCode == authenticate.ContractCode);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (permission.OperationGroupList.Any() && !dbQuery.Any(a => permission.OperationGroupList.Contains(a.Operation.OperationGroupId)))
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


        public async Task<DownloadFileDto> DownloadOperationFileAsync(AuthenticateDto authenticate, Guid operationId, string fileSrc)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermission(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return null;

                var dbQuery = _operationAttachmentRepository
                    .AsNoTracking()
                    .Where(a =>
                    !a.IsDeleted
                    && !a.Operation.IsDeleted
                    && a.Operation.IsActive
                    && a.OperationId == operationId
                    && a.OperationCommentId == null
                    && a.FileSrc == fileSrc);

                if (permission.OperationGroupList.Any() && !dbQuery.Any(a => permission.OperationGroupList.Contains(a.Operation.OperationGroupId)))
                    return null;

                if (!await dbQuery.AnyAsync())
                    return null;

                return await _fileHelper.DownloadDocument(fileSrc, ServiceSetting.UploadFilePathOperation(authenticate.ContractCode, operationId));
            }
            catch (Exception exception)
            {
                return null;
            }
        }

        private async Task SendLogAndNotitificationTaskOnSetConfirmOperationAsync(AuthenticateDto authenticate,
           Operation OperationModel)
        {
            
            var logModel = new AddAuditLogDto
            {
                ContractCode = authenticate.ContractCode,
                Description = OperationModel.OperationDescription,
                FormCode = OperationModel.OperationCode,
                KeyValue = OperationModel.OperationId.ToString(),
                NotifEvent = NotifEvent.OperationFinalization,
                RootKeyValue = OperationModel.OperationId.ToString(),
                PerformerUserId = authenticate.UserId,
                PerformerUserFullName = authenticate.UserFullName,
                OperationGroupId = OperationModel.OperationGroupId
            };

            await _scmLogAndNotificationService
             .SetDonedNotificationAsync(authenticate.UserId, authenticate.ContractCode, OperationModel.OperationId.ToString(), NotifEvent.StartOperation);

         

                var res1 = await _scmLogAndNotificationService.AddScmAuditLogAsync(logModel, null);
            
        }

    }
}
