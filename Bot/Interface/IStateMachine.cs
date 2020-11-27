using System.Threading.Tasks;
using Telegram.Altayskaya97.Bot.Model;
using Telegram.Bot.Types;

namespace Telegram.Altayskaya97.Bot.Interface
{
    public interface IStateMachine
    {
        Task<CommandResult> CreateProcessing(long userId);
        Task<CommandResult> ExecuteStage(long id, Message message = null);
        bool IsExecuting(long id);
    }
}
