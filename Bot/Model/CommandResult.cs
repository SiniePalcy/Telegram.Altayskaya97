using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Altayskaya97.Bot.Model
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
        ChangePassword
    }
    public class CommandResult
    {
        public CommandResultType Type { get; set; } = CommandResultType.None;
        public object Content { get; set; }
        public IDictionary<string, object> Properties { get; set; } =
            new Dictionary<string, object>();
        public ICollection<long> Recievers { get; set; }
        public IReplyMarkup ReplyMarkup { get; set; }

        public CommandResult(object content, 
            CommandResultType type = CommandResultType.None, 
            IReplyMarkup replyMarkup = null)
        {
            Content = content;
            Type = type;
            ReplyMarkup = replyMarkup;
        }
    }
}
