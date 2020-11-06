using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Altayskaya97.Bot.Model
{
    public class KeyboardButtonWithId : KeyboardButton
    {
        public long Id { get; private set; }
        public KeyboardButtonWithId(long id, string text) : base(text)
        {
            this.Id = id;
        }
    }
}
