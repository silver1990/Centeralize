using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Struct;
using Raybod.SCM.ModuleApi.Helper;
using Raybod.SCM.ModuleApi.Helper.Authentication;
using Raybod.SCM.ModuleApi.Helper.Logger;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Utility.Filters;
using Microsoft.AspNetCore.Http;

namespace Raybod.SCM.ModuleApi.Areas.Financial.Controllers
{
    [Route("api/procurementManagement/financialAccount")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "procurementManagement")]
    public class FinancialAccountController : ControllerBase
    {
        private readonly IFinancialAccountService _financialAccountService;
        private readonly ILogger<FinancialAccountController> _logger;

        public FinancialAccountController(
            IFinancialAccountService financialAccountService,
            ILogger<FinancialAccountController> logger,IHttpContextAccessor httpContextAccessor)
        {
            _financialAccountService = financialAccountService;
            _logger = logger;
           
               
           
        }

        /// <summary>
        /// Get Financial Account Base ON Supplier
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("getFinancialBaseOnSupplier")]
        [Permission(Role = SCMRole.FinancialAccountMng)]
        public async Task<object> GetFinancialAccountBaseONSupplierAsync()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _financialAccountService.GetFinancialAccountBaseONSupplierAsync();
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get all Financial Account og Supplier
        /// </summary>
        /// <param name="supplierId"></param>
        /// <param name="currencyType"></param>
        /// <returns></returns>
        [HttpGet, Route("getFinancialAccountOfSupplier/{supplierId:int}")]
        [Permission(Role = SCMRole.FinancialAccountMng)]
        public async Task<object> GetFinancialAccountBySupplierIdAsync(int supplierId, CurrencyType currencyType)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var streamResult = await _financialAccountService.GetFinancialAccountBySupplierIdAsync(supplierId, currencyType);
            if (streamResult == null)
                return NotFound();

            return File(streamResult.Stream, streamResult.ContentType, streamResult.FileName);
        }

        /// <summary>
        /// Export Excel FinancialAccount Base ON Supplier
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("ExportExcelFinancialAccounts")]
        [Permission(Role = SCMRole.FinancialAccountMng)]
        public async Task<object> ExportExcelFinancialAccountBaseONSupplierAsync()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var streamResult = await _financialAccountService.ExportExcelFinancialAccountBaseONSupplierAsync();
            if (streamResult == null)
                return NotFound();

            return File(streamResult.Stream, streamResult.ContentType, streamResult.FileName);
        }

    }
}