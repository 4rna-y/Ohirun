using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ohirun
{
    public class BotService : BackgroundService
    {
        private readonly DiscordSocketClient client;
        private readonly ILogger<BotService> logger;
        private readonly DiscordConfig discordConfig;

        public BotService(DiscordSocketClient client, ILogger<BotService> logger, IOptions<DiscordConfig> discordOptions)
        {
            this.client = client;
            this.logger = logger;
            this.discordConfig = discordOptions.Value;
        }

        public async Task StartAsync()
        {
            client.Log += LogAsync;
            client.Ready += ReadyAsync;
            client.MessageReceived += MessageReceivedAsync;

            if (string.IsNullOrEmpty(discordConfig.Token))
            {
                throw new InvalidOperationException("Discord token is not configured in appsettings.json");
            }

            await client.LoginAsync(TokenType.Bot, discordConfig.Token);
            await client.StartAsync();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }

        private Task LogAsync(LogMessage log)
        {
            LogLevel logLevel = log.Severity switch
            {
                LogSeverity.Critical => LogLevel.Critical,
                LogSeverity.Error => LogLevel.Error,
                LogSeverity.Warning => LogLevel.Warning,
                LogSeverity.Info => LogLevel.Information,
                LogSeverity.Verbose => LogLevel.Debug,
                LogSeverity.Debug => LogLevel.Trace,
                _ => LogLevel.Information
            };

            logger.Log(logLevel, log.Exception, "{Source}: {Message}", log.Source, log.Message);
            return Task.CompletedTask;
        }

        private Task ReadyAsync()
        {
            logger.LogInformation("Bot is connected and ready!");
            return Task.CompletedTask;
        }

        private Task MessageReceivedAsync(SocketMessage message)
        {
            if (message.Author.IsBot)
                return Task.CompletedTask;

            logger.LogInformation("Message from {Username}: {Content}", message.Author.Username, message.Content);
            return Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await client.LogoutAsync();
            await client.StopAsync();
            await base.StopAsync(cancellationToken);
        }
    }
}