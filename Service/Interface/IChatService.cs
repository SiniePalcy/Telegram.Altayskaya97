using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Altayskaya97.Core.Model;

namespace Telegram.Altayskaya97.Service.Interface
{
    public interface IChatService : IService
    {
        Task<ICollection<Chat>> GetChatList();
        Task AddChat(Chat chat);
        Task<Chat> GetChat(long id);
        Task<Chat> GetChat(string name);
        Task DeleteChat(long chatId);
    }
}
