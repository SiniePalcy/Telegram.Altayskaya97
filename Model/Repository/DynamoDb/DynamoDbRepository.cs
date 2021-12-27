using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Altayskaya97.Core.Interface;
using Telegram.Altayskaya97.Model.DbContext;
using Telegram.Altayskaya97.Model.Interface;

namespace Telegram.Altayskaya97.Model.Repository.DynamoDb
{
    public class DynamoDbRepository<TEntity, TModel, TMapper> : IRepository<TModel>
        where TModel : IObject, new()
        where TMapper : IModelEntityMapper<TModel, TEntity>, new()
    {
        private static TMapper _mapper;
        private readonly DynamoDbContext _dbContext;
        private readonly ILogger<DynamoDbRepository<TEntity, TModel, TMapper>> _logger;
       
        private DynamoDBOperationConfig _addOperationConfig;
        private string _tableName;

        public DynamoDbRepository(
            IDbContext dbContext,
            ILogger<DynamoDbRepository<TEntity, TModel, TMapper>> logger
        )
        {
            _logger = logger;
            _dbContext = (DynamoDbContext) dbContext;
            _mapper = new TMapper();

            Initialize();
        }

        public virtual void Initialize()
        {
            var attr = typeof(TEntity)
                .CustomAttributes
                .FirstOrDefault(_ => _.AttributeType == typeof(DynamoDBTableAttribute));

            _tableName = ResolveTableName();

            _addOperationConfig = new DynamoDBOperationConfig
            {
                OverrideTableName = _tableName
            };

            if (!IsTableExists())
            {
                CreateTable();
            }
        }

        public virtual async Task<ICollection<TModel>> GetCollection()
        {
            var conditions = new List<ScanCondition>();
            var list = await _dbContext.Context.ScanAsync<TEntity>(conditions).GetRemainingAsync();
            return _mapper.MapToModelList(list);
        }

        public virtual async Task ClearCollection()
        {
            var collection = await GetCollection();
            Parallel.ForEach(collection, async item => await Remove(item.Id));
        }

        public virtual async Task PushCollection(ICollection<TModel> collection)
        {
            var coll = await GetCollection();
            Parallel.ForEach(coll, async item => await Add(item));
        }

        public virtual async Task<TModel> Get(long id)
        {
            TEntity entity = await _dbContext.Context.LoadAsync<TEntity>(id);
            if (entity == null)
                return default;

            TModel model = _mapper.MapToModel(entity);
            return model;
        }

        public virtual async Task Add(TModel item)
        {
            TEntity entity = _mapper.MapToEntity(item);
            await _dbContext.Context.SaveAsync(entity, _addOperationConfig);
        }

        public virtual async Task Update(long id, TModel item)
        {
            if (id != item.Id)
                await Remove(id);
            await Add(item);
        }

        public virtual async Task Remove(long id)
        {
            await _dbContext.Context.DeleteAsync<TEntity>(id);
        }

        private void CreateTable()
        {
            var request = new CreateTableRequest
            {
                TableName = _tableName,
                AttributeDefinitions = new List<AttributeDefinition>()
                {
                    new AttributeDefinition
                    {
                        AttributeName = "Id",
                        AttributeType = "N"
                    }
                },
                KeySchema = new List<KeySchemaElement>()
                {
                    new KeySchemaElement
                    {
                        AttributeName = "Id",
                        KeyType = "HASH"
                    }
                },
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = 1,
                    WriteCapacityUnits = 1
                }
            };

            var response = _dbContext.Client.CreateTableAsync(request).Result;
            _logger.LogDebug($"Creating table completed with code={response.HttpStatusCode}");
        }

        private bool IsTableExists()
        {
            var tableList = _dbContext.Client.ListTablesAsync().Result;
            return tableList.TableNames.Contains(_tableName);
        }

        private string ResolveTableName()
        {
            var attr = typeof(TEntity)
               .CustomAttributes
               .FirstOrDefault(_ => _.AttributeType == typeof(DynamoDBTableAttribute));

            string tableName = typeof(TModel).Name;

            if (attr != null)
            {
                tableName = attr.ConstructorArguments.First().Value.ToString();
            }

            return tableName;
        }
    }
}
