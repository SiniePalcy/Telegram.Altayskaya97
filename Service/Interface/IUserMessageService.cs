using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Altayskaya97.Core.Model;

namespace Telegram.Altayskaya97.Service.Interface
{
    public interface IUserMessageService : IRepositoryService<UserMessage>
    {
        Task Pin(long id);
        Task UnPin(long id);
        Task<UserMessage> Get(long chatId, long telegramId);
    }
}
