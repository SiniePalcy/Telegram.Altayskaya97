using Microsoft.Extensions.Logging;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Telegram.SafeBot.Core.Model;
using Telegram.SafeBot.Model.Interface;
using Telegram.SafeBot.Service.Interface;
using Telegram.SafeBot.Core.Helpers;
using Telegram.SafeBot.Core.Extensions;
using Telegram.SafeBot.Core.Constant;

namespace Telegram.SafeBot.Service
{
    public class PasswordService : RepositoryService<Password>, IPasswordService
    {
        public PasswordService(ILogger<PasswordService> logger, IRepository<Password> repository) : base(logger, repository)
        {
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

            var bytes = HashHelper.GetBytes(memberPass.Value);
            var hash = HashHelper.ComputeHash(password,  GlobalEnvironment.Encoding);
            return bytes.Same(hash);
        }

        public async Task<Password> GetByType(string chatType)
        {
            var list = await GetList();
            var password = list.FirstOrDefault(c => c.ChatType == chatType);
            return password;
        }
    }
}
