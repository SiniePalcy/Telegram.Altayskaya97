using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Altayskaya97.Core.Model;

namespace Telegram.Altayskaya97.Service.Interface
{
    public interface IChatService : IRepositoryService<Chat>
    {
        Task<Chat> Get(string name);
    }
}
