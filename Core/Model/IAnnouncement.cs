using Telegram.SafeBot.Core.Interface;

namespace Telegram.SafeBot.Core.Model
{
    public class IAnnouncement : IObject
    {
        public long Id { get; set; }
        public int Title { get; set; }
        public string HtmlText { get; set; }
        public byte[] Image { get; set; }
    }
}
