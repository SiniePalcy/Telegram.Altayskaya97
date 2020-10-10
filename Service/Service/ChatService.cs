using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Altayskaya97.Core.Model;
using Telegram.Altayskaya97.Model.Interface;
using Telegram.Altayskaya97.Service.Interface;

namespace Telegram.Altayskaya97.Service
{
    public class ChatService : IChatService
    {
        private readonly IRepository<Chat> _repo;
        private readonly ILogger<ChatService> _logger;

        public ChatService(IDbContext dbContext, ILogger<ChatService> logger)
        {
            _repo = dbContext.ChatRepository;
            _logger = logger;
        }

        public async Task AddChat(Chat chat)
        {
            await _repo.AddItem(chat);
            _logger.LogInformation($"Chat {chat.Title} has been added");
        }

        public async Task<Chat> GetChat(long id)
        {
            return await _repo.GetItem(id);
        }

        public async Task DeleteChat(long chatId)
        {
            var chat = await GetChat(chatId);
            if (chat == null)
                return;

            await _repo.RemoveItem(chatId);
            _logger.LogInformation($"Chat {chat.Title} has been deleted");
        }

        public async Task<ICollection<Chat>> GetChatList()
        {
            return await _repo.GetCollection();
        }
    }
}
