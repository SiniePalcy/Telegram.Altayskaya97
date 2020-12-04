using Microsoft.Extensions.Logging;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Altayskaya97.Core.Model;
using Telegram.Altayskaya97.Model.Interface;
using Telegram.Altayskaya97.Service.Interface;
using Telegram.Altayskaya97.Core.Helpers;
using Telegram.Altayskaya97.Core.Extensions;

namespace Telegram.Altayskaya97.Service
{
    public class PasswordService : RepositoryService<Password>, IPasswordService
    {
        private readonly ILogger<PasswordService> _logger;

        public PasswordService(IDbContext dbContext, ILogger<PasswordService> logger)
        {
            _repo = dbContext.PasswordRepository;
            _logger = logger;
        }

        public override async Task Update(long id, Password updatedItem)
        {
            await base.Update(id, updatedItem);
            _logger.LogInformation($"Password has been changed for chatType={updatedItem.ChatType}");
        }

        public async Task<bool> IsAdminPass(string password)
        {
            return await IsPasswordValid(password, ChatType.Admin);
        }

        public async Task<bool> IsMemberPass(string password)
        {
            return await IsPasswordValid(password, ChatType.Public);
        }

        private async Task<bool> IsPasswordValid(string password, string chatType)
        {
            var passwords = await base.GetList();
            var memberPass = passwords.FirstOrDefault(p => p.ChatType == chatType);
            if (memberPass == null)
                return false;

            var bytes = Encoding.Unicode.GetBytes(memberPass.Value);
            var hash = HashMaker.GetHash(password);
            return bytes.Same(hash);
        }
    }
}
