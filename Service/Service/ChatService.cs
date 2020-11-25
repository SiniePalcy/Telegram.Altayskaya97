using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Altayskaya97.Core.Model;
using Telegram.Altayskaya97.Model.Interface;
using Telegram.Altayskaya97.Service.Interface;

namespace Telegram.Altayskaya97.Service
{
    public class ChatService : RepositoryService<Chat>, IChatService
    {
        private readonly ILogger<ChatService> _logger;

        public ChatService(IDbContext dbContext, ILogger<ChatService> logger)
        {
            _repo = dbContext.ChatRepository;
            _logger = logger;
        }

        public override async Task Add(Chat chat)
        {
            await base.Add(chat);
            _logger.LogInformation($"Chat {chat.Title} has been added");
        }
        
        public override async Task<Chat> Delete(long chatId)
        {
            var chat = await base.Delete(chatId);
            if (chat != null)
                _logger.LogInformation($"Chat {chat.Title} has been deleted");
            return chat;
            
        }

        public async Task<Chat> Get(string name)
        {
            var chatList = await GetList();
            return chatList.FirstOrDefault(c => c.Title.Trim().ToLower() == name.Trim().ToLower());
        }

        public override async Task Update(long id, Chat updatedItem)
        {
            await base.Update(id, updatedItem);
            _logger.LogInformation($"Chat was updated: {updatedItem}");
        }
    }
}
