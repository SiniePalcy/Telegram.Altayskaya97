using Telegram.Altayskaya97.Core.Enum;

namespace Telegram.Altayskaya97.Service.StateMachines.UserStates
{
    public class UnpinMessageUserState : UserState<UnpinMessageState>
    {
        public long ChatId { get; set; }
        public long MessageId { get; set; }
    }
}
