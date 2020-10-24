using Moq;
using System.Threading;
using Telegram.Altayskaya97.Model.Middleware;
using Telegram.Altayskaya97.Service.Interface;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Xunit;

namespace Telegram.Altayskaya97.Test.Bot
{
    public class ListsTests : IClassFixture<BotFixture>
    {
        private BotFixture _fixture = null;
        private Altayskaya97.Bot.Bot _bot = null;

        public ListsTests(BotFixture fixture)
        {
            _fixture = fixture;
            _bot = fixture.Bot;
        }

        [Fact]
        public void ListsUnknownUserTest()
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
            var message = new Message
            {
                Chat = chat,
                From = user,
                Text = "/userlist"
            };

            _fixture.MockBotClient.Reset();

            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(s => s.GetUser(It.IsAny<long>()))
                .ReturnsAsync(default(Core.Model.User));
            _bot.UserService = userServiceMock.Object;

            _bot.RecieveMessage(message).Wait();

            message.Text = "/chatlist";
            _bot.RecieveMessage(message).Wait();

            userServiceMock.Verify(mock => mock.GetUser(It.Is<long>(_ => _ == user.Id)), Times.Exactly(2));
            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
            userServiceMock.Verify(mock => mock.BanUser(It.IsAny<long>()), Times.Never);
            userServiceMock.Verify(mock => mock.UnbanUser(It.IsAny<long>()), Times.Never);
            userServiceMock.Verify(mock => mock.GetUserList(), Times.Never);
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
        public void ListsNonAdminTest()
        {
            string userName = "TestUser";
            var user = new User
            {
                Id = 1,
                Username = userName,
            };
            var userRepo = _fixture.UserMapper.MapToEntity(user);
            var chat = new Chat
            {
                Id = 1,
                Type = ChatType.Private
            };
            var message = new Message
            {
                Chat = chat,
                From = user,
                Text = "/userlist"
            };

            _fixture.MockBotClient.Reset();

            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(s => s.GetUser(It.Is<long>(_ => _ == user.Id)))
                .ReturnsAsync(userRepo);
            _bot.UserService = userServiceMock.Object;

            _bot.RecieveMessage(message).Wait();

            message.Text = "/chatlist";
            _bot.RecieveMessage(message).Wait();

            userServiceMock.Verify(mock => mock.GetUser(It.Is<long>(_ => _ == user.Id)), Times.Exactly(2));
            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
            userServiceMock.Verify(mock => mock.BanUser(It.IsAny<long>()), Times.Never);
            userServiceMock.Verify(mock => mock.UnbanUser(It.IsAny<long>()), Times.Never);
            userServiceMock.Verify(mock => mock.GetUserList(), Times.Never);
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
        public void ListsAdminTest()
        {
            string userName = "TestUser";
            var user1 = new User
            {
                Id = 1,
                Username = userName + "1"
            };
            var user2 = new User
            {
                Id = 2,
                Username = userName + "2"
            };
            var userRepo1 = _fixture.UserMapper.MapToEntity(user1);
            userRepo1.IsAdmin = true;
            userRepo1.Type = Core.Model.UserType.Admin;
            var userRepo2 = _fixture.UserMapper.MapToEntity(user2);
            var chat1 = new Chat
            {
                Id = 1,
                Type = ChatType.Private
            };
            var chat2 = new Chat
            {
                Id = 2,
                Type = ChatType.Private
            };
            var chatRepo1 = _fixture.ChatMapper.MapToEntity(chat1);
            var chatRepo2 = _fixture.ChatMapper.MapToEntity(chat2);
            var message = new Message
            {
                Chat = chat1,
                From = user1,
                Text = "/userlist"
            };

            _fixture.MockBotClient.Reset();
            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(s => s.GetUser(It.Is<long>(_ => _ == user1.Id)))
                .ReturnsAsync(userRepo1);
            userServiceMock.Setup(s => s.IsAdmin(It.Is<long>(_ => _ == user1.Id)))
                .ReturnsAsync(true);
            userServiceMock.Setup(s => s.GetUserList())
                .ReturnsAsync(new Core.Model.User[] { userRepo1, userRepo2 });
            _bot.UserService = userServiceMock.Object;
            var chatServiceMock = new Mock<IChatService>();
            chatServiceMock.Setup(s => s.GetChatList())
                .ReturnsAsync(new Core.Model.Chat[] { chatRepo1, chatRepo2 });
            _bot.ChatService = chatServiceMock.Object;

            _bot.RecieveMessage(message).Wait();

            message.Text = "/chatlist";
            _bot.RecieveMessage(message).Wait();
            
            userServiceMock.Verify(mock => mock.GetUser(It.Is<long>(_ => _ == user1.Id)), Times.Exactly(2));
            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
            userServiceMock.Verify(mock => mock.BanUser(It.IsAny<long>()), Times.Never);
            userServiceMock.Verify(mock => mock.UnbanUser(It.IsAny<long>()), Times.Never);
            userServiceMock.Verify(mock => mock.GetUserList(), Times.Once);
            chatServiceMock.Verify(mock => mock.GetChatList(), Times.Once);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
                 It.Is<ChatId>(_ => _.Identifier == chat1.Id),
                 It.IsAny<string>(),
                 It.IsAny<ParseMode>(),
                 It.IsAny<bool>(),
                 It.IsAny<bool>(),
                 It.IsAny<int>(),
                 It.IsAny<IReplyMarkup>(),
                 It.IsAny<CancellationToken>()), Times.Exactly(2));
        }
    }
}
