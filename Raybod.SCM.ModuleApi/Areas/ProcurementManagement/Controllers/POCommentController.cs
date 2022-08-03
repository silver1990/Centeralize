using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.DataTransferObject.PO.POComment;
using Raybod.SCM.DataTransferObject.User;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Struct;
using Raybod.SCM.ModuleApi.Helper;
using Raybod.SCM.ModuleApi.Helper.Logger;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Utility.Filters;
using Microsoft.AspNetCore.Http;

namespace Raybod.SCM.ModuleApi.Areas.PurchaseManagement.Controllers
{
    [Route("api/procurementManagement/PO/{poId:long}/poComment")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "procurementManagement")]
    public class POCommentController : ControllerBase
    {
        private readonly IPOCommentService _poCommentService;
        private readonly IUserService _userService;
        private readonly ILogger<POCommentController> _logger;

        public POCommentController(
            IPOCommentService poCommentService,
            IUserService userService,
            ILogger<POCommentController> logger,IHttpContextAccessor httpContextAccessor)
        {
            _poCommentService = poCommentService;
            _userService = userService;
            _logger = logger;
            
               
           
        }

        [HttpGet]
        public async Task<object> GetPOCommentAsync(long poId, [FromQuery] POCommentQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.POMng,
                SCMRole.POObs,
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _poCommentService.GetPOCommentAsync(authenticate, poId,PoCommentType.Po, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        [HttpPost]
        public async Task<object> AddPOCommentAsync(long poId, [FromBody] AddPOCommentDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }


            authenticate.Roles = new List<string> {
                SCMRole.POMng,
                SCMRole.POObs
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _poCommentService.AddPOCommentAsync(authenticate, poId, PoCommentType.Po, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        [HttpGet]
        [Route("{commentId:long}/downloadFile")]
        public async Task<object> DownloadPOCommentAttachmentAsync(long poId, long commentId, string fileSrc)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.POMng,
                SCMRole.POObs
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, fileSrc);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var streamResult = await _poCommentService.DownloadPOCommentAttachmentAsync(authenticate, poId, commentId, PoCommentType.Po, fileSrc);
            if (streamResult == null)
                return NotFound();

            return File(streamResult.Stream, streamResult.ContentType,streamResult.FileName);
        }


        [HttpGet]
        [Route("userMention")]
        public async Task<object> GetUserMentionOfPOCommentAsync(long poId, [FromQuery] UserQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.POMng,
                SCMRole.POObs,
                SCMRole.POFinancialMng,
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _poCommentService.GetUserMentionsAsync(authenticate, poId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

    }
}