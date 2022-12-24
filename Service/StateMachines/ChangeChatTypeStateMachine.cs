using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.SafeBot.Core.Enum;
using Telegram.SafeBot.Core.Constant;
using Telegram.SafeBot.Service.Interface;
using Telegram.BotAPI.AvailableTypes;
using Telegram.SafeBot.Service.StateMachines.UserStates;
using Telegram.SafeBot.Core.Model;

namespace Telegram.SafeBot.Service.StateMachines
{
    public class ChangeChatTypeStateMachine : BaseStateMachine<ChangeChatTypeUserState, ChangeChatTypeState>
    {
        private IChatService ChatService { get; }

        public ChangeChatTypeStateMachine(IChatService chatService)
        {
            this.ChatService = chatService;
        }

        public override async Task<CommandResult> ExecuteStage(long id, Message message = null)
        {
            if (!(GetUserStateFlow(id) is ChangeChatTypeUserState userState))
                return new CommandResult(Messages.UnknownError, CommandResultType.TextMessage);

            userState.ExecuteNextStage();

            return userState.CurrentState switch
            {
                ChangeChatTypeState.Start => await StartState(),
                ChangeChatTypeState.ChatChoice => await ChatChoiceState(id, message.Text),
                ChangeChatTypeState.Confirmation => ConfirmationState(id, message.Text),
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
            var newChatState = chat.ChatType == Core.Model.ChatType.Admin ?
                        Core.Model.ChatType.Public :
                        Core.Model.ChatType.Admin;
            userState.ChatType = newChatState;

            return new CommandResult($"Confirm changing to <b>{newChatState}</b>?", 
                CommandResultType.TextMessage,
                new ReplyKeyboardMarkup(confirmButtons));
        }

        private CommandResult ConfirmationState(long userId, string messageText)
        {
            if (!(GetUserStateFlow(userId) is ChangeChatTypeUserState userState))
                return new CommandResult(Messages.UnknownError, CommandResultType.TextMessage);


            CommandResult commandResult = messageText == Messages.OK ?
                new CommandResult("Changed successfully", CommandResultType.ChangeChatType)
                {
                    Properties = new Dictionary<string, object>
                    { 
                        { "ChatId", userState.ChatId},
                        { "ChatType", userState.ChatType},
                    }
                } :
                new CommandResult(Messages.Cancelled, CommandResultType.TextMessage);

            StopUserStateFlow(userId);

            return commandResult;
        }
    }
}
