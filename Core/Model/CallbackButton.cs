using System;
using System.Collections.Generic;
using System.Text;

namespace Telegram.Altayskaya97.Core.Model
{
    public class CallbackButton : Button
    {
        public string CallbackName { get; set; }
        public CallbackButton(string title, string callbackName)
        {
            Title = title;
            CallbackName = callbackName;
        }
    }
}
