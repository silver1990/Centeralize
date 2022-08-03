using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Raybod.SCM.DataAccess.Core;
using Raybod.SCM.DataAccess.Extention;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.PurchaseRequest;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Model;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Utility.Common;
using Raybod.SCM.Utility.EnumType;
using Raybod.SCM.Utility.Extention;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Raybod.SCM.DataTransferObject.Audit;
using Raybod.SCM.Domain.Struct;
using Microsoft.AspNetCore.Hosting;
using Raybod.SCM.DataTransferObject.Notification;
using Raybod.SCM.DataTransferObject.User;
using Raybod.SCM.DataTransferObject._panelPurchase.PurchaseRequestConfirmation;
using Hangfire;

namespace Raybod.SCM.Services.Implementation
{
    public class PurchaseRequestService : IPurchaseRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITeamWorkAuthenticationService _authenticationService;
        private readonly ISCMLogAndNotificationService _scmLogAndNotificationService;
        private readonly IContractFormConfigService _formConfigService;
        private readonly DbSet<PurchaseRequest> _purchaseRequestRepository;
        private readonly DbSet<Product> _productRepository;
        private readonly DbSet<PurchaseRequestItem> _purchaseRequestItemRepository;
        private readonly DbSet<PAttachment> _purchaseRequestAttachmentRepository;
        private readonly DbSet<User> _userRepository;
        private readonly DbSet<ProductGroup> _productGroupRepository;
        private readonly DbSet<Mrp> _mrpRepository;
        private readonly DbSet<PurchaseConfirmationWorkFlow> _purchaseRequestConfirmWorkFlowRepository;
        private readonly DbSet<MrpItem> _mrpPlanningRepository;
        private readonly DbSet<Contract> _contractRepository;
        private readonly Raybod.SCM.Services.Utilitys.FileHelper _fileHelper;
        private readonly CompanyAppSettingsDto _appSettings;

        public PurchaseRequestService(
            IUnitOfWork unitOfWork,
            IWebHostEnvironment hostingEnvironmentRoot,
            ITeamWorkAuthenticationService authenticationService,
            ISCMLogAndNotificationService scmLogAndNotificationService,
            IContractFormConfigService formConfigService,
        IOptions<CompanyAppSettingsDto> appSettings)
        {
            _unitOfWork = unitOfWork;
            _authenticationService = authenticationService;
            _scmLogAndNotificationService = scmLogAndNotificationService;
            _formConfigService = formConfigService;
            _purchaseRequestRepository = _unitOfWork.Set<PurchaseRequest>();
            _purchaseRequestItemRepository = _unitOfWork.Set<PurchaseRequestItem>();
            _purchaseRequestAttachmentRepository = _unitOfWork.Set<PAttachment>();
            _mrpRepository = _unitOfWork.Set<Mrp>();
            _mrpPlanningRepository = _unitOfWork.Set<MrpItem>();
            _productRepository = _unitOfWork.Set<Product>();
            _purchaseRequestConfirmWorkFlowRepository = _unitOfWork.Set<PurchaseConfirmationWorkFlow>();
            _contractRepository = _unitOfWork.Set<Contract>();
            _productGroupRepository = _unitOfWork.Set<ProductGroup>();
            _userRepository = _unitOfWork.Set<User>();
            _appSettings = appSettings.Value;
            _fileHelper = new Utilitys.FileHelper(hostingEnvironmentRoot);
        }

        public async Task<PRWaitingListBadgeCountDto> GetDashbourdWaitingListBadgeCountAsync(AuthenticateDto authenticate)
        {
            var result = new PRWaitingListBadgeCountDto
            {
                WaitingForConfirmQuantity = 0,
                WaitingForNewPRQuantity = 0
            };

            try
            {
                List<string> confirmRole = new List<string> { SCMRole.PurchaseRequestConfirm, SCMRole.PurchaseRequestObs };
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, confirmRole);
                if (permission.HasPermission)
                {
                    var prDbQuery = _purchaseRequestRepository
                        .AsNoTracking()
                        .Where(a => !a.IsDeleted && a.PRStatus == PRStatus.Register && a.ContractCode == authenticate.ContractCode);

                    if (permission.ProductGroupIds.Any())
                        prDbQuery = prDbQuery.Where(a => permission.ProductGroupIds.Contains(a.ProductGroupId));

                    result.WaitingForConfirmQuantity = await prDbQuery.CountAsync();
                }

                List<string> registerRole = new List<string> { SCMRole.PurchaseRequestReg, SCMRole.PurchaseRequestObs };
                var registerPermission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, registerRole);
                if (registerPermission.HasPermission)
                {
                    var mrpDBQuery = _mrpRepository
                        .AsNoTracking()
                        .Where(x => !x.IsDeleted && x.ContractCode == authenticate.ContractCode);

                    if (registerPermission.ProductGroupIds.Any())
                        mrpDBQuery = mrpDBQuery.Where(a => registerPermission.ProductGroupIds.Contains(a.ProductGroupId));

                    result.WaitingForNewPRQuantity = await mrpDBQuery.CountAsync(a => a.MrpItems.Any(c => !c.IsDeleted && c.RemainedStock > 0));
                }

                return result;
            }
            catch (Exception exception)
            {
                return result;
            }
        }


        public async Task<ServiceResult<PRWaitingListBadgeCountDto>> GetWaitingListBadgeCountAsync(AuthenticateDto authenticate, List<string> registerRoles, List<string> confirmRoles)
        {
            try
            {
                var result = new PRWaitingListBadgeCountDto
                {
                    WaitingForConfirmQuantity = 0,
                    WaitingForNewPRQuantity = 0
                };

                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, confirmRoles);
                if (permission.HasPermission)
                {
                    var prDbQuery = _purchaseRequestConfirmWorkFlowRepository
                        .AsNoTracking()
                        .Where(a => !a.IsDeleted &&
                        a.Status == ConfirmationWorkFlowStatus.Pending &&
                        a.PurchaseRequest.ContractCode == authenticate.ContractCode);

                    if (permission.ProductGroupIds.Any())
                        prDbQuery = prDbQuery.Where(a => permission.ProductGroupIds.Contains(a.PurchaseRequest.ProductGroupId));

                    result.WaitingForConfirmQuantity = await prDbQuery.CountAsync();
                }

                var registerPermission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, registerRoles);
                if (registerPermission.HasPermission)
                {
                    var dbQuery = _mrpRepository
                        .AsNoTracking()
                        .Where(x => !x.IsDeleted &&
                        x.ContractCode == authenticate.ContractCode &&
                        x.MrpItems.Any(c => !c.IsDeleted && c.RemainedStock > 0));

                    if (registerPermission.ProductGroupIds.Any())
                        dbQuery = dbQuery.Where(a => registerPermission.ProductGroupIds.Contains(a.ProductGroupId));

                    result.WaitingForNewPRQuantity = await dbQuery.CountAsync(a => a.MrpItems.Any(c => !c.IsDeleted && c.RemainedStock > 0));
                }

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<PRWaitingListBadgeCountDto>(null, exception);
            }
        }

        public async Task<ServiceResult<bool>> AddPurchaseRequestAsync(AuthenticateDto authenticate, AddPurchaseRequestDto model)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                if (model.MrpId <= 0 && !string.IsNullOrEmpty(authenticate.ContractCode))
                    return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                if (model.PurchaseRequestItems == null || model.PurchaseRequestItems.Count() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                if (model.PurchaseRequestItems.Any(a=>a.Quntity<=0))
                    return ServiceResultFactory.CreateError(false, MessageId.QuantityCantLessOrEqualZero);

                var postedProductIds = model.PurchaseRequestItems.Select(a => a.ProductId).ToList();

                if (model.PurchaseRequestItems.GroupBy(a => a.ProductId).Any(c => c.Count() > 1))
                    return ServiceResultFactory.CreateError(false, MessageId.ImpossibleDuplicateProduct);

                var purchaseRequestItemIds = model.PurchaseRequestItems.Select(a => a.ProductId).ToList();

                var dbQuery = _mrpPlanningRepository
                    .Where(a => !a.IsDeleted &&
                    !a.Mrp.IsDeleted &&
                    a.MrpId == model.MrpId &&
                    purchaseRequestItemIds.Contains(a.ProductId) &&
                    a.Mrp.ContractCode == authenticate.ContractCode &&
                    a.RemainedStock > 0);


                var mrpItems = await dbQuery
                    .Include(a => a.Mrp)
                    .ThenInclude(a => a.ProductGroup)
                    .ToListAsync();

                if (mrpItems == null)
                    return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                var mrpModel = mrpItems.FirstOrDefault().Mrp;

                if (permission.ProductGroupIds.Any() && !permission.ProductGroupIds.Contains(mrpModel.ProductGroupId))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                if (model.PurchaseRequestItems.Any(c => mrpItems.Where(a => !a.IsDeleted && a.ProductId == c.ProductId).Sum(a => a.RemainedStock) < c.Quntity))
                    return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);


                var PRModel = new PurchaseRequest
                {
                    ProductGroupId = mrpModel.ProductGroupId,
                    ContractCode = mrpModel.ContractCode,
                    MrpId = mrpModel.Id,
                    Note = model.WorkFlow.Note,
                    PRStatus = PRStatus.Register,
                    TypeOfInquiry = TypeOfInquiry.TCRFP,
                    PurchaseConfirmationWorkFlows = new List<PurchaseConfirmationWorkFlow>()
                };

                PRModel = AddPurchaseRequestItemByMrpItem(PRModel, model.PurchaseRequestItems, mrpItems);
                var confirmWorkFlow = await AddPurchaseRequestConfirmationAsync(authenticate.ContractCode, model.MrpId, model.WorkFlow, model.PRAttachmentDto);
                if (!confirmWorkFlow.Succeeded)
                    return ServiceResultFactory.CreateError(false, MessageId.OperationFailed);
                PRModel.PurchaseConfirmationWorkFlows.Add(confirmWorkFlow.Result);
                if (confirmWorkFlow.Result.Status == ConfirmationWorkFlowStatus.Confirm)
                {
                    PRModel.PRStatus = PRStatus.Confirm;
                    //if (model.PRAttachmentDto != null && model.PRAttachmentDto.Count() > 0)
                    //{
                    //    if (!_fileHelper.FileExistInTemp(model.PRAttachmentDto.Select(c => c.FileSrc).ToList()))
                    //        return ServiceResultFactory.CreateError(false, MessageId.FileNotFound);
                    //    var attachmentResult = await AddPRAttachmentAsync(PRModel, model.PRAttachmentDto);
                    //    if (!attachmentResult.Succeeded)
                    //        return ServiceResultFactory.CreateError(false,
                    //            attachmentResult.Messages.FirstOrDefault().Message);

                    //    PRModel = attachmentResult.Result;
                    //}
                }


                // generate form code
                var count = await _purchaseRequestRepository.CountAsync(a => a.ContractCode == authenticate.ContractCode);
                var codeRes = await _formConfigService.GenerateFormCodeAsync(authenticate.ContractCode, FormName.PR, count);
                if (!codeRes.Succeeded)
                    return ServiceResultFactory.CreateError(false, codeRes.Messages.First().Message);
                PRModel.PRCode = codeRes.Result;

                _purchaseRequestRepository.Add(PRModel);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {

                    if (!await _mrpPlanningRepository.AnyAsync(a => !a.IsDeleted && a.MrpId == model.MrpId && a.RemainedStock > 0))
                        await _scmLogAndNotificationService.SetDonedNotificationAsync(authenticate.UserId, PRModel.ContractCode, PRModel.MrpId.ToString(), NotifEvent.AddPurchaseRequest);

                    int? userId = null;
                    if (PRModel.PRStatus != PRStatus.Confirm)
                    {
                        userId = PRModel.PurchaseConfirmationWorkFlows.First().PurchaseConfirmationWorkFlowUsers.First(a => a.IsBallInCourt).UserId;
                    }
                    if(PRModel.PRStatus != PRStatus.Confirm)
                    {
                        await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                        {
                            ContractCode = mrpModel.ContractCode,
                            FormCode = PRModel.PRCode,
                            KeyValue = PRModel.Id.ToString(),
                            NotifEvent = NotifEvent.AddPurchaseRequest,
                            ProductGroupId = PRModel.ProductGroupId,
                            RootKeyValue = PRModel.Id.ToString(),
                            PerformerUserId = authenticate.UserId,
                            Message = mrpModel.ProductGroup.Title,
                            PerformerUserFullName = authenticate.UserFullName,
                        },
                    PRModel.ProductGroupId,
                    NotifEvent.ConfirmPurchaseRequest, userId);
                    }
                    if (PRModel.PRStatus == PRStatus.Confirm)
                    {
                        await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                        {
                            ContractCode = PRModel.ContractCode,
                            FormCode = PRModel.PRCode,
                            KeyValue = PRModel.Id.ToString(),
                            NotifEvent = NotifEvent.ConfirmPurchaseRequest,
                            ProductGroupId = PRModel.ProductGroupId,
                            RootKeyValue = PRModel.Id.ToString(),
                            Message = mrpModel.ProductGroup.Title,
                            PerformerUserId = authenticate.UserId,
                            PerformerUserFullName = authenticate.UserFullName,

                        },
                    PRModel.ProductGroupId
                    , ConfirmSendNotification());
                    }
                    return ServiceResultFactory.CreateSuccess(true);
                }
               
                
                

                return ServiceResultFactory.CreateError(false, MessageId.SaveFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        private List<NotifToDto> CheckSendNotification()
        {
            return new List<NotifToDto>{ new NotifToDto
                    {
                        NotifEvent = NotifEvent.ConfirmPurchaseRequest,
                        Roles = new List<string>
                    {
                      SCMRole.PurchaseRequestConfirm
                    }

                    }};
        }

        public async Task<ServiceResult<List<ListPurchaseRequestDto>>> GetPurchaseRequestAsync(AuthenticateDto authenticate, PurchaseRequestQueryDto query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<ListPurchaseRequestDto>>(null, MessageId.AccessDenied);

                var dbQuery = _purchaseRequestRepository
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && x.ContractCode == authenticate.ContractCode)
                    .AsQueryable();

                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.ProductGroupIds.Contains(a.ProductGroupId));

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery
                        .Where(x => x.PRCode.Contains(query.SearchText)
                        || x.ContractCode.Contains(query.SearchText)
                        || (x.Mrp != null && x.Mrp.MrpNumber.Contains(query.SearchText))
                        || x.PurchaseRequestItems.Any(c => !c.IsDeleted
                        && (c.Product.Description.Contains(query.SearchText)
                        || c.Product.ProductCode.Contains(query.SearchText)))
                         );

                if (query.Status != null && query.Status.Count() > 0)
                    dbQuery = dbQuery.Where(x => query.Status.Contains(x.PRStatus));
                if (!string.IsNullOrEmpty(query.PurchaseNumber))
                    dbQuery = dbQuery.Where(x => x.PRCode==query.PurchaseNumber);
                var totalCount = dbQuery.Count();
                var columnsMap = new Dictionary<string, Expression<Func<PurchaseRequest, object>>>
                {
                    ["Id"] = v => v.Id,
                    ["PRCode"] = v => v.PRCode
                };

                var list = dbQuery.ApplayOrdering(query, columnsMap).ApplayPageing(query);
                var result = await list.Select(p => new ListPurchaseRequestDto
                {
                    Id = p.Id,
                    PRStatus = p.PRStatus,
                    PRCode = p.PRCode,
                    ContractCode = p.ContractCode,
                    TypeOfInquiry = p.TypeOfInquiry,
                    MrpId = p.MrpId,
                    MrpNumber = p.Mrp != null ? p.Mrp.MrpNumber : "",
                    ProductGroupTitle = p.ProductGroup.Title,
                    ProductGroupId = p.ProductGroupId,
                    PurchasingStream = p.PurchaseRequestItems.Any(a => !a.IsDeleted && a.MrpItem.BomProduct.IsRequiredMRP) ? PurchasingStream.WithoutMrp : PurchasingStream.WithMrp,
                    PurchaseItemQuantity = GetPurchaseRequestItem(p.PurchaseRequestItems.Where(a => !a.IsDeleted).ToList()),
                    Products = GetProductsForPurchaseRequest(p.PurchaseRequestItems
                    .Where(a => !a.IsDeleted).Select(a => a.Product.Description).ToList()),

                    DateStart = p.PurchaseRequestItems.Where(a => !a.IsDeleted)
                    .OrderBy(c => c.DateStart)
                    .Select(v => v.DateStart).FirstOrDefault().ToUnixTimestamp(),

                    DateEnd = p.PurchaseRequestItems.Where(a => !a.IsDeleted)
                    .OrderByDescending(c => c.DateEnd)
                    .Select(v => v.DateEnd).FirstOrDefault().ToUnixTimestamp(),

                    UserAudit = p.AdderUser != null
                        ? new UserAuditLogDto
                        {
                            AdderUserId = p.AdderUserId,
                            AdderUserName = p.AdderUser.FullName,
                            CreateDate = p.UpdateDate.ToUnixTimestamp(),
                            AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall +
                                             p.AdderUser.Image
                        }
                        : null,
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<ListPurchaseRequestDto>>(null, exception);
            }
        }
        public async Task<ServiceResult<List<ListPendingForConfirmPurchaseRequestDto>>> GetPendingForConfirmPurchaseRequestAsync(AuthenticateDto authenticate, PurchaseRequestQueryDto query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<ListPendingForConfirmPurchaseRequestDto>>(null, MessageId.AccessDenied);

                var dbQuery = _purchaseRequestConfirmWorkFlowRepository
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && x.PurchaseRequest.ContractCode == authenticate.ContractCode && x.Status == ConfirmationWorkFlowStatus.Pending && x.PurchaseRequest.PRStatus == PRStatus.Register)
                    .AsQueryable();

                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.ProductGroupIds.Contains(a.PurchaseRequest.ProductGroupId));

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery
                        .Where(x => x.PurchaseRequest.PRCode.Contains(query.SearchText)
                        || x.PurchaseRequest.ContractCode.Contains(query.SearchText)
                        || (x.PurchaseRequest.Mrp != null && x.PurchaseRequest.Mrp.MrpNumber.Contains(query.SearchText))
                        || x.PurchaseRequest.PurchaseRequestItems.Any(c => !c.IsDeleted
                        && (c.Product.Description.Contains(query.SearchText)
                        || c.Product.ProductCode.Contains(query.SearchText)))
                         );


                var totalCount = dbQuery.Count();
                var columnsMap = new Dictionary<string, Expression<Func<PurchaseConfirmationWorkFlow, object>>>
                {
                    ["Id"] = v => v.PurchaseRequest.Id,
                    ["PRCode"] = v => v.PurchaseRequest.PRCode
                };

                var list = dbQuery.ApplayOrdering(query, columnsMap).ApplayPageing(query);
                var result = await list.Select(p => new ListPendingForConfirmPurchaseRequestDto
                {
                    Id = p.PurchaseRequest.Id,
                    PRStatus = p.PurchaseRequest.PRStatus,
                    PRCode = p.PurchaseRequest.PRCode,
                    ContractCode = p.PurchaseRequest.ContractCode,
                    TypeOfInquiry = p.PurchaseRequest.TypeOfInquiry,
                    MrpId = p.PurchaseRequest.MrpId,
                    MrpNumber = p.PurchaseRequest.Mrp != null ? p.PurchaseRequest.Mrp.MrpNumber : "",
                    ProductGroupTitle = p.PurchaseRequest.ProductGroup.Title,
                    ProductGroupId = p.PurchaseRequest.ProductGroupId,
                    PurchasingStream = p.PurchaseRequest.PurchaseRequestItems.Any(a => !a.IsDeleted && !a.MrpItem.BomProduct.IsRequiredMRP) ? PurchasingStream.WithoutMrp : PurchasingStream.WithMrp,
                    PurchaseItemQuantity = GetPurchaseRequestItem(p.PurchaseRequest.PurchaseRequestItems.Where(a => !a.IsDeleted).ToList()),
                    Products = GetProductsForPurchaseRequest(p.PurchaseRequest.PurchaseRequestItems
                    .Where(a => !a.IsDeleted).Select(a => a.Product.Description).ToList()),

                    DateStart = p.PurchaseRequest.PurchaseRequestItems.Where(a => !a.IsDeleted)
                    .OrderBy(c => c.DateStart)
                    .Select(v => v.DateStart).FirstOrDefault().ToUnixTimestamp(),

                    DateEnd = p.PurchaseRequest.PurchaseRequestItems.Where(a => !a.IsDeleted)
                    .OrderByDescending(c => c.DateEnd)
                    .Select(v => v.DateEnd).FirstOrDefault().ToUnixTimestamp(),

                    UserAudit = p.ModifierUser != null
                        ? new UserAuditLogDto
                        {
                            AdderUserId = p.ModifierUserId,
                            AdderUserName = p.ModifierUser.FullName,
                            CreateDate = p.UpdateDate.ToUnixTimestamp(),
                            AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall +
                                             p.ModifierUser.Image
                        }
                        : null,
                    BallInCourtUser = p.PurchaseConfirmationWorkFlowUsers.Any() ?
                    p.PurchaseConfirmationWorkFlowUsers.Where(a => a.IsBallInCourt)
                    .Select(c => new UserAuditLogDto
                    {
                        AdderUserId = c.UserId,
                        AdderUserName = c.User.FirstName + " " + c.User.LastName,
                        AdderUserImage = c.User.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + c.User.Image : ""
                    }).FirstOrDefault() : new UserAuditLogDto()
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<ListPendingForConfirmPurchaseRequestDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<ListPendingForConfirmPurchaseRequestDto>> GetPendingForConfirmPurchaseRequestByIdAsync(AuthenticateDto authenticate, long purchaseRequsetId)
        {
            try
            {


                var dbQuery = _purchaseRequestConfirmWorkFlowRepository
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && x.PurchaseRequest.ContractCode == authenticate.ContractCode && x.Status == ConfirmationWorkFlowStatus.Pending && x.PurchaseRequest.PRStatus == PRStatus.Register && x.PurchaseRequestId == purchaseRequsetId)
                    .AsQueryable();




                var result = await dbQuery.Select(p => new ListPendingForConfirmPurchaseRequestDto
                {
                    Id = p.PurchaseRequest.Id,
                    PRStatus = p.PurchaseRequest.PRStatus,
                    PRCode = p.PurchaseRequest.PRCode,
                    ContractCode = p.PurchaseRequest.ContractCode,
                    TypeOfInquiry = p.PurchaseRequest.TypeOfInquiry,
                    MrpId = p.PurchaseRequest.MrpId,
                    MrpNumber = p.PurchaseRequest.Mrp != null ? p.PurchaseRequest.Mrp.MrpNumber : "",
                    ProductGroupTitle = p.PurchaseRequest.ProductGroup.Title,
                    ProductGroupId = p.PurchaseRequest.ProductGroupId,
                    PurchasingStream = p.PurchaseRequest.PurchaseRequestItems.Any(a => !a.IsDeleted && !a.MrpItem.BomProduct.IsRequiredMRP) ? PurchasingStream.WithoutMrp : PurchasingStream.WithMrp,
                    PurchaseItemQuantity = GetPurchaseRequestItem(p.PurchaseRequest.PurchaseRequestItems.Where(a => !a.IsDeleted).ToList()),
                    Products = GetProductsForPurchaseRequest(p.PurchaseRequest.PurchaseRequestItems
                    .Where(a => !a.IsDeleted).Select(a => a.Product.Description).ToList()),

                    DateStart = p.PurchaseRequest.PurchaseRequestItems.Where(a => !a.IsDeleted)
                    .OrderBy(c => c.DateStart)
                    .Select(v => v.DateStart).FirstOrDefault().ToUnixTimestamp(),

                    DateEnd = p.PurchaseRequest.PurchaseRequestItems.Where(a => !a.IsDeleted)
                    .OrderByDescending(c => c.DateEnd)
                    .Select(v => v.DateEnd).FirstOrDefault().ToUnixTimestamp(),

                    UserAudit = p.ModifierUser != null
                        ? new UserAuditLogDto
                        {
                            AdderUserId = p.ModifierUserId,
                            AdderUserName = p.ModifierUser.FullName,
                            CreateDate = p.UpdateDate.ToUnixTimestamp(),
                            AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall +
                                             p.ModifierUser.Image
                        }
                        : null,
                    BallInCourtUser = p.PurchaseConfirmationWorkFlowUsers.Any() ?
                    p.PurchaseConfirmationWorkFlowUsers.Where(a => a.IsBallInCourt)
                    .Select(c => new UserAuditLogDto
                    {
                        AdderUserId = c.UserId,
                        AdderUserName = c.User.FirstName + " " + c.User.LastName,
                        AdderUserImage = c.User.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + c.User.Image : ""
                    }).FirstOrDefault() : new UserAuditLogDto()
                }).FirstOrDefaultAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<ListPendingForConfirmPurchaseRequestDto>(null, exception);
            }
        }
        public async Task<ServiceResult<PurchaseRequestInfoDto>> GetPurchaseRequestByIdAsync(AuthenticateDto authenticate,
            long purchaseRequestId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<PurchaseRequestInfoDto>(null, MessageId.AccessDenied);

                var dbQuery = _purchaseRequestRepository
                    .AsNoTracking()
                    .Where(a => a.Id == purchaseRequestId && a.ContractCode == authenticate.ContractCode);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.ProductGroupId)))
                    return ServiceResultFactory.CreateError<PurchaseRequestInfoDto>(null, MessageId.AccessDenied);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError(new PurchaseRequestInfoDto(), MessageId.EntityDoesNotExist);

                var result = await dbQuery
                    .AsNoTracking()
                    .Include(a => a.PurchaseConfirmationWorkFlows)
                    .Where(a => a.Id == purchaseRequestId).Select(p => new PurchaseRequestInfoDto
                    {
                        Id = p.Id,
                        PRStatus = p.PRStatus,
                        PRCode = p.PRCode,
                        ContractCode = p.ContractCode,
                        TypeOfInquiry = p.TypeOfInquiry,
                        MrpId = p.MrpId,
                        MrpCode = p.Mrp != null ? p.Mrp.MrpNumber : null,
                        MrpDescription = p.Mrp != null ? p.Mrp.Description : null,
                        ProductGroupId = p.ProductGroupId,
                        ProductGroupTitle = p.ProductGroup.Title,
                        ConfirmNote = p.ConfirmNote,
                        Note = p.Note,
                        IsEditable = (p.PRStatus != PRStatus.Register || (p.PRStatus == PRStatus.Register && p.PurchaseConfirmationWorkFlows.Any(a => !a.IsDeleted && a.PurchaseConfirmationWorkFlowUsers.Any(b => b.IsAccept)))) ? false : true,
                        PurchasingStream = (p.Mrp.MrpItems.Any(a => !a.IsDeleted && !a.BomProduct.IsRequiredMRP)) ? PurchasingStream.WithoutMrp : PurchasingStream.WithMrp,
                        PurchaseRequestItems = p.PurchaseRequestItems
                  .Where(c => !c.IsDeleted)
                  .Select(c => new PurchaseRequestItemInfoDto
                  {
                      Id = c.Id,
                      DateEnd = c.DateEnd.ToUnixTimestamp(),
                      DateStart = c.DateStart.ToUnixTimestamp(),
                      ProductCode = c.Product.ProductCode,
                      ProductDescription = c.Product.Description,
                      ProductGroupName = c.Product.ProductGroup.Title,
                      ProductId = c.ProductId,
                      ProductTechnicalNumber = c.Product.TechnicalNumber,
                      ProductUnit = c.Product.Unit,
                      PurchaseRequestId = c.PurchaseRequestId,
                      Quntity = c.Quntity,
                      DocumentStatus =
                            !c.Product.DocumentProducts.Any(v => !v.Document.IsDeleted && v.Document.IsActive && v.Document.ContractCode == authenticate.ContractCode)
                            ? EngineeringDocumentStatus.No
                            : c.Product.DocumentProducts.Any(v => !v.Document.IsDeleted && v.Document.IsActive && v.Document.ContractCode == authenticate.ContractCode &&
                            (!v.Document.DocumentRevisions.Any(a => !a.IsDeleted) || v.Document.DocumentRevisions.Any(n => !n.IsDeleted && n.RevisionStatus < RevisionStatus.Confirmed)))
                            ? EngineeringDocumentStatus.completing
                            : EngineeringDocumentStatus.Completed,
                  }).ToList(),

                        PurchaseRequestConfirmUserAudit = (p.PurchaseConfirmationWorkFlows.FirstOrDefault() != null && p.PurchaseConfirmationWorkFlows.FirstOrDefault().AdderUser != null) ? new UserAuditLogDto
                        {
                            CreateDate = p.PurchaseConfirmationWorkFlows.First().CreatedDate.ToUnixTimestamp(),
                            AdderUserName = p.PurchaseConfirmationWorkFlows.First().AdderUser.FullName,
                            AdderUserImage = p.PurchaseConfirmationWorkFlows.First().AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + p.PurchaseConfirmationWorkFlows.First().AdderUser.Image : ""
                        } : null,
                        PurchaseRequestConfirmationUserWorkFlows = (p.PurchaseConfirmationWorkFlows.FirstOrDefault() != null) ? p.PurchaseConfirmationWorkFlows.First().PurchaseConfirmationWorkFlowUsers.Where(a => a.UserId > 0)
                            .Select(e => new PurchaseRequestConfirmationUserWorkFlowDto
                            {
                                PurchaseRequestConfirmationWorkFlowUserId = e.PurchaseRequestConfirmWorkFlowUserId,
                                DateEnd = e.DateEnd.ToUnixTimestamp(),
                                DateStart = e.DateStart.ToUnixTimestamp(),
                                IsAccept = e.IsAccept,
                                IsBallInCourt = e.IsBallInCourt,
                                Note = e.Note,
                                OrderNumber = e.OrderNumber,
                                UserId = e.UserId,
                                UserFullName = e.User != null ? e.User.FullName : "",
                                UserImage = e.User != null && e.User.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + e.User.Image : ""
                            }).ToList() : new List<PurchaseRequestConfirmationUserWorkFlowDto>(),
                        Attachments = (p.PurchaseConfirmationWorkFlows.First() != null) ? p.PurchaseConfirmationWorkFlows.First().PurchaseConfirmationAttachments.Where(x => !x.IsDeleted)
                  .Select(c => new BasePRAttachmentDto
                  {
                      Id = c.Id,
                      FileName = c.FileName,
                      FileSize = c.FileSize,
                      FileSrc = c.FileSrc,
                      FileType = c.FileType,
                      PurchaseRequestId = p.Id,
                  }).ToList() : new List<BasePRAttachmentDto>(),
                        UserAudit = p.AdderUser != null
                      ? new UserAuditLogDto
                      {
                          AdderUserId = p.AdderUserId,
                          AdderUserName = p.AdderUser.FullName,
                          CreateDate = p.CreatedDate.ToUnixTimestamp(),
                          AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall +
                                           p.AdderUser.Image
                      }
                      : null
                    }).FirstOrDefaultAsync();
                List<PurchaseRequestItemInfoDto> purchaseItems = new List<PurchaseRequestItemInfoDto>();
                foreach (var item in result.PurchaseRequestItems)
                {
                    if (!purchaseItems.Any(a => a.ProductId == item.ProductId))
                    {
                        purchaseItems.Add(new PurchaseRequestItemInfoDto
                        {
                            Id = item.Id,
                            DateEnd = item.DateEnd,
                            DateStart = item.DateStart,
                            ProductCode = item.ProductCode,
                            ProductDescription = item.ProductDescription,
                            ProductGroupName = item.ProductGroupName,
                            ProductId = item.ProductId,
                            ProductTechnicalNumber = item.ProductTechnicalNumber,
                            ProductUnit = item.ProductUnit,
                            PurchaseRequestId = item.PurchaseRequestId,
                            Quntity = result.PurchaseRequestItems.Where(a => a.ProductId == item.ProductId).Sum(a => a.Quntity),
                            DocumentStatus = item.DocumentStatus
                        });
                    }
                }
                result.PurchaseRequestItems = purchaseItems;
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new PurchaseRequestInfoDto(), exception);
            }
        }
        public async Task<ServiceResult<PurchaseRequestEditInfoDto>> GetPurchaseRequestByIdForEditAsync(AuthenticateDto authenticate, long purchaseRequestId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<PurchaseRequestEditInfoDto>(null, MessageId.AccessDenied);

                var dbQuery = _purchaseRequestRepository
                    .AsNoTracking()
                    .Where(a => a.Id == purchaseRequestId && a.ContractCode == authenticate.ContractCode);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.ProductGroupId)))
                    return ServiceResultFactory.CreateError<PurchaseRequestEditInfoDto>(null, MessageId.AccessDenied);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError(new PurchaseRequestEditInfoDto(), MessageId.EntityDoesNotExist);

                var result = await dbQuery
                    .AsNoTracking()
                    .Include(a => a.PurchaseConfirmationWorkFlows)
                    .Where(a => a.Id == purchaseRequestId).Select(p => new PurchaseRequestEditInfoDto
                    {
                        Id = p.Id,
                        PRStatus = p.PRStatus,
                        PRCode = p.PRCode,
                        ContractCode = p.ContractCode,
                        TypeOfInquiry = p.TypeOfInquiry,
                        MrpId = p.MrpId,
                        MrpCode = p.Mrp != null ? p.Mrp.MrpNumber : null,
                        MrpDescription = p.Mrp != null ? p.Mrp.Description : null,
                        ProductGroupId = p.ProductGroupId,
                        ProductGroupTitle = p.ProductGroup.Title,
                        ConfirmNote = p.ConfirmNote,
                        Note = p.Note,
                        PurchasingStream = (p.Mrp.MrpItems.Any(a => !a.IsDeleted && !a.BomProduct.IsRequiredMRP)) ? PurchasingStream.WithoutMrp : PurchasingStream.WithMrp,
                        IsEditable = ((p.PurchaseConfirmationWorkFlows.Any(a => !a.IsDeleted && a.PurchaseConfirmationWorkFlowUsers.Any(b => b.IsAccept)))) ? false : true,
                        PurchaseRequestItems = p.PurchaseRequestItems
                  .Where(c => !c.IsDeleted)
                  .Select(c => new EditPurchaseRequestItemInfoDto
                  {
                      Id = c.Id,
                      DateEnd = c.DateEnd.ToUnixTimestamp(),
                      DateStart = c.DateStart.ToUnixTimestamp(),
                      ProductCode = c.Product.ProductCode,
                      ProductDescription = c.Product.Description,
                      ProductGroupName = c.Product.ProductGroup.Title,
                      ProductId = c.ProductId,
                      ProductTechnicalNumber = c.Product.TechnicalNumber,
                      ProductUnit = c.Product.Unit,
                      PurchaseRequestId = c.PurchaseRequestId,
                      Quntity = c.PurchaseRequest.Mrp.MrpItems.Where(a => !a.IsDeleted && a.ProductId == c.ProductId).Sum(b => b.RemainedStock),
                      RequiredQuantity = c.Quntity,
                      DocumentStatus =
                            !c.Product.DocumentProducts.Any(v => !v.Document.IsDeleted && v.Document.IsActive && v.Document.ContractCode == authenticate.ContractCode)
                            ? EngineeringDocumentStatus.No
                            : c.Product.DocumentProducts.Any(v => !v.Document.IsDeleted && v.Document.IsActive && v.Document.ContractCode == authenticate.ContractCode &&
                            (!v.Document.DocumentRevisions.Any(a => !a.IsDeleted) || v.Document.DocumentRevisions.Any(n => !n.IsDeleted && n.RevisionStatus < RevisionStatus.Confirmed)))
                            ? EngineeringDocumentStatus.completing
                            : EngineeringDocumentStatus.Completed,
                  }).ToList(),

                        PurchaseRequestConfirmUserAudit = (p.PurchaseConfirmationWorkFlows.FirstOrDefault() != null && p.PurchaseConfirmationWorkFlows.FirstOrDefault().AdderUser != null) ? new UserAuditLogDto
                        {
                            CreateDate = p.PurchaseConfirmationWorkFlows.First().CreatedDate.ToUnixTimestamp(),
                            AdderUserName = p.PurchaseConfirmationWorkFlows.First().AdderUser.FullName,
                            AdderUserImage = p.PurchaseConfirmationWorkFlows.First().AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + p.PurchaseConfirmationWorkFlows.First().AdderUser.Image : ""
                        } : null,
                        Attachments = (p.PurchaseConfirmationWorkFlows.First() != null) ? p.PurchaseConfirmationWorkFlows.First().PurchaseConfirmationAttachments.Where(x => !x.IsDeleted)
                  .Select(c => new BasePRAttachmentDto
                  {
                      Id = c.Id,
                      FileName = c.FileName,
                      FileSize = c.FileSize,
                      FileSrc = c.FileSrc,
                      FileType = c.FileType,
                      PurchaseRequestId = p.Id,
                  }).ToList() : new List<BasePRAttachmentDto>(),
                        UserAudit = p.AdderUser != null
                      ? new UserAuditLogDto
                      {
                          AdderUserId = p.AdderUserId,
                          AdderUserName = p.AdderUser.FullName,
                          CreateDate = p.CreatedDate.ToUnixTimestamp(),
                          AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall +
                                           p.AdderUser.Image
                      }
                      : null
                    }).FirstOrDefaultAsync();
                List<EditPurchaseRequestItemInfoDto> purchaseItems = new List<EditPurchaseRequestItemInfoDto>();
                foreach (var item in result.PurchaseRequestItems)
                {
                    if (!purchaseItems.Any(a => a.ProductId == item.ProductId))
                    {
                        purchaseItems.Add(new EditPurchaseRequestItemInfoDto
                        {
                            Id = item.Id,
                            DateEnd = item.DateEnd,
                            DateStart = item.DateStart,
                            ProductCode = item.ProductCode,
                            ProductDescription = item.ProductDescription,
                            ProductGroupName = item.ProductGroupName,
                            ProductId = item.ProductId,
                            ProductTechnicalNumber = item.ProductTechnicalNumber,
                            ProductUnit = item.ProductUnit,
                            PurchaseRequestId = item.PurchaseRequestId,
                            Quntity = item.Quntity + result.PurchaseRequestItems.Where(a => a.ProductId == item.ProductId).Sum(a => a.RequiredQuantity),
                            RequiredQuantity = result.PurchaseRequestItems.Where(a => a.ProductId == item.ProductId).Sum(a => a.RequiredQuantity),
                            DocumentStatus = item.DocumentStatus
                        });
                    }
                }
                result.PurchaseRequestItems = purchaseItems;
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new PurchaseRequestEditInfoDto(), exception);
            }
        }

        public async Task<ServiceResult<List<PurchaseRequestItemInfoDto>>> GetPurchaseRequestItemsByPRIdAsync(AuthenticateDto authenticate,
            long purchaseRequestId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<PurchaseRequestItemInfoDto>>(null, MessageId.AccessDenied);

                var dbQuery = _purchaseRequestItemRepository
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && x.PurchaseRequestId == purchaseRequestId && x.PurchaseRequest.ContractCode == authenticate.ContractCode);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<List<PurchaseRequestItemInfoDto>>(null, MessageId.EntityDoesNotExist);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.PurchaseRequest.ProductGroupId)))
                    return ServiceResultFactory.CreateError<List<PurchaseRequestItemInfoDto>>(null, MessageId.AccessDenied);

                var purchaseOrderList = await dbQuery
                    .Select(bs => new PurchaseRequestItemInfoDto
                    {
                        Id = bs.Id,
                        PurchaseRequestId = bs.PurchaseRequestId,
                        ProductId = bs.ProductId,
                        ProductCode = bs.Product.ProductCode,
                        ProductDescription = bs.Product.Description,
                        ProductUnit = bs.Product.Unit,
                        ProductTechnicalNumber = bs.Product.TechnicalNumber,
                        ProductGroupName = bs.Product.ProductGroup.Title,
                        Quntity = bs.Quntity,
                        DateEnd = bs.DateEnd.ToUnixTimestamp(),
                        DateStart = bs.DateStart.ToUnixTimestamp(),
                        DocumentStatus =
                            !bs.Product.DocumentProducts.Any(v => !v.Document.IsDeleted && v.Document.IsActive && v.Document.ContractCode == authenticate.ContractCode)
                            ? EngineeringDocumentStatus.No
                            : bs.Product.DocumentProducts.Any(v => !v.Document.IsDeleted && v.Document.IsActive && v.Document.ContractCode == authenticate.ContractCode &&
                            (!v.Document.DocumentRevisions.Any(a => !a.IsDeleted) || v.Document.DocumentRevisions.Any(n => !n.IsDeleted && n.RevisionStatus < RevisionStatus.Confirmed)))
                            ? EngineeringDocumentStatus.completing
                            : EngineeringDocumentStatus.Completed,
                    }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(purchaseOrderList);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new List<PurchaseRequestItemInfoDto>(), exception);
            }
        }


        public async Task<ServiceResult<bool>> ConfirmPurchaseRequestAsync(AuthenticateDto authenticate,
           long purchaseRequestId, AddPrConfirmDto model)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery = _purchaseRequestRepository
                    .Where(c => !c.IsDeleted && c.Id == purchaseRequestId && c.ContractCode == authenticate.ContractCode);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(c => c.PurchaseRequestItems.Any(v => !v.IsDeleted && permission.ProductGroupIds.Contains(v.Product.ProductGroupId))))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var purchaseRequest = await dbQuery
                    .Include(a => a.ProductGroup)
                    .FirstOrDefaultAsync();
                if (purchaseRequest.PRStatus >= PRStatus.Confirm)
                {
                    return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);
                }

                // before change => for get log
                string oldObject = _scmLogAndNotificationService.SerializerObject(purchaseRequest);

                purchaseRequest.ConfirmNote = model.Note;
                purchaseRequest.PRStatus = PRStatus.Confirm;



                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    await _scmLogAndNotificationService.SetDonedNotificationAsync(authenticate.UserId, purchaseRequest.ContractCode, purchaseRequest.Id.ToString(), NotifEvent.ConfirmPurchaseRequest);

                    await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = purchaseRequest.ContractCode,
                        FormCode = purchaseRequest.PRCode,
                        KeyValue = purchaseRequest.Id.ToString(),
                        NotifEvent = NotifEvent.ConfirmPurchaseRequest,
                        ProductGroupId = purchaseRequest.ProductGroupId,
                        RootKeyValue = purchaseRequest.Id.ToString(),
                        Message = purchaseRequest.ProductGroup.Title,
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,

                    },
                    purchaseRequest.ProductGroupId
                    , ConfirmSendNotification());
                    return ServiceResultFactory.CreateSuccess(true);
                }
                return ServiceResultFactory.CreateError(false, MessageId.EditEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        private List<NotifToDto> ConfirmSendNotification()
        {
            return new List<NotifToDto>{ new NotifToDto
            {
                        NotifEvent = NotifEvent.AddRFP,
                        Roles = new List<string>
                        {
                                  SCMRole.RFPMng,
                        }
            }};
        }

        public async Task<ServiceResult<bool>> RejectPurchaseRequestAsync(AuthenticateDto authenticate,
            long purchaseRequestId, AddPrConfirmDto model)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery = _purchaseRequestRepository
                    .Where(c => !c.IsDeleted && c.Id == purchaseRequestId && c.ContractCode == authenticate.ContractCode);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => a.PurchaseRequestItems.Any(c => !c.IsDeleted && permission.ProductGroupIds.Contains(c.PurchaseRequest.ProductGroupId))))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var purchaseRequestModel = await dbQuery
                    .Include(a => a.ProductGroup)
                    .Include(a => a.PurchaseRequestItems)
                    .FirstOrDefaultAsync();
                if (purchaseRequestModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (purchaseRequestModel.PRStatus >= PRStatus.Confirm)
                {
                    return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);
                }

                var prItemProductIds = purchaseRequestModel.PurchaseRequestItems.Where(a => !a.IsDeleted)
                    .Select(c => c.ProductId).ToList();

                var mrpItems = await _mrpPlanningRepository
                    .Where(a => !a.IsDeleted && a.MrpId == purchaseRequestModel.MrpId && prItemProductIds.Contains(a.ProductId))
                    .ToListAsync();

                if (mrpItems == null || mrpItems.Count() != prItemProductIds.Count())
                    return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                UpdateMrpItemAfterRejectPR(purchaseRequestModel.PurchaseRequestItems.Where(a => !a.IsDeleted).ToList(), mrpItems);

                purchaseRequestModel.ConfirmNote = model.Note;
                purchaseRequestModel.PRStatus = PRStatus.Reject;



                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    await _scmLogAndNotificationService.SetDonedNotificationAsync(authenticate.UserId, purchaseRequestModel.ContractCode, purchaseRequestModel.Id.ToString(), NotifEvent.ConfirmPurchaseRequest);
                    await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = purchaseRequestModel.ContractCode,
                        FormCode = purchaseRequestModel.PRCode,
                        KeyValue = purchaseRequestModel.Id.ToString(),
                        NotifEvent = NotifEvent.RejectPurchaseRequest,
                        ProductGroupId = purchaseRequestModel.ProductGroupId,
                        RootKeyValue = purchaseRequestModel.Id.ToString(),
                        Message = purchaseRequestModel.ProductGroup.Title,
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,
                        //SendNotifRoles = new List<string>
                        //{
                        //    SCMRole.RFPManagement,
                        //    SCMRole.RFPRegister,
                        //    SCMRole.RFPObserver
                        //}
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

        public async Task<ServiceResult<bool>> EditPurchaseRequestAsync(AuthenticateDto authenticate, long purchaseRequestId, EditPurchaseRequestDto model)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery = _purchaseRequestRepository

                    .Where(x => !x.IsDeleted && x.Id == purchaseRequestId && x.ContractCode == authenticate.ContractCode);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);



                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => a.PurchaseRequestItems.Any(c => !c.IsDeleted && permission.ProductGroupIds.Contains(c.PurchaseRequest.ProductGroupId))))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var purchaseRequestModel = await dbQuery
                    .Include(a => a.ProductGroup)
                    .Include(a => a.PurchaseRequestItems)
                    .ThenInclude(a => a.MrpItem)
                    .Include(a => a.PRAttachments)
                    .Include(a => a.PurchaseConfirmationWorkFlows)
                    .ThenInclude(a => a.PurchaseConfirmationWorkFlowUsers)
                    .Include(a => a.PurchaseRequestItems)
                    .ThenInclude(a => a.RFPItems)
                    .FirstOrDefaultAsync();

                if (purchaseRequestModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (purchaseRequestModel.PRStatus >= PRStatus.Register && purchaseRequestModel.PurchaseConfirmationWorkFlows.Any(a => !a.IsDeleted && a.PurchaseConfirmationWorkFlowUsers.Any(b => b.IsAccept)))
                    return ServiceResultFactory.CreateError(false, MessageId.EditDontAllowedAfterConfirm);

                if (purchaseRequestModel.PRStatus == PRStatus.Reject)
                    return ServiceResultFactory.CreateError(false, MessageId.EditDontAllowedAfterConfirm);

                if (purchaseRequestModel.PurchaseRequestItems.Any(a=>!a.IsDeleted&&a.RFPItems.Any(a=>!a.IsDeleted&&a.IsActive)))
                    return ServiceResultFactory.CreateError(false, MessageId.EditDontAllowedAfterConfirm);

                if (model.PurchaseRequestItems == null || model.PurchaseRequestItems.Count() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);
                if (model.PurchaseRequestItems.Any(a => a.Quntity <= 0))
                    return ServiceResultFactory.CreateError(false, MessageId.QuantityCantLessOrEqualZero);
                var workflow = purchaseRequestModel.PurchaseConfirmationWorkFlows.FirstOrDefault(a => !a.IsDeleted);
                var oldConfirmer = workflow.PurchaseConfirmationWorkFlowUsers;
                var confirmResult = await AddPurchaseRequestConfirmationsAsync(workflow, model.WorkFlow);
                if (!confirmResult.Succeeded)
                    return ServiceResultFactory.CreateError(false, MessageId.OperationFailed);
                workflow = confirmResult.Result;
                if (workflow.Status == ConfirmationWorkFlowStatus.Confirm)
                    purchaseRequestModel.PRStatus = PRStatus.Confirm;
                else
                    purchaseRequestModel.PRStatus = PRStatus.Register;
                // before change => for get log
                //string oldObject = _scmLogAndNotificationService.SerializerObject(purchaseRequestModel);

                // update pr note and 
                purchaseRequestModel.Note = model.Note;
                purchaseRequestModel.TypeOfInquiry = TypeOfInquiry.TCRFP;

                var updateResult = await EditPurchaseRequestItemAsync(
                    purchaseRequestId,
                    purchaseRequestModel.MrpId.Value,
                    purchaseRequestModel.PurchaseRequestItems.Where(a => !a.IsDeleted).ToList(),
                    model.PurchaseRequestItems, purchaseRequestModel);

                if (!updateResult.Succeeded)
                    return ServiceResultFactory.CreateError(false, updateResult.Messages.First().Message);


                var updateAttachResult = await UpdatePRAttachmentAsync(authenticate.ContractCode, purchaseRequestModel.MrpId.Value, workflow, model.PRAttachments);

                if (!updateAttachResult.Succeeded)
                    return ServiceResultFactory.CreateError(false, updateAttachResult.Messages.First().Message);

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    foreach (var item in oldConfirmer)
                        await _scmLogAndNotificationService.SetDonedNotificationAsync(item.UserId, purchaseRequestModel.ContractCode, purchaseRequestModel.Id.ToString(), NotifEvent.ConfirmPurchaseRequest);
                    foreach (var item in workflow.PurchaseConfirmationWorkFlowUsers)
                        await _scmLogAndNotificationService.SetDonedNotificationAsync(item.UserId, purchaseRequestModel.ContractCode, purchaseRequestModel.MrpId.ToString(), NotifEvent.ConfirmPurchaseRequest);
                    int? userId = null;
                    if (purchaseRequestModel.PRStatus != PRStatus.Confirm)
                    {
                        userId= workflow.PurchaseConfirmationWorkFlowUsers.First(a => a.IsBallInCourt).UserId;
                    }
                    var res = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = purchaseRequestModel.ContractCode,
                        FormCode = purchaseRequestModel.PRCode,
                        KeyValue = purchaseRequestModel.Id.ToString(),
                        ProductGroupId = purchaseRequestModel.ProductGroupId,
                        RootKeyValue = purchaseRequestModel.Id.ToString(),
                        NotifEvent = NotifEvent.EditPurchaseRequest,
                        Message = purchaseRequestModel.ProductGroup.Title,
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName
                    }, purchaseRequestModel.ProductGroupId,NotifEvent.ConfirmPurchaseRequest, userId);
                    return ServiceResultFactory.CreateSuccess(true);
                }
                return ServiceResultFactory.CreateError(false, MessageId.EditEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }


        public async Task<ServiceResult<bool>> EditPurchaseRequestBySysAdminAsync(AuthenticateDto authenticate, long purchaseRequestId, EditPurchaseRequestBySysAdminDto model)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery = _purchaseRequestRepository

                    .Where(x => !x.IsDeleted && x.Id == purchaseRequestId && x.ContractCode == authenticate.ContractCode);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);



                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => a.PurchaseRequestItems.Any(c => !c.IsDeleted && permission.ProductGroupIds.Contains(c.PurchaseRequest.ProductGroupId))))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var purchaseRequestModel = await dbQuery
                    .Include(a => a.ProductGroup)
                    .Include(a => a.PurchaseRequestItems)
                    .ThenInclude(a => a.MrpItem)
                    .Include(a => a.PRAttachments)
                    .Include(a => a.PurchaseConfirmationWorkFlows)
                    .ThenInclude(a => a.PurchaseConfirmationWorkFlowUsers)
                    .Include(a => a.PurchaseRequestItems)
                    .ThenInclude(a => a.RFPItems)
                    .ThenInclude(a=>a.RFP)
                    .FirstOrDefaultAsync();

                if (purchaseRequestModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);




                if (model.PurchaseRequestItems == null || model.PurchaseRequestItems.Count() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                if (model.PurchaseRequestItems.Any(a => a.RequiredQuantity <= 0))
                    return ServiceResultFactory.CreateError(false, MessageId.QuantityCantLessOrEqualZero);
                // before change => for get log
                //string oldObject = _scmLogAndNotificationService.SerializerObject(purchaseRequestModel);

                // update pr note and 

                purchaseRequestModel.TypeOfInquiry = TypeOfInquiry.TCRFP;

                var updateResult = await EditPurchaseRequestItemSysAdminAsync(
                    purchaseRequestId,
                    purchaseRequestModel.MrpId.Value,
                    purchaseRequestModel.PurchaseRequestItems.Where(a => !a.IsDeleted).ToList(),
                    model.PurchaseRequestItems, purchaseRequestModel);

                if (!updateResult.Succeeded)
                    return ServiceResultFactory.CreateError(false, updateResult.Messages.First().Message);

                var workflow = purchaseRequestModel.PurchaseConfirmationWorkFlows.FirstOrDefault(a => !a.IsDeleted);
                var updateAttachResult = await UpdatePRAttachmentAsync(authenticate.ContractCode, purchaseRequestModel.MrpId.Value, workflow, model.PRAttachments);

                if (!updateAttachResult.Succeeded)
                    return ServiceResultFactory.CreateError(false, updateAttachResult.Messages.First().Message);

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    //var res = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    //{
                    //    ContractCode = purchaseRequestModel.ContractCode,
                    //    FormCode = purchaseRequestModel.PRCode,
                    //    KeyValue = purchaseRequestModel.Id.ToString(),
                    //    ProductGroupId = purchaseRequestModel.ProductGroupId,
                    //    RootKeyValue = purchaseRequestModel.Id.ToString(),
                    //    NotifEvent = NotifEvent.EditPurchaseRequest,
                    //    Message = purchaseRequestModel.ProductGroup.Title,
                    //    PerformerUserId = authenticate.UserId,
                    //    PerformerUserFullName = authenticate.UserFullName
                    //}, null);
                    return ServiceResultFactory.CreateSuccess(true);
                }
                return ServiceResultFactory.CreateError(false, MessageId.EditEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        #region for => rfp
        public async Task<long> GetWaitingPRForNewRFPBadgeCountAsync()
        {
            try
            {
                var result = await _purchaseRequestRepository
                    .AsNoTracking()
                    .CountAsync(a => !a.IsDeleted && a.PRStatus >= PRStatus.Confirm
                     && a.PurchaseRequestItems.Any(c => !c.IsDeleted && c.PRItemStatus == PRItemStatus.NotStart));

                return result;
            }
            catch (Exception exception)
            {
                return 0;
            }
        }

        public async Task<ServiceResult<List<WaitingPRForNewRFPListDto>>> GetWaitingPRForNewRFPListAsync(AuthenticateDto authenticate, PurchaseRequestQueryDto query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<WaitingPRForNewRFPListDto>>(null, MessageId.AccessDenied);

                var dbQuery = _purchaseRequestRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.PRStatus >= PRStatus.Confirm && a.ContractCode == authenticate.ContractCode &&
                    a.PurchaseRequestItems.Any(c => !c.IsDeleted && c.PRItemStatus == PRItemStatus.NotStart));


                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery
                         .Where(a => a.PurchaseRequestItems.Any(c => !c.IsDeleted && c.PRItemStatus == PRItemStatus.NotStart &&
                           permission.ProductGroupIds.Contains(c.Product.ProductGroupId)));

                dbQuery = ApplayFilterQuery(query, dbQuery);

                var totalCount = dbQuery.Count();
                var columnsMap = new Dictionary<string, Expression<Func<PurchaseRequest, object>>>
                {
                    ["Id"] = v => v.Id,
                    ["PRCode"] = v => v.PRCode
                };

                var list = dbQuery.ApplayOrdering(query, columnsMap).ApplayPageing(query);
                var result = await list.Select(p => new WaitingPRForNewRFPListDto
                {
                    Id = p.Id,
                    PRCode = p.PRCode,
                    ContractCode = p.ContractCode,
                    ProductGroupId = p.ProductGroupId,
                    ProductGroupTitle = p.ProductGroup.Title,
                    Products = GetProductsForPurchaseRequest(p.PurchaseRequestItems
                    .Where(a => !a.IsDeleted && a.PRItemStatus == PRItemStatus.NotStart)
                    .Select(a => a.Product.Description).ToList()),

                    DateStart = p.PurchaseRequestItems.Where(a => !a.IsDeleted)
                    .OrderBy(c => c.DateStart)
                    .Select(v => v.DateStart).FirstOrDefault().ToUnixTimestamp(),

                    DateEnd = p.PurchaseRequestItems.Where(a => !a.IsDeleted)
                    .OrderByDescending(c => c.DateEnd)
                    .Select(v => v.DateEnd).FirstOrDefault().ToUnixTimestamp(),

                    UserAudit = p.AdderUser != null
                        ? new UserAuditLogDto
                        {
                            AdderUserId = p.AdderUserId,
                            AdderUserName = p.AdderUser.FullName,
                            CreateDate = p.CreatedDate.ToUnixTimestamp(),
                            AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall +
                                             p.AdderUser.Image
                        }
                        : null
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<WaitingPRForNewRFPListDto>>(null, exception);
            }
        }

        private static IQueryable<PurchaseRequest> ApplayFilterQuery(PurchaseRequestQueryDto query, IQueryable<PurchaseRequest> dbQuery)
        {
            if (!string.IsNullOrEmpty(query.SearchText))
                dbQuery = dbQuery
                    .Where(x => x.PRCode.Contains(query.SearchText) || x.ContractCode.Contains(query.SearchText));

            if (query.ContractCodes != null && query.ContractCodes.Count() > 0)
                dbQuery = dbQuery.Where(a => query.ContractCodes.Contains(a.ContractCode));

            if (query.ProductGroupIds != null && query.ProductGroupIds.Count() > 0)
                dbQuery = dbQuery.Where(a => a.PurchaseRequestItems.Any(c => !c.IsDeleted && query.ProductGroupIds.Contains(c.Product.ProductGroupId)));

            if (query.ProductIds != null && query.ProductIds.Count() > 0)
                dbQuery = dbQuery.Where(a => a.PurchaseRequestItems.Any(c => !c.IsDeleted && query.ProductIds.Contains(c.ProductId)));

            if (query.Status != null && query.Status.Count() > 0)
                dbQuery = dbQuery.Where(x => query.Status.Contains(x.PRStatus));

            if (query.FromDateTime != null || query.ToDateTime != null)
            {
                switch (query.PRDateQuery)
                {
                    case PRDateQuery.DateCreate:
                        if (query.FromDateTime != null)
                        {
                            var date = query.FromDateTime.UnixTimestampToDateTime();
                            dbQuery = dbQuery.Where(a => a.CreatedDate != null && a.CreatedDate.Value.Date >= date.Value.Date);
                        }
                        if (query.ToDateTime != null)
                        {
                            var date = query.ToDateTime.UnixTimestampToDateTime();
                            dbQuery = dbQuery.Where(a => a.CreatedDate != null && a.CreatedDate.Value.Date <= date.Value.Date);
                        }
                        break;
                    case PRDateQuery.DateStart:
                        if (query.FromDateTime != null)
                        {
                            var date = query.FromDateTime.UnixTimestampToDateTime();
                            dbQuery = dbQuery.Where(a => a.PurchaseRequestItems.Any(c => !c.IsDeleted && c.DateStart.Date >= date.Value.Date));
                        }
                        if (query.ToDateTime != null)
                        {
                            var date = query.ToDateTime.UnixTimestampToDateTime();
                            dbQuery = dbQuery.Where(a => a.PurchaseRequestItems.Any(c => !c.IsDeleted && c.DateStart.Date <= date.Value.Date));
                        }
                        break;
                    case PRDateQuery.DateEnd:
                        if (query.FromDateTime != null)
                        {
                            var date = query.FromDateTime.UnixTimestampToDateTime();
                            dbQuery = dbQuery.Where(a => a.PurchaseRequestItems.Any(c => !c.IsDeleted && c.DateEnd.Date >= date.Value.Date));
                        }
                        if (query.ToDateTime != null)
                        {
                            var date = query.ToDateTime.UnixTimestampToDateTime();
                            dbQuery = dbQuery.Where(a => a.PurchaseRequestItems.Any(c => !c.IsDeleted && c.DateEnd.Date <= date.Value.Date));
                        }
                        break;
                    default:
                        break;
                }
            }

            return dbQuery;
        }

        public async Task<ServiceResult<PRForNewRFPDto>> GetWaitingPRForAddRFPByPRIdAsync(AuthenticateDto authenticate, long prId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<PRForNewRFPDto>(null, MessageId.AccessDenied);

                var dbQuery = _purchaseRequestRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted
                    && a.Id == prId
                    && a.ContractCode == authenticate.ContractCode
                    && a.PurchaseRequestItems.Any(c => !c.IsDeleted && c.PRItemStatus == PRItemStatus.NotStart));

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<PRForNewRFPDto>(null, MessageId.EntityDoesNotExist);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.ProductGroupId)))
                    return ServiceResultFactory.CreateError<PRForNewRFPDto>(null, MessageId.AccessDenied);


                var prModel = await dbQuery
                    .Select(v => new PRForNewRFPDto
                    {
                        Id = v.Id,
                        ContractCode = v.ContractCode,
                        PRCode = v.PRCode,
                        ProductGroupId = v.ProductGroupId,
                        ProductGroupTitle = v.ProductGroup.Title,
                        PRItems = v.PurchaseRequestItems.Where(b => !b.IsDeleted && b.PRItemStatus == PRItemStatus.NotStart)
                        .Select(b => new PRItemsForNewRFPDTO
                        {
                            PRItemId = b.Id,
                            PRCode = v.PRCode,
                            DateEnd = b.DateEnd.ToUnixTimestamp(),
                            DateStart = b.DateStart.ToUnixTimestamp(),
                            ProductCode = b.Product.ProductCode,
                            ProductDescription = b.Product.Description,
                            ProductGroupName = b.Product.ProductGroup.Title,
                            ProductGroupId = b.Product.ProductGroupId,
                            ProductId = b.ProductId,
                            ProductTechnicalNumber = b.Product.TechnicalNumber,
                            ProductUnit = b.Product.Unit,
                            PurchaseRequestId = v.Id,
                            Quntity = b.Quntity,
                            DocumentStatus =
                            !b.Product.DocumentProducts.Any(v => !v.Document.IsDeleted && v.Document.IsActive && v.Document.ContractCode == authenticate.ContractCode)
                            ? EngineeringDocumentStatus.No
                            : b.Product.DocumentProducts.Any(v => !v.Document.IsDeleted && v.Document.IsActive && v.Document.ContractCode == authenticate.ContractCode &&
                            (!v.Document.DocumentRevisions.Any(a => !a.IsDeleted) || v.Document.DocumentRevisions.Any(n => !n.IsDeleted && n.RevisionStatus < RevisionStatus.Confirmed)))
                            ? EngineeringDocumentStatus.completing
                            : EngineeringDocumentStatus.Completed,
                        }).ToList()
                    }).FirstOrDefaultAsync();

                if (prModel == null)
                    return ServiceResultFactory.CreateError<PRForNewRFPDto>(null, MessageId.AccessDenied);

                List<PRItemsForNewRFPDTO> purchaseItems = new List<PRItemsForNewRFPDTO>();
                foreach (var item in prModel.PRItems)
                {
                    if (!purchaseItems.Any(a => a.ProductId == item.ProductId))
                    {
                        purchaseItems.Add(new PRItemsForNewRFPDTO
                        {
                            PRItemId = item.PRItemId,
                            PRCode = item.PRCode,
                            DateEnd = item.DateEnd,
                            DateStart = item.DateStart,
                            ProductCode = item.ProductCode,
                            ProductDescription = item.ProductDescription,
                            ProductGroupName = item.ProductGroupName,
                            ProductGroupId = item.ProductGroupId,
                            ProductId = item.ProductId,
                            ProductTechnicalNumber = item.ProductTechnicalNumber,
                            ProductUnit = item.ProductUnit,
                            PurchaseRequestId = item.PurchaseRequestId,
                            Quntity = prModel.PRItems.Where(a => a.ProductId == item.ProductId).Sum(a => a.Quntity),
                            DocumentStatus = item.DocumentStatus,
                        });
                    }
                }
                prModel.PRItems = purchaseItems;
                return ServiceResultFactory.CreateSuccess(prModel);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<PRForNewRFPDto>(null, exception);
            }
        }

        public async Task<ServiceResult<List<PRItemsForNewRFPDTO>>> GetWaitingPRItemForAddRFPByProductGroupIdAsync(AuthenticateDto authenticate, int productGroupId,
            PurchaseRequestQueryDto query)
        {
            try
            {

                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<PRItemsForNewRFPDTO>>(null, MessageId.AccessDenied);

                var dbQuery = _purchaseRequestItemRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted &&
                    a.PurchaseRequest.ContractCode == authenticate.ContractCode &&
                    a.PRItemStatus == PRItemStatus.NotStart &&
                    a.PurchaseRequest.PRStatus >= PRStatus.Confirm &&
                    a.PurchaseRequest.ProductGroupId == productGroupId &&
                    a.PurchaseRequest.ContractCode == authenticate.ContractCode);

                if (permission.ProductGroupIds.Any() && !permission.ProductGroupIds.Contains(productGroupId))
                    return ServiceResultFactory.CreateError<List<PRItemsForNewRFPDTO>>(null, MessageId.AccessDenied);

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a => a.Product.ProductCode.Contains(query.SearchText)
                     || a.Product.Description.Contains(query.SearchText)
                     || a.PurchaseRequest.PRCode.Contains(query.SearchText));

                var totalCount = dbQuery.Count();
                dbQuery.ApplayPageing(query);


                var prModel = await dbQuery
                        .Select(b => new PRItemsForNewRFPDTO
                        {
                            PRItemId = b.Id,
                            PRCode = b.PurchaseRequest.PRCode,
                            DateEnd = b.DateEnd.ToUnixTimestamp(),
                            DateStart = b.DateStart.ToUnixTimestamp(),
                            ProductCode = b.Product.ProductCode,
                            ProductDescription = b.Product.Description,
                            ProductGroupName = b.Product.ProductGroup.Title,
                            ProductId = b.ProductId,
                            ProductTechnicalNumber = b.Product.TechnicalNumber,
                            ProductUnit = b.Product.Unit,
                            PurchaseRequestId = b.PurchaseRequestId,
                            Quntity = b.Quntity,
                            DocumentStatus =
                            !b.Product.DocumentProducts.Any(v => !v.Document.IsDeleted && v.Document.IsActive && v.Document.ContractCode == authenticate.ContractCode)
                            ? EngineeringDocumentStatus.No
                            : b.Product.DocumentProducts.Any(v => !v.Document.IsDeleted && v.Document.IsActive && v.Document.ContractCode == authenticate.ContractCode &&
                            (!v.Document.DocumentRevisions.Any(a => !a.IsDeleted) || v.Document.DocumentRevisions.Any(n => !n.IsDeleted && n.RevisionStatus < RevisionStatus.Confirmed)))
                            ? EngineeringDocumentStatus.completing
                            : EngineeringDocumentStatus.Completed,
                        }).ToListAsync();
                List<PRItemsForNewRFPDTO> prItems = new List<PRItemsForNewRFPDTO>();
                foreach (var item in prModel)
                {
                    if (!prItems.Any(a => a.ProductId == item.ProductId && a.PurchaseRequestId == item.PurchaseRequestId))
                    {
                        prItems.Add(new PRItemsForNewRFPDTO
                        {
                            PRItemId = item.PRItemId,
                            PRCode = item.PRCode,
                            DateEnd = item.DateEnd,
                            DateStart = item.DateStart,
                            ProductCode = item.ProductCode,
                            ProductDescription = item.ProductDescription,
                            ProductGroupName = item.ProductGroupName,
                            ProductId = item.ProductId,
                            ProductTechnicalNumber = item.ProductTechnicalNumber,
                            ProductUnit = item.ProductUnit,
                            PurchaseRequestId = item.PurchaseRequestId,
                            Quntity = prModel.Where(a => a.ProductId == item.ProductId && a.PurchaseRequestId == item.PurchaseRequestId).Sum(a => a.Quntity),
                            DocumentStatus = item.DocumentStatus,
                        });
                    }
                }
                return ServiceResultFactory.CreateSuccess(prItems).WithTotalCount(totalCount);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<PRItemsForNewRFPDTO>>(null, exception);
            }
        }

        #endregion

        private async Task<ServiceResult<bool>> UpdatePRAttachmentAsync(string contractCode, long mrpId, PurchaseConfirmationWorkFlow workFlow, List<AddPurchaseRequestAttachmentDto> postedAttachment)
        {
            try
            {
                workFlow.PurchaseConfirmationAttachments = new List<PAttachment>();
                if (postedAttachment != null)
                {
                    foreach (var item in postedAttachment.Where(a => a.Id != null))
                    {
                        var attachment = await _purchaseRequestAttachmentRepository.FirstOrDefaultAsync(a => a.Id == item.Id.Value);
                        if (attachment != null)
                            workFlow.PurchaseConfirmationAttachments.Add(attachment);
                    }
                    if (postedAttachment != null && postedAttachment.Any())
                    {
                        foreach (var item in postedAttachment.Where(a => a.Id == null))
                        {
                            var UploadedFile =
                                await _fileHelper.SaveDocumentFromTemp(item.FileSrc, ServiceSetting.UploadFilePathPRConfirm(contractCode, mrpId));
                            if (UploadedFile == null)
                                return ServiceResultFactory.CreateError<bool>(false, MessageId.UploudFailed);

                            _fileHelper.DeleteDocumentFromTemp(item.FileSrc);
                            workFlow.PurchaseConfirmationAttachments.Add(new PAttachment
                            {
                                IsDeleted = false,
                                FileName = item.FileName,
                                FileSrc = item.FileSrc,
                                FileType = UploadedFile.FileType,
                                FileSize = UploadedFile.FileSize
                            });
                        }

                    }
                }

                return ServiceResultFactory.CreateSuccess(true);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        private async Task<ServiceResult<List<PAttachment>>> AddPRAttachmentAsync(long purchaseRequestId, List<AddAttachmentDto> attachment)
        {
            var prAttachments = new List<PAttachment>();

            foreach (var item in attachment)
            {
                var UploadedFile =
                    await _fileHelper.SaveDocumentFromTemp(item.FileSrc, ServiceSetting.UploadFilePath.PR);
                if (UploadedFile == null)
                    return ServiceResultFactory.CreateError<List<PAttachment>>(null, MessageId.UploudFailed);

                _fileHelper.DeleteDocumentFromTemp(item.FileSrc);
                prAttachments.Add(new PAttachment
                {
                    IsDeleted = false,
                    PurchaseRequestId = purchaseRequestId,
                    FileName = item.FileName,
                    FileSrc = item.FileSrc,
                    FileType = UploadedFile.FileType,
                    FileSize = UploadedFile.FileSize
                });
            }
            return ServiceResultFactory.CreateSuccess(prAttachments);
        }

        private async Task<ServiceResult<bool>> EditPurchaseRequestItemAsync(long purchaseRequestId, long mrpId, List<PurchaseRequestItem> prItems, List<AddPurchaseRequestItemDto> postedPRItems, PurchaseRequest purchaseRequest)
        {
            try
            {

                var prItemProductIds = prItems.Select(a => a.ProductId).ToList();
                var postedPRItemProductIds = postedPRItems.Select(a => a.ProductId).ToList();

                var mrpItems = await _mrpPlanningRepository
                    .Include(a => a.PurchaseRequestItems)
                    .ThenInclude(a => a.RFPItems)
                    .Where(a => !a.IsDeleted && a.MrpId == mrpId && (prItemProductIds.Contains(a.ProductId) || postedPRItemProductIds.Contains(a.ProductId)))
                    .ToListAsync();

                if (mrpItems == null)
                    return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                // remove prItems
                var removePRItem = prItems.Where(a => !postedPRItemProductIds.Contains(a.ProductId)).ToList();
                RemovePRItem(removePRItem, mrpItems);

                // add new prItem
                var addnewPRItems = postedPRItems.Where(a => !prItemProductIds.Contains(a.ProductId))
                     .ToList();
                var addResult = AddPRItems(purchaseRequest, addnewPRItems, mrpItems);
                if (!addResult.Succeeded)
                    return ServiceResultFactory.CreateError(false, addResult.Messages.First().Message);

                // update prItem
                var updateModels = prItems;
                if (removePRItem != null && removePRItem.Count() > 0)
                {
                    var removePRItemProductIds = removePRItem.Select(a => a.ProductId).ToList();
                    updateModels = updateModels.Where(a => !addnewPRItems.Any(b => b.ProductId == a.ProductId) && !removePRItemProductIds.Contains(a.ProductId)).ToList();

                }
                var postedItems = postedPRItems.Where(a => updateModels.Any(b => b.ProductId == a.ProductId)).ToList();
                var updateResult = UpdatePRItems(updateModels, postedItems, mrpItems, purchaseRequestId);
                if (!updateResult.Succeeded)
                    return ServiceResultFactory.CreateError(false, updateResult.Messages.First().Message);

                return ServiceResultFactory.CreateSuccess(true);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }
        private async Task<ServiceResult<bool>> EditPurchaseRequestItemSysAdminAsync(long purchaseRequestId, long mrpId, List<PurchaseRequestItem> prItems, List<AddPurchaseRequestItemsSysAdminDto> postedPRItems, PurchaseRequest purchaseRequest)
        {
            try
            {

                var prItemProductIds = prItems.Select(a => a.ProductId).ToList();
                var postedPRItemProductIds = postedPRItems.Select(a => a.ProductId).ToList();

                var mrpItems = await _mrpPlanningRepository
                    .Include(a => a.PurchaseRequestItems)
                    .Where(a => !a.IsDeleted && a.MrpId == mrpId && (prItemProductIds.Contains(a.ProductId) || postedPRItemProductIds.Contains(a.ProductId)))
                    .ToListAsync();

                if (mrpItems == null)
                    return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                // remove prItems
                var removePRItem = prItems.Where(a => !postedPRItemProductIds.Contains(a.ProductId)).ToList();
                var removeResult = RemovePRItemBySysAdmin(removePRItem, mrpItems);
                if (!removeResult.Succeeded)
                    return ServiceResultFactory.CreateError(false, MessageId.DeleteEntityFailed);
                // add new prItem
                var addnewPRItems = postedPRItems.Where(a => !prItemProductIds.Contains(a.ProductId))
                     .ToList();
                var addResult = AddPRItemsSysAdmin(purchaseRequest, addnewPRItems, mrpItems);
                if (!addResult.Succeeded)
                    return ServiceResultFactory.CreateError(false, addResult.Messages.First().Message);

                // update prItem
                var updateModels = prItems;
                if (removePRItem != null && removePRItem.Count() > 0)
                {
                    var removePRItemProductIds = removePRItem.Select(a => a.ProductId).ToList();
                    updateModels = updateModels.Where(a => !addnewPRItems.Any(b => b.ProductId == a.ProductId) && !removePRItemProductIds.Contains(a.ProductId)).ToList();

                }
                var postedItems = postedPRItems.Where(a => updateModels.Any(b => b.ProductId == a.ProductId)).ToList();
                var updateResult = UpdatePRItemsSysAdmin(updateModels, postedItems, mrpItems, purchaseRequestId);
                if (!updateResult.Succeeded)
                    return ServiceResultFactory.CreateError(false, updateResult.Messages.First().Message);

                return ServiceResultFactory.CreateSuccess(true);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }
        private ServiceResult<bool> UpdatePRItems(List<PurchaseRequestItem> prItems, List<AddPurchaseRequestItemDto> postedPRItems, List<MrpItem> mrpItems, long purchaseRequestId)
        {
            decimal neededQuantity = 0;
            List<PurchaseRequestItem> newPrItems = new List<PurchaseRequestItem>();
            foreach (var item in postedPRItems)
            {
                neededQuantity = item.Quntity;
                var checkQueantity = prItems.Where(a => !a.IsDeleted && a.ProductId == item.ProductId).ToList();
                if (item.Quntity > checkQueantity.Sum(a => a.MrpItem.RemainedStock) + checkQueantity.Sum(a => a.Quntity))
                    return ServiceResultFactory.CreateError(false, MessageId.QuantityCantGreaterThanRemaind);
                foreach (var prItem in prItems.Where(a => !a.IsDeleted && a.ProductId == item.ProductId))
                {


                    if (neededQuantity >= prItem.Quntity )
                    {
                        if (neededQuantity <= prItem.Quntity + prItem.MrpItem.RemainedStock)
                        {
                            prItem.MrpItem.DoneStock -= prItem.Quntity;
                            prItem.MrpItem.RemainedStock += prItem.Quntity;
                            prItem.Quntity = neededQuantity;
                            prItem.MrpItem.DoneStock += prItem.Quntity;
                            prItem.MrpItem.RemainedStock -= prItem.Quntity;
                            if (prItem.RFPItems != null && prItem.RFPItems.Any(a => !a.IsDeleted && a.IsActive) && prItem.RFPItems.First() != null)
                            {
                                prItem.RFPItems.First(a => !a.IsDeleted && a.IsActive).RemainedStock += (prItem.Quntity - prItem.RFPItems.First(a => !a.IsDeleted && a.IsActive).Quantity);
                                prItem.RFPItems.First(a => !a.IsDeleted && a.IsActive).Quantity = prItem.Quntity;

                            }
                            neededQuantity -= prItem.Quntity;
                        }
                        else
                        {
                            neededQuantity -= prItem.Quntity;
                        }
                        


                    }
                    else if (neededQuantity < prItem.Quntity)
                    {
                        prItem.MrpItem.DoneStock -= prItem.Quntity;
                        prItem.MrpItem.RemainedStock += prItem.Quntity;
                        prItem.Quntity = neededQuantity;
                        prItem.MrpItem.DoneStock += prItem.Quntity;
                        prItem.MrpItem.RemainedStock -= prItem.Quntity;
                        if (prItem.RFPItems != null && prItem.RFPItems.Any(a => !a.IsDeleted && a.IsActive) && prItem.RFPItems.First() != null)
                        {
                            prItem.RFPItems.First(a => !a.IsDeleted && a.IsActive).RemainedStock += (prItem.Quntity - prItem.RFPItems.First(a => !a.IsDeleted && a.IsActive).Quantity);
                            prItem.RFPItems.First(a => !a.IsDeleted && a.IsActive).Quantity = prItem.Quntity;

                        }

                        if (prItem.Quntity == 0)
                        {
                            if (mrpItems.Any(a => !a.IsDeleted && a.Id == prItem.MrpItemId && !a.PurchaseRequestItems.Any(a => !a.IsDeleted)))
                                prItem.MrpItem.MrpItemStatus = MrpItemStatus.MRP;
                            prItem.IsDeleted = true;
                            if (prItem.RFPItems != null && prItem.RFPItems.Any(a => !a.IsDeleted && a.IsActive) && prItem.RFPItems.First() != null)
                                prItem.RFPItems.First(a => !a.IsDeleted && a.IsActive).IsDeleted = true;
                        }


                        neededQuantity = 0;
                    }

                }
                if (neededQuantity > 0)
                {
                    var hasRfp = prItems.Any(a => !a.IsDeleted && a.ProductId == item.ProductId && a.RFPItems.Any(a => !a.IsDeleted && a.IsActive));
                    var rfpId = hasRfp ? prItems.First(a => !a.IsDeleted && a.ProductId == item.ProductId && a.RFPItems.Any(a => !a.IsDeleted && a.IsActive)).RFPItems.First().RFPId : 0;
                    foreach (var mrp in mrpItems.Where(a => !a.IsDeleted && a.ProductId == item.ProductId && a.RemainedStock > 0))
                    {
                        if (mrp.RemainedStock >= neededQuantity)
                        {
                            newPrItems.Add(new PurchaseRequestItem
                            {
                                MrpItemId = mrp.Id,
                                Quntity = neededQuantity,
                                PRItemStatus = hasRfp ? PRItemStatus.InProgress : PRItemStatus.NotStart,
                                DateStart = mrp.DateStart,
                                DateEnd = mrp.DateEnd,
                                ProductId = mrp.ProductId,
                                PurchaseRequestId = purchaseRequestId,

                                RFPItems = hasRfp ? new List<RFPItems> { new RFPItems { DateEnd = mrp.DateEnd, DateStart = mrp.DateStart, IsActive = true, ProductId = mrp.ProductId, Quantity = neededQuantity, RemainedStock = neededQuantity, RFPId = rfpId } } : null
                            });
                            mrp.RemainedStock -= neededQuantity;
                            mrp.DoneStock += neededQuantity;
                            mrp.MrpItemStatus = (mrp.MrpItemStatus < MrpItemStatus.PR) ? MrpItemStatus.PR : mrp.MrpItemStatus;
                            neededQuantity = 0;
                        }
                        else if (neededQuantity > 0 && mrp.RemainedStock < neededQuantity)
                        {
                            newPrItems.Add(new PurchaseRequestItem
                            {
                                MrpItemId = mrp.Id,
                                Quntity = mrp.RemainedStock,
                                PRItemStatus = hasRfp ? PRItemStatus.InProgress : PRItemStatus.NotStart,
                                DateStart = mrp.DateStart,
                                DateEnd = mrp.DateEnd,
                                ProductId = mrp.ProductId,
                                PurchaseRequestId = purchaseRequestId,
                                RFPItems = hasRfp ? new List<RFPItems> { new RFPItems { DateEnd = mrp.DateEnd, DateStart = mrp.DateStart, IsActive = true, ProductId = mrp.ProductId, Quantity = neededQuantity, RemainedStock = neededQuantity, RFPId = rfpId } } : null
                            });
                            neededQuantity -= mrp.RemainedStock;
                            mrp.DoneStock += mrp.RemainedStock;
                            mrp.MrpItemStatus = (mrp.MrpItemStatus < MrpItemStatus.PR) ? MrpItemStatus.PR : mrp.MrpItemStatus;
                            mrp.RemainedStock = 0;

                        }
                        else
                            break;
                    }
                }
            }

            if (newPrItems != null && newPrItems.Any())
                _purchaseRequestItemRepository.AddRange(newPrItems);
            return ServiceResultFactory.CreateSuccess(true);

        }
        private ServiceResult<bool> UpdatePRItemsSysAdmin(List<PurchaseRequestItem> prItems, List<AddPurchaseRequestItemsSysAdminDto> postedPRItems, List<MrpItem> mrpItems, long purchaseRequestId)
        {
            decimal neededQuantity = 0;
            decimal rfpQuentity = 0;
            List<PurchaseRequestItem> newPrItems = new List<PurchaseRequestItem>();
            foreach (var item in postedPRItems)
            {
                rfpQuentity = 0;
                neededQuantity = item.RequiredQuantity;
                var checkQueantity = prItems.Where(a => !a.IsDeleted && a.ProductId == item.ProductId).ToList();
                if (item.RequiredQuantity > checkQueantity.Sum(a => a.MrpItem.RemainedStock) + checkQueantity.Sum(a => a.Quntity))
                    return ServiceResultFactory.CreateError(false, MessageId.QuantityCantGreaterThanRemaind);
                foreach (var prItem in prItems.Where(a => !a.IsDeleted && a.ProductId == item.ProductId))
                {
                    if(prItem.RFPItems.Any(a => !a.IsDeleted && !a.RFP.IsDeleted && a.IsActive))
                        rfpQuentity += prItem.RFPItems.Where(a => !a.IsDeleted && !a.RFP.IsDeleted && a.IsActive).Sum(a => a.Quantity-a.RemainedStock);
                    if (item.RequiredQuantity < rfpQuentity)
                        return ServiceResultFactory.CreateError(false, MessageId.QuantityCantLessThanRFPDone);
                }
                foreach (var prItem in prItems.Where(a => !a.IsDeleted && a.ProductId == item.ProductId))
                {


                    if (neededQuantity >= prItem.Quntity)
                    {
                        if (neededQuantity <= prItem.Quntity + prItem.MrpItem.RemainedStock)
                        {
                            prItem.MrpItem.DoneStock -= prItem.Quntity;
                            prItem.MrpItem.RemainedStock += prItem.Quntity;
                            prItem.Quntity = neededQuantity;
                            prItem.MrpItem.DoneStock += prItem.Quntity;
                            prItem.MrpItem.RemainedStock -= prItem.Quntity;
                            if (prItem.RFPItems != null && prItem.RFPItems.Any(a => !a.IsDeleted && a.IsActive) && prItem.RFPItems.First() != null)
                            {
                                prItem.RFPItems.First(a => !a.IsDeleted && a.IsActive).RemainedStock +=(prItem.Quntity-prItem.RFPItems.First(a => !a.IsDeleted && a.IsActive).Quantity);
                                prItem.RFPItems.First(a => !a.IsDeleted && a.IsActive).Quantity = prItem.Quntity;
                                
                            }
                            neededQuantity -= prItem.Quntity;
                        }
                        else
                        {
                            neededQuantity -= prItem.Quntity;
                        }
                        

                    }
                    else if (neededQuantity < prItem.Quntity)
                    {
                        prItem.MrpItem.DoneStock -= prItem.Quntity;
                        prItem.MrpItem.RemainedStock += prItem.Quntity;
                        prItem.Quntity = neededQuantity;
                        prItem.MrpItem.DoneStock += prItem.Quntity;
                        prItem.MrpItem.RemainedStock -= prItem.Quntity;
                        if (prItem.RFPItems != null && prItem.RFPItems.Any(a => !a.IsDeleted && a.IsActive) && prItem.RFPItems.First() != null)
                        {
                            prItem.RFPItems.First(a => !a.IsDeleted && a.IsActive).RemainedStock += (prItem.Quntity - prItem.RFPItems.First(a => !a.IsDeleted && a.IsActive).Quantity);
                            prItem.RFPItems.First(a => !a.IsDeleted && a.IsActive).Quantity = prItem.Quntity;

                        }
                        if (prItem.Quntity == 0)
                        {
                            if (mrpItems.Any(a => !a.IsDeleted && a.Id == prItem.MrpItemId && !a.PurchaseRequestItems.Any(a => !a.IsDeleted)))
                                prItem.MrpItem.MrpItemStatus = MrpItemStatus.MRP;
                            prItem.IsDeleted = true;
                            if (prItem.RFPItems != null && prItem.RFPItems.Any(a => !a.IsDeleted && a.IsActive) && prItem.RFPItems.First() != null)
                                prItem.RFPItems.First(a => !a.IsDeleted && a.IsActive).IsDeleted = true;
                        }


                        neededQuantity = 0;
                    }

                }
                if (neededQuantity > 0)
                {
                    var hasRfp = prItems.Any(a => !a.IsDeleted && a.ProductId == item.ProductId && a.RFPItems.Any(a => !a.IsDeleted && a.IsActive));
                    var rfpId = hasRfp ? prItems.First(a => !a.IsDeleted && a.ProductId == item.ProductId && a.RFPItems.Any(a => !a.IsDeleted && a.IsActive)).RFPItems.First().RFPId : 0;
                    foreach (var mrp in mrpItems.Where(a => !a.IsDeleted && a.ProductId == item.ProductId && a.RemainedStock > 0))
                    {
                        if (mrp.RemainedStock >= neededQuantity)
                        {
                            newPrItems.Add(new PurchaseRequestItem
                            {
                                MrpItemId = mrp.Id,
                                Quntity = neededQuantity,
                                PRItemStatus = hasRfp ? PRItemStatus.InProgress : PRItemStatus.NotStart,
                                DateStart = mrp.DateStart,
                                DateEnd = mrp.DateEnd,
                                ProductId = mrp.ProductId,
                                PurchaseRequestId = purchaseRequestId,

                                RFPItems = hasRfp ? new List<RFPItems> { new RFPItems { DateEnd = mrp.DateEnd, DateStart = mrp.DateStart, IsActive = true, ProductId = mrp.ProductId, Quantity = neededQuantity, RemainedStock = neededQuantity, RFPId = rfpId } } : null
                            });
                            mrp.RemainedStock -= neededQuantity;
                            mrp.DoneStock += neededQuantity;
                            mrp.MrpItemStatus = (mrp.MrpItemStatus < MrpItemStatus.PR) ? MrpItemStatus.PR : mrp.MrpItemStatus;
                            neededQuantity = 0;
                        }
                        else if (neededQuantity > 0 && mrp.RemainedStock < neededQuantity)
                        {
                            newPrItems.Add(new PurchaseRequestItem
                            {
                                MrpItemId = mrp.Id,
                                Quntity = mrp.RemainedStock,
                                PRItemStatus = hasRfp ? PRItemStatus.InProgress : PRItemStatus.NotStart,
                                DateStart = mrp.DateStart,
                                DateEnd = mrp.DateEnd,
                                ProductId = mrp.ProductId,
                                PurchaseRequestId = purchaseRequestId,
                                RFPItems = hasRfp ? new List<RFPItems> { new RFPItems { DateEnd = mrp.DateEnd, DateStart = mrp.DateStart, IsActive = true, ProductId = mrp.ProductId, Quantity = neededQuantity, RemainedStock = neededQuantity, RFPId = rfpId } } : null
                            });
                            neededQuantity -= mrp.RemainedStock;
                            mrp.DoneStock += mrp.RemainedStock;
                            mrp.MrpItemStatus = (mrp.MrpItemStatus < MrpItemStatus.PR) ? MrpItemStatus.PR : mrp.MrpItemStatus;
                            mrp.RemainedStock = 0;

                        }
                        else
                            break;
                    }
                }
            }

            if (newPrItems != null && newPrItems.Any())
                _purchaseRequestItemRepository.AddRange(newPrItems);
            return ServiceResultFactory.CreateSuccess(true);

        }
        private ServiceResult<bool> AddPRItems(PurchaseRequest purchaseRequest, List<AddPurchaseRequestItemDto> postedPRItems, List<MrpItem> mrpItems)
        {
            try
            {
                decimal neededQuantity = 0;
                foreach (var item in postedPRItems)
                {
                    var currentMrpItems = mrpItems.Where(a => !a.IsDeleted && a.ProductId == item.ProductId).ToList();
                    if (currentMrpItems.Sum(a => a.RemainedStock) < item.Quntity)
                        return ServiceResultFactory.CreateError(false, MessageId.QuantityCantGreaterThanRemaind);
                    neededQuantity = item.Quntity;
                    decimal purchaseQuantity = 0;
                    foreach (var mrpItem in currentMrpItems)
                    {

                        if (mrpItem.RemainedStock >= neededQuantity)
                        {
                            purchaseQuantity = neededQuantity;
                            mrpItem.RemainedStock -= neededQuantity;
                            mrpItem.DoneStock += neededQuantity;
                            neededQuantity = 0;
                        }
                        else
                        {
                            purchaseQuantity = mrpItem.RemainedStock;
                            neededQuantity -= mrpItem.RemainedStock;
                            mrpItem.DoneStock += mrpItem.RemainedStock;
                            mrpItem.RemainedStock = 0;
                        }


                        purchaseRequest.PurchaseRequestItems.Add(new PurchaseRequestItem
                        {
                            DateEnd = mrpItem.DateEnd.Date,
                            DateStart = mrpItem.DateStart.Date,
                            ProductId = item.ProductId,
                            Quntity = purchaseQuantity,
                            PRItemStatus = PRItemStatus.NotStart,
                            MrpItemId = mrpItem.Id
                        });

                        if (mrpItem.MrpItemStatus < MrpItemStatus.PR)
                        {
                            mrpItem.MrpItemStatus = MrpItemStatus.PR;
                        }
                        if (neededQuantity <= 0)
                            break;
                    }

                }
                return ServiceResultFactory.CreateSuccess(true);
            }
            catch
            {
                return ServiceResultFactory.CreateError(false, MessageId.OperationFailed);
            }
        }
        private ServiceResult<bool> AddPRItemsSysAdmin(PurchaseRequest purchaseRequest, List<AddPurchaseRequestItemsSysAdminDto> postedPRItems, List<MrpItem> mrpItems)
        {
            try
            {
                decimal neededQuantity = 0;
                foreach (var item in postedPRItems)
                {
                    var currentMrpItems = mrpItems.Where(a => !a.IsDeleted && a.ProductId == item.ProductId).ToList();
                    if (currentMrpItems.Sum(a => a.RemainedStock) < item.RequiredQuantity)
                        return ServiceResultFactory.CreateError(false, MessageId.QuantityCantGreaterThanRemaind);
                    neededQuantity = item.RequiredQuantity;
                    decimal purchaseQuantity = 0;
                    foreach (var mrpItem in currentMrpItems)
                    {

                        if (mrpItem.RemainedStock >= neededQuantity)
                        {
                            purchaseQuantity = neededQuantity;
                            mrpItem.RemainedStock -= neededQuantity;
                            mrpItem.DoneStock += neededQuantity;
                            neededQuantity = 0;
                        }
                        else
                        {
                            purchaseQuantity = mrpItem.RemainedStock;
                            neededQuantity -= mrpItem.RemainedStock;
                            mrpItem.DoneStock += mrpItem.RemainedStock;
                            mrpItem.RemainedStock = 0;
                        }


                        purchaseRequest.PurchaseRequestItems.Add(new PurchaseRequestItem
                        {
                            DateEnd = mrpItem.DateEnd.Date,
                            DateStart = mrpItem.DateStart.Date,
                            ProductId = item.ProductId,
                            Quntity = purchaseQuantity,
                            PRItemStatus = PRItemStatus.NotStart,
                            MrpItemId = mrpItem.Id
                        });

                        if (mrpItem.MrpItemStatus < MrpItemStatus.PR)
                        {
                            mrpItem.MrpItemStatus = MrpItemStatus.PR;
                        }
                        if (neededQuantity <= 0)
                            break;
                    }

                }
                return ServiceResultFactory.CreateSuccess(true);
            }
            catch
            {
                return ServiceResultFactory.CreateError(false, MessageId.OperationFailed);
            }
        }
        private void RemovePRItem(List<PurchaseRequestItem> prItems, List<MrpItem> mrpItems)
        {

            foreach (var item in prItems)
            {

                item.MrpItem.DoneStock -= item.Quntity;
                item.MrpItem.RemainedStock += item.Quntity;
                if (mrpItems.Any(a => !a.IsDeleted && a.Id == item.MrpItemId && !a.PurchaseRequestItems.Any(a => !a.IsDeleted)))
                    item.MrpItem.MrpItemStatus = MrpItemStatus.MRP;
                item.IsDeleted = true;
            }
        }
        private ServiceResult<bool> RemovePRItemBySysAdmin(List<PurchaseRequestItem> prItems, List<MrpItem> mrpItems)
        {
            try
            {
                foreach (var item in prItems)
                {
                    if (item.RFPItems.Any(a => !a.IsDeleted))
                        return ServiceResultFactory.CreateError(false, MessageId.DeleteEntityFailed);
                    if (mrpItems.Any(a => !a.IsDeleted && a.Id == item.MrpItemId && !a.PurchaseRequestItems.Any(a => !a.IsDeleted)))
                        item.MrpItem.MrpItemStatus = MrpItemStatus.MRP;
                    item.MrpItem.DoneStock -= item.Quntity;
                    item.MrpItem.RemainedStock += item.Quntity;
                    item.IsDeleted = true;
                }
                return ServiceResultFactory.CreateSuccess(true);
            }
            catch (Exception ex)
            {
                return ServiceResultFactory.CreateException(false, ex);
            }
        }

        //public async Task<ServiceResult<bool>> DeletePurchaseRequestAsync(AuthenticateDto authenticate,
        //    long purchaseRequestId)
        //{
        //    try
        //    {
        //        var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
        //        if (!permission.HasPermission)
        //            return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

        //        var dbQuery = _purchaseRequestRepository
        //           .Where(c => !c.IsDeleted && c.Id == purchaseRequestId && c.ContractCode == authenticate.ContractCode);

        //        if (dbQuery.Count() == 0)
        //            return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

        //        if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => a.PurchaseRequestItems.Any(c => !c.IsDeleted && permission.ProductGroupIds.Contains(c.Product.ProductGroupId))))
        //            return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);


        //        var purchaseRequestModel = await dbQuery
        //            .Include(a => a.PurchaseRequestItems)
        //            .FirstOrDefaultAsync();

        //        if (purchaseRequestModel == null)
        //            return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

        //        if (purchaseRequestModel.PRStatus > PRStatus.Register)
        //            return ServiceResultFactory.CreateError(false, MessageId.DeleteDontAllowedAfterConfirm);

        //        var removePrItemProductIds = purchaseRequestModel.PurchaseRequestItems
        //            .Where(a => !a.IsDeleted)
        //            .Select(c => c.ProductId)
        //            .ToList();

        //        var mrpItems = await _mrpPlanningRepository
        //        .Where(a => !a.IsDeleted && removePrItemProductIds.Contains(a.ProductId))
        //        .ToListAsync();
        //        if (mrpItems == null || mrpItems.Count() != removePrItemProductIds.Count())
        //            return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

        //        UpdateMrpItemAfterRejectPR(purchaseRequestModel.PurchaseRequestItems.Where(a => !a.IsDeleted).ToList(), mrpItems);

        //        purchaseRequestModel.IsDeleted = true;
        //        foreach (var item in purchaseRequestModel.PurchaseRequestItems)
        //        {
        //            item.IsDeleted = true;
        //        }
        //        if (await _unitOfWork.SaveChangesAsync() > 0)
        //        {
        //            var res = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
        //            {
        //                ContractCode = purchaseRequestModel.ContractCode,
        //                FormCode = purchaseRequestModel.PRCode,
        //                KeyValue = purchaseRequestModel.Id.ToString(),
        //                RootKeyValue = purchaseRequestModel.Id.ToString(),
        //                NotifEvent = NotifEvent.DeletePurchaseRequest,
        //                PerformerUserId = authenticate.UserId,
        //                PerformerUserFullName = authenticate.UserFullName
        //            }, null);
        //            return ServiceResultFactory.CreateSuccess(true);

        //        }
        //        return ServiceResultFactory.CreateError(false, MessageId.DeleteEntityFailed);
        //    }
        //    catch (Exception exception)
        //    {
        //        return ServiceResultFactory.CreateException(false, exception);
        //    }
        //}

        public async Task<ServiceResult<bool>> SetStatusPurchaseRequestAsync(AuthenticateDto authenticate,
            long purchaseRequestId, PRStatus status)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery = _purchaseRequestRepository
                   .Where(c => !c.IsDeleted && c.Id == purchaseRequestId && c.ContractCode == authenticate.ContractCode);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.ProductGroupId)))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var purchaseRequest = await dbQuery
                    .FirstOrDefaultAsync();

                if (purchaseRequest == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);


                if (purchaseRequest.PRStatus >= PRStatus.Confirm && status < PRStatus.Confirm)
                {
                    if (await _purchaseRequestItemRepository.AnyAsync(x => x.PurchaseRequestId == purchaseRequestId))
                        return ServiceResultFactory.CreateError(false, MessageId.RequestProposalTypeChangeNotExist);
                }

                purchaseRequest.PRStatus = status;

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    if (purchaseRequest.PRStatus == PRStatus.Reject)
                    {
                        //var res = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                        //{
                        //    BaseContractCode = purchaseRequest.ContractCode,
                        //    SCMEntityEnum = SCMEntityEnum.PurchaseRequests,
                        //    RootKeyValue = purchaseRequest.Id.ToString(),
                        //    NewValues = purchaseRequest,
                        //    FormCode = purchaseRequest.PRCode,
                        //    KeyValue = purchaseRequest.Id.ToString(),
                        //    NotifEvent = NotifEvent.RejectPurchaseRequest,
                        //    PerformerUserId = authenticate.UserId,
                        //    PerformerUserFullName = authenticate.UserFullName
                        //}, false);
                    }
                    return ServiceResultFactory.CreateSuccess(true);
                }
                return ServiceResultFactory.CreateError(false, MessageId.EditEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<DownloadFileDto> DownloadPRAttachmentAsync(AuthenticateDto authenticate, long purchaseRequestId, string fileSrc)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return null;

                var entity = await _purchaseRequestRepository
                   .Where(a => !a.IsDeleted && a.Id == purchaseRequestId && a.PRAttachments.Any(c => !c.IsDeleted && c.FileSrc == fileSrc))
                   .Select(c => new
                   {
                       ContractCode = c.ContractCode,
                       ProductGroupId = c.ProductGroupId
                   }).FirstOrDefaultAsync();

                if (entity == null)
                    return null;

                if (permission.ProductGroupIds.Any() && !permission.ProductGroupIds.Contains(entity.ProductGroupId))
                    return null;

                var streamResult = await _fileHelper.DownloadDocument(fileSrc, ServiceSetting.FileSection.PR);
                if (streamResult == null)
                    return null;

                return streamResult;
            }
            catch (Exception exception)
            {
                return null;
            }
        }

        public async Task<DownloadFileDto> DownloadPRWorkFlowAttachmentAsync(AuthenticateDto authenticate, long purchaseRequestId, string fileSrc)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return null;

                var entity = await _purchaseRequestRepository
                    .Include(a => a.Mrp)
                    .Include(a => a.PRAttachments)
                   .Where(a => !a.IsDeleted && a.Id == purchaseRequestId && a.PurchaseConfirmationWorkFlows.Any(c => !c.IsDeleted && c.PurchaseConfirmationAttachments.Any(d => d.FileSrc == fileSrc)))
                   .FirstOrDefaultAsync();

                if (entity == null)
                    return null;

                if (permission.ProductGroupIds.Any() && !permission.ProductGroupIds.Contains(entity.ProductGroupId))
                    return null;

                var streamResult = await _fileHelper.DownloadFileDriveDocument(ServiceSetting.UploadFilePathPRConfirm(authenticate.ContractCode, entity.MrpId.Value), fileSrc);
                if (streamResult == null)
                    return null;
                var file = await _purchaseRequestAttachmentRepository.FirstOrDefaultAsync(a => a.FileSrc == fileSrc);
                if (file != null)
                    streamResult.FileName = file.FileName;
                else
                    streamResult.FileName = fileSrc;
                return streamResult;
            }
            catch (Exception exception)
            {
                return null;
            }
        }
        public async Task<ServiceResult<List<UserMentionDto>>> GetConfirmationUserListAsync(AuthenticateDto authenticate, long mrpId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<UserMentionDto>>(null, MessageId.AccessDenied);

                var dbQuery = _mrpRepository
                    .Where(a => !a.IsDeleted && a.Id == mrpId && a.ContractCode == authenticate.ContractCode);

                if (await dbQuery.CountAsync() == 0)
                    return ServiceResultFactory.CreateError<List<UserMentionDto>>(null, MessageId.EntityDoesNotExist);

                var productGroup = await dbQuery.Select(a => a.ProductGroupId).FirstOrDefaultAsync();

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.ProductGroupId)))
                    return ServiceResultFactory.CreateError<List<UserMentionDto>>(null, MessageId.AccessDenied);

                var roles = new List<string> { SCMRole.PurchaseRequestConfirm };
                var list = await _authenticationService.GetAllUserHasAccessPurchaseAsync(authenticate.ContractCode, roles, productGroup);

                foreach (var item in list)
                {
                    item.Image = item.Image != null ?
                        _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + item.Image : "";
                }

                return ServiceResultFactory.CreateSuccess(list);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<UserMentionDto>>(null, exception);
            }
        }
        private PurchaseRequest AddPurchaseRequestItemByMrpItem(PurchaseRequest prModel, List<AddPurchaseRequestItemsDto> addPRItems, List<MrpItem> mrpItems)
        {
            prModel.PurchaseRequestItems = new List<PurchaseRequestItem>();
            decimal neededQuantity = 0;
            foreach (var item in addPRItems)
            {
                var currentMrpItems = mrpItems.Where(a => a.ProductId == item.ProductId).ToList();
                neededQuantity = item.Quntity;
                decimal purchaseQuantity = 0;
                foreach (var mrpItem in currentMrpItems)
                {

                    if (mrpItem.RemainedStock >= neededQuantity)
                    {
                        purchaseQuantity = neededQuantity;
                        mrpItem.RemainedStock -= neededQuantity;
                        mrpItem.DoneStock += neededQuantity;
                        neededQuantity = 0;
                    }
                    else
                    {
                        purchaseQuantity = mrpItem.RemainedStock;
                        neededQuantity -= mrpItem.RemainedStock;
                        mrpItem.DoneStock += mrpItem.RemainedStock;
                        mrpItem.RemainedStock = 0;
                    }


                    prModel.PurchaseRequestItems.Add(new PurchaseRequestItem
                    {
                        DateEnd = mrpItem.DateEnd.Date,
                        DateStart = mrpItem.DateStart.Date,
                        ProductId = item.ProductId,
                        Quntity = purchaseQuantity,
                        PRItemStatus = PRItemStatus.NotStart,
                        MrpItemId = mrpItem.Id
                    });

                    if (mrpItem.MrpItemStatus < MrpItemStatus.PR)
                    {
                        mrpItem.MrpItemStatus = MrpItemStatus.PR;
                    }
                    if (neededQuantity <= 0)
                        break;
                }

            }
            return prModel;
        }


        public async Task<ServiceResult<ListPendingForConfirmPurchaseRequestDto>> SetUserConfirmOwnPurchaseRequestTaskAsync(AuthenticateDto authenticate, long purchaseRequsetId, AddPurchaseRequestConfirmationAnswerDto model)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<ListPendingForConfirmPurchaseRequestDto>(null, MessageId.AccessDenied);

                var dbQuery = _purchaseRequestConfirmWorkFlowRepository
                     .Where(a => !a.IsDeleted &&
                    a.PurchaseRequestId == purchaseRequsetId &&
                    a.Status == ConfirmationWorkFlowStatus.Pending &&
                    a.PurchaseRequest.ContractCode == authenticate.ContractCode &&
                    a.PurchaseRequest.PRStatus == PRStatus.Register)
                     .Include(a => a.PurchaseConfirmationAttachments)
                     .Include(a => a.PurchaseConfirmationWorkFlowUsers)
                     .ThenInclude(c => c.User)
                     .Include(a => a.PurchaseRequest)
                     .ThenInclude(a => a.PurchaseRequestItems)
                     .ThenInclude(a => a.MrpItem)
                     .AsQueryable();


                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<ListPendingForConfirmPurchaseRequestDto>(null, MessageId.EntityDoesNotExist);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.PurchaseRequest.ProductGroupId)))
                    return ServiceResultFactory.CreateError<ListPendingForConfirmPurchaseRequestDto>(null, MessageId.AccessDenied);

                var confirmationModel = await dbQuery.FirstOrDefaultAsync();

                if (confirmationModel == null)
                    return ServiceResultFactory.CreateError<ListPendingForConfirmPurchaseRequestDto>(null, MessageId.EntityDoesNotExist);

                if (confirmationModel.PurchaseConfirmationWorkFlowUsers == null && !confirmationModel.PurchaseConfirmationWorkFlowUsers.Any())
                    return ServiceResultFactory.CreateError<ListPendingForConfirmPurchaseRequestDto>(null, MessageId.DataInconsistency);

                if (!confirmationModel.PurchaseConfirmationWorkFlowUsers.Any(c => c.UserId == authenticate.UserId && c.IsBallInCourt))
                    return ServiceResultFactory.CreateError<ListPendingForConfirmPurchaseRequestDto>(null, MessageId.AccessDenied);


                var userBallInCourtModel = confirmationModel.PurchaseConfirmationWorkFlowUsers.FirstOrDefault(a => a.IsBallInCourt && a.UserId == authenticate.UserId);
                userBallInCourtModel.DateEnd = DateTime.UtcNow;
                if (model.IsAccept)
                {
                    userBallInCourtModel.IsBallInCourt = false;
                    userBallInCourtModel.IsAccept = true;
                    userBallInCourtModel.Note = model.Note;
                    if (!confirmationModel.PurchaseConfirmationWorkFlowUsers.Any(a => a.IsAccept == false))
                    {
                        confirmationModel.Status = ConfirmationWorkFlowStatus.Confirm;
                        confirmationModel.PurchaseRequest.PRStatus = PRStatus.Confirm;

                        //foreach (var item in confirmationModel.PurchaseConfirmationAttachments)
                        //{
                        //    if (item.IsDeleted)
                        //        continue;
                        //    _purchaseRequestAttachmentRepository.Add(new PAttachment
                        //    {
                        //        PurchaseRequestId = confirmationModel.PurchaseRequestId,
                        //        FileName = item.FileName,
                        //        FileSize = item.FileSize,
                        //        FileSrc = item.FileSrc,
                        //        FileType = item.FileType,
                        //    });
                        //}

                    }
                    else
                    {
                        var nextBallInCourtModel = confirmationModel.PurchaseConfirmationWorkFlowUsers.Where(a => !a.IsAccept)
                             .OrderBy(a => a.OrderNumber)
                             .FirstOrDefault();

                        nextBallInCourtModel.IsBallInCourt = true;
                        userBallInCourtModel.DateStart = DateTime.UtcNow;
                    }
                }
                else
                {
                    userBallInCourtModel.IsAccept = false;
                    userBallInCourtModel.Note = model.Note;
                    confirmationModel.Status = ConfirmationWorkFlowStatus.Reject;

                    confirmationModel.PurchaseRequest.PRStatus = PRStatus.Reject;
                    foreach (var item in confirmationModel.PurchaseRequest.PurchaseRequestItems)
                    {
                        item.MrpItem.DoneStock -= item.Quntity;
                        item.MrpItem.RemainedStock += item.Quntity;
                    }
                }

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    await _scmLogAndNotificationService.SetDonedNotificationAsync(authenticate.UserId, confirmationModel.PurchaseRequest.ContractCode, confirmationModel.PurchaseRequest.Id.ToString(), NotifEvent.ConfirmPurchaseRequest);
                    var productGourp = await _productGroupRepository.FirstOrDefaultAsync(a => a.Id == confirmationModel.PurchaseRequest.ProductGroupId);
                    int? userId = null;
                    if (model.IsAccept && confirmationModel.PurchaseRequest.PRStatus != PRStatus.Confirm)
                    {
                        userId = confirmationModel.PurchaseConfirmationWorkFlowUsers.First(a => a.IsBallInCourt).UserId;
                    }
                    if (confirmationModel.PurchaseRequest.PRStatus != PRStatus.Confirm)
                    {
                        await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                        {
                            ContractCode = confirmationModel.PurchaseRequest.ContractCode,
                            FormCode = confirmationModel.PurchaseRequest.PRCode,
                            KeyValue = confirmationModel.PurchaseRequest.Id.ToString(),
                            NotifEvent = (model.IsAccept) ? NotifEvent.ConfirmPurchaseRequest : NotifEvent.RejectPurchaseRequest,
                            ProductGroupId = confirmationModel.PurchaseRequest.ProductGroupId,
                            RootKeyValue = confirmationModel.PurchaseRequest.Id.ToString(),
                            Message = (productGourp != null) ? productGourp.Title : "",
                            PerformerUserId = authenticate.UserId,
                            PerformerUserFullName = authenticate.UserFullName,

                        },
                     confirmationModel.PurchaseRequest.ProductGroupId
                    , NotifEvent.ConfirmPurchaseRequest, userId);
                    }
                    if (confirmationModel.PurchaseRequest.PRStatus == PRStatus.Confirm)
                    {
                        await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                        {
                            ContractCode = confirmationModel.PurchaseRequest.ContractCode,
                            FormCode = confirmationModel.PurchaseRequest.PRCode,
                            KeyValue = confirmationModel.PurchaseRequest.Id.ToString(),
                            NotifEvent = NotifEvent.ConfirmPurchaseRequest,
                            ProductGroupId = confirmationModel.PurchaseRequest.ProductGroupId,
                            RootKeyValue = confirmationModel.PurchaseRequest.Id.ToString(),
                            Message = (productGourp != null) ? productGourp.Title : "",
                            PerformerUserId = authenticate.UserId,
                            PerformerUserFullName = authenticate.UserFullName,

                        },
                    confirmationModel.PurchaseRequest.ProductGroupId
                    , ConfirmSendNotification());
                    }
                    //await SendingLogAndNotificationTaskOnUserConfirmREvisionAsync(authenticate, confirmationModel, userBallInCourtModel);
                    //if (confirmationModel.Status == ConfirmationWorkFlowStatus.Reject)
                    //{
                    //    BackgroundJob.Enqueue(() => SendEmailForRejectDocumentRevisionAsync(authenticate, confirmationModel, model.Note,purchaseRequsetId ));
                    //}
                    var result = await GetPendingForConfirmPurchaseRequestByIdAsync(authenticate, purchaseRequsetId);
                    if (!result.Succeeded)
                        return ServiceResultFactory.CreateError<ListPendingForConfirmPurchaseRequestDto>(null, MessageId.OperationFailed);
                    else
                        return ServiceResultFactory.CreateSuccess(result.Result);
                }
                return ServiceResultFactory.CreateError<ListPendingForConfirmPurchaseRequestDto>(null, MessageId.SaveFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<ListPendingForConfirmPurchaseRequestDto>(null, exception);
            }
        }

        //public async Task<bool> SendEmailForRejectPurchaseRequestAsync(AuthenticateDto authenticate, PurchaseConfirmationWorkFlow confirmationModel, string reason)
        //{
        //    string url = $"/dashboard/documents/RevDetails/{confirmationModel.PurchaseRequestId}?team={authenticate.ContractCode}&wfid={confirmationModel.PurchaseConfirmationWorkFlowId}";

        //    url = _appSettings.ClientHost + url;
        //    var user = await _purchaseRequestRepository.Include(d => d.AdderUser).SingleOrDefaultAsync(d => d.Id == confirmationModel.PurchaseRequestId);
        //    var dat = DateTime.UtcNow.ToPersianDate();
        //    var emailDTO = new RejectRevisionEmailDTO(user.PRCode.ToString(), user.PRCode.DocNumber, authenticate.ContractCode, authenticate.UserName, reason, user.Document.DocTitle, url, user.AdderUser.FullName, _appSettings.CompanyName);

        //    var emalRequest = new EmailRequest
        //    {
        //        Attachment = null,
        //        Subject = $"Reject | {user.Document.DocNumber}",
        //        Body = await _viewRenderService.RenderToStringAsync("_RejectRevisionEmail", emailDTO),
        //    };

        //    emalRequest.To = new List<string> { user.AdderUser.Email };
        //    await _appEmailService.SendAsync(emalRequest);
        //    return true;

        //}

        public async Task<ServiceResult<PurchaseRequestConfirmationWorkflowDto>> GetPendingConfirmPurchaseByPurchaseRequestIdAsync(AuthenticateDto authenticate, long purchaseRequestId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<PurchaseRequestConfirmationWorkflowDto>(null, MessageId.AccessDenied);

                var dbQuery = _purchaseRequestConfirmWorkFlowRepository
                     .AsNoTracking()
                     .Where(a => !a.IsDeleted &&
                   a.PurchaseRequestId == purchaseRequestId &&
                    a.Status == ConfirmationWorkFlowStatus.Pending &&
                     a.PurchaseRequest.ContractCode == authenticate.ContractCode &&
                     a.PurchaseRequest.PRStatus == PRStatus.Register);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<PurchaseRequestConfirmationWorkflowDto>(null, MessageId.EntityDoesNotExist);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.PurchaseRequest.ProductGroupId)))
                    return ServiceResultFactory.CreateError<PurchaseRequestConfirmationWorkflowDto>(null, MessageId.AccessDenied);

                var result = await dbQuery
                        .Select(x => new PurchaseRequestConfirmationWorkflowDto
                        {
                            ConfirmNote = x.ConfirmNote,
                            PurchaseRequestConfirmUserAudit = x.AdderUserId != null ? new UserAuditLogDto
                            {
                                CreateDate = x.CreatedDate.ToUnixTimestamp(),
                                AdderUserName = x.AdderUser.FullName,
                                AdderUserImage = x.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + x.AdderUser.Image : ""
                            } : null,
                            Attachments = x.PurchaseConfirmationAttachments.Where(m => !m.IsDeleted)
                            .Select(c => new BasePRAttachmentDto
                            {
                                FileName = c.FileName,
                                FileSize = c.FileSize,
                                FileSrc = c.FileSrc,
                                FileType = c.FileType,
                                PurchaseRequestId = c.PurchaseRequestId.Value
                            }).ToList(),
                            PurchaseRequestItems = x.PurchaseRequest.PurchaseRequestItems.Where(a => !a.IsDeleted).Select(b => new PurchaseRequestItemInfoDto
                            {
                                ProductCode = b.Product.ProductCode,
                                DocumentStatus = !b.Product.DocumentProducts.Any(v => !v.Document.IsDeleted && v.Document.IsActive && v.Document.ContractCode == authenticate.ContractCode)
                                ? EngineeringDocumentStatus.No
                                : b.Product.DocumentProducts.Any(v => !v.Document.IsDeleted && v.Document.IsActive && v.Document.ContractCode == authenticate.ContractCode &&
                                (!v.Document.DocumentRevisions.Any(a => !a.IsDeleted) || v.Document.DocumentRevisions.Any(n => !n.IsDeleted && n.RevisionStatus < RevisionStatus.Confirmed)))
                                ? EngineeringDocumentStatus.completing
                                : EngineeringDocumentStatus.Completed,
                                Id = b.Id,
                                ProductDescription = b.Product.Description,
                                ProductGroupName = b.Product.ProductGroup.Title,
                                ProductTechnicalNumber = b.Product.TechnicalNumber,
                                ProductId = b.ProductId,
                                ProductUnit = b.Product.Unit,
                                Quntity = b.Quntity,
                                PurchaseRequestId = b.PurchaseRequestId,
                                DateEnd = b.DateEnd.ToUnixTimestamp(),
                                DateStart = b.DateStart.ToUnixTimestamp()
                            }).ToList(),
                            PurchaseRequestConfirmationUserWorkFlows = x.PurchaseConfirmationWorkFlowUsers.Where(a => a.UserId > 0)
                            .Select(e => new PurchaseRequestConfirmationUserWorkFlowDto
                            {
                                PurchaseRequestConfirmationWorkFlowUserId = e.PurchaseRequestConfirmWorkFlowUserId,
                                DateEnd = e.DateEnd.ToUnixTimestamp(),
                                DateStart = e.DateStart.ToUnixTimestamp(),
                                IsAccept = e.IsAccept,
                                IsBallInCourt = e.IsBallInCourt,
                                Note = e.Note,
                                OrderNumber = e.OrderNumber,
                                UserId = e.UserId,
                                UserFullName = e.User != null ? e.User.FullName : "",
                                UserImage = e.User != null && e.User.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + e.User.Image : ""
                            }).ToList()
                        }).FirstOrDefaultAsync();
                List<PurchaseRequestItemInfoDto> purchaseItems = new List<PurchaseRequestItemInfoDto>();
                foreach (var item in result.PurchaseRequestItems)
                {
                    if (!purchaseItems.Any(a => a.ProductId == item.ProductId))
                    {
                        purchaseItems.Add(new PurchaseRequestItemInfoDto
                        {
                            Id = item.Id,
                            DateEnd = item.DateEnd,
                            DateStart = item.DateStart,
                            ProductCode = item.ProductCode,
                            ProductDescription = item.ProductDescription,
                            ProductGroupName = item.ProductGroupName,
                            ProductId = item.ProductId,
                            ProductTechnicalNumber = item.ProductTechnicalNumber,
                            ProductUnit = item.ProductUnit,
                            PurchaseRequestId = item.PurchaseRequestId,
                            Quntity = result.PurchaseRequestItems.Where(a => a.ProductId == item.ProductId).Sum(a => a.Quntity),
                            DocumentStatus = item.DocumentStatus
                        });
                    }
                }
                result.PurchaseRequestItems = purchaseItems;
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<PurchaseRequestConfirmationWorkflowDto>(null, exception);
            }
        }

        private async Task<ServiceResult<PurchaseConfirmationWorkFlow>> AddPurchaseConfirmationAttachmentAsync(string contractCode, long mrpId, PurchaseConfirmationWorkFlow confirmationWorkFlowModel, List<AddAttachmentDto> files)
        {


            if (files == null || !files.Any())
                return ServiceResultFactory.CreateError<PurchaseConfirmationWorkFlow>(null, MessageId.FileNotFound);

            var attachModels = new List<PAttachment>();

            // add oldFiles

            // add new files
            foreach (var item in files)
            {
                var UploadedFile = await _fileHelper
                    .SaveDocumentFromTemp(item.FileSrc, ServiceSetting.UploadFilePathPRConfirm(contractCode, mrpId));

                if (UploadedFile == null)
                    return ServiceResultFactory.CreateError<PurchaseConfirmationWorkFlow>(null, MessageId.UploudFailed);

                _fileHelper.DeleteDocumentFromTemp(item.FileSrc);

                confirmationWorkFlowModel.PurchaseConfirmationAttachments.Add(new PAttachment
                {
                    PurchaseConfirmWorkFlow = confirmationWorkFlowModel,
                    FileSrc = item.FileSrc,
                    FileName = item.FileName,
                    FileType = UploadedFile.FileType,
                    FileSize = UploadedFile.FileSize
                });

            }

            return ServiceResultFactory.CreateSuccess(confirmationWorkFlowModel);
        }

        private async Task<ServiceResult<PurchaseConfirmationWorkFlow>> AddPurchaseRequestConfirmationAsync(string contractCode, long mrpId, AddPurchaseRequestConfirmationDto model, List<AddAttachmentDto> attachments)
        {
            //if (usersIds.Count() == 0)
            //    return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);
            var purchaseRequestConfirmationModel = new PurchaseConfirmationWorkFlow
            {

                ConfirmNote = model.Note,
                Status = ConfirmationWorkFlowStatus.Pending,
                PurchaseConfirmationWorkFlowUsers = new List<PurchaseConfirmationWorkFlowUser>(),
                PurchaseConfirmationAttachments = new List<PAttachment>()
            };

            if (model.Users != null && model.Users.Any())
            {
                var usersIds = model.Users.Select(a => a.UserId).Distinct().ToList();
                if (await _userRepository.CountAsync(a => !a.IsDeleted && a.IsActive && usersIds.Contains(a.Id)) != usersIds.Count())
                    return ServiceResultFactory.CreateError<PurchaseConfirmationWorkFlow>(null, MessageId.DataInconsistency);

                foreach (var item in model.Users)
                {
                    purchaseRequestConfirmationModel.PurchaseConfirmationWorkFlowUsers.Add(new PurchaseConfirmationWorkFlowUser
                    {
                        UserId = item.UserId,
                        OrderNumber = item.OrderNumber
                    });
                }
                if (purchaseRequestConfirmationModel.PurchaseConfirmationWorkFlowUsers.Any())
                {
                    var bollincourtUser = purchaseRequestConfirmationModel.PurchaseConfirmationWorkFlowUsers.OrderBy(a => a.OrderNumber).First();
                    bollincourtUser.IsBallInCourt = true;
                    bollincourtUser.DateStart = DateTime.UtcNow;
                }
            }
            else
            {
                purchaseRequestConfirmationModel.Status = ConfirmationWorkFlowStatus.Confirm;
            }

            if (attachments != null && attachments.Any())
            {
                var res = await AddPurchaseConfirmationAttachmentAsync(contractCode, mrpId, purchaseRequestConfirmationModel, attachments);
                if (!res.Succeeded)
                    return ServiceResultFactory.CreateError<PurchaseConfirmationWorkFlow>(null, res.Messages[0].Message);
            }

            return ServiceResultFactory.CreateSuccess(purchaseRequestConfirmationModel);
        }
        private async Task<ServiceResult<PurchaseConfirmationWorkFlow>> AddPurchaseRequestConfirmationsAsync(PurchaseConfirmationWorkFlow purchaseConfirmationWorkFlow, AddPurchaseRequestConfirmationDto model)
        {
            //if (usersIds.Count() == 0)
            //    return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);
            purchaseConfirmationWorkFlow.PurchaseConfirmationWorkFlowUsers = new List<PurchaseConfirmationWorkFlowUser>();


            if (model.Users != null && model.Users.Any())
            {

                var usersIds = model.Users.Select(a => a.UserId).Distinct().ToList();
                if (await _userRepository.CountAsync(a => !a.IsDeleted && a.IsActive && usersIds.Contains(a.Id)) != usersIds.Count())
                    return ServiceResultFactory.CreateError<PurchaseConfirmationWorkFlow>(null, MessageId.DataInconsistency);
                purchaseConfirmationWorkFlow.Status = ConfirmationWorkFlowStatus.Pending;
                foreach (var item in model.Users)
                {
                    purchaseConfirmationWorkFlow.PurchaseConfirmationWorkFlowUsers.Add(new PurchaseConfirmationWorkFlowUser
                    {
                        UserId = item.UserId,
                        OrderNumber = item.OrderNumber
                    });
                }
                if (purchaseConfirmationWorkFlow.PurchaseConfirmationWorkFlowUsers.Any())
                {
                    var bollincourtUser = purchaseConfirmationWorkFlow.PurchaseConfirmationWorkFlowUsers.OrderBy(a => a.OrderNumber).First();
                    bollincourtUser.IsBallInCourt = true;
                    bollincourtUser.DateStart = DateTime.UtcNow;
                }
            }
            else
            {
                purchaseConfirmationWorkFlow.Status = ConfirmationWorkFlowStatus.Confirm;
            }



            return ServiceResultFactory.CreateSuccess(purchaseConfirmationWorkFlow);
        }
        public async Task<ServiceResult<PurchaseRequestConfirmationWorkflowDto>> GetPendingPurchaseRequestConfirmByPurchaseRequestIdAsync(AuthenticateDto authenticate, long purchaseRequestId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<PurchaseRequestConfirmationWorkflowDto>(null, MessageId.AccessDenied);

                var dbQuery = _purchaseRequestConfirmWorkFlowRepository
                     .AsNoTracking()
                     .Where(a => !a.IsDeleted &&
                    a.PurchaseRequestId == purchaseRequestId &&
                    a.Status == ConfirmationWorkFlowStatus.Pending &&
                     a.PurchaseRequest.ContractCode == authenticate.ContractCode &&
                     a.PurchaseRequest.PRStatus == PRStatus.Register);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<PurchaseRequestConfirmationWorkflowDto>(null, MessageId.EntityDoesNotExist);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.PurchaseRequest.ProductGroupId)))
                    return ServiceResultFactory.CreateError<PurchaseRequestConfirmationWorkflowDto>(null, MessageId.AccessDenied);

                var result = await dbQuery
                        .Select(x => new PurchaseRequestConfirmationWorkflowDto
                        {
                            ConfirmNote = x.ConfirmNote,

                            PurchaseRequestConfirmUserAudit = x.AdderUserId != null ? new UserAuditLogDto
                            {
                                CreateDate = x.CreatedDate.ToUnixTimestamp(),
                                AdderUserName = x.AdderUser.FullName,
                                AdderUserImage = x.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + x.AdderUser.Image : ""
                            } : null,
                            Attachments = x.PurchaseConfirmationAttachments.Where(m => !m.IsDeleted)
                            .Select(c => new BasePRAttachmentDto
                            {
                                FileName = c.FileName,
                                FileSize = c.FileSize,
                                FileSrc = c.FileSrc,
                                FileType = c.FileType,
                                PurchaseRequestId = c.PurchaseRequestId.Value
                            }).ToList(),

                            PurchaseRequestConfirmationUserWorkFlows = x.PurchaseConfirmationWorkFlowUsers.Where(a => a.UserId > 0)
                            .Select(e => new PurchaseRequestConfirmationUserWorkFlowDto
                            {
                                PurchaseRequestConfirmationWorkFlowUserId = e.PurchaseRequestConfirmWorkFlowUserId,
                                DateEnd = e.DateEnd.ToUnixTimestamp(),
                                DateStart = e.DateStart.ToUnixTimestamp(),
                                IsAccept = e.IsAccept,
                                IsBallInCourt = e.IsBallInCourt,
                                Note = e.Note,
                                OrderNumber = e.OrderNumber,
                                UserId = e.UserId,
                                UserFullName = e.User != null ? e.User.FullName : "",
                                UserImage = e.User != null && e.User.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + e.User.Image : ""
                            }).ToList()
                        }).FirstOrDefaultAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<PurchaseRequestConfirmationWorkflowDto>(null, exception);
            }
        }
        private async Task<ServiceResult<PurchaseRequest>> AddPRAttachmentAsync(PurchaseRequest PRModel, List<AddAttachmentDto> attachment)
        {
            PRModel.PRAttachments = new List<PAttachment>();

            foreach (var item in attachment)
            {
                var UploadedFile =
                    await _fileHelper.SaveDocumentFromTemp(item.FileSrc, ServiceSetting.UploadFilePath.PR);
                if (UploadedFile == null)
                    return ServiceResultFactory.CreateError<PurchaseRequest>(null, MessageId.UploudFailed);

                _fileHelper.DeleteDocumentFromTemp(item.FileSrc);
                PRModel.PRAttachments.Add(new PAttachment
                {
                    FileName = item.FileName,
                    FileSrc = item.FileSrc,
                    FileType = UploadedFile.FileType,
                    FileSize = UploadedFile.FileSize
                });
            }
            return ServiceResultFactory.CreateSuccess(PRModel);
        }
        private static int GetPurchaseRequestItem(List<PurchaseRequestItem> purchaseRequestItems)
        {
            List<int> productIds = new List<int>();
            foreach (var item in purchaseRequestItems.Where(a => !a.IsDeleted))
            {
                if (!productIds.Contains(item.ProductId))
                    productIds.Add(item.ProductId);
            }
            return productIds.Count;
        }
        private static List<string> GetProductsForPurchaseRequest(List<string> ProductTitle)
        {
            List<string> productTitles = new List<string>();
            foreach (var item in ProductTitle)
            {
                if (!productTitles.Contains(item))
                    productTitles.Add(item);
            }
            return productTitles;
        }
        private bool UpdateMrpItemAfterRejectPR(List<PurchaseRequestItem> prIem, List<MrpItem> mrpItems)
        {
            foreach (var item in prIem)
            {
                var mrpItem = mrpItems.FirstOrDefault(a => a.ProductId == item.ProductId);
                mrpItem.RemainedStock += item.Quntity;
                mrpItem.DoneStock -= item.Quntity;
            }
            return true;
        }


        //public async Task<bool> SendEmailForRejectDocumentRevisionAsync(AuthenticateDto authenticate, PurchaseConfirmationWorkFlow confirmationModel, string reason,long purchaseRequestId)
        //{
        //    string url = $"/dashboard/documents/RevDetails/{confirmationModel.DocumentRevisionId}?team={authenticate.ContractCode}&wfid={confirmationModel.ConfirmationWorkFlowId}";

        //   // url = _appSettings.ClientHost + url;
        //    //var user = await _purchaseRequestRepository.Include(d => d.AdderUser).SingleOrDefaultAsync(d => d.Id == purchaseRequestId);
        //    //var dat = DateTime.UtcNow.ToPersianDate();
        //    //var emailDTO = new RejectRevisionEmailDTO(user.DocumentRevisionCode.ToString(), user.Document.DocNumber, authenticate.ContractCode, authenticate.UserName, reason, user.Document.DocTitle, url, user.AdderUser.FullName, _appSettings.CompanyName);

        //    //var emalRequest = new EmailRequest
        //    //{
        //    //    Attachment = null,
        //    //    Subject = $"Reject | {user.Document.DocNumber}",
        //    //    Body = await _viewRenderService.RenderToStringAsync("_RejectRevisionEmail", emailDTO),
        //    //};

        //    //emalRequest.To = new List<string> { user.AdderUser.Email };
        //    //await _appEmailService.SendAsync(emalRequest);
        //    return true;

        //}
    }
}