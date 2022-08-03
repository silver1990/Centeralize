using System;
using System.Configuration;
using System.Threading.Tasks;
using Raybod.SCM.Utility.Notification.ChabokNotification;
using Raybod.SCM.Utility.Notification.FirebaseNet.Messaging;
using Message = Raybod.SCM.Utility.Notification.FirebaseNet.Messaging.Message;

namespace Raybod.SCM.Utility.Notification.FirebaseNet
{
    public class FireBasePush : IPushNotificationService
    {

        private readonly string _appId; // Application ID
        private readonly string _appIdDriver; // Application ID
        private readonly string _restApiKey; //Your Rest Api Code
        private readonly string _userAuth; // Your User Auth
        private readonly string _apiUrl; // Your User Auth
        private FcmClient fcmClient;
        private readonly bool _isTopicTaget;
        private readonly bool _isSms;

        /// <summary>
        /// </summary>
        /// <param name="appId">Your application id which can be found at Keys & Ids at App Setting</param>
        /// <param name="appIdDriver"></param>
        /// <param name="restKey">Your Rest Key which can be found at Keys & Ids at App Setting</param>
        /// <param name="userAuth">Your User Auth Key which can be found at user setting</param>
        /// <param name="apiUrl">Base Api Url</param>
        /// <param name="isTopicTaget"></param>
        public FireBasePush(string appId, string appIdDriver, string restKey, string userAuth, string apiUrl, bool isTopicTaget = false)
        {
            _appId = appId;
            _appIdDriver = appIdDriver;
            _restApiKey = restKey;
            _userAuth = userAuth;
            _apiUrl = apiUrl;
            _isTopicTaget = isTopicTaget;
        }
        public FireBasePush(string appId, string appIdDriver, string restKey, string userAuth, bool isTopicTaget = false)
        {
            _appId = appId;
            _appIdDriver = appIdDriver;
            _restApiKey = restKey;
            _userAuth = userAuth;
            _isTopicTaget = isTopicTaget;
            _apiUrl = "https://fcm.googleapis.com/fcm/send";
        }

        /// <summary>
        /// Create new instance and populate properties form app.config 
        /// </summary>
        public FireBasePush(bool isTopicTaget = false)
        {
            _isTopicTaget = isTopicTaget;
            _apiUrl =ConfigurationManager.AppSettings["ChabokPushApiUrl"];
            _appId =ConfigurationManager.AppSettings["ChabokPushAppId"];
            _appIdDriver =ConfigurationManager.AppSettings["ChabokPushDriverAppId"];
            _restApiKey =ConfigurationManager.AppSettings["ChabokPushRestApiKey"];
            _userAuth =ConfigurationManager.AppSettings["ChabokPushUserAuth"];
            //
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
                fcmClient = new FcmClient(_appId, new Uri(_apiUrl));
                var fcmResponse = await fcmClient.SendMessageAsync(new Message
                {
                    Data = chabokpushModel.Data,
                    To = _isTopicTaget ? "/topics/" + chabokpushModel.User : chabokpushModel.User,
                    Notification = new Messaging.Notification
                    {
                        Body = chabokpushModel.Notification.Body,
                        Sound = chabokpushModel.Notification.Sound,
                        Title = chabokpushModel.Notification.Title,
                    }
                });
                return new ChabokPushResult
                {
                    Count = 1,
                    Id = fcmResponse.MessageId,
                    ErrorMessage = fcmResponse.Error
                };
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
            chabokpushModel.Application = _appId;
            var result = new ChabokPushResult
            {
                Count = 0
            };
            try
            {
                fcmClient = new FcmClient(_appIdDriver, new Uri(_apiUrl));
                var fcmResponse = await fcmClient.SendMessageAsync(new Message
                {
                    Data = chabokpushModel.Data,
                    To = _isTopicTaget ? "/topics/" + chabokpushModel.User : chabokpushModel.User,
                    Notification = new Messaging.Notification
                    {
                        Body = chabokpushModel.Notification.Body,
                        Sound = chabokpushModel.Notification.Sound,
                        Title = chabokpushModel.Notification.Title,
                    }
                });
                return new ChabokPushResult
                {
                    Count = 1,
                    Id = fcmResponse.MessageId,
                    ErrorMessage = fcmResponse.Error
                };
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
            return  new ChabokPushResult();
        }



    }
}