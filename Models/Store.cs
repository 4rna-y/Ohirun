using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ohirun.Models
{
    public class Store
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string Genre { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
        
        public ICollection<StoreMeal> StoreMeals { get; set; } = new List<StoreMeal>();
    }
}