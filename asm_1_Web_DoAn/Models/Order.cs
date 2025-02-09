namespace asm_1_Web_DoAn.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public User User { get; set; }
        public ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}
