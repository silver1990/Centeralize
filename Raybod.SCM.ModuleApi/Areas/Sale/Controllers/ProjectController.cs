using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.DataTransferObject.Address;
using Raybod.SCM.DataTransferObject.Consultant;
using Raybod.SCM.DataTransferObject.Contract;
using Raybod.SCM.DataTransferObject.Customer;
using Raybod.SCM.DataTransferObject.Product;
using Raybod.SCM.Domain.Struct;
using Raybod.SCM.ModuleApi.Helper;
using Raybod.SCM.ModuleApi.Helper.Authentication;
using Raybod.SCM.ModuleApi.Helper.Logger;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Utility.Filters;
using Microsoft.AspNetCore.Http;

namespace Raybod.SCM.ModuleApi.Areas.Sale.Controllers
{
    [Route("api/sale/v1/contract")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "sale")]
    public class ProjectController : ControllerBase
    {
        private readonly IContractService _contractService;
        private readonly IConsultantService _consultantService;
        private readonly ICustomerService _customerService;

        private readonly IProductService _productService;
        private readonly ILogger<ProjectController> _logger;

        public ProjectController(
            IContractService contractService,
            ICustomerService customerService,
            IProductService productService,
            ILogger<ProjectController> logger,IHttpContextAccessor httpContextAccessor,
            IConsultantService consultantService)
        {
            _contractService = contractService;
            _customerService = customerService;
            _productService = productService;
            _logger = logger;
            _consultantService = consultantService;
           
               
           
        }

        [HttpGet, Route("BaseProducts")]
        public async Task<object> GetBaseProductAsync([FromQuery]ProductQuery query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.ContractMng, SCMRole.ContractReg, SCMRole.ContractObs };

            query.ProductType = Domain.Enum.ProductType.Product;

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _productService.GetProductMiniInfoAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        [HttpGet, Route("searchProducts")]
        public async Task<object> GetEquipmentProductAsync([FromQuery]ProductQuery query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.ContractMng, SCMRole.ContractReg, SCMRole.ContractObs };

            query.ProductType = Domain.Enum.ProductType.Equipment;

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _productService.GetProductMiniInfoAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        [HttpGet]
        public async Task<object> GetAllContractAsync([FromQuery]ContractQuery query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.ContractMng, SCMRole.ContractReg, SCMRole.ContractObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _contractService.GetAllContractAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        [Route("query")]
        [HttpGet]
        public async Task<object> GetContractMiniInfos([FromQuery]ContractQuery query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.ContractMng, SCMRole.ContractReg, SCMRole.ContractObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _contractService.GetAllContractMiniInfoAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        [Route("{contractCode}")]
        [HttpGet]
        public async Task<object> GetContractByIdAsync(string contractCode)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.ContractMng, SCMRole.ContractReg, SCMRole.ContractObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, contractCode);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _contractService.GetContractByIdAsync(authenticate, contractCode);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        [Route("searchIn")]
        [HttpGet]
        public async Task<object> SearchInContract(string query)
        {
            var qry = new ContractQuery { SearchText = query };

            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.ContractMng, SCMRole.ContractReg, SCMRole.ContractObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _contractService.SearchInContractAsync(authenticate, qry);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }


        [HttpPost]
        public async Task<object> AddContractAsync([FromBody]InsertContractDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
                  new ServiceResult<bool>(false, false,
                          new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                      .ToWebApiResultVCore(authenticate.language,ModelState);
            }

           
            authenticate.Roles = new List<string> { SCMRole.ContractMng,SCMRole.ContractReg};

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _contractService.AddContractAsync(authenticate, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        [Route("{contractCode}")]
        [HttpPut]
        public async Task<object> EditContractAsync([FromBody]EditContractDto model, string contractCode)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
                  new ServiceResult<bool>(false, false,
                          new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                      .ToWebApiResultVCore(authenticate.language,ModelState);
            }

          
            authenticate.Roles = new List<string> { SCMRole.ContractMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _contractService.EditContractAsync(authenticate, contractCode, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        [Route("{consultantId:int}/UpdateProjectConsultant")]
        [HttpPut]
        public async Task<object> UpdateProjectConsultant( int consultantId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
                  new ServiceResult<bool>(false, false,
                          new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                      .ToWebApiResultVCore(authenticate.language,ModelState);
            }

            
            authenticate.Roles = new List<string> { SCMRole.ContractMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _contractService.UpdateProjectConsultantAsync(authenticate, consultantId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        [Route("{contractCode}")]
        [HttpDelete]
        public async Task<object> DeleteContract(string contractCode)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.ContractMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, contractCode);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _contractService.RemoveContractAsync(authenticate, contractCode);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        [Route("customer")]
        [HttpGet]
        public async Task<object> GetCustomer([FromQuery]CustomerQuery query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.ContractMng, SCMRole.ContractReg, SCMRole.ContractObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _customerService.GetCustomerMiniInfoAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        [Route("consultant")]
        [HttpGet]
        public async Task<object> GetConsultant([FromQuery] ConsultantQuery query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.ContractMng, SCMRole.ContractReg, SCMRole.ContractObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _consultantService.GetConsultantMiniInfoAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        
        //[Route("customer/address/{customerId:int}")]
        //[HttpGet]
        //[Permission(Role = SCMRole.ContractManagement + "," + SCMRole.ContractRegister)]
        //public async Task<object> GetCustomereAddress(int customerId)
        //{
        //    var serviceResult = await _customerService.GetCustomerAddressByCustomerIdAsync(customerId);
        //    return serviceResult.ToWebApiResultVCore(authenticate.language);
        //}

        

        

        

        //[Route("address/{id:int}")]
        //[HttpPut]
        //public async Task<object> PutContractAddress(int id, AddCustomerAddressDto model)
        //{
        //    var serviceResult = await _contractService.EditContractAddressAsync(id, model);
        //    return serviceResult.ToWebApiResultVCore(authenticate.language);
        //}

        [Route("attachment")]
        [HttpGet]
        public async Task<object> GetContractAttachmentByIdAsync()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> ();

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _contractService.GetContractAttachmentByIdAsync(authenticate);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }


        [HttpGet, Route("attachment/download")]
        public async Task<object> DownloadContractAttachmentAsync(string fileSrc)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> ();

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, fileSrc);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var streamResult = await _contractService.DownloadContractAttachmentAsync(authenticate, fileSrc);
            if (streamResult == null)
                return NotFound();
            return File(streamResult.Stream,streamResult.ContentType,streamResult.FileName);
        }



        [Route("attachment")]
        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 104857600)]
        [RequestSizeLimit(104857600)]
        public async Task<object> AddContractAttachmentAsync()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.ContractMng, SCMRole.ContractReg };

            var files = HttpContext.Request.Form.Files;
            var serviceResult = await _contractService.AddContractAttachmentAsync(authenticate, files);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        [Route("attachment")]
        [HttpDelete]
        public async Task<object> RemoveAttachmentAsync(string fileSrc)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.ContractMng, SCMRole.ContractReg };

            var serviceResult = await _contractService.RemoveAttachmentAsync(authenticate, authenticate.ContractCode, fileSrc);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }


        [Route("GetProjectDescription")]
        [HttpGet]
        public async Task<object> GetProjectDescription()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string>();

            var serviceResult = await _contractService.GetProjectDescriptionAsync(authenticate);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        [Route("GetProjectDetail")]
        [HttpGet]
        public async Task<object> GetProjectDetail()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string>();

            var serviceResult = await _contractService.GetProjectDetailAsync(authenticate);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

       

        [Route("UpdateProjectDescription")]
        [HttpPut]
        public async Task<object> UpdateProjectDescription([FromBody] ContractDescriptionUpdateDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.ContractMng};

            var serviceResult = await _contractService.UpdateProjectDescriptionAsync(authenticate,model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        [Route("UpdateProjectTimeTable")]
        [HttpPut]
        public async Task<object> UpdateProjectTimeTable([FromBody] ContractDurationDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.ContractMng };

            var serviceResult = await _contractService.UpdateProjectTimeTableAsync(authenticate, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }



        [Route("{customerId:int}/UpdateProjectCustomer")]
        [HttpPut]
        public async Task<object> UpdateProjectCustomerAsync(int customerId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
                  new ServiceResult<bool>(false, false,
                          new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                      .ToWebApiResultVCore(authenticate.language,ModelState);
            }

           
            authenticate.Roles = new List<string> { SCMRole.ContractMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _contractService.UpdateProjectCustomerAsync(authenticate, customerId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        [Route("ProjectVisited")]
        [HttpPut]
        public async Task<object> UpdateProjectVisitedAsync()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            var serviceResult = await _contractService.UpdateProjectVisitedAsync(authenticate);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
    }
}