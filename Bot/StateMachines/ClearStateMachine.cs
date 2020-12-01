using System.Threading.Tasks;
using Telegram.Altayskaya97.Bot.Enum;
using Telegram.Altayskaya97.Bot.Model;
using Telegram.Altayskaya97.Bot.StateMachines.UserStates;
using Telegram.Altayskaya97.Core.Constant;
using Telegram.Altayskaya97.Service.Interface;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Altayskaya97.Bot.StateMachines
{
    public class ClearStateMachine : BaseStateMachine<ClearState>
    {

        public ClearStateMachine(IChatService chatService) : base(chatService) {}

        public override async Task<CommandResult> ExecuteStage(long id, Message message = null)
        {
            if (!(GetProcessing(id) is ClearUserState postProcessing))
                return new CommandResult(Core.Constant.Messages.UnknownError, CommandResultType.TextMessage);

            postProcessing.ExecuteNextStage();

            CommandResult commandResult = null;
            switch (postProcessing.CurrentState)
            {
                case ClearState.Start:
                    commandResult = await StartState();
                    break;
                case ClearState.ChatChoice:
                    commandResult = await ChatChoiceState(id, message.Text);
                    break;
                case ClearState.Confirmation:
                    commandResult = ConfirmationState(id, message.Text);
                    break;
            }
            return commandResult;
        }

        protected override BaseUserState<ClearState> CreateUserState(long userId) => new ClearUserState();


        private async Task<CommandResult> ChatChoiceState(long userId, string chatTitle)
        {
            var chat = await ChatService.Get(chatTitle);
            if (chat == null)
            {
                StopProcessing(userId);
                return new CommandResult(Messages.Cancelled, CommandResultType.TextMessage);
            }

            var postProcessing = GetProcessing(userId);
            postProcessing.ChatId = chat.Id;

            KeyboardButton[] confirmButtons = new KeyboardButton[]
            {
                            new KeyboardButton(Messages.OK),
                            new KeyboardButton(Messages.Cancel)
            };
            return new CommandResult("Confirm removing?", CommandResultType.TextMessage, 
                new ReplyKeyboardMarkup(confirmButtons, true, true));
        }

        private CommandResult ConfirmationState(long userId, string messageText)
        {
            if (!(GetProcessing(userId) is ClearUserState processing))
                return new CommandResult(Core.Constant.Messages.UnknownError, CommandResultType.TextMessage);

            CommandResult commandResult = messageText == Messages.OK ?
                new CommandResult("Cleared", CommandResultType.Delete)
                {
                    Recievers = new long[] { processing.ChatId }
                } :
                new CommandResult(Messages.Cancelled, CommandResultType.TextMessage);

            StopProcessing(userId);

            return commandResult;
        }
    }
}
