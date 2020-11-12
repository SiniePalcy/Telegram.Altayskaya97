using System.Linq;
using Telegram.Altayskaya97.Bot.Model;
using Telegram.Altayskaya97.Service.Interface;
using Telegram.Bot.Types.ReplyMarkups;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Altayskaya97.Bot.Enum;

namespace Telegram.Altayskaya97.Bot.StateMachines
{
    public class PostStateMachine : BaseStateMachine
    {
        public PostStateMachine(IChatService chatService) : base(chatService)  {}

        public async override Task<CommandResult> ExecuteStage(long id, Message message = null)
        {
            PostUserState postProcessing = GetProcessing(id) as PostUserState;
            if (postProcessing == null)
                return new CommandResult(Core.Constant.Messages.UnknownError, CommandResultType.TextMessage);

            postProcessing.ExecuteNextStage();

            CommandResult commandResult = null;
            switch (postProcessing.CurrentState)
            {
                case PostState.Start:
                    commandResult = await StartState();
                    break;
                case PostState.ChatChoice:
                    commandResult = await ChatChoiceState(id, message.Text);
                    break;
                case PostState.Message:
                    commandResult = MessageState(id, message);
                    break;
                case PostState.PinChoice:
                    commandResult = PinChoiceState(id, message.Text);
                    break;
                case PostState.Confirmation:
                    commandResult = ConfirmationState(id, message.Text);
                    break;
            }
            return commandResult;
        }

        protected override BaseUserState CreateUserState(long userId) => new PostUserState(userId);

        private async Task<CommandResult> StartState()
        {
            var chats = await ChatService.GetChatList();
            var buttonsList = chats.Where(c => c.ChatType != Core.Model.ChatType.Private)
                .Select(c => new KeyboardButton(c.Title)).ToList();
            buttonsList.Add(new KeyboardButton("Cancel"));

            var buttonsReplyList = buttonsList.Select(b => new KeyboardButton[1] { b });
            return new CommandResult("Please, select a chat", CommandResultType.KeyboardButtons, new ReplyKeyboardMarkup(buttonsReplyList, true, true))
            {
                KeyboardButtons = buttonsList.ToList()
            };
        }

        private async Task<CommandResult> ChatChoiceState(long id, string chatTitle)
        {
            var chat = await ChatService.GetChat(chatTitle);
            if (chat == null)
            {
                StopProcessing(id);
                return new CommandResult($"Cancelled", CommandResultType.TextMessage);
            }
            else
            {
                var postProcessing = GetProcessing(id);
                postProcessing.ChatId = chat.Id;
                return new CommandResult($"Please, input a message", CommandResultType.TextMessage);
            }
        }

        private CommandResult MessageState(long id, Message message)
        {
            PostUserState postProcessing = GetProcessing(id) as PostUserState;
            if (postProcessing == null)
                return new CommandResult(Core.Constant.Messages.UnknownError, CommandResultType.TextMessage);

            postProcessing.Message = message;

            KeyboardButton[] pinButtons = new KeyboardButton[]
            {
                        new KeyboardButton("Yes"),
                        new KeyboardButton("No"),
                        new KeyboardButton("Cancel")
            };
            return new CommandResult("Pin a message?", CommandResultType.KeyboardButtons, new ReplyKeyboardMarkup(pinButtons, true, true))
            {
                KeyboardButtons = pinButtons
            };
        }

        private CommandResult PinChoiceState(long id, string text)
        {
            var postProcessing = GetProcessing(id);

            if (text == "Yes" || text == "No")
            {
                postProcessing.IsPin = text == "Yes";
                KeyboardButton[] confirmButtons = new KeyboardButton[]
                {
                            new KeyboardButton("OK"),
                            new KeyboardButton("Cancel")
                };
                return new CommandResult("Confirm sending?", CommandResultType.KeyboardButtons, new ReplyKeyboardMarkup(confirmButtons, true, true));
            }
            else
            {
                StopProcessing(id);
                return new CommandResult("Cancelled", CommandResultType.TextMessage);
            }
        }

        private CommandResult ConfirmationState(long id, string messageText)
        {
            PostUserState postProcessing = GetProcessing(id) as PostUserState;
            if (postProcessing == null)
                return new CommandResult(Core.Constant.Messages.UnknownError, CommandResultType.TextMessage);

            CommandResult commandResult;

            if (messageText == "OK")
                commandResult = new CommandResult(postProcessing.Message, CommandResultType.Message)
                {
                    Recievers = new long[] { postProcessing.ChatId },
                    IsPin = postProcessing.IsPin
                };
            else
                commandResult = new CommandResult("Cancelled", CommandResultType.TextMessage);

            StopProcessing(id);

            return commandResult;
        }
    }
}
