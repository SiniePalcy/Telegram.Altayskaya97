using Amazon.DynamoDBv2.DataModel;
using Telegram.SafeBot.Core.Interface;

namespace Telegram.SafeBot.Model.Entity.DynamoDb
{
#if DEBUG
    [DynamoDBTable("Chat" + "Test")]
#else
    [DynamoDBTable("Chat")]
#endif
    public class Chat : IObject
    {
        [DynamoDBHashKey]
        public long Id { get; set; }
        [DynamoDBProperty]
        public string ChatType { get; set; }
        [DynamoDBProperty]
        public string Title { get; set; }
    }
}
