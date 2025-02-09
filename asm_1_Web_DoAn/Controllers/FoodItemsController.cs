using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using asm_1_Web_DoAn.Models;

namespace asm_1_Web_DoAn.Controllers
{
   
    public class FoodItemsController : Controller
    {
        private readonly MyDbConText _context;

        public FoodItemsController(MyDbConText context)
        {
            _context = context;
        }

        // GET: FoodItems
        [HttpGet]
        public async Task<IActionResult> Index(string foodName, decimal? minPrice, decimal? maxPrice, int? minQuantity, int? maxQuantity, string description, int page = 1, int pageSize = 5)
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                // Nếu chưa đăng nhập, chuyển hướng về trang bán hàng
                return RedirectToAction("FoodItems", "BanHang");
            }
            var foodItemsQuery = _context.FoodItems
                .Where(t => t.Status == Status.Active)
                .AsQueryable();

            // Tìm kiếm nâng cao
            if (!string.IsNullOrEmpty(foodName))
            {
                foodItemsQuery = foodItemsQuery.Where(f => f.FoodName.Contains(foodName));
            }

            if (minPrice.HasValue)
            {
                foodItemsQuery = foodItemsQuery.Where(f => f.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                foodItemsQuery = foodItemsQuery.Where(f => f.Price <= maxPrice.Value);
            }

            if (minQuantity.HasValue)
            {
                foodItemsQuery = foodItemsQuery.Where(f => f.FoodQuantity >= minQuantity.Value);
            }

            if (maxQuantity.HasValue)
            {
                foodItemsQuery = foodItemsQuery.Where(f => f.FoodQuantity <= maxQuantity.Value);
            }

            if (!string.IsNullOrEmpty(description))
            {
                foodItemsQuery = foodItemsQuery.Where(f => f.Description.Contains(description));
            }

            // Tổng số phần tử và tính toán phân trang
            var totalItems = await foodItemsQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var foodItems = await foodItemsQuery
                .OrderBy(t => t.FoodItemId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Lưu trữ các giá trị tìm kiếm và phân trang vào ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.FoodName = foodName;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.MinQuantity = minQuantity;
            ViewBag.MaxQuantity = maxQuantity;
            ViewBag.Description = description;

            return View(foodItems);
        }

        public async Task<IActionResult> InactiveItems(string foodName, decimal? minPrice, decimal? maxPrice, int? minQuantity, int? maxQuantity, string description, int page = 1, int pageSize = 5)
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("FoodItems", "BanHang");
            }
            var foodItemsQuery = _context.FoodItems
                .Where(item => item.Status == Status.Inactive)
                .AsQueryable();

            // Tìm kiếm nâng cao
            if (!string.IsNullOrEmpty(foodName))
            {
                foodItemsQuery = foodItemsQuery.Where(f => f.FoodName.Contains(foodName));
            }

            if (minPrice.HasValue)
            {
                foodItemsQuery = foodItemsQuery.Where(f => f.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                foodItemsQuery = foodItemsQuery.Where(f => f.Price <= maxPrice.Value);
            }

            if (minQuantity.HasValue)
            {
                foodItemsQuery = foodItemsQuery.Where(f => f.FoodQuantity >= minQuantity.Value);
            }

            if (maxQuantity.HasValue)
            {
                foodItemsQuery = foodItemsQuery.Where(f => f.FoodQuantity <= maxQuantity.Value);
            }

            if (!string.IsNullOrEmpty(description))
            {
                foodItemsQuery = foodItemsQuery.Where(f => f.Description.Contains(description));
            }

            // Tổng số phần tử và phân trang
            var totalItems = await foodItemsQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var foodItems = await foodItemsQuery
                .OrderBy(f => f.FoodItemId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Lưu trữ thông tin vào ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.FoodName = foodName;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.MinQuantity = minQuantity;
            ViewBag.MaxQuantity = maxQuantity;
            ViewBag.Description = description;

            return View(foodItems);
        }

        // GET: FoodItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                // Nếu chưa đăng nhập, chuyển hướng về trang bán hàng
                return RedirectToAction("FoodItems", "BanHang");
            }
            if (id == null)
            {
                return NotFound();
            }

            var foodItem = await _context.FoodItems
                .FirstOrDefaultAsync(m => m.FoodItemId == id);
            if (foodItem == null)
            {
                return NotFound();
            }

            return View(foodItem);
        }

        // GET: FoodItems/Create
        public IActionResult Create()
        {
            ViewBag.Categories = Enum.GetValues(typeof(FoodCategory)).Cast<FoodCategory>()
                                     .Select(c => new SelectListItem { Value = c.ToString(), Text = c.ToString() });
            ViewBag.Status = Enum.GetValues(typeof(Status)).Cast<Status>()
                                 .Select(b => new SelectListItem { Value = b.ToString(), Text = b.ToString() });
            return View();
        }


        // POST: FoodItems/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FoodItemId,FoodName,Price,FoodQuantity,Description")] FoodItem foodItem, IFormFile uploathinh)
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                // Nếu chưa đăng nhập, chuyển hướng về trang bán hàng
                return RedirectToAction("FoodItems", "BanHang");
            }

            // Khởi tạo lại ViewBag để render dropdown trong trường hợp có lỗi ModelState
            ViewBag.Categories = Enum.GetValues(typeof(FoodCategory)).Cast<FoodCategory>()
                                     .Select(c => new SelectListItem { Value = c.ToString(), Text = c.ToString() });
            ViewBag.Status = Enum.GetValues(typeof(Status)).Cast<Status>()
                                 .Select(b => new SelectListItem { Value = b.ToString(), Text = b.ToString() });

            if (ModelState.IsValid)
            {
                // Xử lý upload hình ảnh
                if (uploathinh != null && uploathinh.Length > 0)
                {
                    var filePath = Path.Combine("wwwroot/images", uploathinh.FileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath)); // Đảm bảo thư mục tồn tại

                    using var stream = new FileStream(filePath, FileMode.Create);
                    await uploathinh.CopyToAsync(stream);
                    foodItem.ImageUrl = "/images/" + uploathinh.FileName;
                }

                // Gán giá trị Category và Status từ form
                if (Enum.TryParse(Request.Form["Category"], out FoodCategory selectedCategory))
                {
                    foodItem.Category = selectedCategory;
                }
                if (Enum.TryParse(Request.Form["Status"], out Status selectedStatus))
                {
                    foodItem.Status = selectedStatus;
                }

                // Thêm sản phẩm vào cơ sở dữ liệu
                _context.Add(foodItem);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "FoodItems");
            }

            return View(foodItem); // Trả về view nếu có lỗi
        }




        // GET: FoodItems/Edit/5
        // GET: FoodItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var foodItem = await _context.FoodItems.FindAsync(id);
            if (foodItem == null)
            {
                return NotFound();
            }

            // Truyền danh sách Categories vào ViewBag
            ViewBag.Categories = Enum.GetValues(typeof(FoodCategory))
                .Cast<FoodCategory>()
                .Select(c => new SelectListItem
                {
                    Value = c.ToString(),
                    Text = c.ToString(),
                    Selected = c == foodItem.Category // Đặt Selected nếu cần
                });

            // Truyền danh sách Status vào ViewBag
            ViewBag.Status = Enum.GetValues(typeof(Status))
                .Cast<Status>()
                .Select(s => new SelectListItem
                {
                    Value = s.ToString(),
                    Text = s.ToString(),
                    Selected = s == foodItem.Status // Đặt Selected nếu cần
                });

            return View(foodItem);
        }
        // POST: FoodItems/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("FoodItemId,FoodName,Price,FoodQuantity,Description,Status,Category")] FoodItem foodItem, IFormFile imageFile)
        {
            if (id != foodItem.FoodItemId)
            {
                return NotFound();
            }

            // Truyền lại danh sách Categories và Status vào ViewBag
            ViewBag.Categories = Enum.GetValues(typeof(FoodCategory))
                .Cast<FoodCategory>()
                .Select(c => new SelectListItem
                {
                    Value = c.ToString(),
                    Text = c.ToString(),
                    Selected = c == foodItem.Category
                });

            ViewBag.Status = Enum.GetValues(typeof(Status))
                .Cast<Status>()
                .Select(s => new SelectListItem
                {
                    Value = s.ToString(),
                    Text = s.ToString(),
                    Selected = s == foodItem.Status
                });

            if (ModelState.IsValid)
            {
                try
                {
                    // Xử lý logic upload ảnh
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", imageFile.FileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }
                        foodItem.ImageUrl = $"/images/{imageFile.FileName}";
                    }

                    _context.Update(foodItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FoodItemExists(foodItem.FoodItemId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            return View(foodItem);
        }


        // GET: FoodItems/Delete/5
        // GET: FoodItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var foodItem = await _context.FoodItems
                .FirstOrDefaultAsync(m => m.FoodItemId == id);
            if (foodItem == null)
            {
                return NotFound();
            }

            return View(foodItem);
        }

        // POST: FoodItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                // Nếu chưa đăng nhập, chuyển hướng về trang bán hàng
                return RedirectToAction("FoodItems", "BanHang");
            }
            var foodItem = await _context.FoodItems.FindAsync(id);
            if (foodItem != null)
            {

                foodItem.Status = Status.Inactive;
                _context.Update(foodItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Restore(int id)
        {
            var foodItem = await _context.FoodItems.FindAsync(id);
            if (foodItem != null)
            {
                foodItem.Status = Status.Active;
                _context.Update(foodItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("InactiveItems", "FoodItems");
        }

        private bool FoodItemExists(int id)
        {
            return _context.FoodItems.Any(e => e.FoodItemId == id);
        }
    }
}
