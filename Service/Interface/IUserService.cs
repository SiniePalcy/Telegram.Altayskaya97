using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Altayskaya97.Core.Model;

namespace Telegram.Altayskaya97.Service.Interface
{
    public interface IUserService : IService
    {
        Task<ICollection<User>> GetUserList();
        Task<User> GetUser(long userId);
        Task<User> GetUser(string userName);
        Task<bool> PromoteUserAdmin(long userId);
        Task<bool> RestrictUser(long userId);
        Task AddUser(User user);
        Task DeleteUser(long userId);
        Task<bool> IsAdmin(long userId);
    }
}
