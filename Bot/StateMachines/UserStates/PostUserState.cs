using Telegram.Altayskaya97.Bot.Enum;
using Telegram.Bot.Types;

namespace Telegram.Altayskaya97.Bot.StateMachines.UserStates
{
    public class PostUserState : UserState<PostState>
    {
        public long ChatId { get; set; }
        public bool IsPin { get; set; }
        public Message Message { get; set; }
    }
}
