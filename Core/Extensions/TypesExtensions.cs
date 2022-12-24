using System;

namespace Telegram.SafeBot.Core.Extensions
{
    public static class TypesExtensions
    {
        public static bool IsCommand(this string self)
        {
            return self != null && self.StartsWith('/'); 
        }

        public static int ParseInt(this string source, int defaultValue)
        {
            if (string.IsNullOrEmpty(source) || !int.TryParse(source, out int result))
                result = defaultValue;

            return result;
        }

        public static string ParseString(this string source, string defaultValue)
        {
            string result = defaultValue;
            if (!string.IsNullOrEmpty(source))
                result = source;

            return result;
        }

        public static TimeSpan ParseTimeSpan(this string source, TimeSpan defaultValue)
        {
            if (string.IsNullOrEmpty(source))
                return defaultValue;

            var parts = source.Split(':');
            if (parts.Length != 2 && parts.Length != 3)
            {
                parts = source.Split('.');
                if (parts.Length != 2 && parts.Length != 3)
                    return defaultValue;
            }

            int seconds = 0;

            if (!int.TryParse(parts[0], out int hours) || !int.TryParse(parts[1], out int minutes))
                return defaultValue;

            if (parts.Length == 3)
            {
                if (!int.TryParse(parts[2], out seconds))
                    return defaultValue;
            }

            return new TimeSpan(hours, minutes, seconds);
        }
    }

}
