namespace Telegram.Altayskaya97.Bot.Enum
{
    public abstract class BaseUserState
    {
        public long ChatId { get; set; }
        public bool IsPin { get; set; }
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
