using Telegram.SafeBot.Core.Enum;

namespace Telegram.SafeBot.Service.StateMachines.UserStates
{
    public class ClearUserState : UserState<ClearState>
    {
        public long ChatId { get; set; }
    }
}
