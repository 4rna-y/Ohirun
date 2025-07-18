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
    public class LinkCommand : ISlashCommand
    {
        private readonly ILogger<LinkCommand> logger;
        private readonly DataManagementService dataManagementService;

        public string Name => "link";

        public LinkCommand(ILogger<LinkCommand> logger, DataManagementService dataManagementService)
        {
            this.logger = logger;
            this.dataManagementService = dataManagementService;
        }

        public SlashCommandBuilder GetCommandBuilder()
        {
            return new SlashCommandBuilder()
                .WithName(Name)
                .WithDescription("店舗と食べ物を関連付けます")
                .AddOption("storeid", ApplicationCommandOptionType.Integer, "店舗ID", isRequired: true, minValue: 1)
                .AddOption("mealid", ApplicationCommandOptionType.Integer, "食べ物ID", isRequired: true, minValue: 1)
                .AddOption("price", ApplicationCommandOptionType.Number, "価格 (任意)", isRequired: false, minValue: 0);
        }

        public async Task HandleAsync(SocketSlashCommand command)
        {
            try
            {
                await command.DeferAsync();

                int storeId = Convert.ToInt32(command.Data.Options.First(o => o.Name == "storeid").Value);
                int mealId = Convert.ToInt32(command.Data.Options.First(o => o.Name == "mealid").Value);
                
                decimal? price = null;
                SocketSlashCommandDataOption? priceOption = command.Data.Options.FirstOrDefault(o => o.Name == "price");
                if (priceOption != null)
                {
                    price = Convert.ToDecimal(priceOption.Value);
                }

                StoreMeal storeMeal = await dataManagementService.LinkStoreMealAsync(storeId, mealId, price);

                Store[] stores = await dataManagementService.GetAllStoresAsync();
                Meal[] meals = await dataManagementService.GetAllMealsAsync();
                
                Store store = stores.First(s => s.Id == storeId);
                Meal meal = meals.First(m => m.Id == mealId);

                EmbedBuilder embed = new EmbedBuilder()
                    .WithTitle("✅ 店舗と食べ物を関連付けました")
                    .WithColor(Color.Green)
                    .AddField("店舗", $"{store.Name} (ID: {store.Id})", true)
                    .AddField("食べ物", $"{meal.Name} (ID: {meal.Id})", true)
                    .WithTimestamp(DateTimeOffset.Now);

                if (price.HasValue)
                {
                    embed.AddField("価格", $"¥{price.Value:N0}", true);
                }

                await command.FollowupAsync(embed: embed.Build());
                logger.LogInformation("User {Username} linked store {StoreId} with meal {MealId}", 
                    command.User.Username, storeId, mealId);
            }
            catch (ArgumentException ex)
            {
                await command.FollowupAsync($"❌ 入力エラー: {ex.Message}", ephemeral: true);
                logger.LogWarning("Invalid input for /link command: {Message}", ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                await command.FollowupAsync($"❌ {ex.Message}", ephemeral: true);
                logger.LogWarning("Operation error for /link command: {Message}", ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to handle /link command");
                await command.FollowupAsync("❌ コマンドの処理中にエラーが発生しました", ephemeral: true);
            }
        }
    }
}