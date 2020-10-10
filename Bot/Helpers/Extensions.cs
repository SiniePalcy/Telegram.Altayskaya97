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
    }
}
