using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Altayskaya97.Core.Model;
using Telegram.Altayskaya97.Model.DbContext;
using Telegram.Altayskaya97.Model.Interface;
using Telegram.Altayskaya97.Model.Middleware;

namespace Telegram.Altayskaya97.Model.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDynamoDbRepositories(this IServiceCollection self)
        {
            self.AddSingleton<IDbContext, DynamoDbContext>();

            self.AddSingleton<IRepository<Password>, Repository.DynamoDb.DynamoDbRepository<Entity.DynamoDb.Password, Password, Middleware.DynamoDb.PasswordMapper>>();
            self.AddSingleton<IRepository<User>, Repository.DynamoDb.DynamoDbRepository<Entity.DynamoDb.User, User, BaseMapper<User, Entity.DynamoDb.User>>>();
            self.AddSingleton<IRepository<Chat>, Repository.DynamoDb.DynamoDbRepository<Entity.DynamoDb.Chat, Chat, BaseMapper<Chat, Entity.DynamoDb.Chat>>>();
            self.AddSingleton<IRepository<UserMessage>, Repository.DynamoDb.DynamoDbRepository<Entity.DynamoDb.UserMessage, UserMessage, BaseMapper<UserMessage, Entity.DynamoDb.UserMessage>>>();

            return self;
        }

        public static IServiceCollection AddMongoDbRepositories(this IServiceCollection self)
        {
            self.AddSingleton<IDbContext, MongoDbContext>();

            self.AddSingleton<IRepository<Password>, Repository.MongoDb.MongoDbRepository<Entity.MongoDb.Password, Password, Middleware.MongoDb.PasswordMapper>>();
            self.AddSingleton<IRepository<User>, Repository.MongoDb.MongoDbRepository<Entity.MongoDb.User, User, BaseMapper<User, Entity.MongoDb.User>>>();
            self.AddSingleton<IRepository<Chat>, Repository.MongoDb.MongoDbRepository<Entity.MongoDb.Chat, Chat, BaseMapper<Chat, Entity.MongoDb.Chat>>>();
            self.AddSingleton<IRepository<UserMessage>, Repository.MongoDb.MongoDbRepository<Entity.MongoDb.UserMessage, UserMessage, BaseMapper<UserMessage, Entity.MongoDb.UserMessage>>>();

            return self;
        }
    }
}
