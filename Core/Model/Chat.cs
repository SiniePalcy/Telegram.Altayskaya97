using Telegram.Altayskaya97.Core.Interface;

namespace Telegram.Altayskaya97.Core.Model
{
    public class ChatType 
    {
        public const string Admin = "Admin";
        public const string Private = "Private";
        public const string Public = "Public";
    };

    public class Chat : IObject
    {
        public long Id { get; set; }
        public string ChatType { get; set; }
        public string Title { get; set; }
    }
}
