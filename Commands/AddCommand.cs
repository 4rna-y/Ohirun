using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Ohirun.Models;
using Ohirun.Services;

namespace Ohirun.Commands
{
    public class AddCommand : ISlashCommand
    {
        private readonly ILogger<AddCommand> logger;
        private readonly DataManagementService dataManagementService;

        public string Name => "add";

        public AddCommand(ILogger<AddCommand> logger, DataManagementService dataManagementService)
        {
            this.logger = logger;
            this.dataManagementService = dataManagementService;
        }

        public SlashCommandBuilder GetCommandBuilder()
        {
            return new SlashCommandBuilder()
                .WithName(Name)
                .WithDescription("店舗または食べ物を追加します")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("store")
                    .WithDescription("新しい店舗を追加します")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                    .AddOption("name", ApplicationCommandOptionType.String, "店舗名", isRequired: true)
                    .AddOption("genre", ApplicationCommandOptionType.String, "ジャンル", isRequired: true))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("meal")
                    .WithDescription("新しい食べ物を追加します")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                    .AddOption("name", ApplicationCommandOptionType.String, "食べ物の名前", isRequired: true)
                    .AddOption("foodtype", ApplicationCommandOptionType.Integer, "食べ物の種類 (1:コメ, 2:麺, 3:パン)", isRequired: true, minValue: 1, maxValue: 3)
                    .AddOption("description", ApplicationCommandOptionType.String, "説明 (任意)", isRequired: false));
        }

        public async Task HandleAsync(SocketSlashCommand command)
        {
            try
            {
                await command.DeferAsync();

                SocketSlashCommandDataOption subCommand = command.Data.Options.First();
                
                switch (subCommand.Name)
                {
                    case "store":
                        await HandleAddStoreAsync(command, subCommand);
                        break;
                    case "meal":
                        await HandleAddMealAsync(command, subCommand);
                        break;
                    default:
                        await command.FollowupAsync("❌ 不明なサブコマンドです", ephemeral: true);
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to handle /add command");
                await command.FollowupAsync("❌ コマンドの処理中にエラーが発生しました", ephemeral: true);
            }
        }

        private async Task HandleAddStoreAsync(SocketSlashCommand command, SocketSlashCommandDataOption subCommand)
        {
            try
            {
                string name = subCommand.Options.First(o => o.Name == "name").Value.ToString()!;
                string genre = subCommand.Options.First(o => o.Name == "genre").Value.ToString()!;

                Store store = await dataManagementService.AddStoreAsync(name, genre);

                EmbedBuilder embed = new EmbedBuilder()
                    .WithTitle("✅ 店舗を追加しました")
                    .WithColor(Color.Green)
                    .AddField("店舗名", store.Name, true)
                    .AddField("ジャンル", store.Genre, true)
                    .AddField("ID", store.Id.ToString(), true)
                    .WithTimestamp(DateTimeOffset.Now);

                await command.FollowupAsync(embed: embed.Build());
                logger.LogInformation("User {Username} added store: {StoreName} ({Genre})", 
                    command.User.Username, store.Name, store.Genre);
            }
            catch (ArgumentException ex)
            {
                await command.FollowupAsync($"❌ 入力エラー: {ex.Message}", ephemeral: true);
                logger.LogWarning("Invalid input for /add store command: {Message}", ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                await command.FollowupAsync($"❌ {ex.Message}", ephemeral: true);
                logger.LogWarning("Operation error for /add store command: {Message}", ex.Message);
            }
        }

        private async Task HandleAddMealAsync(SocketSlashCommand command, SocketSlashCommandDataOption subCommand)
        {
            try
            {
                string name = subCommand.Options.First(o => o.Name == "name").Value.ToString()!;
                int foodTypeId = Convert.ToInt32(subCommand.Options.First(o => o.Name == "foodtype").Value);
                string description = subCommand.Options.FirstOrDefault(o => o.Name == "description")?.Value?.ToString() ?? "";

                Meal meal = await dataManagementService.AddMealAsync(name, foodTypeId, description);

                string foodTypeName = foodTypeId switch
                {
                    1 => "コメ",
                    2 => "麺",
                    3 => "パン",
                    _ => "不明"
                };

                EmbedBuilder embed = new EmbedBuilder()
                    .WithTitle("✅ 食べ物を追加しました")
                    .WithColor(Color.Green)
                    .AddField("食べ物名", meal.Name, true)
                    .AddField("種類", foodTypeName, true)
                    .AddField("ID", meal.Id.ToString(), true)
                    .WithTimestamp(DateTimeOffset.Now);

                if (!string.IsNullOrEmpty(meal.Description))
                {
                    embed.AddField("説明", meal.Description, false);
                }

                await command.FollowupAsync(embed: embed.Build());
                logger.LogInformation("User {Username} added meal: {MealName} (FoodType: {FoodTypeId})", 
                    command.User.Username, meal.Name, foodTypeId);
            }
            catch (ArgumentException ex)
            {
                await command.FollowupAsync($"❌ 入力エラー: {ex.Message}", ephemeral: true);
                logger.LogWarning("Invalid input for /add meal command: {Message}", ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                await command.FollowupAsync($"❌ {ex.Message}", ephemeral: true);
                logger.LogWarning("Operation error for /add meal command: {Message}", ex.Message);
            }
        }
    }
}