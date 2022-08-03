using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Raybod.SCM.DataTransferObject.Document;
using Raybod.SCM.Domain.Struct;
using Raybod.SCM.ModuleApi.Helper;
using Raybod.SCM.ModuleApi.Helper.Logger;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Utility.Filters;

namespace Raybod.SCM.ModuleApi.Areas.DocumentManagment.Controllers
{
    [Route("api/Document/v1/Document")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "documentManagement")]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly IBomProductService _bomProductService;
        private readonly IContractDocumentGroupService _contractDocumentGroupService;
        private readonly ILogger<DocumentController> _logger;

        public DocumentController(
            IDocumentService documentService,
            IBomProductService bomProductService,
            IContractDocumentGroupService contractDocumentGroupService,
            ILogger<DocumentController> logger,IHttpContextAccessor httpContextAccessor
            )
        {
            _documentService = documentService;
            _bomProductService = bomProductService;
            _contractDocumentGroupService = contractDocumentGroupService;
            _logger = logger;
           
            
                
           
           
        }

        ///// <summary>
        ///// Get Product List
        ///// </summary>
        ///// <param name="query"></param>
        ///// <returns></returns>
        //[Route("EquipmentProduct")]
        //[HttpGet]
        //public async Task<object> GetProductMiniInfoAsync([FromQuery] ProductQuery query)
        //{
        //    var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
        //    authenticate.Roles = new List<string> { SCMRole.DocumentMng, SCMRole.DocumentObs, SCMRole.DocumentObs};

        //    var serviceResult = await _productService.GetProductMiniInfoAsync(authenticate, query);
        //    return serviceResult.ToWebApiResultVCore(authenticate.language);
        //}

        /// <summary>
        /// Get Bom Product List For Document ContractCode
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("bomProduct")]
        public async Task<object> BomProducts()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.DocumentMng,
                SCMRole.DocumentObs,
                SCMRole.DocumentGlbObs
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _bomProductService.GetBomProductForDocumentByContractCodeAsync(authenticate);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get Document Group List
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("documentGroups")]
        public async Task<object> GetDocumentGroupList()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.DocumentMng, SCMRole.DocumentObs, SCMRole.DocumentGlbObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _contractDocumentGroupService.GetDocumentGroupListAsync(authenticate);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }


        /// <summary>
        /// get document item logs list 
        /// </summary>
        /// <param name="documentId"></param>
        /// <returns></returns>
        [HttpGet, Route("{documentId:long}/DocumentLog")]
        public async Task<object> GetDocumentLogAsync(long documentId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.DocumentMng,
                SCMRole.DocumentObs,
                SCMRole.DocumentGlbObs,
                SCMRole.DocumentArchiveObs,
                SCMRole.DocumentArchiveLimitedObs,
                SCMRole.DocumentArchiveLimitedGlbObs,
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _documentService.GetDocumentLogAsync(authenticate, documentId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// add list document 
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /Todo
        ///		[
        ///		  {
        ///			"productIds": [
        ///			  1,2
        ///			],
        ///			"docNumber": "doc-202",
        ///			"clientDocNumber": "cli-202",
        ///			"docTitle": "doc-transmital",
        ///			"docRemark": "some description ....",
        ///			"docClass": 1
        ///		  }
        ///		]
        ///    
        /// </remarks>
        /// <param name="documentGroupId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("documentGroup/{documentGroupId:int}")]
        public async Task<object> AddDocumentAsync(int documentGroupId, [FromBody] List<AddListDocumentDto> model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }


            authenticate.Roles = new List<string> { SCMRole.DocumentMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _documentService.AddDocumentAsync(authenticate, documentGroupId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
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
            authenticate.Roles = new List<string> {
                SCMRole.DocumentMng,
                SCMRole.DocumentObs,
                SCMRole.DocumentGlbObs,
                SCMRole.DocumentArchiveObs,
                SCMRole.DocumentArchiveLimitedObs,
                SCMRole.DocumentArchiveLimitedGlbObs
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _documentService.GetDocumentAsync(authenticate, query,null,null);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        //[HttpGet, Route("{documentId:long}")]
        //public async Task<object> GetDocumentByIdAsync(long documentId)
        //{
        //    var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
        //    authenticate.Roles = new List<string> { 
        //        SCMRole.DocumentMng, 
        //        SCMRole.DocumentObs, 
        //        SCMRole.DocumentArchiveMng, 
        //        SCMRole.DocumentArchiveObs,
        //        SCMRole.DocumentArchiveLimitedObs
        //    };

        //    var serviceResult = await _documentService.GetDocumentByIdAsync(authenticate, documentId);
        //    return serviceResult.ToWebApiResultVCore(authenticate.language);
        //}

        // todo remove later
        /// <summary>
        /// Get Document Archive by documentId
        /// </summary>
        /// <param name="documentId"></param>
        /// <returns></returns>
        [HttpGet, Route("{documentId:long}/ArchiveDocument")]
        public async Task<object> GetDocumentArchiveAsync(long documentId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.DocumentArchiveObs,
                SCMRole.DocumentArchiveLimitedObs
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _documentService.GetDocumentArchiveAsync(authenticate, documentId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Edit Document 
        /// </summary>
        /// <param name="documentId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut, Route("{documentId:long}")]
        public async Task<object> EditDocumentAsync(long documentId, [FromBody] AddDocumentDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }


            authenticate.Roles = new List<string> { SCMRole.DocumentMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _documentService.EditDocumentByDocumentIdAsync(authenticate, documentId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Change Active State Of Document
        /// </summary>
        /// <param name="documentId"></param>
        /// <returns></returns>
        [HttpPut, Route("{documentId:long}/ChangeActiveStateOfDocument")]
        public async Task<object> ChangeActiveStateOfDocumentAsync(long documentId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }


            authenticate.Roles = new List<string> { SCMRole.DocumentMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _documentService.ChangeActiveStateOfDocumentAsync(authenticate, documentId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Read Import Excel 
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("ReadImportExcel")]
        public async Task<object> ReadImportDocumentExcelFileAsync()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.DocumentMng };

            var file = HttpContext.Request.Form.Files[0];
            var serviceResult = await _documentService.ReadImportDocumentExcelFileAsync(authenticate, file);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Download Import Excel Template
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("DownloadImportExcelTemplate")]
        public async Task<object> DownloadImportTemplateAsync()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.DocumentMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var streamResult = await _documentService.DownloadImportTemplateAsync(authenticate);
            if (streamResult == null)
                return NotFound();

            return File(streamResult.Stream, streamResult.ContentType, streamResult.FileName);
        }

        /// <summary>
        /// Export Document List To Excel
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("ExportDocumentListToExcel")]
        public async Task<object> ExportDocumentListAsync()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string>
            { 
                SCMRole.DocumentMng,
                SCMRole.DocumentObs,
                SCMRole.DocumentGlbObs,
                SCMRole.DocumentArchiveObs,
                SCMRole.DocumentArchiveLimitedObs,
                SCMRole.DocumentArchiveLimitedGlbObs
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var streamResult = await _documentService.ExportDocumentListAsync(authenticate);
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
            authenticate.Roles = new List<string>
            { 
                SCMRole.DocumentMng,
                SCMRole.DocumentObs,
                SCMRole.DocumentGlbObs,
                SCMRole.DocumentArchiveObs,
                SCMRole.DocumentArchiveLimitedObs,
                SCMRole.DocumentArchiveLimitedGlbObs
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var streamResult = await _documentService.ExportDocumentsHistoryAsync(authenticate);
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
            authenticate.Roles = new List<string>
            { 
                SCMRole.DocumentMng,
                SCMRole.DocumentObs,
                SCMRole.DocumentGlbObs,
                SCMRole.DocumentArchiveObs,
                SCMRole.DocumentArchiveLimitedObs,
                SCMRole.DocumentArchiveLimitedGlbObs 
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var streamResult = await _documentService.ExportDocumentsRevisionHistoryExcelAsync(authenticate);
            if (streamResult == null)
                return NotFound();

            return File(streamResult.Stream, streamResult.ContentType, streamResult.FileName);
        }
    }
}