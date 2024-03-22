using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TestMail.Service;
using WebShop.Extension;
using WebShop.Helpper;
using WebShop.Models;
using WebShop.ModelViews;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebShop.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly textdbMarketsContext _context;

        public INotyfService _notyfService { get; }

        public CheckoutController(textdbMarketsContext context, INotyfService notyfService)
        {
            _context = context;
            _notyfService = notyfService;
        }

        public List<CartItem> GioHang
        {
            get
            {
                var gh = HttpContext.Session.Get<List<CartItem>>("GioHang");
                if (gh == default(List<CartItem>))
                {
                    gh = new List<CartItem>();
                }
                return gh;
            }
        }

        [HttpGet]
        [Authorize]
        [Route("checkout.html", Name = "Checkout")]
        public IActionResult Index(string returnUrl = null)
        {
            //Lay gio hang ra de xu ly
            var cart = HttpContext.Session.Get<List<CartItem>>("GioHang");
            var taikhoanID = HttpContext.Session.GetString("CustomerId");
            MuaHangVM model = new MuaHangVM();
            if (taikhoanID != null)
            {
                var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.CustomerId == Convert.ToInt32(taikhoanID));
                model.CustomerId = khachhang.CustomerId;
                model.FullName = khachhang.FullName;
                model.Email = khachhang.Email;
                model.Phone = khachhang.Phone;
                model.Address = khachhang.Address;
            }
            /*            ViewData["lsTinhThanh"] = new SelectList(_context.Locations.Where(x => x.Levels == 1).OrderBy(x => x.Type).ToList(),"Location","Name");
            */
            ViewBag.GioHang = cart;
            return View(model);
        }

        /*  [HttpGet]
          [Route("testmail")]
          public async Task<IActionResult> TestMail([FromServices] ISendMailService sendMailService)
          {
              // Create a MailContent object
              var emailContent = new MailContent
              {
                  To = "vietkutioppa@gmail.com",
                  Subject = "Kiểm tra thử",
                  Body = "<p><strong> Xin chào .net</strong></p>"
              };

              try
              {
                  // Send the email
                  await sendMailService.SendMail(emailContent);
                  return Ok("Email sent successfully");
              }
              catch (Exception ex)
              {
                  // Handle the exception if needed
                  return StatusCode(500, $"An error occurred while sending the email: {ex.Message}");
              }
          }
  */

        [Authorize]
        [HttpPost]
        [Route("checkout.html", Name = "Checkout")]
        public async Task<IActionResult> Index(MuaHangVM muaHang, XemDonHang xemDonHang, [FromServices] ISendMailService sendMailService)
        {
            // Lấy ra giỏ hàng để xử lý
            var cart = HttpContext.Session.Get<List<CartItem>>("GioHang");
            var taikhoanID = HttpContext.Session.GetString("CustomerId");
            MuaHangVM model = new MuaHangVM();

            if (taikhoanID != null)
            {
                var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.CustomerId == Convert.ToInt32(taikhoanID));
                model.CustomerId = khachhang.CustomerId;
                model.FullName = khachhang.FullName;
                model.Email = khachhang.Email;
                model.Phone = khachhang.Phone;
                model.Address = khachhang.Address;

                _context.Update(khachhang);
                _context.SaveChanges();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    // Khởi tạo đơn hàng
                    Order donhang = new Order();
                    donhang.CustomerId = model.CustomerId;
                    donhang.Address = model.Address;
                    donhang.OrderDate = DateTime.Now;
                    donhang.TransactStatusId = 1; // Đơn hàng mới
                    donhang.Deleted = false;
                    donhang.Paid = false;
                    donhang.TotalMoney = Convert.ToInt32(cart.Sum(x => x.TotalMoney));
                    _context.Add(donhang);
                    _context.SaveChanges();

                    // Tạo danh sách chi tiết đơn hàng
                    foreach (var item in cart)
                    {
                        OrderDetail orderDetail = new OrderDetail();
                        orderDetail.OrderId = donhang.OrderId;
                        orderDetail.ProductId = item.product.ProductId;
                        orderDetail.Amount = item.amount;
                        orderDetail.TotalMoney = donhang.TotalMoney;
                        orderDetail.Price = item.product.Price;
                        orderDetail.CreateDate = DateTime.Now;
                        _context.Add(orderDetail);

                        // Gửi email thông tin đơn hàng
                        // Tính tổng tiền đơn hàng
                        double totalOrderMoney = cart.Sum(item => item.TotalMoney);

                        // Gửi email thông tin đơn hàng
                        var emailContent = new MailContent
                        {
                            To = muaHang.Email,
                            Subject = "HARMIC - Thông tin đơn hàng",
                            Body = $@"
<html>
<head>
    <style>
        body {{
            font-family: Arial, sans-serif;
            margin: 0;
            padding: 0;
            background-color: #f2f2f2;
        }}
        .container {{
            max-width: 600px;
            margin: 20px auto;
            padding: 20px;
            border: 1px solid #ccc;
            border-radius: 10px;
            background-color: #fff;
            box-shadow: 0px 0px 10px 0px rgba(0,0,0,0.1);
        }}
        h1 {{
            color: #333;
            text-align: center;
        }}
        .info {{
            margin-top: 20px;
            padding: 20px;
            border: 1px solid #ddd;
            border-radius: 5px;
            background-color: #f9f9f9;
        }}
        .info p {{
            margin: 10px 0;
            color: #666;
        }}
        .info strong {{
            color: #007bff;
        }}
        table {{
            width: 100%;
            border-collapse: collapse;
        }}
        th, td {{
            border: 1px solid #ddd;
            padding: 8px;
            text-align: left;
        }}
        th {{
            background-color: #f2f2f2;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <h1>Đặt hàng thành công</h1>
        <div class='info'>
            <p><strong>Thông tin đơn hàng:</strong></p>
            <p><strong>Họ và tên:</strong> {muaHang.FullName}</p>
            <p><strong>Điện thoại:</strong> {muaHang.Phone}</p>
            <p><strong>Địa chỉ nhận hàng:</strong> {muaHang.Address}</p>
        </div>
        <table>
            <thead>
                <tr>
                    <th>STT</th>
                    <th>Sản phẩm</th>
                    <th>Số lượng</th>
                    <th>Đơn giá</th>
                    <th>Tổng tiền</th>
                </tr>
            </thead>
            <tbody>
                {string.Join("", cart.Select((item, index) => $@"
                    <tr>
                        <td>{index + 1}</td>
                        <td>{item.product.ProductName}</td>
                        <td>{item.amount}</td>
                        <td>{item.product.Price}</td>
                        <td>{item.TotalMoney}</td>
                    </tr>"))}
            </tbody>
        </table>

        <div class='info'>
            <p><strong>Tổng tiền đơn hàng:</strong> {totalOrderMoney}</p>
            <p><strong>Ngày đặt:</strong> {DateTime.Now}</p>
        </div>
    </div>
</body>
</html>"
                        };

                        await sendMailService.SendMail(emailContent);
                    }

                    // Gửi email thông tin đơn hàng

                    // Lưu thay đổi vào cơ sở dữ liệu
                    _context.SaveChanges();

                    // Xóa giỏ hàng sau khi đặt hàng thành công
                    HttpContext.Session.Remove("GioHang");

                    // Hiển thị thông báo thành công và chuyển hướng đến trang Success
                    _notyfService.Success("Đơn hàng đặt thành công");
                    return RedirectToAction("Success");
                }
            }
            catch
            {
                // Trả về view và hiển thị lại giỏ hàng nếu có lỗi xảy ra
                ViewData["lsTinhThanh"] = new SelectList(_context.Locations.Where(x => x.Levels == 1).OrderBy(x => x.Type).ToList(), "Location", "Name");
                ViewBag.GioHang = cart;
                return View(model);
            }

            // Trả về view nếu không có lỗi và không thành công
            ViewData["lsTinhThanh"] = new SelectList(_context.Locations.Where(x => x.Levels == 1).OrderBy(x => x.Type).ToList(), "Location", "Name");
            ViewBag.GioHang = cart;
            return View(model);
        }

        [Route("dat-hang-thanh-cong.html", Name = "Success")]
        public IActionResult Success()
        {
            try
            {
                var taikhoanID = HttpContext.Session.GetString("CustomerId");
                if (string.IsNullOrEmpty(taikhoanID))
                {
                    return RedirectToAction("Login", "Accounts", new { returnUrl = "/dat-hang-thanh-cong.html" });
                }
                var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.CustomerId == Convert.ToInt32(taikhoanID));
                var donhang = _context.Orders
                    .Where(x => x.CustomerId == Convert.ToInt32(taikhoanID))
                    .OrderByDescending(x => x.OrderDate)
                    .FirstOrDefault();
                MuaHangSuccessVM successVM = new MuaHangSuccessVM();
                successVM.FullName = khachhang.FullName;
                successVM.DonHangID = donhang.OrderId;
                successVM.Phone = khachhang.Phone;
                successVM.Address = khachhang.Address;
                successVM.PhuongXa = GetNameLocation(donhang.Ward.Value);
                successVM.TinhThanh = GetNameLocation(donhang.District.Value);
                return View(successVM);
            }
            catch
            {
                return View();
            }
        }

        public string GetNameLocation(int idlocation)
        {
            try
            {
                var location = _context.Locations.AsNoTracking().SingleOrDefault(x => x.LocationId == idlocation);
                if (location != null)
                {
                    return location.NameWithType;
                }
            }
            catch
            {
                return string.Empty;
            }
            return string.Empty;
        }
    }
}