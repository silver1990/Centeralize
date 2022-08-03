using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.User;
using Raybod.SCM.Utility.Extention;
using System;
using System.Linq;

namespace Raybod.SCM.ModuleApi.Helper.Logger
{
    public static class LogerHelper
    {
        public static LoggerInforMationDto ActionExcuted(string actionName, AuthenticateDto authenticate, object postedObj = null)
        {
            var informationText = actionName + " executed at {date} by {UserId}, {UserName}, {UserFullName}, {ContractCode}, {RemoteIpAddress}, {roles}, {postedData}";
            string Project = authenticate.ContractCode;
            var postedData = postedObj == null ? "" : JsonConvert.SerializeObject(postedObj);
            var roles = authenticate.Roles != null && authenticate.Roles.Any() ? JsonConvert.SerializeObject(authenticate.Roles) : "";

            object[] args = new object[8] {
                DateTime.UtcNow,
                authenticate.UserId,
                authenticate.UserName,
                authenticate.UserFullName,
                Project,
                authenticate.RemoteIpAddress,
                roles,
                postedData };

            return new LoggerInforMationDto
            {
                InformationText = informationText,
                Args = args
            };
        }

        public static LoggerInforMationDto ActionExcuted(HttpRequest httpRequest, AuthenticateDto authenticate, object postedObj = null)
        {
            string ControllerName = httpRequest.RouteValues["Controller"].ToString();
            string ActionName = httpRequest.RouteValues["Action"].ToString();
            string Project = authenticate.ContractCode;

            var informationText = "{controllerName} {actionName} executed at {date} by {UserId}, {UserName}, {UserFullName}, {ContractCode}, {RemoteIpAddress}, {roles}, {postedData}";

            var postedData = postedObj == null ? "" : JsonConvert.SerializeObject(postedObj);
            var roles = authenticate.Roles != null && authenticate.Roles.Any() ? JsonConvert.SerializeObject(authenticate.Roles) : "";

            object[] args = new object[10] {
                ControllerName,
                ActionName,
                DateTime.UtcNow,
                authenticate.UserId,
                authenticate.UserName,
                authenticate.UserFullName,
                Project,
                authenticate.RemoteIpAddress,
                roles,
                postedData };

            return new LoggerInforMationDto
            {
                InformationText = informationText,
                Args = args
            };
        }

        public static LoggerInforMationDto ActionSignInExcuted(HttpRequest httpRequest, SigningApiDto model)
        {
            string controllerName = httpRequest.RouteValues["Controller"].ToString();
            string actionName = httpRequest.RouteValues["Action"].ToString();


            var informationText = "{controllerName} {actionName} executed at {date} by {UserName}, {Password}";

            object[] args = new object[5] {
                controllerName,
                actionName,
                DateTime.Now.ToString("yyyy/MM/dd"),
                model.UserName,
                ""
                };

            return new LoggerInforMationDto
            {
                InformationText = informationText,
                Args = args
            };
        }

    }
}
