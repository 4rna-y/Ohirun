using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Ohirun.Models;
using Ohirun.Services;

namespace Ohirun.Commands
{
    public class OhiruCommand : ISlashCommand
    {
        private readonly ILogger<OhiruCommand> logger;
        private readonly LunchDecisionService lunchDecisionService;

        public string Name => "ohiru";

        public OhiruCommand(ILogger<OhiruCommand> logger, LunchDecisionService lunchDecisionService)
        {
            this.logger = logger;
            this.lunchDecisionService = lunchDecisionService;
        }

        public SlashCommandBuilder GetCommandBuilder()
        {
            return new SlashCommandBuilder()
                .WithName(Name)
                .WithDescription("お昼を決めるコマンド - ランダムに店と食べ物を選びます");
        }

        public async Task HandleAsync(SocketSlashCommand command)
        {
            try
            {
                await command.DeferAsync();

                LunchDecision decision = await lunchDecisionService.DecideRandomLunchAsync(
                    command.User.Id.ToString(), 
                    command.User.Username);
                
                string suggestion = $"**{decision.Store.Name}**で**{decision.Meal.Name}**を買うといいでしょう！";
                
                EmbedBuilder embed = new EmbedBuilder()
                    .WithTitle("🍽️ お昼の提案")
                    .WithDescription(suggestion)
                    .WithColor(Color.Green)
                    .WithTimestamp(DateTimeOffset.Now);

                if (!string.IsNullOrEmpty(decision.Meal.Description))
                {
                    embed.AddField("説明", decision.Meal.Description, false);
                }

                await command.FollowupAsync(embed: embed.Build());
                logger.LogInformation("User {Username} used /ohiru command - decided: {StoreName} - {MealName}", 
                    command.User.Username, decision.Store.Name, decision.Meal.Name);
            }
            catch (InvalidOperationException ex)
            {
                await command.FollowupAsync(ex.Message, ephemeral: true);
                logger.LogWarning("No available options for /ohiru command: {Message}", ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to respond to /ohiru command");
                await command.FollowupAsync("コマンドの処理中にエラーが発生しました", ephemeral: true);
            }
        }
    }
}