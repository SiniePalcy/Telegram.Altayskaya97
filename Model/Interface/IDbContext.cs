using Telegram.Altayskaya97.Core.Model;

namespace Telegram.Altayskaya97.Model.Interface
{
    public interface IDbContext 
    {
        IRepository<User> UserRepository { get; }
        IRepository<Chat> ChatRepository { get; }
        IRepository<UserMessage> UserMessageRepository { get; }
        IRepository<Password> PasswordRepository { get; }
        void Init(string connectionString);
    }
}
