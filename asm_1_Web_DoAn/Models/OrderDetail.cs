using static NuGet.Packaging.PackagingConstants;

namespace asm_1_Web_DoAn.Models
{
    public class OrderDetail
    {
        public int OrderDetailId { get; set; }
        public int OrderId { get; set; }
        public int? FoodItemId { get; set; }
        public int? ComboId { get; set; }
        public int Quantity { get; set; }
        public Order? Order { get; set; }
        public FoodItem? FoodItem { get; set; }
        public Combo Combos { get; set; }
    }
}
