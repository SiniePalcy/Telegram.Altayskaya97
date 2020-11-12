using Moq;
using System.Threading;
using Telegram.Altayskaya97.Core.Constant;
using Telegram.Altayskaya97.Service.Interface;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Xunit;
using System.Linq;
using Telegram.Altayskaya97.Core.Model;
using Telegram.Bot.Types.InputFiles;
using Telegram.Altayskaya97.Bot.StateMachines;

namespace Telegram.Altayskaya97.Test.Bot.Commands
{
    public class PostTests : IClassFixture<BotFixture>
    {
        private readonly BotFixture _fixture = null;
        private readonly Altayskaya97.Bot.Bot _bot = null;

        public PostTests(BotFixture fixture)
        {
            _fixture = fixture;
            _bot = fixture.Bot;
        }

        [Fact]
        public void PostNonAdmin()
        {
            string userName = "TestUser";
            var user1 = new Telegram.Bot.Types.User
            {
                Id = 1,
                Username = userName + "1",
            };
            var userRepo = new Core.Model.User
            {
                Id = user1.Id,
                Type = UserType.Member
            };
            var chat = new Telegram.Bot.Types.Chat
            {
                Id = 1,
                Type = Telegram.Bot.Types.Enums.ChatType.Private
            };
            var chatRepo = _fixture.ChatMapper.MapToEntity(chat);
            var message = new Message
            {
                Chat = chat,
                From = user1,
                Text = "/post"
            };

            _fixture.MockBotClient.Reset();

            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(s => s.GetUser(It.Is<long>(_ => _ == user1.Id)))
                .ReturnsAsync(userRepo);
            _bot.UserService = userServiceMock.Object;

            var chatServiceMock = new Mock<IChatService>();
            chatServiceMock.Setup(s => s.GetChat(It.Is<long>(_ => _ == chat.Id)))
                .ReturnsAsync(chatRepo);
            _bot.ChatService = chatServiceMock.Object;

            var userMessageServiceMock = new Mock<IUserMessageService>();
            _bot.UserMessageService = userMessageServiceMock.Object;

            _bot.RecieveMessage(message).Wait();

            chatServiceMock.Verify(mock => mock.GetChat(It.Is<long>(_ => _ == chat.Id)), Times.Once);
            userMessageServiceMock.Verify(mock => mock.AddUserMessage(It.IsAny<UserMessage>()), Times.Once);
            userServiceMock.Verify(mock => mock.GetUser(It.IsAny<long>()), Times.Once);
            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
            userServiceMock.Verify(mock => mock.IsAdmin(It.IsAny<long>()), Times.Never);

            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat.Id),
                It.IsAny<string>(), It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<int>(), It.IsAny<IReplyMarkup>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public void PostAdminWithoutPermissions()
        {
            string userName = "TestUser";
            var user1 = new Telegram.Bot.Types.User
            {
                Id = 1,
                Username = userName + "1",
            };
            var userRepo = new Core.Model.User
            {
                Id = user1.Id,
                Type = UserType.Admin
            };
            var chat = new Telegram.Bot.Types.Chat
            {
                Id = 1,
                Type = Telegram.Bot.Types.Enums.ChatType.Private
            };
            var chatRepo = _fixture.ChatMapper.MapToEntity(chat);
            var message = new Message
            {
                Chat = chat,
                From = user1,
                Text = "/post"
            };

            _fixture.MockBotClient.Reset();

            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(s => s.GetUser(It.Is<long>(_ => _ == user1.Id)))
                .ReturnsAsync(userRepo);
            userServiceMock.Setup(s => s.IsAdmin(It.Is<long>(_ => _ == user1.Id)))
                .ReturnsAsync(false);
            _bot.UserService = userServiceMock.Object;

            var chatServiceMock = new Mock<IChatService>();
            chatServiceMock.Setup(s => s.GetChat(It.Is<long>(_ => _ == chat.Id)))
                .ReturnsAsync(chatRepo);
            _bot.ChatService = chatServiceMock.Object;

            var userMessageServiceMock = new Mock<IUserMessageService>();
            _bot.UserMessageService = userMessageServiceMock.Object;

            _fixture.MockBotClient.Setup(c => c.GetChatAsync(
                It.Is<ChatId>(_ => _.Identifier == chat.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(chat);

            _bot.RecieveMessage(message).Wait();

            chatServiceMock.Verify(mock => mock.GetChat(It.Is<long>(_ => _ == chat.Id)), Times.Once);
            userMessageServiceMock.Verify(mock => mock.AddUserMessage(It.IsAny<UserMessage>()), Times.Once);
            userServiceMock.Verify(mock => mock.GetUser(It.IsAny<long>()), Times.Once);
            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
            userServiceMock.Verify(mock => mock.IsAdmin(It.IsAny<long>()), Times.Once);

            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat.Id),
                It.Is<string>(_ => _ == Messages.NoPermissions), It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<int>(), It.IsAny<IReplyMarkup>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public void PostAdminWithPermissionsPostCancel()
        {
            string userName = "TestUser";
            var user1 = new Telegram.Bot.Types.User
            {
                Id = 1,
                Username = userName + "1",
            };
            var userRepo = new Core.Model.User
            {
                Id = user1.Id,
                Type = UserType.Admin
            };
            var chat1 = new Telegram.Bot.Types.Chat
            {
                Id = 1,
                Type = Telegram.Bot.Types.Enums.ChatType.Private
            };
            var chat2 = new Telegram.Bot.Types.Chat
            {
                Id = 2,
                Type = Telegram.Bot.Types.Enums.ChatType.Group
            };
            var chat3 = new Telegram.Bot.Types.Chat
            {
                Id = 3,
                Type = Telegram.Bot.Types.Enums.ChatType.Supergroup
            };
            var chatRepo1 = new Core.Model.Chat { Id = chat1.Id, ChatType = Core.Model.ChatType.Private, Title = "Private"};
            var chatRepo2 = new Core.Model.Chat { Id = chat2.Id, ChatType = Core.Model.ChatType.Public, Title = "Public" };
            var chatRepo3 = new Core.Model.Chat { Id = chat3.Id, ChatType = Core.Model.ChatType.Admin, Title = "Admin" };
            var chats = new Core.Model.Chat[] { chatRepo1, chatRepo2, chatRepo3 };
            var message = new Message
            {
                Chat = chat1,
                From = user1,
                Text = "/post"
            };

            _fixture.MockBotClient.Reset();
            
            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(s => s.GetUser(It.Is<long>(_ => _ == user1.Id)))
                .ReturnsAsync(userRepo);
            userServiceMock.Setup(s => s.IsAdmin(It.Is<long>(_ => _ == user1.Id)))
                .ReturnsAsync(true);
            _bot.UserService = userServiceMock.Object;

            var chatServiceMock = new Mock<IChatService>();
            chatServiceMock.Setup(s => s.GetChat(It.Is<long>(_ => _ == chat1.Id)))
                .ReturnsAsync(chatRepo1);
            chatServiceMock.Setup(s => s.GetChatList())
                .ReturnsAsync(chats);
            _bot.ChatService = chatServiceMock.Object;
            _bot.StateMachines = new BaseStateMachine[] { new PostStateMachine(chatServiceMock.Object) };

            var userMessageServiceMock = new Mock<IUserMessageService>();
            _bot.UserMessageService = userMessageServiceMock.Object;

            _fixture.MockBotClient.Setup(c => c.GetChatAsync(
                It.Is<ChatId>(_ => _.Identifier == chat1.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(chat1);

            _bot.RecieveMessage(message).Wait();

            message.Text = "Cancel";
            _bot.RecieveMessage(message).Wait();

            message.Text = "Any message";
            _bot.RecieveMessage(message).Wait();

            chatServiceMock.Verify(mock => mock.GetChat(It.Is<long>(_ => _ == chat1.Id)), Times.Exactly(3));
            userMessageServiceMock.Verify(mock => mock.AddUserMessage(It.IsAny<UserMessage>()), Times.Exactly(3));
            userServiceMock.Verify(mock => mock.GetUser(It.IsAny<long>()), Times.Exactly(2));
            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
            userServiceMock.Verify(mock => mock.IsAdmin(It.IsAny<long>()), Times.Once);

            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id),
                It.Is<string>(_ => _ == "Please, select a chat"), It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(), 
                It.IsAny<int>(), It.Is<IReplyMarkup>(m => KeyboardMarkupActionButtons(m, 3)), It.IsAny<CancellationToken>()),
                Times.Once);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id),
                It.Is<string>(_ => _ == "Cancelled"), It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<int>(), It.IsAny<IReplyMarkup>(), It.IsAny<CancellationToken>()),
                Times.Once);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id),
                It.Is<string>(_ => _ != "Please, select a chat" && _ != "Cancelled"), It.IsAny<ParseMode>(), It.IsAny<bool>(), 
                It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<IReplyMarkup>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public void PostAdminWithPermissionsPostIncorrectChatChoice()
        {
            string userName = "TestUser";
            var user1 = new Telegram.Bot.Types.User
            {
                Id = 1,
                Username = userName + "1",
            };
            var userRepo = new Core.Model.User
            {
                Id = user1.Id,
                Type = UserType.Admin
            };
            var chat1 = new Telegram.Bot.Types.Chat
            {
                Id = 1,
                Type = Telegram.Bot.Types.Enums.ChatType.Private
            };
            var chat2 = new Telegram.Bot.Types.Chat
            {
                Id = 2,
                Type = Telegram.Bot.Types.Enums.ChatType.Group
            };
            var chat3 = new Telegram.Bot.Types.Chat
            {
                Id = 3,
                Type = Telegram.Bot.Types.Enums.ChatType.Supergroup
            };
            var chatRepo1 = new Core.Model.Chat { Id = chat1.Id, ChatType = Core.Model.ChatType.Private, Title = "Private" };
            var chatRepo2 = new Core.Model.Chat { Id = chat2.Id, ChatType = Core.Model.ChatType.Public, Title = "Public" };
            var chatRepo3 = new Core.Model.Chat { Id = chat3.Id, ChatType = Core.Model.ChatType.Admin, Title = "Admin" };
            var chats = new Core.Model.Chat[] { chatRepo1, chatRepo2, chatRepo3 };
            var message = new Message
            {
                Chat = chat1,
                From = user1,
                Text = "/post"
            };

            _fixture.MockBotClient.Reset();

            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(s => s.GetUser(It.Is<long>(_ => _ == user1.Id)))
                .ReturnsAsync(userRepo);
            userServiceMock.Setup(s => s.IsAdmin(It.Is<long>(_ => _ == user1.Id)))
                .ReturnsAsync(true);
            _bot.UserService = userServiceMock.Object;

            var chatServiceMock = new Mock<IChatService>();
            chatServiceMock.Setup(s => s.GetChat(It.Is<long>(_ => _ == chat1.Id)))
                .ReturnsAsync(chatRepo1);
            chatServiceMock.Setup(s => s.GetChat(It.Is<string>(_ => _ == "Private")))
                .ReturnsAsync(chatRepo1);
            chatServiceMock.Setup(s => s.GetChat(It.Is<string>(_ => _ == "Public")))
                .ReturnsAsync(chatRepo2);
            chatServiceMock.Setup(s => s.GetChat(It.Is<string>(_ => _ == "Admin")))
                .ReturnsAsync(chatRepo3);
            chatServiceMock.Setup(s => s.GetChatList())
                .ReturnsAsync(chats);
            _bot.ChatService = chatServiceMock.Object;
            _bot.StateMachines = new BaseStateMachine[] { new PostStateMachine(chatServiceMock.Object) };

            var userMessageServiceMock = new Mock<IUserMessageService>();
            _bot.UserMessageService = userMessageServiceMock.Object;

            _fixture.MockBotClient.Setup(c => c.GetChatAsync(
                It.Is<ChatId>(_ => _.Identifier == chat1.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(chat1);

            _bot.RecieveMessage(message).Wait();

            message.Text = "Incorrect chat";
            _bot.RecieveMessage(message).Wait();

            message.Text = "Any message";
            _bot.RecieveMessage(message).Wait();

            chatServiceMock.Verify(mock => mock.GetChat(It.Is<long>(_ => _ == chat1.Id)), Times.Exactly(3));
            userMessageServiceMock.Verify(mock => mock.AddUserMessage(It.IsAny<UserMessage>()), Times.Exactly(3));
            userServiceMock.Verify(mock => mock.GetUser(It.IsAny<long>()), Times.Exactly(2));
            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
            userServiceMock.Verify(mock => mock.IsAdmin(It.IsAny<long>()), Times.Once);

            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id),
                It.Is<string>(_ => _ == "Please, select a chat"), It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<int>(), It.Is<IReplyMarkup>(m => KeyboardMarkupActionButtons(m, 3)), It.IsAny<CancellationToken>()),
                Times.Once);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id),
                It.Is<string>(_ => _ == "Cancelled"), It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<int>(), It.IsAny<IReplyMarkup>(), It.IsAny<CancellationToken>()),
                Times.Once);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id),
                It.Is<string>(_ => _ != "Please, select a chat" && _ != "Cancelled"), It.IsAny<ParseMode>(), It.IsAny<bool>(), 
                It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<IReplyMarkup>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public void PostAdminWithPermissionsPostSendTextButErrorInPinChoice()
        {
            string userName = "TestUser";
            var user1 = new Telegram.Bot.Types.User
            {
                Id = 1,
                Username = userName + "1",
            };
            var userRepo = new Core.Model.User
            {
                Id = user1.Id,
                Type = UserType.Admin
            };
            var chat1 = new Telegram.Bot.Types.Chat
            {
                Id = 1,
                Type = Telegram.Bot.Types.Enums.ChatType.Private
            };
            var chat2 = new Telegram.Bot.Types.Chat
            {
                Id = 2,
                Type = Telegram.Bot.Types.Enums.ChatType.Group
            };
            var chat3 = new Telegram.Bot.Types.Chat
            {
                Id = 3,
                Type = Telegram.Bot.Types.Enums.ChatType.Supergroup
            };
            var chatRepo1 = new Core.Model.Chat { Id = chat1.Id, ChatType = Core.Model.ChatType.Private, Title = "Private" };
            var chatRepo2 = new Core.Model.Chat { Id = chat2.Id, ChatType = Core.Model.ChatType.Public, Title = "Public" };
            var chatRepo3 = new Core.Model.Chat { Id = chat3.Id, ChatType = Core.Model.ChatType.Admin, Title = "Admin" };
            var chats = new Core.Model.Chat[] { chatRepo1, chatRepo2, chatRepo3 };
            var message = new Message
            {
                Chat = chat1,
                From = user1,
                Text = "/post"
            };

            _fixture.MockBotClient.Reset();

            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(s => s.GetUser(It.Is<long>(_ => _ == user1.Id)))
                .ReturnsAsync(userRepo);
            userServiceMock.Setup(s => s.IsAdmin(It.Is<long>(_ => _ == user1.Id)))
                .ReturnsAsync(true);
            _bot.UserService = userServiceMock.Object;

            var chatServiceMock = new Mock<IChatService>();
            chatServiceMock.Setup(s => s.GetChat(It.Is<long>(_ => _ == chat1.Id)))
                .ReturnsAsync(chatRepo1);
            chatServiceMock.Setup(s => s.GetChat(It.Is<string>(_ => _ == "Private")))
                .ReturnsAsync(chatRepo1);
            chatServiceMock.Setup(s => s.GetChat(It.Is<string>(_ => _ == "Public")))
                .ReturnsAsync(chatRepo2);
            chatServiceMock.Setup(s => s.GetChat(It.Is<string>(_ => _ == "Admin")))
                .ReturnsAsync(chatRepo3);
            chatServiceMock.Setup(s => s.GetChatList())
                .ReturnsAsync(chats);
            _bot.ChatService = chatServiceMock.Object;
            _bot.StateMachines = new BaseStateMachine[]
            {
                new PostStateMachine(chatServiceMock.Object)
            };

            var userMessageServiceMock = new Mock<IUserMessageService>();
            _bot.UserMessageService = userMessageServiceMock.Object;

            _fixture.MockBotClient.Setup(c => c.GetChatAsync(
                It.Is<ChatId>(_ => _.Identifier == chat1.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(chat1);

            _bot.RecieveMessage(message).Wait();

            message.Text = "Public";
            _bot.RecieveMessage(message).Wait();

            message.Text = "Some text";
            _bot.RecieveMessage(message).Wait();

            message.Text = "Nain";
            _bot.RecieveMessage(message).Wait();

            message.Text = "Nain";
            _bot.RecieveMessage(message).Wait();


            chatServiceMock.Verify(mock => mock.GetChat(It.Is<long>(_ => _ == chat1.Id)), Times.Exactly(5));
            userMessageServiceMock.Verify(mock => mock.AddUserMessage(It.IsAny<UserMessage>()), Times.Exactly(5));
            userServiceMock.Verify(mock => mock.GetUser(It.IsAny<long>()), Times.Exactly(2));
            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
            userServiceMock.Verify(mock => mock.IsAdmin(It.IsAny<long>()), Times.Once);

            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id),
                It.Is<string>(_ => _ == "Please, select a chat"), It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<int>(), It.Is<IReplyMarkup>(m => KeyboardMarkupActionButtons(m, 3)), It.IsAny<CancellationToken>()),
                Times.Once);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id),
                It.Is<string>(_ => _ == "Please, input a message"), It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<int>(), It.IsAny<IReplyMarkup>(), It.IsAny<CancellationToken>()),
                Times.Once);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id),
                It.Is<string>(_ => _ == "Pin a message?"), It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<int>(), It.Is<IReplyMarkup>(m => KeyboardMarkupActionButtons(m, 3)), It.IsAny<CancellationToken>()),
                Times.Once);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id),
                It.Is<string>(_ => _ == "Cancelled"), It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<int>(), It.IsAny<IReplyMarkup>(), It.IsAny<CancellationToken>()),
                Times.Once);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.IsAny<ChatId>(),
                It.Is<string>(_ => _ != "Cancelled" && _ != "Pin a message?" && _ != "Please, input a message" && _ != "Please, select a chat"), 
                It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<IReplyMarkup>(), 
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public void PostAdminWithPermissionsPostSendTextCancelInPinChoice()
        {
            string userName = "TestUser";
            var user1 = new Telegram.Bot.Types.User
            {
                Id = 1,
                Username = userName + "1",
            };
            var userRepo = new Core.Model.User
            {
                Id = user1.Id,
                Type = UserType.Admin
            };
            var chat1 = new Telegram.Bot.Types.Chat
            {
                Id = 1,
                Type = Telegram.Bot.Types.Enums.ChatType.Private
            };
            var chat2 = new Telegram.Bot.Types.Chat
            {
                Id = 2,
                Type = Telegram.Bot.Types.Enums.ChatType.Group
            };
            var chat3 = new Telegram.Bot.Types.Chat
            {
                Id = 3,
                Type = Telegram.Bot.Types.Enums.ChatType.Supergroup
            };
            var chatRepo1 = new Core.Model.Chat { Id = chat1.Id, ChatType = Core.Model.ChatType.Private, Title = "Private" };
            var chatRepo2 = new Core.Model.Chat { Id = chat2.Id, ChatType = Core.Model.ChatType.Public, Title = "Public" };
            var chatRepo3 = new Core.Model.Chat { Id = chat3.Id, ChatType = Core.Model.ChatType.Admin, Title = "Admin" };
            var chats = new Core.Model.Chat[] { chatRepo1, chatRepo2, chatRepo3 };
            var message = new Message
            {
                Chat = chat1,
                From = user1,
                Text = "/post"
            };

            _fixture.MockBotClient.Reset();

            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(s => s.GetUser(It.Is<long>(_ => _ == user1.Id)))
                .ReturnsAsync(userRepo);
            userServiceMock.Setup(s => s.IsAdmin(It.Is<long>(_ => _ == user1.Id)))
                .ReturnsAsync(true);
            _bot.UserService = userServiceMock.Object;

            var chatServiceMock = new Mock<IChatService>();
            chatServiceMock.Setup(s => s.GetChat(It.Is<long>(_ => _ == chat1.Id)))
                .ReturnsAsync(chatRepo1);
            chatServiceMock.Setup(s => s.GetChat(It.Is<string>(_ => _ == "Private")))
                .ReturnsAsync(chatRepo1);
            chatServiceMock.Setup(s => s.GetChat(It.Is<string>(_ => _ == "Public")))
                .ReturnsAsync(chatRepo2);
            chatServiceMock.Setup(s => s.GetChat(It.Is<string>(_ => _ == "Admin")))
                .ReturnsAsync(chatRepo3);
            chatServiceMock.Setup(s => s.GetChatList())
                .ReturnsAsync(chats);
            _bot.ChatService = chatServiceMock.Object;
            _bot.StateMachines = new BaseStateMachine[] { new PostStateMachine(chatServiceMock.Object) };

            var userMessageServiceMock = new Mock<IUserMessageService>();
            _bot.UserMessageService = userMessageServiceMock.Object;

            _fixture.MockBotClient.Setup(c => c.GetChatAsync(
                It.Is<ChatId>(_ => _.Identifier == chat1.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(chat1);

            _bot.RecieveMessage(message).Wait();

            message.Text = "Public";
            _bot.RecieveMessage(message).Wait();

            message.Text = "Some text";
            _bot.RecieveMessage(message).Wait();

            message.Text = "Cancel";
            _bot.RecieveMessage(message).Wait();

            message.Text = "Nain";
            _bot.RecieveMessage(message).Wait();


            chatServiceMock.Verify(mock => mock.GetChat(It.Is<long>(_ => _ == chat1.Id)), Times.Exactly(5));
            userMessageServiceMock.Verify(mock => mock.AddUserMessage(It.IsAny<UserMessage>()), Times.Exactly(5));
            userServiceMock.Verify(mock => mock.GetUser(It.IsAny<long>()), Times.Exactly(2));
            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
            userServiceMock.Verify(mock => mock.IsAdmin(It.IsAny<long>()), Times.Once);

            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id),
                It.Is<string>(_ => _ == "Please, select a chat"), It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<int>(), It.Is<IReplyMarkup>(m => KeyboardMarkupActionButtons(m, 3)), It.IsAny<CancellationToken>()),
                Times.Once);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id),
                It.Is<string>(_ => _ == "Please, input a message"), It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<int>(), It.IsAny<IReplyMarkup>(), It.IsAny<CancellationToken>()),
                Times.Once);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id),
                It.Is<string>(_ => _ == "Pin a message?"), It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<int>(), It.Is<IReplyMarkup>(m => KeyboardMarkupActionButtons(m, 3)), It.IsAny<CancellationToken>()),
                Times.Once);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id),
                It.Is<string>(_ => _ == "Cancelled"), It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<int>(), It.IsAny<IReplyMarkup>(), It.IsAny<CancellationToken>()),
                Times.Once);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.IsAny<ChatId>(),
                It.Is<string>(_ => _ != "Cancelled" && _ != "Pin a message?" && _ != "Please, input a message" && _ != "Please, select a chat"),
                It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<IReplyMarkup>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public void PostAdminWithPermissionsPostSendTextErrorInConfirm()
        {
            string userName = "TestUser";
            var user1 = new Telegram.Bot.Types.User
            {
                Id = 1,
                Username = userName + "1",
            };
            var userRepo = new Core.Model.User
            {
                Id = user1.Id,
                Type = UserType.Admin
            };
            var chat1 = new Telegram.Bot.Types.Chat
            {
                Id = 1,
                Type = Telegram.Bot.Types.Enums.ChatType.Private
            };
            var chat2 = new Telegram.Bot.Types.Chat
            {
                Id = 2,
                Type = Telegram.Bot.Types.Enums.ChatType.Group
            };
            var chat3 = new Telegram.Bot.Types.Chat
            {
                Id = 3,
                Type = Telegram.Bot.Types.Enums.ChatType.Supergroup
            };
            var chatRepo1 = new Core.Model.Chat { Id = chat1.Id, ChatType = Core.Model.ChatType.Private, Title = "Private" };
            var chatRepo2 = new Core.Model.Chat { Id = chat2.Id, ChatType = Core.Model.ChatType.Public, Title = "Public" };
            var chatRepo3 = new Core.Model.Chat { Id = chat3.Id, ChatType = Core.Model.ChatType.Admin, Title = "Admin" };
            var chats = new Core.Model.Chat[] { chatRepo1, chatRepo2, chatRepo3 };
            var message = new Message
            {
                Chat = chat1,
                From = user1,
                Text = "/post"
            };

            _fixture.MockBotClient.Reset();

            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(s => s.GetUser(It.Is<long>(_ => _ == user1.Id)))
                .ReturnsAsync(userRepo);
            userServiceMock.Setup(s => s.IsAdmin(It.Is<long>(_ => _ == user1.Id)))
                .ReturnsAsync(true);
            _bot.UserService = userServiceMock.Object;

            var chatServiceMock = new Mock<IChatService>();
            chatServiceMock.Setup(s => s.GetChat(It.Is<long>(_ => _ == chat1.Id)))
                .ReturnsAsync(chatRepo1);
            chatServiceMock.Setup(s => s.GetChat(It.Is<string>(_ => _ == "Private")))
                .ReturnsAsync(chatRepo1);
            chatServiceMock.Setup(s => s.GetChat(It.Is<string>(_ => _ == "Public")))
                .ReturnsAsync(chatRepo2);
            chatServiceMock.Setup(s => s.GetChat(It.Is<string>(_ => _ == "Admin")))
                .ReturnsAsync(chatRepo3);
            chatServiceMock.Setup(s => s.GetChatList())
                .ReturnsAsync(chats);
            _bot.ChatService = chatServiceMock.Object;
            _bot.StateMachines = new BaseStateMachine[] { new PostStateMachine(chatServiceMock.Object) };

            var userMessageServiceMock = new Mock<IUserMessageService>();
            _bot.UserMessageService = userMessageServiceMock.Object;

            _fixture.MockBotClient.Setup(c => c.GetChatAsync(
                It.Is<ChatId>(_ => _.Identifier == chat1.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(chat1);

            _bot.RecieveMessage(message).Wait();

            message.Text = "Public";
            _bot.RecieveMessage(message).Wait();

            message.Text = "Text to post";
            _bot.RecieveMessage(message).Wait();

            message.Text = "Yes";
            _bot.RecieveMessage(message).Wait();

            message.Text = "Stop";
            _bot.RecieveMessage(message).Wait();

            message.Text = "Simple message";
            _bot.RecieveMessage(message).Wait();

            chatServiceMock.Verify(mock => mock.GetChat(It.Is<long>(_ => _ == chat1.Id)), Times.Exactly(6));
            userMessageServiceMock.Verify(mock => mock.AddUserMessage(It.IsAny<UserMessage>()), Times.Exactly(6));
            userServiceMock.Verify(mock => mock.GetUser(It.IsAny<long>()), Times.Exactly(2));
            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
            userServiceMock.Verify(mock => mock.IsAdmin(It.IsAny<long>()), Times.Once);

            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id),
                It.Is<string>(_ => _ == "Please, select a chat"), It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<int>(), It.Is<IReplyMarkup>(m => KeyboardMarkupActionButtons(m, 3)), It.IsAny<CancellationToken>()),
                Times.Once);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id),
                It.Is<string>(_ => _ == "Please, input a message"), It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<int>(), It.IsAny<IReplyMarkup>(), It.IsAny<CancellationToken>()),
                Times.Once);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id),
                It.Is<string>(_ => _ == "Pin a message?"), It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<int>(), It.Is<IReplyMarkup>(m => KeyboardMarkupActionButtons(m, 3)), It.IsAny<CancellationToken>()),
                Times.Once);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id),
                It.Is<string>(_ => _ == "Confirm sending?"), It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<int>(), It.Is<IReplyMarkup>(m => KeyboardMarkupActionButtons(m, 2)), It.IsAny<CancellationToken>()),
                Times.Once);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id),
                It.Is<string>(_ => _ == "Cancelled"), It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<int>(), It.IsAny<IReplyMarkup>(), It.IsAny<CancellationToken>()),
                Times.Once);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.IsAny<ChatId>(),
                It.Is<string>(_ => _ != "Cancelled" && _ != "Confirm sending?" && _ != "Pin a message?" && _ != "Please, input a message" && _ != "Please, select a chat"),
                It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<IReplyMarkup>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public void PostAdminWithPermissionsPostSendTextCancelInConfirm()
        {
            string userName = "TestUser";
            var user1 = new Telegram.Bot.Types.User
            {
                Id = 1,
                Username = userName + "1",
            };
            var userRepo = new Core.Model.User
            {
                Id = user1.Id,
                Type = UserType.Admin
            };
            var chat1 = new Telegram.Bot.Types.Chat
            {
                Id = 1,
                Type = Telegram.Bot.Types.Enums.ChatType.Private
            };
            var chat2 = new Telegram.Bot.Types.Chat
            {
                Id = 2,
                Type = Telegram.Bot.Types.Enums.ChatType.Group
            };
            var chat3 = new Telegram.Bot.Types.Chat
            {
                Id = 3,
                Type = Telegram.Bot.Types.Enums.ChatType.Supergroup
            };
            var chatRepo1 = new Core.Model.Chat { Id = chat1.Id, ChatType = Core.Model.ChatType.Private, Title = "Private" };
            var chatRepo2 = new Core.Model.Chat { Id = chat2.Id, ChatType = Core.Model.ChatType.Public, Title = "Public" };
            var chatRepo3 = new Core.Model.Chat { Id = chat3.Id, ChatType = Core.Model.ChatType.Admin, Title = "Admin" };
            var chats = new Core.Model.Chat[] { chatRepo1, chatRepo2, chatRepo3 };
            var message = new Message
            {
                Chat = chat1,
                From = user1,
                Text = "/post"
            };

            _fixture.MockBotClient.Reset();

            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(s => s.GetUser(It.Is<long>(_ => _ == user1.Id)))
                .ReturnsAsync(userRepo);
            userServiceMock.Setup(s => s.IsAdmin(It.Is<long>(_ => _ == user1.Id)))
                .ReturnsAsync(true);
            _bot.UserService = userServiceMock.Object;

            var chatServiceMock = new Mock<IChatService>();
            chatServiceMock.Setup(s => s.GetChat(It.Is<long>(_ => _ == chat1.Id)))
                .ReturnsAsync(chatRepo1);
            chatServiceMock.Setup(s => s.GetChat(It.Is<long>(_ => _ == chat2.Id)))
                .ReturnsAsync(chatRepo2);
            chatServiceMock.Setup(s => s.GetChat(It.Is<string>(_ => _ == "Private")))
                .ReturnsAsync(chatRepo1);
            chatServiceMock.Setup(s => s.GetChat(It.Is<string>(_ => _ == "Public")))
                .ReturnsAsync(chatRepo2);
            chatServiceMock.Setup(s => s.GetChat(It.Is<string>(_ => _ == "Admin")))
                .ReturnsAsync(chatRepo3);
            chatServiceMock.Setup(s => s.GetChatList())
                .ReturnsAsync(chats);
            _bot.ChatService = chatServiceMock.Object;
            _bot.StateMachines = new BaseStateMachine[] { new PostStateMachine(chatServiceMock.Object) };

            var userMessageServiceMock = new Mock<IUserMessageService>();
            _bot.UserMessageService = userMessageServiceMock.Object;

            _fixture.MockBotClient.Setup(c => c.GetChatAsync(
                It.Is<ChatId>(_ => _.Identifier == chat1.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(chat1);

            _bot.RecieveMessage(message).Wait();

            message.Text = "Public";
            _bot.RecieveMessage(message).Wait();

            message.Text = "Text to post";
            _bot.RecieveMessage(message).Wait();

            message.Text = "No";
            _bot.RecieveMessage(message).Wait();

            message.Text = "Cancel";
            _bot.RecieveMessage(message).Wait();

            message.Text = "Simple message";
            _bot.RecieveMessage(message).Wait();

            chatServiceMock.Verify(mock => mock.GetChat(It.Is<long>(_ => _ == chat1.Id)), Times.Exactly(6));
            userMessageServiceMock.Verify(mock => mock.AddUserMessage(It.IsAny<UserMessage>()), Times.Exactly(6));
            userServiceMock.Verify(mock => mock.GetUser(It.IsAny<long>()), Times.Exactly(2));
            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
            userServiceMock.Verify(mock => mock.IsAdmin(It.IsAny<long>()), Times.Once);

            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id),
                It.Is<string>(_ => _ == "Please, select a chat"), It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<int>(), It.Is<IReplyMarkup>(m => KeyboardMarkupActionButtons(m, 3)), It.IsAny<CancellationToken>()),
                Times.Once);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id),
                It.Is<string>(_ => _ == "Please, input a message"), It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<int>(), It.IsAny<IReplyMarkup>(), It.IsAny<CancellationToken>()),
                Times.Once);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id),
                It.Is<string>(_ => _ == "Pin a message?"), It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<int>(), It.Is<IReplyMarkup>(m => KeyboardMarkupActionButtons(m, 3)), It.IsAny<CancellationToken>()),
                Times.Once);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id),
                It.Is<string>(_ => _ == "Confirm sending?"), It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<int>(), It.Is<IReplyMarkup>(m => KeyboardMarkupActionButtons(m, 2)), It.IsAny<CancellationToken>()),
                Times.Once);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id),
                It.Is<string>(_ => _ == "Cancelled"), It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<int>(), It.IsAny<IReplyMarkup>(), It.IsAny<CancellationToken>()),
                Times.Once);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.IsAny<ChatId>(),
                It.Is<string>(_ => _ != "Cancelled" && _ != "Confirm sending?" && _ != "Pin a message?" && _ != "Please, input a message" && _ != "Please, select a chat"),
                It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<IReplyMarkup>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public void PostAdminWithPermissionsPostSendTextConfirmed()
        {
            string userName = "TestUser";
            var user1 = new Telegram.Bot.Types.User
            {
                Id = 1,
                Username = userName + "1",
            };
            var userRepo = new Core.Model.User
            {
                Id = user1.Id,
                Type = UserType.Admin
            };
            var chat1 = new Telegram.Bot.Types.Chat
            {
                Id = 1,
                Type = Telegram.Bot.Types.Enums.ChatType.Private
            };
            var chat2 = new Telegram.Bot.Types.Chat
            {
                Id = 2,
                Type = Telegram.Bot.Types.Enums.ChatType.Group
            };
            var chat3 = new Telegram.Bot.Types.Chat
            {
                Id = 3,
                Type = Telegram.Bot.Types.Enums.ChatType.Supergroup
            };
            var chatRepo1 = new Core.Model.Chat { Id = chat1.Id, ChatType = Core.Model.ChatType.Private, Title = "Private" };
            var chatRepo2 = new Core.Model.Chat { Id = chat2.Id, ChatType = Core.Model.ChatType.Public, Title = "Public" };
            var chatRepo3 = new Core.Model.Chat { Id = chat3.Id, ChatType = Core.Model.ChatType.Admin, Title = "Admin" };
            var chats = new Core.Model.Chat[] { chatRepo1, chatRepo2, chatRepo3 };
            var message = new Message
            {
                Chat = chat1,
                From = user1,
                Text = "/post"
            };

            _fixture.MockBotClient.Reset();

            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(s => s.GetUser(It.Is<long>(_ => _ == user1.Id)))
                .ReturnsAsync(userRepo);
            userServiceMock.Setup(s => s.IsAdmin(It.Is<long>(_ => _ == user1.Id)))
                .ReturnsAsync(true);
            _bot.UserService = userServiceMock.Object;

            var chatServiceMock = new Mock<IChatService>();
            chatServiceMock.Setup(s => s.GetChat(It.Is<long>(_ => _ == chat1.Id)))
                .ReturnsAsync(chatRepo1);
            chatServiceMock.Setup(s => s.GetChat(It.Is<long>(_ => _ == chat2.Id)))
                .ReturnsAsync(chatRepo2);
            chatServiceMock.Setup(s => s.GetChat(It.Is<long>(_ => _ == chat3.Id)))
                .ReturnsAsync(chatRepo3);
            chatServiceMock.Setup(s => s.GetChat(It.Is<string>(_ => _ == chat1.Title)))
                .ReturnsAsync(chatRepo1);
            chatServiceMock.Setup(s => s.GetChat(It.Is<string>(_ => _ == chat2.Title)))
                .ReturnsAsync(chatRepo2);
            chatServiceMock.Setup(s => s.GetChat(It.Is<string>(_ => _ == chat3.Title)))
                .ReturnsAsync(chatRepo3);
            chatServiceMock.Setup(s => s.GetChatList())
                .ReturnsAsync(chats);
            _bot.ChatService = chatServiceMock.Object;
            _bot.StateMachines = new BaseStateMachine[] { new PostStateMachine(chatServiceMock.Object) };

            var userMessageServiceMock = new Mock<IUserMessageService>();
            _bot.UserMessageService = userMessageServiceMock.Object;

            _fixture.MockBotClient.Setup(c => c.GetChatAsync(
                It.Is<ChatId>(_ => _.Identifier == chat1.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(chat1);
            _fixture.MockBotClient.Setup(c => c.GetChatAsync(
                It.Is<ChatId>(_ => _.Identifier == chat3.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(chat3);

            _bot.RecieveMessage(message).Wait();

            message = new Message { MessageId = 2, Chat = chat1, From = user1, Text = chat3.Title };
            _bot.RecieveMessage(message).Wait();

            message = new Message { MessageId = 3, Chat = chat1, From = user1, Text = "Text to post" };
            _bot.RecieveMessage(message).Wait();

            message = new Message { MessageId = 4, Chat = chat1, From = user1, Text = "No" };
            _bot.RecieveMessage(message).Wait();

            message = new Message { MessageId = 5, Chat = chat1, From = user1, Text = "OK" };
            _bot.RecieveMessage(message).Wait();

            message = new Message { MessageId = 6, Chat = chat1, From = user1, Text = "Simple message" };
            _bot.RecieveMessage(message).Wait();

            chatServiceMock.Verify(mock => mock.GetChat(It.Is<long>(_ => _ == chat1.Id)), Times.Exactly(6));
            userMessageServiceMock.Verify(mock => mock.AddUserMessage(It.IsAny<UserMessage>()), Times.Exactly(6));
            userServiceMock.Verify(mock => mock.GetUser(It.IsAny<long>()), Times.Exactly(2));
            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
            userServiceMock.Verify(mock => mock.IsAdmin(It.IsAny<long>()), Times.Once);

            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id),
                It.Is<string>(_ => _ == "Please, select a chat"), It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<int>(), It.Is<IReplyMarkup>(m => KeyboardMarkupActionButtons(m, 3)), It.IsAny<CancellationToken>()),
                Times.Once);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id),
                It.Is<string>(_ => _ == "Please, input a message"), It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<int>(), It.IsAny<IReplyMarkup>(), It.IsAny<CancellationToken>()),
                Times.Once);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id),
                It.Is<string>(_ => _ == "Pin a message?"), It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<int>(), It.Is<IReplyMarkup>(m => KeyboardMarkupActionButtons(m, 3)), It.IsAny<CancellationToken>()),
                Times.Once);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id),
                It.Is<string>(_ => _ == "Confirm sending?"), It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<int>(), It.Is<IReplyMarkup>(m => KeyboardMarkupActionButtons(m, 2)), It.IsAny<CancellationToken>()),
                Times.Once);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat3.Id),
                It.Is<string>(_ => _ == "Text to post"), It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<int>(), 
                It.IsAny<IReplyMarkup>(), It.IsAny<CancellationToken>()), 
                Times.Once);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.IsAny<ChatId>(),
                It.Is<string>(_ => _ != "Confirm sending?" && _ != "Pin a message?" && _ != "Please, input a message" && _ != "Please, select a chat" && _ != "Text to post"),
                It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<IReplyMarkup>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public void PostAdminWithPermissionsPostSendImageConfirmed()
        {
            string userName = "TestUser";
            var user1 = new Telegram.Bot.Types.User
            {
                Id = 1,
                Username = userName + "1",
            };
            var userRepo = new Core.Model.User
            {
                Id = user1.Id,
                Type = UserType.Admin
            };
            var chat1 = new Telegram.Bot.Types.Chat
            {
                Id = 1,
                Type = Telegram.Bot.Types.Enums.ChatType.Private
            };
            var chat2 = new Telegram.Bot.Types.Chat
            {
                Id = 2,
                Type = Telegram.Bot.Types.Enums.ChatType.Group
            };
            var chat3 = new Telegram.Bot.Types.Chat
            {
                Id = 3,
                Type = Telegram.Bot.Types.Enums.ChatType.Supergroup
            };
            var chatRepo1 = new Core.Model.Chat { Id = chat1.Id, ChatType = Core.Model.ChatType.Private, Title = "Private" };
            var chatRepo2 = new Core.Model.Chat { Id = chat2.Id, ChatType = Core.Model.ChatType.Public, Title = "Public" };
            var chatRepo3 = new Core.Model.Chat { Id = chat3.Id, ChatType = Core.Model.ChatType.Admin, Title = "Admin" };
            var chats = new Core.Model.Chat[] { chatRepo1, chatRepo2, chatRepo3 };
            var message = new Message
            {
                Chat = chat1,
                From = user1,
                Text = "/post"
            };

            _fixture.MockBotClient.Reset();

            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(s => s.GetUser(It.Is<long>(_ => _ == user1.Id)))
                .ReturnsAsync(userRepo);
            userServiceMock.Setup(s => s.IsAdmin(It.Is<long>(_ => _ == user1.Id)))
                .ReturnsAsync(true);
            _bot.UserService = userServiceMock.Object;

            var chatServiceMock = new Mock<IChatService>();
            chatServiceMock.Setup(s => s.GetChat(It.Is<long>(_ => _ == chat1.Id)))
                .ReturnsAsync(chatRepo1);
            chatServiceMock.Setup(s => s.GetChat(It.Is<long>(_ => _ == chat2.Id)))
                .ReturnsAsync(chatRepo2);
            chatServiceMock.Setup(s => s.GetChat(It.Is<long>(_ => _ == chat3.Id)))
                .ReturnsAsync(chatRepo3);
            chatServiceMock.Setup(s => s.GetChat(It.Is<string>(_ => _ == chat1.Title)))
                .ReturnsAsync(chatRepo1);
            chatServiceMock.Setup(s => s.GetChat(It.Is<string>(_ => _ == chat2.Title)))
                .ReturnsAsync(chatRepo2);
            chatServiceMock.Setup(s => s.GetChat(It.Is<string>(_ => _ == chat3.Title)))
                .ReturnsAsync(chatRepo3);
            chatServiceMock.Setup(s => s.GetChatList())
                .ReturnsAsync(chats);
            _bot.ChatService = chatServiceMock.Object;
            _bot.StateMachines = new BaseStateMachine[] { new PostStateMachine(chatServiceMock.Object) };

            var userMessageServiceMock = new Mock<IUserMessageService>();
            _bot.UserMessageService = userMessageServiceMock.Object;

            _fixture.MockBotClient.Setup(c => c.GetChatAsync(
                It.Is<ChatId>(_ => _.Identifier == chat1.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(chat1);
            _fixture.MockBotClient.Setup(c => c.GetChatAsync(
                It.Is<ChatId>(_ => _.Identifier == chat3.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(chat3);

            _bot.RecieveMessage(message).Wait();

            message = new Message { MessageId = 2, Chat = chat1, From = user1, Text = chat3.Title };
            _bot.RecieveMessage(message).Wait();

            var photo = new PhotoSize[] { new PhotoSize { FileId = "1" } };
            var messageWithPhoto = new Message { MessageId = 3, Chat = chat1, From = user1, Photo = photo, Caption = "Text for photo" };
            _bot.RecieveMessage(messageWithPhoto).Wait();

            message = new Message { MessageId = 4, Chat = chat1, From = user1, Text = "No" };
            _bot.RecieveMessage(message).Wait();

            message = new Message { MessageId = 5, Chat = chat1, From = user1, Text = "OK" };
            _bot.RecieveMessage(message).Wait();

            message = new Message { MessageId = 6, Chat = chat1, From = user1, Text = "Simple message" };
            _bot.RecieveMessage(message).Wait();

            chatServiceMock.Verify(mock => mock.GetChat(It.Is<long>(_ => _ == chat1.Id)), Times.Exactly(6));
            userMessageServiceMock.Verify(mock => mock.AddUserMessage(It.IsAny<UserMessage>()), Times.Exactly(6));
            userServiceMock.Verify(mock => mock.GetUser(It.IsAny<long>()), Times.Exactly(2));
            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
            userServiceMock.Verify(mock => mock.IsAdmin(It.IsAny<long>()), Times.Once);

            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id),
                It.Is<string>(_ => _ == "Please, select a chat"), It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<int>(), It.Is<IReplyMarkup>(m => KeyboardMarkupActionButtons(m, 3)), It.IsAny<CancellationToken>()),
                Times.Once);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id),
                It.Is<string>(_ => _ == "Please, input a message"), It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<int>(), It.IsAny<IReplyMarkup>(), It.IsAny<CancellationToken>()),
                Times.Once);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id),
                It.Is<string>(_ => _ == "Pin a message?"), It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<int>(), It.Is<IReplyMarkup>(m => KeyboardMarkupActionButtons(m, 3)), It.IsAny<CancellationToken>()),
                Times.Once);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id),
                It.Is<string>(_ => _ == "Confirm sending?"), It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<int>(), It.Is<IReplyMarkup>(m => KeyboardMarkupActionButtons(m, 2)), It.IsAny<CancellationToken>()),
                Times.Once);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat3.Id),
                It.Is<string>(_ => _ == messageWithPhoto.Caption), It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<int>(),
                It.IsAny<IReplyMarkup>(), It.IsAny<CancellationToken>()),
                Times.Never);
            _fixture.MockBotClient.Verify(mock => mock.SendPhotoAsync(It.Is<ChatId>(_ => _.Identifier == chat3.Id),   
                It.Is<InputOnlineFile>(_ => _.FileId == photo[0].FileId), It.Is<string>(_ => _ == messageWithPhoto.Caption),
                It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<IReplyMarkup>(), It.IsAny<CancellationToken>()),
                Times.Once);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.IsAny<ChatId>(),
                It.Is<string>(_ => _ != "Confirm sending?" && _ != "Pin a message?" && _ != "Please, input a message" && _ != "Please, select a chat" && _ != "Text to post"),
                It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<IReplyMarkup>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public void PostAdminWithPermissionsPostSendImageWithoutTextConfirmed()
        {
            string userName = "TestUser";
            var user1 = new Telegram.Bot.Types.User
            {
                Id = 1,
                Username = userName + "1",
            };
            var userRepo = new Core.Model.User
            {
                Id = user1.Id,
                Type = UserType.Admin
            };
            var chat1 = new Telegram.Bot.Types.Chat
            {
                Id = 1,
                Type = Telegram.Bot.Types.Enums.ChatType.Private
            };
            var chat2 = new Telegram.Bot.Types.Chat
            {
                Id = 2,
                Type = Telegram.Bot.Types.Enums.ChatType.Group
            };
            var chat3 = new Telegram.Bot.Types.Chat
            {
                Id = 3,
                Type = Telegram.Bot.Types.Enums.ChatType.Supergroup
            };
            var chatRepo1 = new Core.Model.Chat { Id = chat1.Id, ChatType = Core.Model.ChatType.Private, Title = "Private" };
            var chatRepo2 = new Core.Model.Chat { Id = chat2.Id, ChatType = Core.Model.ChatType.Public, Title = "Public" };
            var chatRepo3 = new Core.Model.Chat { Id = chat3.Id, ChatType = Core.Model.ChatType.Admin, Title = "Admin" };
            var chats = new Core.Model.Chat[] { chatRepo1, chatRepo2, chatRepo3 };
            var message = new Message
            {
                Chat = chat1,
                From = user1,
                Text = "/post"
            };

            _fixture.MockBotClient.Reset();

            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(s => s.GetUser(It.Is<long>(_ => _ == user1.Id)))
                .ReturnsAsync(userRepo);
            userServiceMock.Setup(s => s.IsAdmin(It.Is<long>(_ => _ == user1.Id)))
                .ReturnsAsync(true);
            _bot.UserService = userServiceMock.Object;

            var chatServiceMock = new Mock<IChatService>();
            chatServiceMock.Setup(s => s.GetChat(It.Is<long>(_ => _ == chat1.Id)))
                .ReturnsAsync(chatRepo1);
            chatServiceMock.Setup(s => s.GetChat(It.Is<long>(_ => _ == chat2.Id)))
                .ReturnsAsync(chatRepo2);
            chatServiceMock.Setup(s => s.GetChat(It.Is<long>(_ => _ == chat3.Id)))
                .ReturnsAsync(chatRepo3);
            chatServiceMock.Setup(s => s.GetChat(It.Is<string>(_ => _ == chat1.Title)))
                .ReturnsAsync(chatRepo1);
            chatServiceMock.Setup(s => s.GetChat(It.Is<string>(_ => _ == chat2.Title)))
                .ReturnsAsync(chatRepo2);
            chatServiceMock.Setup(s => s.GetChat(It.Is<string>(_ => _ == chat3.Title)))
                .ReturnsAsync(chatRepo3);
            chatServiceMock.Setup(s => s.GetChatList())
                .ReturnsAsync(chats);
            _bot.ChatService = chatServiceMock.Object;
            _bot.StateMachines = new BaseStateMachine[] { new PostStateMachine(chatServiceMock.Object) };

            var userMessageServiceMock = new Mock<IUserMessageService>();
            _bot.UserMessageService = userMessageServiceMock.Object;

            _fixture.MockBotClient.Setup(c => c.GetChatAsync(
                It.Is<ChatId>(_ => _.Identifier == chat1.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(chat1);
            _fixture.MockBotClient.Setup(c => c.GetChatAsync(
                It.Is<ChatId>(_ => _.Identifier == chat3.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(chat3);

            _bot.RecieveMessage(message).Wait();

            message = new Message { MessageId = 2, Chat = chat1, From = user1, Text = chat3.Title };
            _bot.RecieveMessage(message).Wait();

            var photo = new PhotoSize[] { new PhotoSize { FileId = "1" } };
            var messageWithPhoto = new Message { MessageId = 3, Chat = chat1, From = user1, Photo = photo };
            _bot.RecieveMessage(messageWithPhoto).Wait();

            message = new Message { MessageId = 4, Chat = chat1, From = user1, Text = "No" };
            _bot.RecieveMessage(message).Wait();

            message = new Message { MessageId = 5, Chat = chat1, From = user1, Text = "OK" };
            _bot.RecieveMessage(message).Wait();

            message = new Message { MessageId = 6, Chat = chat1, From = user1, Text = "Simple message" };
            _bot.RecieveMessage(message).Wait();

            chatServiceMock.Verify(mock => mock.GetChat(It.Is<long>(_ => _ == chat1.Id)), Times.Exactly(6));
            userMessageServiceMock.Verify(mock => mock.AddUserMessage(It.IsAny<UserMessage>()), Times.Exactly(6));
            userServiceMock.Verify(mock => mock.GetUser(It.IsAny<long>()), Times.Exactly(2));
            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
            userServiceMock.Verify(mock => mock.IsAdmin(It.IsAny<long>()), Times.Once);

            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id),
                It.Is<string>(_ => _ == "Please, select a chat"), It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<int>(), It.Is<IReplyMarkup>(m => KeyboardMarkupActionButtons(m, 3)), It.IsAny<CancellationToken>()),
                Times.Once);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id),
                It.Is<string>(_ => _ == "Please, input a message"), It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<int>(), It.IsAny<IReplyMarkup>(), It.IsAny<CancellationToken>()),
                Times.Once);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id),
                It.Is<string>(_ => _ == "Pin a message?"), It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<int>(), It.Is<IReplyMarkup>(m => KeyboardMarkupActionButtons(m, 3)), It.IsAny<CancellationToken>()),
                Times.Once);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id),
                It.Is<string>(_ => _ == "Confirm sending?"), It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<int>(), It.Is<IReplyMarkup>(m => KeyboardMarkupActionButtons(m, 2)), It.IsAny<CancellationToken>()),
                Times.Once);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.Is<ChatId>(_ => _.Identifier == chat3.Id),
                It.Is<string>(_ => _ == messageWithPhoto.Caption), It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<int>(),
                It.IsAny<IReplyMarkup>(), It.IsAny<CancellationToken>()),
                Times.Never);
            _fixture.MockBotClient.Verify(mock => mock.SendPhotoAsync(It.Is<ChatId>(_ => _.Identifier == chat3.Id),
                It.Is<InputOnlineFile>(_ => _.FileId == photo[0].FileId), It.Is<string>(_ => _ == messageWithPhoto.Caption),
                It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<IReplyMarkup>(), It.IsAny<CancellationToken>()),
                Times.Once);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(It.IsAny<ChatId>(),
                It.Is<string>(_ => _ != "Confirm sending?" && _ != "Pin a message?" && _ != "Please, input a message" && _ != "Please, select a chat" && _ != "Text to post"),
                It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<IReplyMarkup>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }


        private bool KeyboardMarkupActionButtons(IReplyMarkup markup, int buttonsCount)
        {
            return markup is ReplyKeyboardMarkup keyboardMarkup &&
                    keyboardMarkup.Keyboard.Any(k =>  k.Any(b => b.Text == "Cancel")) &&
                    (keyboardMarkup.Keyboard.Count() == buttonsCount || 
                    keyboardMarkup.Keyboard.First().Count() == buttonsCount);
        }
    }
}
