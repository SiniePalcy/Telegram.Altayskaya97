using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Altayskaya97.Core.Model;
using Telegram.Altayskaya97.Model.DbContext;
using Telegram.Altayskaya97.Model.Interface;
using Telegram.Altayskaya97.Model.Middleware;
using Telegram.Altayskaya97.Model.Middleware.DynamoDb;
using Telegram.Altayskaya97.Model.Repository.DynamoDb;

namespace Telegram.Altayskaya97.Model.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection self)
        {
            self.AddSingleton<IDbContext, DynamoDbContext>();

            self.AddSingleton<IRepository<Password>, DynamoDbRepository<Entity.DynamoDb.Password, Password, PasswordMapper>>();
            self.AddSingleton<IRepository<User>, DynamoDbRepository<Entity.DynamoDb.User, User, BaseMapper<User, Entity.DynamoDb.User>>>();
            self.AddSingleton<IRepository<Chat>, DynamoDbRepository<Entity.DynamoDb.Chat, Chat, BaseMapper<Chat, Entity.DynamoDb.Chat>>>();
            self.AddSingleton<IRepository<UserMessage>, DynamoDbRepository<Entity.DynamoDb.UserMessage, UserMessage, BaseMapper<UserMessage, Entity.DynamoDb.UserMessage>>>();

            return self;
        }
    }
}
