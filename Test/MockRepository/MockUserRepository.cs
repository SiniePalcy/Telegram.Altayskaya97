using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Altayskaya97.Core.Model;
using Telegram.Altayskaya97.Model.Interface;

namespace Telegram.Altayskaya97.Test.MockRepository
{
    public class MockUserRepository : IRepository<User>
    {
        private List<User> _users = new List<User>()
        {
            new User { Id = 419930845, Name = "FirstUserReal", IsAdmin = true},
            new User { Id = 418176416, Name = "SecondUserReal" },
            new User { Id = 418176416, Name = "FirstUserFake", IsCoordinator = true, Telephone = "+79777271778"},
        };

        public async Task<ICollection<User>> GetCollection()
        {

            var conditions = new List<ScanCondition>();
            var users = await _dbContext.ScanAsync<EntityUser>(conditions).GetRemainingAsync();
            return _dynamoUserMapper.MapToModelList(users);
        }

        public async Task ClearCollection()
        {
            var collection = await GetCollection();
            var result = Parallel.ForEach(collection, async item => await RemoveItem(item.Id));
        }

        public async Task<User> GetItem(long id)
        {
            EntityUser entityUser = await _dbContext.LoadAsync<EntityUser>(id);
            User user = _dynamoUserMapper.MapToModel(entityUser);
            return user;
        }

        public async Task AddItem(User item)
        {
            EntityUser entityUser = _dynamoUserMapper.MapToEntity(item);
            await _dbContext.SaveAsync(entityUser);
        }

        public async Task UpdateItem(User item)
        {
            EntityUser entityUser = _dynamoUserMapper.MapToEntity(item);
            await _dbContext.SaveAsync(entityUser);
        }

        public async Task RemoveItem(long id)
        {
            await _dbContext.DeleteAsync<EntityUser>(id);
        }
    }
}
