using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Raybod.SCM.DataAccess.Core;
using Raybod.SCM.DataAccess.Extention;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Audit;
using Raybod.SCM.DataTransferObject.Notification;
using Raybod.SCM.DataTransferObject.Payment;
using Raybod.SCM.DataTransferObject.PendingForPayment;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Implementation
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITeamWorkAuthenticationService _authenticationService;
        private readonly ISCMLogAndNotificationService _scmLogAndNotificationService;
        private readonly IContractFormConfigService _formConfigService;
        private readonly DbSet<PO> _poRepository;
        private readonly DbSet<POTermsOfPayment> _poTermsOfPaymentRepository;
        private readonly DbSet<FinancialAccount> _financialAccountRepository;
        private readonly DbSet<PaymentConfirmationWorkFlow> _paymentConfirmRepository;
        private readonly DbSet<Payment> _paymentRepository;
        private readonly DbSet<User> _userRepository;
        private readonly DbSet<PendingForPayment> _pendingForPaymentRepository;
        private readonly Utilitys.FileHelper _fileHelper;
        private readonly CompanyConfig _appSettings;

        public PaymentService(
            IUnitOfWork unitOfWork,
            IWebHostEnvironment hostingEnvironmentRoot,
            ITeamWorkAuthenticationService authenticationService,
            IOptions<CompanyAppSettingsDto> appSettings,
            IHttpContextAccessor httpContextAccessor,
            ISCMLogAndNotificationService scmLogAndNotificationService,
            IContractFormConfigService formConfigService)
        {
            _unitOfWork = unitOfWork;
            _authenticationService = authenticationService;
            _formConfigService = formConfigService;
            _scmLogAndNotificationService = scmLogAndNotificationService;
            _paymentRepository = _unitOfWork.Set<Payment>();
            _pendingForPaymentRepository = _unitOfWork.Set<PendingForPayment>();
            _poRepository = _unitOfWork.Set<PO>();
            _userRepository = _unitOfWork.Set<User>();
            _paymentConfirmRepository = _unitOfWork.Set<PaymentConfirmationWorkFlow>();
            _financialAccountRepository = _unitOfWork.Set<FinancialAccount>();
            _poTermsOfPaymentRepository = _unitOfWork.Set<POTermsOfPayment>();
            _fileHelper = new Utilitys.FileHelper(hostingEnvironmentRoot);
            httpContextAccessor.HttpContext.Request.Headers.TryGetValue("companyCode", out var CompanyCode);
            _appSettings = appSettings.Value.CompanyConfig.First(a => a.CompanyCode == CompanyCode);
        }



      

        public async Task<ServiceResult<bool>> AddPendingToPaymentBaseOnTermsOfPaymentOfInvoice(AuthenticateDto authenticate, PO poModel, long invoiceId, decimal amount)
        {
            try
            {
                if (!await _poTermsOfPaymentRepository.AnyAsync(a => !a.IsDeleted && a.POId == poModel.POId && a.PaymentStep == TermsOfPaymentStep.InvoiceIssue))
                    return ServiceResultFactory.CreateSuccess(true);

                var poTermsOfPaymentStep = await _poTermsOfPaymentRepository
                    .Where(a => a.POId == poModel.POId && a.PaymentStep == TermsOfPaymentStep.InvoiceIssue)
                    .FirstOrDefaultAsync();

                if (poTermsOfPaymentStep == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                return await AddPendingForPaymentBaseOnInvoiceAmount(authenticate, poModel, invoiceId, amount, poTermsOfPaymentStep);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }


        public async Task<ServiceResult<int>> GetPendingForPaymentBadgeCountAsync(AuthenticateDto authenticate)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(0, MessageId.AccessDenied);

                var dbQuery = _pendingForPaymentRepository
                    .Where(a => !a.IsDeleted &&
                    a.Status == POPaymentStatus.NotSettled &&
                    a.PO.BaseContractCode == authenticate.ContractCode);

                var result = await dbQuery.CountAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<int>(0, exception);
            }
        }

        public async Task<ServiceResult<List<ListPendingForPaymentDto>>> GetNotSettledPendingForPaymentAsync(AuthenticateDto authenticate, PendingForPaymentQueryDto query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<ListPendingForPaymentDto>>(null, MessageId.AccessDenied);

                var dbQuery = _pendingForPaymentRepository
                    .Where(a => !a.IsDeleted && a.PO.BaseContractCode == authenticate.ContractCode && a.Status != POPaymentStatus.Settled && a.Status != POPaymentStatus.Canceled&&a.AmountRemained>0);

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a =>
                    a.PendingForPaymentNumber.Contains(query.SearchText) ||
                    a.PO.POCode.Contains(query.SearchText) ||
                     a.PO.PRContract.PRContractCode.Contains(query.SearchText) ||
                    (a.Invoice != null && a.Invoice.InvoiceNumber.Contains(query.SearchText)));

                if ((int)query.PendingOFPeymentStatus > 0 && (int)query.PendingOFPeymentStatus < 4)
                {
                    var dateTimeNow = DateTime.UtcNow.Date;
                    if (query.PendingOFPeymentStatus == PendingOFPeymentStatus.NotOverDue)
                        dbQuery = dbQuery.Where(a => a.Status != POPaymentStatus.Settled && a.PaymentDateTime.Date < dateTimeNow.Date);
                    else if (query.PendingOFPeymentStatus == PendingOFPeymentStatus.OverDue)
                        dbQuery = dbQuery.Where(a => a.Status != POPaymentStatus.Settled && a.PaymentDateTime.Date >= dateTimeNow.Date);
                    else if (query.PendingOFPeymentStatus == PendingOFPeymentStatus.Settled)
                        dbQuery = dbQuery.Where(a => a.Status == POPaymentStatus.Settled);
                }

                if (query.SupplierId != null && query.SupplierId.Any())
                    dbQuery = dbQuery.Where(a => query.SupplierId.Contains(a.SupplierId.Value));

                var totalCount = dbQuery.Count();
                var columnsMap = new Dictionary<string, Expression<Func<PendingForPayment, object>>>
                {
                    ["Id"] = v => v.Id,
                    ["PendingForPaymentNumber"] = v => v.PendingForPaymentNumber,
                    ["SupplierName"] = v => v.Supplier.Name
                };

                dbQuery = dbQuery.ApplayOrdering(query, columnsMap).ApplayPageing(query);

                List<ListPendingForPaymentDto> result = await ReturnPendingForPaymentListAsync(dbQuery);

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<ListPendingForPaymentDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<List<ListPendingForPaymentDto>>> GetPendingForConfrimAsync(AuthenticateDto authenticate, PendingForPaymentQueryDto query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<ListPendingForPaymentDto>>(null, MessageId.AccessDenied);

                var dbQuery = _pendingForPaymentRepository
                    .Where(a => !a.IsDeleted && a.PO.BaseContractCode == authenticate.ContractCode && a.Status != POPaymentStatus.Settled && a.Status != POPaymentStatus.Canceled);

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a =>
                    a.PendingForPaymentNumber.Contains(query.SearchText) ||
                    a.PO.POCode.Contains(query.SearchText) ||
                     a.PO.PRContract.PRContractCode.Contains(query.SearchText) ||
                    (a.Invoice != null && a.Invoice.InvoiceNumber.Contains(query.SearchText)));

                if ((int)query.PendingOFPeymentStatus > 0 && (int)query.PendingOFPeymentStatus < 4)
                {
                    var dateTimeNow = DateTime.UtcNow.Date;
                    if (query.PendingOFPeymentStatus == PendingOFPeymentStatus.NotOverDue)
                        dbQuery = dbQuery.Where(a => a.Status != POPaymentStatus.Settled && a.PaymentDateTime.Date < dateTimeNow.Date);
                    else if (query.PendingOFPeymentStatus == PendingOFPeymentStatus.OverDue)
                        dbQuery = dbQuery.Where(a => a.Status != POPaymentStatus.Settled && a.PaymentDateTime.Date >= dateTimeNow.Date);
                    else if (query.PendingOFPeymentStatus == PendingOFPeymentStatus.Settled)
                        dbQuery = dbQuery.Where(a => a.Status == POPaymentStatus.Settled);
                }

                if (query.SupplierId != null && query.SupplierId.Any())
                    dbQuery = dbQuery.Where(a => query.SupplierId.Contains(a.SupplierId.Value));

                var totalCount = dbQuery.Count();
                var columnsMap = new Dictionary<string, Expression<Func<PendingForPayment, object>>>
                {
                    ["Id"] = v => v.Id,
                    ["PendingForPaymentNumber"] = v => v.PendingForPaymentNumber,
                    ["SupplierName"] = v => v.Supplier.Name
                };

                dbQuery = dbQuery.ApplayOrdering(query, columnsMap).ApplayPageing(query);

                List<ListPendingForPaymentDto> result = await ReturnPendingForPaymentListAsync(dbQuery);

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<ListPendingForPaymentDto>>(null, exception);
            }
        }
        public async Task<ServiceResult<List<ListPendingForPaymentDto>>> GetPendingForPaymentAsync(AuthenticateDto authenticate, PendingForPaymentQueryDto query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<ListPendingForPaymentDto>>(null, MessageId.AccessDenied);

                var dbQuery = _pendingForPaymentRepository.Where(a => !a.IsDeleted && a.PO.BaseContractCode == authenticate.ContractCode);

                if (string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a =>
                    a.PendingForPaymentNumber.Contains(query.SearchText) ||
                    a.PO.POCode.Contains(query.SearchText) ||
                     a.PO.PRContract.PRContractCode.Contains(query.SearchText) ||
                    (a.Invoice != null && a.Invoice.InvoiceNumber.Contains(query.SearchText)));

                if ((int)query.PendingOFPeymentStatus > 0 && (int)query.PendingOFPeymentStatus < 4)
                {
                    var dateTimeNow = DateTime.UtcNow.Date;
                    if (query.PendingOFPeymentStatus == PendingOFPeymentStatus.NotOverDue)
                        dbQuery = dbQuery.Where(a => a.Status != POPaymentStatus.Settled && a.PaymentDateTime.Date < dateTimeNow.Date);
                    else if (query.PendingOFPeymentStatus == PendingOFPeymentStatus.OverDue)
                        dbQuery = dbQuery.Where(a => a.Status != POPaymentStatus.Settled && a.PaymentDateTime.Date >= dateTimeNow.Date);
                    else if (query.PendingOFPeymentStatus == PendingOFPeymentStatus.Settled)
                        dbQuery = dbQuery.Where(a => a.Status == POPaymentStatus.Settled);
                }

                var totalCount = dbQuery.Count();
                var columnsMap = new Dictionary<string, Expression<Func<PendingForPayment, object>>>
                {
                    ["Id"] = v => v.Id,
                    ["InvoiceNumber"] = v => v.PendingForPaymentNumber,
                    ["SupplierName"] = v => v.Supplier.Name

                };

                dbQuery = dbQuery.ApplayOrdering(query, columnsMap).ApplayPageing(query);

                List<ListPendingForPaymentDto> result = await ReturnPendingForPaymentListAsync(dbQuery);

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<ListPendingForPaymentDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<PendingForPaymentInfoForPayDto>> GetPendingOfPaymentForPayByPendingOfPaymentIdAsync(AuthenticateDto authenticate, long pendingForPaymentId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<PendingForPaymentInfoForPayDto>(null, MessageId.AccessDenied);

                var dbQuery = _pendingForPaymentRepository
                    .Where(a => !a.IsDeleted &&
                    a.Id == pendingForPaymentId &&
                    a.PO.BaseContractCode == authenticate.ContractCode &&
                    a.Status == POPaymentStatus.NotSettled);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<PendingForPaymentInfoForPayDto>(null, MessageId.EntityDoesNotExist);

                var result = await dbQuery
                    .Select(p => new PendingForPaymentInfoForPayDto
                    {
                        PRContractId = p.PO != null ? p.PO.PRContractId : 0,
                        ContractCode = p.PO != null ? p.PO.BaseContractCode : "",
                        SupplierName = p.Supplier != null ? p.Supplier.Name : "",
                        SupplierId = p.SupplierId.Value,
                        CurrencyType = p.PO != null ? p.PO.CurrencyType : CurrencyType.IRR,
                        SupplierCode = p.Supplier.SupplierCode,
                        SupplierLogo = p.Supplier.Logo != null ? _appSettings.ClientHost + ServiceSetting.UploadImagesPath.LogoSmall + p.Supplier.Logo : "",
                        PendingForPayments = new List<PendingForPaymentForPayedDto> { new PendingForPaymentForPayedDto
                        {
                            Amount=p.Amount,
                            AmountPayed=p.AmountPayed,
                            AmountRemained=p.AmountRemained,
                            PaymentStep =p.POTermsOfPayment.PaymentStep,
                            CurrencyType=p.PO!= null ? p.PO.CurrencyType : CurrencyType.IRR,
                            InvoiceNumber=p.Invoice!= null ? p.Invoice.InvoiceNumber : "",
                            PaymentDateTime=p.PaymentDateTime.ToUnixTimestamp(),
                            PendingForPaymentNumber=p.PendingForPaymentNumber,
                            PendingForPaymentId=p.Id,
                            POCode=p.PO!= null ? p.PO.POCode: "",
                            PRContractCode=p.PO!= null ? p.PO.PRContract.PRContractCode: "",
                            PaymentAmount=p.AmountRemained
                        } }

                    }).FirstOrDefaultAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<PendingForPaymentInfoForPayDto>(null, exception);
            }
        }


        public async Task<ServiceResult<List<PendingForPaymentForPayedDto>>> GetPendingOfPaymentForPayBySupplierIdAsync(AuthenticateDto authenticate, int supplierId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<PendingForPaymentForPayedDto>>(null, MessageId.AccessDenied);

                var dbQuery = _pendingForPaymentRepository
                    .Where(a => !a.IsDeleted &&
                    a.SupplierId == supplierId &&
                    a.PO.BaseContractCode == authenticate.ContractCode && a.Status == POPaymentStatus.NotSettled);

                var result = await dbQuery
                    .Select(p => new PendingForPaymentForPayedDto
                    {
                        Amount = p.Amount,
                        AmountPayed = p.AmountPayed,
                        AmountRemained = p.AmountRemained,
                        CurrencyType = p.PO != null ? p.PO.CurrencyType : CurrencyType.IRR,
                        InvoiceNumber = p.Invoice != null ? p.Invoice.InvoiceNumber : "",
                        PaymentDateTime = p.PaymentDateTime.ToUnixTimestamp(),
                        PendingForPaymentNumber = p.PendingForPaymentNumber,
                        PaymentStep = p.POTermsOfPayment.PaymentStep,
                        PendingForPaymentId = p.Id,
                        POCode = p.PO != null ? p.PO.POCode : "",
                        PRContractCode = p.PO != null ? p.PO.PRContract.PRContractCode : "",
                        PaymentAmount = p.AmountRemained
                    }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<PendingForPaymentForPayedDto>>(null, exception);
            }
        }

        public async Task<DownloadFileDto> DownloadPaymentAttachmentAsync(AuthenticateDto authenticate, long paymentId, string fileSrc)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return null;

                var dbQuery = _paymentRepository
                    .Where(a => !a.IsDeleted && a.PaymentId == paymentId &&
                    a.PaymentAttachments.Any(c => !c.IsDeleted && c.FileSrc == fileSrc));


                var model = await dbQuery
                    .Select(c => new
                    {
                        ContractCode = c.ContractCode,
                    })
                    .FirstOrDefaultAsync();

                if (model == null)
                    return null;

                var streamResult = await _fileHelper.DownloadDocument(fileSrc, ServiceSetting.FileSection.Payment);
                if (streamResult == null)
                    return null;

                return streamResult;
            }
            catch (Exception exception)
            {
                return null;
            }
        }

        #region po 

        // add pending payment

        public async Task<ServiceResult<AddPendingForPaymentResultDto>> AddPendingToPaymentBaseOnTermsOfPaymentExceptInvoiceAsync(AuthenticateDto authenticate, long poId, AddPendingForPaymentDto model)
        {

            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(new AddPendingForPaymentResultDto(), MessageId.AccessDenied);

                var poModel = await _poRepository.Where(a => !a.IsDeleted && a.POId == poId && a.BaseContractCode == authenticate.ContractCode)
                    .Include(a => a.POTermsOfPayments)
                    .FirstOrDefaultAsync();

                if (poModel == null)
                    return ServiceResultFactory.CreateError(new AddPendingForPaymentResultDto(), MessageId.ThisStateSetBefore);
                if (poModel.POStatus == POStatus.Canceled)
                    return ServiceResultFactory.CreateError(new AddPendingForPaymentResultDto(), MessageId.CantDoneBecausePOCanceled);

                //var acceptAbleSteps = new List<TermsOfPaymentStep> { TermsOfPaymentStep.packing, TermsOfPaymentStep.Preparation };
                //if (!acceptAbleSteps.Contains(paymentStep))
                //    return ServiceResultFactory.CreateError(new PendingForPaymentInfoDto(), MessageId.ThisStateSetBefore);

                if (model.RequestAmount <= 0)
                    return ServiceResultFactory.CreateError(new AddPendingForPaymentResultDto(), MessageId.InputDataValidationError);

                var beforePayed = await _pendingForPaymentRepository
                    .Where(a => a.POId == poModel.POId && a.POTermsOfPayment.PaymentStep == model.PaymentStep)
                    .ToListAsync();

                decimal BeforeRequestedAmount = 0;
                if (beforePayed != null && beforePayed.Any())
                    BeforeRequestedAmount = beforePayed.Where(a => !a.IsDeleted).Sum(v => v.Amount);

                var poTermsOfPaymentStep = poModel.POTermsOfPayments
                .Where(a => a.PaymentStep == model.PaymentStep)
                .FirstOrDefault();

                if (poTermsOfPaymentStep == null && model.PaymentStep != TermsOfPaymentStep.Tax)
                    return ServiceResultFactory.CreateError(new AddPendingForPaymentResultDto(), MessageId.DataInconsistency);

                var allThisStepAmount = (model.PaymentStep != TermsOfPaymentStep.Tax) ? (poTermsOfPaymentStep.PaymentPercentage * poModel.TotalAmount) / 100 : (poModel.Tax > 0) ? (poModel.Tax * poModel.TotalAmount) / 100 : 0;

                if (allThisStepAmount == 0)
                    return ServiceResultFactory.CreateError(new AddPendingForPaymentResultDto(), MessageId.PoPaymentCantBeCreate);
                if (allThisStepAmount < model.RequestAmount)
                    return ServiceResultFactory.CreateError(new AddPendingForPaymentResultDto(), MessageId.PoPaymentAmountUpperThenValidAmount);

                if ((allThisStepAmount - BeforeRequestedAmount) < model.RequestAmount)
                    return ServiceResultFactory.CreateError(new AddPendingForPaymentResultDto(), MessageId.PoPaymentAmountUpperThenValidAmount);

                var datePayment = DateTime.UtcNow;

                if (model.PaymentStep != TermsOfPaymentStep.Tax && poTermsOfPaymentStep.IsCreditPayment)
                    datePayment = datePayment.AddDays(poTermsOfPaymentStep.CreditDuration);
                long? potermOfPayment = null;
                bool? IsTax = null;
                if (model.PaymentStep != TermsOfPaymentStep.Tax)
                {
                    potermOfPayment = poTermsOfPaymentStep.Id;

                }
                else
                {
                    IsTax = true;
                }
                var pendigToPaymentModel = new PendingForPayment
                {
                    Amount = model.RequestAmount,
                    AmountPayed = 0,
                    AmountRemained = model.RequestAmount,
                    PaymentDateTime = datePayment,
                    PRContractId = poModel.PRContractId,
                    POId = poModel.POId,
                    POTermsOfPaymentId = potermOfPayment,
                    SupplierId = poModel.SupplierId,
                    Status = POPaymentStatus.NotSettled,
                    BaseContractCode = poModel.BaseContractCode,
                    IsTax = IsTax
                };

                // generate form code
                var count = await _pendingForPaymentRepository.CountAsync(a => a.BaseContractCode == authenticate.ContractCode);
                var codeRes = await _formConfigService.GenerateFormCodeAsync(authenticate.ContractCode, FormName.PendingToPayment, count);
                if (!codeRes.Succeeded)
                    return ServiceResultFactory.CreateError(new AddPendingForPaymentResultDto(), codeRes.Messages.First().Message);
                pendigToPaymentModel.PendingForPaymentNumber = codeRes.Result;

                _pendingForPaymentRepository.Add(pendigToPaymentModel);

                List<RequestedAmountOFPOPaymentStepDto> paymentStepsResult = new List<RequestedAmountOFPOPaymentStepDto>();

                foreach (var item in poModel.POTermsOfPayments)
                {
                    var allThisStepAmounts = (item.PaymentPercentage * poModel.TotalAmount) / 100;


                    var BeforeRequestedAmounts = await _pendingForPaymentRepository
                    .Where(a => !a.IsDeleted && a.POId == poModel.POId && a.POTermsOfPayment.PaymentStep == item.PaymentStep)
                    .SumAsync(v => v.Amount);
                    if (model.PaymentStep == item.PaymentStep)
                        BeforeRequestedAmounts += model.RequestAmount;
                    if (allThisStepAmounts - BeforeRequestedAmounts > 0)
                    {
                        paymentStepsResult.Add(new RequestedAmountOFPOPaymentStepDto
                        {
                            PaymentStep = item.PaymentStep,
                            RemainedPaymentStepAmount = allThisStepAmounts - BeforeRequestedAmounts,

                        });
                    }

                }
                if (poModel.Tax > 0)
                {
                    var allThisStepAmounts = (poModel.Tax * poModel.TotalAmount) / 100;

                    var BeforeRequestedAmounts = await _pendingForPaymentRepository
                    .Where(a => !a.IsDeleted && a.POId == poModel.POId && a.IsTax == true)
                    .SumAsync(v => v.Amount);
                    if (model.PaymentStep == TermsOfPaymentStep.Tax)
                        BeforeRequestedAmounts += model.RequestAmount;
                    if (allThisStepAmounts - BeforeRequestedAmounts != 0)
                    {
                        paymentStepsResult.Add(new RequestedAmountOFPOPaymentStepDto
                        {
                            PaymentStep = TermsOfPaymentStep.Tax,
                            RemainedPaymentStepAmount = allThisStepAmounts - BeforeRequestedAmounts,

                        });
                    }
                }
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    AddPendingForPaymentResultDto result = new AddPendingForPaymentResultDto();
                    var pendingForPaymentInfo = new PendingForPaymentInfoDto
                    {
                        PendingForPaymentId = pendigToPaymentModel.Id,
                        Amount = pendigToPaymentModel.Amount,
                        PendingForPaymentNumber = pendigToPaymentModel.PendingForPaymentNumber,
                        CurrencyType = poModel.CurrencyType,
                        PaymentStep = model.PaymentStep,
                        PendingOFPeymentStatus =
                        pendigToPaymentModel.Status == POPaymentStatus.Settled
                        ? PendingOFPeymentStatus.Settled
                        : pendigToPaymentModel.PaymentDateTime.Date >= DateTime.UtcNow.Date
                        ? PendingOFPeymentStatus.OverDue
                        : PendingOFPeymentStatus.NotOverDue,
                        UserAudit = new UserAuditLogDto
                        {
                            CreateDate = pendigToPaymentModel.CreatedDate.ToUnixTimestamp(),
                            AdderUserName = authenticate.UserFullName,
                            AdderUserImage = authenticate.UserImage != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + authenticate.UserImage : "",
                        },
                        Payments = new List<PaymentOFPendingForPaymentDto>()
                    };
                    result.PendingForPaymentInfo = pendingForPaymentInfo;
                    result.RequestPaymentStepsInfo = paymentStepsResult;
                    var res = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = poModel.BaseContractCode,
                        FormCode = pendigToPaymentModel.PendingForPaymentNumber,
                        Description = poModel.POCode,
                        KeyValue = pendigToPaymentModel.Id.ToString(),
                        NotifEvent = NotifEvent.AddPendingForPayment,
                        RootKeyValue = pendigToPaymentModel.Id.ToString(),
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,

                    }, new List<NotifToDto> {
                            new NotifToDto
                            {
                                NotifEvent=NotifEvent.AddPayment,
                                Roles= new List<string>
                                {
                                    SCMRole.PaymentMng,
                                }
                            }
                });
                    return ServiceResultFactory.CreateSuccess(result);
                }

                return ServiceResultFactory.CreateError(new AddPendingForPaymentResultDto(), MessageId.SaveFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new AddPendingForPaymentResultDto(), exception);
            }
        }

        public async Task<ServiceResult<bool>> AddPendingToPaymentBaseOnTermsOfApprovePOAsync(AuthenticateDto authenticate, PO poModel)
        {
            try
            {
                //var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                //if (!permission.HasPermission)
                //    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                if (await _pendingForPaymentRepository
                    .AnyAsync(a => a.POId == poModel.POId && a.PO.BaseContractCode == authenticate.ContractCode && a.POTermsOfPayment.PaymentStep == TermsOfPaymentStep.ApprovedPo))
                    return ServiceResultFactory.CreateError(false, MessageId.ThisStateSetBefore);

                var poTermsOfPaymentStep = _poTermsOfPaymentRepository
                .Where(a => !a.IsDeleted && a.POId == poModel.POId && a.PO.BaseContractCode == authenticate.ContractCode && a.PaymentStep == TermsOfPaymentStep.ApprovedPo)
                .FirstOrDefault();

                if (poTermsOfPaymentStep == null)
                    return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                var allThisStepAmount = (poTermsOfPaymentStep.PaymentPercentage * poModel.FinalTotalAmount) / 100;

                var datePayment = DateTime.UtcNow;

                if (poTermsOfPaymentStep.IsCreditPayment)
                    datePayment = datePayment.AddDays(poTermsOfPaymentStep.CreditDuration);

                var pendigToPaymentModel = new PendingForPayment
                {
                    Amount = allThisStepAmount,
                    AmountPayed = 0,
                    AmountRemained = allThisStepAmount,
                    PaymentDateTime = datePayment,
                    PRContractId = poModel.PRContractId,
                    POId = poModel.POId,
                    POTermsOfPaymentId = poTermsOfPaymentStep.Id,
                    SupplierId = poModel.SupplierId,
                    Status = POPaymentStatus.NotSettled,
                    BaseContractCode = poModel.BaseContractCode
                };

                // generate form code
                var count = await _pendingForPaymentRepository.CountAsync(a => a.BaseContractCode == authenticate.ContractCode);
                var codeRes = await _formConfigService.GenerateFormCodeAsync(authenticate.ContractCode, FormName.PendingToPayment, count);
                if (!codeRes.Succeeded)
                    return ServiceResultFactory.CreateError(false, codeRes.Messages.First().Message);
                pendigToPaymentModel.PendingForPaymentNumber = codeRes.Result;

                _pendingForPaymentRepository.Add(pendigToPaymentModel);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var res = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = poModel.BaseContractCode,
                        FormCode = pendigToPaymentModel.PendingForPaymentNumber,
                        Description = poModel.POCode,
                        KeyValue = pendigToPaymentModel.Id.ToString(),
                        NotifEvent = NotifEvent.AddPendingForPayment,
                        RootKeyValue = pendigToPaymentModel.Id.ToString(),
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,

                    }, new List<NotifToDto> {
                            new NotifToDto
                            {
                                NotifEvent=NotifEvent.AddPayment,
                                Roles= new List<string>
                                {
                                    SCMRole.PaymentMng,
                                }
                            }
                });
                    return ServiceResultFactory.CreateSuccess(true);
                }
                return ServiceResultFactory.CreateError(false, MessageId.SaveFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }
        private async Task<ServiceResult<bool>> AddPendingForPaymentBaseOnInvoiceAmount(AuthenticateDto authenticate, PO poModel, long invoiceId, decimal amount, POTermsOfPayment thisPaymentStep)
        {
            var PaymentAmount = (thisPaymentStep.PaymentPercentage * amount) / 100;
            var datePayment = DateTime.UtcNow;

            if (thisPaymentStep.IsCreditPayment)
                datePayment = datePayment.AddDays(thisPaymentStep.CreditDuration);

            var pendigToPaymentModel = new PendingForPayment
            {
                Amount = PaymentAmount,
                AmountPayed = 0,
                AmountRemained = PaymentAmount,
                PaymentDateTime = datePayment,
                PRContractId = poModel.PRContractId,
                POId = poModel.POId,
                POTermsOfPaymentId = thisPaymentStep.Id,
                SupplierId = poModel.SupplierId,
                InvoiceId = invoiceId,
                Status = POPaymentStatus.NotSettled,
                BaseContractCode = poModel.BaseContractCode
            };

            // generate form code
            var count = await _pendingForPaymentRepository.CountAsync(a => a.BaseContractCode == authenticate.ContractCode);
            var codeRes = await _formConfigService.GenerateFormCodeAsync(authenticate.ContractCode, FormName.PendingToPayment, count);
            if (!codeRes.Succeeded)
                return ServiceResultFactory.CreateError(false, codeRes.Messages.First().Message);
            pendigToPaymentModel.PendingForPaymentNumber = codeRes.Result;

            _pendingForPaymentRepository.Add(pendigToPaymentModel);
            if (await _unitOfWork.SaveChangesAsync() > 0)
            {
                var res = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                {
                    ContractCode = poModel.BaseContractCode,
                    FormCode = pendigToPaymentModel.PendingForPaymentNumber,
                    KeyValue = pendigToPaymentModel.Id.ToString(),
                    Description = poModel.POCode,
                    NotifEvent = NotifEvent.AddPendingForPayment,
                    RootKeyValue = pendigToPaymentModel.Id.ToString(),
                    PerformerUserId = authenticate.UserId,
                    PerformerUserFullName = authenticate.UserFullName,
                },
                new List<NotifToDto> {
                            new NotifToDto
                            {
                                NotifEvent=NotifEvent.AddPayment,
                                Roles= new List<string>
                                {
                                    SCMRole.PaymentMng,
                                }
                            }
                });

                return ServiceResultFactory.CreateSuccess(true);
            }
            return ServiceResultFactory.CreateError(false, MessageId.SaveFailed);
        }


        // po pendingPayment report
        public async Task<ServiceResult<PendingForPaymentDto>> GetPendingForPaymentByPOIdAsync(AuthenticateDto authenticate, long poId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<PendingForPaymentDto>(null, MessageId.AccessDenied);

                var dbQuery = _pendingForPaymentRepository.Where(a => !a.IsDeleted && a.POId == poId && a.PO.BaseContractCode == authenticate.ContractCode)
                 .AsQueryable();




                var poModel = await _poRepository.AsNoTracking().Include(a => a.POTermsOfPayments).Where(a => !a.IsDeleted && a.BaseContractCode == authenticate.ContractCode && a.POId == poId).FirstOrDefaultAsync();

                if (poModel == null)
                    return ServiceResultFactory.CreateError<PendingForPaymentDto>(null, MessageId.EntityDoesNotExist);



                List<RequestedAmountOFPOPaymentStepDto> paymentStepsResult = new List<RequestedAmountOFPOPaymentStepDto>();

                foreach (var item in poModel.POTermsOfPayments)
                {
                    var allThisStepAmount = (item.PaymentPercentage * poModel.TotalAmount) / 100;

                    var BeforeRequestedAmount = await _pendingForPaymentRepository
                    .Where(a => !a.IsDeleted && a.POId == poModel.POId && a.POTermsOfPayment.PaymentStep == item.PaymentStep && a.Status != POPaymentStatus.Canceled)
                    .SumAsync(v => v.Amount);
                    if (allThisStepAmount - BeforeRequestedAmount != 0)
                    {
                        paymentStepsResult.Add(new RequestedAmountOFPOPaymentStepDto
                        {
                            PaymentStep = item.PaymentStep,
                            RemainedPaymentStepAmount = allThisStepAmount - BeforeRequestedAmount,

                        });
                    }

                }
                if (poModel.Tax > 0)
                {
                    var allThisStepAmount = (poModel.Tax * poModel.TotalAmount) / 100;

                    var BeforeRequestedAmount = await _pendingForPaymentRepository
                    .Where(a => !a.IsDeleted && a.POId == poModel.POId && a.IsTax == true && a.Status != POPaymentStatus.Canceled)
                    .SumAsync(v => v.Amount);
                    if (allThisStepAmount - BeforeRequestedAmount != 0)
                    {
                        paymentStepsResult.Add(new RequestedAmountOFPOPaymentStepDto
                        {
                            PaymentStep = TermsOfPaymentStep.Tax,
                            RemainedPaymentStepAmount = allThisStepAmount - BeforeRequestedAmount,

                        });
                    }
                }



                var pendingForPaymentResult = await dbQuery
                    .Select(p => new PendingForPaymentInfoDto
                    {
                        Amount = p.Amount,
                        PendingForPaymentId = p.Id,
                        PendingForPaymentNumber = p.PendingForPaymentNumber,
                        CurrencyType = p.PO.CurrencyType,
                        PaymentStep = p.POTermsOfPayment.PaymentStep,
                        PendingOFPeymentStatus = p.Status == POPaymentStatus.Canceled ? PendingOFPeymentStatus.Canceled : p.Status == POPaymentStatus.Settled ?
                        PendingOFPeymentStatus.Settled : p.PaymentDateTime.Date >= DateTime.UtcNow.Date
                        ? PendingOFPeymentStatus.OverDue : PendingOFPeymentStatus.NotOverDue,
                        UserAudit = p.AdderUser != null ? new UserAuditLogDto
                        {
                            CreateDate = p.CreatedDate.ToUnixTimestamp(),
                            AdderUserName = p.AdderUser.FullName,
                            AdderUserImage = p.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + p.AdderUser.Image : "",
                        } : null,
                        Payments = p.PaymentPendingForPayments.Select(c => new PaymentOFPendingForPaymentDto
                        {
                            PaymentId = c.PaymentId,
                            PaymentAmount = c.PaymentAmount,
                            PaymentNumber = c.Payment.PaymentNumber,
                            PaymentDate = c.Payment.CreatedDate.ToUnixTimestamp(),
                            CreatedUserName = c.Payment.AdderUser != null ? c.Payment.AdderUser.FullName : ""
                        }).ToList()
                    }).ToListAsync();

                PendingForPaymentDto result = new PendingForPaymentDto();
                result.PendingForPaymentInfo = pendingForPaymentResult;
                result.RequestPaymentStepsInfo = paymentStepsResult;

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<PendingForPaymentDto>(null, exception);
            }
        }

        public async Task<ServiceResult<bool>> DeletePendingForPaymentByPOIdAsync(AuthenticateDto authenticate, long poId, long pendingForPaymentId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery = _pendingForPaymentRepository.Include(a => a.PaymentPendingForPayments).Include(a => a.PO).Where(a => !a.IsDeleted && a.POId == poId && a.Id == pendingForPaymentId && a.PO.BaseContractCode == authenticate.ContractCode);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                var pendingPayment = await dbQuery.FirstOrDefaultAsync();
                if (pendingPayment == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (pendingPayment.PO.POStatus == POStatus.Canceled)
                    return ServiceResultFactory.CreateError(false, MessageId.CantDoneBecausePOCanceled);

                if (pendingPayment.PaymentPendingForPayments.Any(a=>a.Payment.Status!=PaymentStatus.Reject))
                    return ServiceResultFactory.CreateError(false, MessageId.PendingForPaymentHasPayment);

                pendingPayment.IsDeleted = true;



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
        #endregion

        #region PRcontract
        public async Task<ServiceResult<List<PendingForPaymentReportForContractDto>>> GetReportPendingForPaymentByPRContractIdAsync(AuthenticateDto authenticate, long prContractId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<PendingForPaymentReportForContractDto>>(null, MessageId.AccessDenied);


                var dbQuery = _pendingForPaymentRepository.Where(a => !a.IsDeleted && a.PO.BaseContractCode == authenticate.ContractCode && a.PO.PRContractId == prContractId);

                var result = await dbQuery.Select(c => new PendingForPaymentReportForContractDto
                {
                    PendingForPaymentId = c.Id,
                    Amount = c.Amount,
                    CurrencyType = c.PO.CurrencyType,
                    PaymentStep = c.POTermsOfPayment.PaymentStep,
                    InvoiceNumber = c.Invoice != null ? c.Invoice.InvoiceNumber : "",
                    PaymentDateTime = c.PaymentDateTime.ToUnixTimestamp(),
                    POCode = c.PO != null ? c.PO.POCode : "",
                    PendingOFPeymentStatus = c.Status == POPaymentStatus.Settled ?
                    PendingOFPeymentStatus.Settled : c.PaymentDateTime.Date >= DateTime.UtcNow.Date
                    ? PendingOFPeymentStatus.OverDue : PendingOFPeymentStatus.NotOverDue,
                    PendingForPaymentNumber = c.PendingForPaymentNumber,
                    AmountPayed = c.AmountPayed,
                    AmountRemained = c.AmountRemained,
                    UserAudit = c.AdderUser != null ? new UserAuditLogDto
                    {
                        AdderUserName = c.AdderUser.FullName,
                        AdderUserImage = c.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + c.AdderUser.Image : "",
                        CreateDate = c.CreatedDate.ToUnixTimestamp(),
                    } : null
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<PendingForPaymentReportForContractDto>>(null, exception);
            }
        }

        #endregion

        //public async Task<ServiceResult<List<PaymentListDto>>> GetPaymentsByPoIdAsync(AuthenticateDto authenticate, long poId)
        //{
        //    try
        //    {
        //        var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
        //        if (!permission.HasPermission)
        //            return ServiceResultFactory.CreateError<List<PaymentListDto>>(null, MessageId.AccessDenied);

        //        var dbQuery = _paymentRepository.Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.PaymentPendingForPayments.Any(c => c.PendingForPayment.POId == poId));

        //        dbQuery = dbQuery.OrderByDescending(c => c.PaymentId).AsQueryable();

        //        var result = await dbQuery.Select(c => new PaymentListDto
        //        {
        //            PaymentId = c.PaymentId,
        //            DateCreate = c.CreatedDate.ToUnixTimestamp(),
        //            Amount = c.Amount,
        //            Note = c.Note,
        //            PaymentNumber = c.PaymentNumber,
        //            SupplierId = c.SupplierId,
        //            SupplierName = c.Supplier.Name,
        //        }).ToListAsync();

        //        return ServiceResultFactory.CreateSuccess(result);

        //    }
        //    catch (Exception exception)
        //    {
        //        return ServiceResultFactory.CreateException<List<PaymentListDto>>(null, exception);
        //    }
        //}

        //public async Task<ServiceResult<PaymentInfoDto>> GetPaymentByIdAndPoIdAsync(AuthenticateDto authenticate, long poId, long paymentId)
        //{
        //    try
        //    {
        //        var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
        //        if (!permission.HasPermission)
        //            return ServiceResultFactory.CreateError<PaymentInfoDto>(null, MessageId.AccessDenied);

        //        var dbQuery = _paymentRepository.Where(a => !a.IsDeleted && a.PaymentId == paymentId && a.ContractCode == authenticate.ContractCode && a.PaymentPendingForPayments.Any(c => c.PendingForPayment.POId == poId))
        //            .AsQueryable();

        //        if (dbQuery.Count() == 0)
        //            return ServiceResultFactory.CreateError<PaymentInfoDto>(null, MessageId.EntityDoesNotExist);

        //        return await ReturnPaymentAsync(dbQuery);
        //    }
        //    catch (Exception exception)
        //    {
        //        return ServiceResultFactory.CreateException<PaymentInfoDto>(null, exception);
        //    }
        //}

        public async Task<ServiceResult<bool>> AddPaymentByPendingForPaymentAsync(AuthenticateDto authenticate, int supplierId, AddPaymentDto model)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                if (model.PendingForPayments == null || model.PendingForPayments.Count() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);



                if (model.PendingForPayments.Any(a => a.PaymentAmount <= 0))
                    return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                var pendingForPaymentIds = model.PendingForPayments.Select(a => a.PendingForPaymentId).ToList();


                var dbQuery = _pendingForPaymentRepository
                    .Include(a => a.PO).Where(a => a.SupplierId == supplierId &&
                      a.PO.BaseContractCode == authenticate.ContractCode &&
                      a.Status == POPaymentStatus.NotSettled &&
                      pendingForPaymentIds.Contains(a.Id));

                var pendingForPaymentModels = await dbQuery.Include(a => a.PO)
                    .OrderBy(a => a.Id)
                    .ToListAsync();

                var pos = pendingForPaymentModels.Select(a => a.PO).ToList();
                if (pendingForPaymentModels == null || pendingForPaymentModels.Count() != pendingForPaymentIds.Count())
                    return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                var onependingForPaymentModels = pendingForPaymentModels.FirstOrDefault(a => a.POId > 0);

                var paymentModel = new Payment
                {
                    IsDeleted = false,
                    Note = model.WorkFlow.Note,
                    ContractCode = authenticate.ContractCode,
                    SupplierId = supplierId,
                    Status = PaymentStatus.Register,
                    CurrencyType = pendingForPaymentModels.First().PO.CurrencyType,
                   
                    PaymentPendingForPayments = new List<PaymentPendingForPayment>(),
                    PaymentConfirmationWorkFlows = new List<PaymentConfirmationWorkFlow>()
                };
                var confirmWorkFlow = await AddPaymentConfirmationAsync(model.WorkFlow);
                if (!confirmWorkFlow.Succeeded)
                    return ServiceResultFactory.CreateError(false, MessageId.OperationFailed);
                paymentModel.PaymentConfirmationWorkFlows.Add(confirmWorkFlow.Result);
                if (confirmWorkFlow.Result.Status == ConfirmationWorkFlowStatus.Confirm)
                {
                    paymentModel.Status = PaymentStatus.Confirm;

                }

                if(paymentModel.Status == PaymentStatus.Confirm)
                {
                    var res = UpdatePendingForPaymentAmountandConfirmPaymentPendingForPayment(pendingForPaymentModels, paymentModel, model.PendingForPayments);
                    if (!res.Succeeded)
                        return res;
                }
                else
                {
                    var res = UpdatePendingForPaymentAmountandAddPaymentPendingForPayment(pendingForPaymentModels, paymentModel, model.PendingForPayments);
                    if (!res.Succeeded)
                        return res;
                }

                paymentModel.Amount = paymentModel.PaymentPendingForPayments.Sum(a => a.PaymentAmount);
                // add attachment



                // add financialAccount
                if (paymentModel.Status == PaymentStatus.Confirm)
                    paymentModel = AddFinancialAccountByPayment(paymentModel);

                // generate form code
                var count = await _paymentRepository.CountAsync(a => a.ContractCode == authenticate.ContractCode);
                var codeRes = await _formConfigService.GenerateFormCodeAsync(authenticate.ContractCode, FormName.Payment, count);
                if (!codeRes.Succeeded)
                    return ServiceResultFactory.CreateError(false, codeRes.Messages.First().Message);
                paymentModel.PaymentNumber = codeRes.Result;
                _paymentRepository.Add(paymentModel);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var fullPayedPendingForPaymentModels = pendingForPaymentModels
                        .Where(a => a.AmountRemained == 0)
                        .ToList();
                    await UpdateIsPaymentPo(pos);
                    if (fullPayedPendingForPaymentModels.Any())
                    {
                        await _scmLogAndNotificationService
                            .SetDonedNotificationAsync(authenticate.UserId, paymentModel.ContractCode,
                            fullPayedPendingForPaymentModels.Select(c => c.Id.ToString()).ToList(), NotifEvent.AddPayment);
                    }
                    int? userId = null;
                    if (paymentModel.Status != PaymentStatus.Confirm)
                    {
                        userId = paymentModel.PaymentConfirmationWorkFlows.First().PaymentConfirmationWorkFlowUsers.First(a => a.IsBallInCourt).UserId;
                    }

                    var res12 = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = paymentModel.ContractCode,
                        FormCode = paymentModel.PaymentNumber,
                        KeyValue = paymentModel.PaymentId.ToString(),
                        NotifEvent = NotifEvent.AddPayment,
                        Description=(paymentModel.Status==PaymentStatus.Confirm)?"2":"1",
                        RootKeyValue = paymentModel.PaymentId.ToString(),
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName
                    }, NotifEvent.ConfirmPayment, userId);

                    return ServiceResultFactory.CreateSuccess(true);
                }
                return ServiceResultFactory.CreateError(false, MessageId.AddEntityFailed);
            }

            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<List<PaymentListDto>>> GetPaymentAsync(AuthenticateDto authenticate, PaymentQueryDto query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<PaymentListDto>>(null, MessageId.AccessDenied);

                var dbQuery = _paymentRepository.Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode);

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a => a.Supplier.Name.Contains(query.SearchText) || a.PaymentNumber.Contains(query.SearchText));

                var totalCount = dbQuery.Count();
                var columnsMap = new Dictionary<string, Expression<Func<Payment, object>>>
                {
                    ["PaymentId"] = v => v.PaymentId,
                    ["PaymentNumber"] = v => v.PaymentNumber,
                    ["SupplierName"] = v => v.Supplier.Name
                };

                dbQuery = dbQuery.ApplayOrdering(query, columnsMap).ApplayPageing(query);

                var result = await dbQuery.Select(c => new PaymentListDto
                {
                    PaymentId = c.PaymentId,
                    DateCreate = c.CreatedDate.ToUnixTimestamp(),
                    Amount = c.Amount,
                    Note = c.Note,
                    PaymentNumber = c.PaymentNumber,
                    CurrencyType = c.CurrencyType,
                    //SupplierCode = c.Supplier.SupplierCode,
                    SupplierId = c.SupplierId,
                    SupplierName = c.Supplier.Name,
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<PaymentListDto>>(null, exception);
            }
        }
        public async Task<ServiceResult<List<PaymentListDto>>> GetPaymentListAsync(AuthenticateDto authenticate, PaymentQueryDto query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<PaymentListDto>>(null, MessageId.AccessDenied);

                var dbQuery = _paymentRepository.Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode);

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a => a.Supplier.Name.Contains(query.SearchText) || a.PaymentNumber.Contains(query.SearchText));

                var totalCount = dbQuery.Count();
                var columnsMap = new Dictionary<string, Expression<Func<Payment, object>>>
                {
                    ["PaymentId"] = v => v.PaymentId,
                    ["PaymentNumber"] = v => v.PaymentNumber,
                    ["SupplierName"] = v => v.Supplier.Name
                };

                dbQuery = dbQuery.ApplayOrdering(query, columnsMap).ApplayPageing(query);

                var result = await dbQuery.Select(c => new PaymentListDto
                {
                    PaymentId = c.PaymentId,
                    DateCreate = c.CreatedDate.ToUnixTimestamp(),
                    Amount = c.Amount,
                    Note = c.Note,
                    PaymentNumber = c.PaymentNumber,
                    CurrencyType = c.CurrencyType,
                    //SupplierCode = c.Supplier.SupplierCode,
                    SupplierId = c.SupplierId,
                    SupplierName = c.Supplier.Name,
                    Status=c.Status
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<PaymentListDto>>(null, exception);
            }
        }
        public async Task<ServiceResult<List<PendingForConfirmPaymentListDto>>> GetPendingForConfirmPaymentAsync(AuthenticateDto authenticate, PaymentQueryDto query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<PendingForConfirmPaymentListDto>>(null, MessageId.AccessDenied);

                var dbQuery = _paymentRepository.Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.Status == PaymentStatus.Register && a.PaymentConfirmationWorkFlows.Any(a => !a.IsDeleted && a.Status == ConfirmationWorkFlowStatus.Pending));

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a => a.Supplier.Name.Contains(query.SearchText) || a.PaymentNumber.Contains(query.SearchText));

                var totalCount = dbQuery.Count();
                var columnsMap = new Dictionary<string, Expression<Func<Payment, object>>>
                {
                    ["PaymentId"] = v => v.PaymentId,
                    ["PaymentNumber"] = v => v.PaymentNumber,
                    ["SupplierName"] = v => v.Supplier.Name
                };

                dbQuery = dbQuery.ApplayOrdering(query, columnsMap).ApplayPageing(query);

                var result = await dbQuery.Select(c => new PendingForConfirmPaymentListDto
                {
                    PaymentId = c.PaymentId,
                    DateCreate = c.CreatedDate.ToUnixTimestamp(),
                    Amount = c.Amount,
                    Note = c.Note,
                    PaymentNumber = c.PaymentNumber,
                    CurrencyType = c.CurrencyType,
                    //SupplierCode = c.Supplier.SupplierCode,
                    SupplierId = c.SupplierId,
                    SupplierName = c.Supplier.Name,
                    UserAudit = c.AdderUser != null
                        ? new UserAuditLogDto
                        {
                            AdderUserId = c.AdderUserId,
                            AdderUserName = c.AdderUser.FullName,
                            CreateDate = c.CreatedDate.ToUnixTimestamp(),
                            AdderUserImage = (!String.IsNullOrEmpty(c.AdderUser.Image)) ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall +
                                             c.AdderUser.Image : ""
                        }
                        : null,
                    BallInCourtUser = c.PaymentConfirmationWorkFlows.Any() ?
                    c.PaymentConfirmationWorkFlows.First().PaymentConfirmationWorkFlowUsers.Where(a => a.IsBallInCourt)
                    .Select(d => new UserAuditLogDto
                    {
                        AdderUserId = d.UserId,
                        AdderUserName = d.User.FirstName + " " + d.User.LastName,
                        AdderUserImage = d.User.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + d.User.Image : ""
                    }).FirstOrDefault() : new UserAuditLogDto()
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<PendingForConfirmPaymentListDto>>(null, exception);
            }
        }
        public async Task<ServiceResult<PaymentInfoDto>> GetPaymentByIdAsync(AuthenticateDto authenticate, long paymentId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<PaymentInfoDto>(null, MessageId.AccessDenied);

                var dbQuery = _paymentRepository.Where(a => !a.IsDeleted && a.PaymentId == paymentId && a.ContractCode == authenticate.ContractCode)
                    .AsQueryable();

                if (dbQuery == null)
                    return ServiceResultFactory.CreateError<PaymentInfoDto>(null, MessageId.EntityDoesNotExist);

                return await ReturnPaymentAsync(dbQuery);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<PaymentInfoDto>(null, exception);
            }
        }

        public async Task<ServiceResult<PaymentInfoWithWorkFlowDto>> GetPaymentInfoByIdAsync(AuthenticateDto authenticate, long paymentId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<PaymentInfoWithWorkFlowDto>(null, MessageId.AccessDenied);

                var dbQuery = _paymentRepository.Where(a => !a.IsDeleted && a.PaymentId == paymentId && a.ContractCode == authenticate.ContractCode)
                    .AsQueryable();

                if (dbQuery == null)
                    return ServiceResultFactory.CreateError<PaymentInfoWithWorkFlowDto>(null, MessageId.EntityDoesNotExist);

                var result = await dbQuery
                .Select(p => new PaymentInfoWithWorkFlowDto
                {
                    PaymentId = p.PaymentId,
                    Amount = p.Amount,
                    Note = p.Note,
                    PaymentNumber = p.PaymentNumber,
                    SupplierId = p.SupplierId,
                    CurrencyType = p.CurrencyType,
                    DateCreate = p.CreatedDate.ToUnixTimestamp(),
                    SupplierCode = p.Supplier.SupplierCode,
                    SupplierLogo = p.Supplier.Logo != null ? _appSettings.ClientHost + ServiceSetting.UploadImagesPath.LogoSmall + p.Supplier.Logo : "",
                    SupplierName = p.Supplier.Name,
                    UserAudit = p.AdderUser != null ? new UserAuditLogDto
                    {
                        AdderUserName = p.AdderUser.FullName,
                        AdderUserImage = p.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + p.AdderUser.Image : "",
                        CreateDate = p.CreatedDate.ToUnixTimestamp()
                    } : null,
                    PendingForPayments = p.PaymentPendingForPayments.Select(pp => new PendingForPaymentForPayedDto
                    {
                        PendingForPaymentId = pp.PendingForPaymentId,
                        PendingForPaymentNumber = pp.PendingForPayment.PendingForPaymentNumber,
                        PaymentDateTime = pp.PendingForPayment.PaymentDateTime.ToUnixTimestamp(),
                        Amount = pp.PendingForPayment.Amount,
                        AmountPayed = pp.PendingForPayment.AmountPayed,
                        AmountRemained = pp.PendingForPayment.AmountRemained,
                        PaymentAmount = pp.PaymentAmount,
                        InvoiceNumber = pp.PendingForPayment.Invoice != null ? pp.PendingForPayment.Invoice.InvoiceNumber : "",
                        CurrencyType = p.CurrencyType,
                        PaymentStep = (pp.PendingForPayment.POTermsOfPayment != null) ? pp.PendingForPayment.POTermsOfPayment.PaymentStep : TermsOfPaymentStep.Tax,
                        POCode = pp.PendingForPayment.PO != null ? pp.PendingForPayment.PO.POCode : "",
                        PRContractCode = pp.PendingForPayment.PO != null ? pp.PendingForPayment.PO.PRContract.PRContractCode : "",
                    }).ToList(),
                    PaymentConfirmationUserWorkFlows = (p.PaymentConfirmationWorkFlows.FirstOrDefault() != null) ? p.PaymentConfirmationWorkFlows.First().PaymentConfirmationWorkFlowUsers.Where(a => a.UserId > 0)
                            .Select(e => new PaymentConfirmationUserWorkFlowDto
                            {
                                PaymentConfirmationWorkFlowUserId = e.PaymentConfirmWorkFlowUserId,
                                IsAccept = e.IsAccept,
                                IsBallInCourt = e.IsBallInCourt,
                                Note = e.Note,
                                OrderNumber = e.OrderNumber,
                                UserId = e.UserId,
                                UserFullName = e.User != null ? e.User.FullName : "",
                                UserImage = e.User != null && e.User.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + e.User.Image : "",
                                DateStart = e.DateStart.ToUnixTimestamp(),
                                DateEnd = e.DateEnd.ToUnixTimestamp()
                            }).ToList() : new List<PaymentConfirmationUserWorkFlowDto>(),
                }).FirstOrDefaultAsync();
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<PaymentInfoWithWorkFlowDto>(null, exception);
            }
        }
        public async Task<ServiceResult<bool>> CancelPendingForPaymentByPendingForPaymentIdAsync(AuthenticateDto authenticate, long pendingForPaymentId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery = _pendingForPaymentRepository.Include(a => a.PaymentPendingForPayments).Include(a => a.PO).Where(a => !a.IsDeleted && a.Id == pendingForPaymentId && a.PO.BaseContractCode == authenticate.ContractCode);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                var pendingPayment = await dbQuery.FirstOrDefaultAsync();
                if (pendingPayment == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (pendingPayment.PO.POStatus == POStatus.Canceled)
                    return ServiceResultFactory.CreateError(false, MessageId.CantDoneBecausePOCanceled);

                if (pendingPayment.PaymentPendingForPayments.Any())
                    return ServiceResultFactory.CreateError(false, MessageId.PendingForPaymentHasPayment);

                pendingPayment.Status = POPaymentStatus.Canceled;



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

        public async Task<ServiceResult<List<UserMentionDto>>> GetConfirmationUserListAsync(AuthenticateDto authenticate)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<UserMentionDto>>(null, MessageId.AccessDenied);

                var roles = new List<string> { SCMRole.PaymentConfirm };
                var list = await _authenticationService.GetAllUserHasAccessPurchaseAsync(authenticate.ContractCode, roles);

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

        public async Task<ServiceResult<List<SupplierMiniInfoDto>>> GetSuppliersAsync(AuthenticateDto authenticate, SupplierQuery query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<SupplierMiniInfoDto>>(null, MessageId.AccessDenied);

                var dbQuery = _pendingForPaymentRepository
                    .Where(a => !a.IsDeleted && a.PO.BaseContractCode == authenticate.ContractCode && a.Status != POPaymentStatus.Settled && a.Status != POPaymentStatus.Canceled);

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a => a.Supplier.Name.Contains(query.SearchText) || a.Supplier.SupplierCode.Contains(query.SearchText));

                if (query.ProductGroupIds != null && query.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(a => a.Supplier.SupplierProductGroups.Any(c => query.ProductGroupIds.Contains(c.ProductGroupId)));



                var result = await dbQuery.Select(a => new SupplierMiniInfoDto
                {
                    Id = a.Supplier.Id,
                    Email = a.Supplier.Email,
                    Name = a.Supplier.Name,
                    SupplierCode = a.Supplier.SupplierCode,
                    TellPhone = a.Supplier.TellPhone,
                    Logo = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.LogoSmall + a.Supplier.Logo,
                    ProductGroups = null
                }).ToListAsync();
                List<SupplierMiniInfoDto> supplierResult = new List<SupplierMiniInfoDto>();
                foreach (var item in result)
                {
                    if (!supplierResult.Any(a => a.Id == item.Id))
                    {
                        supplierResult.Add(item);
                    }
                }

                return ServiceResultFactory.CreateSuccess(supplierResult);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new List<SupplierMiniInfoDto>(), exception);
            }
        }
        public async Task<ServiceResult<PaymentConfirmationWorkflowDto>> GetPendingConfirmPaymentByPaymentIdAsync(AuthenticateDto authenticate, long paymentId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<PaymentConfirmationWorkflowDto>(null, MessageId.AccessDenied);

                var dbQuery = _paymentConfirmRepository
                     .AsNoTracking()
                     .Where(a => !a.IsDeleted &&
                   a.PaymentId == paymentId &&
                    a.Status == ConfirmationWorkFlowStatus.Pending &&
                     a.Payment.ContractCode == authenticate.ContractCode &&
                     a.Payment.Status == PaymentStatus.Register);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<PaymentConfirmationWorkflowDto>(null, MessageId.EntityDoesNotExist);

               

                var result = await dbQuery
                        .Select(x => new PaymentConfirmationWorkflowDto
                        {
                            ConfirmNote = x.ConfirmNote,
                            Amount=x.Payment.Amount,
                            CurrencyType=x.Payment.CurrencyType,
                            PaymentNumber=x.Payment.PaymentNumber,
                            SupplierName=x.Payment.Supplier.Name,
                            SupplierLogo = x.Payment.Supplier.Logo != null ? _appSettings.ClientHost + ServiceSetting.UploadImagesPath.LogoSmall + x.Payment.Supplier.Logo : "",
                            PaymentConfirmUserAudit = x.AdderUserId != null ? new UserAuditLogDto
                            {
                                CreateDate = x.CreatedDate.ToUnixTimestamp(),
                                AdderUserName = x.AdderUser.FullName,
                                AdderUserImage = x.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + x.AdderUser.Image : ""
                            } : null,
                           
                            PaymentItems = x.Payment.PaymentPendingForPayments.Select(pp => new PendingForPaymentForPayedDto
                            {
                                PendingForPaymentId = pp.PendingForPaymentId,
                                PendingForPaymentNumber = pp.PendingForPayment.PendingForPaymentNumber,
                                PaymentDateTime = pp.PendingForPayment.PaymentDateTime.ToUnixTimestamp(),
                                Amount = pp.PendingForPayment.Amount,
                                AmountPayed = pp.PendingForPayment.AmountPayed,
                                AmountRemained = pp.PendingForPayment.AmountRemained,
                                PaymentAmount = pp.PaymentAmount,
                                InvoiceNumber = pp.PendingForPayment.Invoice != null ? pp.PendingForPayment.Invoice.InvoiceNumber : "",
                                CurrencyType = x.Payment.CurrencyType,
                                PaymentStep=(pp.PendingForPayment.POTermsOfPayment!=null)?pp.PendingForPayment.POTermsOfPayment.PaymentStep:TermsOfPaymentStep.Tax,
                                POCode = pp.PendingForPayment.PO != null ? pp.PendingForPayment.PO.POCode : "",
                                PRContractCode = pp.PendingForPayment.PO != null ? pp.PendingForPayment.PO.PRContract.PRContractCode : "",
                            }).ToList(),
                            PaymentConfirmationUserWorkFlows = x.PaymentConfirmationWorkFlowUsers.Where(a => a.UserId > 0)
                            .Select(e => new PaymentConfirmationUserWorkFlowDto
                            {
                                PaymentConfirmationWorkFlowUserId = e.PaymentConfirmWorkFlowUserId,
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
                return ServiceResultFactory.CreateException<PaymentConfirmationWorkflowDto>(null, exception);
            }
        }

        public async Task<ServiceResult<List<PendingForConfirmPaymentListDto>>> SetUserConfirmOwnPurchaseRequestTaskAsync(AuthenticateDto authenticate, long paymentId, AddPaymentConfirmationAnswerDto model)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<PendingForConfirmPaymentListDto>>(null, MessageId.AccessDenied);

                var dbQuery = _paymentConfirmRepository
                     .Where(a => !a.IsDeleted &&
                    a.PaymentId == paymentId &&
                    a.Status == ConfirmationWorkFlowStatus.Pending &&
                    a.Payment.ContractCode == authenticate.ContractCode &&
                    a.Payment.Status == PaymentStatus.Register)
                     .Include(a => a.PaymentConfirmationWorkFlowUsers)
                     .ThenInclude(c => c.User)
                     .Include(a => a.Payment)
                     .ThenInclude(a => a.PaymentPendingForPayments)
                     .ThenInclude(a => a.PendingForPayment)
                     .AsQueryable();


                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<List<PendingForConfirmPaymentListDto>>(null, MessageId.EntityDoesNotExist);



                var confirmationModel = await dbQuery.FirstOrDefaultAsync();

                if (confirmationModel == null)
                    return ServiceResultFactory.CreateError<List<PendingForConfirmPaymentListDto>>(null, MessageId.EntityDoesNotExist);

                if (confirmationModel.PaymentConfirmationWorkFlowUsers == null && !confirmationModel.PaymentConfirmationWorkFlowUsers.Any())
                    return ServiceResultFactory.CreateError<List<PendingForConfirmPaymentListDto>>(null, MessageId.DataInconsistency);

                if (!confirmationModel.PaymentConfirmationWorkFlowUsers.Any(c => c.UserId == authenticate.UserId && c.IsBallInCourt))
                    return ServiceResultFactory.CreateError<List<PendingForConfirmPaymentListDto>>(null, MessageId.AccessDenied);


                var userBallInCourtModel = confirmationModel.PaymentConfirmationWorkFlowUsers.FirstOrDefault(a => a.IsBallInCourt && a.UserId == authenticate.UserId);
                userBallInCourtModel.DateEnd = DateTime.UtcNow;
                if (model.IsAccept)
                {
                    userBallInCourtModel.IsBallInCourt = false;
                    userBallInCourtModel.IsAccept = true;
                    userBallInCourtModel.Note = model.Note;
                    if (!confirmationModel.PaymentConfirmationWorkFlowUsers.Any(a => a.IsAccept == false))
                    {
                        confirmationModel.Status = ConfirmationWorkFlowStatus.Confirm;
                        confirmationModel.Payment.Status = PaymentStatus.Confirm;
                        confirmationModel.Payment = AddFinancialAccountByPayment(confirmationModel.Payment);
                        foreach (var item in confirmationModel.Payment.PaymentPendingForPayments)
                        {
                            if (item.PendingForPayment.AmountRemained == 0)
                                item.PendingForPayment.Status = POPaymentStatus.Settled;

                        }


                    }
                    else
                    {
                        var nextBallInCourtModel = confirmationModel.PaymentConfirmationWorkFlowUsers.Where(a => !a.IsAccept)
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

                    confirmationModel.Payment.Status = PaymentStatus.Reject;
                    foreach (var item in confirmationModel.Payment.PaymentPendingForPayments)
                    {
                        item.PendingForPayment.AmountPayed -= item.PaymentAmount;
                        item.PendingForPayment.AmountRemained += item.PaymentAmount;
                    }
                }

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    await _scmLogAndNotificationService.SetDonedNotificationAsync(authenticate.UserId, confirmationModel.Payment.ContractCode, confirmationModel.Payment.PaymentId.ToString(), NotifEvent.ConfirmPayment);
                    int? userId = null;
                    if (confirmationModel.Payment.Status != PaymentStatus.Confirm)
                    {
                        userId = confirmationModel.Payment.PaymentConfirmationWorkFlows.First().PaymentConfirmationWorkFlowUsers.First(a => a.IsBallInCourt).UserId;
                    }

                    var res12 = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = confirmationModel.Payment.ContractCode,
                        FormCode = confirmationModel.Payment.PaymentNumber,
                        KeyValue = confirmationModel.Payment.PaymentId.ToString(),
                        Description = confirmationModel.Payment.Status == PaymentStatus.Reject ? "3" : "2",
                        NotifEvent = NotifEvent.ConfirmPayment,
                        RootKeyValue = confirmationModel.Payment.PaymentId.ToString(),
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName
                    }, NotifEvent.ConfirmPayment, userId);
                    var result = await GetPendingForConfirmPaymentAsync(authenticate,new PaymentQueryDto { Page=1,PageSize=9999});
                    if (!result.Succeeded)
                        return ServiceResultFactory.CreateError<List<PendingForConfirmPaymentListDto>>(null, MessageId.OperationFailed);
                    else
                        return ServiceResultFactory.CreateSuccess(result.Result);
                }
                return ServiceResultFactory.CreateError<List<PendingForConfirmPaymentListDto>>(null, MessageId.SaveFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<PendingForConfirmPaymentListDto>>(null, exception);
            }
        }

        //private async Task<ServiceResult<bool>> AddPendingForPaymentBaseOnPercentageOFTermsOfPayment(PO poModel, POTermsOfPayment thisPaymentStep)
        //{

        //    if (await _pendingForPaymentRepository.AnyAsync(a => a.POId == poModel.POId && a.POTermsOfPaymentId == thisPaymentStep.Id))
        //        return ServiceResultFactory.CreateError(false, MessageId.ThisStateSetBefore);

        //    var amountPayment = (thisPaymentStep.PaymentPercentage * poModel.TotalAmount) / 100;
        //    var datePayment = DateTime.UtcNow;

        //    if (thisPaymentStep.IsCreditPayment)
        //        datePayment.AddDays(thisPaymentStep.CreditDuration);

        //    var pendigToPaymentModel = new PendingForPayment
        //    {
        //        Amount = amountPayment,
        //        AmountPayed = 0,
        //        AmountRemained = amountPayment,
        //        PaymentDateTime = datePayment,
        //        PRContractId = poModel.PRContractId,
        //        POId = poModel.POId,
        //        POTermsOfPaymentId = thisPaymentStep.Id,
        //        SupplierId = poModel.SupplierId,
        //        Status = POPaymentStatus.NotSettled,
        //        BaseContractCode = poModel.BaseContractCode
        //    };
        //    pendigToPaymentModel.PendingForPaymentNumber = CodeGenerator.SCMFormCodeGenerator(await _pendingForPaymentRepository.CountAsync(), SCMForm.PendingToPayment);

        //    _pendingForPaymentRepository.Add(pendigToPaymentModel);
        //    return await _unitOfWork.SaveChangesAsync() > 0
        //        ? ServiceResultFactory.CreateSuccess(true)
        //        : ServiceResultFactory.CreateError(false, MessageId.SaveFailed);
        //}


        private async Task<ServiceResult<PaymentInfoDto>> ReturnPaymentAsync(IQueryable<Payment> dbQuery)
        {
            try
            {
                var result = await dbQuery
                .Select(p => new PaymentInfoDto
                {
                    PaymentId = p.PaymentId,
                    Amount = p.Amount,
                    Note = p.Note,
                    PaymentNumber = p.PaymentNumber,
                    SupplierId = p.SupplierId,
                    CurrencyType = p.CurrencyType,
                    DateCreate = p.CreatedDate.ToUnixTimestamp(),
                    SupplierCode = p.Supplier.SupplierCode,
                    SupplierLogo = p.Supplier.Logo != null ? _appSettings.ClientHost + ServiceSetting.UploadImagesPath.LogoSmall + p.Supplier.Logo : "",
                    SupplierName = p.Supplier.Name,
                    UserAudit = p.AdderUser != null ? new UserAuditLogDto
                    {
                        AdderUserName = p.AdderUser.FullName,
                        AdderUserImage = p.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + p.AdderUser.Image : "",
                        CreateDate = p.CreatedDate.ToUnixTimestamp()
                    } : null,
                    PaymentAttachment = p.PaymentAttachments.Select(s => new BasePaymentAttachmentDto
                    {
                        FileName = s.FileName,
                        Id = s.Id,
                        FileSize = s.FileSize,
                        FileSrc = s.FileSrc,
                        FileType = s.FileType,
                        PaymentId = s.PaymentId.Value,
                        Title = s.Title
                    }).ToList(),
                    PendingForPayments = p.PaymentPendingForPayments.Select(pp => new PendingForPaymentForPayedDto
                    {
                        PendingForPaymentId = pp.PendingForPaymentId,
                        PendingForPaymentNumber = pp.PendingForPayment.PendingForPaymentNumber,
                        PaymentDateTime = pp.PendingForPayment.PaymentDateTime.ToUnixTimestamp(),
                        Amount = pp.PendingForPayment.Amount,
                        AmountPayed = pp.PendingForPayment.AmountPayed,
                        AmountRemained = pp.PendingForPayment.AmountRemained,
                        PaymentAmount = pp.PaymentAmount,
                        InvoiceNumber = pp.PendingForPayment.Invoice != null ? pp.PendingForPayment.Invoice.InvoiceNumber : "",
                        CurrencyType = p.CurrencyType,
                        POCode = pp.PendingForPayment.PO != null ? pp.PendingForPayment.PO.POCode : "",
                        PRContractCode = pp.PendingForPayment.PO != null ? pp.PendingForPayment.PO.PRContract.PRContractCode : "",
                    }).ToList(),
                }).FirstOrDefaultAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<PaymentInfoDto>(null, exception);
            }

        }

        private Payment AddFinancialAccountByPayment(Payment paymentModel)
        {
            paymentModel.FinancialAccount = new FinancialAccount();
            var financialModel = new FinancialAccount
            {
                DateDone = DateTime.UtcNow,
                PaymentId = paymentModel.PaymentId,
                CurrencyType = paymentModel.CurrencyType,
                FinancialAccountType = FinancialAccountType.Payment,
                SupplierId = paymentModel.SupplierId
            };

            var someReminded = _financialAccountRepository
                .Where(a => a.SupplierId == paymentModel.SupplierId && a.CurrencyType == paymentModel.CurrencyType)
                .Sum(v => (v.PurchaseAmount - (v.RejectPurchaseAmount + v.PaymentAmount)));

            financialModel.PaymentAmount = paymentModel.Amount ;
            financialModel.PurchaseAmount = 0;
            financialModel.RemainedAmount = someReminded - financialModel.PaymentAmount;
            paymentModel.FinancialAccount = financialModel;
            return paymentModel;
        }

        private async Task<List<ListPendingForPaymentDto>> ReturnPendingForPaymentListAsync(IQueryable<PendingForPayment> dbQuery)
        {
            return await dbQuery.Select(c => new ListPendingForPaymentDto
            {
                PendingForPaymentId = c.Id,
                Amount = c.Amount,
                InvoiceNumber = c.Invoice != null ? c.Invoice.InvoiceNumber : "",
                InvoiceId = c.InvoiceId,
                PaymentDateTime = c.PaymentDateTime.ToUnixTimestamp(),
                POCode = c.PO != null ? c.PO.POCode : "",
                POId = c.POId,
                PRContractId = c.PRContractId,
                PendingOFPeymentStatus = c.Status == POPaymentStatus.Settled ?
                PendingOFPeymentStatus.Settled : c.PaymentDateTime.Date >= DateTime.UtcNow.Date
                ? PendingOFPeymentStatus.OverDue : PendingOFPeymentStatus.NotOverDue,
                PaymentStep = c.POTermsOfPayment.PaymentStep,
                PendingForPaymentNumber = c.PendingForPaymentNumber,
                AmountPayed = c.AmountPayed,
                AmountRemained = c.AmountRemained,
                SupplierId = c.SupplierId,
                SupplierName = c.Supplier.Name,
                SupplierCode = c.Supplier.SupplierCode,
                PRContractCode = c.PO != null ? c.PO.PRContract.PRContractCode : "",
                CurrencyType = c.PO.CurrencyType,
                UserAudit = c.AdderUser != null ? new UserAuditLogDto
                {
                    AdderUserName = c.AdderUser.FullName,
                    AdderUserImage = c.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + c.AdderUser.Image : "",
                    CreateDate = c.CreatedDate.ToUnixTimestamp(),
                } : null
            }).ToListAsync();
        }

        private static ServiceResult<bool> UpdatePendingForPaymentAmountandAddPaymentPendingForPayment(List<PendingForPayment> pendingForPaymentModels, Payment paymentModel, List<PaymentSubjectDto> postedPendingForPayments)
        {
            foreach (var item in pendingForPaymentModels)
            {
                var postedSubject = postedPendingForPayments.FirstOrDefault(a => a.PendingForPaymentId == item.Id);
                if (postedSubject == null)
                    return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                if (item.AmountRemained >= postedSubject.PaymentAmount)
                {
                    item.AmountPayed += postedSubject.PaymentAmount;
                    item.AmountRemained -= postedSubject.PaymentAmount;
                    paymentModel.PaymentPendingForPayments.Add(new PaymentPendingForPayment
                    {
                        PendingForPaymentId = item.Id,
                        PaymentAmount = postedSubject.PaymentAmount,
                    });
                }
                else
                {
                    return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);
                }
            }

            return ServiceResultFactory.CreateSuccess(true);
        }
        private static ServiceResult<bool> UpdatePendingForPaymentAmountandConfirmPaymentPendingForPayment(List<PendingForPayment> pendingForPaymentModels, Payment paymentModel, List<PaymentSubjectDto> postedPendingForPayments)
        {
            foreach (var item in pendingForPaymentModels)
            {
                var postedSubject = postedPendingForPayments.FirstOrDefault(a => a.PendingForPaymentId == item.Id);
                if (postedSubject == null)
                    return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                if (item.AmountRemained >= postedSubject.PaymentAmount)
                {
                    item.AmountPayed += postedSubject.PaymentAmount;
                    item.AmountRemained -= postedSubject.PaymentAmount;
                    paymentModel.PaymentPendingForPayments.Add(new PaymentPendingForPayment
                    {
                        PendingForPaymentId = item.Id,
                        PaymentAmount = postedSubject.PaymentAmount,
                    });

                    if (item.AmountRemained == 0)
                    {
                        item.Status = POPaymentStatus.Settled;

                    }


                }
                else
                {
                    return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);
                    //paymentModel.PaymentPendingForPayments.Add(new PaymentPendingForPayment
                    //{
                    //    PendingForPaymentId = item.Id,
                    //    PaymentAmount = item.AmountRemained
                    //});
                    //item.AmountPayed = item.Amount;
                    //item.AmountRemained = 0;
                    //paymentAmount -= item.AmountRemained;
                    //item.Status = POPaymentStatus.Settled;
                }
            }

            return ServiceResultFactory.CreateSuccess(true);
        }
        private async Task<ServiceResult<Payment>> AddPaymentAttachmentAsync(Payment payment, List<AddAttachmentDto> attachment)
        {
            payment.PaymentAttachments = new List<PaymentAttachment>();



            foreach (var item in attachment)
            {
                var UploadedFile =
                    await _fileHelper.SaveDocumentFromTemp(item.FileSrc, ServiceSetting.UploadFilePath.Payment);
                if (UploadedFile == null)
                    return ServiceResultFactory.CreateError<Payment>(null, MessageId.UploudFailed);

                _fileHelper.DeleteDocumentFromTemp(item.FileSrc);
                payment.PaymentAttachments.Add(new PaymentAttachment
                {
                    FileType = UploadedFile.FileType,
                    FileSize = UploadedFile.FileSize,
                    FileName = item.FileName,
                    FileSrc = item.FileSrc,
                });
            }

            return ServiceResultFactory.CreateSuccess(payment);
        }
        private async Task UpdateIsPaymentPo(List<PO> pos)
        {
            try
            {
                bool saveChange = false;
                foreach (var item in pos)
                {
                    var pendingForPayments = await _pendingForPaymentRepository.Where(a => a.POId == item.POId).ToListAsync();
                    if (pendingForPayments.Where(a => !a.IsDeleted && a.Status == POPaymentStatus.Settled).Sum(a => a.AmountPayed) == item.FinalTotalAmount)
                    {
                        saveChange = true;
                        item.IsPaymentDone = true;
                        _poRepository.Update(item);

                    }

                }
                if (saveChange)
                    await _unitOfWork.SaveChangesAsync();
            }
            catch
            {

            }
        }

        private async Task<ServiceResult<PaymentConfirmationWorkFlow>> AddPaymentConfirmationAsync(AddPaymentConfirmationDto model)
        {
            //if (usersIds.Count() == 0)
            //    return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);
            var paymentConfirmationModel = new PaymentConfirmationWorkFlow
            {

                ConfirmNote = model.Note,
                Status = ConfirmationWorkFlowStatus.Pending,
                PaymentConfirmationWorkFlowUsers = new List<PaymentConfirmationWorkFlowUser>(),
            };

            if (model.Users != null && model.Users.Any())
            {
                var usersIds = model.Users.Select(a => a.UserId).Distinct().ToList();
                if (await _userRepository.CountAsync(a => !a.IsDeleted && a.IsActive && usersIds.Contains(a.Id)) != usersIds.Count())
                    return ServiceResultFactory.CreateError<PaymentConfirmationWorkFlow>(null, MessageId.DataInconsistency);

                foreach (var item in model.Users)
                {
                    paymentConfirmationModel.PaymentConfirmationWorkFlowUsers.Add(new PaymentConfirmationWorkFlowUser
                    {
                        UserId = item.UserId,
                        OrderNumber = item.OrderNumber
                    });
                }
                if (paymentConfirmationModel.PaymentConfirmationWorkFlowUsers.Any())
                {
                    var bollincourtUser = paymentConfirmationModel.PaymentConfirmationWorkFlowUsers.OrderBy(a => a.OrderNumber).First();
                    bollincourtUser.IsBallInCourt = true;
                    bollincourtUser.DateStart = DateTime.UtcNow;
                }
            }
            else
            {
                paymentConfirmationModel.Status = ConfirmationWorkFlowStatus.Confirm;
            }



            return ServiceResultFactory.CreateSuccess(paymentConfirmationModel);
        }


    }
}
