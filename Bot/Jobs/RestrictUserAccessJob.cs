using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.SafeBot.Core.Model;
using Telegram.SafeBot.Model;
using Telegram.SafeBot.Service.Interface;

namespace Telegram.SafeBot.Bot.Jobs
{
    [DisallowConcurrentExecution]
    public class RestrictUserAccessJob : IJob
    {
        private readonly ILogger<RestrictUserAccessJob> _logger;
        private readonly IUserService _userService;
        private readonly IUserMessageService _userMessageService;
        private Configuration _configuration;

        public RestrictUserAccessJob(
            ILogger<RestrictUserAccessJob> logger, 
            IUserService userService,
            IUserMessageService userMessageService,
            IConfiguration configuration)
        {
            _logger = logger;
            _userService = userService;
            _userMessageService = userMessageService;
            _configuration = configuration.Get<Configuration>();
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var userList = await _userService.GetList();
            foreach (var user in userList.Where(x => x.IsAdmin))
            {
                var chatMessages = await _userMessageService.GetList();
                var now = DateTime.UtcNow;
                var lastChatMessage = (await _userMessageService.GetList())
                    .Where(x => x.UserId == user.Id && x.ChatType == ChatType.Private)
                    .OrderByDescending(x => x.When)
                    .FirstOrDefault();
                if (lastChatMessage == null)
                {
                    continue;
                }

                var passedFromLastMsg = now.Subtract(lastChatMessage.When).TotalMinutes;
                if (passedFromLastMsg >= _configuration.PeriodResetAccessMin)
                {
                    await _userService.RestrictUser(user.Id);
                    _logger.LogInformation($"User '{user.Name}' was restricted");
                }
            }
        }
    }
}
