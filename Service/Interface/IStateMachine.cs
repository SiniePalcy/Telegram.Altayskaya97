using System.Threading.Tasks;
using Telegram.BotAPI.AvailableTypes;

namespace Telegram.SafeBot.Service.Interface
{
    public interface IStateMachine
    {
        Task<CommandResult> CreateUserStateFlow(long userId);
        bool StopUserStateFlow(long userId);
        Task<CommandResult> ExecuteStage(long id, Message message = null);
        bool IsExecuting(long id);
    }
}
