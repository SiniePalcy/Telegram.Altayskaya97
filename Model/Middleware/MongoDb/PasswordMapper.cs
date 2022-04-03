using AutoMapper;
using Telegram.Altayskaya97.Core.Model;
using Telegram.Altayskaya97.Core.Helpers;
using Telegram.Altayskaya97.Core.Constant;

namespace Telegram.Altayskaya97.Model.Middleware.MongoDb
{
    public class PasswordMapper : BaseMapper<Password, Entity.MongoDb.Password>
    {
        public PasswordMapper()
        {
            ModelToEntityConfig = new MapperConfiguration(cfg => 
                cfg.CreateMap<Password, Password>()
                .ForMember(nameof(Entity.MongoDb.Password.Hash), 
                    opt => opt.MapFrom(c => HashHelper.ComputeHash(c.Value, GlobalEnvironment.Encoding))));
            EntityToModelConfig = new MapperConfiguration(cfg => 
                cfg.CreateMap<Entity.MongoDb.Password, Password>()
                .ForMember(nameof(Password.Value), 
                    opt => opt.MapFrom(c => HashHelper.GetString(c.Hash))));
        }
    }
}
