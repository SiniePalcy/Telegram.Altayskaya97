using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Altayskaya97.Core.Model;

namespace Telegram.Altayskaya97.Service.Interface
{
    public interface IUserMessageService : IService
    {
        Task<ICollection<UserMessage>> GetUserMessageList();
        Task AddUserMessage(UserMessage chat);
        Task<UserMessage> GetUserMessage(long id);
        Task DeleteUserMessage(long chatId);
    }
}
