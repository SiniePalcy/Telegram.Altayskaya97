using System.Collections.Generic;
using Telegram.Altayskaya97.Bot.Enum;

namespace Telegram.Altayskaya97.Bot.StateMachines.UserStates
{
    public class PollUserState : UserState<PollState>
    {
        public long ChatId { get; set; }
        public bool IsPin { get; set; }
        public string Question { get; set; }
        public ICollection<string> Cases { get; set; }
        public bool IsAnonymous { get; set; }
        public bool IsMultiAnswers { get; set; }

        public PollUserState() 
        {
            Cases = new List<string>();
        }
    }
}
