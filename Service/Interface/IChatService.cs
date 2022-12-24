using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.SafeBot.Core.Model;

namespace Telegram.SafeBot.Service.Interface
{
    public interface IChatService : IRepositoryService<Chat>
    {
        Task<Chat> Get(string name);
    }
}
