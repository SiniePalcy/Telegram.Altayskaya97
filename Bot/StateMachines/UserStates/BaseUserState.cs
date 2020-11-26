namespace Telegram.Altayskaya97.Bot.StateMachines.UserStates
{
    public abstract class BaseUserState<T> where T: System.Enum
    {
        public long ChatId { get; set; }
        public bool IsPin { get; set; }
        public T CurrentState { get; set; }
        public virtual long UserId { get; protected set; }
        public BaseUserState(long userId)
        {
            UserId = userId;
        }

        public abstract void ExecuteNextStage();
        public abstract void End();
        public abstract bool IsFinished { get; }
    }
}
