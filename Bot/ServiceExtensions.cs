using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Altayskaya97.Bot.Jobs;

namespace Telegram.Altayskaya97.Bot
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddQuartz(this IServiceCollection self, IConfiguration config)
        {
            self.AddQuartz(q =>
            {
                q.MisfireThreshold = TimeSpan.FromSeconds(60);
                q.UseDefaultThreadPool(conf => conf.MaxConcurrency = 5);
                q.UseMicrosoftDependencyInjectionJobFactory();
                q.AddJobAndTrigger<NoWalkJob>(config);
                q.AddJobAndTrigger<ActualUsernameJob>(config);
                q.AddJobAndTrigger<RestrictUserAccessJob>(config);
                q.AddJobAndTrigger<ClearMessagesJob>(config);
            });

            return self;
        }

        private static void AddJobAndTrigger<T>(
            this IServiceCollectionQuartzConfigurator quartz,
            IConfiguration config)
            where T : IJob
        {
            string jobName = typeof(T).Name;

            var configKey = $"Quartz:{jobName}";
            var cronSchedule = config[configKey];

            if (string.IsNullOrEmpty(cronSchedule))
            {
                throw new Exception($"No Quartz.NET Cron schedule found for job in configuration at {configKey}");
            }

            var jobKey = new JobKey(jobName);
            quartz.AddJob<T>(opts => opts.WithIdentity(jobKey));

            quartz.AddTrigger(opts =>
            {
                opts
                    .ForJob(jobKey)
                    .WithIdentity(jobName + "-trigger")
                    .WithCronSchedule(cronSchedule, cron => cron.InTimeZone(TimeZoneInfo.Utc));
            });
        }
    }
}
