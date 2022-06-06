using Telegram.Altayskaya97.Bot.Enum;

namespace Telegram.Altayskaya97.Bot.StateMachines.UserStates
{
    public class UnpinMessageUserState : UserState<UnpinMessageState>
    {
        public long ChatId { get; set; }
        public long MessageId { get; set; }
    }
}
