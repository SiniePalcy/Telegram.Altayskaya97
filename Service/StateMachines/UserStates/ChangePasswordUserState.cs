using Telegram.SafeBot.Core.Enum;

namespace Telegram.SafeBot.Service.StateMachines.UserStates
{
    public class ChangePasswordUserState : UserState<ChangePasswordState>
    {
        public string ChatType { get; set; }
        public string NewPassword { get; set; }
    }
}
