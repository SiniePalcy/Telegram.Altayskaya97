using Microsoft.Extensions.Configuration;
using System;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;
using Telegram.BotAPI.GettingUpdates;

namespace Telegram.SafeBot.Bot
{
    public sealed class BotProperties : IBotProperties
    {
        private readonly BotCommandHelper _commandHelper;

        public BotProperties(IConfiguration configuration)
        {
            string botName = Environment.GetEnvironmentVariable("BOT_NAME");

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
