using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using Telegram.Altayskaya97.Core.Interface;
using Telegram.Altayskaya97.Model.Attributes;

namespace Telegram.Altayskaya97.Model.Entity.MongoDb
{
#if DEBUG
    [Collection("User" + "Test")]
#else
    [Collection("User")]
#endif
    [BsonIgnoreExtraElements]
    public class User : IObject
    {
        [BsonId]
        [BsonRepresentation(BsonType.Int64)]
        [BsonElement("id")]
        public long Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("is_admin")]
        public bool IsAdmin { get; set; }

        [BsonElement("telephone")]
        public string Telephone { get; set; }

        [BsonElement("type")]
        public string Type { get; set; } = Core.Model.UserType.Member;

        [BsonElement("last_message_time")]
        public DateTime? LastMessageTime { get; set; }

        [BsonElement("no_walk")]
        public bool? NoWalk { get; set; }
    }
}
