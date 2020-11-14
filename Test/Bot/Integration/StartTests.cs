using Moq;
using System.Threading;
using Telegram.Altayskaya97.Bot.StateMachines;
using Telegram.Altayskaya97.Service.Interface;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Xunit;

namespace Telegram.Altayskaya97.Test.Bot
{
    public class StartTests : IClassFixture<BotFixture>
    {
        private readonly BotFixture _fixture = null;
        private readonly Altayskaya97.Bot.Bot _bot = null;

        public StartTests(BotFixture fixture)
        {
            _fixture = fixture;
            _bot = fixture.Bot;
        }

        [Fact]
        public void UnknownMessageTest()
        {
            string userName = "TestUser";
            var user = new User
            {
                Id = 1,
                Username = userName,
            };
            var chat = new Chat
            {
                Id = 1,
                Type = ChatType.Private
            };
            var chatRepo = _fixture.ChatMapper.MapToEntity(chat);
            var message = new Message
            {
                Chat = chat,
                From = user,
                Text = "/sturt"
            };

            _fixture.MockBotClient.Reset();

            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(s => s.GetUser(It.IsAny<string>()))
                .ReturnsAsync(default(Core.Model.User));
            _bot.UserService = userServiceMock.Object;

            var chatServiceMock = new Mock<IChatService>();
            chatServiceMock.Setup(s => s.GetChat(It.Is<long>(_ => _ == chat.Id)))
                .ReturnsAsync(chatRepo);
            _bot.ChatService = chatServiceMock.Object;
            _bot.StateMachines = new BaseStateMachine[] { new PostStateMachine(chatServiceMock.Object) };

            _bot.RecieveMessage(message).Wait();

            userServiceMock.Verify(mock => mock.GetUser(It.IsAny<long>()), Times.Never);
            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
                 It.IsAny<ChatId>(),
                 It.IsAny<string>(),
                 It.IsAny<ParseMode>(),
                 It.IsAny<bool>(),
                 It.IsAny<bool>(),
                 It.IsAny<int>(),
                 It.IsAny<IReplyMarkup>(),
                 It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public void StartTest()
        {
            string userName = "TestUser";
            var user = new User
            {
                Id = 1,
                Username = userName
            };
            var userRepo = _fixture.UserMapper.MapToEntity(user);
            var chat = new Chat
            {
                Id = 1,
                Type = ChatType.Private
            };
            var chatRepo = _fixture.ChatMapper.MapToEntity(chat);
            var message = new Message
            {
                Chat = chat,
                From = user,
                Text = "/start"
            };

            _fixture.MockBotClient.Reset();
            _fixture.MockBotClient.Setup(b => b.GetChatAsync(It.Is<ChatId>(_ => _.Identifier == chat.Id),
                It.IsAny<CancellationToken>())).ReturnsAsync(chat);

            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(s => s.GetUser(It.IsAny<string>()))
                .ReturnsAsync(default(Core.Model.User));
            _bot.UserService = userServiceMock.Object;

            var chatServiceMock = new Mock<IChatService>();
            chatServiceMock.Setup(s => s.GetChat(It.Is<long>(_ => _ == chat.Id)))
                .ReturnsAsync(chatRepo);
            _bot.ChatService = chatServiceMock.Object;

            _bot.RecieveMessage(message).Wait();

            userServiceMock.Verify(mock => mock.GetUser(It.Is<long>(_ => _ == 1)), Times.Once);
            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
            chatServiceMock.Verify(mock => mock.GetChat(It.Is<long>(_ => _ == 1)), Times.Once);

            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
                 It.Is<ChatId>(_ => _.Identifier == chat.Id),
                 It.IsAny<string>(),
                 It.Is<ParseMode>(_ => _ == ParseMode.Html),
                 It.IsAny<bool>(),
                 It.IsAny<bool>(),
                 It.IsAny<int>(),
                 It.IsAny<InlineKeyboardMarkup>(),
                 It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
