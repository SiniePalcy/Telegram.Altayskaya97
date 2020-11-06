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
        private readonly ConcurrentDictionary<long, PostProcessing> _postProcessings = new ConcurrentDictionary<long, PostProcessing>();

        public PostStateMachine(IChatService chatService)
        {
            _chatService = chatService;
        }
        

        public PostProcessing GetPostProcessing(long id)
        {
            return _postProcessings.TryGetValue(id, out PostProcessing postProcessing) ? postProcessing : null;
        }

        public async Task<CommandResult> CreatePostProcessing(long id)
        {
            if (!StopPostProcessing(id))
                return new CommandResult($"Sorry, something got wrong", CommandResultType.TextMessage);

            CommandResult result = new CommandResult($"Sorry, something got wrong", CommandResultType.TextMessage);
            
            PostProcessing postProcessing = new PostProcessing(id);
            
            if (_postProcessings.TryAdd(id, postProcessing))
            {
                postProcessing.ExecuteNextStage();

                var chats = await _chatService.GetChatList();

                var buttonsList = chats.Where(c => c.ChatType != Core.Model.ChatType.Private)
                    .Select(c => new KeyboardButtonWithId(c.Id, c.Title)).ToList();
                buttonsList.Add(new KeyboardButtonWithId(0, "Cancel"));
                
                var buttonsReplyList = buttonsList.Select(b => new KeyboardButtonWithId[1] { b });

                result = new CommandResult("Please, select a chat", CommandResultType.KeyboardButtons, new ReplyKeyboardMarkup(buttonsReplyList))
                {
                    KeyboardButtons = buttonsList.ToList()
                };
            }

            return result;
        }

        public bool IsPostExecuting(long id)
        {
            PostProcessing postProcessing = GetPostProcessing(id);
            return postProcessing != null && !postProcessing.IsFinished;
        }

        public async Task<CommandResult> ExecuteStage(long id, Message message)
        {
            PostProcessing postProcessing = GetPostProcessing(id);
            if (postProcessing == null)
                return new CommandResult($"Sorry, something got wrong", CommandResultType.TextMessage);

            postProcessing.ExecuteNextStage();

            CommandResult commandResult = null;
            switch(postProcessing.CurrentState)
            {
                case PostState.ChatChoice:
                    var chat = await _chatService.GetChat(message.Text);
                    if (chat == null)
                    {
                        commandResult = new CommandResult($"Cancelled", CommandResultType.TextMessage);
                        StopPostProcessing(id);
                    }
                    else
                        postProcessing.ChatId = chat.Id;
                    break;
                case PostState.Message:
                    postProcessing.Message = message;
                    commandResult = new CommandResult(message, CommandResultType.Message);
                    break;
                case PostState.PinChoice:
                    postProcessing.Message = message;
                    commandResult = new CommandResult(message, CommandResultType.Message);
                    break;

                case PostState.Stop:
                    StopPostProcessing(id);
                    commandResult = new CommandResult($"Complete", CommandResultType.TextMessage);
                    break;


            }
            return commandResult;
        }

        

        public bool StopPostProcessing(long id)
        {
            PostProcessing postProcessing = GetPostProcessing(id);
            if (postProcessing == null)
                return true;

            return _postProcessings.TryRemove(id, out _);
        }

    }
}
