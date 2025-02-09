using System.ComponentModel.DataAnnotations;

namespace asm_1_Web_DoAn.Models
{
    public class FoodItem
    {
        public int FoodItemId { get; set; }
        public string FoodName { get; set; }
        public decimal Price { get; set; }
        public int FoodQuantity { get; set; }
        public string Description { get; set; }
        public string? ImageUrl { get; set; }
        public Status Status { get; set; }
        public FoodCategory? Category { get; set; }
        public ICollection<ComboFoodItem>? ComboFoodItems { get; set; }
    }
}
