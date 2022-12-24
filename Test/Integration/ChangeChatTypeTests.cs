//using Moq;
//using System.Linq;
//using System.Threading;
//using Telegram.SafeBot.Bot.Interface;
//using Telegram.SafeBot.Bot.StateMachines;
//using Telegram.SafeBot.Core.Constant;
//using Telegram.SafeBot.Core.Model;
//using Telegram.SafeBot.Service.Interface;
//using Telegram.Bot;
//using Telegram.Bot.Types;
//using Telegram.Bot.Types.ReplyMarkups;
//using Telegram.Bot.Types.Enums;
//using Xunit;
//using System.Collections.Generic;

//namespace Telegram.SafeBot.Test.Integration
//{
//    public class ChangeChatTypeTests : IClassFixture<BotFixture>
//    {
//        private readonly BotFixture _fixture = null;
//        private readonly SafeBot.Bot.Bot _bot = null;

//        public ChangeChatTypeTests(BotFixture fixture)
//        {
//            _fixture = fixture;
//            _bot = fixture.Bot;
//        }

//        [Fact]
//        public void ChangeChatTypeNonAdmin()
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
//                Type = SafeBot.Core.Model.UserType.Member
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
//                Text = "/changechattype"
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
//            chatServiceMock.Verify(mock => mock.Update(It.IsAny<long>(), 
//                It.IsAny<SafeBot.Core.Model.Chat>()), Times.Never);
//            chatServiceMock.Verify(mock => mock.Update(
//                It.IsAny<SafeBot.Core.Model.Chat>()), Times.Never);
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
//        public void ChangeChatTypeAdminWithoutPermissions()
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
//                Text = "/changechattype"
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
//            chatServiceMock.Verify(mock => mock.Update(It.IsAny<long>(),
//                It.IsAny<SafeBot.Core.Model.Chat>()), Times.Never);
//            chatServiceMock.Verify(mock => mock.Update(
//                It.IsAny<SafeBot.Core.Model.Chat>()), Times.Never);
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
//        public void ChangeChatTypeAdminWithPermissionsCancel()
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
//                Text = "/changechattype"
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
//            _bot.StateMachines = new IStateMachine[] { new ChangeChatTypeStateMachine(chatServiceMock.Object) };

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
//            chatServiceMock.Verify(mock => mock.Update(It.IsAny<long>(),
//                It.IsAny<SafeBot.Core.Model.Chat>()), Times.Never);
//            chatServiceMock.Verify(mock => mock.Update(
//                It.IsAny<SafeBot.Core.Model.Chat>()), Times.Never);
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
//        public void ChangeChatTypeAdminWithPermissionsIncorrectChatChoice()
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
//                Text = "/changechattype"
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
//            _bot.StateMachines = new IStateMachine[] { new ChangeChatTypeStateMachine (chatServiceMock.Object) };

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
//                It.IsAny<bool>(),
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
//                It.IsAny<bool>(),
//                It.IsAny<IReplyMarkup>(), 
//                It.IsAny<CancellationToken>()),
//                Times.Never);
//        }

//        [Fact]
//        public void ChangeChatTypeAdminWithPermissionsCancelInConfirmaion()
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
//                Text = "/changechattype"
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
//            _bot.StateMachines = new IStateMachine[] { new ChangeChatTypeStateMachine(chatServiceMock.Object) };

//            var userMessageServiceMock = new Mock<IUserMessageService>();
//            _bot.UserMessageService = userMessageServiceMock.Object;

//            _fixture.MockBotClient.Setup(c => c.GetChatAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(chat1);

//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Public";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = Messages.Cancel;
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Nain";
//            _bot.RecieveMessage(message).Wait();


//            chatServiceMock.Verify(mock => mock.Get(It.Is<long>(_ => _ == chat1.Id)), Times.Exactly(4));
//            chatServiceMock.Verify(mock => mock.Update(It.IsAny<long>(),
//                It.IsAny<SafeBot.Core.Model.Chat>()), Times.Never);
//            chatServiceMock.Verify(mock => mock.Update(
//                It.IsAny<SafeBot.Core.Model.Chat>()), Times.Never);
//            userMessageServiceMock.Verify(mock => mock.Add(It.IsAny<UserMessage>()), Times.Exactly(4));
//            userMessageServiceMock.Verify(mock => mock.Delete(It.IsAny<long>()), Times.Never);
//            userServiceMock.Verify(mock => mock.Get(It.IsAny<long>()), Times.Once);
//            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
//            userServiceMock.Verify(mock => mock.IsAdmin(It.IsAny<long>()), Times.Once);

//            string confirmText = "Confirm changing to <b>Admin</b>?";
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
//                It.Is<string>(_ => _ == confirmText), 
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
//                It.Is<string>(_ => _ != Messages.Cancelled && _ != Messages.SelectChat && _ != confirmText),
//                It.IsAny<ParseMode>(),
//                It.IsAny<IEnumerable<MessageEntity>>(),
//                It.IsAny<bool>(), 
//                It.IsAny<bool>(), 
//                It.IsAny<int>(),
//                It.IsAny<bool?>(),
//                It.IsAny<IReplyMarkup>(),
//                It.IsAny<CancellationToken>()), Times.Never);
//            _fixture.MockBotClient.Verify(mock => mock.DeleteMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat2.Id),
//                It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
//        }

//        [Fact]
//        public void ChangeChatTypeAdminWithPermissionsConfirmed()
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
//                Id = 3324252,
//                Type = Telegram.Bot.Types.Enums.ChatType.Private
//            };
//            var chat2 = new Telegram.Bot.Types.Chat
//            {
//                Id = -123324252,
//                Type = Telegram.Bot.Types.Enums.ChatType.Group
//            };
//            var chat3 = new Telegram.Bot.Types.Chat
//            {
//                Id = 41252,
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
//                Text = "/changechattype"
//            };

//            var userMessage1 = new UserMessage
//            {
//                Id = 1,
//                ChatId = chat1.Id
//            };
//            var userMessage2 = new UserMessage
//            {
//                Id = 2,
//                ChatId = chat2.Id
//            };
//            var userMessage3 = new UserMessage
//            {
//                Id = 3,
//                ChatId = chat3.Id
//            };
//            var userMessage4 = new UserMessage
//            {
//                Id = 4,
//                ChatId = chat3.Id
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
//            chatServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == chat3.Id)))
//                .ReturnsAsync(chatRepo3);       
//            chatServiceMock.Setup(s => s.Get(It.Is<string>(_ => _ == "Private")))
//                .ReturnsAsync(chatRepo1);
//            chatServiceMock.Setup(s => s.Get(It.Is<string>(_ => _ == "Public")))
//                .ReturnsAsync(chatRepo2);
//            chatServiceMock.Setup(s => s.Get(It.Is<string>(_ => _ == "Admin")))
//                .ReturnsAsync(chatRepo3);
//            chatServiceMock.Setup(s => s.GetList())
//                .ReturnsAsync(chats);
//            _bot.ChatService = chatServiceMock.Object;
//            _bot.StateMachines = new IStateMachine[] { new ChangeChatTypeStateMachine(chatServiceMock.Object) };

//            var userMessageServiceMock = new Mock<IUserMessageService>();
//            userMessageServiceMock.Setup(s => s.GetList())
//                .ReturnsAsync(new UserMessage[] { userMessage1, userMessage2, userMessage3, userMessage4 });
//            _bot.UserMessageService = userMessageServiceMock.Object;

//            _fixture.MockBotClient.Setup(c => c.GetChatAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(chat1);

//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Admin";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = Messages.OK;
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Nain";
//            _bot.RecieveMessage(message).Wait();

//            string confirmText = "Confirm changing to <b>Public</b>?";
//            string changedText = "Chat type changed to Public";
//            chatServiceMock.Verify(mock => mock.Get(It.Is<long>(_ => _ == chat1.Id)), Times.Exactly(4));
//            chatServiceMock.Verify(mock => mock.Update(It.IsAny<long>(),
//                It.IsAny<SafeBot.Core.Model.Chat>()), Times.Never);
//            chatServiceMock.Verify(mock => mock.Update(
//                It.IsAny<SafeBot.Core.Model.Chat>()), Times.Once);
//            userMessageServiceMock.Verify(mock => mock.Add(It.IsAny<UserMessage>()), Times.Exactly(4));
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
//                It.Is<string>(_ => _ == confirmText), 
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
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ == changedText), 
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
//                It.Is<string>(_ => _ != changedText && _ != Messages.Cancelled && 
//                _ != Messages.SelectChat && _ != confirmText), 
//                It.IsAny<ParseMode>(),
//                It.IsAny<IEnumerable<MessageEntity>>(),
//                It.IsAny<bool>(), 
//                It.IsAny<bool>(), 
//                It.IsAny<int>(),
//                It.IsAny<bool?>(),
//                It.IsAny<IReplyMarkup>(),
//                It.IsAny<CancellationToken>()), Times.Never);
//        }

//        private bool KeyboardMarkupActionButtons(IReplyMarkup markup, int buttonsCount)
//        {
//            return markup is ReplyKeyboardMarkup keyboardMarkup &&
//                    keyboardMarkup.Keyboard.Any(k => k.Any(b => b.Text == Messages.Cancel)) &&
//                    (keyboardMarkup.Keyboard.Count() == buttonsCount ||
//                    keyboardMarkup.Keyboard.First().Count() == buttonsCount);
//        }

//    }
//}
