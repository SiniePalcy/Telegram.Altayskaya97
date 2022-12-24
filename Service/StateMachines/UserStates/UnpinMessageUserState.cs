using Telegram.SafeBot.Core.Enum;

namespace Telegram.SafeBot.Service.StateMachines.UserStates
{
    public class UnpinMessageUserState : UserState<UnpinMessageState>
    {
        public long ChatId { get; set; }
        public long MessageId { get; set; }
    }
}
