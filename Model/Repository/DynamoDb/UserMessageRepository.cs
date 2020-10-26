using Amazon.DynamoDBv2.DataModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Altayskaya97.Core.Model;
using Telegram.Altayskaya97.Model.Interface;
using Telegram.Altayskaya97.Model.Middleware;
using EntityUserMessage = Telegram.Altayskaya97.Model.Entity.DynamoDb.UserMessage;

namespace Telegram.Altayskaya97.Model.Repository.DynamoDb
{
    public class UserMessageRepository : IRepository<UserMessage>
    {
        private readonly static BaseMapper<UserMessage, EntityUserMessage> _dynamoChatMapper = new BaseMapper<UserMessage, EntityUserMessage>();
        private readonly DynamoDBContext _dbContext;
        public UserMessageRepository(DynamoDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ICollection<UserMessage>> GetCollection()
        {
            var conditions = new List<ScanCondition>();
            var userMessages = await _dbContext.ScanAsync<EntityUserMessage>(conditions).GetRemainingAsync();
            return _dynamoChatMapper.MapToModelList(userMessages);
        }

        public async Task ClearCollection()
        {
            var collection = await GetCollection();
            var result = Parallel.ForEach(collection, async item => await RemoveItem(item.Id));
        }

        public async Task<UserMessage> GetItem(long id)
        {
            EntityUserMessage entityUserMessage = await _dbContext.LoadAsync<EntityUserMessage>(id);
            UserMessage userMessage = _dynamoChatMapper.MapToModel(entityUserMessage);
            return userMessage;
        }

        public async Task AddItem(UserMessage item)
        {
            EntityUserMessage entityUser = _dynamoChatMapper.MapToEntity(item);
            await _dbContext.SaveAsync(entityUser);
        }

        public async Task UpdateItem(UserMessage item)
        {
            EntityUserMessage entityUser = _dynamoChatMapper.MapToEntity(item);
            await _dbContext.SaveAsync(entityUser);
        }

        public async Task RemoveItem(long id)
        {
            await _dbContext.DeleteAsync<EntityUserMessage>(id);
        }
    }
}
