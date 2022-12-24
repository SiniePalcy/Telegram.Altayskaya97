using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.SafeBot.Core.Interface;
using Telegram.SafeBot.Model.Attributes;
using Telegram.SafeBot.Model.DbContext;
using Telegram.SafeBot.Model.Interface;

namespace Telegram.SafeBot.Model.Repository.MongoDb
{
    public class MongoDbRepository<TEntity, TModel, TMapper> : IRepository<TModel>
        where TEntity: IObject, new()
        where TModel : IObject, new()
        where TMapper : IModelEntityMapper<TModel, TEntity>, new()
    {
        private const string DATABASE_NAME = "Bot";

        private static TMapper _mapper;
        private readonly MongoDbContext _dbContext;

        private IMongoCollection<TEntity> _collection;
        private string _tableName;

        public MongoDbRepository(IDbContext dbContext)
        {
            _dbContext = (MongoDbContext) dbContext;
            _mapper = new TMapper();

            Initialize();
        }

        public virtual void Initialize()
        {
            _tableName = ResolveTableName();

            _collection = _dbContext.Client
                .GetDatabase(DATABASE_NAME)
                .GetCollection<TEntity>(_tableName);
        }

        public virtual async Task<ICollection<TModel>> GetCollection()
        {
            var list = await _collection.AsQueryable().ToListAsync();
            return _mapper.MapToModelList(list);
        }

        public virtual async Task ClearCollection()
        {
            var collection = _collection.AsQueryable();
            Parallel.ForEach(collection, async item => await Remove(item.Id));
        }

        public virtual async Task PushCollection(ICollection<TModel> collection)
        {
            var entityColl = _mapper.MapToEntityList(collection);
            await _collection.InsertManyAsync(entityColl);
        }

        public virtual async Task<TModel> Get(long id)
        {
            var entity = await _collection.AsQueryable().FirstOrDefaultAsync(_ => _.Id == id);
            return _mapper.MapToModel(entity);
        }

        public virtual async Task Add(TModel item)
        {
            TEntity entity = _mapper.MapToEntity(item);
            await _collection.InsertOneAsync(entity);
        }

        public virtual async Task Update(long id, TModel item)
        {
            var filter = Builders<TEntity>.Filter.Eq(doc => doc.Id, id);
            await _collection.ReplaceOneAsync(filter, _mapper.MapToEntity(item));
        }

        public virtual async Task Remove(long id)
        {
            var filter =  Builders<TEntity>.Filter.Eq(doc => doc.Id, id);
            await _collection.FindOneAndDeleteAsync(filter);
        }

        private string ResolveTableName()
        {
            var attr = typeof(TEntity)
               .CustomAttributes
               .FirstOrDefault(_ => _.AttributeType == typeof(CollectionAttribute));

            string tableName = typeof(TModel).Name;

            if (attr != null)
            {
                tableName = attr.ConstructorArguments.First().Value.ToString();
            }

            return tableName;
        }
    }
}
