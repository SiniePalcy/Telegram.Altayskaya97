using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Altayskaya97.Bot.Model
{
    public enum CommandResultType { None, TextMessage, Message, Links, Image, KeyboardButtons }
    public class CommandResult
    {
        public CommandResultType Type { get; set; } = CommandResultType.None;
        public object Content { get; set; }
        public bool IsPin { get; set; }
        public ICollection<Link> Links { get; set; }
        public ICollection<KeyboardButtonWithId> KeyboardButtons { get; set; }
        public ICollection<long> Recievers { get; set; }
        public IReplyMarkup ReplyMarkup { get; set; }

        public CommandResult(object content, 
            CommandResultType type = CommandResultType.None, 
            IReplyMarkup replyMarkup = null)
        {
            Content = content;
            Type = type;
            if (type == CommandResultType.Links)
                Links = new List<Link>();
            ReplyMarkup = replyMarkup;
        }
    }
}
