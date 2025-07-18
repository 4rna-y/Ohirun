using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ohirun.Configuration;

namespace Ohirun.Services
{
    public class BotService : BackgroundService
    {
        private readonly DiscordSocketClient client;
        private readonly ILogger<BotService> logger;
        private readonly ApplicationConfig applicationConfig;
        private readonly SlashCommandService slashCommandService;

        public BotService(DiscordSocketClient client, ILogger<BotService> logger, IOptions<ApplicationConfig> applicationOptions, SlashCommandService slashCommandService)
        {
            this.client = client;
            this.logger = logger;
            this.applicationConfig = applicationOptions.Value;
            this.slashCommandService = slashCommandService;
        }

        public async Task StartAsync()
        {
            client.Log += LogAsync;
            client.Ready += ReadyAsync;
            client.MessageReceived += MessageReceivedAsync;
            client.SlashCommandExecuted += SlashCommandExecutedAsync;

            if (string.IsNullOrEmpty(applicationConfig.Token))
            {
                throw new InvalidOperationException("Discord token is not configured in appsettings.json");
            }

            await client.LoginAsync(TokenType.Bot, applicationConfig.Token);
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

        private async Task ReadyAsync()
        {
            logger.LogInformation("Bot is connected and ready!");
            await slashCommandService.RegisterCommandsAsync();
        }

        private Task MessageReceivedAsync(SocketMessage message)
        {
            if (message.Author.IsBot)
                return Task.CompletedTask;

            logger.LogInformation("Message from {Username}: {Content}", message.Author.Username, message.Content);
            return Task.CompletedTask;
        }

        private async Task SlashCommandExecutedAsync(SocketSlashCommand command)
        {
            await slashCommandService.HandleSlashCommandAsync(command);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await client.LogoutAsync();
            await client.StopAsync();
            await base.StopAsync(cancellationToken);
        }
    }
}