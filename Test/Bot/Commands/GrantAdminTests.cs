using Moq;
using System.Threading;
using Telegram.Altayskaya97.Core.Constant;
using Telegram.Altayskaya97.Model.Middleware;
using Telegram.Altayskaya97.Service.Interface;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Xunit;

namespace Telegram.Altayskaya97.Test.Bot
{
    public class GrantAdminTests : IClassFixture<BotFixture>
    {
        private BotFixture _fixture = null;
        private Altayskaya97.Bot.Bot _bot = null;

        public GrantAdminTests(BotFixture fixture)
        {
            _fixture = fixture;
            _bot = fixture.Bot;
        }

        [Fact]
        public void GrantNonAdminTest()
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
                Text = "/shpic"
            };

            _fixture.MockBotClient.Reset();

            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(s => s.GetUser(It.IsAny<long>()))
                .ReturnsAsync(default(Core.Model.User));
            _bot.UserService = userServiceMock.Object;

            _bot.RecieveMessage(message).Wait();

            userServiceMock.Verify(mock => mock.GetUser(It.Is<long>(_ => _ == user.Id)), Times.Once);
            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
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
        public void GrantBlockedKickedAdminWithoutPermissionsTest()
        {
            string userName = "TestUser";
            var user = new User
            {
                Id = 1,
                Username = userName,
            };
            var userRepo = _fixture.UserMapper.MapToEntity(user);
            userRepo.Type = Core.Model.UserType.Admin;
            var chat = new Chat
            {
                Id = 1,
                Type = ChatType.Private
            };
            var chat1 = new Chat
            {
                Id = 2,
                Type = ChatType.Group
            };
            var chat2 = new Chat
            {
                Id = 3,
                Type = ChatType.Supergroup
            };
            var chatRepo1 = _fixture.ChatMapper.MapToEntity(chat);
            chatRepo1.ChatType = Core.Model.ChatType.Private;
            var chatRepo2 = _fixture.ChatMapper.MapToEntity(chat1);
            chatRepo2.ChatType = Core.Model.ChatType.Admin;
            var chatRepo3 = _fixture.ChatMapper.MapToEntity(chat2);
            chatRepo3.ChatType = Core.Model.ChatType.Public;
            var chats = new Core.Model.Chat[] { chatRepo1, chatRepo2, chatRepo3 };

            var chatMember = new ChatMember { User = user, Status = ChatMemberStatus.Kicked };
            var message = new Message
            {
                Chat = chat,
                From = user,
                Text = "/shpic"
            };

            _fixture.MockBotClient.Reset();
            _fixture.MockBotClient.Setup(mock => mock.GetChatMemberAsync(It.IsAny<ChatId>(),
                It.Is<int>(_ => _ == user.Id), It.IsAny<CancellationToken>())).
                ReturnsAsync(chatMember);

            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(s => s.GetUser(It.Is<long>(_ => _ == user.Id)))
                .ReturnsAsync(userRepo);
            userServiceMock.Setup(s => s.IsAdmin(It.Is<long>(_ => _ == user.Id)))
                .ReturnsAsync(userRepo.IsAdmin);
            _bot.UserService = userServiceMock.Object;

            var chatServiceMock = new Mock<IChatService>();
            chatServiceMock.Setup(s => s.GetChatList())
                .ReturnsAsync(chats);
            _bot.ChatService = chatServiceMock.Object;

            _bot.RecieveMessage(message).Wait();

            userServiceMock.Verify(mock => mock.GetUser(It.Is<long>(_ => _ == user.Id)), Times.Once);
            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Once);
            userServiceMock.Verify(mock => mock.GetUserList(), Times.Never);
            _fixture.MockBotClient.Verify(mock => mock.ExportChatInviteLinkAsync(
                It.IsAny<ChatId>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            _fixture.MockBotClient.Verify(mock => mock.UnbanChatMemberAsync(It.IsAny<ChatId>(), 
                It.Is<int>(_ => _ == user.Id), It.IsAny<CancellationToken>()), Times.Exactly(2));
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
                 It.Is<ChatId>(_ => _.Identifier == chat.Id),
                 It.IsAny<string>(),
                 It.IsAny<ParseMode>(),
                 It.IsAny<bool>(),
                 It.IsAny<bool>(),
                 It.IsAny<int>(),
                 It.IsAny<IReplyMarkup>(),
                 It.IsAny<CancellationToken>()), Times.Exactly(2));
                        _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
                 It.Is<ChatId>(_ => _.Identifier == chat.Id),
                 It.IsAny<string>(),
                 It.IsAny<ParseMode>(),
                 It.IsAny<bool>(),
                 It.IsAny<bool>(),
                 It.IsAny<int>(),
                 It.IsAny<IReplyMarkup>(),
                 It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Fact]
        public void GrantBlockedLeftAdminWithoutPermissionsTest()
        {
            string userName = "TestUser";
            var user = new User
            {
                Id = 1,
                Username = userName,
            };
            var userRepo = _fixture.UserMapper.MapToEntity(user);
            userRepo.Type = Core.Model.UserType.Admin;
            var chat = new Chat
            {
                Id = 1,
                Type = ChatType.Private
            };
            var chat1 = new Chat
            {
                Id = 2,
                Type = ChatType.Group
            };
            var chat2 = new Chat
            {
                Id = 3,
                Type = ChatType.Supergroup
            };
            var chatRepo1 = _fixture.ChatMapper.MapToEntity(chat);
            chatRepo1.ChatType = Core.Model.ChatType.Private;
            var chatRepo2 = _fixture.ChatMapper.MapToEntity(chat1);
            chatRepo2.ChatType = Core.Model.ChatType.Admin;
            var chatRepo3 = _fixture.ChatMapper.MapToEntity(chat2);
            chatRepo3.ChatType = Core.Model.ChatType.Public;
            var chats = new Core.Model.Chat[] { chatRepo1, chatRepo2, chatRepo3 };

            var chatMember = new ChatMember { User = user, Status = ChatMemberStatus.Left };
            var message = new Message
            {
                Chat = chat,
                From = user,
                Text = "/shpic"
            };

            _fixture.MockBotClient.Reset();
            _fixture.MockBotClient.Setup(mock => mock.GetChatMemberAsync(It.IsAny<ChatId>(),
                It.Is<int>(_ => _ == user.Id), It.IsAny<CancellationToken>())).
                ReturnsAsync(chatMember);

            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(s => s.GetUser(It.Is<long>(_ => _ == user.Id)))
                .ReturnsAsync(userRepo);
            userServiceMock.Setup(s => s.IsAdmin(It.Is<long>(_ => _ == user.Id)))
                .ReturnsAsync(userRepo.IsAdmin);
            _bot.UserService = userServiceMock.Object;

            var chatServiceMock = new Mock<IChatService>();
            chatServiceMock.Setup(s => s.GetChatList())
                .ReturnsAsync(chats);
            _bot.ChatService = chatServiceMock.Object;

            _bot.RecieveMessage(message).Wait();

            chatMember.Status = ChatMemberStatus.Kicked;
            _bot.RecieveMessage(message).Wait();

            userServiceMock.Verify(mock => mock.GetUser(It.Is<long>(_ => _ == user.Id)), Times.Exactly(2));
            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Exactly(2));
            userServiceMock.Verify(mock => mock.GetUserList(), Times.Never);
            _fixture.MockBotClient.Verify(mock => mock.ExportChatInviteLinkAsync(
                It.IsAny<ChatId>(), It.IsAny<CancellationToken>()), Times.Exactly(4));
            _fixture.MockBotClient.Verify(mock => mock.UnbanChatMemberAsync(It.IsAny<ChatId>(),
                It.Is<int>(_ => _ == user.Id), It.IsAny<CancellationToken>()), Times.Exactly(2));
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
                 It.Is<ChatId>(_ => _.Identifier == chat.Id),
                 It.IsAny<string>(),
                 It.IsAny<ParseMode>(),
                 It.IsAny<bool>(),
                 It.IsAny<bool>(),
                 It.IsAny<int>(),
                 It.IsAny<IReplyMarkup>(),
                 It.IsAny<CancellationToken>()), Times.Exactly(4));
        }

    }
}
