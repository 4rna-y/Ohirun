using System;
using System.ComponentModel.DataAnnotations;

namespace Ohirun.Models
{
    public class LunchHistory
    {
        public int Id { get; set; }
        
        public int StoreId { get; set; }
        public Store Store { get; set; } = null!;
        
        public int MealId { get; set; }
        public Meal Meal { get; set; } = null!;
        
        [Required]
        [MaxLength(100)]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;
        
        public DateTime SuggestedAt { get; set; }
    }
}