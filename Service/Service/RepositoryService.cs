using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.SafeBot.Core.Interface;
using Telegram.SafeBot.Model.Interface;
using Telegram.SafeBot.Service.Interface;

namespace Telegram.SafeBot.Service
{
    public abstract class RepositoryService<T> : IRepositoryService<T> where T : IObject
    {
        protected ILogger _logger;
        protected IRepository<T> _repo;

        public RepositoryService(ILogger logger, IRepository<T> repository)
        {
            _logger = logger;
            _repo = repository;
        }

        public virtual async Task<IEnumerable<T>> GetList()
        {
            var collection = await _repo.GetCollection() ?? Enumerable.Empty<T>();
            return collection;
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
