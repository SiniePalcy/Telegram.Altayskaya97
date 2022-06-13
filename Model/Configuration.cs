namespace Telegram.Altayskaya97.Model
{
    public class Configuration
    {
        public int PeriodEchoSec { get; set; } = 30;
        public int PeriodResetAccessMin { get; set; } = 30;
        public int PerionChatListMin { get; set; } = 180;
        public int PeriodClearPrivateChatMin { get; set; } = 20;
        public int PeriodClearGroupChatHours { get; set; }
        public int PeriodInactiveUserDays { get; set; } = 7;
        public int OwnerId { get; set; }
    }
}
