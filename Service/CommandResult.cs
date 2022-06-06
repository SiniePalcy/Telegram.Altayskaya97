using System.Collections.Generic;
using Telegram.BotAPI.AvailableTypes;

namespace Telegram.Altayskaya97.Service
{
    public enum CommandResultType 
    { 
        None, 
        TextMessage, 
        Message, 
        Links, 
        Image, 
        Delete, 
        Pool,
        ChangePassword,
        ChangeChatType,
        Unpin
    }
    public class CommandResult
    {
        public CommandResultType Type { get; set; } = CommandResultType.None;
        public object Content { get; set; }
        public IDictionary<string, object> Properties { get; set; } =
            new Dictionary<string, object>();
        public ICollection<long> Recievers { get; set; }
        public ReplyMarkup ReplyMarkup { get; set; }

        public CommandResult(object content, 
            CommandResultType type = CommandResultType.None, 
            ReplyMarkup replyMarkup = null)
        {
            Content = content;
            Type = type;
            ReplyMarkup = replyMarkup;
        }
    }
}
