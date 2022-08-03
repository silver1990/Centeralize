using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Utility.Config;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace Raybod.SCM.ModuleApi.Helper
{

    public static class WebApiResultClass
    {

        /// <summary>
        /// ToWebApiResult
        /// </summary>
        /// <param name="source"></param>
        /// <param name="modelState"></param>
        /// <param name="token"></param>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public static HttpResponseMessage ToWebApiResult<TResult>(this ServiceResult<TResult> source, ModelStateDictionary modelState = null, string token = null)
        {
            var errors = modelState?.Where(n => n.Value.Errors.Count > 0).Select(a => new
            {
                Key = a.Key.Replace("model.", ""),
                Error = a.Value.Errors.Select(b => b.ErrorMessage).ToList()
            }).ToList();
            var result = new WebApiResult<TResult>
            {
                Exception = JsonConvert.SerializeObject(source.Exception, SerializerJsonConfigs.ReferenceLoopHandlingIgnoreJsonConfig()),
                Succeeded = source.Succeeded,
                Result = source.Result,
                ModelStateError = new List<object> { errors },
                Messages = source.Messages.GetPersianMessage()
            };
            return result.ToHttpResponse(token: token);
        }

        /// <summary>
        /// ToWebApiResult
        /// </summary>
        /// <param name="source"></param>
        /// <param name="modelState"></param>
        /// <param name="token"></param>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public static object ToWebApiResultVCore<TResult>(this ServiceResult<TResult> source, string language, ModelStateDictionary modelState = null, string token = null)
        {
            var errors = modelState?.Where(n => n.Value.Errors.Count > 0).Select(a => new
            {
                Key = a.Key.Replace("model.", ""),
                Error = a.Value.Errors.Select(b => b.ErrorMessage).ToList()
            }).ToList();

            if (source.Result is IEnumerable)
            {
                var result = new WebApiResultWithPagination<TResult>
                {
                    Exception = JsonConvert.SerializeObject(source.Exception, SerializerJsonConfigs.ReferenceLoopHandlingIgnoreJsonConfig()),
                    Succeeded = source.Succeeded,
                    Result = source.Result,
                    ModelStateError = new List<object> { errors },
                    Messages =(language=="en")?source.Messages.GetEnglishMessage():source.Messages.GetPersianMessage(),
                    TotalCount = source.TotalCount
                };
                return result.ToHttpResponseV2(token: token);

            }
            else
            {
                var result = new WebApiResult<TResult>
                {
                    Exception = JsonConvert.SerializeObject(source.Exception, SerializerJsonConfigs.ReferenceLoopHandlingIgnoreJsonConfig()),
                    Succeeded = source.Succeeded,
                    Result = source.Result,
                    ModelStateError = new List<object> { errors },
                    Messages = (language == "en") ? source.Messages.GetEnglishMessage() : source.Messages.GetPersianMessage()
                };
                return result.ToHttpResponseV2(token: token);
            }

        }

        /// <summary>
        /// ToWebApiResult
        /// </summary>
        /// <param name="source"></param>
        /// <param name="modelState"></param>
        /// <param name="token"></param>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public static object ToWebApiResultLicense<TResult>(this ServiceResult<TResult> source, string language, ModelStateDictionary modelState = null, string token = null)
        {
            var errors = modelState?.Where(n => n.Value.Errors.Count > 0).Select(a => new
            {
                Key = a.Key.Replace("model.", ""),
                Error = a.Value.Errors.Select(b => b.ErrorMessage).ToList()
            }).ToList();

            if (source.Result is IEnumerable)
            {
                var result = new WebApiResultWithPagination<TResult>
                {
                    Exception = JsonConvert.SerializeObject(source.Exception, SerializerJsonConfigs.ReferenceLoopHandlingIgnoreJsonConfig()),
                    Succeeded = source.Succeeded,
                    Result = source.Result,
                    ModelStateError = new List<object> { errors },
                    Messages = (language == "en") ? source.Messages.GetEnglishMessage() :source.Messages.GetPersianMessage() ,
                    TotalCount = source.TotalCount
                };
                return result.ToHttpResponseV2((HttpStatusCode)601,token: token);

            }
            else
            {
                var result = new WebApiResult<TResult>
                {
                    Exception = JsonConvert.SerializeObject(source.Exception, SerializerJsonConfigs.ReferenceLoopHandlingIgnoreJsonConfig()),
                    Succeeded = source.Succeeded,
                    Result = source.Result,
                    ModelStateError = new List<object> { errors },
                    Messages = (language == "en") ? source.Messages.GetEnglishMessage():source.Messages.GetPersianMessage() ,
                };
                return result.ToHttpResponseV2((HttpStatusCode)601, token: token);
            }

        }


        /// <summary>
        /// ToWebApiResult
        /// </summary>
        /// <param name="source"></param>
        /// <param name="modelState"></param>
        /// <param name="token"></param>
        /// <param name="refreshToken"></param>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public static object ToWebApiResultV3Core<TResult>(this ServiceResult<TResult> source, string language, ModelStateDictionary modelState = null, string token = null, string refreshToken = null)
        {
            var errors = modelState?.Where(n => n.Value.Errors.Count > 0).Select(a => new
            {
                Key = a.Key.Replace("model.", ""),
                Error = a.Value.Errors.Select(b => b.ErrorMessage).ToList()
            }).ToList();

            if (source.Result is IEnumerable)
            {
                var result = new WebApiResultWithPagination<TResult>
                {
                    Exception = JsonConvert.SerializeObject(source.Exception, SerializerJsonConfigs.ReferenceLoopHandlingIgnoreJsonConfig()),
                    Succeeded = source.Succeeded,
                    Result = source.Result,
                    ModelStateError = new List<object> { errors },
                    Messages = (language == "en") ? source.Messages.GetEnglishMessage(): source.Messages.GetPersianMessage() ,
                    TotalCount = source.TotalCount
                };
                return result.ToHttpResponseV3(token: token, refreshToken: refreshToken);

            }
            else
            {
                var result = new WebApiResult<TResult>
                {
                    Exception = JsonConvert.SerializeObject(source.Exception, SerializerJsonConfigs.ReferenceLoopHandlingIgnoreJsonConfig()),
                    Succeeded = source.Succeeded,
                    Result = source.Result,
                    ModelStateError = new List<object> { errors },
                    Messages = (language == "en") ? source.Messages.GetEnglishMessage(): source.Messages.GetPersianMessage() ,
                };
                return result.ToHttpResponseV3(token: token, refreshToken: refreshToken);
            }


        }

        public static object ToWebApiResultVCore(this ServiceResult source, string language, ModelStateDictionary modelState = null, string token = null)
        {
            var errors = modelState?.Where(n => n.Value.Errors.Count > 0).Select(a => new
            {
                Key = a.Key.Replace("model.", ""),
                Error = a.Value.Errors.Select(b => b.ErrorMessage).ToList()
            }).ToList();
            var result = new WebApiResult
            {
                Exception = JsonConvert.SerializeObject(source.Exception, SerializerJsonConfigs.ReferenceLoopHandlingIgnoreJsonConfig()),
                Succeeded = source.Succeeded,
                ModelStateError = new List<object> { errors },
                Messages = (language == "en") ? source.Messages.GetEnglishMessage():source.Messages.GetPersianMessage()  ,
            };
            return result.ToHttpResponseV2(token: token);
        }

        /// <summary>
        /// ToWebApiResult
        /// </summary>
        /// <param name="source"></param>
        /// <param name="modelState"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static HttpResponseMessage ToWebApiResult(this ServiceResult source, string language, ModelStateDictionary modelState = null, string token = null)
        {
            var errors = modelState?.Where(n => n.Value.Errors.Count > 0).Select(a => new
            {
                Key = a.Key.Replace("model.", ""),
                Error = a.Value.Errors.Select(b => b.ErrorMessage).ToList()
            }).ToList();
            var result = new WebApiResult
            {
                Exception = JsonConvert.SerializeObject(source.Exception, SerializerJsonConfigs.ReferenceLoopHandlingIgnoreJsonConfig()),
                Succeeded = source.Succeeded,
                ModelStateError = new List<object> { errors },
                Messages = (language == "en") ? source.Messages.GetEnglishMessage():source.Messages.GetPersianMessage() ,
            };
            return result.ToHttpResponse(token: token);
        }

    }
}
