﻿using Moq;
using System.Threading;
using Telegram.Altayskaya97.Model.Middleware;
using Telegram.Altayskaya97.Service.Interface;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Xunit;

namespace Telegram.Altayskaya97.Test.Bot
{
    public class PublicMessagesTests : IClassFixture<BotFixture>
    {
        private BotFixture _fixture = null;
        private Altayskaya97.Bot.Bot _bot = null;

        public PublicMessagesTests(BotFixture fixture)
        {
            _fixture = fixture;
            _bot = fixture.Bot;
        }

        [Fact]
        public void HelpGroupWhenGroupNotExistTest()
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
                Type = ChatType.Group
            };
            var chatRepo = new Core.Model.Chat
            {
                Id = 1,
                ChatType = Core.Model.ChatType.Public
            };
            var message = new Message
            {
                Chat = chat,
                From = user,
                Text = "/help"
            };

            _fixture.MockBotClient.Reset();

            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(s => s.GetUser(It.Is<string>(_ => _ == userName)))
                .ReturnsAsync(_fixture.UserMapper.MapToEntity(user));
            userServiceMock.Setup(s => s.GetUser(It.Is<long>(_ => _ == user.Id)))
                .ReturnsAsync(_fixture.UserMapper.MapToEntity(user));
            var chatServiceMock = new Mock<IChatService>(); 
            chatServiceMock.SetupSequence(s => s.GetChat(It.Is<long>(_ => _ == chat.Id)))
                .ReturnsAsync(default(Core.Model.Chat))
                .ReturnsAsync(chatRepo);
            _bot.UserService = userServiceMock.Object;
            _bot.ChatService = chatServiceMock.Object;

            _bot.RecieveMessage(message).Wait();

            userServiceMock.Verify(mock => mock.GetUser(It.IsAny<long>()), Times.Once);
            chatServiceMock.Verify(mock => 
                mock.AddChat(It.Is<Core.Model.Chat>(c => c.Id == chat.Id)), Times.Once);
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

        [Fact]
        public void HelpGroupWhenGroupExistTest()
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
                Type = ChatType.Group
            };
            var chatRepo = new Core.Model.Chat
            {
                Id = 1,
                ChatType = Core.Model.ChatType.Public
            };
            var message = new Message
            {
                Chat = chat,
                From = user,
                Text = "/help"
            };

            _fixture.MockBotClient.Reset();

            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(s => s.GetUser(It.Is<string>(_ => _ == userName)))
                .ReturnsAsync(_fixture.UserMapper.MapToEntity(user));
            userServiceMock.Setup(s => s.GetUser(It.Is<long>(_ => _ == user.Id)))
                .ReturnsAsync(_fixture.UserMapper.MapToEntity(user));
            var chatServiceMock = new Mock<IChatService>();
            chatServiceMock.Setup(s => s.GetChat(It.Is<long>(_ => _ == chat.Id)))
                .ReturnsAsync(chatRepo);
            _bot.UserService = userServiceMock.Object;
            _bot.ChatService = chatServiceMock.Object;
            
            _bot.RecieveMessage(message).Wait();

            userServiceMock.Verify(mock => mock.GetUser(It.IsAny<long>()), Times.Once);
            userServiceMock.Verify(mock => mock.UpdateUser(It.Is<Core.Model.User>(_ => _.Id == user.Id)), Times.Once);
            chatServiceMock.Verify(mock =>
                mock.AddChat(It.Is<Core.Model.Chat>(ct => ct.Id == chat.Id)), Times.Never);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
                 It.Is<ChatId>(_ => _.Identifier == chat.Id),
                 It.IsAny<string>(),
                 It.Is<ParseMode>(_ => _ == ParseMode.Html),
                 It.IsAny<bool>(),
                 It.IsAny<bool>(),
                 It.IsAny<int>(),
                 It.IsAny<IReplyMarkup>(),
                 It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void HelpGroupWhenGroupIsAdminTest()
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
                Type = ChatType.Group
            };
            var chatRepo = new Core.Model.Chat
            {
                Id = 1,
                ChatType = Core.Model.ChatType.Admin
            };
            var message = new Message
            {
                Chat = chat,
                From = user,
                Text = "/help"
            };

            _fixture.MockBotClient.Reset();

            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(s => s.GetUser(It.Is<string>(_ => _ == userName)))
                .ReturnsAsync(default(Core.Model.User));
            var chatServiceMock = new Mock<IChatService>();
            chatServiceMock.Setup(s => s.GetChat(It.Is<long>(_ => _ == chat.Id)))
                .ReturnsAsync(chatRepo);
            _bot.UserService = userServiceMock.Object;
            _bot.ChatService = chatServiceMock.Object;

            _bot.RecieveMessage(message).Wait();

            userServiceMock.Verify(mock => mock.GetUser(It.Is<long>(_ => _ == user.Id)), Times.Once);
            userServiceMock.Verify(mock => mock.AddUser(It.Is<Core.Model.User>(u => u.Id == user.Id)), Times.Once);
            userServiceMock.Verify(mock => mock.UpdateUser(It.Is<Core.Model.User>(_ => _.Id == user.Id)), Times.Once);
            chatServiceMock.Verify(mock =>
                mock.AddChat(It.Is<Core.Model.Chat>(_ => _.Id == chat.Id)), Times.Never);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
                 It.Is<ChatId>(_ => _.Identifier == chat.Id),
                 It.IsAny<string>(),
                 It.Is<ParseMode>(_ => _ == ParseMode.Html),
                 It.IsAny<bool>(),
                 It.IsAny<bool>(),
                 It.IsAny<int>(),
                 It.IsAny<IReplyMarkup>(),
                 It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void HelpGroupWhenGroupIsAdminButUserNotExistTest()
        {
            string userName = "TestUser";
            var user = new User
            {
                Id = 1,
                Username = userName,
            };
            var userRepo = _fixture.UserMapper.MapToEntity(user);
            userRepo.Name = user.Username;
            var chat = new Chat
            {
                Id = 1,
                Type = ChatType.Group
            };
            var chatRepo = new Core.Model.Chat
            {
                Id = 1,
                ChatType = Core.Model.ChatType.Admin
            };
            var message = new Message
            {
                Chat = chat,
                From = user,
                Text = "/help"
            };

            _fixture.MockBotClient.Reset();

            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(s => s.GetUser(It.Is<long>(_ => _ == user.Id)))
                .ReturnsAsync(userRepo);
            var chatServiceMock = new Mock<IChatService>();
            chatServiceMock.Setup(s => s.GetChat(It.Is<long>(_ => _ == chat.Id)))
                .ReturnsAsync(chatRepo);
            _bot.UserService = userServiceMock.Object;
            _bot.ChatService = chatServiceMock.Object;

            _bot.RecieveMessage(message).Wait();

            userServiceMock.Verify(mock => mock.GetUser(It.Is<long>(_ => _ == user.Id)), Times.Once);
            userServiceMock.Verify(mock => mock.AddUser(It.Is<Core.Model.User>(u => u.Id == user.Id)), Times.Never);
            chatServiceMock.Verify(mock =>
                mock.AddChat(It.Is<Core.Model.Chat>(_ => _.Id == chat.Id)), Times.Never);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
                  It.Is<ChatId>(_ => _.Identifier == chat.Id),
                 It.IsAny<string>(),
                 It.Is<ParseMode>(_ => _ == ParseMode.Html),
                 It.IsAny<bool>(),
                 It.IsAny<bool>(),
                 It.IsAny<int>(),
                 It.IsAny<IReplyMarkup>(),
                 It.IsAny<CancellationToken>()), Times.Once);
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
            var userRepo = _fixture.UserMapper.MapToEntity(user);
            userRepo.Name = user.Username;
            var chat = new Chat
            {
                Id = 1,
                Type = ChatType.Group
            };
            var chatRepo = new Core.Model.Chat
            {
                Id = 1,
                ChatType = Core.Model.ChatType.Admin
            };
            var message = new Message
            {
                Chat = chat,
                From = user,
                Text = "/hlep"
            };

            _fixture.MockBotClient.Reset();

            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(s => s.GetUser(It.Is<long>(_ => _ == user.Id)))
                .ReturnsAsync(userRepo);
            var chatServiceMock = new Mock<IChatService>();
            chatServiceMock.Setup(s => s.GetChat(It.Is<long>(_ => _ == chat.Id)))
                .ReturnsAsync(chatRepo);
            _bot.UserService = userServiceMock.Object;
            _bot.ChatService = chatServiceMock.Object;

            _bot.RecieveMessage(message).Wait();

            userServiceMock.Verify(mock => mock.GetUser(It.Is<long>(_ => _ == user.Id)), Times.Once);
            userServiceMock.Verify(mock => mock.AddUser(It.Is<Core.Model.User>(u => u.Id == user.Id)), Times.Never);
            chatServiceMock.Verify(mock =>
                mock.AddChat(It.Is<Core.Model.Chat>(chat => chat.Id == chat.Id)), Times.Never);
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
    }
}
