using System.Threading.Tasks;
using Raybod.SCM.Utility.Notification.ChabokNotification;

namespace Raybod.SCM.Utility.Notification
{

    public interface IPushNotificationService
    {
        /// <summary>
        /// send Notification
        /// </summary>
        /// <param name="chabokpushModel"></param>
        /// <returns></returns>
        Task<ChabokPushResult> SendNotificationAsync(ChabokpushModel chabokpushModel);

        Task<ChabokPushResult> SendNotificationForDriverAsync(ChabokpushModel chabokpushModel);

        /// <summary>
        /// send Notification
        /// </summary>
        /// <param name="chabokpushModel"></param>
        /// <returns></returns>
        Task<ChabokPushResult> SendNotificationByQueryAsync(ChabokpushModel chabokpushModel);
         
    }
}