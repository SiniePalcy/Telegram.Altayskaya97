using Telegram.Altayskaya97.Core.Enum;

namespace Telegram.Altayskaya97.Service.StateMachines.UserStates
{
    public class ChangeChatTypeUserState : UserState<ChangeChatTypeState>
    {
        public long ChatId { get; set; }
        public string ChatType { get; set; }
    }
}

