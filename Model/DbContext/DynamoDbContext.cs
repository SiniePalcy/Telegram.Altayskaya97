using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.Configuration;
using System;
using Telegram.SafeBot.Core.Model;
using Telegram.SafeBot.Model.Interface;
using Telegram.SafeBot.Model.Middleware;
using Telegram.SafeBot.Model.Middleware.DynamoDb;
using Telegram.SafeBot.Model.Repository.DynamoDb;

namespace Telegram.SafeBot.Model.DbContext
{
    public class DynamoDbContext : IDbContext, IDisposable
    {
        private bool disposedValue;

        public DynamoDBContext Context { get; private set; }
        public AmazonDynamoDBClient Client { get; private set; }

        public DynamoDbContext(IConfiguration configuration)
        {
            var connString = configuration
                .GetSection("ConnectionStrings")
                .GetSection("DynamoConnectionString")
                .Value;

            Init(connString);
        }

        public void Init(string connectionString)
        {
            var keys = ExtractAccessKeys(connectionString);
            var endpoint =  Amazon.RegionEndpoint.GetBySystemName(keys.Item3);
            Client = new AmazonDynamoDBClient(keys.Item1, keys.Item2, endpoint);
            Context = new DynamoDBContext(Client, new DynamoDBContextConfig { ConsistentRead = true, SkipVersionCheck = true });
        }

        private Tuple<string, string, string> ExtractAccessKeys(string connectionString)
        {
            string[] pairs = connectionString.Split(';');
            string[] accessKeyPair = pairs[0].Split(':');
            string[] secretKeyPair = pairs[1].Split(':');
            string[] regionKeyPair = pairs[2].Split(':');
            return new Tuple<string, string, string>(accessKeyPair[1].Trim(), secretKeyPair[1].Trim(), regionKeyPair[1].Trim());
        }
     
        #region IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Context.Dispose();
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
