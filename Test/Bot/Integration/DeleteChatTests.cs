using Moq;
using System;
using System.Threading;
using Telegram.Altayskaya97.Service;
using Telegram.Altayskaya97.Service.Interface;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Xunit;

namespace Telegram.Altayskaya97.Test.Bot.Integration
{
    public class DeleteChatTests : IClassFixture<BotFixture>
    {
        private readonly BotFixture _fixture = null;
        private readonly Altayskaya97.Bot.Bot _bot = null;

        public DeleteChatTests(BotFixture fixture)
        {
            _fixture = fixture;
            _bot = fixture.Bot;
        }

        [Fact]
        public void DeleteChatUnknownUserTest()
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
                Type = ChatType.Private,
                Title = "TestChat"
            };
            var chatRepo = _fixture.ChatMapper.MapToEntity(chat);
            var message = new Message
            {
                Chat = chat,
                From = user,
                Text = "/deletechat testchat"
            };

            _fixture.MockBotClient.Reset();

            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(s => s.GetUser(It.IsAny<long>()))
                .ReturnsAsync(default(Core.Model.User));
            _bot.UserService = userServiceMock.Object;

            var chatServiceMock = new Mock<IChatService>();
            chatServiceMock.Setup(s => s.GetChat(It.Is<long>(_ => _ == chat.Id)))
                .ReturnsAsync(chatRepo);
            chatServiceMock.Setup(s => s.GetChat(It.Is<string>(_ => _.Trim().ToLower() == chat.Title.Trim().ToLower())))
                .ReturnsAsync(chatRepo);
            _bot.ChatService = chatServiceMock.Object;

            _bot.RecieveMessage(message).Wait();

            message.Text = "/deletechat";
            _bot.RecieveMessage(message).Wait();

            userServiceMock.Verify(mock => mock.GetUser(It.Is<long>(_ => _ == user.Id)), Times.Exactly(1));
            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
            userServiceMock.Verify(mock => mock.GetUserList(), Times.Never);
            chatServiceMock.Verify(mock => mock.GetChat(It.Is<long>(_ => _ == chat.Id)), Times.Exactly(2));
            chatServiceMock.Verify(mock => mock.GetChat(It.Is<string>(_ => _.Trim().ToLower() == chat.Title.Trim().ToLower())), Times.Never);
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
        public void DeleteChatNonAdminTest()
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
                Type = ChatType.Private,
                Title = "TestChat"
            };
            var chatRepo = _fixture.ChatMapper.MapToEntity(chat);
            var message = new Message
            {
                Chat = chat,
                From = user,
                Text = "/deletechat testchat"
            };

            _fixture.MockBotClient.Reset();

            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(s => s.GetUser(It.Is<long>(_ => _ == user.Id)))
                .ReturnsAsync(userRepo);
            _bot.UserService = userServiceMock.Object;

            var chatServiceMock = new Mock<IChatService>();
            chatServiceMock.Setup(s => s.GetChat(It.Is<long>(_ => _ == chat.Id)))
                .ReturnsAsync(chatRepo);
            _bot.ChatService = chatServiceMock.Object;

            _bot.RecieveMessage(message).Wait();

            message.Text = "/deletechat";
            _bot.RecieveMessage(message).Wait();

            userServiceMock.Verify(mock => mock.GetUser(It.Is<long>(_ => _ == user.Id)), Times.Exactly(1));
            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
            userServiceMock.Verify(mock => mock.GetUserList(), Times.Never);
            chatServiceMock.Verify(mock => mock.GetChat(It.Is<long>(_ => _ == chat.Id)), Times.Exactly(2));
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
        public void DeleteChatAdminWithoutPermissionTest()
        {
            string userName = "TestUser";
            var user = new User
            {
                Id = 1,
                Username = userName
            };
            var userRepo = _fixture.UserMapper.MapToEntity(user);
            userRepo.Type = Core.Model.UserType.Admin;
            var chat = new Chat
            {
                Id = 1,
                Type = ChatType.Private,
                Title = "TestChat"
            };
            var chatRepo = _fixture.ChatMapper.MapToEntity(chat);
            var message = new Message
            {
                Chat = chat,
                From = user,
                Text = "/deletechat testchat"
            };

            _fixture.MockBotClient.Reset();

            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(s => s.GetUser(It.Is<long>(_ => _ == user.Id)))
                .ReturnsAsync(userRepo);
            userServiceMock.Setup(s => s.IsAdmin(It.Is<long>(_ => _ == user.Id)))
                .ReturnsAsync(false);
            _bot.UserService = userServiceMock.Object;

            var chatServiceMock = new Mock<IChatService>();
            chatServiceMock.Setup(s => s.GetChat(It.Is<long>(_ => _ == chat.Id)))
                .ReturnsAsync(chatRepo);
            _bot.ChatService = chatServiceMock.Object;

            _fixture.MockBotClient.Setup(s => s.GetChatAsync(It.Is<ChatId>(
                _ => _.Identifier == chat.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(chat);
            
            _bot.RecieveMessage(message).Wait();

            message.Text = "/deletechat";
            _bot.RecieveMessage(message).Wait();

            userServiceMock.Verify(mock => mock.GetUser(It.Is<long>(_ => _ == user.Id)), Times.Once);
            userServiceMock.Verify(mock => mock.IsAdmin(It.Is<long>(_ => _ == user.Id)), Times.Once);
            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
            userServiceMock.Verify(mock => mock.GetUserList(), Times.Never);
            chatServiceMock.Verify(mock => mock.GetChat(It.Is<long>(_ => _ == chat.Id)), Times.Exactly(2));
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
                 It.IsAny<ChatId>(),
                 It.Is<string>(_ => _ == Core.Constant.Messages.NoPermissions),
                 It.IsAny<ParseMode>(),
                 It.IsAny<bool>(),
                 It.IsAny<bool>(),
                 It.IsAny<int>(),
                 It.IsAny<IReplyMarkup>(),
                 It.IsAny<CancellationToken>()), Times.Once);
        }


        [Fact]
        public void DeleteChatAdminWithPermissionTest()
        {
            string userName = "TestUser";
            var user = new User
            {
                Id = 1,
                Username = userName
            };
            var userRepo = _fixture.UserMapper.MapToEntity(user);
            userRepo.Type = Core.Model.UserType.Admin;
            var chat1 = new Chat
            {
                Id = 1,
                Type = ChatType.Private
            };
            var chat2 = new Chat
            {
                Id = 2,
                Type = ChatType.Private,
                Title = "TestChat"
            };
            var chatRepo1 = _fixture.ChatMapper.MapToEntity(chat1);
            var chatRepo2 = _fixture.ChatMapper.MapToEntity(chat2);
            chatRepo2.Title = "TestChat";
            var message = new Message
            {
                Chat = chat1,
                From = user,
                Text = "/deletechat testchat"
            };

            _fixture.MockBotClient.Reset();

            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(s => s.GetUser(It.Is<long>(_ => _ == user.Id)))
                .ReturnsAsync(userRepo);
            userServiceMock.Setup(s => s.IsAdmin(It.Is<long>(_ => _ == user.Id)))
                .ReturnsAsync(true);
            _bot.UserService = userServiceMock.Object;

            var chatServiceMock = new Mock<IChatService>();
            chatServiceMock.Setup(s => s.GetChat(It.Is<long>(_ => _ == chat1.Id)))
                .ReturnsAsync(chatRepo1);
            chatServiceMock.Setup(s => s.GetChat(It.Is<long>(_ => _ == chat2.Id)))
                .ReturnsAsync(chatRepo2);
            chatServiceMock.Setup(s => s.GetChat(It.Is<string>(
                _ => _.Trim().ToLower() == chat2.Title.Trim().ToLower())))
                .ReturnsAsync(chatRepo2);
            _bot.ChatService = chatServiceMock.Object;

            _fixture.MockBotClient.Setup(s => s.GetChatAsync(It.Is<ChatId>(
                _ => _.Identifier == chat1.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(chat1);
            _fixture.MockBotClient.Setup(s => s.GetChatAsync(It.Is<ChatId>(
                _ => _.Identifier == chat2.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(chat2);

            _bot.RecieveMessage(message).Wait();

            message.Text = "/deletechat";
            _bot.RecieveMessage(message).Wait();

            userServiceMock.Verify(mock => mock.GetUser(It.Is<long>(_ => _ == user.Id)), Times.Once);
            userServiceMock.Verify(mock => mock.IsAdmin(It.Is<long>(_ => _ == user.Id)), Times.Once);
            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
            userServiceMock.Verify(mock => mock.GetUserList(), Times.Never);
            chatServiceMock.Verify(mock => mock.GetChat(It.Is<long>(_ => _ == chat1.Id)), Times.Exactly(2));
            chatServiceMock.Verify(mock => mock.GetChat(It.Is<string>(
                _ => _.Trim().ToLower() == chat2.Title.Trim().ToLower())), Times.Once);
            chatServiceMock.Verify(mock => mock.DeleteChat(It.Is<long>(_ => _ == chat2.Id)), Times.Once);
            _fixture.MockBotClient.Verify(mock => mock.LeaveChatAsync(
                It.Is<ChatId>(_ => _.Identifier == chat2.Id), It.IsAny<CancellationToken>()),
                Times.Once);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
                 It.IsAny<ChatId>(),
                 It.Is<string>(_ => _.Contains("deleted")),
                 It.IsAny<ParseMode>(),
                 It.IsAny<bool>(),
                 It.IsAny<bool>(),
                 It.IsAny<int>(),
                 It.IsAny<IReplyMarkup>(),
                 It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void ListInactiveUsersNonAdminTest()
        {
            //DateTime dtNow = DateTime.Parse("");
            string userName = "TestUser";
            var user = new User
            {
                Id = 0,
                Username = userName,
            };
            var userRepo = new Core.Model.User { Id = 0 };

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
                Text = "/inactive"
            };

            _fixture.MockBotClient.Reset();

            var userServiceMock = new Mock<IUserService>();
            userServiceMock.SetupSequence(s => s.GetUser(It.Is<long>(_ => _ == user.Id)))
                .ReturnsAsync(default(Core.Model.User))
                .ReturnsAsync(userRepo);
            userServiceMock.Setup(s => s.IsAdmin(It.Is<long>(_ => _ == user.Id)))
                .ReturnsAsync(false);
            _bot.UserService = userServiceMock.Object;

            var chatServiceMock = new Mock<IChatService>();
            chatServiceMock.Setup(s => s.GetChat(It.Is<long>(_ => _ == chat.Id)))
                .ReturnsAsync(chatRepo);
            _bot.ChatService = chatServiceMock.Object;

            _bot.RecieveMessage(message).Wait();

            _bot.RecieveMessage(message).Wait();

            userServiceMock.Verify(mock => mock.IsAdmin(It.Is<long>(_ => _ == user.Id)), Times.Never);
            userServiceMock.Verify(mock => mock.GetUser(It.Is<long>(_ => _ == user.Id)), Times.Exactly(2));
            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
            userServiceMock.Verify(mock => mock.GetUserList(), Times.Never);
            chatServiceMock.Verify(mock => mock.GetChat(It.Is<long>(_ => _ == chat.Id)), Times.Exactly(2));
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
        public void ListInactiveUsersAdminTest()
        {
            DateTime dt = DateTime.Parse("2020-10-28T10:16:15.242Z");
            DateTime dt0 = dt.AddDays(-2);
            DateTime dt1 = dt.AddHours(-72);
            DateTime dt2 = dt.AddHours(-71);
            DateTime dt3 = dt.AddHours(-73);
            var member = Core.Model.UserType.Member;
            var bot = Core.Model.UserType.Bot;
            var admin = Core.Model.UserType.Admin;
            var coordinator = Core.Model.UserType.Coordinator;

            var user = new User { Id = 0 };
            var user1 = new User { Id = 1 };
            var user2 = new User { Id = 2 };
            var user3 = new User { Id = 3 };
            var userRepo = new Core.Model.User { Id = 0, Name = "user0", Type = Core.Model.UserType.Admin, LastMessageTime = dt0 };
            var userRepo1 = new Core.Model.User { Id = 1, LastMessageTime = dt1, Name = "user1", Type = member };
            var userRepo2 = new Core.Model.User { Id = 2, LastMessageTime = dt2, Name = "user2", Type = member };
            var userRepo3 = new Core.Model.User { Id = 3, LastMessageTime = dt3, Name = "user3", Type = member };
            var userRepo4 = new Core.Model.User { Id = 4, LastMessageTime = null, Name = "user4", Type = member };
            var userRepo5 = new Core.Model.User { Id = 5, LastMessageTime = null, Name = "user5", Type = admin };
            var userRepo6 = new Core.Model.User { Id = 6, LastMessageTime = null, Name = "user6", Type = coordinator };
            var userRepo7 = new Core.Model.User { Id = 7, LastMessageTime = null, Name = "user7", Type = bot };

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
                Text = "/inactive"
            };

            _fixture.MockBotClient.Reset();

            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(s => s.GetUser(It.Is<long>(_ => _ == user.Id)))
                .ReturnsAsync(userRepo);
            userServiceMock.Setup(s => s.IsAdmin(It.Is<long>(_ => _ == user.Id)))
                .ReturnsAsync(true);
            userServiceMock.Setup(s => s.GetUserList())
                .ReturnsAsync(new Core.Model.User[] { userRepo, userRepo1, userRepo2, userRepo3, userRepo4, userRepo5, userRepo6, userRepo7 });
            _bot.UserService = userServiceMock.Object;

            var chatServiceMock = new Mock<IChatService>();
            chatServiceMock.Setup(s => s.GetChat(It.Is<long>(_ => _ == chat.Id)))
                .ReturnsAsync(chatRepo);
            _bot.ChatService = chatServiceMock.Object;

            var dateTimeServiceMock = new Mock<IDateTimeService>();
            dateTimeServiceMock.Setup(s => s.GetDateTimeUTCNow())
                .Returns(dt);
            _bot.DateTimeService = dateTimeServiceMock.Object;
            DateTimeService dtService = new DateTimeService();
            Console.WriteLine(dtService.FormatToString(userRepo1.LastMessageTime));
            Console.WriteLine(dtService.FormatToString(userRepo2.LastMessageTime));
            Console.WriteLine(dtService.FormatToString(userRepo3.LastMessageTime));
            Console.WriteLine(dtService.FormatToString(userRepo4.LastMessageTime));
            Console.WriteLine(dtService.FormatToString(userRepo5.LastMessageTime));
            Console.WriteLine(dtService.FormatToString(userRepo6.LastMessageTime));
            Console.WriteLine(dtService.FormatToString(userRepo7.LastMessageTime));
            dateTimeServiceMock.Setup(s => s.GetDateTimeUTCNow())
                .Returns(dt);
            _bot.DateTimeService = dateTimeServiceMock.Object;



            _fixture.MockBotClient.Setup(s => s.GetChatAsync(It.Is<ChatId>(_ => _.Identifier == chat.Id),
                It.IsAny<CancellationToken>())).ReturnsAsync(chat);

            _bot.RecieveMessage(message).Wait();

            userServiceMock.Verify(mock => mock.IsAdmin(It.Is<long>(_ => _ == user.Id)), Times.Once);
            userServiceMock.Verify(mock => mock.GetUser(It.Is<long>(_ => _ == user.Id)), Times.Once);
            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
            userServiceMock.Verify(mock => mock.GetUserList(), Times.Once);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
                 It.Is<ChatId>(_ => _.Identifier == chatRepo.Id),
                 It.Is<string>(_ => _.Contains("user1") && _.Contains("user3") && _.Contains("user4") && _.Contains("user5") &&
                 !_.Contains("user0") && !_.Contains("user2") && !_.Contains("user6") && !_.Contains("user7")),
                 It.IsAny<ParseMode>(),
                 It.IsAny<bool>(),
                 It.IsAny<bool>(),
                 It.IsAny<int>(),
                 It.IsAny<IReplyMarkup>(),
                 It.IsAny<CancellationToken>()), Times.Once);
        }


    }
}
