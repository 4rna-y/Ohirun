using Microsoft.EntityFrameworkCore;
using Ohirun.Models;

namespace Ohirun.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Store> Stores { get; set; }
        public DbSet<FoodType> FoodTypes { get; set; }
        public DbSet<Meal> Meals { get; set; }
        public DbSet<StoreMeal> StoreMeals { get; set; }
        public DbSet<LunchHistory> LunchHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // StoreMeal の複合キーを設定
            modelBuilder.Entity<StoreMeal>()
                .HasKey(sm => new { sm.StoreId, sm.MealId });

            // Store と StoreMeal の関係
            modelBuilder.Entity<StoreMeal>()
                .HasOne(sm => sm.Store)
                .WithMany(s => s.StoreMeals)
                .HasForeignKey(sm => sm.StoreId);

            // Meal と StoreMeal の関係
            modelBuilder.Entity<StoreMeal>()
                .HasOne(sm => sm.Meal)
                .WithMany(m => m.StoreMeals)
                .HasForeignKey(sm => sm.MealId);

            // FoodType と Meal の関係
            modelBuilder.Entity<Meal>()
                .HasOne(m => m.FoodType)
                .WithMany(ft => ft.Meals)
                .HasForeignKey(m => m.FoodTypeId);

            // LunchHistory と Store の関係
            modelBuilder.Entity<LunchHistory>()
                .HasOne(lh => lh.Store)
                .WithMany()
                .HasForeignKey(lh => lh.StoreId);

            // LunchHistory と Meal の関係
            modelBuilder.Entity<LunchHistory>()
                .HasOne(lh => lh.Meal)
                .WithMany()
                .HasForeignKey(lh => lh.MealId);

            // LunchHistory のインデックス設定
            modelBuilder.Entity<LunchHistory>()
                .HasIndex(lh => new { lh.UserId, lh.SuggestedAt })
                .HasDatabaseName("IX_LunchHistory_UserId_SuggestedAt");

            // 初期データの追加
            modelBuilder.Entity<FoodType>().HasData(
                new FoodType { Id = 1, Name = "コメ", Description = "ご飯系の食べ物" },
                new FoodType { Id = 2, Name = "麺", Description = "麺類の食べ物" },
                new FoodType { Id = 3, Name = "パン", Description = "パン系の食べ物" }
            );

            base.OnModelCreating(modelBuilder);
        }
    }
}