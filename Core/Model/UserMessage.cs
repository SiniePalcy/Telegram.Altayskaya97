using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Altayskaya97.Core.Interface;

namespace Telegram.Altayskaya97.Core.Model
{
    public class UserMessage : IObject
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long ChatId { get; set; }
        public string ChatType { get; set; }
        public string Text { get; set; }
        public DateTime When { get; set; }
    }
}
