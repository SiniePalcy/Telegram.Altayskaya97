using Amazon.DynamoDBv2.DataModel;
using Telegram.Altayskaya97.Core.Interface;

namespace Telegram.Altayskaya97.Model.Entity.DynamoDb
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
