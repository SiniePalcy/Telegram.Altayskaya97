using Telegram.Altayskaya97.Core.Enum;

namespace Telegram.Altayskaya97.Service.StateMachines.UserStates
{
    public class ChangePasswordUserState : UserState<ChangePasswordState>
    {
        public string ChatType { get; set; }
        public string NewPassword { get; set; }
    }
}
