using System;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Telegram.SafeBot.Model.Interface;

namespace Telegram.SafeBot.Model.DbContext
{
    public class MongoDbContext : IDbContext
    {
        public MongoClient Client { get; private set; }

        public MongoDbContext(IConfiguration configuration)
        {
            var connString = configuration
                .GetSection("ConnectionStrings")
                .GetSection("MongoDbConnectionString")
                .Value;

            Init(connString);
        }

        public void Init(string connectionString)
        {
            Client = new MongoClient(connectionString);

            var dbList = Client.ListDatabases().ToList();

            Console.WriteLine("The list of databases on this server is: ");
            foreach (var db in dbList)
            {
                Console.WriteLine(db);
            }
        }
    }
}
