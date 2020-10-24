using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Altayskaya97.Model.DbContext;
using Telegram.Altayskaya97.Model.Interface;
using Telegram.Altayskaya97.Model.Middleware;
using Telegram.Altayskaya97.Service;
using Telegram.Altayskaya97.Service.Interface;

namespace Telegram.Altayskaya97.Bot
{
    public class Program
    {
        private static IConfiguration Configuration { get; set; }

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    Configuration = hostContext.Configuration;
                    services.AddHostedService<Bot>();
                    services.AddSingleton<IDbContext, DynamoDbContext>(InitDynamoDbContext);
                    services.AddTransient<IWelcomeService, WelcomeService>();
                    services.AddTransient<IMenuService, MenuService>();
                    services.AddTransient<IUserService, UserService>();
                    services.AddTransient<IChatService, ChatService>();
                });

        private static DynamoDbContext InitDynamoDbContext(IServiceProvider provider)
        {
            string connString = Configuration.GetSection("Configuration").GetSection("ConnectionStrings").GetSection("DynamoConnectionString").Value;
            return new DynamoDbContext(connString);
        }
    }
}
