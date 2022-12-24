using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.SafeBot.Service.Extensions;
using Telegram.SafeBot.Service.Interface;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;

namespace Telegram.SafeBot.Bot.Jobs
{
    [DisallowConcurrentExecution]
    public class ActualUsernameJob : IJob
    {
        private readonly ILogger<ActualUsernameJob> _logger;
        private readonly IUserService _userService;
        private readonly IChatService _chatService;
        private readonly BotClient _botApi;

        public ActualUsernameJob(
            ILogger<ActualUsernameJob> logger, 
            IUserService userService,
            IChatService chatService,
            BotClient botApi)
        {
            _logger = logger;
            _userService = userService;
            _chatService = chatService;
            _botApi = botApi;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var userList = await _userService.GetList();
            var chatList = await _chatService.GetList();
            
            foreach (var userRepo in userList)
            {
                User user = null;
                foreach (var chatRepo in chatList)
                {
                    try
                    {
                        var chatMember = await _botApi.GetChatMemberAsync(chatRepo.Id, userRepo.Id);
                        if (chatMember != null)
                        {
                            user = chatMember.User;
                            break;
                        }
                    }
                    catch
                    { }
                }

                if (user == null)
                {
                    _logger.LogWarning($"User '{userRepo.Name}' not found!");
                    continue;
                }

                var userName = user.GetUserName();
                if (userName != userRepo.Name)
                {
                    var oldName = userRepo.Name;
                    userRepo.Name = userName;
                    await _userService.Update(userRepo);
                    _logger.LogInformation($"User name updated from '{oldName}' to '{userName}'");
                }
            }
        }
    }
}