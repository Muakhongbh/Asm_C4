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
    public class OrdersController : Controller
    {
        private readonly MyDbConText _context;

        public OrdersController(MyDbConText context)
        {
            _context = context;
        }

        // GET: Orders
        public async Task<IActionResult> Index(DateTime? orderDate, OrderStatus? status, int? userId)
        {
            // Khởi tạo truy vấn với Include để lấy thêm thông tin từ bảng User
            var myDbContext = _context.Orders.Include(o => o.User).AsQueryable();

            // Lọc theo ngày đặt hàng
            if (orderDate.HasValue)
            {
                myDbContext = myDbContext.Where(o => o.OrderDate.Date == orderDate.Value.Date);
            }

            // Lọc theo trạng thái (OrderStatus)
            if (status.HasValue)
            {
                myDbContext = myDbContext.Where(o => o.OrderStatus == status.Value);
            }

            // Lọc theo User ID
            if (userId.HasValue)
            {
                myDbContext = myDbContext.Where(o => o.UserId == userId.Value);
            }

            // Trả về danh sách kết quả cho view
            return View(await myDbContext.ToListAsync());
        }


        //public Order GetOrCreatePendingOrder(int userId)
        //{
        //    var order = _context.Orders.FirstOrDefault(o => o.UserId == userId && o.Status == "Pending");
        //    if (order == null)
        //    {
        //        order = new Order
        //        {
        //            UserId = userId,
        //            OrderDate = DateTime.Now,
        //            Status = "Pending"
        //        };
        //        _context.Orders.Add(order);
        //        _context.SaveChanges();
        //    }
        //    return order;
        //}

        //public IActionResult Checkout(int orderId)
        //{
        //    var acc = HttpContext.Session.GetString("Username");
        //    if (acc == null)
        //    {
        //        return RedirectToAction("Login", "Account");
        //    }

        //    var user = _context.Users.FirstOrDefault(u => u.Username == acc);
        //    if (user == null)
        //    {
        //        return RedirectToAction("Login", "Account");
        //    }

        //    var order = _context.Orders.FirstOrDefault(o => o.OrderId == orderId && o.UserId == user.UserId && o.Status == "Pending");
        //    if (order == null)
        //    {
        //        return Content("Không có đơn hàng nào để thanh toán.");
        //    }

        //    order.Status = "Completed";
        //    _context.Orders.Update(order);
        //    _context.SaveChanges();

        //    return RedirectToAction("UserOrders");
        //}

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.User) // Join với bảng User để lấy thông tin người dùng
                .Include(o => o.OrderDetails) // Join với bảng OrderDetail để lấy thông tin chi tiết đơn hàng
                    .ThenInclude(od => od.FoodItem) // Join với bảng FoodItem (nếu có)
                .Include(o => o.OrderDetails) // Nếu bạn có bảng Combo, có thể join với Combo
                    .ThenInclude(od => od.Combos) // Join với bảng Combo (nếu có)
                .FirstOrDefaultAsync(m => m.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            ViewData["StatusList"] = Enum.GetValues(typeof(OrderStatus)).Cast<OrderStatus>();

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string newStatus)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            // Cập nhật trạng thái đơn hàng
            if (Enum.TryParse(typeof(OrderStatus), newStatus, out var status))
            {
                order.OrderStatus = (OrderStatus)status;
            }

            try
            {
                _context.Update(order);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(order.OrderId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToAction(nameof(Details), new { id = order.OrderId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            // Cập nhật trạng thái đơn hàng thành Canceled_Order
            order.OrderStatus = OrderStatus.Canceled_Order;

            try
            {
                _context.Update(order);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(order.OrderId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToAction(nameof(Details), new { id = order.OrderId });
        }

        public async Task<IActionResult> UserOrders()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                return NotFound();
            }

            var orders = await _context.Orders
                         .Include(o => o.OrderDetails)
                         .ThenInclude(od => od.FoodItem) // Nếu bạn có mối quan hệ này
                         .Include(o => o.OrderDetails)
                         .ThenInclude(od => od.Combos) // Nếu bạn có mối quan hệ này
                         .Where(o => o.UserId == user.UserId)
                         .ToListAsync();

            return View(orders);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId");
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderId,UserId,OrderDate,Status,OrderStatus")] Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", order.UserId);
            return View(order);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Tìm đơn hàng theo ID
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            // Truyền danh sách trạng thái (OrderStatus) sang view
            ViewData["StatusList"] = Enum.GetValues(typeof(OrderStatus)).Cast<OrderStatus>();

            // Trả về view với model đơn hàng
            return View(order);
        }

        // POST: Orders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,OrderStatus")] Order order)
        {
            if (id != order.OrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy đơn hàng hiện tại từ cơ sở dữ liệu
                    var existingOrder = await _context.Orders.FindAsync(id);
                    if (existingOrder == null)
                    {
                        return NotFound();
                    }

                    // Chỉ cập nhật trạng thái (OrderStatus)
                    existingOrder.OrderStatus = order.OrderStatus;

                    // Cập nhật và lưu vào cơ sở dữ liệu
                    _context.Update(existingOrder);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.OrderId))
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

            // Nếu có lỗi, trả về view với danh sách trạng thái
            ViewData["StatusList"] = Enum.GetValues(typeof(OrderStatus)).Cast<OrderStatus>();
            return View(order);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.OrderId == id);
        }
    }
}
