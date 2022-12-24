using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Telegram.SafeBot.Model.Extensions;
using Telegram.SafeBot.Service;
using Telegram.SafeBot.Service.Interface;
using Telegram.SafeBot.Service.Service;

namespace Telegram.SafeBot.Bot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = CreateHostBuilder(args);
            var props = builder.Properties;
            //configuration[$"Telegram:{botName}"];
            builder.Build().Run();
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
