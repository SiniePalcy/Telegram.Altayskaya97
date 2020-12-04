using AutoMapper;
using System.Collections.Generic;
using Telegram.Altayskaya97.Model.Interface;

namespace Telegram.Altayskaya97.Model.Middleware
{
    public class BaseMapper<TModel, TEntity> : IModelEntityMapper<TModel, TEntity>
    {
        public MapperConfiguration ModelToEntityConfig { get; protected set; }
        public MapperConfiguration EntityToModelConfig { get; protected set; }
        public BaseMapper()
        {
            ModelToEntityConfig = new MapperConfiguration(cfg => cfg.CreateMap<TModel, TEntity>());
            EntityToModelConfig = new MapperConfiguration(cfg => cfg.CreateMap<TEntity, TModel>());
        }

        public virtual TEntity MapToEntity(TModel item)
        {
            if (item == null)
                return default;

            var mapper = new Mapper(ModelToEntityConfig);
            var entity = mapper.Map<TEntity>(item);
            return entity;
        }

        public virtual TModel MapToModel(TEntity item)
        {
            if (item == null)
                return default;

            var mapper = new Mapper(EntityToModelConfig);
            var model = mapper.Map<TModel>(item);
            return model;
        }

        public virtual ICollection<TEntity> MapToEntityList(ICollection<TModel> item)
        {
            var mapper = new Mapper(ModelToEntityConfig);
            var list = mapper.Map<ICollection<TEntity>>(item);
            return list;
        }

        public virtual ICollection<TModel> MapToModelList(ICollection<TEntity> item)
        {
            var mapper = new Mapper(EntityToModelConfig);
            var list = mapper.Map<ICollection<TModel>>(item);
            return list;
        }
    }
}
