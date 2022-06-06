using Telegram.Altayskaya97.Core.Enum;

namespace Telegram.Altayskaya97.Service.StateMachines.UserStates
{
    public class ClearUserState : UserState<ClearState>
    {
        public long ChatId { get; set; }
    }
}
