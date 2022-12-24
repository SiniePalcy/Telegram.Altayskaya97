using Telegram.SafeBot.Service.Interface;
using System.Threading.Tasks;
using Telegram.SafeBot.Core.Enum;
using Telegram.SafeBot.Core.Constant;
using System.Collections.Generic;
using System.Linq;
using Telegram.BotAPI.AvailableTypes;
using Telegram.SafeBot.Service.StateMachines.UserStates;
using Telegram.SafeBot.Core.Model;

namespace Telegram.SafeBot.Service.StateMachines
{
    public class PostStateMachine : BaseStateMachine<PostUserState, PostState>
    {
        private IChatService ChatService { get; }

        public PostStateMachine(IChatService chatService)
        {
            this.ChatService = chatService;
        }

        public async override Task<CommandResult> ExecuteStage(long id, Message message = null)
        {
            if (!(GetUserStateFlow(id) is PostUserState userState))
                return new CommandResult(Messages.UnknownError, CommandResultType.TextMessage);

            userState.ExecuteNextStage();

            return userState.CurrentState switch
            {
                PostState.Start => await StartState(),
                PostState.ChatChoice => await ChatChoiceState(id, message.Text),
                PostState.Message => MessageState(id, message),
                PostState.PinChoice => PinChoiceState(id, message.Text),
                PostState.Confirmation => ConfirmationState(id, message.Text),
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

        private async Task<CommandResult> ChatChoiceState(long id, string chatTitle)
        {
            var chat = await ChatService.Get(chatTitle);
            if (chat == null)
            {
                StopUserStateFlow(id);
                return new CommandResult(Messages.Cancelled, CommandResultType.TextMessage);
            }
            else
            {
                var postProcessing = GetUserStateFlow(id);
                postProcessing.ChatId = chat.Id;
                return new CommandResult($"Please, input a message", CommandResultType.TextMessage);
            }
        }

        private CommandResult MessageState(long id, Message message)
        {
            if (!(GetUserStateFlow(id) is PostUserState postProcessing))
                return new CommandResult(Core.Constant.Messages.UnknownError, CommandResultType.TextMessage);

            postProcessing.Message = message;

            KeyboardButton[] pinButtons = new KeyboardButton[]
            {
                        new KeyboardButton(Messages.Yes),
                        new KeyboardButton(Messages.No),
                        new KeyboardButton(Messages.Cancel)
            };
            return new CommandResult("Pin a message?", CommandResultType.TextMessage,
                new ReplyKeyboardMarkup(pinButtons));
        }

        private CommandResult PinChoiceState(long id, string text)
        {
            if (!(GetUserStateFlow(id) is PostUserState processing))
                return new CommandResult(Core.Constant.Messages.UnknownError, CommandResultType.TextMessage);

            if (text == Messages.Yes || text == Messages.No)
            {
                processing.IsPin = text == Messages.Yes;
                KeyboardButton[] confirmButtons = new KeyboardButton[]
                {
                            new KeyboardButton(Messages.OK),
                            new KeyboardButton(Messages.Cancel)
                };
                return new CommandResult("Confirm sending?", CommandResultType.TextMessage, new ReplyKeyboardMarkup(confirmButtons));
            }
            else
            {
                StopUserStateFlow(id);
                return new CommandResult(Messages.Cancelled, CommandResultType.TextMessage);
            }
        }

        private CommandResult ConfirmationState(long id, string messageText)
        {
            if (!(GetUserStateFlow(id) is PostUserState postProcessing))
                return new CommandResult(Core.Constant.Messages.UnknownError, CommandResultType.TextMessage);

            CommandResult commandResult;

            if (messageText == Messages.OK)
                commandResult = new CommandResult(postProcessing.Message, CommandResultType.Message)
                {
                    Recievers = new long[] { postProcessing.ChatId },
                    Properties = new Dictionary<string, object> 
                    { { "IsPin", postProcessing.IsPin} }
                };
            else
                commandResult = new CommandResult(Messages.Cancelled, CommandResultType.TextMessage);

            StopUserStateFlow(id);

            return commandResult;
        }
    }
}
