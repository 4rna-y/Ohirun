using System.ComponentModel.DataAnnotations;

namespace Ohirun.Models
{
    public class StoreMeal
    {
        public int StoreId { get; set; }
        public Store Store { get; set; } = null!;
        
        public int MealId { get; set; }
        public Meal Meal { get; set; } = null!;
        
        public decimal? Price { get; set; }
        
        public bool IsAvailable { get; set; } = true;
    }
}