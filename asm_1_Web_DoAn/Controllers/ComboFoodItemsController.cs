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
 
    public class ComboFoodItemsController : Controller
    {
        private readonly MyDbConText _context;

        public ComboFoodItemsController(MyDbConText context)
        {
            _context = context;
        }

        // GET: ComboFoodItems
        public async Task<IActionResult> Index(string comboName, string foodName, int? minQuantity, int? maxQuantity)
        {
            var query = _context.ComboFoodItems
                .Include(cfi => cfi.Combo)
                .Include(cfi => cfi.FoodItem)
                .Select(cfi => new
                {
                    cfi.Combo.ComboId,
                    cfi.Combo.ComboName,
                    cfi.Combo.ComboPrice,
                    cfi.Combo.ImageUrlcombo,
                    cfi.FoodItem.FoodName,
                    cfi.FoodItem.Price,
                    cfi.FoodItem.FoodQuantity,
                    cfi.FoodItem.ImageUrl,
                    cfi.Quantity,
                    cfi.Combo.Status
                });

            // Lọc theo tên combo
            if (!string.IsNullOrEmpty(comboName))
            {
                query = query.Where(item => item.ComboName.Contains(comboName));
            }

            // Lọc theo tên món ăn
            if (!string.IsNullOrEmpty(foodName))
            {
                query = query.Where(item => item.FoodName.Contains(foodName));
            }

            // Lọc theo số lượng
            if (minQuantity.HasValue)
            {
                query = query.Where(item => item.Quantity >= minQuantity.Value);
            }
            if (maxQuantity.HasValue)
            {
                query = query.Where(item => item.Quantity <= maxQuantity.Value);
            }


            ViewBag.ComboName = comboName;
            ViewBag.FoodName = foodName;
            ViewBag.MinQuanTiti = minQuantity;
            ViewBag.MaxQuanTiti = maxQuantity;

            var result = await query.ToListAsync(); // Chuyển đổi sang List

            return View(result); // Trả về danh sách
        }





        // GET: ComboFoodItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comboFoodItem = await _context.ComboFoodItems
                .Include(c => c.Combo)
                .Include(c => c.FoodItem)
                .FirstOrDefaultAsync(m => m.ComboId == id);
            if (comboFoodItem == null)
            {
                return NotFound();
            }

            return View(comboFoodItem);
        }

        // GET: ComboFoodItems/Create
        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Combos"] = _context.Combos.ToList();
            ViewData["FoodItems"] = _context.FoodItems.ToList();
            return View();
        }

        // POST: ComboFoodItems/Create
        // POST: ComboFoodItems/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int ComboId, List<int> selectedFoodItems, List<int> quantities)
        {
            // Lấy Combo đã chọn từ cơ sở dữ liệu
            var selectedCombo = await _context.Combos
                .Include(c => c.ComboFoodItems) // Bao gồm các ComboFoodItems liên quan
                .FirstOrDefaultAsync(c => c.ComboId == ComboId);

            // Kiểm tra xem Combo có tồn tại không
            if (selectedCombo == null)
            {
                return NotFound();
            }

            // Kiểm tra nếu selectedFoodItems và quantities không phải null
            if (selectedFoodItems == null || quantities == null)
            {
                ModelState.AddModelError("", "Danh sách thực phẩm hoặc số lượng không được để trống.");
                return View(selectedCombo);
            }

            // Kiểm tra số lượng phần tử
            if (selectedFoodItems.Count != quantities.Count)
            {
                ModelState.AddModelError("", "Số lượng thực phẩm và số lượng không khớp.");
                return View(selectedCombo);
            }

            decimal totalComboPrice = 0; // Bắt đầu với giá 0

            // Tạo danh sách để lưu trữ các ComboFoodItems mới
            var newComboFoodItems = new List<ComboFoodItem>();

            for (int i = 0; i < selectedFoodItems.Count; i++)
            {
                // Tìm FoodItem theo ID
                var foodItem = await _context.FoodItems.FindAsync(selectedFoodItems[i]);
                int quantity = quantities[i];

                if (foodItem != null)
                {
                    // Tính tổng giá
                    totalComboPrice += foodItem.Price * quantity;

                    // Tạo mới ComboFoodItem và thêm vào danh sách
                    newComboFoodItems.Add(new ComboFoodItem
                    {
                        FoodItemId = foodItem.FoodItemId,
                        Quantity = quantity,
                        ComboId = selectedCombo.ComboId // Đảm bảo ComboId được thiết lập
                    });
                }
            }

            // Cập nhật giá Combo sau khi tính lại
            selectedCombo.ComboPrice = totalComboPrice;
            _context.Update(selectedCombo); // Cập nhật Combo

            // Thêm các ComboFoodItems mới vào ngữ cảnh
            _context.ComboFoodItems.AddRangeAsync(newComboFoodItems); // Sử dụng AddRangeAsync
            _context.SaveChangesAsync(); // Lưu thay đổi

            // Cung cấp dữ liệu cho View
            ViewData["Combos"] = _context.Combos.ToListAsync(); // Sử dụng ToListAsync để đảm bảo không chặn luồng
            ViewData["FoodItems"] = _context.FoodItems.ToListAsync();

            return View(selectedCombo); // Trả về Combo đã cập nhật giá
        }





        // GET: ComboFoodItems/Edit/5
        // Controller
        public async Task<IActionResult> Edit(int id)
        {
            // Lấy danh sách ComboFoodItems liên quan đến Combo
            var comboFoodItems = await _context.ComboFoodItems
                .Include(cfi => cfi.Combo)
                .Include(cfi => cfi.FoodItem)
                .Where(cfi => cfi.ComboId == id)
                .ToListAsync();

            if (comboFoodItems == null || !comboFoodItems.Any())
            {
                return NotFound();
            }

            // Lấy Combo và danh sách FoodItems
            var combo = await _context.Combos.FindAsync(id);
            var foodItems = await _context.FoodItems.ToListAsync();

            // Cung cấp dữ liệu cho View
            ViewData["Combo"] = combo;
            ViewData["FoodItems"] = foodItems;

            return View(comboFoodItems);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, List<int> selectedFoodItems, List<int> quantities)
        {
            if (ModelState.IsValid)
            {
                // Lấy ComboFoodItems hiện tại
                var existingComboFoodItems = await _context.ComboFoodItems
                    .Where(cfi => cfi.ComboId == id)
                    .ToListAsync();

                // Xóa các ComboFoodItem cũ
                _context.ComboFoodItems.RemoveRange(existingComboFoodItems);

                decimal totalComboPrice = 0; // Khởi tạo tổng giá

                // Thêm các ComboFoodItem mới
                for (int i = 0; i < selectedFoodItems.Count; i++)
                {
                    var foodItem = await _context.FoodItems.FindAsync(selectedFoodItems[i]);

                    if (foodItem != null)
                    {
                        var comboFoodItem = new ComboFoodItem
                        {
                            ComboId = id,
                            FoodItemId = foodItem.FoodItemId,
                            Quantity = quantities[i],
                            Status = Status.Active
                        };

                        totalComboPrice += foodItem.Price * quantities[i]; // Cập nhật tổng giá
                        _context.Add(comboFoodItem);
                    }
                }

                // Cập nhật giá Combo
                var combo = await _context.Combos.FindAsync(id);
                combo.ComboPrice = totalComboPrice;

                _context.Update(combo);

                // Lưu thay đổi vào cơ sở dữ liệu
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "ComboFoodItems");
            }

            // Nếu ModelState không hợp lệ, trả về view với dữ liệu đã lấy trước đó
            var comboFoodItems = await _context.ComboFoodItems
                .Include(cfi => cfi.Combo)
                .Include(cfi => cfi.FoodItem)
                .Where(cfi => cfi.ComboId == id)
                .ToListAsync();

            ViewData["Combo"] = await _context.Combos.FindAsync(id);
            ViewData["FoodItems"] = await _context.FoodItems.ToListAsync();

            return View(comboFoodItems);
        }



        // GET: ComboFoodItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comboFoodItem = await _context.ComboFoodItems
                .Include(c => c.Combo)
                .Include(c => c.FoodItem)
                .FirstOrDefaultAsync(m => m.ComboId == id);

            if (comboFoodItem == null)
            {
                return NotFound();
            }

            return View(comboFoodItem);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var comboFoodItem = await _context.ComboFoodItems.FindAsync(id);
            if (comboFoodItem != null)
            {
                comboFoodItem.Status = Status.Inactive;
                _context.ComboFoodItems.Update(comboFoodItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ComboFoodItemExists(int id)
        {
            return _context.ComboFoodItems.Any(e => e.ComboId == id);
        }
    }
}
