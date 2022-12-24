using System.Threading.Tasks;
using Telegram.SafeBot.Core.Model;

namespace Telegram.SafeBot.Service.Interface
{
    public interface IPasswordService : IRepositoryService<Password>
    {
        Task<Password> GetByType(string chatType);
        Task<bool> IsMemberPass(string password);
        Task<bool> IsAdminPass(string password);
    }
}
