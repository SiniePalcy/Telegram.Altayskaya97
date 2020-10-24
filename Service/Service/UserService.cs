using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Altayskaya97.Core.Model;
using Telegram.Altayskaya97.Model.Interface;
using Telegram.Altayskaya97.Service.Interface;

namespace Telegram.Altayskaya97.Service
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _repo;
        private readonly ILogger<UserService> _logger;

        public UserService(IDbContext dbContext, ILogger<UserService> logger)
        {
            _repo = dbContext.UserRepository;
            _logger = logger;
        }

        public async Task<ICollection<User>> GetUserList()
        {
            return await _repo.GetCollection();
        }

        public async Task<User> GetUser(long userId)
        {
            return await _repo.GetItem(userId);
        }

        public async Task<User> GetUser(string userName)
        {
            var users = await _repo.GetCollection();
            return users.FirstOrDefault(u => u.Name.ToLower() == userName.ToLower().Trim());
        }

        public async Task<bool> PromoteUserAdmin(long userId)
        {
            User user = await _repo.GetItem(userId);
            if (user == null)
                return false;
            
            user.IsAdmin = true;
            await _repo.UpdateItem(user);
            _logger.LogInformation($"User {user.Name} has been promoted to admin");
            return true;
        }

        public async Task<bool> RestrictUser(long userId)
        {
            var user = await _repo.GetItem(userId);
            if (user == null)
                return false;

            user.IsAdmin = false;
            await _repo.UpdateItem(user);
            _logger.LogInformation($"User {user.Name} has been restricted");
            return true;
        }

        public async Task<bool> BanUser(long userId)
        {
            var user = await _repo.GetItem(userId);
            if (user == null)
                return false;

            user.IsBlocked = true;
            await _repo.UpdateItem(user);
            _logger.LogInformation($"User {user.Name} has been blocked");
            return true;
        }

        public async Task<bool> UnbanUser(long userId)
        {
            var user = await _repo.GetItem(userId);
            if (user == null)
                return false;

            user.IsBlocked = false;
            await _repo.UpdateItem(user);
            _logger.LogInformation($"User {user.Name} has been unblocked");
            return true;
        }

        public async Task<bool> BanUser(string userName)
        {
            var users = await _repo.GetCollection();
            var findUser = users.FirstOrDefault(u => u.Name.ToLower() == userName.ToLower());
            if (findUser == null)
                return false;

            return await BanUser(findUser.Id);
        }

        public async Task<bool> UnbanUser(string userName)
        {
            var users = await _repo.GetCollection();
            var findUser = users.FirstOrDefault(u => u.Name.ToLower() == userName.ToLower());
            if (findUser == null)
                return false;

            return await UnbanUser(findUser.Id);
        }

        public async Task AddUser(User user)
        {
            await _repo.AddItem(user);
            _logger.LogInformation($"User {user.Name} has been added");
        }

        public async Task DeleteUser(long userId)
        {
            var user = await _repo.GetItem(userId);
            if (user == null)
                return;

            await _repo.RemoveItem(userId);
            _logger.LogInformation($"User {user.Name} has been deleted");
        }

        public async Task<bool> IsAdmin(long userId)
        {
            User user = await _repo.GetItem(userId);
            if (user == null)
                return false;

            return user.IsAdmin;
        }

        public async Task<bool> IsBlocked(long userId)
        {
            User user = await _repo.GetItem(userId);
            if (user == null)
                return false;

            return user.IsBlocked;
        }
    }
}
