namespace asm_1_Web_DoAn.Models
{
    public class CartItem
    {
        public int CartItemId { get; set; }
        public int CartId { get; set; }
        public int? FoodItemId { get; set; }
        public int? ComboId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public Combo Combos { get; set; }
        public FoodItem FoodItems { get; set; }
        public Cart Cart { get; set; }
    }
}
