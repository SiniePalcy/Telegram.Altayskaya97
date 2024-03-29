﻿using Moq;
using System.Threading;
using Telegram.Altayskaya97.Core.Constant;
using Telegram.Altayskaya97.Service.Interface;
using Telegram.Altayskaya97.Bot.Helpers;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Xunit;
using System;

namespace Telegram.Altayskaya97.Test.Integration
{
    public class BanTests : IClassFixture<BotFixture>
    {
        private readonly BotFixture _fixture = null;
        private readonly Altayskaya97.Bot.Bot _bot = null;

        public BanTests(BotFixture fixture)
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
            var userRepo = _fixture.UserMapper.MapToEntity(user1);
            var chat = new Chat
            {
                Id = 1,
                Type = ChatType.Private
            };
            var chatRepo = new Altayskaya97.Core.Model.Chat { Id = chat.Id };
            var message = new Message
            {
                Chat = chat,
                From = user1,
                Text = "/ban testuser2"
            };

            _fixture.MockBotClient.Reset();

            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == user1.Id)))
                .ReturnsAsync(userRepo);
            _bot.UserService = userServiceMock.Object;

            var chatServiceMock = new Mock<IChatService>();
            chatServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == chat.Id)))
                .ReturnsAsync(chatRepo);
            _bot.ChatService = chatServiceMock.Object;

            _bot.RecieveMessage(message).Wait();

            userServiceMock.Verify(mock => mock.Get(It.IsAny<long>()), Times.Once);
            chatServiceMock.Verify(mock => mock.Get(It.IsAny<long>()), Times.Once);
            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
            userServiceMock.Verify(mock => mock.GetList(), Times.Never);
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
            var chatRepo1 = _fixture.ChatMapper.MapToEntity(chat1);
            var chatRepo2 = _fixture.ChatMapper.MapToEntity(chat2);
            var chats = new Altayskaya97.Core.Model.Chat[] { chatRepo1, chatRepo2 };

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
            var userRepo1 = _fixture.UserMapper.MapToEntity(user1);
            userRepo1.IsAdmin = true;
            userRepo1.Type = Altayskaya97.Core.Model.UserType.Admin;
            var userRepo2 = _fixture.UserMapper.MapToEntity(user2);
            var users = new Altayskaya97.Core.Model.User[] { userRepo1, userRepo2 };
            
            var chatMember = new ChatMember { Status = ChatMemberStatus.Member };

            var message = new Message
            {
                Chat = chat1,
                From = user1,
                Text = "/ban testuser3"
            };

            _fixture.MockBotClient.Reset();

            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == user1.Id)))
                .ReturnsAsync(userRepo1);
            userServiceMock.Setup(s => s.IsAdmin(It.Is<long>(_ => _ == user1.Id)))
                .ReturnsAsync(true);
            userServiceMock.Setup(s => s.GetByName(It.Is<string>(_ => _ == user2.GetUserName().ToLower())))
                .ReturnsAsync(userRepo2);
            userServiceMock.Setup(s => s.GetByIdOrName(It.Is<string>(_ => _ == user2.GetUserName().ToLower())))
                .ReturnsAsync(userRepo2);
            _bot.UserService = userServiceMock.Object;

            var chatServiceMock = new Mock<IChatService>();
            chatServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == chat1.Id)))
                .ReturnsAsync(chatRepo1);
            chatServiceMock.SetupSequence(s => s.GetList())
                .ReturnsAsync(new Altayskaya97.Core.Model.Chat[0])
                .ReturnsAsync(new Altayskaya97.Core.Model.Chat[] { chatRepo1, chatRepo2 });
            _bot.ChatService = chatServiceMock.Object;

            _fixture.MockBotClient.SetupSequence(s => s.GetChatAsync(It.IsAny<ChatId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(chat1)
                .ReturnsAsync(chat2);
            _fixture.MockBotClient.Setup(s => s.GetChatMemberAsync(It.IsAny<ChatId>(),
                It.Is<int>(_ => _ == user2.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(chatMember);
            _fixture.MockBotClient.Setup(b => b.GetChatAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id),
                It.IsAny<CancellationToken>())).ReturnsAsync(chat1);
            _fixture.MockBotClient.Setup(b => b.GetChatAsync(It.Is<ChatId>(_ => _.Identifier == chat2.Id),
                It.IsAny<CancellationToken>())).ReturnsAsync(chat2);

            _bot.RecieveMessage(message).Wait();

            message.Text = "/ban testuser2";
            _bot.RecieveMessage(message).Wait();

            userRepo2.Type = Altayskaya97.Core.Model.UserType.Coordinator;
            _bot.RecieveMessage(message).Wait();

            userRepo2.Type = Altayskaya97.Core.Model.UserType.Bot;
            _bot.RecieveMessage(message).Wait();

            userRepo2.Type = Altayskaya97.Core.Model.UserType.Member;
            _bot.RecieveMessage(message).Wait();

            userServiceMock.Verify(mock => mock.Get(It.IsAny<long>()), Times.Exactly(5));
            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
            userServiceMock.Verify(mock => mock.GetList(), Times.Never);
            chatServiceMock.Verify(mock => mock.GetList(), Times.Exactly(2));
            _fixture.MockBotClient.Verify(mock => mock.KickChatMemberAsync(
                It.Is<ChatId>(_ => _.Identifier == chatRepo1.Id || _.Identifier == chatRepo2.Id), 
                It.Is<int>(_ => _ == userRepo2.Id), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), 
                Times.Exactly(2));
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
                It.Is<string>(_ => _.Contains("kicked from chat")), It.IsAny<ParseMode>(), It.IsAny<bool>(),
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
            var userRepo = _fixture.UserMapper.MapToEntity(user1);
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
            userServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == user1.Id)))
                .ReturnsAsync(userRepo);
            _bot.UserService = userServiceMock.Object;

            var chatServiceMock = new Mock<IChatService>();
            chatServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == chat.Id)))
                .ReturnsAsync(new Altayskaya97.Core.Model.Chat { Id = 1 });
            _bot.ChatService = chatServiceMock.Object;

            _bot.RecieveMessage(message).Wait();

            chatServiceMock.Verify(mock => mock.Get(It.IsAny<long>()), Times.Once);
            userServiceMock.Verify(mock => mock.Get(It.IsAny<long>()), Times.Once);
            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
            userServiceMock.Verify(mock => mock.GetList(), Times.Never);
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
            var chatRepo1 = _fixture.ChatMapper.MapToEntity(chat1);
            var chatRepo2 = _fixture.ChatMapper.MapToEntity(chat2);
            var chats = new Altayskaya97.Core.Model.Chat[] { chatRepo1, chatRepo2 };

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
            var userRepo1 = _fixture.UserMapper.MapToEntity(user1);
            userRepo1.IsAdmin = true;
            userRepo1.Type = Altayskaya97.Core.Model.UserType.Admin;
            var userRepo2 = _fixture.UserMapper.MapToEntity(user2);
            var users = new Altayskaya97.Core.Model.User[] { userRepo1, userRepo2 };

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
            userServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == user1.Id)))
                .ReturnsAsync(userRepo1);
            userServiceMock.Setup(s => s.IsAdmin(It.Is<long>(_ => _ == user1.Id)))
                .ReturnsAsync(true);
            userServiceMock.Setup(s => s.GetByName(It.Is<string>(_ => _ == user2.GetUserName().ToLower())))
                .ReturnsAsync(userRepo2);
            userServiceMock.Setup(s => s.GetByIdOrName(It.Is<string>(_ => _ == user1.Id.ToString())))
                .ReturnsAsync(userRepo1);
            userServiceMock.Setup(s => s.GetByIdOrName(It.Is<string>(_ => _ == user2.Id.ToString())))
                .ReturnsAsync(userRepo2);
            userServiceMock.Setup(s => s.GetList())
                .ReturnsAsync(users);
            _bot.UserService = userServiceMock.Object;

            var chatServiceMock = new Mock<IChatService>();
            chatServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == chat1.Id)))
                .ReturnsAsync(chatRepo1);
            chatServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == chat2.Id)))
                .ReturnsAsync(chatRepo2);
            chatServiceMock.Setup(s => s.GetList())
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

            _bot.RecieveMessage(message).Wait();
            _fixture.MockBotClient.Verify(mock => mock.KickChatMemberAsync(
                It.Is<ChatId>(_ => _.Identifier == chatRepo1.Id || _.Identifier == chatRepo2.Id),
                It.Is<int>(_ => _ == userRepo1.Id), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
                Times.Exactly(2));
            _fixture.MockBotClient.Verify(mock => mock.KickChatMemberAsync(
                It.Is<ChatId>(_ => _.Identifier == chatRepo1.Id || _.Identifier == chatRepo2.Id),
                It.Is<int>(_ => _ == userRepo2.Id), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
                Times.Exactly(2));

            userRepo2.Type = Altayskaya97.Core.Model.UserType.Coordinator;
            _bot.RecieveMessage(message).Wait();
            _fixture.MockBotClient.Verify(mock => mock.KickChatMemberAsync(
                It.Is<ChatId>(_ => _.Identifier == chatRepo1.Id || _.Identifier == chatRepo2.Id),
                It.Is<int>(_ => _ == userRepo1.Id), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
                Times.Exactly(4));
            _fixture.MockBotClient.Verify(mock => mock.KickChatMemberAsync(
                It.Is<ChatId>(_ => _.Identifier == chatRepo1.Id || _.Identifier == chatRepo2.Id),
                It.Is<int>(_ => _ == userRepo2.Id), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
                Times.Exactly(2));

            userRepo2.Type = Altayskaya97.Core.Model.UserType.Bot;
            _bot.RecieveMessage(message).Wait();
            _fixture.MockBotClient.Verify(mock => mock.KickChatMemberAsync(
                It.Is<ChatId>(_ => _.Identifier == chatRepo1.Id || _.Identifier == chatRepo2.Id),
                It.Is<int>(_ => _ == userRepo1.Id), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
                Times.Exactly(6));
            _fixture.MockBotClient.Verify(mock => mock.KickChatMemberAsync(
                It.Is<ChatId>(_ => _.Identifier == chatRepo1.Id || _.Identifier == chatRepo2.Id),
                It.Is<int>(_ => _ == userRepo2.Id), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
                Times.Exactly(2));

            userRepo2.Type = Altayskaya97.Core.Model.UserType.Member;
            _bot.RecieveMessage(message).Wait();
            _fixture.MockBotClient.Verify(mock => mock.KickChatMemberAsync(
                It.Is<ChatId>(_ => _.Identifier == chatRepo1.Id || _.Identifier == chatRepo2.Id),
                It.Is<int>(_ => _ == userRepo1.Id), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
                Times.Exactly(8));
            _fixture.MockBotClient.Verify(mock => mock.KickChatMemberAsync(
                It.Is<ChatId>(_ => _.Identifier == chatRepo1.Id || _.Identifier == chatRepo2.Id),
                It.Is<int>(_ => _ == userRepo2.Id), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
                Times.Exactly(4));

            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
            userServiceMock.Verify(mock => mock.GetList(), Times.Exactly(4));
            chatServiceMock.Verify(mock => mock.GetList(), Times.Exactly(6));
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id),
                It.IsAny<string>(), It.IsAny<ParseMode>(), It.IsAny<bool>(),
                It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<IReplyMarkup>(), It.IsAny<CancellationToken>()),
                Times.Exactly(4));
        }
    }
}
