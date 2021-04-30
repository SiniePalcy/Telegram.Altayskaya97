using System;
using Telegram.Altayskaya97.Core.Interface;

namespace Telegram.Altayskaya97.Core.Model
{
    public class UserType
    {
        public const string Coordinator = "Coordinator";
        public const string Admin = "Admin";
        public const string Member = "Member";
        public const string Bot = "Bot";
    };

    [Serializable]
    public class User : IObject
    {
        public virtual long Id { get; set; }
        public virtual string Name { get; set; }
        public virtual bool IsAdmin { get; set; }
        public virtual string Telephone { get; set; }
        public virtual string Type { get; set; } = UserType.Member;
        public virtual DateTime? LastMessageTime { get; set; }
        public virtual bool? NoWalk { get; set; }
    }
}
