using System;
using Telegram.SafeBot.Core.Interface;

namespace Telegram.SafeBot.Core.Model
{
    [Serializable]
    public class UserMessage : IObject
    {
        public long Id { get; set; }
        public int TelegramId { get; set; }
        public long UserId { get; set; }
        public long ChatId { get; set; }
        public string ChatType { get; set; }
        public string Text { get; set; }
        public DateTime When { get; set; }
        public bool? Pinned { get; set; }
    }
}
