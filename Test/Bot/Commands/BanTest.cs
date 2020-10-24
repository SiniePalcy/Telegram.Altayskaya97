using Moq;
using System.Threading;
using Telegram.Altayskaya97.Core.Constant;
using Telegram.Altayskaya97.Model.Middleware;
using Telegram.Altayskaya97.Service.Interface;
using Telegram.Altayskaya97.Bot.Helpers;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Xunit;
using System;

namespace Telegram.Altayskaya97.Test.Bot
{
    public class BanTest : IClassFixture<BotFixture>
    {
        private BotFixture _fixture = null;
        private Altayskaya97.Bot.Bot _bot = null;
        private BaseMapper<User, Core.Model.User> _userMapper = new BaseMapper<User, Core.Model.User>();
        private BaseMapper<Chat, Core.Model.Chat> _chatMapper = new BaseMapper<Chat, Core.Model.Chat>();

        public BanTest(BotFixture fixture)
        {
            _fixture = fixture;
            _bot = fixture.Bot;
        }

        [Fact]
        public void BanFromNonAdminTest()
        {
            string userName = "TestUser";
            var user1 = new User
            {
                Id = 1,
                Username = userName + "1",
            };
            var userRepo = _userMapper.MapToEntity(user1);
            var chat = new Chat
            {
                Id = 1,
                Type = ChatType.Private
            };
            var message = new Message
            {
                Chat = chat,
                From = user1,
                Text = "/ban testuser2"
            };

            _fixture.MockBotClient.Reset();

            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(s => s.GetUser(It.Is<long>(_ => _ == user1.Id)))
                .ReturnsAsync(userRepo);
            _bot.UserService = userServiceMock.Object;

            _bot.RecieveMessage(message).Wait();

            userServiceMock.Verify(mock => mock.GetUser(It.IsAny<long>()), Times.Once);
            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
            userServiceMock.Verify(mock => mock.BanUser(It.IsAny<long>()), Times.Never);
            userServiceMock.Verify(mock => mock.UnbanUser(It.IsAny<long>()), Times.Never);
            userServiceMock.Verify(mock => mock.GetUserList(), Times.Never);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat.Id),
                It.IsAny<string>(), It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<int>(), It.IsAny<IReplyMarkup>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }


        [Fact]
        public void BanFromAdminTest()
        {
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
            var chatRepo1 = _chatMapper.MapToEntity(chat1);
            var chatRepo2 = _chatMapper.MapToEntity(chat2);
            var chats = new Core.Model.Chat[] { chatRepo1, chatRepo2 };

            string userName = "TestUser";
            var user1 = new User
            {
                Id = 1,
                Username = userName + "1",
            };
            var user2 = new User
            {
                Id = 2,
                Username = userName + "2",
            };
            var userRepo1 = _userMapper.MapToEntity(user1);
            userRepo1.IsAdmin = true;
            var userRepo2 = _userMapper.MapToEntity(user2);
            userRepo2.IsBlocked = true;
            var users = new Core.Model.User[] { userRepo1, userRepo2 };
            
            var message = new Message
            {
                Chat = chat1,
                From = user1,
                Text = "/ban testuser3"
            };

            _fixture.MockBotClient.Reset();

            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(s => s.GetUser(It.Is<long>(_ => _ == user1.Id)))
                .ReturnsAsync(userRepo1);
            userServiceMock.Setup(s => s.GetUser(It.Is<string>(_ => _ == user2.GetUserName().ToLower())))
                .ReturnsAsync(userRepo2);
            _bot.UserService = userServiceMock.Object;

            var chatServiceMock = new Mock<IChatService>();
            chatServiceMock.Setup(s => s.GetChat(It.Is<long>(_ => _ == chat1.Id)))
                .ReturnsAsync(chatRepo1);
            chatServiceMock.SetupSequence(s => s.GetChatList())
                .ReturnsAsync(new Core.Model.Chat[0])
                .ReturnsAsync(new Core.Model.Chat[] { chatRepo1, chatRepo2 });
            _bot.ChatService = chatServiceMock.Object;

            _fixture.MockBotClient.SetupSequence(s => s.GetChatAsync(It.IsAny<ChatId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(chat1)
                .ReturnsAsync(chat2);

            _bot.RecieveMessage(message).Wait();

            message.Text = "/ban testuser2";
            _bot.RecieveMessage(message).Wait();

            userRepo2.IsCoordinator = true;
            userRepo2.IsBlocked = false;
            _bot.RecieveMessage(message).Wait();

            userRepo2.IsCoordinator = false;
            userRepo2.IsBot = true;
            _bot.RecieveMessage(message).Wait();

            userRepo2.IsBot = false;
            _bot.RecieveMessage(message).Wait();

            _bot.RecieveMessage(message).Wait();

            userServiceMock.Verify(mock => mock.GetUser(It.IsAny<long>()), Times.Exactly(6));
            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
            userServiceMock.Verify(mock => mock.BanUser(It.IsAny<long>()), Times.Exactly(2));
            userServiceMock.Verify(mock => mock.UnbanUser(It.IsAny<long>()), Times.Never);
            userServiceMock.Verify(mock => mock.GetUserList(), Times.Never);
            chatServiceMock.Verify(mock => mock.GetChatList(), Times.Exactly(2));
            _fixture.MockBotClient.Verify(mock => mock.KickChatMemberAsync(
                It.Is<ChatId>(_ => _.Identifier == chatRepo1.Id || _.Identifier == chatRepo2.Id), 
                It.Is<int>(_ => _ == userRepo2.Id), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), 
                Times.Exactly(2));
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id),
                It.Is<string>(_ => _ == Messages.UserNotFound), It.IsAny<ParseMode>(), It.IsAny<bool>(), 
                It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<IReplyMarkup>(), It.IsAny<CancellationToken>()),
                Times.Once);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id),
                It.Is<string>(_ => _ == Messages.UserBlocked), It.IsAny<ParseMode>(), It.IsAny<bool>(), 
                It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<IReplyMarkup>(), It.IsAny<CancellationToken>()),
                Times.Once);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id),
                It.Is<string>(_ => _ == Messages.YouCantBanCoordinator), It.IsAny<ParseMode>(), It.IsAny<bool>(), 
                It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<IReplyMarkup>(), It.IsAny<CancellationToken>()),
                Times.Once);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id),
                It.Is<string>(_ => _ == Messages.YouCantBanBot), It.IsAny<ParseMode>(), It.IsAny<bool>(), 
                It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<IReplyMarkup>(), It.IsAny<CancellationToken>()),
                Times.Once);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id),
                It.Is<string>(_ => _ == Messages.NoAnyChats), It.IsAny<ParseMode>(), It.IsAny<bool>(), 
                It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<IReplyMarkup>(), It.IsAny<CancellationToken>()),
                Times.Once);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id),
                It.Is<string>(_ => _.Contains("deleted from chat")), It.IsAny<ParseMode>(), It.IsAny<bool>(),
                It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<IReplyMarkup>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public void BanAllFromNonAdminTest()
        {
            string userName = "TestUser";
            var user1 = new User
            {
                Id = 1,
                Username = userName + "1",
            };
            var userRepo = _userMapper.MapToEntity(user1);
            var chat = new Chat
            {
                Id = 1,
                Type = ChatType.Private
            };
            var message = new Message
            {
                Chat = chat,
                From = user1,
                Text = "/banall"
            };

            _fixture.MockBotClient.Reset();

            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(s => s.GetUser(It.Is<long>(_ => _ == user1.Id)))
                .ReturnsAsync(userRepo);
            _bot.UserService = userServiceMock.Object;

            _bot.RecieveMessage(message).Wait();

            userServiceMock.Verify(mock => mock.GetUser(It.IsAny<long>()), Times.Once);
            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
            userServiceMock.Verify(mock => mock.BanUser(It.IsAny<long>()), Times.Never);
            userServiceMock.Verify(mock => mock.UnbanUser(It.IsAny<long>()), Times.Never);
            userServiceMock.Verify(mock => mock.GetUserList(), Times.Never);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat.Id),
                It.IsAny<string>(), It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<int>(), It.IsAny<IReplyMarkup>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }


        [Fact]
        public void BanAllFromAdminTest()
        {
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
            var chatRepo1 = _chatMapper.MapToEntity(chat1);
            var chatRepo2 = _chatMapper.MapToEntity(chat2);
            var chats = new Core.Model.Chat[] { chatRepo1, chatRepo2 };

            string userName = "TestUser";
            var user1 = new User
            {
                Id = 1,
                Username = userName + "1",
            };
            var user2 = new User
            {
                Id = 2,
                Username = userName + "2",
            };
            var userRepo1 = _userMapper.MapToEntity(user1);
            userRepo1.IsAdmin = true;
            var userRepo2 = _userMapper.MapToEntity(user2);
            var users = new Core.Model.User[] { userRepo1, userRepo2 };

            var chatMember1 = new ChatMember { User = user1 };
            var chatMember2 = new ChatMember { User = user2 };

            var message = new Message
            {
                Chat = chat1,
                From = user1,
                Text = "/banall"
            };
            

            _fixture.MockBotClient.Reset();

            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(s => s.GetUser(It.Is<long>(_ => _ == user1.Id)))
                .ReturnsAsync(userRepo1);
            userServiceMock.Setup(s => s.GetUser(It.Is<string>(_ => _ == user2.GetUserName().ToLower())))
                .ReturnsAsync(userRepo2);
            userServiceMock.Setup(s => s.GetUserList())
                .ReturnsAsync(users);
            _bot.UserService = userServiceMock.Object;

            var chatServiceMock = new Mock<IChatService>();
            chatServiceMock.Setup(s => s.GetChat(It.Is<long>(_ => _ == chat1.Id)))
                .ReturnsAsync(chatRepo1);
            chatServiceMock.Setup(s => s.GetChat(It.Is<long>(_ => _ == chat2.Id)))
                .ReturnsAsync(chatRepo2);
            chatServiceMock.Setup(s => s.GetChatList())
                .ReturnsAsync(chats);
            _bot.ChatService = chatServiceMock.Object;

            _fixture.MockBotClient.Setup(s => s.GetChatAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(chat1);
            _fixture.MockBotClient.Setup(s => s.GetChatAsync(It.Is<ChatId>(_ => _.Identifier == chat2.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(chat2);
            _fixture.MockBotClient.Setup(s => s.GetChatMemberAsync(It.IsAny<ChatId>(), It.Is<int>(_ => _ == user1.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(chatMember1);
            _fixture.MockBotClient.Setup(s => s.GetChatMemberAsync(It.IsAny<ChatId>(), It.Is<int>(_ => _ == user2.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(chatMember2);

            userRepo2.IsBlocked = true;
            _bot.RecieveMessage(message).Wait();
            _fixture.MockBotClient.Verify(mock => mock.KickChatMemberAsync(
                It.Is<ChatId>(_ => _.Identifier == chatRepo1.Id || _.Identifier == chatRepo2.Id),
                It.Is<int>(_ => _ == userRepo1.Id), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
                Times.Exactly(2));
            userServiceMock.Verify(mock => mock.BanUser(It.Is<long>(_ => _ == user1.Id)), Times.Exactly(2));
            _fixture.MockBotClient.Verify(mock => mock.KickChatMemberAsync(
                It.Is<ChatId>(_ => _.Identifier == chatRepo1.Id || _.Identifier == chatRepo2.Id),
                It.Is<int>(_ => _ == userRepo2.Id), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
                Times.Never);
            userServiceMock.Verify(mock => mock.BanUser(It.Is<long>(_ => _ == user2.Id)), Times.Never);

            userRepo2.IsCoordinator = true;
            userRepo2.IsBlocked = false;
            _bot.RecieveMessage(message).Wait();
            _fixture.MockBotClient.Verify(mock => mock.KickChatMemberAsync(
                It.Is<ChatId>(_ => _.Identifier == chatRepo1.Id || _.Identifier == chatRepo2.Id),
                It.Is<int>(_ => _ == userRepo1.Id), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
                Times.Exactly(4));
            userServiceMock.Verify(mock => mock.BanUser(It.Is<long>(_ => _ == user1.Id)), Times.Exactly(4));
            _fixture.MockBotClient.Verify(mock => mock.KickChatMemberAsync(
                It.Is<ChatId>(_ => _.Identifier == chatRepo1.Id || _.Identifier == chatRepo2.Id),
                It.Is<int>(_ => _ == userRepo2.Id), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
                Times.Never);
            userServiceMock.Verify(mock => mock.BanUser(It.Is<long>(_ => _ == user2.Id)), Times.Never);

            userRepo2.IsCoordinator = false;
            userRepo2.IsBot = true;
            userRepo2.IsBlocked = false;
            userRepo1.IsBlocked = false;
            _bot.RecieveMessage(message).Wait();
            _fixture.MockBotClient.Verify(mock => mock.KickChatMemberAsync(
                It.Is<ChatId>(_ => _.Identifier == chatRepo1.Id || _.Identifier == chatRepo2.Id),
                It.Is<int>(_ => _ == userRepo1.Id), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
                Times.Exactly(6));
            userServiceMock.Verify(mock => mock.BanUser(It.Is<long>(_ => _ == user1.Id)), Times.Exactly(6));
            _fixture.MockBotClient.Verify(mock => mock.KickChatMemberAsync(
                It.Is<ChatId>(_ => _.Identifier == chatRepo1.Id || _.Identifier == chatRepo2.Id),
                It.Is<int>(_ => _ == userRepo2.Id), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
                Times.Never);
            userServiceMock.Verify(mock => mock.BanUser(It.Is<long>(_ => _ == user2.Id)), Times.Never);

            userRepo2.IsBot = false;
            userRepo2.IsBlocked = false;
            userRepo1.IsBlocked = false;
            _bot.RecieveMessage(message).Wait();
            _fixture.MockBotClient.Verify(mock => mock.KickChatMemberAsync(
                It.Is<ChatId>(_ => _.Identifier == chatRepo1.Id || _.Identifier == chatRepo2.Id),
                It.Is<int>(_ => _ == userRepo1.Id), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
                Times.Exactly(8));
            userServiceMock.Verify(mock => mock.BanUser(It.Is<long>(_ => _ == user1.Id)), Times.Exactly(8));
            _fixture.MockBotClient.Verify(mock => mock.KickChatMemberAsync(
                It.Is<ChatId>(_ => _.Identifier == chatRepo1.Id || _.Identifier == chatRepo2.Id),
                It.Is<int>(_ => _ == userRepo2.Id), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
                Times.Exactly(2));
            userServiceMock.Verify(mock => mock.BanUser(It.Is<long>(_ => _ == user2.Id)), Times.Exactly(2));

            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
            userServiceMock.Verify(mock => mock.UnbanUser(It.IsAny<long>()), Times.Never);
            userServiceMock.Verify(mock => mock.GetUserList(), Times.Exactly(4));
            chatServiceMock.Verify(mock => mock.GetChatList(), Times.Exactly(4));
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id),
                It.IsAny<string>(), It.IsAny<ParseMode>(), It.IsAny<bool>(),
                It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<IReplyMarkup>(), It.IsAny<CancellationToken>()),
                Times.Exactly(4));
        }
    }
}
