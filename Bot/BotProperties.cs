using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Altayskaya97.Core.Constant;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;
using Telegram.BotAPI.GettingUpdates;

namespace Telegram.Altayskaya97.Bot
{
    public sealed class BotProperties : IBotProperties
    {
        private readonly BotCommandHelper _commandHelper;

        public BotProperties(IConfiguration configuration)
        {
            string botName = GlobalEnvironment.BotName.StartsWith("@") 
                ? GlobalEnvironment.BotName.Remove(0, 1) 
                : GlobalEnvironment.BotName;

            var botToken = configuration[$"{botName}"]; 
            Api = new BotClient(botToken);
            User = Api.GetMe();

            _commandHelper = new BotCommandHelper(this);

            // Delete my old commands
            Api.DeleteMyCommands();
            // Set my commands
            Api.SetMyCommands(
                new BotCommand("hello", "Hello world!"));

            // Delete webhook to use Long Polling
            Api.DeleteWebhook();
        }

        public BotClient Api { get; }
        public User User { get; }

        IBotCommandHelper IBotProperties.CommandHelper => _commandHelper;
    }
}
