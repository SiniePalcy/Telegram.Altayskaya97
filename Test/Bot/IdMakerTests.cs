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
            var dtNow = DateTime.UtcNow;
            Console.WriteLine(dtNow.Millisecond);
            Console.WriteLine(dtNow.Ticks);
        }
    }
}
