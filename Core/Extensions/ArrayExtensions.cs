using System;
using System.Collections.Generic;
using System.Text;

namespace Telegram.SafeBot.Core.Extensions
{
    public static class ArrayExtensions
    {
        public static bool Same(this byte[] source, byte[] arg)
        {
            for (int i = 0; i < source.Length; i++)
            {
                if (source[i] != arg[i])
                    return false;
            }
            return true;
        }
    }
}
