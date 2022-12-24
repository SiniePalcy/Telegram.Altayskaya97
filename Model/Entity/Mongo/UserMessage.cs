using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using Telegram.SafeBot.Core.Interface;
using Telegram.SafeBot.Model.Attributes;

namespace Telegram.SafeBot.Model.Entity.MongoDb
{
   
#if DEBUG
    [Collection("UserMessage" + "Test")]
#else
    [Collection("UserMessage")]
#endif
    [BsonIgnoreExtraElements]
    public class UserMessage : IObject
    {
        [BsonId]
        [BsonRepresentation(BsonType.Int64)]
        [BsonElement("id")]
        public long Id { get; set; }

        [BsonElement("telegram_id")]
        public long TelegramId { get; set; }

        [BsonElement("user_id")]
        public long UserId { get; set; }

        [BsonElement("chat_id")]
        public long ChatId { get; set; }

        [BsonElement("chat_type")]
        public string ChatType { get; set; }

        [BsonElement("text")]
        public string Text { get; set; }

        [BsonElement("when")]
        public DateTime When { get; set; }

        [BsonElement("pinned")]
        public bool? Pinned { get; set; }
    }
}
