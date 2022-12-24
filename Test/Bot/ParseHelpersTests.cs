using System;
using Telegram.SafeBot.Core.Extensions;
using Xunit;

namespace Telegram.SafeBot.Test.Bot
{
    public class ParseHelpersTests
    {
        [Fact]
        public void ParseTimespanTest()
        {
            TimeSpan defaultTs = new TimeSpan(10, 20, 30);

            string case1 = "13:30";
            var parsedTs = case1.ParseTimeSpan(defaultTs);
            Assert.Equal(parsedTs, new TimeSpan(13, 30, 00));

            string case2 = "13:30:45";
            var parsedTs2 = case2.ParseTimeSpan(defaultTs);
            Assert.Equal(parsedTs2, new TimeSpan(13, 30, 45));

            string case3 = "10.30.46";
            var parsedTs3 = case3.ParseTimeSpan(defaultTs);
            Assert.Equal(parsedTs3, new TimeSpan(10, 30, 46));

            string case4 = "12.20.a";
            var parsedTs4 = case4.ParseTimeSpan(defaultTs);
            Assert.Equal(parsedTs4, defaultTs);


        }
    }
}
