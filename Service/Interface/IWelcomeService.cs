using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Altayskaya97.Service.Interface
{
    public interface IWelcomeService : IService
    {
        string GetWelcomeMessage(string userName);
        IEnumerable<IEnumerable<InlineKeyboardButton>> GetWelcomeButtons(bool isAdmin);
    }
}
