namespace Telegram.Altayskaya97.Core.Constant
{
    public static class GlobalEnvironment
    {
#if DEBUG
        public const string BotName = "@altayskaya97_test_bot";
#else
        public const string BotName = "@altayski_bot";
#endif
    }
}
