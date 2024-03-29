﻿using System;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Telegram.Altayskaya97.Core.Model;
using Telegram.Altayskaya97.Model.Interface;

namespace Telegram.Altayskaya97.Model.DbContext
{
    public class MongoDbContext : IDbContext
    {
        public MongoClient Client { get; private set; }

        public MongoDbContext(IConfiguration configuration)
        {
            var connString = configuration
                .GetSection("Configuration")
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
