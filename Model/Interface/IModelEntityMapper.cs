using AutoMapper;
using System.Collections.Generic;

namespace Telegram.SafeBot.Model.Interface
{
    public interface IModelEntityMapper<TModel, TEntity>
    {
        MapperConfiguration ModelToEntityConfig { get; }
        MapperConfiguration EntityToModelConfig { get; }
        TEntity MapToEntity(TModel item);
        TModel MapToModel(TEntity item);
        ICollection<TEntity> MapToEntityList(ICollection<TModel> item);
        ICollection<TModel> MapToModelList(ICollection<TEntity> item);
    }
}
