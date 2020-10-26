using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Altayskaya97.Core.Model;
using Telegram.Altayskaya97.Model.Interface;
using Telegram.Altayskaya97.Service.Interface;
namespace Telegram.Altayskaya97.Service
{
    public class UserMessageService : IUserMessageService
    {
        private readonly IRepository<UserMessage> _repo;
        private readonly ILogger<UserMessageService> _logger;

        public UserMessageService(IDbContext dbContext, ILogger<UserMessageService> logger)
        {
            _repo = dbContext.UserMessageRepository;
            _logger = logger;
        }

        public async Task AddUserMessage(UserMessage userMessage)
        {
            await _repo.AddItem(userMessage);
            _logger.LogInformation($"Added message from {userMessage.UserId} in chat {userMessage.ChatId}'");
        }

        public async Task DeleteUserMessage(long id)
        {
            var userMessage = await GetUserMessage(id);
            if (userMessage == null)
                return;

            await _repo.RemoveItem(id);
        }

        public async Task<UserMessage> GetUserMessage(long id)
        {
            return await _repo.GetItem(id);
        }

        public async Task<ICollection<UserMessage>> GetUserMessageList()
        {
            return await _repo.GetCollection();
        }
    }
}
