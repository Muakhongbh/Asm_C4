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
    public class OrderDetailsController : Controller
    {
        private readonly MyDbConText _context;

        public OrderDetailsController(MyDbConText context)
        {
            _context = context;
        }

        // GET: OrderDetails    
        public async Task<IActionResult> Index()
        {
            // Truy vấn dữ liệu từ các bảng liên quan
            var orderDetails = from od in _context.OrderDetails
                               join o in _context.Orders on od.OrderId equals o.OrderId
                               join u in _context.Users on o.UserId equals u.UserId
                               join fi in _context.FoodItems on od.FoodItemId equals fi.FoodItemId into foodJoin
                               from fi in foodJoin.DefaultIfEmpty() // Join với bảng FoodItem
                               join cb in _context.Combos on od.ComboId equals cb.ComboId into comboJoin
                               from cb in comboJoin.DefaultIfEmpty() // Join với bảng Combo
                               select new
                               {
                                   OrderDetailId = od.OrderDetailId,
                                   Username = u.Username,
                                   OrderDate = o.OrderDate,
                                   OrderStatus = o.Status,
                                   FoodName = fi != null ? fi.FoodName : null,
                                   ComboName = cb != null ? cb.ComboName : null,
                                   Quantity = od.Quantity,
                                   UnitPrice = fi != null ? fi.Price : cb.ComboPrice,
                                   TotalPrice = (fi != null ? fi.Price : cb.ComboPrice) * od.Quantity
                               };

            // Trả về View với dữ liệu đã truy vấn
            return View(await orderDetails.ToListAsync());
        }


        // GET: OrderDetails/Details/5
        public IActionResult Details(int id)
        {
            var orderDetail = (from od in _context.OrderDetails
                               join o in _context.Orders on od.OrderId equals o.OrderId
                               join fi in _context.FoodItems on od.FoodItemId equals fi.FoodItemId into foodJoin
                               from fi in foodJoin.DefaultIfEmpty()
                               join cb in _context.Combos on od.ComboId equals cb.ComboId into comboJoin
                               from cb in comboJoin.DefaultIfEmpty()
                               where od.OrderDetailId == id
                               select new
                               {
                                   OrderId = o.OrderId,
                                   OrderStatus = o.Status,
                                   FoodName = fi != null ? fi.FoodName : null,
                                   FoodImageUrl = fi != null ? fi.ImageUrl : null,
                                   ComboName = cb != null ? cb.ComboName : null,
                                   ComboImageUrl = cb != null ? cb.ImageUrlcombo : null,
                                   Quantity = od.Quantity,
                                   UnitPrice = fi != null ? fi.Price : cb.ComboPrice,
                                   TotalPrice = (fi != null ? fi.Price : cb.ComboPrice) * od.Quantity
                               }).FirstOrDefault();

            if (orderDetail == null)
            {
                return NotFound("Không tìm thấy thông tin chi tiết đơn hàng.");
            }

            // Lấy role từ Session
            var role = HttpContext.Session.GetString("Role");

            // Truyền dữ liệu sang View
            ViewBag.Role = role;

            return View(orderDetail);
        }



        // GET: OrderDetails/Create
        //public IActionResult Create()
        //{
        //    ViewData["ComboId"] = new SelectList(_context.Combos, "ComboId", "ComboId");
        //    ViewData["FoodItemId"] = new SelectList(_context.FoodItems, "FoodItemId", "FoodItemId");
        //    ViewData["OrderId"] = new SelectList(_context.Orders, "OrderId", "OrderId");
        //    return View();
        //}

        // POST: OrderDetails/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
		[ValidateAntiForgeryToken]
        public async Task<IActionResult> Create()
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null)
            {
                return Content("Bạn cần đăng nhập để thanh toán.");
            }

            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
            {
                return Content("Người dùng không tồn tại.");
            }

            var cart = _context.Carts.FirstOrDefault(c => c.UserId == user.UserId);
            if (cart == null || !_context.CartItems.Any(ci => ci.CartId == cart.CartId))
            {
                return Content("Giỏ hàng của bạn hiện đang trống.");
            }

            var newOrder = new Order
            {
                UserId = user.UserId,
                OrderDate = DateTime.Now,
                Status = "Pending",
                OrderStatus = OrderStatus.Pending
            };
            _context.Orders.Add(newOrder);
            await _context.SaveChangesAsync();

            var cartItems = _context.CartItems.Where(ci => ci.CartId == cart.CartId).ToList();
            int? firstOrderDetailId = null; // Biến lưu trữ OrderDetailId đầu tiên

            foreach (var item in cartItems)
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = newOrder.OrderId,
                    FoodItemId = item.FoodItemId,
                    ComboId = item.ComboId,
                    Quantity = item.Quantity
                };
                _context.OrderDetails.Add(orderDetail);
                await _context.SaveChangesAsync(); // Lưu ngay để có OrderDetailId

                // Lưu ID của OrderDetail đầu tiên
                if (firstOrderDetailId == null)
                {
                    firstOrderDetailId = orderDetail.OrderDetailId;
                }

                // Cập nhật số lượng sản phẩm trong kho
                if (item.FoodItemId.HasValue)
                {
                    var foodItem = await _context.FoodItems.FindAsync(item.FoodItemId.Value);
                    if (foodItem != null)
                    {
                        foodItem.FoodQuantity -= item.Quantity; // Giả sử FoodItem có thuộc tính Quantity
                        _context.FoodItems.Update(foodItem);
                    }
                }
                //else if (item.ComboId.HasValue)
                //{
                //    var combo = await _context.Combos.FindAsync(item.ComboId.Value);
                //    if (combo != null)
                //    {
                //        combo.ComboQuantity -= item.Quantity; // Giả sử Combo có thuộc tính AvailableQuantity
                //        _context.Combos.Update(combo);
                //    }
                //}
            }

            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            // Điều hướng đến trang chi tiết của OrderDetail đầu tiên
            if (firstOrderDetailId.HasValue)
            {
                return RedirectToAction("Details", "OrderDetails", new { id = firstOrderDetailId.Value });
            }

            return Content("Không có chi tiết đơn hàng nào được tạo.");
        }


        // GET: OrderDetails/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Truy vấn OrderDetail bao gồm Order liên quan
            var orderDetail = await _context.OrderDetails
                .Include(od => od.Order)
                .FirstOrDefaultAsync(od => od.OrderDetailId == id);

            if (orderDetail == null)
            {
                return NotFound();
            }

            // Gửi trạng thái hiện tại của Order để hiển thị trong View
            ViewBag.CurrentStatus = orderDetail.Order.Status;

            ViewData["ComboId"] = new SelectList(_context.Combos, "ComboId", "ComboId", orderDetail.ComboId);
            ViewData["FoodItemId"] = new SelectList(_context.FoodItems, "FoodItemId", "FoodItemId", orderDetail.FoodItemId);
            ViewData["OrderId"] = new SelectList(_context.Orders, "OrderId", "OrderId", orderDetail.OrderId);
            return View(orderDetail);
        }


        // POST: OrderDetails/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, string action)
        //{
        //    // Tìm OrderDetail bao gồm Order
        //    var orderDetail = await _context.OrderDetails
        //        .Include(od => od.Order)
        //        .FirstOrDefaultAsync(od => od.OrderDetailId == id);

        //    if (orderDetail == null)
        //    {
        //        return NotFound("Không tìm thấy OrderDetail cần sửa.");
        //    }

        //    // Lấy Order liên quan
        //    var order = orderDetail.Order;
        //    if (order == null)
        //    {
        //        return NotFound("Không tìm thấy Order liên quan.");
        //    }

        //    // Kiểm tra hành động (chuyển trạng thái hoặc hủy đơn)
        //    if (action == "Cancel" && (order.Status == "Pending" || order.Status == "Approved"))
        //    {
        //        // Chuyển trạng thái thành hủy
        //        order.Status = "Canceled_Order";
        //    }
        //    else if (action == "Next")
        //    {
        //        // Chuyển trạng thái kế tiếp
        //        switch (order.Status)
        //        {
        //            case "Pending":
        //                order.Status = "Approved";
        //                break;
        //            case "Approved":
        //                order.Status = "Shipped";
        //                break;
        //            case "Shipped":
        //                order.Status = "Completed";
        //                break;
        //            default:
        //                return Content("Không thể chuyển trạng thái từ trạng thái hiện tại.");
        //        }
        //    }
        //    else
        //    {
        //        return Content("Hành động không hợp lệ.");
        //    }

        //    // Lưu thay đổi trạng thái Order
        //    try
        //    {
        //        _context.Update(order); // Cập nhật Order
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index)); // Điều hướng về danh sách
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!_context.Orders.Any(o => o.OrderId == order.OrderId))
        //        {
        //            return NotFound("Không tồn tại Order này.");
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }
        //}


        // GET: OrderDetails/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderDetail = await _context.OrderDetails
                .Include(o => o.Combos)
                .Include(o => o.FoodItem)
                .Include(o => o.Order)
                .FirstOrDefaultAsync(m => m.OrderDetailId == id);
            if (orderDetail == null)
            {
                return NotFound();
            }

            return View(orderDetail);
        }

        // POST: OrderDetails/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var orderDetail = await _context.OrderDetails.FindAsync(id);
            if (orderDetail != null)
            {
                _context.OrderDetails.Remove(orderDetail);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderDetailExists(int id)
        {
            return _context.OrderDetails.Any(e => e.OrderDetailId == id);
        }
    }
}
