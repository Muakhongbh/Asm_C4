using asm_1_Web_DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace asm_1_Web_DoAn.Controllers
{
    public class AccountController : Controller
    {
        private readonly MyDbConText _context;

        public AccountController(MyDbConText context)
        {
            _context = context;
        }

        // GET: Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User users)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == users.Username);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Username", "Username already exists.");
                    return View(users);
                }
                _context.Users.Add(users);
                _context.SaveChanges();
                Cart giohang = new Cart();

                giohang.UserId = users.UserId;
                giohang.Username = users.Username;
              

                // Thêm user mới
                _context.Carts.Add(giohang);
                _context.SaveChangesAsync();
                TempData["Status"] = " Chúc mừng bạn đã tạo tài khoản thành công";
                return RedirectToAction("Login");
            }
            return View(users);
        }

        // GET: Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.Password == password);

            if (user != null)
            {
                HttpContext.Session.SetString("UserId", user.UserId.ToString());
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("Role", user.Role);
                if (user.Role == "Admin")
                {
                    return RedirectToAction("Index", "FoodItems" );
                }
                else if (user.Role == "Khach hang")
                {
                    return RedirectToAction("FoodItems", "BanHang");
                }
            }
            ModelState.AddModelError("", "Invalid login attempt.");
            return View();
        }

        // GET: Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("Username");
            return RedirectToAction("FoodItems", "BanHang");
        }
    }
}
