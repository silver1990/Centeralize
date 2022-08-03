using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Raybod.SCM.DataAccess.Core;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.FinancialAccount;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Model;
using Raybod.SCM.Domain.View;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Services.Utilitys.exportToExcel;
using Raybod.SCM.Utility.Extention;

namespace Raybod.SCM.Services.Implementation
{
    public class FinancialAccountService : IFinancialAccountService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITeamWorkAuthenticationService _authenticationService;
        private readonly DbSet<FinancialAccount> _financialAccountRepository;
        private readonly DbSet<PO> _poRepository;
        private readonly DbSet<FinancialAccountBaseOnSupplier> _financialAccountBaseOnSupplierView;
        private readonly CompanyAppSettingsDto _appSettings;

        public FinancialAccountService(IUnitOfWork unitOfWork, ITeamWorkAuthenticationService authenticationService,
            IOptions<CompanyAppSettingsDto> appSettings)
        {
            _unitOfWork = unitOfWork;
            _authenticationService = authenticationService;
            _poRepository = _unitOfWork.Set<PO>();
            _financialAccountRepository = _unitOfWork.Set<FinancialAccount>();
            _financialAccountBaseOnSupplierView = _unitOfWork.Query<FinancialAccountBaseOnSupplier>();
            _appSettings = appSettings.Value;
        }

        public async Task<ServiceResult<List<FinancialAccountBaseOnSupplier>>> GetFinancialAccountBaseONSupplierAsync()
        {
            try
            {
                List<FinancialAccountBaseOnSupplier> result = await _financialAccountBaseOnSupplierView
                    .AsNoTracking()
                    .ToListAsync();

                foreach (var item in result)
                {
                    item.Logo = _appSettings.ClientHost + ServiceSetting.UploadImagesPath.LogoSmall + item.Logo;
                }

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<FinancialAccountBaseOnSupplier>>(null, exception);
            }
        }

        public async Task<DownloadFileDto> ExportExcelFinancialAccountBaseONSupplierAsync()
        {
            try
            {
                List<FinancialAccountBaseOnSupplier> query = await _financialAccountBaseOnSupplierView
                    .AsNoTracking()
                    .ToListAsync();
                int i = 1;
                var result = query.Select(c => new ExportExcelFinancialAccountDto
                {
                    Name = c.Name,
                    CurrencyType = ReturnCurrencyTypeValue(c.CurrencyType),
                    PaymentAmount = c.PaymentAmount,
                    PurchaseAmount = c.PurchaseAmount,
                    RejectPurchaseAmount = c.RejectPurchaseAmount,
                    RemainedAmount = c.RemainedAmount,
                    Currency=c.CurrencyType,
                }).ToList();
                
                return ExcelHelper.FinancialAccountExportToExcelWithStyle(result, "financialAccount", "financialAccount");
            }
            catch (Exception exception)
            {
                return null;
            }
        }


        private string ReturnCurrencyTypeValue(CurrencyType currencyType)
        {
            switch (currencyType)
            {
                case CurrencyType.IRR:
                    return "ریال";
                case CurrencyType.EUR:
                    return "یورو";
                case CurrencyType.USD:
                    return "دلار";
                default:
                    return "";
            }
        }

        //public async Task<ServiceResult<List<FinancialAccountOfSupplierDto>>> GetFinancialAccountByPoIdAsync(AuthenticateDto authenticate, long poId)
        //{
        //    try
        //    {
        //        var poModel = await _poRepository.Where(a => !a.IsDeleted && a.POId == poId)
        //            .Select(c => new { Id = c.POId, baseContratCode = c.BaseContractCode })
        //            .FirstOrDefaultAsync();
        //        if (poModel == null)
        //            return ServiceResultFactory.CreateError<List<FinancialAccountOfSupplierDto>>(null, MessageId.EntityDoesNotExist);

        //        if (!_authenticationService.HasPermission(authenticate.UserId, poModel.baseContratCode, authenticate.Roles))
        //            return ServiceResultFactory.CreateError<List<FinancialAccountOfSupplierDto>>(null, MessageId.AccessDenied);


        //        var result = await _financialAccountRepository
        //            .Where(c => c.POId == poId)
        //            .Select(a => new FinancialAccountOfSupplierDto
        //            {
        //                Id = a.Id,
        //                DateDone = a.DateDone.ToUnixTimestamp(),
        //                FinancialAccountType = a.FinancialAccountType,
        //                InvoiceId = a.InvoiceId,
        //                PaymentAmount = a.PaymentAmount,
        //                PaymentId = a.PaymentId,
        //                PurchaseAmount = a.PurchaseAmount,
        //                SupplierId = a.SupplierId,
        //                RemainedAmount = a.PaymentAmount,
        //                POId = a.POId,                        
        //                RefNumber = a.FinancialAccountType == FinancialAccountType.Payment && a.Payment != null
        //                ? a.Payment.PaymentNumber : a.Invoice != null
        //                ? a.Invoice.InvoiceNumber : ""
        //            }).AsNoTracking()
        //            .ToListAsync();

        //        return ServiceResultFactory.CreateSuccess(result);
        //    }
        //    catch (Exception exception)
        //    {
        //        return ServiceResultFactory.CreateException<List<FinancialAccountOfSupplierDto>>(null, exception);
        //    }
        //}

        public async Task<DownloadFileDto> GetFinancialAccountBySupplierIdAsync(int supplierId, CurrencyType currencyType)
        {
            try
            {
                var excelData = await _financialAccountRepository
                    .Where(c => c.SupplierId == supplierId && c.CurrencyType == currencyType)
                    .Select(a => new FinancialAccountOfSupplierDto
                    {
                        Id = a.Id,
                        SupplierName=a.Supplier.Name,
                        DateDone = a.DateDone.ToUnixTimestamp(),
                        FinancialAccountType = a.FinancialAccountType,
                        InvoiceId = a.InvoiceId,
                        PaymentAmount =  a.PaymentAmount,
                        PaymentId = a.PaymentId,
                        PurchaseAmount = a.PurchaseAmount,
                        SupplierId = a.SupplierId,
                        RemainedAmount = a.RemainedAmount,
                        POId = a.POId,
                        PurchaseRejectAmount=a.RejectPurchaseAmount,
                        RefNumber = a.FinancialAccountType == FinancialAccountType.Payment && a.Payment != null
                        ? a.Payment.PaymentNumber : a.Invoice != null
                        ? a.Invoice.InvoiceNumber : ""
                    }).AsNoTracking()
                    .ToListAsync();
                var result =  ExcelHelper.SupplierFinancialExportToExcelWithStyle(excelData, $"{excelData.First().SupplierName}-FinancialAccount", excelData.First().SupplierName,(currencyType==CurrencyType.IRR)?"ریالی":"ارزی", currencyType);
                return result;
            }
            catch (Exception exception)
            {
                return null;
            }
        }

    }
}
