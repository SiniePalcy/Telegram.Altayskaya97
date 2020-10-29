using System;
using Telegram.Bot.Types;

namespace Telegram.Altayskaya97.Bot.Helpers
{
    public static class Extensions
    {
        public static string GetUserName(this User user)
        {
            string result;
            if (!string.IsNullOrEmpty(user.Username))
            {
                result = user.Username;
            }
            else
            {
                result = user.FirstName;
                if (!string.IsNullOrEmpty(user.LastName))
                {
                    result += " " + user.LastName;
                }
            }
            return result;
        }

        public static int ParseInt(this string source, int defaultValue)
        {
            if (string.IsNullOrEmpty(source) || !int.TryParse(source, out int result))
                result = defaultValue;

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
