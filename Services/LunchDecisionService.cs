using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ohirun.Data;
using Ohirun.Models;

namespace Ohirun.Services
{
    public class LunchDecisionService
    {
        private readonly ApplicationDbContext dbContext;
        private readonly ILogger<LunchDecisionService> logger;
        private readonly Random random;

        public LunchDecisionService(ApplicationDbContext dbContext, ILogger<LunchDecisionService> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
            this.random = new Random();
        }

        public async Task<LunchDecision> DecideRandomLunchAsync(string userId, string username)
        {
            DateTime yesterday = DateTime.Today.AddDays(-1);
            
            // 昨日提案されたStoreMealの組み合わせを取得
            int[] recentSuggestionIds = await dbContext.LunchHistories
                .Where(lh => lh.UserId == userId && lh.SuggestedAt >= yesterday)
                .Select(lh => new { lh.StoreId, lh.MealId })
                .Select(x => x.StoreId * 10000 + x.MealId) // 簡易的な組み合わせID
                .ToArrayAsync();

            StoreMeal[] availableOptions = await dbContext.StoreMeals
                .Where(sm => sm.IsAvailable)
                .Include(sm => sm.Store)
                .Include(sm => sm.Meal)
                .ThenInclude(m => m.FoodType)
                .ToArrayAsync();

            if (availableOptions.Length == 0)
            {
                throw new InvalidOperationException("利用可能な店舗と食べ物の組み合わせがありません。");
            }

            // 最近の提案を除外
            StoreMeal[] filteredOptions = availableOptions
                .Where(sm => !recentSuggestionIds.Contains(sm.StoreId * 10000 + sm.MealId))
                .ToArray();

            // フィルタ後のオプションがない場合は全てのオプションから選択
            StoreMeal[] finalOptions = filteredOptions.Length > 0 ? filteredOptions : availableOptions;

            StoreMeal selectedOption = finalOptions[random.Next(finalOptions.Length)];
            
            // 履歴に保存
            LunchHistory history = new LunchHistory
            {
                StoreId = selectedOption.StoreId,
                MealId = selectedOption.MealId,
                UserId = userId,
                Username = username,
                SuggestedAt = DateTime.Now
            };
            
            dbContext.LunchHistories.Add(history);
            await dbContext.SaveChangesAsync();
            
            logger.LogInformation("Decided lunch: {StoreName} - {MealName} ({FoodType}) for user {Username}", 
                selectedOption.Store.Name, 
                selectedOption.Meal.Name, 
                selectedOption.Meal.FoodType.Name,
                username);

            return new LunchDecision 
            { 
                Store = selectedOption.Store, 
                Meal = selectedOption.Meal 
            };
        }

        public async Task<LunchDecision> DecideRandomLunchByFoodTypeAsync(int foodTypeId)
        {
            StoreMeal[] availableOptions = await dbContext.StoreMeals
                .Where(sm => sm.IsAvailable && sm.Meal.FoodTypeId == foodTypeId)
                .Include(sm => sm.Store)
                .Include(sm => sm.Meal)
                .ThenInclude(m => m.FoodType)
                .ToArrayAsync();

            if (availableOptions.Length == 0)
            {
                throw new InvalidOperationException("指定された食べ物の種類で利用可能な店舗と食べ物の組み合わせがありません。");
            }

            StoreMeal selectedOption = availableOptions[random.Next(availableOptions.Length)];
            
            logger.LogInformation("Decided lunch by food type: {StoreName} - {MealName} ({FoodType})", 
                selectedOption.Store.Name, 
                selectedOption.Meal.Name, 
                selectedOption.Meal.FoodType.Name);

            return new LunchDecision 
            { 
                Store = selectedOption.Store, 
                Meal = selectedOption.Meal 
            };
        }

        public async Task<LunchDecision> DecideRandomLunchByStoreAsync(int storeId)
        {
            StoreMeal[] availableOptions = await dbContext.StoreMeals
                .Where(sm => sm.IsAvailable && sm.StoreId == storeId)
                .Include(sm => sm.Store)
                .Include(sm => sm.Meal)
                .ThenInclude(m => m.FoodType)
                .ToArrayAsync();

            if (availableOptions.Length == 0)
            {
                throw new InvalidOperationException("指定された店舗で利用可能な食べ物がありません。");
            }

            StoreMeal selectedOption = availableOptions[random.Next(availableOptions.Length)];
            
            logger.LogInformation("Decided lunch by store: {StoreName} - {MealName} ({FoodType})", 
                selectedOption.Store.Name, 
                selectedOption.Meal.Name, 
                selectedOption.Meal.FoodType.Name);

            return new LunchDecision 
            { 
                Store = selectedOption.Store, 
                Meal = selectedOption.Meal 
            };
        }

        public async Task<FoodType[]> GetAllFoodTypesAsync()
        {
            return await dbContext.FoodTypes.OrderBy(ft => ft.Name).ToArrayAsync();
        }

        public async Task<Store[]> GetAllStoresAsync()
        {
            return await dbContext.Stores.Where(s => s.IsActive).OrderBy(s => s.Name).ToArrayAsync();
        }
    }
}