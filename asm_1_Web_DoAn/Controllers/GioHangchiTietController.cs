using asm_1_Web_DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace asm_1_Web_DoAn.Controllers
{
    public class GioHangChiTietController : Controller
    {
        private readonly MyDbConText _db;

        public GioHangChiTietController(MyDbConText context)
        {
            _db = context;
        }

        public IActionResult Index()
        {
            // Kiểm tra phiên đăng nhập
            var username = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(username))
            {
                return Content("Bạn chưa đăng nhập hoặc phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.");
            }

            // Lấy thông tin người dùng dựa vào Username
            var user = _db.Users.FirstOrDefault(x => x.Username == username);

            if (user == null)
            {
                return Content("Người dùng không tồn tại. Vui lòng kiểm tra lại.");
            }

            // Lấy thông tin giỏ hàng dựa trên UserId
            var cart = _db.Carts.FirstOrDefault(x => x.UserId == user.UserId);

            if (cart == null)
            {
                return Content("Bạn chưa có giỏ hàng. Vui lòng thêm sản phẩm để tạo giỏ hàng.");
            }

            // Lấy toàn bộ thông tin giỏ hàng chi tiết (Include thông tin liên quan)
            var cartItems = _db.CartItems
                .Include(x => x.FoodItems)
                .Include(x => x.Combos)
                .Where(x => x.CartId == cart.CartId)
                .ToList();

            return View(cartItems);
        }

        [HttpPost]
        public IActionResult RemoveCart(int id)
        {
            var cartItem = _db.CartItems.FirstOrDefault(x => x.FoodItemId == id);
            if (cartItem == null)
            {
                return Content("Sản phẩm không tồn tại trong giỏ hàng.");
            }

            _db.CartItems.Remove(cartItem); 
            _db.SaveChanges();
            TempData["Message"] = "Sản phẩm đã được xóa khỏi giỏ hàng.";
            return RedirectToAction("Index", "GioHangChiTiet");
        }
    }
}
