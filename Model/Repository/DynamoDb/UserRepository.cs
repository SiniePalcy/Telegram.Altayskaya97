using Amazon.DynamoDBv2.DataModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Altayskaya97.Core.Model;
using Telegram.Altayskaya97.Model.Interface;
using Telegram.Altayskaya97.Model.Middleware;
using EntityUser = Telegram.Altayskaya97.Model.Entity.DynamoDb.User;

namespace Telegram.Altayskaya97.Model.Repository.DynamoDb
{
    public class UserRepository : IRepository<User>
    {
        private readonly static BaseMapper<User, EntityUser> _dynamoUserMapper = new BaseMapper<User, EntityUser>();
        private readonly DynamoDBContext _dbContext;

        public UserRepository(DynamoDBContext dbContext)
        {
            _dbContext = dbContext;
        }

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
