using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ohirun.Models
{
    public class Meal
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(200)]
        public string Description { get; set; } = string.Empty;
        
        public int FoodTypeId { get; set; }
        public FoodType FoodType { get; set; } = null!;
        
        public ICollection<StoreMeal> StoreMeals { get; set; } = new List<StoreMeal>();
    }
}