using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Altayskaya97.Core.Model;
using Telegram.Altayskaya97.Model.Interface;
using Telegram.Altayskaya97.Service.Interface;
namespace Telegram.Altayskaya97.Service
{
    public class UserMessageService : RepositoryService<UserMessage>, IUserMessageService
    {
        private readonly ILogger<UserMessageService> _logger;

        public UserMessageService(IDbContext dbContext, ILogger<UserMessageService> logger)
        {
            _repo = dbContext.UserMessageRepository;
            _logger = logger;
        }

        public override async Task Add(UserMessage userMessage)
        {
            await base.Add(userMessage);
            _logger.LogInformation($"Added message from {userMessage.UserId} in chat {userMessage.ChatId}'");
        }

        public override async Task Update(long id, UserMessage updatedItem)
        {
            await _repo.Update(id, updatedItem);
            _logger.LogInformation($"Updated message from {updatedItem.UserId} in chat {updatedItem.ChatId}'");
        }
    }
}
