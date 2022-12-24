using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.BotAPI.AvailableTypes;

namespace Telegram.SafeBot.Service.Interface
{
    public interface IStateMachineContainer
    {
        Task<CommandResult> StartStateMachine<T>(long userId)
            where T : IStateMachine;

        Task<CommandResult> TryProcessStage(long userId, Message message);
    }
}
