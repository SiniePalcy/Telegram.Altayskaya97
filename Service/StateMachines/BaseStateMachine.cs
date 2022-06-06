using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Telegram.Altayskaya97.Bot.Interface;
using Telegram.Altayskaya97.Bot.Model;
using Telegram.Altayskaya97.Bot.StateMachines.UserStates;
using Telegram.Altayskaya97.Core.Constant;
using Telegram.BotAPI.AvailableTypes;

namespace Telegram.Altayskaya97.Bot.StateMachines
{
    public abstract class BaseStateMachine<TUserState, TState> : IStateMachine 
        where TUserState : UserState<TState>
        where TState: struct
    {
        protected ConcurrentDictionary<long, TUserState> UserStates { get; }

        public BaseStateMachine()
        {
            UserStates = new ConcurrentDictionary<long, TUserState>();
        }


        public async Task<CommandResult> CreateUserStateFlow(long userId)
        {
            if (!StopUserStateFlow(userId))
                return new CommandResult(Messages.UnknownError, CommandResultType.TextMessage);

            CommandResult result = new CommandResult(Messages.UnknownError, CommandResultType.TextMessage);

            var processing = MakeUserStateInstance();

            if (UserStates.TryAdd(userId, processing))
            {
                result = await ExecuteStage(userId);
            }

            return result;
        }

        protected TUserState GetUserStateFlow(long userId)
        {
            return UserStates.TryGetValue(userId, out TUserState userState) ? userState : null;
        }

        public bool IsExecuting(long userId)
        {
            var processing = GetUserStateFlow(userId);
            return processing != null && !processing.IsFinished;
        }

        public bool StopUserStateFlow(long userId)
        {
            TUserState postProcessing = GetUserStateFlow(userId);
            if (postProcessing == null)
                return true;

            return UserStates.TryRemove(userId, out _);
        }

        public abstract Task<CommandResult> ExecuteStage(long userId, Message message = null);

        protected virtual TUserState MakeUserStateInstance()
        {
            var useState = (TUserState)Activator.CreateInstance(typeof(TUserState));
            return useState;
        }
    }
}
