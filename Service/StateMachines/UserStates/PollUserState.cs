using System.Collections.Generic;
using Telegram.SafeBot.Core.Enum;

namespace Telegram.SafeBot.Service.StateMachines.UserStates
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
