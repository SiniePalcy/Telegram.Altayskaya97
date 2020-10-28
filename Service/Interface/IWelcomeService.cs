using System.Collections.Generic;
using Telegram.Altayskaya97.Core.Model;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Altayskaya97.Service.Interface
{
    public interface IWelcomeService : IService
    {
        string GetWelcomeMessage(string userName);
        IEnumerable<IEnumerable<InlineKeyboardButton>> GetWelcomeButtons(string chatType);
    }
}
