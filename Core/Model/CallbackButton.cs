namespace Telegram.SafeBot.Core.Model
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
