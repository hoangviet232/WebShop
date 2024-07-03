using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestMail.Service;
using WebShop.Extension;
using WebShop.Helpper;
using WebShop.Models;
using WebShop.ModelViews;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebShop.Controllers
{
    [Authorize]
    public class AccountsController : Controller
    {
        private readonly textdbMarketsContext _context;
        public INotyfService _notyfService { get; }

        public AccountsController(textdbMarketsContext context, INotyfService notyfService)
        {
            _context = context;
            _notyfService = notyfService;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ValidatePhone(string Phone)
        {
            try
            {
                var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.Phone.ToLower() == Phone.ToLower());
                if (khachhang != null)
                    return Json(data: "Số điện thoại : " + Phone + "đã được sử dụng");

                return Json(data: true);
            }
            catch
            {
                return Json(data: true);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ValidateEmail(string Email)
        {
            try
            {
                var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.Email.ToLower() == Email.ToLower());
                if (khachhang != null)
                    return Json(data: "Email : " + Email + " đã được sử dụng");
                return Json(data: true);
            }
            catch
            {
                return Json(data: true);
            }
        }

        [Route("tai-khoan-cua-toi.html", Name = "Dashboard")]
        public IActionResult Dashboard()
        {
            var taikhoanID = HttpContext.Session.GetString("CustomerId");
            if (taikhoanID != null)
            {
                var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.CustomerId == Convert.ToInt32(taikhoanID));
                if (khachhang != null)
                {
                    var lsDonHang = _context.Orders
                        .Include(x => x.TransactStatus)
                        .AsNoTracking()
                        .Where(x => x.CustomerId == khachhang.CustomerId)
                        .OrderByDescending(x => x.OrderDate)
                        .ToList();
                    ViewBag.DonHang = lsDonHang;
                    return View(khachhang);
                }
            }
            return RedirectToAction("Login");
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("dang-ky.html", Name = "DangKy")]
        public IActionResult DangkyTaiKhoan()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("dang-ky.html", Name = "DangKy")]
        public async Task<IActionResult> DangkyTaiKhoan(RegisterViewModel taikhoan, [FromServices] ISendMailService sendMailService)
        {
            try
            {
                var ktemail = await _context.Customers.FirstOrDefaultAsync(x => x.Email.ToLower() == taikhoan.Email.ToLower());
                if (ktemail != null)
                {
                    ModelState.AddModelError(string.Empty, "Email đã được sử dụng.");
                    return View(taikhoan);
                }
                var ktsdt = await _context.Customers.FirstOrDefaultAsync(x => x.Phone.ToLower() == taikhoan.Phone.ToLower());
                if (ktsdt != null)
                {
                    ModelState.AddModelError(string.Empty, "Số điện thoại đã được sử dụng.");
                    return View(taikhoan);
                }

                // Kiểm tra dữ liệu hợp lệ trước khi gửi email
                if (ModelState.IsValid)
                {
                    var emailContent = new MailContent
                    {
                        To = taikhoan.Email,
                        Subject = "HARMIC - Đăng kí tài khoản thành công",
                        Body = $@"
                    <html>
                    <head>
                        <style>
                            body {{
                                font-family: Arial, sans-serif;
                                margin: 0;
                                padding: 0;
                            }}
                            .container {{
                                max-width: 600px;
                                margin: 20px auto;
                                padding: 20px;
                                border: 1px solid #ccc;
                                border-radius: 10px;
                                background-color: #f9f9f9;
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
                                background-color: #fff;
                            }}
                            .info p {{
                                margin: 10px 0;
                            }}
                            .info strong {{
                                color: #007bff;
                            }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <h1>Đăng kí tài khoản thành công</h1>
                            <div class='info'>
                                <p><strong>Thông tin tài khoản:</strong></p>
                                <p><strong>Họ và tên:</strong> {taikhoan.FullName}</p>
                                <p><strong>Địa chỉ email:</strong> {taikhoan.Email}</p>
                                <p><strong>Điện thoại:</strong> {taikhoan.Phone}</p>
                                <p><strong>Địa chỉ:</strong> {taikhoan.Address}</p>
                            </div>
                        </div>
                    </body>
                    </html>"
                    };

                    await sendMailService.SendMail(emailContent);

                    // Thêm thông tin tài khoản vào cơ sở dữ liệu
                    string salt = Utilities.GetRandomKey();
                    Customer khachhang = new Customer
                    {
                        FullName = taikhoan.FullName,
                        Address = taikhoan.Address,
                        Phone = taikhoan.Phone.Trim().ToLower(),
                        Email = taikhoan.Email.Trim().ToLower(),
                        Password = (taikhoan.Password + salt.Trim()).ToMD5(),
                        Active = true,
                        Salt = salt,
                        CreateDate = DateTime.Now
                    };

                    _context.Add(khachhang);
                    await _context.SaveChangesAsync();

                    // Đăng nhập và chuyển hướng đến trang Dashboard
                    HttpContext.Session.SetString("CustomerId", khachhang.CustomerId.ToString());
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, khachhang.FullName),
                new Claim("CustomerId", khachhang.CustomerId.ToString())
            };
                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "login");
                    ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                    await HttpContext.SignInAsync(claimsPrincipal);

                    _notyfService.Success("Đăng ký thành công");
                    return RedirectToAction("Dashboard", "Accounts");
                }
                else
                {
                    return View(taikhoan);
                }
            }
            catch
            {
                return View(taikhoan);
            }
        }

        private Task<string> RenderViewToStringAsync(string v, RegisterViewModel taikhoan)
        {
            throw new NotImplementedException();
        }

        [AllowAnonymous]
        [Route("dang-nhap.html", Name = "DangNhap")]
        public IActionResult Login(string returnUrl = null)
        {
            var taikhoanID = HttpContext.Session.GetString("CustomerId");
            if (taikhoanID != null)
            {
                return RedirectToAction("Dashboard", "Accounts");
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("dang-nhap.html", Name = "DangNhap")]
        public async Task<IActionResult> Login(LoginViewModel customer, string returnUrl)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    bool isEmail = Utilities.IsValidEmail(customer.UserName);
                    if (!isEmail) return View(customer);

                    var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.Email.Trim() == customer.UserName);

                    if (khachhang == null) return RedirectToAction("DangkyTaiKhoan");
                    string pass = (customer.Password + khachhang.Salt.Trim()).ToMD5();
                    if (khachhang.Password != pass)
                    {
                        _notyfService.Success("Thông tin đăng nhập chưa chính xác");
                        return View(customer);
                    }
                    //kiem tra xem account co bi disable hay khong

                    if (khachhang.Active == false)
                    {
                        return RedirectToAction("ThongBao", "Accounts");
                    }

                    //Luu Session MaKh
                    HttpContext.Session.SetString("CustomerId", khachhang.CustomerId.ToString());
                    var taikhoanID = HttpContext.Session.GetString("CustomerId");

                    //Identity
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, khachhang.FullName),
                        new Claim("CustomerId", khachhang.CustomerId.ToString())
                    };
                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "login");
                    ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                    await HttpContext.SignInAsync(claimsPrincipal);
                    _notyfService.Success("Đăng nhập thành công");
                    if (string.IsNullOrEmpty(returnUrl))
                    {
                        return RedirectToAction("Dashboard", "Accounts");
                    }
                    else
                    {
                        return Redirect(returnUrl);
                    }
                }
            }
            catch
            {
                return RedirectToAction("DangkyTaiKhoan", "Accounts");
            }
            return View(customer);
        }

        [HttpGet]
        [Route("dang-xuat.html", Name = "DangXuat")]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync();
            HttpContext.Session.Remove("CustomerId");
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            try
            {
                var taikhoanID = HttpContext.Session.GetString("CustomerId");
                if (taikhoanID == null)
                {
                    return RedirectToAction("Login", "Accounts");
                }

                if (ModelState.IsValid)
                {
                    var taikhoan = _context.Customers.Find(Convert.ToInt32(taikhoanID));
                    if (taikhoan == null) return RedirectToAction("Login", "Accounts");

                    // Kiểm tra mật khẩu hiện tại
                    var pass = (model.PasswordNow.Trim() + taikhoan.Salt.Trim()).ToMD5();
                    if (pass != taikhoan.Password)
                    {
                        _notyfService.Error("Mật khẩu hiện tại không đúng");
                        return RedirectToAction("Dashboard", "Accounts");
                    }

                    // Kiểm tra mật khẩu mới và xác nhận mật khẩu mới
                    if (model.Password != model.ConfirmPassword)
                    {
                        _notyfService.Error("Mật khẩu mới không khớp");
                        return RedirectToAction("Dashboard", "Accounts");
                    }

                    // Nếu mọi thứ hợp lệ, thì thay đổi mật khẩu
                    string passnew = (model.Password.Trim() + taikhoan.Salt.Trim()).ToMD5();
                    taikhoan.Password = passnew;
                    _context.Update(taikhoan);
                    _context.SaveChanges();
                    _notyfService.Success("Đổi mật khẩu thành công");
                    return RedirectToAction("Dashboard", "Accounts");
                }
            }
            catch
            {
                _notyfService.Error("Thay đổi mật khẩu không thành công");
                return RedirectToAction("Dashboard", "Accounts");
            }

            _notyfService.Error("Thay đổi mật khẩu không thành công");
            return RedirectToAction("Dashboard", "Accounts");
        }
    }
}