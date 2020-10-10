using System;
using MongoDB.Driver;
using Telegram.Altayskaya97.Core.Model;
using Telegram.Altayskaya97.Model.Interface;

namespace Telegram.Altayskaya97.Model.DbContext
{
    public class MongoDbContext : IDbContext
    {
        public MongoDbContext()
        {
        }

        IRepository<User> IDbContext.UserRepository => throw new NotImplementedException();

        IRepository<Chat> IDbContext.ChatRepository => throw new NotImplementedException();

        public void Init(string connectionString)
        {
            MongoClient dbClient = new MongoClient(connectionString);

            var dbList = dbClient.ListDatabases().ToList();

            Console.WriteLine("The list of databases on this server is: ");
            foreach (var db in dbList)
            {
                Console.WriteLine(db);
            }
        }
    }
}
