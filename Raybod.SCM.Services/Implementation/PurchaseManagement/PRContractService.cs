using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Options;
using Raybod.SCM.DataAccess.Core;
using Raybod.SCM.DataAccess.Extention;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Audit;
using Raybod.SCM.DataTransferObject.Notification;
using Raybod.SCM.DataTransferObject.PO;
using Raybod.SCM.DataTransferObject.PRContract;
using Raybod.SCM.DataTransferObject.RFP;
using Raybod.SCM.DataTransferObject.Supplier;
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

namespace Raybod.SCM.Services.Implementation
{
    public class PRContractService : IPRContractService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPOService _poService;
        private readonly ISCMLogAndNotificationService _scmLogAndNotificationService;
        private readonly IContractFormConfigService _formConfigService;
        private readonly DbSet<PRContract> _prContractRepository;
        private readonly DbSet<PrContractConfirmationWorkFlow> _prContractWorkFlowRepository;
        private readonly DbSet<PAttachment> _prContractAttachmentRepository;
        private readonly DbSet<PRContractSubject> _prContractSubjectRepository;
        private readonly DbSet<POTermsOfPayment> _poTermsOfPaymentRepository;
        private readonly DbSet<Product> _productRepository;
        private readonly DbSet<Supplier> _supplierRepository;
        private readonly DbSet<RFP> _rfpRepository;
        private readonly DbSet<RFPItems> _rfpItemRepository;
        private readonly DbSet<MrpItem> _mrpItemRepository;
        private readonly DbSet<PO> _poRepository;
        private readonly DbSet<User> _userRepository;
        private readonly DbSet<POSubject> _poSubjectRepository;
        private readonly DbSet<ProductGroup> _productGroupRepository;
        private readonly ITeamWorkAuthenticationService _authenticationServices;
        private readonly Utilitys.FileHelper _fileHelper;
        private readonly CompanyAppSettingsDto _appSettings;

        public PRContractService(
            IUnitOfWork unitOfWork,
            IWebHostEnvironment hostingEnvironmentRoot,
            IOptions<CompanyAppSettingsDto> appSettings,
            ITeamWorkAuthenticationService authenticationServices,
            IPOService poService,
            ISCMLogAndNotificationService scmLogAndNotificationService,
            IContractFormConfigService formConfigService)
        {
            _unitOfWork = unitOfWork;
            _poService = poService;
            _authenticationServices = authenticationServices;
            _scmLogAndNotificationService = scmLogAndNotificationService;
            _formConfigService = formConfigService;
            _appSettings = appSettings.Value;
            _poTermsOfPaymentRepository = _unitOfWork.Set<POTermsOfPayment>();
            _prContractWorkFlowRepository = _unitOfWork.Set<PrContractConfirmationWorkFlow>();
            _prContractRepository = _unitOfWork.Set<PRContract>();
            _prContractAttachmentRepository = _unitOfWork.Set<PAttachment>();
            _prContractSubjectRepository = _unitOfWork.Set<PRContractSubject>();
            _productRepository = _unitOfWork.Set<Product>();
            _supplierRepository = _unitOfWork.Set<Supplier>();
            _rfpRepository = _unitOfWork.Set<RFP>();
            _rfpItemRepository = _unitOfWork.Set<RFPItems>();
            _poRepository = _unitOfWork.Set<PO>();
            _userRepository = _unitOfWork.Set<User>();
            _mrpItemRepository = _unitOfWork.Set<MrpItem>();
            _productGroupRepository = _unitOfWork.Set<ProductGroup>();
            _poSubjectRepository = _unitOfWork.Set<POSubject>();
            _fileHelper = new Utilitys.FileHelper(hostingEnvironmentRoot);
        }

        public async Task<ServiceResult<string>> AddPRContractAsync(AuthenticateDto authenticate, AddPRContractDto model)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError("", MessageId.AccessDenied);

                if (model.SupplierId <= 0)
                    return ServiceResultFactory.CreateError<string>("", MessageId.DataInconsistency);

                if (!IsCorrectDeliveryLocationByPContractType(model.ContractType.SelectedType.Value, model.ContractType.SelectedDelivery.Value))
                    return ServiceResultFactory.CreateError<string>("", MessageId.InputDataValidationError);

                if (model.ContractTimeTable.DateEnd < model.ContractTimeTable.DateIssued)
                    return ServiceResultFactory.CreateError("", MessageId.InputDataValidationError);
                //quantity


                var supplierModel = await _supplierRepository.FirstOrDefaultAsync(a => a.Id == model.SupplierId);
                if (supplierModel == null)
                    return ServiceResultFactory.CreateError<string>("", MessageId.SupplierNotFound);


                if (model.PRContractSubjects.GroupBy(c => c.ProductId).Any(c => c.Count() > 1))
                    return ServiceResultFactory.CreateError<string>("", MessageId.ImpossibleDuplicateProduct);

                var postedRFPIds = model.PRContractSubjects.Select(a => a.RFPId).ToList();
                var postedProductIds = model.PRContractSubjects.Select(a => a.ProductId).ToList();
                var rfpItemModels = await _rfpItemRepository
                    .Where(a => a.IsActive &&
                    !a.IsDeleted &&
                    a.RemainedStock > 0 &&
                    a.RFP.ContractCode == authenticate.ContractCode &&
                    (postedProductIds.Contains(a.ProductId) && postedRFPIds.Contains(a.RFPId)))
                    .Include(a => a.Product)
                    .Include(a => a.RFP)
                    .ThenInclude(a => a.RFPSuppliers)
                    .Include(a => a.PurchaseRequestItem)
                    .ThenInclude(a => a.PurchaseRequest)
                    .Include(a=>a.PurchaseRequestItem)
                    .ThenInclude(a=>a.MrpItem)
                    .ToListAsync();

                //if (postedRFPItemIds.Count() != rfpItemModels.Count())
                //    return ServiceResultFactory.CreateError<string>("", MessageId.DataInconsistency);

                if (rfpItemModels.GroupBy(a => a.RFP.ProductGroupId).Count() > 1)
                    return ServiceResultFactory.CreateError<string>("", MessageId.DataInconsistency);

                if (rfpItemModels.Any(c => c.RFP.RFPSuppliers.Any(v => !v.IsDeleted && v.IsActive && !v.IsWinner && v.SupplierId == model.SupplierId)))
                    return ServiceResultFactory.CreateError<string>("", MessageId.DataInconsistency);

                model.ProductGroupId = rfpItemModels.Select(a => a.RFP.ProductGroupId).First();

                if (permission.ProductGroupIds.Any() && !permission.ProductGroupIds.Contains(model.ProductGroupId))
                    return ServiceResultFactory.CreateError("", MessageId.AccessDenied);


                var prContractModel = new PRContract
                {
                    DateIssued = model.ContractTimeTable.DateIssued.UnixTimestampToDateTime().Date,
                    DateEnd = model.ContractTimeTable.DateEnd.UnixTimestampToDateTime().Date,
                    PRContractStatus = PRContractStatus.Register,
                    BaseContractCode = authenticate.ContractCode,
                    CurrencyType = model.ContractType.SelectedCurrency.Value,
                    ContractDuration = model.ContractTimeTable.ContractDuration,
                    DeliveryLocation = model.ContractType.SelectedDelivery.Value,
                    PContractType = model.ContractType.SelectedType.Value,
                    ProductGroupId = model.ProductGroupId,
                    SupplierId = model.SupplierId,
                    Tax = model.ContractType.Tax,
                    PRContractSubjects = new List<PRContractSubject>(),

                    PrContractConfirmationWorkFlows = new List<PrContractConfirmationWorkFlow>(),
                };

                var addPRContractSubjectResult = await AddPRContractSubjectAsync(prContractModel, model.PRContractSubjects, rfpItemModels);
                if (!addPRContractSubjectResult.Succeeded)
                    return ServiceResultFactory.CreateError("", addPRContractSubjectResult.Messages.First().Message);
                var confirmWorkFlow = await AddPrContractConfirmationAsync(authenticate.ContractCode, model.WorkFlow, model.PRCAttachments);
                if (!confirmWorkFlow.Succeeded)
                    return ServiceResultFactory.CreateError("", MessageId.OperationFailed);
                if (confirmWorkFlow.Result.Status == ConfirmationWorkFlowStatus.Confirm)
                {
                    prContractModel.PRContractStatus = PRContractStatus.Active;
                    if (model.PRCAttachments != null && model.PRCAttachments.Any())
                    {
                        if (!_fileHelper.FileExistInTemp(model.PRCAttachments.Select(c => c.FileSrc).ToList()))
                            return ServiceResultFactory.CreateError<string>("", MessageId.FileNotFound);
                        var attachmentResult = await AddPRContractAttachment(prContractModel, model.PRCAttachments);
                        if (!attachmentResult.Succeeded)
                            return ServiceResultFactory.CreateError<string>("",
                                attachmentResult.Messages.FirstOrDefault().Message);

                        prContractModel = attachmentResult.Result;
                    }
                }
                prContractModel.PrContractConfirmationWorkFlows.Add(confirmWorkFlow.Result);
                prContractModel = addPRContractSubjectResult.Result;

                if (model.PaymentSteps != null)
                {
                    if (model.PaymentSteps.PackPayment.Percent + model.PaymentSteps.PrepPayment.Percent + model.PaymentSteps.OrderPayment.Percent + model.PaymentSteps.InvoicePayment.Percent != 100)
                        return ServiceResultFactory.CreateError<string>("", MessageId.PaymentPercentShouldBeEqualHundred);

                    prContractModel = AddPRContractTermsOfPayments(prContractModel, model.PaymentSteps);
                }

                // calculateContractAmount
                CalculateContractAmount(model.ContractType.Tax, prContractModel);

                // generate form code
                var count = await _prContractRepository.CountAsync(a => a.BaseContractCode == authenticate.ContractCode);
                var codeRes = await _formConfigService.GenerateFormCodeAsync(authenticate.ContractCode, FormName.PRContract, count);
                if (!codeRes.Succeeded)
                    return ServiceResultFactory.CreateError("", codeRes.Messages.First().Message);
                prContractModel.PRContractCode = codeRes.Result;
                if (prContractModel.PRContractStatus == PRContractStatus.Active)
                {
                    var serviceResult = await _poService.AddPOToConfirmedByPRContractAsync(prContractModel, rfpItemModels);
                    if (!serviceResult.Succeeded)
                        return ServiceResultFactory.CreateError("", serviceResult.Messages.FirstOrDefault().Message);

                    _poRepository.AddRange(serviceResult.Result);
                }

                _prContractRepository.Add(prContractModel);

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var rfpIds = rfpItemModels.Select(a => a.RFPId).ToList();
                    var donedRFPIds = await _rfpRepository
                         .Where(a => !a.IsDeleted && rfpIds.Contains(a.Id) && !a.RFPItems.Any(c => c.IsActive && !c.IsDeleted && c.RemainedStock > 0))
                         .Select(v => v.Id.ToString())
                         .ToListAsync();

                    if (donedRFPIds.Any())
                        await _scmLogAndNotificationService.SetDonedNotificationAsync(authenticate.UserId, authenticate.ContractCode, donedRFPIds, NotifEvent.AddPrContract);
                    int? userId = null;
                    if (prContractModel.PRContractStatus != PRContractStatus.Active)
                    {
                        userId = prContractModel.PrContractConfirmationWorkFlows.First().PrContractConfirmationWorkFlowUsers.First(a => a.IsBallInCourt).UserId;
                    }
                    if (prContractModel.PRContractStatus == PRContractStatus.Active)
                    {
                        await SendNotifOnConfirmContract(authenticate,prContractModel,supplierModel.Name);
                    }
                    await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = prContractModel.BaseContractCode,
                        FormCode = prContractModel.PRContractCode,
                        ProductGroupId = model.ProductGroupId,
                        Description = supplierModel.Name,
                        KeyValue = prContractModel.Id.ToString(),
                        RootKeyValue = prContractModel.Id.ToString(),
                        NotifEvent = NotifEvent.AddPrContract,
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,
                    },
                    model.ProductGroupId,
                    NotifEvent.ConfirmPRContract,
                    userId
                    );
                    return ServiceResultFactory.CreateSuccess(prContractModel.Id.ToString());
                }
                return ServiceResultFactory.CreateError<string>("", MessageId.SaveFailed);
            }
            catch (Exception e)
            {
                return ServiceResultFactory.CreateException("", exception: e);
            }
        }
        public async Task<ServiceResult<string>> EditPRContractAsync(AuthenticateDto authenticate, long prContractId, AddPRContractDto model)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError("", MessageId.AccessDenied);

                if (model.SupplierId <= 0)
                    return ServiceResultFactory.CreateError<string>("", MessageId.DataInconsistency);

                if (!IsCorrectDeliveryLocationByPContractType(model.ContractType.SelectedType.Value, model.ContractType.SelectedDelivery.Value))
                    return ServiceResultFactory.CreateError<string>("", MessageId.InputDataValidationError);

                if (model.ContractTimeTable.DateEnd < model.ContractTimeTable.DateIssued)
                    return ServiceResultFactory.CreateError("", MessageId.InputDataValidationError);
                //quantity


                var supplierModel = await _supplierRepository.FirstOrDefaultAsync(a => a.Id == model.SupplierId);
                if (supplierModel == null)
                    return ServiceResultFactory.CreateError<string>("", MessageId.SupplierNotFound);

                // check supplier info is completed


                if (model.PRContractSubjects.GroupBy(c => c.ProductId).Any(c => c.Count() > 1))
                    return ServiceResultFactory.CreateError<string>("", MessageId.ImpossibleDuplicateProduct);

                var postedRFPIds = model.PRContractSubjects.Select(a => a.RFPId).ToList();
                var postedProductIds = model.PRContractSubjects.Select(a => a.ProductId).ToList();
                var rfpItemModels = await _rfpItemRepository
                    .Where(a => a.IsActive &&
                    !a.IsDeleted &&
                    a.RFP.ContractCode == authenticate.ContractCode &&
                    (postedProductIds.Contains(a.ProductId) && postedRFPIds.Contains(a.RFPId)))
                    .Include(a => a.Product)
                    .Include(a => a.RFP)
                    .ThenInclude(a => a.RFPSuppliers)
                    .Include(a => a.PurchaseRequestItem)
                    .ThenInclude(a => a.PurchaseRequest)
                    .ToListAsync();

                //if (postedRFPItemIds.Count() != rfpItemModels.Count())
                //    return ServiceResultFactory.CreateError<string>("", MessageId.DataInconsistency);

                if (rfpItemModels.GroupBy(a => a.RFP.ProductGroupId).Count() > 1)
                    return ServiceResultFactory.CreateError<string>("", MessageId.DataInconsistency);

                if (rfpItemModels.Any(c => c.RFP.RFPSuppliers.Any(v => !v.IsDeleted && v.IsActive && !v.IsWinner && v.SupplierId == model.SupplierId)))
                    return ServiceResultFactory.CreateError<string>("", MessageId.DataInconsistency);

                model.ProductGroupId = rfpItemModels.Select(a => a.RFP.ProductGroupId).First();

                if (permission.ProductGroupIds.Any() && !permission.ProductGroupIds.Contains(model.ProductGroupId))
                    return ServiceResultFactory.CreateError("", MessageId.AccessDenied);
                var dbQuery = _prContractRepository
                        .Where(a => a.Id == prContractId && a.BaseContractCode == authenticate.ContractCode)
                        .Include(a => a.Supplier)
                        .Include(a => a.PRContractSubjects)
                        .ThenInclude(c => c.RFPItem)
                        .ThenInclude(c => c.PurchaseRequestItem)
                        .ThenInclude(c => c.MrpItem)
                        .Include(a => a.PRContractSubjects)
                        .Include(a=>a.PrContractConfirmationWorkFlows)
                        .Include(a=>a.TermsOfPayments)
                        .AsQueryable();
                var prContractModel = await dbQuery.FirstOrDefaultAsync();
                if (prContractModel.PRContractStatus != PRContractStatus.Rejected)
                {
                    var workflow = prContractModel.PrContractConfirmationWorkFlows.Where(a => !a.IsDeleted).OrderByDescending(a => a.PrContractConfirmWorkFlowId).FirstOrDefault();
                    if (workflow == null)
                        return ServiceResultFactory.CreateError("", MessageId.OperationFailed);
                    if (workflow.PrContractConfirmationWorkFlowUsers.Any(a => a.IsAccept))
                        return ServiceResultFactory.CreateError("", MessageId.OperationFailed);
                }
                ServiceResult<PRContract> addPRContractSubjectResult = null;
                ServiceResult<PrContractConfirmationWorkFlow> confirmWorkFlow = null;
                if (prContractModel.SupplierId != model.SupplierId)
                {
                    addPRContractSubjectResult = await AddPRContractSubjectAsync(prContractModel, model.PRContractSubjects, rfpItemModels);
                    if (!addPRContractSubjectResult.Succeeded)
                        return ServiceResultFactory.CreateError("", addPRContractSubjectResult.Messages.First().Message);
                    var removeResult = await RemovePRContractSubject(prContractModel.PRContractSubjects.ToList());
                    if (!removeResult.Succeeded)
                        return ServiceResultFactory.CreateError("", removeResult.Messages.First().Message);

                }
                else
                {
                    if (model.PRContractSubjects != null && model.PRContractSubjects.Any())
                    {

                        var res = await EditContractSubjectAsync(prContractModel, model.PRContractSubjects, authenticate.ContractCode);
                        if (!res.Succeeded)
                            return ServiceResultFactory.CreateError("", res.Messages.First().Message);
                    }

                }
                if (prContractModel.PRContractStatus == PRContractStatus.Rejected)
                {
                    prContractModel.PRContractStatus = PRContractStatus.Register;
                    confirmWorkFlow = await AddPrContractConfirmationAsync(authenticate.ContractCode, model.WorkFlow, model.PRCAttachments);
                    if (confirmWorkFlow.Succeeded)
                        prContractModel.PrContractConfirmationWorkFlows.Add(confirmWorkFlow.Result);
                }
                else
                {


                    confirmWorkFlow = await EditPrContractConfirmationAsync(prContractModel, model.WorkFlow, model.PRCAttachments);


                }

                if (!confirmWorkFlow.Succeeded)
                    return ServiceResultFactory.CreateError("", MessageId.OperationFailed);
                if (confirmWorkFlow.Result.Status == ConfirmationWorkFlowStatus.Confirm)
                {
                    prContractModel.PRContractStatus = PRContractStatus.Active;
                    if (model.PRCAttachments != null && model.PRCAttachments.Any())
                    {
                        if (!_fileHelper.FileExistInTemp(model.PRCAttachments.Select(c => c.FileSrc).ToList()))
                            return ServiceResultFactory.CreateError<string>("", MessageId.FileNotFound);
                        var attachmentResult = await AddPRContractAttachment(prContractModel, model.PRCAttachments);
                        if (!attachmentResult.Succeeded)
                            return ServiceResultFactory.CreateError<string>("",
                                attachmentResult.Messages.FirstOrDefault().Message);

                        prContractModel = attachmentResult.Result;
                    }
                }


                if (prContractModel.SupplierId != model.SupplierId)
                    prContractModel = addPRContractSubjectResult.Result;


                if (model.PaymentSteps != null)
                {
                    if (model.PaymentSteps.PackPayment.Percent + model.PaymentSteps.PrepPayment.Percent + model.PaymentSteps.OrderPayment.Percent + model.PaymentSteps.InvoicePayment.Percent > 100)
                        return ServiceResultFactory.CreateError<string>("", MessageId.InputDataValidationError);
                    prContractModel = AddPRContractTermsOfPayments(prContractModel, model.PaymentSteps);
                }
                // calculateContractAmount
                CalculateContractAmount(model.ContractType.Tax, prContractModel);

                if (prContractModel.PRContractStatus == PRContractStatus.Active)
                {
                    var serviceResult = await _poService.AddPOToPendingByPRContractAsync(prContractModel);
                    if (!serviceResult.Succeeded)
                        return ServiceResultFactory.CreateError("", serviceResult.Messages.FirstOrDefault().Message);

                    _poRepository.AddRange(serviceResult.Result);
                }
                prContractModel.DateIssued = model.ContractTimeTable.DateIssued.UnixTimestampToDateTime().Date;
                prContractModel.DateEnd = model.ContractTimeTable.DateEnd.UnixTimestampToDateTime().Date;
                prContractModel.BaseContractCode = authenticate.ContractCode;
                prContractModel.CurrencyType = model.ContractType.SelectedCurrency.Value;
                prContractModel.ContractDuration = model.ContractTimeTable.ContractDuration;
                prContractModel.DeliveryLocation = model.ContractType.SelectedDelivery.Value;
                prContractModel.PContractType = model.ContractType.SelectedType.Value;
                prContractModel.ProductGroupId = model.ProductGroupId;
                prContractModel.SupplierId = model.SupplierId;
                prContractModel.Tax = model.ContractType.Tax;
                //_prContractRepository.Add(prContractModel);

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var rfpIds = rfpItemModels.Select(a => a.RFPId).ToList();
                    var donedRFPIds = await _rfpRepository
                         .Where(a => !a.IsDeleted && rfpIds.Contains(a.Id) && !a.RFPItems.Any(c => c.IsActive && !c.IsDeleted && c.RemainedStock > 0))
                         .Select(v => v.Id.ToString())
                         .ToListAsync();

                    if (donedRFPIds.Any())
                        await _scmLogAndNotificationService.SetDonedNotificationAsync(authenticate.UserId, authenticate.ContractCode, donedRFPIds, NotifEvent.AddPrContract);
                    int? userId = null;
                    if (prContractModel.PRContractStatus != PRContractStatus.Active)
                    {
                        userId = prContractModel.PrContractConfirmationWorkFlows.First(a=>!a.IsDeleted&&a.Status==ConfirmationWorkFlowStatus.Pending).PrContractConfirmationWorkFlowUsers.First(a => a.IsBallInCourt).UserId;
                    }
                    await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = prContractModel.BaseContractCode,
                        FormCode = prContractModel.PRContractCode,
                        ProductGroupId = model.ProductGroupId,
                        Description = supplierModel.Name,
                        KeyValue = prContractModel.Id.ToString(),
                        RootKeyValue = prContractModel.Id.ToString(),
                        NotifEvent = (prContractModel.PRContractStatus != PRContractStatus.Active)? NotifEvent.EditPRContract:NotifEvent.AddPrContract,
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,
                    },
                    model.ProductGroupId,
                    NotifEvent.ConfirmPRContract,
                    userId
                    );
                    return ServiceResultFactory.CreateSuccess(prContractModel.Id.ToString());
                }
                return ServiceResultFactory.CreateError<string>("", MessageId.SaveFailed);
            }
            catch (Exception e)
            {
                return ServiceResultFactory.CreateException("", exception: e);
            }
        }
        private static void CalculateContractAmount(decimal tax, PRContract prContractModel)
        {
            var totalAmount = prContractModel.PRContractSubjects.Where(a => !a.IsDeleted).Sum(a => a.TotalPrice);
           

            prContractModel.TotalAmount = totalAmount;
            if (tax <= 0)
                prContractModel.FinalTotalAmount = totalAmount;
            else
                prContractModel.FinalTotalAmount = totalAmount + (totalAmount * (tax / 100));
        }

        

        public async Task<ServiceResult<bool>> ApprovePRContract(AuthenticateDto authenticate, long prContractId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery = _prContractRepository
                    .Where(a => a.Id == prContractId && a.BaseContractCode == authenticate.ContractCode)
                    .Include(a => a.Supplier)
                    .Include(a => a.TermsOfPayments)
                    .Include(a => a.PRContractSubjects)
                    .ThenInclude(a => a.RFPItem)
                    .AsQueryable();

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(c => permission.ProductGroupIds.Contains(c.ProductGroupId)))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var prContractModel = await dbQuery
                    .FirstOrDefaultAsync();

                if (prContractModel.PRContractStatus >= PRContractStatus.Active)
                    return ServiceResultFactory.CreateError(false, MessageId.ApproveBefore);

                prContractModel.PRContractStatus = PRContractStatus.Active;

                var serviceResult = await _poService.AddPOToPendingByPRContractAsync(prContractModel);

                if (!serviceResult.Succeeded)
                    return ServiceResultFactory.CreateError(false, serviceResult.Messages.FirstOrDefault().Message);

                _poRepository.AddRange(serviceResult.Result);
                if (!(await _unitOfWork.SaveChangesAsync() > 0))
                    return ServiceResultFactory.CreateError(false, MessageId.EditEntityFailed);

                await _scmLogAndNotificationService.SetDonedNotificationAsync(authenticate.UserId, prContractModel.BaseContractCode, prContractModel.Id.ToString(), NotifEvent.ConfirmPRContract);
                await AddLogAndNotificationOnApprovePRContractAsync(authenticate, prContractModel, serviceResult);

                return ServiceResultFactory.CreateSuccess(true);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        private async Task AddLogAndNotificationOnApprovePRContractAsync(AuthenticateDto authenticate, PRContract prContractModel, ServiceResult<List<PO>> serviceResult)
        {
            var logModel = new AddAuditLogDto
            {
                ContractCode = prContractModel.BaseContractCode,
                FormCode = prContractModel.PRContractCode,
                KeyValue = prContractModel.Id.ToString(),
                Description = prContractModel.Supplier.Name,
                RootKeyValue = prContractModel.Id.ToString(),
                NotifEvent = NotifEvent.ConfirmPRContract,
                ProductGroupId = prContractModel.ProductGroupId,
                PerformerUserId = authenticate.UserId,
                PerformerUserFullName = authenticate.UserFullName,
            };

            await _scmLogAndNotificationService.AddScmAuditLogAsync(logModel, null);
            var task = new NotifToDto
            {
                NotifEvent = NotifEvent.AddPOPending,
                Roles = new List<string> { SCMRole.POMng }
            };
            var poIds = serviceResult.Result.Select(c => c.POId).ToList();
            await _scmLogAndNotificationService.AddPendingPOTaskNotificationAsync(logModel, task, authenticate.ContractCode, poIds);
        }

        public async Task<ServiceResult<List<EditTermsOfPaymentDto>>> GetPRContractTermOFPaymentByContractIdAsync(AuthenticateDto authenticate, long prContractId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<EditTermsOfPaymentDto>>(null, MessageId.AccessDenied);

                var dbQuery = _prContractRepository
                    .AsNoTracking()
                    .Where(a => a.Id == prContractId && a.BaseContractCode == authenticate.ContractCode);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<List<EditTermsOfPaymentDto>>(null, MessageId.EntityDoesNotExist);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(c => permission.ProductGroupIds.Contains(c.ProductGroupId)))
                    return ServiceResultFactory.CreateError<List<EditTermsOfPaymentDto>>(null, MessageId.AccessDenied);

                var result = await dbQuery.SelectMany(a => a.TermsOfPayments)
                    .Where(c => !c.IsDeleted)
                    .Select(c => new EditTermsOfPaymentDto
                    {
                        Id = c.Id,
                        CreditDuration = c.CreditDuration,
                        PaymentPercentage = c.PaymentPercentage,
                        IsCreditPayment = c.IsCreditPayment,
                        PaymentStep = c.PaymentStep,
                        PRContractId = c.PRContractId.Value,
                    }).ToListAsync();
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<EditTermsOfPaymentDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<List<ListPRContractDto>>> GetPRContractsAsync(AuthenticateDto authenticate, PRContractQuery query)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<ListPRContractDto>>(null, MessageId.AccessDenied);

                var dbQuery = _prContractRepository
                    .AsNoTracking()
                    .Where(a => a.BaseContractCode == authenticate.ContractCode);

                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(c => permission.ProductGroupIds.Contains(c.ProductGroupId));

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(x =>
                        x.Supplier.Name.Contains(query.SearchText) ||
                        x.PRContractCode.Contains(query.SearchText) ||
                        x.Supplier.SupplierCode.Contains(query.SearchText) ||
                        x.PRContractSubjects.Any(c => c.RFPItem.RFP.RFPNumber.Contains(query.SearchText)));

                if (query.PRContractStatuses != null && query.PRContractStatuses.Any())
                    dbQuery = dbQuery.Where(x => query.PRContractStatuses.Contains(x.PRContractStatus));

                if (query.ProductGroupIds != null && query.ProductGroupIds.Any())
                {

                    dbQuery = dbQuery.Where(x => x.PRContractSubjects.Any(v => !v.IsDeleted && query.ProductGroupIds.Contains(v.Product.ProductGroupId)));
                }


                if (query.ProductIds != null && query.ProductIds.Any())
                {

                    dbQuery = dbQuery.Where(x => x.PRContractSubjects.Any(v => !v.IsDeleted && query.ProductIds.Contains(v.ProductId)));
                }


                if (query.SupplierIds != null && query.SupplierIds.Any())
                {
                    dbQuery = dbQuery.Where(x => query.SupplierIds.Contains(x.SupplierId));
                }


                var pageCount = dbQuery.Count();

                var columnsMap = new Dictionary<string, Expression<Func<PRContract, object>>>
                {
                    ["Id"] = v => v.Id,
                    ["PRContractCode"] = v => v.PRContractCode,
                };

                dbQuery = dbQuery.ApplayOrdering(query, columnsMap).ApplayPageing(query);
                var result = await dbQuery.Select(x => new ListPRContractDto
                {
                    PRContractId = x.Id,
                    PRContractCode = x.PRContractCode,
                    PRContractStatus = x.PRContractStatus,
                    DateIssued = x.DateIssued.ToUnixTimestamp(),
                    DateEnd = x.DateEnd.ToUnixTimestamp(),
                    RFPNumber = "",
                    SupplierCode = x.Supplier.SupplierCode,
                    SupplierName = x.Supplier.Name,
                    ProductGroupId = x.ProductGroupId,
                    ProductGroupTitle = x.ProductGroup.Title,
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
            catch (Exception e)
            {
                return ServiceResultFactory.CreateException(new List<ListPRContractDto>(), e);
            }
        }

        public async Task<ServiceResult<PRContractInfoDto>> GetPRContractByPRContractIdAsync(AuthenticateDto authenticate, long prContractId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<PRContractInfoDto>(null, MessageId.AccessDenied);

                var dbQuery = _prContractRepository

                    .AsNoTracking()
                    .Where(a => a.Id == prContractId && a.BaseContractCode == authenticate.ContractCode);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<PRContractInfoDto>(null, MessageId.EntityDoesNotExist);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(c => permission.ProductGroupIds.Contains(c.ProductGroupId)))
                    return ServiceResultFactory.CreateError<PRContractInfoDto>(null, MessageId.AccessDenied);

                var result = await dbQuery
                    .Select(x => new PRContractInfoDto
                    {
                        PRContractId = x.Id,
                        CurrencyType = x.CurrencyType,
                        PRContractCode = x.PRContractCode,
                        TotalAmount = x.TotalAmount,
                        FinalTotalAmountInLetters = x.FinalTotalAmount.NumberToText(Language.Persian),
                        PRContractStatus = x.PRContractStatus,
                        SupplierId = x.SupplierId,
                        PContractType = x.PContractType,
                        DateIssued = x.DateIssued.ToUnixTimestamp(),
                        DateEnd = x.DateEnd.ToUnixTimestamp(),
                        ContractDuration = x.ContractDuration,
                        Tax = x.Tax,
                        DeliveryLocation = x.DeliveryLocation,
                        FinalTotalAmount = x.FinalTotalAmount,
                        TaxAmount = (x.TotalAmount * (x.Tax / 100)),
                        ProductGroupId = x.ProductGroupId,
                        ProductGroupTitle = x.ProductGroup.Title,
                        PRContractSubjects = x.PRContractSubjects.Where(c => !c.IsDeleted).Select(c => new ListPRContractSubjectDto
                        {
                            PRContractSubjectId = c.Id,
                            Quantity = c.Quantity,
                            Price = c.UnitPrice,
                            ProductCode = c.Product.ProductCode,
                            ProductUnit = c.Product.Unit,
                            ProductName = c.Product.Description,
                            ProductId = c.ProductId,
                            RFPId = c.RFPItem.RFPId,
                            RFPNumber = c.RFPItem.RFP.RFPNumber,
                            TechnicalNumber = c.Product.TechnicalNumber,
                            ProductGroupTitle = c.Product.ProductGroup.Title,
                            OrderQuantity = c.Product.POSubjects.Where(a => a.PO.POStatus != POStatus.Pending && a.PO.PRContractId == prContractId).Sum(v => v.Quantity),
                            ReceiptQuantity = c.Product.POSubjects.Where(a => a.PO.POStatus != POStatus.Pending && a.PO.PRContractId == prContractId).Sum(v => v.ReceiptedQuantity),

                        }).ToList(),
                        Attachments = x.PRContractAttachments.Where(a => !a.IsDeleted).Select(a => new BasePRContractAttachmentDto
                        {
                            PRContractId = a.PRContractId.Value,
                            FileName = a.FileName,
                            FileType = a.FileType,
                            FileSize = a.FileSize,
                            FileSrc = a.FileSrc,
                            Id = a.Id
                        }).ToList(),
                        Supplier = new ListSupplierDto
                        {
                            Id = x.SupplierId,
                            SupplierCode = x.Supplier.SupplierCode,
                            Address = x.Supplier.Address,
                            Email = x.Supplier.Email,
                            Name = x.Supplier.Name,
                            EconomicCode = x.Supplier.EconomicCode,
                            NationalId = x.Supplier.NationalId,
                            TellPhone = x.Supplier.TellPhone,
                            Logo = _appSettings.ClientHost + ServiceSetting.UploadImagesPath.LogoSmall + x.Supplier.Logo
                        },
                        TermsOfPayments = x.TermsOfPayments.Where(a => !a.IsDeleted).Select(c => new EditTermsOfPaymentDto
                        {
                            Id = c.Id,
                            CreditDuration = c.CreditDuration,
                            PaymentPercentage = c.PaymentPercentage,
                            IsCreditPayment = c.IsCreditPayment,
                            PaymentStep = c.PaymentStep,
                            PRContractId = c.PRContractId.Value,
                        }).ToList(),


                        UserAudit = x.AdderUser != null
                            ? new UserAuditLogDto
                            {
                                AdderUserId = x.AdderUserId,
                                AdderUserName = x.AdderUser.FullName,
                                CreateDate = x.CreatedDate.ToUnixTimestamp(),
                                AdderUserImage = (x.AdderUser.Image != null) ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall +
                                                 x.AdderUser.Image : ""
                            }
                            : null
                    }).FirstOrDefaultAsync();
                List<ListPRContractSubjectDto> prContractSubjects = new List<ListPRContractSubjectDto>();
                foreach (var item in result.PRContractSubjects)
                {
                    if (!prContractSubjects.Any(a => a.ProductId == item.ProductId))
                    {
                        prContractSubjects.Add(new ListPRContractSubjectDto
                        {
                            PRContractSubjectId = item.PRContractSubjectId,
                            Quantity = result.PRContractSubjects.Where(p => p.ProductId == item.ProductId).Sum(p => p.Quantity),
                            Price = item.Price,
                            ProductCode = item.ProductCode,
                            ProductUnit = item.ProductUnit,
                            ProductName = item.ProductName,
                            ProductId = item.ProductId,
                            RFPId = item.RFPId,
                            RFPNumber = item.RFPNumber,
                            TechnicalNumber = item.TechnicalNumber,
                            ProductGroupTitle = item.ProductGroupTitle,
                            OrderQuantity = result.PRContractSubjects.Where(p => p.ProductId == item.ProductId).Sum(p => p.OrderQuantity),
                            ReceiptQuantity = result.PRContractSubjects.Where(p => p.ProductId == item.ProductId).Sum(p => p.ReceiptQuantity)
                        });
                    }
                }
                result.PRContractSubjects = prContractSubjects;
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception e)
            {
                return ServiceResultFactory.CreateException(new PRContractInfoDto(), e);
            }
        }
        public async Task<ServiceResult<EditPrContractInfoDto>> GetPRContractDetailsByPRContractIdAsync(AuthenticateDto authenticate, long prContractId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<EditPrContractInfoDto>(null, MessageId.AccessDenied);

                var dbQuery = _prContractRepository

                    .AsNoTracking()
                    .Where(a => a.Id == prContractId && a.BaseContractCode == authenticate.ContractCode);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<EditPrContractInfoDto>(null, MessageId.EntityDoesNotExist);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(c => permission.ProductGroupIds.Contains(c.ProductGroupId)))
                    return ServiceResultFactory.CreateError<EditPrContractInfoDto>(null, MessageId.AccessDenied);

                var result = await dbQuery
                    .Select(x => new EditPrContractInfoDto
                    {
                        ContractTimeTable = new ContractTimeTable
                        {
                            ContractDuration = x.ContractDuration,
                            DateEnd = x.DateEnd.ToUnixTimestamp(),
                            DateIssued = x.DateIssued.ToUnixTimestamp()
                        },
                        PRContractId = x.Id,
                        ContractType = new ContractForEditType
                        {
                            SelectedCurrency = x.CurrencyType,
                            SelectedDelivery = x.DeliveryLocation,
                            SelectedType = x.PContractType,
                            Tax = x.Tax
                        },
                        PRContractCode = x.PRContractCode,
                        TotalAmount = x.TotalAmount,
                        FinalTotalAmountInLetters = x.FinalTotalAmount.NumberToText(Language.Persian),
                        PRContractStatus = x.PRContractStatus,
                        SupplierId = x.SupplierId,

                        FinalTotalAmount = x.FinalTotalAmount,
                        TaxAmount = (x.TotalAmount * (x.Tax / 100)),
                        ProductGroupId = x.ProductGroupId,
                        ProductGroupTitle = x.ProductGroup.Title,
                        PRContractSubjects = x.PRContractSubjects.Where(c => !c.IsDeleted).Select(c => new ListPRContractSubjectToEditInfoDto
                        {
                            Id=c.RFPItemId,
                            PRContractSubjectId = c.Id,
                            Quantity = c.Quantity,
                            Price = c.UnitPrice,
                            ProductCode = c.Product.ProductCode,
                            ProductUnit = c.Product.Unit,
                            ProductDescription = c.Product.Description,
                            ProductId = c.ProductId,
                            RFPId = c.RFPItem.RFPId,
                            RFPNumber = c.RFPItem.RFP.RFPNumber,
                            ProductTechnicalNumber = c.Product.TechnicalNumber,
                            ProductGroupTitle = c.Product.ProductGroup.Title,
                            OrderQuantity = c.Product.POSubjects.Where(a => a.PO.POStatus != POStatus.Pending && a.PO.PRContractId == prContractId).Sum(v => v.Quantity),
                            ReceiptQuantity = c.Product.POSubjects.Where(a => a.PO.POStatus != POStatus.Pending && a.PO.PRContractId == prContractId).Sum(v => v.ReceiptedQuantity),

                        }).ToList(),
                        Attachments = x.PRContractAttachments.Where(a => !a.IsDeleted).Select(a => new BasePRContractAttachmentDto
                        {
                            PRContractId = a.PRContractId.Value,
                            FileName = a.FileName,
                            FileType = a.FileType,
                            FileSize = a.FileSize,
                            FileSrc = a.FileSrc,
                            Id = a.Id
                        }).ToList(),
                        Supplier = new SupplierMiniInfoDto
                        {
                            Id = x.SupplierId,
                            SupplierCode = x.Supplier.SupplierCode,
                            Address = x.Supplier.Address,
                            Email = x.Supplier.Email,
                            Name = x.Supplier.Name,
                            EconomicCode = x.Supplier.EconomicCode,
                            NationalId = x.Supplier.NationalId,
                            TellPhone = x.Supplier.TellPhone,
                            Logo = _appSettings.ClientHost + ServiceSetting.UploadImagesPath.LogoSmall + x.Supplier.Logo
                        },
                        TermsOfPayments = x.TermsOfPayments.Where(a => !a.IsDeleted).Select(c => new EditTermsOfPaymentDto
                        {
                            Id = c.Id,
                            CreditDuration = c.CreditDuration,
                            PaymentPercentage = c.PaymentPercentage,
                            IsCreditPayment = c.IsCreditPayment,
                            PaymentStep = c.PaymentStep,
                            PRContractId = c.PRContractId.Value,
                        }).ToList(),

                        UserAudit = x.AdderUser != null
                            ? new UserAuditLogDto
                            {
                                AdderUserId = x.AdderUserId,
                                AdderUserName = x.AdderUser.FullName,
                                CreateDate = x.CreatedDate.ToUnixTimestamp(),
                                AdderUserImage = (x.AdderUser.Image != null) ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall +
                                                 x.AdderUser.Image : ""
                            }
                            : null
                    }).FirstOrDefaultAsync();
                List<ListPRContractSubjectToEditInfoDto> prContractSubjects = new List<ListPRContractSubjectToEditInfoDto>();
                foreach (var item in result.PRContractSubjects)
                {
                    if (!prContractSubjects.Any(a => a.ProductId == item.ProductId))
                    {
                        prContractSubjects.Add(new ListPRContractSubjectToEditInfoDto
                        {
                            PRContractSubjectId = item.PRContractSubjectId,
                            Quantity = result.PRContractSubjects.Where(p => p.ProductId == item.ProductId).Sum(p => p.Quantity),
                            Price = item.Price,
                            ProductCode = item.ProductCode,
                            ProductUnit = item.ProductUnit,
                            ProductDescription = item.ProductDescription,
                            ProductId = item.ProductId,
                            RFPId = item.RFPId,
                            RFPNumber = item.RFPNumber,
                            ProductTechnicalNumber = item.ProductTechnicalNumber,
                            ProductGroupTitle = item.ProductGroupTitle,
                            OrderQuantity = result.PRContractSubjects.Where(p => p.ProductId == item.ProductId).Sum(p => p.OrderQuantity),
                            ReceiptQuantity = result.PRContractSubjects.Where(p => p.ProductId == item.ProductId).Sum(p => p.ReceiptQuantity)
                        });
                    }
                }
                PaymentSteps paymentSteps = new PaymentSteps();
                foreach (var item in result.TermsOfPayments)
                {
                    if (item.PaymentStep == TermsOfPaymentStep.ApprovedPo)
                    {
                        paymentSteps.OrderPayment = new PaymentStep { Credit = item.CreditDuration, Percent = Convert.ToInt32(item.PaymentPercentage) };
                    }
                    else if (item.PaymentStep == TermsOfPaymentStep.InvoiceIssue)
                    {
                        paymentSteps.InvoicePayment = new PaymentStep { Credit = item.CreditDuration, Percent = Convert.ToInt32(item.PaymentPercentage) };
                    }
                    else if (item.PaymentStep == TermsOfPaymentStep.Preparation)
                    {
                        paymentSteps.PrepPayment = new PaymentStep { Credit = item.CreditDuration, Percent = Convert.ToInt32(item.PaymentPercentage) };
                    }
                    else if (item.PaymentStep == TermsOfPaymentStep.packing)
                    {
                        paymentSteps.PackPayment = new PaymentStep { Credit = item.CreditDuration, Percent = Convert.ToInt32(item.PaymentPercentage) };
                    }
                }
                result.PaymentSteps = paymentSteps;
                result.PRContractSubjects = prContractSubjects;
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception e)
            {
                return ServiceResultFactory.CreateException(new EditPrContractInfoDto(), e);
            }
        }
        public async Task<ServiceResult<PRContractSubjectViewDto>> GetPRContractSubjectsAndServiceByContractIdAsync(AuthenticateDto authenticate,
            long prContractId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<PRContractSubjectViewDto>(null, MessageId.AccessDenied);

                var dbQuery = _prContractRepository
                    .AsNoTracking()
                    .Where(a => a.Id == prContractId && a.BaseContractCode == authenticate.ContractCode);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<PRContractSubjectViewDto>(null, MessageId.EntityDoesNotExist);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(c => permission.ProductGroupIds.Contains(c.ProductGroupId)))
                    return ServiceResultFactory.CreateError<PRContractSubjectViewDto>(null, MessageId.AccessDenied);

                var result = await dbQuery
                           .Select(a => new PRContractSubjectViewDto
                           {
                               PRContractSubjects = a.PRContractSubjects.Where(c => !c.IsDeleted)
                               .Select(c => new ListPRContractSubjectDto
                               {
                                   PRContractSubjectId = c.Id,
                                   Quantity = c.Quantity,
                                   Price = c.UnitPrice,
                                   ProductCode = c.Product.ProductCode,
                                   ProductUnit = c.Product.Unit,
                                   ProductName = c.Product.Description,
                                   ProductId = c.ProductId,
                                   RFPId = c.RFPItem.RFPId,
                                   RFPNumber = c.RFPItem.RFP.RFPNumber,
                                   TechnicalNumber = c.Product.TechnicalNumber,
                                   ProductGroupTitle = c.Product.ProductGroup.Title,
                                   OrderQuantity = c.Product.POSubjects.Where(a => a.PO.POStatus != POStatus.Pending && a.PO.PRContractId == prContractId).Sum(v => v.Quantity),
                                   ReceiptQuantity = c.Product.POSubjects.Where(a => a.PO.POStatus != POStatus.Pending && a.PO.PRContractId == prContractId).Sum(v => v.ReceiptedQuantity),

                               }).ToList(),
                              
                           }).FirstOrDefaultAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception e)
            {
                return ServiceResultFactory.CreateException<PRContractSubjectViewDto>(null, e);
            }
        }

        public async Task<ServiceResult<List<BasePRContractAttachmentDto>>> GetPRContractAttachmentByPRContractIdAsync(AuthenticateDto authenticate, long prContractId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<BasePRContractAttachmentDto>>(null, MessageId.AccessDenied);

                var dbQuery = _prContractRepository
                    .AsNoTracking()
                    .Where(a => a.Id == prContractId && a.BaseContractCode == authenticate.ContractCode);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<List<BasePRContractAttachmentDto>>(null, MessageId.EntityDoesNotExist);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(c => permission.ProductGroupIds.Contains(c.ProductGroupId)))
                    return ServiceResultFactory.CreateError<List<BasePRContractAttachmentDto>>(null, MessageId.AccessDenied);

                var list = await dbQuery.SelectMany(x => x.PRContractAttachments)
                    .Where(a => !a.IsDeleted).Select(a => new BasePRContractAttachmentDto
                    {
                        PRContractId = a.PRContractId.Value,
                        FileName = a.FileName,
                        FileType = a.FileType,
                        FileSize = a.FileSize,
                        FileSrc = a.FileSrc,
                        Id = a.Id
                    }).ToListAsync();
                return ServiceResultFactory.CreateSuccess(list);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new List<BasePRContractAttachmentDto>(), exception);
            }
        }

        public async Task<ServiceResult<bool>> EditPRContractBaseInfoAsync(AuthenticateDto authenticate, long PrContractId, EditPRContractBaseInfoDto model)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery = _prContractRepository
                    .Where(a => a.Id == PrContractId && a.BaseContractCode == authenticate.ContractCode);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(c => permission.ProductGroupIds.Contains(c.ProductGroupId)))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var prContractModel = await dbQuery
                    .Include(a => a.Supplier)
                    .FirstOrDefaultAsync();

                if (prContractModel == null)
                    return ServiceResultFactory.CreateError<bool>(false, MessageId.EntityDoesNotExist);

                if (model.DateEnd < model.DateIssued)
                    return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                if (model.ContractDuration <= 0)
                    return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                prContractModel.DateIssued = model.DateIssued.UnixTimestampToDateTime().Date;
                prContractModel.DateEnd = model.DateEnd.UnixTimestampToDateTime().Date;
                prContractModel.ContractDuration = model.ContractDuration;

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = prContractModel.BaseContractCode,
                        FormCode = prContractModel.PRContractCode,
                        KeyValue = prContractModel.Id.ToString(),
                        ProductGroupId = prContractModel.ProductGroupId,
                        Description = prContractModel.Supplier.Name,
                        RootKeyValue = prContractModel.Id.ToString(),
                        NotifEvent = NotifEvent.EditPRContract,
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,
                    }, null);
                }

                return ServiceResultFactory.CreateSuccess(true);
            }
            catch (Exception e)
            {
                return ServiceResultFactory.CreateException(false, e);
            }
        }

        

        private async Task<ServiceResult<bool>> EditContractSubjectAsync(PRContract prContractModel, List<EditPRContractSubjectDto> model, string contractCode)
        {


            if (model == null || !model.Any())
                return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

            if (prContractModel == null || prContractModel.PRContractSubjects == null || !prContractModel.PRContractSubjects.Any())
                return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

            var prContractSubjects = prContractModel.PRContractSubjects.Where(a => !a.IsDeleted).ToList();
            if (prContractSubjects == null || !prContractSubjects.Any())
                return ServiceResultFactory.CreateError<bool>(false, MessageId.EntityDoesNotExist);

            if (model.Any(a => a.ProductId <= 0))
                return ServiceResultFactory.CreateError<bool>(false, MessageId.InputDataValidationError);

            if (model.Any(a => a.Price <= 0))
                return ServiceResultFactory.CreateError<bool>(false, MessageId.InputDataValidationError);

            var postedItemProductIds = model.Select(a => a.ProductId).Distinct().ToList();

            var rfpIds = model.Select(a => a.RFPId).ToList();
            var productIds = model.Select(a => a.ProductId).ToList();
            //if (model.Any(a => !(rfpIds.Contains(a.RFPId)&&productIds.Contains(a.ProductId))))
            //    return ServiceResultFactory.CreateError<bool>(false, MessageId.DataInconsistency);

            //var rfpItems = await _rfpItemRepository
            //    .Where(a => !a.IsDeleted && a.IsActive && a.RFPId == prContractModel.RFPId)
            //    .ToListAsync();
            //if (rfpItems == null || rfpItems.Any(c => !prSubjectProductIds.Contains(c.ProductId)))
            //    return ServiceResultFactory.CreateError<bool>(false, MessageId.DataInconsistency);
            var rfpItemModels = await _rfpItemRepository
                    .Where(a => a.IsActive &&
                    !a.IsDeleted &&
                    a.RemainedStock > 0 &&
                    a.RFP.ContractCode == contractCode &&
                    (productIds.Contains(a.ProductId) && rfpIds.Contains(a.RFPId)))
                    .Include(a => a.Product)
                    .Include(a => a.RFP)
                    .ThenInclude(a => a.RFPSuppliers)
                    .Include(a => a.PurchaseRequestItem)
                    .ThenInclude(a => a.PurchaseRequest)
                    .ToListAsync();
            var removeItems = prContractSubjects.Where(a => !a.IsDeleted && !model.Any(b => b.ProductId == a.ProductId)).ToList();
            var addedNewPrContractSubject = model.Where(a => !prContractSubjects.Any(b => !b.IsDeleted && b.ProductId == a.ProductId)).ToList();
            var updateItems = model.Where(a => !addedNewPrContractSubject.Any(b => b.ProductId == a.ProductId) && !removeItems.Any(b => b.ProductId == a.ProductId)).ToList();
            var removeResult = await RemovePRContractSubject(removeItems);
            if (!removeResult.Succeeded)
                return ServiceResultFactory.CreateError<bool>(false, MessageId.EditEntityFailed);
            var addResult = await AddPRItems(prContractModel, addedNewPrContractSubject, rfpItemModels);
            if (!addResult.Succeeded)
                return ServiceResultFactory.CreateError<bool>(false, MessageId.EditEntityFailed);
            var updateResult = await UpdatePRItems(prContractModel, prContractSubjects, updateItems, rfpItemModels);
            if (!updateResult.Succeeded)
                return ServiceResultFactory.CreateError<bool>(false, MessageId.EditEntityFailed);

            //var sumDonedQuantityOfRFPItem = await GetSumQuantityOfDonedRFPItemsAsync(prContractModel.Id, rfpIds,productIds);

            //foreach (var item in prContractSubjects)
            //{
            //    var postedSubject = model.FirstOrDefault(a => a.ProductId == item.ProductId);
            //    if (postedSubject == null)
            //        continue;

            //    UpdateRfpItem(sumDonedQuantityOfRFPItem, item, postedSubject);

            //    item.Quantity = postedSubject.Quantity;
            //    item.RemainedStock = postedSubject.Quantity;
            //    item.UnitPrice = postedSubject.PriceUnit;
            //    item.TotalPrice = postedSubject.PriceTotal;

            //    foreach (var part in item.PRContractSubjectPartLists)
            //    {
            //        if (postedSubject.PRContractSubjectPartLists != null || postedSubject.PRContractSubjectPartLists.Any(a => a.ProductId == part.ProductId))
            //        {
            //            var selectedPart = postedSubject.PRContractSubjectPartLists.First(a => a.ProductId == part.ProductId);
            //            if (selectedPart.Quantity <= 0)
            //                return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

            //            if (item.PRContractSubjectPartLists.Any(c => (c.Quantity % item.Quantity) != 0))
            //                return ServiceResultFactory.CreateError(false, MessageId.PartListCoefficientUseValidation);

            //            part.Quantity = selectedPart.Quantity;
            //            part.CoefficientUse = selectedPart.Quantity / item.Quantity;
            //        }
            //    }
            //}
            return ServiceResultFactory.CreateSuccess(true);
        }

        private async Task<ServiceResult<bool>> EditContractSubjectAsync(PRContract prContractModel, List<AddPRContractSubjectDto> model, string contractCode)
        {


            if (model == null || !model.Any())
                return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

            if (prContractModel == null || prContractModel.PRContractSubjects == null || !prContractModel.PRContractSubjects.Any())
                return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

            var prContractSubjects = prContractModel.PRContractSubjects.Where(a => !a.IsDeleted).ToList();
            if (prContractSubjects == null || !prContractSubjects.Any())
                return ServiceResultFactory.CreateError<bool>(false, MessageId.EntityDoesNotExist);

            if (model.Any(a => a.ProductId <= 0))
                return ServiceResultFactory.CreateError<bool>(false, MessageId.InputDataValidationError);

            if (model.Any(a => a.Price < 0))
                return ServiceResultFactory.CreateError<bool>(false, MessageId.InputDataValidationError);

            var postedItemProductIds = model.Select(a => a.ProductId).Distinct().ToList();

            var rfpIds = model.Select(a => a.RFPId).ToList();
            var productIds = model.Select(a => a.ProductId).ToList();
            //if (model.Any(a => !(rfpIds.Contains(a.RFPId)&&productIds.Contains(a.ProductId))))
            //    return ServiceResultFactory.CreateError<bool>(false, MessageId.DataInconsistency);

            //var rfpItems = await _rfpItemRepository
            //    .Where(a => !a.IsDeleted && a.IsActive && a.RFPId == prContractModel.RFPId)
            //    .ToListAsync();
            //if (rfpItems == null || rfpItems.Any(c => !prSubjectProductIds.Contains(c.ProductId)))
            //    return ServiceResultFactory.CreateError<bool>(false, MessageId.DataInconsistency);
            var rfpItemModels = await _rfpItemRepository
                    .Where(a => a.IsActive &&
                    !a.IsDeleted &&
                    a.RFP.ContractCode == contractCode &&
                    (productIds.Contains(a.ProductId) && rfpIds.Contains(a.RFPId)))
                    .Include(a => a.Product)
                    .Include(a => a.RFP)
                    .ThenInclude(a => a.RFPSuppliers)
                    .Include(a => a.PurchaseRequestItem)
                    .ThenInclude(a => a.PurchaseRequest)
                    .ToListAsync();

            var removeItems = prContractSubjects.Where(a => !a.IsDeleted && !model.Any(b => b.ProductId == a.ProductId)).ToList();
            var addedNewPrContractSubject = model.Where(a => !prContractSubjects.Any(b => !b.IsDeleted && b.ProductId == a.ProductId)).ToList();
            var updateItems = model.Where(a => !addedNewPrContractSubject.Any(b => b.ProductId == a.ProductId) && !removeItems.Any(b => b.ProductId == a.ProductId)).ToList();
            var removeResult = await RemovePRContractSubject(removeItems);
            if (!removeResult.Succeeded)
                return ServiceResultFactory.CreateError<bool>(false, MessageId.EditEntityFailed);
            var addResult = await AddPRItems(prContractModel, addedNewPrContractSubject, rfpItemModels);
            if (!addResult.Succeeded)
                return ServiceResultFactory.CreateError<bool>(false, MessageId.EditEntityFailed);
            var updateResult = await UpdatePRItems(prContractModel, prContractSubjects, updateItems, rfpItemModels);
            if (!updateResult.Succeeded)
                return ServiceResultFactory.CreateError<bool>(false, MessageId.EditEntityFailed);

            //var sumDonedQuantityOfRFPItem = await GetSumQuantityOfDonedRFPItemsAsync(prContractModel.Id, rfpIds,productIds);

            //foreach (var item in prContractSubjects)
            //{
            //    var postedSubject = model.FirstOrDefault(a => a.ProductId == item.ProductId);
            //    if (postedSubject == null)
            //        continue;

            //    UpdateRfpItem(sumDonedQuantityOfRFPItem, item, postedSubject);

            //    item.Quantity = postedSubject.Quantity;
            //    item.RemainedStock = postedSubject.Quantity;
            //    item.UnitPrice = postedSubject.PriceUnit;
            //    item.TotalPrice = postedSubject.PriceTotal;

            //    foreach (var part in item.PRContractSubjectPartLists)
            //    {
            //        if (postedSubject.PRContractSubjectPartLists != null || postedSubject.PRContractSubjectPartLists.Any(a => a.ProductId == part.ProductId))
            //        {
            //            var selectedPart = postedSubject.PRContractSubjectPartLists.First(a => a.ProductId == part.ProductId);
            //            if (selectedPart.Quantity <= 0)
            //                return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

            //            if (item.PRContractSubjectPartLists.Any(c => (c.Quantity % item.Quantity) != 0))
            //                return ServiceResultFactory.CreateError(false, MessageId.PartListCoefficientUseValidation);

            //            part.Quantity = selectedPart.Quantity;
            //            part.CoefficientUse = selectedPart.Quantity / item.Quantity;
            //        }
            //    }
            //}
            return ServiceResultFactory.CreateSuccess(true);
        }
        private ServiceResult<bool> EditPRContractServiceAsync(PRContract prContractModel, List<EditPrContractServiceDto> model)
        {
            if (model == null)
                return ServiceResultFactory.CreateError<bool>(false, MessageId.InputDataValidationError);

            if (model.Any(a => a.Quantity <= 0 || a.PriceUnit < 0))
                return ServiceResultFactory.CreateError<bool>(false, MessageId.InputDataValidationError);

            if (prContractModel == null)
                return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

           
            return ServiceResultFactory.CreateSuccess(true);
        }
       
        private async Task<List<AddPRContractSubjectDto>> GetSumQuantityOfDonedRFPItemsAsync(long prContractId, List<long> rfpIds, List<int> productId)
        {
            var sumDonedQuantityOfRFPItem = await _rfpItemRepository
                 .Where(a => !a.IsDeleted &&
                  (rfpIds.Contains(a.RFPId) && productId.Contains(a.ProductId)))
                 .Select(c => new AddPRContractSubjectDto
                 {
                     ProductId = c.ProductId,
                     RFPId = c.RFPId,
                     Quantity = c.PRContractSubjects.Where(a => a.Id != prContractId).Sum(c => c.Quantity)
                 })
                 .ToListAsync();
            return sumDonedQuantityOfRFPItem;
        }

        private static void UpdateRfpItem(List<AddPRContractSubjectDto> sumDonedQuantityOfRFPItem, PRContractSubject item, EditPRContractSubjectDto postedItem)
        {
            if (sumDonedQuantityOfRFPItem != null && sumDonedQuantityOfRFPItem.Any(a => a.ProductId == item.ProductId))
            {
                var selectedsumDonedQuantityOfRFPItem = sumDonedQuantityOfRFPItem.First(a => a.ProductId == item.ProductId);
                if (selectedsumDonedQuantityOfRFPItem.Quantity < item.RFPItem.Quantity)
                {
                    var realRemainedQuantity = item.RFPItem.Quantity - selectedsumDonedQuantityOfRFPItem.Quantity;
                    if (postedItem.Quantity >= realRemainedQuantity)
                    {
                        item.RFPItem.RemainedStock = 0;
                    }
                    else
                    {
                        item.RFPItem.RemainedStock = realRemainedQuantity - postedItem.Quantity;
                    }
                }
            }
            else
            {
                item.RFPItem.RemainedStock = item.RFPItem.Quantity;
                if (item.RFPItem.RemainedStock > postedItem.Quantity)
                    item.RFPItem.RemainedStock -= postedItem.Quantity;
                else
                    item.RFPItem.RemainedStock = 0;
            }
        }

        public async Task<ServiceResult<bool>> EditPRContractTermsOfPaymentAsync(AuthenticateDto authenticate, long PrContractId, List<EditTermsOfPaymentDto> model)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery = _prContractRepository
                    .Where(a => a.Id == PrContractId && a.BaseContractCode == authenticate.ContractCode)
                    .Include(a => a.Supplier)
                    .Include(a => a.TermsOfPayments)
                    .AsQueryable();

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(c => permission.ProductGroupIds.Contains(c.ProductGroupId)))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var prContractModel = await dbQuery
                    .FirstOrDefaultAsync();

                if (prContractModel == null)
                    return ServiceResultFactory.CreateError<bool>(false, MessageId.EntityDoesNotExist);

                if (prContractModel.PRContractStatus >= PRContractStatus.Active)
                    return ServiceResultFactory.CreateError(false, MessageId.ImpossibleEdit);


                var termsOfPayments = prContractModel.TermsOfPayments != null
                    ? prContractModel.TermsOfPayments.Where(a => !a.IsDeleted).ToList()
                    : null;

                // addnew item
                var addItems = model.Where(a => a.Id <= 0).Select(c => new POTermsOfPayment
                {
                    PRContractId = prContractModel.Id,
                    CreditDuration = c.CreditDuration,
                    IsDeleted = false,
                    PaymentPercentage = c.PaymentPercentage,
                    IsCreditPayment = c.IsCreditPayment,
                    PaymentStep = c.PaymentStep,
                }).ToList();

                if (termsOfPayments == null)
                {
                    _poTermsOfPaymentRepository.AddRange(addItems);
                }
                else
                {

                    if (model.Any(a => a.IsCreditPayment && a.CreditDuration <= 0))
                        return ServiceResultFactory.CreateError<bool>(false, MessageId.InputDataValidationError);

                    var postedItemIds = model.Select(a => a.Id).ToList();

                    var beforeSubjectIds = termsOfPayments.Select(a => a.Id).ToList();
                    var removeItem = termsOfPayments.Where(a => !postedItemIds.Contains(a.Id)).ToList();

                    var updateSubjectIds = model.Where(a => a.Id > 0).Select(c => c.Id).ToList();

                    //if (updateSubjectIds.Any(a => termsOfPayments.Any(c => c.Id != a)))
                    //    return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                    foreach (var item in termsOfPayments.Where(a => updateSubjectIds.Contains(a.Id)))
                    {
                        var postMethod = model.FirstOrDefault(a => a.Id == item.Id);
                        if (postMethod == null) continue;
                        item.CreditDuration = postMethod.CreditDuration;
                        item.PaymentPercentage = postMethod.PaymentPercentage;
                        item.IsCreditPayment = postMethod.IsCreditPayment;
                        item.PaymentStep = postMethod.PaymentStep;
                    }
                    foreach (var item in removeItem)
                    {
                        item.IsDeleted = true;
                    }
                    _poTermsOfPaymentRepository.AddRange(addItems);
                }

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = prContractModel.BaseContractCode,
                        FormCode = prContractModel.PRContractCode,
                        KeyValue = prContractModel.Id.ToString(),
                        RootKeyValue = prContractModel.Id.ToString(),
                        Description = prContractModel.Supplier.Name,
                        ProductGroupId = prContractModel.ProductGroupId,
                        NotifEvent = NotifEvent.EditPRContract,
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,
                    }, null);
                }
                return ServiceResultFactory.CreateSuccess(true);
            }
            catch (Exception e)
            {
                return ServiceResultFactory.CreateException(false, e);
            }
        }

        public async Task<ServiceResult<List<BasePRContractAttachmentDto>>> AddAttachmentAsync(AuthenticateDto authenticate, long PrContractId, List<AddAttachmentDto> attachments)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<BasePRContractAttachmentDto>>(null, MessageId.AccessDenied);

                var dbQuery = _prContractRepository
                    .Where(a => a.Id == PrContractId && a.BaseContractCode == authenticate.ContractCode)
                    .AsQueryable();

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<List<BasePRContractAttachmentDto>>(null, MessageId.EntityDoesNotExist);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(c => permission.ProductGroupIds.Contains(c.ProductGroupId)))
                    return ServiceResultFactory.CreateError<List<BasePRContractAttachmentDto>>(null, MessageId.AccessDenied);

                var prContractModel = await dbQuery.FirstOrDefaultAsync();
                if (prContractModel == null)
                    return ServiceResultFactory.CreateError<List<BasePRContractAttachmentDto>>(null, MessageId.EntityDoesNotExist);

                var prContractAttachments = new List<PAttachment>();

                foreach (var item in attachments)
                {
                    var UploadedFile =
                       await _fileHelper.SaveDocumentFromTemp(item.FileSrc, ServiceSetting.UploadFilePath.PrContract);
                    if (UploadedFile == null)
                        return ServiceResultFactory.CreateError<List<BasePRContractAttachmentDto>>(null, MessageId.UploudFailed);


                    _fileHelper.DeleteDocumentFromTemp(item.FileSrc);
                    prContractAttachments.Add(new PAttachment
                    {
                        PRContractId = PrContractId,
                        FileType = UploadedFile.FileType,
                        FileSize = UploadedFile.FileSize,
                        FileName = item.FileName,
                        FileSrc = item.FileSrc,
                    });
                }

                _prContractAttachmentRepository.AddRange(prContractAttachments);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var result = prContractAttachments.Select(a => new BasePRContractAttachmentDto
                    {
                        Id = a.Id,
                        PRContractId = PrContractId,
                        FileName = a.FileName,
                        FileType = a.FileType,
                        FileSize = a.FileSize,
                        FileSrc = a.FileSrc,
                    }).ToList();
                    return ServiceResultFactory.CreateSuccess(result);
                }

                return ServiceResultFactory.CreateError<List<BasePRContractAttachmentDto>>(null, MessageId.DeleteEntityFailed);
            }
            catch (Exception e)
            {
                return ServiceResultFactory.CreateException(new List<BasePRContractAttachmentDto>(), e);
            }
        }

        public async Task<ServiceResult<bool>> RemoveAttachmentAsync(AuthenticateDto authenticate, long PrContractId, long attachmentId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery = _prContractAttachmentRepository
                    .Where(a => a.Id == attachmentId && a.PRContractId == PrContractId && a.PRContract.BaseContractCode == authenticate.ContractCode)
                    .AsQueryable();

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(c => permission.ProductGroupIds.Contains(c.PRContract.ProductGroupId)))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

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

        public async Task<ServiceResult<bool>> RemoveContractAsync(AuthenticateDto authenticate, long PrContractId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery = _prContractRepository
                    .Include(s => s.PRContractAttachments)
                    .Include(x => x.PRContractSubjects)
                    .Include(c => c.TermsOfPayments)
                    .Where(x => x.Id == PrContractId && x.BaseContractCode == authenticate.ContractCode);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(c => permission.ProductGroupIds.Contains(c.ProductGroupId)))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var selectedPRContract = await dbQuery.FirstOrDefaultAsync();

                if (selectedPRContract == null)
                    return ServiceResultFactory.CreateError<bool>(false, MessageId.EntityDoesNotExist);

                if (selectedPRContract.PRContractStatus >= PRContractStatus.Active)
                    return ServiceResultFactory.CreateError<bool>(false, MessageId.RemoveIsLimited);

                _prContractAttachmentRepository.RemoveRange(selectedPRContract.PRContractAttachments);

                _prContractSubjectRepository.RemoveRange(selectedPRContract.PRContractSubjects);
                _prContractRepository.Remove(selectedPRContract);
                return await _unitOfWork.SaveChangesAsync() > 0
                    ? ServiceResultFactory.CreateSuccess(true)
                    : ServiceResultFactory.CreateError(false, MessageId.DeleteDontAllowedBeforeSubset);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateError(false, MessageId.DeleteDontAllowedBeforeSubset);
            }
        }

        public async Task<DownloadFileDto> DownloadAttachmentAsync(AuthenticateDto authenticate, long prContractId, string FileSrc)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return null;

                var dbQuery = _prContractAttachmentRepository
                    .Where(a => a.PRContractId == prContractId &&
                    a.PRContract.BaseContractCode == authenticate.ContractCode &&
                    a.FileSrc == FileSrc);

                if (dbQuery.Count() == 0)
                    return null;

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(c => permission.ProductGroupIds.Contains(c.PRContract.ProductGroupId)))
                    return null;
                var attachment = await dbQuery.FirstOrDefaultAsync();
                var streamResult = await _fileHelper.DownloadAttachmentDocument(FileSrc, ServiceSetting.FileSection.PRContract, attachment.FileName);
                if (streamResult == null)
                    return null;

                return streamResult;
            }
            catch (Exception exception)
            {
                return null;
            }
        }

        public async Task<DownloadFileDto> DownloadConfirmWorkFlowAttachmentAsync(AuthenticateDto authenticate, long prContractConfirmWorkFlowId, string FileSrc)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return null;

                var dbQuery = _prContractAttachmentRepository
                    .Where(a => a.PrContractConfirmWorkFlowId == prContractConfirmWorkFlowId &&
                    a.FileSrc == FileSrc);

                if (dbQuery.Count() == 0)
                    return null;

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(c => permission.ProductGroupIds.Contains(c.PRContract.ProductGroupId)))
                    return null;
                var attachment = await dbQuery.FirstOrDefaultAsync();
                var streamResult = await _fileHelper.DownloadAttachmentDocument(FileSrc, ServiceSetting.FileSection.PRContract, attachment.FileName);
                if (streamResult == null)
                    return null;

                return streamResult;
            }
            catch (Exception exception)
            {
                return null;
            }
        }

        #region mrp
        public async Task<ServiceResult<bool>> IsThereAnyAvailablePRContractForThisProduct(AuthenticateDto authenticate, int productId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var availableStatus = new List<PRContractStatus> { PRContractStatus.Active };
                var boolResult = await _prContractSubjectRepository
                    .AnyAsync(a => !a.IsDeleted && availableStatus.Contains(a.PRContract.PRContractStatus)
                    && a.ProductId == productId && a.RemainedStock > 0);

                return ServiceResultFactory.CreateSuccess(true);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<List<AddPOByMrpDto>>> GetAvailablePRContractForThisProduct(AuthenticateDto authenticate, int productId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<AddPOByMrpDto>>(null, MessageId.AccessDenied);

                var availableStatus = new List<PRContractStatus> { PRContractStatus.Active };
                var availableContractSubject = await _prContractRepository
                    .AsNoTracking()
                    .Where(a => availableStatus.Contains(a.PRContractStatus) && a.PRContractSubjects.Any(c => c.ProductId == productId && c.RemainedStock > 0))
                    .SelectMany(v => v.PRContractSubjects.Where(d => !d.IsDeleted && d.ProductId == productId)
                    .Select(s => new AddPOByMrpDto
                    {
                        PRContractId = s.PRContractId,
                        PRContractCode = s.PRContract.PRContractCode,
                        SupplierName = s.PRContract.Supplier.Name,
                        PRContractSubjectId = s.Id,
                        ProductId = s.ProductId,
                        RemainedStock = s.RemainedStock,
                        Quantity = s.Quantity,
                        SupplierId = s.PRContract.SupplierId
                    }))
                    .ToListAsync();

                //if (availableContractSubject == null)
                //    return ServiceResultFactory.CreateSuccess(availableContractSubject);

                //availableContractSubject = availableContractSubject.GroupBy(a => new { a.PRContractId, a.ProductId })
                //    .Select(s => new AddPOByMrpDto
                //    {
                //        PRContractId = s.Key.PRContractId,
                //        ProductId = s.Key.ProductId,
                //        PRContractCode = s.First().PRContractCode,
                //        SupplierId = s.First().SupplierId,
                //        SupplierName = s.First().SupplierName,
                //        PRContractSubjectId = s.First().PRContractSubjectId,
                //        RemainedStock = s.Sum(a => a.RemainedStock),
                //        Quantity = s.Sum(a => a.Quantity)
                //    }).ToList();

                return ServiceResultFactory.CreateSuccess(availableContractSubject);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new List<AddPOByMrpDto>(), exception);
            }
        }
        #endregion

        public async Task<ServiceResult<List<RFPItemInfoDto>>> GetRFPItemOfThisPRContractbyprContractIdAsync(AuthenticateDto authenticate, long prContractId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<RFPItemInfoDto>>(null, MessageId.AccessDenied);

                var dbcQuery = _prContractRepository
                    .AsNoTracking()
                    .Where(a => a.Id == prContractId && a.BaseContractCode == authenticate.ContractCode)
                    .Include(a => a.PRContractSubjects)
                    .AsQueryable();

                if (dbcQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<List<RFPItemInfoDto>>(null, MessageId.EntityDoesNotExist);

                if (permission.ProductGroupIds.Any() && !dbcQuery.Any(c => permission.ProductGroupIds.Contains(c.ProductGroupId)))
                    return ServiceResultFactory.CreateError<List<RFPItemInfoDto>>(null, MessageId.AccessDenied);

                var prContractModel = await dbcQuery
                    .FirstOrDefaultAsync();

                if (prContractModel == null)
                    return ServiceResultFactory.CreateError<List<RFPItemInfoDto>>(null, MessageId.EntityDoesNotExist);

                var productIds = new List<int>();
                if (prContractModel.PRContractSubjects != null && prContractModel.PRContractSubjects.Any())
                {

                    productIds = prContractModel.PRContractSubjects
                        .Where(a => !a.IsDeleted)
                        .Select(a => a.ProductId)
                        .Distinct()
                        .ToList();
                }

                //todo
                var dbQuery = _rfpItemRepository
                    .AsNoTracking()
                    .Where(a => a.RFPId == 11)
                    .AsQueryable();

                var result = new List<RFPItemInfoDto>();
                if (productIds != null && productIds.Any())
                {
                    dbQuery = dbQuery
                   .Where(x => (productIds.Contains(x.ProductId) || (!productIds.Contains(x.ProductId) && x.RemainedStock > 0)));
                }
                else
                {
                    dbQuery = dbQuery
                        .Where(x => x.RemainedStock > 0);
                }
                result = await dbQuery.Select(c => new RFPItemInfoDto
                {
                    Id = c.Id,
                    IsActive = c.IsActive,
                    DateEnd = c.DateEnd.ToUnixTimestamp(),
                    DateStart = c.DateStart.ToUnixTimestamp(),
                    PRCode = c.PurchaseRequestItem != null ? c.PurchaseRequestItem.PurchaseRequest.PRCode : "",
                    ProductCode = c.Product.ProductCode,
                    ProductDescription = c.Product.Description,
                    ProductGroupName = c.Product.ProductGroup.Title,
                    ProductId = c.ProductId,
                    ProductTechnicalNumber = c.Product.TechnicalNumber,
                    ProductUnit = c.Product.Unit,
                    Quantity = c.Quantity,
                    //DocumentStatus =
                    //        !c.Product.DocumentProducts.Any(v => !v.Document.IsDeleted && v.Document.IsActive && v.Document.ContractCode == authenticate.ContractCode)
                    //        ? EngineeringDocumentStatus.No
                    //        : c.Product.DocumentProducts.Any(v => !v.Document.IsDeleted && v.Document.IsActive && v.Document.ContractCode == authenticate.ContractCode &&
                    //        (!v.Document.DocumentRevisions.Any(a => !a.IsDeleted) || v.Document.DocumentRevisions.Any(n => !n.IsDeleted && n.RevisionStatus < RevisionStatus.Confirmed)))
                    //        ? EngineeringDocumentStatus.completing
                    //        : EngineeringDocumentStatus.Completed,
                }).ToListAsync();
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<RFPItemInfoDto>>(null, exception);
            }
        }
        public async Task<ServiceResult<int>> GetPendingPRContractBadgeAsync(AuthenticateDto authenticate)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(0, MessageId.AccessDenied);

                var dbQuery = _prContractRepository
                    .AsNoTracking()
                    .Where(x => x.BaseContractCode == authenticate.ContractCode && x.PRContractStatus == PRContractStatus.Register)
                    .AsQueryable();

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(c => permission.ProductGroupIds.Contains(c.ProductGroupId)))
                    return ServiceResultFactory.CreateError(0, MessageId.AccessDenied);

                var result = await dbQuery.CountAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(0, exception);
            }
        }

        public async Task<ServiceResult<List<ListPendingPRContractDto>>> GetPendingForConfirmPrContractstAsync(AuthenticateDto authenticate, PRContractQuery query)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<ListPendingPRContractDto>>(null, MessageId.AccessDenied);

                var dbQuery = _prContractWorkFlowRepository
                    .AsNoTracking()
                    .Where(a => a.PRContract.BaseContractCode == authenticate.ContractCode && a.Status == ConfirmationWorkFlowStatus.Pending && !a.IsDeleted);

                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(c => permission.ProductGroupIds.Contains(c.PRContract.ProductGroupId));

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(x =>
                        x.PRContract.Supplier.Name.Contains(query.SearchText) ||
                        x.PRContract.PRContractCode.Contains(query.SearchText) ||
                        x.PRContract.Supplier.SupplierCode.Contains(query.SearchText) ||
                        x.PRContract.PRContractSubjects.Any(c => c.RFPItem.RFP.RFPNumber.Contains(query.SearchText)));

                if (query.PRContractStatuses != null && query.PRContractStatuses.Any())
                    dbQuery = dbQuery.Where(x => query.PRContractStatuses.Contains(x.PRContract.PRContractStatus));

                if (query.ProductGroupIds != null && query.ProductGroupIds.Any())
                {

                    dbQuery = dbQuery.Where(x => x.PRContract.PRContractSubjects.Any(v => !v.IsDeleted && query.ProductGroupIds.Contains(v.Product.ProductGroupId)));
                }


                if (query.ProductIds != null && query.ProductIds.Any())
                {

                    dbQuery = dbQuery.Where(x => x.PRContract.PRContractSubjects.Any(v => !v.IsDeleted && query.ProductIds.Contains(v.ProductId)));
                }


                if (query.SupplierIds != null && query.SupplierIds.Any())
                {
                    dbQuery = dbQuery.Where(x => query.SupplierIds.Contains(x.PRContract.SupplierId));
                }


                var pageCount = dbQuery.Count();

                var columnsMap = new Dictionary<string, Expression<Func<PrContractConfirmationWorkFlow, object>>>
                {
                    ["Id"] = v => v.PRContract.Id,
                    ["PRContractCode"] = v => v.PRContract.PRContractCode,
                };

                dbQuery = dbQuery.ApplayOrdering(query, columnsMap).ApplayPageing(query);
                var result = await dbQuery.Select(x => new ListPendingPRContractDto
                {
                    PRContractId = x.PRContract.Id,
                    PRContractCode = x.PRContract.PRContractCode,
                    PRContractStatus = x.PRContract.PRContractStatus,
                    DateIssued = x.PRContract.DateIssued.ToUnixTimestamp(),
                    DateEnd = x.PRContract.DateEnd.ToUnixTimestamp(),
                    RFPNumber = "",
                    SupplierCode = x.PRContract.Supplier.SupplierCode,
                    SupplierName = x.PRContract.Supplier.Name,
                    ProductGroupId = x.PRContract.ProductGroupId,
                    ProductGroupTitle = x.PRContract.ProductGroup.Title,
                    ContractType = x.PRContract.PContractType,
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
                    BallInCourtUser = x.PrContractConfirmationWorkFlowUsers.Any() ?
                    x.PrContractConfirmationWorkFlowUsers.Where(a => a.IsBallInCourt)
                    .Select(c => new UserAuditLogDto
                    {
                        AdderUserId = c.UserId,
                        AdderUserName = c.User.FirstName + " " + c.User.LastName,
                        AdderUserImage = c.User.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + c.User.Image : ""
                    }).FirstOrDefault() : new UserAuditLogDto()
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<ListPendingPRContractDto>>(null, exception);
            }
        }



        public async Task<ServiceResult<PrContractConfirmationWorkflowDto>> GetPendingConfirmPrContractByPrContractIdAsync(AuthenticateDto authenticate, long prContractId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<PrContractConfirmationWorkflowDto>(null, MessageId.AccessDenied);

                var dbQuery = _prContractWorkFlowRepository
                     .AsNoTracking()
                     .Where(a => !a.IsDeleted &&
                   a.PrContractId == prContractId &&
                    a.Status == ConfirmationWorkFlowStatus.Pending &&
                     a.PRContract.BaseContractCode == authenticate.ContractCode &&
                     a.PRContract.PRContractStatus == PRContractStatus.Register);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<PrContractConfirmationWorkflowDto>(null, MessageId.EntityDoesNotExist);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.PRContract.ProductGroupId)))
                    return ServiceResultFactory.CreateError<PrContractConfirmationWorkflowDto>(null, MessageId.AccessDenied);

                var result = await dbQuery
                        .Select(x => new PrContractConfirmationWorkflowDto
                        {
                            WorkFlowId=x.PrContractConfirmWorkFlowId,
                            PrContractConfirmWorkFlowId=x.PrContractConfirmWorkFlowId,
                            ConfirmNote = x.ConfirmNote,
                            PrContractConfirmUserAudit = x.AdderUserId != null ? new UserAuditLogDto
                            {
                                CreateDate = x.CreatedDate.ToUnixTimestamp(),
                                AdderUserName = x.AdderUser.FullName,
                                AdderUserImage = x.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + x.AdderUser.Image : ""
                            } : null,
                            Attachments = x.PrContractConfirmationAttachments.Where(m => !m.IsDeleted)
                            .Select(c => new BasePRContractConfirmationAttachmentDto
                            {
                                FileName = c.FileName,
                                FileSize = c.FileSize,
                                FileSrc = c.FileSrc,
                                FileType = c.FileType,
                                PrContractConfirmWorkFlowId = c.PrContractConfirmWorkFlowId
                            }).ToList(),

                            PrContractConfirmationUserWorkFlows = x.PrContractConfirmationWorkFlowUsers.Where(a => a.UserId > 0)
                            .Select(e => new PrContractConfirmationUserWorkFlowDto
                            {
                                PrContractConfirmationWorkFlowUserId = e.PrContractConfirmWorkFlowUserId,
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
                return ServiceResultFactory.CreateException<PrContractConfirmationWorkflowDto>(null, exception);
            }
        }
        public async Task<ServiceResult<PrContractConfirmationWorkflowDto>> GetLastPrContractWorkFlowByPrContractIdAsync(AuthenticateDto authenticate, long prContractId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<PrContractConfirmationWorkflowDto>(null, MessageId.AccessDenied);

                var dbQuery = _prContractWorkFlowRepository
                     .AsNoTracking()
                     .Where(a => !a.IsDeleted &&
                     a.PrContractId == prContractId &&
                     a.PRContract.BaseContractCode == authenticate.ContractCode).OrderByDescending(a => a.CreatedDate);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<PrContractConfirmationWorkflowDto>(null, MessageId.EntityDoesNotExist);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.PRContract.ProductGroupId)))
                    return ServiceResultFactory.CreateError<PrContractConfirmationWorkflowDto>(null, MessageId.AccessDenied);

                var result = await dbQuery
                        .Select(x => new PrContractConfirmationWorkflowDto
                        {
                            WorkFlowId=x.PrContractConfirmWorkFlowId,
                            PrContractConfirmWorkFlowId=x.PrContractConfirmWorkFlowId,
                            ConfirmNote = x.ConfirmNote,
                            PrContractConfirmUserAudit = x.AdderUserId != null ? new UserAuditLogDto
                            {
                                CreateDate = x.CreatedDate.ToUnixTimestamp(),
                                AdderUserName = x.AdderUser.FullName,
                                AdderUserImage = x.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + x.AdderUser.Image : ""
                            } : null,
                            Attachments = x.PrContractConfirmationAttachments.Where(m => !m.IsDeleted)
                            .Select(c => new BasePRContractConfirmationAttachmentDto
                            {
                                FileName = c.FileName,
                                FileSize = c.FileSize,
                                FileSrc = c.FileSrc,
                                FileType = c.FileType,
                                PrContractConfirmWorkFlowId = c.PrContractConfirmWorkFlowId
                            }).ToList(),

                            PrContractConfirmationUserWorkFlows = x.PrContractConfirmationWorkFlowUsers.Where(a => a.UserId > 0)
                            .Select(e => new PrContractConfirmationUserWorkFlowDto
                            {
                                PrContractConfirmationWorkFlowUserId = e.PrContractConfirmWorkFlowUserId,
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
                return ServiceResultFactory.CreateException<PrContractConfirmationWorkflowDto>(null, exception);
            }
        }

        public async Task<ServiceResult<List<ListPendingPRContractDto>>> SetUserConfirmOwnPrContractTaskAsync(AuthenticateDto authenticate, long prContractId, AddPrContractConfirmationAnswerDto model)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<ListPendingPRContractDto>>(null, MessageId.AccessDenied);

                var dbQuery = _prContractWorkFlowRepository
                     .Where(a => !a.IsDeleted &&
                    a.PrContractId == prContractId &&
                    a.Status == ConfirmationWorkFlowStatus.Pending &&
                    a.PRContract.BaseContractCode == authenticate.ContractCode &&
                    a.PRContract.PRContractStatus == PRContractStatus.Register)
                     .Include(a => a.PrContractConfirmationAttachments)
                     .Include(a => a.PrContractConfirmationWorkFlowUsers)
                     .ThenInclude(c => c.User)
                     .Include(a => a.PRContract)
                     .ThenInclude(a => a.PRContractSubjects)
                     .ThenInclude(a => a.RFPItem)
                     .Include(a => a.PRContract)
                     .ThenInclude(a => a.Supplier)
                     .Include(a => a.PRContract)
                     .ThenInclude(a => a.TermsOfPayments)
                     .AsQueryable();


                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<List<ListPendingPRContractDto>>(null, MessageId.EntityDoesNotExist);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.PRContract.ProductGroupId)))
                    return ServiceResultFactory.CreateError<List<ListPendingPRContractDto>>(null, MessageId.AccessDenied);

                var confirmationModel = await dbQuery.FirstOrDefaultAsync();

                if (confirmationModel == null)
                    return ServiceResultFactory.CreateError<List<ListPendingPRContractDto>>(null, MessageId.EntityDoesNotExist);

                if (confirmationModel.PrContractConfirmationWorkFlowUsers == null && !confirmationModel.PrContractConfirmationWorkFlowUsers.Any())
                    return ServiceResultFactory.CreateError<List<ListPendingPRContractDto>>(null, MessageId.DataInconsistency);

                if (!confirmationModel.PrContractConfirmationWorkFlowUsers.Any(c => c.UserId == authenticate.UserId && c.IsBallInCourt))
                    return ServiceResultFactory.CreateError<List<ListPendingPRContractDto>>(null, MessageId.AccessDenied);


                var userBallInCourtModel = confirmationModel.PrContractConfirmationWorkFlowUsers.FirstOrDefault(a => a.IsBallInCourt && a.UserId == authenticate.UserId);
                userBallInCourtModel.DateEnd = DateTime.UtcNow;
                if (model.IsAccept)
                {
                    userBallInCourtModel.IsBallInCourt = false;
                    userBallInCourtModel.IsAccept = true;
                    userBallInCourtModel.Note = model.Note;
                    if (!confirmationModel.PrContractConfirmationWorkFlowUsers.Any(a => a.IsAccept == false))
                    {
                        confirmationModel.Status = ConfirmationWorkFlowStatus.Confirm;
                        confirmationModel.PRContract.PRContractStatus = PRContractStatus.Active;
                        var serviceResult = await _poService.AddPOToPendingByPRContractAsync(confirmationModel.PRContract);

                        if (!serviceResult.Succeeded)
                            return ServiceResultFactory.CreateError<List<ListPendingPRContractDto>>(null, serviceResult.Messages.FirstOrDefault().Message);

                        _poRepository.AddRange(serviceResult.Result);
                        foreach (var item in confirmationModel.PrContractConfirmationAttachments)
                        {
                            if (item.IsDeleted)
                                continue;
                            _prContractAttachmentRepository.Add(new PAttachment
                            {
                                PRContractId = confirmationModel.PrContractId,
                                FileName = item.FileName,
                                FileSize = item.FileSize,
                                FileSrc = item.FileSrc,
                                FileType = item.FileType,
                            });
                        }

                    }
                    else
                    {
                        var nextBallInCourtModel = confirmationModel.PrContractConfirmationWorkFlowUsers.Where(a => !a.IsAccept)
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

                    confirmationModel.PRContract.PRContractStatus = PRContractStatus.Rejected;
                    //foreach (var item in confirmationModel.PRContract.PRContractSubjects)
                    //{

                    //    item.RFPItem.RemainedStock += item.Quantity;
                    //}
                }

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    await _scmLogAndNotificationService.SetDonedNotificationAsync(authenticate.UserId, confirmationModel.PRContract.BaseContractCode, confirmationModel.PRContract.Id.ToString(), NotifEvent.ConfirmPRContract);
                    var productGourp = await _productGroupRepository.FirstOrDefaultAsync(a => a.Id == confirmationModel.PRContract.ProductGroupId);
                    int? userId = null;
                    if (model.IsAccept && confirmationModel.PRContract.PRContractStatus != PRContractStatus.Active)
                    {
                        userId = confirmationModel.PrContractConfirmationWorkFlowUsers.First(a => a.IsBallInCourt).UserId;
                    }
                    if(confirmationModel.PRContract.PRContractStatus == PRContractStatus.Active)
                    {
                        await SendNotifOnConfirmContract(authenticate, confirmationModel.PRContract, confirmationModel.PRContract.Supplier.Name);
                    }
                    await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = confirmationModel.PRContract.BaseContractCode,
                        FormCode = confirmationModel.PRContract.PRContractCode,
                        KeyValue = confirmationModel.PRContract.Id.ToString(),
                        NotifEvent = (model.IsAccept) ? NotifEvent.ConfirmPRContract : NotifEvent.RejectPRContract,
                        ProductGroupId = confirmationModel.PRContract.ProductGroupId,
                        RootKeyValue = confirmationModel.PRContract.Id.ToString(),
                        Message = (productGourp != null) ? productGourp.Title : "",
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,
                        Description=confirmationModel.PRContract.Supplier.Name

                    },
                     confirmationModel.PRContract.ProductGroupId
                    , NotifEvent.ConfirmPRContract,
                     userId
                     );

                    //await SendingLogAndNotificationTaskOnUserConfirmREvisionAsync(authenticate, confirmationModel, userBallInCourtModel);
                    //if (confirmationModel.Status == ConfirmationWorkFlowStatus.Reject)
                    //{
                    //    BackgroundJob.Enqueue(() => SendEmailForRejectDocumentRevisionAsync(authenticate, confirmationModel, model.Note));
                    //}
                    var result = await GetPendingForConfirmPrContractstAsync(authenticate, new PRContractQuery { Page = 1, PageSize = 999 });
                    if (!result.Succeeded)
                        return ServiceResultFactory.CreateError<List<ListPendingPRContractDto>>(null, MessageId.OperationFailed);
                    else
                        return ServiceResultFactory.CreateSuccess(result.Result);
                }
                return ServiceResultFactory.CreateError<List<ListPendingPRContractDto>>(null, MessageId.SaveFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<ListPendingPRContractDto>>(null, exception);
            }
        }
        public async Task<ServiceResult<bool>> CancelPrContractAsync(AuthenticateDto authenticate, long prContractId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery = _prContractRepository
                    .Include(a => a.PRContractSubjects)
                    .ThenInclude(a => a.RFPItem)
                    .ThenInclude(a => a.PurchaseRequestItem)
                    .ThenInclude(a => a.MrpItem)

                    .Include(a => a.POs)
                    .Where(x => x.PRContractStatus != PRContractStatus.Canceled &&
                    x.PRContractStatus != PRContractStatus.Compeleted &&
                    x.BaseContractCode == authenticate.ContractCode &&
                    x.Id == prContractId);
                if (await dbQuery.CountAsync() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);
                var productGroupId = await dbQuery.Select(a => a.ProductGroupId).FirstAsync();
                if (permission.ProductGroupIds.Any() && !permission.ProductGroupIds.Contains(productGroupId))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                if (await dbQuery.AnyAsync(a => a.POs.Any(b => !b.IsDeleted && !String.IsNullOrEmpty(b.POCode))))
                    return ServiceResultFactory.CreateError(false, MessageId.PrContractCantCancelIfHasPo);
                var prContract = await dbQuery.FirstAsync();
                foreach (var po in prContract.POs)
                    po.IsDeleted = true;
                var cancelRFPItems = await CancelPrContractSubjectAsync(prContract.PRContractSubjects.ToList());
                if (!cancelRFPItems.Succeeded)
                    return ServiceResultFactory.CreateError(false, MessageId.EditEntityFailed);
                prContract.PRContractSubjects = cancelRFPItems.Result;
                prContract.PRContractStatus = PRContractStatus.Canceled;


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
        public async Task<ServiceResult<List<UserMentionDto>>> GetConfirmationUserListAsync(AuthenticateDto authenticate, int productGroupId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<UserMentionDto>>(null, MessageId.AccessDenied);
                if (permission.ProductGroupIds.Any() && !permission.ProductGroupIds.Contains(productGroupId))
                    return ServiceResultFactory.CreateError<List<UserMentionDto>>(null, MessageId.AccessDenied);

                var roles = new List<string> { SCMRole.PrContractMng };
                var list = await _authenticationServices.GetAllUserHasAccessPurchaseAsync(authenticate.ContractCode, roles, productGroupId);

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
        #region Report
        public async Task<ServiceResult<List<ContractPOSubjectReportDto>>> GetReportPOSubjectbyprContractIdAsync(AuthenticateDto authenticate, long prContractId)
        {

            var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
            if (!permission.HasPermission)
                return ServiceResultFactory.CreateError<List<ContractPOSubjectReportDto>>(null, MessageId.AccessDenied);

            var dbQuery = _poSubjectRepository
                .Where(a =>!a.PO.IsDeleted && a.PO.POStatus >= POStatus.Approved &&
                a.PO.BaseContractCode == authenticate.ContractCode &&
                a.PO.PRContractId == prContractId);

            if (dbQuery.Count() == 0)
                return ServiceResultFactory.CreateError<List<ContractPOSubjectReportDto>>(null, MessageId.EntityDoesNotExist);

            if (permission.ProductGroupIds.Any())
                dbQuery = dbQuery.Where(c => permission.ProductGroupIds.Contains(c.Product.ProductGroupId));

            var result = await dbQuery
                .Select(c => new ContractPOSubjectReportDto
                {
                    POId = c.POId.Value,
                    POCode = c.PO.POCode,
                    MRPCode = (c.MrpItem.BomProduct.IsRequiredMRP)?c.MrpItem.Mrp.MrpNumber:"",
                    ProductCode = c.Product.ProductCode,
                    ProductId = c.ProductId,
                    ProductName = c.Product.Description,
                    Quantity = c.Quantity,
                    ReceiptQuantity = c.ReceiptedQuantity,
                    DateRequired = c.MrpItem.DateEnd.ToUnixTimestamp(),
                    UserAudit = c.PO.AdderUser != null ? new UserAuditLogDto
                    {
                        AdderUserName = c.PO.AdderUser.FullName,
                        AdderUserId = c.PO.AdderUserId,
                        CreateDate = c.PO.CreatedDate.ToUnixTimestamp(),
                        AdderUserImage = c.PO.AdderUser.Image
                    } : null
                }).ToListAsync();
            return ServiceResultFactory.CreateSuccess(result);
        }

        #endregion

        private void UpdatePurchaseRequestStatus(PurchaseRequest purchaseRequest)
        {
            if (!purchaseRequest.PurchaseRequestItems.Any(a => !a.IsDeleted))
                purchaseRequest.PRStatus = PRStatus.PRContractSigned;
        }



        private bool IsCorrectDeliveryLocationByPContractType(PContractType contractType, POIncoterms DeliveryLocation)
        {
            var correctStatusInternal = new List<POIncoterms>
                {
                POIncoterms.SupplierLocation,
                POIncoterms.CompanyLocation,
                };
            var correctStatusForeign = new List<POIncoterms>
            {
                POIncoterms.SupplierLocation,
                POIncoterms.OriginPort,
                POIncoterms.DestinationPort,
                POIncoterms.CompanyLocation,
            };

            if (contractType == PContractType.Internal)
                return correctStatusInternal.Contains(DeliveryLocation);
            else if (contractType == PContractType.Foreign)
                return correctStatusForeign.Contains(DeliveryLocation);
            else
                return false;
        }

        private async Task<ServiceResult<PRContract>> AddPRContractSubjectAsync(PRContract prContractModel, List<AddPRContractSubjectDto> prContractSubjects, List<RFPItems> rfpItems)
        {
            prContractModel.PRContractSubjects = new List<PRContractSubject>();

            if (prContractSubjects == null || !prContractSubjects.Any() || prContractSubjects.Any(c => c.Quantity == 0))
                return ServiceResultFactory.CreateError<PRContract>(null, MessageId.DataInconsistency);

            var rfpItemProductIds = rfpItems.Select(a => a.ProductId).ToList();
            if (prContractSubjects.Any(a => !rfpItemProductIds.Contains(a.ProductId)))
                return ServiceResultFactory.CreateError<PRContract>(null, MessageId.DataInconsistency);

            decimal neededQuantity = 0;
            decimal requiredQuantity = 0;
            foreach (var item in prContractSubjects)
            {

                var selectedRFPItems = rfpItems.Where(a => a.IsActive && !a.IsDeleted && a.ProductId == item.ProductId && a.RFPId == item.RFPId).ToList();
                if (selectedRFPItems == null)
                    return ServiceResultFactory.CreateError<PRContract>(null, MessageId.DataInconsistency);
                neededQuantity = item.Quantity;

                foreach (var rfpItem in selectedRFPItems)
                {
                    if (neededQuantity > 0)
                    {
                        requiredQuantity = 0;
                        var mrpItem = await _mrpItemRepository
                                                .Where(a => a.ProductId == item.ProductId &&
                                                a.MrpId == rfpItem.PurchaseRequestItem.PurchaseRequest.MrpId && !a.IsDeleted)
                                                .FirstOrDefaultAsync();

                        if (mrpItem == null)
                            return ServiceResultFactory.CreateError<PRContract>(null, MessageId.DataInconsistency);

                        if (mrpItem.MrpItemStatus < MrpItemStatus.PRC)
                            mrpItem.MrpItemStatus = MrpItemStatus.PRC;

                        if (rfpItem.RemainedStock > neededQuantity)
                        {
                            rfpItem.RemainedStock -= neededQuantity;
                            requiredQuantity = neededQuantity;
                            neededQuantity = 0;
                        }
                        else 
                        {
                            var index = selectedRFPItems.IndexOf(rfpItem);
                            if (index == selectedRFPItems.Count - 1)
                            {
                                
                                requiredQuantity = neededQuantity;
                                rfpItem.RemainedStock = 0;
                                neededQuantity =0;
                            }
                            else
                            {
                                neededQuantity -= rfpItem.RemainedStock;
                                requiredQuantity = rfpItem.RemainedStock;
                                rfpItem.RemainedStock = 0;
                            }
                            



                        }
                        prContractModel.PRContractSubjects.Add(new PRContractSubject
                        {
                            ProductId = item.ProductId,
                            TotalPrice = item.PriceTotal,
                            UnitPrice = item.Price,
                            Quantity = requiredQuantity,
                            ReservedStock = 0,
                            RFPItemId = rfpItem.Id,
                            RemainedStock = requiredQuantity,
                            RemainedQuantityToInvoice = requiredQuantity,
                        });
                    }
                    else
                        break;

                }
                

            }

            return ServiceResultFactory.CreateSuccess(prContractModel);
        }

        private PRContract AddPRContractTermsOfPayments(PRContract prContractModel, PaymentSteps termsOfPayments)
        {
            if(prContractModel.TermsOfPayments!=null && prContractModel.TermsOfPayments.Any())
            {
                foreach (var item in prContractModel.TermsOfPayments)
                    item.IsDeleted = true;
            }
            prContractModel.TermsOfPayments = new List<POTermsOfPayment>();

            if (termsOfPayments.OrderPayment.Percent > 0)
            {
                prContractModel.TermsOfPayments.Add(new POTermsOfPayment
                {
                    CreditDuration = termsOfPayments.OrderPayment.Credit,
                    IsDeleted = false,
                    PaymentPercentage = termsOfPayments.OrderPayment.Percent,
                    IsCreditPayment = (termsOfPayments.OrderPayment.Credit) > 0 ? true : false,
                    PaymentStep = TermsOfPaymentStep.ApprovedPo,
                });
            }

            if (termsOfPayments.PrepPayment.Percent > 0)
            {
                prContractModel.TermsOfPayments.Add(new POTermsOfPayment
                {
                    CreditDuration = termsOfPayments.PrepPayment.Credit,
                    IsDeleted = false,
                    PaymentPercentage = termsOfPayments.PrepPayment.Percent,
                    IsCreditPayment = (termsOfPayments.PrepPayment.Credit) > 0 ? true : false,
                    PaymentStep = TermsOfPaymentStep.Preparation,
                });
            }
            if (termsOfPayments.PackPayment.Percent > 0)
            {
                prContractModel.TermsOfPayments.Add(new POTermsOfPayment
                {
                    CreditDuration = termsOfPayments.PackPayment.Credit,
                    IsDeleted = false,
                    PaymentPercentage = termsOfPayments.PackPayment.Percent,
                    IsCreditPayment = (termsOfPayments.PackPayment.Credit) > 0 ? true : false,
                    PaymentStep = TermsOfPaymentStep.packing,
                });
            }
            if (termsOfPayments.InvoicePayment.Percent > 0)
            {
                prContractModel.TermsOfPayments.Add(new POTermsOfPayment
                {
                    CreditDuration = termsOfPayments.InvoicePayment.Credit,
                    IsDeleted = false,
                    PaymentPercentage = termsOfPayments.InvoicePayment.Percent,
                    IsCreditPayment = (termsOfPayments.InvoicePayment.Credit) > 0 ? true : false,
                    PaymentStep = TermsOfPaymentStep.InvoiceIssue,
                });
            }
            return prContractModel;
        }

        private async Task<ServiceResult<PRContract>> AddPRContractAttachment(PRContract prContractModel, List<AddAttachmentDto> attachment)
        {
            prContractModel.PRContractAttachments = new List<PAttachment>();

            foreach (var item in attachment)
            {
                var UploadedFile =
                    await _fileHelper.SaveDocumentFromTemp(item.FileSrc, ServiceSetting.UploadFilePath.PrContract);
                if (UploadedFile == null)
                    return ServiceResultFactory.CreateError<PRContract>(null, MessageId.UploudFailed);

                _fileHelper.DeleteDocumentFromTemp(item.FileSrc);
                prContractModel.PRContractAttachments.Add(new PAttachment
                {
                    FileType = UploadedFile.FileType,
                    FileSize = UploadedFile.FileSize,
                    FileName = item.FileName,
                    FileSrc = item.FileSrc,
                });
            }

            return ServiceResultFactory.CreateSuccess(prContractModel);
        }

        private async Task<ServiceResult<bool>> RemovePRContractSubject(List<PRContractSubject> prContracSubjects)
        {
            try
            {
                foreach (var item in prContracSubjects)
                {
                    var mrpItemId = item.RFPItem.PurchaseRequestItem.MrpItemId;
                    item.RFPItem.RemainedStock += item.Quantity;
                    if (!await _mrpItemRepository.AnyAsync(a => !a.IsDeleted && a.Id == mrpItemId && a.PurchaseRequestItems.Any(b => !b.IsDeleted && b.RFPItems.Any(c => !c.IsDeleted && c.PRContractSubjects.Any(d => !d.IsDeleted)))))
                        item.RFPItem.PurchaseRequestItem.MrpItem.MrpItemStatus = MrpItemStatus.RFPDone;
                    item.IsDeleted = true;
                }

                return ServiceResultFactory.CreateSuccess(true);
            }
            catch (Exception ex)
            {
                return ServiceResultFactory.CreateException(false, ex);
            }

        }

        private async Task<ServiceResult<bool>> AddPRItems(PRContract prContract, List<EditPRContractSubjectDto> postedPrContractSubjects, List<RFPItems> rfpItems)
        {
            try
            {
                decimal neededQuantity = 0;
                foreach (var item in postedPrContractSubjects)
                {
                    var currentRFPItems = rfpItems.Where(a => !a.IsDeleted && a.ProductId == item.ProductId).ToList();
                    if (currentRFPItems.Sum(a => a.RemainedStock) < item.Quantity)
                        return ServiceResultFactory.CreateError(false, MessageId.QuantityCantGreaterThanRemaind);
                    neededQuantity = item.Quantity;
                    decimal rfpQuantity = 0;
                    foreach (var rfpItem in currentRFPItems)
                    {
                        var mrpItem = await _mrpItemRepository
                                                .Where(a => a.ProductId == item.ProductId &&
                                                a.MrpId == rfpItem.PurchaseRequestItem.PurchaseRequest.MrpId && !a.IsDeleted)
                                                .FirstOrDefaultAsync();

                        if (mrpItem == null)
                            return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                        if (mrpItem.MrpItemStatus < MrpItemStatus.PRC)
                            mrpItem.MrpItemStatus = MrpItemStatus.PRC;
                        if (rfpItem.RemainedStock >= neededQuantity)
                        {
                            rfpQuantity = neededQuantity;
                            rfpItem.RemainedStock -= neededQuantity;
                            neededQuantity = 0;
                        }
                        else
                        {
                            rfpQuantity = rfpItem.RemainedStock;
                            neededQuantity -= rfpItem.RemainedStock;
                            rfpItem.RemainedStock = 0;
                        }


                        prContract.PRContractSubjects.Add(new PRContractSubject
                        {
                            ProductId = item.ProductId,
                            TotalPrice = item.PriceTotal,
                            UnitPrice = item.Price,
                            Quantity = rfpQuantity,
                            ReservedStock = 0,
                            RFPItemId = rfpItem.Id,
                            RemainedStock = rfpQuantity,
                            RemainedQuantityToInvoice = rfpQuantity,
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
        private async Task<ServiceResult<bool>> AddPRItems(PRContract prContract, List<AddPRContractSubjectDto> postedPrContractSubjects, List<RFPItems> rfpItems)
        {
            try
            {
                decimal neededQuantity = 0;
                foreach (var item in postedPrContractSubjects)
                {
                    var currentRFPItems = rfpItems.Where(a => !a.IsDeleted && a.ProductId == item.ProductId).ToList();
                    if (currentRFPItems.Sum(a => a.RemainedStock) < item.Quantity)
                        return ServiceResultFactory.CreateError(false, MessageId.QuantityCantGreaterThanRemaind);
                    neededQuantity = item.Quantity;
                    decimal rfpQuantity = 0;
                    foreach (var rfpItem in currentRFPItems)
                    {
                        var mrpItem = await _mrpItemRepository
                                                .Where(a => a.ProductId == item.ProductId &&
                                                a.MrpId == rfpItem.PurchaseRequestItem.PurchaseRequest.MrpId && !a.IsDeleted)
                                                .FirstOrDefaultAsync();

                        if (mrpItem == null)
                            return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                        if (mrpItem.MrpItemStatus < MrpItemStatus.PRC)
                            mrpItem.MrpItemStatus = MrpItemStatus.PRC;
                        if (rfpItem.RemainedStock >= neededQuantity)
                        {
                            rfpQuantity = neededQuantity;
                            rfpItem.RemainedStock -= neededQuantity;
                            neededQuantity = 0;
                        }
                        else
                        {
                            rfpQuantity = rfpItem.RemainedStock;
                            neededQuantity -= rfpItem.RemainedStock;
                            rfpItem.RemainedStock = 0;
                        }


                        prContract.PRContractSubjects.Add(new PRContractSubject
                        {
                            ProductId = item.ProductId,
                            TotalPrice = item.PriceTotal,
                            UnitPrice = item.Price,
                            Quantity = rfpQuantity,
                            ReservedStock = 0,
                            RFPItemId = rfpItem.Id,
                            RemainedStock = rfpQuantity,
                            RemainedQuantityToInvoice = rfpQuantity,
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
        private async Task<ServiceResult<bool>> UpdatePRItems(PRContract prContract, List<PRContractSubject> prContractSubjects, List<EditPRContractSubjectDto> postedPrContractSubjects, List<RFPItems> rfpItems)
        {
            decimal neededQuantity = 0;
            List<PRContractSubject> newPrContractSubjects = new List<PRContractSubject>();
            foreach (var item in postedPrContractSubjects)
            {
                neededQuantity = item.Quantity;
                var checkQueantity = rfpItems.Where(a => a.RFPId == prContract.PRContractSubjects.First(a => !a.IsDeleted).RFPItem.RFPId && a.ProductId == item.ProductId);
                if (item.Quantity > checkQueantity.Sum(a => a.RemainedStock) + prContractSubjects.Where(a => !a.IsDeleted && a.ProductId == item.ProductId).Sum(a => a.Quantity))
                    return ServiceResultFactory.CreateError(false, MessageId.QuantityCantGreaterThanRemaind);
                foreach (var prContractSubject in prContractSubjects.Where(a => !a.IsDeleted && a.ProductId == item.ProductId))
                {


                    if (neededQuantity >= prContractSubject.Quantity)
                    {
                        if (neededQuantity <= prContractSubject.Quantity + prContractSubject.RFPItem.RemainedStock)
                        {
                            prContractSubject.RFPItem.RemainedStock += prContractSubject.Quantity;
                            prContractSubject.Quantity = neededQuantity;
                            prContractSubject.RFPItem.RemainedStock -= prContractSubject.Quantity;
                            neededQuantity -= prContractSubject.Quantity;
                        }
                        else
                        {
                            neededQuantity -= prContractSubject.Quantity;
                        }
                        


                    }
                    else if (neededQuantity < prContractSubject.Quantity)
                    {
                        prContractSubject.RFPItem.RemainedStock += prContractSubject.Quantity;
                        prContractSubject.Quantity = neededQuantity;
                        prContractSubject.RFPItem.RemainedStock -= prContractSubject.Quantity;

                        if (prContractSubject.Quantity == 0)
                        {
                            var mrpItemId = prContractSubject.RFPItem.PurchaseRequestItem.MrpItemId;
                            if (!await _mrpItemRepository.AnyAsync(a => !a.IsDeleted && a.Id == mrpItemId && a.PurchaseRequestItems.Any(b => !b.IsDeleted && b.RFPItems.Any(c => !c.IsDeleted && c.PRContractSubjects.Any(d => !d.IsDeleted)))))
                                prContractSubject.RFPItem.PurchaseRequestItem.MrpItem.MrpItemStatus = MrpItemStatus.RFPDone;
                            prContractSubject.IsDeleted = true;

                        }


                        neededQuantity = 0;
                    }

                }
                if (neededQuantity > 0)
                {
                    //var hasRfp = prItems.Any(a => !a.IsDeleted && a.ProductId == item.ProductId && a.RFPItems.Any(a => !a.IsDeleted && a.IsActive));
                    //var rfpId = hasRfp ? prItems.First(a => !a.IsDeleted && a.ProductId == item.ProductId && a.RFPItems.Any(a => !a.IsDeleted && a.IsActive)).RFPItems.First().RFPId : 0;
                    foreach (var rfpItem in rfpItems.Where(a => !a.IsDeleted && a.ProductId == item.ProductId && a.RemainedStock > 0))
                    {
                        if (rfpItem.RemainedStock >= neededQuantity)
                        {
                            newPrContractSubjects.Add(new PRContractSubject
                            {
                                ProductId = item.ProductId,
                                TotalPrice = item.PriceTotal,
                                UnitPrice = item.Price,
                                Quantity = neededQuantity,
                                ReservedStock = 0,
                                RFPItemId = rfpItem.Id,
                                RemainedStock = neededQuantity,
                                RemainedQuantityToInvoice = neededQuantity,
                            });
                            rfpItem.RemainedStock -= neededQuantity;
                            rfpItem.PurchaseRequestItem.MrpItem.MrpItemStatus = (rfpItem.PurchaseRequestItem.MrpItem.MrpItemStatus < MrpItemStatus.PRC) ? MrpItemStatus.PRC : rfpItem.PurchaseRequestItem.MrpItem.MrpItemStatus;
                            neededQuantity = 0;
                        }
                        else if (neededQuantity > 0 && rfpItem.RemainedStock < neededQuantity)
                        {

                            newPrContractSubjects.Add(new PRContractSubject
                            {
                                ProductId = item.ProductId,
                                TotalPrice = item.PriceTotal,
                                UnitPrice = item.Price,
                                Quantity = rfpItem.RemainedStock,
                                ReservedStock = 0,
                                RFPItemId = rfpItem.Id,
                                RemainedStock = rfpItem.RemainedStock,
                                RemainedQuantityToInvoice = rfpItem.RemainedStock,
                            });
                            neededQuantity -= rfpItem.RemainedStock;
                            rfpItem.PurchaseRequestItem.MrpItem.MrpItemStatus = (rfpItem.PurchaseRequestItem.MrpItem.MrpItemStatus < MrpItemStatus.PRC) ? MrpItemStatus.PRC : rfpItem.PurchaseRequestItem.MrpItem.MrpItemStatus;
                            rfpItem.RemainedStock = 0;
                        }
                        else
                            break;
                    }
                }
            }

            if (newPrContractSubjects != null && newPrContractSubjects.Any())
                _prContractSubjectRepository.AddRange(newPrContractSubjects);
            return ServiceResultFactory.CreateSuccess(true);

        }
        private async Task<ServiceResult<bool>> UpdatePRItems(PRContract prContract, List<PRContractSubject> prContractSubjects, List<AddPRContractSubjectDto> postedPrContractSubjects, List<RFPItems> rfpItems)
        {
            decimal neededQuantity = 0;
            List<PRContractSubject> newPrContractSubjects = new List<PRContractSubject>();
            foreach (var item in postedPrContractSubjects)
            {
                neededQuantity = item.Quantity;
                var checkQueantity = rfpItems.Where(a => a.RFPId == prContract.PRContractSubjects.First(a => !a.IsDeleted).RFPItem.RFPId && a.ProductId == item.ProductId);
                if (item.Quantity > checkQueantity.Sum(a => a.RemainedStock) + prContractSubjects.Where(a => !a.IsDeleted && a.ProductId == item.ProductId).Sum(a => a.Quantity))
                    return ServiceResultFactory.CreateError(false, MessageId.QuantityCantGreaterThanRemaind);
                foreach (var prContractSubject in prContractSubjects.Where(a => !a.IsDeleted && a.ProductId == item.ProductId))
                {


                    if (neededQuantity >= prContractSubject.Quantity)
                    {
                        if (neededQuantity <= prContractSubject.Quantity + prContractSubject.RFPItem.RemainedStock)
                        {
                            prContractSubject.RFPItem.RemainedStock += prContractSubject.Quantity;
                            prContractSubject.Quantity = neededQuantity;
                            prContractSubject.UnitPrice = item.Price;
                            prContractSubject.TotalPrice = item.Price* prContractSubject.Quantity;
                            prContractSubject.RFPItem.RemainedStock -= prContractSubject.Quantity;
                            neededQuantity -= prContractSubject.Quantity;
                        }
                        else
                        {
                            neededQuantity -= prContractSubject.Quantity;

                        }
                        


                    }
                    else if (neededQuantity < prContractSubject.Quantity)
                    {
                        prContractSubject.RFPItem.RemainedStock += prContractSubject.Quantity;
                        prContractSubject.Quantity = neededQuantity;
                        prContractSubject.RFPItem.RemainedStock -= prContractSubject.Quantity;
                        prContractSubject.UnitPrice = item.Price;
                        prContractSubject.TotalPrice = item.Price * prContractSubject.Quantity;
                        if (prContractSubject.Quantity == 0)
                        {
                            var mrpItemId = prContractSubject.RFPItem.PurchaseRequestItem.MrpItemId;
                            if (!await _mrpItemRepository.AnyAsync(a => !a.IsDeleted && a.Id == mrpItemId && a.PurchaseRequestItems.Any(b => !b.IsDeleted && b.RFPItems.Any(c => !c.IsDeleted && c.PRContractSubjects.Any(d => !d.IsDeleted)))))
                                prContractSubject.RFPItem.PurchaseRequestItem.MrpItem.MrpItemStatus = MrpItemStatus.RFPDone;
                            prContractSubject.IsDeleted = true;

                        }


                        neededQuantity = 0;
                    }

                }
                if (neededQuantity > 0)
                {
                    //var hasRfp = prItems.Any(a => !a.IsDeleted && a.ProductId == item.ProductId && a.RFPItems.Any(a => !a.IsDeleted && a.IsActive));
                    //var rfpId = hasRfp ? prItems.First(a => !a.IsDeleted && a.ProductId == item.ProductId && a.RFPItems.Any(a => !a.IsDeleted && a.IsActive)).RFPItems.First().RFPId : 0;
                    foreach (var rfpItem in rfpItems.Where(a => !a.IsDeleted && a.ProductId == item.ProductId && a.RemainedStock > 0))
                    {
                        if (rfpItem.RemainedStock >= neededQuantity)
                        {
                            newPrContractSubjects.Add(new PRContractSubject
                            {
                                ProductId = item.ProductId,
                                TotalPrice = neededQuantity* item.Price,
                                UnitPrice = item.Price,
                                Quantity = neededQuantity,
                                ReservedStock = 0,
                                RFPItemId = rfpItem.Id,
                                RemainedStock = neededQuantity,
                                RemainedQuantityToInvoice = neededQuantity,
                            });
                            rfpItem.RemainedStock -= neededQuantity;
                            rfpItem.PurchaseRequestItem.MrpItem.MrpItemStatus = (rfpItem.PurchaseRequestItem.MrpItem.MrpItemStatus < MrpItemStatus.PRC) ? MrpItemStatus.PRC : rfpItem.PurchaseRequestItem.MrpItem.MrpItemStatus;
                            neededQuantity = 0;
                        }
                        else if (neededQuantity > 0 && rfpItem.RemainedStock < neededQuantity)
                        {

                            newPrContractSubjects.Add(new PRContractSubject
                            {
                                ProductId = item.ProductId,
                                TotalPrice = item.Price* rfpItem.RemainedStock,
                                UnitPrice = item.Price,
                                Quantity = rfpItem.RemainedStock,
                                ReservedStock = 0,
                                RFPItemId = rfpItem.Id,
                                RemainedStock = rfpItem.RemainedStock,
                                RemainedQuantityToInvoice = rfpItem.RemainedStock,
                            });
                            neededQuantity -= rfpItem.RemainedStock;
                            rfpItem.PurchaseRequestItem.MrpItem.MrpItemStatus = (rfpItem.PurchaseRequestItem.MrpItem.MrpItemStatus < MrpItemStatus.PRC) ? MrpItemStatus.PRC : rfpItem.PurchaseRequestItem.MrpItem.MrpItemStatus;
                            rfpItem.RemainedStock = 0;
                        }
                        else
                            break;
                    }
                }
            }

            if (newPrContractSubjects != null && newPrContractSubjects.Any())
                _prContractSubjectRepository.AddRange(newPrContractSubjects);
            return ServiceResultFactory.CreateSuccess(true);

        }
        private async Task<ServiceResult<PrContractConfirmationWorkFlow>> AddPrContractConfirmationAsync(string contractCode, AddPrContractConfirmationDto model, List<AddAttachmentDto> attachments)
        {
            //if (usersIds.Count() == 0)
            //    return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);
            var prContractConfirmationModel = new PrContractConfirmationWorkFlow
            {

                ConfirmNote = model.Note,
                Status = ConfirmationWorkFlowStatus.Pending,
                PrContractConfirmationWorkFlowUsers = new List<PrContractConfirmationWorkFlowUser>(),
                PrContractConfirmationAttachments = new List<PAttachment>()
            };

            if (model.Users != null && model.Users.Any())
            {
                var usersIds = model.Users.Select(a => a.UserId).Distinct().ToList();
                if (await _userRepository.CountAsync(a => !a.IsDeleted && a.IsActive && usersIds.Contains(a.Id)) != usersIds.Count())
                    return ServiceResultFactory.CreateError<PrContractConfirmationWorkFlow>(null, MessageId.DataInconsistency);

                foreach (var item in model.Users)
                {
                    prContractConfirmationModel.PrContractConfirmationWorkFlowUsers.Add(new PrContractConfirmationWorkFlowUser
                    {
                        UserId = item.UserId,
                        OrderNumber = item.OrderNumber
                    });
                }
                if (prContractConfirmationModel.PrContractConfirmationWorkFlowUsers.Any())
                {
                    var bollincourtUser = prContractConfirmationModel.PrContractConfirmationWorkFlowUsers.OrderBy(a => a.OrderNumber).First();
                    bollincourtUser.IsBallInCourt = true;
                    bollincourtUser.DateStart = DateTime.UtcNow;
                }
            }
            else
            {
                prContractConfirmationModel.Status = ConfirmationWorkFlowStatus.Confirm;
            }

            if (attachments != null && attachments.Any())
            {
                var res = await AddPrContractConfirmationAttachmentAsync(prContractConfirmationModel, attachments);
                if (!res.Succeeded)
                    return ServiceResultFactory.CreateError<PrContractConfirmationWorkFlow>(null, res.Messages[0].Message);
            }

            return ServiceResultFactory.CreateSuccess(prContractConfirmationModel);
        }
        private async Task<ServiceResult<PrContractConfirmationWorkFlow>> EditPrContractConfirmationAsync(PRContract prContractModel, AddPrContractConfirmationDto model, List<AddAttachmentDto> attachments)
        {
            //if (usersIds.Count() == 0)
            //    return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

            var prContractConfirmationModel = prContractModel.PrContractConfirmationWorkFlows.Where(a => !a.IsDeleted && a.Status == ConfirmationWorkFlowStatus.Pending).OrderByDescending(a => a.PrContractConfirmWorkFlowId).FirstOrDefault();
            if (prContractConfirmationModel != null)
                return ServiceResultFactory.CreateError<PrContractConfirmationWorkFlow>(null, MessageId.OperationFailed);
            prContractConfirmationModel.ConfirmNote = model.Note;
            prContractConfirmationModel.Status = ConfirmationWorkFlowStatus.Pending;
            prContractConfirmationModel.PrContractConfirmationWorkFlowUsers = new List<PrContractConfirmationWorkFlowUser>();
            prContractConfirmationModel.PrContractConfirmationAttachments = new List<PAttachment>();


            if (model.Users != null && model.Users.Any())
            {
                var usersIds = model.Users.Select(a => a.UserId).Distinct().ToList();
                if (await _userRepository.CountAsync(a => !a.IsDeleted && a.IsActive && usersIds.Contains(a.Id)) != usersIds.Count())
                    return ServiceResultFactory.CreateError<PrContractConfirmationWorkFlow>(null, MessageId.DataInconsistency);

                foreach (var item in model.Users)
                {
                    prContractConfirmationModel.PrContractConfirmationWorkFlowUsers.Add(new PrContractConfirmationWorkFlowUser
                    {
                        UserId = item.UserId,
                        OrderNumber = item.OrderNumber
                    });
                }
                if (prContractConfirmationModel.PrContractConfirmationWorkFlowUsers.Any())
                {
                    var bollincourtUser = prContractConfirmationModel.PrContractConfirmationWorkFlowUsers.OrderBy(a => a.OrderNumber).First();
                    bollincourtUser.IsBallInCourt = true;
                    bollincourtUser.DateStart = DateTime.UtcNow;
                }
            }
            else
            {
                prContractConfirmationModel.Status = ConfirmationWorkFlowStatus.Confirm;
            }

            if (attachments != null && attachments.Any())
            {
                var res = await AddPrContractConfirmationAttachmentAsync(prContractConfirmationModel, attachments);
                if (!res.Succeeded)
                    return ServiceResultFactory.CreateError<PrContractConfirmationWorkFlow>(null, res.Messages[0].Message);
            }

            return ServiceResultFactory.CreateSuccess(prContractConfirmationModel);
        }
        private async Task<ServiceResult<PrContractConfirmationWorkFlow>> AddPrContractConfirmationAttachmentAsync(PrContractConfirmationWorkFlow prContractConfirmationWorkFlowModel, List<AddAttachmentDto> files)
        {


            if (files == null || !files.Any())
                return ServiceResultFactory.CreateError<PrContractConfirmationWorkFlow>(null, MessageId.FileNotFound);

            var attachModels = new List<PAttachment>();

            // add oldFiles

            // add new files
            foreach (var item in files)
            {
                var UploadedFile = await _fileHelper
                    .SaveDocumentFromTemp(item.FileSrc, ServiceSetting.UploadFilePath.PrContract);

                if (UploadedFile == null)
                    return ServiceResultFactory.CreateError<PrContractConfirmationWorkFlow>(null, MessageId.UploudFailed);

                if(prContractConfirmationWorkFlowModel.Status==ConfirmationWorkFlowStatus.Pending)
                    _fileHelper.DeleteDocumentFromTemp(item.FileSrc);

                prContractConfirmationWorkFlowModel.PrContractConfirmationAttachments.Add(new PAttachment
                {
                    PrContractConfirmationWorkFlow = prContractConfirmationWorkFlowModel,
                    FileSrc = item.FileSrc,
                    FileName = item.FileName,
                    FileType = UploadedFile.FileType,
                    FileSize = UploadedFile.FileSize
                });

            }

            return ServiceResultFactory.CreateSuccess(prContractConfirmationWorkFlowModel);
        }
        private async Task<ServiceResult<ListPendingPRContractDto>> GetPendingForConfirmPrContractByIdAsync(AuthenticateDto authenticate, long prContractId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<ListPendingPRContractDto>(null, MessageId.AccessDenied);

                var dbQuery = _prContractWorkFlowRepository
                    .AsNoTracking()
                    .Where(a => a.PRContract.BaseContractCode == authenticate.ContractCode && !a.IsDeleted && a.PrContractId == prContractId);

                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(c => permission.ProductGroupIds.Contains(c.PRContract.ProductGroupId));



                var result = await dbQuery.Select(x => new ListPendingPRContractDto
                {
                    PRContractId = x.PRContract.Id,
                    PRContractCode = x.PRContract.PRContractCode,
                    PRContractStatus = x.PRContract.PRContractStatus,
                    DateIssued = x.PRContract.DateIssued.ToUnixTimestamp(),
                    DateEnd = x.PRContract.DateEnd.ToUnixTimestamp(),
                    RFPNumber = "",
                    SupplierCode = x.PRContract.Supplier.SupplierCode,
                    SupplierName = x.PRContract.Supplier.Name,
                    ProductGroupId = x.PRContract.ProductGroupId,
                    ProductGroupTitle = x.PRContract.ProductGroup.Title,
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
                    BallInCourtUser = x.PrContractConfirmationWorkFlowUsers.Any() ?
                    x.PrContractConfirmationWorkFlowUsers.Where(a => a.IsBallInCourt)
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
                return ServiceResultFactory.CreateException<ListPendingPRContractDto>(null, exception);
            }
        }

        private async Task<ServiceResult<List<PRContractSubject>>> CancelPrContractSubjectAsync(List<PRContractSubject> prContractSubjects)
        {
            try
            {

                foreach (var item in prContractSubjects)
                {
                    var mrpItemId = item.RFPItem.PurchaseRequestItem.MrpItem.Id;
                    item.RFPItem.RemainedStock = item.Quantity;
                    if (await _mrpItemRepository.AnyAsync(a => !a.IsDeleted && a.Id == mrpItemId && !a.PurchaseRequestItems.Any(b => !b.IsDeleted && b.RFPItems.Any(c => !c.IsDeleted && c.IsActive && c.PRContractSubjects.Any(d => !d.IsDeleted && d.PRContract.PRContractStatus != PRContractStatus.Canceled && d.PRContract.PRContractStatus != PRContractStatus.Rejected)))))
                        item.RFPItem.PurchaseRequestItem.MrpItem.MrpItemStatus = MrpItemStatus.RFPDone;
                }
                return ServiceResultFactory.CreateSuccess(prContractSubjects);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<PRContractSubject>>(null, exception);
            }
        }

        private async Task SendNotifOnConfirmContract(AuthenticateDto authenticate, PRContract prContractModel,string supplierName)
        {
            var logModel = new AddAuditLogDto
            {
                ContractCode = prContractModel.BaseContractCode,
                FormCode = prContractModel.PRContractCode,
                KeyValue = prContractModel.Id.ToString(),
                Description = supplierName,
                RootKeyValue = prContractModel.Id.ToString(),
                NotifEvent = NotifEvent.ConfirmPRContract,
                ProductGroupId = prContractModel.ProductGroupId,
                PerformerUserId = authenticate.UserId,
                PerformerUserFullName = authenticate.UserFullName,
            };
            var task = new NotifToDto
            {
                NotifEvent = NotifEvent.AddPOPending,
                Roles = new List<string> { SCMRole.POMng }
            };
            var poIds = prContractModel.POs.Select(a => a.POId).ToList();
            await _scmLogAndNotificationService.AddPendingPOTaskNotificationAsync(logModel, task, authenticate.ContractCode, poIds);
        }
    }
}