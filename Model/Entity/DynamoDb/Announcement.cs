using Amazon.DynamoDBv2.DataModel;
using Telegram.Altayskaya97.Core.Interface;

namespace Telegram.Altayskaya97.Model.Entity.DynamoDb
{
#if DEBUG
    [DynamoDBTable("Announcement" + "Test")]
#else
    [DynamoDBTable("Announcement")]
#endif
    public class Announcement : IObject
    {
        [DynamoDBHashKey]
        public long Id { get; set; }
        [DynamoDBProperty]
        public int Title { get; set; }
        [DynamoDBProperty]
        public string HtmlText { get; set; }
        [DynamoDBProperty]
        public byte[] Image { get; set; }
    }
}
