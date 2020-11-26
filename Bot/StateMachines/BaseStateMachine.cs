using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Altayskaya97.Bot.Enum;
using Telegram.Altayskaya97.Bot.Interface;
using Telegram.Altayskaya97.Bot.Model;
using Telegram.Altayskaya97.Bot.StateMachines.UserStates;
using Telegram.Altayskaya97.Service.Interface;
using Telegram.Bot.Types;

namespace Telegram.Altayskaya97.Bot.StateMachines
{
    public abstract class BaseStateMachine<State> : IStateMachine where State : System.Enum
    {
        protected ConcurrentDictionary<long, BaseUserState<State>> Processings { get; set; }
        public IChatService ChatService { get; protected set; }
        public BaseStateMachine(IChatService chatService) 
        {
            ChatService = chatService;
            Processings = new ConcurrentDictionary<long, BaseUserState<State>>();
        }

        public async Task<CommandResult> CreateProcessing(long userId)
        {
            if (!StopProcessing(userId))
                return new CommandResult(Core.Constant.Messages.UnknownError, CommandResultType.TextMessage);

            CommandResult result = new CommandResult(Core.Constant.Messages.UnknownError, CommandResultType.TextMessage);

            var postProcessing = CreateUserState(userId);

            if (Processings.TryAdd(userId, postProcessing))
            {
                result = await ExecuteStage(userId);
            }

            return result;
        }

        protected BaseUserState<State> GetProcessing(long id)
        {
            return Processings.TryGetValue(id, out BaseUserState<State> postProcessing) ? postProcessing : null;
        }

        public bool IsExecuting(long id)
        {
            var processing = GetProcessing(id);
            return processing != null && !processing.IsFinished;
        }

        public bool StopProcessing(long id)
        {
            BaseUserState<State> postProcessing = GetProcessing(id);
            if (postProcessing == null)
                return true;

            return Processings.TryRemove(id, out _);
        }

        public abstract Task<CommandResult> ExecuteStage(long id, Message message = null);

        protected abstract BaseUserState<State> CreateUserState(long userId);
    }
}
