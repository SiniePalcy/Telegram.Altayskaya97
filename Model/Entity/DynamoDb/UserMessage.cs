using Amazon.DynamoDBv2.DataModel;
using System;
using Telegram.Altayskaya97.Core.Interface;

namespace Telegram.Altayskaya97.Model.Entity.DynamoDb
{
#if DEBUG
    [DynamoDBTable("UserMessage" + "Test")]
#else
    [DynamoDBTable("UserMessage")]
#endif
    public class UserMessage : IObject
    {
        [DynamoDBHashKey]
        public long Id { get; set; }
        [DynamoDBProperty]
        public long UserId { get; set; }
        [DynamoDBProperty]
        public long ChatId { get; set; }
        [DynamoDBProperty]
        public string ChatType { get; set; }
        [DynamoDBProperty]
        public string Text { get; set; }
        [DynamoDBProperty]
        public DateTime When { get; set; }
    }
}
