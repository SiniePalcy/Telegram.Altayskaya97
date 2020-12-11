using Telegram.Altayskaya97.Bot.Enum;

namespace Telegram.Altayskaya97.Bot.StateMachines.UserStates
{
    public class ChangeChatTypeUserState : UserState<ChangeChatTypeState>
    {
        public long ChatId { get; set; }
        public string ChatType { get; set; }
    }
}

