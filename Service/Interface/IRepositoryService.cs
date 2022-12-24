using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.SafeBot.Core.Interface;

namespace Telegram.SafeBot.Service.Interface
{
    public interface IRepositoryService<T> :IService where T : IObject
    {
        Task<IEnumerable<T>> GetList();
        Task Clear();
        Task<T> Get(long id);
        Task Add(T item);
        Task Add(ICollection<T> items);
        Task<T> Delete(long id);
        Task Update(long id, T updatedItem);
        Task Update(T updatedItem);
    }
}
