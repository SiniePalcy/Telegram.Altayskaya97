//using Moq;
//using System;
//using System.Collections.Generic;
//using System.Threading;
//using Telegram.SafeBot.Service;
//using Telegram.SafeBot.Service.Interface;
//using Telegram.Bot;
//using Telegram.Bot.Types;
//using Telegram.Bot.Types.Enums;
//using Telegram.Bot.Types.ReplyMarkups;
//using Xunit;

//namespace Telegram.SafeBot.Test.Integration
//{
//    public class ListsTests : IClassFixture<BotFixture>
//    {
//        private readonly BotFixture _fixture = null;
//        private readonly SafeBot.Bot.Bot _bot = null;

//        public ListsTests(BotFixture fixture)
//        {
//            _fixture = fixture;
//            _bot = fixture.Bot;
//        }

//        [Fact]
//        public void ListsUnknownUserTest()
//        {
//            string userName = "TestUser";
//            var user = new User
//            {
//                Id = 1,
//                Username = userName,
//            };
//            var chat = new Chat
//            {
//                Id = 1,
//                Type = ChatType.Private
//            };
//            var chatRepo = _fixture.ChatMapper.MapToEntity(chat);
//            var message = new Message
//            {
//                Chat = chat,
//                From = user,
//                Text = "/userlist"
//            };

//            _fixture.MockBotClient.Reset();

//            var userServiceMock = new Mock<IUserService>();
//            userServiceMock.Setup(s => s.Get(It.IsAny<long>()))
//                .ReturnsAsync(default(SafeBot.Core.Model.User));
//            _bot.UserService = userServiceMock.Object;

//            var chatServiceMock = new Mock<IChatService>();
//            chatServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == chat.Id)))
//                .ReturnsAsync(chatRepo);
//            _bot.ChatService = chatServiceMock.Object;

//            _bot.RecieveMessage(message).Wait();

//            message.Text = "/chatlist";
//            _bot.RecieveMessage(message).Wait();

//            userServiceMock.Verify(mock => mock.Get(It.Is<long>(_ => _ == user.Id)), Times.Exactly(2));
//            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
//            userServiceMock.Verify(mock => mock.GetList(), Times.Never);
//            chatServiceMock.Verify(mock => mock.Get(It.Is<long>(_ => _ == chat.Id)), Times.Exactly(2));
//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                 It.IsAny<ChatId>(),
//                 It.IsAny<string>(),
//                 It.IsAny<ParseMode>(),
//                 It.IsAny<IEnumerable<MessageEntity>>(),
//                 It.IsAny<bool>(),
//                 It.IsAny<bool>(),
//                 It.IsAny<int>(),
//                 It.IsAny<bool?>(),
//                 It.IsAny<IReplyMarkup>(),
//                 It.IsAny<CancellationToken>()), Times.Never);
//        }

//        [Fact]
//        public void ListsNonAdminTest()
//        {
//            string userName = "TestUser";
//            var user = new User
//            {
//                Id = 1,
//                Username = userName,
//            };
//            var userRepo = _fixture.UserMapper.MapToEntity(user);
//            var chat = new Chat
//            {
//                Id = 1,
//                Type = ChatType.Private
//            };
//            var chatRepo = _fixture.ChatMapper.MapToEntity(chat);
//            var message = new Message
//            {
//                Chat = chat,
//                From = user,
//                Text = "/userlist"
//            };

//            _fixture.MockBotClient.Reset();

//            var userServiceMock = new Mock<IUserService>();
//            userServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == user.Id)))
//                .ReturnsAsync(userRepo);
//            _bot.UserService = userServiceMock.Object;

//            var chatServiceMock = new Mock<IChatService>();
//            chatServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == chat.Id)))
//                .ReturnsAsync(chatRepo);
//            _bot.ChatService = chatServiceMock.Object;

//            _bot.RecieveMessage(message).Wait();

//            message.Text = "/chatlist";
//            _bot.RecieveMessage(message).Wait();

//            userServiceMock.Verify(mock => mock.Get(It.Is<long>(_ => _ == user.Id)), Times.Exactly(2));
//            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
//            userServiceMock.Verify(mock => mock.GetList(), Times.Never);
//            chatServiceMock.Verify(mock => mock.Get(It.Is<long>(_ => _ == chat.Id)), Times.Exactly(2));
//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                 It.IsAny<ChatId>(),
//                 It.IsAny<string>(),
//                 It.IsAny<ParseMode>(),
//                 It.IsAny<IEnumerable<MessageEntity>>(),
//                 It.IsAny<bool>(),
//                 It.IsAny<bool>(),
//                 It.IsAny<int>(),
//                 It.IsAny<bool?>(),
//                 It.IsAny<IReplyMarkup>(),
//                 It.IsAny<CancellationToken>()), Times.Never);
//        }

//        [Fact]
//        public void ListsAdminTest()
//        {
//            string userName = "TestUser";
//            var user1 = new User
//            {
//                Id = 1,
//                Username = userName + "1"
//            };
//            var user2 = new User
//            {
//                Id = 2,
//                Username = userName + "2"
//            };
//            var userRepo1 = _fixture.UserMapper.MapToEntity(user1);
//            userRepo1.IsAdmin = true;
//            userRepo1.Type = SafeBot.Core.Model.UserType.Admin;
//            var userRepo2 = _fixture.UserMapper.MapToEntity(user2);
//            var chat1 = new Chat
//            {
//                Id = 1,
//                Type = ChatType.Private
//            };
//            var chat2 = new Chat
//            {
//                Id = 2,
//                Type = ChatType.Private
//            };
//            var chatRepo1 = _fixture.ChatMapper.MapToEntity(chat1);
//            var chatRepo2 = _fixture.ChatMapper.MapToEntity(chat2);
//            var message = new Message
//            {
//                Chat = chat1,
//                From = user1,
//                Text = "/userlist"
//            };

//            _fixture.MockBotClient.Reset();
//            _fixture.MockBotClient.Setup(b => b.GetChatAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.IsAny<CancellationToken>())).ReturnsAsync(chat1);
//            _fixture.MockBotClient.Setup(b => b.GetChatAsync(It.Is<ChatId>(_ => _.Identifier == chat2.Id),
//                It.IsAny<CancellationToken>())).ReturnsAsync(chat2);

//            var userServiceMock = new Mock<IUserService>();
//            userServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == user1.Id)))
//                .ReturnsAsync(userRepo1);
//            userServiceMock.Setup(s => s.IsAdmin(It.Is<long>(_ => _ == user1.Id)))
//                .ReturnsAsync(true);
//            userServiceMock.Setup(s => s.GetList())
//                .ReturnsAsync(new SafeBot.Core.Model.User[] { userRepo1, userRepo2 });
//            _bot.UserService = userServiceMock.Object;
//            var chatServiceMock = new Mock<IChatService>();
//            chatServiceMock.Setup(s => s.GetList())
//                .ReturnsAsync(new SafeBot.Core.Model.Chat[] { chatRepo1, chatRepo2 });
//            _bot.ChatService = chatServiceMock.Object;

//            _bot.RecieveMessage(message).Wait();

//            message.Text = "/chatlist";
//            _bot.RecieveMessage(message).Wait();
            
//            userServiceMock.Verify(mock => mock.Get(It.Is<long>(_ => _ == user1.Id)), Times.Exactly(2));
//            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
//            userServiceMock.Verify(mock => mock.GetList(), Times.Once);
//            chatServiceMock.Verify(mock => mock.GetList(), Times.Once);
//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                 It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                 It.IsAny<string>(),
//                 It.IsAny<ParseMode>(),
//                 It.IsAny<IEnumerable<MessageEntity>>(),
//                 It.IsAny<bool>(),
//                 It.IsAny<bool>(),
//                 It.IsAny<int>(),
//                 It.IsAny<bool?>(),
//                 It.IsAny<IReplyMarkup>(),
//                 It.IsAny<CancellationToken>()), Times.Exactly(2));
//        }

//        [Fact]
//        public void ListInactiveUsersNonAdminTest()
//        {
//            //DateTime dtNow = DateTime.Parse("");
//            string userName = "TestUser";
//            var user = new User
//            {
//                Id = 0,
//                Username = userName,
//            };
//            var userRepo = new SafeBot.Core.Model.User { Id = 0 };

//            var chat = new Chat
//            {
//                Id = 1,
//                Type = ChatType.Private
//            };
//            var chatRepo = _fixture.ChatMapper.MapToEntity(chat);
//            var message = new Message
//            {
//                Chat = chat,
//                From = user,
//                Text = "/inactive"
//            };

//            _fixture.MockBotClient.Reset();

//            var userServiceMock = new Mock<IUserService>();
//            userServiceMock.SetupSequence(s => s.Get(It.Is<long>(_ => _ == user.Id)))
//                .ReturnsAsync(default(SafeBot.Core.Model.User))
//                .ReturnsAsync(userRepo);
//            userServiceMock.Setup(s => s.IsAdmin(It.Is<long>(_ => _ == user.Id)))
//                .ReturnsAsync(false);
//            _bot.UserService = userServiceMock.Object;

//            var chatServiceMock = new Mock<IChatService>();
//            chatServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == chat.Id)))
//                .ReturnsAsync(chatRepo);
//            _bot.ChatService = chatServiceMock.Object;

//            _bot.RecieveMessage(message).Wait();

//            _bot.RecieveMessage(message).Wait();

//            userServiceMock.Verify(mock => mock.IsAdmin(It.Is<long>(_ => _ == user.Id)), Times.Never);
//            userServiceMock.Verify(mock => mock.Get(It.Is<long>(_ => _ == user.Id)), Times.Exactly(2));
//            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
//            userServiceMock.Verify(mock => mock.GetList(), Times.Never);
//            chatServiceMock.Verify(mock => mock.Get(It.Is<long>(_ => _ == chat.Id)), Times.Exactly(2));
//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                 It.IsAny<ChatId>(),
//                 It.IsAny<string>(),
//                 It.IsAny<ParseMode>(),
//                 It.IsAny<IEnumerable<MessageEntity>>(),
//                 It.IsAny<bool>(),
//                 It.IsAny<bool>(),
//                 It.IsAny<int>(),
//                 It.IsAny<bool?>(),
//                 It.IsAny<IReplyMarkup>(),
//                 It.IsAny<CancellationToken>()), Times.Never);
//        }

//        [Fact]
//        public void ListInactiveUsersAdminTest()
//        {
//            DateTime dt = DateTime.Parse("2020-10-28T10:16:15.242Z");
//            DateTime dt0 = dt.AddDays(-2);
//            DateTime dt1 = dt.AddHours(-72);
//            DateTime dt2 = dt.AddHours(-71);
//            DateTime dt3 = dt.AddHours(-73);
//            var member = SafeBot.Core.Model.UserType.Member;
//            var bot = SafeBot.Core.Model.UserType.Bot;
//            var admin = SafeBot.Core.Model.UserType.Admin;
//            var coordinator = SafeBot.Core.Model.UserType.Coordinator;

//            var user = new User  { Id = 0 };
//            var user1 = new User { Id = 1 };
//            var user2 = new User { Id = 2 };
//            var user3 = new User { Id = 3 };
//            var userRepo = new SafeBot.Core.Model.User { Id = 0, Name = "user0", Type = SafeBot.Core.Model.UserType.Admin, LastMessageTime = dt0 };
//            var userRepo1 = new SafeBot.Core.Model.User { Id = 1, LastMessageTime = dt1, Name = "user1", Type = member };
//            var userRepo2 = new SafeBot.Core.Model.User { Id = 2, LastMessageTime = dt2, Name = "user2", Type = member };
//            var userRepo3 = new SafeBot.Core.Model.User { Id = 3, LastMessageTime = dt3, Name = "user3", Type = member };
//            var userRepo4 = new SafeBot.Core.Model.User { Id = 4, LastMessageTime = null, Name = "user4", Type = member };
//            var userRepo5= new SafeBot.Core.Model.User { Id = 5, LastMessageTime = null, Name = "user5", Type = admin };
//            var userRepo6 = new SafeBot.Core.Model.User { Id = 6, LastMessageTime = null, Name = "user6", Type = coordinator };
//            var userRepo7 = new SafeBot.Core.Model.User { Id = 7, LastMessageTime = null, Name = "user7", Type = bot };

//            var chat = new Chat 
//            {
//                Id = 1,
//                Type = ChatType.Private
//            };
//            var chatRepo = _fixture.ChatMapper.MapToEntity(chat);
//            var message = new Message
//            {
//                Chat = chat,
//                From = user,
//                Text = "/inactive"
//            };

//            _fixture.MockBotClient.Reset();

//            var userServiceMock = new Mock<IUserService>();
//            userServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == user.Id)))
//                .ReturnsAsync(userRepo);
//            userServiceMock.Setup(s => s.IsAdmin(It.Is<long>(_ => _ == user.Id)))
//                .ReturnsAsync(true);
//            userServiceMock.Setup(s => s.GetList())
//                .ReturnsAsync(new SafeBot.Core.Model.User[] { userRepo, userRepo1, userRepo2, userRepo3, userRepo4, userRepo5, userRepo6, userRepo7 });
//            _bot.UserService = userServiceMock.Object;

//            var chatServiceMock = new Mock<IChatService>();
//            chatServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == chat.Id)))
//                .ReturnsAsync(chatRepo);
//            _bot.ChatService = chatServiceMock.Object;

//            var dateTimeServiceMock = new Mock<IDateTimeService>();
//            dateTimeServiceMock.Setup(s => s.GetDateTimeUTCNow())
//                .Returns(dt);
//            _bot.DateTimeService = dateTimeServiceMock.Object;
//            DateTimeService dtService = new DateTimeService();
//            Console.WriteLine(dtService.FormatToString(userRepo1.LastMessageTime));
//            Console.WriteLine(dtService.FormatToString(userRepo2.LastMessageTime));
//            Console.WriteLine(dtService.FormatToString(userRepo3.LastMessageTime));
//            Console.WriteLine(dtService.FormatToString(userRepo4.LastMessageTime));
//            Console.WriteLine(dtService.FormatToString(userRepo5.LastMessageTime));
//            Console.WriteLine(dtService.FormatToString(userRepo6.LastMessageTime));
//            Console.WriteLine(dtService.FormatToString(userRepo7.LastMessageTime));
//            dateTimeServiceMock.Setup(s => s.GetDateTimeUTCNow())
//                .Returns(dt);
//            _bot.DateTimeService = dateTimeServiceMock.Object;



//            _fixture.MockBotClient.Setup(s => s.GetChatAsync(It.Is<ChatId>(_ => _.Identifier == chat.Id),
//                It.IsAny<CancellationToken>())).ReturnsAsync(chat);

//            _bot.RecieveMessage(message).Wait();

//            userServiceMock.Verify(mock => mock.IsAdmin(It.Is<long>(_ => _ == user.Id)), Times.Once);
//            userServiceMock.Verify(mock => mock.Get(It.Is<long>(_ => _ == user.Id)), Times.Once);
//            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
//            userServiceMock.Verify(mock => mock.GetList(), Times.Once);
//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                 It.Is<ChatId>(_ => _.Identifier == chatRepo.Id),
//                 It.Is<string>(_ => _.Contains("user1") && _.Contains("user3") && _.Contains("user4") && _.Contains("user5") && 
//                 !_.Contains("user0") && !_.Contains("user2") &&  !_.Contains("user6") && !_.Contains("user7")),
//                 It.IsAny<ParseMode>(),
//                 It.IsAny<IEnumerable<MessageEntity>>(),
//                 It.IsAny<bool>(),
//                 It.IsAny<bool>(),
//                 It.IsAny<int>(),
//                 It.IsAny<bool?>(),
//                 It.IsAny<IReplyMarkup>(),
//                 It.IsAny<CancellationToken>()), Times.Once);
//        }


//    }
//}
