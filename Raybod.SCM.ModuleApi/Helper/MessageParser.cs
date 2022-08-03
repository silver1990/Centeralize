using Microsoft.AspNetCore.Mvc.ModelBinding;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Utility.Extention;
using System.Collections.Generic;
using System.Linq;

namespace Raybod.SCM.ModuleApi.Helper
{
    public static class MessageParser
    {
        public static IList<ClientMessage> GetPersianMessage(this IList<ServiceMessage> messages)
        {
            if (messages == null || messages.Count <= 0) return null;
            return messages
                .Select(item => new ClientMessage { Message = item?.Message.GetMessageDescription(), Id = (int)(item?.Message ?? 0), Type = (item?.Type ?? MessageType.Error).ToMessageType() })
                .ToList();
        }
        public static IList<ClientMessage> GetEnglishMessage(this IList<ServiceMessage> messages)
        {
            if (messages == null || messages.Count <= 0) return null;
            return messages
                .Select(item => new ClientMessage { Message = item.Message.GetDisplayName(), Id = (int)(item?.Message ?? 0), Type = (item?.Type ?? MessageType.Error).ToMessageType() })
                .ToList();
        }
        private static string ToPersianMessageString(this MessageId message)
        {
            return message.ToPersianMessageString();
        }

        private static string ToMessageType(this MessageType type)
        {
            var typeViewModel = type;
            return typeViewModel.GetDisplayName();
        }

        internal static List<ClientMessage> ToClientMessage(this string message, MessageType type)
        {
            return new List<ClientMessage> { new ClientMessage { Type = type.GetDisplayName(), Message = message } };
        }

        public static ClientMessage ToSingleClientMessage(this string message, MessageType type)
        {
            return new ClientMessage { Type = type.GetDisplayName(), Message = message };
        }

        public static List<ClientMessage> GetErrors(this ModelStateDictionary modelState)
        {
            return (from item in modelState
                    where item.Value.Errors.Any()
                    select item.Value.Errors[0].ErrorMessage.ToSingleClientMessage(MessageType.Error)).ToList();
        }


    }
}
