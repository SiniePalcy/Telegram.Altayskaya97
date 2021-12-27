using Telegram.Altayskaya97.Core.Model;

namespace Telegram.Altayskaya97.Model.Interface
{
    public interface IDbContext 
    {
        void Init(string connectionString);
    }
}
