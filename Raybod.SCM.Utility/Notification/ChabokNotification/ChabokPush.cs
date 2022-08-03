using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Raybod.SCM.Utility.Notification.ChabokNotification
{

    public class ChabokPush : IPushNotificationService
    {

        private readonly string _appId; // Application ID
        private readonly string _appIdDriver; // Application ID
        private readonly string _restApiKey; //Your Rest Api Code
        private readonly string _userAuth; // Your User Auth
        private readonly string _apiUrl; // Your User Auth


        /// <summary>
        /// </summary>
        /// <param name="appId">Your application id which can be found at Keys & Ids at App Setting</param>
        /// <param name="appIdDriver"></param>
        /// <param name="restKey">Your Rest Key which can be found at Keys & Ids at App Setting</param>
        /// <param name="userAuth">Your User Auth Key which can be found at user setting</param>
        /// <param name="apiUrl">Base Api Url</param>
        public ChabokPush(string appId, string appIdDriver, string restKey, string userAuth, string apiUrl)
        {
            _appId = appId;
            _appIdDriver = appIdDriver;
            _restApiKey = restKey;
            _userAuth = userAuth;
            _apiUrl = apiUrl;
        }
        public ChabokPush(string appId, string appIdDriver, string restKey, string userAuth)
        {
            _appId = appId;
            _appIdDriver = appIdDriver;
            _restApiKey = restKey;
            _userAuth = userAuth;
            _apiUrl = "push.adpdigital.com/api";
        }

        /// <summary>
        /// Create new instance and populate properties form app.config 
        /// </summary>
        public ChabokPush()
        {
            _apiUrl = ConfigurationManager.AppSettings["ChabokPushApiUrl"];
            _appId =ConfigurationManager.AppSettings["ChabokPushAppId"];
            _appIdDriver =ConfigurationManager.AppSettings["ChabokPushDriverAppId"];
            _restApiKey =ConfigurationManager.AppSettings["ChabokPushRestApiKey"];
            _userAuth =ConfigurationManager.AppSettings["ChabokPushUserAuth"];
        }

        /// <summary>
        /// send Notification
        /// </summary>
        /// <param name="chabokpushModel"></param>
        /// <returns></returns>
        public async Task<ChabokPushResult> SendNotificationAsync(ChabokpushModel chabokpushModel)
        {
            chabokpushModel.Application = _appId;
            var result = new ChabokPushResult
            {
                Count = 0
            };
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri($"https://{_appId}.{_apiUrl}/push/toUsers?access_token={_userAuth}");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");
                    var serializeObject = JsonConvert.SerializeObject(chabokpushModel, new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    });
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, client.BaseAddress)
                    {
                        Content = new StringContent(serializeObject, Encoding.UTF8, "application/json")
                    };
                    var response = await client.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        result = JsonConvert.DeserializeObject<List<ChabokPushResult>>(data).FirstOrDefault();
                    }
                }
            }
            catch (Exception exception)
            {
                //ignore
            }
            return result;
        }

        /// <summary>
        /// send Notification
        /// </summary>
        /// <param name="chabokpushModel"></param>
        /// <returns></returns>
        public async Task<ChabokPushResult> SendNotificationForDriverAsync(ChabokpushModel chabokpushModel)
        {
            chabokpushModel.Application = _appIdDriver;
            var result = new ChabokPushResult
            {
                Count = 0
            };

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri($"https://sandbox.{_apiUrl}/push/toUsers?access_token=4ce19cbdfb9544e48c25a64038a56df2f2de5001");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");
                    var serializeObject = JsonConvert.SerializeObject(chabokpushModel, new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    });
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, client.BaseAddress);
                    request.Content = new StringContent(serializeObject,
                        Encoding.UTF8,
                        "application/json");
                    var response = await client.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        result = JsonConvert.DeserializeObject<List<ChabokPushResult>>(data).FirstOrDefault();
                    }

                }
            }
            catch (Exception exception)
            {
                //ignore
            }
            return result;
        }


        /// <summary>
        /// send Notification
        /// </summary>
        /// <param name="chabokpushModel"></param>
        /// <returns></returns>
        public async Task<ChabokPushResult> SendNotificationByQueryAsync(ChabokpushModel chabokpushModel)
        {
            chabokpushModel.Application = _appId;
            var result = new ChabokPushResult
            {
                Count = 0
            };

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri($"https://{_appId}.{_apiUrl}/push/byQuery?access_token={_userAuth}");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");
                    var serializeObject = JsonConvert.SerializeObject(chabokpushModel, new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    });
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, client.BaseAddress)
                    {
                        Content = new StringContent(serializeObject, Encoding.UTF8, "application/json")
                    };

                    var response = await client.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        result = JsonConvert.DeserializeObject<ChabokPushResult>(data);
                    }

                }
            }
            catch (Exception exception)
            {
                //ignore
            }
            return result;
        }



    }
}