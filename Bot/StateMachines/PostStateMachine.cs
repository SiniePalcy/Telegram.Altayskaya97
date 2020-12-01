using Telegram.Altayskaya97.Bot.Model;
using Telegram.Altayskaya97.Service.Interface;
using Telegram.Bot.Types.ReplyMarkups;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Altayskaya97.Bot.Enum;
using Telegram.Altayskaya97.Bot.StateMachines.UserStates;
using Telegram.Altayskaya97.Core.Constant;
using System.Collections.Generic;

namespace Telegram.Altayskaya97.Bot.StateMachines
{
    public class PostStateMachine : BaseStateMachine<PostState>
    {
        public PostStateMachine(IChatService chatService) : base(chatService)  {}

        public async override Task<CommandResult> ExecuteStage(long id, Message message = null)
        {
            if (!(GetProcessing(id) is PostUserState postProcessing))
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

        protected override BaseUserState<PostState> CreateUserState(long userId) => new PostUserState();

        private async Task<CommandResult> ChatChoiceState(long id, string chatTitle)
        {
            var chat = await ChatService.Get(chatTitle);
            if (chat == null)
            {
                StopProcessing(id);
                return new CommandResult(Messages.Cancelled, CommandResultType.TextMessage);
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
            if (!(GetProcessing(id) is PostUserState postProcessing))
                return new CommandResult(Core.Constant.Messages.UnknownError, CommandResultType.TextMessage);

            postProcessing.Message = message;

            KeyboardButton[] pinButtons = new KeyboardButton[]
            {
                        new KeyboardButton(Messages.Yes),
                        new KeyboardButton(Messages.No),
                        new KeyboardButton(Messages.Cancel)
            };
            return new CommandResult("Pin a message?", CommandResultType.TextMessage,
                new ReplyKeyboardMarkup(pinButtons, true, true));
        }

        private CommandResult PinChoiceState(long id, string text)
        {
            if (!(GetProcessing(id) is PostUserState processing))
                return new CommandResult(Core.Constant.Messages.UnknownError, CommandResultType.TextMessage);

            if (text == Messages.Yes || text == Messages.No)
            {
                processing.IsPin = text == Messages.Yes;
                KeyboardButton[] confirmButtons = new KeyboardButton[]
                {
                            new KeyboardButton(Messages.OK),
                            new KeyboardButton(Messages.Cancel)
                };
                return new CommandResult("Confirm sending?", CommandResultType.TextMessage, new ReplyKeyboardMarkup(confirmButtons, true, true));
            }
            else
            {
                StopProcessing(id);
                return new CommandResult(Messages.Cancelled, CommandResultType.TextMessage);
            }
        }

        private CommandResult ConfirmationState(long id, string messageText)
        {
            if (!(GetProcessing(id) is PostUserState postProcessing))
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

            StopProcessing(id);

            return commandResult;
        }
    }
}
