using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Telegram.SafeBot.Core.Interface;
using Telegram.SafeBot.Model.Attributes;

namespace Telegram.SafeBot.Model.Entity.MongoDb
{
#if DEBUG
    [Collection("Password" + "Test")]
#else
    [Collection("Password")]
#endif
    [BsonIgnoreExtraElements]
    public class Password : IObject
    {
        [BsonId]
        [BsonRepresentation(BsonType.Int64)]
        [BsonElement("id")]
        public long Id { get; set; }

        [BsonElement("chat_type")]
        public string ChatType { get; set; }

        [BsonElement("hash")]
        public byte[] Hash { get; set; }
    }
}
