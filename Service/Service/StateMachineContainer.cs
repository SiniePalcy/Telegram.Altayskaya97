using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.SafeBot.Service.Interface;
using Telegram.SafeBot.Service.StateMachines;
using Telegram.BotAPI.AvailableTypes;

namespace Telegram.SafeBot.Service.Service
{
    public class StateMachineContainer : IStateMachineContainer
    {
        IStateMachine[] _stateMachines;
        ILogger<StateMachineContainer> _logger;

        public StateMachineContainer(
            ILogger<StateMachineContainer> logger,
            IChatService chatService, 
            IPasswordService passwordService,
            IUserMessageService userMessageService)
        {
            _logger = logger;
            _stateMachines = new IStateMachine[]
            {
                new PostStateMachine(chatService),
                new PollStateMachine(chatService),
                new ClearStateMachine(chatService),
                new ChangePasswordStateMachine(passwordService),
                new ChangeChatTypeStateMachine(chatService),
                new UnpinMessageStateMachine(chatService, userMessageService)
            };
        }    

        public async Task<CommandResult> StartStateMachine<T>(long userId)
            where T: IStateMachine
        {
            StopAnotherStateMachines(userId);
            var stateMachine = _stateMachines.First(sm => sm.GetType() == typeof(T));
            return await stateMachine.CreateUserStateFlow(userId);
        }

        public async Task<CommandResult> TryProcessStage(long userId, Message message)
        {
            var result = new CommandResult("Something got wrong", CommandResultType.TextMessage);
            var stateMachine = _stateMachines.FirstOrDefault(sm => sm.IsExecuting(userId));
            if (stateMachine != null)
            {
                try
                {
                    result = await stateMachine.ExecuteStage(userId, message);
                }
                catch(Exception ex)
                {
                    _logger.LogError($"Error in processing stage of '{stateMachine.GetType()}' for userId '{userId}'");
                }
            }
            return result;
        }

        private void StopAnotherStateMachines(long userId)
        {
            var stateMachines = _stateMachines.Where(sm => sm.IsExecuting(userId));
            foreach(var stateMachine in stateMachines)
            {
                stateMachine.StopUserStateFlow(userId);
            }
        }
    }
}
