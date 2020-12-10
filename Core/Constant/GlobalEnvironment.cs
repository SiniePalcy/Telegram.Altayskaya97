using System.Text;

namespace Telegram.Altayskaya97.Core.Constant
{
    public static class GlobalEnvironment
    {
        public static readonly Encoding Encoding = Encoding.UTF8;
#if DEBUG
        public const string BotName = "@altayskaya97_test_bot";
#else
        public const string BotName = "@altayski_bot";
#endif
    }
}
