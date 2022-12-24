using Telegram.SafeBot.Core.Model;

namespace Telegram.SafeBot.Model.Interface
{
    public interface IDbContext 
    {
        void Init(string connectionString);
    }
}
