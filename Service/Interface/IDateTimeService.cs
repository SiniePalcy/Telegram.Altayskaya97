using System;

namespace Telegram.SafeBot.Service.Interface
{
    public interface IDateTimeService : IService
    {
        DateTime GetDateTimeUTCNow();
        DateTime GetDateTimeNow();
        string FormatToString(DateTime? dateTime);
    }
}
