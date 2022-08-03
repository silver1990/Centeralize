using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Raybod.SCM.ModuleApi.Helper
{
    public static class ExtentionV2Response
    { 
      
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="status"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static HttpResponseMessage ToHttpResponse(this object data, HttpStatusCode status = HttpStatusCode.OK, string token = "")
        {
            dynamic objectData = data;
            var outData = "";
            if (!string.IsNullOrWhiteSpace(token))
            {
                outData = JsonConvert.SerializeObject(new
                {
                    data = objectData,
                    status = (int)status,
                    token
                }, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            }
            else
            {
                outData = JsonConvert.SerializeObject(new
                {
                    data = objectData,
                    status = (int)status, 
                }, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            } 
            var response = new HttpResponseMessage(status)
            {
                Content = new StringContent(outData, System.Text.Encoding.UTF8, "application/json")
            };
            return response;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="status"></param>
        /// <param name="token"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static object ToHttpResponseV2(this object data, HttpStatusCode status = HttpStatusCode.OK, string token = "")
        {
            dynamic objectData = data;
            object outData;
            if (!string.IsNullOrWhiteSpace(token))
            {
                outData = new
                {
                    data = objectData,
                    status = (int)status,
                    token,
                };
            }
            else
            {
                outData = new
                {
                    data = objectData,
                    status = (int)status, 
                };
            } 
            return outData;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="status"></param>
        /// <param name="token"></param>
        /// <param name="refreshToken"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static object ToHttpResponseV3(this object data, HttpStatusCode status = HttpStatusCode.OK, string token = "", string refreshToken = "")
        {
            dynamic objectData = data;
            object outData;
            if (!string.IsNullOrWhiteSpace(token))
            {
                outData = new
                {
                    data = objectData,
                    status = (int)status,
                    token,
                    refreshToken
                };
            }
            else
            {
                outData = new
                {
                    data = objectData,
                    status = (int)status,
                };
            }           
            return outData;
        }
    }
}