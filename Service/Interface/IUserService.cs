using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Altayskaya97.Core.Model;

namespace Telegram.Altayskaya97.Service.Interface
{
    public interface IUserService : IRepositoryService<User>
    {
        Task<User> GetUser(string userName);
        Task<bool> PromoteUserAdmin(long userId);
        Task<bool> RestrictUser(long userId);
        Task<bool> IsAdmin(long userId);
    }
}
