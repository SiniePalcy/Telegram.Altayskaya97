//using Moq;
//using System;
//using System.Collections.Generic;
//using System.Threading;
//using Telegram.SafeBot.Bot.Helpers;
//using Telegram.SafeBot.Core.Constant;
//using Telegram.SafeBot.Service.Interface;
//using Telegram.Bot;
//using Telegram.Bot.Types;
//using Telegram.Bot.Types.Enums;
//using Telegram.Bot.Types.ReplyMarkups;
//using Xunit;

//namespace Telegram.SafeBot.Test.Integration
//{
//    public class IWalkTests : IClassFixture<BotFixture>
//    {
//        private readonly BotFixture _fixture = null;
//        private readonly SafeBot.Bot.Bot _bot = null;

//        public IWalkTests(BotFixture fixture)
//        {
//            _fixture = fixture;
//            _bot = fixture.Bot;
//        }

//        [Fact]
//        public void IWalkTest()
//        {
//            var chat1 = new Chat
//            {
//                Id = 1,
//                Type = ChatType.Private
//            };
//            var chat2 = new Chat
//            {
//                Id = 2,
//                Type = ChatType.Group
//            };
//            var chatRepo1 = _fixture.ChatMapper.MapToEntity(chat1);
//            chatRepo1.ChatType = SafeBot.Core.Model.ChatType.Admin;
//            var chatRepo2 = _fixture.ChatMapper.MapToEntity(chat2);
//            chatRepo1.ChatType = SafeBot.Core.Model.ChatType.Public;
//            var chats = new SafeBot.Core.Model.Chat[] { chatRepo1, chatRepo2 };

//            string userName = "TestUser";
//            var user = new User
//            {
//                Id = 1,
//                Username = userName + "1",
//            };

//            var userRepo = _fixture.UserMapper.MapToEntity(user);
//            userRepo.Type = SafeBot.Core.Model.UserType.Member;
//            userRepo.Name = user.GetUserName();
//            var users = new SafeBot.Core.Model.User[] { userRepo };

//            ChatMember chatMember = new ChatMemberMember();

//            var message = new Message
//            {
//                Chat = chat1,
//                From = user,
//                Text = "/iwalk"
//            };

//            _fixture.MockBotClient.Reset();

//            var userServiceMock = new Mock<IUserService>();
//            userServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == user.Id)))
//                .ReturnsAsync(userRepo);
//            userServiceMock.Setup(s => s.GetByName(It.Is<string>(_ => _.ToLower() == user.Username.ToLower())))
//                .ReturnsAsync(userRepo);
//            userServiceMock.Setup(s => s.GetByIdOrName(It.Is<string>(_ => _.ToLower() == user.Username.ToLower())))
//                .ReturnsAsync(userRepo);
//            userServiceMock.Setup(s => s.GetByIdOrName(It.Is<string>(_ => _ == user.Id.ToString())))
//                .ReturnsAsync(userRepo);
//            userServiceMock.Setup(s => s.IsAdmin(It.Is<long>(_ => _ == user.Id)))
//                .ReturnsAsync(false);
//            _bot.UserService = userServiceMock.Object;

//            var chatServiceMock = new Mock<IChatService>();
//            chatServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == chat1.Id)))
//                .ReturnsAsync(chatRepo1);
//            chatServiceMock.SetupSequence(s => s.GetList())
//                .ReturnsAsync(new SafeBot.Core.Model.Chat[0])
//                .ReturnsAsync(new SafeBot.Core.Model.Chat[] { chatRepo1, chatRepo2 });
//            _bot.ChatService = chatServiceMock.Object;

//            _fixture.MockBotClient.Setup(s => s.GetChatAsync(It.Is<ChatId>(_ => _.Identifier == 1),
//                It.IsAny<CancellationToken>())).ReturnsAsync(chat1);
//            _fixture.MockBotClient.Setup(s => s.GetChatAsync(It.Is<ChatId>(_ => _.Identifier == 2),
//                It.IsAny<CancellationToken>())).ReturnsAsync(chat2);
//            _fixture.MockBotClient.Setup(s => s.GetChatMemberAsync(It.IsAny<ChatId>(),
//                It.Is<int>(_ => _ == user.Id), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(chatMember);

//            _bot.RecieveMessage(message).Wait();
            
//            _bot.RecieveMessage(message).Wait();

//            userServiceMock.Verify(mock => mock.Get(It.IsAny<long>()), Times.Exactly(2));
//            userServiceMock.Verify(mock => mock.GetByIdOrName(It.IsAny<string>()), Times.Exactly(2));
//            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
//            userServiceMock.Verify(mock => mock.GetList(), Times.Never);
//            chatServiceMock.Verify(mock => mock.GetList(), Times.Exactly(2));
//            _fixture.MockBotClient.Verify(mock => mock.BanChatMemberAsync(
//                It.Is<ChatId>(_ => _.Identifier == chatRepo1.Id || _.Identifier == chatRepo2.Id),
//                It.Is<int>(_ => _ == userRepo.Id), 
//                It.IsAny<DateTime>(),
//                It.IsAny<bool?>(),
//                It.IsAny<CancellationToken>()),
//                Times.Exactly(2));
//            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
//                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
//                It.Is<string>(_ => _ == Messages.NoAnyChats), 
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
//                It.Is<string>(_ => _.Contains("kicked from chat")), 
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

//    }
//}
