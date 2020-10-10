using Telegram.Altayskaya97.Core.Interface;

namespace Telegram.Altayskaya97.Core.Model
{
    public class IAnnouncement : IObject
    {
        public long Id { get; set; }
        public int Title { get; set; }
        public string HtmlText { get; set; }
        public byte[] Image { get; set; }
    }
}
