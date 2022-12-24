//using Moq;
//using System.Threading;
//using Telegram.SafeBot.Core.Constant;
//using Telegram.SafeBot.Service.Interface;
//using Xunit;
//using System.Linq;
//using System.Collections.Generic;
//using Telegram.SafeBot.Core.Model;
//using Telegram.SafeBot.Bot.StateMachines;
//using Telegram.SafeBot.Bot.Interface;

//namespace Telegram.SafeBot.Test.Integration
//{
//    public class PollTests : IClassFixture<BotFixture>
//    {
//        private readonly BotFixture _fixture = null;
//        private readonly SafeBot.Bot.Bot _bot = null;

//        public PollTests(BotFixture fixture)
//        {
//            _fixture = fixture;
//            _bot = fixture.Bot;
//        }

//        [Fact]
//        public void PollNonAdmin()
//        {
//            string userName = "TestUser";
//            var user1 = new Telegram.Bot.Types.User
//            {
//                Id = 1,
//                Username = userName + "1",
//            };
//            var userRepo = new SafeBot.Core.Model.User
//            {
//                Id = user1.Id,
//                Type = UserType.Member
//            };
//            var chat = new Telegram.Bot.Types.Chat
//            {
//                Id = 1,
//                Type = Telegram.Bot.Types.Enums.ChatType.Private
//            };
//            var chatRepo = _fixture.ChatMapper.MapToEntity(chat);
//            var message = new Message
//            {
//                Chat = chat,
//                From = user1,
//                Text = "/poll"
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

//            var userMessageServiceMock = new Mock<IUserMessageService>();
//            _bot.UserMessageService = userMessageServiceMock.Object;

//            _bot.RecieveMessage(message).Wait();

//            chatServiceMock.Verify(mock => mock.Get(It.Is<long>(_ => _ == chat.Id)), Times.Once);
//            userMessageServiceMock.Verify(mock => mock.Add(It.IsAny<UserMessage>()), Times.Once);
//            userServiceMock.Verify(mock => mock.Get(It.IsAny<long>()), Times.Once);
//            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
//            userServiceMock.Verify(mock => mock.IsAdmin(It.IsAny<long>()), Times.Never);

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
//        public void PollAdminWithoutPermissions()
//        {
//            string userName = "TestUser";
//            var user1 = new Telegram.Bot.Types.User
//            {
//                Id = 1,
//                Username = userName + "1",
//            };
//            var userRepo = new SafeBot.Core.Model.User
//            {
//                Id = user1.Id,
//                Type = UserType.Admin
//            };
//            var chat = new Telegram.Bot.Types.Chat
//            {
//                Id = 1,
//                Type = Telegram.Bot.Types.Enums.ChatType.Private
//            };
//            var chatRepo = _fixture.ChatMapper.MapToEntity(chat);
//            var message = new Message
//            {
//                Chat = chat,
//                From = user1,
//                Text = "/poll"
//            };

//            _fixture.MockBotClient.Reset();

//            var userServiceMock = new Mock<IUserService>();
//            userServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == user1.Id)))
//                .ReturnsAsync(userRepo);
//            userServiceMock.Setup(s => s.IsAdmin(It.Is<long>(_ => _ == user1.Id)))
//                .ReturnsAsync(false);
//            _bot.UserService = userServiceMock.Object;

//            var chatServiceMock = new Mock<IChatService>();
//            chatServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == chat.Id)))
//                .ReturnsAsync(chatRepo);
//            _bot.ChatService = chatServiceMock.Object;

//            var userMessageServiceMock = new Mock<IUserMessageService>();
//            _bot.UserMessageService = userMessageServiceMock.Object;

//            _fixture.MockBotClient.Setup(c => c.GetChatAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat.Id), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(chat);

//            _bot.RecieveMessage(message).Wait();

//            chatServiceMock.Verify(mock => mock.Get(It.Is<long>(_ => _ == chat.Id)), Times.Once);
//            userMessageServiceMock.Verify(mock => mock.Add(It.IsAny<UserMessage>()), Times.Once);
//            userServiceMock.Verify(mock => mock.Get(It.IsAny<long>()), Times.Once);
//            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
//            userServiceMock.Verify(mock => mock.IsAdmin(It.IsAny<long>()), Times.Once);

//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat.Id),
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
//        }

//        [Fact]
//        public void PollAdminWithPermissionsCancel()
//        {
//            string userName = "TestUser";
//            var user1 = new Telegram.Bot.Types.User
//            {
//                Id = 1,
//                Username = userName + "1",
//            };
//            var userRepo = new SafeBot.Core.Model.User
//            {
//                Id = user1.Id,
//                Type = UserType.Admin
//            };
//            var chat1 = new Telegram.Bot.Types.Chat
//            {
//                Id = 1,
//                Type = Telegram.Bot.Types.Enums.ChatType.Private
//            };
//            var chat2 = new Telegram.Bot.Types.Chat
//            {
//                Id = 2,
//                Type = Telegram.Bot.Types.Enums.ChatType.Group
//            };
//            var chat3 = new Telegram.Bot.Types.Chat
//            {
//                Id = 3,
//                Type = Telegram.Bot.Types.Enums.ChatType.Supergroup
//            };
//            var chatRepo1 = new SafeBot.Core.Model.Chat { Id = chat1.Id, ChatType = SafeBot.Core.Model.ChatType.Private, Title = "Private"};
//            var chatRepo2 = new SafeBot.Core.Model.Chat { Id = chat2.Id, ChatType = SafeBot.Core.Model.ChatType.Public, Title = "Public" };
//            var chatRepo3 = new SafeBot.Core.Model.Chat { Id = chat3.Id, ChatType = SafeBot.Core.Model.ChatType.Admin, Title = "Admin" };
//            var chats = new SafeBot.Core.Model.Chat[] { chatRepo1, chatRepo2, chatRepo3 };
//            var message = new Message
//            {
//                Chat = chat1,
//                From = user1,
//                Text = "/poll"
//            };

//            _fixture.MockBotClient.Reset();
            
//            var userServiceMock = new Mock<IUserService>();
//            userServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == user1.Id)))
//                .ReturnsAsync(userRepo);
//            userServiceMock.Setup(s => s.IsAdmin(It.Is<long>(_ => _ == user1.Id)))
//                .ReturnsAsync(true);
//            _bot.UserService = userServiceMock.Object;

//            var chatServiceMock = new Mock<IChatService>();
//            chatServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == chat1.Id)))
//                .ReturnsAsync(chatRepo1);
//            chatServiceMock.Setup(s => s.GetList())
//                .ReturnsAsync(chats);
//            _bot.ChatService = chatServiceMock.Object;
//            _bot.StateMachines = new IStateMachine[] { new PollStateMachine(chatServiceMock.Object) };

//            var userMessageServiceMock = new Mock<IUserMessageService>();
//            _bot.UserMessageService = userMessageServiceMock.Object;

//            _fixture.MockBotClient.Setup(c => c.GetChatAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(chat1);

//            _bot.RecieveMessage(message).Wait();

//            message.Text = Messages.Cancel;
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Any message";
//            _bot.RecieveMessage(message).Wait();

//            chatServiceMock.Verify(mock => mock.Get(It.Is<long>(_ => _ == chat1.Id)), Times.Exactly(3));
//            userMessageServiceMock.Verify(mock => mock.Add(It.IsAny<UserMessage>()), Times.Exactly(3));
//            userServiceMock.Verify(mock => mock.Get(It.IsAny<long>()), Times.Once);
//            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
//            userServiceMock.Verify(mock => mock.IsAdmin(It.IsAny<long>()), Times.Once);

//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ == Messages.SelectChat), 
//                It.IsAny<ParseMode>(),
//                It.IsAny<IEnumerable<MessageEntity>>(),
//                It.IsAny<bool>(), 
//                It.IsAny<bool>(), 
//                It.IsAny<int>(),
//                It.IsAny<bool?>(),
//                It.Is<IReplyMarkup>(m => KeyboardMarkupActionButtons(m, 3)), 
//                It.IsAny<CancellationToken>()),
//                Times.Once);
//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ == Messages.Cancelled),
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
//                It.Is<string>(_ => _ != Messages.SelectChat && _ != Messages.Cancelled), 
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
//        public void PollAdminWithPermissionsIncorrectChatChoice()
//        {
//            string userName = "TestUser";
//            var user1 = new Telegram.Bot.Types.User
//            {
//                Id = 1,
//                Username = userName + "1",
//            };
//            var userRepo = new SafeBot.Core.Model.User
//            {
//                Id = user1.Id,
//                Type = UserType.Admin
//            };
//            var chat1 = new Telegram.Bot.Types.Chat
//            {
//                Id = 1,
//                Type = Telegram.Bot.Types.Enums.ChatType.Private
//            };
//            var chat2 = new Telegram.Bot.Types.Chat
//            {
//                Id = 2,
//                Type = Telegram.Bot.Types.Enums.ChatType.Group
//            };
//            var chat3 = new Telegram.Bot.Types.Chat
//            {
//                Id = 3,
//                Type = Telegram.Bot.Types.Enums.ChatType.Supergroup
//            };
//            var chatRepo1 = new SafeBot.Core.Model.Chat { Id = chat1.Id, ChatType = SafeBot.Core.Model.ChatType.Private, Title = "Private" };
//            var chatRepo2 = new SafeBot.Core.Model.Chat { Id = chat2.Id, ChatType = SafeBot.Core.Model.ChatType.Public, Title = "Public" };
//            var chatRepo3 = new SafeBot.Core.Model.Chat { Id = chat3.Id, ChatType = SafeBot.Core.Model.ChatType.Admin, Title = "Admin" };
//            var chats = new SafeBot.Core.Model.Chat[] { chatRepo1, chatRepo2, chatRepo3 };
//            var message = new Message
//            {
//                Chat = chat1,
//                From = user1,
//                Text = "/poll"
//            };

//            _fixture.MockBotClient.Reset();

//            var userServiceMock = new Mock<IUserService>();
//            userServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == user1.Id)))
//                .ReturnsAsync(userRepo);
//            userServiceMock.Setup(s => s.IsAdmin(It.Is<long>(_ => _ == user1.Id)))
//                .ReturnsAsync(true);
//            _bot.UserService = userServiceMock.Object;

//            var chatServiceMock = new Mock<IChatService>();
//            chatServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == chat1.Id)))
//                .ReturnsAsync(chatRepo1);
//            chatServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == chat2.Id)))
//                .ReturnsAsync(chatRepo2);
//            chatServiceMock.Setup(s => s.Get(It.Is<string>(_ => _ == "Private")))
//                .ReturnsAsync(chatRepo1);
//            chatServiceMock.Setup(s => s.Get(It.Is<string>(_ => _ == "Public")))
//                .ReturnsAsync(chatRepo2);
//            chatServiceMock.Setup(s => s.Get(It.Is<string>(_ => _ == "Admin")))
//                .ReturnsAsync(chatRepo3);
//            chatServiceMock.Setup(s => s.GetList())
//                .ReturnsAsync(chats);
//            _bot.ChatService = chatServiceMock.Object;
//            _bot.StateMachines = new IStateMachine[] { new PollStateMachine(chatServiceMock.Object) };

//            var userMessageServiceMock = new Mock<IUserMessageService>();
//            _bot.UserMessageService = userMessageServiceMock.Object;

//            _fixture.MockBotClient.Setup(c => c.GetChatAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(chat1);

//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Incorrect chat";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Any message";
//            _bot.RecieveMessage(message).Wait();

//            chatServiceMock.Verify(mock => mock.Get(It.Is<long>(_ => _ == chat1.Id)), Times.Exactly(3));
//            userMessageServiceMock.Verify(mock => mock.Add(It.IsAny<UserMessage>()), Times.Exactly(3));
//            userServiceMock.Verify(mock => mock.Get(It.IsAny<long>()), Times.Once);
//            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
//            userServiceMock.Verify(mock => mock.IsAdmin(It.IsAny<long>()), Times.Once);

//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ == Messages.SelectChat),
//                It.IsAny<ParseMode>(),
//                It.IsAny<IEnumerable<MessageEntity>>(),
//                It.IsAny<bool>(), 
//                It.IsAny<bool>(),
//                It.IsAny<int>(),
//                It.IsAny<bool?>(),
//                It.Is<IReplyMarkup>(m => KeyboardMarkupActionButtons(m, 3)), 
//                It.IsAny<CancellationToken>()),
//                Times.Once);
//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ == Messages.Cancelled), 
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
//                It.Is<string>(_ => _ != Messages.SelectChat && _ != Messages.Cancelled), 
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
//        public void PollAdminWithPermissions0Cases()
//        {
//            string userName = "TestUser";
//            var user1 = new Telegram.Bot.Types.User
//            {
//                Id = 1,
//                Username = userName + "1",
//            };
//            var userRepo = new SafeBot.Core.Model.User
//            {
//                Id = user1.Id,
//                Type = UserType.Admin
//            };
//            var chat1 = new Telegram.Bot.Types.Chat
//            {
//                Id = 1,
//                Type = Telegram.Bot.Types.Enums.ChatType.Private
//            };
//            var chat2 = new Telegram.Bot.Types.Chat
//            {
//                Id = 2,
//                Type = Telegram.Bot.Types.Enums.ChatType.Group
//            };
//            var chat3 = new Telegram.Bot.Types.Chat
//            {
//                Id = 3,
//                Type = Telegram.Bot.Types.Enums.ChatType.Supergroup
//            };
//            var chatRepo1 = new SafeBot.Core.Model.Chat { Id = chat1.Id, ChatType = SafeBot.Core.Model.ChatType.Private, Title = "Private" };
//            var chatRepo2 = new SafeBot.Core.Model.Chat { Id = chat2.Id, ChatType = SafeBot.Core.Model.ChatType.Public, Title = "Public" };
//            var chatRepo3 = new SafeBot.Core.Model.Chat { Id = chat3.Id, ChatType = SafeBot.Core.Model.ChatType.Admin, Title = "Admin" };
//            var chats = new SafeBot.Core.Model.Chat[] { chatRepo1, chatRepo2, chatRepo3 };
//            var message = new Message
//            {
//                Chat = chat1,
//                From = user1,
//                Text = "/poll"
//            };

//            _fixture.MockBotClient.Reset();

//            var userServiceMock = new Mock<IUserService>();
//            userServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == user1.Id)))
//                .ReturnsAsync(userRepo);
//            userServiceMock.Setup(s => s.IsAdmin(It.Is<long>(_ => _ == user1.Id)))
//                .ReturnsAsync(true);
//            _bot.UserService = userServiceMock.Object;

//            var chatServiceMock = new Mock<IChatService>();
//            chatServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == chat1.Id)))
//                .ReturnsAsync(chatRepo1);
//            chatServiceMock.Setup(s => s.Get(It.Is<string>(_ => _ == "Private")))
//                .ReturnsAsync(chatRepo1);
//            chatServiceMock.Setup(s => s.Get(It.Is<string>(_ => _ == "Public")))
//                .ReturnsAsync(chatRepo2);
//            chatServiceMock.Setup(s => s.Get(It.Is<string>(_ => _ == "Admin")))
//                .ReturnsAsync(chatRepo3);
//            chatServiceMock.Setup(s => s.GetList())
//                .ReturnsAsync(chats);
//            _bot.ChatService = chatServiceMock.Object;
//            _bot.StateMachines = new IStateMachine[]
//            {
//                new PollStateMachine(chatServiceMock.Object)
//            };

//            var userMessageServiceMock = new Mock<IUserMessageService>();
//            _bot.UserMessageService = userMessageServiceMock.Object;

//            _fixture.MockBotClient.Setup(c => c.GetChatAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(chat1);

//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Public";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Question";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "/done";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Other message";
//            _bot.RecieveMessage(message).Wait();

//            chatServiceMock.Verify(mock => mock.Get(It.Is<long>(_ => _ == chat1.Id)), Times.Exactly(5));
//            userMessageServiceMock.Verify(mock => mock.Add(It.IsAny<UserMessage>()), Times.Exactly(5));
//            userServiceMock.Verify(mock => mock.Get(It.IsAny<long>()), Times.Once);
//            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
//            userServiceMock.Verify(mock => mock.IsAdmin(It.IsAny<long>()), Times.Once);

//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ == Messages.SelectChat), 
//                It.IsAny<ParseMode>(),
//                It.IsAny<IEnumerable<MessageEntity>>(),
//                It.IsAny<bool>(), 
//                It.IsAny<bool>(),
//                It.IsAny<int>(),
//                It.IsAny<bool?>(),
//                It.Is<IReplyMarkup>(m => KeyboardMarkupActionButtons(m, 3)), 
//                It.IsAny<CancellationToken>()),
//                Times.Once);
//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ == "Please, input a question"),
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
//                It.Is<string>(_ => _ == "Please, input first case"), 
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
//                It.Is<string>(_ => _ == "Cancelled: cases must be minimum 2"), 
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
//                It.IsAny<ChatId>(),
//                It.Is<string>(_ => _ != "Cancelled: cases must be minimum 2" && _ != "Please, input first case" && 
//                    _!= "Please, input a question" && _!= Messages.SelectChat), 
//                It.IsAny<ParseMode>(),
//                It.IsAny<IEnumerable<MessageEntity>>(),
//                It.IsAny<bool>(), 
//                It.IsAny<bool>(), 
//                It.IsAny<int>(),
//                It.IsAny<bool?>(),
//                It.IsAny<IReplyMarkup>(), 
//                It.IsAny<CancellationToken>()), Times.Never);
//        }

//        [Fact]
//        public void PollAdminWithPermissions1Cases()
//        {
//            string userName = "TestUser";
//            var user1 = new Telegram.Bot.Types.User
//            {
//                Id = 1,
//                Username = userName + "1",
//            };
//            var userRepo = new SafeBot.Core.Model.User
//            {
//                Id = user1.Id,
//                Type = UserType.Admin
//            };
//            var chat1 = new Telegram.Bot.Types.Chat
//            {
//                Id = 1,
//                Type = Telegram.Bot.Types.Enums.ChatType.Private
//            };
//            var chat2 = new Telegram.Bot.Types.Chat
//            {
//                Id = 2,
//                Type = Telegram.Bot.Types.Enums.ChatType.Group
//            };
//            var chat3 = new Telegram.Bot.Types.Chat
//            {
//                Id = 3,
//                Type = Telegram.Bot.Types.Enums.ChatType.Supergroup
//            };
//            var chatRepo1 = new SafeBot.Core.Model.Chat { Id = chat1.Id, ChatType = SafeBot.Core.Model.ChatType.Private, Title = "Private" };
//            var chatRepo2 = new SafeBot.Core.Model.Chat { Id = chat2.Id, ChatType = SafeBot.Core.Model.ChatType.Public, Title = "Public" };
//            var chatRepo3 = new SafeBot.Core.Model.Chat { Id = chat3.Id, ChatType = SafeBot.Core.Model.ChatType.Admin, Title = "Admin" };
//            var chats = new SafeBot.Core.Model.Chat[] { chatRepo1, chatRepo2, chatRepo3 };
//            var message = new Message
//            {
//                Chat = chat1,
//                From = user1,
//                Text = "/poll"
//            };

//            _fixture.MockBotClient.Reset();

//            var userServiceMock = new Mock<IUserService>();
//            userServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == user1.Id)))
//                .ReturnsAsync(userRepo);
//            userServiceMock.Setup(s => s.IsAdmin(It.Is<long>(_ => _ == user1.Id)))
//                .ReturnsAsync(true);
//            _bot.UserService = userServiceMock.Object;

//            var chatServiceMock = new Mock<IChatService>();
//            chatServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == chat1.Id)))
//                .ReturnsAsync(chatRepo1);
//            chatServiceMock.Setup(s => s.Get(It.Is<string>(_ => _ == "Private")))
//                .ReturnsAsync(chatRepo1);
//            chatServiceMock.Setup(s => s.Get(It.Is<string>(_ => _ == "Public")))
//                .ReturnsAsync(chatRepo2);
//            chatServiceMock.Setup(s => s.Get(It.Is<string>(_ => _ == "Admin")))
//                .ReturnsAsync(chatRepo3);
//            chatServiceMock.Setup(s => s.GetList())
//                .ReturnsAsync(chats);
//            _bot.ChatService = chatServiceMock.Object;
//            _bot.StateMachines = new IStateMachine[]
//            {
//                new PollStateMachine(chatServiceMock.Object)
//            };

//            var userMessageServiceMock = new Mock<IUserMessageService>();
//            _bot.UserMessageService = userMessageServiceMock.Object;

//            _fixture.MockBotClient.Setup(c => c.GetChatAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(chat1);

//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Public";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Question";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "First case";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "/done";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Other message";
//            _bot.RecieveMessage(message).Wait();

//            chatServiceMock.Verify(mock => mock.Get(It.Is<long>(_ => _ == chat1.Id)), Times.Exactly(6));
//            userMessageServiceMock.Verify(mock => mock.Add(It.IsAny<UserMessage>()), Times.Exactly(6));
//            userServiceMock.Verify(mock => mock.Get(It.IsAny<long>()), Times.Once);
//            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
//            userServiceMock.Verify(mock => mock.IsAdmin(It.IsAny<long>()), Times.Once);

//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ == Messages.SelectChat), 
//                It.IsAny<ParseMode>(),
//                It.IsAny<IEnumerable<MessageEntity>>(),
//                It.IsAny<bool>(), 
//                It.IsAny<bool>(),
//                It.IsAny<int>(),
//                It.IsAny<bool?>(),
//                It.Is<IReplyMarkup>(m => KeyboardMarkupActionButtons(m, 3)), 
//                It.IsAny<CancellationToken>()),
//                Times.Once);
//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ == "Please, input a question"), 
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
//                It.Is<string>(_ => _ == "Please, input first case"), 
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
//                It.Is<string>(_ => _ == "Please, input next case or <code>/done</code> for stop"), 
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
//                It.Is<string>(_ => _ == "Cancelled: cases must be minimum 2"), 
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
//                It.IsAny<ChatId>(),
//                It.Is<string>(_ => _ != "Cancelled: cases must be minimum 2" && _ != "Please, input next case or <code>/done</code> for stop" &&
//                    _ != "Please, input first case" && _ != "Please, input a question" && _ != Messages.SelectChat),
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
//        public void PollAdminWithPermissionsMultiAnswersIncorrectChoice()
//        {
//            string userName = "TestUser";
//            var user1 = new Telegram.Bot.Types.User
//            {
//                Id = 1,
//                Username = userName + "1",
//            };
//            var userRepo = new SafeBot.Core.Model.User
//            {
//                Id = user1.Id,
//                Type = UserType.Admin
//            };
//            var chat1 = new Telegram.Bot.Types.Chat
//            {
//                Id = 1,
//                Type = Telegram.Bot.Types.Enums.ChatType.Private
//            };
//            var chat2 = new Telegram.Bot.Types.Chat
//            {
//                Id = 2,
//                Type = Telegram.Bot.Types.Enums.ChatType.Group
//            };
//            var chat3 = new Telegram.Bot.Types.Chat
//            {
//                Id = 3,
//                Type = Telegram.Bot.Types.Enums.ChatType.Supergroup
//            };
//            var chatRepo1 = new SafeBot.Core.Model.Chat { Id = chat1.Id, ChatType = SafeBot.Core.Model.ChatType.Private, Title = "Private" };
//            var chatRepo2 = new SafeBot.Core.Model.Chat { Id = chat2.Id, ChatType = SafeBot.Core.Model.ChatType.Public, Title = "Public" };
//            var chatRepo3 = new SafeBot.Core.Model.Chat { Id = chat3.Id, ChatType = SafeBot.Core.Model.ChatType.Admin, Title = "Admin" };
//            var chats = new SafeBot.Core.Model.Chat[] { chatRepo1, chatRepo2, chatRepo3 };
//            var message = new Message
//            {
//                Chat = chat1,
//                From = user1,
//                Text = "/poll"
//            };

//            _fixture.MockBotClient.Reset();

//            var userServiceMock = new Mock<IUserService>();
//            userServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == user1.Id)))
//                .ReturnsAsync(userRepo);
//            userServiceMock.Setup(s => s.IsAdmin(It.Is<long>(_ => _ == user1.Id)))
//                .ReturnsAsync(true);
//            _bot.UserService = userServiceMock.Object;

//            var chatServiceMock = new Mock<IChatService>();
//            chatServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == chat1.Id)))
//                .ReturnsAsync(chatRepo1);
//            chatServiceMock.Setup(s => s.Get(It.Is<string>(_ => _ == "Private")))
//                .ReturnsAsync(chatRepo1);
//            chatServiceMock.Setup(s => s.Get(It.Is<string>(_ => _ == "Public")))
//                .ReturnsAsync(chatRepo2);
//            chatServiceMock.Setup(s => s.Get(It.Is<string>(_ => _ == "Admin")))
//                .ReturnsAsync(chatRepo3);
//            chatServiceMock.Setup(s => s.GetList())
//                .ReturnsAsync(chats);
//            _bot.ChatService = chatServiceMock.Object;
//            _bot.StateMachines = new IStateMachine[]
//            {
//                new PollStateMachine(chatServiceMock.Object)
//            };

//            var userMessageServiceMock = new Mock<IUserMessageService>();
//            _bot.UserMessageService = userMessageServiceMock.Object;

//            _fixture.MockBotClient.Setup(c => c.GetChatAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(chat1);

//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Public";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Question";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "First case";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Second case";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "/done";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Nain";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Other message";
//            _bot.RecieveMessage(message).Wait();

//            chatServiceMock.Verify(mock => mock.Get(It.Is<long>(_ => _ == chat1.Id)), Times.Exactly(8));
//            userMessageServiceMock.Verify(mock => mock.Add(It.IsAny<UserMessage>()), Times.Exactly(8));
//            userServiceMock.Verify(mock => mock.Get(It.IsAny<long>()), Times.Once);
//            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
//            userServiceMock.Verify(mock => mock.IsAdmin(It.IsAny<long>()), Times.Once);

//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ == Messages.SelectChat), 
//                It.IsAny<ParseMode>(),
//                It.IsAny<IEnumerable<MessageEntity>>(),
//                It.IsAny<bool>(), 
//                It.IsAny<bool>(),
//                It.IsAny<int>(),
//                It.IsAny<bool?>(),
//                It.Is<IReplyMarkup>(m => KeyboardMarkupActionButtons(m, 3)), 
//                It.IsAny<CancellationToken>()),
//                Times.Once);
//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ == "Please, input a question"), 
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
//                It.Is<string>(_ => _ == "Please, input first case"), 
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
//                It.Is<string>(_ => _ == "Please, input next case or <code>/done</code> for stop"),
//                It.IsAny<ParseMode>(),
//                It.IsAny<IEnumerable<MessageEntity>>(),
//                It.IsAny<bool>(), 
//                It.IsAny<bool>(), 
//                It.IsAny<int>(),
//                It.IsAny<bool?>(),
//                It.IsAny<IReplyMarkup>(), 
//                It.IsAny<CancellationToken>()),
//                Times.Exactly(2));
//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ == "Is the pool with multiple answers?"),
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
//                It.Is<string>(_ => _ == Messages.Cancelled), 
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
//                It.IsAny<ChatId>(),
//                It.Is<string>(_ => _ != Messages.Cancelled && _ != "Is the pool with multiple answers?" && 
//                    _ != "Please, input next case or <code>/done</code> for stop" && _ != "Please, input first case" && 
//                    _ != "Please, input a question" && _ != Messages.SelectChat),
//                It.IsAny<ParseMode>(),
//                It.IsAny<IEnumerable<MessageEntity>>(),
//                It.IsAny<bool>(), 
//                It.IsAny<bool>(), 
//                It.IsAny<int>(),
//                It.IsAny<bool?>(),
//                It.IsAny<IReplyMarkup>(),
//                It.IsAny<CancellationToken>()), Times.Never);
//        }

//        [Fact]
//        public void PollAdminWithPermissionsMultiAnswersCancelled()
//        {
//            string userName = "TestUser";
//            var user1 = new Telegram.Bot.Types.User
//            {
//                Id = 1,
//                Username = userName + "1",
//            };
//            var userRepo = new SafeBot.Core.Model.User
//            {
//                Id = user1.Id,
//                Type = UserType.Admin
//            };
//            var chat1 = new Telegram.Bot.Types.Chat
//            {
//                Id = 1,
//                Type = Telegram.Bot.Types.Enums.ChatType.Private
//            };
//            var chat2 = new Telegram.Bot.Types.Chat
//            {
//                Id = 2,
//                Type = Telegram.Bot.Types.Enums.ChatType.Group
//            };
//            var chat3 = new Telegram.Bot.Types.Chat
//            {
//                Id = 3,
//                Type = Telegram.Bot.Types.Enums.ChatType.Supergroup
//            };
//            var chatRepo1 = new SafeBot.Core.Model.Chat { Id = chat1.Id, ChatType = SafeBot.Core.Model.ChatType.Private, Title = "Private" };
//            var chatRepo2 = new SafeBot.Core.Model.Chat { Id = chat2.Id, ChatType = SafeBot.Core.Model.ChatType.Public, Title = "Public" };
//            var chatRepo3 = new SafeBot.Core.Model.Chat { Id = chat3.Id, ChatType = SafeBot.Core.Model.ChatType.Admin, Title = "Admin" };
//            var chats = new SafeBot.Core.Model.Chat[] { chatRepo1, chatRepo2, chatRepo3 };
//            var message = new Message
//            {
//                Chat = chat1,
//                From = user1,
//                Text = "/poll"
//            };

//            _fixture.MockBotClient.Reset();

//            var userServiceMock = new Mock<IUserService>();
//            userServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == user1.Id)))
//                .ReturnsAsync(userRepo);
//            userServiceMock.Setup(s => s.IsAdmin(It.Is<long>(_ => _ == user1.Id)))
//                .ReturnsAsync(true);
//            _bot.UserService = userServiceMock.Object;

//            var chatServiceMock = new Mock<IChatService>();
//            chatServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == chat1.Id)))
//                .ReturnsAsync(chatRepo1);
//            chatServiceMock.Setup(s => s.Get(It.Is<string>(_ => _ == "Private")))
//                .ReturnsAsync(chatRepo1);
//            chatServiceMock.Setup(s => s.Get(It.Is<string>(_ => _ == "Public")))
//                .ReturnsAsync(chatRepo2);
//            chatServiceMock.Setup(s => s.Get(It.Is<string>(_ => _ == "Admin")))
//                .ReturnsAsync(chatRepo3);
//            chatServiceMock.Setup(s => s.GetList())
//                .ReturnsAsync(chats);
//            _bot.ChatService = chatServiceMock.Object;
//            _bot.StateMachines = new IStateMachine[]
//            {
//                new PollStateMachine(chatServiceMock.Object)
//            };

//            var userMessageServiceMock = new Mock<IUserMessageService>();
//            _bot.UserMessageService = userMessageServiceMock.Object;

//            _fixture.MockBotClient.Setup(c => c.GetChatAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(chat1);

//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Public";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Question";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "First case";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Second case";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "/done";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = Messages.Cancel;
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Other message";
//            _bot.RecieveMessage(message).Wait();

//            chatServiceMock.Verify(mock => mock.Get(It.Is<long>(_ => _ == chat1.Id)), Times.Exactly(8));
//            userMessageServiceMock.Verify(mock => mock.Add(It.IsAny<UserMessage>()), Times.Exactly(8));
//            userServiceMock.Verify(mock => mock.Get(It.IsAny<long>()), Times.Once);
//            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
//            userServiceMock.Verify(mock => mock.IsAdmin(It.IsAny<long>()), Times.Once);

//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ == Messages.SelectChat), 
//                It.IsAny<ParseMode>(),
//                It.IsAny<IEnumerable<MessageEntity>>(),
//                It.IsAny<bool>(), 
//                It.IsAny<bool>(),
//                It.IsAny<int>(),
//                It.IsAny<bool?>(),
//                It.Is<IReplyMarkup>(m => KeyboardMarkupActionButtons(m, 3)), 
//                It.IsAny<CancellationToken>()),
//                Times.Once);
//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ == "Please, input a question"), 
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
//                It.Is<string>(_ => _ == "Please, input first case"), 
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
//                It.Is<string>(_ => _ == "Please, input next case or <code>/done</code> for stop"),
//                It.IsAny<ParseMode>(),
//                It.IsAny<IEnumerable<MessageEntity>>(),
//                It.IsAny<bool>(), 
//                It.IsAny<bool>(), 
//                It.IsAny<int>(),
//                It.IsAny<bool?>(),
//                It.IsAny<IReplyMarkup>(), 
//                It.IsAny<CancellationToken>()),
//                Times.Exactly(2));
//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ == "Is the pool with multiple answers?"),
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
//                It.Is<string>(_ => _ == Messages.Cancelled),
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
//                It.IsAny<ChatId>(),
//                It.Is<string>(_ => _ != Messages.Cancelled && _ != "Is the pool with multiple answers?" &&
//                    _ != "Please, input next case or <code>/done</code> for stop" && _ != "Please, input first case" &&
//                    _ != "Please, input a question" && _ != Messages.SelectChat),
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
//        public void PollAdminWithPermissionsAnonymousIncorrectChoice()
//        {
//            string userName = "TestUser";
//            var user1 = new Telegram.Bot.Types.User
//            {
//                Id = 1,
//                Username = userName + "1",
//            };
//            var userRepo = new SafeBot.Core.Model.User
//            {
//                Id = user1.Id,
//                Type = UserType.Admin
//            };
//            var chat1 = new Telegram.Bot.Types.Chat
//            {
//                Id = 1,
//                Type = Telegram.Bot.Types.Enums.ChatType.Private
//            };
//            var chat2 = new Telegram.Bot.Types.Chat
//            {
//                Id = 2,
//                Type = Telegram.Bot.Types.Enums.ChatType.Group
//            };
//            var chat3 = new Telegram.Bot.Types.Chat
//            {
//                Id = 3,
//                Type = Telegram.Bot.Types.Enums.ChatType.Supergroup
//            };
//            var chatRepo1 = new SafeBot.Core.Model.Chat { Id = chat1.Id, ChatType = SafeBot.Core.Model.ChatType.Private, Title = "Private" };
//            var chatRepo2 = new SafeBot.Core.Model.Chat { Id = chat2.Id, ChatType = SafeBot.Core.Model.ChatType.Public, Title = "Public" };
//            var chatRepo3 = new SafeBot.Core.Model.Chat { Id = chat3.Id, ChatType = SafeBot.Core.Model.ChatType.Admin, Title = "Admin" };
//            var chats = new SafeBot.Core.Model.Chat[] { chatRepo1, chatRepo2, chatRepo3 };
//            var message = new Message
//            {
//                Chat = chat1,
//                From = user1,
//                Text = "/poll"
//            };

//            _fixture.MockBotClient.Reset();

//            var userServiceMock = new Mock<IUserService>();
//            userServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == user1.Id)))
//                .ReturnsAsync(userRepo);
//            userServiceMock.Setup(s => s.IsAdmin(It.Is<long>(_ => _ == user1.Id)))
//                .ReturnsAsync(true);
//            _bot.UserService = userServiceMock.Object;

//            var chatServiceMock = new Mock<IChatService>();
//            chatServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == chat1.Id)))
//                .ReturnsAsync(chatRepo1);
//            chatServiceMock.Setup(s => s.Get(It.Is<string>(_ => _ == "Private")))
//                .ReturnsAsync(chatRepo1);
//            chatServiceMock.Setup(s => s.Get(It.Is<string>(_ => _ == "Public")))
//                .ReturnsAsync(chatRepo2);
//            chatServiceMock.Setup(s => s.Get(It.Is<string>(_ => _ == "Admin")))
//                .ReturnsAsync(chatRepo3);
//            chatServiceMock.Setup(s => s.GetList())
//                .ReturnsAsync(chats);
//            _bot.ChatService = chatServiceMock.Object;
//            _bot.StateMachines = new IStateMachine[]
//            {
//                new PollStateMachine(chatServiceMock.Object)
//            };

//            var userMessageServiceMock = new Mock<IUserMessageService>();
//            _bot.UserMessageService = userMessageServiceMock.Object;

//            _fixture.MockBotClient.Setup(c => c.GetChatAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(chat1);

//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Public";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Question";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "First case";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Second case";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "/done";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = Messages.Yes;
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Nain";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Other message";
//            _bot.RecieveMessage(message).Wait();

//            chatServiceMock.Verify(mock => mock.Get(It.Is<long>(_ => _ == chat1.Id)), Times.Exactly(9));
//            userMessageServiceMock.Verify(mock => mock.Add(It.IsAny<UserMessage>()), Times.Exactly(9));
//            userServiceMock.Verify(mock => mock.Get(It.IsAny<long>()), Times.Once);
//            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
//            userServiceMock.Verify(mock => mock.IsAdmin(It.IsAny<long>()), Times.Once);

//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ == Messages.SelectChat), 
//                It.IsAny<ParseMode>(),
//                It.IsAny<IEnumerable<MessageEntity>>(),
//                It.IsAny<bool>(), 
//                It.IsAny<bool>(),
//                It.IsAny<int>(),
//                It.IsAny<bool?>(),
//                It.Is<IReplyMarkup>(m => KeyboardMarkupActionButtons(m, 3)), 
//                It.IsAny<CancellationToken>()),
//                Times.Once);
//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ == "Please, input a question"),
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
//                It.Is<string>(_ => _ == "Please, input first case"),
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
//                It.Is<string>(_ => _ == "Please, input next case or <code>/done</code> for stop"),
//                It.IsAny<ParseMode>(),
//                It.IsAny<IEnumerable<MessageEntity>>(),
//                It.IsAny<bool>(),
//                It.IsAny<bool>(),
//                It.IsAny<int>(),
//                It.IsAny<bool?>(),
//                It.IsAny<IReplyMarkup>(),
//                It.IsAny<CancellationToken>()),
//                Times.Exactly(2));
//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ == "Is the pool with multiple answers?"),
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
//                It.Is<string>(_ => _ == "Is the pool anonymous?"),
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
//                It.Is<string>(_ => _ == Messages.Cancelled), 
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
//                It.IsAny<ChatId>(),
//                It.Is<string>(_ => _ != Messages.Cancelled && _ != "Is the pool with multiple answers?" &&
//                    _ != "Please, input next case or <code>/done</code> for stop" && _ != "Please, input first case" &&
//                    _ != "Please, input a question" && _ != Messages.SelectChat && _ != "Is the pool anonymous?"),
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
//        public void PollAdminWithPermissionsAnonymousChoiceCancel()
//        {
//            string userName = "TestUser";
//            var user1 = new Telegram.Bot.Types.User
//            {
//                Id = 1,
//                Username = userName + "1",
//            };
//            var userRepo = new SafeBot.Core.Model.User
//            {
//                Id = user1.Id,
//                Type = UserType.Admin
//            };
//            var chat1 = new Telegram.Bot.Types.Chat
//            {
//                Id = 1,
//                Type = Telegram.Bot.Types.Enums.ChatType.Private
//            };
//            var chat2 = new Telegram.Bot.Types.Chat
//            {
//                Id = 2,
//                Type = Telegram.Bot.Types.Enums.ChatType.Group
//            };
//            var chat3 = new Telegram.Bot.Types.Chat
//            {
//                Id = 3,
//                Type = Telegram.Bot.Types.Enums.ChatType.Supergroup
//            };
//            var chatRepo1 = new SafeBot.Core.Model.Chat { Id = chat1.Id, ChatType = SafeBot.Core.Model.ChatType.Private, Title = "Private" };
//            var chatRepo2 = new SafeBot.Core.Model.Chat { Id = chat2.Id, ChatType = SafeBot.Core.Model.ChatType.Public, Title = "Public" };
//            var chatRepo3 = new SafeBot.Core.Model.Chat { Id = chat3.Id, ChatType = SafeBot.Core.Model.ChatType.Admin, Title = "Admin" };
//            var chats = new SafeBot.Core.Model.Chat[] { chatRepo1, chatRepo2, chatRepo3 };
//            var message = new Message
//            {
//                Chat = chat1,
//                From = user1,
//                Text = "/poll"
//            };

//            _fixture.MockBotClient.Reset();

//            var userServiceMock = new Mock<IUserService>();
//            userServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == user1.Id)))
//                .ReturnsAsync(userRepo);
//            userServiceMock.Setup(s => s.IsAdmin(It.Is<long>(_ => _ == user1.Id)))
//                .ReturnsAsync(true);
//            _bot.UserService = userServiceMock.Object;

//            var chatServiceMock = new Mock<IChatService>();
//            chatServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == chat1.Id)))
//                .ReturnsAsync(chatRepo1);
//            chatServiceMock.Setup(s => s.Get(It.Is<string>(_ => _ == "Private")))
//                .ReturnsAsync(chatRepo1);
//            chatServiceMock.Setup(s => s.Get(It.Is<string>(_ => _ == "Public")))
//                .ReturnsAsync(chatRepo2);
//            chatServiceMock.Setup(s => s.Get(It.Is<string>(_ => _ == "Admin")))
//                .ReturnsAsync(chatRepo3);
//            chatServiceMock.Setup(s => s.GetList())
//                .ReturnsAsync(chats);
//            _bot.ChatService = chatServiceMock.Object;
//            _bot.StateMachines = new IStateMachine[]
//            {
//                new PollStateMachine(chatServiceMock.Object)
//            };

//            var userMessageServiceMock = new Mock<IUserMessageService>();
//            _bot.UserMessageService = userMessageServiceMock.Object;

//            _fixture.MockBotClient.Setup(c => c.GetChatAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(chat1);

//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Public";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Question";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "First case";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Second case";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "/done";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = Messages.No;
//            _bot.RecieveMessage(message).Wait();

//            message.Text = Messages.Cancel;
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Other message";
//            _bot.RecieveMessage(message).Wait();

//            chatServiceMock.Verify(mock => mock.Get(It.Is<long>(_ => _ == chat1.Id)), Times.Exactly(9));
//            userMessageServiceMock.Verify(mock => mock.Add(It.IsAny<UserMessage>()), Times.Exactly(9));
//            userServiceMock.Verify(mock => mock.Get(It.IsAny<long>()), Times.Once);
//            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
//            userServiceMock.Verify(mock => mock.IsAdmin(It.IsAny<long>()), Times.Once);

//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ == Messages.SelectChat), 
//                It.IsAny<ParseMode>(),
//                It.IsAny<IEnumerable<MessageEntity>>(),
//                It.IsAny<bool>(), 
//                It.IsAny<bool>(),
//                It.IsAny<int>(),
//                It.IsAny<bool?>(),
//                It.Is<IReplyMarkup>(m => KeyboardMarkupActionButtons(m, 3)), 
//                It.IsAny<CancellationToken>()),
//                Times.Once);
//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ == "Please, input a question"), 
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
//                It.Is<string>(_ => _ == "Please, input first case"), 
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
//                It.Is<string>(_ => _ == "Please, input next case or <code>/done</code> for stop"),
//                It.IsAny<ParseMode>(),
//                It.IsAny<IEnumerable<MessageEntity>>(),
//                It.IsAny<bool>(),
//                It.IsAny<bool>(),
//                It.IsAny<int>(),
//                It.IsAny<bool?>(),
//                It.IsAny<IReplyMarkup>(), 
//                It.IsAny<CancellationToken>()),
//                Times.Exactly(2));
//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ == "Is the pool with multiple answers?"),
//                It.IsAny<ParseMode>(),
//                It.IsAny<IEnumerable<MessageEntity>>(),
//                It.IsAny<bool>(), 
//                It.IsAny<bool>(), 
//                It.IsAny<int>(),
//                It.IsAny<bool?>(),
//                It.IsAny<IReplyMarkup>(), 
//                It.IsAny<CancellationToken>()),
//                Times.Once);
//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ == "Is the pool anonymous?"),
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
//                It.Is<string>(_ => _ == Messages.Cancelled), 
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
//                It.IsAny<ChatId>(),
//                It.Is<string>(_ => _ != Messages.Cancelled && _ != "Is the pool with multiple answers?" &&
//                    _ != "Please, input next case or <code>/done</code> for stop" && _ != "Please, input first case" &&
//                    _ != "Please, input a question" && _ != Messages.SelectChat && _ != "Is the pool anonymous?"),
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
//        public void PollAdminWithPermissionsPinIncorrectChoice()
//        {
//            string userName = "TestUser";
//            var user1 = new Telegram.Bot.Types.User
//            {
//                Id = 1,
//                Username = userName + "1",
//            };
//            var userRepo = new SafeBot.Core.Model.User
//            {
//                Id = user1.Id,
//                Type = UserType.Admin
//            };
//            var chat1 = new Telegram.Bot.Types.Chat
//            {
//                Id = 1,
//                Type = Telegram.Bot.Types.Enums.ChatType.Private
//            };
//            var chat2 = new Telegram.Bot.Types.Chat
//            {
//                Id = 2,
//                Type = Telegram.Bot.Types.Enums.ChatType.Group
//            };
//            var chat3 = new Telegram.Bot.Types.Chat
//            {
//                Id = 3,
//                Type = Telegram.Bot.Types.Enums.ChatType.Supergroup
//            };
//            var chatRepo1 = new SafeBot.Core.Model.Chat { Id = chat1.Id, ChatType = SafeBot.Core.Model.ChatType.Private, Title = "Private" };
//            var chatRepo2 = new SafeBot.Core.Model.Chat { Id = chat2.Id, ChatType = SafeBot.Core.Model.ChatType.Public, Title = "Public" };
//            var chatRepo3 = new SafeBot.Core.Model.Chat { Id = chat3.Id, ChatType = SafeBot.Core.Model.ChatType.Admin, Title = "Admin" };
//            var chats = new SafeBot.Core.Model.Chat[] { chatRepo1, chatRepo2, chatRepo3 };
//            var message = new Message
//            {
//                Chat = chat1,
//                From = user1,
//                Text = "/poll"
//            };

//            _fixture.MockBotClient.Reset();

//            var userServiceMock = new Mock<IUserService>();
//            userServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == user1.Id)))
//                .ReturnsAsync(userRepo);
//            userServiceMock.Setup(s => s.IsAdmin(It.Is<long>(_ => _ == user1.Id)))
//                .ReturnsAsync(true);
//            _bot.UserService = userServiceMock.Object;

//            var chatServiceMock = new Mock<IChatService>();
//            chatServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == chat1.Id)))
//                .ReturnsAsync(chatRepo1);
//            chatServiceMock.Setup(s => s.Get(It.Is<string>(_ => _ == "Private")))
//                .ReturnsAsync(chatRepo1);
//            chatServiceMock.Setup(s => s.Get(It.Is<string>(_ => _ == "Public")))
//                .ReturnsAsync(chatRepo2);
//            chatServiceMock.Setup(s => s.Get(It.Is<string>(_ => _ == "Admin")))
//                .ReturnsAsync(chatRepo3);
//            chatServiceMock.Setup(s => s.GetList())
//                .ReturnsAsync(chats);
//            _bot.ChatService = chatServiceMock.Object;
//            _bot.StateMachines = new IStateMachine[]
//            {
//                new PollStateMachine(chatServiceMock.Object)
//            };

//            var userMessageServiceMock = new Mock<IUserMessageService>();
//            _bot.UserMessageService = userMessageServiceMock.Object;

//            _fixture.MockBotClient.Setup(c => c.GetChatAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(chat1);

//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Public";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Question";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "First case";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Second case";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "/done";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = Messages.Yes; //multianswers
//            _bot.RecieveMessage(message).Wait();

//            message.Text = Messages.No;  //anonymous
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Nain";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Other message";
//            _bot.RecieveMessage(message).Wait();

//            chatServiceMock.Verify(mock => mock.Get(It.Is<long>(_ => _ == chat1.Id)), Times.Exactly(10));
//            userMessageServiceMock.Verify(mock => mock.Add(It.IsAny<UserMessage>()), Times.Exactly(10));
//            userServiceMock.Verify(mock => mock.Get(It.IsAny<long>()), Times.Once);
//            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
//            userServiceMock.Verify(mock => mock.IsAdmin(It.IsAny<long>()), Times.Once);

//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ == Messages.SelectChat),
//                It.IsAny<ParseMode>(),
//                It.IsAny<IEnumerable<MessageEntity>>(),
//                It.IsAny<bool>(), 
//                It.IsAny<bool>(),
//                It.IsAny<int>(),
//                It.IsAny<bool?>(),
//                It.Is<IReplyMarkup>(m => KeyboardMarkupActionButtons(m, 3)), 
//                It.IsAny<CancellationToken>()),
//                Times.Once);
//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ == "Please, input a question"),
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
//                It.Is<string>(_ => _ == "Please, input first case"), 
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
//                It.Is<string>(_ => _ == "Please, input next case or <code>/done</code> for stop"),
//                It.IsAny<ParseMode>(),
//                It.IsAny<IEnumerable<MessageEntity>>(),
//                It.IsAny<bool>(),
//                It.IsAny<bool>(), 
//                It.IsAny<int>(),
//                It.IsAny<bool?>(),
//                It.IsAny<IReplyMarkup>(),
//                It.IsAny<CancellationToken>()),
//                Times.Exactly(2));
//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ == "Is the pool with multiple answers?"),
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
//                It.Is<string>(_ => _ == "Is the pool anonymous?"),
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
//                It.Is<string>(_ => _ == "Pin the pool?"),
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
//                It.Is<string>(_ => _ == Messages.Cancelled),
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
//                It.IsAny<ChatId>(),
//                It.Is<string>(_ => _ != Messages.Cancelled && _ != "Is the pool with multiple answers?" && _ != "Pin the pool?" &&
//                    _ != "Please, input next case or <code>/done</code> for stop" && _ != "Please, input first case" &&
//                    _ != "Please, input a question" && _ != Messages.SelectChat && _ != "Is the pool anonymous?"),
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
//        public void PollAdminWithPermissionsPinChoiceCancel()
//        {
//            string userName = "TestUser";
//            var user1 = new Telegram.Bot.Types.User
//            {
//                Id = 1,
//                Username = userName + "1",
//            };
//            var userRepo = new SafeBot.Core.Model.User
//            {
//                Id = user1.Id,
//                Type = UserType.Admin
//            };
//            var chat1 = new Telegram.Bot.Types.Chat
//            {
//                Id = 1,
//                Type = Telegram.Bot.Types.Enums.ChatType.Private
//            };
//            var chat2 = new Telegram.Bot.Types.Chat
//            {
//                Id = 2,
//                Type = Telegram.Bot.Types.Enums.ChatType.Group
//            };
//            var chat3 = new Telegram.Bot.Types.Chat
//            {
//                Id = 3,
//                Type = Telegram.Bot.Types.Enums.ChatType.Supergroup
//            };
//            var chatRepo1 = new SafeBot.Core.Model.Chat { Id = chat1.Id, ChatType = SafeBot.Core.Model.ChatType.Private, Title = "Private" };
//            var chatRepo2 = new SafeBot.Core.Model.Chat { Id = chat2.Id, ChatType = SafeBot.Core.Model.ChatType.Public, Title = "Public" };
//            var chatRepo3 = new SafeBot.Core.Model.Chat { Id = chat3.Id, ChatType = SafeBot.Core.Model.ChatType.Admin, Title = "Admin" };
//            var chats = new SafeBot.Core.Model.Chat[] { chatRepo1, chatRepo2, chatRepo3 };
//            var message = new Message
//            {
//                Chat = chat1,
//                From = user1,
//                Text = "/poll"
//            };

//            _fixture.MockBotClient.Reset();

//            var userServiceMock = new Mock<IUserService>();
//            userServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == user1.Id)))
//                .ReturnsAsync(userRepo);
//            userServiceMock.Setup(s => s.IsAdmin(It.Is<long>(_ => _ == user1.Id)))
//                .ReturnsAsync(true);
//            _bot.UserService = userServiceMock.Object;

//            var chatServiceMock = new Mock<IChatService>();
//            chatServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == chat1.Id)))
//                .ReturnsAsync(chatRepo1);
//            chatServiceMock.Setup(s => s.Get(It.Is<string>(_ => _ == "Private")))
//                .ReturnsAsync(chatRepo1);
//            chatServiceMock.Setup(s => s.Get(It.Is<string>(_ => _ == "Public")))
//                .ReturnsAsync(chatRepo2);
//            chatServiceMock.Setup(s => s.Get(It.Is<string>(_ => _ == "Admin")))
//                .ReturnsAsync(chatRepo3);
//            chatServiceMock.Setup(s => s.GetList())
//                .ReturnsAsync(chats);
//            _bot.ChatService = chatServiceMock.Object;
//            _bot.StateMachines = new IStateMachine[]
//            {
//                new PollStateMachine(chatServiceMock.Object)
//            };

//            var userMessageServiceMock = new Mock<IUserMessageService>();
//            _bot.UserMessageService = userMessageServiceMock.Object;

//            _fixture.MockBotClient.Setup(c => c.GetChatAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(chat1);

//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Public";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Question";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "First case";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Second case";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "/done";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = Messages.No; //multianswers
//            _bot.RecieveMessage(message).Wait();

//            message.Text = Messages.Yes;  //anonymous
//            _bot.RecieveMessage(message).Wait();

//            message.Text = Messages.Cancel;
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Other message";
//            _bot.RecieveMessage(message).Wait();

//            chatServiceMock.Verify(mock => mock.Get(It.Is<long>(_ => _ == chat1.Id)), Times.Exactly(10));
//            userMessageServiceMock.Verify(mock => mock.Add(It.IsAny<UserMessage>()), Times.Exactly(10));
//            userServiceMock.Verify(mock => mock.Get(It.IsAny<long>()), Times.Once);
//            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
//            userServiceMock.Verify(mock => mock.IsAdmin(It.IsAny<long>()), Times.Once);

//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ == Messages.SelectChat), 
//                It.IsAny<ParseMode>(),
//                It.IsAny<IEnumerable<MessageEntity>>(),
//                It.IsAny<bool>(), 
//                It.IsAny<bool>(),
//                It.IsAny<int>(),
//                It.IsAny<bool?>(),
//                It.Is<IReplyMarkup>(m => KeyboardMarkupActionButtons(m, 3)), 
//                It.IsAny<CancellationToken>()),
//                Times.Once);
//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ == "Please, input a question"),
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
//                It.Is<string>(_ => _ == "Please, input first case"),
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
//                It.Is<string>(_ => _ == "Please, input next case or <code>/done</code> for stop"),
//                It.IsAny<ParseMode>(),
//                It.IsAny<IEnumerable<MessageEntity>>(),
//                It.IsAny<bool>(), 
//                It.IsAny<bool>(), 
//                It.IsAny<int>(),
//                It.IsAny<bool?>(),
//                It.IsAny<IReplyMarkup>(),
//                It.IsAny<CancellationToken>()),
//                Times.Exactly(2));
//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ == "Is the pool with multiple answers?"),
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
//                It.Is<string>(_ => _ == "Is the pool anonymous?"),
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
//                It.Is<string>(_ => _ == "Pin the pool?"),
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
//                It.Is<string>(_ => _ == Messages.Cancelled),
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
//                It.IsAny<ChatId>(),
//                It.Is<string>(_ => _ != Messages.Cancelled && _ != "Is the pool with multiple answers?" && _ != "Pin the pool?" &&
//                    _ != "Please, input next case or <code>/done</code> for stop" && _ != "Please, input first case" &&
//                    _ != "Please, input a question" && _ != Messages.SelectChat && _ != "Is the pool anonymous?"),
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
//        public void PollAdminWithPermissionsConfirmationIncorrectChoice()
//        {
//            string userName = "TestUser";
//            var user1 = new Telegram.Bot.Types.User
//            {
//                Id = 1,
//                Username = userName + "1",
//            };
//            var userRepo = new SafeBot.Core.Model.User
//            {
//                Id = user1.Id,
//                Type = UserType.Admin
//            };
//            var chat1 = new Telegram.Bot.Types.Chat
//            {
//                Id = 1,
//                Type = Telegram.Bot.Types.Enums.ChatType.Private
//            };
//            var chat2 = new Telegram.Bot.Types.Chat
//            {
//                Id = 2,
//                Type = Telegram.Bot.Types.Enums.ChatType.Group
//            };
//            var chat3 = new Telegram.Bot.Types.Chat
//            {
//                Id = 3,
//                Type = Telegram.Bot.Types.Enums.ChatType.Supergroup
//            };
//            var chatRepo1 = new SafeBot.Core.Model.Chat { Id = chat1.Id, ChatType = SafeBot.Core.Model.ChatType.Private, Title = "Private" };
//            var chatRepo2 = new SafeBot.Core.Model.Chat { Id = chat2.Id, ChatType = SafeBot.Core.Model.ChatType.Public, Title = "Public" };
//            var chatRepo3 = new SafeBot.Core.Model.Chat { Id = chat3.Id, ChatType = SafeBot.Core.Model.ChatType.Admin, Title = "Admin" };
//            var chats = new SafeBot.Core.Model.Chat[] { chatRepo1, chatRepo2, chatRepo3 };
//            var message = new Message
//            {
//                Chat = chat1,
//                From = user1,
//                Text = "/poll"
//            };

//            _fixture.MockBotClient.Reset();

//            var userServiceMock = new Mock<IUserService>();
//            userServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == user1.Id)))
//                .ReturnsAsync(userRepo);
//            userServiceMock.Setup(s => s.IsAdmin(It.Is<long>(_ => _ == user1.Id)))
//                .ReturnsAsync(true);
//            _bot.UserService = userServiceMock.Object;

//            var chatServiceMock = new Mock<IChatService>();
//            chatServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == chat1.Id)))
//                .ReturnsAsync(chatRepo1);
//            chatServiceMock.Setup(s => s.Get(It.Is<string>(_ => _ == "Private")))
//                .ReturnsAsync(chatRepo1);
//            chatServiceMock.Setup(s => s.Get(It.Is<string>(_ => _ == "Public")))
//                .ReturnsAsync(chatRepo2);
//            chatServiceMock.Setup(s => s.Get(It.Is<string>(_ => _ == "Admin")))
//                .ReturnsAsync(chatRepo3);
//            chatServiceMock.Setup(s => s.GetList())
//                .ReturnsAsync(chats);
//            _bot.ChatService = chatServiceMock.Object;
//            _bot.StateMachines = new IStateMachine[]
//            {
//                new PollStateMachine(chatServiceMock.Object)
//            };

//            var userMessageServiceMock = new Mock<IUserMessageService>();
//            _bot.UserMessageService = userMessageServiceMock.Object;

//            _fixture.MockBotClient.Setup(c => c.GetChatAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(chat1);

//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Public";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Question";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "First case";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Second case";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "/done";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = Messages.No;  //multianswers
//            _bot.RecieveMessage(message).Wait();

//            message.Text = Messages.Yes; //anonymous
//            _bot.RecieveMessage(message).Wait();

//            message.Text = Messages.No;  //pin
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Nain";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Other message";
//            _bot.RecieveMessage(message).Wait();

//            chatServiceMock.Verify(mock => mock.Get(It.Is<long>(_ => _ == chat1.Id)), Times.Exactly(11));
//            userMessageServiceMock.Verify(mock => mock.Add(It.IsAny<UserMessage>()), Times.Exactly(11));
//            userServiceMock.Verify(mock => mock.Get(It.IsAny<long>()), Times.Once);
//            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
//            userServiceMock.Verify(mock => mock.IsAdmin(It.IsAny<long>()), Times.Once);

//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ == Messages.SelectChat), 
//                It.IsAny<ParseMode>(),
//                It.IsAny<IEnumerable<MessageEntity>>(),
//                It.IsAny<bool>(), 
//                It.IsAny<bool>(),
//                It.IsAny<int>(),
//                It.IsAny<bool?>(),
//                It.Is<IReplyMarkup>(m => KeyboardMarkupActionButtons(m, 3)), 
//                It.IsAny<CancellationToken>()),
//                Times.Once);
//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ == "Please, input a question"), 
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
//                It.Is<string>(_ => _ == "Please, input first case"),
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
//                It.Is<string>(_ => _ == "Please, input next case or <code>/done</code> for stop"),
//                It.IsAny<ParseMode>(),
//                It.IsAny<IEnumerable<MessageEntity>>(),
//                It.IsAny<bool>(), 
//                It.IsAny<bool>(), 
//                It.IsAny<int>(),
//                It.IsAny<bool?>(),
//                It.IsAny<IReplyMarkup>(),
//                It.IsAny<CancellationToken>()),
//                Times.Exactly(2));
//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ == "Is the pool with multiple answers?"),
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
//                It.Is<string>(_ => _ == "Is the pool anonymous?"),
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
//                It.Is<string>(_ => _ == "Pin the pool?"),
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
//                It.Is<string>(_ => _ == "Confirm sending pool?"),
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
//                It.Is<string>(_ => _ == Messages.Cancelled), 
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
//                It.IsAny<ChatId>(),
//                It.Is<string>(_ => _ != Messages.Cancelled && _ != "Confirm sending pool?" && _ != "Is the pool with multiple answers?" && 
//                    _ != "Pin the pool?" && _ != "Please, input next case or <code>/done</code> for stop" && _ != "Please, input first case" &&
//                    _ != "Please, input a question" && _ != Messages.SelectChat && _ != "Is the pool anonymous?"),
//                It.IsAny<ParseMode>(),
//                It.IsAny<IEnumerable<MessageEntity>>(),
//                It.IsAny<bool>(), 
//                It.IsAny<bool>(),
//                It.IsAny<int>(),
//                It.IsAny<bool?>(),
//                It.IsAny<IReplyMarkup>(),
//                It.IsAny<CancellationToken>()),
//                Times.Never);
//            _fixture.MockBotClient.Verify(mock => mock.PinChatMessageAsync(It.IsAny<ChatId>(),
//                It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
//                Times.Never);
//        }

//        [Fact]
//        public void PollAdminWithPermissionsConfirmationChoiceCancel()
//        {
//            string userName = "TestUser";
//            var user1 = new Telegram.Bot.Types.User
//            {
//                Id = 1,
//                Username = userName + "1",
//            };
//            var userRepo = new SafeBot.Core.Model.User
//            {
//                Id = user1.Id,
//                Type = UserType.Admin
//            };
//            var chat1 = new Telegram.Bot.Types.Chat
//            {
//                Id = 1,
//                Type = Telegram.Bot.Types.Enums.ChatType.Private
//            };
//            var chat2 = new Telegram.Bot.Types.Chat
//            {
//                Id = 2,
//                Type = Telegram.Bot.Types.Enums.ChatType.Group
//            };
//            var chat3 = new Telegram.Bot.Types.Chat
//            {
//                Id = 3,
//                Type = Telegram.Bot.Types.Enums.ChatType.Supergroup
//            };
//            var chatRepo1 = new SafeBot.Core.Model.Chat { Id = chat1.Id, ChatType = SafeBot.Core.Model.ChatType.Private, Title = "Private" };
//            var chatRepo2 = new SafeBot.Core.Model.Chat { Id = chat2.Id, ChatType = SafeBot.Core.Model.ChatType.Public, Title = "Public" };
//            var chatRepo3 = new SafeBot.Core.Model.Chat { Id = chat3.Id, ChatType = SafeBot.Core.Model.ChatType.Admin, Title = "Admin" };
//            var chats = new SafeBot.Core.Model.Chat[] { chatRepo1, chatRepo2, chatRepo3 };
//            var message = new Message
//            {
//                Chat = chat1,
//                From = user1,
//                Text = "/poll"
//            };

//            _fixture.MockBotClient.Reset();

//            var userServiceMock = new Mock<IUserService>();
//            userServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == user1.Id)))
//                .ReturnsAsync(userRepo);
//            userServiceMock.Setup(s => s.IsAdmin(It.Is<long>(_ => _ == user1.Id)))
//                .ReturnsAsync(true);
//            _bot.UserService = userServiceMock.Object;

//            var chatServiceMock = new Mock<IChatService>();
//            chatServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == chat1.Id)))
//                .ReturnsAsync(chatRepo1);
//            chatServiceMock.Setup(s => s.Get(It.Is<string>(_ => _ == "Private")))
//                .ReturnsAsync(chatRepo1);
//            chatServiceMock.Setup(s => s.Get(It.Is<string>(_ => _ == "Public")))
//                .ReturnsAsync(chatRepo2);
//            chatServiceMock.Setup(s => s.Get(It.Is<string>(_ => _ == "Admin")))
//                .ReturnsAsync(chatRepo3);
//            chatServiceMock.Setup(s => s.GetList())
//                .ReturnsAsync(chats);
//            _bot.ChatService = chatServiceMock.Object;
//            _bot.StateMachines = new IStateMachine[]
//            {
//                new PollStateMachine(chatServiceMock.Object)
//            };

//            var userMessageServiceMock = new Mock<IUserMessageService>();
//            _bot.UserMessageService = userMessageServiceMock.Object;

//            _fixture.MockBotClient.Setup(c => c.GetChatAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(chat1);

//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Public";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Question";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "First case";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Second case";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "/done";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = Messages.No;  //multianswers
//            _bot.RecieveMessage(message).Wait();

//            message.Text = Messages.No; //anonymous
//            _bot.RecieveMessage(message).Wait();

//            message.Text = Messages.No;  //pin
//            _bot.RecieveMessage(message).Wait();

//            message.Text = Messages.Cancel;
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Other message";
//            _bot.RecieveMessage(message).Wait();

//            chatServiceMock.Verify(mock => mock.Get(It.Is<long>(_ => _ == chat1.Id)), Times.Exactly(11));
//            userMessageServiceMock.Verify(mock => mock.Add(It.IsAny<UserMessage>()), Times.Exactly(11));
//            userServiceMock.Verify(mock => mock.Get(It.IsAny<long>()), Times.Once);
//            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
//            userServiceMock.Verify(mock => mock.IsAdmin(It.IsAny<long>()), Times.Once);

//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ == Messages.SelectChat), 
//                It.IsAny<ParseMode>(),
//                It.IsAny<IEnumerable<MessageEntity>>(),
//                It.IsAny<bool>(),
//                It.IsAny<bool>(),
//                It.IsAny<int>(),
//                It.IsAny<bool?>(),
//                It.Is<IReplyMarkup>(m => KeyboardMarkupActionButtons(m, 3)), 
//                It.IsAny<CancellationToken>()),
//                Times.Once);
//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ == "Please, input a question"), 
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
//                It.Is<string>(_ => _ == "Please, input first case"), 
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
//                It.Is<string>(_ => _ == "Please, input next case or <code>/done</code> for stop"),
//                It.IsAny<ParseMode>(),
//                It.IsAny<IEnumerable<MessageEntity>>(),
//                It.IsAny<bool>(), 
//                It.IsAny<bool>(), 
//                It.IsAny<int>(),
//                It.IsAny<bool?>(),
//                It.IsAny<IReplyMarkup>(),
//                It.IsAny<CancellationToken>()),
//                Times.Exactly(2));
//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ == "Is the pool with multiple answers?"),
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
//                It.Is<string>(_ => _ == "Is the pool anonymous?"),
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
//                It.Is<string>(_ => _ == "Pin the pool?"),
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
//                It.Is<string>(_ => _ == "Confirm sending pool?"),
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
//                It.Is<string>(_ => _ == Messages.Cancelled), 
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
//                It.IsAny<ChatId>(),
//                It.Is<string>(_ => _ != Messages.Cancelled && _ != "Confirm sending pool?" && _ != "Is the pool with multiple answers?" &&
//                    _ != "Pin the pool?" && _ != "Please, input next case or <code>/done</code> for stop" && _ != "Please, input first case" &&
//                    _ != "Please, input a question" && _ != Messages.SelectChat && _ != "Is the pool anonymous?"),
//                It.IsAny<ParseMode>(),
//                It.IsAny<IEnumerable<MessageEntity>>(),
//                It.IsAny<bool>(), 
//                It.IsAny<bool>(), 
//                It.IsAny<int>(),
//                It.IsAny<bool?>(),
//                It.IsAny<IReplyMarkup>(),
//                It.IsAny<CancellationToken>()), 
//                Times.Never);
//            _fixture.MockBotClient.Verify(mock => mock.PinChatMessageAsync(It.IsAny<ChatId>(),
//                It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
//                Times.Never);
//        }

//        [Fact]
//        public void PollAdminWithPermissionsConfirmedPinned()
//        {
//            string userName = "TestUser";
//            var user1 = new Telegram.Bot.Types.User
//            {
//                Id = 1,
//                Username = userName + "1",
//            };
//            var userRepo = new SafeBot.Core.Model.User
//            {
//                Id = user1.Id,
//                Type = UserType.Admin
//            };
//            var chat1 = new Telegram.Bot.Types.Chat
//            {
//                Id = 1,
//                Type = Telegram.Bot.Types.Enums.ChatType.Private
//            };
//            var chat2 = new Telegram.Bot.Types.Chat
//            {
//                Id = 2,
//                Type = Telegram.Bot.Types.Enums.ChatType.Group
//            };
//            var chat3 = new Telegram.Bot.Types.Chat
//            {
//                Id = 3,
//                Type = Telegram.Bot.Types.Enums.ChatType.Supergroup
//            };
//            var chatRepo1 = new SafeBot.Core.Model.Chat { Id = chat1.Id, ChatType = SafeBot.Core.Model.ChatType.Private, Title = "Private" };
//            var chatRepo2 = new SafeBot.Core.Model.Chat { Id = chat2.Id, ChatType = SafeBot.Core.Model.ChatType.Public, Title = "Public" };
//            var chatRepo3 = new SafeBot.Core.Model.Chat { Id = chat3.Id, ChatType = SafeBot.Core.Model.ChatType.Admin, Title = "Admin" };
//            var chats = new SafeBot.Core.Model.Chat[] { chatRepo1, chatRepo2, chatRepo3 };
//            var message = new Message
//            {
//                Chat = chat1,
//                From = user1,
//                Text = "/poll"
//            };
//            var message2 = new Message
//            {
//                Chat = chat2,
//                From = user1
//            };
//            _fixture.MockBotClient.Reset();

//            var userServiceMock = new Mock<IUserService>();
//            userServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == user1.Id)))
//                .ReturnsAsync(userRepo);
//            userServiceMock.Setup(s => s.IsAdmin(It.Is<long>(_ => _ == user1.Id)))
//                .ReturnsAsync(true);
//            _bot.UserService = userServiceMock.Object;

//            var chatServiceMock = new Mock<IChatService>();
//            chatServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == chat1.Id)))
//                .ReturnsAsync(chatRepo1);
//            chatServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == chat2.Id)))
//                .ReturnsAsync(chatRepo2);
//            chatServiceMock.Setup(s => s.Get(It.Is<string>(_ => _ == "Private")))
//                .ReturnsAsync(chatRepo2);
//            chatServiceMock.Setup(s => s.Get(It.Is<string>(_ => _ == "Public")))
//                .ReturnsAsync(chatRepo2);
//            chatServiceMock.Setup(s => s.Get(It.Is<string>(_ => _ == "Admin")))
//                .ReturnsAsync(chatRepo3);
//            chatServiceMock.Setup(s => s.GetList())
//                .ReturnsAsync(chats);
//            _bot.ChatService = chatServiceMock.Object;
//            _bot.StateMachines = new IStateMachine[]
//            {
//                new PollStateMachine(chatServiceMock.Object)
//            };

//            var userMessageServiceMock = new Mock<IUserMessageService>();
//            _bot.UserMessageService = userMessageServiceMock.Object;

//            _fixture.MockBotClient.Setup(c => c.GetChatAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(chat1);
//            _fixture.MockBotClient.Setup(c => c.GetChatAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat2.Id), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(chat2);
//            _fixture.MockBotClient.Setup(c => c.SendPollAsync(
//                    It.IsAny<ChatId>(),             //chatId
//                    It.IsAny<string>(),             //question
//                    It.IsAny<IEnumerable<string>>(), //options
//                    It.IsAny<bool>(),               //isAnonymous
//                    It.IsAny<PollType?>(),          //type
//                    It.IsAny<bool?>(),              //allowsMultipleAnswers
//                    It.IsAny<int>(),                //correctOptionId
//                    It.IsAny<string>(),             //explanation
//                    It.IsAny<ParseMode?>(),         //explanationParseMode
//                    It.IsAny<IEnumerable<MessageEntity>>(), //explanationEntities
//                    It.IsAny<int?>(),               //openPeriod
//                    It.IsAny<System.DateTime?>(),   //closeDate
//                    It.IsAny<bool?>(),              //isClosed
//                    It.IsAny<bool?>(),              //disableNotification
//                    It.IsAny<int?>(),               //replyToMessageId
//                    It.IsAny<bool?>(),              //allowSendingWithoutReply
//                    It.IsAny<IReplyMarkup>(),       //replyMarkup
//                    It.IsAny<CancellationToken>()))  //cancellationToken
//                .ReturnsAsync(message2);

//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Public";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Question";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "First case";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Second case";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Third case";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "/done";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = Messages.Yes;  //multianswers
//            _bot.RecieveMessage(message).Wait();

//            message.Text = Messages.Yes; //anonymous
//            _bot.RecieveMessage(message).Wait();

//            message.Text = Messages.Yes;  //pin
//            _bot.RecieveMessage(message).Wait();

//            message.Text = Messages.OK;
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Other message";
//            _bot.RecieveMessage(message).Wait();

//            chatServiceMock.Verify(mock => mock.Get(It.Is<long>(_ => _ == chat1.Id)), Times.Exactly(12));
//            userMessageServiceMock.Verify(mock => mock.Add(It.IsAny<UserMessage>()), Times.Exactly(13));
//            userServiceMock.Verify(mock => mock.Get(It.IsAny<long>()), Times.Once);
//            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
//            userServiceMock.Verify(mock => mock.IsAdmin(It.IsAny<long>()), Times.Once);

//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ == Messages.SelectChat), 
//                It.IsAny<ParseMode>(),
//                It.IsAny<IEnumerable<MessageEntity>>(),
//                It.IsAny<bool>(),
//                It.IsAny<bool>(),
//                It.IsAny<int>(),
//                It.IsAny<bool?>(),
//                It.Is<IReplyMarkup>(m => KeyboardMarkupActionButtons(m, 3)), 
//                It.IsAny<CancellationToken>()),
//                Times.Once);
//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ == "Please, input a question"), 
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
//                It.Is<string>(_ => _ == "Please, input first case"),
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
//                It.Is<string>(_ => _ == "Please, input next case or <code>/done</code> for stop"),
//                It.IsAny<ParseMode>(),
//                It.IsAny<IEnumerable<MessageEntity>>(),
//                It.IsAny<bool>(),
//                It.IsAny<bool>(), 
//                It.IsAny<int>(),
//                It.IsAny<bool?>(),
//                It.IsAny<IReplyMarkup>(),
//                It.IsAny<CancellationToken>()),
//                Times.Exactly(3));
//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ == "Is the pool with multiple answers?"),
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
//                It.Is<string>(_ => _ == "Is the pool anonymous?"),
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
//                It.Is<string>(_ => _ == "Pin the pool?"),
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
//                It.Is<string>(_ => _ == "Confirm sending pool?"),
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
//                It.Is<string>(_ => _ == Messages.Cancelled),
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
//                It.IsAny<ChatId>(),
//                It.Is<string>(_ => _ != Messages.Cancelled && _ != "Confirm sending pool?" && _ != "Is the pool with multiple answers?" &&
//                    _ != "Pin the pool?" && _ != "Please, input next case or <code>/done</code> for stop" && _ != "Please, input first case" &&
//                    _ != "Please, input a question" && _ != Messages.SelectChat && _ != "Is the pool anonymous?"),
//                It.IsAny<ParseMode>(),
//                It.IsAny<IEnumerable<MessageEntity>>(),
//                It.IsAny<bool>(), 
//                It.IsAny<bool>(), 
//                It.IsAny<int>(),
//                It.IsAny<bool?>(),
//                It.IsAny<IReplyMarkup>(),
//                It.IsAny<CancellationToken>()),
//                Times.Never);
//            _fixture.MockBotClient.Verify(mock => mock.PinChatMessageAsync(
//                It.IsAny<ChatId>(),
//                It.IsAny<int>(), 
//                It.IsAny<bool>(), 
//                It.IsAny<CancellationToken>()),
//                Times.Once);
//            _fixture.MockBotClient.Verify(mock => mock.SendPollAsync(
//                It.Is<ChatId>(_ => _.Identifier == 2),
//                It.Is<string>(_ => _ == "Question"), 
//                It.Is<IEnumerable<string>>( _ => _.Count() == 3),
//                    It.IsAny<bool>(),               //isAnonymous
//                    It.IsAny<PollType?>(),          //type
//                    It.IsAny<bool?>(),              //allowsMultipleAnswers
//                    It.IsAny<int>(),                //correctOptionId
//                    It.IsAny<string>(),             //explanation
//                    It.IsAny<ParseMode>(),         //explanationParseMode
//                    It.IsAny<IEnumerable<MessageEntity>>(), //explanationEntities
//                    It.IsAny<int?>(),               //openPeriod
//                    It.IsAny<System.DateTime?>(),   //closeDate
//                    It.IsAny<bool?>(),              //isClosed
//                    It.IsAny<bool?>(),              //disableNotification
//                    It.IsAny<int?>(),               //replyToMessageId
//                    It.IsAny<bool?>(),              //allowSendingWithoutReply
//                    It.IsAny<IReplyMarkup>(),       //replyMarkup
//                    It.IsAny<CancellationToken>())  //cancellationToken, 
//                ,Times.Once);
//        }

//        [Fact]
//        public void PollAdminWithPermissionsConfirmedNotPinned()
//        {
//            string userName = "TestUser";
//            var user1 = new Telegram.Bot.Types.User
//            {
//                Id = 1,
//                Username = userName + "1",
//            };
//            var userRepo = new SafeBot.Core.Model.User
//            {
//                Id = user1.Id,
//                Type = UserType.Admin
//            };
//            var chat1 = new Telegram.Bot.Types.Chat
//            {
//                Id = 1,
//                Type = Telegram.Bot.Types.Enums.ChatType.Private
//            };
//            var chat2 = new Telegram.Bot.Types.Chat
//            {
//                Id = 2,
//                Type = Telegram.Bot.Types.Enums.ChatType.Group
//            };
//            var chat3 = new Telegram.Bot.Types.Chat
//            {
//                Id = 3,
//                Type = Telegram.Bot.Types.Enums.ChatType.Supergroup
//            };
//            var chatRepo1 = new SafeBot.Core.Model.Chat { Id = chat1.Id, ChatType = SafeBot.Core.Model.ChatType.Private, Title = "Private" };
//            var chatRepo2 = new SafeBot.Core.Model.Chat { Id = chat2.Id, ChatType = SafeBot.Core.Model.ChatType.Public, Title = "Public" };
//            var chatRepo3 = new SafeBot.Core.Model.Chat { Id = chat3.Id, ChatType = SafeBot.Core.Model.ChatType.Admin, Title = "Admin" };
//            var chats = new SafeBot.Core.Model.Chat[] { chatRepo1, chatRepo2, chatRepo3 };
//            var message = new Message
//            {
//                Chat = chat1,
//                From = user1,
//                Text = "/poll"
//            };
//            var message2 = new Message
//            {
//                Chat = chat2,
//                From = user1
//            };
//            _fixture.MockBotClient.Reset();

//            var userServiceMock = new Mock<IUserService>();
//            userServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == user1.Id)))
//                .ReturnsAsync(userRepo);
//            userServiceMock.Setup(s => s.IsAdmin(It.Is<long>(_ => _ == user1.Id)))
//                .ReturnsAsync(true);
//            _bot.UserService = userServiceMock.Object;

//            var chatServiceMock = new Mock<IChatService>();
//            chatServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == chat1.Id)))
//                .ReturnsAsync(chatRepo1);
//            chatServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == chat2.Id)))
//                .ReturnsAsync(chatRepo2);
//            chatServiceMock.Setup(s => s.Get(It.Is<string>(_ => _ == "Private")))
//                .ReturnsAsync(chatRepo2);
//            chatServiceMock.Setup(s => s.Get(It.Is<string>(_ => _ == "Public")))
//                .ReturnsAsync(chatRepo2);
//            chatServiceMock.Setup(s => s.Get(It.Is<string>(_ => _ == "Admin")))
//                .ReturnsAsync(chatRepo3);
//            chatServiceMock.Setup(s => s.GetList())
//                .ReturnsAsync(chats);
//            _bot.ChatService = chatServiceMock.Object;
//            _bot.StateMachines = new IStateMachine[]
//            {
//                new PollStateMachine(chatServiceMock.Object)
//            };

//            var userMessageServiceMock = new Mock<IUserMessageService>();
//            _bot.UserMessageService = userMessageServiceMock.Object;

//            _fixture.MockBotClient.Setup(c => c.GetChatAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(chat1);
//            _fixture.MockBotClient.Setup(c => c.GetChatAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat2.Id), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(chat2);
//            _fixture.MockBotClient.Setup(c => c.SendPollAsync(
//                    It.IsAny<ChatId>(),             //chatId
//                    It.IsAny<string>(),             //question
//                    It.IsAny<IEnumerable<string>>(), //options
//                    It.IsAny<bool>(),               //isAnonymous
//                    It.IsAny<PollType?>(),          //type
//                    It.IsAny<bool?>(),              //allowsMultipleAnswers
//                    It.IsAny<int>(),                //correctOptionId
//                    It.IsAny<string>(),             //explanation
//                    It.IsAny<ParseMode?>(),         //explanationParseMode
//                    It.IsAny<IEnumerable<MessageEntity>>(), //explanationEntities
//                    It.IsAny<int?>(),               //openPeriod
//                    It.IsAny<System.DateTime?>(),   //closeDate
//                    It.IsAny<bool?>(),              //isClosed
//                    It.IsAny<bool?>(),              //disableNotification
//                    It.IsAny<int?>(),               //replyToMessageId
//                    It.IsAny<bool?>(),              //allowSendingWithoutReply
//                    It.IsAny<IReplyMarkup>(),       //replyMarkup
//                    It.IsAny<CancellationToken>()))  //cancellationToken
//                .ReturnsAsync(message2);

//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Public";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Question";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "First case";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Second case";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Third case";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "/done";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = Messages.No;  //multianswers
//            _bot.RecieveMessage(message).Wait();

//            message.Text = Messages.No; //anonymous
//            _bot.RecieveMessage(message).Wait();

//            message.Text = Messages.No;  //pin
//            _bot.RecieveMessage(message).Wait();

//            message.Text = Messages.OK;
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Other message";
//            _bot.RecieveMessage(message).Wait();

//            chatServiceMock.Verify(mock => mock.Get(It.Is<long>(_ => _ == chat1.Id)), Times.Exactly(12));
//            userMessageServiceMock.Verify(mock => mock.Add(It.IsAny<UserMessage>()), Times.Exactly(13));
//            userServiceMock.Verify(mock => mock.Get(It.IsAny<long>()), Times.Once);
//            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
//            userServiceMock.Verify(mock => mock.IsAdmin(It.IsAny<long>()), Times.Once);

//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ == Messages.SelectChat), 
//                It.IsAny<ParseMode>(),
//                It.IsAny<IEnumerable<MessageEntity>>(),
//                It.IsAny<bool>(), 
//                It.IsAny<bool>(),
//                It.IsAny<int>(),
//                It.IsAny<bool?>(),
//                It.Is<IReplyMarkup>(m => KeyboardMarkupActionButtons(m, 3)), 
//                It.IsAny<CancellationToken>()),
//                Times.Once);
//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ == "Please, input a question"), 
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
//                It.Is<string>(_ => _ == "Please, input first case"),
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
//                It.Is<string>(_ => _ == "Please, input next case or <code>/done</code> for stop"),
//                It.IsAny<ParseMode>(),
//                It.IsAny<IEnumerable<MessageEntity>>(),
//                It.IsAny<bool>(),
//                It.IsAny<bool>(), 
//                It.IsAny<int>(),
//                It.IsAny<bool?>(),
//                It.IsAny<IReplyMarkup>(),
//                It.IsAny<CancellationToken>()),
//                Times.Exactly(3));
//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ == "Is the pool with multiple answers?"),
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
//                It.Is<string>(_ => _ == "Is the pool anonymous?"),
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
//                It.Is<string>(_ => _ == "Pin the pool?"),
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
//                It.Is<string>(_ => _ == "Confirm sending pool?"),
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
//                It.Is<string>(_ => _ == Messages.Cancelled), 
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
//                It.IsAny<ChatId>(),
//                It.Is<string>(_ => _ != Messages.Cancelled && _ != "Confirm sending pool?" && _ != "Is the pool with multiple answers?" &&
//                    _ != "Pin the pool?" && _ != "Please, input next case or <code>/done</code> for stop" && _ != "Please, input first case" &&
//                    _ != "Please, input a question" && _ != Messages.SelectChat && _ != "Is the pool anonymous?"),
//                It.IsAny<ParseMode>(),
//                It.IsAny<IEnumerable<MessageEntity>>(),
//                It.IsAny<bool>(), 
//                It.IsAny<bool>(), 
//                It.IsAny<int>(),
//                It.IsAny<bool?>(),
//                It.IsAny<IReplyMarkup>(),
//                It.IsAny<CancellationToken>()), 
//                Times.Never);
//            _fixture.MockBotClient.Verify(mock => mock.PinChatMessageAsync(It.IsAny<ChatId>(),
//                It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
//                Times.Never);
//            _fixture.MockBotClient.Verify(mock => mock.SendPollAsync(
//                It.Is<ChatId>(_ => _.Identifier == 2),
//                It.Is<string>(_ => _ == "Question"), 
//                It.Is<IEnumerable<string>>(_ => _.Count() == 3),
//                    It.IsAny<bool>(),               //isAnonymous
//                    It.IsAny<PollType?>(),          //type
//                    It.IsAny<bool?>(),              //allowsMultipleAnswers
//                    It.IsAny<int>(),                //correctOptionId
//                    It.IsAny<string>(),             //explanation
//                    It.IsAny<ParseMode?>(),         //explanationParseMode
//                    It.IsAny<IEnumerable<MessageEntity>>(), //explanationEntities
//                    It.IsAny<int?>(),               //openPeriod
//                    It.IsAny<System.DateTime?>(),   //closeDate
//                    It.IsAny<bool?>(),              //isClosed
//                    It.IsAny<bool?>(),              //disableNotification
//                    It.IsAny<int?>(),               //replyToMessageId
//                    It.IsAny<bool?>(),              //allowSendingWithoutReply
//                    It.IsAny<IReplyMarkup>(),       //replyMarkup
//                    It.IsAny<CancellationToken>())  //cancellationToken, 
//            , Times.Once);
//        }


//        private bool KeyboardMarkupActionButtons(IReplyMarkup markup, int buttonsCount)
//        {
//            return markup is ReplyKeyboardMarkup keyboardMarkup &&
//                    keyboardMarkup.Keyboard.Any(k =>  k.Any(b => b.Text == Messages.Cancel)) &&
//                    (keyboardMarkup.Keyboard.Count() == buttonsCount || 
//                    keyboardMarkup.Keyboard.First().Count() == buttonsCount);
//        }
//    }
//}
