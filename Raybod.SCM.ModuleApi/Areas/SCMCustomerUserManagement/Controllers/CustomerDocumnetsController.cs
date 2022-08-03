using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.DataTransferObject.Document;
using Raybod.SCM.Domain.Struct;
using Raybod.SCM.ModuleApi.Helper;
using Raybod.SCM.ModuleApi.Helper.Logger;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Utility.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Raybod.SCM.ModuleApi.Areas.SCMCustomerUserManagement.Controllers
{
    [Route("api/SCMCustomerDocumentManagement/v1/CustomerDocuments")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "SCMCustomerPanelManagement")]
    public class CustomerDocumentsController : ControllerBase
    {

        private readonly IDocumentService _documentService;
        private readonly IUserService _userService;
        private readonly ILogger<CustomerDocumentsController> _logger;

        public CustomerDocumentsController(IDocumentService documentService, ILogger<CustomerDocumentsController> logger,IHttpContextAccessor httpContextAccessor, IUserService userService)
        {
            _documentService = documentService;
            _logger = logger;
            _userService = userService;
           
               
           
        }

        /// <summary>
        /// Get Document list
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<object> GetDocumentsAsync([FromQuery] DocumentQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string>();

            var accessability = await _userService.IsUserCustomerOrSupperUserAccess(authenticate);
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _documentService.GetDocumentForCustomerUserAsync(authenticate, query,true,null,accessability.Result);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
        /// <summary>
        /// Export Document List To Excel
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("ExportDocumentListToExcel")]
        public async Task<object> ExportDocumentListAsync()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> ();
            var accessability = await _userService.IsUserCustomerOrSupperUserAccess(authenticate);
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var streamResult = await _documentService.ExportDocumentListForCustomerUserAsync(authenticate, accessability.Result);
            if (streamResult == null)
                return NotFound();

            return File(streamResult.Stream, streamResult.ContentType, streamResult.FileName);
        }
        /// <summary>
        /// Export Document History To Excel
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("ExportDocumentsHistoryExcel")]
        public async Task<object> ExportDocumentsHistoryExcelAsync()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> ();
            var accessability = await _userService.IsUserCustomerOrSupperUserAccess(authenticate);
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var streamResult = await _documentService.ExportDocumentsHistoryForCustomerUserAsync(authenticate, accessability.Result);
            if (streamResult == null)
                return NotFound();

            return File(streamResult.Stream, streamResult.ContentType, streamResult.FileName);
        }
        /// <summary>
        /// Export Documents Revision History To Excel
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("ExportDocumentsRevisionHistoryExcel")]
        public async Task<object> ExportDocumentsRevisionHistoryExcelAsync()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> ();
            var accessability = await _userService.IsUserCustomerOrSupperUserAccess(authenticate);
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var streamResult = await _documentService.ExportDocumentsRevisionHistoryExcelForCustomerUserAsync(authenticate,accessability.Result);
            if (streamResult == null)
                return NotFound();

            return File(streamResult.Stream, streamResult.ContentType, streamResult.FileName);
        }
    }
}
