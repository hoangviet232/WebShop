using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebShop.Models;
using WebShop.ModelViews;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebShop.Controllers
{
    public class SearchController : Controller
    {
        private readonly textdbMarketsContext _context;
        private const int PageSize = 10; // Adjust the value as needed

        public SearchController(textdbMarketsContext context)
        {
            _context = context;
        }

        //GET: /<controller>/
        [Route("api/search/findproduct")]
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

        [Route("api/search/findproduct1")]
        [HttpPost]
        public IActionResult FindProduct1(string keyword)
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

            return PartialView("_ListProductPartialView", ls);
        }
    }
}