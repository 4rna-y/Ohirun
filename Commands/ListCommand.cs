using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ohirun.Data;
using Ohirun.Models;
using Ohirun.Services;

namespace Ohirun.Commands
{
    public class ListCommand : ISlashCommand
    {
        private readonly ILogger<ListCommand> logger;
        private readonly DataManagementService dataManagementService;
        private readonly ApplicationDbContext dbContext;

        public string Name => "list";

        public ListCommand(ILogger<ListCommand> logger, DataManagementService dataManagementService, ApplicationDbContext dbContext)
        {
            this.logger = logger;
            this.dataManagementService = dataManagementService;
            this.dbContext = dbContext;
        }

        public SlashCommandBuilder GetCommandBuilder()
        {
            return new SlashCommandBuilder()
                .WithName(Name)
                .WithDescription("登録されているデータを表示します")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("stores")
                    .WithDescription("登録されている店舗一覧を表示します")
                    .WithType(ApplicationCommandOptionType.SubCommand))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("meals")
                    .WithDescription("登録されている食べ物一覧を表示します")
                    .WithType(ApplicationCommandOptionType.SubCommand))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("links")
                    .WithDescription("店舗と食べ物の関連付け一覧を表示します")
                    .WithType(ApplicationCommandOptionType.SubCommand));
        }

        public async Task HandleAsync(SocketSlashCommand command)
        {
            try
            {
                await command.DeferAsync();

                SocketSlashCommandDataOption subCommand = command.Data.Options.First();
                
                switch (subCommand.Name)
                {
                    case "stores":
                        await HandleListStoresAsync(command);
                        break;
                    case "meals":
                        await HandleListMealsAsync(command);
                        break;
                    case "links":
                        await HandleListLinksAsync(command);
                        break;
                    default:
                        await command.FollowupAsync("❌ 不明なサブコマンドです", ephemeral: true);
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to handle /list command");
                await command.FollowupAsync("❌ コマンドの処理中にエラーが発生しました", ephemeral: true);
            }
        }

        private async Task HandleListStoresAsync(SocketSlashCommand command)
        {
            Store[] stores = await dataManagementService.GetAllStoresAsync();

            if (stores.Length == 0)
            {
                await command.FollowupAsync("📭 登録されている店舗がありません", ephemeral: true);
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithTitle("🏪 登録されている店舗一覧")
                .WithColor(Color.Blue)
                .WithTimestamp(DateTimeOffset.Now);

            StringBuilder description = new StringBuilder();
            foreach (Store store in stores)
            {
                description.AppendLine($"**ID: {store.Id}** - {store.Name} ({store.Genre})");
            }

            embed.WithDescription(description.ToString());
            embed.WithFooter($"合計: {stores.Length} 店舗");

            await command.FollowupAsync(embed: embed.Build());
            logger.LogInformation("User {Username} listed {Count} stores", command.User.Username, stores.Length);
        }

        private async Task HandleListMealsAsync(SocketSlashCommand command)
        {
            Meal[] meals = await dataManagementService.GetAllMealsAsync();

            if (meals.Length == 0)
            {
                await command.FollowupAsync("📭 登録されている食べ物がありません", ephemeral: true);
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithTitle("🍱 登録されている食べ物一覧")
                .WithColor(Color.Green)
                .WithTimestamp(DateTimeOffset.Now);

            StringBuilder description = new StringBuilder();
            foreach (Meal meal in meals)
            {
                string mealInfo = $"**ID: {meal.Id}** - {meal.Name} ({meal.FoodType.Name})";
                if (!string.IsNullOrEmpty(meal.Description))
                {
                    mealInfo += $" - {meal.Description}";
                }
                description.AppendLine(mealInfo);
            }

            embed.WithDescription(description.ToString());
            embed.WithFooter($"合計: {meals.Length} 食べ物");

            await command.FollowupAsync(embed: embed.Build());
            logger.LogInformation("User {Username} listed {Count} meals", command.User.Username, meals.Length);
        }

        private async Task HandleListLinksAsync(SocketSlashCommand command)
        {
            StoreMeal[] storeMeals = await dbContext.StoreMeals
                .Include(sm => sm.Store)
                .Include(sm => sm.Meal)
                .ThenInclude(m => m.FoodType)
                .Where(sm => sm.IsAvailable)
                .OrderBy(sm => sm.Store.Name)
                .ThenBy(sm => sm.Meal.Name)
                .ToArrayAsync();

            if (storeMeals.Length == 0)
            {
                await command.FollowupAsync("📭 登録されている関連付けがありません", ephemeral: true);
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithTitle("🔗 店舗と食べ物の関連付け一覧")
                .WithColor(Color.Orange)
                .WithTimestamp(DateTimeOffset.Now);

            StringBuilder description = new StringBuilder();
            foreach (StoreMeal storeMeal in storeMeals)
            {
                string linkInfo = $"**{storeMeal.Store.Name}** × **{storeMeal.Meal.Name}** ({storeMeal.Meal.FoodType.Name})";
                if (storeMeal.Price.HasValue)
                {
                    linkInfo += $" - ¥{storeMeal.Price.Value:N0}";
                }
                description.AppendLine(linkInfo);
            }

            embed.WithDescription(description.ToString());
            embed.WithFooter($"合計: {storeMeals.Length} 関連付け");

            await command.FollowupAsync(embed: embed.Build());
            logger.LogInformation("User {Username} listed {Count} store-meal links", command.User.Username, storeMeals.Length);
        }
    }
}