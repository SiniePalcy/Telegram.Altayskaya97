using Telegram.BotAPI.AvailableTypes;

namespace Telegram.SafeBot.Service.Interface
{
    public interface IHtmlTextFormatGenerator
    {
        string GenerateHtmlText(Message message);
    }
}
