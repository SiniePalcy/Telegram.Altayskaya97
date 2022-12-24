using Telegram.SafeBot.Core.Enum;
using Telegram.BotAPI.AvailableTypes;

namespace Telegram.SafeBot.Service.StateMachines.UserStates
{
    public class PostUserState : UserState<PostState>
    {
        public long ChatId { get; set; }
        public bool IsPin { get; set; }
        public Message Message { get; set; }
    }
}
