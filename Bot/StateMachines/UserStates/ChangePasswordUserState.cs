using Telegram.Altayskaya97.Bot.Enum;

namespace Telegram.Altayskaya97.Bot.StateMachines.UserStates
{
    public class ChangePasswordUserState : UserState<ChangePasswordState>
    {
        public string ChatType { get; set; }
        public string NewPassword { get; set; }
    }
}
