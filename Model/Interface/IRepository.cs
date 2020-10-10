using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Altayskaya97.Core.Interface;

namespace Telegram.Altayskaya97.Model.Interface
{
    public interface IRepository<T> where T:IObject
    {
        Task<ICollection<T>> GetCollection();
        Task ClearCollection();
        Task<T> GetItem(long id);
        Task AddItem(T item);
        Task UpdateItem(T item);
        Task RemoveItem(long id);
    }
}
