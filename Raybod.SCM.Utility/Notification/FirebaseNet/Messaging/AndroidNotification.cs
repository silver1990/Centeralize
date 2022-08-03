using Newtonsoft.Json;

namespace Raybod.SCM.Utility.Notification.FirebaseNet.Messaging
{
    public class AndroidNotification : Notification
    {
        /// <summary>
        /// Indicates notification icon. Sets value to myicon for drawable resource myicon.
        /// </summary>
        [JsonProperty(PropertyName = "icon")]
        public string Icon { get; set; }

        /// <summary>
        /// Indicates whether each notification results in a new entry in the notification drawer on Android. 
        /// If not set, each request creates a new notification.
        /// If set, and a notification with the same tag is already being shown, the new notification replaces the existing one in the notification drawer.
        /// </summary>
        [JsonProperty(PropertyName = "tag")]
        public string Tag { get; set; }

        /// <summary>
        /// Indicates color of the icon, expressed in #rrggbb format
        /// </summary>
        [JsonProperty(PropertyName = "color")]
        public string Color { get; set; }

    }
}
