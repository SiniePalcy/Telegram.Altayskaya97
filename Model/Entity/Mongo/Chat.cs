using Amazon.DynamoDBv2.DataModel;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Telegram.SafeBot.Core.Interface;
using Telegram.SafeBot.Model.Attributes;

namespace Telegram.SafeBot.Model.Entity.MongoDb
{
#if DEBUG
    [Collection("Chat" + "Test")]
#else
    [Collection("Chat")]
#endif
    [BsonIgnoreExtraElements]
    public class Chat : IObject
    {
        [BsonId]
        [BsonRepresentation(BsonType.Int64)]
        [BsonElement("id")]
        public long Id { get; set; }

        [BsonElement("chat_type")]
        public string ChatType { get; set; }

        [BsonElement("title")]
        public string Title { get; set; }
    }
}
