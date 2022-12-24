using System;
using System.Collections.Generic;
using System.Text;

namespace Telegram.SafeBot.Core.Model
{
    [Serializable]
    public class BackupContaner
    {
        public ICollection<User> Users { get; set; }
        public ICollection<UserMessage> UserMessages { get; set; }
        public ICollection<Chat> Chats { get; set; }
    }
}
