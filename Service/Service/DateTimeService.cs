using System;
using System.Collections.Generic;
using System.Text;
using Telegram.SafeBot.Service.Interface;

namespace Telegram.SafeBot.Service
{
    public class DateTimeService : IDateTimeService
    {
        public string FormatToString(DateTime? dateTime)
        {
            return dateTime.HasValue ? dateTime.Value.ToUniversalTime().ToString("dd-MM-yyyy HH:mm") : string.Empty;
        }

        public DateTime GetDateTimeUTCNow()
        {
            return DateTime.UtcNow;
        }

        public DateTime GetDateTimeNow()
        {
            return DateTime.Now;
        }
    }
}
