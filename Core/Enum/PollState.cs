namespace Telegram.SafeBot.Core.Enum
{
    public enum PollState 
    { 
        None,
        Start, 
        ChatChoice, 
        AddQuestion, 
        AddCase,
        FinishCase,
        MultiAnswersChoice,
        AnonymousChoice, 
        PinChoice, 
        Confirmation,
        Stop
    }
}
