using Telegram.Altayskaya97.Bot.Enum;
using Telegram.Bot.Types;

namespace Telegram.Altayskaya97.Bot.StateMachines.UserStates
{
    public class ClearUserState : BaseUserState<ClearState>
    {
        public ClearUserState() { }

        public override void ExecuteNextStage()
        {
            if (CurrentState == ClearState.None)
            {
                CurrentState = ClearState.Start;
                return;
            }

            switch (CurrentState)
            {
                case ClearState.Start:
                    CurrentState = ClearState.ChatChoice;
                    break;
                case ClearState.ChatChoice:
                    CurrentState = ClearState.Confirmation;
                    break;
                case ClearState.Confirmation:
                    CurrentState = ClearState.Stop;
                    break;
            }
        }

        public override void End()
        {
            CurrentState = ClearState.Stop;
        }

        public override bool IsFinished => CurrentState == ClearState.Stop;
    }
}
