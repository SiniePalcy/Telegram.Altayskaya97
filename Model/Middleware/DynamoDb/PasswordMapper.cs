using AutoMapper;
using System.Text;
using Telegram.Altayskaya97.Core.Model;
using Telegram.Altayskaya97.Core.Helpers;

namespace Telegram.Altayskaya97.Model.Middleware.DynamoDb
{
    public class PasswordMapper : BaseMapper<Password, Entity.DynamoDb.Password>
    {
        private static readonly Encoding ENCODING = Core.Constant.GlobalEnvironment.Encoding;
        public PasswordMapper()
        {
            ModelToEntityConfig = new MapperConfiguration(cfg => 
                cfg.CreateMap<Password, Entity.DynamoDb.Password>()
                .ForMember(nameof(Entity.DynamoDb.Password.Hash), 
                    opt => opt.MapFrom(c => HashMaker.ComputeHash(c.Value, ENCODING))));
            EntityToModelConfig = new MapperConfiguration(cfg => 
                cfg.CreateMap<Entity.DynamoDb.Password, Password>()
                .ForMember(nameof(Password.Value), 
                    opt => opt.MapFrom(c => ENCODING.GetString(c.Hash))));
        }
    }
}
