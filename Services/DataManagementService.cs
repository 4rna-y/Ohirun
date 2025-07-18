using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ohirun.Data;
using Ohirun.Models;

namespace Ohirun.Services
{
    public class DataManagementService
    {
        private readonly ApplicationDbContext dbContext;
        private readonly ILogger<DataManagementService> logger;

        public DataManagementService(ApplicationDbContext dbContext, ILogger<DataManagementService> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
        }

        public async Task<Store> AddStoreAsync(string name, string genre)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("店舗名は必須です。", nameof(name));
            }

            if (string.IsNullOrWhiteSpace(genre))
            {
                throw new ArgumentException("ジャンルは必須です。", nameof(genre));
            }

            bool existingStore = await dbContext.Stores.AnyAsync(s => s.Name == name);
            if (existingStore)
            {
                throw new InvalidOperationException($"店舗「{name}」は既に登録されています。");
            }

            Store store = new Store
            {
                Name = name,
                Genre = genre,
                IsActive = true
            };

            dbContext.Stores.Add(store);
            await dbContext.SaveChangesAsync();

            logger.LogInformation("Added new store: {Name} ({Genre})", name, genre);
            return store;
        }

        public async Task<Meal> AddMealAsync(string name, int foodTypeId, string description = "")
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("食べ物の名前は必須です。", nameof(name));
            }

            bool foodTypeExists = await dbContext.FoodTypes.AnyAsync(ft => ft.Id == foodTypeId);
            if (!foodTypeExists)
            {
                throw new ArgumentException($"食べ物の種類ID「{foodTypeId}」が見つかりません。", nameof(foodTypeId));
            }

            bool existingMeal = await dbContext.Meals.AnyAsync(m => m.Name == name && m.FoodTypeId == foodTypeId);
            if (existingMeal)
            {
                throw new InvalidOperationException($"食べ物「{name}」は既に登録されています。");
            }

            Meal meal = new Meal
            {
                Name = name,
                FoodTypeId = foodTypeId,
                Description = description ?? string.Empty
            };

            dbContext.Meals.Add(meal);
            await dbContext.SaveChangesAsync();

            logger.LogInformation("Added new meal: {Name} (FoodTypeId: {FoodTypeId})", name, foodTypeId);
            return meal;
        }

        public async Task<StoreMeal> LinkStoreMealAsync(int storeId, int mealId, decimal? price = null)
        {
            bool storeExists = await dbContext.Stores.AnyAsync(s => s.Id == storeId);
            if (!storeExists)
            {
                throw new ArgumentException($"店舗ID「{storeId}」が見つかりません。", nameof(storeId));
            }

            bool mealExists = await dbContext.Meals.AnyAsync(m => m.Id == mealId);
            if (!mealExists)
            {
                throw new ArgumentException($"食べ物ID「{mealId}」が見つかりません。", nameof(mealId));
            }

            bool existingLink = await dbContext.StoreMeals.AnyAsync(sm => sm.StoreId == storeId && sm.MealId == mealId);
            if (existingLink)
            {
                throw new InvalidOperationException("この店舗と食べ物の組み合わせは既に登録されています。");
            }

            StoreMeal storeMeal = new StoreMeal
            {
                StoreId = storeId,
                MealId = mealId,
                Price = price,
                IsAvailable = true
            };

            dbContext.StoreMeals.Add(storeMeal);
            await dbContext.SaveChangesAsync();

            logger.LogInformation("Linked store {StoreId} with meal {MealId}", storeId, mealId);
            return storeMeal;
        }

        public async Task<Store[]> GetAllStoresAsync()
        {
            return await dbContext.Stores.Where(s => s.IsActive).OrderBy(s => s.Name).ToArrayAsync();
        }

        public async Task<Meal[]> GetAllMealsAsync()
        {
            return await dbContext.Meals
                .Include(m => m.FoodType)
                .OrderBy(m => m.FoodType.Name)
                .ThenBy(m => m.Name)
                .ToArrayAsync();
        }

        public async Task<FoodType[]> GetAllFoodTypesAsync()
        {
            return await dbContext.FoodTypes.OrderBy(ft => ft.Name).ToArrayAsync();
        }
    }
}