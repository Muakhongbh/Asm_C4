using asm_1_Web_DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace asm_1_Web_DoAn.Controllers
{
    public class BanHangController : Controller
    {
        private readonly MyDbConText _context;

        public BanHangController(MyDbConText context)
        {
            _context = context;
        }
        // GET: Menu/FoodItems
        public IActionResult Index()
        {
            // Kiểm tra phiên đăng nhập
            var username = HttpContext.Session.GetString("Username");
            if (username != null)
            {
                // Lấy thông tin người dùng từ session
                var user = _context.Users.FirstOrDefault(x => x.Username == username);
                if (user != null)
                {
                    // Lấy giỏ hàng của người dùng
                    var cart = _context.Carts.FirstOrDefault(x => x.UserId == user.UserId);
                    if (cart != null)
                    {
                        // Tính tổng số sản phẩm trong giỏ hàng
                        var cartItemCount = _context.CartItems
                            .Where(x => x.CartId == cart.CartId)
                            .Sum(x => x.Quantity); // Tính tổng số lượng sản phẩm trong giỏ

                        // Gán giá trị vào ViewData để hiển thị trên view
                        ViewData["CartItemCount"] = cartItemCount;
                    }
                    else
                    {
                        ViewData["CartItemCount"] = 0; // Nếu không có giỏ hàng
                    }
                }
                else
                {
                    ViewData["CartItemCount"] = 0; // Nếu không tìm thấy người dùng
                }
            }
            else
            {
                ViewData["CartItemCount"] = 0; // Nếu chưa đăng nhập
            }

            return View();
        }

        public async Task<IActionResult> FoodItems()
        {
            var foodItems = await _context.FoodItems.ToListAsync();
            return View(foodItems);
        }


        // GET: Menu/Combos
        public async Task<IActionResult> Combos()
        {
            var combos = await _context.Combos
                       .Include(c => c.ComboFoodItems)
                       .ThenInclude(cf => cf.FoodItem)
                       .ToListAsync();
            return View(combos);
        }

        // GET: Menu/FoodItem/Details/5
        public async Task<IActionResult> FoodItemDetails(int id)
        {
            var foodItem = await _context.FoodItems.FindAsync(id);
            if (foodItem == null)
            {
                return NotFound();
            }
            return View(foodItem);
        }

        // GET: Menu/Combo/Details/5
        // Phương thức ComboDetails
        public async Task<IActionResult> ComboDetails(int id)
        {
            var activeComboDetails = await _context.Combos
                .Where(c => c.Status == Status.Active && c.ComboId == id)
                .Select(c => new
                {
                    ComboId = c.ComboId,
                    ComboName = c.ComboName,
                    ComboPrice = c.ComboPrice,
                    ComboImageUrl = c.ImageUrlcombo,
                    ComboDescription = c.ComboDescription,
                    FoodItems = c.ComboFoodItems
                        .Where(cfi => cfi.Status == Status.Active)
                        .Select(cfi => new
                        {
                            FoodName = cfi.FoodItem.FoodName,
                            FoodPrice = cfi.FoodItem.Price,
                            FoodImageUrl = cfi.FoodItem.ImageUrl,
                            Quantity = cfi.Quantity,
                            TotalFoodPrice = cfi.FoodItem.Price * cfi.Quantity
                        })
                        .ToList(),
                    TotalComboPrice = c.ComboFoodItems
                        .Where(cfi => cfi.Status == Status.Active)
                        .Sum(cfi => cfi.FoodItem.Price * cfi.Quantity)
                })
                .FirstOrDefaultAsync(); // Sử dụng FirstOrDefaultAsync

            if (activeComboDetails == null)
            {
                return NotFound();
            }

            return View(activeComboDetails); // Trả về activeComboDetails
        }



        public async Task<IActionResult> Search(string searchTerm)
        {
            var results = await _context.FoodItems
                .Where(f => f.FoodName.Contains(searchTerm))
                .ToListAsync();
            TempData["SearchTerm"] = searchTerm;

            return View("FoodItems", results); // Trả về view với kết quả tìm kiếm
        }
        public async Task<IActionResult> AdvancedSearch(string name, decimal? minPrice, decimal? maxPrice, FoodCategory? category)
        {

            var query = _context.FoodItems.AsQueryable();


            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(f => f.FoodName.Contains(name));
            }
            if (minPrice.HasValue)
            {
                query = query.Where(f => f.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(f => f.Price <= maxPrice.Value);
            }
            if (category.HasValue)
            {
                query = query.Where(f => f.Category == category.Value);
            }
            ViewBag.SearchName = name;
            ViewBag.SearchMinPrice = minPrice;
            ViewBag.SearchMaxPrice = maxPrice;
            ViewBag.Categories = Enum.GetValues(typeof(FoodCategory)).Cast<FoodCategory>().Select(c => new SelectListItem
            {
                Value = c.ToString(),
                Text = c.ToString(),
                Selected = c == category
            });

            var results = await query.ToListAsync();

            return View("FoodItems", results);
        }

        [HttpPost]
        public IActionResult AddToCart(int id, int soLuong, string type)
        {
            var acc = HttpContext.Session.GetString("Username");
            if (acc == null)
            {
                return Content("Bạn chưa đăng nhập. Vui lòng đăng nhập.");
            }

            var getacc = _context.Users.FirstOrDefault(x => x.Username == acc);
            if (getacc == null)
            {
                return Content("Người dùng không tồn tại.");
            }

            var cart = _context.Carts.FirstOrDefault(x => x.UserId == getacc.UserId);
            if (cart == null)
            {
                return Content("Bạn chưa có giỏ hàng. Vui lòng tạo giỏ hàng trước.");
            }

            if (string.IsNullOrEmpty(type))
            {
                return Content("Loại sản phẩm không được để trống.");
            }

            if (type == "FoodItem")
            {
                var foodItem = _context.FoodItems.FirstOrDefault(x => x.FoodItemId == id);
                if (foodItem == null)
                {
                    return Content("Sản phẩm không tồn tại.");
                }

                var cartItem = _context.CartItems.FirstOrDefault(x => x.CartId == cart.CartId && x.FoodItemId == id);
                if (cartItem == null)
                {
                    _context.CartItems.Add(new CartItem
                    {
                        CartId = cart.CartId,
                        FoodItemId = id,
                        Quantity = soLuong,
                        Price = foodItem.Price * soLuong
                    });
                }
                else
                {
                    cartItem.Quantity += soLuong;
                    cartItem.Price += foodItem.Price * soLuong;
                    _context.CartItems.Update(cartItem);
                }
                _context.SaveChanges();
            }
            else if (type == "Combo")
            {
                var combo = _context.Combos.FirstOrDefault(x => x.ComboId == id);
                if (combo == null)
                {
                    return Content("Combo không tồn tại.");
                }

                var cartItem = _context.CartItems.FirstOrDefault(x => x.CartId == cart.CartId && x.ComboId == id);
                if (cartItem == null)
                {
                    _context.CartItems.Add(new CartItem
                    {
                        CartId = cart.CartId,
                        ComboId = id,
                        Quantity = soLuong,
                        Price = combo.ComboPrice * soLuong
                    });
                }
                else
                {
                    cartItem.Quantity += soLuong;
                    cartItem.Price += combo.ComboPrice * soLuong;
                    _context.CartItems.Update(cartItem);
                }
                _context.SaveChanges();
            }
            else
            {
                return Content("Loại sản phẩm không hợp lệ.");
            }

            return RedirectToAction("FoodItems", "BanHang");
        }


        private decimal GetFoodItemPrice(int foodItemId)
        {
            var foodItem = _context.FoodItems.FirstOrDefault(x => x.FoodItemId == foodItemId);
            return foodItem != null ? foodItem.Price : 0;
        }
    }
}
