using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Altayskaya97.Service.Interface;

namespace Telegram.Altayskaya97.Bot.Jobs
{
    [DisallowConcurrentExecution]
    public class NoWalkJob : IJob
    {
        private readonly ILogger<NoWalkJob> _logger;
        private readonly IUserService _userService;

        public NoWalkJob(ILogger<NoWalkJob> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        public async Task Execute(IJobExecutionContext context)
        { 
            var userList = await _userService.GetList();
            foreach (var user in userList)
            {
                if (user.NoWalk.HasValue && user.NoWalk.Value)
                {
                    user.NoWalk = false;
                    await _userService.Update(user);
                    _logger.LogInformation($"User '{user.Name}' hasn't 'No walk' status yet");
                }
            }
            _logger.LogInformation("Nowalk for next day reset");
        }
    }
}
