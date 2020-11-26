using Moq;
using System.Threading;
using Telegram.Altayskaya97.Service.Interface;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Xunit;

namespace Telegram.Altayskaya97.Test.Integration
{
    public class ReturnTests : IClassFixture<BotFixture>
    {
        private readonly BotFixture _fixture = null;
        private readonly Altayskaya97.Bot.Bot _bot = null;

        public ReturnTests(BotFixture fixture)
        {
            _fixture = fixture;
            _bot = fixture.Bot;
        }

        [Fact]
        public void ReturnUserTest()
        {
            string userName = "TestUser";
            var user = new User
            {
                Id = 1,
                Username = userName,
            };
            var userRepo = _fixture.UserMapper.MapToEntity(user);
            userRepo.Type = Altayskaya97.Core.Model.UserType.Member;
            var chat1 = new Chat
            {
                Id = 1,
                Type = ChatType.Private
            };
            var chat2 = new Chat
            {
                Id = 2,
                Type = ChatType.Group
            };
            var chat3 = new Chat
            {
                Id = 3,
                Type = ChatType.Supergroup
            };
            var chat4 = new Chat
            {
                Id = 4,
                Type = ChatType.Supergroup
            };
            var chatMember1 = new ChatMember
            {
                User = user,
                Status = ChatMemberStatus.Administrator
            };
            var chatMember2 = new ChatMember
            {
                User = user,
                Status = ChatMemberStatus.Kicked
            };
            var chatMember3 = new ChatMember
            {
                User = user,
                Status = ChatMemberStatus.Member
            };
            var chatMember4 = new ChatMember
            {
                User = user,
                Status = ChatMemberStatus.Left
            };

            var chatRepo1 = _fixture.ChatMapper.MapToEntity(chat1);
            chatRepo1.ChatType = Altayskaya97.Core.Model.ChatType.Private;
            var chatRepo2 = _fixture.ChatMapper.MapToEntity(chat2);
            chatRepo2.ChatType = Altayskaya97.Core.Model.ChatType.Public;
            var chatRepo3 = _fixture.ChatMapper.MapToEntity(chat3);
            chatRepo3.ChatType = Altayskaya97.Core.Model.ChatType.Public;
            var chatRepo4 = _fixture.ChatMapper.MapToEntity(chat4);
            chatRepo4.ChatType = Altayskaya97.Core.Model.ChatType.Admin;
            var chats = new Altayskaya97.Core.Model.Chat[] { chatRepo1, chatRepo2, chatRepo3, chatRepo4 };

            var message = new Message
            {
                Chat = chat1,
                From = user,
                Text = "/triton"
            };

            _fixture.MockBotClient.Reset();
            _fixture.MockBotClient.Setup(b => b.GetChatAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id),
                It.IsAny<CancellationToken>())).ReturnsAsync(chat1);
            _fixture.MockBotClient.Setup(b => b.GetChatAsync(It.Is<ChatId>(_ => _.Identifier == chat2.Id),
                It.IsAny<CancellationToken>())).ReturnsAsync(chat2);
            _fixture.MockBotClient.Setup(b => b.GetChatAsync(It.Is<ChatId>(_ => _.Identifier == chat3.Id),
                It.IsAny<CancellationToken>())).ReturnsAsync(chat3);
            _fixture.MockBotClient.Setup(b => b.GetChatAsync(It.Is<ChatId>(_ => _.Identifier == chat4.Id),
                It.IsAny<CancellationToken>())).ReturnsAsync(chat4);


            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(s => s.Get(It.Is<long>(_ => _ == user.Id)))
                .ReturnsAsync(userRepo);
            userServiceMock.Setup(s => s.IsAdmin(It.Is<long>(_ => _ == user.Id)))
                .ReturnsAsync(userRepo.IsAdmin);
            _bot.UserService = userServiceMock.Object;

            var chatServiceMock = new Mock<IChatService>();
            chatServiceMock.Setup(s => s.GetList())
                .ReturnsAsync(chats);
            _bot.ChatService = chatServiceMock.Object;

            var clientMock = _fixture.MockBotClient;
            clientMock.Setup(s => s.GetChatMemberAsync(It.Is<ChatId>(_ => _.Identifier == chat1.Id),
                It.Is<int>(_ => _ == user.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(chatMember1);
            clientMock.Setup(s => s.GetChatMemberAsync(It.Is<ChatId>(_ => _.Identifier == chat2.Id),
                It.Is<int>(_ => _ == user.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(chatMember2);
            clientMock.Setup(s => s.GetChatMemberAsync(It.Is<ChatId>(_ => _.Identifier == chat3.Id),
                It.Is<int>(_ => _ == user.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(chatMember4);
            clientMock.Setup(s => s.GetChatMemberAsync(It.Is<ChatId>(_ => _.Identifier == chat4.Id),
                It.Is<int>(_ => _ == user.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(chatMember4);
            _bot.RecieveMessage(message).Wait();

            userServiceMock.Verify(mock => mock.Get(It.Is<long>(_ => _ == user.Id)), Times.Once);
            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
            userServiceMock.Verify(mock => mock.GetList(), Times.Never);
            chatServiceMock.Verify(mock => mock.GetList(), Times.Once);
            _fixture.MockBotClient.Verify(mock => mock.GetChatMemberAsync(
                It.IsAny<ChatId>(), It.Is<int>(_ => _ == user.Id), It.IsAny<CancellationToken>()), 
                Times.Exactly(2));
            _fixture.MockBotClient.Verify(mock => mock.ExportChatInviteLinkAsync(
                It.IsAny<ChatId>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            _fixture.MockBotClient.Verify(mock => mock.UnbanChatMemberAsync(It.IsAny<ChatId>(),
                It.Is<int>(_ => _ == user.Id), It.IsAny<CancellationToken>()), Times.Once);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
                It.Is<ChatId>(_ => _.Identifier == chat1.Id),
                It.IsAny<string>(),
                It.IsAny<ParseMode>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<int>(),
                It.IsAny<IReplyMarkup>(),
                It.IsAny<CancellationToken>()), Times.Exactly(2));
        }
    }
}
