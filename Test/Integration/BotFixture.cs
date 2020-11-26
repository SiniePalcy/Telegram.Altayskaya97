using Microsoft.Extensions.Configuration;
using Moq;
using System;
using Telegram.Altayskaya97.Model.Middleware;
using Telegram.Altayskaya97.Service;
using Telegram.Altayskaya97.Service.Interface;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Telegram.Altayskaya97.Test.Integration
{
    public class BotFixture : IDisposable
    {
        public Altayskaya97.Bot.Bot Bot { get; }

        //private Mock<IConfiguration> _configMock;

        public Mock<ITelegramBotClient> MockBotClient { get; }// = new Mock<ITelegramBotClient>();
        public BaseMapper<User, Altayskaya97.Core.Model.User> UserMapper => new BaseMapper<User, Altayskaya97.Core.Model.User>();
        public BaseMapper<Chat, Altayskaya97.Core.Model.Chat> ChatMapper => new BaseMapper<Chat, Altayskaya97.Core.Model.Chat>();

        public BotFixture()
        {
            var _configMock = SetUpConfigMock();

            var dateTimeServiceMock = new Mock<IDateTimeService>();
            dateTimeServiceMock.Setup(s => s.GetDateTimeUTCNow()).Returns(DateTime.UtcNow);
            dateTimeServiceMock.Setup(s => s.FormatToString(It.IsAny<DateTime>())).Returns(DateTime.UtcNow.ToString());

            Bot = new Altayskaya97.Bot.Bot(new Logger<Altayskaya97.Bot.Bot>(), 
                _configMock.Object, 
                new ButtonsService(), 
                new MenuService(), 
                null, 
                null, 
                null,
                dateTimeServiceMock.Object,
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

            var configClearGroupChatHoursMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(c => c.GetSection(It.Is<string>(s => s == "PeriodClearGroupChatHours")))
                .Returns(configClearGroupChatHoursMock.Object);
            configClearGroupChatHoursMock.SetupGet(c => c.Value).Returns("1");

            var configInactiveUserDaysMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(c => c.GetSection(It.Is<string>(s => s == "PeriodInactiveUserDays")))
                .Returns(configInactiveUserDaysMock.Object);
            configInactiveUserDaysMock.SetupGet(c => c.Value).Returns("3");

            var configWalkingTimeMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(c => c.GetSection(It.Is<string>(s => s == "WalkingTime")))
                .Returns(configWalkingTimeMock.Object);
            configWalkingTimeMock.SetupGet(c => c.Value).Returns("10:30");

            var banDaysMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(c => c.GetSection(It.Is<string>(s => s == "BanDays")))
                .Returns(banDaysMock.Object);
            banDaysMock.SetupGet(c => c.Value).Returns("Sunday");

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
