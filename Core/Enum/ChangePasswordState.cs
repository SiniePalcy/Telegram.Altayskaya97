namespace Telegram.SafeBot.Core.Enum
{
    public enum ChangePasswordState
    {
        None,
        Start,
        PasswordTypeChoice,
        NewPasswordInput,
        Confirmation,
        Stop
    }
}
