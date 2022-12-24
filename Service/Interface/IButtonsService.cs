using System.Collections.Generic;
using Telegram.SafeBot.Core.Model;
using Telegram.BotAPI.AvailableTypes;

namespace Telegram.SafeBot.Service.Interface
{
    public interface IButtonsService : IService
    {
        string GetWelcomeMessage(string userName);
        IEnumerable<IEnumerable<InlineKeyboardButton>> GetWelcomeButtons(string chatType);
    }
}
