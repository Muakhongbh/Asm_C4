namespace asm_1_Web_DoAn.Models
{
    public class Cart
    {
        public int CartId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public DateTime CreatedDate { get; set; }
        public ICollection<CartItem>? CartItems { get; set; }
    }
}
