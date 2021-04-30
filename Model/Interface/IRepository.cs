using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Altayskaya97.Core.Interface;

namespace Telegram.Altayskaya97.Model.Interface
{
    public interface IRepository<T> where T:IObject
    {
        Task<ICollection<T>> GetCollection();
        Task PushCollection(ICollection<T> collection);
        Task ClearCollection();
        Task<T> Get(long id);
        Task Add(T item);
        Task Update(long id, T item);
        Task Remove(long id);
    }
}
