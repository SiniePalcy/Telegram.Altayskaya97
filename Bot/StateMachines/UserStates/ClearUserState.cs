using Telegram.Altayskaya97.Bot.Enum;

namespace Telegram.Altayskaya97.Bot.StateMachines.UserStates
{
    public class ClearUserState : UserState<ClearState>
    {
        public long ChatId { get; set; }
    }
}
