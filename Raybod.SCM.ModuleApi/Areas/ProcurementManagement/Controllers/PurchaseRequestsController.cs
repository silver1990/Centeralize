using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.DataTransferObject._panelPurchase.PurchaseRequestConfirmation;
using Raybod.SCM.DataTransferObject.Mrp;
using Raybod.SCM.DataTransferObject.PurchaseRequest;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Struct;
using Raybod.SCM.ModuleApi.Helper;
using Raybod.SCM.ModuleApi.Helper.Logger;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Utility.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Raybod.SCM.ModuleApi.Areas.Purchase.Controllers
{
    [Route("api/procurementManagement/purchaseRequest")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "procurementManagement")]
    public class PurchaseRequestsController : ControllerBase
    {
        private readonly IPurchaseRequestService _purchaseRequestService;
        private readonly ILogger<PurchaseRequestsController> _logger;
        private readonly IMrpService _mrpService;

        public PurchaseRequestsController(
            IPurchaseRequestService purchaseRequestService,
            IMrpService mrpService,
            ILogger<PurchaseRequestsController> logger,IHttpContextAccessor httpContextAccessor)
        {
            _purchaseRequestService = purchaseRequestService;
            _mrpService = mrpService;
            _logger = logger;
           
               
           
        }


        /// <summary>
        /// get pr confirm user  list 
        /// </summary>
        /// <param name="mrpId"></param>
        /// <returns></returns>
        [HttpGet, Route("workFlowRegister/{mrpId:long}/users")]
        public async Task<object> GetConfirmUserAsync(long mrpId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PurchaseRequestReg, SCMRole.PurchaseRequestConfirm, SCMRole.PurchaseRequestObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, mrpId);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _purchaseRequestService.GetConfirmationUserListAsync(authenticate, mrpId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// add new pr
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost,Route("register")]
        public async Task<object> AddPurchaseRequestAsync([FromBody] AddPurchaseRequestDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }

            
            authenticate.Roles = new List<string> { SCMRole.PurchaseRequestReg };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _purchaseRequestService.AddPurchaseRequestAsync(authenticate, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get Purchae Request Pending Confiem  Item by purchaseRequestId
        /// </summary>
        /// <param name="purchaseRequestId"></param>
        /// <returns></returns>
        [HttpGet, Route("workflowConfirm/{purchaseRequestId:long}/pendingConfirm")]
        public async Task<object> GetPendingPurchaseRequestConfiemByPurchaseRequestIdAsync(long purchaseRequestId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.PurchaseRequestConfirm,
                SCMRole.PurchaseRequestReg,
                SCMRole.PurchaseRequestObs,
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _purchaseRequestService.GetPendingConfirmPurchaseByPurchaseRequestIdAsync(authenticate, purchaseRequestId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        
        [HttpPost, Route("workflowConfirm/{purchaseRequestId:long}/ConfirmationTask")]
        public async Task<object> SetUserConfirmOwnpurchaseRequestTaskAsync(long purchaseRequestId, [FromBody] AddPurchaseRequestConfirmationAnswerDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }

            
            authenticate.Roles = new List<string> { SCMRole.PurchaseRequestConfirm};

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _purchaseRequestService.SetUserConfirmOwnPurchaseRequestTaskAsync(authenticate, purchaseRequestId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get pr waiting confirm list
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet, Route("waitingForConfirm")]
        public async Task<object> GetConfirms([FromQuery] PurchaseRequestQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PurchaseRequestReg, SCMRole.PurchaseRequestConfirm, SCMRole.PurchaseRequestObs };

            

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _purchaseRequestService.GetPendingForConfirmPurchaseRequestAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get pr list
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet,Route("list")]
        public async Task<object> GetPurchaseRequestAsync([FromQuery] PurchaseRequestQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PurchaseRequestReg, SCMRole.PurchaseRequestConfirm, SCMRole.PurchaseRequestObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _purchaseRequestService.GetPurchaseRequestAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get Waiting Mrp For new pr
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet, Route("mrpListForPrRegisteration")]
        public async Task<object> GetWaitingMrpForNewPr(string query)
        {
            var qry = new MrpQuery { SearchText = query };
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PurchaseRequestReg, SCMRole.PurchaseRequestObs, SCMRole.PurchaseRequestConfirm };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _mrpService.GetWaitingMrpForNewPrAsync(authenticate, qry);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get waiting list badge
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("WaitingListBadgeCount")]
        public async Task<object> GetWaitingListBadgeCount()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PurchaseRequestReg, SCMRole.PurchaseRequestConfirm, SCMRole.PurchaseRequestObs };

            List<string> registerRole = new List<string> { SCMRole.PurchaseRequestReg, SCMRole.PurchaseRequestConfirm, SCMRole.PurchaseRequestObs };
            List<string> confirmRole = new List<string> { SCMRole.PurchaseRequestReg, SCMRole.PurchaseRequestConfirm, SCMRole.PurchaseRequestObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _purchaseRequestService.GetWaitingListBadgeCountAsync(authenticate, registerRole, confirmRole);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        [HttpGet, Route("workFlowAttachment/{purchaseRequestId:long}/download")]
        public async Task<object> DownloadPRAttachmentAsync(long purchaseRequestId, string fileSrc)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PurchaseRequestReg, SCMRole.PurchaseRequestConfirm, SCMRole.PurchaseRequestObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, fileSrc);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var streamResult = await _purchaseRequestService.DownloadPRWorkFlowAttachmentAsync(authenticate, purchaseRequestId, fileSrc);
            if (streamResult == null)
                return NotFound();
            return File(streamResult.Stream, streamResult.ContentType,streamResult.FileName);

        }

        /// <summary>
        /// get pr details
        /// </summary>
        /// <param name="purchaseRequestId"></param>
        /// <returns></returns>
        [HttpGet, Route("{purchaseRequestId:long}")]
        public async Task<object> GetPurchaseRequestByIdAsync(long purchaseRequestId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PurchaseRequestReg, SCMRole.PurchaseRequestConfirm, SCMRole.PurchaseRequestObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, purchaseRequestId);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _purchaseRequestService.GetPurchaseRequestByIdAsync(authenticate, purchaseRequestId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
        /// <summary>
        /// Get Waiting Mrp  Item For New Pr 
        /// </summary>
        /// <param name="mrpId"></param>
        /// <returns></returns>
        [HttpGet, Route("WaitingMrpForNewPr/{mrpId:long}")]
        public async Task<object> GetWaitingMrpForNewPrbyMrpId(long mrpId)
        {

            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PurchaseRequestReg, SCMRole.PurchaseRequestConfirm, SCMRole.PurchaseRequestObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, mrpId);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _mrpService.GetWaitingMrpByIdForNewPrAsync(authenticate, mrpId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
        /// <summary>
        /// edit pr item
        /// </summary>
        /// <param name="purchaseRequestId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut, Route("{purchaseRequestId:long}")]
        public async Task<object> EditPurchaseRequestAsync(long purchaseRequestId, [FromBody] EditPurchaseRequestDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }

          
            authenticate.Roles = new List<string> { SCMRole.PurchaseRequestReg };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _purchaseRequestService.EditPurchaseRequestAsync(authenticate, purchaseRequestId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get pr details
        /// </summary>
        /// <param name="purchaseRequestId"></param>
        /// <returns></returns>
        [HttpGet, Route("{purchaseRequestId:long}/editInfo")]
        public async Task<object> GetPurchaseRequestByIdForEditAsync(long purchaseRequestId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PurchaseRequestReg, SCMRole.PurchaseRequestConfirm, SCMRole.PurchaseRequestObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, purchaseRequestId);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _purchaseRequestService.GetPurchaseRequestByIdForEditAsync(authenticate, purchaseRequestId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }


        /// <summary>
        /// edit pr item
        /// </summary>
        /// <param name="purchaseRequestId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut, Route("{purchaseRequestId:long}/EditBySysAdmin")]
        public async Task<object> EditPurchaseRequestBySysAdminAsync(long purchaseRequestId, [FromBody] EditPurchaseRequestBySysAdminDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }

            
            authenticate.Roles = new List<string> { SCMRole.SYSADMIN };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _purchaseRequestService.EditPurchaseRequestBySysAdminAsync(authenticate, purchaseRequestId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }


        /// <summary>
        /// Get Waiting MrpItems List
        /// </summary>
        /// <param name="mrpId"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet, Route("WaitingMrpItemForNewPr/{mrpId:long}")]
        public async Task<object> GetWaitingMrpItemsByMrpIdAsync(long mrpId, [FromQuery] MrpQuery query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PurchaseRequestReg, SCMRole.PurchaseRequestConfirm, SCMRole.PurchaseRequestObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _mrpService.GetWaitingMrpItemsByMrpIdAsync(authenticate, mrpId, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get Waiting MrpItems List
        /// </summary>
        /// <param name="purchaseRequestId"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet, Route("WaitingMrpItemForEditPr/{purchaseRequestId:long}")]
        public async Task<object> GetWaitingMrpItemsByPurchaseRequestIdAsync(long purchaseRequestId, [FromQuery] MrpQuery query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PurchaseRequestReg, SCMRole.PurchaseRequestConfirm, SCMRole.PurchaseRequestObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _mrpService.GetWaitingMrpItemsByPurchaseRequestIdAsync(authenticate, purchaseRequestId, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
    }
}
