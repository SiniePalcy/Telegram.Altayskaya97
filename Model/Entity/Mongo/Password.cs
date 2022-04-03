using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Telegram.Altayskaya97.Core.Interface;
using Telegram.Altayskaya97.Model.Attributes;

namespace Telegram.Altayskaya97.Model.Entity.MongoDb
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
