using System.ComponentModel.DataAnnotations;
using System.Data;
using static NuGet.Packaging.PackagingConstants;

namespace asm_1_Web_DoAn.Models
{
    public class User 
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
        public ICollection<Order>? Orders { get; set; }
    }
}
