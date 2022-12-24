﻿using AutoMapper;
using Telegram.SafeBot.Core.Model;
using Telegram.SafeBot.Core.Helpers;
using Telegram.SafeBot.Core.Constant;

namespace Telegram.SafeBot.Model.Middleware.MongoDb
{
    public class PasswordMapper : BaseMapper<Password, Entity.MongoDb.Password>
    {
        public PasswordMapper()
        {
            ModelToEntityConfig = new MapperConfiguration(cfg => 
                cfg.CreateMap<Password, Entity.MongoDb.Password>()
                .ForMember(nameof(Entity.MongoDb.Password.Hash), 
                    opt => opt.MapFrom(c => HashHelper.ComputeHash(c.Value, GlobalEnvironment.Encoding))));
            EntityToModelConfig = new MapperConfiguration(cfg => 
                cfg.CreateMap<Entity.MongoDb.Password, Password>()
                .ForMember(nameof(Password.Value), 
                    opt => opt.MapFrom(c => HashHelper.GetString(c.Hash))));
        }
    }
}