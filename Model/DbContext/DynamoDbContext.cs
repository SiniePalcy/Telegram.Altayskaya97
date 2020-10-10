using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Altayskaya97.Core.Model;
using Telegram.Altayskaya97.Model.Interface;

namespace Telegram.Altayskaya97.Model.DbContext
{
    public class DynamoDbContext : IDbContext, IDisposable
    {
        private AmazonDynamoDBClient _client;
        private DynamoDBContext _dbContext;
        private bool disposedValue;

        #region Repositories
        public IRepository<User> UserRepository { get; }
        public IRepository<Chat> ChatRepository { get; }
        #endregion
        public DynamoDbContext(string connectionString)
        {
            Init(connectionString);
            UserRepository = new Repository.DynamoDb.UserRepository(_dbContext);
            ChatRepository = new Repository.DynamoDb.ChatRepository(_dbContext);
        }

        public void Init(string connectionString)
        {
            var keys = ExtractAccessKeys(connectionString);
            var endpoint =  Amazon.RegionEndpoint.GetBySystemName(keys.Item3);
            _client = new AmazonDynamoDBClient(keys.Item1, keys.Item2, endpoint);
            _dbContext = new DynamoDBContext(_client, new DynamoDBContextConfig { ConsistentRead = true, SkipVersionCheck = true });
        }

        private Tuple<string, string, string> ExtractAccessKeys(string connectionString)
        {
            string[] pairs = connectionString.Split(';');
            string[] accessKeyPair = pairs[0].Split(':');
            string[] secretKeyPair = pairs[1].Split(':');
            string[] regionKeyPair = pairs[2].Split(':');
            return new Tuple<string, string, string>(accessKeyPair[1].Trim(), secretKeyPair[1].Trim(), regionKeyPair[1].Trim());
        }

        public async Task<ICollection<T>> GetCollection<T>()
        {
            var conditions = new List<ScanCondition>();
            return await _dbContext.ScanAsync<T>(conditions).GetRemainingAsync();
        }
      
        #region IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _dbContext.Dispose();
                    _client.Dispose();
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        ~DynamoDbContext()
        {
             // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
             Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
