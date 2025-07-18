using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Ohirun.Commands;
using Ohirun.Configuration;
using Ohirun.Data;
using Ohirun.Services;

namespace Ohirun
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            IHost host = CreateHostBuilder(args).Build();
            
            BotService botService = host.Services.GetRequiredService<BotService>();
            
            try
            {
                await botService.StartAsync();
                await host.RunAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddNLog();
                })
                .ConfigureServices((context, services) =>
                {
                    services.Configure<ApplicationConfig>(context.Configuration.GetSection("Discord"));
                    
                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseSqlite(context.Configuration.GetConnectionString("DefaultConnection")));
                    
                    services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                    {
                        LogLevel = LogSeverity.Info,
                        MessageCacheSize = 100,
                        GatewayIntents = GatewayIntents.Guilds,
                        UseInteractionSnowflakeDate = false,
                        DefaultRetryMode = RetryMode.AlwaysRetry
                    }));
                    
                    services.AddScoped<LunchDecisionService>();
                    services.AddScoped<DataManagementService>();
                    services.AddScoped<OhiruCommand>();
                    services.AddScoped<AddCommand>();
                    services.AddScoped<LinkCommand>();
                    services.AddScoped<ListCommand>();
                    services.AddSingleton<ISlashCommandRegistry, SlashCommandRegistry>();
                    services.AddSingleton<SlashCommandService>();
                    services.AddSingleton<BotService>();
                    services.AddHostedService<BotService>();
                });
    }
}
