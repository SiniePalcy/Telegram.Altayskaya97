using System;
using System.Collections.Generic;
using System.Text;

namespace Telegram.Altayskaya97.Core.Model
{
    [Serializable]
    public class BackupContaner
    {
        public ICollection<User> Users { get; set; }
        public ICollection<UserMessage> UserMessages { get; set; }
        public ICollection<Chat> Chats { get; set; }
    }
}
