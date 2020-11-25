using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Altayskaya97.Core.Interface;

namespace Telegram.Altayskaya97.Service.Interface
{
    public interface IRepositoryService<T> :IService where T : IObject
    {
        Task<ICollection<T>> GetList();
        Task<T> Get(long id);
        Task Add(T item);
        Task<T> Delete(long id);
        Task Update(long id, T updatedItem);
        Task Update(T updatedItem);
    }
}
