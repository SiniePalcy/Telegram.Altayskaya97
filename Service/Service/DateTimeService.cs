using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Altayskaya97.Service.Interface;

namespace Telegram.Altayskaya97.Service
{
    public class DateTimeService : IDateTimeService
    {
        public string FormatToString(DateTime? dateTime)
        {
            return dateTime?.ToFileTimeUtc().ToString("dd-MM-yyyy HH:mm") ?? string.Empty;
        }

        public DateTime GetDateTimeUTCNow()
        {
            return DateTime.UtcNow;
        }
    }
}
