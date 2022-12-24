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
//    public class DeleteChatTests : IClassFixture<BotFixture>
//    {
//        private readonly BotFixture _fixture = null;
//        private readonly SafeBot.Bot.Bot _bot = null;

//        public DeleteChatTests(BotFixture fixture)
//        {
//            _fixture = fixture;
//            _bot = fixture.Bot;
//        }

//        [Fact]
//        public void DeleteChatUnknownUserTest()
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
//                Type = ChatType.Private,
//                Title = "TestChat"
//            };
//            var chatRepo = _fixture.ChatMapper.MapToEntity(chat);
//            var message = new Message
//            {
//                Chat = chat,
//                From = user,
//                Text = "/deletechat testchat"
//            };

//            _fixture.MockBotClient.Reset();

//            var userServiceMock = new Mock<IUserService>();
//            userServiceMock.Setup(s => s.Get(It.IsAny<long>()))
//                .ReturnsAsync(default(SafeBot.Core.Model.User));
//            _bot.UserService = userServiceMock.Object;

//            var chatServiceMock = new Mock<IChatService>();
//            chatServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == chat.Id)))
//                .ReturnsAsync(chatRepo);
//            chatServiceMock.Setup(s => s.Get(It.Is<string>(_ => _.Trim().ToLower() == chat.Title.Trim().ToLower())))
//                .ReturnsAsync(chatRepo);
//            _bot.ChatService = chatServiceMock.Object;

//            _bot.RecieveMessage(message).Wait();

//            message.Text = "/deletechat";
//            _bot.RecieveMessage(message).Wait();

//            userServiceMock.Verify(mock => mock.Get(It.Is<long>(_ => _ == user.Id)), Times.Exactly(1));
//            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
//            userServiceMock.Verify(mock => mock.GetList(), Times.Never);
//            chatServiceMock.Verify(mock => mock.Get(It.Is<long>(_ => _ == chat.Id)), Times.Exactly(2));
//            chatServiceMock.Verify(mock => mock.Get(It.Is<string>(_ => _.Trim().ToLower() == chat.Title.Trim().ToLower())), Times.Never);
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
//        public void DeleteChatNonAdminTest()
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
//                Type = ChatType.Private,
//                Title = "TestChat"
//            };
//            var chatRepo = _fixture.ChatMapper.MapToEntity(chat);
//            var message = new Message
//            {
//                Chat = chat,
//                From = user,
//                Text = "/deletechat testchat"
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

//            message.Text = "/deletechat";
//            _bot.RecieveMessage(message).Wait();

//            userServiceMock.Verify(mock => mock.Get(It.Is<long>(_ => _ == user.Id)), Times.Exactly(1));
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
//        public void DeleteChatAdminWithoutPermissionTest()
//        {
//            string userName = "TestUser";
//            var user = new User
//            {
//                Id = 1,
//                Username = userName
//            };
//            var userRepo = _fixture.UserMapper.MapToEntity(user);
//            userRepo.Type = SafeBot.Core.Model.UserType.Admin;
//            var chat = new Chat
//            {
//                Id = 1,
//                Type = ChatType.Private,
//                Title = "TestChat"
//            };
//            var chatRepo = _fixture.ChatMapper.MapToEntity(chat);
//            var message = new Message
//            {
//                Chat = chat,
//                From = user,
//                Text = "/deletechat testchat"
//            };

//            _fixture.MockBotClient.Reset();

//            var userServiceMock = new Mock<IUserService>();
//            userServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == user.Id)))
//                .ReturnsAsync(userRepo);
//            userServiceMock.Setup(s => s.IsAdmin(It.Is<long>(_ => _ == user.Id)))
//                .ReturnsAsync(false);
//            _bot.UserService = userServiceMock.Object;

//            var chatServiceMock = new Mock<IChatService>();
//            chatServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == chat.Id)))
//                .ReturnsAsync(chatRepo);
//            _bot.ChatService = chatServiceMock.Object;

//            _fixture.MockBotClient.Setup(s => s.GetChatAsync(It.Is<ChatId>(
//                _ => _.Identifier == chat.Id), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(chat);
            
//            _bot.RecieveMessage(message).Wait();

//            message.Text = "/deletechat";
//            _bot.RecieveMessage(message).Wait();

//            userServiceMock.Verify(mock => mock.Get(It.Is<long>(_ => _ == user.Id)), Times.Once);
//            userServiceMock.Verify(mock => mock.IsAdmin(It.Is<long>(_ => _ == user.Id)), Times.Once);
//            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
//            userServiceMock.Verify(mock => mock.GetList(), Times.Never);
//            chatServiceMock.Verify(mock => mock.Get(It.Is<long>(_ => _ == chat.Id)), Times.Exactly(2));
//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                 It.IsAny<ChatId>(),
//                 It.Is<string>(_ => _ == SafeBot.Core.Constant.Messages.NoPermissions),
//                 It.IsAny<ParseMode>(),
//                 It.IsAny<IEnumerable<MessageEntity>>(),
//                 It.IsAny<bool>(),
//                 It.IsAny<bool>(),
//                 It.IsAny<int>(),
//                 It.IsAny<bool?>(),
//                 It.IsAny<IReplyMarkup>(),
//                 It.IsAny<CancellationToken>()), Times.Once);
//        }


//        [Fact]
//        public void DeleteChatAdminWithPermissionTest()
//        {
//            string userName = "TestUser";
//            var user = new User
//            {
//                Id = 1,
//                Username = userName
//            };
//            var userRepo = _fixture.UserMapper.MapToEntity(user);
//            userRepo.Type = SafeBot.Core.Model.UserType.Admin;
//            var chat1 = new Chat
//            {
//                Id = 1,
//                Type = ChatType.Private
//            };
//            var chat2 = new Chat
//            {
//                Id = 2,
//                Type = ChatType.Private,
//                Title = "TestChat"
//            };
//            var chatRepo1 = _fixture.ChatMapper.MapToEntity(chat1);
//            var chatRepo2 = _fixture.ChatMapper.MapToEntity(chat2);
//            chatRepo2.Title = "TestChat";
//            var message = new Message
//            {
//                Chat = chat1,
//                From = user,
//                Text = "/deletechat testchat"
//            };

//            var userMessage1 = new SafeBot.Core.Model.UserMessage
//            {
//                Id = 1,
//                ChatId = chat2.Id
//            };
//            var userMessage2 = new SafeBot.Core.Model.UserMessage
//            {
//                Id = 2,
//                ChatId = chat1.Id
//            };


//            _fixture.MockBotClient.Reset();

//            var userServiceMock = new Mock<IUserService>();
//            userServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == user.Id)))
//                .ReturnsAsync(userRepo);
//            userServiceMock.Setup(s => s.IsAdmin(It.Is<long>(_ => _ == user.Id)))
//                .ReturnsAsync(true);
//            _bot.UserService = userServiceMock.Object;

//            var chatServiceMock = new Mock<IChatService>();
//            chatServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == chat1.Id)))
//                .ReturnsAsync(chatRepo1);
//            chatServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == chat2.Id)))
//                .ReturnsAsync(chatRepo2);
//            chatServiceMock.Setup(s => s.Get(It.Is<string>(
//                _ => _.Trim().ToLower() == chat2.Title.Trim().ToLower())))
//                .ReturnsAsync(chatRepo2);
//            _bot.ChatService = chatServiceMock.Object;

//            var userMessageServiceMock = new Mock<IUserMessageService>();
//            userMessageServiceMock.Setup(s => s.GetList())
//                .ReturnsAsync(new SafeBot.Core.Model.UserMessage[] { userMessage1, userMessage2});
//            _bot.UserMessageService = userMessageServiceMock.Object;

//            _fixture.MockBotClient.Setup(s => s.GetChatAsync(It.Is<ChatId>(
//                _ => _.Identifier == chat1.Id), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(chat1);
//            _fixture.MockBotClient.Setup(s => s.GetChatAsync(It.Is<ChatId>(
//                _ => _.Identifier == chat2.Id), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(chat2);

//            _bot.RecieveMessage(message).Wait();

//            message.Text = "/deletechat";
//            _bot.RecieveMessage(message).Wait();

//            userServiceMock.Verify(mock => mock.Get(It.Is<long>(_ => _ == user.Id)), Times.Once);
//            userServiceMock.Verify(mock => mock.IsAdmin(It.Is<long>(_ => _ == user.Id)), Times.Once);
//            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
//            userServiceMock.Verify(mock => mock.GetList(), Times.Never);
//            chatServiceMock.Verify(mock => mock.Get(It.Is<long>(_ => _ == chat1.Id)), Times.Exactly(2));
//            chatServiceMock.Verify(mock => mock.Get(It.Is<string>(
//                _ => _.Trim().ToLower() == chat2.Title.Trim().ToLower())), Times.Once);
//            chatServiceMock.Verify(mock => mock.Delete(It.Is<long>(_ => _ == chat2.Id)), Times.Once);
//            userMessageServiceMock.Verify(mock => mock.Delete(It.IsAny<long>()), Times.Once);
//            _fixture.MockBotClient.Verify(mock => mock.LeaveChatAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat2.Id), It.IsAny<CancellationToken>()),
//                Times.Once);
//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                 It.IsAny<ChatId>(),
//                 It.Is<string>(_ => _.Contains("deleted")),
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
