//using Microsoft.Extensions.Configuration;
//using Moq;
//using System;
//using Telegram.SafeBot.Model.Middleware;
//using Telegram.SafeBot.Service;
//using Telegram.SafeBot.Service.Interface;
//using Telegram.Bot;
//using Telegram.Bot.Types;

//namespace Telegram.SafeBot.Test.Integration
//{
//    public class BotFixture : IDisposable
//    {
//        public SafeBot.Bot.Bot Bot { get; }

//        //private Mock<IConfiguration> _configMock;

//        public Mock<ITelegramBotClient> MockBotClient { get; }// = new Mock<ITelegramBotClient>();
//        public BaseMapper<User, SafeBot.Core.Model.User> UserMapper => new BaseMapper<User, SafeBot.Core.Model.User>();
//        public BaseMapper<Chat, SafeBot.Core.Model.Chat> ChatMapper => new BaseMapper<Chat, SafeBot.Core.Model.Chat>();

//        public BotFixture()
//        {
//            var _configMock = SetUpConfigMock();

//            var dateTimeServiceMock = new Mock<IDateTimeService>();
//            dateTimeServiceMock.Setup(s => s.GetDateTimeUTCNow()).Returns(DateTime.UtcNow);
//            dateTimeServiceMock.Setup(s => s.FormatToString(It.IsAny<DateTime>())).Returns(DateTime.UtcNow.ToString());

//            Bot = new SafeBot.Bot.Bot(new Logger<SafeBot.Bot.Bot>(),
//                _configMock.Object,
//                new ButtonsService(),
//                new MenuService(),
//                null,
//                null,
//                null,
//                null,
//                dateTimeServiceMock.Object,
//                false,
//                false);
//            MockBotClient = new Mock<ITelegramBotClient>();
//            Bot.BotClient = MockBotClient.Object;
//            Bot.UserMessageService = new Mock<IUserMessageService>().Object;
//        }

//        private Mock<IConfiguration> SetUpConfigMock()
//        {
//            var configMock = new Mock<IConfiguration>();
//            var configSectionMock = new Mock<IConfigurationSection>();
//            configMock.Setup(c => c.GetSection(It.Is<string>(s => s == "Configuration")))
//                .Returns(configSectionMock.Object);

//            var configPeriodEchoSecMock = new Mock<IConfigurationSection>();
//            configSectionMock.Setup(c => c.GetSection(It.Is<string>(s => s == "PeriodEchoSec")))
//                .Returns(configPeriodEchoSecMock.Object);
//            configPeriodEchoSecMock.SetupGet(c => c.Value).Returns("5");

//            var configPeriodResetAccessMinMock = new Mock<IConfigurationSection>();
//            configSectionMock.Setup(c => c.GetSection(It.Is<string>(s => s == "PeriodResetAccessMin")))
//                .Returns(configPeriodResetAccessMinMock.Object);
//            configPeriodResetAccessMinMock.SetupGet(c => c.Value).Returns("1");

//            var configPeriodChatListMinMock = new Mock<IConfigurationSection>();
//            configSectionMock.Setup(c => c.GetSection(It.Is<string>(s => s == "PeriodChatListMin")))
//                .Returns(configPeriodChatListMinMock.Object);
//            configPeriodChatListMinMock.SetupGet(c => c.Value).Returns("1");

//            var configClearPrivateChatMinMock = new Mock<IConfigurationSection>();
//            configSectionMock.Setup(c => c.GetSection(It.Is<string>(s => s == "PeriodClearPrivateChatMin")))
//                .Returns(configClearPrivateChatMinMock.Object);
//            configClearPrivateChatMinMock.SetupGet(c => c.Value).Returns("1");

//            var configClearGroupChatHoursMock = new Mock<IConfigurationSection>();
//            configSectionMock.Setup(c => c.GetSection(It.Is<string>(s => s == "PeriodClearGroupChatHours")))
//                .Returns(configClearGroupChatHoursMock.Object);
//            configClearGroupChatHoursMock.SetupGet(c => c.Value).Returns("1");

//            var configInactiveUserDaysMock = new Mock<IConfigurationSection>();
//            configSectionMock.Setup(c => c.GetSection(It.Is<string>(s => s == "PeriodInactiveUserDays")))
//                .Returns(configInactiveUserDaysMock.Object);
//            configInactiveUserDaysMock.SetupGet(c => c.Value).Returns("3");

//            var configWalkingTimeMock = new Mock<IConfigurationSection>();
//            configSectionMock.Setup(c => c.GetSection(It.Is<string>(s => s == "WalkingTime")))
//                .Returns(configWalkingTimeMock.Object);
//            configWalkingTimeMock.SetupGet(c => c.Value).Returns("10:30");

//            var banDaysMock = new Mock<IConfigurationSection>();
//            configSectionMock.Setup(c => c.GetSection(It.Is<string>(s => s == "BanDays")))
//                .Returns(banDaysMock.Object);
//            banDaysMock.SetupGet(c => c.Value).Returns("Sunday");

//            var configBotCredsMock = new Mock<IConfigurationSection>();
//            configSectionMock.Setup(c => c.GetSection(It.Is<string>(s => s == "SafeBot_test_bot")))
//                .Returns(configBotCredsMock.Object);
//            configBotCredsMock.SetupGet(c => c.Value).Returns("1334252997:AAHXInE3TR2M1aW78MNmj1W0Bid6Zhcs5B0");

//            return configMock;
//        }

//        public void Dispose()
//        {
//            Bot.Dispose();
//        }
//    }
//}
