using System.ComponentModel.DataAnnotations;

namespace asm_1_Web_DoAn.Models
{
    public class Combo
    {
        public int ComboId { get; set; }
        public string ComboName { get; set; }
        public decimal ComboPrice { get; set; }
        public string? ImageUrlcombo { get; set; }
        public string ComboDescription { get; set; }
        public CartItem? CartItems { get; set; }
        public Status Status { get; set; }
        public ICollection<ComboFoodItem>? ComboFoodItems { get; set; }
    }
}
