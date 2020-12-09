using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Altayskaya97.Bot.Enum;
using Telegram.Altayskaya97.Bot.Helpers;
using Xunit;

namespace Telegram.Altayskaya97.Test.Bot
{
    public class EnumConvertorTest
    {
        [Fact]
        public void EnumToIntTest()
        {
            ClearState state = ClearState.None;
            var intValue = EnumConvertor.EnumToInt(state);
            Assert.Equal(0, intValue);

            state = ClearState.Start;
            intValue = EnumConvertor.EnumToInt(state);
            Assert.Equal(1, intValue);
        }

        [Fact]
        public void IntToEnumTest()
        {
            var intValue = 0;
            ClearState state = EnumConvertor.IntToEnum<ClearState>(intValue);
            Assert.Equal(ClearState.None, state);

            intValue = 4;
            state = EnumConvertor.IntToEnum<ClearState>(intValue);
            Assert.Equal(ClearState.Stop, state);
        }
    }
}
