using System.Threading.Tasks;
using Telegram.Altayskaya97.Core.Model;

namespace Telegram.Altayskaya97.Service.Interface
{
    public interface IPasswordService : IRepositoryService<Password>
    {
        Task<Password> GetByType(string chatType);
        Task<bool> IsMemberPass(string password);
        Task<bool> IsAdminPass(string password);
    }
}
