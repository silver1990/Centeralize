using Newtonsoft.Json;

namespace Raybod.SCM.Utility.Notification.FirebaseNet.Messaging
{
    public class IOSNotification : Notification
    {

        /// <summary>
        /// Indicates the badge on the client app home icon.
        /// </summary>
        [JsonProperty(PropertyName = "badge")]
        public string Badge { get; set; }

    }
}
