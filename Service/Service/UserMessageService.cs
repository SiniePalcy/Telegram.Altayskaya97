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
        public UserMessageService(ILogger<UserMessageService> logger, IRepository<UserMessage> repository) : base(logger, repository)
        {
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
