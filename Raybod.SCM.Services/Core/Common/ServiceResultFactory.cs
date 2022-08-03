using Raybod.SCM.Services.Core.Common.Message;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.Services.Core.Common
{
   public class ServiceResultFactory
    {
        public static ServiceResult<TResult> CreateSuccess<TResult>(TResult result)
        {
            return new ServiceResult<TResult>(true, result,
                new List<ServiceMessage> { new ServiceMessage(MessageType.Succeed, MessageId.Succeeded) });
        }

        public static ServiceResult<TResult> CreateSuccess<TResult>(TResult result, ServiceMessage message)
        {
            return new ServiceResult<TResult>(true, result, new List<ServiceMessage> { message });
        }

        public static ServiceResult<TResult> CreateError<TResult>(TResult result, MessageId messageId)
        {
            var serviceMessages = new List<ServiceMessage> { new ServiceMessage(MessageType.Error, messageId) };

            return new ServiceResult<TResult>(false, result, serviceMessages);
        }

        public static ServiceResult<TResult> CreateException<TResult>(TResult result, Exception exception)
        {
            var serviceMessages = new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.Exception) };

            return new ServiceResult<TResult>(false, result, serviceMessages, exception) as ServiceResult<TResult>;
        }

        public static ServiceResult<TResult> CreateException<TResult>(TResult result, MessageId messageId, Exception exception)
        {
            var serviceMessages = new List<ServiceMessage> { new ServiceMessage(MessageType.Error, messageId) };

            return new ServiceResult<TResult>(false, result, serviceMessages, exception) as ServiceResult<TResult>;
        }
    }
}
