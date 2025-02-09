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
    public class ComboesController : Controller
    {
        private readonly MyDbConText _context;

        public ComboesController(MyDbConText context)
        {
            _context = context;
        }

        // GET: Comboes
        public async Task<IActionResult> Index(string comBoName, decimal? giaComBomax, decimal? giaCombomin, string moTaComBo, int page = 1, int pageSize = 5)
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("FoodItems", "BanHang");
            }

            var combosquery = _context.Combos.AsQueryable();
            if (!string.IsNullOrEmpty(comBoName))
            {
                combosquery = combosquery.Where(x => x.ComboName.Contains(comBoName));
            }

            if (giaCombomin.HasValue)
            {
                combosquery = combosquery.Where(x => x.ComboPrice >= giaCombomin.Value); // Sửa điều kiện
            }
            if (giaComBomax.HasValue)
            {
                combosquery = combosquery.Where(x => x.ComboPrice <= giaComBomax.Value);
            }
            if (!string.IsNullOrEmpty(moTaComBo))
            {
                combosquery = combosquery.Where(x => x.ComboDescription.Contains(moTaComBo));
            }

            ViewBag.ComBoName = comBoName;
            ViewBag.ComboMax = giaComBomax;
            ViewBag.ComBoMixn = giaCombomin;
            ViewBag.MoTa = moTaComBo;

            var combos = await combosquery.ToListAsync();

            return View(combos);
        }


        // GET: Comboes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var combo = await _context.Combos
                .FirstOrDefaultAsync(m => m.ComboId == id);
            if (combo == null)
            {
                return NotFound();
            }

            return View(combo);
        }

        // GET: Comboes/Create
        public IActionResult Create()
        {
            // Gán giá combo bằng 0 khi tạo mới
            var newCombo = new Combo
            {
                ComboPrice = 0 // Gán giá bằng 0
            };

            ViewData["Combos"] = _context.Combos.ToList();
            ViewData["FoodItems"] = _context.FoodItems.ToList();
            return View(newCombo); // Trả về model mới
        }

        // POST: Comboes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ComboId,ComboName,ComboPrice,ComboDescription")] Combo combo, IFormFile ImageUrlcombo)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem người dùng đã tải lên hình ảnh chưa
                if (ImageUrlcombo != null && ImageUrlcombo.Length > 0)
                {
                    // Tạo đường dẫn để lưu hình ảnh
                    var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    // Đường dẫn đầy đủ đến file hình ảnh
                    var filePath = Path.Combine(directoryPath, ImageUrlcombo.FileName);

                    // Lưu hình ảnh vào thư mục
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageUrlcombo.CopyToAsync(stream);
                    }

                    // Gán đường dẫn hình ảnh cho thuộc tính trong model
                    combo.ImageUrlcombo = "/images/" + ImageUrlcombo.FileName;
                }

                // Thêm combo vào cơ sở dữ liệu
                _context.Add(combo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Nếu model không hợp lệ, trả về lại view với model
            ViewData["Combos"] = _context.Combos.ToList();
            ViewData["FoodItems"] = _context.FoodItems.ToList();
            return View(combo);
        }

        // GET: Comboes/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var combo = await _context.Combos.FindAsync(id);
            if (combo == null)
            {
                return NotFound();
            }
            return View(combo);
        }

        // POST: Comboes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ComboId,ComboName,ComboPrice,ComboDescription")] Combo combo, IFormFile ImageUrlcombo)
        {
            if (id != combo.ComboId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra xem người dùng đã tải lên hình ảnh mới chưa
                    if (ImageUrlcombo != null && ImageUrlcombo.Length > 0)
                    {
                        // Tạo đường dẫn để lưu hình ảnh
                        var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                        if (!Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(directoryPath);
                        }

                        // Đường dẫn đầy đủ đến file hình ảnh
                        var filePath = Path.Combine(directoryPath, ImageUrlcombo.FileName);

                        // Lưu hình ảnh vào thư mục
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await ImageUrlcombo.CopyToAsync(stream);
                        }

                        // Gán đường dẫn hình ảnh cho thuộc tính trong model
                        combo.ImageUrlcombo = "/images/" + ImageUrlcombo.FileName;
                    }

                    // Cập nhật combo vào cơ sở dữ liệu
                    _context.Update(combo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ComboExists(combo.ComboId))
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

            // Nếu ModelState không hợp lệ, trả lại view với model
            return View(combo);
        }


        // GET: Comboes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var combo = await _context.Combos
                .FirstOrDefaultAsync(m => m.ComboId == id);
            if (combo == null)
            {
                return NotFound();
            }

            return View(combo);
        }

        // POST: Comboes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var combo = await _context.Combos.FindAsync(id);
            if (combo != null)
            {
                _context.Combos.Remove(combo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ComboExists(int id)
        {
            return _context.Combos.Any(e => e.ComboId == id);
        }
    }
}
