using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Telegram.Altayskaya97.Model.DbContext;
using Telegram.Altayskaya97.Model.Extensions;
using Telegram.Altayskaya97.Model.Interface;
using Telegram.Altayskaya97.Bot;
using Telegram.Altayskaya97.Service;
using Telegram.Altayskaya97.Service.Interface;
using Telegram.Altayskaya97.Service.Service;

namespace Telegram.Altayskaya97.Bot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton(hostContext.Configuration);
                    services.AddSingleton<BotProperties>();
                    services.AddScoped<Bot>();                   
                    //services.AddDynamoDbRepositories();
                    services.AddMongoDbRepositories();
                    services.AddTransient<IButtonsService, ButtonsService>();
                    services.AddTransient<IMenuService, MenuService>();
                    services.AddTransient<IUserService, UserService>();
                    services.AddTransient<IChatService, ChatService>();
                    services.AddTransient<IUserMessageService, UserMessageService>();
                    services.AddTransient<IPasswordService, PasswordService>();
                    services.AddTransient<IDateTimeService, DateTimeService>();
                    services.AddSingleton<IStateMachineContainer, StateMachineContainer>();
                    services.AddQuartz(hostContext.Configuration);
                    services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
                    services.AddHostedService<Worker>();
                });
    }
}
