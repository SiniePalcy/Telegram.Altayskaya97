using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Altayskaya97.Core.Model;
using Telegram.Altayskaya97.Model.Interface;
using Telegram.Altayskaya97.Service.Interface;

namespace Telegram.Altayskaya97.Service
{
    public class ChatService : RepositoryService<Chat>, IChatService
    {
        public ChatService(ILogger<ChatService> logger, IRepository<Chat> repository) : base(logger, repository)
        {
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
