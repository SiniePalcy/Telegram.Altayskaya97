using Telegram.Altayskaya97.Core.Interface;

namespace Telegram.Altayskaya97.Core.Model
{
    public class Password : IObject
    {
        public virtual long Id { get; set; }
        public virtual string ChatType { get; set; }
        public virtual string Value { get; set; }
    }
}
