namespace Raybod.SCM.Services.Core.Common.Message
{
    /// <summary>
    /// این کلاس جهت ارسال پیام های لایه سرویس به لایه های دیگر استفاده می شود
    /// </summary>
    public class ServiceMessage
    {
        /// <summary>
        /// نوع پیامی که برای لایه ی دیگر ارسال میشود، از نوع موفق و یا خطا و یا ....
        /// </summary>
        public MessageType Type { get; }

        /// <summary>
        /// شرح خطایی که رخ داده است
        /// </summary>
        public MessageId Message { get; }

        public ServiceMessage(MessageType type, MessageId message)
        {
            Type = type;
            Message = message;
        }
    }
}
