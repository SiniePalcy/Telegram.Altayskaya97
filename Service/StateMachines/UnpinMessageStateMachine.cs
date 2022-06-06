using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Altayskaya97.Core.Enum;
using Telegram.Altayskaya97.Core.Constant;
using Telegram.Altayskaya97.Service.Interface;
using Telegram.BotAPI.AvailableTypes;
using Telegram.Altayskaya97.Service.StateMachines.UserStates;
using Telegram.Altayskaya97.Core.Model;

namespace Telegram.Altayskaya97.Service.StateMachines
{
    public class UnpinMessageStateMachine : 
        BaseStateMachine<UnpinMessageUserState, UnpinMessageState>
    {
        private IChatService ChatService { get; }
        private IUserMessageService UserMessageService { get; }

        public UnpinMessageStateMachine(IChatService chatService, IUserMessageService userMessageService)
        {
            this.ChatService = chatService;
            this.UserMessageService = userMessageService;
        }

        public override async Task<CommandResult> ExecuteStage(long id, Message message = null)
        {
            if (!(GetUserStateFlow(id) is UnpinMessageUserState userState))
                return new CommandResult(Messages.UnknownError, CommandResultType.TextMessage);

            userState.ExecuteNextStage();

            return userState.CurrentState switch
            {
                UnpinMessageState.Start => await StartState(),
                UnpinMessageState.ChatChoice => await ChatChoiceState(id, message.Text),
                UnpinMessageState.MessageChoice => await MessageChoiceState(id, message.Text),
                UnpinMessageState.Confirmation => ConfirmationState(id, message.Text),
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

            var allMessages = await UserMessageService.GetList();
            var buttonsList = allMessages.Where(m => m.Pinned != null && 
                    m.Pinned.HasValue && m.Pinned.Value && m.ChatId == chat.Id).
                    Select(b => b.Text.Substring(0, b.Text.Length < 124 ? b.Text.Length : 124))
                    .ToList();
            if (!buttonsList.Any())
            {
                StopUserStateFlow(userId);
                return new CommandResult($"No pinned messages for chat <b>{chatTitle}</b>", 
                    CommandResultType.TextMessage);
            }

            buttonsList.Add(Messages.Cancel);
            var buttonsMarkupList = buttonsList.Select(b => new KeyboardButton[1] { new KeyboardButton(b) });

            return new CommandResult($"Please, select a message to unpin", 
                CommandResultType.TextMessage,
                new ReplyKeyboardMarkup(buttonsMarkupList));
        }

        private async Task<CommandResult> MessageChoiceState(long userId, string messageTitle)
        {
            var userState = GetUserStateFlow(userId);

            var allMessages = await UserMessageService.GetList();
            var msg = allMessages.FirstOrDefault(m => m.ChatId == userState.ChatId && m.Pinned != null &&
                    m.Pinned.HasValue && m.Pinned.Value && m.Text.Contains(messageTitle));
            if (msg == null)
            {
                StopUserStateFlow(userId);
                return new CommandResult(Messages.Cancelled, CommandResultType.TextMessage);
            }

            userState.MessageId = msg.Id;

            KeyboardButton[] confirmButtons = new KeyboardButton[]
            {
                            new KeyboardButton(Messages.OK),
                            new KeyboardButton(Messages.Cancel)
            };

            return new CommandResult($"Confirm unpin messages?",
                CommandResultType.TextMessage,
                new ReplyKeyboardMarkup(confirmButtons));
        }


        private CommandResult ConfirmationState(long userId, string messageText)
        {
            if (!(GetUserStateFlow(userId) is UnpinMessageUserState userState))
                return new CommandResult(Messages.UnknownError, CommandResultType.TextMessage);


            CommandResult commandResult = messageText == Messages.OK ?
                new CommandResult("Unpinned successfully", CommandResultType.Unpin)
                {
                    Properties = new Dictionary<string, object>
                    {
                        { "ChatId", userState.ChatId},
                        { "MessageId", userState.MessageId}
                    }
                } :
                new CommandResult(Messages.Cancelled, CommandResultType.TextMessage);

            StopUserStateFlow(userId);

            return commandResult;
        }
    }
}
