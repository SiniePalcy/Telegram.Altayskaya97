using Telegram.SafeBot.Core.Enum;

namespace Telegram.SafeBot.Service.StateMachines.UserStates
{
    public class ChangeChatTypeUserState : UserState<ChangeChatTypeState>
    {
        public long ChatId { get; set; }
        public string ChatType { get; set; }
    }
}

