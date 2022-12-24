using System;
using System.Linq;
using System.Diagnostics;
using Telegram.SafeBot.Core.Constant;
using Telegram.SafeBot.Core.Helpers;
using Telegram.SafeBot.Core.Model;
using Xunit;
using Xunit.Abstractions;
using Telegram.SafeBot.Core.Extensions;

namespace Telegram.SafeBot.Test.Bot
{
    public class HashHelperTest
    {
        [Theory()]
        [InlineData("/12345")]
        [InlineData("/admin")]
        [InlineData("/cpihsc")]
        [InlineData("/shepecis")]
        [InlineData("/retont")]
        [InlineData("/tnoter")]
        [InlineData("/ritont")]
        [InlineData("/tnotir")]
        [InlineData("/withBigSymbols")]
        [InlineData("/абвгдёжйъ")]
        public void GetHash(string pass)
        {
            
            var encoding = GlobalEnvironment.Encoding;

            var hash = HashHelper.ComputeHash(pass, encoding);

            var hashStr = HashHelper.GetString(hash);
            Console.WriteLine(hashStr);

            var hashBytes = HashHelper.GetBytes(hashStr);

            Assert.True(hash.Same(hashBytes));
        }
    }
}
