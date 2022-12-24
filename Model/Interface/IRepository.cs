using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.SafeBot.Core.Interface;

namespace Telegram.SafeBot.Model.Interface
{
    public interface IRepository<T> where T:IObject
    {
        void Initialize();
        Task<ICollection<T>> GetCollection();
        Task PushCollection(ICollection<T> collection);
        Task ClearCollection();
        Task<T> Get(long id);
        Task Add(T item);
        Task Update(long id, T item);
        Task Remove(long id);
    }
}
