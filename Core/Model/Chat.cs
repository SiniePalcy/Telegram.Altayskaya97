using System;
using Telegram.SafeBot.Core.Interface;

namespace Telegram.SafeBot.Core.Model
{
    public class ChatType 
    {
        public const string Admin = "Admin";
        public const string Private = "Private";
        public const string Public = "Public";
    };

    [Serializable]
    public class Chat : IObject
    {
        public long Id { get; set; }
        public string ChatType { get; set; }
        public string Title { get; set; }
    }
}
