using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.DataTransferObject;
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
using Microsoft.AspNetCore.Http;

namespace Raybod.SCM.ModuleApi.Areas.Purchase.Controllers
{
    [Route("api/purchase/v1/purchaseRequest")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "procurementManagement")]
    public class PurchaseRequestController : ControllerBase
    {
        private readonly IPurchaseRequestService _purchaseRequestService;
        private readonly IMrpService _mrpService;
        private readonly IDocumentRevisionService _revisionService;
        private readonly ILogger<PurchaseRequestController> _logger;

        public PurchaseRequestController(
            IPurchaseRequestService purchaseRequestService,
            IMrpService mrpService,
            IDocumentRevisionService revisionService,
            ILogger<PurchaseRequestController> logger,IHttpContextAccessor httpContextAccessor)
        {
            _purchaseRequestService = purchaseRequestService;
            _mrpService = mrpService;
            _revisionService = revisionService;
            _logger = logger;
           
               
           
        }

        /// <summary>
        /// get prItem document list
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        [HttpGet, Route("productDocuments/{productId:int}")]
        public async Task<object> GetDocumentByProductIdBaseOnContractAsync(int productId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PurchaseRequestReg, SCMRole.PurchaseRequestConfirm, SCMRole.PurchaseRequestObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, productId);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _revisionService.GetDocumentByProductIdBaseOnContractAsync(authenticate, productId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

       
       
        
    }
}