using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Altayskaya97.Core.Interface;
using Telegram.Altayskaya97.Model.Interface;
using Telegram.Altayskaya97.Service.Interface;

namespace Telegram.Altayskaya97.Service
{
    public abstract class RepositoryService<T> : IRepositoryService<T> where T : IObject
    {
        protected IRepository<T> _repo;
        public virtual async Task<ICollection<T>> GetList()
        {
            return await _repo.GetCollection();
        }

        public virtual async Task<T> Get(long id)
        {
            return await _repo.Get(id);
        }

        public virtual async Task Add(T item)
        {
            await _repo.Add(item);
        }

        public virtual async Task<T> Delete(long id)
        {
            var item = await _repo.Get(id);
            if (item == null)
                return default;

            await _repo.Remove(id);
            return item;
        }

        public virtual async Task Update(long id, T updatedItem)
        {
            await _repo.Update(id, updatedItem);
        }

        public virtual async Task Update(T updatedItem)
        {
            await Update(updatedItem.Id, updatedItem);
        }

        public virtual async Task Clear()
        {
            await _repo.ClearCollection();
        }

        public virtual async Task Add(ICollection<T> items)
        {
            await _repo.PushCollection(items);
        }
    }
}
