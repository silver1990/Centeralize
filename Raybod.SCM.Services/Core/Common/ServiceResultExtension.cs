using Raybod.SCM.Services.Core.Common.Message;

namespace Raybod.SCM.Services.Core.Common
{
    public static class ServiceResultExtension
    {
        public static ServiceResult<TResult> AddMessage<TResult>(this ServiceResult<TResult> result,
            ServiceMessage messages)
        {
            result.Messages.Add(messages);
            return result;
        }
        public static ServiceResult<TResult> WithTotalCount<TResult>(this ServiceResult<TResult> result,
            int pageCount)
        {
            return new ServiceResult<TResult>(result.Succeeded, result.Result, result.Messages, result.Exception, pageCount);
        }
    }
}
