using Telegram.Altayskaya97.Bot.Enum;
using Telegram.Bot.Types;

namespace Telegram.Altayskaya97.Bot.StateMachines.UserStates
{
    public class PostUserState : BaseUserState<PostState>
    {
        public bool IsPin { get; set; }
        public Message Message { get; set; }

        public PostUserState() { }

        public override void ExecuteNextStage()
        {
            if (CurrentState == PostState.None)
            {
                CurrentState = PostState.Start;
                return;
            }

            switch (CurrentState)
            {
                case PostState.Start:
                    CurrentState = PostState.ChatChoice;
                    break;
                case PostState.ChatChoice:
                    CurrentState = PostState.Message;
                    break;
                case PostState.Message:
                    CurrentState = PostState.PinChoice;
                    break;
                case PostState.PinChoice:
                    CurrentState = PostState.Confirmation;
                    break;
                case PostState.Confirmation:
                    CurrentState = PostState.Stop;
                    break;
            }
        }

        public override void End()
        {
            CurrentState = PostState.Stop;
        }

        public override bool IsFinished => CurrentState == PostState.Stop;
    }
}
