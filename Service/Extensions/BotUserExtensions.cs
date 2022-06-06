using Telegram.BotAPI.AvailableTypes;

namespace Telegram.Altayskaya97.Service.Extensions
{
    public static class BotUserExtensions
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
