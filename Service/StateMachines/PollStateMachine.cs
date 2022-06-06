using Telegram.Altayskaya97.Service.Interface;
using System.Threading.Tasks;
using Telegram.Altayskaya97.Core.Enum;
using Telegram.Altayskaya97.Core.Constant;
using System.Collections.Generic;
using System.Linq;
using Telegram.BotAPI.AvailableTypes;
using Telegram.Altayskaya97.Service.StateMachines.UserStates;
using Telegram.Altayskaya97.Core.Model;

namespace Telegram.Altayskaya97.Service.StateMachines
{
    public class PollStateMachine : BaseStateMachine<PollUserState, PollState>
    {
        private IChatService ChatService { get; }

        public PollStateMachine(IChatService chatService)
        {
            this.ChatService = chatService;
        }

        public override async Task<CommandResult> ExecuteStage(long id, Message message = null)
        {
            if (!(GetUserStateFlow(id) is PollUserState userState))
                return new CommandResult(Core.Constant.Messages.UnknownError, CommandResultType.TextMessage);

            if (userState.CurrentState != PollState.AddCase)
                userState.ExecuteNextStage();

            return userState.CurrentState switch
            {
                PollState.Start => await StartState(),
                PollState.ChatChoice => await ChatChoiceState(id, message.Text),
                PollState.AddQuestion => AddQuestionState(id, message.Text),
                PollState.AddCase => AddCaseState(id, message.Text),
                PollState.MultiAnswersChoice => MultiAnswersChoiceState(id, message.Text),
                PollState.AnonymousChoice => AnonymousChoiceState(id, message.Text),
                PollState.PinChoice => PinChoiceState(id, message.Text),
                PollState.Confirmation => ConfirmationState(id, message.Text),
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
                var processing = GetUserStateFlow(id);
                processing.ChatId = chat.Id;
                return new CommandResult($"Please, input a question", CommandResultType.TextMessage);
            }
        }

        private CommandResult AddQuestionState(long id, string question)
        {
            if (!(GetUserStateFlow(id) is PollUserState processing))
                return new CommandResult(Messages.UnknownError, CommandResultType.TextMessage);

            processing.Question = question;
            return new CommandResult("Please, input first case", CommandResultType.TextMessage);
        }

        private CommandResult AddCaseState(long id, string nextCase)
        {
            if (!(GetUserStateFlow(id) is PollUserState processing))
                return new CommandResult(Messages.UnknownError, CommandResultType.TextMessage);

            if (nextCase == "/done")
            {
                if (processing.Cases.Count < 2)
                {
                    processing.CurrentState = PollState.Stop;
                    return new CommandResult($"Cancelled: cases must be minimum 2", CommandResultType.TextMessage);
                }
                processing.CurrentState = PollState.FinishCase;
                KeyboardButton[] pinButtons = new KeyboardButton[]
                {
                        new KeyboardButton(Messages.Yes),
                        new KeyboardButton(Messages.No),
                        new KeyboardButton(Messages.Cancel)
                };
                return new CommandResult("Is the pool with multiple answers?", CommandResultType.TextMessage,
                    new ReplyKeyboardMarkup(pinButtons));
            }
            
            processing.Cases.Add(nextCase);
            return new CommandResult("Please, input next case or <code>/done</code> for stop", CommandResultType.TextMessage);
        }

        private CommandResult MultiAnswersChoiceState(long id, string text)
        {
            if (!(GetUserStateFlow(id) is PollUserState processing))
                return new CommandResult(Messages.UnknownError, CommandResultType.TextMessage);

            if (text == Messages.Yes || text == Messages.No)
            {
                processing.IsMultiAnswers = text == Messages.Yes;
                KeyboardButton[] confirmButtons = new KeyboardButton[]
                {
                        new KeyboardButton(Messages.Yes),
                        new KeyboardButton(Messages.No),
                        new KeyboardButton(Messages.Cancel)
                };
                return new CommandResult("Is the pool anonymous?", CommandResultType.TextMessage, new ReplyKeyboardMarkup(confirmButtons));
            }
            else
            {
                StopUserStateFlow(id);
                return new CommandResult(Messages.Cancelled, CommandResultType.TextMessage);
            }
        }

        private CommandResult AnonymousChoiceState(long id, string text)
        {
            if (!(GetUserStateFlow(id) is PollUserState processing))
                return new CommandResult(Messages.UnknownError, CommandResultType.TextMessage);

            if (text == Messages.Yes || text == Messages.No)
            {
                processing.IsAnonymous = text == Messages.Yes;
                KeyboardButton[] confirmButtons = new KeyboardButton[]
                {
                        new KeyboardButton(Messages.Yes),
                        new KeyboardButton(Messages.No),
                        new KeyboardButton(Messages.Cancel)
                };
                return new CommandResult("Pin the pool?", CommandResultType.TextMessage, new ReplyKeyboardMarkup(confirmButtons));
            }
            else
            {
                StopUserStateFlow(id);
                return new CommandResult(Messages.Cancelled, CommandResultType.TextMessage);
            }
        }

        private CommandResult PinChoiceState(long id, string text)
        {
            if (!(GetUserStateFlow(id) is PollUserState processing))
                return new CommandResult(Core.Constant.Messages.UnknownError, CommandResultType.TextMessage);

            if (text == Messages.Yes || text == Messages.No)
            {
                processing.IsPin = text == Messages.Yes;
                KeyboardButton[] confirmButtons = new KeyboardButton[]
                {
                            new KeyboardButton(Messages.OK),
                            new KeyboardButton(Messages.Cancel)
                };
                return new CommandResult("Confirm sending pool?", CommandResultType.TextMessage, new ReplyKeyboardMarkup(confirmButtons));
            }
            else
            {
                StopUserStateFlow(id);
                return new CommandResult(Messages.Cancelled, CommandResultType.TextMessage);
            }
        }

        private CommandResult ConfirmationState(long id, string messageText)
        {
            if (!(GetUserStateFlow(id) is PollUserState processing))
                return new CommandResult(Messages.UnknownError, CommandResultType.TextMessage);

            CommandResult commandResult;

            if (messageText == Messages.OK)
                commandResult = new CommandResult(processing.Question, CommandResultType.Pool)
                {
                    Recievers = new long[] { processing.ChatId },
                    Properties = new Dictionary<string, object>
                    {
                        { "IsMultiAnswers", processing.IsMultiAnswers },
                        { "IsAnonymous", processing.IsAnonymous },
                        { "IsPin", processing.IsPin },
                        { "Cases", processing.Cases }
                    }
                };
            else
                commandResult = new CommandResult(Messages.Cancelled, CommandResultType.TextMessage);

            StopUserStateFlow(id);

            return commandResult;
        }
    }
}
