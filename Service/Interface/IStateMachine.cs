using System.Threading.Tasks;
using Telegram.BotAPI.AvailableTypes;

namespace Telegram.Altayskaya97.Service.Interface
{
    public interface IStateMachine
    {
        Task<CommandResult> CreateUserStateFlow(long userId);
        Task<CommandResult> ExecuteStage(long id, Message message = null);
        bool IsExecuting(long id);
    }
}
