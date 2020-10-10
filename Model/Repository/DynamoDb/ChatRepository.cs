using Amazon.DynamoDBv2.DataModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Altayskaya97.Core.Model;
using Telegram.Altayskaya97.Model.Interface;
using Telegram.Altayskaya97.Model.Middleware;
using EntityChat = Telegram.Altayskaya97.Model.Entity.DynamoDb.Chat;

namespace Telegram.Altayskaya97.Model.Repository.DynamoDb
{
    public class ChatRepository : IRepository<Chat>
    {
        private readonly static BaseMapper<Chat, EntityChat> _dynamoChatMapper = new BaseMapper<Chat, EntityChat>();
        private readonly DynamoDBContext _dbContext;
        public ChatRepository(DynamoDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ICollection<Chat>> GetCollection()
        {
            var conditions = new List<ScanCondition>();
            var chats = await _dbContext.ScanAsync<EntityChat>(conditions).GetRemainingAsync();
            return _dynamoChatMapper.MapToModelList(chats);
        }

        public async Task ClearCollection()
        {
            var collection = await GetCollection();
            var result = Parallel.ForEach(collection, async item => await RemoveItem(item.Id));
        }

        public async Task<Chat> GetItem(long id)
        {
            EntityChat entityChat = await _dbContext.LoadAsync<EntityChat>(id);
            Chat chat = _dynamoChatMapper.MapToModel(entityChat);
            return chat;
        }

        public async Task AddItem(Chat item)
        {
            EntityChat entityUser = _dynamoChatMapper.MapToEntity(item);
            await _dbContext.SaveAsync(entityUser);
        }

        public async Task UpdateItem(Chat item)
        {
            EntityChat entityUser = _dynamoChatMapper.MapToEntity(item);
            await _dbContext.SaveAsync(entityUser);
        }

        public async Task RemoveItem(long id)
        {
            await _dbContext.DeleteAsync<EntityChat>(id);
        }
    }
}
