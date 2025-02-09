 namespace asm_1_Web_DoAn.Models
{
    public class ComboFoodItem
    {
        public int ComboId { get; set; }
        public int FoodItemId { get; set; }
        public int Quantity { get; set; }
        public Status Status { get; set; }
        public Combo? Combo { get; set; }
        public FoodItem? FoodItem { get; set; }
    }
}
