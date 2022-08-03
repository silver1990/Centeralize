using Newtonsoft.Json;

namespace Raybod.SCM.Utility.Notification.FirebaseNet.Messaging
{
    public class TopicMessageResponse : FcmResponse
    {
        /// <summary>
        /// The topic message ID when FCM has successfully received the request and will attempt to deliver to all subscribed devices.
        /// </summary>
        [JsonProperty(PropertyName = "message_id")]
        public new long? MessageId { get; set; }
         
    }
}
