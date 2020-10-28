using Amazon.DynamoDBv2.DataModel;
using System;
using Telegram.Altayskaya97.Core.Interface;

namespace Telegram.Altayskaya97.Model.Entity.DynamoDb
{
#if DEBUG
    [DynamoDBTable("User" + "Test")]
#else
    [DynamoDBTable("User")]
#endif
    public class User : IObject
    {
        [DynamoDBHashKey]
        public long Id { get; set; }
        [DynamoDBProperty]
        public string Name { get; set; }
        [DynamoDBProperty]
        public bool IsAdmin { get; set; }
        [DynamoDBProperty]
        public string Telephone { get; set; }
        [DynamoDBProperty]
        public string Type { get; set; } = Core.Model.UserType.Member;
        [DynamoDBProperty]
        public virtual DateTime? LastMessageTime { get; set; }
    }
}
