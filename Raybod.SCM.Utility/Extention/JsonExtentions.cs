using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Raybod.SCM.Utility.Extention
{
    public static class JsonExtentions
    {

        public static string GetCamelCase(this object result)
        {
            var jsonSerializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            var json = JsonConvert.SerializeObject(result, jsonSerializerSettings);
            return json;
        }
        public static HttpResponseMessage HttpResponseJson(this object data, HttpStatusCode status = HttpStatusCode.OK)
        {
            var resp = new HttpResponseMessage()
            {
                Content = new StringContent(data.GetCamelCase())
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            resp.StatusCode = status;
            return resp;
        }
        public static string ToJson(this object value)
        {
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            return JsonConvert.SerializeObject(value, Formatting.Indented, settings);
        }
        public static HttpResponseMessage HttpResponseJsonV2(this object data, HttpStatusCode status = HttpStatusCode.OK, int? total = null, string exceptionCode = "", string message = "")
        {
            dynamic formatData;
            if (total != null)
            {
                formatData = new
                {
                    value = data,
                    total = total
                };
            }
            else
            {
                formatData = new
                {
                    value = data
                };
            }

            var resopnse = new
            {
                data = formatData,
                error = new { code = exceptionCode, message = message }
            };
            var resp = new HttpResponseMessage()
            {
                Content = new StringContent(resopnse.GetCamelCase())
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            resp.StatusCode = status;
            return resp;
        }
        public class JsonCamelCaseResult : IActionResult
        {
            public Encoding ContentEncoding { get; set; }

            public string ContentType { get; set; }

            public object Data { get; set; }



            public async Task ExecuteResultAsync(ActionContext context)
            {
                if (context == null)
                {
                    throw new ArgumentNullException(nameof(context));
                }

                var response = context.HttpContext.Response;
                response.ContentType = !string.IsNullOrEmpty(ContentType) ? ContentType : "application/json";

                if (Data != null)
                {
                    var jsonSerializerSettings = new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    };
                    var serializeObject = JsonConvert.SerializeObject(Data, jsonSerializerSettings);
                    if (ContentEncoding != null)
                    {
                        await response.WriteAsync(serializeObject, encoding: ContentEncoding);
                        return;
                    }
                    await response.WriteAsync(serializeObject);
                }
            }
        }


    }


}
