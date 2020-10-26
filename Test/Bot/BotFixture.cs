using Microsoft.Extensions.Configuration;
using Moq;
using System;
using Telegram.Altayskaya97.Model.Middleware;
using Telegram.Altayskaya97.Service;
using Telegram.Altayskaya97.Service.Interface;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Telegram.Altayskaya97.Test.Bot
{
    public class BotFixture : IDisposable
    {
        public Altayskaya97.Bot.Bot Bot { get; }

        //private Mock<IConfiguration> _configMock;

        public Mock<ITelegramBotClient> MockBotClient { get; }// = new Mock<ITelegramBotClient>();
        public BaseMapper<User, Core.Model.User> UserMapper => new BaseMapper<User, Core.Model.User>();
        public BaseMapper<Chat, Core.Model.Chat> ChatMapper => new BaseMapper<Chat, Core.Model.Chat>();

        public BotFixture()
        {
            var _configMock = SetUpConfigMock();

            Bot = new Altayskaya97.Bot.Bot(new Logger<Altayskaya97.Bot.Bot>(), 
                _configMock.Object, 
                new WelcomeService(), 
                new MenuService(), 
                null, 
                null, 
                null,
                false, 
                false);
            MockBotClient = new Mock<ITelegramBotClient>();
            Bot.BotClient = MockBotClient.Object;
            Bot.UserMessageService = new Mock<IUserMessageService>().Object;
        }

        private Mock<IConfiguration> SetUpConfigMock()
        {
            var configMock = new Mock<IConfiguration>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configMock.Setup(c => c.GetSection(It.Is<string>(s => s == "Configuration")))
                .Returns(configSectionMock.Object);

            var configPeriodEchoSecMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(c => c.GetSection(It.Is<string>(s => s == "PeriodEchoSec")))
                .Returns(configPeriodEchoSecMock.Object);
            configPeriodEchoSecMock.SetupGet(c => c.Value).Returns("5");

            var configPeriodResetAccessMinMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(c => c.GetSection(It.Is<string>(s => s == "PeriodResetAccessMin")))
                .Returns(configPeriodResetAccessMinMock.Object);
            configPeriodResetAccessMinMock.SetupGet(c => c.Value).Returns("1");

            var configPeriodChatListMinMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(c => c.GetSection(It.Is<string>(s => s == "PeriodChatListMin")))
                .Returns(configPeriodChatListMinMock.Object);
            configPeriodChatListMinMock.SetupGet(c => c.Value).Returns("1");

            var configClearPrivateChatMinMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(c => c.GetSection(It.Is<string>(s => s == "PeriodClearPrivateChatMin")))
                .Returns(configClearPrivateChatMinMock.Object);
            configClearPrivateChatMinMock.SetupGet(c => c.Value).Returns("1");

            var configBotCredsMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(c => c.GetSection(It.Is<string>(s => s == "altayskaya97_test_bot")))
                .Returns(configBotCredsMock.Object);
            configBotCredsMock.SetupGet(c => c.Value).Returns("1334252997:AAHXInE3TR2M1aW78MNmj1W0Bid6Zhcs5B0");

            return configMock;
        }

        public void Dispose()
        {
            Bot.Dispose();
        }
    }
}
