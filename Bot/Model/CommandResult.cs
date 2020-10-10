using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Altayskaya97.Bot.Model
{
    public enum CommandResultType { None, Message, Links, Image }
    public class CommandResult
    {
        public CommandResultType Type { get; set; } = CommandResultType.None;
        public string Content { get; set; } = string.Empty;
        public List<Link> Links { get; set; }
        public List<long> Recievers { get; set; }
        public IReplyMarkup ReplyMarkup { get; set; }

        public CommandResult(string content, CommandResultType type = CommandResultType.None, IReplyMarkup replyMarkup = null)
        {
            Content = content;
            Type = type;
            if (type == CommandResultType.Links)
                Links = new List<Link>();
            ReplyMarkup = replyMarkup;
        }
    }
}
