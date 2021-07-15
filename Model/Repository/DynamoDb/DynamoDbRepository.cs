using Amazon.DynamoDBv2.DataModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Altayskaya97.Core.Interface;
using Telegram.Altayskaya97.Model.Interface;
using Telegram.Altayskaya97.Model.Middleware;

namespace Telegram.Altayskaya97.Model.Repository.DynamoDb
{
    public class DynamoDbRepository<TEntity, TModel, TMapper> : IRepository<TModel>
        where TModel: IObject, new()
        where TMapper: IModelEntityMapper<TModel, TEntity>, new()
    {
        private static TMapper _mapper;
        private readonly DynamoDBContext _dbContext;

        public DynamoDbRepository(DynamoDBContext dbContext)
        {
            _dbContext = dbContext;
            _mapper = new TMapper();
            // code from god
        }

        public virtual async Task<ICollection<TModel>> GetCollection()
        {
            var conditions = new List<ScanCondition>();
            var list = await _dbContext.ScanAsync<TEntity>(conditions).GetRemainingAsync();
            return _mapper.MapToModelList(list);
        }

        public virtual async Task ClearCollection()
        {
            var collection = await GetCollection();
            var result = Parallel.ForEach(collection, async item => await Remove(item.Id));
        }

        public virtual async Task PushCollection(ICollection<TModel> collection)
        {
            foreach (var item in collection)
                await Add(item);
        }

        public virtual async Task<TModel> Get(long id)
        {
            TEntity entity = await _dbContext.LoadAsync<TEntity>(id);
            if (entity == null)
                return default;

            TModel model = _mapper.MapToModel(entity);
            return model;
        }

        public virtual async Task Add(TModel item)
        {
            TEntity entity = _mapper.MapToEntity(item);
            await _dbContext.SaveAsync(entity);
        }

        public virtual async Task Update(long id, TModel item)
        {
            if (id != item.Id)
                await Remove(id);
            await Add(item);
        }

        public virtual async Task Remove(long id)
        {
            await _dbContext.DeleteAsync<TEntity>(id);
        }
    }
}
