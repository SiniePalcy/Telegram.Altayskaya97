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

namespace Telegram.Altayskaya97.Bot
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _serviceProvider;

        private readonly BotClient _api;
        private readonly CancellationTokenSource _cts = new();

        public Worker(
            ILogger<Worker> logger, 
            IServiceProvider serviceProvider,
            BotProperties botProperties)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _api = botProperties.Api;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
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
    }
}
