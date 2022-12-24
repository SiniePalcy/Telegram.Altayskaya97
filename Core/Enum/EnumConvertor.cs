namespace Telegram.SafeBot.Core.Enum
{
    public static class EnumConvertor
    {
        public static int EnumToInt<T>(T enumValue)
        {
            return (int) (object) enumValue;
        }

        public static T IntToEnum<T>(int intValue)
        {
            return (T)(object)intValue;
        }

    }
}
