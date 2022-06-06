//using Moq;
//using System.Linq;
//using System.Threading;
//using Telegram.Altayskaya97.Bot.Interface;
//using Telegram.Altayskaya97.Bot.StateMachines;
//using Telegram.Altayskaya97.Core.Constant;
//using Telegram.Altayskaya97.Core.Model;
//using Telegram.Altayskaya97.Service.Interface;
//using Xunit;
//using System.Collections.Generic;

//namespace Telegram.Altayskaya97.Test.Integration
//{
//    public class ChangePasswordTests : IClassFixture<BotFixture>
//    {
//        private readonly BotFixture _fixture = null;
//        private readonly Altayskaya97.Bot.Bot _bot = null;

//        public ChangePasswordTests(BotFixture fixture)
//        {
//            _fixture = fixture;
//            _bot = fixture.Bot;
//        }

//        [Fact]
//        public void ChangePasswordNonAdmin()
//        {
//            string userName = "TestUser";
//            var user1 = new Telegram.Bot.Types.User
//            {
//                Id = 1,
//                Username = userName + "1",
//            };
//            var userRepo = new Altayskaya97.Core.Model.User
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
//                Text = "/changepass"
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
//        public void ChangePasswordAdminWithoutPermissions()
//        {
//            string userName = "TestUser";
//            var user1 = new Telegram.Bot.Types.User
//            {
//                Id = 1,
//                Username = userName + "1",
//            };
//            var userRepo = new Altayskaya97.Core.Model.User
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
//                Text = "/changepass"
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
//        public void ChangePasswordAdminWithPermissionsCancel()
//        {
//            string userName = "TestUser";
//            var user1 = new Telegram.Bot.Types.User
//            {
//                Id = 1,
//                Username = userName + "1",
//            };
//            var userRepo = new Altayskaya97.Core.Model.User
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
//            var chatRepo1 = new Altayskaya97.Core.Model.Chat { Id = chat1.Id, ChatType = Altayskaya97.Core.Model.ChatType.Private, Title = "Private" };
//            var chatRepo2 = new Altayskaya97.Core.Model.Chat { Id = chat2.Id, ChatType = Altayskaya97.Core.Model.ChatType.Public, Title = "Public" };
//            var chatRepo3 = new Altayskaya97.Core.Model.Chat { Id = chat3.Id, ChatType = Altayskaya97.Core.Model.ChatType.Admin, Title = "Admin" };
//            var chats = new Altayskaya97.Core.Model.Chat[] { chatRepo1, chatRepo2, chatRepo3 };
//            var message = new Message
//            {
//                Chat = chat1,
//                From = user1,
//                Text = "/changepass"
//            };

//            var password1 = new Password
//            {
//                Id = 1,
//                ChatType = "Admin",
//                Value = "/adminpass"
//            };
//            var password2 = new Password
//            {
//                Id = 2,
//                ChatType = "Member",
//                Value = "/memberpass"
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

//            var passwordServiceMock = new Mock<IPasswordService>();
//            passwordServiceMock.Setup(s => s.GetList())
//                .ReturnsAsync(new Password[] { password1, password2 });
//            _bot.PasswordService = passwordServiceMock.Object;
//            _bot.StateMachines = new IStateMachine[] 
//            { 
//                new ChangePasswordStateMachine(_bot.PasswordService) 
//            };

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
//            passwordServiceMock.Verify(mock => mock.GetList(), Times.Once);
//            userServiceMock.Verify(mock => mock.Get(It.IsAny<long>()), Times.Once);
//            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
//            userServiceMock.Verify(mock => mock.IsAdmin(It.IsAny<long>()), Times.Once);

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
//                It.Is<string>(_ => _ != Messages.SelectChatType && _ != Messages.Cancelled), 
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
//        public void ChangePasswordAdminWithPermissionsIncorrectChatTypeChoice()
//        {
//            string userName = "TestUser";
//            var user1 = new Telegram.Bot.Types.User
//            {
//                Id = 1,
//                Username = userName + "1",
//            };
//            var userRepo = new Altayskaya97.Core.Model.User
//            {
//                Id = user1.Id,
//                Type = UserType.Admin
//            };
//            var chat1 = new Telegram.Bot.Types.Chat
//            {
//                Id = 1,
//                Type = Telegram.Bot.Types.Enums.ChatType.Private
//            };
//            var password1 = new Password
//            {
//                Id = 1,
//                ChatType = "Admin",
//                Value = "/adminpass"
//            };
//            var password2 = new Password
//            {
//                Id = 2,
//                ChatType = "Member",
//                Value = "/memberpass"
//            };
//            var chatRepo1 = new Altayskaya97.Core.Model.Chat { Id = chat1.Id, ChatType = Altayskaya97.Core.Model.ChatType.Private, Title = "Private" };
//            var passwords = new Password[] { password1, password2 };
//            var message = new Message
//            {
//                Chat = chat1,
//                From = user1,
//                Text = "/changepass"
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
//            _bot.ChatService = chatServiceMock.Object;

//            var passwordServiceMock = new Mock<IPasswordService>();
//            passwordServiceMock.Setup(s => s.GetList())
//                .ReturnsAsync(new Password[] { password1, password2 });
//            _bot.PasswordService = passwordServiceMock.Object;
//            _bot.StateMachines = new IStateMachine[]
//            {
//                new ChangePasswordStateMachine(_bot.PasswordService)
//            };

//            var userMessageServiceMock = new Mock<IUserMessageService>();
//            _bot.UserMessageService = userMessageServiceMock.Object;

//            _fixture.MockBotClient.Setup(c => c.GetChatAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(chat1);

//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Incorrect choice";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Any message";
//            _bot.RecieveMessage(message).Wait();

//            chatServiceMock.Verify(mock => mock.Get(It.Is<long>(_ => _ == chat1.Id)), Times.Exactly(3));
//            userMessageServiceMock.Verify(mock => mock.Add(It.IsAny<UserMessage>()), Times.Exactly(3));
//            userServiceMock.Verify(mock => mock.Get(It.IsAny<long>()), Times.Once);
//            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
//            userServiceMock.Verify(mock => mock.IsAdmin(It.IsAny<long>()), Times.Once);
//            passwordServiceMock.Verify(mock => mock.Update(It.IsAny<Password>()), Times.Never);

//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ == Messages.SelectChatType), 
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
//                It.Is<string>(_ => _ != Messages.SelectChatType && _ != Messages.Cancelled), 
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
//        public void ChangePasswordAdminWithPermissionsIncorrectPasswordInput()
//        {
//            string userName = "TestUser";
//            var user1 = new Telegram.Bot.Types.User
//            {
//                Id = 1,
//                Username = userName + "1",
//            };
//            var userRepo = new Altayskaya97.Core.Model.User
//            {
//                Id = user1.Id,
//                Type = UserType.Admin
//            };
//            var chat1 = new Telegram.Bot.Types.Chat
//            {
//                Id = 1,
//                Type = Telegram.Bot.Types.Enums.ChatType.Private
//            };
//            var chatRepo1 = new Altayskaya97.Core.Model.Chat
//            {
//                Id = chat1.Id,
//                Title = "Private"
//            };
//            var password1 = new Password
//            {
//                Id = 1,
//                ChatType = "Admin",
//                Value = "/adminpass"
//            };
//            var password2 = new Password
//            {
//                Id = 2,
//                ChatType = "Member",
//                Value = "/memberpass"
//            };
//            var message = new Message
//            {
//                Chat = chat1,
//                From = user1,
//                Text = "/changepass"
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
//            _bot.ChatService = chatServiceMock.Object;

//            var passwordServiceMock = new Mock<IPasswordService>();
//            passwordServiceMock.Setup(s => s.GetList())
//                .ReturnsAsync(new Password[] { password1, password2 });
//            _bot.PasswordService = passwordServiceMock.Object;
//            _bot.StateMachines = new IStateMachine[]
//            {
//                new ChangePasswordStateMachine(_bot.PasswordService)
//            };

//            var userMessageServiceMock = new Mock<IUserMessageService>();
//            _bot.UserMessageService = userMessageServiceMock.Object;

//            _fixture.MockBotClient.Setup(c => c.GetChatAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(chat1);

//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Admin";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "password";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Any message";
//            _bot.RecieveMessage(message).Wait();

//            chatServiceMock.Verify(mock => mock.Get(It.Is<long>(_ => _ == chat1.Id)), Times.Exactly(4));
//            userMessageServiceMock.Verify(mock => mock.Add(It.IsAny<UserMessage>()), Times.Exactly(4));
//            userServiceMock.Verify(mock => mock.Get(It.IsAny<long>()), Times.Once);
//            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
//            userServiceMock.Verify(mock => mock.IsAdmin(It.IsAny<long>()), Times.Once);
//            passwordServiceMock.Verify(mock => mock.Update(It.IsAny<Password>()), Times.Never);

//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _.Contains(Messages.Cancelled)), 
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
//                It.Is<string>(_ => _ != Messages.SelectChatType && _ != Messages.Cancelled && _ != Messages.InputNewPassword), 
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
//        public void ChangePasswordAdminWithPermissionsConfirmationCancel()
//        {
//            string userName = "TestUser";
//            var user1 = new Telegram.Bot.Types.User
//            {
//                Id = 1,
//                Username = userName + "1",
//            };
//            var userRepo = new Altayskaya97.Core.Model.User
//            {
//                Id = user1.Id,
//                Type = UserType.Admin
//            };
//            var chat1 = new Telegram.Bot.Types.Chat
//            {
//                Id = 1,
//                Type = Telegram.Bot.Types.Enums.ChatType.Private
//            };
//            var chatRepo1 = new Altayskaya97.Core.Model.Chat { Id = chat1.Id, ChatType = Altayskaya97.Core.Model.ChatType.Private, Title = "Private" };
//            var password1 = new Password
//            {
//                Id = 1,
//                ChatType = "Admin",
//                Value = "/adminpass"
//            };
//            var password2 = new Password
//            {
//                Id = 2,
//                ChatType = "Public",
//                Value = "/memberpass"
//            };
//            var message = new Message
//            {
//                Chat = chat1,
//                From = user1,
//                Text = "/changepass"
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
//            _bot.ChatService = chatServiceMock.Object;

//            var passwordServiceMock = new Mock<IPasswordService>();
//            passwordServiceMock.Setup(s => s.GetList())
//                .ReturnsAsync(new Password[] { password1, password2 });
//            passwordServiceMock.Setup(s => s.GetByType(It.Is<string>(_ =>
//                _ == password1.ChatType))).ReturnsAsync(password1);
//            passwordServiceMock.Setup(s => s.GetByType(It.Is<string>(_ =>
//                _ == password2.ChatType))).ReturnsAsync(password2);
//            _bot.PasswordService = passwordServiceMock.Object;
//            _bot.StateMachines = new IStateMachine[]
//            {
//                new ChangePasswordStateMachine(_bot.PasswordService)
//            };

//            var userMessageServiceMock = new Mock<IUserMessageService>();
//            _bot.UserMessageService = userMessageServiceMock.Object;

//            _fixture.MockBotClient.Setup(c => c.GetChatAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(chat1);

//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Public";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "/password";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Cancel";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Any message";
//            _bot.RecieveMessage(message).Wait();

//            chatServiceMock.Verify(mock => mock.Get(It.Is<long>(_ => _ == chat1.Id)), Times.Exactly(5));
//            userMessageServiceMock.Verify(mock => mock.Add(It.IsAny<UserMessage>()), Times.Exactly(5));
//            userServiceMock.Verify(mock => mock.Get(It.IsAny<long>()), Times.Once);
//            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
//            userServiceMock.Verify(mock => mock.IsAdmin(It.IsAny<long>()), Times.Once);
//            passwordServiceMock.Verify(mock => mock.Update(It.IsAny<Password>()), Times.Never);

//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ == Messages.InputNewPassword), 
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
//                It.Is<string>(_ => _ == Messages.Confirmation), 
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
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ != Messages.SelectChatType && _ != Messages.Cancelled && _ != Messages.InputNewPassword &&_ != Messages.Confirmation), 
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
//        public void ChangePasswordAdminWithPermissionsConfirmationIncorrect()
//        {
//            string userName = "TestUser";
//            var user1 = new Telegram.Bot.Types.User
//            {
//                Id = 1,
//                Username = userName + "1",
//            };
//            var userRepo = new Altayskaya97.Core.Model.User
//            {
//                Id = user1.Id,
//                Type = UserType.Admin
//            };
//            var chat1 = new Telegram.Bot.Types.Chat
//            {
//                Id = 1,
//                Type = Telegram.Bot.Types.Enums.ChatType.Private
//            };
//            var chatRepo1 = new Altayskaya97.Core.Model.Chat { Id = chat1.Id, ChatType = Altayskaya97.Core.Model.ChatType.Private, Title = "Private" };
//            var password1 = new Password
//            {
//                Id = 1,
//                ChatType = "Admin",
//                Value = "/adminpass"
//            };
//            var password2 = new Password
//            {
//                Id = 2,
//                ChatType = "Member",
//                Value = "/memberpass"
//            };
//            var message = new Message
//            {
//                Chat = chat1,
//                From = user1,
//                Text = "/changepass"
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
//            _bot.ChatService = chatServiceMock.Object;

//            var passwordServiceMock = new Mock<IPasswordService>();
//            passwordServiceMock.Setup(s => s.GetList())
//                .ReturnsAsync(new Password[] { password1, password2 });
//            passwordServiceMock.Setup(s => s.GetByType(It.Is<string>(_ =>
//                _ == password1.ChatType))).ReturnsAsync(password1);
//            passwordServiceMock.Setup(s => s.GetByType(It.Is<string>(_ =>
//                _ == password2.ChatType))).ReturnsAsync(password2);
//            _bot.PasswordService = passwordServiceMock.Object;
//            _bot.StateMachines = new IStateMachine[]
//            {
//                new ChangePasswordStateMachine(_bot.PasswordService)
//            };

//            var userMessageServiceMock = new Mock<IUserMessageService>();
//            _bot.UserMessageService = userMessageServiceMock.Object;

//            _fixture.MockBotClient.Setup(c => c.GetChatAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(chat1);

//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Admin";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "/password";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Incorrect choice";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Any message";
//            _bot.RecieveMessage(message).Wait();

//            chatServiceMock.Verify(mock => mock.Get(It.Is<long>(_ => _ == chat1.Id)), Times.Exactly(5));
//            userMessageServiceMock.Verify(mock => mock.Add(It.IsAny<UserMessage>()), Times.Exactly(5));
//            userServiceMock.Verify(mock => mock.Get(It.IsAny<long>()), Times.Once);
//            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
//            userServiceMock.Verify(mock => mock.IsAdmin(It.IsAny<long>()), Times.Once);
//            passwordServiceMock.Verify(mock => mock.Update(It.IsAny<Password>()), Times.Never);

//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ == Messages.InputNewPassword), 
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
//                It.Is<string>(_ => _ == Messages.Confirmation), 
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
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ != Messages.SelectChatType && _ != Messages.Cancelled && _ != Messages.InputNewPassword && _ != Messages.Confirmation), 
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
//        public void ChangePasswordAdminWithPermissionsConfirmed()
//        {
//            string userName = "TestUser";
//            var user1 = new Telegram.Bot.Types.User
//            {
//                Id = 1,
//                Username = userName + "1",
//            };
//            var userRepo = new Altayskaya97.Core.Model.User
//            {
//                Id = user1.Id,
//                Type = UserType.Admin
//            };
//            var chat1 = new Telegram.Bot.Types.Chat
//            {
//                Id = 1,
//                Type = Telegram.Bot.Types.Enums.ChatType.Private
//            };
//            var chatRepo1 = new Altayskaya97.Core.Model.Chat { Id = chat1.Id, ChatType = Altayskaya97.Core.Model.ChatType.Private, Title = "Private" };
//            var password1 = new Password
//            {
//                Id = 1,
//                ChatType = "Admin",
//                Value = "/adminpass"
//            };
//            var password2 = new Password
//            {
//                Id = 2,
//                ChatType = "Member",
//                Value = "/memberpass"
//            };
//            var message = new Message
//            {
//                Chat = chat1,
//                From = user1,
//                Text = "/changepass"
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
//            _bot.ChatService = chatServiceMock.Object;

//            var passwordServiceMock = new Mock<IPasswordService>();
//            passwordServiceMock.Setup(s => s.GetList())
//                .ReturnsAsync(new Password[] { password1, password2 });
//            passwordServiceMock.Setup(s => s.GetByType(It.Is<string>(_ =>
//                _ == password1.ChatType))).ReturnsAsync(password1);
//            passwordServiceMock.Setup(s => s.GetByType(It.Is<string>(_ =>
//                _ == password2.ChatType))).ReturnsAsync(password2);
//            _bot.PasswordService = passwordServiceMock.Object;
//            _bot.StateMachines = new IStateMachine[]
//            {
//                new ChangePasswordStateMachine(_bot.PasswordService)
//            };

//            var userMessageServiceMock = new Mock<IUserMessageService>();
//            _bot.UserMessageService = userMessageServiceMock.Object;

//            _fixture.MockBotClient.Setup(c => c.GetChatAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(chat1);

//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Admin";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "/password";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "OK";
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "Any message";
//            _bot.RecieveMessage(message).Wait();

//            chatServiceMock.Verify(mock => mock.Get(It.Is<long>(_ => _ == chat1.Id)), Times.Exactly(5));
//            userMessageServiceMock.Verify(mock => mock.Add(It.IsAny<UserMessage>()), Times.Exactly(5));
//            userServiceMock.Verify(mock => mock.Get(It.IsAny<long>()), Times.Once);
//            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
//            userServiceMock.Verify(mock => mock.IsAdmin(It.IsAny<long>()), Times.Once);
//            passwordServiceMock.Verify(mock => mock.Update(It.IsAny<Password>()), Times.Once);

//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ == Messages.InputNewPassword), 
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
//                It.Is<string>(_ => _ == Messages.Confirmation), 
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
//                It.Is<string>(_ => _.Contains(Messages.Cancelled)), 
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
//                It.Is<string>(_ => _ == "Password updated"), 
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
//                It.Is<string>(_ => _ != Messages.SelectChatType && _ != Messages.Cancelled && _ != Messages.InputNewPassword && _ != Messages.Confirmation && _ != "Password updated"), 
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

//        private bool KeyboardMarkupActionButtons(IReplyMarkup markup, int buttonsCount)
//        {
//            return markup is ReplyKeyboardMarkup keyboardMarkup &&
//                    keyboardMarkup.Keyboard.Any(k => k.Any(b => b.Text == Messages.Cancel)) &&
//                    (keyboardMarkup.Keyboard.Count() == buttonsCount ||
//                    keyboardMarkup.Keyboard.First().Count() == buttonsCount);
//        }
//    }
//}
