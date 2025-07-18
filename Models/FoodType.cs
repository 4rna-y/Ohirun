using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ohirun.Models
{
    public class FoodType
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(200)]
        public string Description { get; set; } = string.Empty;
        
        public ICollection<Meal> Meals { get; set; } = new List<Meal>();
    }
}