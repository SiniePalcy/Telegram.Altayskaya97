using System.Collections.Generic;
using Telegram.Altayskaya97.Bot.Enum;

namespace Telegram.Altayskaya97.Bot.StateMachines.UserStates
{
    public class PollUserState : BaseUserState<PollState>
    {
        public bool IsPin { get; set; }
        public string Question { get; set; }
        public ICollection<string> Cases { get; set; }
        public bool IsAnonymous { get; set; }
        public bool IsMultiAnswers { get; set; }

        public PollUserState() 
        {
            Cases = new List<string>();
        }

        public override void ExecuteNextStage()
        {
            if (CurrentState == PollState.None)
            {
                CurrentState = PollState.Start;
                return;
            }

            switch (CurrentState)
            {
                case PollState.Start:
                    CurrentState = PollState.ChatChoice;
                    break;
                case PollState.ChatChoice:
                    CurrentState = PollState.AddQuestion;
                    break;
                case PollState.AddQuestion:
                    CurrentState = PollState.AddCase;
                    break;
                case PollState.AddCase:
                    CurrentState = PollState.FinishCase;
                    break;
                case PollState.FinishCase:
                    CurrentState = PollState.MultiAnswersChoice;
                    break;
                case PollState.MultiAnswersChoice:
                    CurrentState = PollState.AnonymousChoice;
                    break;
                case PollState.AnonymousChoice:
                    CurrentState = PollState.PinChoice;
                    break;
                case PollState.PinChoice:
                    CurrentState = PollState.Confirmation;
                    break;
                case PollState.Confirmation:
                    CurrentState = PollState.Stop;
                    break;
            }
        }

        public override void End()
        {
            CurrentState = PollState.Stop;
        }

        public override bool IsFinished => CurrentState == PollState.Stop;
    }
}
