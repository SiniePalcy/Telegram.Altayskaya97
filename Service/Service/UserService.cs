using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Altayskaya97.Core.Model;
using Telegram.Altayskaya97.Model.Interface;
using Telegram.Altayskaya97.Service.Interface;

namespace Telegram.Altayskaya97.Service
{
    public class UserService : RepositoryService<User>, IUserService
    {
        private readonly ILogger<UserService> _logger;

        public UserService(IDbContext dbContext, ILogger<UserService> logger)
        {
            _repo = dbContext.UserRepository;
            _logger = logger;
        }

        public async Task<User> GetUser(string userName)
        {
            var users = await _repo.GetCollection();
            return users.FirstOrDefault(u => u.Name.ToLower() == userName.ToLower().Trim());
        }

        public async Task<bool> PromoteUserAdmin(long userId)
        {
            User user = await _repo.Get(userId);
            if (user == null || user.Type == UserType.Member)
                return false;
            
            user.IsAdmin = true;
            await _repo.Update(user.Id, user);
            _logger.LogInformation($"User {user.Name} has been promoted to admin");
            return true;
        }

        public async Task<bool> RestrictUser(long userId)
        {
            var user = await _repo.Get(userId);
            if (user == null || user.Type == UserType.Member || user.Type == UserType.Bot)
                return false;

            user.IsAdmin = false;
            await _repo.Update(user.Id, user);
            _logger.LogInformation($"User {user.Name} has been restricted");
            return true;
        }

        public override async Task Add(User user)
        {
            await base.Add(user);
            _logger.LogInformation($"User {user.Name} has been added");
        }

        public override async Task<User> Delete(long userId)
        {
            var user = await base.Delete(userId);
            if (user != null)
                _logger.LogInformation($"User {user.Name} has been deleted");
            return user;
        }

        public async Task<bool> IsAdmin(long userId)
        {
            User user = await _repo.Get(userId);
            if (user == null)
                return false;

            return user.Type == UserType.Admin && user.IsAdmin;
        }

        public override async Task Update(long id, User updatedItem)
        {
            await base.Update(id, updatedItem);
            _logger.LogInformation($"User was updated: {updatedItem}");
        }
    }
}
