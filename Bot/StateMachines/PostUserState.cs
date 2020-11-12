﻿using Telegram.Altayskaya97.Bot.Enum;
using Telegram.Bot.Types;

namespace Telegram.Altayskaya97.Bot.StateMachines
{
    public class PostUserState : BaseUserState
    {
        public Message Message { get; set; }
        public PostState? CurrentState { get; private set; }

        public PostUserState(long userId) : base(userId) { }

        public override void ExecuteNextStage()
        {
            if (CurrentState == null)
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

        public override bool IsFinished => CurrentState != null && CurrentState == PostState.Stop;
    }
}
