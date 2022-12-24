using Telegram.SafeBot.Core.Interface;

namespace Telegram.SafeBot.Core.Model
{
    public class Password : IObject
    {
        public virtual long Id { get; set; }
        public virtual string ChatType { get; set; }
        public virtual string Value { get; set; }
    }
}
