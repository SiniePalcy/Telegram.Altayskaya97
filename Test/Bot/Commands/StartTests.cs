﻿using Moq;
using System.Threading;
using Telegram.Altayskaya97.Model.Middleware;
using Telegram.Altayskaya97.Service.Interface;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Xunit;


namespace Telegram.Altayskaya97.Test.Bot
{
    public class StartTests : IClassFixture<BotFixture>
    {
        private BotFixture _fixture = null;
        private Altayskaya97.Bot.Bot _bot = null;

        public StartTests(BotFixture fixture)
        {
            _fixture = fixture;
            _bot = fixture.Bot;
        }

        [Fact]
        public void UnknownMessageTest()
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
                Type = ChatType.Private
            };
            var message = new Message
            {
                Chat = chat,
                From = user,
                Text = "/sturt"
            };

            _fixture.MockBotClient.Reset();

            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(s => s.GetUser(It.IsAny<string>()))
                .ReturnsAsync(default(Core.Model.User));
            _bot.UserService = userServiceMock.Object;

            _bot.RecieveMessage(message).Wait();

            userServiceMock.Verify(mock => mock.GetUser(It.IsAny<long>()), Times.Never);
            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
            userServiceMock.Verify(mock => mock.BanUser(It.IsAny<long>()), Times.Never);
            userServiceMock.Verify(mock => mock.UnbanUser(It.IsAny<long>()), Times.Never);
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
        public void StartTest()
        {
            string userName = "TestUser";
            var user = new User
            {
                Id = 1,
                Username = userName
            };
            var userRepo = _fixture.UserMapper.MapToEntity(user);
            var chat = new Chat
            {
                Id = 1,
                Type = ChatType.Private
            };
            var message = new Message
            {
                Chat = chat,
                From = user,
                Text = "/start"
            };

            _fixture.MockBotClient.Reset();

            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(s => s.GetUser(It.IsAny<string>()))
                .ReturnsAsync(default(Core.Model.User));
            _bot.UserService = userServiceMock.Object;

            _bot.RecieveMessage(message).Wait();

            userServiceMock.Verify(mock => mock.GetUser(It.Is<long>(_ => _ == 1)), Times.Once);
            userServiceMock.Verify(mock => mock.PromoteUserAdmin(It.IsAny<long>()), Times.Never);
            userServiceMock.Verify(mock => mock.BanUser(It.IsAny<long>()), Times.Never);
            userServiceMock.Verify(mock => mock.UnbanUser(It.IsAny<long>()), Times.Never);
            _fixture.MockBotClient.Verify(mock => mock.SendTextMessageAsync(
                 It.Is<ChatId>(_ => _.Identifier == chat.Id),
                 It.IsAny<string>(),
                 It.Is<ParseMode>(_ => _ == ParseMode.Html),
                 It.IsAny<bool>(),
                 It.IsAny<bool>(),
                 It.IsAny<int>(),
                 It.IsAny<InlineKeyboardMarkup>(),
                 It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}