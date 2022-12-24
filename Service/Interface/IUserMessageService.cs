using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.SafeBot.Core.Model;

namespace Telegram.SafeBot.Service.Interface
{
    public interface IUserMessageService : IRepositoryService<UserMessage>
    {
        Task Pin(long id);
        Task UnPin(long id);
        Task<UserMessage> Get(long chatId, long telegramId);
    }
}
