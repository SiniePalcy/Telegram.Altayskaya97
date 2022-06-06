//using Moq;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using Telegram.Altayskaya97.Core;
//using Telegram.Altayskaya97.Core.Constant;
//using Telegram.Altayskaya97.Service.Interface;
//using Telegram.Altayskaya97.Bot.Helpers;
//using Xunit;
//using System;

//namespace Telegram.Altayskaya97.Test.Integration
//{
//    public class ChangeUserTypeTests : IClassFixture<BotFixture>
//    {
//        private readonly BotFixture _fixture = null;
//        private readonly Altayskaya97.Bot.Bot _bot = null;

//        public ChangeUserTypeTests(BotFixture fixture)
//        {
//            _fixture = fixture;
//            _bot = fixture.Bot;
//        }

//        [Fact]
//        public void ChangeUserTypeFromNonAdminTest()
//        {
//            string userName = "TestUser";
//            var user1 = new User
//            {
//                Id = 1,
//                Username = userName + "1",
//            };
//            var userRepo = _fixture.UserMapper.MapToEntity(user1);
//            var chat = new Chat
//            {
//                Id = 1,
//                Type = ChatType.Private
//            };
//            var chatRepo = new Altayskaya97.Core.Model.Chat { Id = chat.Id };
//            var message = new Message
//            {
//                Chat = chat,
//                From = user1,
//                Text = "/changeusertype admin testuser1"
//            };

//            _fixture.MockBotClient.Reset();

//            var userServiceMock = new Mock<IUserService>();
//            userServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == user1.Id)))
//                .ReturnsAsync(userRepo);
//            _bot.UserService = userServiceMock.Object;

//            var chatServiceMock = new Mock<IChatService>();
//            chatServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == chat.Id)))
//                .ReturnsAsync(chatRepo);
//            _bot.ChatService = chatServiceMock.Object;

//            _bot.RecieveMessage(message).Wait();

//            userServiceMock.Verify(mock => mock.Get(It.IsAny<long>()), Times.Once);
//            chatServiceMock.Verify(mock => mock.Get(It.IsAny<long>()), Times.Once);
//            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
//            userServiceMock.Verify(mock => mock.GetList(), Times.Never);
//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat.Id),
//                It.IsAny<string>(), 
//                It.IsAny<ParseMode>(),
//                It.IsAny<IEnumerable<MessageEntity>>(),
//                It.IsAny<bool>(), 
//                It.IsAny<bool>(),
//                It.IsAny<int>(),
//                It.IsAny<bool?>(),
//                It.IsAny<IReplyMarkup>(), 
//                It.IsAny<CancellationToken>()),
//                Times.Never);
//        }


//        [Fact]
//        public void ChangeUserTypeFromAdminWithoutPermission()
//        {
//            var chat1 = new Chat
//            {
//                Id = 1,
//                Type = ChatType.Private
//            };
//            var chatRepo1 = _fixture.ChatMapper.MapToEntity(chat1);

//            string userName = "TestUser";
//            var user1 = new User
//            {
//                Id = 1,
//                Username = userName + "1",
//            };
//            var user2 = new User
//            {
//                Id = 2,
//                Username = userName + "2",
//            };
//            var userRepo1 = _fixture.UserMapper.MapToEntity(user1);
//            userRepo1.IsAdmin = false;
//            userRepo1.Type = Altayskaya97.Core.Model.UserType.Admin;
//            var userRepo2 = _fixture.UserMapper.MapToEntity(user2);
//            var users = new Altayskaya97.Core.Model.User[] { userRepo1, userRepo2 };

//            var chatMember = new ChatMemberMember();

//            var message = new Message
//            {
//                Chat = chat1,
//                From = user1,
//                Text = "/changeusertype admin testuser2"
//            };

//            _fixture.MockBotClient.Reset();

//            var userServiceMock = new Mock<IUserService>();
//            userServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == user1.Id)))
//                .ReturnsAsync(userRepo1);
//            userServiceMock.Setup(s => s.IsAdmin(It.Is<long>(_ => _ == user1.Id)))
//                .ReturnsAsync(false);
//            userServiceMock.Setup(s => s.GetByName(It.Is<string>(_ => _ == user2.GetUserName().ToLower())))
//                .ReturnsAsync(userRepo2);
//            userServiceMock.Setup(s => s.GetByIdOrName(It.Is<string>(_ => _ == user2.GetUserName().ToLower())))
//                .ReturnsAsync(userRepo2);
//            _bot.UserService = userServiceMock.Object;

//            var chatServiceMock = new Mock<IChatService>();
//            chatServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == chat1.Id)))
//                .ReturnsAsync(chatRepo1);
//            _bot.ChatService = chatServiceMock.Object;

//            _fixture.MockBotClient.Setup(s => s.GetChatAsync(It.IsAny<ChatId>(), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(chat1);
//            _fixture.MockBotClient.Setup(s => s.GetChatMemberAsync(It.IsAny<ChatId>(),
//                It.Is<int>(_ => _ == user2.Id), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(chatMember);
//            _fixture.MockBotClient.Setup(b => b.GetChatAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.IsAny<CancellationToken>())).ReturnsAsync(chat1);

//            _bot.RecieveMessage(message).Wait();

//            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
//            userServiceMock.Verify(mock => mock.GetList(), Times.Never);
//            userServiceMock.Verify(mock => mock.Update(It.IsAny<long>(),
//                It.IsAny<Altayskaya97.Core.Model.User>()), Times.Never);
//            userServiceMock.Verify(mock => mock.Update(It.IsAny<Altayskaya97.Core.Model.User>()),
//                Times.Never);
//            chatServiceMock.Verify(mock => mock.GetList(), Times.Never);
//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ == Messages.NoPermissions), 
//                It.IsAny<ParseMode>(),
//                It.IsAny<IEnumerable<MessageEntity>>(),
//                It.IsAny<bool>(),
//                It.IsAny<bool>(), 
//                It.IsAny<int>(),
//                It.IsAny<bool?>(),
//                It.IsAny<IReplyMarkup>(), 
//                It.IsAny<CancellationToken>()),
//                Times.Once);
//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ != Messages.NoPermissions), 
//                It.IsAny<ParseMode>(),
//                It.IsAny<IEnumerable<MessageEntity>>(),
//                It.IsAny<bool>(),
//                It.IsAny<bool>(), 
//                It.IsAny<int>(),
//                It.IsAny<bool?>(),
//                It.IsAny<IReplyMarkup>(), 
//                It.IsAny<CancellationToken>()),
//                Times.Never);
//        }

//        [Fact]
//        public void ChangeUserTypeFromAdminWithPermission()
//        {
//            var chat1 = new Chat
//            {
//                Id = 1,
//                Type = ChatType.Private
//            };
//            var chatRepo1 = _fixture.ChatMapper.MapToEntity(chat1);

//            string userName = "TestUser";
//            var user1 = new User
//            {
//                Id = 1,
//                Username = userName + "1",
//            };
//            var user2 = new User
//            {
//                Id = 2,
//                Username = userName + "2",
//            };
//            var userRepo1 = _fixture.UserMapper.MapToEntity(user1);
//            userRepo1.IsAdmin = false;
//            userRepo1.Type = Altayskaya97.Core.Model.UserType.Admin;
//            var userRepo2 = _fixture.UserMapper.MapToEntity(user2);
//            userRepo2.Type = Altayskaya97.Core.Model.UserType.Member;
//            var users = new Altayskaya97.Core.Model.User[] { userRepo1, userRepo2 };

//            var message = new Message
//            {
//                Chat = chat1,
//                From = user1,
//                Text = "/changeusertype coordinator testuser2"
//            };

//            _fixture.MockBotClient.Reset();

//            var userServiceMock = new Mock<IUserService>();
//            userServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == user1.Id)))
//                .ReturnsAsync(userRepo1);
//            userServiceMock.Setup(s => s.IsAdmin(It.Is<long>(_ => _ == user1.Id)))
//                .ReturnsAsync(true);
//            userServiceMock.Setup(s => s.GetByName(It.Is<string>(_ => _ == user2.GetUserName().ToLower())))
//                .ReturnsAsync(userRepo2);
//            userServiceMock.Setup(s => s.GetByIdOrName(It.Is<string>(_ => _ == user2.GetUserName().ToLower())))
//                .ReturnsAsync(userRepo2);
//            _bot.UserService = userServiceMock.Object;

//            var chatServiceMock = new Mock<IChatService>();
//            chatServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == chat1.Id)))
//                .ReturnsAsync(chatRepo1);
//            _bot.ChatService = chatServiceMock.Object;

//            _fixture.MockBotClient.Setup(s => s.GetChatAsync(It.Is<ChatId>(
//                _ => _.Identifier == chat1.Id), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(chat1);

//            _bot.RecieveMessage(message).Wait();

//            message.Text = message.Text.Replace("coordinator", "Admin");
//            _bot.RecieveMessage(message).Wait();

//            message.Text = message.Text.Replace("testuser2", "testuser3");
//            _bot.RecieveMessage(message).Wait();

//            message.Text = message.Text.Replace("Admin", "incorrecttype");
//            _bot.RecieveMessage(message).Wait();

//            chatServiceMock.Verify(mock => mock.GetList(), Times.Never);
//            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()),
//                Times.Never);
//            userServiceMock.Verify(mock => mock.GetList(), Times.Never);
//            userServiceMock.Verify(mock => mock.Update(It.IsAny<long>(),
//                It.IsAny<Altayskaya97.Core.Model.User>()), Times.Never);
//            userServiceMock.Verify(mock => mock.Update(
//                It.Is<Altayskaya97.Core.Model.User>(_ => _.Id == user2.Id)),
//                Times.Once);
            
//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ == Messages.NoPermissions), 
//                It.IsAny<ParseMode>(),
//                It.IsAny<IEnumerable<MessageEntity>>(),
//                It.IsAny<bool>(),
//                It.IsAny<bool>(), 
//                It.IsAny<int>(),
//                It.IsAny<bool?>(),
//                It.IsAny<IReplyMarkup>(), 
//                It.IsAny<CancellationToken>()),
//                Times.Never);
//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ == "Only coordinators can set Coordinator type"),
//                It.IsAny<ParseMode>(),
//                It.IsAny<IEnumerable<MessageEntity>>(),
//                It.IsAny<bool>(), 
//                It.IsAny<bool>(), 
//                It.IsAny<int>(),
//                It.IsAny<bool?>(),
//                It.IsAny<IReplyMarkup>(), 
//                It.IsAny<CancellationToken>()),  
//                Times.Once);
//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ == Messages.UserNotFound), 
//                It.IsAny<ParseMode>(),
//                It.IsAny<IEnumerable<MessageEntity>>(),
//                It.IsAny<bool>(), 
//                It.IsAny<bool>(), 
//                It.IsAny<int>(),
//                It.IsAny<bool?>(),
//                It.IsAny<IReplyMarkup>(),
//                It.IsAny<CancellationToken>()), 
//                Times.Once);
//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _.Contains("Changed type of user")), 
//                It.IsAny<ParseMode>(),
//                It.IsAny<IEnumerable<MessageEntity>>(),
//                It.IsAny<bool>(), 
//                It.IsAny<bool>(), 
//                It.IsAny<int>(),
//                It.IsAny<bool?>(),
//                It.IsAny<IReplyMarkup>(),
//                It.IsAny<CancellationToken>()), 
//                Times.Once);
//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _.Contains("Incorrect user type")), 
//                It.IsAny<ParseMode>(),
//                It.IsAny<IEnumerable<MessageEntity>>(),
//                It.IsAny<bool>(), 
//                It.IsAny<bool>(), 
//                It.IsAny<int>(),
//                It.IsAny<bool?>(),
//                It.IsAny<IReplyMarkup>(),
//                It.IsAny<CancellationToken>()), 
//                Times.Once);
//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ != "Only coordinators can set Coordinator type" && _ != Messages.UserNotFound && !_.Contains("Changed type of user") && _ != "Incorrect user type"), 
//                It.IsAny<ParseMode>(),
//                It.IsAny<IEnumerable<MessageEntity>>(),
//                It.IsAny<bool>(),  
//                It.IsAny<bool>(), 
//                It.IsAny<int>(),
//                It.IsAny<bool?>(),
//                It.IsAny<IReplyMarkup>(), 
//                It.IsAny<CancellationToken>()), Times.Never);
//        }
//    }
//}

