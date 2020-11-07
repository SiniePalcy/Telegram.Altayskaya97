using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using Telegram.Altayskaya97.Bot.Model;
using Telegram.Altayskaya97.Service.Interface;
using Telegram.Bot.Types.ReplyMarkups;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Telegram.Altayskaya97.Bot
{
    public class PostStateMachine
    {
        private readonly IChatService _chatService;
        private readonly ConcurrentDictionary<long, PostUserState> _postProcessings = new ConcurrentDictionary<long, PostUserState>();

        public PostStateMachine(IChatService chatService)
        {
            _chatService = chatService;
        }
        

        public PostUserState GetPostProcessing(long id)
        {
            return _postProcessings.TryGetValue(id, out PostUserState postProcessing) ? postProcessing : null;
        }

        public async Task<CommandResult> CreatePostProcessing(long id)
        {
            if (!StopPostProcessing(id))
                return new CommandResult($"Sorry, something got wrong", CommandResultType.TextMessage);

            CommandResult result = new CommandResult($"Sorry, something got wrong", CommandResultType.TextMessage);
            
            PostUserState postProcessing = new PostUserState(id);
            
            if (_postProcessings.TryAdd(id, postProcessing))
            {
                result = await ExecuteStage(id);
            }

            return result;
        }

        public bool IsPostExecuting(long id)
        {
            PostUserState postProcessing = GetPostProcessing(id);
            return postProcessing != null && !postProcessing.IsFinished;
        }

        public async Task<CommandResult> ExecuteStage(long id, Message message = null)
        {
            PostUserState postProcessing = GetPostProcessing(id);
            if (postProcessing == null)
                return new CommandResult($"Sorry, something got wrong", CommandResultType.TextMessage);

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

        public bool StopPostProcessing(long id)
        {
            PostUserState postProcessing = GetPostProcessing(id);
            if (postProcessing == null)
                return true;

            return _postProcessings.TryRemove(id, out _);
        }

        private async Task<CommandResult> StartState()
        {
            var chats = await _chatService.GetChatList();
            var buttonsList = chats.Where(c => c.ChatType != Core.Model.ChatType.Private)
                .Select(c => new KeyboardButtonWithId(c.Id, c.Title)).ToList();
            buttonsList.Add(new KeyboardButtonWithId(0, "Cancel"));

            var buttonsReplyList = buttonsList.Select(b => new KeyboardButtonWithId[1] { b });
            return new CommandResult("Please, select a chat", CommandResultType.KeyboardButtons, new ReplyKeyboardMarkup(buttonsReplyList, true, true))
            {
                KeyboardButtons = buttonsList.ToList()
            };
        }

        private async Task<CommandResult> ChatChoiceState(long id, string chatTitle)
        {
            var chat = await _chatService.GetChat(chatTitle);
            if (chat == null)
            {
                StopPostProcessing(id);
                return new CommandResult($"Cancelled", CommandResultType.TextMessage);
            }
            else
            {
                var postProcessing = GetPostProcessing(id);
                postProcessing.ChatId = chat.Id;
                return new CommandResult($"Please, input a message", CommandResultType.TextMessage);
            }
        }

        private CommandResult MessageState(long id, Message message)
        {
            var postProcessing = GetPostProcessing(id);
            postProcessing.Message = message;

            KeyboardButtonWithId[] pinButtons = new KeyboardButtonWithId[]
            {
                        new KeyboardButtonWithId(1, "Yes"),
                        new KeyboardButtonWithId(2, "No"),
                        new KeyboardButtonWithId(3, "Cancel")
            };
            return new CommandResult("Pin a message?", CommandResultType.KeyboardButtons, new ReplyKeyboardMarkup(pinButtons, true, true))
            {
                KeyboardButtons = pinButtons
            };
        }

        private CommandResult PinChoiceState(long id, string text)
        {
            var postProcessing = GetPostProcessing(id);

            if (text == "Yes" || text == "No")
            {
                postProcessing.IsPin = text == "Yes";
                KeyboardButtonWithId[] confirmButtons = new KeyboardButtonWithId[]
                {
                            new KeyboardButtonWithId(1, "OK"),
                            new KeyboardButtonWithId(2, "Cancel")
                };
                return new CommandResult("Confirm sending?", CommandResultType.KeyboardButtons, new ReplyKeyboardMarkup(confirmButtons, true, true));
            }
            else
            {
                StopPostProcessing(id);
                return new CommandResult("Cancelled", CommandResultType.TextMessage);
            }
        }

        private CommandResult ConfirmationState(long id, string messageText)
        {
            var postProcessing = GetPostProcessing(id);

            CommandResult commandResult;

            if (messageText == "OK")
                commandResult = new CommandResult(postProcessing.Message, CommandResultType.Message)
                {
                    Recievers = new long[] { postProcessing.ChatId },
                    IsPin = postProcessing.IsPin
                };
            else
                commandResult = new CommandResult("Cancelled", CommandResultType.TextMessage);

            StopPostProcessing(id);

            return commandResult;
        }
    }
}
