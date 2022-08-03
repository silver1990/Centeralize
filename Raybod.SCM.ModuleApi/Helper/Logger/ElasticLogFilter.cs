using Microsoft.AspNetCore.Mvc.Filters;
using Raybod.SCM.DataTransferObject;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Raybod.SCM.ModuleApi.Helper.Logger
{
    public class ElasticLogFilter : ActionFilterAttribute
    {
         private readonly  ILogger _logger;
            public ElasticLogFilter (ILogger logger)
            {
              this._logger= logger;
            }
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);

            var authenticate = context.HttpContext.Items["authenticate"] as AuthenticateDto;
            var logInformation = LogerHelper.ActionExcuted("CustomerController GetCustomerCompanyInfoAsync", authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            
        }

        

    }
}
