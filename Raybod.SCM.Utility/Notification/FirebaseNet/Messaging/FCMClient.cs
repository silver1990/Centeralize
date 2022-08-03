using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Raybod.SCM.Utility.Notification.FirebaseNet.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Raybod.SCM.Utility.Notification.FirebaseNet.Messaging
{
    public class FcmClient
    {
        private readonly Uri _fcmUri;


        private readonly string _serverKey;

        public FcmClient(string serverKey, Uri fcmUri)
        {
            _serverKey = serverKey;
            _fcmUri = fcmUri;
        }


        public async Task<T> SendMessageAsync<T>(Message message) where T : class
        {
            var result = await SendMessageAsync(message);
            return result as T;
        }

        public async Task<FcmResponse> SendMessageAsync(Message message)
        {
            if (TestMode) { message.DryRun = true; }
            var serializedMessage = JsonConvert.SerializeObject(message, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            var request = new HttpRequestMessage(HttpMethod.Post, _fcmUri);
            request.Headers.TryAddWithoutValidation("Authorization", "key=" + _serverKey);
            request.Content = new StringContent(serializedMessage, Encoding.UTF8, "application/json");

            var client = _httpClient;
            var result = await client.SendAsync(request);


            if (result.StatusCode != System.Net.HttpStatusCode.OK)
            {
                if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new FcmUnauthorizedException();
                }
                var errorMessage = await result.Content.ReadAsStringAsync();
                throw new FcmException(result.StatusCode, errorMessage);
            }

            var content = await result.Content.ReadAsStringAsync();

            //if contains a multicast_id field, it's a downstream message
            if (content.Contains("multicast_id"))
            {
                return JsonConvert.DeserializeObject<DownstreamMessageResponse>(content);
            }

            //otherwhise it's a topic message
            return JsonConvert.DeserializeObject<TopicMessageResponse>(content);

        }

        /// <summary>
        /// Automatically sets all measages as dry_run
        /// </summary>
        private bool TestMode { get; set; }

        private readonly HttpClient _httpClient = new HttpClient();


    }
}
