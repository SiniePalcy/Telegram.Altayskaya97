using AutoMapper;
using Telegram.Altayskaya97.Core.Model;
using Telegram.Altayskaya97.Core.Helpers;
using Telegram.Altayskaya97.Core.Constant;

namespace Telegram.Altayskaya97.Model.Middleware.DynamoDb
{
    public class PasswordMapper : BaseMapper<Password, Entity.DynamoDb.Password>
    {
        public PasswordMapper()
        {
            ModelToEntityConfig = new MapperConfiguration(cfg => 
                cfg.CreateMap<Password, Entity.DynamoDb.Password>()
                .ForMember(nameof(Entity.DynamoDb.Password.Hash), 
                    opt => opt.MapFrom(c => HashHelper.ComputeHash(c.Value, GlobalEnvironment.Encoding))));
            EntityToModelConfig = new MapperConfiguration(cfg => 
                cfg.CreateMap<Entity.DynamoDb.Password, Password>()
                .ForMember(nameof(Password.Value), 
                    opt => opt.MapFrom(c => HashHelper.GetString(c.Hash))));
        }
    }
}
