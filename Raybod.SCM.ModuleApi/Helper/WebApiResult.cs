using System;
using System.Collections.Generic;
using Raybod.SCM.Services.Core.Common.Message;

namespace Raybod.SCM.ModuleApi.Helper
{
    public class WebApiResult
    {
        public bool Succeeded { get; set; }

        public IList<ClientMessage> Messages { get; set; }

        public string Exception { get; set; }

        public List<object> ModelStateError { get; set; }
    }

    public class WebApiResult<TResult> : WebApiResult
    {
        public TResult Result { get; set; }
    }

    public class WebApiResultWithPagination<TResult> : WebApiResult<TResult>
    {
        public int TotalCount { get; set; }
    }
}