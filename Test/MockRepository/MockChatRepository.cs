using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Altayskaya97.Core.Model;
using Telegram.Altayskaya97.Model.Interface;

namespace Telegram.Altayskaya97.Test.MockRepository
{
    public class MockChatRepository : IRepository<Chat>
    {
        public Task AddItem(Chat item)
        {
            throw new NotImplementedException();
        }

        public Task ClearCollection()
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<Chat>> GetCollection()
        {
            throw new NotImplementedException();
        }

        public Task<Chat> GetItem(long id)
        {
            throw new NotImplementedException();
        }

        public Task RemoveItem(long id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateItem(Chat item)
        {
            throw new NotImplementedException();
        }
    }
}
