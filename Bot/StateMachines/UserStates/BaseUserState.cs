namespace Telegram.Altayskaya97.Bot.StateMachines.UserStates
{
    public abstract class BaseUserState<T> where T: System.Enum
    {
        public long ChatId { get; set; }
        public T CurrentState { get; set; }
        public BaseUserState()
        {
        }

        public abstract void ExecuteNextStage();
        public abstract void End();
        public abstract bool IsFinished { get; }
    }
}
