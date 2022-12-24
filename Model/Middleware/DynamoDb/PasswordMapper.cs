using AutoMapper;
using Telegram.SafeBot.Core.Model;
using Telegram.SafeBot.Core.Helpers;
using Telegram.SafeBot.Core.Constant;

namespace Telegram.SafeBot.Model.Middleware.DynamoDb
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
