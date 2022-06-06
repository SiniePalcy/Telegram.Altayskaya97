using Telegram.Altayskaya97.Core.Enum;
using Telegram.BotAPI.AvailableTypes;

namespace Telegram.Altayskaya97.Service.StateMachines.UserStates
{
    public class PostUserState : UserState<PostState>
    {
        public long ChatId { get; set; }
        public bool IsPin { get; set; }
        public Message Message { get; set; }
    }
}
