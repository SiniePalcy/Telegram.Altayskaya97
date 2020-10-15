using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Altayskaya97.Bot;
using Telegram.Altayskaya97.Model.Interface;
using Telegram.Altayskaya97.Service;
using Telegram.Altayskaya97.Service.Interface;
using Telegram.Altayskaya97.Test.MockRepository;

namespace Telegram.Altayskaya97.Test.Bot
{
    public abstract class BaseBotTest : IDisposable
    {
        protected Altayskaya97.Bot.Bot bot { get; }

        private Mock<IDbContext> _mockDbContext;

        private Mock<IConfiguration> _configMock;
        private Mock<IConfigurationSection> _configSectionMock;
        private Mock<IConfigurationSection> _configPeriodEchoSecMock;
        private Mock<IConfigurationSection> _configPeriodResetAccessMinMock;
        private Mock<IConfigurationSection> _configPeriodChatListMinMock;

        protected BaseBotTest()
        {
            var settings = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("JWT:Issuer", "TestIssuer"),
                new KeyValuePair<string, string>("JWT:Audience", "TestAudience"),
                new KeyValuePair<string, string>("JWT:SecurityKey", "TestSecurityKey")
            };
            var builder = new ConfigurationBuilder().AddInMemoryCollection(settings);
            var configuration   = builder.Build();

            _configMock = SetUpConfigMock();

            _mockDbContext = new Mock<IDbContext>();
            var userRepository = new MockUserRepository();
            var chatRepository = new MockChatRepository();
            _mockDbContext.SetupGet(s => s.UserRepository).Returns(userRepository);
            _mockDbContext.SetupGet(s => s.ChatRepository).Returns(chatRepository);

            UserService userService = new UserService(_mockDbContext.Object, new Logger<UserService>());
            ChatService chatService = new ChatService(_mockDbContext.Object, new Logger<ChatService>());

            bot = new Altayskaya97.Bot.Bot(new Logger<Altayskaya97.Bot.Bot>(), 
                _configMock.Object, new WelcomeService(), new MenuService(), userService, chatService);
        }

        private Mock<IConfiguration> SetUpConfigMock()
        {
            var configMock = new Mock<IConfiguration>();
            _configSectionMock = new Mock<IConfigurationSection>();
            configMock.Setup(c => c.GetSection(It.Is<string>(s => s == "Configuration")))
                .Returns(_configSectionMock.Object);

            _configPeriodEchoSecMock = new Mock<IConfigurationSection>();
            _configSectionMock.Setup(c => c.GetSection(It.Is<string>(s => s == "PeriodEchoSec")))
                .Returns(_configPeriodEchoSecMock.Object);
            _configPeriodEchoSecMock.SetupGet(c => c.Value).Returns("5");

            _configPeriodResetAccessMinMock = new Mock<IConfigurationSection>();
            _configSectionMock.Setup(c => c.GetSection(It.Is<string>(s => s == "PeriodResetAccessMin")))
                .Returns(_configPeriodResetAccessMinMock.Object);
            _configPeriodResetAccessMinMock.SetupGet(c => c.Value).Returns("1");


            _configPeriodChatListMinMock = new Mock<IConfigurationSection>();
            _configSectionMock.Setup(c => c.GetSection(It.Is<string>(s => s == "PeriodChatListMin")))
                .Returns(_configPeriodChatListMinMock.Object);
            _configPeriodChatListMinMock.SetupGet(c => c.Value).Returns("1");

            return configMock;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
