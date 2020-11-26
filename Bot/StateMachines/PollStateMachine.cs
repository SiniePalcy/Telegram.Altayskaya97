using System.Linq;
using Telegram.Altayskaya97.Bot.Model;
using Telegram.Altayskaya97.Service.Interface;
using Telegram.Bot.Types.ReplyMarkups;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Altayskaya97.Bot.Enum;
using Telegram.Altayskaya97.Bot.StateMachines.UserStates;

namespace Telegram.Altayskaya97.Bot.StateMachines
{
    public class PollStateMachine : BaseStateMachine<PollState>
    {
        public PollStateMachine(IChatService chatService) : base(chatService) { }

        public override async Task<CommandResult> ExecuteStage(long id, Message message = null)
        {
            if (!(GetProcessing(id) is PollUserState processing))
                return new CommandResult(Core.Constant.Messages.UnknownError, CommandResultType.TextMessage);

            if (processing.CurrentState != PollState.AddCase)
                processing.ExecuteNextStage();

            CommandResult commandResult = null;
            switch (processing.CurrentState)
            {
                case PollState.Start:
                    commandResult = await StartState();
                    break;
                case PollState.ChatChoice:
                    commandResult = await ChatChoiceState(id, message.Text);
                    break;
                case PollState.AddQuestion:
                    commandResult = AddQuestionState(id, message.Text);
                    break;
                case PollState.AddCase:
                    commandResult = AddCaseState(id, message.Text);
                    break;
                case PollState.MultiAnswersChoice:
                    commandResult = MultiAnswersChoiceState(id, message.Text);
                    break;
                case PollState.AnonymousChoice:
                    commandResult = AnonymousChoiceState(id, message.Text);
                    break;
                case PollState.PinChoice:
                    commandResult = PinChoiceState(id, message.Text);
                    break;
                case PollState.Confirmation:
                    commandResult = ConfirmationState(id, message.Text);
                    break;
            }
            return commandResult;
        }

        protected override BaseUserState<PollState> CreateUserState(long userId) => new PollUserState();

        private async Task<CommandResult> StartState()
        {
            var chats = await ChatService.GetList();
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
            var chat = await ChatService.Get(chatTitle);
            if (chat == null)
            {
                StopProcessing(id);
                return new CommandResult($"Cancelled", CommandResultType.TextMessage);
            }
            else
            {
                var processing = GetProcessing(id);
                processing.ChatId = chat.Id;
                return new CommandResult($"Please, input a question", CommandResultType.TextMessage);
            }
        }

        private CommandResult AddQuestionState(long id, string question)
        {
            if (!(GetProcessing(id) is PollUserState processing))
                return new CommandResult(Core.Constant.Messages.UnknownError, CommandResultType.TextMessage);

            processing.Question = question;
            return new CommandResult("Please, input first case", CommandResultType.TextMessage);
        }

        private CommandResult AddCaseState(long id, string nextCase)
        {
            if (!(GetProcessing(id) is PollUserState processing))
                return new CommandResult(Core.Constant.Messages.UnknownError, CommandResultType.TextMessage);

            if (nextCase == "/done")
            {
                processing.CurrentState = PollState.FinishCase;
                KeyboardButton[] pinButtons = new KeyboardButton[]
                {
                        new KeyboardButton("Yes"),
                        new KeyboardButton("No"),
                        new KeyboardButton("Cancel")
                };
                return new CommandResult("Is the pool with multiple answers?", CommandResultType.KeyboardButtons, new ReplyKeyboardMarkup(pinButtons, true, true))
                {
                    KeyboardButtons = pinButtons
                };
            }
            
            processing.Cases.Add(nextCase);
            return new CommandResult("Please, input next case or <code>/done</code> for stop", CommandResultType.TextMessage);
        }

        private CommandResult MultiAnswersChoiceState(long id, string text)
        {
            if (!(GetProcessing(id) is PollUserState processing))
                return new CommandResult(Core.Constant.Messages.UnknownError, CommandResultType.TextMessage);

            if (text == "Yes" || text == "No")
            {
                processing.IsMultiAnswers = text == "Yes";
                KeyboardButton[] confirmButtons = new KeyboardButton[]
                {
                        new KeyboardButton("Yes"),
                        new KeyboardButton("No"),
                        new KeyboardButton("Cancel")
                };
                return new CommandResult("Is the pool anonymous?", CommandResultType.KeyboardButtons, new ReplyKeyboardMarkup(confirmButtons, true, true));
            }
            else
            {
                StopProcessing(id);
                return new CommandResult("Cancelled", CommandResultType.TextMessage);
            }
        }

        private CommandResult AnonymousChoiceState(long id, string text)
        {
            if (!(GetProcessing(id) is PollUserState processing))
                return new CommandResult(Core.Constant.Messages.UnknownError, CommandResultType.TextMessage);

            if (text == "Yes" || text == "No")
            {
                processing.IsAnonymous = text == "Yes";
                KeyboardButton[] confirmButtons = new KeyboardButton[]
                {
                        new KeyboardButton("Yes"),
                        new KeyboardButton("No"),
                        new KeyboardButton("Cancel")
                };
                return new CommandResult("Pin the pool?", CommandResultType.KeyboardButtons, new ReplyKeyboardMarkup(confirmButtons, true, true));
            }
            else
            {
                StopProcessing(id);
                return new CommandResult("Cancelled", CommandResultType.TextMessage);
            }
        }

        private CommandResult PinChoiceState(long id, string text)
        {
            if (!(GetProcessing(id) is PollUserState processing))
                return new CommandResult(Core.Constant.Messages.UnknownError, CommandResultType.TextMessage);

            if (text == "Yes" || text == "No")
            {
                processing.IsPin = text == "Yes";
                KeyboardButton[] confirmButtons = new KeyboardButton[]
                {
                            new KeyboardButton("OK"),
                            new KeyboardButton("Cancel")
                };
                return new CommandResult("Confirm sending pool?", CommandResultType.KeyboardButtons, new ReplyKeyboardMarkup(confirmButtons, true, true));
            }
            else
            {
                StopProcessing(id);
                return new CommandResult("Cancelled", CommandResultType.TextMessage);
            }
        }

        private CommandResult ConfirmationState(long id, string messageText)
        {
            if (!(GetProcessing(id) is PollUserState processing))
                return new CommandResult(Core.Constant.Messages.UnknownError, CommandResultType.TextMessage);

            CommandResult commandResult;

            if (messageText == "OK")
                commandResult = new CommandResult(processing.Question, CommandResultType.Pool)
                {
                    Recievers = new long[] { processing.ChatId },
                    IsMultiAnswers = processing.IsMultiAnswers,
                    IsAnonymous = processing.IsAnonymous,
                    IsPin = processing.IsPin,
                    Cases = processing.Cases
                };
            else
                commandResult = new CommandResult("Cancelled", CommandResultType.TextMessage);

            StopProcessing(id);

            return commandResult;
        }
    }
}
