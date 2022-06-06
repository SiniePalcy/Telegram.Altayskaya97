using System;
using System.Collections.Generic;
using System.Text;

namespace Telegram.Altayskaya97.Core.Enum
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
