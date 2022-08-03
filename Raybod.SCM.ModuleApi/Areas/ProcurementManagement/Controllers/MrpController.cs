using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.DataTransferObject.Mrp;
using Raybod.SCM.DataTransferObject.MrpItem;
using Raybod.SCM.Domain.Struct;
using Raybod.SCM.ModuleApi.Helper;
using Raybod.SCM.ModuleApi.Helper.Authentication;
using Raybod.SCM.ModuleApi.Helper.Logger;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Utility.Filters;
using Microsoft.AspNetCore.Http;

namespace Raybod.SCM.ModuleApi.Areas.Planning.Controllers
{
    [Route("api/planning/v1/mrp")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "procurementManagement")]
    public class MrpController : ControllerBase
    {
        private readonly IMrpService _mrpService;
        private readonly IPRContractService _prContractService;
        private readonly IMasterMrService _masterMrService;
        private readonly ILogger<MrpController> _logger;

        public MrpController(
            IMrpService mrpService,
            IPRContractService prContractService,
            IMasterMrService masterMrService,
            ILogger<MrpController> logger,IHttpContextAccessor httpContextAccessor)
        {
            _mrpService = mrpService;
            _prContractService = prContractService;
            _masterMrService = masterMrService;
            _logger = logger;
           
               
           
        }

        /// <summary>
        /// waiting for create mrp badge count
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("WaitingContract/badgeCount")]
        public async Task<object> WaitingContractForMrpBadgeCountAsync()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.MrpMng, SCMRole.MrpObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _masterMrService.WaitingContractForMrpBadgeCountAsync(authenticate);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// waiting for create mrp => list
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("WaitingContract")]
        public async Task<object> WaitingMasterMrListGroupedByProductGroupIdAsync()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.MrpMng, SCMRole.MrpObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _masterMrService.WaitingMasterMrListGroupedByProductGroupIdAsync(authenticate);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get waiting object for create mrp
        /// </summary>
        /// <param name="productGroupId"></param>
        /// <returns></returns>
        [HttpGet, Route("WaitingContract/{productGroupId:int}")]
        public async Task<object> WaitingContractForMrpByContractCodeAsync(int productGroupId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.MrpMng, SCMRole.MrpObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, productGroupId);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _masterMrService.WaitingContractForMrpByContractCodeAsync(authenticate, productGroupId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get waiting productList by productGroupId
        /// </summary>
        /// <param name="productGroupId"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet, Route("GetMasterMR/{productGroupId:int}")]
        public async Task<object> GetMasterMRBYProductGroupIdAsync(int productGroupId, [FromQuery] MasterMRQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.MrpMng, SCMRole.MrpObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _masterMrService.GetMasterMRBYProductGroupIdAsync(authenticate, productGroupId, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get mrp list 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="mrpNumber"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<object> GetMrpAsync([FromQuery]MrpQuery query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.MrpMng, SCMRole.MrpObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _mrpService.GetMrpAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get mrp Items by mrpId
        /// </summary>
        /// <param name="mrpId"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet, Route("{mrpId:long}")]
        public async Task<object> GetMrpItemsByMrpIdAsync(long mrpId, [FromQuery] MrpQuery query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.MrpMng, SCMRole.MrpObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _mrpService.GetMrpItemsByMrpIdAsync(authenticate, mrpId, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }


        /// <summary>
        /// get mrp details 
        /// </summary>
        /// <param name="mrpId"></param>
        /// <returns></returns>
        [HttpGet, Route("{mrpId:long}/Edit")]
        public async Task<object> GetMrpByMrpIdForEditAsync(long mrpId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.MrpMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, mrpId);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _mrpService.GetMrpByMrpIdForEditAsync(authenticate, mrpId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// add mrp
        /// </summary>
        /// <param name="contractCode"></param>
        /// <param name="productGroupId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("{contractCode}/productGroup/{productGroupId:int}")]
        public async Task<object> AddMrpAsync(string contractCode, int productGroupId, [FromBody] List<AddMrpItemDto> model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }

            authenticate.Roles = new List<string> { SCMRole.MrpMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _mrpService.AddMrpAsync(authenticate, contractCode, productGroupId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// update  mrp
        /// </summary>
        /// <param name="mrpId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut, Route("{mrpId:long}")]
        public async Task<object> EditMrpAsync(long mrpId, [FromBody]List<AddMrpItemDto> model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }

            authenticate.Roles = new List<string> { SCMRole.MrpMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _mrpService.EditMrpAsync(authenticate, mrpId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// checked Is There Any Available PRContract For This Product
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        [HttpGet, Route("CheckFreePRContract/{productId}")]
        public async Task<object> IsThereAnyAvailablePRContractForThisProduct(int productId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.MrpMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, productId);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _prContractService.IsThereAnyAvailablePRContractForThisProduct(authenticate, productId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get Available PRContract For This Product
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        [HttpGet, Route("GetAvailablePRContract/{productId}")]
        public async Task<object> GetAvailablePRContractForThisProduct(int productId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.MrpMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, productId);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _prContractService.GetAvailablePRContractForThisProduct(authenticate, productId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// posted excel for read
        /// </summary>
        /// <param name="contractCode"></param>
        /// <param name="isPersianDateFormat"></param>
        /// <returns></returns>
        [HttpPost, Route("ImportExcel/{contractCode}")]
        public async Task<object> ReadExcelFileAsync(string contractCode, bool isPersianDateFormat)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.MrpMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var file = HttpContext.Request.Form.Files[0];
            var serviceResult = await _mrpService.ReadExcelFileAsync(authenticate, contractCode, file, isPersianDateFormat);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        ///  expory excel
        /// </summary>
        /// <param name="productGroupId"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet, Route("ExportMasterMR/{productGroupId:int}")]
        public async Task<object> ExportMasterMRAsync(int productGroupId, [FromQuery] MasterMRQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.MrpMng, SCMRole.MrpObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var streamResult = await _mrpService.ExportMasterMRAsync(authenticate, productGroupId, query);
            if (streamResult == null)
                return NotFound();

            return File(streamResult.Stream, streamResult.ContentType, streamResult.FileName);
        }

    }
}