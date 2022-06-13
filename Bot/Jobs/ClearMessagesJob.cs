using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Altayskaya97.Core.Model;
using Telegram.Altayskaya97.Model;
using Telegram.Altayskaya97.Service.Interface;

namespace Telegram.Altayskaya97.Bot.Jobs
{
    [DisallowConcurrentExecution]
    public class ClearMessagesJob : IJob
    {
        private readonly ILogger<ClearMessagesJob> _logger;
        private readonly IUserService _userService;
        private readonly IUserMessageService _userMessageService;
        private readonly IDateTimeService _dateTimeService;
        private Configuration _configuration;

        public ClearMessagesJob(
            ILogger<ClearMessagesJob> logger, 
            IUserService userService,
            IUserMessageService userMessageService,
            IConfiguration configuration,
            IDateTimeService dateTimeService)
        {
            _logger = logger;
            _userService = userService;
            _userMessageService = userMessageService;
            _dateTimeService = dateTimeService;
            _configuration = configuration.Get<Configuration>();
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var allMessages = await _userMessageService.GetList();

            var dtNow = _dateTimeService.GetDateTimeUTCNow();
            List<UserMessage> messagesForDelete = new List<UserMessage>();
            foreach (var message in allMessages)
            {
                var msgDateTime = message.When.ToUniversalTime();
                var timePassed = dtNow - msgDateTime;
                var minutePassed = timePassed.TotalMinutes;
                if ((string.IsNullOrEmpty(message.ChatType) || message.ChatType == Core.Model.ChatType.Private)
                    && minutePassed >= _configuration.PeriodClearPrivateChatMin)
                    messagesForDelete.Add(message);
            }

            foreach (var message in messagesForDelete)
            {
                try
                {
                    await _userMessageService.Delete(message.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Can't delete message from user id='{message.UserId}': {ex.Message}");
                }
            }
        }
    }
}
