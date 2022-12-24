using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.BotAPI;
using Telegram.BotAPI.GettingUpdates;
using Telegram.SafeBot.Service.Interface;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;
using Telegram.SafeBot.Service.Extensions;
using Telegram.SafeBot.Core.Model;
using Telegram.SafeBot.Model;

namespace Telegram.SafeBot.Bot
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly BotClient _api;
        private readonly CancellationTokenSource _cts = new();
        private readonly string _ownerId;
        private readonly IPasswordService _passwordService;
        private readonly IUserService _userService;
        private readonly IChatService _chatService;

        public Worker(
            ILogger<Worker> logger, 
            IServiceProvider serviceProvider,
            BotProperties botProperties,
            IPasswordService passwordService,
            IConfiguration configurationService,
            IUserService userService,
            IChatService chatService)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _api = botProperties.Api;

            _passwordService = passwordService;
            _userService = userService;
            _chatService = chatService;

            _ownerId = configurationService["owner_id"];

        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await InitDb();
            _logger.LogInformation("Bot starting at: {Time}", DateTimeOffset.Now);
            await base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _cts.Cancel();
            _logger.LogInformation("Bot stopping at: {Time}", DateTimeOffset.Now);
            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker running at: {Time}", DateTimeOffset.Now);

            // Long Polling
            var updates = await _api.GetUpdatesAsync(cancellationToken: stoppingToken).ConfigureAwait(false);
            while (!stoppingToken.IsCancellationRequested)
            {
                if (updates.Any())
                {
                    Parallel.ForEach(updates, (update) => ProcessUpdate(update));

                    updates = await _api.GetUpdatesAsync(updates[^1].UpdateId + 1, cancellationToken: stoppingToken).ConfigureAwait(false);
                }
                else
                {
                    updates = await _api.GetUpdatesAsync(cancellationToken: stoppingToken).ConfigureAwait(false);
                }
            }
        }

        private void ProcessUpdate(Update update)
        {
            using var scope = _serviceProvider.CreateScope();
            var bot = scope.ServiceProvider.GetRequiredService<Bot>();
            bot.OnUpdate(update);
        }

        private async Task InitDb()
        {
            var passwords = await _passwordService.GetList();
#if DEBUG
            foreach (var pass in passwords)
                await _passwordService.Delete(pass.Id);
            passwords = await _passwordService.GetList();
#endif
            var maxId = !passwords.Any() ? 0 : passwords.Select(p => p.Id).Max();
            if (!passwords.Any(p => p.ChatType == Core.Model.ChatType.Admin))
            {
                await _passwordService.Add(new Password
                {
                    Id = ++maxId,
                    ChatType = Core.Model.ChatType.Admin,
                    Value = "/admin"
                });
            }
            if (!passwords.Any(p => p.ChatType == Core.Model.ChatType.Public))
            {
                await _passwordService.Add(new Password
                {
                    Id = ++maxId,
                    ChatType = Core.Model.ChatType.Public,
                    Value = "/public"
                });
            }

            var userList = await _userService.GetList();
            if (!userList.Any())
            {
                await _userService.Add(new Core.Model.User
                {
                    Id = long.Parse(_ownerId),
                    Name = Environment.GetEnvironmentVariable("admin_name"),
                    Type = UserType.Admin,
                    IsAdmin = true,
                });
            }

            var chatList = await _chatService.GetList();
            if (!chatList.Any())
                return;

            var adminChats = chatList.Where(c => c.ChatType == Core.Model.ChatType.Admin);
            List<ChatMember> admins = new List<ChatMember>();
            foreach (var adminChat in adminChats)
            {
                try
                {
                    var adminsOfChat = await _api.GetChatAdministratorsAsync(adminChat.Id);
                    admins.AddRange(adminsOfChat.Where(usr => !usr.User.IsBot));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Chat {adminChat.Title} is unavailable and will be deleted");
                    await _chatService.Delete(adminChat.Id);
                }
            }

            foreach (var admin in admins)
            {
                var userInRepo = await _userService.Get(admin.User.Id);
                if (userInRepo != null)
                {
                    _logger.LogInformation($"User with id={userInRepo.Id}, name={userInRepo.Name} is already exist");
                    continue;
                }

                string userName = admin.User.GetUserName();
                var newUser = new Core.Model.User
                {
                    Id = admin.User.Id,
                    Name = userName,
                    IsAdmin = true,
                    Type = admin.User.IsBot ? UserType.Bot : UserType.Admin
                };
                await _userService.Add(newUser);
                _logger.LogInformation($"User saved with id={newUser.Id}, name={newUser.Name}, type={newUser.Type}");
            }
        }

    }
}
