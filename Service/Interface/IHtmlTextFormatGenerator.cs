using Telegram.BotAPI.AvailableTypes;

namespace Telegram.Altayskaya97.Service.Interface
{
    public interface IHtmlTextFormatGenerator
    {
        string GenerateHtmlText(Message message);
    }
}
