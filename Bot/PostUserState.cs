using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Types;

namespace Telegram.Altayskaya97.Bot
{
    public enum PostState { Start, Stop, ChatChoice, Message, PinChoice, Confirmation };

    public class PostUserState
    {
        public long ChatId { get; set; }
        public Message Message { get; set; }
        public bool IsPin { get; set; }
        public PostState? CurrentState { get; private set; }
        public long UserId { get; private set; }
        public PostUserState(long userId)
        {
            UserId = userId;
        }

        public void ExecuteNextStage()
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

        public void End()
        {
            CurrentState = PostState.Stop;
        }

        public bool IsFinished => CurrentState != null && CurrentState == PostState.Stop;
    }
}
