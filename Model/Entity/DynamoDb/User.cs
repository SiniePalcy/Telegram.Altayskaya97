using Amazon.DynamoDBv2.DataModel;
using System;
using Telegram.SafeBot.Core.Interface;

namespace Telegram.SafeBot.Model.Entity.DynamoDb
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
        public DateTime? LastMessageTime { get; set; }
        [DynamoDBProperty]
        public bool? NoWalk { get; set; }
    }
}
