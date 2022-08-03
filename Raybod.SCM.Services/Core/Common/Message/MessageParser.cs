using System.Collections.Generic;
using System.Linq;
using Raybod.SCM.Utility.Extention;

namespace Raybod.SCM.Services.Core.Common.Message
{
    public static class MessageParser
    {
        public static IList<ClientMessage> GetMessageDescription(this IList<ServiceMessage> messages)
        {
            if (messages == null || messages.Count <= 0) return null;
            return messages.Select(item => new ClientMessage { Message = item?.Message.GetEnumDescription(), Type = (item?.Type ?? MessageType.Error).ToMessageType() }).ToList();
        }

        public static string GetMessageDescription(this MessageId message)
        {
            return message.GetEnumDescription();
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

        internal static ClientMessage ToSingleClientMessage(this string message, MessageType type)
        {
            return new ClientMessage { Type = type.GetDisplayName(), Message = message };
        }
    }
}