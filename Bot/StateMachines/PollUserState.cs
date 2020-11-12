using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Altayskaya97.Bot.Enum;
using Telegram.Bot.Types;

namespace Telegram.Altayskaya97.Bot.StateMachines
{
    public class PollUserState : BaseUserState
    {
        public string Question { get; set; }
        public ICollection<string> Cases { get; set; }
        public PollState? CurrentState { get; set; }
        public bool IsAnonymous { get; set; }
        public bool IsMultiAnswers { get; set; }

        public PollUserState(long userId) : base(userId) 
        {
            Cases = new List<string>();
        }

        public override void ExecuteNextStage()
        {
            if (CurrentState == null)
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

        public override bool IsFinished => CurrentState != null && CurrentState == PollState.Stop;
    }
}
