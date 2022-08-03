using System;
using System.Collections.Generic;
using Raybod.SCM.Services.Core.Common.Message;

namespace Raybod.SCM.Services.Core.Common
{
    public class ServiceResult
    {
        public bool Succeeded { get; }
        public IList<ServiceMessage> Messages { get; }
        public Exception Exception { get; }

        /// <inheritdoc />
        /// <summary>
        /// ایجاد یک وهله از خروجی
        /// </summary>
        /// <param name="succeeded">آیا عملیات موفق اجرا شده است؟</param>
        /// <param name="result">خروجی مورد انتظار، می تواند لیست و یا هر ابجکتی باشد</param>
        /// <param name="messages">لیست پیغام های ارسالی به لایه های دیگر</param>
        /// <param name="exception">در صورت رخ دادن استثنا، استثنای پیش امده را  ارسال میکنیم</param>
        /// <param name="pageCount">در صورتی که خروجی از نوع لیست باشد، تعداد کل موجودیت های مورد نظر  را شامل خواهد شد</param>
        public ServiceResult(bool succeeded, IList<ServiceMessage> messages = null, Exception exception = null)
        {
            Succeeded = succeeded;
            Messages = messages;
            Exception = exception;
        }
    }

    /// <summary>
    /// جهت استفاده به همراه تمپیت جنریک
    /// <br/>
    /// این کلاس جهت خروجی لایه سرویس به لایه های دیگر در نظر گرفته شده، تمامی خروجی های لایه سرویس از این نوع خواهند بود
    /// <br/>
    /// </summary>
    /// <typeparam name="TResult">نوع کلاس خروجی را مشخص میکند</typeparam>
    public class ServiceResult<TResult> : ServiceResult
    {
        public TResult Result { get; }

        public int TotalCount { get; }

        /// <inheritdoc />
        /// <summary>
        /// ایجاد یک وهله از خروجی
        /// </summary>
        /// <param name="succeeded">آیا عملیات موفق اجرا شده است؟</param>
        /// <param name="result">خروجی مورد انتظار، می تواند لیست و یا هر ابجکتی باشد</param>
        /// <param name="messages">لیست پیغام های ارسالی به لایه های دیگر</param>
        /// <param name="exception">در صورت رخ دادن استثنا، استثنای پیش امده را  ارسال میکنیم</param>
        /// <param name="pageCount">در صورتی که خروجی از نوع لیست باشد، تعداد کل موجودیت های مورد نظر  را شامل خواهد شد</param>
        public ServiceResult(bool succeeded, TResult result, IList<ServiceMessage> messages = null,
            Exception exception = null, int pageCount = 0)
            : base(succeeded, messages, exception)
        {
            Result = result;
            TotalCount = pageCount;
        }

    }
}
