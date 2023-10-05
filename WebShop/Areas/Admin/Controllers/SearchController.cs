using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebShop.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebShop.Areas.Admin.Controllerst
{
    [Area("Admin")]
    [Authorize]
    public class SearchController : Controller
    {
        private readonly textdbMarketsContext _context;

        public SearchController(textdbMarketsContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult FindProduct(string keyword)
        {
            List<Product> ls = new List<Product>();

            if (string.IsNullOrEmpty(keyword) || keyword.Length < 1)
            {
                // Select All Products
                ls = _context.Products.AsNoTracking()
                                      .Include(a => a.Cat)
                                      .OrderByDescending(x => x.ProductName)
                                      .Take(10)
                                      .ToList();
            }
            else
            {
                // Select Products matching the keyword
                ls = _context.Products.AsNoTracking()
                                      .Include(a => a.Cat)
                                      .Where(x => x.ProductName.Contains(keyword))
                                      .OrderByDescending(x => x.ProductName)
                                      .Take(10)
                                      .ToList();
            }

            return PartialView("ListProductsSearchPartial", ls);
        }

        [HttpPost]
        public IActionResult FindOrder(string keyword)
        {
            List<Order> ls = new List<Order>();

            if (string.IsNullOrEmpty(keyword) || keyword.Length < 1)
            {
                // Select All Orders
                ls = _context.Orders.AsNoTracking()
                                  .Include(a => a.Customer)
                                  .OrderByDescending(x => x.OrderDate)
                                  .Take(10)
                                  .ToList();
            }
            else
            {
                // Select Orders matching the keyword
                ls = _context.Orders.AsNoTracking()
                                  .Include(a => a.Customer)
                                  .Where(x => x.Customer.FullName.Contains(keyword))
                                  .OrderByDescending(x => x.OrderDate)
                                  .Take(10)
                                  .ToList();
            }

            return PartialView("ListOrderSeachPartial", ls);
        }

        [HttpPost]
        public IActionResult FindTinDang(string keyword)
        {
            List<TinDang> ls = new List<TinDang>();

            if (string.IsNullOrEmpty(keyword) || keyword.Length < 1)
            {
                // Select All Orders
                ls = _context.TinDangs.AsNoTracking()
                                  .OrderByDescending(x => x.Title)
                                  .Take(10)
                                  .ToList();
            }
            else
            {
                // Select Orders matching the keyword
                ls = _context.TinDangs.AsNoTracking()
                                  .Where(x => x.Title.Contains(keyword))
                                  .OrderByDescending(x => x.Title)
                                  .Take(10)
                                  .ToList();
            }

            return PartialView("ListTinDangSearchPartial", ls);
        }

        [HttpPost]
        public IActionResult FindCustomers(string keyword)
        {
            List<Customer> ls = new List<Customer>();

            if (string.IsNullOrEmpty(keyword) || keyword.Length < 1)
            {
                // Select All Orders
                ls = _context.Customers.AsNoTracking()
                                  .OrderByDescending(x => x.FullName)
                                  .Take(10)
                                  .ToList();
            }
            else
            {
                // Select Orders matching the keyword
                ls = _context.Customers.AsNoTracking()
                                  .Where(x => x.FullName.Contains(keyword)||
                                         x.Phone.Contains(keyword) ||
                                         x.Email.Contains(keyword) ||
                                         x.Address.Contains(keyword))          
                                  .OrderByDescending(x => x.FullName)
                                  .Take(10)
                                  .ToList();
            }

            return PartialView("ListCustomersSearchPartial", ls);
        }
    }
}