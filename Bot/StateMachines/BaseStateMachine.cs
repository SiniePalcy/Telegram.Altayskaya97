using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Altayskaya97.Bot.Interface;
using Telegram.Altayskaya97.Bot.Model;
using Telegram.Altayskaya97.Bot.StateMachines.UserStates;
using Telegram.Altayskaya97.Core.Constant;
using Telegram.Altayskaya97.Service.Interface;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

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

        protected BaseUserState<State> GetProcessing(long userId)
        {
            return Processings.TryGetValue(userId, out BaseUserState<State> postProcessing) ? postProcessing : null;
        }

        public bool IsExecuting(long userId)
        {
            var processing = GetProcessing(userId);
            return processing != null && !processing.IsFinished;
        }

        public bool StopProcessing(long userId)
        {
            BaseUserState<State> postProcessing = GetProcessing(userId);
            if (postProcessing == null)
                return true;

            return Processings.TryRemove(userId, out _);
        }

        public abstract Task<CommandResult> ExecuteStage(long userId, Message message = null);

        protected abstract BaseUserState<State> CreateUserState(long userId);

        protected async Task<CommandResult> StartState()
        {
            var chats = await ChatService.GetList();
            var buttonsList = chats.Where(c => c.ChatType != Core.Model.ChatType.Private)
                .Select(c => new KeyboardButton(c.Title)).ToList();
            buttonsList.Add(new KeyboardButton(Messages.Cancel));

            var buttonsReplyList = buttonsList.Select(b => new KeyboardButton[1] { b });
            return new CommandResult(Messages.SelectChat, CommandResultType.TextMessage,
                new ReplyKeyboardMarkup(buttonsReplyList, true, true));
        }
    }
}
