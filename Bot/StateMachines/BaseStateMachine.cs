using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Altayskaya97.Bot.Enum;
using Telegram.Altayskaya97.Bot.Model;
using Telegram.Altayskaya97.Service.Interface;
using Telegram.Bot.Types;

namespace Telegram.Altayskaya97.Bot.StateMachines
{
    public abstract class BaseStateMachine
    {
        protected ConcurrentDictionary<long, BaseUserState> Processings { get; set; }
        public IChatService ChatService { get; protected set; }
        public BaseStateMachine(IChatService chatService) 
        {
            ChatService = chatService;
            Processings = new ConcurrentDictionary<long, BaseUserState>();
        }

        public async Task<CommandResult> CreateProcessing(long userId)
        {
            if (!StopProcessing(userId))
                return new CommandResult(Core.Constant.Messages.UnknownError, CommandResultType.TextMessage);

            CommandResult result = new CommandResult(Core.Constant.Messages.UnknownError, CommandResultType.TextMessage);

            BaseUserState postProcessing = CreateUserState(userId);

            if (Processings.TryAdd(userId, postProcessing))
            {
                result = await ExecuteStage(userId);
            }

            return result;
        }

        protected BaseUserState GetProcessing(long id)
        {
            return Processings.TryGetValue(id, out BaseUserState postProcessing) ? postProcessing : null;
        }

        public bool IsExecuting(long id)
        {
            var processing = GetProcessing(id);
            return processing != null && !processing.IsFinished;
        }

        public bool StopProcessing(long id)
        {
            BaseUserState postProcessing = GetProcessing(id);
            if (postProcessing == null)
                return true;

            return Processings.TryRemove(id, out _);
        }

        public abstract Task<CommandResult> ExecuteStage(long id, Message message = null);

        protected abstract BaseUserState CreateUserState(long userId);
    }
}
