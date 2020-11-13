using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Altayskaya97.Bot.Helpers;
using Xunit;

namespace Telegram.Altayskaya97.Test.Bot
{
    public class IdMakerTests
    {
        [Fact]
        public void IdMakerTest()
        {
            long chatId = -1001461233648;
            int telegramMessageId = 20037;
            var messageId = IdMaker.MakeMessageId(chatId, telegramMessageId);
            Assert.Equal(-100146123364820037, messageId);
            telegramMessageId = IdMaker.GetTelegramMessageId(messageId, chatId);
            Assert.Equal(20037, telegramMessageId);
            
            chatId = 1373638014;
            telegramMessageId = 4439;
            messageId = IdMaker.MakeMessageId(chatId, telegramMessageId);
            Assert.Equal(13736380144439, messageId);
            telegramMessageId = IdMaker.GetTelegramMessageId(messageId, chatId);
            Assert.Equal(4439, telegramMessageId);

            chatId = -10001461233648;
            telegramMessageId = 20037;
            messageId = IdMaker.MakeMessageId(chatId, telegramMessageId);
            Assert.Equal(-1000146123364820037, messageId);
            telegramMessageId = IdMaker.GetTelegramMessageId(messageId, chatId);
            Assert.Equal(20037, telegramMessageId);

            chatId = -5001461233648;
            telegramMessageId = 800000;
            messageId = IdMaker.MakeMessageId(chatId, telegramMessageId);
            Assert.Equal(-5001461233648800000, messageId);
            telegramMessageId = IdMaker.GetTelegramMessageId(messageId, chatId);
            Assert.Equal(800000, telegramMessageId);
        }
    }
}
