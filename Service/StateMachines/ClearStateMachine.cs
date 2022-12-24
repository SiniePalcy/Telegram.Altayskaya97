using System.Linq;
using System.Threading.Tasks;
using Telegram.SafeBot.Core.Enum;
using Telegram.SafeBot.Core.Constant;
using Telegram.SafeBot.Service.Interface;
using Telegram.BotAPI.AvailableTypes;
using Telegram.SafeBot.Service.StateMachines.UserStates;

namespace Telegram.SafeBot.Service.StateMachines
{
    public class ClearStateMachine : BaseStateMachine<ClearUserState, ClearState>
    {
        private IChatService ChatService { get; }

        public ClearStateMachine(IChatService chatService) 
        {
            this.ChatService = chatService;
        }
        
        public override async Task<CommandResult> ExecuteStage(long id, Message message = null)
        {
            if (!(GetUserStateFlow(id) is ClearUserState userState))
                return new CommandResult(Messages.UnknownError, CommandResultType.TextMessage);

            userState.ExecuteNextStage();

            return userState.CurrentState switch
            {
                ClearState.Start => await StartState(),
                ClearState.ChatChoice => await ChatChoiceState(id, message.Text),
                ClearState.Confirmation => ConfirmationState(id, message.Text),
                _ => default
            };
        }

        protected async Task<CommandResult> StartState()
        {
            var chats = await ChatService.GetList();
            var buttonsList = chats.Where(c => c.ChatType != Core.Model.ChatType.Private)
                .Select(c => new KeyboardButton(c.Title)).ToList();
            buttonsList.Add(new KeyboardButton(Messages.Cancel));

            var buttonsReplyList = buttonsList.Select(b => new KeyboardButton[1] { b });
            return new CommandResult(Messages.SelectChat, CommandResultType.TextMessage,
                new ReplyKeyboardMarkup(buttonsReplyList));
        }

        private async Task<CommandResult> ChatChoiceState(long userId, string chatTitle)
        {
            var chat = await ChatService.Get(chatTitle);
            if (chat == null)
            {
                StopUserStateFlow(userId);
                return new CommandResult(Messages.Cancelled, CommandResultType.TextMessage);
            }

            var userState = GetUserStateFlow(userId);
            userState.ChatId = chat.Id;

            KeyboardButton[] confirmButtons = new KeyboardButton[]
            {
                            new KeyboardButton(Messages.OK),
                            new KeyboardButton(Messages.Cancel)
            };
            return new CommandResult("Confirm removing?", CommandResultType.TextMessage, 
                new ReplyKeyboardMarkup(confirmButtons));
        }

        private CommandResult ConfirmationState(long userId, string messageText)
        {
            if (!(GetUserStateFlow(userId) is ClearUserState userState))
                return new CommandResult(Messages.UnknownError, CommandResultType.TextMessage);

            CommandResult commandResult = messageText == Messages.OK ?
                new CommandResult("Cleared", CommandResultType.Delete)
                {
                    Recievers = new long[] { userState.ChatId }
                } :
                new CommandResult(Messages.Cancelled, CommandResultType.TextMessage);

            StopUserStateFlow(userId);

            return commandResult;
        }
    }
}
