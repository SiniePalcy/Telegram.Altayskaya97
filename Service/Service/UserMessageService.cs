using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<UserMessage> Get(long chatId, long telegramId)
        {
            var list = await GetList();
            return list.FirstOrDefault(el => el.ChatId == chatId && 
                el.TelegramId == telegramId);
        }

        public async Task Pin(long id)
        {
            var msg = await Get(id);
            if (msg != null)
            {
                msg.Pinned = true;
                await Update(msg);
            }
        }

        public async Task UnPin(long id)
        {
            var msg = await Get(id);
            if (msg != null)
            {
                msg.Pinned = false;
                await Update(msg);
            }
        }

        public override async Task Update(long id, UserMessage updatedItem)
        {
            await _repo.Update(id, updatedItem);
            _logger.LogInformation($"Updated message from {updatedItem.UserId} in chat {updatedItem.ChatId}'");
        }
    }
}
